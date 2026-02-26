# docs/coding-rules/ - コーディングルール・実装ガイドライン

OnoCoro プロジェクトにおける C# コーディング規約、Unity 設計パターン、命名規則を定義します。

---

## [NOTE] 重要

すべての Developer と AI Agent は、実装前に以下を確認してください：

1. [AGENTS.md](../../AGENTS.md) - 最上位ルール（必読）
2. [coding-csharp.md](coding-csharp.md) - C# コーディング規約
3. [naming-conventions.md](naming-conventions.md) - 命名規則
4. [folder-structure.md](folder-structure.md) - ファイル配置ルール

---

## ファイル構成

| ファイル | 責務 | 対象者 |
|---------|------|--------|
| **coding-csharp.md** | C# 実装規約（マジックナンバー禁止、括弧必須など） | All Developer |
| **naming-conventions.md** | Class Suffix、Access Modifier、定数命名 | All Developer |
| **unity-design-patterns.md** | Singleton、MonoBehaviour パターン、層構造 | All Developer |
| **folder-structure.md** | Assets/Scripts 下のフォルダ構成・配置ルール | All Developer |
| **debugging-and-logging.md** | エラー確認、ログ取得方法、トラブルシューティング | All Developer |

---

## クイックリファレンス

## 学習パス

### 新規開発者向け

1. [AGENTS.md](../../AGENTS.md) を読む（プロジェクト全体ルール）
2. [coding-csharp.md](coding-csharp.md) を読む（言語ルール）
3. [naming-conventions.md](naming-conventions.md) を読む（命名ルール）
4. [folder-structure.md](folder-structure.md) を読む（配置ルール）
5. [unity-design-patterns.md](unity-design-patterns.md) を読む（パターン習得）

### ドキュメント作成者向け

1. [MARKDOWN-STYLE-GUIDE.md](MARKDOWN-STYLE-GUIDE.md) を読む（表記ルール）
2. [coding-csharp.md](coding-csharp.md) 内のコード例を参考にする
3. 記号 [OK] [NG] [WARN] [NOTE] を使用する
4. 絵文字を使用しない

## チェックリスト

### [coding-csharp.md](coding-csharp.md) - C# 実装前

- [ ] マジックナンバー・文字列を定数化
- [ ] すべての制御文に {} をつけた
- [ ] 三項演算子 `? :` を使っていない
- [ ] Null 結合演算子 `?.` を使っていない
- [ ] Early Return パターンで平坦化
- [ ] 関数は 40 行以内
- [ ] 変数名は意味のあるものか
- [ ] Debug エイリアスを using で宣言

### [naming-conventions.md](naming-conventions.md) - 命名前

- [ ] Namespace は `CommonsUtility` か
- [ ] Class Suffix（Manager / System / Controller など）の選択が適切か
- [ ] Private フィールドは `_camelCase` か
- [ ] 定数は大文字スネークケースか
- [ ] Boolean は `is` / `has` / `can` / `should` で始まるか
- [ ] メソッドは動詞で始まるか

### [folder-structure.md](folder-structure.md) - ファイル配置前

- [ ] 責務に合った層（Presentation / Game / Data / Core）を選択
- [ ] その層の適切な subfolder を選択
- [ ] 層の下向き依存のみか（逆方向がないか）
- [ ] 新規 subfolder 作成の場合はドキュメント更新

### Access Modifier

- **default**: `internal` （Manager/System/Utility は内部用）
- **public**: Public Interface のみ（安定した API）
- **protected**: 継承拡張ポイント
- **private**: クラス内部用

### フォルダ構成

```
Assets/Scripts/
├── Presentation/  (UI, Input, Cameras)
├── Game/          (Game Logic, Systems, Units)
├── Data/          (Models, Repositories)
└── Core/          (Managers, Utilities, Handlers, Constants)
```

---

## 実装前チェックリスト

実装を開始する前に以下を確認してください：

- [ ] **命名規則を確認**: Class Suffix, Access Modifier は正しいか
- [ ] **フォルダ配置を確認**: Assets/Scripts 下の正しい層に配置するか
- [ ] **マジックナンバーを確認**: すべてを定数化したか
- [ ] **括弧を確認**: すべての if/for/while に {} があるか
- [ ] **Early Return**: 深いネストがないか（最大 2-3 レベル）
- [ ] **関数長**: 40 行以下か
- [ ] **Access Modifier**: internal/public を正しく使い分けたか

---

## 関連リンク

- [AGENTS.md](../../AGENTS.md) - AI Agent ルール（最上位）
- [.github/instructions.md](../../.github/instructions.md) - プロジェクト管理
- [../architecture/](../architecture/) - システム設計の詳細

---

**Last Updated**: 2026-02-01
