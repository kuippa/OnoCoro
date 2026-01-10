# TODO / 備忘録

## 作業履歴

### 2026-01-11: CameraCtrlのリファクタリング

**実施内容**:

1. **カメラモードのenum化**
   - `CameraMode`列挙型を追加（FPS, TPS, LongShot, BirdView）
   - モード判定ロジックを`GetCameraModeFromZoomLevel()`に統一

2. **責務の分離**
   - 巨大だった`SetCameraDistance()`を各モード専用のメソッドに分割：
     - `ApplyFPSMode()` - FPSモードの設定
     - `ApplyTPSMode()` - TPSモードの設定
     - `ApplyLongShotMode()` - ロングショットの設定
     - `ApplyBirdViewMode()` - バードビューの設定

3. **最小値・最大値の適切な処理**
   - FPS/TPSモードで`Mathf.Clamp()`により範囲制限を明示化
   - カメラ切り替え時の境界値処理を改善

4. **カメラオブジェクトのキャッシュ**
   - `CacheCamera()`メソッドで初回にオブジェクトをキャッシュ
   - 毎フレーム`GameObject.Find()`を実行しないように最適化

5. **補間処理の統一**
   - `UpdateCameraDistanceSmooth()`メソッドで補間処理を一元化
   - モード切替時の補間リセット処理を整理

6. **不要コードの削除**
   - 未使用の`EasingCameraOffset()`メソッドを削除
   - 未使用の`GetPlayerCamera()`メソッドを削除

**TODO**:
- [ ] **ズームレベルの動作感の再調整**: 現在の実装では動作感が適切でないため、将来的にズーム速度や範囲の調整が必要

**関連ファイル**:
- `Assets/Scripts/View/CameraCtrl.cs`

---

### 2026-01-10: ユーティリティクラスへの大規模リファクタリング

**実施内容**:

1. **LoadStreamingAssetの拡張**
   - YAML関連メソッド追加: `GetYamlFileName()`, `YamlFileExists()`
   - ファイル名定数の追加: `YAML_FILE_EXTENSION`, `STAGE_LIST_FILE_NAME`, `ABOUT_GAME_FILE_NAME`, `NOTICE_FILE_NAME`
   - StreamingAssets操作の完全な集約化

2. **新規ユーティリティクラスの作成**
   - **FileOperationUtility** (122行): プラットフォーム別エディタ起動、画像読み込み
   - **UIHelper** (216行): GameObject検索、ボタン登録、スクロール制御、パネルセットアップ
   - **LogUtility** (173行): レベル別ログ出力(Debug/Info/Warning/Error)、ファイル記録機能
   - **SceneLoaderUtility** (44行): シーン遷移管理、遅延読み込み対応
   - **StageDataManager** (137行): CSV読み込み、シーンデータ管理、重複処理

3. **UIスクロール制御の改善**
   - `UIHelper.ResetScrollbarInPanel()`: ScrollRect.normalizedPositionを使用
   - アクティブ状態チェックを関数内に内包
   - 変数格納不要、親パネルから自動検索
   - GetComponentInChildren(true)で非アクティブな子も検索可能

4. **TitleStartCtrlの大幅削減**
   - **742行 → 約300行 (約60%削減)**
   - 削除した要素:
     - フィールド: `_StageScrollbar`, `_EditorScrollbar`
     - 定数: `_CHILD_PATH_SCROLLVIEW_SCROLLBAR`, ファイル関連定数(4個)、エディタコマンド(4個)、メッセージ(8個)
     - メソッド: 7個のUI操作、4個のデータ管理、LoadScene関係
   - ユーティリティクラスへの移行完了

5. **FontAssetPostProcessor修正**
   - 無限ループ問題の解決: `hasChanges`フラグで条件付きSaveAssets実行

6. **instructions.md規約追加**
   - セクション8: ユーティリティクラスの設計原則
   - セクション9: UIスクロールビューの取り扱い
   - コード提案前のチェックリスト(11項目)
   - 各ユーティリティクラスの使用例とパターン

**技術詳細**:

- **ScrollRect制御**: `Scrollbar.value`直接操作から`ScrollRect.normalizedPosition`に変更
  - 理由: ScrollRectがScrollbarを制御するため直接値変更は無視される
  - 実装: `normalizedPosition = new Vector2(0, 1)` でトップに移動
  - タイミング: コンテンツ設定後(テキスト・アイテム追加後)にリセット

- **LogUtility機能**:
  - エディタ: 全レベル表示、ファイル出力なし
  - ビルド: Warning以上のみ表示+ファイル出力
  - ファイル: `Application.persistentDataPath/game.log`
  - 条件付きコンパイル: `#if UNITY_EDITOR`

- **CSV重複処理**: `ContainsKey`チェックでスキップ+警告ログ

**コミット情報**:
- コミット: `5239f1a3`
- メッセージ: "refactor(ui): ユーティリティクラスへの機能集約とスクロール制御の改善"
- 変更: 20ファイル, +1468行, -710行

**次のステップ**:
- Phase 2完了に向けたStageUIBuilder作成(オプション)
- Phase 3: TitleStartConstants作成(オプション)
- 目標: TitleStartCtrl 250-300行

---

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
