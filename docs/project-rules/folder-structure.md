# フォルダ構成・レイヤー構造

**目的**: 一貫性のあるファイル配置・責務分離・依存関係の明確化

---

## 4 層アーキテクチャ

OnoCoro は以下の 4 層で責務を明確に分離：

| 層 | 責務 | 外部依存性 | 配置フォルダ |
|----|------|---------|-----------|
| **Presentation** | UI・入力・表示 | Game, View | Presentation/ |
| **Game** | ゲームロジック | Data, Units | Game/ |
| **Data** | データ定義・アクセス | Core/Utilities | Data/ |
| **Core** | インフラ共通機能 | (何にも依存しない) | Core/ |

### 層の依存関係ルール

[WARN] **層は上から下のみ依存可能。逆方向の依存は禁止**

```
Presentation ──┐
               │
Game ──────────┼──→ Data
               │
               └──→ Core (基盤層 - 誰にも依存しない)

[OK] Presentation → Game → Data → Core
[NG] Core → Data  (逆方向依存)
[NG] Game → Presentation  (上層依存)
```

---

## Presentation 層（プレゼンテーション）

**責務**: ユーザーインターフェース・入力受付・表示制御

### フォルダ構成

```
Presentation/
├── UI/
│   ├── Controls/              (UI コンポーネント制御)
│   │   ├── ButtonController.cs
│   │   └── SliderController.cs
│   ├── Dialogs/               (ダイアログ・ウィンドウ)
│   │   ├── MessageBoxController.cs
│   │   └── ConfirmDialogController.cs
│   ├── HUD/                   (常時表示 UI)
│   │   ├── GameHUDController.cs
│   │   └── HealthIndicatorController.cs
│   ├── Panels/                (メニュー・パネル)
│   │   ├── PauseMenuController.cs
│   │   └── GameInfoPanelController.cs
│   └── UICanvasManager.cs     (Canvas 一元管理)
├── View/
│   ├── Cameras/               (カメラ制御)
│   │   ├── CameraController.cs
│   │   └── CineMachineBrain.cs (Cinemachine)
│   ├── Rendering/             (レンダリング)
│   │   └── PostProcessController.cs
│   └── Effects/               (エフェクト)
│       └── ParticleEffectController.cs
└── Input/
    ├── InputController.cs
    └── PlayerInputs.cs        (Input System)
```

### ファイル配置ルール

[OK] **UI コンポーネント → Controls/ に配置**

```csharp
// [OK] UI ボタン制御
Presentation/UI/Controls/ButtonController.cs

// [OK] UI スライダー制御
Presentation/UI/Controls/SliderController.cs
```

[OK] **ダイアログ・モーダル → Dialogs/ に配置**

```csharp
// [OK] メッセージボックス
Presentation/UI/Dialogs/MessageBoxController.cs

// [OK] 確認ダイアログ
Presentation/UI/Dialogs/ConfirmDialogController.cs
```

[OK] **HUD (Heads Up Display) → HUD/ に配置**

```csharp
// [OK] ゲーム中の常時表示 UI
Presentation/UI/HUD/GameHUDController.cs

// [OK] 体力表示
Presentation/UI/HUD/HealthIndicatorController.cs
```

---

## Game 層（ゲームロジック）

**責務**: ゲーム進行・ゲームシステム・ゲームエンティティ

### フォルダ構成

```
Game/
├── GameManager/
│   ├── GameManager.cs         (ゲーム進行管理)
│   ├── DemoController.cs
│   └── StageGoalController.cs
├── Systems/
│   ├── Stage/
│   │   ├── StageInitializer.cs
│   │   └── StageController.cs
│   ├── Spawn/
│   │   ├── SpawnSystem.cs
│   │   └── SpawnPoint.cs
│   ├── Weather/
│   │   └── WeatherSystem.cs
│   └── Audio/
│       └── AudioSystem.cs
├── Units/
│   ├── Towers/
│   │   ├── Tower.cs           (基底クラス)
│   │   ├── SentryGuard.cs     (実装)
│   │   └── TowerFactory.cs    (生成)
│   ├── Enemies/
│   │   ├── Enemy.cs           (基底)
│   │   ├── Litter.cs          (実装)
│   │   └── EnemyFactory.cs    (生成)
│   ├── Items/
│   │   ├── Item.cs
│   │   └── ItemFactory.cs
│   └── Projectiles/
│       ├── Projectile.cs
│       └── ProjectileFactory.cs
└── Events/
    ├── GameOverHandler.cs
    ├── WaveCompleteHandler.cs
    └── TowerPlacementHandler.cs
```

