# Phase 1.4: UI 改善 - 詳細実装計画

**対象**: Canvas Scaler 整頓、フォントサイズ規定化、UI レイアウト標準化  
**期間**: 2026-02-初旬～03 末  
**工数**: 10-14人日  
**進捗**: 100%（全項目完了 2026-02-03）

---

## 1.4.1: Canvas Scaler 整頓

**目標**: 全 Canvas に 1920×1080 基準を適用

### 実装内容

- [ ] Canvas 現況調査（設定がバラバラの Canvas を特定）
- [ ] Editor スクリプト作成（一括設定ツール）
- [ ] 全 Canvas に設定適用
- [ ] 複数解像度（1920×1080, 1280×720, 2560×1440, iPad）でテスト

### 影響を受ける Canvas

| Canvas | 現在設定 | 目標設定 | 優先度 |
|--------|---------|---------|--------|
| MainCanvas | 1280×720 | 1920×1080 | [OK] |
| PauseMenuCanvas | WorldSpace | ScreenSpace | [NOTE] 検討中 |
| DebugCanvas | Scale モード混在 | ScaleWithScreenSize | [OK] |
| UITabMenuCanvas | Fixed | ScaleWithScreenSize | [OK] |
| PopupCanvas | WorldSpace | ScreenSpace | [NOTE] 検討中 |

### テスト解像度一覧

| 解像度 | 縦横比 | Scale 計算 | 検証項目 |
|--------|--------|----------|---------|
| 1920×1080 | 16:9 | 1.0× | 基準 |
| 1280×720 | 16:9 | 0.67× | UI 縮小検証 |
| 2560×1440 | 16:9 | 1.33× | UI 拡大検証 |
| 1024×768 | 4:3 | 0.53× | タブレット検証 |

### チェックリスト

- [x] Canvas 現況ドキュメント化
- [x] Editor スクリプト実装
- [x] 全 Canvas 設定反映
- [x] 複数解像度テスト合格
- [x] ドキュメント完了

**期間**: 2-3日 [OK] 完了

---

## 1.4.2: フォントサイズ規定化

**目標**: フォントサイズを6レベルで統一、解像度変化に自動対応

### フォントサイズレベル定義

```csharp
// UIFont.cs
public enum UIFont
{
    XSmall = 24,    // 補足・ツールチップ用
    Small = 32,     // 副見出し・小型 UI
    Normal = 40,    // 通常テキスト
    Large = 48,     // 大見出し・重要情報
    XLarge = 56,    // ステージ選択・ボタン
    XXLarge = 72,   // タイトル・メインメニュー
}
```

### フォントサイズ適用テーブル

| UI 要素 | 用途 | フォントサイズ | 例 |
|---------|------|---------------|-----|
| **タイトル** | メインメニュー | XXLarge (72) | "OnoCoro" |
| **大見出し** | ステージ選択 | XLarge (56) | "Stage 1" |
| **ボタンテキスト** | UI ボタン | Large (48) | "Start", "Options" |
| **通常テキスト** | 説明文 | Normal (40) | 説明文本体 |
| **ラベル** | UI ラベル | Small (32) | "Health:", "Gold:" |
| **補足情報** | ツールチップ | XSmall (24) | 詳細説明 |

### TextMeshPro 拡張メソッド

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
}
```

### 使用例

```csharp
// Old: ハードコーディング
textMeshPro.fontSize = 48;

// New: enum 利用（推奨）
textMeshPro.SetFontSize(UIFont.Large);

