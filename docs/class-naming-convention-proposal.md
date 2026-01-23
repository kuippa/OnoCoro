# C# クラス命名規則 統一提案書

**作成日**: 2026-01-23  
**対象**: OnoCoro v0.1.0-alpha (Prototype Phase)  
**目的**: クラス命名の一貫性確保による保守性向上

---

## 📊 現状分析

### 🔴 発見された命名の揺れ

#### 1. Ctrl 系命名の混在

```
【*Ctrl で統一されているもの】
- GameCtrl             # ゲーム全体制御
- GameSpeedCtrl        # ゲーム速度制御
- LangCtrl             # 言語管理制御
- WindCtrl             # 風シミュレーション制御
- BloomPathCtrl        # ブルームパス表示制御
- MarkerIndicatorCtrl  # マーカー表示制御
- NavMeshCtrl          # NavMesh 制御

【*Manager で統一されているもの】
- InitializationManager   # 初期化マネージャー
- MaterialManager         # マテリアルマネージャー
- PrefabManager          # プレファブマネージャー

【*System で統一されているもの】
- (未使用 - 拡張予定)

【suffix なし（曖昧）】
- CoroutineRunner        # コルーチン実行機能
- GameObjectTreat        # GameObject 操作ユーティリティ
- CommonsCalcs           # 共通計算関数
- XMLparser              # XML パーサー
```

#### 2. 責務の曖昧さ

| クラス | 実際の役割 | 推奨される名前 |
|--------|----------|-----------------|
| GameCtrl | UI ボタンやゲーム進行の制御 | GameController か GameManager |
| GameSpeedCtrl | ゲーム速度の状態管理 | GameSpeedManager |
| NavMeshCtrl | NavMesh 再ベーク処理 | NavMeshSystem |
| WindCtrl | 風のシミュレーション | WeatherSystem |
| MaterialManager | マテリアルリソース管理 | (既に適切) |

#### 3. 命名規則表の整理不足

現在のドキュメント（scripts-folder-restructure-proposal.md）では部分的な提案のみ:
- Manager / System / Controller / Utility の4種類のみ
- Handler, Service, Provider, Factory などの複合的なパターンに対応していない
- MonoBehaviour と static class の命名区別がない

---

## 🎯 提案する統一的な命名規則

### 基本原則

```
【suffix の役割】
MonoBehaviour/Manager/System/Service (Runtime時に存在) → スネークケース + suffix
Utility/Helper/Factory (static メソッド集) → PascalCase のみ
```

### 1. Manager（リソース・状態管理）

**用途**: Singleton や static manager でリソース・状態を一元管理

**特徴**:
- リソース管理（Asset 読み込み、キャッシュ管理）
- 状態管理（ゲーム設定、言語設定）
- グローバル状態の保持

**命名パターン**: `<Domain>Manager.cs`

**例**:
```csharp
// OK
InitializationManager       # 初期化順序管理
SceneManager               # シーン遷移管理（Unity 標準との区別に注意）
ConfigManager              # ゲーム設定管理
LanguageManager            # 言語リソース管理
PrefabManager              # プレファブ読み込み・キャッシュ
AssetLoader                # ～Utility の方がふさわしい場合も

// NG
GameCtrl                   # → GameManager が推奨
GameSpeedCtrl              # → GameSpeedManager が推奨
```

**配置場所**:
```
Core/Managers/
├── InitializationManager.cs
├── SceneManager.cs
├── ConfigManager.cs
├── LanguageManager.cs
├── PrefabManager.cs
└── ...
```

**実装例**:
```csharp
public static class InitializationManager
{
    private static bool _isInitialized = false;
    
    public static void Initialize()
    {
        if (_isInitialized) return;
        // リソース初期化
        _isInitialized = true;
    }
}

// または MonoBehaviour の場合
public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

---

### 2. System（ゲームシステム）

**用途**: ゲーム進行に必要な各種システム（複合的な処理）

**特徴**:
- ゲームロジックの実装
- 複数エンティティの相互作用を管理
- イベント駆動的な設計

**命名パターン**: `<Domain>System.cs`

**例**:
```csharp
// OK
SpawnSystem                # 敵スポーン管理システム
WeatherSystem              # 天候・環境イベントシステム
PhysicsSystem              # 物理・衝突判定システム
StageSystem                # ステージ進行システム
AudioSystem                # 音声再生システム
NavMeshSystem              # NavMesh 管理・再ベークシステム