### ファイル配置ルール

[OK] **ゲーム進行 → GameManager/ に配置**

```csharp
// [OK] ゲーム全体管理
Game/GameManager/GameManager.cs

// [OK] ステージ管理
Game/GameManager/StageGoalController.cs
```

[OK] **ゲームシステム → Systems/`<Domain>`/ に配置**

```csharp
// [OK] スポーンシステム
Game/Systems/Spawn/SpawnSystem.cs

// [OK] 天候システム
Game/Systems/Weather/WeatherSystem.cs

// [OK] オーディオシステム
Game/Systems/Audio/AudioSystem.cs
```

[OK] **ゲームエンティティ → Units/`<Type>`/ に配置**

```csharp
// [OK] タワー（基底・実装・生成）
Game/Units/Towers/Tower.cs
Game/Units/Towers/SentryGuard.cs
Game/Units/Towers/TowerFactory.cs

// [OK] 敵
Game/Units/Enemies/Enemy.cs
Game/Units/Enemies/Litter.cs
```

[OK] **イベント処理 → Events/ に配置**

```csharp
// [OK] ゲームオーバーイベント
Game/Events/GameOverHandler.cs

// [OK] ウェーブ完了イベント
Game/Events/WaveCompleteHandler.cs
```

---

## Data 層（データ定義）

**責務**: データモデル定義・データ永続化・外部データ読み込み

### フォルダ構成

```
Data/
├── Models/
│   ├── GameData.cs            (ゲームデータ構造体)
│   ├── TowerData.cs
│   ├── EnemyData.cs
│   ├── StageData.cs
│   └── ItemData.cs
├── Repositories/
│   ├── StageRepository.cs     (YAML ロード)
│   ├── GameSaveRepository.cs  (ファイル保存)
│   └── ConfigRepository.cs    (設定ロード)
└── PLATEAU/
    ├── PlateauLoader.cs       (PLATEAU SDK)
    ├── CityDataCache.cs
    └── CoordinateTransform.cs (座標変換)
```

### ファイル配置ルール

[OK] **データモデル → Models/ に配置**

```csharp
// [OK] ゲームデータ構造体
Data/Models/GameData.cs

// [OK] タワーデータ
Data/Models/TowerData.cs
```

[OK] **データリポジトリ → Repositories/ に配置**

```csharp
// [OK] ステージデータリポジトリ
Data/Repositories/StageRepository.cs

// [OK] ゲーム保存リポジトリ
Data/Repositories/GameSaveRepository.cs
```

[OK] **PLATEAU 連携 → PLATEAU/ に配置**

```csharp
// [OK] PLATEAU ローダー
Data/PLATEAU/PlateauLoader.cs

// [OK] 座標変換
Data/PLATEAU/CoordinateTransform.cs
```

---

## Core 層（基盤インフラ）

**責務**: 共通ユーティリティ・マネージャー・定数・ハンドラー

### フォルダ構成

```
Core/
├── Managers/
│   ├── GameSpeedManager.cs    (ゲーム速度管理)
│   ├── ConfigManager.cs       (設定管理)
│   ├── LanguageManager.cs     (多言語管理)
│   ├── PrefabManager.cs       (プレファブ管理)
│   └── InitializationManager.cs (初期化管理)
├── Utilities/
│   ├── FileUtility.cs         (ファイル操作)
│   ├── LogUtility.cs          (ログ出力)
│   ├── MathUtility.cs         (数学計算)
│   └── GameObjectUtility.cs   (GameObject 操作)
├── Handlers/
│   ├── ExceptionHandler.cs    (例外処理)
│   ├── SceneLoaderManager.cs  (シーン遷移)
│   └── EventAggregator.cs     (イベント配信)
├── Constants/
│   ├── GameConst.cs           (ゲーム定数)
│   ├── UIConst.cs             (UI 定数)
│   └── GlobalConst.cs         (グローバル定数)
└── Helpers/
    ├── CoroutineHelper.cs
    └── AsyncHelper.cs
```

### ファイル配置ルール

[OK] **リソース・状態管理 → Managers/ に配置**

