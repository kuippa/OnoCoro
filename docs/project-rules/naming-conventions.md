# 命名規則

**目的**: コード可読性・保守性・一貫性の向上・型安全性の確保

---

## 基本方針

[OK] **命名は責務を示す** - 名前からその役割が分かるべき  
[OK] **スコープに応じた命名** - アクセス範囲で命名規則を明確に  
[OK] **一貫性を保つ** - プロジェクト全体で統一

---

## Namespace

### 統一 Namespace

[WARN] **すべてのプロジェクトコードは `CommonsUtility` namespace を使用**

```csharp
// [OK] 全ファイル共通
namespace CommonsUtility
{
    public class GameManager { }
    internal class ConfigManager { }
}

// [NG] 階層的な namespace（禁止）
namespace OnoCoro.Core.Managers { }      // ❌
namespace Game.Systems { }                // ❌
```

[NOTE] **理由**: C# の namespace 単体では Assembly 外との境界を守れない。Assembly boundary に加えて、`internal` modifier で real encapsulation を実現

---

## クラス名（Class Suffixes）

### クラス命名規則表

| Suffix | 用途 | 責務 | 配置場所 | 例 |
|--------|------|------|---------|-----|
| **Manager** | リソース・状態管理 | Singleton/static で一元管理 | `Core/Managers/` | ConfigManager, PrefabManager |
| **System** | ゲームシステム | 複数エンティティの相互作用 | `Game/Systems/<Domain>/` | SpawnSystem, WeatherSystem |
| **Controller** | UI・入力制御 | MonoBehaviour として制御 | `Presentation/UI/` or `Presentation/Input/` | InputController, PauseMenuController |
| **Handler** | イベント処理 | 特定イベントに応答 | `Game/Events/` | CollisionHandler, GameOverHandler |
| **Service** | 特定機能提供 | 複合的で管理的な機能 | `Core/Services/` | SaveGameService, LocalizationService |
| **Factory** | オブジェクト生成 | 生成ロジック集約 | `Game/Units/Factories/` | TowerFactory, EnemyFactory |
| **Provider** | データ提供 | キャッシュ・遅延読み込み | `Data/Providers/` | StageDataProvider, ConfigProvider |
| **Utility** | 静的ユーティリティ | static メソッド集（no state） | `Core/Utilities/` | FileUtility, MathUtility |
| **(none)** | ゲームエンティティ | Scene の actor | `Game/Units/` | Tower, Enemy, Player |

### 詳細パターン別ガイド

#### 1. Manager（リソース・状態管理）

**用途**: Singleton や static manager でリソース・状態を一元管理

**特徴**:
- リソース管理（Asset 読み込み、キャッシュ管理）
- 状態管理（ゲーム設定、言語設定）
- グローバル状態の保持

**命名パターン**: `<Domain>Manager.cs`

**例**:
```csharp
InitializationManager       # 初期化順序管理
ConfigManager               # ゲーム設定管理
LanguageManager             # 言語リソース管理
PrefabManager               # プレファブ読み込み・キャッシュ

// [NG]
GameCtrl                    # → GameManager が推奨
GameSpeedCtrl               # → GameSpeedManager が推奨
```

**実装例**:
```csharp
// Singleton パターン
public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

// Static メソッド パターン
public static class ConfigManager
{
    public static int GameDifficulty { get; set; }
    public static float MasterVolume { get; set; }
}
```

---

#### 2. System（ゲームシステム）

**用途**: ゲーム進行に必要な各種システム（複合的な処理）

**特徴**:
- ゲームロジックの実装
- 複数エンティティの相互作用を管理
- イベント駆動的な設計

**命名パターン**: `<Domain>System.cs`

**例**:
```csharp
SpawnSystem                 # 敵スポーン管理システム
WeatherSystem               # 天候・環境イベントシステム
PhysicsSystem               # 物理・衝突判定システム
AudioSystem                 # 音声再生システム
NavMeshSystem               # NavMesh 管理・再ベークシステム

// [NG]
WindCtrl                    # → WeatherSystem が推奨
NavMeshCtrl                 # → NavMeshSystem が推奨
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

#### 3. Controller（UI・入力制御）

**用途**: UI コンポーネント・ユーザー入力の制御

**特徴**:
- MonoBehaviour として UI や GameObject に attach
- UI イベント（ボタンクリック）や入力の処理
- 表示・非表示の切り替え

**命名パターン**: `<Component>Controller.cs` または `<Panel>PanelController.cs`

**例**:
```csharp
InputController             # 入力受付制御
PauseMenuController         # メニュー UI 制御
GameTimerController         # ゲームタイマー UI 制御
MessageBoxController        # メッセージボックス表示制御

