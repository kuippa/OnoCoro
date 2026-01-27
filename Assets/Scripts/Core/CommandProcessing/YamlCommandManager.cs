using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// YAML コマンド管理クラス
    /// すべての YAML コマンドを一元管理
    /// ユーザージェネレーティブ対応の中核
    /// </summary>
    internal static class YamlCommandManager
    {
        // Error/Warning messages（重要な条件のみ）
        private const string _MSG_INVALID_EVENT_TYPE = "不正な YamlEventType: {0}（実装済みのイベント型: spawn_unit, spawn_enemy_unit, weather, solar, wind, watersurface, earthquake, building_break, notice, telop, subtelop, bloom_path, off_bloom_path, volcano）";
        private const string _MSG_PARSE_FAIL_TIME = "イベント時刻パース失敗: {0}";
        private const string _MSG_INVALID_TIME = "イベント時刻が無効です（0 以上の値が必要）";
        private const string _MSG_MISSING_NAME_FIELD = "Board コマンド: name フィールドが見つかりません";
        private const string _MSG_VALIDATION_FAIL_GOAL = "GoalCommand バリデーション失敗: {0}";
        private const string _MSG_VALIDATION_FAIL_GAMEOVER = "GameOverCommand バリデーション失敗: {0}";
        private const string _MSG_VALIDATION_FAIL_EVENT = "EventCommand バリデーション失敗 (time={0}): {1}";
        private const string _MSG_VALIDATION_FAIL_BOARD = "BoardCommand バリデーション失敗: {0}";
        
        /// <summary>
        /// YAML から GoalCommand のリストに変換
        /// </summary>
        internal static List<GoalCommand> ParseGoalCommands(List<Dictionary<string, string>> yamlDataList)
        {
            return ParseCommandList(yamlDataList, CreateGoalCommand, _MSG_VALIDATION_FAIL_GOAL);
        }
        
        /// <summary>
        /// YAML から GameOverCommand のリストに変換
        /// </summary>
        internal static List<GameOverCommand> ParseGameOverCommands(List<Dictionary<string, string>> yamlDataList)
        {
            return ParseCommandList(yamlDataList, CreateGameOverCommand, _MSG_VALIDATION_FAIL_GAMEOVER);
        }
        
        /// <summary>
        /// YAML から BoardCommand のリストに変換
        /// </summary>
        internal static List<BoardCommand> ParseBoardCommands(List<Dictionary<string, string>> yamlDataList)
        {
            return ParseCommandList(yamlDataList, CreateBoardCommand, _MSG_VALIDATION_FAIL_BOARD);
        }
        
        /// <summary>
        /// YAML から EventCommand のリストに変換
        /// 時間をキーとした Dictionary に格納（時間トリガー管理用）
        /// </summary>
        internal static Dictionary<float, List<EventCommand>> ParseTimedEventCommands(List<Dictionary<string, string>> yamlDataList)
        {
            var commandsByTime = new Dictionary<float, List<EventCommand>>();
            
            if (yamlDataList == null || yamlDataList.Count == 0)
            {
                return commandsByTime;
            }
            
            string timeField = TimedEventCommandFields.time.ToString();
            
            foreach (var yamlData in yamlDataList)
            {
                // time を解析
                if (!yamlData.TryGetValue(timeField, out var timeStr))
                {
                    continue;
                }
                
                if (!float.TryParse(timeStr, out var triggerTime))
                {
                    Debug.LogWarning(string.Format(_MSG_PARSE_FAIL_TIME, timeStr));
                    continue;
                }
                
                if (triggerTime < 0f)
                {
                    Debug.LogWarning(_MSG_INVALID_TIME);
                    continue;
                }
                
                // EventCommand を生成
                var command = CreateEventCommand(yamlData, triggerTime);
                
                // バリデーション
                ValidateEventCommand(command, triggerTime);
                
                // Dictionary に格納
                if (!commandsByTime.ContainsKey(triggerTime))
                {
                    commandsByTime[triggerTime] = new List<EventCommand>();
                }
                commandsByTime[triggerTime].Add(command);
            }
            
            return commandsByTime;
        }
        
        /// <summary>
        /// 利用可能なすべてのコマンドタイプを取得
        /// （ユーザードキュメント用）
        /// </summary>
        internal static List<string> GetAvailableGoalTypes()
        {
            return System.Enum.GetNames(typeof(GoalType)).ToList();
        }
        
        internal static List<string> GetAvailableGameOverTypes()
        {
            return System.Enum.GetNames(typeof(GameOverType)).ToList();
        }
        
        internal static List<string> GetAvailableEventTypes()
        {
            return System.Enum.GetNames(typeof(YamlEventType)).ToList();
        }
        
        internal static List<string> GetAvailableBoardConfigs()
        {
            return System.Enum.GetNames(typeof(BoardConfigType)).ToList();
        }
        
        /// <summary>
        /// 汎用コマンドリストパーサー（DRY: 重複を排除）
        /// Goal/GameOver/Board コマンド用（戻り値は List<T>）
        /// </summary>
        private static List<T> ParseCommandList<T>(
            List<Dictionary<string, string>> yamlDataList,
            Func<Dictionary<string, string>, T> factory,
            string validationMessageTemplate) where T : IYamlCommand
        {
            var commands = new List<T>();
            
            if (yamlDataList == null || yamlDataList.Count == 0)
            {
                return commands;
            }
            
            foreach (var yamlData in yamlDataList)
            {
                var command = factory(yamlData);
                ValidateCommand(command, validationMessageTemplate);
                commands.Add(command);
            }
            
            return commands;
        }
        
        /// <summary>
        /// GoalCommand ファクトリーメソッド
        /// </summary>
        private static GoalCommand CreateGoalCommand(Dictionary<string, string> yamlData)
        {
            var command = new GoalCommand();
            
            string goalTypeField = GoalCommandFields.goal_type.ToString();
            string thresholdField = GoalCommandFields.threshold.ToString();
            string descriptionField = GoalCommandFields.description.ToString();
            
            // goal_type を解析（失敗時は黙って続行）
            if (yamlData.TryGetValue(goalTypeField, out var goalTypeStr))
            {
                Enum.TryParse<GoalType>(goalTypeStr, ignoreCase: true, out var goalType);
                command.Type = goalType;
            }
            
            // threshold を解析
            if (yamlData.TryGetValue(thresholdField, out var thresholdStr))
            {
                if (int.TryParse(thresholdStr, out var threshold))
                {
                    command.Threshold = threshold;
                }
            }
            
            // description を解析
            if (yamlData.TryGetValue(descriptionField, out var description))
            {
                command.Description = description;
            }
            
            return command;
        }
        
        /// <summary>
        /// GameOverCommand ファクトリーメソッド
        /// </summary>
        private static GameOverCommand CreateGameOverCommand(Dictionary<string, string> yamlData)
        {
            var command = new GameOverCommand();
            
            string gameoverTypeField = GameOverCommandFields.gameover_type.ToString();
            string thresholdField = GameOverCommandFields.threshold.ToString();
            
            // gameover_type を解析（失敗時は黙って続行）
            if (yamlData.TryGetValue(gameoverTypeField, out var gameoverTypeStr))
            {
                Enum.TryParse<GameOverType>(gameoverTypeStr, ignoreCase: true, out var gameoverType);
                command.Type = gameoverType;
            }
            
            // threshold を解析
            if (yamlData.TryGetValue(thresholdField, out var thresholdStr))
            {
                if (int.TryParse(thresholdStr, out var threshold))
                {
                    command.Threshold = threshold;
                }
            }
            
            return command;
        }
        
        /// <summary>
        /// BoardCommand ファクトリーメソッド
        /// </summary>
        private static BoardCommand CreateBoardCommand(Dictionary<string, string> yamlData)
        {
            var command = new BoardCommand();
            
            string nameField = BoardCommandFields.name.ToString();
            string valueField = BoardCommandFields.value.ToString();
            
            // name フィールドから BoardConfigType を判定
            if (!yamlData.TryGetValue(nameField, out var nameStr))
            {
                Debug.LogWarning(_MSG_MISSING_NAME_FIELD);
                return command;
            }
            
            // 型解析失敗時も黙って続行
            Enum.TryParse<BoardConfigType>(nameStr, ignoreCase: true, out var configType);
            command.ConfigType = configType;
            
            // value を解析
            if (yamlData.TryGetValue(valueField, out var value))
            {
                command.Value = value;
            }
            
            return command;
        }
        
        /// <summary>
        /// IYamlCommand のバリデーション結果を処理
        /// </summary>
        private static void ValidateCommand(IYamlCommand command, string messageTemplate)
        {
            var validationResult = command.Validate();
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(", ", validationResult.Errors);
                Debug.LogWarning(string.Format(messageTemplate, errorMessage));
            }
        }
        
        /// <summary>
        /// EventCommand の専用バリデーション処理
        /// </summary>
        private static void ValidateEventCommand(EventCommand command, float triggerTime)
        {
            var validationResult = command.Validate();
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(", ", validationResult.Errors);
                Debug.LogWarning(string.Format(_MSG_VALIDATION_FAIL_EVENT, triggerTime, errorMessage));
            }
        }
        
        /// <summary>
        /// YAML データから EventCommand を生成
        /// </summary>
        private static EventCommand CreateEventCommand(Dictionary<string, string> yamlData, float triggerTime)
        {
            var command = new EventCommand
            {
                TriggerTime = triggerTime,
                Parameters = new Dictionary<string, string>()
            };
            
            string timeField = TimedEventCommandFields.time.ToString();
            string eventField = TimedEventCommandFields.@event.ToString();
            
            // YAML データをパラメータに格納
            foreach (var entry in yamlData)
            {
                if (entry.Key == timeField)
                {
                    continue;
                }
                
                if (entry.Key == eventField)
                {
                    if (Enum.TryParse<YamlEventType>(entry.Value, ignoreCase: true, out var eventType))
                    {
                        command.Type = eventType;
                    }
                    else
                    {
                        Debug.LogWarning(string.Format(_MSG_INVALID_EVENT_TYPE, entry.Value));
                    }
                }
                else
                {
                    command.Parameters[entry.Key] = entry.Value;
                }
            }
            
            return command;
        }
    }
}