```csharp
// [OK] ゲーム速度管理
Core/Managers/GameSpeedManager.cs

// [OK] プレファブ管理
Core/Managers/PrefabManager.cs

// [OK] 初期化管理
Core/Managers/InitializationManager.cs
```

[OK] **ユーティリティ関数 → Utilities/ に配置**

```csharp
// [OK] ファイル操作ユーティリティ
Core/Utilities/FileUtility.cs

// [OK] ログ出力ユーティリティ
Core/Utilities/LogUtility.cs

// [OK] GameObject 操作ユーティリティ
Core/Utilities/GameObjectUtility.cs
```

[OK] **定数 → Constants/ に配置**

```csharp
// [OK] ゲーム定数グループ
Core/Constants/GameConst.cs

// [OK] UI 定数グループ
Core/Constants/UIConst.cs
```

[OK] **イベント・ハンドラー → Handlers/ に配置**

```csharp
// [OK] シーン遷移ハンドラー
Core/Handlers/SceneLoaderManager.cs

// [OK] 例外処理
Core/Handlers/ExceptionHandler.cs
```

---

## テストスクリプト（UnitTest/）

**用途**: 機能テスト・デバッグ用スクリプト（一時的）

### ファイル配置ルール

[OK] **テストスクリプト → UnitTest/ に配置（作成・実行中）**

```csharp
// [OK] 開発中のテスト
Assets/Scripts/UnitTest/LogUtilityTest.cs
Assets/Scripts/UnitTest/DebugClassTest.cs
Assets/Scripts/UnitTest/PrefabManagerTest.cs
```

[OK] **使用済みテスト → Core/Editor/ に移動（アーカイブ）**

```csharp
// [OK] 参考資料として保管
Assets/Scripts/Core/Editor/UICanvasManagerTest.cs (archived)
```

[NOTE] **UnitTest/ フォルダはビルドから除外可能** (.asmdef または .meta 設定)

---

## ファイル配置チェックリスト

新規ファイル作成時に確認：

- [ ] **責務の確認**: Manager / System / Controller / Utility のいずれか
- [ ] **適切な層**: Presentation / Game / Data / Core のいずれか
- [ ] **フォルダ選択**: 責務に合った subfolder を選択
- [ ] **命名規則**: Class Suffix （Manager / System / Controller など） を確認
- [ ] **Namespace**: 全ファイル `CommonsUtility` を使用
- [ ] **依存性**: 層の下向き依存のみ確認
- [ ] **ドキュメント**: フォルダ構成ドキュメント更新検討

---

## 層の依存関係ルール（詳細）

### 許可される依存

[OK] **以下の依存は許可**:

```csharp
// [OK] Presentation が Game を使用
public class GameHUDController : MonoBehaviour
{
    private GameManager _gameManager;  // Game 層
}

// [OK] Game が Data を使用
public class StageController : MonoBehaviour
{
    private StageRepository _repository;  // Data 層
}

// [OK] 全層が Core を使用
public class AnyClass
{
    private FileUtility _utility;        // Core 層 - OK
    private ConfigManager _config;       // Core 層 - OK
}
```

### 禁止される依存

[WARN] **以下の依存は禁止**:

```csharp
// [NG] Core が上層に依存
public static class FileUtility
{
    public GameManager GetGameManager() { }  // ❌ Core が Game を参照
}

// [NG] Data が Presentation に依存
public class StageRepository
{
    public void UpdateUI(GameObject panel) { }  // ❌ Data が UI を操作
}

// [NG] 同層でも無限ループ
// Example: A → B → A の循環依存
public class TowerFactory
{
    public Tower CreateTower(EnemyFactory factory)  // ❌ 循環依存リスク
    {
    }
}
```

---

## フォルダ構成更新時の手順

新しいフォルダカテゴリを追加する場合：

1. [STEP] 該当層を確認（Presentation / Game / Data / Core）
2. [STEP] 既存 subfolder に該当するか確認
3. [STEP] 必要な場合のみ新規 subfolder 作成
4. [STEP] このドキュメント（folder-structure.md）を更新
5. [STEP] 担当者に通知（重複の可能性を回避）

---

**関連資料**:
- [naming-conventions.md](naming-conventions.md) - Class Suffix 定義
- [unity-design-patterns.md](unity-design-patterns.md) - パターン実装
- [coding-csharp.md](coding-csharp.md) - コーディング規約
- [AGENTS.md](../AGENTS.md) - プロジェクト全体ルール
