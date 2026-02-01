# x_archive 層 - 非推奨・統合済みドキュメント（ゴミ箱）

**目的**: 古いドキュメント・統合済みファイルを保存  
**配置**: docs/x_archive/ （アルファベット `x_` で最下位、低優先度）  
**更新日**: 2026-02-02  
**状態**: Phase 1 ドキュメント層の統合により、8 個のファイルをアーカイブ化

---

## アーカイブ対象ファイル（統合済み）

| アーカイブファイル | 統合先 | 理由 | アーカイブ日 |
|-----------------|------|------|-----------|
| **archived-architecture-2026-02-02.md** | [docs/architecture/](README.md) | アーキテクチャ層を organization 単位で再構成（初期化フロー、UI、PLATEAU 等） | 2026-02-02 |
| **archived-coding-standards-2026-02-02.md** | [docs/project-rules/coding-csharp.md](../project-rules/coding-csharp.md) | C# コーディング規約を project-rules に統合 | 2026-02-02 |
| **archived-introduction-2026-02-02.md** | [docs/vision/introduction.md](../vision/introduction.md) | プロジェクト紹介を vision 層に統合・改訂 | 2026-02-02 |
| **archived-yaml-specification-2026-02-02.md** | [docs/reference/yaml-format.md](../reference/yaml-format.md) | YAML 仕様を reference 層（データ仕様）に統合 | 2026-02-02 |
| **archived-ui-improvement-phase-1-4-2026-02-02.md** | [docs/architecture/ui-system.md](../architecture/ui-system.md) | UI システム設計を architecture 層に統合 | 2026-02-02 |
| **archived-ui-initialization-reference-2026-02-02.md** | [docs/architecture/initialization-flow.md](../architecture/initialization-flow.md) | 初期化フロー仕様を architecture 層に統合 | 2026-02-02 |
| **archived-scripts-folder-structure-completed-2026-02-02.md** | [docs/project-rules/folder-structure.md](../project-rules/folder-structure.md) | フォルダ構成仕様を project-rules に統合 | 2026-02-02 |
| **archived-prototype-phase-roadmap-2026-02-02.md** | [docs/vision/roadmap.md](../vision/roadmap.md) | Prototype Phase ロードマップを vision 層に統合 | 2026-02-02 |

---

## ドキュメント層の組織再編（概要）

### 旧構造（docs/ ルート直置き）

```
docs/
├─ architecture.md              (単一ファイル・概要のみ)
├─ coding-standards.md          (長大・保守困難)
├─ introduction.md              (古いフォーマット)
├─ yaml-specification.md        (仕様のみ・理論的背景なし)
└─ ...（7 個の md ファイル）
```

**問題点**:
- [NG] ファイルが多く、目的ごとに分散
- [NG] 責務が曖昧（architecture.md が何を定義しているか不明確）
- [NG] 更新時に関連ファイル複数を修正必要

### 新構造（責務ベース層分割）

```
docs/
├─ vision/                      [OK] プロジェクト目的・ロードマップ（3 ファイル）
├─ project-rules/               [OK] 実装ルール・標準（6 ファイル）
├─ architecture/                [OK] システム設計・詳細設計（6 ファイル）
├─ reference/                   [OK] 仕様・データモデル定義（3 ファイル）
└─ archive/                     [OK] 非推奨・統合済みファイル
```

---

## 参照方法

### アーカイブファイルの参照

**新規参加者・開発者向け**:
- [NG] アーカイブファイルを直接参照しない
- [OK] 新構造の対応ファイルを参照

```markdown
[NG] 古い: docs/architecture.md を読む
[OK] 新しい: docs/architecture/README.md から始める
           → initialization-flow.md
           → ui-system.md
           → plateau-integration.md
```

**参考資料として確認したい場合**:
- [OK] archive/ のファイルはいつでも参照可能
- [OK] git log で旧バージョンも確認可能

---

## アーカイブの役割

### 設計思想

x_archive は以下の特性を持ちます：

- **ゴミ箱的な扱い**: 不要になったドキュメントの一時保管
- **アルファベット最下位**: フォルダ名 `x_` で意図的に最下位に配置
- **低優先度**: アクセス頻度は極めて低い
- **参考資料**: 必要に応じて参照可能な知識ベース

---

## 検索方法

Git に履歴が残っているため、古いドキュメントが必要な場合：

```powershell
# ログで旧ドキュメント履歴を確認
git log --oneline -- docs/archive/

# 旧ドキュメントの内容を表示
git show HEAD~N:docs/archive/filename.md
```

---

## 方針

- **保管期間**: 次の大型リファクタリングまで（1-2 フェーズ）
- **削除予定**: Phase 3 以降の判断で削除可能
- **Git 履歴**: すべて Git に記録されているため、必要時は復元可能
- **命名規則**: `archived-<filename>-YYYY-MM-DD.md`

---

**関連ドキュメント**:
- [docs/README.md](../README.md) - ドキュメント層全体インデックス
- [docs/vision/](../vision/) - プロジェクト目的・ロードマップ（新）
- [docs/project-rules/](../project-rules/) - 実装ルール（新）
- [docs/architecture/](../architecture/) - システム設計（新）
- [docs/reference/](../reference/) - データ仕様（新）

**作成日**: 2026-02-02  
**最終更新**: 2026-02-02
