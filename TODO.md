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

### 2026-01-11: InitializationManagerの実装

**実施内容**:

1. **InitializationManagerクラスの作成**
   - コンポーネント初期化の順序制御を行うマネージャー
   - シングルトンパターンで実装
   - 全てのAwakeメソッド完了後に順序制御された初期化を実行

2. **主要機能**
   - `IsInitialized`: 全体の初期化完了フラグ
   - `IsStepInitialized(stepName)`: ステップごとの初期化完了確認
   - `InitializeAllComponents()`: 段階的な初期化処理
   - `ResetInitialization()`: シーン遷移時の初期化状態リセット

3. **初期化ステップ**
   - ステップ1: リソースローダーの初期化
   - ステップ2: マネージャークラスの初期化
   - ステップ3: UIコンポーネントの初期化
   - 各ステップは拡張可能な設計

4. **使用パターン**
   - パターン1: 全体の初期化完了を待つ（シンプル）
   - パターン2: 部品側に初期化完了フラグを実装
   - パターン3: ステップごとの初期化制御（高度）
   - パターン4: 非同期処理の完了待ち

5. **詳細ドキュメント**
   - `InitializationManager_Usage.md`: 使用ガイド
   - 問題点の説明、セットアップ手順、使用方法
   - ベストプラクティス、トラブルシューティング
   - 完全な実装例とサンプルコード

**推奨設定**:
- Script Execution Order: `-100` (最優先実行)
- シーンに空のGameObjectを作成してアタッチ
- DontDestroyOnLoadでシーン遷移時も保持

**TODO**:
- [ ] **各Ctrlクラスへの適用**: FireCubeCtrl、GarbageCubeCtrl等に初期化完了フラグを実装
- [ ] **UnitFireDisasterの修正**: InitializationManagerを使用した初期化待機処理に変更
- [ ] **Script Execution Orderの設定**: Project Settingsで実行順序を設定

**関連ファイル**:
- `Assets/Scripts/Utility/InitializationManager.cs`
- `Assets/Scripts/Utility/InitializationManager_Usage.md`

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

---

### 2026-01-16: ルートフォルダの.csprojファイル整理

**削除予定のソリューションファイル**:
- `OnoCoro.sln` (旧プロジェクト名)
- `OnoCoro2024.sln` (旧プロジェクト名)

**OnoCoro2026.slnxに参照されていない削除予定の.csprojファイル**:

以下のファイルは最新のソリューション（OnoCoro2026.slnx）に参照されていません。
バックアップからの復元が完全に完了したら削除を検討してください。

1. `Cinemachine.csproj` (旧バージョン、Unity.Cinemachine.csprojが使用中)
2. `com.unity.cinemachine.editor.csproj` (旧バージョン)
3. `FbxBuildTestAssets.csproj` (Autodesk.Fbx.BuildTestAssets.csprojが使用中)
4. `PLATEAU.EditModeTests.csproj`
5. `PLATEAU.PlayModeTests.csproj`
6. `PLATEAU.Sample.csproj`
7. `PLATEAU.TestUtils.csproj`
8. `PPv2URPConverters.csproj`
9. `Unity.FilmInternalUtilities.csproj`
10. `Unity.FilmInternalUtilities.Editor.csproj`
11. `Unity.InternalAPIEditorBridge.003.csproj`
12. `Unity.InternalAPIEngineBridge.003.csproj`
13. `Unity.Rendering.LightTransport.Runtime.csproj`
14. `Unity.RenderPipeline.Universal.ShaderLibrary.csproj`
15. `Unity.RenderPipelines.Core.Samples.Editor.csproj`
16. `Unity.RenderPipelines.Core.Samples.Runtime.csproj`
17. `Unity.RenderPipelines.Universal.2D.Runtime.csproj`
18. `Unity.RenderPipelines.Universal.Config.Runtime.csproj`
19. `Unity.RenderPipelines.Universal.Editor.csproj`
20. `Unity.RenderPipelines.Universal.Runtime.csproj`
21. `Unity.RenderPipelines.Universal.Shaders.csproj`
22. `Unity.Services.Core.Analytics.csproj`
23. `Unity.Services.Core.Configuration.csproj`
24. `Unity.Services.Core.Configuration.Editor.csproj`
25. `Unity.Services.Core.csproj`
26. `Unity.Services.Core.Device.csproj`
27. `Unity.Services.Core.Editor.csproj`
28. `Unity.Services.Core.Environments.csproj`
29. `Unity.Services.Core.Environments.Internal.csproj`
30. `Unity.Services.Core.Internal.csproj`
31. `Unity.Services.Core.Networking.csproj`
32. `Unity.Services.Core.Registration.csproj`
33. `Unity.Services.Core.Scheduler.csproj`
34. `Unity.Services.Core.Telemetry.csproj`
35. `Unity.Services.Core.Threading.csproj`
36. `Unity.Toonshader.csproj`
37. `Unity.Toonshader.Editor.csproj`
38. `Unity.VSCode.Editor.csproj`
39. `VFXGraph.OutputEventHandlers.csproj`
40. `VFXGraph.OutputEventHandlers.Editor.csproj`
41. `YamlDotNet.Examples.csproj`

