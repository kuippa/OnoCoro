# UI 初期化順序リファレンス

**最終更新**: 2026-01-29  
**対象**: OnoCoro Phase 1.5 実装版

---

## 実装済みコンポーネント（9/22）

### Panels（5個） ✅

| クラス名 | ファイルパス | 状態 |
|---------|-------------|------|
| `EscMenuCtrl` | Presentation/UI/Panels/ | ✅ UIControllerBase 継承 |
| `TabMenuCtrl` | Presentation/UI/Panels/ | ✅ UIControllerBase 継承 |
| `NoticeCtrl` | Presentation/UI/Panels/ | ✅ UIControllerBase 継承 |
| `DebugInfoCtrl` | Presentation/UI/Panels/ | ✅ UIControllerBase 継承 |
| `SpawnMarkerPointerCtrl` | Presentation/UI/Panels/ | ✅ UIControllerBase 継承 |

### Dialogs（4個） ✅

| クラス名 | ファイルパス | 状態 |
|---------|-------------|------|
| `EventLogCtrl` | Presentation/UI/Dialogs/ | ✅ UIControllerBase 継承 |
| `GameTimerCtrl` | Presentation/UI/Dialogs/ | ✅ UIControllerBase 継承 |
| `InfoWindowCtrl` | Presentation/UI/Dialogs/ | ✅ UIControllerBase 継承 |
| `MessageBoxCtrl` | Presentation/UI/Dialogs/ | ✅ UIControllerBase 継承 |

### 非実装（13個 - プロトタイプ対象外）

- **HUD**（8個）: MarkerIndicatorCtrl, MarkerPointerCtrl, PathMakerCtrl, ScoreCtrl, TelopCtrl, TooltipInfoCtrl, MouseOverTipsCtrl, CircularIndicator
- **Controls**（5個）: ClickCtrl, ClosebtnCtrl, OkbtnCtrl, WindowCloseCtrl, WindowDragCtrl

---

## 初期化フロー（簡潔版）

```
【Awake Phase - 時間: 0ms】
  各コンポーネント.Awake()
  └─ base.Awake()
  └─ 参照取得のみ（実質的な初期化は行わない）
  └─ IsInitialized = false のまま

【Start Phase - 時間: 50-100ms】
  各コンポーネント.Start()（UIControllerBase が自動制御）
  ├─ StartCoroutine(InitializeAsync())
  │  ├─ yield return Initialize()
  │  ├─ IsInitialized = true に自動設定
  │  └─ ログ出力: "[UIControllerBase] {ClassName} 初期化完了"
  └─ 全 9個のコンポーネントが初期化完了

【初期化完了 - 時間: 100-200ms】
  InitializationManager が IInitializable × 9個を動的検出
  └─ GetComponentsInChildren<IInitializable>()
```

---

## UIControllerBase 実装（必須パターン）

### 基本テンプレート

```csharp
public class MyPanelCtrl : UIControllerBase
{
    private Button _button;
    
    // [1] Awake - 参照取得のみ
    protected override void Awake()
    {
        base.Awake();  // 必須
        _button = GetComponentInChildren<Button>();
    }
    
    // [2] Initialize - 実際の初期化処理
    protected override IEnumerator Initialize()
    {
        _button.onClick.AddListener(OnButtonClick);
        yield return null;  // 必須
    }
    
    // [3] IsInitialized は自動設定（手動で設定しない）
}
```

### チェックリスト

- [ ] `base.Awake()` を Awake の最初に呼び出している
- [ ] Awake では参照取得のみ（初期化処理はしていない）
- [ ] Initialize() で yield している（空でも `yield return null;`）
- [ ] IsInitialized を手動で設定していない
- [ ] System.Collections の using がある

---

## Script Execution Order（必須設定）

Unity Editor で以下の順序を設定:

```
Edit → Project Settings → Script Execution Order
```

| 優先度 | クラス | 設定状況 |
|--------|--------|---------|
| -100 | `InitializationManager` | 📌 必須 |
| -50 | `GamePrefabs` | 📌 必須 |
| 0 | Panels, Dialogs（デフォルト） | ✅ 不要 |

**重要**: Unity 再起動後、設定が保存されたか確認

---

## よくある問題と対策

### 問題 1: IsInitialized が true にならない

**原因**: Initialize() で `yield` していない

```csharp
// ❌ NG
protected override IEnumerator Initialize()
{
    _button.onClick.AddListener(OnClick);
    // yield なし
}

// ✅ OK
protected override IEnumerator Initialize()
{
    _button.onClick.AddListener(OnClick);
    yield return null;  // 必ず yield する
}
```

### 問題 2: 参照が null になっている

**原因**: Initialize() で参照を取得しようとしている

```csharp
// ❌ NG
protected override IEnumerator Initialize()
{
    _button = GetComponentInChildren<Button>();  // 遅い
    yield return null;
}

// ✅ OK
protected override void Awake()
{
    base.Awake();
    _button = GetComponentInChildren<Button>();  // Awake で取得
}
```

### 問題 3: Panels/Dialogs が初期化待ち対象から外れた

**原因**: UIControllerBase を継承していない

```csharp
// ❌ NG
public class MyCtrl : MonoBehaviour  // 継承なし

// ✅ OK
public class MyCtrl : UIControllerBase  // 必ず継承
```

---

## デバッグ方法

### コンソールで初期化状況を確認

```
[UIControllerBase] EscMenuCtrl 初期化完了
[UIControllerBase] TabMenuCtrl 初期化完了
[UIControllerBase] NoticeCtrl 初期化完了
[UIControllerBase] DebugInfoCtrl 初期化完了
[UIControllerBase] SpawnMarkerPointerCtrl 初期化完了
[UIControllerBase] EventLogCtrl 初期化完了
[UIControllerBase] GameTimerCtrl 初期化完了
[UIControllerBase] InfoWindowCtrl 初期化完了
[UIControllerBase] MessageBoxCtrl 初期化完了
```

各コンポーネントが `初期化完了` ログを出力していれば OK

### InitializationManager のログで確認

```
[InitializationManager] GamePrefabs のコントローラーを自動検出
[InitializationManager] 検出: 9個（IInitializable 実装）
```

---

## 関連ドキュメント

- [phase-1-5-initialization-order-design.md](phase-1-5-initialization-order-design.md) - 詳細設計書（作業手順）
- [docs/architecture.md](architecture.md) - システム全体の設計
- [AGENTS.md](../AGENTS.md) - プロジェクト全体のガイドライン
