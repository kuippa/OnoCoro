# OnoCoro Agent Skills

このフォルダには GitHub Copilot 用の **Agent Skills** が含まれています。Agent Skills は、指定のタスク向けに bundled resources（スクリプト、参考資料、テンプレート）を提供する self-contained フォルダです。

## 📋 スキル一覧

| スキル | 説明 | コマンド |
|--------|------|---------|
| **documentation-loader** | 必須 md 自動ロード（AGENTS.md, coding-standards.md等） | `/readmd` |
| **microsoft-docs** | Microsoft 公式ドキュメント検索（C#、.NET、Unity 用） | 検索時 |
| **microsoft-code-reference** | Microsoft API リファレンス・コードサンプル検索 | 検索時 |
| **make-skill-template** | 新規 Agent Skill を作成するためのテンプレート | 参考資料 |

## 🚀 使用方法

### スキルを使用する

Copilot Chat で以下のように参照：

```# ドキュメントを自動ロード（推奨：セッション開始時）
/readmd
# Microsoft Docs スキルで C# ドキュメントを検索
C# null-safety best practices について Microsoft Docs から調べてください

# Microsoft Code Reference スキルで API 検証
Transform.Find() の正確な使用方法を示すサンプルコードを探してください

# 新規スキルを作成
Unity Recovery Validator スキルを create します
```

### スキル構成

各スキルは以下のような構造を持ちます：

```
.github/skills/[skill-name]/
├── SKILL.md                    # スキル定義（必須）
├── scripts/                    # (Optional) 自動化スクリプト
│   └── validate-skill.ps1
├── references/                 # (Optional) 参考資料
│   ├── pattern-guide.md
│   └── checklist.md
└── assets/                     # (Optional) テンプレート・サンプル
    └── template.cs
```

## 📚 スキル詳細

### 0. documentation-loader（推奨）

**説明**: OnoCoro の必須ドキュメント（AGENTS.md, coding-standards.md など）を自動読み込み。セッション開始時に自動実行、または `/readmd` コマンドで手動実行。

**用途**:
- セッション開始時に AGENTS.md などを自動読み込み
- ドキュメント変更時に再読み込み
- AI context を常に最新状態に保つ
- Recovery フェーズガイドラインを確実に適用

**OnoCoro カスタマイズ**:
- Mandatory docs: AGENTS.md, coding-standards.md, recovery-workflow.md
- AI-focused: 人間向けドキュメント（instructions.md）は除外
- 自動実行：セッション開始時に自動トリガー
- 手動実行：`/readmd` で常にリロード可能

**使用方法**:

```
# セッション開始時（自動実行）
/readmd

# または明示的に実行
/readmd
```

**参考**: [SKILL.md](documentation-loader/SKILL.md)

---

### 1. microsoft-docs

**説明**: Microsoft 公式ドキュメントを検索し、C#、.NET、Azure、Unity の概念・チュートリアル・設定を取得します。

**用途**:
- C# 言語機能の理解
- Unity API の学習
- PLATEAU SDK ドキュメント検索
- パフォーマンス・ベストプラクティス確認

**OnoCoro カスタマイズ**:
- Recovery フェーズ C# 開発向けのクエリ例
- PLATEAU SDK 統合の質問パターン
- アセット管理のベストプラクティス検索

**参考**: [SKILL.md](microsoft-docs/SKILL.md)

### 2. microsoft-code-reference

**説明**: Microsoft API リファレンス・コードサンプルを検索し、API メソッド・署名・使用例を確認します。

**用途**:
- API メソッド署名検証
- コードサンプル検索
- エラー原因の特定
- deprecated パターンの検出

**OnoCoro カスタマイズ**:
- Recovery フェーズ コード検証用例
- Transform.Find() / GetComponent() パターン検証
- PrefabManager キャッシング戦略検証
- null チェック・リソース管理パターン

