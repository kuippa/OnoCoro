using System.Collections.Generic;
using CommonsUtility;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

namespace CommonsUtility
{
    /// <summary>
    /// years セクション（年サイクル）をパースする Provider（Season 3 W1）
    ///
    /// 各年の schedule（スポーンパターン編成）を「at + パターン内相対 time」で
    /// 実時刻に展開し、年内 events とマージして EventLoader に年別辞書として登録する。
    /// years セクションが無いステージでは何もしない（後方互換）。
    /// 仕様: docs/reference/yaml-format.md「Season 3 拡張」を参照
    /// </summary>
    internal static class YearScheduleYamlProvider
    {
        private const string _SLOT_PREFIX = "{";
        private const string _SLOT_SUFFIX = "}";

        /// <summary>
        /// years セクションをパースして EventLoader に年別イベントを登録
        /// </summary>
        internal static void LoadYears(YamlStream yaml, EventLoader eventLoader)
        {
            if (yaml == null || eventLoader == null)
            {
                Debug.LogWarning("[YearScheduleYamlProvider.LoadYears] yaml または eventLoader が null");
                return;
            }

            YamlSequenceNode yearsSequence = YamlParserHelper.GetYamlSequenceNode(yaml, YamlSectionKeys.Years);
            if (yearsSequence == null)
            {
                // years セクション無し: 従来のタイムライン駆動ステージ（後方互換）
                return;
            }

            SpawnPatternRepository.LoadAllPatterns();
            eventLoader.ClearYearEvents();

            foreach (YamlNode yearNode in yearsSequence)
            {
                YamlMappingNode yearMapping = yearNode as YamlMappingNode;
                if (yearMapping == null)
                {
                    Debug.LogWarning("[YearScheduleYamlProvider.LoadYears] 非MappingNode の年定義をスキップ");
                    continue;
                }
                LoadSingleYear(yearMapping, eventLoader);
            }

            Debug.Log($"[YearScheduleYamlProvider.LoadYears] {eventLoader.GetYearCount()} 年分のイベントを登録しました");
        }

        /// <summary>
        /// 年定義 1 件をパース・展開して EventLoader に登録
        /// </summary>
        private static void LoadSingleYear(YamlMappingNode yearMapping, EventLoader eventLoader)
        {
            string yearText = YamlParserHelper.GetChildScalar(yearMapping, YearCommandFields.year.ToString());
            if (!int.TryParse(yearText, out int yearNumber) || yearNumber <= 0)
            {
                Debug.LogWarning($"[YearScheduleYamlProvider] year が不正な年定義をスキップ: '{yearText}'");
                return;
            }

            string durationText = YamlParserHelper.GetChildScalar(yearMapping, YearCommandFields.duration.ToString());
            if (!float.TryParse(durationText, out float duration) || duration <= 0f)
            {
                Debug.LogWarning($"[YearScheduleYamlProvider] duration が不正な年定義をスキップ: year {yearNumber}");
                return;
            }

            List<Dictionary<string, string>> flatEvents = BuildFlatYearEvents(yearMapping, yearNumber);

            Dictionary<float, List<EventCommand>> commandsByTime = YamlCommandManager.ParseTimedEventCommands(flatEvents);
            Dictionary<float, List<Dictionary<string, string>>> timerEvents = BuildTimerEventDictionary(commandsByTime);

            eventLoader.SetYearEvents(yearNumber, timerEvents, duration);

            // 任意: 消火なし想定火災延焼棟数（ベースライン・W3 Task 4）
            string baselineText = YamlParserHelper.GetChildScalar(yearMapping, YearCommandFields.baseline.ToString());
            if (int.TryParse(baselineText, out int baseline) && baseline >= 0)
            {
                eventLoader.SetYearBaseline(yearNumber, baseline);
            }
        }

        /// <summary>
        /// 年内の単発 events と schedule 展開結果をマージしたフラットなイベントリストを構築
        /// </summary>
        private static List<Dictionary<string, string>> BuildFlatYearEvents(YamlMappingNode yearMapping, int yearNumber)
        {
            List<Dictionary<string, string>> flatEvents = new List<Dictionary<string, string>>();

            YamlSequenceNode eventsSequence
                = YamlParserHelper.GetChildSequence(yearMapping, YearCommandFields.events.ToString());
            if (eventsSequence != null)
            {
                flatEvents.AddRange(YamlParserHelper.BuildDictionaryListFromSequence(eventsSequence));
            }

            YamlSequenceNode scheduleSequence
                = YamlParserHelper.GetChildSequence(yearMapping, YearCommandFields.schedule.ToString());
            if (scheduleSequence != null)
            {
                foreach (Dictionary<string, string> scheduleEntry in YamlParserHelper.BuildDictionaryListFromSequence(scheduleSequence))
                {
                    ExpandScheduleEntry(scheduleEntry, flatEvents, yearNumber);
                }
            }

            return flatEvents;
        }