// NG
WindCtrl                   # → WeatherSystem が推奨
NavMeshCtrl                # → NavMeshSystem が推奨
```

**配置場所**:
```
Game/Systems/
├── Stage/
│   └── StageSystem.cs
├── Spawn/
│   └── SpawnSystem.cs
├── Weather/
│   └── WeatherSystem.cs
├── Physics/
│   └── PhysicsSystem.cs
└── Audio/
    └── AudioSystem.cs
```

**実装例**:
```csharp
public class WeatherSystem : MonoBehaviour
{
    public void ApplyWind(Vector3 windDirection)
    {
        // 風の影響を計算・適用
    }
    
    public void StartRain()
    {
        // 雨イベント開始
    }
}
```

---

### 3. Controller（UI・入力制御）

**用途**: UI コンポーネント・ユーザー入力の制御

**特徴**:
- MonoBehaviour として UI や GameObject に attach
- UI イベント（ボタンクリック）や入力の処理
- 表示・非表示の切り替え

**命名パターン**: `<Component>Controller.cs` または `<Panel>PanelController.cs`

**例**:
```csharp
// OK
InputController            # 入力受付制御
PanelController            # UI パネル制御
ButtonController           # ボタン動作制御
GameTimerController        # ゲームタイマー UI 制御
MessageBoxController       # メッセージボックス表示制御

// NG
GameCtrl                   # → GameController が推奨（UI 制御なら）
LangCtrl                   # → LanguageManager が推奨（言語管理なら）
```

**配置場所**:
```
Presentation/UI/
├── Panels/
│   └── GamePanelController.cs
├── Controls/
│   └── ButtonController.cs
└── HUD/
    └── GameTimerController.cs

Presentation/Input/
└── InputController.cs
```

**実装例**:
```csharp
public class GamePanelController : MonoBehaviour
{
    private Button _startButton;
    
    void Start()
    {
        _startButton = this.gameObject.GetComponent<Button>();
        _startButton.onClick.AddListener(OnStartButtonClicked);
    }
    
    private void OnStartButtonClicked()
    {
        // ゲーム開始処理
    }
}
```

---

### 4. Service（特定機能の提供）

**用途**: 特定の機能を提供するサービスクラス（複合的で管理的）

**特徴**:
- 複数クラスから利用されるサービス
- 責務が限定されている
- static メソッドと instance メソッドの混在可

**命名パターン**: `<Function>Service.cs`

**例**:
```csharp
// OK
SaveGameService            # セーブゲーム機能を提供
LoadGameService            # ロードゲーム機能を提供
AnalyticsService           # アナリティクス送信機能を提供
LocalizationService        # 多言語化機能を提供

// NG
LangCtrl                   # → LocalizationService か LanguageManager
```

**配置場所**:
```
Core/Services/             # 新規フォルダ作成推奨
├── SaveGameService.cs
├── LoadGameService.cs
├── AnalyticsService.cs
└── LocalizationService.cs
```

---

### 5. Handler（イベント処理）

**用途**: イベント駆動的な処理を担当

**特徴**:
- 特定のイベントに応答
- 副次的な処理
- Event callback として使用される

**命名パターン**: `<Event>Handler.cs`

**例**:
```csharp
// OK
CollisionHandler           # 衝突イベント処理
TowerPlacementHandler      # タワー配置イベント処理
GameOverHandler            # ゲームオーバーイベント処理

// NG
BuildingBreak (suffix なし) # → BuildingBreakHandler が推奨
```

**配置場所**:
```
Game/Events/
├── Environmental/
│   ├── CollisionHandler.cs
│   └── BuildingBreakHandler.cs
└── Handlers/
    ├── TowerPlacementHandler.cs
    └── GameOverHandler.cs
```

---

### 6. Utility（静的ユーティリティ）

**用途**: 静的メソッド集（singleton ではない）

**特徴**:
- static class（MonoBehaviour ではない）
- 関数型プログラミング的
- 依存性が最小

**命名パターン**: `<Function>Utility.cs` または `<Function>Helper.cs`

**例**:
```csharp
// OK
FileUtility                # ファイル操作ユーティリティ
LogUtility                 # ログ出力ユーティリティ
MathUtility                # 数学演算ユーティリティ
GameObjectUtility          # GameObject 操作ユーティリティ
ColliderUtility            # Collider ユーティリティ

