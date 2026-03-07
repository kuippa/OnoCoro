using System;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

/// <summary>
/// ステージ環境・ゲーム要素をパースするProvider
/// itemlists セクション: 配置可能アイテム一覧
/// stages セクション: BIT/CLK スコア初期化
/// events セクション: タイマーイベント
/// </summary>
internal static class EnvironmentalYamlProvider
{
    /// <summary>
    /// itemlists セクションをパースして ItemList に登録
    /// YAML ファイルの大文字小文字混在に対応（enum 値と大文字小文字を区別しない比較）
    /// </summary>
    internal static void LoadItemLists(List<Dictionary<string, string>> yamlDataList)
    {
        if (yamlDataList == null || yamlDataList.Count == 0)
        {
            return;
        }

        List<string> itemList = StageYamlRepository._ItemList;
        itemList.Clear();  // 新しいステージの itemlists をロード前にリセット（二重防止）

        foreach (Dictionary<string, string> rowData in yamlDataList)
        {
            foreach (KeyValuePair<string, string> entry in rowData)
            {
                string itemName = entry.Value;
                if (string.IsNullOrEmpty(itemName))
                {
                    continue;
                }
                if (itemList.Contains(itemName))
                {
                    continue;
                }
                if (!IsValidModelType(itemName))
                {
                    Debug.Log($"itemname '{itemName}' は GameEnum.ModelsType に定義されていません");
                    continue;
                }

                itemList.Add(itemName);
            }
        }
    }

    /// <summary>
    /// 指定文字列が GameEnum.ModelsType に存在するか（大文字小文字を区別しない）
    /// </summary>
    private static bool IsValidModelType(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            return false;
        }

        string[] enumNames = Enum.GetNames(typeof(GameEnum.ModelsType));
        foreach (string enumValue in enumNames)
        {
            if (string.Equals(itemName, enumValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// stages セクションをパースして ScoreCtrl にスコアを初期化 + mode フラグを設定
    /// mode: debug が指定されている場合, EventLoader._isDebugMode = true を設定
    /// </summary>
    internal static void LoadStageInit(YamlStream yaml, EventLoader eventLoader)
    {
        if (yaml == null)
        {
            Debug.LogWarning("[EnvironmentalYamlProvider.LoadStageInit] yaml is null");
            return;
        }

        if (eventLoader == null)
        {
            Debug.LogWarning("[EnvironmentalYamlProvider.LoadStageInit] eventLoader is null");
            return;
        }

        List<Dictionary<string, string>> yamlDataList = YamlParserHelper.BuildDictionaryListFromYaml(yaml, YamlSectionKeys.Stages);
        if (yamlDataList.Count == 0)
        {
            return;
        }

        foreach (Dictionary<string, string> rowData in yamlDataList)
        {
            foreach (KeyValuePair<string, string> entry in rowData)
            {
                // mode フィールドを抽出（debug モード判定）
                if (string.Equals(entry.Key, "mode", StringComparison.OrdinalIgnoreCase))
                {
                    eventLoader._isDebugMode = string.Equals(entry.Value, "debug", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                // スコア初期化（既存ロジック）
                if (!TrySetScore(GlobalConst.SHORT_SCORE1_SCALE, entry.Key, entry.Value))
                {
                    TrySetScore(GlobalConst.SHORT_SCORE2_SCALE, entry.Key, entry.Value);
                }
            }
        }
    }

    /// <summary>
    /// events セクションをパースして EventLoader にタイマーイベントを登録
    /// </summary>
    internal static void LoadTimerEvents(YamlStream yaml, EventLoader eventLoader)
    {
        if (yaml == null)
        {
            Debug.LogWarning("[EnvironmentalYamlProvider.LoadTimerEvents] yaml is null");
            return;
        }

        if (eventLoader == null)
        {
            Debug.LogWarning("[EnvironmentalYamlProvider.LoadTimerEvents] eventLoader is null");
            return;
        }

        List<Dictionary<string, string>> yamlDataList = YamlParserHelper.BuildDictionaryListFromYaml(yaml, YamlSectionKeys.Events);
        if (yamlDataList.Count == 0)
        {
            return;
        }

        Dictionary<float, List<EventCommand>> eventCommandsByTime = YamlCommandManager.ParseTimedEventCommands(yamlDataList);
        if (eventCommandsByTime.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<float, List<EventCommand>> timeEntry in eventCommandsByTime)
        {
            float eventTime = timeEntry.Key;
            List<Dictionary<string, string>> eventList = new List<Dictionary<string, string>>();

            foreach (EventCommand eventCommand in timeEntry.Value)
            {
                Dictionary<string, string> eventDict = new Dictionary<string, string>(eventCommand.Parameters);
                eventDict[TimedEventCommandFields.@event.ToString()] = eventCommand.Type.ToString();
                eventList.Add(eventDict);
            }

            eventLoader._timer_events[eventTime] = eventList;
        }

        eventLoader.SetEventToTimer();
    }

    /// <summary>
    /// スコアタイプ名をグローバル定数と比較（大文字小文字非依存）
    /// YAML で bit/BIT/Bit などと書かれても認識する
    /// </summary>
    private static bool TrySetScore(string scoreType, string key, string value)
    {
        // const 定数との大文字小文字非依存比較
        if (!string.Equals(scoreType, key, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!int.TryParse(value, out int intValue))
        {
            return false;
        }

        ScoreCtrl.InitScore(intValue, key);
        return true;
    }
}
