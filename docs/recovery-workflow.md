# Recovery Phase Workflow

このファイルは `docs/project-rules/coding-csharp.md` および関連 Recovery フェーズ資料への互換エイリアスです。

Recovery フェーズの作業では、以下のドキュメントを参照してください。

- [docs/project-rules/coding-csharp.md](project-rules/coding-csharp.md)
- [docs/project-rules/access-modifiers.md](project-rules/access-modifiers.md)
- [docs/project-rules/folder-structure.md](project-rules/folder-structure.md)

## Recovery フェーズの重要ガイド

- 既存の UI/Assets を壊さないように変更を最小化する
- null チェックと防御的プログラミングを徹底する
- `internal` をデフォルトとし、`public` は公開インターフェースのみで使う
- `AGENTS.md` の指示に従う
- PLATEAU SDK でのデータ処理ではメモリ・読み込み順序に注意する
