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

**OnoCoro** is a geospatial visualization application implemented in **Unity 6.3** using C#.

**Key Characteristics**:
- Processes and visualizes **CityGML format geographical data** via PLATEAU SDK
- Built with Unity 6.3 (cannot be changed)
- Season 2 (2026-01 to 2026-05): tower defense mechanics with environmental cleanup themes
- Season 3 (2026-06 onward): repurposed as a **disaster-prevention investment simulation** for community workshops, reusing the tower defense assets
- Originally a 2-year-old backup recovery project

**Project Goals**:
- Display Japanese urban 3D data (PLATEAU format)
- Provide interactive geospatial visualization
- Provide a turn-based (Year cycle) disaster simulation for workshop facilitation

**Related Documentation**:
- [docs/vision/roadmap_season3.md](docs/vision/roadmap_season3.md) - Season 3 roadmap and project direction
- [docs/architecture/README.md](docs/architecture/README.md) - System architecture
- [docs/project-rules/coding-csharp.md](docs/project-rules/coding-csharp.md) - C# implementation standards
- [.github/instructions.md](.github/instructions.md) - Project management guide

---

## Session Information Requirements

**MANDATORY**: All AI Agents must display session information at the start of each response.

### Required Format

```
**Model**: [Model Name (e.g., Claude Haiku 4.5)]
**Type**: [Agent Type (Fixed / Auto)]
**Session**: [Session Status (Continuous / New Start)]
**Context**: [Loaded Documents (AGENTS.md, coding-standards.md, .github/instructions/ etc.)]
```

### Example

```
**Model**: Claude Haiku 4.5
**Type**: Fixed
**Session**: Continuous
**Context**: AGENTS.md, coding-standards.md, .github/instructions/, access-modifiers.md, naming-conventions.md, folder-structure.md (6 files) loaded
```

### Purpose

- Enables users to understand current agent context
- Provides visibility into session reset events
- Confirms document loading state
- Clarifies which instruction files are active

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

> **Complete Standards**: See [docs/project-rules/coding-csharp.md](docs/project-rules/coding-csharp.md)

**Key Requirements** (summary):
1. **No magic numbers/strings** - Use constants (`_CONSTANT_NAME` for private, `CONSTANT_NAME` for public)
2. **Required braces** - All control statements must use `{}`
3. **No ternary/null-coalescing** - Avoid `? :` and `?.` operators
4. **Early return pattern** - Use guard clauses instead of nested if statements
5. **Function length** - Maximum 40 lines per function
6. **Meaningful names** - Use descriptive variable names, not abbreviations
7. **Utility classes** - Consolidate related functionality (see standards doc)
8. **Debug alias** - Always use the wrapper: `using Debug = CommonsUtility.Debug;`
   （`UnityEngine.Debug` を直接使うと `DebugLevel` による出力制御が効かず、
   `Debug.LogTrace()` も使えない）
   - `namespace CommonsUtility` 内のクラスは、名前解決でラッパーが優先されるため
     エイリアス不要（書いても害は無い）
   - **例外1**: `Core/Utilities/LogUtility.cs` はラッパーの出力先そのもの。
     ラッパーを使うと無限再帰するため `UnityEngine.Debug` を完全修飾で使う
   - **例外2**: `Editor/` 配下のエディタ拡張。インポート時・ビルド時に動くもので、
     実行時のログファイル出力に載せる意味が無いため対象外
9. **Keep logging sparse** - See "Logging Policy" below
10. **Avoid MonoBehaviour.Update** - See "Update Policy" below
11. **No DontDestroyOnLoad** - See "Scene Lifetime Policy" below

For detailed code examples and rationale, see [docs/project-rules/coding-csharp.md](docs/project-rules/coding-csharp.md).

### Update Policy

**`MonoBehaviour.Update` は極力使わないこと。**

理由は 2 つある。

1. **倍速・一時停止に追従しない** — 本プロジェクトは `GameSpeedManager` が
   `Time.timeScale` を操作する。`Update` で自前に時間を数えると倍速処理を
   個別に実装することになり、実装漏れが起きる
2. **毎フレームに処理が集中する** — Update を持つコンポーネントが増えるほど
   1 フレームの負荷が積み上がる

**代わりに使うもの**:

| 用途 | 使うもの |
|------|---------|
| 一定間隔の監視・定期処理 | コルーチン + `yield return new WaitForSeconds(n)` |
| 大量オブジェクトの分散処理 | コルーチンでキューを小分けに消化する |
| 他システムと足並みを揃えたい処理 | 既存の統合ループから呼ぶ |

`WaitForSeconds` は `Time.timeScale` に従うため、**倍速・一時停止に自動で追従する**。
これが Update より優れている一番の理由。

[NOTE] 入力受付やカメラ追従など、毎フレームでなければ成立しない処理は Update でよい。