// NG
GameObjectTreat            # → GameObjectUtility が推奨
CommonsCalcs               # → MathUtility が推奨
XMLparser                  # → XMLUtility が推奨
```

**配置場所**:
```
Core/Utilities/
├── FileUtility.cs
├── LogUtility.cs
├── MathUtility.cs
├── GameObjectUtility.cs
└── PrefabManager.cs        # マネージャーの混在もここ
```

**実装例**:
```csharp
public static class GameObjectUtility
{
    public static void SetActive(GameObject obj, bool active)
    {
        if (obj == null) return;
        obj.SetActive(active);
    }
    
    public static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        return component ?? obj.AddComponent<T>();
    }
}
```

---

### 7. Factory（生成工場）

**用途**: オブジェクト生成を一元管理

**特徴**:
- 複雑な生成ロジック
- 複数の生成パターン
- Pooling との組み合わせ

**命名パターン**: `<Type>Factory.cs`

**例**:
```csharp
// OK
TowerFactory               # タワーインスタンス生成工場
EnemyFactory               # 敵インスタンス生成工場
ProjectileFactory          # 発射物インスタンス生成工場
```

**配置場所**:
```
Game/Units/Factories/      # 新規フォルダ作成推奨
├── TowerFactory.cs
├── EnemyFactory.cs
└── ProjectileFactory.cs
```

---

### 8. Provider（データ提供）

**用途**: データ取得・キャッシュ管理（取得に特化）

**特徴**:
- キャッシュ機構あり
- データベースアクセス抽象化
- 遅延読み込み

**命名パターン**: `<Data>Provider.cs`

**例**:
```csharp
// OK
StageDataProvider          # ステージデータ提供者
ConfigProvider             # 設定データ提供者
LocalizationProvider       # 多言語テキスト提供者
```

**配置場所**:
```
Data/Providers/            # 新規フォルダ作成推奨
├── StageDataProvider.cs
├── ConfigProvider.cs
└── LocalizationProvider.cs
```

---

### 9. Struct / Data（データ構造）

**用途**: データの定義のみ（ロジックなし）

**特徴**:
- readonly struct 推奨
- ロジックを持たない
- Serializable

**命名パターン**: `<Entity>Data.cs` または `<Entity>Struct.cs`

**例**:
```csharp
// OK
TowerData                  # タワー属性データ
EnemyData                  # 敵属性データ
StageData                  # ステージ属性データ
ItemData                   # アイテム属性データ

// NG
CharacterStruct            # → CharacterData が推奨
ItemStruct                 # → ItemData が推奨
```

**配置場所**:
```
Data/Models/
├── Structs/
│   ├── TowerData.cs
│   ├── EnemyData.cs
│   └── StageData.cs
└── Enums/
    └── GameEnum.cs
```

---

### 10. MonoBehaviour 直接継承（Game Entity）

**用途**: ゲームエンティティの実装

**特徴**:
- Scene に配置されるオブジェクト
- 複雑な状態遷移あり
- イベント駆動

**命名パターン**: `<Entity>.cs` または `<Entity>Controller.cs`

**例**:
```csharp
// OK
Tower                      # タワーエンティティ（基底）
SentryGuard                # 監視塔タワー（実装）
FireTower                  # 火炎タワー（実装）
Player                     # プレイヤーエンティティ
Enemy                      # 敵エンティティ（基底）
Litter                     # ゴミ敵（実装）

// NG
TowerCtrl                  # → Tower または TowerController が推奨
```

**配置場所**:
```
Game/Units/
├── Base/
│   ├── Tower.cs          # 基底クラス
│   └── Enemy.cs
├── Towers/
│   ├── SentryGuard.cs
│   ├── FireTower.cs
│   └── WaterTurret.cs
└── Enemies/
    ├── Litter.cs
    └── Debris.cs
