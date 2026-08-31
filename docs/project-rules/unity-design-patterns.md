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

### Update を避ける（重要）

**`MonoBehaviour.Update` は極力使わない。**

理由は 2 つある。

1. **倍速・一時停止に追従しない**
   本プロジェクトは `GameSpeedManager` が `Time.timeScale` を操作する。
   `Update` 内で自前に経過時間を数えると、倍速対応を個別に実装することになり
   実装漏れが起きる。`WaitForSeconds` は `Time.timeScale` に従うため、
   **倍速も一時停止も自動で正しく動く**
2. **毎フレームに処理が集中する**
   Update を持つコンポーネントが増えるほど 1 フレームの負荷が積み上がる

```csharp
// [NG] Update で自前にタイマーを回す
private float _timer = 0f;
private void Update()
{
    _timer += Time.deltaTime;
    if (_timer < 0.5f) { return; }
    _timer = 0f;
    CheckSomething();
}

// [OK] コルーチンで間隔を作る（倍速・一時停止に自動追従）
private void Start()
{
    StartCoroutine(MonitorLoop());
}

private IEnumerator MonitorLoop()
{
    WaitForSeconds interval = new WaitForSeconds(0.5f);
    while (true)
    {
        yield return interval;
        CheckSomething();
    }
}
```

| 用途 | 使うもの |
|------|---------|
| 一定間隔の監視・定期処理 | コルーチン + `WaitForSeconds` |
| 大量オブジェクトの分散処理 | コルーチンでキューを小分けに消化 |
| 他システムと足並みを揃える処理 | 既存の統合ループから呼ぶ |

[NOTE] 入力受付・カメラ追従など、毎フレームでなければ成立しない処理は Update でよい。

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

### Collider トリガー（OnTriggerEnter/Exit）の注意点

[NOTE] **複数 Collider での多重発火に注意**

Unity の物理エンジンは、**Collider の組み合わせごとに OnTriggerEnter/Exit を独立して発火** します。

#### 問題が起こるシナリオ

```
シナリオ：Player が SignboardCtrl のトリガーに出入りする

Player（複数 Collider を持つ）
  ├── Capsule (移動用)
  └── Sphere (トリガー判定用)

SignboardCtrl のトリガー Collider
  └── Box (トリガー範囲)

動作フロー：
・Player Capsule がトリガーに進入 → OnTriggerEnter 発火
・Player Sphere がトリガーに進入 → OnTriggerEnter 再発火 ← 複数回！

カウント実装だとこうなる：
  count++  // Capsule enter
  count++  // Sphere enter
  count--  // Capsule exit
  count--  // Sphere exit より先に別の物体が進入
  → count が 1 のまま → 状態不一致！
```

#### 危険なパターン [NG]

```csharp
private int _playerInTriggerCount = 0;  // カウンター方式

private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        _playerInTriggerCount++;  // [NG] インクリメント
        if (_playerInTriggerCount == 1)
        {
            ShowBoard();
        }
    }
}

private void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Player"))
    {
        _playerInTriggerCount--;  // [NG] デクリメント
        if (_playerInTriggerCount == 0)
        {
            HideBoard();  // タイミング問題で起こる
        }
    }
}
// 複数発火 + 遅延処理でカウント不一致が発生しやすい
```

#### 推奨パターン [OK]

```csharp
private HashSet<Collider> _playersInTrigger = new HashSet<Collider>();

private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        // 既存のコルーチンをキャンセル（再進入対応）
        CancelToggleBoardOffCoroutine();
        
        bool isFirstPlayer = _playersInTrigger.Count == 0;
        _playersInTrigger.Add(other);  // [OK] Set に追加（冪等性あり）
        
        if (isFirstPlayer)  // 最初の進入だけ処理
        {
            SetBoardState(true, true);
        }
    }
}

private void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Player"))
    {
        _playersInTrigger.Remove(other);  // [OK] Set から削除（冪等性あり）
        
        if (_playersInTrigger.Count == 0)  // 誰もいなくなったら処理
        {
            StartDelayedToggleBoardOff();
        }
    }
}

private void CancelToggleBoardOffCoroutine()
{
    if (_toggleBoardOffCoroutine != null)
    {
        StopCoroutine(_toggleBoardOffCoroutine);
        _toggleBoardOffCoroutine = null;
    }
}

private void StartDelayedToggleBoardOff()
{
    CancelToggleBoardOffCoroutine();
    _toggleBoardOffCoroutine = StartCoroutine(DelayedToggleBoardOff());
}
```

