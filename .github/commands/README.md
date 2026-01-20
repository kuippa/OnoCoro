# OnoCoro Commands

このフォルダには **Slash Commands** が含まれています。Copilot Chat で即座に実行できる操作を提供します。

## 📋 コマンド一覧

### 🔧 基本コマンド

| コマンド | 説明 | 用途 | ソース |
|--------|------|------|--------|
| `/readmd` | ドキュメント自動読み込み | セッション開始時 | Skill |
| `/plan` | 機能実装計画策定 | 新機能企画時 | Prompt |
| `/code-review` | コード品質レビュー | PR レビュー前 | Prompt |
| `/create-implementation-plan` | 実装計画書生成 | 機能分割時 | Prompt |

### 📚 ドキュメント関連

| コマンド | 説明 | 用途 |
|--------|------|------|
| `/create-readme` | README 自動生成 | ドキュメント作成 |
| `/create-llms-txt` | AI コンテキスト生成 | コンテキスト化 |
| `/update-docs` | ドキュメント同期 | ドキュメント更新 |

### 🏗️ 設計・アーキテクチャ

| コマンド | 説明 | 用途 |
|--------|------|------|
| `/create-specification` | 仕様書テンプレート | 仕様定義 |
| `/create-architectural-decision-record` | ADR 生成 | 設計決定記録 |
| `/create-technical-spike` | 技術検証ドキュメント | 技術検証 |

### 🔀 Git・ワークフロー

| コマンド | 説明 | 用途 |
|--------|------|------|
| `/conventional-commit` | コミットメッセージ生成 | Git コミット |
| `/git-flow-branch-creator` | Git Flow ブランチ作成 | ブランチ管理 |
| `/repo-story-time` | コミット履歴の物語化 | 変更履歴の要約 |

---

## 🚀 クイック使用例

### 例 1: セッション開始時

```
/readmd
```

→ AGENTS.md, coding-standards.md など必須ドキュメント自動読み込み

### 例 2: 新機能計画

```
/plan

Feature: Implement PLATEAU SDK city loading system
Requirements:
- Load CityGML files
- Transform coordinates to Unity space
- Optimize for large datasets
Constraints: Recovery phase defensive programming required
```

→ 実装計画書自動生成

### 例 3: コードレビュー

```
/code-review

File: Assets/Scripts/RainDropsCtrl.cs

[コードを貼り付けまたはファイル指定]

Focus: Null safety, PrefabManager usage, Recovery phase compliance
```

→ 違反・改善提案リスト

### 例 4: コミットメッセージ生成

```
/conventional-commit

Changes:
- Added null check to RainDropsCtrl.ChangeColliderSize()
- Integrated PrefabManager for puddle prefab loading
- Removed Resources.Load() direct call
```

→ Conventional Commits 形式で生成

---

## 📋 コマンドの分類

### パフォーマンスに応じた実行順序

#### 🟢 軽量コマンド（すぐ実行可能）

```
/readmd          → ドキュメント読み込み
/conventional-commit → コミットメッセージ生成
/repo-story-time → 変更履歴要約
```

#### 🟡 中量コマンド（5-10分）

```
/code-review     → コード品質チェック
/plan            → 機能計画書
/create-specification → 仕様書生成
```

#### 🔴 重量コマンド（10-30分）

```
/create-implementation-plan → 詳細実装計画
/create-architectural-decision-record → ADR 生成
/create-technical-spike → 技術検証報告書
```

---

## 🎯 ワークフロー別コマンドシーケンス

### 新機能開発ワークフロー

```
1. /readmd
   セッションコンテキスト初期化

2. /plan
   Feature: [機能名]
   実装計画書自動生成

3. 実装作業
   コード実装

4. /code-review
   File: [実装ファイル]
   品質チェック

5. /conventional-commit
   Changes: [変更内容]
   コミットメッセージ生成

6. Git push
```

### コード改善ワークフロー

```
1. /code-review
   File: [修正対象]
   品質チェック

2. 修正実装

3. /code-review
   File: [修正後]
   再チェック（OK 確認）

4. /conventional-commit
   Changes: [改善内容]
   コミットメッセージ生成
```

### ドキュメント更新ワークフロー

```
1. /update-docs
   Files: [更新対象]
   ドキュメント同期

2. /create-readme
   Project: [プロジェクト名]
   README 生成

3. /create-llms-txt
   Project: [プロジェクト名]
   AI コンテキスト化
```

---

## 🔗 関連ドキュメント

### コマンド定義ファイル
- 各コマンドの詳細は個別の `.md` ファイルを参照
- 例: `plan.md`, `code-review.md` など

### 背景・ガイド
- [.github/copilot/README.md](../copilot/README.md) - Copilot カスタマイズ概要
- [.github/agents/README.md](../agents/README.md) - Agent 委任ガイド
- [.github/skills/README.md](../skills/README.md) - Skill 説明

### プロジェクトガイド
- [AGENTS.md](../../AGENTS.md) - プロジェクト全体ルール
- [docs/coding-standards.md](../../docs/coding-standards.md) - C# 基準

---

## 💡 ベストプラクティス

### コマンド実行のコツ

1. **セッション開始時は `/readmd` から**
   ```
   /readmd
   ```
   常にドキュメントコンテキストを初期化

2. **具体的な指示で精度向上**
   ```
   ❌ /plan Feature: New system
   ✅ /plan
      Feature: Implement puddle physics
      Requirements:
      - Gravity simulation
      - PrefabManager integration
      Duration estimate: 5 days
   ```

3. **コマンドの組み合わせ**
   ```
   実装 → /code-review → 修正 → /conventional-commit → Push
   ```

4. **エラーが出たら再実行**
   ```
   コマンド実行エラー → コンテキスト確認 → /readmd 再実行 → リトライ
   ```

---

## 📊 コマンド対 Agent 使い分け

| 用途 | コマンド | Agent |
|------|--------|-------|
| 計画・分割 | `/plan` | `/planner` |
| コード品質レビュー | `/code-review` | `/code-reviewer` |
| Recovery フェーズチェック | `/code-review` | `/recovery-validator` |
| PLATEAU SDK チェック | `/code-review` | `/plateau-specialist` |
| Git コミット | `/conventional-commit` | 不要 |
| ドキュメント生成 | `/create-readme` | 不要 |

---

## 🔄 コマンド追加手順

新しいコマンドを追加する場合：

1. `.github/commands/` フォルダに `.md` ファイル作成
   ```
   new-command.md
   ```

2. Prompt フォーマットに従う：
   ```markdown
   ---
   name: new-command
   description: [説明]
   ---
   
   # [コマンド名]
   [詳細説明]
   ```

3. このファイル (README.md) に追加
4. `.github/copilot/README.md` に記載

---

## 🎓 例：カスタムコマンド作成

OnoCoro 特化コマンドの例：

```markdown
---
name: recovery-check
description: Recovery phase code validation
---

# /recovery-check

Validate code for Recovery phase readiness.

Usage:
/recovery-check
File: [path]
Focus: [null safety, error handling, etc.]
```

---

**Last Updated**: 2026-01-20  
**Command Version**: 1.0 (everything-claude-code adapted for OnoCoro)
