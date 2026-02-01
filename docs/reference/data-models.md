# ゲームデータモデル定義

**バージョン**: 1.0.0  
**対象**: ゲームデータ構造・エンティティモデル  
**最終更新**: 2026-02-02

---

## 概要

OnoCoro の **ゲームエンティティ・ゲームデータ** を定義します。各モデルは以下の責務を持ちます：

- **ゲームデータ**: ゲーム状態の永続化・キャッシング（readonly struct）
- **ゲームエンティティ**: シーン内の動作するオブジェクト（MonoBehaviour）
- **管理モデル**: グローバル設定・マネージャー

---

## ゲームデータモデル

### 1. TowerData（タワー属性）

**目的**: タワーの基本属性を定義  
**実装**: `readonly struct`  
**バージョン**: 1.0.0

```csharp
public readonly struct TowerData
{
    /// <summary>
    /// タワーの一意な ID（prefab 名から自動生成）
    /// </summary>
    public readonly string TowerID;

    /// <summary>
    /// UI 表示名
    /// </summary>
    public readonly string DisplayName;

    /// <summary>
    /// 説明文
    /// </summary>
    public readonly string Description;

    /// <summary>
    /// 攻撃力（ダメージ値）
    /// </summary>
    public readonly float AttackPower;

    /// <summary>
    /// 攻撃範囲（ユニット）
    /// </summary>
    public readonly float AttackRange;

    /// <summary>
    /// 攻撃速度（秒）
    /// </summary>
    public readonly float AttackSpeed;

    /// <summary>
    /// 建設コスト
    /// </summary>
    public readonly int BuildCost;

    /// <summary>
    /// 建設時間（秒）
    /// </summary>
    public readonly float BuildTime;

    /// <summary>
    /// 最大 HP
    /// </summary>
    public readonly float MaxHealth;

    /// <summary>
    /// 特殊効果タイプ
    /// </summary>
    public readonly string SpecialEffect;

    public TowerData(
        string towerId,
        string displayName,
        string description,
        float attackPower,
        float attackRange,
        float attackSpeed,
        int buildCost,
        float buildTime,
        float maxHealth,
        string specialEffect = "None")
    {
        TowerID = towerId;
        DisplayName = displayName;
        Description = description;
        AttackPower = attackPower;
        AttackRange = attackRange;
        AttackSpeed = attackSpeed;
        BuildCost = buildCost;
        BuildTime = buildTime;
        MaxHealth = maxHealth;
        SpecialEffect = specialEffect;
    }

    /// <summary>
    /// DPS（Damage Per Second）を計算
    /// </summary>
    public float CalculateDPS()
    {
        return AttackPower / AttackSpeed;
    }

    /// <summary>
    /// 効率性スコア（コストパフォーマンス）
    /// </summary>
    public float CalculateEfficiency()
    {
        if (BuildCost <= 0) return 0f;
        return CalculateDPS() / BuildCost;
    }
}
```

### 使用例

```csharp
var sentryGuardData = new TowerData(
    towerId: "SentryGuard",
    displayName: "監視員",
    description: "敵を検知・標的化するタワー",
    attackPower: 10f,
    attackRange: 8f,
    attackSpeed: 1.5f,
    buildCost: 100,
    buildTime: 3f,
    maxHealth: 50f,
    specialEffect: "Target"
);

float dps = sentryGuardData.CalculateDPS();
Debug.Log($"DPS: {dps:F2}");  // 出力: DPS: 6.67
```

---

### 2. EnemyData（敵属性）

**目的**: 敵ユニットの基本属性を定義  
**実装**: `readonly struct`  
**バージョン**: 1.0.0

```csharp
public readonly struct EnemyData
{
    /// <summary>
    /// 敵の一意な ID（prefab 名から自動生成）
    /// </summary>
    public readonly string EnemyID;

    /// <summary>
    /// UI 表示名
    /// </summary>
    public readonly string DisplayName;

    /// <summary>
    /// 敵の HP
    /// </summary>
    public readonly float Health;

    /// <summary>
    /// 移動速度（ユニット/秒）
    /// </summary>
    public readonly float MoveSpeed;

    /// <summary>
    /// ダメージ（基地に与えるダメージ）
    /// </summary>
    public readonly int Damage;

    /// <summary>
    /// 倒した時の報酬リソース
    /// </summary>
    public readonly int RewardResource;

    /// <summary>
    /// 敵タイプ分類
    /// </summary>
    public readonly string EnemyType;

    public EnemyData(
        string enemyId,
        string displayName,
        float health,
        float moveSpeed,
        int damage,
        int rewardResource,
        string enemyType = "Normal")
    {
        EnemyID = enemyId;
        DisplayName = displayName;
        Health = health;
        MoveSpeed = moveSpeed;
        Damage = damage;
        RewardResource = rewardResource;
        EnemyType = enemyType;
    }

    /// <summary>
    /// 敵の難易度スコア（バランス調整用）
    /// </summary>
    public float CalculateDifficultyScore()
    {
        return Health * MoveSpeed * Damage / 10f;
    }
}
```

