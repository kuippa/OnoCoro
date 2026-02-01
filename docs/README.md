# OnoCoro ドキュメンテーション

OnoCoro プロジェクトの公式ドキュメントです。各層に分かれており、必要な情報を素早く見つけられるように構成されています。

---

## 📐 ドキュメント表記規則

すべてのドキュメント作成者は以下を参照してください：

- [MARKDOWN-STYLE-GUIDE.md](project-rules/MARKDOWN-STYLE-GUIDE.md) - Markdown 表記統一ルール

**重要な注意点**:
- [NOTE] 絵文字は使用禁止（エンコーディングエラー防止）
- [NOTE] テキストマーク [OK], [NG], [NOTE], [WARN] を使用
- [NOTE] 見出しは H2 (##) からスタート
- [NOTE] リンクは相対パスのみ

---

## 📋 ドキュメント体系

### [Tasklist] タスク管理層 - 最優先アクセス

実装中のバグ・改善案・機能要望を管理（アルファベット先頭 `_` で最上位）

- [_tasklist/README.md](_tasklist/README.md) - タスク管理層概要
- [_tasklist/bugs.md](_tasklist/bugs.md) - バグ報告・修正追跡
- [_tasklist/fixme.md](_tasklist/fixme.md) - コード内 FIXME 集約
- [_tasklist/backlog.md](_tasklist/backlog.md) - 機能要望・改善案

---

### [Vision] プロジェクト方針層

プロジェクトの目的・方針・ロードマップ

- [introduction.md](vision/introduction.md) - プロジェクト概要・目的・非目的
- [roadmap.md](vision/roadmap.md) - 段階別ロードマップ

### [Coding Rules] 実装ルール層

コーディング規約・設計パターン・命名規則

- [README.md](project-rules/README.md) - プロジェクトルール索引
- [MARKDOWN-STYLE-GUIDE.md](project-rules/MARKDOWN-STYLE-GUIDE.md) - Markdown 表記ルール
- [coding-csharp.md](project-rules/coding-csharp.md) - C# コーディング規約
- [naming-conventions.md](project-rules/naming-conventions.md) - 命名規則・Class Suffixes
- [unity-design-patterns.md](project-rules/unity-design-patterns.md) - Unity 設計パターン
- [folder-structure.md](project-rules/folder-structure.md) - フォルダ構成ルール

### [Architecture] 技術設計層

特定機能・システムの詳細設計

- [README.md](architecture/README.md) - システム設計索引
- [initialization-flow.md](architecture/initialization-flow.md) - 初期化フロー・順序制御
- [ui-system.md](architecture/ui-system.md) - UI System（Canvas Scaler, 解像度対応）
- [plateau-integration.md](architecture/plateau-integration.md) - PLATEAU SDK 統合ガイド
- [asset-management.md](architecture/asset-management.md) - アセット管理・Prefab ローディング
- [recovery-guidelines.md](architecture/recovery-guidelines.md) - Recovery フェーズガイドライン

### [Reference] 参考層

技術リファレンス・データ定義

- [yaml-format.md](reference/yaml-format.md) - YAML ファイル形式仕様
- [data-models.md](reference/data-models.md) - データ構造定義

### [Archive] 非推奨ファイル

古いドキュメント・廃止予定ファイル

- [README.md](archive/README.md) - アーカイブ方針

---

## 🚀 クイックスタート

### 新規参加者向け

1. [introduction.md](vision/introduction.md) でプロジェクト概要を確認
2. [README.md](project-rules/README.md) で実装ルールを学習
3. [folder-structure.md](project-rules/folder-structure.md) でコード配置を理解
4. [initialization-flow.md](architecture/initialization-flow.md) で初期化フローを把握

### AI Agent 向け

1. [AGENTS.md](../AGENTS.md) を確認（最上位ルール）
2. [coding-csharp.md](project-rules/coding-csharp.md) で詳細ルールを確認
3. [naming-conventions.md](project-rules/naming-conventions.md) で命名規則を確認
4. 必要に応じて [architecture/](architecture/) から詳細設計を参照

### 既存コード修正・追加開発向け

1. [folder-structure.md](coding-rules/folder-structure.md) で配置先を決定
2. [coding-csharp.md](coding-rules/coding-csharp.md) で実装規約を確認
3. [naming-conventions.md](coding-rules/naming-conventions.md) で命名を決定
4. 関連機能の [architecture/](architecture/) ドキュメントを参照

---

## 📖 各層の説明

| 層 | 役割 | ファイル数 | 対象者 |
|---|---|---|---|
| **Vision** | プロジェクト方針・ロードマップ | 2 | PM / Lead / 新規参加者 |
| **Coding Rules** | 実装ルール・設計パターン | 5 | Developer / AI Agent |
| **Architecture** | システム設計の詳細 | 6 | Developer（システム作成時） |
| **Reference** | 技術リファレンス | 2 | Developer（必要時） |
| **Archive** | 非推奨・廃止予定 | 可変 | 参考資料 |

---

## 🔍 ドキュメント検索

### テーマ別

**UI 実装**
- [ui-system.md](architecture/ui-system.md) - Canvas Scaler, 解像度対応
- [naming-conventions.md](coding-rules/naming-conventions.md) - Controller 命名

**初期化・セットアップ**
- [initialization-flow.md](architecture/initialization-flow.md) - フェーズ・順序制御

**地理データ統合**
- [plateau-integration.md](architecture/plateau-integration.md) - CityGML, 座標変換

**アセット管理**
- [asset-management.md](architecture/asset-management.md) - Prefab, キャッシュ

**復旧フェーズ**
- [recovery-guidelines.md](architecture/recovery-guidelines.md) - 注意点・グローバル状態

---

## 📝 ドキュメント更新ガイドライン

### ドキュメント追加時

1. 責務に応じた層を選択（Vision / Coding Rules / Architecture / Reference）
2. 該当する README.md に索引を追加
3. ファイル作成時に絵文字を避ける（エンコーディング問題回避）

### ドキュメント廃止時

1. `archive/` フォルダに移動
2. ファイル名に `[archived-YYYY-MM-DD]` プレフィックスを付与
3. 参照元リンクを更新

---

**Last Updated**: 2026-02-01
