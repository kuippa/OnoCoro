using System;
using System.Collections.Generic;

namespace CommonsUtility
{
    /// <summary>
    /// ゴール定義（YAML の goals セクション対応）
    /// </summary>
    internal struct GoalCommand : IYamlCommand
    {
        public GoalType Type { get; set; }
        
        /// <summary>
        /// ゴール達成の閾値（しきい値）
        /// GoalType によって意味が異なります:
        /// - ScoreThreshold: 達成に必要な得点
        /// - TimeLimit: 制限時間（秒）
        /// - AllEnemiesDefeated: 倒すべき敵の数
        /// - WavesCompleted: クリアすべきウェーブ数
        /// </summary>
        public int Threshold { get; set; }
        public string Description { get; set; }
        
        public YamlSectionType SectionType => YamlSectionType.Goals;
        
        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            if (Threshold <= 0)
            {
                result.AddError("Threshold は 1 以上である必要があります");
            }
            
            return result;
        }
    }

    /// <summary>
    /// ゲームオーバー定義（YAML の gameovers セクション対応）
    /// </summary>
    internal struct GameOverCommand : IYamlCommand
    {
        public GameOverType Type { get; set; }
        
        /// <summary>
        /// ゲームオーバーの閾値（しきい値）
        /// GameOverType によって意味が異なります:
        /// - HealthDefeated: 致命的なHP値（この値以下で敗北）
        /// - EnemyReachedGoal: 許容できる敵到達数
        /// - TimeExpired: 制限時間超過
        /// - NoPlaceableUnits: 配置不可ユニット数
        /// </summary>
        public int Threshold { get; set; }
        
        public YamlSectionType SectionType => YamlSectionType.GameOvers;
        
        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            if (Type == GameOverType.HealthDefeated && Threshold <= 0)
            {
                result.AddError("HP は 1 以上である必要があります");
            }
            
            return result;
        }
    }

    /// <summary>
    /// イベント定義（YAML の events セクション対応）
    /// YamlEventType enum による型安全なバリデーション
    /// </summary>
    internal struct EventCommand : IYamlCommand
    {
        public float TriggerTime { get; set; }
        public YamlEventType Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        
        public YamlSectionType SectionType => YamlSectionType.Events;
        
        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            // TriggerTime バリデーション
            if (TriggerTime < 0)
            {
                result.AddError("TriggerTime は 0 以上である必要があります");
            }
            
            // Type バリデーション（enum により自動的に有効性確保）
            if (!Enum.IsDefined(typeof(YamlEventType), Type))
            {
                result.AddError($"不正なイベント型: {Type}");
            }
            
            // event_value パース可能性の詳細チェックは EventLoader の実装側に委譲
            // Parameters は YamlCommandManager が生成するが、実装側では event_value を直接解析するため
            
            return result;
        }
    }

    /// <summary>
    /// ボード設定コマンド（YAML の boards セクション対応）
    /// </summary>
    internal struct BoardCommand : IYamlCommand
    {
        public BoardConfigType ConfigType { get; set; }
        public string Value { get; set; }
        
        public YamlSectionType SectionType => YamlSectionType.Boards;
        
        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrEmpty(Value))
            {
                result.AddError("Board コマンドの値が設定されていません");
                return result;
            }
            
            // BoardSize のみ形式検証（実装側で使用される）
            // SpawnPoints と DifficultyModifier は文字列として保存されるだけで、
            // 実装側で型変換されていないため、ここでの範囲チェックは不要
            if (ConfigType == BoardConfigType.BoardSize)
            {
                if (!IsBoardSizeValid(Value))
                {
                    result.AddError("BoardSize は \"WxH\" 形式である必要があります（例: 256x256）");
                }
            }
            
            return result;
        }
        
        private bool IsBoardSizeValid(string boardSize)
        {
            if (string.IsNullOrEmpty(boardSize))
            {
                return false;
            }
            
            var parts = boardSize.Split('x');
            return parts.Length == 2 
                && int.TryParse(parts[0], out _) 
                && int.TryParse(parts[1], out _);
        }
    }
}
