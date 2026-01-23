# OnoCoro TODO リスト

## 概要

OnoCoro プロジェクトの実装状況、既知の問題、および今後の実装予定をまとめたドキュメントです。実装難度ごとにセクションを分けています。

---

## プロトタイプ版で実装予定の項目

テストユーザーへのリリース（v0.1.0-alpha）を目標とした実装。
実装難度が低く、短期的に完了可能な項目です。

**詳細ロードマップ**: [docs/prototype-phase-roadmap.md](docs/prototype-phase-roadmap.md)  
**目標リリース**: 2026-02-末  
**工数**: 約 60 人日（1人チーム 3-4週間）

### Phase 1: コア機能整備 & デバッグ環境 (Week 1-2)

#### Debug.Log ラッピング実装

- [ ] DebugLogger クラスの実装
- [ ] LogUtility.cs で5段階ログレベル実装（Debug/Info/Warning/Error/Fatal）
- [ ] Application.persistentDataPath へログファイル出力
- [ ] Assets/Scripts/ 内のすべての Debug → LogUtility/DebugLogger へ置換
- [ ] Development Build でのログ表示フィルタ設定
- [ ] 詳細: [docs/debug-logger-guide.md](docs/debug-logger-guide.md)

#### YAML ファイルフォーマット検証

- [ ] YamlValidator クラスの実装
- [ ] 検証対象: pathmakers, itemlists, stages, goals, boards セクション
- [ ] マーカー名・座標形式検証
- [ ] Editor スクリプトでの事前チェック機能
- [ ] エラーメッセージ定義・ユーザー向け出力
- [ ] 詳細: [docs/yaml-validation-guide.md](docs/yaml-validation-guide.md)

#### UIタグ導入 & キャンバス動的調整

- [ ] UIプレファブ系にUIタグを振る
- [ ] キャンバスレイヤーのサイズを動的に変更できるようにする
- [ ] 複数画面解像度テスト（1920×1080, 1280×720, タブレット対応）

#### Resources.Load の PrefabManager 統一

- [ ] Resources.Load で PrefabManager を使っていないものをすべてチェック
- [ ] すべての Resources.Load を PrefabManager で統一
- [ ] Bonfire の削除確認および削除処理
- [ ] 詳細: [.github/instructions/prefab-asset-management.instructions.md](.github/instructions/prefab-asset-management.instructions.md)

### Phase 2: ステージデザイン & ゲーム性調整 (Week 2-3)

#### ステージレベルデザイン (5段階)

- [ ] **Stage 1: チュートリアル** (Easy) - タワー配置基本操作
- [ ] **Stage 2: 基本マップ** (Normal) - 複数敵タイプ対応
- [ ] **Stage 3: 複合マップ** (Normal) - 複合タワー戦略
- [ ] **Stage 4: 高難度マップ** (Hard) - 全タイプ敵出現
- [ ] **Stage 5: エクストラ** (Very Hard) - 制限時間チャレンジ

**ステージ設計チェック**: PLATEAU 整合性、スポーン点配置、タワー配置エリア明確化、ゴール到達可能性、ウェーブバランス、UI見易さ、FPS安定性

#### ゲームバランス調整

- [ ] ユニットのコスト・効果のバランス調整
- [ ] 敵ユニットの難易度調整（HP・速度）
- [ ] ステージ難易度の段階的な設定
- [ ] リソース配分最適化（初期ゴールド・増加量）
- [ ] 10回プレイテスト（クリア率 60-70% 目標）
- [ ] バランス調整ドキュメント作成

#### カメラの Z-Fighting 対策

- [ ] 同じ優先順位の描画オブジェクト配置時に発生する Z-Fighting を修正
- [ ] y座標位置の微調整で画面ちらつきを解決
- [ ] Sorting Order ドキュメント作成

#### カメラ制御の最適化

- [ ] ズームレベルの動作感を再調整（期待値: 0.5秒で target zoom に到達）
- [ ] 視界カメラの障害物衝突による上下反転を修正
- [ ] カメラ制御のチュートリアル UI ガイダンス追加

