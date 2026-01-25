# AGENTS.md - OnoCoro Project Agent Guidelines

This file defines the essential rules and guidelines that GitHub Copilot and AI Agents must follow when working on the OnoCoro project.

---

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Session Information Requirements](#session-information-requirements)
- [Technology Stack](#technology-stack)
- [Coding Standards](#coding-standards)
- [Class Naming Convention](#class-naming-convention)
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
- [docs/recovery-workflow.md](docs/recovery-workflow.md) - Recovery merge rules and guidelines
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

## Class Naming Convention

**MANDATORY**: All C# classes must follow the unified naming convention.

> **Complete Convention**: See [docs/class-naming-convention-proposal.md](docs/class-naming-convention-proposal.md)

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

### ⚠️ Legacy Naming Detection & Warning

**When you encounter classes with outdated naming patterns:**

#### Pattern 1: `*Ctrl` Suffix (Deprecated)

```csharp
// 🔴 DEPRECATED (needs refactoring decision)
public class GameCtrl : MonoBehaviour { }
public class GameSpeedCtrl : MonoBehaviour { }
public class NavMeshCtrl : MonoBehaviour { }
public class WindCtrl : MonoBehaviour { }

// ✅ ACTION REQUIRED (when modifying these classes):
// - Determine actual responsibility
// - Mark with [Obsolete] attribute with migration guidance
// - Add comment with new name recommendation
```

**Recommended refactoring mapping**:
```csharp
// When you touch these classes, add guidance comment:

// 🔴 GameCtrl → ❓ GameController / GameManager?
// - If UI control: rename to GameController
// - If state management: rename to GameManager
// Add below to class:
[Obsolete("GameCtrl is deprecated. Use GameController (UI control) or GameManager (state management). See docs/class-naming-convention-proposal.md")]
public class GameCtrl : MonoBehaviour { }

// 🔴 GameSpeedCtrl → 🟢 GameSpeedManager
// Clearly state management - can be renamed with confidence
[Obsolete("GameSpeedCtrl renamed to GameSpeedManager. Update references and migrate. See docs/class-naming-convention-proposal.md", false)]
public class GameSpeedCtrl : MonoBehaviour { }

// 🔴 NavMeshCtrl → 🟢 NavMeshSystem
// Clearly system implementation
[Obsolete("NavMeshCtrl renamed to NavMeshSystem. See docs/class-naming-convention-proposal.md", false)]
public class NavMeshCtrl : MonoBehaviour { }

// 🔴 WindCtrl → 🟢 WeatherSystem
// Part of weather system
[Obsolete("WindCtrl integrated into WeatherSystem. See docs/class-naming-convention-proposal.md", false)]
public class WindCtrl : MonoBehaviour { }
```

#### Pattern 2: No Suffix (Ambiguous)

```csharp
// 🔴 AMBIGUOUS (needs classification)
public class CoroutineRunner { }          // → CoroutineManager
public class GameObjectTreat { }          // → GameObjectUtility
public class CommonsCalcs { }             // → MathUtility
public class XMLparser { }                // → XMLUtility

// ✅ ACTION REQUIRED (when modifying):
[Obsolete("Add appropriate suffix (Manager/Utility/etc). See docs/class-naming-convention-proposal.md")]
public class CoroutineRunner { }
```

#### Pattern 3: Mixed Manager/Ctrl

```csharp
// 🔴 INCONSISTENT (Manager と Ctrl が同じ役割)
public class InitializationManager { }    // ✅ OK - already correct
public class MaterialManager { }          // ✅ OK - already correct
public class GameCtrl { }                 // ❓ Uncertain - check responsibility
public class LangCtrl { }                 // → LanguageManager (state mgmt)

// ✅ ACTION REQUIRED (when modifying GameCtrl or LangCtrl):
[Obsolete("GameCtrl inconsistent with Manager suffix. Determine if GameController (UI) or GameManager (state) is appropriate.")]
public class GameCtrl : MonoBehaviour { }
```

### Action Checklist (When Touching Existing Classes)

**Each time you modify a legacy-named class:**

- [ ] **Recognize the pattern**
  - [ ] `*Ctrl` suffix detected?
  - [ ] No suffix on manager-like class?
  - [ ] Inconsistent naming with similar classes?

- [ ] **Assess responsibility**
  - [ ] Is this a Manager (state/resource)?
  - [ ] Is this a System (game feature)?
  - [ ] Is this a Controller (UI/input)?
  - [ ] Is this a Utility (static methods)?
  - [ ] Is this a Handler/Service/Factory/Provider?

- [ ] **Add migration guidance**
  ```csharp
  // Option 1: If responsibility is CLEAR
  [Obsolete("Rename to <NewName>Manager/System/Controller. See docs/class-naming-convention-proposal.md")]
  public class LegacyCtrl : MonoBehaviour { }
  
  // Option 2: If responsibility is UNCLEAR
  [Obsolete("Class naming needs refactoring decision. Check docs/class-naming-convention-proposal.md and apply appropriate suffix (Manager/System/Controller/etc)")]
  public class AmbiguousClass : MonoBehaviour { }
  ```

- [ ] **Log to commit message**
  ```
  fix(legacy): ClassName refactoring guidance added
  
  - Added [Obsolete] attribute with migration path
  - See docs/class-naming-convention-proposal.md
  - Future: plan full rename in Phase X
  ```

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
└── Core/                    【Layer 4: Core (Orthogonal)】
    ├── Managers/            (GameSpeedManager, LanguageManager, etc.)
    ├── Utilities/           (FileUtility, MathUtility, etc.)
    ├── Handlers/            (ExceptionHandler, etc.)
    ├── Constants/
    ├── Helpers/
    └── Editor/
```

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

**Allowed** ✅:
- Game layer using Data layer classes
- Game layer using Core layer classes
- Presentation layer using Game layer classes

**Forbidden** ❌:
- Core layer using Game layer classes
- Core layer using Data layer classes
- Data layer using Game layer classes

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

## Recovery Merge Rules

**See [docs/recovery-workflow.md](docs/recovery-workflow.md) for detailed recovery merge guidelines.**

**Key principle**: Do NOT modify code if there's no functional improvement. Minor refactoring without functional change should be SKIPPED.

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

**Last Updated**: 2026-01-20
**Project**: OnoCoro (Unity 6.3 Geospatial Visualization)
