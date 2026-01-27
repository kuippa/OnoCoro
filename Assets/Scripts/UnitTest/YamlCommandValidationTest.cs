using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// YAML Command のバリデーションテスト
/// 各コマンドの Validate() メソッドの動作確認用
/// 
/// 使用方法：
/// 1. このスクリプトを GameManager などにアタッチ
/// 2. Play ボタンで実行
/// 3. Console に結果が表示される
/// 4. 使用後は Core/Editor/YamlCommandValidationTest.cs に移動
/// </summary>
public class YamlCommandValidationTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("YAML Command Validation Test Started");
        Debug.Log("========================================");
        
        TestGoalCommandValidation();
        Debug.Log("");
        TestGameOverCommandValidation();
        Debug.Log("");
        TestEventCommandValidation();
        Debug.Log("");
        TestBoardCommandValidation();
        
        Debug.Log("========================================");
        Debug.Log("YAML Command Validation Test Completed");
        Debug.Log("========================================");
    }

    /// <summary>
    /// GoalCommand のバリデーションテスト
    /// </summary>
    private void TestGoalCommandValidation()
    {
        Debug.Log("[TEST] GoalCommand Validation");
        
        // ケース 1: 正常系
        {
            var command = new GoalCommand
            {
                Type = GoalType.AllEnemiesDefeated,
                Threshold = 10,
                Description = "全敵を倒す"
            };
            var result = command.Validate();
            LogTestResult("Valid goal (Threshold=10)", result.IsValid, result);
        }
        
        // ケース 2: 異常系 - Threshold が 0
        {
            var command = new GoalCommand
            {
                Type = GoalType.ScoreThreshold,
                Threshold = 0
            };
            var result = command.Validate();
            LogTestResult("Invalid goal (Threshold=0)", !result.IsValid, result);
        }
        
        // ケース 3: 異常系 - Threshold が負数
        {
            var command = new GoalCommand
            {
                Type = GoalType.TimeLimit,
                Threshold = -5
            };
            var result = command.Validate();
            LogTestResult("Invalid goal (Threshold=-5)", !result.IsValid, result);
        }
        
        // ケース 4: 境界値 - Threshold = 1
        {
            var command = new GoalCommand
            {
                Type = GoalType.WavesCompleted,
                Threshold = 1
            };
            var result = command.Validate();
            LogTestResult("Valid goal (Threshold=1)", result.IsValid, result);
        }
    }

    /// <summary>
    /// GameOverCommand のバリデーションテスト
    /// </summary>
    private void TestGameOverCommandValidation()
    {
        Debug.Log("[TEST] GameOverCommand Validation");
        
        // ケース 1: 正常系 - HealthDefeated
        {
            var command = new GameOverCommand
            {
                Type = GameOverType.HealthDefeated,
                Threshold = 100
            };
            var result = command.Validate();
            LogTestResult("Valid GameOver (HealthDefeated, Threshold=100)", result.IsValid, result);
        }
        
        // ケース 2: 異常系 - HealthDefeated with Threshold = 0
        {
            var command = new GameOverCommand
            {
                Type = GameOverType.HealthDefeated,
                Threshold = 0
            };
            var result = command.Validate();
            LogTestResult("Invalid GameOver (HealthDefeated, Threshold=0)", !result.IsValid, result);
        }
        
        // ケース 3: 正常系 - EnemyReachedGoal（Threshold は使用しない）
        {
            var command = new GameOverCommand
            {
                Type = GameOverType.EnemyReachedGoal,
                Threshold = 0  // この値は検証されない
            };
            var result = command.Validate();
            LogTestResult("Valid GameOver (EnemyReachedGoal, Threshold=0)", result.IsValid, result);
        }
        
        // ケース 4: 正常系 - TimeExpired
        {
            var command = new GameOverCommand
            {
                Type = GameOverType.TimeExpired,
                Threshold = 300
            };
            var result = command.Validate();
            LogTestResult("Valid GameOver (TimeExpired, Threshold=300)", result.IsValid, result);
        }
    }

    /// <summary>
    /// EventCommand のバリデーションテスト
    /// </summary>
    private void TestEventCommandValidation()
    {
        Debug.Log("[TEST] EventCommand Validation");
        
        // ケース 1: 正常系
        {
            var command = new EventCommand
            {
                TriggerTime = 10.5f,
                Type = YamlEventType.spawn_unit,
                Parameters = new Dictionary<string, string>
                {
                    { "wave_number", "1" },
                    { "enemy_count", "5" }
                }
            };
            var result = command.Validate();
            LogTestResult("Valid event (TriggerTime=10.5f)", result.IsValid, result);
        }
        
        // ケース 2: 異常系 - TriggerTime が負数
        {
            var command = new EventCommand
            {
                TriggerTime = -1f,
                Type = YamlEventType.weather,
                Parameters = new Dictionary<string, string>()
            };
            var result = command.Validate();
            LogTestResult("Invalid event (TriggerTime=-1f)", !result.IsValid, result);
        }
        
        // ケース 3: 警告 - Parameters なし
        {
            var command = new EventCommand
            {
                TriggerTime = 5f,
                Type = YamlEventType.wind,
                Parameters = new Dictionary<string, string>()
            };
            var result = command.Validate();
            LogTestResult("Event with warnings (empty Parameters)", result.IsValid && result.Warnings.Count > 0, result);
        }
        
        // ケース 4: 境界値 - TriggerTime = 0
        {
            var command = new EventCommand
            {
                TriggerTime = 0f,
                Type = YamlEventType.telop,
                Parameters = new Dictionary<string, string>
                {
                    { "resource_type", "gold" }
                }
            };
            var result = command.Validate();
            LogTestResult("Valid event (TriggerTime=0f)", result.IsValid, result);
        }
    }

    /// <summary>
    /// BoardCommand のバリデーションテスト
    /// 
    /// 注：SpawnPoints と DifficultyModifier のテストケースは以下の理由で保持：
    /// - 実装側では値が文字列として保存・取得されるだけで、型変換がない
    /// - ゲームプレイロジックでこれらの値が実際に消費されていない
    /// - 将来的な実装に備えてテストケースは残すが、Validate() では検証しない
    /// </summary>
    private void TestBoardCommandValidation()
    {
        Debug.Log("[TEST] BoardCommand Validation");
        
        // ケース 1: 正常系 - BoardSize
        {
            var command = new BoardCommand
            {
                ConfigType = BoardConfigType.BoardSize,
                Value = "256x256"
            };
            var result = command.Validate();
            LogTestResult("Valid board (BoardSize=256x256)", result.IsValid, result);
        }
        
        // ケース 2: 異常系 - BoardSize 形式不正
        {
            var command = new BoardCommand
            {
                ConfigType = BoardConfigType.BoardSize,
                Value = "256_256"  // "x" ではなく "_"
            };
            var result = command.Validate();
            LogTestResult("Invalid board (BoardSize=256_256)", !result.IsValid, result);
        }
        
        // ケース 3: SpawnPoints（値は文字列として保存されるのみ - バリデーション不実装）
        {
            var command = new BoardCommand
            {
                ConfigType = BoardConfigType.SpawnPoints,
                Value = "8"
            };
            var result = command.Validate();
            LogTestResult("Board (SpawnPoints=8) - No validation at this layer", result.IsValid, result);
        }
        
        // ケース 4: SpawnPoints が数値でない場合も受け入れる（実装側がない）
        {
            var command = new BoardCommand
            {
                ConfigType = BoardConfigType.SpawnPoints,
                Value = "invalid"
            };
            var result = command.Validate();
            LogTestResult("Board (SpawnPoints=invalid) - Accepted as string", result.IsValid, result);
        }
        
        // ケース 5: DifficultyModifier（値は文字列として保存されるのみ - バリデーション不実装）
        {
            var command = new BoardCommand
            {
                ConfigType = BoardConfigType.DifficultyModifier,
                Value = "1.5"
            };
            var result = command.Validate();
            LogTestResult("Board (DifficultyModifier=1.5) - No validation at this layer", result.IsValid, result);
        }
        
        // ケース 6: DifficultyModifier が範囲外でも受け入れる（実装側がない）
        {
            var command = new BoardCommand
            {
                ConfigType = BoardConfigType.DifficultyModifier,
                Value = "10.0"
            };
            var result = command.Validate();
            LogTestResult("Board (DifficultyModifier=10.0) - Accepted as string", result.IsValid, result);
        }
        
        // ケース 7: 空値はエラー
        {
            var command = new BoardCommand
            {
                ConfigType = BoardConfigType.BoardSize,
                Value = ""
            };
            var result = command.Validate();
            LogTestResult("Invalid board (empty Value)", !result.IsValid, result);
        }
    }

    /// <summary>
    /// テスト結果をログ出力（ヘルパーメソッド）
    /// </summary>
    private void LogTestResult(string testName, bool expectedPass, ValidationResult result)
    {
        string status = expectedPass ? "✓ PASS" : "✗ FAIL";
        string details = "";
        
        if (result.Errors.Count > 0)
        {
            details += $" | Errors: {string.Join(", ", result.Errors)}";
        }
        if (result.Warnings.Count > 0)
        {
            details += $" | Warnings: {string.Join(", ", result.Warnings)}";
        }
        
        Debug.Log($"  {status}: {testName}{details}");
    }
}
