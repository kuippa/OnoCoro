# Recovery フェーズ開発ガイドライン

**目的**: 2 年のバックアップから復元されたコード・データの安全な開発・保守ガイド

---

## Recovery フェーズの特徴

OnoCoro は **2 年前の SSD バックアップから復元された** ため、以下の特性があります：

| 特性 | 説明 | 対応方法 |
|------|------|--------|
| **古い参照** | 削除されたファイル・クラスの参照が残存 | Null チェック必須 |
| **不完全な状態** | 初期化されていないコンポーネント | Defensive programming |
| **データ不整合** | Serialize されたオブジェクト参照の破損 | 参照再構築スクリプト |
| **パフォーマンス未最適化** | 2 年前の最適化レベル | 段階的改善 |

---

## Defensive Programming（防御的プログラミング）

### 原則：すべての参照は null の可能性がある

[WARN] **`GetComponent` / `Find` / `transform.Find` の結果は必ず null チェック**

```csharp
// [NG] null チェックなし（Recovery では危険）
RainAbsorbCtrl rainAbsorb = absorbCollider.GetComponent<RainAbsorbCtrl>();
rainAbsorb.Absorb();  // ❌ rainAbsorb が null なら NullReferenceException

// [OK] null チェック必須
RainAbsorbCtrl rainAbsorb = absorbCollider.GetComponent<RainAbsorbCtrl>();
if (rainAbsorb == null)
{
    Debug.LogWarning($"RainAbsorbCtrl not found on {absorbCollider.name}");
    return;
}
rainAbsorb.Absorb();  // ✅ 安全
```

### Transform 検索の多段階チェック

[WARN] **中間の GameObject も null チェック**

```csharp
// [NG] 多段階の参照でどこかが null の可能性
Transform childTransform = transform.Find("child");
RainAbsorbCtrl controller = childTransform.GetComponent<RainAbsorbCtrl>();
controller.Absorb();  // ❌ childTransform が null なら失敗

// [OK] 各ステップで null チェック
Transform childTransform = transform.Find("child");
if (childTransform == null)
{
    Debug.LogWarning("'child' object not found");
    return;
}

RainAbsorbCtrl controller = childTransform.GetComponent<RainAbsorbCtrl>();
if (controller == null)
{
    Debug.LogWarning($"RainAbsorbCtrl not found on {childTransform.name}");
    return;
}

controller.Absorb();  // ✅ 安全
```

### GetComponent の一般化（避けるべき）

[WARN] **型を指定して GetComponent 呼び出し**

```csharp
// [NG] 文字列で型指定（エラーが実行時に発見される）
Collider collider = (Collider)GetComponent("BoxCollider");

// [OK] ジェネリック型で指定（型安全）
BoxCollider boxCollider = GetComponent<BoxCollider>();
if (boxCollider == null)
{
    Debug.LogWarning("BoxCollider not found");
    return;
}
```

---

## Early Return パターン（深いネストを避ける）

### フラット化構造

[OK] **Guard clause で早期リターン**

```csharp
// [NG] 深いネスト（Recovery コードの保守が困難）
private void ProcessTower(Tower tower)
{
    if (tower != null)
    {
        if (tower.IsActive)
        {
            if (tower.Fuel > 0)
            {
                if (tower.Target != null)
                {
                    // 実処理（5階層目）
                    tower.Fire();
                }
            }
        }
    }
}

// [OK] Early Return で平坦化
private void ProcessTower(Tower tower)
{
    // [CHECK 1] tower 自体
    if (tower == null)
    {
        Debug.LogWarning("Tower is null");
        return;
    }
    
    // [CHECK 2] tower.IsActive
    if (!tower.IsActive)
    {
        return;  // 非アクティブなら処理しない
    }
    
    // [CHECK 3] tower.Fuel
    if (tower.Fuel <= 0)
    {
        return;  // 燃料なしなら処理しない
    }
    
    // [CHECK 4] tower.Target
    if (tower.Target == null)
    {
        return;  // ターゲットなしなら処理しない
    }
    
    // [PROCESS] 実処理（1階層のみ）
    tower.Fire();
}
```

---

## マジックナンバー・文字列の定数化

### 復旧困難な "失われた定義"