### Scene Lifetime Policy

**`DontDestroyOnLoad` は使わないこと。** ステージ寿命のオブジェクトは
ステージ開始処理の中で生成し、シーン遷移で破棄させる。

詳細と、常駐させた場合に何が壊れるかは
[docs/project-rules/unity-design-patterns.md](docs/project-rules/unity-design-patterns.md)
の「シーン寿命とオブジェクトの生成場所」を参照。

[IMPORTANT] `[RuntimeInitializeOnLoadMethod]` での自己生成も同じ理由で禁止。
**起動時に一度しか走らないため、タイトル画面で生成されてステージロードで消える。**
エディタはステージシーンで直接 Play するので露見せず、
ビルド版でだけ機能が丸ごと動かなくなる。

### Logging Policy

**ログは「後で読む人が追える量」に保つこと。** 大量のトレースログは Console を流し、
必要な情報を埋もれさせる。ログ行が増えるとコード自体も読みにくくなる。

**書く前に「これは平常時に読む価値があるか？」を自問する。** 無いなら書かないか、
`Debug.LogTrace()` を使う。

| 用途 | 使うもの | 出力されるか |
|------|---------|------------|
| 処理の節目・結果のサマリー（1 イベントに 1 行程度） | `Debug.Log()` | 既定で出る |
| ループ内・毎フレーム・オブジェクト単位の追跡用 | `Debug.LogTrace()` | 既定では出ない |
| 想定外だが処理は継続できる | `Debug.LogWarning()` | 出る |
| 処理が破綻している | `Debug.LogError()` | 出る |

- ❌ ループの中で `Debug.Log()` を回さない（`Debug.LogTrace()` を使う）
- ❌ 「関数に入った」「値を取得した」だけのログを書かない
- ❌ デバッグで使ったログをコメントアウトして残さない。**消す**（履歴は git にある）
- ✅ N 件を 1 行に集約する（`{count} 件を処理` であって 1 件ずつ出さない）
- ✅ 上限や打ち切りなど、**黙って挙動が変わる箇所には必ず警告を出す**
- ✅ 調査用の一時的なダンプは、調査が終わったらコードごと削除する

トレースを見たいときだけ `GameConfig.DebugLevel = DebugLevel.Trace` に切り替える。

---

## Access Modifier Policy

**MANDATORY**: Use `internal` as default for all Manager/System/Utility classes. Use `public` only for public interfaces.

### Quick Reference

| Modifier | Usage | Example |
|----------|-------|---------|
| **internal** | Default for Manager/System/Utility | GameConfig, PrefabManager, FileUtility |
| **public** | Public API, Interfaces, main entry | IPrefabManager, GameMainController |
| **protected** | Inheritance extension points | Base controller classes |
| **private** | Class-internal only | Helper methods, cache variables |

### Core Principle

- Default to `internal` for all Manager/System/Utility classes
- Use `internal` as "implementation detail; may change"
- Use `public` only as "stable API; won't change"
- Expose via `public interface`, hide with `internal` implementation

### Recovery Phase Context

OnoCoro is recovering from a 2-year-old backup:
- **`public`** = This is a stable, documented API
- **`internal`** = This is implementation detail; may refactor

This prevents unintended access to global state and coupling to implementation details.

**[詳細ガイド](docs/project-rules/access-modifiers.md)** - Pre-Commit チェックリスト、実装例、詳細な使い分けガイド

---

## Class Naming Convention

**MANDATORY**: All C# classes must follow the unified naming convention.

### Class Name Suffixes (Quick Reference)

| Suffix | Usage | Example |
|--------|-------|---------|
| **Manager** | Resource/state management | ConfigManager, PrefabManager |
| **System** | Game features & systems | WeatherSystem, SpawnSystem |
| **Controller** | UI/input control | InputController, PauseMenuController |
| **Handler** | Event processing | CollisionHandler, GameOverHandler |
| **Service** | Specific functionality | SaveGameService, LocalizationService |
| **Factory** | Object creation | TowerFactory, EnemyFactory |
| **Provider** | Data provisioning | StageDataProvider, ConfigProvider |
| **Utility** | Static utilities | FileUtility, MathUtility |
| **(none)** | Game entity | Tower, Enemy, Player |

### Core Principles

1. **Manager** - リソース・状態管理 (Singleton/static)
   - Asset読み込み、キャッシュ、グローバル状態
   - 配置: `Core/Managers/`

2. **System** - ゲームシステム実装
   - ゲームロジック、複数エンティティの相互作用
   - 配置: `Game/Systems/<Domain>/`

3. **Controller** - UI・入力制御 (MonoBehaviour)
   - UI イベント、入力処理
   - 配置: `Presentation/UI/` or `Presentation/Input/`

4. **Service/Factory/Provider/Utility/Handler** - See detailed guide