### 使用例

```csharp
var litterData = new EnemyData(
    enemyId: "Litter",
    displayName: "ごみをまく敵",
    health: 30f,
    moveSpeed: 2.5f,
    damage: 5,
    rewardResource: 50,
    enemyType: "Hazard"
);

float difficulty = litterData.CalculateDifficultyScore();
Debug.Log($"Difficulty: {difficulty:F2}");
```

---

### 3. StageData（ステージ属性）

**目的**: ステージ全体の設定を定義  
**実装**: `class`（大規模データのため参照型）  
**バージョン**: 1.0.0

```csharp
public class StageData
{
    /// <summary>
    /// ステージID（ファイル名）
    /// </summary>
    public string StageID { get; set; }

    /// <summary>
    /// ステージ表示名
    /// </summary>
    public string StageName { get; set; }

    /// <summary>
    /// ステージ説明
    /// </summary>
    public string StageDescription { get; set; }

    /// <summary>
    /// 難易度（1-5）
    /// </summary>
    public int DifficultyLevel { get; set; }

    /// <summary>
    /// 推奨プレイ時間（秒）
    /// </summary>
    public float RecommendedPlayTime { get; set; }

    /// <summary>
    /// ゲーム目標条件のリスト
    /// </summary>
    public List<GoalCondition> Goals { get; set; }

    /// <summary>
    /// ゲームオーバー条件のリスト
    /// </summary>
    public List<GameOverCondition> GameOvers { get; set; }

    /// <summary>
    /// ゲーム進行イベントのリスト
    /// </summary>
    public List<TimedEvent> Events { get; set; }

    /// <summary>
    /// 利用可能なユニットのリスト
    /// </summary>
    public List<string> AvailableUnits { get; set; }

    /// <summary>
    /// パスマーカーのリスト（敵の経路）
    /// </summary>
    public List<PathMarker> PathMarkers { get; set; }

    public StageData()
    {
        Goals = new List<GoalCondition>();
        GameOvers = new List<GameOverCondition>();
        Events = new List<TimedEvent>();
        AvailableUnits = new List<string>();
        PathMarkers = new List<PathMarker>();
    }

    /// <summary>
    /// ステージの完全性をチェック
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(StageID)
            && !string.IsNullOrEmpty(StageName)
            && Goals.Count > 0
            && GameOvers.Count > 0
            && PathMarkers.Count >= 2;  // 最低でも start と goal が必要
    }
}
```

### 関連クラス

#### GoalCondition

```csharp
public class GoalCondition
{
    /// <summary>
    /// 目標タイプ（notfailtime, building, garbage）
    /// </summary>
    public string GoalType { get; set; }

    /// <summary>
    /// パラメータ（可変長）
    /// </summary>
    public List<int> Parameters { get; set; }

    public GoalCondition(string goalType, params int[] parameters)
    {
        GoalType = goalType;
        Parameters = new List<int>(parameters);
    }
}
```

#### GameOverCondition

```csharp
public class GameOverCondition
{
    /// <summary>
    /// 敗北条件タイプ（garbage, building, base）
    /// </summary>
    public string OverType { get; set; }

    /// <summary>
    /// 判定パラメータ
    /// </summary>
    public int Threshold { get; set; }

    public GameOverCondition(string overType, int threshold)
    {
        OverType = overType;
        Threshold = threshold;
    }
}
```

#### TimedEvent

```csharp
public class TimedEvent
{
    /// <summary>
    /// イベント発火時刻（秒）
    /// </summary>
    public float TriggerTime { get; set; }

    /// <summary>
    /// イベントタイプ（weather, wind, spawn_unit など）
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// イベントパラメータ
    /// </summary>
    public string Value { get; set; }

    public TimedEvent(float triggerTime, string eventType, string value)
    {
        TriggerTime = triggerTime;
        EventType = eventType;
        Value = value;
    }
}
```

#### PathMarker

```csharp
public class PathMarker
{
    /// <summary>
    /// マーカー名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// ワールド座標
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// マーカーの GameObject 参照
    /// </summary>
    [SerializeField]
    private GameObject _markerObject;

    public PathMarker(string name, Vector3 position)
    {
        Name = name;
        Position = position;
    }
}
```

