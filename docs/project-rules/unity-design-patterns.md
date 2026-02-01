# Unity 設計パターン

**目的**: Unity 特有の実装パターン・ベストプラクティス・落とし穴の回避

---

## MonoBehaviour パターン

### 基本構造

[OK] **MonoBehaviour ライフサイクルを理解して実装**

```csharp
public class GameController : MonoBehaviour
{
    // 1. SerializeField (Inspector)
    [SerializeField] private float _spawnRate = 2.0f;
    [SerializeField] private GameObject _enemyPrefab;
    
    // 2. Private フィールド
    private List<Enemy> _activeEnemies;
    private bool _isInitialized;
    
    // 3. Unity ライフサイクル
    private void Awake()
    {
        // 最初に呼ばれる（Gamepad 初期化など）
    }
    
    private void Start()
    {
        // 最初の Update の前に呼ばれる（初期化処理）
        Initialize();
    }
    
    private void Update()
    {
        // フレームごと
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnEnemy();
        }
    }
    
    private void OnDestroy()
    {
        // オブジェクト削除時
        Cleanup();
    }
    
    // 4. Private メソッド
    private void Initialize() { }
    private void SpawnEnemy() { }
    private void Cleanup() { }
}
```

### ライフサイクルの実行順序

| 順番 | メソッド | 用途 |
|------|---------|------|
| 1 | `OnEnable()` | 有効化時 |
| 2 | `Awake()` | 初期化（シーン読み込み時） |
| 3 | `OnTransformParentChanged()` | 親変更時 |
| 4 | `Start()` | 初期化（最初の Update 前） |
| 5 | `Update()` | フレームごと |
| 6 | `LateUpdate()` | Update 後（カメラ追従など） |
| 7 | `OnDisable()` | 無効化時 |
| 8 | `OnDestroy()` | 削除時 |

### Coroutine パターン

[OK] **時間経過が必要な処理は Coroutine で実装**

```csharp
public class GameManager : MonoBehaviour
{
    // Coroutine 起動
    private IEnumerator WaitAndSpawn()
    {
        yield return new WaitForSeconds(2.0f);  // 2 秒待機
        SpawnEnemy();
    }
    
    // 使用例
    private void Start()
    {
        StartCoroutine(WaitAndSpawn());
    }
    
    // キャッシュ化（パフォーマンス重視）
    private WaitForSeconds _spawnWait = new WaitForSeconds(2.0f);
    
    private IEnumerator WaitAndSpawnCached()
    {
        yield return _spawnWait;
        SpawnEnemy();
    }
}
```

[WARN] **WaitForSeconds は毎フレーム生成しない**

```csharp
// [NG] 毎回新規作成（GC pressure）
private void Update()
{
    if (someCondition)
    {
        StartCoroutine(WaitForSeconds(1.0f));  // ❌
    }
}

// [OK] フィールドでキャッシュ
private WaitForSeconds _cachedWait = new WaitForSeconds(1.0f);
private void Update()
{
    if (someCondition)
    {
        StartCoroutine(DoSomethingAfterDelay());
    }
}

private IEnumerator DoSomethingAfterDelay()
{
    yield return _cachedWait;
    DoSomething();
}
```

---

## Singleton パターン

### Manager Singleton（状態管理）

[OK] **ゲーム状態を管理する Manager は Singleton パターンを使用**

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // シーン特定の場合は DontDestroyOnLoad 不要
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        // シーン遷移で破棄される（DontDestroyOnLoad なし）
    }
    
    public void LoadScene(string sceneName)
    {
        // ゲーム状態操作
    }
}
```

[WARN] **DontDestroyOnLoad の使用は慎重に**

```csharp
// [NG] 不要な DontDestroyOnLoad
public class UICanvasManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);  // ❌ Canvas はシーン依存
    }
}

// [OK] 静的ユーティリティ化
public static class UICanvasManager
{
    public static void InitializeCanvasSettings() { }
}

// [OK] 本当に必要な場合のみ
public class AudioManager : MonoBehaviour
{
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);  // ✅ 音声再生は全シーン共通
    }
}
```

### Static Utility クラス（無状態）

[OK] **状態を持たない機能は static class で実装**

```csharp
// [OK] 無状態の static utility
internal static class FileUtility
{
    public static bool FileExists(string path)
    {
        return File.Exists(path);
    }
    
    public static string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }
}

// 使用例
if (FileUtility.FileExists("game.json"))
{
    string data = FileUtility.ReadAllText("game.json");
}
```

---

## Serialization パターン

### Inspector 公開

[OK] **変更可能な値は `[SerializeField]` で公開**

```csharp
public class TowerController : MonoBehaviour
{
    // [OK] Serializable な値
    [SerializeField] private float _fireRate = 2.0f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private Transform _shootPoint;
    
    // [NG] Logic を SerializeField にしない
    [SerializeField] private bool _isAttacking;  // ❌ 状態管理
}
```

### Custom Inspector

[OK] **複雑な値は CustomPropertyDrawer で可視化**

```csharp
[System.Serializable]
public struct TowerStats
{
    public float fireRate;
    public int damage;
    public float range;
}