### Phase 3: QA & リリース準備 (Week 3-4)

#### 進行不能バグの排除

- [ ] クリア不可バグの排除（敵が詰まる、タワー配置不可、ゴール到達不可）
- [ ] UI 非応答バグの排除（ボタン無反応、メニュー固定）
- [ ] データ破損バグの排除（ステージロード失敗、YAML パース失敗）
- [ ] クラッシュ排除（NullReferenceException、OutOfMemory）
- [ ] 全ステージ完全攻略プレイテスト（10時間以上連続プレイでクラッシュなし）
- [ ] ログ確認（Error/Exception なし）

#### パフォーマンス確保

- [ ] フレームレート 60 FPS（最小 45 FPS）を確保
- [ ] メモリ使用量 < 1GB
- [ ] ロード時間 < 3秒/ステージ
- [ ] Profiler での FPS・メモリ測定
- [ ] Texture 圧縮設定確認
- [ ] Physics.autoSimulation の最適化確認

#### ユーザー向けドキュメント作成

- [ ] README_Prototype.md（ゲーム概要・操作方法）
- [ ] GAMEPLAY_GUIDE.md（ゲーム進行ガイド・戦略）
- [ ] KNOWN_ISSUES.md（既知の問題・回避方法）
- [ ] BUG_REPORT_TEMPLATE.md（バグ報告テンプレート）
- [ ] LOG_GUIDE.md（ログファイル確認方法）

#### リリース前最終チェック

- [ ] Debug/Development/Release Build 成功
- [ ] Windows 10/11 動作確認
- [ ] GPU 互換性確認（Intel/NVIDIA/AMD）
- [ ] 最低仕様環境での動作確認
- [ ] Version 0.1.0-alpha に設定
- [ ] BuildNumber 001 に設定
- [ ] CHANGELOG.md 記載

#### リリース & テストユーザー対応

- [ ] Git tag: v0.1.0-alpha 作成
- [ ] Release notes 作成
- [ ] GitHub Releases で配布
- [ ] テストユーザー配布完了
- [ ] バグ報告チャネル設定完了

### .csproj ファイル整理（Recovery Phase 完了）

- [x] OnoCoro.sln (旧プロジェクト名) の削除 ✅ 2026-01-23
- [x] OnoCoro2024.sln (旧プロジェクト名) の削除 ✅ 2026-01-23
- [x] 不要な .csproj ファイル削除（42個） ✅ 2026-01-23

---

## アルファ版で実装予定の項目

実装難度が中程度で、複数の検証ステップが必要な項目です。詳細は [docs/alpha-phase.md](docs/alpha-phase.md) を参照してください。

### PlayMode バグ修正

- [ ] PlayMode で itemリスト が読み込まれないバグを修正

### カメラ制御の最適化

- [ ] ズームレベルの動作感を再調整
- [ ] 視界カメラの障害物衝突による上下反転を修正

### UI マウスポインター動作

- [ ] WASD移動時のマウスポインター消失を修正
- [ ] スポーンポイントマーカー表示時の挙動改善

### InitializationManager 統合

- [ ] 各CtrlクラスへのInitializationManager適用（FireCubeCtrl、GarbageCubeCtrl等）
- [ ] UnitFireDisaster の初期化待機処理を修正
- [ ] Script Execution Order の設定

### Navmesh 調整

- [ ] NavMesh Surface が地面から浮いている状態を修正

### SentryGuard アーキテクチャの統一化

- [ ] SentryGuard.cs を動的アタッチ（GetOrAddComponent パターン）に変更
- [ ] 関連ファイル: `SentryGuardCtrl.cs`, `SpawnCtrl.cs`, `SentryGuard.cs`

### 敵ユニット移動経路の最適化

- [ ] 敵ユニット（Litter等）がタワー配置後に詰まる問題を対策
- [ ] 対策案A: 動的 NavMesh 再ベイク
- [ ] 対策案B: 経路探索ロジック追加
- [ ] 対策案C: 配置前検証
- [ ] 詳細: [docs/fixes/enemy-pathfinding.md](docs/fixes/enemy-pathfinding.md)