---

### 4. GameState（ゲーム状態）

**目的**: 現在のゲーム進行状況を管理  
**実装**: `class`（状態管理のため参照型）  
**バージョン**: 1.0.0

```csharp
public class GameState
{
    /// <summary>
    /// 現在のステージデータ
    /// </summary>
    public StageData CurrentStage { get; set; }

    /// <summary>
    /// 現在のゲーム時間（秒）
    /// </summary>
    public float ElapsedTime { get; set; }

    /// <summary>
    /// 基地の現在 HP
    /// </summary>
    public int BaseHealth { get; set; }

    /// <summary>
    /// 現在のリソース保有量
    /// </summary>
    public int CurrentResource { get; set; }

    /// <summary>
    /// スポーンされた敵の現在数
    /// </summary>
    public int EnemyCount { get; set; }

    /// <summary>
    /// 現在のゴミ数（環境汚染度）
    /// </summary>
    public int GarbageCount { get; set; }

    /// <summary>
    /// 配置されたタワーのリスト
    /// </summary>
    public List<Tower> PlacedTowers { get; set; }

    /// <summary>
    /// ゲーム一時停止中か
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// ゲーム終了状態
    /// </summary>
    public GameEndState EndState { get; set; }

    public GameState()
    {
        PlacedTowers = new List<Tower>();
        ElapsedTime = 0f;
        IsPaused = false;
        EndState = GameEndState.InProgress;
    }

    /// <summary>
    /// ステージクリア判定
    /// </summary>
    public bool IsGoalAchieved()
    {
        if (CurrentStage == null || CurrentStage.Goals.Count == 0)
        {
            return false;
        }

        // 任意の Goal を達成したら true
        foreach (GoalCondition goal in CurrentStage.Goals)
        {
            if (IsGoalConditionMet(goal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// ゲームオーバー判定
    /// </summary>
    public bool IsGameOver()
    {
        if (CurrentStage == null || CurrentStage.GameOvers.Count == 0)
        {
            return false;
        }

        foreach (GameOverCondition over in CurrentStage.GameOvers)
        {
            if (IsGameOverConditionMet(over))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsGoalConditionMet(GoalCondition goal)
    {
        switch (goal.GoalType)
        {
            case "notfailtime":
                return goal.Parameters.Count > 0 && ElapsedTime >= goal.Parameters[0];
            
            case "garbage":
                return goal.Parameters.Count >= 2 
                    && GarbageCount < goal.Parameters[0]
                    && ElapsedTime >= goal.Parameters[1];
            
            default:
                return false;
        }
    }

    private bool IsGameOverConditionMet(GameOverCondition over)
    {
        switch (over.OverType)
        {
            case "garbage":
                return GarbageCount > over.Threshold;
            
            case "base":
                return BaseHealth <= 0;
            
            default:
                return false;
        }
    }
}

public enum GameEndState
{
    InProgress = 0,
    Victory = 1,
    Defeat = 2,
}
```

---

### 5. ResourceData（リソース管理）

**目的**: ゲーム内リソース（お金・エネルギーなど）を管理  
**実装**: `readonly struct`  
**バージョン**: 1.0.0

```csharp
public readonly struct ResourceData
{
    /// <summary>
    /// 金銭リソース
    /// </summary>
    public readonly int Gold;

    /// <summary>
    /// エネルギーリソース
    /// </summary>
    public readonly int Energy;

    /// <summary>
    /// スペシャルリソース（限定）
    /// </summary>
    public readonly int Special;

    public ResourceData(int gold = 0, int energy = 0, int special = 0)
    {
        Gold = gold;
        Energy = energy;
        Special = special;
    }

    /// <summary>
    /// リソース足りているか確認
    /// </summary>
    public bool HasEnough(int goldRequired, int energyRequired, int specialRequired)
    {
        return Gold >= goldRequired && Energy >= energyRequired && Special >= specialRequired;
    }

    /// <summary>
    /// リソース消費
    /// </summary>
    public ResourceData Spend(int gold, int energy, int special)
    {
        return new ResourceData(
            Gold - gold,
            Energy - energy,
            Special - special
        );
    }

    /// <summary>
    /// リソース獲得
    /// </summary>
    public ResourceData Earn(int gold, int energy, int special)
    {
        return new ResourceData(
            Gold + gold,
            Energy + energy,
            Special + special
        );
    }
}
```

---

## ゲームエンティティ

### 1. Tower（タワー実装）

