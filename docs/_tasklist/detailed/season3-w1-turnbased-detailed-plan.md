# Season 3 Week 1 詳細計画 - ターンベース化（年サイクル骨格）

**作成日**: 2026-06-10
**更新日**: 2026-06-10（スポーンパターンのパッケージ化を反映）
**対象期間**: 2026-06-10 ～ 2026-06-14（W1 残り）
**工数見積**: 約 5 時間
**前提**: 本計画は [../../season3_schedure.md](../../season3_schedure.md) の旧 Week 1 タスクを置き換える

---

## 設計判断の記録（ヒアリング結果）

| 論点 | 決定 | 理由 |
|------|------|------|
| Year の中身 | 既存のタイムライン駆動（EventLoader）を維持し、年の開始をユーザートリガー（ボタン押下）に変更 | 既存資産を最大限流用。火災延焼のリアルタイム表現はワークショップの見せ場 |
| Year 定義の置き場所 | 既存 YAML を拡張し `years` 区切りを追加 | 非プログラマでも会場ごとにシナリオ差し替え可能 |
| 年の終了条件 | 固定時間（duration を YAML で年ごとに指定、目安 60-90 秒） | 進行が読みやすく実装が最単純。既存 countdown 機構を流用 |
| 状態の引き継ぎ | 配置済み施策（タワー）のみ Year 2 以降へ引き継ぐ | 「積み上げ投資」の実感。被害結果・予算繰越は MVP 対象外 |
| プレイ規模 | 3 年 x 約 5 分（議論込み）、合計 15-20 分 | ワークショップ 1 セッションの定番尺 |
| W1 スコープ | 年サイクル骨格のみ（施策配置 UI は W2、結果計算は W3） | 週 3-4h に収める |
| スポーンパターンの分離 | 敵出現パターンを別 YAML（パッケージ）に切り出し、ステージ側は「年 = パターンの編成表」として参照する | パターンの再利用・年難易度調整の容易化・非プログラマによる編集。年切り替え時の停止も開始オフセットの変更だけで対応できる（2026-06-10 相談で決定） |
| パターンの表現形式 | 宣言的データ（いつ・何を・どこに）に限定。手続き的コマンド列は採用しない | YamlValidator による検証可能性と既存 EventLoader との整合を維持 |

---

## 現状アーキテクチャの理解（計画の根拠）

イベント駆動の実体は以下の 3 クラス：

| クラス | 場所 | 役割 |
|--------|------|------|
| EnvironmentalYamlProvider | Data/Repositories/ | YAML `events` をパースし `EventLoader._timer_events` に格納 |
| EventLoader | Game/Events/System/ | イベント辞書の保持と `ActionEvent()` 実行 |
| GameTimerCtrl | Presentation/UI/Dialogs/ | `Update()` で `_time` を進め、到達したイベントを発火。`_isPaused` フラグあり |

**ターンベース化の本質**: 「全イベントを 1 本のタイムラインで流す」を「年ごとのイベント集合に分割し、年の開始をボタンで、年の終了を固定時間で制御する」に変える。GameTimerCtrl の `_isPaused` と countdown 機構が既にあるため、改造は小さい。

**旧 W1 計画の問題点（修正済み）**:

- 「GameManager に統合」とあったが GameManager.cs はほぼ空（終了処理のみ）で統合先として不適切
- タイムライン駆動という既存設計に触れておらず、Year 管理を「上載せ」する具体的な接続点が不明だった

---

## YAML 拡張仕様（2 階層構成）

スポーンパターン（再利用可能パッケージ）とステージ（年ごとの編成表）の 2 階層に分離する。既存ステージとの後方互換を維持し、`years` セクションが無い YAML は従来どおり動作する。

### 階層 1: スポーンパターン定義（再利用パッケージ）

配置: `Assets/StreamingAssets/staging/patterns/*.yaml`

