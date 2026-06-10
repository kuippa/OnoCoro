# Reference 層 - 仕様・データ定義

reference/ 層は、OnoCoro プロジェクトの **標準仕様・データフォーマット・モデル定義** を文書化します。

[OK] **責務**: 実装の詳細よりも、データ構造・ファイル形式・インターフェース仕様を記載

---

## ファイル一覧

| ファイル | 説明 | 対象者 |
|---------|------|--------|
| **yaml-format.md** | YAML ステージファイルの仕様 | ステージ設計者・開発者 |
| **data-models.md** | ゲームデータモデル定義（struct/class） | プログラマ・データエンジニア |
| **coordinate-systems.md** | 座標系・変換・PLATEAU 統合 | GIS 開発者・エンジニア |

---

## 読出順序

### [学習] 初めて OnoCoro に参加する場合

1. [vision/project-statement.md](../vision/project-statement.md) - プロジェクト概要
2. [project-rules/README.md](../project-rules/README.md) - 実装ルール
3. **[yaml-format.md](yaml-format.md)** - ステージデータの仕組み
4. [architecture/asset-management.md](../architecture/asset-management.md) - リソース管理
5. **[data-models.md](data-models.md)** - ゲームデータ構造

### [開発] YAML ステージを作成する場合

1. **[yaml-format.md](yaml-format.md)** - セクション・プロパティ定義
2. [architecture/plateau-integration.md](../architecture/plateau-integration.md) - マップデータ
3. **実装**: YAML ファイル作成（StreamingAssets/staging/）

### [開発] 新しいゲームエンティティを追加する場合

1. **[data-models.md](data-models.md)** - モデル定義
2. [project-rules/naming-conventions.md](../project-rules/naming-conventions.md) - クラス命名
3. [../../AGENTS.md](../../AGENTS.md) - Null チェック基準
4. **実装**: Assets/Scripts/ に追加

### [GIS] PLATEAU 統合を実装する場合

1. **[coordinate-systems.md](coordinate-systems.md)** - 座標系変換
2. [architecture/plateau-integration.md](../architecture/plateau-integration.md) - PLATEAU SDK 統合
3. [.github/instructions/plateau-sdk-geospatial.instructions.md](../../.github/instructions/plateau-sdk-geospatial.instructions.md) - 実装パターン

---

## チェックリスト

### YAML ステージ作成時

- [ ] **yaml-format.md** を読んだ
- [ ] `stagename`, `stageid`, `stagenotice` が定義されている
- [ ] `stages` セクションに stage 情報がある
- [ ] `itemlists` でタワー・敵が定義されている
- [ ] `pathmakers` でパスマーカーが配置されている
- [ ] `events` でゲーム進行イベントが定義されている
- [ ] `goals` または `gameovers` で勝利・敗北条件が定義されている
- [ ] YAML ファイルが有効な YAML 形式（YamlDotNet で検証）

### ゲームデータモデル追加時

- [ ] **data-models.md** で仕様を定義した
- [ ] `readonly struct` または `internal class` で実装
- [ ] `[SerializeField]` で Unity 互換性を確保
- [ ] バリデーションロジックを実装
- [ ] ドキュメントをこのファイルに追加した

### PLATEAU 統合時

- [ ] **coordinate-systems.md** を読んだ
- [ ] 座標系変換（GeoCoordinate ↔ Unity Vector3）を実装
- [ ] CityGML データ読み込みエラーハンドリング
- [ ] LOD 設定でパフォーマンス確保
- [ ] テスト環境で座標精度を検証

---

## 仕様変更時

reference/ ファイルを更新する際は：

1. **大前提**: プロジェクト目的に符合しているか確認
2. **変更内容**: 破壊的変更か互換性維持か明確化
3. **バージョン**: `ver: X.Y.Z` で変更の大きさを記録
4. **マイグレーション**: 既存データの対応方法を記載

**例**: YAML フォーマット v1.0.0 → v1.1.0 に変更する場合

```yaml
# yaml-format.md の先頭に記載
version: 1.1.0
date_updated: 2026-02-02
breaking_changes:
  - `weather` イベントのパラメータ形式を変更
migration:
  - 既存 YAML ファイルは自動変換スクリプトで対応可能
  - 詳細: [migration-guide.md](migration-guide.md)
```

---

## 関連ドキュメント

- [project-rules/](../project-rules/) - 実装ルール
- [architecture/](../architecture/) - システム設計
- [vision/](../vision/) - プロジェクト概要
- [.github/instructions/](../../.github/instructions/) - SDK・パターン実装ガイド