Recovery フェーズでは、定数定義が失われている可能性があります。

[WARN] **すべてのマジックナンバー・文字列を定数化して保守性向上**

```csharp
// [NG] マジックナンバー・文字列（定義が失われた可能性）
void SpawnTower()
{
    GameObject tower = Resources.Load<GameObject>("Prefabs/Tower/Tier2Tower");
    tower.SetActive(true);
    tower.transform.position = new Vector3(10, 0, 20);
}

// [OK] 定数化して見通し良く
private const string TOWER_PREFAB_PATH = "Prefabs/Tower/Tier2Tower";
private const float DEFAULT_SPAWN_X = 10f;
private const float DEFAULT_SPAWN_Y = 0f;
private const float DEFAULT_SPAWN_Z = 20f;

void SpawnTower()
{
    GameObject tower = Resources.Load<GameObject>(TOWER_PREFAB_PATH);
    if (tower == null)
    {
        Debug.LogWarning($"Tower prefab not found: {TOWER_PREFAB_PATH}");
        return;
    }
    
    tower.SetActive(true);
    tower.transform.position = new Vector3(
        DEFAULT_SPAWN_X,
        DEFAULT_SPAWN_Y,
        DEFAULT_SPAWN_Z
    );
}
```

---

## Serialized Field の参照破損対応

### 症状：Inspector で赤い警告

[SYMPTOM] **Serialized field が Missing Reference**

```
The referenced script on this Behaviour (Game Object 'Tower') is missing!
```

### 原因

1. **クラス名が変更された** - 例: `TowerCtrl` → `TowerController`
2. **ファイルが削除された** - Serialize された参照先が存在しない
3. **Namespace が変更された** - クラスの完全修飾名が変わった

### 対応手順

[STEP 1] **参照元のスクリプトを特定**

```csharp
// Inspector で Missing Reference のオブジェクトを選択
// 該当スクリプトを確認
```

[STEP 2] **参照先を重新構築**

```csharp
// [OK] Awake で遅延初期化
public class TowerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _towerPrefab;  // ← Missing の可能性
    private GameObject _cachedTowerPrefab;
    
    private void Awake()
    {
        // [RECOVERY] Inspector の参照が失われていない確認
        if (_towerPrefab == null)
        {
            Debug.LogWarning("Tower prefab reference is missing, attempting to load from resources");
            _cachedTowerPrefab = Resources.Load<GameObject>("Prefabs/Tower/TowerPrefab");
        }
        else
        {
            _cachedTowerPrefab = _towerPrefab;
        }
        
        if (_cachedTowerPrefab == null)
        {
            Debug.LogError("Failed to load tower prefab from both Inspector and Resources");
            enabled = false;
        }
    }
}
```

[STEP 3] **スクリプト再割り当て**

```
1. Inspector で Missing Reference オブジェクトを選択
2. スクリプトコンポーネントの歯車アイコン → Remove Component
3. 新しいスクリプトをドラッグして追加
4. ゲーム実行して確認
```

---

## Compile Errors 対応

### " Reference 'X' could not be resolved" エラー

[SYMPTOM] **存在しないクラス・Namespace への参照**

```csharp
// [NG] 存在しないクラス
using GameUtils;        // ← このファイルが削除されている可能性
GameUtils.Helper.DoSomething();  // ❌ Compile Error
```

### 対応

[STEP 1] **参照元を特定**

```powershell
# 該当するファイルが存在するか確認
Get-ChildItem -Recurse -Filter "GameUtils.cs"
```

[STEP 2] **新しいパスで import**

```csharp
// [OK] 正しい Namespace に変更
using CommonsUtility;  // OnoCoro では統一 Namespace

// または該当ファイルの新しい場所を特定
using CommonsUtility.Utilities;
```

[STEP 3] **コンパイル再実行**

```powershell
# VS Code でファイル保存 → Unity が自動再コンパイル
```

---

## デバッグ・ログ出力

### 復旧プロセスのトラッキング

[OK] **Debug.Log で各初期化ステップをトラック**

