# Season 3 Week 3 詳細計画 - 結果表示（被害率・ROI）+ デモフロー

**作成日**: 2026-06-13
**対象期間**: 2026-06-14 ～ 2026-06-28（W2 が 8 日前倒し完了のため前倒し開始可）
**工数見積**: エージェント作業 約 3-4 時間 + ユーザー作業（動画撮影・資料）
**前提**: [season3-w2-policy-map-detailed-plan.md](season3-w2-policy-map-detailed-plan.md) の成果（年サイクル・施策・投資台帳・倒壊予測）の上に構築する

---

## 設計判断の記録（2026-06-13 ヒアリング結果）

| 論点 | 決定 | 理由 |
|------|------|------|
| 被害率の計算基準 | 地震倒壊と火災延焼を分離 | 地震倒壊は「避けられない初期被害」、火災延焼は「施策で減らせる被害」。防災投資の効果が延焼分にハッキリ出る。教育ツールとして筋が良い |
| ROI のベースライン | 倒壊予定棟数を想定被害とする | 出火点（地震倒壊建物）から延焼しうる想定被害を基準に、実際に焼け残った差分を「救った棟数」とする。追加シミュレーション不要で軽い |
| 結果パネルのタイミング | 毎年終了時 + 最終総括 | 年ごとに振り返りつつ、3年完走後に累計サマリーを出す。ワークショップの進行に合う |
| W3 エージェント範囲 | 結果パネル + 計算ロジックまで | 動画撮影・YouTube投稿・首長向けスライドはユーザー作業（Mac実行・OBS録画）。エージェントは撮影しやすいデモフロー（ハッピーパス）を整える |

---

## 被害・ROI モデル（確定仕様）

### データソース（既存資産）

- `_doomedBuildings`（PlateauBuildingInteractor）: 地震倒壊 + 火災焼失の両方が入る建物リスト。**年をまたいで蓄積**する
- `building_break` の棟数 N: 年イベントから事前に読める（HazardForecastSystem.GetForecastBreakCount と同じ方式）
- `InvestmentLedger`: 年別・施策別の投資額

### 3 スナップショット方式（年ごとの被害分離）

`_doomedBuildings` が累積するため、年ごとの新規被害を差分で測る。1 年につき 3 回 Count を取得：

```
D_prev      = 年初（building_break 前）の _doomedBuildings.Count
D_afterQuake = building_break 発火直後の Count
D_end       = 年末（OnYearTimeUp 時点）の Count

地震倒壊（この年）= D_afterQuake - D_prev
火災延焼（この年）= D_end - D_afterQuake
```

[NOTE] D_afterQuake の捕捉は BuildingBreak に「実際に新規倒壊させた棟数」を報告させる方式が確実
（building_break は先頭 N 棟・倒壊済みスキップのため、意図値 N と実数がずれうる）。
BuildingBreak.EventBreakBuilding に DamageReportSystem への通知を追加する。

### 想定被害とROI（ベースライン）

```
延焼係数 K     = 施策ゼロ時に 1 出火点あたり延焼する想定棟数（定数・バランス調整対象）
想定延焼被害   = 地震倒壊（出火点数）× K
救った棟数     = max(0, 想定延焼被害 - 火災延焼（実測））
ROI           = 救った棟数 ÷ (投資額 / 投資単位)   ※ ゼロ除算ガード
```

施策（消火栓・防火水槽）が範囲内の火災を鎮火 → 実測の火災延焼が減る → 救った棟数が増える、という因果がそのまま数値に出る。

### 避難広場（Plaza）の扱い

Q1 は建物被害の話。Plaza は人的被害の軽減施策だが、人口モデルが無いため W3 では簡易化：
- Plaza の効果範囲が建物をカバーした割合を「避難カバー率」として**別指標**で表示
- 被害率（建物）には混ぜない。人的被害の本格モデルは W4 以降の検討事項

---

## 実装タスク

### Task 1: DamageReportSystem（被害・ROI 計算）（エージェント・1.5h）

- [ ] 新規 `DamageReportSystem`（Game/Systems/Simulation/、internal static）
  - 年別結果 `YearResult` を保持（year, 地震倒壊, 火災延焼, 想定延焼, 救った棟数, 投資額, ROI, 避難カバー率）
  - `OnYearStart(year)`: D_prev を記録
  - `OnQuakeDone(actualBrokenCount)`: D_afterQuake を記録（BuildingBreak から通知）
  - `OnYearEnd(year)`: D_end を測定し YearResult を確定、リストに追加
  - `GetYearResult(year)` / `GetSummary()`（累計）/ `Reset()`