### 不使用フィジックマテリアルの削除

- [ ] `Assets/Resources/New Physic Material.physicMaterial` を削除
- [ ] PlayerArmature の使用状況を完全に確認

### Debug.log ラッピング実装

- [ ] DebugLogger クラスの実装
- [ ] 既存コード内の Debug → DebugLogger への置換
- [ ] ファイル出力機能（.log ファイル）
- [ ] 詳細: [docs/debug-logger-guide.md](docs/debug-logger-guide.md)

### YAML ファイルフォーマット検証

- [ ] YamlValidator クラスの実装
- [ ] 検証対象: pathmakers, itemlists, stages, goals, boards セクション
- [ ] マーカー名・座標形式検証
- [ ] Editor スクリプトでの事前チェック機能
- [ ] 詳細: [docs/yaml-validation-guide.md](docs/yaml-validation-guide.md)

### 建物燃焼の視覚化 - 順序マーカー実装

- [ ] 燃焼順序リストの実装
- [ ] UI 表示（TextMeshPro による番号表示）
- [ ] カメラズーム対応
- [ ] 詳細: [docs/features/fire-visualization.md](docs/features/fire-visualization.md)

### ゲーム効果音・BGM

- [ ] パワーキューブ獲得時の効果音
- [ ] ユニット配置時の効果音
- [ ] ステージクリア・ゲームオーバーBGM
- [ ] イベント別効果音の実装

### ゲームバランス調整

- [ ] ユニットのコスト・効果のバランス調整
- [ ] 敵ユニットの難易度調整
- [ ] ステージ難易度の段階的な設定

### ユニット充実・ユニットアップグレード

- [ ] 既存タワーの種類拡充
- [ ] 敵ユニットの種類拡充
- [ ] ユニットのアップグレード機能実装
- [ ] アップグレード UI の構築

### ステージ装飾

- [ ] ステージの環境演出改善
- [ ] パーティクルエフェクト追加
- [ ] カメラアニメーション演出

### ユニットアンロック要素

- [ ] ユニット解放条件の設定
- [ ] ユニット解放 UI の実装
- [ ] プログレッション管理システム

---

## ベータ版で実装予定の項目

実装難度が高く、詳細な設計と複数工数が必要な項目です。詳細は [docs/beta-phase.md](docs/beta-phase.md) を参照してください。

### Bloom シェーダー修正

- [ ] Bloom付きシェーダーマテリアルの不透明度問題を解決
- [ ] エラー頻発の根本原因を特定し修正

### ゲームセーブ機能

- [ ] セーブデータ形式の定義（JSON/Binary）
- [ ] セーブ・ロード機能の実装
- [ ] セーブスロット管理 UI
- [ ] プレイヤープログレッション保存

### マテリアル・グラフィックス改善

- [ ] 建物崩壊時のひび割れマテリアル
- [ ] ダメージ表現の視覚化
- [ ] パーティクルエフェクト強化
- [ ] シェーダーの最適化

### ブレンダー・3Dアセット制作

- [ ] Blender での 3D モデル制作パイプライン確立
- [ ] UV マッピング・テクスチャリング
- [ ] PLATEAU データ連携での カスタムモデル統合

### リファクタリング・最適化

- [ ] `GameObject.Find()` の廃止（キャッシュ化）
- [ ] `Resources.Load()` の削除・PrefabManager 統一
- [ ] スクリプト構造の大規模見直し
- [ ] 詳細: [docs/performance-tuning.md](docs/performance-tuning.md)

### NavMesh 非依存の経路探索実装

- [ ] PathfinderUtility クラスの実装（A*アルゴリズム）
- [ ] グリッドベース経路計算
- [ ] 既存 NavMeshAgent との統合テスト
- [ ] 詳細: [docs/pathfinding-guide.md](docs/pathfinding-guide.md)

### パフォーマンス チューニング

実装予定の最適化項目：