**注意事項**:
- これらのファイルはUnityが自動生成したプロジェクトファイルです
- 削除前に必ずバックアップを確認してください
- Unityエディタでプロジェクトを開くと再生成される場合があります
- 削除後にUnity起動時に問題が発生した場合は、Unityに再生成させることで復旧可能です

**TODO**:
- [ ] バックアップからの復元が完全に完了したことを確認
- [ ] 上記の.csprojファイルの削除を実行
- [ ] OnoCoro.slnとOnoCoro2024.slnの削除を実行
- [ ] Unity起動確認とプロジェクトファイル再生成の確認
---

### 2026-01-16: Recovery フォルダのマージ作業（進行中）

**実施内容**:

1. **比較マージの実施**
   - TabMenuCtrl.cs - UIBG_mask制御機能を復元
   - TelopCtrl.cs - 問題なし（改善のみ）
   - TitleStartCtrl.cs - 大規模な改善確認
   - TooltipInfoCtrl.cs - 問題なし
   - TowerSweeper.cs - IsPowerState()とゲーム速度調整を復元
   - TowerSentryGuard.cs - 新規作成
   - TowerDustBox.cs - 新規作成
   - UnitPathBloom.cs - 新規作成

2. **重要な気づき**
   - `Agent type = Auto` 時にセッション情報が失われることを確認
   - ドキュメント読み込みルールを AGENTS.md に明記
   - 機能的な変更を見落とさないよう複数回確認が必要

**TODO**:
- [ ] **Assets/Recovery/.Editor/ILspy_CS フォルダの削除** - マージ完了後、不要な復元ファイルフォルダを削除
- [ ] 残りのマージ対象ファイルの確認と処理
- [ ] エラーログの確認とデバッグ

---

### 2026-01-17: SVG 画像読み込みの統一と最適化

**実施内容**:

1. **SpriteResourceLoader ユーティリティクラスの作成**
   - SVG/通常 Sprite のリソース読み込みを一元化
   - パスの自動変換（サフィックス付き/なし対応）
   - Null チェックと詳細なエラー処理

2. **自動 Sprite Mesh 有効化**
   - Reflection を使用して `image.useSpriteMesh = true` を自動設定
   - Editor 上での手動チェック忘れを防止
   - 動的画像設定時もコード側で対応

3. **既存コードの統一**
   - ItemHolderCtrl.cs: Resources.Load → SpriteResourceLoader 経由に変更
   - ItemCreateCtrl.cs: SetSpriteToImage メソッド利用に統一
   - TitleStartCtrl.cs: 既に FileOperationUtility 経由で対応

4. **ドキュメント整備**
   - svg-image-import-guide.md に手順と注意点を記載
   - ソース画像なしでも Sprite Mesh 設定可能な方法を記載
   - useSpriteMesh プロパティの詳細を記載

**関連ファイル**:
- `Assets/Scripts/Utilities/SpriteResourceLoader.cs`
- `Assets/Scripts/Item/ItemHolderCtrl.cs`
- `Assets/Scripts/Item/ItemCreateCtrl.cs`
- `docs/svg-image-import-guide.md`

**TODO**:
- [ ] **フォルダ構成の統一: Utility/ → Utilities/（複数形）への移行**
  - 現状: Assets/Scripts/Utility/ (26ファイル) と Assets/Scripts/Utilities/ (新規、SpriteResourceLoader.cs)
  - 標準的な C# 命名慣例に従い複数形に統一
  - 参考: System.Collections, System.IO など
  - 実施時期: プロジェクト安定化後、全体リファクタリング時