public class TowerController : MonoBehaviour
{
    [SerializeField] private TowerStats _stats;  // [OK] 構造化
}
```

### Non-Serialized

[OK] **Serializable ではない値は明示的に `[System.NonSerialized]`**

```csharp
public class GameManager : MonoBehaviour
{
    [System.NonSerialized] public int _temporaryData;  // [OK] 非永続化
    private List<Tower> _activeTowers;                // [OK] Implicit
}
```

---

## Prefab パターン

### Prefab の命名規則

[OK] **Prefab は機能別に命名**

```
Assets/Prefabs/
├── UI/
│   ├── ButtonPrefab.prefab
│   ├── PanelPrefab.prefab
│   └── MessageBoxPrefab.prefab
├── Units/
│   ├── TowerSentryGuardPrefab.prefab
│   ├── EnemyLitterPrefab.prefab
│   └── ProjectileBulletPrefab.prefab
└── VFX/
    ├── ExplosionVFXPrefab.prefab
    └── DamageIndicatorVFXPrefab.prefab
```

### Prefab インスタンス化

[OK] **Prefab 読み込みは Manager を経由**

```csharp
// [OK] PrefabManager を使用
public class EnemySpawner : MonoBehaviour
{
    private void SpawnEnemy()
    {
        GameObject enemyPrefab = PrefabManager.GetPrefab("EnemyLitterPrefab");
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}

// [NG] 直接 Instantiate
private void SpawnEnemy()
{
    Resources.Load<GameObject>("Prefabs/EnemyLitterPrefab");  // ❌
}
```

---

## Canvas・UI パターン

### Canvas 設定の一元化

[OK] **すべての Canvas は UICanvasManager で設定**

```csharp
// [OK] 初期化時に一括設定
internal static class UICanvasManager
{
    internal static void InitializeCanvasSettings()
    {
        // 全 Canvas を探して設定
        Canvas[] allCanvas = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvas)
        {
            ApplyStandardScalerSettings(canvas);
        }
    }
    
    private static void ApplyStandardScalerSettings(Canvas canvas)
    {
        // WorldSpace Canvas はスキップ（3D UI）
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.Log($"Canvas '{canvas.gameObject.name}' uses WorldSpace - Skipped");
            return;
        }
        
        // ScreenSpace Canvas を設定
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
    }
}
```

[WARN] **WorldSpace Canvas を手動で設定しない**

```csharp
// [NG] WorldSpace Canvas を無視してスケーリング
Canvas waterTurretUI = GetComponent<Canvas>();
waterTurretUI.renderMode = RenderMode.ScreenSpaceOverlay;  // ❌ 壊れる

// [OK] WorldSpace は保持する
// UICanvasManager は自動的にスキップ
```

---

## Input パターン

### Input System 使用

[OK] **新しい Input System を使用**

```csharp
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;
    
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }
    
    private void OnMove(InputValue value)
    {
        Vector2 moveInput = value.Get<Vector2>();
        Move(moveInput);
    }
    
    private void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            Fire();
        }
    }
}
```

### Legacy Input の回避

[WARN] **`Input.GetKey()` は非推奨** - Input System を使用

```csharp
// [NG] Legacy Input
private void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))  // ❌
    {
        Jump();
    }
}

// [OK] Input System
private void OnJump(InputValue value)
{
    Jump();
}
```

---

## Physics パターン

### Rigidbody の使用

[OK] **物理演算が必要な場合のみ Rigidbody を使用**

```csharp
public class Projectile : MonoBehaviour
{
    [SerializeField] private float _force = 10.0f;
    private Rigidbody _rb;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    private void Fire(Vector3 direction)
    {
        _rb.velocity = direction * _force;
    }
}
```

### Collider と Layer

[OK] **Collider は責務別に Layer を分ける**

```
Layers:
- Default (terrain など)
- Towers (タワー衝突判定)
- Enemies (敵衝突判定)
- Projectiles (発射物衝突判定)
- Trigger (トリガーのみ)
```

---

## Pre-Commit チェックリスト

Unity 設計実装後、以下を確認：

- [ ] **MonoBehaviour 構造**: Awake → Start → Update の順序を理解
- [ ] **Singleton 安全性**: DontDestroyOnLoad は必要か判定済み
- [ ] **Coroutine**: WaitForSeconds をキャッシュ化済み
- [ ] **Serialization**: 適切に SerializeField で公開
- [ ] **Prefab 管理**: Manager 経由で読み込み
- [ ] **Canvas**: WorldSpace 検出・保持機能あり
- [ ] **Input System**: Legacy Input 未使用
- [ ] **Physics**: Layer・Collider 適切に設定

---

**関連資料**:
- [coding-csharp.md](coding-csharp.md) - C# コーディング規約
- [naming-conventions.md](naming-conventions.md) - 命名規則
- [folder-structure.md](folder-structure.md) - フォルダ構成