// [NG]
GameCtrl                    # → GameController が推奨（UI 制御なら）
```

**実装例**:
```csharp
public class PauseMenuController : MonoBehaviour
{
    private Button _resumeButton;
    
    void Start()
    {
        _resumeButton = GetComponentInChildren<Button>();
        _resumeButton.onClick.AddListener(OnResumeButtonClicked);
    }
    
    private void OnResumeButtonClicked()
    {
        // ゲーム再開処理
    }
}
```

---

#### 4. Service（特定機能の提供）

**用途**: 特定の機能を提供するサービスクラス（複合的で管理的）

**特徴**:
- 複数クラスから利用されるサービス
- 責務が限定されている
- static メソッドと instance メソッドの混在可

**命名パターン**: `<Function>Service.cs`

**例**:
```csharp
SaveGameService             # セーブゲーム機能を提供
LoadGameService             # ロードゲーム機能を提供
AnalyticsService            # アナリティクス送信機能を提供
LocalizationService         # 多言語化機能を提供
```

---

#### 5. Handler（イベント処理）

**用途**: イベント駆動的な処理を担当

**特徴**:
- 特定のイベントに応答
- 副次的な処理
- Event callback として使用される

**命名パターン**: `<Event>Handler.cs`

**例**:
```csharp
CollisionHandler            # 衝突イベント処理
TowerPlacementHandler       # タワー配置イベント処理
GameOverHandler             # ゲームオーバーイベント処理
```

---

#### 6. Factory（生成工場）

**用途**: オブジェクト生成を一元管理

**特徴**:
- 複雑な生成ロジック
- 複数の生成パターン
- Pooling との組み合わせ

**命名パターン**: `<Type>Factory.cs`

**例**:
```csharp
TowerFactory                # タワーインスタンス生成工場
EnemyFactory                # 敵インスタンス生成工場
ProjectileFactory           # 発射物インスタンス生成工場
```

---

#### 7. Provider（データ提供）

**用途**: データ取得・キャッシュ管理（取得に特化）

**特徴**:
- キャッシュ機構あり
- データベースアクセス抽象化
- 遅延読み込み

**命名パターン**: `<Data>Provider.cs`

**例**:
```csharp
StageDataProvider           # ステージデータ提供者
ConfigProvider              # 設定データ提供者
LocalizationProvider        # 多言語テキスト提供者
```

---

#### 8. Utility（静的ユーティリティ）

**用途**: 静的メソッド集（singleton ではない）

**特徴**:
- static class（MonoBehaviour ではない）
- 関数型プログラミング的
- 依存性が最小

**命名パターン**: `<Function>Utility.cs` または `<Function>Helper.cs`

**例**:
```csharp
FileUtility                 # ファイル操作ユーティリティ
LogUtility                  # ログ出力ユーティリティ
MathUtility                 # 数学演算ユーティリティ
GameObjectUtility           # GameObject 操作ユーティリティ

// [NG]
GameObjectTreat             # → GameObjectUtility が推奨
CommonsCalcs                # → MathUtility が推奨
XMLparser                   # → XMLUtility が推奨
```

**実装例**:
```csharp
public static class GameObjectUtility
{
    public static void SafeDestroy(Object obj)
    {
        if (obj == null) return;
        Object.Destroy(obj);
    }
    
    public static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        return component ?? obj.AddComponent<T>();
    }
}
```

---

#### 9. Data Models（データ構造）

**用途**: データの定義のみ（ロジックなし）

**特徴**:
- readonly struct 推奨
- ロジックを持たない
- Serializable

**命名パターン**: `<Entity>Data.cs` または `<Entity>Struct.cs`

**例**:
```csharp
TowerData                   # タワー属性データ
EnemyData                   # 敵属性データ
StageData                   # ステージ属性データ
ItemData                    # アイテム属性データ
```

---

#### 10. Game Entity（ゲームエンティティ）

**用途**: ゲームエンティティの実装

**特徴**:
- Scene に配置されるオブジェクト
- 複雑な状態遷移あり
- MonoBehaviour を直接継承

**命名パターン**: `<Entity>.cs` または `<Entity>Controller.cs`

**例**:
```csharp
Tower                       # タワーエンティティ（基底）
SentryGuard                 # 監視塔タワー（実装）
Enemy                       # 敵エンティティ（基底）
Litter                      # ゴミ敵（実装）