### Naming Mapping (既存 → 推奨)

| Old Name | New Name | 理由 |
|----------|----------|------|
| GameCtrl | GameController | UI/ゲーム制御 |
| GameSpeedCtrl | GameSpeedManager | 状態管理 |
| CoroutineRunner | CoroutineManager | リソース管理 |
| GameObjectTreat | GameObjectUtility | ユーティリティ |

### Legacy Pattern Detection

古い命名パターン (`*Ctrl` suffix, no suffix) を見かけたら、[Obsolete] 属性を追加してマイグレーション指針を示します。

**[詳細ガイド](docs/project-rules/naming-conventions.md)** - 10 パターン詳細、実装例、Legacy Detection チェックリスト

---

## Folder Structure

**MANDATORY**: All C# files must be placed in the correct folder according to this layer structure.

### 4-Layer Architecture

OnoCoro uses clear responsibility separation:

| Layer | Responsibility | Examples |
|-------|-----------------|----------|
| **Presentation** | UI display, Input handling | CameraController, InputController, HUD |
| **Game** | Game logic & systems | SpawnController, WeatherController |
| **Data** | Data models & repositories | StageRepository, Models, PLATEAU |
| **Core** | Common infrastructure | Managers, Utilities, Handlers, Constants |

### Folder Structure

```
Assets/Scripts/
├── Presentation/            [Layer 1]
│   ├── UI/          ├── View/         ├── Input/
├── Game/                    [Layer 2]
│   ├── GameManager/ ├── Systems/      ├── Units/  ├── Events/
├── Data/                    [Layer 3]
│   ├── Models/      ├── Repositories/ ├── Plateau/
├── Core/                    [Layer 4]
│   ├── Managers/    ├── Utilities/    ├── Handlers/
│   ├── Constants/   ├── Helpers/      ├── Editor/
└── UnitTest/                [Tests]
```

### File Placement Rules

| Type | Folder | Example |
|------|--------|---------|
| Manager | Core/Managers/ | GameSpeedManager.cs |
| System/Controller | Game/Systems/ | WeatherSystem.cs |
| UI Component | Presentation/UI/ | PanelController.cs |
| Utility | Core/Utilities/ | FileUtility.cs |
| Repository | Data/Repositories/ | StageRepository.cs |
| Handler | Core/Handlers/ | EventHandler.cs |
| Data Model | Data/Models/ | GameStruct.cs |
| Entity | Game/Units/ | Tower.cs |

### Namespace Rule

**MANDATORY**: Use unified namespace `CommonsUtility` for all code.

```csharp
namespace CommonsUtility
{
    public class GameSpeedManager { }  // [OK]
}
```

### Layer Dependency Rules

**STRICT**: Layers depend on lower layers only. NO upward dependencies.

```
Presentation → Game → Data → Core (independent)
```

**Allowed**: Presentation uses Game/Data/Core; Game uses Data/Core; Data uses Core
**Forbidden**: Core/Data/Game cannot use upper layers

### File Creation Checklist

When adding a file:

- [ ] Correct folder (appropriate layer)
- [ ] Correct namespace (`CommonsUtility`)
- [ ] Correct suffix (Manager/Controller/Utility/etc.)
- [ ] No upward layer dependencies
- [ ] Brief class responsibility comments

**[詳細ガイド](docs/project-rules/folder-structure.md)** - テストスクリプト管理、層依存例、詳細チェックリスト

---

## Development Workflow

### Document Loading

**MANDATORY**: Load these documents before any merge or editing work:

