# UI 改善ドキュメント - Phase 1.4

Canvas Scaler 整頓、フォントサイズ規定化、UI レイアウト標準化の詳細実装計画

**Phase**: 1.4  
**目標リリース**: 2026-02-末  
**開始日**: 2026-02-初旬  
**最終更新**: 2026-01-30

---

## 📋 目次

1. [現状分析](#現状分析)
2. [実装計画](#実装計画)
3. [Phase 1.4.1: Canvas Scaler 整頓](#phase-141-canvas-scaler-整頓)
4. [Phase 1.4.2: フォントサイズ規定化](#phase-142-フォントサイズ規定化)
5. [Phase 1.4.3: UI コンポーネント配置標準化](#phase-143-ui-コンポーネント配置標準化)
6. [Phase 1.4.4: UIUtility カプセル化](#phase-144-uiutility-カプセル化)
7. [チェックリスト](#チェックリスト)

---

## 現状分析

### Canvas Scaler 現状

**現状問題**:
- Canvas ごとに Scaler 設定がバラバラ
- Reference Resolution がゲーム解像度に統一されていない
- UI Scale Mode が不統一（Scale / Constant Pixel / Constant Physical）
- スクリーンサイズ変更時の動作が予測困難

**確認項目**:
```
Canvas コンポーネント
├─ Render Mode（Screen Space / World Space 混在）
├─ Canvas Scaler
│  ├─ UI Scale Mode（統一？）
│  └─ Reference Resolution（1920×1080 で統一？）
└─ GraphicRaycaster
```

**スクリーンサイズ別対応**:
| 解像度 | 縦横比 | 対応方法 |
|--------|--------|---------|
| 1920×1080 | 16:9 | 基準（Reference） |
| 1280×720 | 16:9 | Scale Down |
| 2560×1440 | 16:9 | Scale Up |
| iPad 4:3 | 4:3 | 要検証 |

### フォントサイズ現状

**問題点**:
- TextMeshPro フォントサイズが各コンポーネントでハードコーディング
- 画面解像度変更時にフォント比率が崩れる
- デザイン変更時に全コンポーネントを手動修正が必要
- UI Scale に伴うフォントサイズ自動調整がない

**確認項目**:
```
TextMeshPro コンポーネント
├─ Font Size（固定値: 36, 48, 60 など）
├─ Text Style（Bold / Italic / Underline 混在）
└─ Alignment（不統一）
```

### UI コンポーネント配置

**問題点**:
- Rect Transform の Anchor / Pivot が各コンポーネント異なる
- 親キャンバスのパディング設定がない
- レスポンシブ対応がない

---

## 実装計画

### 4ステップ構成

```
Phase 1.4.1: Canvas Scaler 整頓
  └─ Reference Resolution 統一（1920×1080）
  └─ UI Scale Mode 統一（Scale With Screen Size）
  └─ Match（幅・高さ）の基準設定
  └─ 全 Canvas の設定反映（4-5個）
  ┗─ 期間: 2-3日

Phase 1.4.2: フォントサイズ規定化
  └─ フォントサイズ規定表作成（6レベル）
  └─ UIFont enum 実装
  └─ TextMeshPro 拡張メソッド実装
  └─ 既存コンポーネントへの適用（20-30個）
  ┗─ 期間: 3-4日

Phase 1.4.3: UI コンポーネント配置標準化
  └─ Anchor / Pivot テンプレート定義
  └─ レイアウトガイドライン作成
  └─ Rect Transform 自動設定 Editor ツール
  └─ 既存コンポーネント配置最適化（全パネル）
  ┗─ 期間: 3-4日

Phase 1.4.4: UIUtility カプセル化
  └─ UIUtility クラス設計（静的ヘルパー）
  └─ RectTransform 拡張メソッド実装
  └─ TextMeshPro 拡張メソッド実装
  └─ フォント設定カプセル化
  └─ ドキュメント作成
  ┗─ 期間: 2-3日

【総合期間】: 10-14日（1人チームで 2週間）
```

---

## Phase 1.4.1: Canvas Scaler 整頓

### 目標

全 Canvas の Scaler 設定を統一し、**1920×1080 基準でスケーリング一元管理**

### 実装内容

#### 1.1 Canvas Scaler 統一設定

**全 Canvas に適用する標準設定**:

```csharp
Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
canvasScaler.referenceResolution = new Vector2(1920, 1080);
canvasScaler.matchWidthOrHeight = 0.5f;  // 幅・高さ等比スケーリング
canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
```

**設定根拠**:
- Reference Resolution: 1920×1080（スタンダード解像度）
- UI Scale Mode: ScaleWithScreenSize（レスポンシブ対応）
- Match: 0.5（幅・高さを等しく重視）

#### 1.2 影響を受ける Canvas リスト

| Canvas | 現在の設定 | 統一後の設定 | 優先度 |
|--------|----------|----------|--------|
| MainCanvas | ScreenSpace, 1280×720 | 1920×1080 | [OK] |
| PauseMenuCanvas | WorldSpace | ScreenSpace | [NOTE] 検討中 |
| DebugCanvas | ScreenSpace, Scale | ScaleWithScreenSize | [OK] |
| UITabMenuCanvas | ScreenSpace, Fixed | ScaleWithScreenSize | [OK] |
| PopupCanvas | WorldSpace | ScreenSpace | [NOTE] 検討中 |

**実装手順**:
1. 各 Canvas の現在設定をドキュメント化
2. Editor スクリプト作成（一括設定）
3. 各 Canvas に適用
4. 動作検証（複数解像度）

#### 1.3 テスト解像度一覧

| 解像度 | 縦横比 | Scale 計算 | 検証項目 |
|--------|--------|----------|---------|
| 1920×1080 | 16:9 | 1.0× | 基準 |
| 1280×720 | 16:9 | 0.67× | UI 縮小検証 |
| 2560×1440 | 16:9 | 1.33× | UI 拡大検証 |
| 1024×768 | 4:3 | 0.53× | タブレット検証 |

### チェックリスト

- [ ] Canvas 現況調査完了
- [ ] Editor スクリプト実装完了
- [ ] 全 Canvas 設定適用完了
- [ ] 複数解像度でテスト完了

---

## Phase 1.4.2: フォントサイズ規定化

### 目標

**フォントサイズを 6 レベルで規定化し、解像度変化に自動対応**

### 実装内容

#### 2.1 フォントサイズレベル定義

```csharp
// UIFont.cs
public enum UIFont
{
    // XSmall: 24pt（補足・ツールチップ用）
    XSmall = 24,
    
    // Small: 32pt（副見出し・小型 UI）
    Small = 32,
    
    // Normal: 40pt（通常テキスト）
    Normal = 40,
    
    // Large: 48pt（大見出し・重要情報）
    Large = 48,
    
    // XLarge: 56pt（ステージ選択・ボタン）
    XLarge = 56,
    
    // XXLarge: 72pt（タイトル・メインメニュー）
    XXLarge = 72,
}
```

#### 2.2 フォントサイズ適用テーブル

| UI 要素 | 用途 | フォントサイズ | 例 |
|---------|------|---------------|-----|
| **タイトル** | メインメニュー | XXLarge (72) | "OnoCoro" |
| **大見出し** | ステージ選択 | XLarge (56) | "Stage 1" |
| **ボタンテキスト** | UI ボタン | Large (48) | "Start", "Options" |
| **通常テキスト** | 説明文 | Normal (40) | 説明文本体 |
| **ラベル** | UI ラベル | Small (32) | "Health:", "Gold:" |
| **補足情報** | ツールチップ | XSmall (24) | 詳細説明 |

#### 2.3 TextMeshPro 拡張メソッド

```csharp
// TextMeshProUtility.cs
public static class TextMeshProUtility
{
    /// <summary>
    /// TextMeshPro のフォントサイズを enum で設定
    /// </summary>
    public static void SetFontSize(this TextMeshProUGUI textMeshPro, UIFont fontLevel)
    {
        textMeshPro.fontSize = (int)fontLevel;
    }

    /// <summary>
    /// フォント設定を一括適用（フォントサイズ + Alignment）
    /// </summary>
    public static void ApplyUIStyle(
        this TextMeshProUGUI textMeshPro,
        UIFont fontLevel,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        textMeshPro.fontSize = (int)fontLevel;
        textMeshPro.alignment = alignment;
    }

    /// <summary>
    /// 解像度に応じたスケーリング計算（オプション）
    /// </summary>
    public static float GetScaledFontSize(this TextMeshProUGUI textMeshPro, UIFont baseSize)
    {
        float screenScale = Screen.height / 1080f;  // 1080p 基準
        return (int)baseSize * screenScale;
    }
}
```

#### 2.4 使用例

```csharp
// Old: ハードコーディング
textMeshPro.fontSize = 48;

// New: enum 利用（推奨）
textMeshPro.SetFontSize(UIFont.Large);

// New: スタイル一括設定
textMeshPro.ApplyUIStyle(UIFont.XLarge, TextAlignmentOptions.TopLeft);
```

### チェックリスト

- [ ] UIFont enum 実装完了
- [ ] TextMeshProUtility 実装完了
- [ ] 既存コンポーネント（20-30個）へ適用完了
- [ ] スクリーンサイズ変更テスト完了

---

## Phase 1.4.3: UI コンポーネント配置標準化

### 目標

**Anchor / Pivot / Offset を規定化し、レスポンシブレイアウト実装**

### 実装内容

#### 3.1 Anchor / Pivot テンプレート定義

| 配置方式 | 親位置 | Anchor | Pivot | 用途 |
|---------|--------|--------|-------|------|
| **Full Screen** | Canvas | (0,0)-(1,1) | (0.5, 0.5) | 全画面背景 |
| **Top Left** | 左上 | (0,1) | (0, 1) | メニューパネル |
| **Top Center** | 上中央 | (0.5, 1) | (0.5, 1) | ヘッダー |
| **Top Right** | 右上 | (1, 1) | (1, 1) | リソース表示 |
| **Center** | 中央 | (0.5, 0.5) | (0.5, 0.5) | ダイアログ |
| **Bottom Center** | 下中央 | (0.5, 0) | (0.5, 0) | フッター |
| **Stretch H** | 水平全幅 | (0, y)-(1, y) | (0.5, 0.5) | 横幅最大 |
| **Stretch V** | 垂直全幅 | (x, 0)-(x, 1) | (0.5, 0.5) | 縦幅最大 |

#### 3.2 RectTransform 拡張メソッド

```csharp
// RectTransformUtility.cs
public static class RectTransformUtility
{
    /// <summary>
    /// Anchor を Top Left（左上）に設定
    /// </summary>
    public static void SetAnchorTopLeft(this RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
    }

    /// <summary>
    /// Anchor を Center（中央）に設定
    /// </summary>
    public static void SetAnchorCenter(this RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    /// <summary>
    /// Anchor を Stretch（全幅）に設定
    /// </summary>
    public static void SetAnchorStretchHorizontal(this RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(1, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
}
```

#### 3.3 レイアウトガイドライン

**パネル配置規則**:
```
┌─────────────────────────────┐
│ Top Panel (Padding: 20px)   │  ← Top Center Anchor
├─────────────────────────────┤
│                             │
│  Main Content Area          │  ← Center Anchor or Stretch
│  (Padding: 30px)            │
│                             │
├─────────────────────────────┤
│ Bottom Panel (Padding: 20px)│  ← Bottom Center Anchor
└─────────────────────────────┘
```

**パディング規定**:
- Canvas 外周: 20px
- パネル間隔: 15px
- テキスト周辺: 10px

### チェックリスト

- [ ] RectTransform 拡張メソッド実装完了
- [ ] レイアウトガイドライン文書化完了
- [ ] 既存パネル配置最適化完了（5個）
- [ ] レスポンシブテスト完了

---

## Phase 1.4.4: UIManager 実装 と UIUtility 分離

### 目標

**Canvas 操作を Manager パターンで一元管理**し、表示と機能を疎結合化

### 実装内容

#### 4.1 責務分離設計

**従来の UIUtility（汎用ヘルパー）**:
```
❌ 問題: Canvas 設定 + RectTransform 操作 + Font 設定 が混在
   → 表示と機能が密接に結合
   → Reference Resolution が分散
```

**改善案（責務を分離）**:
```
✅ UICanvasManager（Manager パターン）
   └─ Canvas Scaler 設定の一元管理
   └─ Reference Resolution の統一（1920×1080）
   └─ UI Scale Mode の統一管理
   └─ 複数解像度対応の中枢

✅ UIUtility（汎用ヘルパー）
   └─ RectTransform の Anchor 設定
   └─ レイアウトプリセット
   └─ パディング設定

✅ TextMeshProUtility（汎用ヘルパー）
   └─ フォントサイズ設定
   └─ テキストスタイル適用
```

#### 4.2 UICanvasManager 設計

```csharp
// Core/Managers/UICanvasManager.cs
/// <summary>
/// UI Canvas 一元管理（Singleton）
/// 全 Canvas の Scaler 設定、解像度対応を統一
/// </summary>
internal class UICanvasManager : MonoBehaviour
{
    private static UICanvasManager _instance;
    
    // ═════════════════════════════════════════
    // 設定定数（変更時は全 Canvas に反映）
    // ═════════════════════════════════════════
    
    /// <summary>
    /// 基準解像度（全 Canvas の Reference Resolution）
    /// 複数解像度対応時はここだけ変更すれば全 Canvas に反映
    /// </summary>
    internal static readonly Vector2 REFERENCE_RESOLUTION = new Vector2(1920, 1080);
    
    /// <summary>
    /// Canvas Scaler の幅・高さマッチ設定
    /// 0.0 = 幅優先, 0.5 = 等比, 1.0 = 高さ優先
    /// </summary>
    internal static readonly float MATCH_WIDTH_OR_HEIGHT = 0.5f;
    
    /// <summary>
    /// デフォルト Render Mode
    /// </summary>
    internal static readonly RenderMode DEFAULT_RENDER_MODE = RenderMode.ScreenSpaceOverlay;
    
    internal static UICanvasManager Instance => _instance;
    
    // ═════════════════════════════════════════
    // Canvas Scaler 管理
    // ═════════════════════════════════════════
    
    /// <summary>
    /// Canvas に標準 Scaler 設定を自動適用
    /// 全 Canvas でこのメソッドを呼び出すことで統一
    /// </summary>
    internal static void ApplyStandardScalerSettings(Canvas canvas)
    {
        if (canvas == null) return;
        
        canvas.renderMode = DEFAULT_RENDER_MODE;
        
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }
        
        // [重要] これらの値は REFERENCE_RESOLUTION を変更するだけで全 Canvas に反映
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = MATCH_WIDTH_OR_HEIGHT;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    }
    
    /// <summary>
    /// 現在の Canvas スケール係数を取得（複数解像度対応）
    /// </summary>
    internal static float GetCurrentCanvasScale()
    {
        return Screen.height / REFERENCE_RESOLUTION.y;
    }
    
    /// <summary>
    /// 複数解像度対応時に全 Canvas を更新
    /// 例: 1920×1080 → 2560×1440 に変更したい場合
    /// </summary>
    internal static void UpdateAllCanvasesForResolution(Vector2 newResolution)
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.referenceResolution = newResolution;
            }
        }
    }
    
    private void Awake()
    {
        if (_instance != null) return;
        _instance = this;
    }
}
```

#### 4.3 UIUtility（汎用ヘルパー）

```csharp
// Core/Utilities/UIUtility.cs
/// <summary>
/// UI 汎用ヘルパー（Canvas 設定は含まない）
/// RectTransform 操作、レイアウト設定のみ
/// </summary>
public static class UIUtility
{
    // ═════════════════════════════════════════
    // RectTransform レイアウト
    // ═════════════════════════════════════════
    
    /// <summary>
    /// RectTransform に Anchor 設定を適用
    /// </summary>
    public static void SetLayoutAnchor(
        RectTransform rectTransform,
        LayoutPreset preset)
    {
        switch (preset)
        {
            case LayoutPreset.TopLeft:
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                break;
            case LayoutPreset.Center:
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                break;
            // ... 他のプリセット
        }
    }
    
    /// <summary>
    /// パネルに統一パディングを適用
    /// </summary>
    public static void ApplyPanelPadding(
        RectTransform panelRect,
        int paddingPixels = 20)
    {
        panelRect.offsetMin = new Vector2(paddingPixels, paddingPixels);
        panelRect.offsetMax = new Vector2(-paddingPixels, -paddingPixels);
    }
}
```

#### 4.4 使用例（EscMenuCtrl）

```csharp
// EscMenuCtrl.cs - Initialize() で Canvas 設定を実施
protected override IEnumerator Initialize()
{
    // [重要] Canvas Scaler は Manager 経由で統一設定
    Canvas canvas = GetComponent<Canvas>();
    UICanvasManager.ApplyStandardScalerSettings(canvas);
    
    // RectTransform はユーティリティで設定
    RectTransform canvasRect = GetComponent<RectTransform>();
    UIUtility.SetLayoutAnchor(canvasRect, LayoutPreset.FullScreen);
    
    // UI レイアウト構築
    SetupMenuLayout();
    
    // ボタンリスナー設定
    SetupButtonListeners();
    
    yield return null;
}

private void SetupMenuLayout()
{
    GameObject menuWindow = transform.Find("menuWindow").gameObject;
    RectTransform menuRect = menuWindow.GetComponent<RectTransform>();
    
    // ユーティリティで配置設定
    UIUtility.SetLayoutAnchor(menuRect, LayoutPreset.Center);
    UIUtility.ApplyPanelPadding(menuRect, paddingPixels: 30);
    
    // ボタングループ
    foreach (Button btn in menuWindow.GetComponentsInChildren<Button>())
    {
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        UIUtility.SetLayoutAnchor(btnRect, LayoutPreset.StretchHorizontal);
        
        // フォント設定は TextMeshProUtility で（表示レイヤー）
        TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.SetFontSize(UIFont.Large);
        }
    }
}
```

#### 4.5 複数解像度対応時の利点

```csharp
// 将来: 2560×1440 対応に変更したい場合

// ❌ 従来の UIUtility 方式
// → 全 Canvas を手動で修正する必要がある

// ✅ UICanvasManager 方式
public static readonly Vector2 REFERENCE_RESOLUTION = new Vector2(2560, 1440);
// → この 1 行の変更で全 Canvas に自動反映
```

---

## 責務分離テーブル

| クラス | 責務 | 管理対象 |
|--------|------|---------|
| **UICanvasManager** | Canvas Scaler 一元管理 | Reference Resolution, UI Scale Mode |
| **UIUtility** | RectTransform ヘルパー | Anchor, Pivot, Padding 設定 |
| **TextMeshProUtility** | TextMeshPro ヘルパー | フォントサイズ, スタイル |

---

## 実装チェックリスト

### Phase 1.4.4 更新版

- [ ] UICanvasManager クラス実装完了
- [ ] REFERENCE_RESOLUTION, MATCH_WIDTH_OR_HEIGHT を定数化完了
- [ ] ApplyStandardScalerSettings() メソッド実装完了
- [ ] GetCurrentCanvasScale() メソッド実装完了
- [ ] UpdateAllCanvasesForResolution() メソッド実装完了
- [ ] UIUtility を汎用ヘルパーのみに修正完了
- [ ] EscMenuCtrl へ UICanvasManager を適用完了
- [ ] 複数解像度テスト完了（1920×1080, 1280×720, 2560×1440）
- [ ] ドキュメント更新完了

---

## チェックリスト

### Phase 1.4.1 (Canvas Scaler 整頓)

- [ ] Canvas 現況調査完了
- [ ] Editor スクリプト実装完了
- [ ] 全 Canvas 設定適用完了
- [ ] 複数解像度でテスト完了（3解像度以上）
- [ ] テスト結果ドキュメント化

### Phase 1.4.2 (フォントサイズ規定化)

- [ ] UIFont enum 実装完了
- [ ] TextMeshProUtility 実装完了
- [ ] フォントサイズ適用テーブル作成完了
- [ ] 既存コンポーネント（20-30個）へ適用完了
- [ ] スクリーンサイズ変更テスト完了

### Phase 1.4.3 (UI コンポーネント配置標準化)

- [ ] RectTransformUtility 拡張メソッド実装完了
- [ ] レイアウトガイドライン文書化完了
- [ ] 既存パネル配置最適化完了（5個）
- [ ] レスポンシブテスト完了（複数解像度）
- [ ] ビジュアルテスト完了（各パネルのスクリーンショット）

### Phase 1.4.4 (UIUtility カプセル化)

- [ ] UIUtility クラス実装完了
- [ ] UIConfig ScriptableObject 作成完了
- [ ] ドキュメント（UIUtility 使用例）作成完了
- [ ] 既存コンポーネント修正完了
- [ ] 実装例テスト完了

### 全体検証

- [ ] 1920×1080 レファレンス解像度での完全動作確認
- [ ] 1280×720、2560×1440 でのスケーリング確認
- [ ] UI Scale 変更時の挙動確認
- [ ] パフォーマンス測定（メモリ使用量、FPS）
- [ ] ドキュメント最終確認

---

## 関連ドキュメント

| ドキュメント | 参照箇所 |
|------------|---------|
| AGENTS.md | クラス命名規則、アクセス修飾子ポリシー |
| coding-standards.md | コーディング規約 |
| ui-initialization-reference.md | UI 初期化システム |
| prototype-phase-roadmap.md | 全体スケジュール |

---

## 工数見積もり

| フェーズ | 内容 | 日数 | 進捗 |
|---------|------|------|------|
| 1.4.1 | Canvas Scaler 整頓 | 2-3日 | 0% |
| 1.4.2 | フォントサイズ規定化 | 3-4日 | 0% |
| 1.4.3 | UI 配置標準化 | 3-4日 | 0% |
| 1.4.4 | UIUtility カプセル化 | 2-3日 | 0% |
| **合計** | **UI 改善** | **10-14日** | **0%** |

**リソース**: 1 人チーム  
**期間**: 2026-02-初旬 ～ 2026-02-末（2週間）

---

**ドキュメント作成日**: 2026-01-30  
**最終更新**: 2026-01-30
