# YAML ステージフォーマット仕様

**バージョン**: 1.0.0  
**対象ファイル**: `Assets/StreamingAssets/staging/*.yaml`  
**最終更新**: 2026-02-02

---

## 概要

OnoCoro のステージデータは **YAML 形式** で定義されます。各ステージは以下の情報を含みます：

- **ステージメタデータ**: 名前・難易度・目標条件
- **ユニット定義**: タワー・敵のリスト
- **マップレイアウト**: パスマーカー・敵の経路
- **ゲーム進行イベント**: 敵スポーン・天候・風・ゲーム終了条件

---

## ファイル構造（全体）

```yaml
---
stagename: <ステージ表示名>                    # REQUIRED
stagenotice: <ステージ説明>                    # REQUIRED
stageid: <ステージ ID>                         # REQUIRED（ファイル名と同じ）
ver: 1.0.0                                   # 仕様バージョン

stages:                                      # ステージ基本情報
  - name: <ステージ内部名>
    note: <説明>
    BIT: <基地 HP>                            # (※ 現在は未使用)
    CLK: <制限時間>                           # (※ 現在は未使用)

itemlists:                                   # アイテム・ユニット定義
  - item: <ユニット名>

pathmakers:                                  # マップマーカー配置
  - name: <マーカー名>
    pos: <X>, <Y>, <Z>

goals:                                       # ステージクリア条件
  - <目標タイプ>: <パラメータ>

gameovers:                                   # ゲームオーバー条件
  - <敗北タイプ>: <パラメータ>

events:                                      # ゲーム進行イベント
  - time: <トリガー時刻（秒）>
    event: <イベントタイプ>
    value: <パラメータ>
```

---

## セクション詳細

### 1. ステージメタデータ（必須）

```yaml
stagename: "火災延焼"              # UI に表示される名前
stagenotice: "火災を消火するステージ"   # ステージ説明
stageid: UnitFireDisaster          # 内部 ID（ファイル名と一致）
ver: 1.0.0                         # YAML 仕様バージョン
```

[IMPORTANT] `stageid` はシステム内で一意である必要があります。重複するとシーン読み込みエラーが発生します。

---

### 2. Stages セクション

```yaml
stages:
  - name: UnitFireDisaster         # ステージ内部名（現在は表示名と同じ）
    note: "火災延焼"               # 説明
    BIT: 5000                      # ✗ 現在は未使用（互換性維持）
    CLK: 100                       # ✗ 現在は未使用（互換性維持）
```

[NOTE] `BIT` と `CLK` は仕様に含まれていますが、**現在は機能していません**（Phase 2 以降で実装予定）。

---

### 3. Itemlists セクション（敵・タワー・アイテム定義）

**用途**: ステージで利用可能なユニット種別を列挙

```yaml
itemlists:
  - item: FireCube      # 敵: 火事ブロック
  - item: Sweeper       # タワー: 掃除機
  - item: GarbageCube   # 敵: ゴミキューブ
  - item: WaterTurret   # タワー: 消火塔
  - item: PowerCube     # 敵: 電源ブロック
```

### 利用可能なユニット

[OK] **敵タイプ**:

| ユニット | 説明 | 分類 |
|---------|------|------|
| `FireCube` | 火事ブロック | 敵 |
| `GarbageCube` | ゴミキューブ | 敵 |
| `GarbageCubeBox` | ゴミ箱 | 敵 |
| `PowerCube` | 電源ブロック | 敵 |
| `Litter` | ごみをまく敵（敵ユニット） | 敵 |
| `DustBox` | ダストボックス | 敵 |

[OK] **タワータイプ**:

| ユニット | 説明 | 効果 |
|---------|------|------|
| `Sweeper` | 掃除機タワー | ゴミ掃除 |
| `WaterTurret` | 消火塔 | 火災消火 |
| `SentryGuard` | 監視員タワー | 敵検知 |

[OK] **その他**:

| ユニット | 説明 |
|---------|------|
| `StopPlate` | 止水板（障害物） |

### 注意点

[WARN] **itemlists に含まれないユニットはスポーンできません**

```yaml
itemlists:
  - item: FireCube    # ✓ OK: イベントで spawn 可能
# - item: Litter     # ✗ itemlists に含まれないため spawn_enemy_unit は失敗

events:
  - time: 3
    event: spawn_unit
    value: FireCube, 4, 2, 4      # ✓ OK
```

---

### 4. Pathmakers セクション（敵経路マーカー）

**用途**: 敵の進路をマーク。敵ユニットはこのマーカー順に移動します。

