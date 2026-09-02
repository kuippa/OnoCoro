# FIXME・TODO コメント集約（Season 3）

**更新日**: 2026-06-10
**対象**: ソースコード内の `// FIXME:` `// TODO:` コメント
**状態**: Season 3 開始によりリセット（Season 2 分は [season2_archive/fixme_season2.md](../season2_archive/fixme_season2.md) を参照）

---

## 次フェーズ送り（2026-09-03 v0.0.25-prototype リリース時に判断）

CityHack 2026 の発表を優先するため、以下は次フェーズ以降へ送った。

### プロジェクト名の表記統一

| 項目 | 現状 | あるべき姿 |
|------|------|-----------|
| Product Name | `Onokoro` → 変更済み | `OnoCoro` |
| 実行ファイル名 | `Onokoro.exe` | `OnoCoro.exe` |
| ログの保存先 | `AppData\LocalLow\Hagurachaya\Onokoro\` | `...\OnoCoro\` |
| Company Name | `Hagurachaya` | 変更しない |

**正式名称は `OnoCoro`。** Product Name はプロジェクト設定で変更済み（2026-09-03）だが、
次のビルドから実行ファイル名とログの保存先が変わる。

[IMPORTANT] 変更が効くのは**次のビルド以降**。以下に波及するので、次回リリース時にまとめて直す。

- `README.md` の実行ファイル名・zip 名
- `RELEASE_NOTES.md` のログ保存先の案内
- `docs/howto/release-build.md` のログ保存先
- 過去のログ（`Hagurachaya\Onokoro\`）は移行されず、新旧が別フォルダに分かれる

### ドキュメント類の整理

CityHack 向けの実装で追加・変更した内容が各所に散っている。
次フェーズでまとめて棚卸しする。

| 対象 | 内容 |
|------|------|
| `README.md` 冒頭 | 「タワーディフェンスゲーム」のまま。Season 3 で防災投資シミュレーションへ方向転換しており、v0.0.25 の内容とも合っていない。看板文なので方針を決めてから変更する |
| `docs/reference/yaml-format.md` | `ocean` / `swell` / `flood` / `auto_end` / `demolition` の各セクションが未記載 |
| `docs/BUILD_ENVIRONMENT.md` | Scene List・Product Name が実際と食い違っている（最終更新 2026-03-08） |
| `docs/cityhack2026/` | 発表後に成果物と作業メモを整理する。残すもの・畳むものを仕分ける |
| `docs/_tasklist/fixme.md`（本ファイル） | Season 3 の記載が 2026-06-10 のまま。CityHack 分を取り込んで更新する |

### 開発用の表示物

| 対象 | 内容 |
|------|------|
| `cvsMemo`（三鷹駅前 / 兼六園） | 作業再開時の備忘録。**意図的に sorting order 999 で最前面**。配布物には載せないので、リリースビルド前に非表示にする手順が必要 |
| `今日はここまで` シーン | 開発配信の終了表示用。ゲーム内容ではない。ビルドに含めるかを整理する |

### 実装の積み残し（CityHack 2026）

| 対象 | 内容 |
|------|------|
| 災害廃棄物の原単位 | 資料にある「焼失」区分（0.23 t/㎡・不燃 99.9%）が未適用。現状は構造別のみで判定 |
| baseline | 消火なしの延焼棟数が未計測。京都舞鶴の `baseline` は暫定値 |
| 猫の移動経路 | 経路マーカーが道に沿っていない |
| 津波・土砂災害 | 舞鶴は想定区域データを含むが対応イベントが未実装 |

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
| （シーン作成手順） | - | 新規マップ作成時に DEM へ NavMesh Surface を追加し忘れるとユニットが配置できない。将来のユーザージェネレーティブなマップ作成に向けて、自動生成または起動時警告の仕組みが必要（2026-06-13 三鷹井の頭５丁目で発生。手順は staging/Scenes作成手順メモ.md 参照） | TODO | 中 |
| CameraController.cs | 全体 | 入り組んでおりコード品質が低くバグの温床になりつつある。リファクタリング予定（下記詳細） | REFACTOR | 中 |
| InfrastructureFactory.cs | _spawnCounter | 施策ユニットの命名 ID を static カウンタでインクリメントしているが、他ユニットは _idx を足す方式で共通規格がない（ユニットごとにバラバラ）。将来、命名 ID を呼び出しクラスが持つ等の共通規格に統一する（ユニークである必要はないが極力ユニークに）。PrefabManager.GetNextUID の活用も検討 | REFACTOR | 低 |
| 防災装置の和名表示 | Hydrant/Cistern | ビルドメニュー等で英名（Hydrant/Cistern）のまま。他タワーも英名なので、UI 抜本変更のタイミングで和名化する（撤去情報ウィンドウは InfrastructureUnit.GetUnitStruct で和名対応済み） | TODO | 低 |
| ログ全体の整理 | 全体 | 1施行で Log 515件・Warning 999+ とログ洪水状態（2026-07-03 ユーザー報告）。TriggerHandler 誤警告と鎮火ログは対処済みだが、既存の高頻度ログ（クリックごとの raycast ログ、スポーン系、UIFontManager 等）が多数残る。LogUtility のログレベル活用を含め、リファクタリングフェーズでまとめて整理する | REFACTOR | 中 |

---

## リファクタリング予定: CameraController.cs

**背景**: Season 3 の一連のカメラ不具合（BUG-S3-004/007/015/017）の修正で、static 状態管理・
モード切替・ズーム・ブレンド制御・各種補正が 1 つの static クラスに積み重なり、入り組んだ状態。
将来のバグの温床になりやすいため、機能が一段落した段階で再設計する。

**現状の問題点**:
- `public static class CameraController` に状態（static フィールド）と振る舞いが密結合
- static 状態が Play 再起動・シーンをまたいで残り、初期化漏れの事故が起きやすい（S3-017 の温床）
- ズームレベル ⇔ カメラ距離/高さの変換、モード判定、優先度設定、Cinemachine ブレンド制御、
  高さ補正（S3-004）、マーカー非表示（S3-007）、ブレンドゲート（S3-015）が 1 メソッド群に混在
- `_zoom_lv` を Initialize でカメラ距離から逆算する箇所と、ResetState で固定値に戻す箇所が二重管理

**リファクタリング方針（案）**:
- MonoBehaviour 化 or 明示的なライフサイクル管理クラスに分離し、状態を確実に初期化する
  （static 残存問題を構造的に解消。SubsystemRegistration 依存をやめる）
- 責務分割: ズーム/モード判定（純粋ロジック）/ Cinemachine 制御（副作用）/ 補正（S3-004/015）を分ける
- ズームレベルを単一の真実とし、カメラ距離からの逆算を廃止
- 各 BUG-S3-004/007/015/017 の修正意図がコメントで散在しているのを設計に落とし込む

**着手タイミング**: ゲームバランス調整・W3 デモ準備が一段落した後（W4 以降）。
UI の抜本再調整（BUG-S3-003 の UI Toolkit 化）と同じ「機能確定後にまとめて」フェーズが適切。
