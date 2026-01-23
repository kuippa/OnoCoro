# Assets/Scripts フォルダ構成改善提案書

**作成日**: 2026-01-23  
**対象**: OnoCoro v0.1.0-alpha (Prototype Phase)  
**目的**: Scripts フォルダの整理・標準化による保守性向上

---

## 📊 現状分析

### 現在のフォルダ構成（20フォルダ）

```
Assets/Scripts/
├── .Editor/                    # Editor 関連スクリプト
├── APP/                        # ゲーム全体管理（6ファイル）
├── Bullet/                     # 発射物システム
├── Editor/                     # Editor 拡張機能
├── Enemy/                      # 敵ユニット管理
├── GameEvents/                 # ゲーム進行イベント（10ファイル）
├── Item/                       # アイテムシステム
├── Models/                     # データモデル（15ファイル）
├── Plateau/                    # PLATEAU SDK 統合
├── Player/                     # プレイヤー操作管理
├── StageOrnaments/             # ステージ装飾（パーティクル等）
├── Stages/                     # ステージデータ・管理
├── Tower/                      # タワーシステム（15ファイル）
├── UI/                         # UI コンポーネント（20ファイル）
├── UnitTest/                   # ユニットテスト
├── Utilities/                  # ユーティリティ（1ファイル）← 名前重複
├── Utility/                    # ユーティリティ（25ファイル）← 名前重複
└── View/                       # ビューシステム
```

**ファイル数合計**: 約 140 ファイル

### 🔴 発見された問題

#### 1. 命名の混在
- `Utilities/` (複数形 - 新しい命名規則)
- `Utility/` (単数形 - 従来の命名規則)
- **問題**: C# 標準 namespace 命名と矛盾（`System.Collections`, `System.IO` は複数形）

#### 2. Utility フォルダの過度な混在
`Utility/` に25ファイルが混在：
```
ユーティリティ クラス（ 7個）:
  - FileOperationUtility
  - LogUtility
  - SceneLoaderUtility
  - StageDataManager
  - UIHelper
  - PlateauUtility
  - PrefabManager

マネージャー（ 6個）:
  - GameCtrl
  - GameSpeedCtrl
  - InitializationManager
  - MaterialManager
  - NavMeshCtrl
  - StagingYamlCtrl

制御クラス（ 5個）:
  - BloomPathCtrl
  - CoroutineRunner
  - LangCtrl
  - MarkerIndicatorCtrl
  - WindCtrl

データ・定数クラス（ 5個）:
  - GameConfig
  - GameConst
  - LangConst
  - PlateauInfo
  - XMLparser

その他（ 2個）:
  - CommonsCalcs
  - GameObjectTreat
```

#### 3. 責務の不明確性
| フォルダ | 責務 | 現状 |
|---------|------|------|
| APP | ゲーム全体制御 | InputController, GameEnum など混在 |
| Models | データモデル | ユニット Struct と実体クラス混在 |
| GameEvents | イベント駆動 | イベント定義 + 制御クラス混在 |
| Plateau | PLATEAU 統合 | PLATEAU 専用 Utility が Utility に分散 |

#### 4. Editor スクリプトの配置
- `.Editor/` と `Editor/` の2フォルダが存在
- 責務が不明確

---

## 🎯 現在のフォルダ構成【実装済み】

### コアアーキテクチャ: 3層 + オーソゴナル構成

**更新日**: 2026-01-24 (フォルダ移行 Phase 1-2 完了)