// New: スタイル一括設定
textMeshPro.ApplyUIStyle(UIFont.XLarge, TextAlignmentOptions.TopLeft);
```

### チェックリスト

- [x] UIFont enum 実装
- [x] TextMeshProUtility 実装
- [x] 既存コンポーネント（20-30個）へ適用
- [x] スクリーンサイズ変更テスト合格

**期間**: 3-4日 [OK] 完了

---

## 1.4.3: UI コンポーネント配置標準化

**目標**: Anchor/Pivot テンプレート定義、レスポンシブレイアウト実装

### Anchor / Pivot テンプレート定義

| 配置方式 | 親位置 | Anchor | Pivot | 用途 |
|---------|--------|--------|-------|------|
| **FullScreen** | Canvas | (0,0)-(1,1) | (0.5, 0.5) | 全画面背景 |
| **TopLeft** | 左上 | (0,1) | (0, 1) | メニューパネル |
| **TopCenter** | 上中央 | (0.5, 1) | (0.5, 1) | ヘッダー |
| **TopRight** | 右上 | (1, 1) | (1, 1) | リソース表示 |
| **Center** | 中央 | (0.5, 0.5) | (0.5, 0.5) | ダイアログ |
| **BottomCenter** | 下中央 | (0.5, 0) | (0.5, 0) | フッター |
| **StretchH** | 水平全幅 | (0, y)-(1, y) | (0.5, 0.5) | 横幅最大 |
| **StretchV** | 垂直全幅 | (x, 0)-(x, 1) | (0.5, 0.5) | 縦幅最大 |

### RectTransform 拡張メソッド

```csharp
// RectTransformUtility.cs
public static class RectTransformUtility
{
    /// <summary>
    /// Anchor を TopLeft（左上）に設定
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
    /// Anchor を StretchHorizontal（水平全幅）に設定
    /// </summary>
    public static void SetAnchorStretchHorizontal(this RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(1, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
}
```

### レイアウトガイドライン

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

- [x] RectTransform 拡張メソッド実装
- [x] レイアウトガイドライン完成
- [x] Editor ツール実装
- [x] 既存パネル配置最適化（5個）
- [x] レスポンシブテスト合格

**期間**: 3-4日 [OK] 完了

---

## 1.4.4: UICanvasManager 実装と UIUtility 分離

**目標**: Canvas 操作を Manager パターンで一元管理、表示と機能を疎結合化  
**状態**: [OK] 完了（2026-01-30）

### 責務分離設計

```
[OK] UICanvasManager（Manager パターン）
   └─ Canvas Scaler 設定の一元管理
   └─ Reference Resolution の統一（1920×1080）
   └─ UI Scale Mode の統一管理
   └─ 複数解像度対応の中枢

[OK] UIUtility（汎用ヘルパー）
   └─ RectTransform の Anchor 設定
   └─ レイアウトプリセット
   └─ パディング設定

[OK] TextMeshProUtility（汎用ヘルパー）
   └─ フォントサイズ設定
   └─ テキストスタイル適用
```

### UICanvasManager 設計

```csharp
// Core/Managers/UICanvasManager.cs
internal class UICanvasManager : MonoBehaviour
{
    // 基準解像度（変更時は全 Canvas に反映）
    internal static readonly Vector2 REFERENCE_RESOLUTION = new Vector2(1920, 1080);
    
    // 幅・高さマッチ設定（0.5 = 等比スケーリング）
    internal static readonly float MATCH_WIDTH_OR_HEIGHT = 0.5f;
    
    /// <summary>
    /// Canvas に標準 Scaler 設定を自動適用
    /// </summary>
    internal static void ApplyStandardScalerSettings(Canvas canvas)
    {
        if (canvas == null) return;
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }
        
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = MATCH_WIDTH_OR_HEIGHT;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    }
    
    /// <summary>
    /// 現在の Canvas スケール係数を取得
    /// </summary>
    internal static float GetCurrentCanvasScale()
    {
        return Screen.height / REFERENCE_RESOLUTION.y;
    }
}
```

### UIUtility 設計

```csharp
// Core/Utilities/UIUtility.cs
public static class UIUtility
{
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

### 複数解像度対応時の利点

```csharp
// 将来: 2560×1440 対応に変更したい場合

// [NG] 従来の方式
// → 全 Canvas を手動で修正する必要がある

// [OK] UICanvasManager 方式
internal static readonly Vector2 REFERENCE_RESOLUTION = new Vector2(2560, 1440);
// → この 1 行の変更で全 Canvas に自動反映
```

### 実装完了項目（2026-02-03）

- [x] UICanvasManager クラス実装
- [x] ResolutionPreset enum（HD/FullHD/TwoK/iPad）
- [x] ApplyStandardScalerSettings() メソッド
- [x] GetCurrentCanvasScale() メソッド
- [x] UIUtility クラス実装
- [x] UICanvasManagerTest テストスクリプト実装

### チェックリスト

- [x] UICanvasManager 実装
- [x] UIUtility 実装
- [x] UICanvasManagerTest 実装
- [x] 複数解像度テスト（1280×720, 2560×1440）
- [x] ドキュメント確認

---

## 工数見積もり

| フェーズ | 内容 | 日数 | 進捗 | 状態 |
|--------|------|------|------|------|
| 1.4.1 | Canvas Scaler 整頓 | 2-3日 | 100% | [OK] 完了 |
| 1.4.2 | フォントサイズ規定化 | 3-4日 | 100% | [OK] 完了 |
| 1.4.3 | UI 配置標準化 | 3-4日 | 100% | [OK] 完了 |
| 1.4.4 | UICanvasManager + UIUtility | 2-3日 | 100% | [OK] 完了 |
| **合計** | **UI 改善** | **10-14日** | **100%** | **[OK] 完了** |

**リソース**: 1人チーム  
**期間**: 2026-02-初旬～末（2週間）

---

## 関連ドキュメント

- [AGENTS.md](../../AGENTS.md) - クラス命名規則、アクセス修飾子ポリシー
- [docs/coding-standards.md](../../docs/coding-standards.md) - コーディング規約
- [docs/architecture/ui-system.md](../../docs/architecture/ui-system.md) - UI システム設計
- [../roadmap-phase-1-4-2-3.md](../roadmap-phase-1-4-2-3.md) - 全体ロードマップ

---

**最終更新**: 2026-02-03
**ステータス**: [OK] 完了（全項目実装済み）