```yaml
# patterns/fire_small.yaml
pattern_id: fire_small          # システム内で一意
note: "小規模火災（FireCube 1 体）"
ver: 1.0.0
events:                         # time はパターン内の相対秒
  - time: 5
    event: spawn_unit
    value: FireCube, {spot}     # 場所はスロット（ステージ側で束縛）
```

**設計ルール**:

- `time` は必ずパターン内相対秒。絶対時刻は持たない
- マーカー名・座標の直書き禁止。`{spot}` 等の名前付きスロットにして場所への結び付けはステージ側に委ねる（場所はステージ固有のため。これを破るとパターンが再利用できなくなる）
- スロットの束縛値は任意文字列（座標 `"-184, 40, -52"`・マーカー名・`random_position`）。既存ステージの実態（spawn_unit + 座標指定）に合わせ、`{route}` 限定ではなく一般化した（2026-06-10 Task 1 実装時に修正）
- ルートベースのスポーン（spawn_enemy_unit）を使う場合はスロットにルート名を束縛し、既存の `_routeNameDict` 機構で解決する

### 階層 2: ステージ YAML（年 = パターンの編成表）

```yaml
stagename: "防災投資シミュレーション 兼六園"
stageid: SimFireKenrokuen
ver: 1.1.0

# 従来セクション（itemlists / pathmakers / goals 等）はそのまま

years:
  - year: 1
    duration: 60          # 年の長さ（秒）。経過で自動的に結果フェーズへ
    note: "小規模な火災"
    schedule:             # この年に編成するパターンのリスト
      - pattern: fire_small
        at: 5             # 年内の開始オフセット（秒）
        route: route_east # スロット束縛
    events:               # パターン化しない単発イベント（天候等）も併用可
      - time: 3
        event: wind
        value: strong
  - year: 3
    duration: 90
    note: "大規模延焼"
    schedule:
      - pattern: fire_small
        at: 5
        route: route_east
      - pattern: fire_small   # 同一パターンの多重編成で規模を表現
        at: 20
        route: route_west
```

**展開ロジック**: 年の開始時に `schedule` の各エントリを「`at` + パターン内相対 time」で実時刻に展開し、`events` の単発イベントとマージして `_timer_events` に積む。GameTimerCtrl 側の発火機構は変更不要。

**W1 で実装する範囲（最小形）**: パターン別ファイル化・相対時間・`at` オフセット・`route` スロット束縛のみ。

**将来拡張（W1 では実装しない）**: 敵数や強度のパラメータ化（`count:` 等）、パターンのネスト、条件付き発火。

[NOTE] `goals` / `gameovers` ベースの即時クリア判定はシミュレーションステージでは使わない（3 年完走が終了条件）。StageGoalController が年の途中で発火しないよう、シナリオ YAML 側で条件を満たさない設定にする。

---

## 年サイクル状態遷移

```
[Idle/Placement] --(Start Year ボタン)--> [YearRunning]
[YearRunning]    --(duration 経過)------> [YearEnd 処理] --> [Placement(次年)]
[Placement(Y3 終了後)] --> [Finished（最終結果。W3 で実装）]
```

| フェーズ | タイマー | プレイヤー操作 | 画面表示 |
|---------|---------|--------------|---------|
| Placement | 停止（_isPaused） | 施策配置（W2 で実装。W1 では待機のみ） | "Year N - 準備中" + Start Year ボタン |
| YearRunning | 進行 | 観戦（既存のタワー操作は許容） | "Year N" + 残り時間 |
| YearEnd | 停止 | なし（自動処理） | （W3 で結果パネル） |
| Finished | 停止 | リトライ or タイトルへ | （W3 で実装） |

**YearEnd 処理の内容（W1 実装分）**:

1. タイマー一時停止
2. 残存する敵ユニット（FireCube 等）をシーンから除去
   - 理由: タイマー停止はイベント発火を止めるだけで、敵 MonoBehaviour の Update は止まらない。配置フェーズ中に延焼が進む事故を防ぐ
3. 配置済みタワーはそのまま残す（引き継ぎ要件）
4. 年カウンタをインクリメントし次年の Placement へ