```yaml
pathmakers:
  - name: path_marker_start     # 開始地点（"start" を含む）
    pos: 5, 0, 5                # X, Y, Z 座標（Y に auto 指定可能）
  - name: path_marker_01
    pos: 10, auto, -6           # Y = auto → Raycast で地面高さを自動検出
  - name: path_marker_goal      # ゴール地点（"goal" を含む）
    pos: 15, 0, 5
```

### pos フィールドの Y 座標指定

| 値 | 説明 |
|-----|------|
| `0.0` など数値 | そのまま使用 |
| `auto` | その (X, Z) 位置の地面を Raycast (上方から下方向) で自動検出 |

[NOTE] `auto` は大文字小文字を問わず有効（`Auto` / `AUTO` も可）

[NOTE] Raycast で地面が検出できない場合は Y=0 を使用

**使用例** (高低差のある PLATEAU 3D マップ):
```yaml
pathmakers:
  - name: path_marker_start
    pos: -1.8, auto, 140.4    # Y は地形に合わせて自動計算
  - name: path_marker_goal
    pos: 30.0, auto, 135.0
```

### マーカー命名規則

[OK] **特殊マーカー**:

- **`*_start`**: 敵スポーン地点（プレイ画面で強調表示）
- **`*_goal`**: ゴール地点（プレイ画面で強調表示）

[OK] **一般マーカー**:

- 名前形式: `path_marker_<番号>` または任意の英数字
- 同名マーカーが既存する場合、**位置のみ更新** される

### イベント内での参照

pathmakers はイベントで参照されます：

```yaml
events:
  - time: 0
    event: bloom_path          # マーカーをハイライト
    value: path_marker_start, path_marker_01, path_marker_goal

  - time: 3
    event: spawn_enemy_unit    # 敵を経路沿いでスポーン
    value: Litter, path_marker_start, path_marker_01, path_marker_goal
```

---

### 5. Goals セクション（ステージクリア条件）

**用途**: ステージをクリアするための条件を定義

```yaml
goals:
  - notfailtime: 100            # 100 秒間敗北しなかったらクリア
  - building: repair_all        # すべての建物を修復したらクリア
  - garbage: 10, 20             # ゴミが 10 個を超えず、20 秒経過でクリア
```

### Goal タイプ一覧

| タイプ | パラメータ | 説明 |
|--------|----------|------|
| `notfailtime` | `<秒数>` | 指定時間敗北せずに経過 |
| `building` | `repair_all` | すべての建物を修復 |
| `garbage` | `<上限>, <時間>` | ゴミ < 上限かつ時間経過 |

[NOTE] **複数 goals が定義できます**。任意の 1 つを達成するとクリア。

```yaml
goals:
  - notfailtime: 60             # クリア条件 1: 60 秒生き残る
  - garbage: 5, 30              # クリア条件 2: ゴミ < 5 かつ 30 秒経過
```

---

### 6. Gameovers セクション（ゲームオーバー条件）

**用途**: ゲームオーバーになる条件を定義

```yaml
gameovers:
  - garbage: 10                 # ゴミが 10 個を超えたらゲームオーバー
  - building: damaged           # 建物が破壊されたらゲームオーバー
```

### Gameover タイプ一覧

| タイプ | パラメータ | 説明 |
|--------|----------|------|
| `garbage` | `<数値>` | ゴミ数が指定値を超過 |
| `building` | `damaged` | 建物が破壊される |
| `base` | `destroyed` | 基地が破壊される |

[WARN] **gameovers セクションがない場合のデフォルト**

```
デフォルト敗北条件: ゴミ数 > 20
```

---

### 7. Events セクション（ゲーム進行）

**用途**: ゲーム進行中に発生するイベントを時系列で定義

```yaml
events:
  - time: 0
    event: weather
    value: sunny, 0.00, 0.25, 500    # 天候, 雨強度, 雲強度, 霧視界

  - time: 0
    event: wind
    value: 5, 225                    # 風速, 風向き(度)

  - time: 3
    event: spawn_unit
    value: FireCube, 4, 2, 4         # ユニット名, X, Y, Z

  - time: 5
    event: bloom_path
    value: path_marker_start, path_marker_01, path_marker_goal  # マーカー列挙

  - time: 10
    event: spawn_enemy_unit
    value: Litter, path_marker_start, path_marker_01, path_marker_goal  # 敵, 経路
```

### イベントタイプ一覧

#### 7.1 `weather` イベント

**説明**: 天候を設定

```yaml
value: <weather_type>, <rain_intensity>, <cloud_density>, <fog_distance>
```

| パラメータ | 範囲 | 説明 |
|-----------|------|------|
| `weather_type` | `sunny` / `rain` / `snow` | 天候タイプ |
| `rain_intensity` | 0.0 - 1.0 | 雨の強さ（0 = なし） |
| `cloud_density` | 0.0 - 1.0 | 雲の量 |
| `fog_distance` | 1 - 1000 | 霧の視界距離 |