```
Assets/Scripts/
│
├── Presentation/               # 【層 1: プレゼンテーション層】 ✅ CREATED
│   ├── UI/                     # UI コンポーネント ✅ RESTRUCTURED
│   │   ├── Controls/           # UI ボタン・ウィジェット制御
│   │   │   ├── ClickCtrl.cs
│   │   │   ├── ClosebtnCtrl.cs
│   │   │   ├── OkbtnCtrl.cs
│   │   │   ├── WindowCloseCtrl.cs
│   │   │   └── WindowDragCtrl.cs
│   │   ├── Dialogs/            # ダイアログ・ウィンドウ
│   │   │   ├── EventLogCtrl.cs
│   │   │   ├── GameTimerCtrl.cs
│   │   │   ├── InfoWindowCtrl.cs
│   │   │   └── MessageBoxCtrl.cs
│   │   ├── HUD/                # ゲーム中の常時表示情報
│   │   │   ├── CircularIndicator.cs
│   │   │   ├── MarkerIndicatorCtrl.cs
│   │   │   ├── MarkerPointerCtrl.cs
│   │   │   ├── MouseOverTipsCtrl.cs
│   │   │   ├── PathMakerCtrl.cs
│   │   │   ├── ScoreCtrl.cs
│   │   │   ├── TelopCtrl.cs
│   │   │   └── TooltipInfoCtrl.cs
│   │   └── Panels/             # メニュー・ゲーム情報パネル
│   │       ├── DebugInfoCtrl.cs
│   │       ├── EscMenuCtrl.cs
│   │       ├── NoticeCtrl.cs
│   │       ├── SpawnMarkerPointerCtrl.cs
│   │       └── TabMenuCtrl.cs
│   │
│   ├── View/                   # ビューロジック ✅ RESTRUCTURED
│   │   ├── Cameras/            # カメラ制御
│   │   │   ├── CameraController.cs
│   │   │   └── EnvironmentCameraController.cs
│   │   ├── Rendering/          # レンダリング・光処理
│   │   │   ├── BloomPathController.cs
│   │   │   └── EnvironmentLightController.cs
│   │   └── Effects/            # エフェクト制御
│   │       └── SignPowerOutageController.cs
│   │
│   └── Input/                  # 入力管理 ✅ CREATED
│       ├── InputController.cs
│       └── PlayerInputs.cs
│
├── Game/                       # 【層 2: ゲームロジック層】 ✅ CREATED
│   ├── GameManager/            # ゲーム進行管理 ✅ CREATED
│   │   ├── GameManager.cs
│   │   ├── DemController.cs
│   │   ├── StageGoalController.cs
│   │   └── NarakuController.cs
│   │
│   ├── Systems/                # ゲームシステム ✅ CREATED
│   │   ├── Stage/
│   │   │   ├── TitleStartController.cs
│   │   │   └── UnitFireDisaster.cs
│   │   ├── Spawn/
│   │   │   └── SpawnCtrl.cs             (⚠️ SpawnController にリネーム待ち)
│   │   ├── Weather/
│   │   │   ├── WeatherCtrl.cs           (⚠️ WeatherController にリネーム待ち)
│   │   │   ├── WindCtrl.cs              (⚠️ WeatherSystem にリネーム待ち)
│   │   │   └── PuddleCtrl.cs            (⚠️ PuddleController にリネーム待ち)
│   │   ├── Physics/
│   │   │   └── (empty - 実装予定)
│   │   └── Audio/
│   │       └── (empty - 実装予定)
│   │
│   ├── Units/                  # ユニット管理 ✅ CREATED
│   │   ├── Base/
│   │   │   ├── UnitStruct.cs
│   │   │   ├── CharacterStruct.cs
│   │   │   └── ItemStruct.cs
│   │   │
│   │   ├── Shared/             # 複数ユニットで共有するオブジェクト ✅ CREATED
│   │   │   ├── GarbageCube.cs
│   │   │   ├── GarbageCubeCtrl.cs
│   │   │   ├── GarbageCubeBig.cs
│   │   │   ├── GarbageCubeBox.cs
│   │   │   ├── FireCube.cs
│   │   │   ├── FireCubeCtrl.cs
│   │   │   ├── PowerCube.cs
│   │   │   └── PowerCubeCtrl.cs
│   │   │
│   │   ├── Towers/
│   │   │   ├── DustBox.cs
│   │   │   ├── DustBoxCtrl.cs
│   │   │   ├── SentryGuard.cs
│   │   │   ├── SentryGuardCtrl.cs
│   │   │   ├── Sweeper.cs
│   │   │   ├── SweepCtrl.cs
│   │   │   ├── WaterTurret.cs
│   │   │   ├── WaterTurretCtrl.cs
│   │   │   ├── TowerDustBoxCtrl.cs
│   │   │   ├── TowerMoveCtrl.cs
│   │   │   ├── TowerSentryGuardCtrl.cs
│   │   │   └── TowerSweeper.cs
│   │   │
│   │   ├── Enemies/
│   │   │   ├── Litter.cs
│   │   │   ├── EnemyLitter.cs
│   │   │   └── EnemyStatus.cs
│   │   │
│   │   ├── Items/
│   │   │   ├── Loupe.cs
│   │   │   ├── LoupeCtrl.cs
│   │   │   ├── ItemAction.cs
│   │   │   ├── ItemCreateCtrl.cs
│   │   │   ├── ItemHolderCtrl.cs
│   │   │   └── ItemListCtrl.cs
│   │   │
│   │   ├── Bullets/
│   │   │   ├── WaterSphereCtrl.cs
│   │   │   └── WaterSurfaceCtrl.cs
│   │   │
│   │   ├── Structures/
│   │   │   ├── SignboardCtrl.cs
│   │   │   ├── SimpleSwitchBox.cs
│   │   │   └── StopPlate.cs
│   │   │
│   │   └── Player/
│   │       └── (empty - 実装予定: プレイヤーユニット)
│   │
│   └── Events/                 # ゲームイベント ✅ CREATED
│       ├── Environmental/      # 環境災害イベント
│       │   ├── BuildingBreak.cs
│       │   ├── Burning.cs
│       │   ├── Earthquake.cs
│       │   ├── Flame.cs
│       │   ├── RainDrop.cs
│       │   ├── Raining.cs
│       │   ├── RainAbsorbCtrl.cs        (⚠️ RainAbsorbController にリネーム待ち)
│       │   ├── RainDropsCtrl.cs         (⚠️ RainDropsController にリネーム待ち)
│       │   └── (PathMakerCtrl.cs → Presentation/UI/HUD へ移動済み)
│       ├── Handlers/
│       │   └── (実装予定: イベントハンドラー)
│       └── System/
│           └── EventLoader.cs
│
├── Data/                       # 【層 3: データ層】 ✅ CREATED
│   ├── Models/                 # ゲームデータ定義 ✅ CREATED
│   │   ├── Structs/
│   │   │   └── (Struct ファイルは Units/Base に移動済み)
│   │   ├── Config/
│   │   │   ├── LangConst.cs             (⚠️ LanguageConstants にリネーム待ち)
│   │   │   └── ModelsEnum.cs
│   │   └── YAML/
│   │       └── (yaml 定義ファイル?)
│   │
│   ├── Repositories/           # データアクセス層 ✅ CREATED
│   │   ├── StageDataManager.cs          (⚠️ StageRepository にリネーム待ち)
│   │   ├── StagingYamlCtrl.cs           (⚠️ StagingYamlRepository にリネーム待ち)
│   │   └── LoadStreamingAsset.cs
│   │
│   └── Plateau/                # PLATEAU SDK 統合 ✅ CREATED
│       ├── Integration/
│       │   ├── PlateauBuildingInteractor.cs
│       │   ├── PlateauCubeMaker.cs
│       │   ├── PlateauDataExtractor.cs
│       │   └── PlateauObjectSelector.cs
│       ├── Data/
│       │   └── PlateauInfoManager.cs
│       └── Utilities/
│           ├── PlateauUtility.cs
│           └── PlateauUIManager.cs
│
└── Core/                       # 【オーソゴナル層: 共通機能】 ✅ CREATED
    ├── Managers/               # マネージャー群 ⚠️ リネーム待ち
    │   ├── InitializationManager.cs
    │   ├── GameSpeedCtrl.cs             (⚠️ GameSpeedManager にリネーム待ち)
    │   ├── LangCtrl.cs                  (⚠️ LanguageManager にリネーム待ち)
    │   ├── MaterialManager.cs
    │   ├── NavMeshCtrl.cs               (⚠️ NavMeshManager にリネーム待ち)
    │   ├── PrefabManager.cs
    │   ├── GameConfig.cs                (⚠️ ConfigManager にリネーム待ち)
    │   └── SceneLoaderUtility.cs        (⚠️ SceneManager にリネーム待ち)
    │
    ├── Interfaces/             # インターフェース定義（共用） ✅ CREATED
    │   └── GameComponentInterfaces.cs
    │
    ├── Utilities/              # ユーティリティ関数 ✅ CREATED
    │   ├── FileOperationUtility.cs      (⚠️ FileUtility にリネーム待ち)
    │   ├── GameObjectTreat.cs           (⚠️ GameObjectUtility にリネーム待ち)
    │   ├── LogUtility.cs
    │   ├── CommonsCalcs.cs              (⚠️ MathUtility にリネーム待ち)
    │   ├── XMLparser.cs                 (⚠️ XMLUtility にリネーム待ち)
    │   ├── DebugUtility.cs              ✅ NEW
    │   └── SpriteResourceLoader.cs
    │
    ├── Handlers/               # ハンドラー関数 ✅ CREATED
    │   └── ExceptionHandler.cs          ✅ NEW
    │
    ├── Constants/              # 定数定義 ✅ CREATED
    │   ├── GameConstants.cs             ⚠️ 未実装（GameConst.cs が必要）
    │   └── GameEnum.cs
    │
    ├── Helpers/                # ヘルパー関数 ✅ CREATED
    │   ├── UIHelper.cs
    │   ├── CoroutineRunner.cs           (⚠️ CoroutineHelper にリネーム待ち)
    │   └── (MarkerIndicatorHelper → Presentation/UI/HUD/MarkerIndicatorCtrl へ)
    │
    ├── Editor/                 # Editor 拡張 ✅ CREATED
    │   ├── Tools/
    │   │   ├── FontAssetPostProcessor.cs
    │   │   └── PostProcessBuild.cs
    │   ├── Windows/
    │   │   └── (empty - 実装予定)
    │   ├── Validators/
    │   │   └── (empty - 実装予定)
    │   └── Builders/
    │       └── (empty - 実装予定)
    │
    └── [旧フォルダ - Editor/] ✅ 統合済み
        (Editor/ → Core/Editor/Tools/ に統合)

【新規ファイル】
✅ Core/Utilities/DebugUtility.cs        (Debug統一管理)
✅ Core/Handlers/ExceptionHandler.cs     (例外処理統一)
✅ Presentation/View/Rendering/EnvironmentLightController.cs (実装済み確認)

【除外ファイル (.Editor/ フォルダ - テスト用・バックアップ)】
.Editor/
├── BasicRigidBodyPush.cs
├── bk_PlateauInfo.cs
├── CharacterStruct.cs (重複)
├── PlaguesserInputCtrl.cs
├── PlateauInfo.cs (重複)
├── TestBtnClick.cs
├── ThirdPersonController.cs
├── tmp_TowerSweeper.cs
├── UnitBtnInteractable.cs
├── UnitCollider.cs
├── UnitNavMesh.cs
├── UnitNPC.cs
├── UnitNPCact.cs
├── UnitOnTrigger.cs
├── UnitSpawn.cs
└── UnitVFXPrefab.cs
→ テスト用・バックアップ扱い（本実装に含めない）

【旧フォルダ - 移行完了】
✅ Utility/ → Core/Utilities/ + Core/Managers/ + Data/Repositories/
✅ Utilities/ → Core/Utilities/ (SpriteResourceLoader)
✅ Editor/ → Core/Editor/Tools/
✅ UI/ → Presentation/UI/ (+ Controls/, Dialogs/, HUD/, Panels/)
✅ View/ → Presentation/View/ (+ Cameras/, Rendering/, Effects/)
✅ APP/ → Game/GameManager/ + Presentation/Input/
✅ Tower/, Enemy/, Item/ → Game/Units/ (+ Towers/, Enemies/, Items/, Shared/)
✅ Bullet/ → Game/Units/Bullets/
✅ GameEvents/ → Game/Events/ (+ Environmental/, System/) + Presentation/UI/HUD/ (PathMaker)
✅ Stages/ → Game/Systems/Stage/
✅ StageOrnaments/ → Presentation/View/Effects/
✅ Models/ → Game/Units/ + Data/Models/
✅ Plateau/ → Data/Plateau/
✅ Player/ → Game/Units/Player/ (実装予定)
```
```

### 📝 ファイル移動マッピング詳細

**凡例**: `新フォルダ/新ファイル.cs  (← 旧フォルダ/旧ファイル.cs)`

---

## 📋 フォルダ構成変更マップ

### Phase 1: フォルダ再構成（大枠）

| 現フォルダ | 新フォルダ | 変更内容 |
|-----------|----------|--------|
| Utility + Utilities | Core/Utilities | 複数形に統一、ユーティリティ関数に絞る |
| APP | Game + Core/Managers | 責務分離（全体制御・マネージャー） |
| Models | Data/Models + Units | 層の分離（Struct と実装クラス） |
| Tower | Units/Towers | ユニットシステムの統合 |
| Enemy | Units/Enemies | ユニットシステムの統合 |
| Item | Units/Items | ユニットシステムの統合 |
| GameEvents | Events | リネーム（名前簡潔化） |
| StageOrnaments | Presentation/View/Effects | ビュー層に統合 |
| View/CameraCtrl | Presentation/View/Cameras | カメラ管理の整理 |
| Stages + Utility/StageDataManager | Data/Repositories | データアクセス層 |
| Utility/StagingYamlCtrl + LoadStreamingAsset | Data/Repositories | データアクセス層に統一 |
| Player + Input | Presentation/Input | 入力管理の統合 |
| Bullet | Game/Systems/Physics | 物理・発射物システム |

### Phase 2: マネージャー・Ctrl の統一

**新しい命名規則**:
- `*Manager.cs` - リソース・状態管理（InitializationManager, SceneManager 等）
- `*System.cs` - ゲームシステム（SpawnSystem, WeatherSystem 等）
- `*Controller.cs` - UI コンポーネント制御（PanelController, ButtonController 等）
- `*Utility.cs` - 静的ユーティリティ関数（LogUtility, FileUtility 等）

| 現在の名前 | 新しい名前 | 理由 |
|-----------|----------|------|
| GameCtrl | GameController | UI制御の一貫性 |
| GameSpeedCtrl | GameSpeedManager | リソース管理 |
| NavMeshCtrl | NavMeshSystem | ゲームシステム |
| WindCtrl | WeatherSystem | ゲームシステム |
| LangCtrl | LanguageManager | リソース管理 |
| MaterialManager | (そのまま) | 既に適切な命名 |

### Phase 3: 定数・Config の整理

**新しい構成**:
```
Core/Constants/
├── GameConstants.cs          # CONSTANT_NAME (public const)
├── LanguageConstants.cs      # 言語定数
├── LayerConstants.cs         # レイヤー定数
└── TagConstants.cs           # タグ定数