---

## 実装タスク

### Task 1: YAML 仕様確定 + パターン/サンプルステージ作成（45 分）[完了 2026-06-10]

- [x] 上記 2 階層スキーマ（patterns + years/schedule）を確定し [../../reference/yaml-format.md](../../reference/yaml-format.md) に追記（v1.1.0）
- [x] パターンファイルを 2 個作成: `patterns/fire_small.yaml`、`patterns/fire_spread.yaml`
- [x] 兼六園ステージの座標を流用した `SimFireKenrokuen.yaml`（3 年構成、schedule 参照）を作成
- 配置: `Assets/StreamingAssets/staging/` および `staging/patterns/`

**Task 1 で判明した制約（Task 5 への申し送り）**:

- YAML ファイル名 = シーン名の束縛（`LoadStreamingAsset.GetYamlFileName()`）があるため、SimFireKenrokuen を起動するには Unity Editor で「石川県金沢市兼六園」シーンを複製して `SimFireKenrokuen.unity` を作成し、ビルドのシーンリストと `stagelist.csv` に登録する必要がある（Editor 作業、約 10-15 分）
- stagelist.csv は Shift-JIS エンコーディング。編集時に注意

### Task 2: パターン読み込み + 年別イベント展開（1.5 時間）[実装完了 2026-06-10・検証待ち]

- [x] パターンリポジトリの新設: `SpawnPatternRepository`（Data/Repositories/）
  - `patterns/*.yaml` を読み込み `pattern_id` → 相対イベントリストの辞書を構築
  - `pattern_id` 重複・events 空・パース失敗は警告ログでスキップ
- [x] `YearScheduleYamlProvider`（Data/Repositories/ 新設）に `years` セクションのパースを実装
  - [NOTE] 計画では EnvironmentalYamlProvider への追記だったが、既存の Provider 分割慣習
    （UIYamlProvider / RouteYamlProvider / ObjectiveYamlProvider）に合わせ専用 Provider とした
  - `schedule` エントリを「`at` + パターン内相対 time」で実時刻に展開し、年内 `events` とマージ
  - 格納形式: `Dictionary<int year, Dictionary<float, List<Dictionary<string,string>>>>`
  - 名前付きスロット `{名前}` を schedule の束縛値で文字列置換（pattern / at は予約キーで除外）
  - `years` が無い場合は何もしない（後方互換）
  - 呼び出しは StageYamlRepository.LoadYamlData() に追加
- [x] EventLoader に年別辞書の保持と `LoadYearEvents(int year)` を追加
  - `SetYearEvents` / `ClearYearEvents` / `HasYearEvents` / `GetYearCount` / `GetYearDuration` を追加
  - LoadYearEvents は `_timer_events` を Clear + 再充填（dict 参照を保つため差し替えではなく中身を更新）
  - タイマー側（GameTimerCtrl）の時刻リセット・発火リスト再構築は Task 3 で接続
- [x] YamlSectionType.cs に Years キー・YearCommandFields・ScheduleCommandFields・YamlPatternKeys を追加
- [x] YamlParserHelper に BuildDictionaryListFromSequence / GetChildSequence / GetChildScalar を追加
- [x] 手動実行テスト `UnitTest/YearScheduleExpansionTest.cs`（スロット置換・at オフセット展開）
- [x] 武蔵野堺南木密.yaml を years 3年構成に変更（quake_fire パターン使用）、SimFireKenrokuen.yaml は削除
- 配置: 既存ファイルへの追記 + Repository/Provider 各 1 件新設

### Task 3: YearCycleSystem 実装（1 時間）

- [ ] 新規クラス `YearCycleSystem`（internal、namespace CommonsUtility）
  - 配置: `Assets/Scripts/Game/Systems/Simulation/YearCycleSystem.cs`
  - 責務: 年カウンタ・フェーズ状態機械・年開始/終了の制御
  - 公開メンバ: `CurrentYear`, `CurrentPhase`, `StartYear()`, `AdvanceToNextYear()`, `ResetSimulation()`