**例**:
```yaml
- time: 0
  event: weather
  value: rain, 0.65, 0.75, 200    # 雨、雲濃い、霧あり
```

#### 7.2 `wind` イベント

**説明**: 風を設定（敵移動・パーティクル等に影響）

```yaml
value: <wind_speed>, <wind_direction>
```

| パラメータ | 範囲 | 説明 |
|-----------|------|------|
| `wind_speed` | 0 - 20 | 風速 |
| `wind_direction` | 0 - 360 | 風向き（度）※ 北=0°, 東=90°, 南=180°, 西=270° |

**例**:
```yaml
- time: 0
  event: wind
  value: 5, 225    # 風速 5、南西から吹く
```

#### 7.3 `spawn_unit` イベント

**説明**: 固定位置にユニットをスポーン

```yaml
value: <unit_name>, <x>, <y>, <z>
```

[NOTE] 敵・タワー区別なくスポーン可能。座標指定は絶対位置。

**例**:
```yaml
- time: 5
  event: spawn_unit
  value: PowerCube, 4, 2, 4    # PowerCube を (4, 2, 4) にスポーン
```

#### 7.4 `spawn_enemy_unit` イベント

**説明**: パスマーカー経由で敵をスポーン

```yaml
value: <enemy_name>, <marker_start>, <marker_1>, <marker_2>, ..., <marker_goal>
```

**例**:
```yaml
- time: 3
  event: spawn_enemy_unit
  value: Litter, path_marker_start, path_marker_01, path_marker_02, path_marker_goal
```

[NOTE] 敵は `path_marker_start` から出発し、各マーカーを順に訪問して `path_marker_goal` へ向かいます。

#### 7.5 `bloom_path` イベント

**説明**: マーカーをハイライト（UI で表示）

```yaml
value: <marker_1>, <marker_2>, ...
```

**例**:
```yaml
- time: 0
  event: bloom_path
  value: path_marker_start, path_marker_03, path_marker_goal
```

[NOTE] UI 上で敵経路を可視化。敵移動に影響しません。

#### 7.6 `off_bloom_path` イベント

**説明**: ハイライト表示を消す

```yaml
value: <marker_1>, <marker_2>, ...
```

**例**:
```yaml
- time: 20
  event: off_bloom_path
  value: path_marker_start, path_marker_03, path_marker_goal
```

---

## バリデーション規則

### YAML パース

[OK] **要件**:
- YamlDotNet で正常にパース可能
- コメント（`#`） で説明可能

[NG] **エラー**:
- インデント不正（スペース数不正）
- リスト形式の混在
- 無効な YAML 構文

### セマンティック検証

| 検査項目 | 基準 | 警告条件 |
|---------|------|--------|
| **stageid** | 一意性 | 重複する場合はエラー |
| **itemlists** | 有効なユニット名 | 未定義のユニット参照はエラー |
| **pathmakers** | 座標が数値または `auto` | 無効な座標はエラー |
| **events.time** | 非負数 | 負数はエラー |
| **events.value** | 形式一致 | 形式不一致はエラー |
| **goals** | 目標定義 | goals も gameovers も定義がない場合は警告 |

### テストツール

[OK] **検証スクリプト**: `Assets/Scripts/UnitTest/YamlFileValidationTest.cs`

```csharp
// Play Mode で実行
// StreamingAssets/staging/ 内のすべて YAML をチェック
```

---

## サンプルファイル

### 最小ステージ例

```yaml
---
stagename: "チュートリアル"
stagenotice: "基本的なゲームプレイを学ぶ"
stageid: Tutorial
ver: 1.0.0

stages:
  - name: Tutorial
    note: "チュートリアルステージ"
    BIT: 100
    CLK: 300

itemlists:
  - item: FireCube
  - item: Sweeper

pathmakers:
  - name: path_marker_start
    pos: 0, 0, 0
  - name: path_marker_goal
    pos: 10, 0, 0

goals:
  - notfailtime: 60

gameovers:
  - garbage: 5

events:
  - time: 0
    event: weather
    value: sunny, 0.0, 0.0, 1000

  - time: 0
    event: wind
    value: 0, 0

  - time: 5
    event: spawn_unit
    value: FireCube, 5, 0, 5

  - time: 10
    event: spawn_enemy_unit
    value: FireCube, path_marker_start, path_marker_goal
```

### 複雑なステージ例

