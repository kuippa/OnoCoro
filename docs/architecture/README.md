# docs/architecture/ - システム技術設計

OnoCoro プロジェクトの特定機能・システムの詳細設計・実装ガイドです。

---

## ファイル一覧

| ファイル | 対象機能 | 対象者 |
|---------|--------|--------|
| **initialization-flow.md** | InitializationManager・段階的初期化フロー | All Developer |
| **ui-system.md** | Canvas・UICanvasManager・解像度対応 | UI Developer |
| **plateau-integration.md** | PLATEAU SDK・CityGML・座標変換 | GIS Developer |
| **asset-management.md** | PrefabManager・アセットキャッシング | All Developer |


---

## 読む順序

### シスム全体を理解したい場合

1. [initialization-flow.md](initialization-flow.md) - ゲーム起動から Play までの初期化フロー
2. [ui-system.md](ui-system.md) - UI システムの全体像
3. [plateau-integration.md](plateau-integration.md) - GIS データ読み込みの全体像

### 機能実装時

1. 該当する architecture/*.md を読む
2. [project-rules/](../project-rules/) で実装ルール確認
3. 関連する既存ファイルを参考にコーディング

### Recovery フェーズで作業する場合

1. [AGENTS.md](../../AGENTS.md) - Null チェック基準
2. [project-rules/coding-csharp.md](../project-rules/coding-csharp.md) - C# ルール確認
3. [project-rules/naming-conventions.md](../project-rules/naming-conventions.md) - 命名規則確認

---

## チェックリスト

### システム設計レビュー時

- [ ] 該当ファイルで設計パターンを確認
- [ ] 依存関係が 4 層ルールに従っているか
- [ ] エラーハンドリングが明示的か
- [ ] ログ出力が十分か

### 実装時

- [ ] project-rules/ ドキュメント確認済み
- [ ] 該当する architecture/*.md のパターン適用済み
- [ ] Recovery フェーズガイドラインに従っているか
- [ ] テスト・デバッグで動作確認済み

---

**関連資料**:
- [project-rules/](../project-rules/) - コーディング規約・設計パターン
- [vision/introduction.md](../vision/introduction.md) - プロジェクト概要
- [AGENTS.md](../../AGENTS.md) - プロジェクト全体ルール