```

---

## 📋 命名規則マッピング表

### 既存クラス → 推奨名への変更案

| 現在の名前 | 推奨される名前 | 理由 | 配置場所 |
|-----------|-----------------|------|---------|
| GameCtrl | GameController | UI/ゲーム進行制御 | Presentation/ または Game/GameManager/ |
| GameSpeedCtrl | GameSpeedManager | ゲーム速度の状態管理 | Core/Managers/ |
| NavMeshCtrl | NavMeshSystem | NavMesh システム管理 | Game/Systems/ |
| WindCtrl | WeatherSystem | 天候・環境システム | Game/Systems/Weather/ |
| LangCtrl | LanguageManager | 言語リソース管理 | Core/Managers/ |
| BloomPathCtrl | BloomPathController | UI 制御 | Presentation/View/Rendering/ |
| MarkerIndicatorCtrl | MarkerIndicatorController | マーカー表示 UI | Presentation/UI/HUD/ |
| CoroutineRunner | CoroutineManager | コルーチン管理 | Core/Managers/ |
| GameObjectTreat | GameObjectUtility | GameObject ユーティリティ | Core/Utilities/ |
| CommonsCalcs | MathUtility | 数学計算ユーティリティ | Core/Utilities/ |
| XMLparser | XMLUtility | XML パース ユーティリティ | Data/Utilities/ |
| MaterialManager | (そのまま) | 既に適切 | Core/Managers/ |
| InitializationManager | (そのまま) | 既に適切 | Core/Managers/ |
| PrefabManager | (そのまま) | 既に適切 | Core/Managers/ |

---

## 🔄 実装パターン別の命名・配置ガイド

### パターン 1: グローバル設定管理

```csharp
// ✅ 推奨パターン
public static class ConfigManager
{
    public static int GameDifficulty { get; set; }
    public static float MasterVolume { get; set; }
}

// または Singleton MonoBehaviour
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
}

// 配置場所: Core/Managers/ConfigManager.cs
```

### パターン 2: ゲームシステム（複合的なゲーム機能）

```csharp
// ✅ 推奨パターン
public class WeatherSystem : MonoBehaviour
{
    public void ApplyWind(Vector3 direction) { }
    public void StartRain() { }
}

public class SpawnSystem : MonoBehaviour
{
    public void SpawnEnemy(Vector3 position) { }
}

// 配置場所: Game/Systems/<Domain>/
```

### パターン 3: UI コンポーネント制御

```csharp
// ✅ 推奨パターン
public class PauseMenuController : MonoBehaviour
{
    public void OnResumeButtonClicked() { }
    public void OnQuitButtonClicked() { }
}

// 配置場所: Presentation/UI/Panels/PauseMenuController.cs
```

### パターン 4: 入力処理

```csharp
// ✅ 推奨パターン
public class InputController : MonoBehaviour
{
    public void OnMoveInput(Vector2 direction) { }
    public void OnActionInput() { }
}

// 配置場所: Presentation/Input/InputController.cs
```

### パターン 5: ユーティリティ関数集

```csharp
// ✅ 推奨パターン
public static class GameObjectUtility
{
    public static T GetOrAddComponent<T>(GameObject obj) 
        where T : Component { }
    
    public static void SafeDestroy(Object obj) { }
}

// 配置場所: Core/Utilities/GameObjectUtility.cs
```

---

## 📐 決定フロー図

```
クラス作成時の命名判定フロー

1. 何の役割か？
   ├─ リソース・状態管理？ → Manager
   ├─ ゲームシステム実装？ → System
   ├─ イベント処理？ → Handler
   ├─ UI・入力制御？ → Controller
   ├─ 特定機能提供？ → Service
   ├─ オブジェクト生成？ → Factory
   ├─ データ提供（キャッシュ）？ → Provider
   ├─ 静的ユーティリティ？ → Utility
   └─ ゲームエンティティ？ → (suffix なし)

2. 実装方式？
   ├─ static class？ → Utility / Helper
   ├─ Singleton？ → Manager
   └─ MonoBehaviour？ → Manager / System / Controller / (Entity)

3. 配置場所を決定
   ├─ Core/ → Managers, Utilities, Constants, Helpers
   ├─ Presentation/ → UI, Input, View
   ├─ Game/ → Systems, Units, Events, GameManager
   └─ Data/ → Models, Plateau, Providers
```

---

## 🎯 メリット・デメリット

### ✅ メリット

| メリット | 効果 |
|---------|------|
| **責務の明確化** | クラス名から役割が一目瞭然 |
| **学習コスト削減** | チーム全体で同じ命名ルール |
| **IDE 検索性向上** | `*Manager` で管理クラスをまとめて検索可能 |
| **スケーラビリティ** | 新規クラス追加時の判断が容易 |
| **業界標準準拠** | Unity/C# コミュニティの一般的慣例に準拠 |

### ⚠️ デメリット & 対策

| デメリット | 対策 |
|-----------|------|
| **既存クラスの リネーム** | 段階的な移行（Prototype Phase で新規ルール適用） |
| **参照パスの更新** | クラス移動と同時に namespace 更新 |
| **学習期間** | チーム内で命名ガイドを共有・確認 |

---

## 🚀 実装スケジュール

### Prototype Phase (2026年2月末)

```
【優先度 1: 新規ファイルから適用】
Week 1-2:
  □ 本提案書を AGENTS.md に追加
  □ 新規作成ファイルはすべて新命名ルールに従う
  □ チーム内で命名ルールを周知