```yaml
---
stagename: "高難度ステージ"
stagenotice: "複数敵タイプとマルチパス攻撃"
stageid: AdvancedStage
ver: 1.0.0

stages:
  - name: AdvancedStage
    note: "難易度：上級"
    BIT: 5000
    CLK: 600

itemlists:
  - item: FireCube
  - item: GarbageCube
  - item: PowerCube
  - item: Sweeper
  - item: WaterTurret
  - item: StopPlate

pathmakers:
  - name: path_marker_start
    pos: 0, 0, 0
  - name: path_marker_01
    pos: 10, 0, 0
  - name: path_marker_02
    pos: 20, 0, 0
  - name: path_marker_goal
    pos: 30, 0, 0

goals:
  - notfailtime: 120
  - garbage: 15, 90

gameovers:
  - garbage: 30
  - building: damaged

events:
  - time: 0
    event: weather
    value: rain, 0.5, 0.6, 300

  - time: 0
    event: wind
    value: 8, 180

  - time: 5
    event: bloom_path
    value: path_marker_start, path_marker_01, path_marker_02, path_marker_goal

  - time: 10
    event: spawn_enemy_unit
    value: FireCube, path_marker_start, path_marker_01, path_marker_02, path_marker_goal

  - time: 20
    event: spawn_enemy_unit
    value: GarbageCube, path_marker_start, path_marker_01, path_marker_goal

  - time: 60
    event: spawn_unit
    value: PowerCube, 15, 0, 15
```

---

## 互換性・バージョン管理

### 現在のバージョン

**yaml-format.md v1.0.0**

- すべてのセクションが実装完了
- YAML DotNet でのバリデーション確立

### 将来予定

**v1.1.0 以降**:
- `BIT` / `CLK` 機能の実装
- `solar` イベント（太陽高度制御）の実装
- データバインディングシステムの追加
- ステージエディタ UI の実装

---

## 実装リファレンス

### YAML セクションキー定義

YAML のトップレベルセクション名（`goals`, `events` 等）は C# コード内で以下に一元管理されています。

**ファイル**: `Assets/Scripts/Core/CommandProcessing/YamlSectionType.cs`

```csharp
// YAML トップレベルセクションキー
internal static class YamlSectionKeys
{
    internal const string Stages      = "stages";
    internal const string StageNotice = "stagenotice";
    internal const string ItemLists   = "itemlists";
    internal const string PathMakers  = "pathmakers";
    internal const string Goals       = "goals";
    internal const string GameOvers   = "gameovers";
    internal const string Events      = "events";
    internal const string Boards      = "boards";
}
```

[NOTE] YAML セクション名を変更する場合は、`YamlSectionKeys` の定数値を修正するだけで機能します。

### Y 座標 auto キーワード定義

`pos` フィールドの `auto` キーワードは以下で定義されています。

**ファイル**: `Assets/Scripts/Core/CommandProcessing/YamlSectionType.cs`

```csharp
internal static class YamlValueKeywords
{
    // Y 座標を Raycast で自動検出するキーワード（例: pos: 0, auto, 135）
    internal const string AutoHeight = "auto";
}
```

**処理場所**: `Assets/Scripts/Core/Utilities/CommonsCalcs.cs` の `Utility.ParseVector3WithAutoHeight()`

---

## トラブルシューティング

### よくあるエラー

#### [NG] Error: Scene 'StageName' couldn't be loaded

**原因**: `stageid` がステージ名と一致していない

**解決**: YAML の `stageid` をステージファイル名と同じにする

```yaml
# UnitEnemy.yaml の場合
stageid: UnitEnemy    # ✓ ファイル名と同じ
```

#### [NG] Null reference when spawning unit

**原因**: `itemlists` に定義されていないユニットを spawn しようとしている

**解決**: イベントで参照するユニットをすべて `itemlists` に追加

```yaml
itemlists:
  - item: FireCube      # ✓ 後で spawn で使用可能
  
events:
  - time: 5
    event: spawn_unit
    value: FireCube, ...  # ✓ OK
```

#### [NG] Enemy doesn't follow path

**原因**: `spawn_enemy_unit` のマーカー名が `pathmakers` に定義されていない

**解決**: `pathmakers` セクションでマーカーを定義してから参照

```yaml
pathmakers:
  - name: path_marker_goal   # ✓ 定義

events:
  - time: 5
    event: spawn_enemy_unit
    value: FireCube, path_marker_start, path_marker_goal  # ✓ OK
```

---

## 関連ドキュメント

- [README.md](README.md) - reference 層概要
- [data-models.md](data-models.md) - ゲームデータモデル
- [architecture/asset-management.md](../architecture/asset-management.md) - Prefab 管理
- [YAML バリデーションテスト](../../Assets/Scripts/UnitTest/YamlFileValidationTest.cs)