| Document | Path | Timing | 優先度 |
|----------|------|--------|--------|
| **AGENTS.md** | `AGENTS.md` | Before all work | 最高 |
| **coding-csharp.md** | `docs/project-rules/coding-csharp.md` | Before all work | 最高 |
| **.github/instructions/** | `.github/instructions/` | Before coding work | 最高 |
| Access Modifiers Guide | `docs/project-rules/access-modifiers.md` | Before access modifier decisions | 高 |
| Naming Conventions Guide | `docs/project-rules/naming-conventions.md` | Before naming classes | 高 |
| Folder Structure Guide | `docs/project-rules/folder-structure.md` | Before adding new files | 高 |
| Markdown Style Guide | `docs/project-rules/MARKDOWN-STYLE-GUIDE.md` | When creating documentation | 高 |
| Unity Design Patterns | `docs/project-rules/unity-design-patterns.md` | Before implementing MonoBehaviour | 高 |
| Architecture docs | `docs/architecture/README.md` | Before class design or refactoring | 高 |
| Folder Structure Guide | `docs/project-rules/folder-structure.md` | Before adding new files | 高 |
| Season 3 Roadmap | `docs/vision/roadmap_season3.md` | For policy confirmation | 中 |

### .github/instructions/ フォルダについて

**.github/instructions/ には3つの重要なファイルがあります**：

| ファイル | 内容 | 対象 |
|---------|------|------|
| **unity-csharp-recovery.instructions.md** | Recovery フェーズの C# 実装基準、Null チェック、ブレース要件 | *.cs |
| **plateau-sdk-geospatial.instructions.md** | PLATEAU SDK 使用方法、CityGML 処理、座標変換、3D レンダリング | *.cs |
| **prefab-asset-management.instructions.md** | PrefabManager 使用パターン、アセット管理戦略、キャッシング機構 | *.cs |

**これらは AGENTS.md・docs/project-rules/coding-csharp.md と同等の重要性を持ちます**：
- C# コード作成時は必ず参照してください
- AGENTS.md の基準と組み合わせて使用します
- 実装詳細が AGENTS.md より詳しく記載されている場合があります

**Confirm loading in session message**:
```
**Session**: Continuous
**Context**: AGENTS.md, coding-standards.md, .github/instructions/ (3 files) loaded
```

### Markdown ドキュメント作成時の注意事項

**重要**: Markdown ファイル（.md）を作成・更新する際は、**絵文字を極力使用しないこと**

**理由**:
- Unicode 絵文字（U+1F***など）はサーバー側でエンコーディングエラーを引き起こす可能性が高い
- クライアント・サーバー間の文字化けにより、ファイル更新が 502 エラーで失敗することがある
- 特に CI/CD パイプラインやファイルアップロード処理で問題が発生しやすい

**推奨される記法**:
- [OK] 、 [NOTE] 、 [NG] などの ASCII テキスト表記を使用
- 箇条書きには `-` 、 `*` を使用
- 強調は **太字** や `コード` を活用

**使用を避けるべき絵文字の例**:
- ✅ (U+2705) → [OK] に置き換え
- ❌ (U+274C) → [NG] に置き換え
- ⚠️ (U+26A0+FE0F) → [NOTE] に置き換え
- 📋 (U+1F4CB) → 削除またはテキスト説明で対応
- 📍 (U+1F4CD) → 削除またはテキスト説明で対応

**ファイル更新テンプレート例** (yaml-specification.md より):
```markdown
## イベントタイプ一覧

### [OK] 実装済みイベントタイプ（EventLoader で処理）

| イベント | パラメータ | 状態 |
|---------|----------|------|
| `weather` | weather_type, ... | [OK] |
| `volcano` | - | [NOTE] 未実装 |

**実装上の特性**:
- [OK] 値は文字列として保存
- [NOTE] スポーン設定は未定義
```

### Windows PowerShell Environment

**REQUIRED**: This is a Windows-only project

- [OK] **Allowed**: PowerShell cmdlets
- [NG] **Forbidden**: Linux/macOS bash commands

| Linux/macOS | Windows PowerShell |
|-------------|-------------------|
| `ls -la` | `Get-ChildItem -Force` |
| `grep pattern` | `Select-String "pattern"` |
| `cat file` | `Get-Content file` |
| `find . -name "*.cs"` | `Get-ChildItem -Recurse -Filter "*.cs"` |
| `rm -rf folder` | `Remove-Item -Path folder -Recurse -Force` |

---

## Additional Reference Documentation

### Architecture Documentation

The following detailed architecture documents provide context for system design decisions:

| Document | Path | Purpose |
|----------|------|---------|
| Asset Management | `docs/architecture/asset-management.md` | Resource loading and caching strategies |
| Initialization Flow | `docs/architecture/initialization-flow.md` | Game startup and system initialization sequence |
| PLATEAU Integration | `docs/architecture/plateau-integration.md` | Geospatial data processing and PLATEAU SDK integration |
| UI System | `docs/architecture/ui-system.md` | UI framework and interaction patterns |

### Vision & Roadmap

| Document | Path | Purpose |
|----------|------|---------|
| Season 3 Roadmap | `docs/vision/roadmap_season3.md` | Current roadmap (disaster-prevention simulation MVP) |
| Weekly Schedule | `docs/season3_schedure.md` | Season 3 week-by-week schedule |
| Project Statement | `docs/vision/project-statement.md` | Vision, milestones, KPI |

### Reference Data Models

| Document | Path | Purpose |
|----------|------|---------|
| Data Models | `docs/reference/data-models.md` | Game data structure specifications |
| YAML Format | `docs/reference/yaml-format.md` | YAML configuration and data format specifications |

### Development Tools & Debugging

| Document | Path | Purpose |
|----------|------|---------|
| Debugging and Logging | `docs/project-rules/debugging-and-logging.md` | Unity Editor ログ取得方法、エラー確認、トラブルシューティング手法 |

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

**Last Updated**: 2026-06-10
**Project**: OnoCoro (Unity 6.3 Geospatial Visualization)
