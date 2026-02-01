# UI System（Canvas・解像度対応）

**目的**: Canvas スケーラー・マルチ解像度対応・WorldSpace UI 保持の実装

---

## UI System 全体図

```
ゲーム起動
    ↓
Phase 3: UICanvasManager.InitializeCanvasSettings()
    ↓
[Loop] 全 Canvas を検出
    ├─ WorldSpace Canvas → スキップ（3D UI を保持）
    └─ ScreenSpace Canvas → Scaler 設定適用
         ├─ Reference Resolution: 1920×1080 (Full HD)
         ├─ Scale Mode: ScaleWithScreenSize
         └─ Screen Match Mode: Expand (または MatchWidthOrHeight)
    ↓
ゲーム Play 開始
```

---

## Canvas スケーラー設定

### Reference Resolution（基準解像度）

[WARN] **すべての ScreenSpace Canvas は 1920×1080 を基準に設定**

```csharp
// [OK] Reference Resolution 1920×1080 で統一
private const float REFERENCE_RESOLUTION_WIDTH = 1920f;
private const float REFERENCE_RESOLUTION_HEIGHT = 1080f;

internal static void ApplyStandardScalerSettings(Canvas canvas)
{
    CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(
        REFERENCE_RESOLUTION_WIDTH,
        REFERENCE_RESOLUTION_HEIGHT
    );
}
```

### Scale Mode: ScaleWithScreenSize

[OK] **UI は画面サイズに応じて自動スケーリング**

```csharp
// スクリーンサイズが 1920×1080 より小さい場合
// UI 要素も自動的に縮小（または拡大）される
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

// 例: 1280×720 の場合
// Scale = 1280 / 1920 ≈ 0.67 または 720 / 1080 ≈ 0.67
// すべての UI が約 67% サイズで表示される
```

### Screen Match Mode（選択可能）

| Mode | 説明 | 用途 |
|------|------|------|
| **Expand** | 短辺に合わせて拡大（黒枠可能性あり） | 安全な余白でコンテンツ保護 |
| **MatchWidthOrHeight** | 幅または高さに合わせる（選択可能） | アスペクト比が固定の場合 |
| **Shrink** | 長辺に合わせて縮小（切り取り可能性） | 没入感重視（リスク高） |

[RECOMMENDED] **OnoCoro では Expand 推奨**（UI 保護）

```csharp
// [OK] Expand モード（UI 全体が画面に収まる）
scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

// [ALTERNATIVE] MatchWidthOrHeight（幅を優先する例）
scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
scaler.matchWidthOrHeight = 0f;  // 0 = 幅を優先, 1 = 高さを優先
```

---

## WorldSpace Canvas（3D UI）の保持

### 概要

[WARN] **WorldSpace Canvas は 3D シーン内に配置される UI**

```
例: WaterTurret の SignPowerOutage UI
- RenderMode: WorldSpace（3D オブジェクト扱い）
- 位置・回転: Transform で制御（ScreenSpace ではない）
- 表示形式: 3D オブジェクトとして Scene に配置
```

### スキップ Logic

[OK] **UICanvasManager は自動的に WorldSpace Canvas をスキップ**

```csharp
internal static void ApplyStandardScalerSettings(Canvas canvas)
{
    // Step 1: WorldSpace Canvas を検出
    if (canvas.renderMode == RenderMode.WorldSpace)
    {
        Debug.Log($"Canvas '{canvas.gameObject.name}' uses WorldSpace - Skipped");
        return;  // 以降の処理をスキップ（renderMode を変更しない）
    }
    
    // Step 2: ScreenSpace Canvas のみ設定
    CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920f, 1080f);
}
```

### WorldSpace Canvas の手動配置例

```
SignPowerOutage (GameObject)
├─ Canvas (WorldSpace, スケーラー設定不要)
│  └─ Image (UI 要素)
│     └─ Text (テキスト要素)
└─ RectTransform (親は Canvas)
```

[NOTE] **WorldSpace Canvas は Canvas Scaler 不要** - Transform で位置制御

```csharp
// [OK] WorldSpace Canvas は Transform で制御
Canvas waterTurretUI = GetComponent<Canvas>();
waterTurretUI.renderMode = RenderMode.WorldSpace;  // 3D UI モード

// UI 要素の位置・大きさは RectTransform で管理
RectTransform rect = waterTurretUI.GetComponent<RectTransform>();
rect.position = new Vector3(0, 2.0f, 0);  // 高さ 2.0 の位置に配置
rect.sizeDelta = new Vector2(100, 50);   // サイズ指定
```

---

## マルチ解像度対応

### サポート解像度

OnoCoro がテストしている解像度：

| 解像度 | アスペクト比 | デバイス例 |
|--------|----------|----------|
| **1920×1080** | 16:9 | PC (Full HD) |
| **1280×720** | 16:9 | 多くのモバイル |
| **2560×1440** | 16:9 | 高解像度 PC |
| **1024×768** | 4:3 | タブレット |

