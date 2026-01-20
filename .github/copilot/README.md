# OnoCoro Copilot Customization

このフォルダには OnoCoro プロジェクト向けの GitHub Copilot カスタマイズが含まれています。

## 📁 フォルダ構成

```
.github/
├── agents/               # 専門エージェント（委任用）
│   ├── planner.md                    # 機能実装計画
│   ├── code-reviewer.md              # コード品質レビュー
│   ├── recovery-validator.md         # Recovery フェーズ検証
│   ├── plateau-specialist.md         # PLATEAU SDK 専門家
│   └── README.md
├── commands/             # Slash commands（統合版）
│   ├── README.md         # コマンド概要
│   └── [個別コマンド定義]
├── copilot/
│   ├── prompts/          # カスタム Copilot プロンプト（アーカイブ）
│   └── README.md         # このファイル
├── instructions/         # Copilot カスタム instructions
│   ├── unity-csharp-recovery.instructions.md
│   ├── prefab-asset-management.instructions.md
│   ├── plateau-sdk-geospatial.instructions.md
│   └── [Phase 2+] ...
├── skills/               # Agent Skills (bundled resources)
│   ├── documentation-loader/
│   ├── microsoft-docs/
│   ├── microsoft-code-reference/
│   ├── make-skill-template/
│   └── README.md
└── instructions.md       # グローバル開発ガイド
```

## 🤖 Agents（専門エージェント）

### 優先度 A: 必須（everything-claude-code から採用）

これらのエージェントに**委任**することで、特化した判断が得られます。詳細は [.github/agents/README.md](../agents/README.md) を参照。

| Agent | 説明 | コマンド |
|-------|------|---------|
| `planner.md` | 機能要件を実装計画に分解 | `/planner` |
| `code-reviewer.md` | AGENTS.md 規約の厳密なチェック | `/code-reviewer` |
| `recovery-validator.md` | Recovery フェーズ防御的プログラミング検証 | `/recovery-validator` |
| `plateau-specialist.md` | PLATEAU SDK・座標変換・メモリ効率検証 | `/plateau-specialist` |

**使用例**:
```
/planner
Feature: Implement puddle physics system

→ 実装フェーズ・受け入れ基準・依存関係を自動分析
```

**推奨フロー**:
```
1. /planner で計画
2. 実装
3. /code-reviewer でコード品質チェック
4. /recovery-validator で Recovery 対応確認
5. PLATEAU 関連なら /plateau-specialist でチェック
```

---

## 🎯 プロンプト一覧（従来のコマンド）

### 優先度 A: 必須推奨

これらのプロンプトは Recovery フェーズで即座に活用できます。

| プロンプト | 用途 |
|-----------|------|
| `csharp-async-best-practices.prompt.md` | 非同期処理の標準化（PLATEAU SDK 統合） |
| `csharp-documentation-best-practices.prompt.md` | XML ドキュメント生成 |
| `create-implementation-plan.prompt.md` | 機能実装計画（Recovery フェーズ向け） |
| `conventional-commit.prompt.md` | Git コミットメッセージ規格化 |

### 優先度 B: 強く推奨

プロジェクト設計と品質管理に活用します。

| プロンプト | 用途 |
|-----------|------|
| `create-specification.prompt.md` | 仕様書テンプレート |
| `create-technical-spike.prompt.md` | 技術検証ドキュメント |
| `review-and-refactor.prompt.md` | コードレビュー支援 |
| `create-architectural-decision-record.prompt.md` | ADR 生成 |

### 優先度 C: 推奨

ドキュメント作成と体系化に活用します。

| プロンプト | 用途 |
|-----------|------|
| `git-flow-branch-creator.prompt.md` | Git Flow ブランチ作成 |
| `project-folder-structure-blueprint.prompt.md` | フォルダ構造ドキュメント |
| `create-readme.prompt.md` | README 自動生成 |
| `repo-story-time.prompt.md` | コミット履歴の物語化 |
| `create-llms-txt.prompt.md` | AI コンテキスト生成 |

---

## 🛠️ Agent Skills 一覧

### 優先度 A: 必須

これらの Skills は即座に活用できます。詳細は [.github/skills/README.md](../skills/README.md) を参照。

| Skill | 用途 | コマンド |
|-------|------|--------|
| `documentation-loader` | 必須ドキュメント自動ロード（AGENTS.md など） | `/readmd` |
| `microsoft-docs` | Microsoft 公式ドキュメント検索（C#、.NET、Unity） | 検索時 |
| `microsoft-code-reference` | Microsoft API リファレンス・コードサンプル検索 | 検索時 |
| `make-skill-template` | 新規 Agent Skill テンプレート | 参考 |

**OnoCoro カスタマイズ**: 
- `documentation-loader`: Recovery フェーズガイドライン常時読み込み
- その他 Skills: Recovery フェーズ C# 開発、PLATEAU SDK 統合、PrefabManager パターン向けにカスタマイズ

## 🚀 使用方法

### VS Code で Copilot Chat を使用

**ドキュメント読み込み（推奨：最初に実行）:**
```
/readmd
```
→ AGENTS.md, coding-standards.md など必須ドキュメント自動読み込み

**プロンプト実行例:**

1. **プロンプトを実行**:
   ```
   /csharp-async-best-practices
   ```

2. **コードを指定**:
   ```
   /review-and-refactor

   [ここにコードを貼り付け]
   ```

3. **計画を作成**:
   ```
   /create-implementation-plan

   機能: PLATEAU SDK CityGML ローダー
   ```