Core/Managers/
├── ConfigManager.cs          # ゲーム設定（GameConfig のリネーム）
└── LanguageManager.cs        # 言語設定（LangCtrl のリネーム）
```

**削除予定**:
- `GameConst.cs` → 内容を `GameConstants.cs` に統合
- `LangConst.cs` → 内容を `LanguageConstants.cs` に統合
- `GameConfig.cs` → 内容を `ConfigManager.cs` に統合

---

## 🔄 実装予定

### Migration Strategy: 段階的移行

#### 段階 1: フォルダ構造を作成（破壊的変更なし）

```powershell
# 新フォルダ構造を Assets/Scripts_New に作成
# 既存 Scripts はそのまま保持
# Assets/Scripts_New/ で新しい構成を実験
```

#### 段階 2: 新規作成ファイルから適用

```
Prototype Phase 中に新規作成するファイルは
新フォルダ構造（Core/, Systems/, Units/ 等）に従う

既存ファイルは段階的に移動：
Week 1: Core 層の基本ファイル移動
Week 2-3: Units 層の大規模移動
Week 4+: 残りのファイル移動
```

#### 段階 3: 既存 namespaces の更新

```csharp
// 例: SpriteResourceLoader
// 移動前: Assets/Scripts/Utilities/SpriteResourceLoader.cs
// 移動後: Assets/Scripts/Core/Utilities/SpriteResourceLoader.cs
// 変更: namespace は新しい構造に更新