### スケーリング計算例

[OK] **Reference Resolution 1920×1080 での自動スケーリング**

```csharp
// 解像度: 1280×720 の場合
float scaleX = 1280f / 1920f = 0.667f  (約 67%)
float scaleY = 720f / 1080f = 0.667f   (約 67%)

// すべての UI 要素がスケールされる
// Button: 100px → 67px
// Text: 32sp → 21sp (約 67% に自動縮小)
```

### 画面方向の対応

[OK] **Expand モード使用時**

```csharp
// ポートレート (800×1280) の場合
//┌───────────────┐
//│ [UI]          │  ← UI は幅で制限
//│               │
//│               │  ← 上下に黒枠（余白）
//└───────────────┘

// ランドスケープ (1280×800) の場合
//┌──────────────────────┐
//│ [UI]                 │  ← UI は高さで制限
//└──────────────────────┘  ← 左右に黒枠（余白）
```

---

## UICanvasManager 実装詳細

### 初期化処理

```csharp
internal static class UICanvasManager
{
    private const float REFERENCE_RESOLUTION_WIDTH = 1920f;
    private const float REFERENCE_RESOLUTION_HEIGHT = 1080f;
    
    internal static void InitializeCanvasSettings()
    {
        // 全 Canvas を検出
        Canvas[] allCanvas = FindObjectsOfType<Canvas>();
        Debug.Log($"Found {allCanvas.Length} Canvas objects");
        
        foreach (Canvas canvas in allCanvas)
        {
            ApplyStandardScalerSettings(canvas);
        }
        
        Debug.Log("Canvas initialization complete");
    }
    
    private static void ApplyStandardScalerSettings(Canvas canvas)
    {
        // WorldSpace Canvas スキップ
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.Log($"Canvas '{canvas.gameObject.name}' uses WorldSpace - Skipped");
            return;
        }
        
        // ScreenSpace Canvas を設定
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            Debug.LogWarning($"Canvas '{canvas.gameObject.name}' missing CanvasScaler");
            return;
        }
        
        // Scaler 設定
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(
            REFERENCE_RESOLUTION_WIDTH,
            REFERENCE_RESOLUTION_HEIGHT
        );
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        
        Debug.Log($"Canvas '{canvas.gameObject.name}' configured with {scaler.referenceResolution}");
    }
}
```

### InitializationManager からの呼び出し

```csharp
private IEnumerator InitializeUIComponents()
{
    // Phase 3: UI コンポーネント初期化
    
    // Canvas Scaler 統一設定
    InitializeCanvasSettings();
    yield return new WaitForEndOfFrame();
    
    Debug.Log("UI initialization complete");
}

private void InitializeCanvasSettings()
{
    try
    {
        UICanvasManager.InitializeCanvasSettings();
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Failed to initialize canvas settings: {ex.Message}");
    }
}
```

---

## トラブルシューティング

### "UI がぼやけている"

[STEP] **Canvas Scaler の設定確認**

```csharp
Canvas canvas = GetComponent<Canvas>();
CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();

Debug.Log($"Scale Mode: {scaler.uiScaleMode}");
Debug.Log($"Reference Resolution: {scaler.referenceResolution}");
Debug.Log($"Current Scale: {canvas.scaleFactor}");
```

### "WorldSpace Canvas が ScreenSpace に変更される"

[STEP] **UICanvasManager の WorldSpace チェック確認**

```csharp
// [OK] WorldSpace の場合、以下のログが出力される
Debug.Log("Canvas 'SignPowerOutage' uses WorldSpace - Skipped");

// ログが出力されない場合、renderMode 確認
Canvas canvas = GetComponent<Canvas>();
Debug.Log($"RenderMode: {canvas.renderMode}");  // WorldSpace か確認
```

### "異なる解像度で UI レイアウトが崩れる"

[STEP] **Layout Group の設定確認**

```csharp
// Canvas スケーラーの設定は正常でも
// Layout Group 設定が競合する可能性

LayoutGroup layout = GetComponent<LayoutGroup>();
if (layout != null)
{
    layout.childForceExpandWidth = true;   // 幅を親に合わせる
    layout.childForceExpandHeight = true;  // 高さを親に合わせる
}
```

---

## チェックリスト

UI System 実装時：

- [ ] **Reference Resolution**: 1920×1080 で設定
- [ ] **Scale Mode**: ScaleWithScreenSize で設定
- [ ] **WorldSpace Canvas**: 自動スキップ確認
- [ ] **各解像度でテスト**: 1280×720, 1920×1080, 2560×1440 で確認
- [ ] **ログ出力**: Canvas 初期化ログ確認
- [ ] **Layout Group**: 画面サイズ変更時に再計算確認

---

**関連資料**:
- [initialization-flow.md](initialization-flow.md) - Phase 3 初期化フロー
- [asset-management.md](asset-management.md) - UI Prefab 管理
- [project-rules/unity-design-patterns.md](../project-rules/unity-design-patterns.md) - Canvas パターン
