# CHANGELOG

OnoCoro プロジェクトの変更履歴です。

---

## [0.0.25-prototype] - 2026-09-03

**前回リリース**: 0.0.21-prototype (2026-03-10)
**Status**: Prototype / PLATEAU CityHack Challenge 2026
**Build Number**: 25
**Tag**: `v0.0.25-prototype`

### [NEW] 新機能

#### 災害廃棄物の推計

- **発生量の算出** - 建物の延床面積と構造種別から廃棄物量を算出
  - 原単位は PLATEAU 技術資料 `plateau_tech_doc_0015`「5.7 災害廃棄物発生量の採用原単位」に準拠
  - 木造 0.6 t/㎡ / 非木造 1.0 t/㎡
  - 構造種別は PLATEAU の耐火構造種別・階数・用途地域からの推定
- **可燃/不燃の分別** - 組成比（重量比）で廃材を分けて集計。`GarbageCubeNoBurn` を追加
- **4t トラック換算** - リザルトに可燃・不燃それぞれの台数を表示
- **建物情報ウィンドウ** - 構造種別と解体廃棄物量を表示
- **被害要因の分離** - 地震 / 浸水 / 火災の延焼 / 猫の解体に分けて集計

#### 高潮・海面

- **`ocean` イベント** - 海面の高さ・色・濁り（吸収距離）を YAML から操作
  - 秒数指定でなめらかに変化させられる
  - `watersurface` は Ocean / River を束ねる親を動かすもので海面の高さではないため、別イベントとして分離
- **`swell` イベント** - 海の遠方風速と荒れ具合を変更
- **浸水被害** - 一定の深さに一定時間浸かった建物が倒壊。1 秒あたりの倒壊数に上限

#### ユニット・設置物

- **巨大猫（EnemyCat）** - 経路上の建物を次々と解体していく敵ユニット
- **水位標（量水標）** - 20cm ごとの紅白帯と 1m ごとの数字。潮位を目盛りで読める

#### ステージ

- **京都舞鶴** - 港湾都市のマップ。デモ用に 1 年・180 秒の一本道構成
- **リザルト** - 「ステージを見て回る」ボタンを追加

### [FIX] バグ修正

#### Critical

- **ビルド版で浸水による建物破壊が起きない** - `FloodDamageMonitor` を
  `RuntimeInitializeOnLoadMethod` で自己生成しており、タイトル画面で作られて
  ステージロード時に破棄されていた。ステージ開始処理から生成する形に変更
- **リザルト表示中に ESC メニューへ到達できない** - パネルの Canvas sorting order が
  UIEscMenu(99) を越えていたため、ゲーム終了もタイトルへ戻ることもできなかった
  （YearPanel 100→90 / ResultPanel 200→91）

[IMPORTANT] 上の 2 件はいずれもエディタでは再現せず、ビルド版でだけ起きる不具合だった。

#### Major

- **火災延焼が 0 棟になる** - 浸水倒壊が倒壊済みの建物を二重計上し、
  「総被害 - 地震 - 浸水」で求める延焼数が潰れていた
- **年が演出の途中で終わる** - 火災鎮火による年の自動終了を YAML の `auto_end` で切れるようにした
- **瓦礫の係数が黙って頭打ちになる** - スポーン上限に達しても無警告で打ち切っていた。
  警告を出し、YAML の `max_cubes` で調整できるようにした
- **浸水倒壊の取りこぼし** - 倒壊済みフラグが無く同じ建物を再処理し続け、
  1 秒あたりの倒壊枠を食い潰していた

#### Minor

- 地震のカメラシェイクを半分の振幅にし、イージングでならした
- 瓦礫の爆散を解体時のみに限定し、地震倒壊は従来どおりの落ち方に戻した
- 猫が経路上で停止する問題への復帰処理を追加
- `Flame.cs` の `Debug` エイリアス欠落によるコンパイルエラーを修正

### [REFACTOR] リファクタリング

- **ログの統一** - 全ランタイムコードを `CommonsUtility.Debug` ラッパー経由に統一。
  トレース用のログを `Debug.LogTrace` に分離し、既定では出力しないようにした
- **浸水監視のコルーチン化** - `Update` から `IEnumerator` に変更。
  `WaitForSeconds` が `Time.timeScale` に従うため倍速・一時停止に自動追従する