// [NG]
TowerCtrl                   # → Tower または TowerController が推奨
```

---

### 命名規則マッピング表（既存 → 推奨）

| 現在の名前 | 推奨される名前 | 理由 |
|-----------|-----------------|------|
| GameCtrl | GameController | UI/ゲーム進行制御 |
| GameSpeedCtrl | GameSpeedManager | ゲーム速度の状態管理 |
| NavMeshCtrl | NavMeshSystem | NavMesh システム管理 |
| WindCtrl | WeatherSystem | 天候・環境システム |
| LangCtrl | LanguageManager | 言語リソース管理 |
| BloomPathCtrl | BloomPathController | UI 制御 |
| MarkerIndicatorCtrl | MarkerIndicatorController | マーカー表示 UI |
| CoroutineRunner | CoroutineManager | コルーチン管理 |
| GameObjectTreat | GameObjectUtility | GameObject ユーティリティ |
| CommonsCalcs | MathUtility | 数学計算ユーティリティ |
| XMLparser | XMLUtility | XML パース ユーティリティ |

---

### Legacy 命名検出・警告

[WARN] **既存コードで古い命名パターンが見つかる場合の対応**

#### Pattern 1: `*Ctrl` Suffix（廃止予定）

```csharp
// [NG] 古い Ctrl suffix
public class GameCtrl : MonoBehaviour { }
public class GameSpeedCtrl : MonoBehaviour { }

// [ACTION] Obsolete 属性で警告
[Obsolete("GameCtrl は deprecated です。GameController (UI) または GameManager (state) を使用してください。naming-conventions.md を参照。")]
public class GameCtrl : MonoBehaviour { }
```

#### Pattern 2: サフィックスなし（曖昧）

```csharp
// [AMBIGUOUS] 分類が必要
public class CoroutineRunner { }          // → CoroutineManager
public class GameObjectTreat { }          // → GameObjectUtility
public class CommonsCalcs { }             // → MathUtility
```

#### Action チェックリスト（既存クラス修正時）

既存クラスを変更するときに確認：

- [ ] **パターン認識**: `*Ctrl` サフィックスまたはサフィックスなし？
- [ ] **責務評価**: Manager/System/Controller/Utility などどのカテゴリ？
- [ ] **マイグレーション追加**: [Obsolete] 属性を追加して指針を示す
- [ ] **コミットメッセージ記録**: リファクタリングの意図をログに記録

---

## 変数命名

### ローカル変数・パラメータ

[OK] **camelCase で記述** - 最初は小文字

```csharp
private void ProcessTower()
{
    int towerIndex = 0;              // [OK] ローカル変数
    float damageAmount = 10.5f;      // [OK] 明確な名前
    
    foreach (Tower tower in towers)  // [OK] パラメータ
    {
        // 処理
    }
}
```

### Private フィールド

[OK] **アンダースコア + camelCase** - `_fieldName`

```csharp
public class GameManager : MonoBehaviour
{
    private float _masterVolume;          // [OK] private field
    private List<Tower> _activeTowers;    // [OK] コレクション
    private bool _isInitialized;          // [OK] 状態フラグ
}
```

### SerializeField

[OK] **アンダースコア + camelCase** - Inspector に表示

```csharp
public class TowerController : MonoBehaviour
{
    [SerializeField] private Transform _shootPoint;       // [OK]
    [SerializeField] private float _fireRate = 2.0f;      // [OK]
    [SerializeField] private GameObject _explosionVFX;    // [OK]
}
```

### Public フィールド

[WARN] **原則禁止** - Property を使用すること

```csharp
// [NG] public field
public class GameConfig
{
    public float GameSpeed;        // ❌ 変更追跡不可
}

// [OK] Property で保護
public class GameConfig
{
    public float GameSpeed { get; set; } = 1.0f;    // [OK]
}

// [OK] Read-only property
public class GameConfig
{
    private float _gameSpeed = 1.0f;
    public float GameSpeed => _gameSpeed;           // [OK] getter only
}
```

### Boolean 変数

[OK] **`is`, `has`, `can`, `should` prefix を使用**

```csharp
// [OK] Boolean prefix
private bool _isActive;              // 状態
private bool _hasInitialized;        // 完了フラグ
private bool _canAttack;             // 可否判定
private bool _shouldRetry;           // 意図
private bool _isDebugMode;           // デバッグ状態