- [ ] BuildingBreak.EventBreakBuilding に DamageReportSystem.OnQuakeDone 通知を追加
- [ ] YearCycleSystem のフェーズ遷移にフック（StartYear で OnYearStart、OnYearTimeUp で OnYearEnd）
- [ ] 延焼係数 K・投資単位は定数化（バランス調整は Task 4 で）

### Task 2: 結果パネル UI（毎年）（エージェント・1h）

- [ ] 新規 `ResultPanelController`（Presentation/UI/Panels/、自己構築型・YearPanel と同方式）
  - 年末（OnYearEnd 後）に「Year N 結果」を表示:
    ```
    【Year N 結果】
    地震倒壊: ◯棟（初期被害）
    火災延焼: ◯棟  ← 施策で◯棟を延焼から救った
    今年の投資: ◯ゴールド
    ROI: ◯.◯
    ```
  - 「次の年へ」ボタンで閉じる → 次年の配置フェーズへ
  - [NOTE] YearCycleSystem の年末→次年配置の遷移に「結果確認待ち」を挟む必要あり。
    現状は OnYearTimeUp で即 Placement へ進むため、結果パネルを挟む状態を追加する

### Task 3: 最終総括パネル（エージェント・0.5-1h）

- [ ] Finished フェーズで総括サマリーを表示:
    ```
    【3年間の総括】
    累計投資: ◯ゴールド
    救った建物: 累計◯棟
    総合ROI: ◯.◯
    避難カバー率: ◯%
    （メッセージ: 投資効果の所感）
    ```
- [ ] 「もう一度」（リトライ）/「タイトルへ」ボタン

### Task 4: バランス調整 + デモフロー整備（エージェント + ユーザー・0.5h+）

- [ ] 延焼係数 K・施策の効果半径/鎮火力を、小マップで「施策を置くと救った棟数が目に見えて増える」よう調整
- [ ] 三鷹井の頭５丁目で 3 年通しのハッピーパス（地震→ヒートマップ→投資→結果）が破綻なく流れることを確認
- [ ] 撮影用に分かりやすい初期予算・年数バランスへ微調整

### Task 5: PlayMode 検証（ユーザー + エージェント・30分）

- [ ] 毎年の結果パネルに地震倒壊・火災延焼・投資・ROI が表示される
- [ ] 施策を多く置いた年は火災延焼が減り、救った棟数・ROI が上がる
- [ ] 3 年完走後に総括パネルが出る
- [ ] 数値がログ（DamageReportSystem）と一致する
- [ ] 既存ステージ（兼六園）が壊れていない

---

## ユーザー作業（エージェント範囲外・W3 後半）

- [ ] Mac で三鷹井の頭５丁目を実行し 3-5 分のデモ動画を撮影（OBS / QuickTime）
- [ ] YouTube ライブアーカイブに投稿
- [ ] 首長向け 1 枚スライド + 説明台本（W4 相当・必要なら docx/pptx 生成をエージェントに依頼可）

---

## リスクと対策

| リスク | 影響 | 対策 |
|--------|------|------|
| _doomedBuildings の年またぎ蓄積で被害分離がずれる | 数値が不正確 | 3 スナップショット方式 + BuildingBreak の実倒壊数通知で正確に分離 |
| 結果パネル挟み込みで年遷移フローが壊れる | 進行不能 | YearCycleSystem に「結果確認待ち」状態を追加し、ボタンで次年へ進む形に |
| 延焼係数 K が実測と乖離（救った棟数がマイナス/過大） | ROI が不自然 | K を定数化し max(0,...) でクランプ。Task 4 で小マップ実測に合わせて調整 |
| 施策の鎮火が弱く火災延焼が減らない | ROI が動かず体験が成立しない | Task 4 でバランス調整（W2 から定数化済み）。最悪は鎮火力を上げる |

---

## W4（条件付き・前倒し余地）

スケジュール上の W4（首長向け資料）は、エージェントが docx/pptx でドラフト作成可能。
ユーザーの動画撮影と並行で、依頼があれば着手する。

---

**Last Updated**: 2026-06-13