- コメントアウトされた `Debug.Log` を削除（127 行）

### [POLICY] 開発方針

`AGENTS.md` に以下を追加。

- **`MonoBehaviour.Update` を極力使わない** - 倍速・一時停止への追従と負荷分散のため
- **`DontDestroyOnLoad` を使わない** - シーンをまたぐ状態の持ち越しが
  再現条件の追いにくい不具合を生むため。ステージ寿命のオブジェクトは
  ステージ開始処理で生成する
- **ログは追える量に保つ**

### [DOCS] ドキュメント

#### 新規作成

- `docs/reference/ui-sorting-order.md` - UI Canvas の重なり順の割当表
- `docs/howto/release-build.md` - ビルドから GitHub Release 公開までの手順
- `docs/howto/import-builtin-assets-to-hdrp.md` - Built-in 用アセットの HDRP 変換手順
- `docs/cityhack2026/` - 設計メモ・発表スクリプト・災害廃棄物データの出典

#### 更新

- `docs/project-rules/unity-design-patterns.md` - シーン寿命・Canvas 順序の方針
- `AGENTS.md` - Update / DontDestroyOnLoad / ログの各ポリシー

### [KNOWN_ISSUES] 既知の問題

- 広域浸水時のフレームレート低下（倒壊数の上限で緩和済み）
- 焼失建物の発生原単位（0.23 t/㎡）が未適用。構造別のみで判定している
- 猫の移動経路が道に沿っていない
- 三鷹駅前・兼六園に開発用の備忘録（`cvsMemo`）が表示されたまま。配布物からは外す
- プロジェクト名の表記ゆれ（`Onokoro` / `OnoCoro`）。正式名称は `OnoCoro`

---

## [0.0.20-prototype] - 2026-03-10

**前回リリース**: 0.0.6 (2026-01-26)  
**Status**: Prototype / Recovery Phase 2.2  
**Build Number**: 20  
**Tag**: `v0.0.20-prototype`

### [NEW] 新機能

#### 起動パフォーマンス診断
- **LogWithTimestamp()** - 初期化各フェーズでタイムスタンプ記録
- **LogWithMilliseconds()** - 各フェーズの実行時間を計測
- **InitializationManager 計測** - Phase 2-3 の ResourceLoaders / Managers / UIComponents ごとにタイミング記録
- **パフォーマンスログ出力** - Application.persistentDataPath に YYYYMMDD_onoco.log として自動出力

#### ドキュメントの充実
- **versioning.md** - バージョン管理・タグ付ルール、BuildDate.txt フォーマット、GitHub リリースタグ規則
- **debugging-and-logging.md** - デバッグログ・パフォーマンス診断ガイド
- **Release documentation** - README.md / RELEASE_NOTES.md / aboutthisgame.txt の統一化

### [FIX] バグ修正

#### Critical
- **(2026-03-08) シーン遷移時 itemlist 更新されない** - `StageYamlRepository._ItemList.Clear()` を追加 #e31348b4
  - 影響: 複数シーン間でユニット作成メニューのアイテムがシーンごとに正しく更新される
  - ItemCreateCtrl も `static _UIItemCreate` をインスタンス変数に変更し MissingReferenceException 対策

#### Major
- **(2026-02-27) GarbageCount 表示されない** - TextMeshProUGUI 参照をシーン遷移時にリフレッシュ
- **(2026-02-11) Stage Goal 判定** - static フラグリセット漏れ & UINotice 自動閉じ機能追加
- **(2026-02-07) Raycast QueryTriggerInteraction** - IgnoreTriggersを全 Physics.Raycast に統一（衝突判定の安定化）

#### Minor
- Enemy Litter タイムアウト検出と破壊フォールバック改善
- Naraku 落下フォールバック改善 + Y位置プレイヤースポーン追従
- CircularIndicator Prefab パス修正

### [REFACTOR] リファクタリング

#### Code Quality
- **(2026-03-08) ItemCreateCtrl 全体リファクタリング** - 責任範囲を明確化
  - `RegisterButtonListeners()` - ボタン登録のみに限定
  - `RebuildItemList()` - 最新データでリスト再構築（SwitchActive 時）
  - `RefreshView()` - SetActive 後に画面更新（アイコン再描画保証）