```csharp
public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[RECOVERY] GameInitializer.Awake() started");
        
        // Step 1
        InitializeManagers();
        Debug.Log("[RECOVERY] Managers initialized");
        
        // Step 2
        InitializeUIComponents();
        Debug.Log("[RECOVERY] UI components initialized");
        
        // Step 3
        LoadGameData();
        Debug.Log("[RECOVERY] Game data loaded");
        
        Debug.Log("[RECOVERY] Initialization complete");
    }
}
```

### Warning・Error ログの分類

[OK] **ログレベルを明確に**

```csharp
// [OK] Warning レベル - 処理継続（参照が失われた etc）
Debug.LogWarning("Tower prefab reference is missing, using fallback");

// [OK] Error レベル - 処理中断（致命的エラー）
Debug.LogError("Failed to initialize game - cannot continue");

// [OK] Info レベル - トレース（初期化進度）
Debug.Log("[INIT] Phase 1 complete");
```

---

## テスト・検証

### Play Mode テスト

[STEP] **毎回の実装後に確認すべき項目**

```
1. Unity Editor で Play を開始
2. Console パネルで Error・Warning がないか確認
3. ゲーム画面が正常に表示されるか確認
4. 主要機能（スポーン、ゲーム進行）が動作するか確認
5. Performance Profiler で FPS・GC を確認
```

### 自動テスト（UnitTest）

[OK] **Recovery 用テストスクリプト**

```csharp
// Assets/Scripts/UnitTest/RecoveryTest.cs
public class RecoveryTest : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void ValidateRecovery()
    {
        // [TEST 1] GameManager 初期化確認
        if (GameManager.Instance == null)
        {
            Debug.LogError("[RECOVERY] GameManager not initialized");
            return;
        }
        
        // [TEST 2] PrefabManager 確認
        GameObject tower = PrefabManager.GetPrefab("TowerPrefab");
        if (tower == null)
        {
            Debug.LogError("[RECOVERY] TowerPrefab not found");
            return;
        }
        
        // [TEST 3] シーンロード確認
        try
        {
            SceneLoaderManager.LoadScene("MainGame");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[RECOVERY] Scene load failed: {ex.Message}");
        }
        
        Debug.Log("[RECOVERY] Validation complete");
    }
}
```

---

## チェックリスト

Recovery フェーズ開発時：

- [ ] **null チェック**: GetComponent / Find の結果すべて確認
- [ ] **Early Return**: ネストが 2 階層以下
- [ ] **定数化**: マジックナンバー・文字列なし
- [ ] **Serialized Field**: Missing Reference なし（Inspector 確認）
- [ ] **Compile Error**: すべて解決済み
- [ ] **ログ出力**: 初期化プロセスをトラッキング
- [ ] **Play Mode テスト**: ゲーム起動・進行が正常
- [ ] **Console**: Error・Warning がない（許容範囲内）

---

## ベストプラクティス集

### 参照の安全な取得

```csharp
// [PATTERN 1] GetComponent + null check
T component = GetComponent<T>();
if (component == null)
{
    Debug.LogWarning($"{typeof(T).Name} not found");
    return;
}

// [PATTERN 2] GetOrAddComponent
T component = GetComponent<T>();
if (component == null)
{
    component = gameObject.AddComponent<T>();
}

// [PATTERN 3] Find + 連鎖 null check
Transform child = transform.Find("childName");
if (child != null)
{
    T component = child.GetComponent<T>();
}
```

### 初期化パターン

```csharp
// [PATTERN] Awake/Start の責務分離
public class MyComponent : MonoBehaviour
{
    private void Awake()
    {
        // [OK] ローカル参照の取得・検証
        ValidateLocalReferences();
    }
    
    private void Start()
    {
        // [OK] 他のコンポーネント・シーンの初期化を待つ
        InitializeAfterOtherComponents();
    }
    
    private void ValidateLocalReferences()
    {
        // GetComponent チェック
    }
    
    private void InitializeAfterOtherComponents()
    {
        // FindObjectOfType / シーン参照の取得
    }
}
```

---

**関連資料**:
- [initialization-flow.md](initialization-flow.md) - 初期化フロー
- [project-rules/coding-csharp.md](../project-rules/coding-csharp.md) - C# コーディング規約
- [project-rules/unity-design-patterns.md](../project-rules/unity-design-patterns.md) - MonoBehaviour パターン
- [AGENTS.md](../../AGENTS.md) - プロジェクト全体ルール