- [ ] `GameObject.Find()` の廃止（アセット参照キャッシュ化）
- [ ] Unity Profiler（Development Build）での計測
- [ ] Memory Profiler での分析
- [ ] Heap Explorer（OSS）での詳細検査
- [ ] Physics.autoSimulation の最適化検討
- [ ] AssetPostprocessor の活用（テクスチャ圧縮等）

詳細: [docs/performance-tuning.md](docs/performance-tuning.md)

### 多言語対応

- [ ] 日本語・英語・中国語などの言語対応
- [ ] ローカライゼーション UI の実装
- [ ] 詳細: [docs/localization-support.md](docs/localization-support.md)

### SubstancePainter テクスチャ作成

- [ ] SubstancePainter を使用したテクスチャ制作ワークフロー導入
- [ ] 自動化スクリプト作成検討

### Utility/ フォルダ構成の統一

- [ ] Utility/ → Utilities/（複数形）への移行
- [ ] C# 標準命名慣例に従う（System.Collections, System.IO など参考）
- [ ] 実施時期: プロジェクト安定化後、全体リファクタリング時

---

## リリース後・将来の実装予定

Prototype/Alpha/Beta 完成後の拡張機能及び大規模システム

### マルチプレイ対応

- [ ] ネットワーク同期システムの設計
- [ ] プレイヤー間通信プロトコル定義
- [ ] クライアント・サーバーアーキテクチャ検討
- [ ] ロビー・マッチメイキング機能
- [ ] リアルタイムプレイヤー同期
- [ ] 詳細: [docs/multiplayer-design.md](docs/multiplayer-design.md)

### AR（拡張現実）サポート

- [ ] ARCore/ARKit 統合
- [ ] 地理座標に基づく AR オーバーレイ
- [ ] 実世界地図との同期
- [ ] モバイル UI 適応
- [ ] ジオロケーション機能
- [ ] 詳細: [docs/ar-implementation.md](docs/ar-implementation.md)

---

## 完了した項目

### ✅ 2026-01-22: 建物燃焼の視覚化要件検討

建物が燃えた順序を UI 上で視覚的に表示する要件を確定。

### ✅ 2026-01-21: NavMesh 非依存経路探索の設計

A* アルゴリズムによる経路計算エンジンの実装予定を確定。

### ✅ 2026-01-21: Debug.log ラッピングの実装設計

DebugLogger クラスの実装予定とファイル出力ロジックを確定。

### ✅ 2026-01-19: Path Marker 名前の定数化

`GameEnum.PathMarkerNameParts` に YAML 命名規則用定数を追加。

**実施内容**:
- GameEnum に PathMarkerNameParts クラスを追加
- START = "start", GOAL = "goal" の定数化
- PathMakerCtrl.GetMarkerColorByName() をハードコード文字列から定数参照に変更

**関連ファイル**:
- Assets/Scripts/APP/GameEnum.cs
- Assets/Scripts/GameEvents/PathMakerCtrl.cs

### ✅ 2026-01-18: SpawnMarkerPointerCtrl 復旧

マーカー表示ロジックが正常に動作。

**実施内容**:
- SpawnMarkerPointerCtrl 復旧（Awake内で TextMeshPro キャッシング機能実装）
- Canvas 配下の階層対応で柔軟な配置に対応
- ItemHolderCtrl で IsLoupe チェック時のマーカーOFF処理を追加
- 不使用アセット `Assets/GameEvents/GroundPhysicMaterial.physicMaterial` を削除

**関連ファイル**:
- Assets/Scripts/UI/SpawnMarkerPointerCtrl.cs
- Assets/Scripts/Item/ItemHolderCtrl.cs

### ✅ 2026-01-17: SVG 画像読み込みの統一

SpriteResourceLoader ユーティリティで SVG/通常 Sprite の読み込みを一元化。

**実施内容**:
- SpriteResourceLoader ユーティリティクラスの作成
- SVG/通常 Sprite のリソース読み込みを一元化
- 自動 Sprite Mesh 有効化（Reflection を使用）
- ItemHolderCtrl.cs, ItemCreateCtrl.cs を統一