#### 重要な概念：冪等性

**冪等性** = 何度実行しても同じ結果になる性質

- `HashSet.Add(x)`: 既に存在すれば何もしない → 冪等性 [OK]
- `int count++`: 実行するたびに増える → 冪等性なし [NG]

#### ベストプラクティス

| 要件 | パターン | 理由 |
|------|---------|------|
| 単一進入時の動作 | `Count == 0 → Count > 0` | 冪等性あり |
| 最終離脱時の動作 | `Count > 0 → Count == 0` | 冪等性あり |
| 再進入対応 | 既存コルーチンをキャンセル | 遅延処理中の再適用を防ぐ |
| データ構造 | `HashSet<Collider>` | 重複排除 + 冪等性 |

#### 実装チェックリスト

- [ ] **複数 Collider 対応**: Player が複数 Collider を持つことを想定済み
- [ ] **冪等性**: インクリメンタルなカウンター未使用
- [ ] **コルーチン管理**: 二重開始・キャンセル漏れなし
- [ ] **再進入対応**: OnTriggerEnter で既存コルーチンをキャンセル
- [ ] **状態判定**: `Count == 0` など条件が明確

### Physics.Raycast と トリガーコライダーの相互作用

[NOTE] **QueryTriggerInteraction パラメータを明示的に指定**

`Physics.Raycast` は、デフォルトでは **トリガーコライダーを無視** しますが、LayerMask との組み合わせによって予期しない挙動が起こります。

#### 問題が起こるシナリオ

```
シナリオ：UI から Raycast でオブジェクトをクリック選択

InfoWindow (UI) の位置に トリガーコライダーが存在
  → マウスクリック時に Raycast が トリガーコライダーに引っかかる
  → 意図した背後のオブジェクトが選択されない

典型的なコード [NG]：
  Ray ray = Camera.main.ScreenPointToRay(mousePos);
  if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
  {
      // トリガーコライダーが hit した場合もここに入る
      SelectObject(hit.collider.gameObject);
  }
```

#### QueryTriggerInteraction の選択肢

| 値 | 説明 | 用途 |
|---|-----|------|
| **Ignore** | トリガーコライダーを無視 | UI クリック・NPC 選択（推奨） |
| **Collide** | トリガーコライダーにも衝突判定 | 範囲判定・エリア検出 |
| **UseGlobal** | RigidBody.isKinematic 設定に従う | 使用場面が限定的 |

#### 危険なパターン [NG]

```csharp
private void GetTargetUnit()
{
    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    
    // [NG] QueryTriggerInteraction を明示していない
    if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
    {
        // トリガーコライダーに引っかかってしまう
        ProcessHit(hit);
    }
}
```

#### 推奨パターン [OK]

```csharp
private void GetTargetUnit()
{
    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    int layerMask = ~LayerMask.GetMask(GameEnum.LayerType.AreaIgnoreRaycast.ToString());
    
    // [OK] QueryTriggerInteraction.Ignore を明示的に指定
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask, QueryTriggerInteraction.Ignore))
    {
        UnitStruct? unitStruct = GetUnitStruct(hit.collider.gameObject);
        if (SetInfo(unitStruct))
        {
            ToggleInfoWindow(isActive: true);
        }
    }
}
```

#### 実装チェックリスト

