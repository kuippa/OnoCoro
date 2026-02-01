# 初期化フロー・段階的初期化

**目的**: ゲーム起動から Play までの初期化順序・タイミング・依存関係の明確化

---

## 初期化フロー全体図

```
ゲーム起動
    ↓
[Phase 0] ビルドパイプライン完了 (Unity自動)
    ↓
[Phase 1] リソースローダー初期化
    - SceneLoaderManager 初期化
    - PrefabManager 初期化
    ↓
[Phase 2] マネージャー初期化
    - GameSpeedManager 初期化
    - ConfigManager 初期化
    - InitializationManager が IInitializable を実行
    ↓
[Phase 3] UI コンポーネント初期化
    - UICanvasManager 初期化 (Canvas Scaler 設定)
    - FontManager 初期化 (Canvas 設定完了後)
    - HUD 初期化
    ↓
ゲーム Play 開始
```

---

## Phase 0: ビルドパイプライン（Unity 自動）

[NOTE] Unity エンジン自動実行。開発者は制御不可

### 実行内容

- Scene ロード
- GameObject 生成
- `Awake()` メソッド呼び出し（シーン内すべてのスクリプト）
- `OnEnable()` メソッド呼び出し

### 順序保証

[WARN] **Phase 0 内での順序は保証されない**

```csharp
// [NG] Phase 0 で順序を期待してはいけない
public class GameObject1 : MonoBehaviour
{
    private void Awake()
    {
        // GameObject2.Awake() が先かどうか不確定
        // GameManager が初期化されているか不確定
    }
}
```

---

## Phase 1: リソースローダー初期化

[WARN] **Phase 0 の Awake() 内で実行開始**

### 実行内容

| コンポーネント | 用途 | 実行契機 |
|-------------|------|--------|
| **SceneLoaderManager** | シーン遷移管理 | GameManager.Awake() |
| **PrefabManager** | Prefab キャッシング | GameManager.Awake() |

### 実装例

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        // Phase 1: リソースローダー初期化
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // PrefabManager が使用可能にする
        InitializePrefabManager();
    }
    
    private void InitializePrefabManager()
    {
        // PrefabManager キャッシング初期化
        // 以降 PrefabManager.GetPrefab() が使用可能
    }
}
```

### 依存関係

[OK] **Phase 1 は独立** - 他のコンポーネントに依存しない

```csharp
// [OK] Phase 1 内での依存関係
PrefabManager.GetPrefab("TowerPrefab");  // 相互依存なし
SceneLoaderManager.LoadScene("MainGame");
```

---

## Phase 2: マネージャー初期化

[WARN] **Phase 1 完了後、`Start()` 内で実行**

### 実行内容

| コンポーネント | 用途 | 依存関係 |
|-------------|------|--------|
| **ConfigManager** | ゲーム設定・言語設定 | Phase 1 完了後 |
| **GameSpeedManager** | ゲーム速度管理 | ConfigManager 依存 |
| **IInitializable 実装** | 初期化インターフェース | GameManager が調整 |

### 実装例

```csharp
public class InitializationManager : MonoBehaviour
{
    private void Awake()
    {
        // Phase 1 完了待機
    }
    
    private void Start()
    {
        // Phase 2: マネージャー初期化
        StartCoroutine(InitializeManagers());
    }
    
    private IEnumerator InitializeManagers()
    {
        // ConfigManager 初期化（言語、設定ロード）
        InitializeConfigManager();
        yield return new WaitForEndOfFrame();
        
        // GameSpeedManager 初期化（ConfigManager 依存）
        InitializeGameSpeedManager();
        yield return new WaitForEndOfFrame();
        
        // IInitializable インターフェース実行
        InitializeIInitializableComponents();
        yield return new WaitForEndOfFrame();
        
        // Phase 3 へ遷移
        yield return StartCoroutine(InitializeUIComponents());
    }
}
```

### 依存順序（重要）

[WARN] **ConfigManager → GameSpeedManager → IInitializable の順守**

```csharp
// [NG] 逆順で初期化（設定がない）
InitializeGameSpeedManager();  // ❌ ConfigManager 未初期化
InitializeConfigManager();

// [OK] 正順で初期化
InitializeConfigManager();
InitializeGameSpeedManager();  // ✅ ConfigManager 初期化済み
```

---

## Phase 3: UI コンポーネント初期化

[WARN] **Phase 2 完了後、`Start()` 内で実行**

### 実行内容

| コンポーネント | 用途 | 依存関係 |
|-------------|------|--------|
| **UICanvasManager** | Canvas Scaler 一元設定 | Phase 2 完了後 |
| **FontManager** | フォント設定 | UICanvasManager 完了後 |
| **HUD** | ゲーム中 UI 初期化 | FontManager 完了後 |

### 実装例

```csharp
public class InitializationManager : MonoBehaviour
{
    private IEnumerator InitializeUIComponents()
    {
        // Phase 3: UI コンポーネント初期化
        
        // Step 1: Canvas Scaler 統一設定
        InitializeCanvasSettings();
        yield return new WaitForEndOfFrame();
        
        // Step 2: フォント初期化（Canvas 設定完了後）
        // TODO: FontManager 実装時に実行
        // FontManager.InitializeFonts();
        // yield return new WaitForEndOfFrame();
        
        // Step 3: HUD 初期化
        InitializeHUD();
        yield return new WaitForEndOfFrame();
        
        Debug.Log("Phase 3 UI initialization complete");
    }
    
