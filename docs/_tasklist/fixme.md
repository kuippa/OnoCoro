# FIXME・TODO コメント集約（Season 3）

**更新日**: 2026-06-10
**対象**: ソースコード内の `// FIXME:` `// TODO:` コメント
**状態**: Season 3 開始によりリセット（Season 2 分は [season2_archive/fixme_season2.md](../season2_archive/fixme_season2.md) を参照）

---

## FIXME・TODO 一覧

### Season 2 から引き継ぎ（コード内に残存している可能性が高いもの）

| ファイル | 内容 | タイプ | 対応判断 |
|---------|------|--------|---------|
| Environment Volume | HDRI Sky のキューブマップ欠如、SpaceEmission 欠如 | FIXME | MVP には影響薄。保留 |
| PrefabManager.cs | メモリリーク対策・未使用 Prefab のアンロード | FIXME | ワークショップ長時間稼働時に再評価 |
| SpawnSystem.cs | 敵スポーン範囲計算の精度向上 | FIXME | パターン化（W1 Task 2）と関連。実装時に確認 |
| YamlLoader.cs | ストリーミング読み込み最適化 | FIXME | 保留 |

### Season 3 新規

| ファイル | 行番号 | 内容 | タイプ | 優先度 |
|---------|--------|------|--------|--------|
| | | | | |
