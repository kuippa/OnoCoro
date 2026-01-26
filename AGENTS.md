# AGENTS.md - OnoCoro Project Agent Guidelines

This file defines the essential rules and guidelines that GitHub Copilot and AI Agents must follow when working on the OnoCoro project.

---

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Session Information Requirements](#session-information-requirements)
- [Technology Stack](#technology-stack)
- [Coding Standards](#coding-standards)
- [Class Naming Convention](#class-naming-convention)
- [Folder Structure](#folder-structure)
- [Development Workflow](#development-workflow)
- [Git Workflow](#git-workflow)
- [Data Protection](#data-protection)
- [Pre-Commit Checklist](#pre-commit-checklist)
- [Contributing](#contributing)

---

## Project Overview

**OnoCoro** is a geospatial visualization application (tower defense game) implemented in **Unity 6.3** using C#.

**Key Characteristics**:
- Processes and visualizes **CityGML format geographical data** via PLATEAU SDK
- Built with Unity 6.3 (cannot be changed)
- Implements tower defense mechanics with environmental cleanup themes
- Originally a 2-year-old backup recovery project

**Project Goals**:
- Display Japanese urban 3D data (PLATEAU format)
- Provide interactive geospatial visualization
- Implement tower defense gameplay mechanics

**Related Documentation**:
- [docs/introduction.md](docs/introduction.md) - Project purpose and non-goals
- [docs/architecture.md](docs/architecture.md) - System architecture
- [docs/coding-standards.md](docs/coding-standards.md) - C# implementation standards
- [.github/instructions.md](.github/instructions.md) - Project management guide

---

## Session Information Requirements

**MANDATORY**: All AI Agents must display session information at the start of each response.

### Required Format

```
**Model**: [Model Name (e.g., Claude Haiku 4.5)]
**Type**: [Agent Type (Fixed / Auto)]
**Session**: [Session Status (Continuous / New Start)]
```

### Example

```
**Model**: Claude Haiku 4.5
**Type**: Fixed
**Session**: Continuous (AGENTS.md, coding-standards.md loaded)
```

### Purpose

- Enables users to understand current agent context
- Provides visibility into session reset events
- Confirms document loading state

### Response Language

**MANDATORY**: All AI Agents must respond in **Japanese (日本語)** by default.

- ✅ **Respond in Japanese** for all code changes, documentation, and explanations
- ✅ **Use Japanese** for error messages, warnings, and logs added to code
- ✅ **Japanese first** for session information and interaction with users
- ✅ **Accept English requests** but respond in Japanese unless otherwise specified

---

## Technology Stack

### Required Technologies (Fixed - Cannot Be Changed)

| Technology | Version | Purpose |
|-----------|---------|---------|
| **Unity** | 6.3 | Game engine |
| **C#** | Latest | Programming language |
| **PLATEAU SDK** | Latest | Geospatial data processing |
| **Cinemachine** | Unity Standard | Camera control |
| **glTFast** | Unity Standard | 3D model loading |
| **Input System** | Unity Standard | Input management |

### Prohibited Suggestions

- ❌ **Unity version changes**
- ❌ **PLATEAU SDK removal**
- ❌ **External frameworks** (React, Vue, Angular, etc.)
- ❌ **Language migration** (JavaScript/TypeScript)
- ❌ **Python script generation** for Unity code

---

## Coding Standards

> **Complete Standards**: See [docs/coding-standards.md](docs/coding-standards.md)

**Key Requirements** (summary):
1. **No magic numbers/strings** - Use constants (`_CONSTANT_NAME` for private, `CONSTANT_NAME` for public)
2. **Required braces** - All control statements must use `{}`
3. **No ternary/null-coalescing** - Avoid `? :` and `?.` operators
4. **Early return pattern** - Use guard clauses instead of nested if statements
5. **Function length** - Maximum 40 lines per function
6. **Meaningful names** - Use descriptive variable names, not abbreviations
7. **Utility classes** - Consolidate related functionality (see standards doc)
8. **UnityEngine.Debug** - Always use explicit alias: `using Debug = UnityEngine.Debug;`

For detailed code examples and rationale, see [docs/coding-standards.md](docs/coding-standards.md).

---

## Access Modifier Policy

**MANDATORY**: Use `internal` as default for all Manager/System/Utility classes. Use `public` only for public interfaces.

### Basic Principle

OnoCoro prioritizes Assembly boundary encapsulation over namespace isolation, because C# `namespace` alone is insufficient for true encapsulation. The `internal` modifier provides:

- **Assembly boundary protection** - Prevents unintended external access
- **Recovery phase safety** - Makes global state dependencies explicit
- **Future extensibility** - Supports plugin/DLC architecture without API breakage

### Access Modifier Usage Guidelines

| Modifier | Usage Context | Example | Reasoning |
|----------|---------------|---------|-----------|
| **public** | Public API, stable contract | Interface definitions, main entry points | Guarantees backward compatibility |
| **internal** | Project-internal implementation | GameConfig, Manager classes, Utility classes | Restricts access to this assembly only |
| **protected** | Inheritance extension points | Base controller classes | Supports intentional subclassing |
| **private** | Class-internal only | Helper methods, cache variables | Hides implementation details |

### Default Pattern: internal

```csharp
// ✅ CORRECT: Manager classes use internal
internal class GameConfig : MonoBehaviour
{
    internal static string _APP_GAME_MODE = GlobalConst.GAME_MODE_DEBUG;
    internal static DebugLevel DebugLevel { get; set; } = DebugLevel.All;
}

// ✅ CORRECT: Utility classes use internal
internal static class LogUtility
{
    public static void WriteLog(LogLevel level, string message) { }
}

// ✅ CORRECT: Expose public interface, hide internal implementation
public interface IGameConfig { }  // Stable public API
internal class GameConfig : IGameConfig { } // Internal implementation
```

### When to Use public

Use `public` only in these scenarios:

1. **Public Interface/Contract** - Designed for external use
2. **Main Entry Point** - Game initialization, scene controller
3. **Asset Reference** - Serialized field that Unity Inspector needs access to

```csharp
// ✅ OK: Public interface is expected
public interface IPrefabManager
{
    GameObject GetPrefab(string prefabName);
}

// ✅ OK: Scene controller may be public for editor/testing
public class GameMainController : MonoBehaviour { }

// ❌ NG: No reason to expose this globally
public static class LogUtility { }  // → use internal
```

### Recovery Phase Context (Critical for OnoCoro)

Given that OnoCoro is recovering from a 2-year-old backup:

- **`public`** = "This is a stable, documented API that won't change"
- **`internal`** = "This is implementation detail; may change or refactor"

This distinction helps prevent:
- Unintended access to global state
- Coupling to internal implementation details
- Regression when refactoring recovered code
- Accidental API surface expansion

### Example: GameConfig Design

```csharp
// ✅ CORRECT: Restrict access, promote via interface if needed
internal sealed class GameConfig : MonoBehaviour
{
    // All state is internal - prevents external manipulation
    internal static string _APP_GAME_MODE = GlobalConst.GAME_MODE_DEBUG;
    internal static DebugLevel DebugLevel { get; set; } = DebugLevel.All;
    internal static string LogFileName { get; set; } = GlobalConst._LOG_FILE_NAME;
}

// If external code needs read-only access, use interface
public interface IGameConfigProvider
{
    string GetGameMode();
    DebugLevel GetDebugLevel();
}

// Internal implementation of public interface
internal class GameConfigProvider : IGameConfigProvider
{
    public string GetGameMode() => GameConfig._APP_GAME_MODE;
    public DebugLevel GetDebugLevel() => GameConfig.DebugLevel;
}
```

### Pre-Commit Checklist for Access Modifiers

When reviewing code changes:

- [ ] **Default internal**: Manager/System/Utility classes are `internal` unless justified
- [ ] **No premature public**: Avoid `public` to "future-proof" code
- [ ] **Interface-driven**: If external access needed, expose via `public interface`, hide implementation with `internal`
- [ ] **Consistent with Recovery policy**: Global state is protected from external manipulation
- [ ] **Assembly boundary respected**: No reliance on `namespace` alone for encapsulation

---

## Class Naming Convention

**MANDATORY**: All C# classes must follow the unified naming convention.

### Class Name Suffixes (Standard Patterns)

**Use appropriate suffix based on class responsibility**:

| Suffix | Usage | Example |
|--------|-------|---------|
| **Manager** | リソース・状態管理 (Singleton/static) | `ConfigManager`, `PrefabManager` |
| **System** | ゲームシステム実装 (複合的なゲーム機能) | `WeatherSystem`, `SpawnSystem` |
| **Controller** | UI・入力制御 (MonoBehaviour) | `InputController`, `PauseMenuController` |
| **Handler** | イベント処理 (event callback) | `CollisionHandler`, `GameOverHandler` |
| **Service** | 特定機能提供 (複合的で管理的) | `SaveGameService`, `LocalizationService` |
| **Factory** | オブジェクト生成 (生成ロジック集約) | `TowerFactory`, `EnemyFactory` |
| **Provider** | データ提供 (キャッシュ機構あり) | `StageDataProvider`, `ConfigProvider` |
| **Utility** | 静的ユーティリティ (static メソッド集) | `FileUtility`, `MathUtility` |
| **(none)** | ゲームエンティティ (game entity) | `Tower`, `Enemy`, `Player` |

### 1. Manager（リソース・状態管理）

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

// NG
GameCtrl                    # → GameManager が推奨
GameSpeedCtrl               # → GameSpeedManager が推奨
```

**配置場所**: `Core/Managers/`

**実装例**:
```csharp
public static class ConfigManager
{
    public static int GameDifficulty { get; set; }
    public static float MasterVolume { get; set; }
}

// または MonoBehaviour の場合
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
SpawnSystem                 # 敵スポーン管理システム
WeatherSystem               # 天候・環境イベントシステム
PhysicsSystem               # 物理・衝突判定システム
AudioSystem                 # 音声再生システム
NavMeshSystem               # NavMesh 管理・再ベークシステム

// NG
WindCtrl                    # → WeatherSystem が推奨
NavMeshCtrl                 # → NavMeshSystem が推奨
```

**配置場所**: `Game/Systems/<Domain>/`

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
InputController             # 入力受付制御
PauseMenuController         # メニュー UI 制御
GameTimerController         # ゲームタイマー UI 制御
MessageBoxController        # メッセージボックス表示制御

// NG
GameCtrl                    # → GameController が推奨（UI 制御なら）
```

**配置場所**: `Presentation/UI/` または `Presentation/Input/`

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

### 4. Service（特定機能の提供）

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

**配置場所**: `Core/Services/`

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
CollisionHandler            # 衝突イベント処理
TowerPlacementHandler       # タワー配置イベント処理
GameOverHandler             # ゲームオーバーイベント処理
```

**配置場所**: `Game/Events/`

---

### 6. Factory（生成工場）

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

**配置場所**: `Game/Units/Factories/`

---

### 7. Provider（データ提供）

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

**配置場所**: `Data/Providers/`

---

### 8. Utility（静的ユーティリティ）

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

// NG
GameObjectTreat             # → GameObjectUtility が推奨
CommonsCalcs                # → MathUtility が推奨
XMLparser                   # → XMLUtility が推奨
```

**配置場所**: `Core/Utilities/`

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

### 9. Data Models（データ構造）

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

**配置場所**: `Data/Models/`

---

### 10. Game Entity（ゲームエンティティ）

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

// NG
TowerCtrl                   # → Tower または TowerController が推奨
```

**配置場所**: `Game/Units/`

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

### ⚠️ Legacy Naming Detection & Warning

**When you encounter classes with outdated naming patterns:**

#### Pattern 1: `*Ctrl` Suffix (Deprecated)

```csharp
// 🔴 DEPRECATED (needs refactoring decision)
public class GameCtrl : MonoBehaviour { }
public class GameSpeedCtrl : MonoBehaviour { }

// ✅ ACTION REQUIRED (when modifying these classes):
[Obsolete("GameCtrl is deprecated. Use GameController (UI) or GameManager (state). See AGENTS.md Class Naming Convention.")]
public class GameCtrl : MonoBehaviour { }
```

#### Pattern 2: No Suffix (Ambiguous)

```csharp
// 🔴 AMBIGUOUS (needs classification)
public class CoroutineRunner { }          // → CoroutineManager
public class GameObjectTreat { }          // → GameObjectUtility
public class CommonsCalcs { }             // → MathUtility
```

### Action Checklist (When Touching Existing Classes)

- [ ] **Recognize the pattern**: `*Ctrl` suffix or no suffix?
- [ ] **Assess responsibility**: Manager/System/Controller/Utility/etc.?
- [ ] **Add migration guidance** with [Obsolete] attribute
- [ ] **Log to commit message** with refactoring intent

---

## Folder Structure

**MANDATORY**: All C# files must be placed in the correct folder according to this layer structure.

> **Complete Reference**: See [docs/scripts-folder-structure-completed.md](docs/scripts-folder-structure-completed.md)

### Layer Architecture (4 Layers)

OnoCoro uses a **4-layer architecture** with clear responsibility separation:

| Layer | Responsibility | Examples | External Dependencies |
|-------|-----------------|----------|----------------------|
| **Presentation** | UI display, Input handling | CameraController, InputController, HUD | Game, View |
| **Game** | Game logic, Systems | SpawnController, WeatherController, GameManager | Data, Units |
| **Data** | Data models, Repositories | StageRepository, Models, PLATEAU | Core/Utilities |
| **Core** | Common infrastructure | Managers, Utilities, Handlers, Constants | (none - independent) |

### Folder Structure by Layer

```
Assets/Scripts/
├── Presentation/            【Layer 1: Presentation】
│   ├── UI/                  (Controls/, Dialogs/, HUD/, Panels/)
│   ├── View/                (Cameras/, Rendering/, Effects/)
│   └── Input/               (InputController, PlayerInputs)
├── Game/                    【Layer 2: Game Logic】
│   ├── GameManager/
│   ├── Systems/             (Stage/, Spawn/, Weather/)
│   ├── Units/               (Towers/, Enemies/, Items/, Bullets/)
│   └── Events/              (Environmental/, System/)
├── Data/                    【Layer 3: Data】
│   ├── Models/
│   ├── Repositories/        (StageRepository, StageYamlRepository)
│   └── Plateau/
├── Core/                    【Layer 4: Core (Orthogonal)】
│   ├── Managers/            (GameSpeedManager, LanguageManager, etc.)
│   ├── Utilities/           (FileUtility, MathUtility, etc.)
│   ├── Handlers/            (ExceptionHandler, etc.)
│   ├── Constants/
│   ├── Helpers/
│   └── Editor/
└── UnitTest/                【テストスクリプト】
    ├── LogUtilityTest.cs    (一時的なテストスクリプト)
    └── ...
```

### テストスクリプトの管理

**テストスクリプトの配置と移動**:

| フェーズ | 場所 | 説明 |
|---------|------|------|
| **作成・実行中** | `Assets/Scripts/UnitTest/` | 機能テスト・デバッグ用スクリプト |
| **使用後** | `Assets/Scripts/Core/Editor/` | アーカイブ・参考資料として保管 |
| **削除** | 削除 | テストが不要になった場合 |

**テストスクリプトの命名規則**:
```csharp
// ✅ CORRECT: 機能名 + Test サフィックス
LogUtilityTest.cs
DebugClassTest.cs
PrefabManagerTest.cs

// ❌ WRONG: 曖昧な命名
Test.cs
MyTest.cs
TestScript.cs
```

**テストスクリプトの特徴**:
- `UnitTest/` フォルダはビルドから除外可能（.asmdef または .meta 設定）
- Editor Only で機能するテストも含む
- 使用後は `Core/Editor/` に移動してアーカイブ化
- 参考実装として他の開発者が参照できるようにしておく

### File Placement Rules

**When creating a new file, determine the correct folder by responsibility**:

| Type | Suffix | Folder | Example |
|------|--------|--------|---------|
| **Resource/State Management** | Manager | Core/Managers/ | GameSpeedManager.cs |
| **Game Features** | System or Controller | Game/Systems/ | WeatherController.cs |
| **UI Components** | Controller | Presentation/UI/ | PanelController.cs |
| **Static Functions** | Utility | Core/Utilities/ | FileUtility.cs |
| **Data Access** | Repository | Data/Repositories/ | StageRepository.cs |
| **Event Handling** | Handler | Core/Handlers/ | EventHandler.cs |
| **Data Definition** | (none) | Data/Models/ | GameStruct.cs |
| **Game Entity** | (none) | Game/Units/ | Tower.cs, Enemy.cs |

### Namespace Rules

**MANDATORY**: Use unified namespace `CommonsUtility` for all project code.

```csharp
// ✅ CORRECT
namespace CommonsUtility
{
    public class GameSpeedManager { }
}

// ❌ WRONG - Do not use hierarchical namespaces
namespace OnoCoro.Core.Managers { }
namespace OnoCoro.Game.Systems { }
```

### Layer Dependency Rules

**STRICT**: Layers can only depend on layers below them. NO upward dependencies allowed.

```
Presentation ──┐
               │
Game ──────────┼──→ Data
               │
               └──→ Core (lowest layer - depends on nothing)
```

**Key**: Presentation layer includes both UI and View subsystems (cameras, rendering, effects).

**Allowed** ✅:
- Presentation layer using Game, Data, and Core layer classes
- Game layer using Data and Core layer classes
- Data layer using Core layer classes

**Forbidden** ❌:
- Any upward dependencies (Core/Data/Game cannot use upper layers)

### File Creation Checklist

When adding a new file, verify:

- [ ] **Correct Folder**: File is in the appropriate layer folder
- [ ] **Correct Namespace**: Using `CommonsUtility`
- [ ] **Correct Suffix**: Class name has appropriate suffix (Manager/Controller/Utility/etc.)
- [ ] **Correct Layer**: No forbidden upward dependencies
- [ ] **Documentation**: Brief comments explaining class responsibility
- [ ] **Related Reference**: Update [docs/scripts-folder-structure-completed.md](docs/scripts-folder-structure-completed.md) if creating a new folder category

---

## Development Workflow

### Document Loading

**MANDATORY**: Load these documents before any merge or editing work:

| Document | Path | Timing |
|----------|------|--------|
| AGENTS.md (this file) | `AGENTS.md` | Before all merge/edit work |
| coding-standards.md | `docs/coding-standards.md` | Before all merge/edit work |
| architecture.md | `docs/architecture.md` | Before new class design or major refactoring |
| **scripts-folder-structure-completed.md** | `docs/scripts-folder-structure-completed.md` | **Before adding new files to Assets/Scripts/** |
| introduction.md | `docs/introduction.md` | For policy confirmation |

**Confirm loading in session message**:
```
**Session**: Continuous (AGENTS.md, coding-standards.md loaded)
```

### Windows PowerShell Environment

**REQUIRED**: This is a Windows-only project

- ✅ **Allowed**: PowerShell cmdlets
- ❌ **Forbidden**: Linux/macOS bash commands

| Linux/macOS | Windows PowerShell |
|-------------|-------------------|
| `ls -la` | `Get-ChildItem -Force` |
| `grep pattern` | `Select-String "pattern"` |
| `cat file` | `Get-Content file` |
| `find . -name "*.cs"` | `Get-ChildItem -Recurse -Filter "*.cs"` |
| `rm -rf folder` | `Remove-Item -Path folder -Recurse -Force` |

---

## Git Workflow

### Branch Strategy

- `main`: Stable release branch
- `develop`: Development branch
- `feature/*`: Feature branch
- `bugfix/*`: Bug fix branch

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Type Values**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code formatting (no meaning change)
- `refactor`: Code cleanup
- `perf`: Performance improvement
- `test`: Test code
- `chore`: Build/tool changes

---

## Data Protection

This project recovered from SSD failure. Data protection is critical.

### Required Actions

- ✅ **Commit frequently to Git**
- ✅ **Consult before adding large files**
- ✅ **Follow `.gitignore` rules**

### Prohibited Actions

- ❌ **Add Library, Temp, Obj folders**
- ❌ **Add files >100MB without consultation**
- ❌ **Add binary formats (.blend, .fbx, .psd) without consultation**

---

## Pre-Commit Checklist

Before proposing code, verify:

- [ ] **Constants**: No magic numbers/strings
- [ ] **Braces**: All control statements have `{}`
- [ ] **Operators**: No ternary `? :` or `?.`
- [ ] **Nesting**: Early return used; no nested ifs
- [ ] **Function Length**: ≤40 lines
- [ ] **Variable Names**: Meaningful, not abbreviated
- [ ] **Utilities**: Common logic in utility classes
- [ ] **ScrollRect**: Using `normalizedPosition`
- [ ] **PowerShell**: No Linux/macOS commands
- [ ] **Coding Standards**: All standards followed

**Fix violations before proposing code.**

---

## Contributing

This is a community-driven project. Contributions welcome!

**See Also**:
- [.github/instructions.md](.github/instructions.md) - Project management guide
- [CONTRIBUTING.md](CONTRIBUTING.md) (when created) - Contribution guidelines
- [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) (when created) - Community standards

### Key Reminders

1. **Load documentation first** - Read AGENTS.md and coding-standards.md before starting work
2. **Follow the rules** - Do not propose changes that violate these guidelines
3. **Test thoroughly** - Verify code follows standards before proposing
4. **Ask if uncertain** - When in doubt, ask the user before proceeding
5. **Protect data** - Commit frequently and follow Git workflow strictly

---

## License

MIT License - See [LICENSE](LICENSE) for details

---

**Last Updated**: 2026-01-26
**Project**: OnoCoro (Unity 6.3 Geospatial Visualization)