【優先度 2: 大規模リネーム】
Week 2-3:
  □ GameCtrl → GameController へリネーム
  □ GameSpeedCtrl → GameSpeedManager へリネーム
  □ NavMeshCtrl → NavMeshSystem へリネーム
  
【優先度 3: 全体調和】
Week 4+:
  □ 残存する Ctrl 命名をリネーム
  □ suffix なしクラスを分類・リネーム
```

### Alpha Phase (2026年3月以降)

```
□ すべてのクラスを命名ルールに準拠
□ namespace を新構造に完全統一
```

---

## 📝 実装チェックリスト

### 新規クラス作成時の確認

- [ ] **命名**の決定
  - [ ] 適切な suffix を選択（Manager, System, Controller等）
  - [ ] PascalCase で記述
  - [ ] 1-2 語で表現可能か（意味が明確か）

- [ ] **配置場所**の確認
  - [ ] フォルダ構造に従っているか
  - [ ] 関連クラスと同じ場所か

- [ ] **責務**の確認
  - [ ] 1つの責務に限定されているか
  - [ ] 他クラスとの責務分離は明確か

- [ ] **namespace** の設定
  - [ ] OnoCoro.Core.Managers (Manager の場合)
  - [ ] OnoCoro.Game.Systems (System の場合)
  - [ ] 配置フォルダ構造と対応しているか

---

## 参考: Unity / C# の標準命名規則との対比

### Unity 標準に準拠した命名

```csharp
// Unity が使っている命名パターン

// 1. Manager
InputManager              // Unity 標準
AudioManager             // Unity 標準

// 2. System
ParticleSystem           // Unity 標準
AnimationSystem          // (未使用だが概念的)

// 3. Controller
AnimationController      // Unity 標準
CharacterController      // Unity 標準

// 4. Handler
EventHandler             // .NET 標準

// 5. Utility
Vector3.Distance()       // static utility
Array.Sort()            // static utility
```

**結論**: 本提案は Unity / .NET の標準慣例に準拠しており、業界標準に合わせている。

---

## 推奨される最初の一手

1. **新規ファイルから適用**（最優先）
   ```
   本提案ルールに従って新規クラスを作成
   既存コードへの影響最小化
   ```

2. **大規模クラスからリネーム**
   ```
   GameCtrl → GameController
   GameSpeedCtrl → GameSpeedManager
   NavMeshCtrl → NavMeshSystem
   ```

3. **段階的な完全移行**
   ```
   Alpha Phase で全体統一
   ```

---

## 結論

現在の命名は `*Ctrl` が主流で、Manager / System が混在しており、suffix なしのクラスも散見されます。

**提案する統一的な命名規則** は：

- ✅ **Manager** - リソース・状態管理（Singleton/static）
- ✅ **System** - ゲームシステム実装（複合的なゲーム機能）
- ✅ **Controller** - UI・入力制御（MonoBehaviour）
- ✅ **Handler** - イベント処理
- ✅ **Service** - 特定機能提供
- ✅ **Factory** - オブジェクト生成
- ✅ **Provider** - データ提供（キャッシュあり）
- ✅ **Utility** - 静的ユーティリティ（monoBehaviour ではない）
- ✅ **Struct/Data** - データ定義
- ✅ **(suffix なし)** - ゲームエンティティ（Tower, Enemy 等）

この規則により、**クラス名から責務が一目瞭然** になり、チーム開発の効率が向上します。

---

**参考資料**:
- [docs/scripts-folder-restructure-proposal.md](scripts-folder-restructure-proposal.md) - フォルダ構成改善提案
- [docs/architecture.md](architecture.md) - システムアーキテクチャ
- [docs/coding-standards.md](coding-standards.md) - C# コーディング規約

**次のステップ**: 本ドキュメントを AGENTS.md に統合し、新規ファイル作成時から適用開始（Prototype Phase Week 1）