- [ ] GameTimerCtrl との接続
  - `StartYear()`: `LoadYearEvents(year)` → `_time = 0` リセット → `_countdown_time = duration` → `_isPaused = false`
  - duration 経過の検出 → `YearCycleSystem.OnYearTimeUp()` 呼び出し → YearEnd 処理
  - GameTimerCtrl に「年リセット用の internal メソッド」を追加する（外部から `_time` を直接触らない）
- [ ] YearEnd 処理: 敵ユニット一括除去（タグ or GameEnum.UnitType で検索）、タワーは残す

### Task 4: UI - Year 表示 + Start Year ボタン（1 時間）

- [ ] 新規 `YearPanelController`（Presentation/UI/Panels/、UIControllerBase 派生）
  - "Year N / 3" 表示
  - "Start Year" ボタン（Placement 中のみ表示）→ `YearCycleSystem.StartYear()`
  - YearRunning 中は残り時間表示（既存 GameTimerCtrl のカウントダウン表示を流用）
- [ ] Year 3 終了後は暫定で "Simulation Finished" 表示（結果パネルは W3）

### Task 5: PlayMode 検証（30 分）

- [ ] Year 1 Placement で開始 → Start Year → パターン由来イベントが `at` オフセットどおり発火 → duration 経過で停止
- [ ] 同一パターンを多重編成した年（Year 3）で全編成が発火する
- [ ] Year 1 で配置したタワーが Year 2 でも残存している
- [ ] Year 1 の残存敵が Year 2 開始時に存在しない
- [ ] Year 1 → 2 → 3 → Finished まで完走できる
- [ ] `years` セクションの無い既存ステージが従来どおり動く（後方互換）

---

## リスクと対策

| リスク | 影響 | 対策 |
|--------|------|------|
| StageGoalController が年の途中で Clear/Fail を発火 | 強制的にタイトルへ戻される | シナリオ YAML の goals/gameovers を発火しない条件にする。W1 検証項目に含める |
| タイマー停止中も敵の延焼が進む | 配置フェーズが成立しない | YearEnd で敵ユニット一括除去（Task 3） |
| `_timer_events` の年差し替えと初期化順序の競合 | InitializationManager 絡みの初期化バグ | LoadYearEvents は IsInitialized 後にのみ呼ぶ。初期年（Year 1）は Placement 開始でタイマー停止状態から始める |
| パターン内にマーカー名を直書きしてしまう | パターンがステージ間で再利用不能になる | スロット（`{route}`）必須をスキーマルール化し、検証で直書きを警告 |
| パターン展開後の同時刻イベント衝突（同じ実時刻キーに複数編成が重なる） | 後勝ちでイベント消失 | `_timer_events` の値は List のため追記マージで対応（既存構造のまま可）。展開処理でキー衝突時に Add ではなく List 追加を徹底 |
| GameTimerCtrl が UI 層にありながらゲーム進行を握っている | 層依存違反のリスク（Presentation → Game 呼び出しは適合だが逆は不可） | YearCycleSystem（Game 層）→ GameTimerCtrl（Presentation 層）の直接参照は **不可**。GameTimerCtrl 側が YearCycleSystem を参照する方向（Presentation → Game）に統一する |

[NOTE] 層依存の整理: `YearCycleSystem` は状態とロジックのみ持ち、GameTimerCtrl・YearPanelController（Presentation 層）が YearCycleSystem を参照・通知する。Game 層から UI を触らない。

---

## W2 以降への引き渡し事項

- 施策（消火栓・防火水槽・避難広場）は既存タワー派生で実装予定 → Placement フェーズに配置 UI を載せる土台が本計画の状態機械
- 結果計算（W3）は YearEnd 処理にフックを足す形で実装する
- 投資額の記録は W2 の施策定義（コスト）とセットで導入

---

**Last Updated**: 2026-06-10