**関連ファイル**:
- Assets/Scripts/Utilities/SpriteResourceLoader.cs
- Assets/Scripts/Item/ItemHolderCtrl.cs
- Assets/Scripts/Item/ItemCreateCtrl.cs
- docs/svg-image-import-guide.md

### ✅ 2026-01-16: Recovery フォルダマージ作業

複数の CS ファイルを比較マージし、機能の統合を完了。

**実施内容**:
- TabMenuCtrl.cs - UIBG_mask制御機能を復元
- TelopCtrl.cs - 問題なし（改善のみ）
- TitleStartCtrl.cs - 大規模な改善確認
- TooltipInfoCtrl.cs - 問題なし
- TowerSweeper.cs - IsPowerState()とゲーム速度調整を復元
- TowerSentryGuard.cs - 新規作成
- TowerDustBox.cs - 新規作成
- UnitPathBloom.cs - 新規作成

**TODO**: Assets/Recovery/.Editor/ILspy_CS フォルダの削除

### ✅ 2026-01-16: ルートフォルダ .csproj ファイル整理

旧ソリューション（OnoCoro.sln, OnoCoro2024.sln）の削除対象を確定。

**削除予定ファイル一覧**: 42ファイル（詳細は [docs/cleanup/csproj-files-to-delete.md](docs/cleanup/csproj-files-to-delete.md) 参照）

**注意事項**:
- Unityが自動生成したプロジェクトファイル
- 削除後にUnity起動時に再生成される可能性
- 削除前に必ずバックアップを確認

### ✅ 2026-01-11: CameraCtrl リファクタリング

カメラモードの enum 化と責務分離を実装。

**実施内容**:
- カメラモードの enum 化（FPS, TPS, LongShot, BirdView）
- SetCameraDistance() を各モード専用メソッドに分割
  - ApplyFPSMode()
  - ApplyTPSMode()
  - ApplyLongShotMode()
  - ApplyBirdViewMode()
- カメラオブジェクトのキャッシュ化による最適化
- 補間処理の一元化（UpdateCameraDistanceSmooth()）
- 不要コード削除（EasingCameraOffset(), GetPlayerCamera()）

**関連ファイル**: Assets/Scripts/View/CameraCtrl.cs

**TODO**: ズームレベルの動作感を再調整

### ✅ 2026-01-11: InitializationManager 実装

マネージャークラスによるコンポーネント初期化の順序制御を実装。

**実施内容**:
- InitializationManager クラスの作成（シングルトンパターン）
- 全 Awake メソッド完了後に順序制御された初期化を実行
- IsInitialized: 全体の初期化完了フラグ
- IsStepInitialized(stepName): ステップごとの初期化完了確認
- InitializeAllComponents(): 段階的な初期化処理
- ResetInitialization(): シーン遷移時の初期化状態リセット

**初期化ステップ**:
- ステップ1: リソースローダーの初期化
- ステップ2: マネージャークラスの初期化
- ステップ3: UIコンポーネントの初期化

**推奨設定**:
- Script Execution Order: -100 (最優先実行)
- シーンに空の GameObject を作成してアタッチ
- DontDestroyOnLoad でシーン遷移時も保持

**関連ファイル**:
- Assets/Scripts/Utility/InitializationManager.cs
- Assets/Scripts/Utility/InitializationManager_Usage.md

**TODO**:
- 各 Ctrl クラスへの適用（FireCubeCtrl, GarbageCubeCtrl等）
- UnitFireDisaster の修正
- Script Execution Order の設定

### ✅ 2026-01-10: ユーティリティクラスへの大規模リファクタリング

複数のユーティリティクラスを新規作成し、TitleStartCtrl を 60% 削減。

**実施内容**:

**1. LoadStreamingAsset 拡張**
- YAML関連メソッド追加: GetYamlFileName(), YamlFileExists()
- ファイル名定数追加: YAML_FILE_EXTENSION, STAGE_LIST_FILE_NAME 等
- StreamingAssets操作の完全な集約化