// [NG] 接尾辞（避けるべき）
private bool _activeFlag;            // ❌
private bool _initialized;           // ❌
private bool _attack;                // ❌
```

---

## 定数命名

### Private 定数

[OK] **アンダースコア + 大文字スネークケース** - `_CONSTANT_NAME`

```csharp
public class GameConfig : MonoBehaviour
{
    private const int _MAX_RETRY_COUNT = 3;           // [OK]
    private const string _DEFAULT_GAME_MODE = "Hard"; // [OK]
    private const float _GRAVITY_ACCELERATION = 9.8f; // [OK]
}
```

### Public 定数

[OK] **大文字スネークケース** - `CONSTANT_NAME`

```csharp
public class GameConstants
{
    public const string GAME_VERSION = "0.1.0";       // [OK]
    public const int DEFAULT_VOLUME = 80;             // [OK]
    public const float REFERENCE_RESOLUTION = 1920f;  // [OK]
}
```

### 定数のグループ化

[OK] **関連定数を static class で組織化**

```csharp
internal static class GameConst
{
    // 難易度定数
    internal const int EASY_DIFFICULTY = 1;
    internal const int NORMAL_DIFFICULTY = 2;
    internal const int HARD_DIFFICULTY = 3;
    
    // ゲーム定数
    internal const float GRAVITY_ACCELERATION = 9.8f;
    internal const float TERMINAL_VELOCITY = 53.0f;
}

internal static class UIConst
{
    internal const string PREFAB_BUTTON = "prefabs/ui/button";
    internal const float ANIMATION_DURATION = 0.3f;
}
```

### 定数化の判断基準

[OK] **以下の場合は定数化する**:
- コード内で複数回出現する値
- 変更の可能性がある値
- ハードコード化すると可読性が下がる値

[ACCEPTABLE] **以下の場合は直値でも許可**:
- 一度しか使われない値
- 値そのものが意味を持つ場合（1, 0, null など）
- 定数化することで逆に可読性が下がる場合

```csharp
// [OK] 複数回使用 → 定数化
private const int MAX_ENEMIES = 50;
for (int i = 0; i < MAX_ENEMIES; i++)
{
    // 処理
}

// [ACCEPTABLE] 一度きり → 直値でも許可
private void Initialize()
{
    _loading = UIHelper.FindOrInstantiatePrefab("nowloading", path, missingObjects);
}
```

---

## メソッド名

### 動詞で開始

[OK] **メソッド名は動詞で始まる** - 処理内容を示す

```csharp
// [OK] 動詞で開始
private void ProcessTower() { }
private bool IsEnemyAlive() { }
private void CacheData() { }
private void RegisterEventListener() { }

// [NG] 動詞で開始していない
private void UpdateCheck() { }        // ❌ Update をメソッド名に
private void Running() { }            // ❌ 状態を説明
```

### Boolean 返却メソッド

[OK] **`Is`, `Has`, `Can`, `Should` で開始**

```csharp
// [OK] Boolean を返すメソッド
private bool IsTowerActive() { return true; }
private bool HasEnoughFuel() { return fuel > 0; }
private bool CanAttack() { return _cooldownTimer <= 0; }
private bool ShouldRetry() { return retryCount < MAX_RETRY; }

// [NG] 命令形（避けるべき）
private bool Check() { }      // ❌ 何をチェック？
private bool Validate() { }   // ❌ 何を validate？
```

### 非同期メソッド

[OK] **`Async` suffix で終了**

```csharp
private async Task LoadGameDataAsync() { }      // [OK]
private async Task SaveProgressAsync() { }      // [OK]
private IEnumerator WaitForInitialization() { } // [OK] Coroutine
```

---

## Generic パラメータ

[OK] **単一文字（T, U, K, V）または説明的な名前**

```csharp
// [OK] 一般的な Generic パラメータ
public class Repository<T> where T : class { }
public Dictionary<string, T> GetByName<T>() { }

// [OK] 複数の Generic パラメータ
public class Pair<TKey, TValue> { }

// [OK] 説明的な Generic パラメータ（複雑な場合）
public interface IFactory<TEntity> where TEntity : class { }
```

---

## Pre-Commit チェックリスト

コミット前に以下を確認：

- [ ] **Namespace**: すべて `CommonsUtility` か確認
- [ ] **Class suffix**: 責務に合った suffix を使用
- [ ] **プライベートフィールド**: `_camelCase` を使用
- [ ] **定数**: 大文字スネークケース、適切にグループ化
- [ ] **Boolean 変数**: `is`, `has`, `can`, `should` prefix で開始
- [ ] **メソッド名**: 動詞で開始、意味明確
- [ ] **レガシー名**: `*Ctrl` suffix や曖昧な名前がないか
- [ ] **一貫性**: ファイル内・プロジェクト内で命名規則に従っているか

---

**関連資料**:
- [coding-csharp.md](coding-csharp.md) - C# コーディング規約
- [unity-design-patterns.md](unity-design-patterns.md) - MonoBehaviour パターン
- [AGENTS.md](../AGENTS.md) - Class Naming Convention 詳細