**参考**: [SKILL.md](microsoft-code-reference/SKILL.md)

### 3. make-skill-template

**説明**: 新規 Agent Skill を作成するための完全なテンプレート。スキル構成・SKILL.md フォーマット・サポートファイルのガイダンス。

**用途**:
- 新規スキルの作成
- スキル構造のスケーフォールディング
- OnoCoro 特化スキルの開発

**OnoCoro 推奨スキル候補**:
1. `unity-recovery-validator` — Recovery フェーズコード検証
2. `plateau-data-processor` — PLATEAU データ処理・検証
3. `prefab-manager-assistant` — PrefabManager 統合支援

**参考**: [SKILL.md](make-skill-template/SKILL.md)

## 🎯 推奨される使い方

### Recovery フェーズでの使用

```
1. コード修正後
   → `microsoft-code-reference` で API 署名検証
   → `microsoft-docs` で ベストプラクティス確認

2. エラー時
   → `microsoft-code-reference` で エラーパターン検索
   → 適切な例外処理を適用
```

### 新機能開発時

```
1. 仕様決定時
   → `microsoft-docs` で 関連する C#/Unity API 概要確認

2. 実装時
   → `microsoft-code-reference` で コードサンプル検索
   → 基準に準拠したコード作成

3. レビュー時
   → `microsoft-code-reference` で API 検証
   → [AGENTS.md](../../AGENTS.md) との照合
```

## 📝 新規スキルの作成手順

### Step 1: テンプレートを確認

[make-skill-template](make-skill-template/SKILL.md) のガイダンスに従う

### Step 2: スキルディレクトリ作成

```powershell
New-Item -Type Directory -Path ".github/skills/[skill-name]"
New-Item -Type File -Path ".github/skills/[skill-name]/SKILL.md"
```

### Step 3: SKILL.md を作成

```markdown
---
name: [skill-name]
description: [説明 - OnoCoro コンテキスト付き]
compatibility: [互換性情報]
---

# [Skill Name]

[詳細内容]
```

### Step 4: このファイル (README.md) に登録

上記の スキル一覧テーブルに追加

### Step 5: .github/copilot/README.md に参照リンク追加

## � 関連ドキュメント

### AI Agent・Copilot
- **[AGENTS.md](../../AGENTS.md)** - AI Agent ガイドライン（必読）
- **[.github/copilot/README.md](../copilot/README.md)** - Copilot カスタマイズ全体
- **[.github/copilot/prompts/](../copilot/prompts/)** - 13個のカスタムプロンプト
- **[.github/instructions/](../instructions/)** - Copilot カスタム instructions

### コーディング・設計
- **[docs/coding-standards.md](../../docs/coding-standards.md)** - C# 基準
- **[docs/recovery-workflow.md](../../docs/recovery-workflow.md)** - Recovery マージルール
- **[docs/architecture.md](../../docs/architecture.md)** - システム構成
- **[docs/introduction.md](../../docs/introduction.md)** - プロジェクト概要

## 📖 awesome-copilot 参考資料

- **Skills Overview**: https://github.com/github/awesome-copilot/tree/main/skills
- **Skills Documentation**: https://github.com/github/awesome-copilot/blob/main/docs/README.skills.md
- **Agent Skills Spec**: https://agentskills.io/specification

## ✅ チェックリスト

新規スキル作成時:

- [ ] SKILL.md を作成
- [ ] `---` frontmatter に name, description, compatibility を記述
- [ ] 使用例・ベストプラクティスを記載
- [ ] OnoCoro コンテキストを明示
- [ ] scripts/, references/, assets/ フォルダを作成（必要な場合）
- [ ] このファイル (README.md) に登録
- [ ] .github/copilot/README.md に参照リンク追加
- [ ] AGENTS.md との整合性を確認

---

**Last Updated**: 2026-01-20  
**Project**: OnoCoro (Unity 6.3 + PLATEAU SDK)
