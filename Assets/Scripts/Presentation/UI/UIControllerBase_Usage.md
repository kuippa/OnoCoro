# UIControllerBase - 使用ガイド

**バージョン**: 1.0  
**作成日**: 2026-01-29  
**対象**: すべての UI コントローラー（*Ctrl）クラス  

---

## 概要

`UIControllerBase` は、UI コントローラークラスの共通基底クラスです。

**主な機能**:
- [OK] `IsInitialized` フラグで初期化状態を一元管理
- [OK] `Initialize()` コルーチンで非同期初期化に対応
- [OK] InitializationManager との自動連携

---

## 従来のパターン（問題あり）

```csharp
// [NG] 初期化完了フラグがない
public class EscMenuCtrl : MonoBehaviour
{
    private void Awake()
    {
        _button = GetComponentInChildren<Button>();
    }
    
    private void Start()
    {
        _button.onClick.AddListener(OnClick);
        // ← ここで初期化完了。外部から状態を確認できない
    }
}

// 外部から初期化完了を確認する手段がない
yield return new WaitUntil(() => escMenuCtrl.IsInitialized);  // [NG] プロパティなし
```

---

## 新しいパターン（UIControllerBase 継承）

### Step 1: クラス宣言を変更

```csharp
// [変更前]
public class EscMenuCtrl : MonoBehaviour

// [変更後]
public class EscMenuCtrl : UIControllerBase
```

### Step 2: Awake() をオーバーライド（参照取得のみ）

```csharp
protected override void Awake()
{
    base.Awake();  // ← 重要：base.Awake() を呼び出し
    
    // Awake ではコンポーネント参照取得のみ
    _button = GetComponentInChildren<Button>();
    _text = GetComponentInChildren<Text>();
}
```

### Step 3: Initialize() コルーチンを実装

```csharp
protected override IEnumerator Initialize()
{
    // 実際の初期化処理をここに記述
    
    // パターン A: 即座に完了する場合
    _button.onClick.AddListener(OnClick);
    yield return null;
    
    // パターン B: GamePrefabs の準備完了を待つ場合
    yield return new WaitUntil(() => InitializationManager.IsGamePrefabsReady);
    _prefabs = GamePrefabs.GetGamePrefabs();
    
    // パターン C: EventLoader の準備完了を待つ場合
    yield return new WaitUntil(() => EventLoader.Instance.IsInitialized);
    _events = EventLoader.Instance.GetEvents();
}
```

### Step 4: Start() は削除（自動実行される）

```csharp
// [削除]
private void Start()  // ← 削除（base.Start() が自動的に Initialize() を呼び出す）
{
    // ...
}

// Start() をオーバーライドする場合は base.Start() を呼び出す
protected override void Start()
{
    base.Start();  // ← 重要：Initialize() を実行
    
    // その他の処理...
}
```

---

## 利用例

### 例 1: 単純なボタン制御

```csharp
public class ClosebtnCtrl : UIControllerBase
{
    private Button _closeButton;
    
    protected override void Awake()
    {
        base.Awake();
        _closeButton = GetComponentInChildren<Button>();
    }
    
    protected override IEnumerator Initialize()
    {
        // ボタンイベント設定
        _closeButton.onClick.AddListener(OnCloseButtonClicked);
        yield return null;
    }
    
    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }
}
```

**初期化フロー**:
```
Awake: base.Awake() → Awake 完了 (IsInitialized = false)
Start: base.Start() → StartCoroutine(InitializeAsync())
       → yield return Initialize()
       → _closeButton.onClick.AddListener(...)
       → yield return null
       → IsInitialized = true ✅
```

### 例 2: GamePrefabs に依存する場合

```csharp
public class MarkerIndicatorCtrl : UIControllerBase
{
    private Image _markerImage;
    private GameObject _markerPrefab;
    
    protected override void Awake()
    {
        base.Awake();
        _markerImage = GetComponentInChildren<Image>();
    }
    
    protected override IEnumerator Initialize()
    {
        // GamePrefabs の準備完了まで待機
        yield return new WaitUntil(() => InitializationManager.IsGamePrefabsReady);
        
        // GamePrefabs からマーカープレファブ取得
        var prefabs = GamePrefabs.GetGamePrefabs();
        if (prefabs.TryGetValue("MarkerPrefab", out var prefab))
        {
            _markerPrefab = prefab;
        }
        
        yield return null;
    }
}
```

**初期化フロー**:
```
Awake: base.Awake() → 完了 (IsInitialized = false)
Start: base.Start() → StartCoroutine(InitializeAsync())
       → yield return Initialize()
       → yield return new WaitUntil(() => GamePrefabs 準備完了)
       → prefabs 取得
       → yield return null
       → IsInitialized = true ✅
```

### 例 3: EventLoader に依存する場合

```csharp
public class GameTimerCtrl : UIControllerBase
{
    private Text _timerText;
    private float _remainingTime;
    
    protected override void Awake()
    {
        base.Awake();
        _timerText = GetComponentInChildren<Text>();
    }
    
    protected override IEnumerator Initialize()
    {
        // EventLoader 準備完了まで待機
        yield return new WaitUntil(() => EventLoader.Instance.IsInitialized);
        
        // タイマーイベントを取得
        var events = EventLoader.Instance.GetTimerEvents();
        _remainingTime = events.FirstOrDefault().Duration;
        
        yield return null;
    }
    
    private void Update()
    {
        if (!IsInitialized) return;  // 初期化完了まで何もしない
        
        _remainingTime -= Time.deltaTime;
        _timerText.text = _remainingTime.ToString("F1");
    }
}
```

