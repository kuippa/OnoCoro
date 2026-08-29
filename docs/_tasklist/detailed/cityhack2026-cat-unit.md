---
title: 巨大猫ユニット（CityHack 2026）
description: 経路上の建物を解体する敵ユニットの実装メモとプレファブ要件
---

# 巨大猫ユニット

`EnemyLitter`（ほかしもん）を下敷きにした、指定経路を移動する敵ユニット。
ゴミを撒く代わりに、近づいた建物を解体（更地化）して廃材を出す。

## プレファブ要件

**配置場所**: `Assets/Resources/Prefabs/EnemyUnit/EnemyCat.prefab`

パスは `PrefabManager` に登録済み（`Prefabs/EnemyUnit/EnemyCat`）。
名前が違うとロードに失敗するので、この名前で作ること。

| 項目 | 必須 | 内容 |
|------|------|------|
| タグ | 必須 | `Cat`（TagManager に追加済み） |
| `NavMeshAgent` | 必須 | 無いと EnemyCat が自身を無効化する |
| `EnemyCat` スクリプト | 必須 | 本体の挙動 |
| `CapsuleCollider` or `BoxCollider` | 推奨 | 足元を接地させる計算に使う。無い場合はピボットが地面基準になる |
| `Cat` スクリプト | 不要 | 生成時に自動で付く |
| 子オブジェクト | 不要 | `EnemyLitter` と違い CapsuleHead / Hand は要らない |

[NOTE] 猫のモデルはアニメーションしなくてよい（敵ユニットとして経路移動するだけ）。
見た目の階層構造は自由。

## 呼び出し方（ステージ YAML）

```yaml
- time: 5
  event: spawn_enemy_unit
  value: Cat, path_marker_start, path_marker_01, path_marker_goal
```

事前にシーンへ `Prefabs/Marker/path_marker` を配置し、名前を合わせておくこと。

## 能力パラメータ

`Cat.cs` の定数で調整する。

| 定数 | 既定値 | 意味 |
|------|--------|------|
| `DEMOLISH_INTERVAL` | 1.5 秒 | 建物を解体する間隔 |
| `DEMOLISH_RADIUS` | 12 m | 解体対象を探す半径 |
| `MAX_DEMOLISH_COUNT` | 20 棟 | 1 匹が解体できる上限 |

## 設計メモ

### 接触判定ではなく近接判定にした理由

建物は NavMesh の障害物なので、エージェントは建物を避けて動く。
つまり物理的な接触は起きにくく、`OnCollisionEnter` では壊せない場面が多い。
そこで一定間隔で周囲を `OverlapSphere` で探し、最も近い 1 棟を解体している。
経路脇の建物も壊せるので、猫が通った跡が帯状に更地になる。

1 回につき 1 棟に絞っているのは、瓦礫の生成負荷を分散させるため。
`DEMOLISH_INTERVAL` を短くしすぎると 1 フレームあたりのキューブ生成が増えて重くなる。

### 進めなくなったときの挙動

`EnemyLitter` は移動タイムアウト時に近隣タワーを破壊するが、
猫は**目の前の建物を解体して進む**ようにしている。挟まって止まる事故を防ぐため。

### 負荷について

解体 1 棟につき最大 `max_cubes` 個（既定 1000）の Rigidbody が発生する。
`MAX_DEMOLISH_COUNT` 20 棟をすべて壊すと最大 20,000 個になりうる。
重い場合は、まずステージ YAML の `max_cubes` を下げること
（`debris_per_ton` と `max_cubes` は見た目専用のつまみで、廃棄物のトン数には影響しない）。

## 未実装・今後

- 猫が壊した棟数をリザルトに出す（現状は Console のみ）
- 解体時の演出（足跡・土煙など）
- 複数匹の同時スポーン時の負荷検証