**目的**: シーン内で動作するタワーオブジェクト  
**実装**: `MonoBehaviour`  
**参照**: [project-rules/naming-conventions.md](../project-rules/naming-conventions.md)

```csharp
internal class Tower : MonoBehaviour
{
    [SerializeField]
    private TowerData _data;

    [SerializeField]
    private float _currentHealth;

    private List<Enemy> _targets = new List<Enemy>();

    public TowerData Data => _data;

    public float CurrentHealth => _currentHealth;

    public bool IsDestroyed => _currentHealth <= 0;

    private void Awake()
    {
        _currentHealth = _data.MaxHealth;
    }

    private void Update()
    {
        UpdateTargets();
        Attack();
    }

    private void UpdateTargets()
    {
        _targets.RemoveAll(e => e == null || e.IsDestroyed);

        // 攻撃範囲内の敵を検索
        // （実装詳細は省略）
    }

    private void Attack()
    {
        if (_targets.Count == 0) return;

        Enemy target = _targets[0];
        target.TakeDamage((int)_data.AttackPower);
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (IsDestroyed)
        {
            Destroy(gameObject);
        }
    }
}
```

---

### 2. Enemy（敵実装）

**目的**: シーン内で動作する敵オブジェクト  
**実装**: `MonoBehaviour`

```csharp
internal class Enemy : MonoBehaviour
{
    [SerializeField]
    private EnemyData _data;

    [SerializeField]
    private float _currentHealth;

    private Vector3 _currentTarget;
    private int _pathIndex = 0;
    private List<Vector3> _path = new List<Vector3>();

    public EnemyData Data => _data;

    public float CurrentHealth => _currentHealth;

    public bool IsDestroyed => _currentHealth <= 0;

    private void Awake()
    {
        _currentHealth = _data.Health;
    }

    private void Update()
    {
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (_path.Count == 0) return;

        Vector3 direction = (_path[_pathIndex] - transform.position).normalized;
        transform.position += direction * _data.MoveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, _path[_pathIndex]) < 0.1f)
        {
            _pathIndex++;
            if (_pathIndex >= _path.Count)
            {
                ReachGoal();
            }
        }
    }

    private void ReachGoal()
    {
        // ゴール到達時の処理
        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (IsDestroyed)
        {
            Destroy(gameObject);
        }
    }

    public void SetPath(List<Vector3> path)
    {
        _path = path;
    }
}
```

---

## マネージャー・グローバルモデル

### 1. GameConfig（ゲーム設定）

**目的**: グローバルゲーム設定を保持  
**実装**: `internal static class`  
**参照**: [AGENTS.md - Access Modifier Policy](../../AGENTS.md#access-modifier-policy)

```csharp
internal static class GameConfig
{
    /// <summary>
    /// ゲームモード（Debug / Release）
    /// </summary>
    internal static string _APP_GAME_MODE = GlobalConst.GAME_MODE_DEBUG;

    /// <summary>
    /// ログレベル
    /// </summary>
    internal static DebugLevel DebugLevel { get; set; } = DebugLevel.All;

    /// <summary>
    /// ログファイル名
    /// </summary>
    internal static string LogFileName { get; set; } = GlobalConst._LOG_FILE_NAME;
}
```

---

## バージョン管理

### 現在のバージョン

**data-models.md v1.0.0**

- TowerData: 完成
- EnemyData: 完成
- StageData: 完成
- GameState: 完成
- ResourceData: 完成

### 将来予定（Phase 2）

**v1.1.0**:
- WeaponData（武装データ）の追加
- BuffData（一時効果データ）の追加
- ディフィケルティプリセットの追加

---

## セリアライゼーション

### Unity 互換性

すべてのデータモデルは `[Serializable]` 対応：

```csharp
[System.Serializable]
public readonly struct TowerData
{
    [SerializeField]
    public string TowerID;
    
    // ... その他フィールド
}
```

### JSON エクスポート

データモデルは JSON で保存・復元可能：

```csharp
// 保存
string json = JsonUtility.ToJson(towerData);
File.WriteAllText("towerdata.json", json);

// 復元
string loaded = File.ReadAllText("towerdata.json");
TowerData restored = JsonUtility.FromJson<TowerData>(loaded);
```

---

## 関連ドキュメント

- [yaml-format.md](yaml-format.md) - YAML ステージフォーマット
- [architecture/asset-management.md](../architecture/asset-management.md) - リソース管理
- [architecture/recovery-guidelines.md](../architecture/recovery-guidelines.md) - Recovery パターン
- [project-rules/naming-conventions.md](../project-rules/naming-conventions.md) - クラス命名規則