    private void InitializeCanvasSettings()
    {
        // 全 Canvas に統一された Scaler 設定を適用
        UICanvasManager.InitializeCanvasSettings();
    }
    
    private void InitializeHUD()
    {
        // ゲーム中の常時表示 UI 初期化
        // 例: ゲームタイマー、スコア表示
    }
}
```

### Canvas Scaler 設定の注意点

[WARN] **WorldSpace Canvas は自動スキップ**

```csharp
// [OK] UICanvasManager が自動判定
internal static class UICanvasManager
{
    internal static void InitializeCanvasSettings()
    {
        Canvas[] allCanvas = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvas)
        {
            ApplyStandardScalerSettings(canvas);
        }
    }
    
    private static void ApplyStandardScalerSettings(Canvas canvas)
    {
        // WorldSpace Canvas はスキップ（3D UI の表示を保持）
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.Log($"Canvas '{canvas.gameObject.name}' uses WorldSpace - Skipped");
            return;
        }
        
        // ScreenSpace Canvas のみ設定
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
    }
}
```

---

## 初期化順序の依存関係マトリックス

| コンポーネント | 依存先 | 説明 |
|-------------|--------|------|
| **GameManager** | (なし) | 最初に初期化 |
| **PrefabManager** | GameManager | リソース読み込み前提 |
| **SceneLoaderManager** | GameManager | リソース読み込み前提 |
| **ConfigManager** | PrefabManager | Phase 1 完了後 |
| **GameSpeedManager** | ConfigManager | 設定値参照 |
| **IInitializable** | GameSpeedManager | ゲーム状態が確定後 |
| **UICanvasManager** | IInitializable | ゲーム状態確定後 |
| **FontManager** | UICanvasManager | Canvas 設定完了後 |
| **HUD** | FontManager | フォント設定完了後 |

---

## 新規コンポーネント追加時の判定

新しいマネージャー・システムを追加する場合：

### Step 1: 依存関係確認

[STEP] **どのコンポーネントに依存しているか**

```csharp
public class MyNewManager : MonoBehaviour
{
    // ConfigManager に依存? → Phase 2
    // PrefabManager に依存? → Phase 1 後
    // FontManager に依存? → Phase 3
}
```

### Step 2: 適切な Phase に配置

| 条件 | Phase |
|------|-------|
| リソース読み込みのみ | 1 |
| 設定・状態管理 | 2 |
| UI・表示関連 | 3 |

### Step 3: InitializationManager に登録

```csharp
// [OK] Phase 2 に新規マネージャーを追加
private IEnumerator InitializeManagers()
{
    InitializeConfigManager();
    yield return new WaitForEndOfFrame();
    
    InitializeGameSpeedManager();
    yield return new WaitForEndOfFrame();
    
    // [NEW] 新規マネージャー追加
    InitializeMyNewManager();
    yield return new WaitForEndOfFrame();
    
    // 以降の Phase へ
    yield return StartCoroutine(InitializeUIComponents());
}
```

---

## デバッグ・トラブルシューティング

### "コンポーネント X が見つかりません" エラー

[STEP] **初期化順序を確認**

```csharp
// [NG] 依存先が初期化されていない
MyNewManager myManager = GetComponent<MyNewManager>();
myManager.UseConfigManager();  // ❌ ConfigManager 未初期化

// [OK] 依存先の初期化を待つ
private IEnumerator InitializeMyNewManager()
{
    yield return StartCoroutine(WaitForConfigManager());  // 待機
    
    MyNewManager myManager = GetComponent<MyNewManager>();
    myManager.UseConfigManager();  // ✅ ConfigManager 初期化済み
}
```

### "WorldSpace Canvas が変更される" 問題

[STEP] **UICanvasManager のスキップ logic を確認**

```csharp
// [OK] WorldSpace Canvas 保持確認
if (canvas.renderMode == RenderMode.WorldSpace)
{
    Debug.Log($"Skipped: {canvas.gameObject.name}");
    return;  // スキップして renderMode を変更しない
}
```

---

## チェックリスト

初期化フロー実装時：

- [ ] **Phase 順序**: Phase 1 → 2 → 3 の順守
- [ ] **依存関係**: マトリックスで依存先確認
- [ ] **null チェック**: 各初期化ステップで null チェック
- [ ] **ログ出力**: 各 Phase 開始・完了でログ
- [ ] **Coroutine**: `yield return new WaitForEndOfFrame()` で同期化
- [ ] **WorldSpace Canvas**: UICanvasManager が自動スキップ確認
- [ ] **エラーハンドリ**: 初期化失敗時の fallback 処理

---

**関連資料**:
- [ui-system.md](ui-system.md) - Canvas・UICanvasManager 詳細
- [asset-management.md](asset-management.md) - PrefabManager 詳細
- [project-rules/unity-design-patterns.md](../project-rules/unity-design-patterns.md) - MonoBehaviour パターン
- [recovery-guidelines.md](recovery-guidelines.md) - 防御的プログラミング