// Prototype Phase では両方の namespace を並行サポート
#pragma warning disable CS0618  // Obsolete warning 抑止
// 既存コードは古い namespace で継続可能
```

---

## 📐 フォルダ構成の原則

### 責務分離の原則

| 層 | 責務 | 外部依存 |
|---|------|---------|
| **UI** | ユーザー表示・入力 | View, Game → Systems |
| **Game/Systems** | ゲームロジック | Units, Data/Repositories |
| **Data/Models** | データ定義のみ | (外部なし) |
| **Data/Repositories** | データ読み書き仲介 | Core/Utilities (IO) |
| **Core** | インフラ共通 | (何にも依存しない) |

### 命名規則の統一

```
【フォルダ】
- 複数形統一: Utilities, Systems, Units, Constants, Helpers
- 例外: Core (概念的親フォルダ)

【ファイル】
- UtilityClass: FileUtility.cs (static メソッド集)
- Manager: GameManager.cs (リソース・状態管理)
- System: SpawnSystem.cs (ゲームシステム)
- Controller: PanelController.cs (UI 制御)
- Handler: EventHandler.cs (イベント処理)
- Struct: GameData.cs (データ構造)

【Namespace】
OnoCoro.Core.Utilities
OnoCoro.Core.Managers
OnoCoro.Systems.Stage
OnoCoro.Units.Towers
OnoCoro.UI.Panels
```

---

## 📊 現フォルダのマッピング表

### UI 層（変更少ない）

| 現フォルダ | 新フォルダ | ファイル数 | 備考 |
|-----------|----------|---------|------|
| UI | UI | 20 | 細分化推奨: Panels/, Controls/, HUD/ |
| View | View | ? | カメラ制御を Cameras/ に分離 |

### Game 層（大規模リファクタリング）

| 現フォルダ | 新フォルダ | ファイル数 | 備考 |
|-----------|----------|---------|------|
| APP | Game + Core/Managers | 6 | 責務分離（GameController + 管理機能） |
| Tower | Units/Towers | 15 | 敵・アイテムと統合 |
| Enemy | Units/Enemies | ? | Tower と統合 |
| Item | Units/Items | ? | Tower と統合 |
| Stages | Systems/Stage | ? | ステージシステム化 |
| GameEvents | Events | 10 | イベントシステム再構成 |

### Data 層（新規）

| 現フォルダ | 新フォルダ | ファイル数 | 備考 |
|-----------|----------|---------|------|
| Models | Data/Models | 15 | Struct のみに集約 |
| Utility/StageDataManager + Utility/StagingYamlCtrl + Utility/LoadStreamingAsset | Data/Repositories | 3 | データアクセス層に統一 |
| Plateau | Data/Plateau | 6 | PLATEAU データハンドラー |

### Core 層（新規統合）

| 現フォルダ | 新フォルダ | ファイル数 | 備考 |
|-----------|----------|---------|------|
| Utility | Core/Utilities + Core/Managers + Core/Constants | 25 | 責務別に分散 |
| Utilities | Core/Utilities | 1 | (SpriteResourceLoader) |
| .Editor + Editor | Editor/ | ? | 統合 |

---

## 🎯 メリット・デメリット

### ✅ メリット

| メリット | 効果 |
|---------|------|
| **責務明確化** | 新機能追加時の配置場所が一目瞭然 |
| **保守性向上** | 関連クラスが同じフォルダに集約 |
| **スケーラビリティ** | 機能拡張時のフォルダ追加が容易 |
| **命名統一** | C# 標準に従う（学習コスト低下） |
| **IDE ナビゲーション** | プロジェクトツリーが理解しやすい |
| **新規開発者対応** | オンボーディングが容易 |

### ⚠️ デメリット & 対策

| デメリット | 対策 |
|-----------|------|
| **大規模リファクタリング** | 段階的移行（各フェーズで少量ずつ） |
| **参照パスの更新** | 自動置換 + grep で検証 |
| **Namespace 競合** | 一定期間は古い namespace も並行サポート |
| **テスト費用** | Unit テスト充実化で対応 |

---

## 🚀 実装スケジュール

### Prototype Phase (2026年2月末)

```
Week 1-2: Core フォルダ構造作成
  □ Assets/Scripts/Core/ を作成
  □ Core/Utilities/, Core/Managers/, Core/Constants/ を作成
  □ 既存 Utility/*.cs を Core 層に移動開始

Week 2-3: Units フォルダ統合
  □ Assets/Scripts/Units/ を作成
  □ Tower/, Enemy/, Item/ を Units/ に統合
  □ Models/ → Data/Models/ に移動

Week 3-4: Game 層・Systems 層整備
  □ Game/, Systems/ フォルダを作成
  □ APP/ → Game/ に移動
  □ GameEvents/ → Events/ にリネーム

Week 4+: 段階的な詳細フォルダ作成
  □ Units/Towers/, Units/Enemies/, Units/Items/ 細分化
  □ UI/Panels/, UI/Controls/, UI/HUD/ 細分化
  □ View/Cameras/, View/Effects/ 細分化
```

### Alpha Phase (2026年3月以降)

```
□ 残存する旧フォルダの完全クリーンアップ
□ すべてのファイルを新フォルダ構造に移行完了
□ すべての namespace を新規則に統一
```

---

## 📝 マイグレーション チェックリスト

### 各ファイル移動時の確認項目

- [ ] **Namespace 確認**: 新フォルダ構造に合わせて更新
- [ ] **参照更新**: `using` 文を新 namespace に変更
- [ ] **asset 参照**: Prefab/Scene での component 参照を確認
- [ ] **Scripts フォルダアイコン**: meta ファイルも移動
- [ ] **Git 管理**: `git mv` で移動（履歴保持）
- [ ] **コンパイル確認**: エラーなし
- [ ] **テスト実行**: Unit テスト + Play テスト実行

---

## 📌 推奨される最初の一手

1. **Core/ フォルダ作成** (破壊的変更なし)
   ```
   Assets/Scripts/Core/
   ├── Managers/
   ├── Utilities/
   └── Constants/
   ```

2. **既存の Utility/*.cs を Core/Managers/,  Core/Utilities/ に分類**
   ```
   Utility/GameCtrl.cs → Core/Managers/GameController.cs
   Utility/LogUtility.cs → Core/Utilities/LogUtility.cs
   Utility/GameConst.cs → Core/Constants/GameConstants.cs
   ```

3. **Utilities/SpriteResourceLoader.cs を Core/Utilities/ に統一**
   ```
   Utilities/ フォルダは削除可能
   ```

4. **新規ファイル作成時から新規則を適用**
   ```
   新しいマネージャーは Core/Managers/ に作成
   新しいシステムは Systems/ に作成
   ```

---

## 結論

現在のフォルダ構成は「機能別」に分類されていますが、責務の曖昧さと命名の混在により、スケーラビリティに課題があります。

**提案する理想形** は「層 + 機能」の2軸分類で、以下の効果を期待：

- ✅ 新規ファイルの配置場所が明確
- ✅ 既存ファイルの役割が一目瞭然
- ✅ C# 標準命名規則に準拠
- ✅ チーム開発で混乱が減少

**段階的な導入** により、既存機能への影響を最小化しながら改善できます。

---

**参考資料**:
- [docs/coding-standards.md](coding-standards.md) - C# コーディング規約
- [docs/architecture.md](architecture.md) - システムアーキテクチャ
- [TODO.md](../TODO.md) - Utility 構成統一タスク（ベータ版）

**次のステップ**: Core/ フォルダ作成と既存 Utility/*.cs の分類作業（Prototype Phase Week 1-2）
