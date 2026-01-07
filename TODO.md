# TODO / 備忘録

## UI関連

### WindowCloseCtrl - イベント重複登録の懸念

**日付**: 2026-01-08

**問題**: 
- `WindowCloseCtrl.Awake()`で`GetPersistentEventCount() > 0`をチェックしてイベント登録を回避している
- しかし、この方法はエディタ上で設定されたイベント（Persistent Event）のみを検出する
- 実行時に他のスクリプトの初期化処理で`AddListener()`された場合は検出できない

**懸念シナリオ**:
```csharp
// 例: シーン固有の初期化スクリプト
void Start()
{
    closeButton.onClick.AddListener(CustomCloseHandler);
    // この後、WindowCloseCtrl.Awake()が実行されると
    // CustomCloseHandlerとCloseWindowの両方が登録されてしまう
}
```

**影響**:
- ボタンクリック時に複数のCloseメソッドが呼ばれる
- 意図しない動作や二重処理が発生する可能性

**対策案**:
1. **命名規則の統一**: Close処理を持つボタンには特定のタグや命名規則を設定
2. **イベント登録の一元管理**: シーン初期化時にすべてのUIイベントを登録する専用マネージャーを作成
3. **実行時チェックの追加**: `button.onClick.GetPersistentEventCount() + button.onClick.GetListenerCount()`で総数をチェック
   - ただし、GetListenerCount()は存在しないため、独自の管理が必要
4. **ドキュメント化**: WindowCloseCtrlを使用する場合は、手動でイベントを登録しないことをルール化

**推奨アクション**:
- プロジェクト全体でボタンイベントの登録パターンを統一する
- WindowCloseCtrlを使用するボタンには、Inspector上でもスクリプトでもイベントを登録しないルールを徹底

**関連ファイル**:
- `Assets/Scripts/UI/WindowCloseCtrl.cs`