---

## 外部からの初期化状態確認

### パターン A: WaitUntil で待機

```csharp
public class SomeGameSystem : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(WaitForUIReady());
    }
    
    private IEnumerator WaitForUIReady()
    {
        var escMenuCtrl = FindObjectOfType<EscMenuCtrl>();
        
        // IsInitialized フラグが true になるまで待機
        yield return new WaitUntil(() => escMenuCtrl.IsInitialized);
        
        // ここで EscMenuCtrl が確実に初期化完了している
        escMenuCtrl.DoSomething();
    }
}
```

### パターン B: IsReady プロパティで確認

```csharp
public class SomeGameSystem : MonoBehaviour
{
    private void Update()
    {
        var timerCtrl = FindObjectOfType<GameTimerCtrl>();
        
        // IsReady プロパティで確認
        if (timerCtrl.IsReady)
        {
            // GameTimerCtrl が初期化完了している
            Debug.Log("Timer is ready!");
        }
    }
}
```

### パターン C: InitializationManager で一括確認（推奨）

```csharp
public class UnitFireDisaster : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(StartGame());
    }
    
    private IEnumerator StartGame()
    {
        // すべてのコンポーネント初期化完了を待機
        yield return new WaitUntil(() => InitializationManager.IsInitialized);
        
        // ここでゲーム開始
        SpawnEnemies();
        StartGameTimer();
    }
}
```

---

## 実装チェックリスト

### クラス変更時に確認

- [OK] クラス宣言を `UIControllerBase` から継承に変更
- [OK] `base.Awake()` を Awake() の最初で呼び出し
- [OK] `Initialize()` コルーチンを実装（最低でも `yield return null`）
- [OK] 既存の `Start()` 処理を `Initialize()` に移行
- [OK] `Update()` で `if (!IsInitialized) return;` を先頭に追加（推奨）
- [OK] コンパイルエラーないか確認

### テスト時に確認

- [OK] Awake 後に IsInitialized が false か確認
- [OK] Initialize() 完了後に IsInitialized が true か確認
- [OK] WaitUntil で正しく待機できるか確認
- [OK] GamePrefabs への依存がある場合、順序は正しいか確認

---

## よくある質問

### Q1: 既存の Start() にあるコードはどうする？

**A**: `Initialize()` コルーチンに移行してください。

```csharp
// [Before]
private void Start()
{
    _button.onClick.AddListener(OnClick);
    _text.text = "Ready";
}

// [After]
protected override IEnumerator Initialize()
{
    _button.onClick.AddListener(OnClick);
    _text.text = "Ready";
    yield return null;
}
```

### Q2: Awake() で初期化処理をしていたらどうする？

**A**: `Initialize()` に移行してください。Awake() は参照取得のみにしてください。

```csharp
// [Before - NG]
private void Awake()
{
    _pool = new ObjectPool();
    _pool.Initialize(100);  // ← これは Initialize() へ
}

// [After - OK]
protected override void Awake()
{
    base.Awake();
    _pool = new ObjectPool();  // ← 参照取得のみ
}

protected override IEnumerator Initialize()
{
    _pool.Initialize(100);  // ← 実際の初期化
    yield return null;
}
```

### Q3: Start() をオーバーライドする必要がある場合は？

**A**: `base.Start()` を最初に呼び出してください。

```csharp
protected override void Start()
{
    base.Start();  // ← 重要：Initialize() を実行
    
    // その他の Start() 処理
    // 例: Animation 開始など
}
```

### Q4: yield return なしで完了する場合は？

**A**: `yield return null;` を記述してください。コルーチンには最低でも1回の yield が必要です。

```csharp
protected override IEnumerator Initialize()
{
    // 同期的に完了する処理のみ
    _button.onClick.AddListener(OnClick);
    
    yield return null;  // ← 必須（コルーチンの最小形）
}
```

---

## トラブルシューティング

### [症状] IsInitialized が false のまま

**原因**: `Initialize()` に `yield return` がない

**対策**: `Initialize()` に `yield return null;` を追加

```csharp
protected override IEnumerator Initialize()
{
    // 処理...
    yield return null;  // ← これがないとコルーチンが完了しない
}
```

### [症状] base.Start() を呼び出していない

**原因**: 既存の Start() を UIControllerBase に変更した際に base.Start() を忘れた

**対策**: `base.Start()` を追加

```csharp
protected override void Start()
{
    base.Start();  // ← 追加
    // ...
}
```

### [症状] Awake() で複雑な初期化をしている

**原因**: UIControllerBase はパターンに合わない場合がある

**対策**: `Initialize()` に移行するか、必要に応じて Awake() をカスタマイズ

```csharp
protected override void Awake()
{
    base.Awake();
    
    // 参照取得
    _button = GetComponentInChildren<Button>();
    
    // 複雑な初期化は Initialize() へ
    // (Awake は最小限に)
}
```

---

## まとめ

| 項目 | 従来 | UIControllerBase |
|------|------|-----------------|
| 初期化状態確認 | [NG] 手段なし | [OK] IsInitialized フラグ |
| 初期化待機 | [NG] 不可 | [OK] WaitUntil で可能 |
| 非同期対応 | [NG] StartCoroutine 必須 | [OK] Initialize() で対応 |
| 統一性 | [NG] 各クラスで異なる | [OK] 統一的な管理 |

**推奨**: すべての UI コントローラー（*Ctrl）を UIControllerBase から継承に変更してください。

---

**作成者**: GitHub Copilot  
**最終更新**: 2026-01-29