### Copilot Chat のコマンド例

```bash
# 📚 ドキュメントコンテキストを読み込み（最初に実行推奨）
/readmd

# 非同期処理のベストプラクティスを確認
/csharp-async-best-practices [コードスニペット]

# コードレビューを実施
/review-and-refactor [ファイルパス]

# 実装計画を立案
/create-implementation-plan 新機能: [機能名]

# 技術検証を記録
/create-technical-spike [トピック]

# ADR を生成
/create-architectural-decision-record [決定内容]

# コミットメッセージを生成
/conventional-commit 変更内容: [説明]

# ブランチを作成
/git-flow-branch-creator feature/[機能名]
```

## 📚 関連ドキュメント

- **[AGENTS.md](../../AGENTS.md)** - AI Agent ガイドライン（必読）
- **[docs/coding-standards.md](../../docs/coding-standards.md)** - C# 実装規約
- **[docs/recovery-workflow.md](../../docs/recovery-workflow.md)** - Recovery マージ規則
- **[.github/instructions/](../instructions/)** - Copilot カスタム instructions
  - `unity-csharp-recovery.instructions.md` - Recovery フェーズ C# 開発
  - `prefab-asset-management.instructions.md` - アセット管理パターン
  - `plateau-sdk-geospatial.instructions.md` - PLATEAU SDK 統合ガイド
- **[.github/skills/](../skills/)** - Agent Skills
  - `microsoft-docs/SKILL.md` - Microsoft ドキュメント検索
  - `microsoft-code-reference/SKILL.md` - API リファレンス・サンプル
  - `make-skill-template/SKILL.md` - 新規スキル作成ガイド
- **[.github/instructions.md](../instructions.md)** - 開発ガイド（人向け）

## 🔧 セットアップ

### VS Code 設定

`.vscode/settings.json` に以下を追加：

```json
{
  "github.copilot.enable": {
    "*": true
  }
}
```

### カスタム指示の有効化

Copilot カスタム instructions は自動的に適用されます：

1. `.github/instructions/` 内の `.instructions.md` ファイルを Copilot が自動検出
2. ファイルタイプ（C#、PLATEAU SDK など）に応じて自動適用
3. グローバル指示（`.github/copilot-instructions.md`）と統合

## 💡 使用シナリオ

### Recovery フェーズでのコード復旧

```
1. `review-and-refactor` でコード品質を検証
2. `csharp-async-best-practices` で非同期処理を確認
3. `conventional-commit` でコミット
```

### 新機能開発

```
1. `create-specification` で仕様を作成
2. `create-implementation-plan` で計画を立案
3. `create-technical-spike` で技術検証（必要時）
4. `review-and-refactor` で実装をレビュー
5. `conventional-commit` でコミット
```

### ドキュメント生成

```
1. `create-readme` で README を生成
2. `project-folder-structure-blueprint` でフォルダ構造を記述
3. `create-llms-txt` で AI コンテキストを生成
```

## ✅ チェックリスト

新規開発時の確認項目：

- [ ] [AGENTS.md](../../AGENTS.md) を読んだ
- [ ] [docs/coding-standards.md](../../docs/coding-standards.md) を確認
- [ ] 適切なプロンプトを選択
- [ ] コードが基準に準拠している
- [ ] テストが通っている
- [ ] コミットメッセージが Conventional Commits に準拠

## 📝 プロンプト作成ガイド
50 x 8
既存の instructions.md を確認しました。それでは、優先度 A～C のプロンプトを .github/copilot/ に導入します。

次に、優先度 A のプロンプトを作成します。

次に、優先度 B のプロンプトを作成します。

次に、優先度 C のプロンプトを作成します。

次に、copilot ディレクトリに README を作成し、最後にまとめのドキュメントを作成します。

README.md が作成されました

最後に、instructions.md にこの Copilot カスタマイズについてのリンクを追加します。

instructions.md の 1 から 20 行を読み取り

Edited
instructions.md+0-0
完了しました！OnoCoro プロジェクトに GitHub Copilot カスタマイズを導入しました。

📊 実施内容
導入構成
各プロンプトの特徴
優先度 A（必須）

Recovery フェーズですぐに活用
非同期処理、ドキュメント、計画、コミット規格化
優先度 B（強く推奨）

プロジェクト設計と品質管理に特化
仕様書、技術検証、コードレビュー、ADR
優先度 C（推奨）

ドキュメント体系化
Git Flow、フォルダ構造、README、履歴、AI コンテキスト
プロンプト内容
各プロンプトは：

OnoCoro 専用にカスタマイズ
PLATEAU SDK 統合を考慮


新しいプロンプトを追加する場合：

```yaml
---
agent: 'copilot'
description: '[簡潔な説明]'
model: 'gpt-4'  # または gpt-3.5-turbo
tools: []
---

# [プロンプト名]

[詳細な指示と例]
```

## 🤝 フィードバック

プロンプトの改善提案は GitHub Issues で：
- タイトル: `[Copilot] [プロンプト名]: [提案内容]`
- 説明: 現在の問題点と改善案

## 参考資料

- [awesome-copilot - Prompts](https://github.com/github/awesome-copilot/tree/main/prompts)
- [GitHub Copilot Chat Documentation](https://code.visualstudio.com/docs/copilot/chat)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [llms.txt Specification](https://llmstxt.org/)

---

**Last Updated**: 2026-01-20
**Project**: OnoCoro (Unity 6.3 + PLATEAU SDK)
