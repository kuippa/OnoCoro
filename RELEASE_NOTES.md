# OnoCoro v0.0.25-prototype - Prototype Build

**Release Date**: 2026-09-03
**Tag**: `v0.0.25-prototype`
**Version**: v0.0.25-prototype
**Build Number**: 25
**Build Target**: StandaloneWindows64

---

## 概要

PLATEAU CityHack Challenge 2026 に向けて開発した **災害廃棄物シミュレーション** を中心としたビルド。
前回リリース（v0.0.21-prototype, 2026-03-10）以降の開発成果をまとめています。

本ビルドの主題は、**街が壊れたときに出る廃材の量を推計して見せる**ことです。
京都・舞鶴のマップで、高潮による浸水 → 地震と火災 → 巨大猫による解体 と被害が進み、
最後に発生した災害廃棄物を可燃・不燃に分けて 4t トラック換算で表示します。

> 国土交通省の PLATEAU を使った、まちづくりをテーマにしたゲーム

---

## ダウンロード

| 形式 | リンク |
|------|--------|
| **Standalone PC (EXE)** | [OnoCoro_v0.0.25-prototype.zip](https://github.com/kuippa/OnoCoro/releases/download/v0.0.25-prototype/OnoCoro_v0.0.25-prototype.zip) |
| **ソースコード** | GitHub リポジトリから Clone |

---

## インストール・実行

1. zip をダウンロードして任意のフォルダに解凍
2. `Onokoro.exe` をダブルクリック

**動作環境:**
- OS: Windows 10 / 11
- RAM: 8GB 以上推奨
- GPU: VRAM 2GB 以上

[NOTE] 舞鶴ステージは建物が約 7,900 棟あります。
広域が浸水すると倒壊処理が集中するため、低スペック環境ではフレームレートが落ちることがあります。

---

## 新機能 [FEATURE]

### 災害廃棄物の推計

建物を壊すと、その建物の延床面積と構造種別から廃棄物の発生量を算出します。

- **発生原単位は公的資料の採用値** - PLATEAU 技術資料 `plateau_tech_doc_0015`
  「5.7 災害廃棄物発生量の採用原単位」に準拠
  （横浜市災害廃棄物処理計画 / 環境省 災害廃棄物対策指針 技術資料）
- **木造 0.6 t/㎡・非木造 1.0 t/㎡** を構造種別に応じて適用
- **可燃・不燃の組成比**（重量比）で廃材を分けて集計
- **4t トラック換算** をリザルト画面に表示。可燃・不燃それぞれの台数を出す
- 建物情報ウィンドウに**構造種別と解体廃棄物量**を表示

[NOTE] PLATEAU のデータに建物構造そのものは含まれないため、
耐火構造種別・階数・用途地域から**推定**しています。

### 被害の要因を分けて集計

リザルトで、倒壊の原因を**地震 / 浸水 / 火災の延焼 / 猫の解体**に分けて表示します。

### 高潮による浸水被害

- `ocean` イベントで海面の高さ・色・濁り（吸収距離）を YAML から操作
- 秒数を指定して**なめらかに潮位を変化**させられる
- 一定の深さに一定時間浸かった建物が倒壊する
- 1 秒あたりの倒壊数に上限を設けて負荷を制御

### 海のうねり

- `swell` イベントで海の遠方風速と荒れ具合を変更

### 巨大猫（EnemyCat）

- 経路上の建物を次々と解体していく敵ユニット
- 壊し方が違っても、出てくる廃材の計算は共通

### 水位標（量水標）

- 20cm ごとの紅白帯と 1m ごとの数字を持つ設置物
- 潮位の上昇を目盛りで読み取れる

### 京都舞鶴ステージ

港湾都市のマップを追加。デモ用に 1 年・180 秒の一本道構成にしています。

---

## バグ修正 [FIX]

### Build 25 での修正

- **ビルド版で浸水による建物破壊が起きない** -
  監視オブジェクトを `RuntimeInitializeOnLoadMethod` で自己生成しており、
  タイトル画面で作られてステージロード時に破棄されていた。
  ステージ開始処理から生成する形に変更
- **リザルト表示中に ESC メニューへ到達できない** -
  パネルの Canvas sorting order が UIEscMenu を越えていたため、
  ゲーム終了もタイトルへ戻ることもできなかった

[IMPORTANT] 上の 2 件はいずれも**エディタでは再現せず、ビルド版でだけ起きる**不具合でした。

### その他の修正

- 火災延焼が 0 棟になる浸水倒壊の二重計上を修正
- 火災鎮火による年の自動終了を YAML で切れるようにした（演出の途中で年が終わる問題）
- 地震のカメラシェイクを半分の振幅にし、イージングでならした
- 瓦礫の爆散を解体時のみに限定し、地震倒壊は従来どおりの落ち方に戻した
- 瓦礫のスポーン上限で係数が黙って頭打ちになる問題を修正（警告を出すようにした）
- 猫が経路上で停止する問題への復帰処理を追加

---

## 開発方針の変更 [POLICY]

コーディング規約に以下を追加しました（[AGENTS.md](https://github.com/kuippa/OnoCoro/blob/main/AGENTS.md)）。

- **`MonoBehaviour.Update` を極力使わない** -
  `Time.timeScale` による倍速・一時停止に自動追従させるため、コルーチンを優先する
- **`DontDestroyOnLoad` を使わない** -
  シーンをまたぐ状態の持ち越しが、再現条件の追いにくい不具合を生むため
- **ログは追える量に保つ** - トレース用のログは `Debug.LogTrace` に分離

---

## ドキュメント [DOCS]

### 新規作成

- `docs/reference/ui-sorting-order.md` - UI Canvas の重なり順の割当表
- `docs/howto/release-build.md` - ビルドから GitHub Release 公開までの手順
- `docs/howto/import-builtin-assets-to-hdrp.md` - Built-in 用アセットの HDRP 変換手順
- `docs/cityhack2026/` - CityHack 2026 の設計メモ・発表資料

### 更新

- `docs/project-rules/unity-design-patterns.md` - シーン寿命・Canvas 順序の方針を追加
- `AGENTS.md` - Update / DontDestroyOnLoad / ログの各ポリシー

---

## リリース対象シーン

| シーン | 説明 |
|--------|------|
| **TitlteStart** | タイトル・ステージ選択 |
| **京都舞鶴** | 災害廃棄物シミュレーション（本ビルドの主題） |
| **石川県金沢市兼六園** | チュートリアルステージ |
| **三鷹大沢 / 三鷹駅前 / 三鷹井の頭 / 三鷹井の頭５丁目** | 各種テストステージ |
| **新宿都庁 / 武蔵野堺南木密** | 各種テストステージ |
| **今日はここまで** | 開発配信の終了表示用（ゲーム内容ではありません） |

---

## 基本操作

| 操作 | 機能 |
|------|------|
| **WASD** | 移動 |
| **Shift + WASD** | 走行 |
| **マウス** | 視点操作 |
| **マウスホイール** | ズーム |
| **SPACE** | ジャンプ |
| **右クリック** | 建物情報の表示 |
| **TAB** | ユニット作成メニュー |
| **1-5** | ユニット選択 |
| **ESC** | メニュー（終了・タイトルへ戻る） |
| **F2** | 一時停止（デバッグ） |
| **F3-F5** | 時間倍速（デバッグ） |

---

## 既知の問題 [KNOWN_ISSUES]

| 問題 | 状態 |
|------|------|
| 広域浸水時のフレームレート低下 | 倒壊数の上限で緩和済みだが、低スペック環境では残る |
| 焼失建物の発生原単位が未適用 | 資料にある焼失区分（0.23 t/㎡）は未実装。構造別のみで判定している |
| 猫の移動経路が道に沿っていない | 経路マーカーの調整が未了 |
| 三鷹駅前・兼六園に開発用のメモが表示される | 作業用の備忘録。次回リリースまでに非表示にする |
| ゲーム効果音・BGM | 未実装 |
| セーブ機能 | 未実装 |

---

## 技術仕様

| 項目 | バージョン |
|------|----------|
| **Unity Engine** | 6.3.10f1 |
| **HDRP (Render Pipeline)** | 17.3.0 |
| **Cinemachine** | 3.1.6 |
| **PLATEAU SDK** | Latest |
| **Input System** | 1.18.0 |
| **Visual Effect Graph** | 17.3.0 |

**詳細**: [docs/BUILD_ENVIRONMENT.md](https://github.com/kuippa/OnoCoro/blob/main/docs/BUILD_ENVIRONMENT.md)

---

## ステージを自分で作る

ステージの進行は YAML で記述されており、**ビルド版でもそのまま編集できます**。

```
<解凍先>\Onokoro_Data\StreamingAssets\staging\
```

イベントの書式は
[docs/reference/yaml-format.md](https://github.com/kuippa/OnoCoro/blob/main/docs/reference/yaml-format.md) を参照してください。
`京都舞鶴.yaml` にはコメントで各パラメータの意味を書いてあります。

---

## バグ報告

[GitHub Issues](https://github.com/kuippa/OnoCoro/issues) へお願いします。

```
**Title**: [BUG] 現象を一行で説明

**Environment**:
- Windows バージョン:
- GPU モデル:
- ビルド番号: v0.0.25-prototype

**Reproduction**:
1. 手順1
2. 手順2
3. 現象発生

**Logs**: 下記フォルダのログを添付してください
```

ログの場所:

```
C:\Users\<ユーザー名>\AppData\LocalLow\Hagurachaya\Onokoro\
├── Player.log            Unity 標準ログ
└── <YYYYMMDD>_onocoro.log  ゲーム側のログ
```

---

## 開発者向け情報

- **Repository**: https://github.com/kuippa/OnoCoro
- **Branch**: main
- **Version**: v0.0.25-prototype (Build 25)

| ドキュメント | 内容 |
|-----------|------|
| [README.md](https://github.com/kuippa/OnoCoro/blob/main/README.md) | プロジェクト概要 |
| [AGENTS.md](https://github.com/kuippa/OnoCoro/blob/main/AGENTS.md) | コーディング基準（必読） |
| [CHANGELOG.md](https://github.com/kuippa/OnoCoro/blob/main/CHANGELOG.md) | 変更履歴 |
| [docs/](https://github.com/kuippa/OnoCoro/tree/main/docs) | 設計ドキュメント |

---

## ライセンス

MIT License - [LICENSE](https://github.com/kuippa/OnoCoro/blob/main/LICENSE)

---

## コミュニティ

- [バグ報告](https://github.com/kuippa/OnoCoro/issues)
- [Discussions](https://github.com/kuippa/OnoCoro/discussions)
- [開発ライブ配信](https://www.youtube.com/playlist?list=PLxWlv9T7cA6YhDW4aLlfn6BCZQFYPrOJ3)

---

**作成日**: 2026-09-03
**バージョン**: v0.0.25-prototype
**ビルド番号**: 25
**Tag**: `v0.0.25-prototype`
