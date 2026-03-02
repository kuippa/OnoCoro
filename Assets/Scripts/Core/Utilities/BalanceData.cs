using System.Collections.Generic;

namespace CommonsUtility
{
    /// <summary>
    /// タイムラインイベントの種類
    /// </summary>
    public enum TimelineEventType
    {
        EnemySpawn,        // 敵スポーン
        TowerDeploy,       // タワー配置
        ResourceGain       // リソース取得（PowerCube等）
    }

    /// <summary>
    /// 統合タイムラインイベント
    /// イベント発生前後のゴミ蓄積数と、その時点のタワー処理上限を保持する
    /// </summary>
    public class TimelineEvent
    {
        public float Time { get; set; }
        public TimelineEventType EventType { get; set; }
        public string EventName { get; set; }              // 表示用ラベル
        public string Description { get; set; }            // 補足説明

        // --- 敵スポーン専用フィールド ---
        public string EnemyType { get; set; }              // 敵タイプ名
        public int EnemyCount { get; set; }                // スポーン数
        public int GarbageSpawned { get; set; }            // このイベントで発生するゴミ数

        // --- タワー配置専用フィールド ---
        public string TowerName { get; set; }              // タワー名
        public int TowerCount { get; set; }                // 配置数
        public int TowerCapacityAdded { get; set; }        // このイベントで加算される処理上限
        public int TowerCreateCost { get; set; }           // タワー作成コスト（spawn_unit_debug用）
        public string TowerCostType { get; set; }          // コストタイプ（BIT/CLK）

        // --- リソース取得専用フィールド（PowerCube） ---
        public int ResourceGain { get; set; }              // 取得できるリソース量
        public string ResourceGainType { get; set; }       // リソース種別（BIT/CLK）

        // --- 計算結果（全イベント共通） ---
        public string RouteName { get; set; }              // 対象ルート名
        public int GarbageBeforeEvent { get; set; }        // イベント発生前の未処理ゴミ累積数（ルート単位）
        public int GarbageAfterEvent { get; set; }         // イベント発生後の未処理ゴミ累積数（ルート単位）
        public int TotalCapacityAtEvent { get; set; }      // このイベント時点での合計タワー処理上限（ルート単位）
    }

    /// <summary>
    /// タワーのバランス情報
    /// </summary>
    public class TowerBalanceData
    {
        public string TowerName { get; set; }
        public int CreateCost { get; set; }
        public string CostType { get; set; }           // "BIT" or "CLK"
        public int UpdateCost { get; set; }
        public int DeleteCost { get; set; }
        public float CostTime { get; set; }
        public int BaseScore { get; set; }
        // タワー能力値（想定値）
        public int EstimatedGarbageProcessCapacity { get; set; }   // 処理可能なゴミ数
        public string GarbageProcessNote { get; set; }              // 説明（例："ゴミ10を処理予定"）
    }

    /// <summary>
    /// 敵のバランス情報
    /// </summary>
    public class EnemyBalanceData
    {
        public string EnemyName { get; set; }
        public int CreateCost { get; set; }            // 置かれる時のコスト（敵の場合は負値）
        public string CostType { get; set; }           // "BIT" or "CLK"
        public int BaseScore { get; set; }             // 倒した時のスコア
        public int GarbageDropCount { get; set; }      // 1体が発生させるゴミ数（最大値）
        public string GarbageDropNote { get; set; }    // 説明（例："最大〜ゴミ20個を散らかす"）
    }

    /// <summary>
    /// ステージ分析結果
    /// </summary>
    public class StageAnalysisResult
    {
        public string StageName { get; set; }
        public int InitialBIT { get; set; }
        public int InitialCLK { get; set; }
        public bool IsDebugMode { get; set; }                       // デバッグモード (mode: debug)
        public List<TowerBalanceData> AvailableTowers { get; set; } = new List<TowerBalanceData>();
        public int TotalEnemiesCount { get; set; }
        public int TotalEnemyDropValue { get; set; }
        public int EstimatedGarbageScore { get; set; }              // ゴミ処理スコア
        public int MaximumLogicalScore { get; set; }                // 最大理論値（敵スコア + ゴミスコア）
        public List<EnemySpawnInfo> EnemySpawns { get; set; } = new List<EnemySpawnInfo>();
        public List<TimelineEvent> UnifiedTimeline { get; set; } = new List<TimelineEvent>();  // 統合タイムライン
        public RecommendedStrategy RecommendedStrategy { get; set; }
    }

    /// <summary>
    /// ステージ内の敵生成情報
    /// </summary>
    public class EnemySpawnInfo
    {
        public float SpawnTime { get; set; }
        public string EnemyType { get; set; }
        public int Count { get; set; }
        public string RouteName { get; set; }            // 使用ルート名
        public int CumulativeGarbageCount { get; set; }  // この時点での累積ゴミ数
    }

    /// <summary>
    /// 推奨タワー配置情報
    /// </summary>
    public class StrategyTowerInfo
    {
        public string TowerName { get; set; }
        public int Count { get; set; }
        public float DeployTime { get; set; }
        public string CostType { get; set; }
        public int TotalCost { get; set; }
        public string Description { get; set; }
        public string RouteName { get; set; }            // 担当ルート名
    }

    /// <summary>
    /// リソース収支分析
    /// </summary>
    public class ResourceBreakdown
    {
        public int InitialBIT { get; set; }
        public int InitialCLK { get; set; }
        public int UsedBIT { get; set; }
        public int UsedCLK { get; set; }
        public int RemainingBIT { get; set; }
        public int RemainingCLK { get; set; }
        public string Status { get; set; }  // COMFORTABLE / TIGHT / IMPOSSIBLE
    }

    /// <summary>
    /// 推奨装略
    /// </summary>
    public class RecommendedStrategy
    {
        public string Description { get; set; }
        public string Difficulty { get; set; }  // Easy / Normal / Hard
        public List<StrategyTowerInfo> Towers { get; set; } = new List<StrategyTowerInfo>();
        public ResourceBreakdown ResourceBreakdown { get; set; }
        public float EstimatedClearTime { get; set; }
        public string SkillLevel { get; set; }
    }
}