- **GameConfig ログ設定** - LogFilePath / LogFileName を Application.persistentDataPath に統一
- **Debug.cs 拡張** - LogWithTimestamp / LogWithMilliseconds メソッド追加

#### Documentation
- **AGENTS.md** - セッション情報要件の統一
- **versioning.md** - バージョン管理規則の体系化
- **aboutthisgame.txt** - プロトタイプ版表記・Unity バージョン更新

### [DOCS] ドキュメント

#### 新規作成
- `docs/architecture/camera-exposure-settings.md` - カメラ・Depth of Field トラブルシューティング
- `docs/architecture/camera-deoccluder-implementation.md` - CinemachineDeoccluder 実装ガイド

#### 更新
- AGENTS.md - スリム化・ドキュメント構造整理
- prototype-phase-roadmap.md - Phase 2-3 進捗追跡
- TODO.md - carrera地面潜り対策 TODO 追加

### [PERF] パフォーマンス

- 起動遅延診断インフラストラクチャ実装（詳細な初期化タイミング記録）
- Phase 2 段階での計測準備完了

### [KNOWN_ISSUES] 既知の問題

- [ ] ⏳ **低スペック環境での起動遅延** - Phase 2.2 で診断中 (詳細は README.md 参照)
- [ ] マップの端から落ちる可能性がある
- [ ] Fire イベント延焼範囲の表示が未実装
- [ ] ユニットアップグレード機能が未実装
- [ ] ゲーム効果音・BGM が未実装
- [ ] セーブ機能が未実装

### 技術基盤

| 項目 | バージョン |
|------|----------|
| **Unity** | 6.3.10f1 |
| **PLATEAU SDK** | Latest |
| **Cinemachine** | Unity Standard |

詳細なパッケージ構成と開発環境設定は [docs/BUILD_ENVIRONMENT.md](docs/BUILD_ENVIRONMENT.md) を参照。

---

## [0.0.6] - 2026-01-26

Phase 0 初期ビルド版。エディター参照削除、設定更新実施。

---

## このリリースに含まれるシーン

- ✅ TitleScene - タイトル・シーン選択画面
- ✅ 石川県金沢市兼六園 - チュートリアルステージ（推奨）
- ✅ 三鷹井の頭 - Wave テストステージ

---

## このリリースでのテスト焦点

### ✓ Prototype 段階で確認済み
- [x] マルチシーン遷移・UI初期化
- [x] itemlist シーン別更新
- [x] YAML イベント・パス読み込み
- [x] ユニット作成・配置
- [x] Enemy Litter スポーン・移動
- [x] カメラ制御（FPS/TPS/LongShot/BirdView）
- [x] 起動タイミング・パフォーマンスログ記録

### ⏳ テストユーザー向け検証必要事項
- [ ] GPU 互換性（Intel/NVIDIA/AMD）
- [ ] 低仕様環境での FPS 安定性（特に新宿都庁、東京駅）
- [ ] 長時間連続プレイ（メモリリーク検証）
- [ ] 起動遅延の詳細分析（ログ提供 / フィードバック希望）

---

## テストユーザー向け報告方法

GitHub Issues で以下の形式で報告をお願いします。**ログファイルの添付が特に重要です**：

```
**Title**: [BUG] 現象を一行で説明

**Environment**:
- OS: Windows XXX
- GPU: モデル
- ビルド番号: v0.0.20-prototype

**Logs**:
- Player.log: %AppData%/../LocalLow/Hagurachaya/Onokoro/
- Performance Log: YYYYMMDD_onoco.log (same directory)

**Reproduction**:
1. 手順1
2. 手順2

**Expected vs Actual**:
期待: xxx
実際: yyy
```

ログファイル位置: `C:\Users\[ユーザー名]\AppData\LocalLow\Hagurachaya\Onokoro\`

---

## インストール・起動方法

1. `OnoCoro-v0.0.20-prototype.zip` を解凍
2. `OnoCoro.exe` を実行
3. シーン選択画面から「石川県金沢市兼六園」（チュートリアル）を選択
4. TAB キーでユニット作成メニュー表示
5. ユニットを配置してテスト

---

**公式リポジトリ**: https://github.com/kuippa/OnoCoro  
**ライセンス**: MIT License