        /// <summary>
        /// schedule エントリ 1 件をパターン参照から実イベント列に展開
        /// pattern / at 以外のキーはすべてスロット束縛として扱う
        /// </summary>
        internal static void ExpandScheduleEntry(
            Dictionary<string, string> scheduleEntry,
            List<Dictionary<string, string>> resultEvents,
            int yearNumber)
        {
            if (!scheduleEntry.TryGetValue(ScheduleCommandFields.pattern.ToString(), out string patternId))
            {
                Debug.LogWarning($"[YearScheduleYamlProvider] pattern 指定の無い schedule エントリをスキップ: year {yearNumber}");
                return;
            }

            if (!scheduleEntry.TryGetValue(ScheduleCommandFields.at.ToString(), out string atText)
                || !float.TryParse(atText, out float atOffset) || atOffset < 0f)
            {
                Debug.LogWarning($"[YearScheduleYamlProvider] at が不正な schedule エントリをスキップ: {patternId} (year {yearNumber})");
                return;
            }

            if (!SpawnPatternRepository.TryGetPattern(patternId, out List<Dictionary<string, string>> patternEvents))
            {
                Debug.LogWarning($"[YearScheduleYamlProvider] 未定義のパターン ID をスキップ: {patternId} (year {yearNumber})");
                return;
            }

            string timeField = TimedEventCommandFields.time.ToString();
            foreach (Dictionary<string, string> patternEvent in patternEvents)
            {
                Dictionary<string, string> expandedEvent = ExpandSinglePatternEvent(patternEvent, scheduleEntry, atOffset, timeField);
                if (expandedEvent != null)
                {
                    resultEvents.Add(expandedEvent);
                }
            }
        }

        /// <summary>
        /// パターン内イベント 1 件を実時刻化・スロット置換してコピーを返す（不正時は null）
        /// </summary>
        private static Dictionary<string, string> ExpandSinglePatternEvent(
            Dictionary<string, string> patternEvent,
            Dictionary<string, string> slotBindings,
            float atOffset,
            string timeField)
        {
            if (!patternEvent.TryGetValue(timeField, out string relativeTimeText)
                || !float.TryParse(relativeTimeText, out float relativeTime) || relativeTime < 0f)
            {
                Debug.LogWarning($"[YearScheduleYamlProvider] time が不正なパターンイベントをスキップ: '{relativeTimeText}'");
                return null;
            }

            Dictionary<string, string> expandedEvent = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in patternEvent)
            {
                if (entry.Key == timeField)
                {
                    continue;
                }
                expandedEvent[entry.Key] = ReplaceSlots(entry.Value, slotBindings);
            }
            expandedEvent[timeField] = (atOffset + relativeTime).ToString();

            return expandedEvent;
        }

        /// <summary>
        /// 文字列中の {スロット名} を束縛値で置換
        /// pattern / at は予約キーのため置換対象から除外する
        /// </summary>
        internal static string ReplaceSlots(string input, Dictionary<string, string> slotBindings)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string result = input;
            foreach (KeyValuePair<string, string> binding in slotBindings)
            {
                if (binding.Key == ScheduleCommandFields.pattern.ToString()
                    || binding.Key == ScheduleCommandFields.at.ToString())
                {
                    continue;
                }
                result = result.Replace(_SLOT_PREFIX + binding.Key + _SLOT_SUFFIX, binding.Value);
            }

            return result;
        }

        /// <summary>
        /// EventCommand 辞書を EventLoader の _timer_events と同形式の辞書に変換
        /// （EnvironmentalYamlProvider.LoadTimerEvents と同じ変換ロジック）
        /// </summary>
        private static Dictionary<float, List<Dictionary<string, string>>> BuildTimerEventDictionary(
            Dictionary<float, List<EventCommand>> commandsByTime)
        {
            Dictionary<float, List<Dictionary<string, string>>> timerEvents
                = new Dictionary<float, List<Dictionary<string, string>>>();

            foreach (KeyValuePair<float, List<EventCommand>> timeEntry in commandsByTime)
            {
                List<Dictionary<string, string>> eventList = new List<Dictionary<string, string>>();
                foreach (EventCommand eventCommand in timeEntry.Value)
                {
                    Dictionary<string, string> eventDict = new Dictionary<string, string>(eventCommand.Parameters);
                    eventDict[TimedEventCommandFields.@event.ToString()] = eventCommand.Type.ToString();
                    eventList.Add(eventDict);
                }
                timerEvents[timeEntry.Key] = eventList;
            }

            return timerEvents;
        }
    }
}
