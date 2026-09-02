---
title: UI Canvas Sorting Order 割当表
description: Canvas の重なり順の一覧と、新規追加時に守る割当ルール
---

# UI Canvas Sorting Order 割当表

Canvas の `sortingOrder` は数字が大きいほど手前に描画され、
手前の Canvas が後ろの Canvas のクリックを奪う。

[IMPORTANT] **操作を受け付ける UI を覆い隠すと、その機能に到達できなくなる。**
特に UIEscMenu はゲーム終了・タイトルへ戻るの唯一の出口なので、
これを越える Canvas を常時表示するとステージから抜けられなくなる。

## 割当の考え方

| 帯域 | 用途 |
|------|------|
| 0 - 9 | 常設 HUD。ゲーム画面に貼り付いている情報表示 |
| 10 - 19 | 選択・詳細表示。HUD より前に出るが一時的なもの |
| 20 - 88 | 予備 |
| 89 - 98 | 全画面を覆うパネル。ここまでは ESC メニューに譲る |
| 99 | **UIEscMenu 予約**。これを越えてはならない |
| 100 以上 | **使用禁止**（GameInterface のルート Canvas を除く） |

## 現在の割当

### プレファブに設定されているもの

`Assets/Resources/Prefabs/UI/` 配下。値は Canvas コンポーネントに直接入っている。

| Canvas | Order | 役割 |
|--------|-------|------|
| UIToolTips | 1 | ツールチップ |
| UIEventLog | 2 | イベントログ |
| UIRightTop | 2 | 右上情報 |
| UIRightBottom | 2 | 右下情報 |
| UIItemCreate | 4 | アイテム設置メニュー |
| UIInfo | 8 | 情報表示 |
| InfoBox | 10 | 情報ボックス |
| UIBuildingInfo | 10 | 建物情報（右クリック） |
| UIBoard | 11 | 看板 |
| UITelop | 11 | テロップ |
| UIMessageBox | 15 | 確認ダイアログ |
| UINotice | 89 | 全画面通知 |
| **UIEscMenu** | **99** | **ESC メニュー（ゲーム終了・タイトルへ戻る）** |
| GameInterface | 200 | ルート Canvas。下記の注記を参照 |

### コードで生成しているもの

プレファブを持たず、コントローラが実行時に Canvas を作る。
値は各クラスの `_CANVAS_SORT_ORDER` 定数。

| Canvas | Order | 定義場所 |
|--------|-------|---------|
| YearPanelCanvas | 90 | `Presentation/UI/Panels/YearPanelController.cs` |
| ResultPanelCanvas | 91 | `Presentation/UI/Panels/ResultPanelController.cs` |

[NOTE] 以前はそれぞれ 100 / 200 だった。UIEscMenu(99) を越えていたため、
リザルト表示中に ESC メニューへ到達できずゲームを終了できない不具合があった。

### シーンに直接置かれているもの

| シーン | Canvas | Order | 備考 |
|-------|--------|-------|------|
| TitlteStart | cvsBG | 5 | タイトル背景 |
| TitlteStart | UICvs | 10 | タイトル UI |
| 三鷹駅前 | cvsMemo | 999 | 開発用の備忘録。**意図的に最前面** |
| 石川県金沢市兼六園 | cvsMemo | 999 | 同上 |

[NOTE] TitlteStart の 2 件はタイトル画面専用で、ステージ UI とは共存しないため衝突しない。

[NOTE] `cvsMemo` は作業を再開するときの備忘録を画面に出しておくためのもので、
**何より前に出ることが目的**なので 999 で正しい。帯域の対象外として扱う。
配布物に載せる性質のものではないため、リリース前に非表示にすること。

## GameInterface の 200 について

GameInterface のルート Canvas だけは 200 で、上の帯域から外れている。
これは他の UI プレファブを子として抱えるコンテナであり、
子 Canvas が `Override Sorting` を持つ限り、この値は子の重なり順に影響しない。

[NOTE] 子 Canvas で `Override Sorting` が外れていると、
その Canvas の `sortingOrder` は無視されて親の値が使われる。
数字を正しく設定したのに効かないときはここを確認する。

## Render Mode との関係

[IMPORTANT] **`sortingOrder` の比較は同じ Render Mode の中でしか成立しない。**

Screen Space - Overlay の Canvas は、Screen Space - Camera の Canvas より
**常に手前**に描画される。数字の大小は関係ない。

そのため「数字を下げたのに、まだ前に出てしまう」場合は
Render Mode が食い違っている。片方に合わせること。

## 新しい Canvas を追加するとき

1. この表を見て空いている値を選ぶ
2. 99 以上は使わない
3. この表に行を追加する
4. コードで設定する場合は定数名を `_CANVAS_SORT_ORDER` に統一する

## 関連ドキュメント

- [project-rules/unity-design-patterns.md](../project-rules/unity-design-patterns.md) - Canvas・UI パターン