**2. 新規ユーティリティクラス**:
- FileOperationUtility (122行): プラットフォーム別エディタ起動、画像読み込み
- UIHelper (216行): GameObject検索、ボタン登録、スクロール制御、パネルセットアップ
- LogUtility (173行): レベル別ログ出力(Debug/Info/Warning/Error)、ファイル記録機能
- SceneLoaderUtility (44行): シーン遷移管理、遅延読み込み対応
- StageDataManager (137行): CSV読み込み、シーンデータ管理、重複処理

**3. UIスクロール制御の改善**:
- ScrollRect.normalizedPosition を使用
- アクティブ状態チェックを関数内に内包
- GetComponentInChildren(true)で非アクティブな子も検索可能

**4. TitleStartCtrl の大幅削減**:
- 742行 → 約300行 (約60%削減)
- 削除したフィールド: _StageScrollbar, _EditorScrollbar
- 削除した定数: ファイル関連定数(4個)、エディタコマンド(4個)、メッセージ(8個)
- 削除したメソッド: UI操作(7個)、データ管理(4個)、LoadScene関係

**5. FontAssetPostProcessor 修正**:
- 無限ループ問題を hasChanges フラグで解決

**技術詳細**:

**ScrollRect 制御**:
- Scrollbar.value 直接操作から ScrollRect.normalizedPosition に変更
- 理由: ScrollRect がScrollbar を制御するため直接値変更は無視される
- 実装: normalizedPosition = new Vector2(0, 1) でトップに移動
- タイミング: コンテンツ設定後（テキスト・アイテム追加後）にリセット

**LogUtility 機能**:
- エディタ: 全レベル表示、ファイル出力なし
- ビルド: Warning 以上のみ表示 + ファイル出力
- ファイル: Application.persistentDataPath/game.log
- 条件付きコンパイル: #if UNITY_EDITOR で制御

**CSV 重複処理**: ContainsKey チェックでスキップ + 警告ログ

**コミット情報**:
- コミット: 5239f1a3
- メッセージ: "refactor(ui): ユーティリティクラスへの機能集約とスクロール制御の改善"
- 変更: 20ファイル, +1468行, -710行

**関連ファイル**:
- docs/refactoring/utility-classes.md

---

## 既知の問題

### WindowCloseCtrl - イベント重複登録の懸念

**概要**: 
WindowCloseCtrl.Awake() で Persistent イベントのみを検出するため、実行時に追加されたリスナーとの重複登録の可能性がある。

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

**推奨対策**:
- ボタンイベント登録パターンの統一
- WindowCloseCtrl 使用時は手動イベント登録を禁止
- 一元的なイベント管理マネージャーの導入検討

**関連ファイル**: Assets/Scripts/UI/WindowCloseCtrl.cs

詳細: [docs/known-issues/button-event-duplication.md](docs/known-issues/button-event-duplication.md)

---

## ファイル構成

新規作成予定のドキュメントファイル一覧:

```
docs/
  ├── alpha-phase.md                        アルファ版実装詳細
  ├── beta-phase.md                         ベータ版実装詳細
  ├── performance-tuning.md                 パフォーマンス最適化
  ├── localization-support.md               多言語対応
  ├── debug-logger-guide.md                 Debug ラッピング使用ガイド
  ├── yaml-validation-guide.md              YAML バリデーション使用ガイド
  ├── pathfinding-guide.md                  経路探索（A*）使用ガイド
  ├── cleanup/
  │   └── csproj-files-to-delete.md         削除対象 .csproj ファイルリスト
  ├── refactoring/
  │   └── utility-classes.md                ユーティリティクラス詳細
  ├── fixes/
  │   ├── playmode-item-list-bug.md         PlayMode バグ詳細
  │   ├── bloom-shader-issue.md             Bloom シェーダー問題
  │   └── enemy-pathfinding.md              敵経路最適化詳細
  ├── features/
  │   ├── camera-optimization.md            カメラ制御詳細
  │   └── fire-visualization.md             建物燃焼視覚化詳細
  └── known-issues/
      └── button-event-duplication.md       ボタンイベント重複登録の懸念
```

---

**最終更新**: 2026-01-23
**プロジェクト**: OnoCoro (Unity 6.3 Geospatial Visualization)
