# バグ報告・修正追跡（Season 3）

**更新日**: 2026-06-10
**状態**: Season 3 開始によりリセット（Season 2 分は [season2_archive/bugs_season2.md](../season2_archive/bugs_season2.md) を参照）
**担当者**: Team

---

## バグ一覧

| バグID | 内容 | 重大度 | 状態 | 報告日 | 修正予定日 | 備考 |
|--------|------|--------|------|--------|----------|------|
| BUG-S3-001 | 火災が見えない（FireCube が原点に湧く疑い） | 高 | 調査中 | 2026-06-12 | W1 内 | 診断ログ + フォールバック導入済み |
| BUG-S3-002 | 地震で床抜け（プレイヤー・オブジェクトが奈落落下） | 中 | 未修正 | 2026-06-12 | W2 以降 | Season 2 由来の既知挙動 |
| BUG-S3-003 | YearPanel の文字重なり・フォント未参照 | 低 | 保留 | 2026-06-12 | UI 再調整時 | 機能検証優先のため意図的に保留 |

## バグメモ（Season 2 から引き継ぎ）

- Litter がなにかの拍子にゴミをまきちらかさなくなることがある。再現性低い。監視モードの IN/OUT が原因？

---

## バグ詳細

### BUG-S3-001: 火災が見えない場所に湧く

**症状**: 武蔵野堺南木密の年サイクルで Start Year 後、火災が視認できない。
ログ上は FireCube がスポーンしており（年末除去数がスポーン数と一致）、
`random_doom_building` の位置解決が疑われる。

**原因（推定）**: `EventLoader` の `random_doom_building` は倒壊建物リスト
（`PlateauBuildingInteractor._doomedBuildings`）が空の場合に Vector3.zero（原点）を
返すため、マップ外/地中に湧いて見えない。building_break が機能していない可能性も含めて調査中。

**修正（一次対応 2026-06-12）**: 倒壊 0 棟時は `random_position` にフォールバック + 警告ログ。
建物選択時・倒壊指定時の診断ログを追加（`[EventLoader]` / `[BuildingBreak]` プレフィックス）。

**テスト**: 武蔵野堺南木密で Year 1 を 1 回実行し、Console の診断ログで
倒壊棟数・スポーン位置を確認する。

### BUG-S3-002: 地震イベントで床抜けする

**症状**: earthquake イベントが DEM を上下させる際、地面配置オブジェクトや
プレイヤーが追従せず奈落（Naraku）に落ちる。

**原因**: earthquake の実装が DEM の変位のみで、上に載っている Rigidbody や
CharacterController の位置補正を行っていない（Season 2 からの既知挙動）。

**対応方針**: W1 スコープ外。ワークショップデモでは地震の演出時間を短くする・
揺れ幅を抑える等の YAML 側調整で回避可能。恒久対応は W2 以降で検討。

### BUG-S3-003: YearPanel の表示崩れ

**症状**: YearPanelController のラベル・ボタン文字が重なり判読しづらい
（フォント参照・画面サイズへの追従が未調整）。

**対応方針**: ユーザー方針により機能検証を優先し意図的に保留
（[../notes_season3.md](../notes_season3.md) 参照）。表示項目が確定した段階で
既存 UI 共通基盤（GamePrefabs/GameInterface 配下）への統合と UI Toolkit 等への
抜本差し替えをまとめて行う。
