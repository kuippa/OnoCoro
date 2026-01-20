---
agent: 'copilot'
description: 'OnoCoro LLMs.txt ファイル生成 - AI アシスタント向けコンテキスト'
model: 'gpt-4'
tools: []
---

# OnoCoro LLMs.txt ファイル生成

You are a technical information architect optimizing AI model context.

## Your Role

Generate comprehensive llms.txt file that provides AI assistants with complete OnoCoro project context:
- プロジェクト概要
- ドキュメント構造
- コーディング規約
- 技術スタック

## llms.txt Template

```markdown
# OnoCoro: 地理情報ゲーム（Unity 6.3 + PLATEAU SDK）

## Project Summary

OnoCoro は SSD 故障からの復旧フェーズにある地理情報可視化ゲームプロジェクトです。

**Status**: Recovery & Development Phase
**Last Updated**: 2026-01-20
**Repository**: https://github.com/kuippa/OnoCoro

## Quick Stats

- **Language**: C#
- **Engine**: Unity 6.3
- **Scripts**: 393 個（復旧済み）
- **Storage**: 22.38 GB
- **Team**: [メンバー]

## Core Technologies

### Fixed (Cannot Change)
- **Unity**: 6.3
- **PLATEAU SDK**: Latest
- **Cinemachine**: Unity Standard
- **glTFast**: Unity Standard
- **Input System**: Unity Standard

### Development Tools
- **Git Workflow**: nvie Git Flow
- **Copilot**: GitHub Copilot with custom instructions
- **CI/CD**: GitHub Actions (planned)

## Essential Documentation

### Quick Reference
1. **[AGENTS.md](./AGENTS.md)** - AI Agent ガイドライン（必読）
2. **[docs/coding-standards.md](./docs/coding-standards.md)** - C# 実装規約
3. **[docs/architecture.md](./docs/architecture.md)** - システムアーキテクチャ
4. **[.github/instructions.md](./.github/instructions.md)** - 開発ガイド
5. **[docs/recovery-workflow.md](./docs/recovery-workflow.md)** - Recovery マージ規則

### Detailed Documentation
- **[docs/introduction.md](./docs/introduction.md)** - プロジェクト目的
- **[docs/recovery-workflow.md](./docs/recovery-workflow.md)** - Recovery フェーズガイド

## Key Architecture Components

### Game Systems
- **Tower Defense**: Enemy/Tower/State Management
- **UI**: Panel, Button, Popup systems
- **Utilities**: PrefabManager, UIHelper, FileOperationUtility

### PLATEAU Integration
- CityGML ロード
- 地理座標↔ゲーム座標変換
- LOD 管理

### Data Structure
```
Assets/
├── Scripts/
│   ├── Game/
│   ├── PLATEAU/
│   ├── UI/
│   └── Utility/
├── Prefabs/
├── Resources/
└── StreamingAssets/
```

## Coding Standards Summary

### Must Follow
- ✅ No magic numbers/strings → Use constants
- ✅ Required braces for all control statements
- ✅ No ternary (? :) or null-coalescing (?.) operators
- ✅ Early return pattern (guard clauses)
- ✅ Max 40 lines per function
- ✅ Meaningful variable names
- ✅ UnityEngine.Debug explicit: \`using Debug = UnityEngine.Debug;\`

### Recovery Phase Rules
- ✅ Preserve variable initialization (even defaults)
- ✅ Use \`this.gameObject\` not \`base\`
- ✅ Keep existing comments
- ✅ Only merge for functional improvement (skip style-only changes)

## Development Workflow

### Branch Strategy
- **main**: Stable release
- **develop**: Active development
- **feature/***: New features
- **bugfix/***: Bug fixes

### Git Commit Format
\`\`\`
<type>(<scope>): <subject>

<body>

<footer>
\`\`\`

**Types**: feat, fix, docs, style, refactor, perf, test, chore, recovery

## Common Tasks

### Creating a New Feature
1. \`git checkout develop\`
2. \`git pull origin develop\`
3. \`git checkout -b feature/[scope]-[name]\`
4. Implement with AGENTS.md & coding-standards.md compliance
5. Commit with Conventional Commits
6. Create Pull Request with description

### Reviewing Code
Use prompt: \`/review-and-refactor\`
- Check AGENTS.md compliance
- Verify null checks
- Ensure constants over magic numbers
- Validate function length

### Debugging
Key tools:
- \`UnityEngine.Debug.Log()\` - Logging
- RainDropsCtrl - null check example pattern
- null validation before component access

## File Patterns

| Pattern | Purpose |
|---------|---------|
| \`**/*.cs\` | C# Scripts |
| \`**/*.unity\` | Scene files |
| \`Assets/Prefabs/**\` | Prefab templates |
| \`Assets/Resources/**\` | Runtime loaded assets |
| \`.github/copilot/prompts/**\` | Custom Copilot prompts |

## Important Links

- 📖 [PLATEAU Documentation](https://www.mlit.go.jp/plateau/)
- 🎮 [Unity 6.3 Documentation](https://docs.unity3d.com/)
- 📊 [Project Issues](https://github.com/kuippa/OnoCoro/issues)
- 💬 [Discussions](https://github.com/kuippa/OnoCoro/discussions)

## Contact & Support

- **Issues**: GitHub Issues
- **Documentation**: See /docs folder
- **Code Review**: GitHub Pull Requests

---

**This file was generated for AI model context optimization using llms.txt specification.**
**Reference**: https://llmstxt.org/
```

## Context

- **Project**: OnoCoro
- **Purpose**: AI アシスタント向けコンテキスト最適化
- **Specification**: https://llmstxt.org/