- [ ] **QueryTriggerInteraction 明示**: `Ignore` / `Collide` / `UseGlobal` を明確に指定
- [ ] **LayerMask 確認**: 不可視レイヤーが正しく除外されているか
- [ ] **UI オブジェクト**: トリガーコライダーを持つ UI には Physics.Raycast を使わない
- [ ] **背後のオブジェクト選択**: `QueryTriggerInteraction.Ignore` でトリガーを透視

---

## NavMeshAgent パターン

### [NG] テレポート時に transform.position を直接書き換えない

NavMeshAgent がアタッチされたオブジェクトの位置を直接変更すると、
エージェントが NavMesh に再登録されるのは**次フレーム以降**になります。
呼び出し直後に `SetDestination` を呼ぶと `isOnNavMesh = false` のまま失敗します。

```csharp
// [NG] 直後の SetDestination が isOnNavMesh=false で失敗する
transform.position = targetPosition;
navMeshAgent.SetDestination(destination);  // ← 失敗

// [OK] Warp() は呼び出し直後に isOnNavMesh=true になる
bool warped = navMeshAgent.Warp(targetPosition);
if (!warped)
{
    Debug.LogWarning("NavMesh surface not found near: " + targetPosition);
}
navMeshAgent.SetDestination(destination);  // ← 成功
```

[NOTE] `Warp()` は戻り値 `false` の場合、その座標周辺に NavMesh が存在しません。
NavMesh Bake 範囲を Inspector で確認してください。

---

### スポーン位置とコライダー底面オフセット

Prefab のピボット（`transform.position` の基準）が中心にある場合、
パスマーカー座標をそのまま `transform.position` に設定すると
オブジェクトが地面に半分埋まります。
コライダー情報から底面オフセットを計算して補正してください。

```csharp
/// <summary>
/// ピボットからコライダー底面までのオフセットを返す。
/// スポーン位置がコライダー底面に来るよう transform.position を補正するために使用。
/// </summary>
private float GetBottomOffset()
{
    CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
    if (capsuleCollider != null)
    {
        // ピボットから底面 = height/2 - center.y
        return capsuleCollider.height / 2f - capsuleCollider.center.y;
    }

    BoxCollider boxCollider = GetComponent<BoxCollider>();
    if (boxCollider != null)
    {
        // ピボットから底面 = size.y/2 - center.y
        return boxCollider.size.y / 2f - boxCollider.center.y;
    }

    Debug.LogWarning(name + ": Collider not found. bottomOffset=0");
    return 0f;
}

// スポーン時の使用例
private void WarpToSpawnPosition(Vector3 spawnPosition)
{
    float bottomOffset = GetBottomOffset();
    Vector3 warpPosition = new Vector3(
        spawnPosition.x,
        spawnPosition.y + bottomOffset,
        spawnPosition.z
    );
    bool warped = _navMeshAgent.Warp(warpPosition);
    if (!warped)
    {
        Debug.LogWarning(name + ": Warp failed at " + warpPosition);
    }
}
```

**`bottomOffset` の値と意味**:

| Collider 設定 | center.y | bottomOffset | 意味 |
|-------------|----------|-------------|------|
| height=1.8, center.y=0.9 | 0.9 | 0.0 | ピボットが既に足元 |
| height=1.8, center.y=0.0 | 0.0 | 0.9 | ピボットが中心 → 0.9 上に補正 |
| height=1.8, center.y=0.5 | 0.5 | 0.4 | 部分的に補正 |

[NOTE] Unity の Humanoid キャラは `center.y = height/2` が標準（ピボットが足元）のため
bottomOffset=0 になります。カスタム Prefab では必ず Inspector で確認してください。

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
- [ ] **NavMeshAgent テレポート**: `transform.position` 直接書き換えでなく `Warp()` を使用
- [ ] **スポーン位置**: コライダー底面オフセットを計算して埋まり込みを防止

---

**関連資料**:
- [coding-csharp.md](coding-csharp.md) - C# コーディング規約
- [naming-conventions.md](naming-conventions.md) - 命名規則
- [folder-structure.md](folder-structure.md) - フォルダ構成
