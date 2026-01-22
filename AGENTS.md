# AGENTS.md - OnoCoro Project Agent Guidelines

This file defines the essential rules and guidelines that GitHub Copilot and AI Agents must follow when working on the OnoCoro project.

---

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Session Information Requirements](#session-information-requirements)
- [Technology Stack](#technology-stack)
- [Coding Standards](#coding-standards)
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

## Development Workflow

### Document Loading

**MANDATORY**: Load these documents before any merge or editing work:

| Document | Path | Timing |
|----------|------|--------|
| AGENTS.md (this file) | `AGENTS.md` | Before all merge/edit work |
| coding-standards.md | `docs/coding-standards.md` | Before all merge/edit work |
| architecture.md | `docs/architecture.md` | Before new class design or major refactoring |
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
