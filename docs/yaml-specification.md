# OnoCoro YAML ファイル仕様書

ゲームステージ・ユニット・レベルデータを定義する YAML フォーマットの運用ドキュメント

**最終更新**: 2026-01-28  
**仕様バージョン**: 2.0.0（実装に基づいて統一）  
**対象ファイル**: Assets/StreamingAssets/staging/*.yaml  
**パーサー**: StageYamlRepository（YamlDotNet）  
**イベント処理**: EventLoader.ActionEvent（15 イベントタイプ実装済み）

---

## 目次

1. [概要](#概要)
2. [ファイル構成](#ファイル構成)
3. [セクション詳細](#セクション詳細)
4. [イベントタイプ一覧](#イベントタイプ一覧)
5. [データ型定義](#データ型定義)
6. [バリデーション規則](#バリデーション規則)
7. [設計目的](#設計目的)
8. [既知の課題](#既知の課題)

---

## 概要

- Unity 6.3 / YamlDotNet ベース
- パーサー: `StageYamlRepository`（YamlStream で解析）
- イベント処理: `EventLoader.ActionEvent()`（`nameof(YamlEventType.X)` ベースの型安全な dispatch）
- **Goals / GameOvers / Events / Boards**：任意の key-value を許容し、未知フィールドは警告のみで処理続行
- Events の `time` は float で取り込み、複数イベントを同一時刻に登録可能
- Boards は実装途中（値は文字列として保存・取得）

---

## ファイル構成

- 1 YAML = 1 ステージ定義 + ヘッダー
- 配置先: Assets/StreamingAssets/staging/
- 参考サンプル: 以下の「セクション詳細」セクションを参照

---

## セクション詳細

### 1. ヘッダー (必須)

**目的**: ステージの基本メタ情報を定義

```yaml
stagename: "三鷹駅前"
stagenotice: "東京都三鷹市の駅前マップ"
stageid: "Mitaka_Station"
ver: "1.0.0"
```

**フィールド仕様**:

| フィールド | 型 | 必須 | 説明 |
|-----------|---|------|------|
| `stagename` | string | [OK] | UI 表示用のステージ名 |
| `stagenotice` | string | [OK] | ステージ説明 |
| `stageid` | string | [OK] | 一意識別子（英数字と `_` `-` 推奨） |
| `ver` | string | [OK] | セマンティックバージョン |

### 2. stages (必須)

**目的**: ゲーム内で実際にプレイされるステージの定義

```yaml
stages:
  - name: "Mitaka_Stage_01"
    note: "チュートリアルステージ"
    BIT: 5000
    CLK: 5100
```

**フィールド仕様**:

| フィールド | 型 | 必須 | 説明 |
|-----------|---|------|------|
| `name` | string | [OK] | ステージ内部識別子（ユニーク推奨） |
| `note` | string | [OK] | ステージの簡潔な説明 |
| `BIT` | integer | [OK] | 初期ビットレート |
| `CLK` | integer | [OK] | 初期クロック |

### 3. itemlists (必須)

**目的**: このステージで配置・使用可能なユニット一覧

```yaml
itemlists:
  - item: "SentryGuard"
  - item: "Sweeper"
  - item: "Litter"
```

| フィールド | 型 | 必須 | 説明 |
|-----------|---|------|------|
| `item` | string | [OK] | ユニット/敵のクラス名 |

### 4. pathmakers (必須)

**目的**: ゲーム上のマーカー（敵の進路上の重要地点）を定義

```yaml
pathmakers:
  - name: "path_marker_start"
    pos: "5, 0, 5"
  - name: "path_marker_01"
    pos: "10, 0, -6"
  - name: "path_marker_goal"
    pos: "50, 0, 20"
```

| フィールド | 型 | 必須 | 説明 |
|-----------|---|------|------|
| `name` | string | [OK] | マーカー名（ユニーク必須） |
| `pos` | string | [OK] | `x, y, z` 形式の座標文字列 |

**特別な命名規則**:
- `start` を含む名前 → スポーン地点（特殊色自動表示）
- `goal` を含む名前 → ゴール地点（特殊色自動表示）
- その他 → 中継地点

### 5. goals (任意)

**目的**: ステージクリア条件を定義

**設計**: 任意の key-value ペアを受け入れる（拡張性重視）

```yaml
goals:
  - notfailtime: 100
  - building: repair_all
  - garbage: 0
```

**実装上の特性**:
- [OK] `StageYamlRepository.SetGoalsRequirements()` は任意の key-value を受け入れ
- [OK] `StageGoalController._dict_req` に辞書として格納
- [OK] 未知フィールドは警告のみで処理続行

### 5.5 gameovers (任意)

**目的**: ゲームオーバー条件を定義

**設計**: 任意の key-value ペアを受け入れる（拡張性重視）

```yaml
gameovers:
  - garbage: 10
  - health: 0
  - enemy_reached_goal: true
```

**実装上の特性**:
- [OK] `StageYamlRepository.SetGameOversRequirements()` は任意の key-value ペアを受け入れ
- [OK] `StageGoalController._dict_fail` に辞書として格納
- [OK] 未知フィールドは警告のみで処理続行

### 6. events (任意)

**目的**: ステージ経過時間ベースでイベントを発火させる

**設計**: `time`（float）をキーに、イベント種別と任意パラメータを登録

```yaml
events:
  - time: 0
    event: weather
    value: sunny, 0.00, 0.25, 500

  - time: 3
    event: spawn_enemy_unit
    value: Litter, path_marker_start, path_marker_goal

  - time: 10.5
    event: wind
    value: 5, 225
```

**フィールド仕様**:

| フィールド | 型 | 必須 | 説明 |
|-----------|---|------|------|
| `time` | float | [OK] | イベント発火時刻（秒） |
| `event` | string | [OK] | イベント種別（15 タイプ実装済み - 下表参照） |
| `value` | string | 任意 | イベント固有パラメータ |

**実装上の特性**:
- [OK] `time` は float に変換され、同一時刻の複数イベント登録可能
- [OK] `event` 値は EventLoader の YamlEventType enum と照合される
- [OK] パラメータは `Dictionary<string, string>` として各イベントハンドラに渡される
- [OK] 未知の event タイプはスキップされる（警告出力）

### 6.1 boards (任意・実装途中)

**目的**: ゲームボード・掲示板関連の設定

**現行の仕様**:

```yaml
boards:
  - code: ReadMeText1
    text: "掲示板のある場所にむかってください。"
  - code: ReadMeText2
    text: "SHIFTキーを押しながら移動すると、はやく移動できます。"
  - code: BoardSize
    text: "256x256"
```

**フィールド仕様**:

| フィールド | 型 | 必須 | 説明 |
|-----------|---|------|------|
| `code` | string | [OK] | ボード設定のキー (BoardSize / SpawnPoints / MaxWaves / DifficultyModifier など) |
| `text` | string | [OK] | 設定値（文字列として保存・取得） |

**実装上の特性**:
- [OK] BoardCommand は `code` → ConfigType（enum）に変換
- [OK] 値は文字列として `EventLoader._board_data` に保存
- [OK] 取得時は `EventLoader.GetBoardText(board_code)` で文字列として返却
- [NOTE] 値が実装側で型変換・消費されるかどうかは実装に依存（SignboardCtrl など）
- [NOTE] SpawnPoints / DifficultyModifier は値として保存されるが、現在ゲームロジックで消費されていない

---

## イベントタイプ一覧

### [OK] 実装済みイベントタイプ（EventLoader で処理）

**Note**: 実装と YamlEventType enum は 2026-01-28 に統一されました。以下の 15 タイプのみが有効です。

#### 環境・気象イベント

| イベント | パラメータ | 処理機能 | コード例 |
|---------|----------|--------|--------|
| `weather` | weather_type, strength, cloudStrength, fogStrength | WeatherController で天候変更 | `weather, sunny, 0.00, 0.25, 500` |
| `solar` | 太陽高度 (0-1) | WeatherController で時間帯変更 | `solar, 0.75` |
| `wind` | windSpeed, windDirection | WindController で風設定 | `wind, 5, 225` |
| `watersurface` | 水面高度（メートル） | WaterSurfaceCtrl で水面上昇 | `watersurface, 10.5` |

#### 自然災害イベント

| イベント | パラメータ | 処理機能 | コード例 |
|---------|----------|--------|--------|
| `earthquake` | スケール値 | Earthquake コンポーネント で地震実行 | `earthquake, 2.5` |
| `building_break` | all / 建物ID | BuildingBreak で建物破壊 | `building_break, all` |

#### UI・通知イベント

| イベント | パラメータ | 処理機能 | コード例 |
|---------|----------|--------|--------|
| `notice` | メッセージ | NoticeCtrl で通知表示 | `notice, ゲームを再開しました` |
| `telop` | テロップテキスト | TelopCtrl で大型テロップ表示 | `telop, 敵の大波が襲来！` |
| `subtelop` | サブテロップテキスト | TelopCtrl で小型テロップ表示 | `subtelop, 5 秒後に再開します` |

#### パス・ビジュアルイベント

| イベント | パラメータ | 処理機能 | コード例 |
|---------|----------|--------|--------|
| `bloom_path` | マーカー名（カンマ区切り） | BloomPathController でパス強調表示 | `bloom_path, path_marker_start, path_marker_01, path_marker_goal` |
| `off_bloom_path` | マーカー名（カンマ区切り） | BloomPathController でパス強調解除 | `off_bloom_path, path_marker_start, path_marker_01` |

#### スポーン関連イベント

| イベント | パラメータ | 処理機能 | コード例 |
|---------|----------|--------|--------|
| `spawn_unit` | UnitName, X, Y, Z / random_position / random_doom_building | SpawnController でユニット配置 | `spawn_unit, SentryGuard, 10, 0, 10` |
| `spawn_enemy_unit` | EnemyName, marker1, marker2, ... | SpawnController で敵配置 | `spawn_enemy_unit, Litter, path_marker_start, path_marker_goal` |

#### 未実装イベント

| イベント | パラメータ | 状態 |
|---------|----------|------|
| `volcano` | - | YamlEventType で定義済みだが未実装 |

---

## データ型定義

### スカラー型

| 型 | 例 | 備考 |
|---|---|------|
| integer | 0, 10, 5000 | 32-bit 相当を想定 |
| float | 0.0, 3.5, 10.5 | 小数は `.` 区切り |
| string | "text" | UTF-8 文字列 |
| boolean | true / false | 小文字表記 |

### 複合型

#### Array（リスト）

```yaml
itemlists:
  - item: "UnitName1"
  - item: "UnitName2"
```

#### Mapping（マップ）

```yaml
goals:
  - notfailtime: 100
  - garbage: 0
```

### 座標フォーマット

- `pos` は `"x, y, z"` の文字列。`,` 区切りで空白許容。
- 例: `"35.703, 139.56, 50.0"`

---

## バリデーション規則

**Header**:
- `stageid` は空文字禁止。英数字と `_` `-` の使用を推奨（重複は運用で防止）。
- `ver` は `X.Y.Z` 形式推奨。

**Stages**:
- `BIT` / `CLK` は 1 以上の整数。

**Pathmakers**:
- `start` と `goal` を含むマーカーが各 1 つ必須。

**Events**:
- `time` は float 変換に失敗するとスキップされる。
- `event` は YamlEventType enum の 15 値のいずれかであることが期待される。不正な値はスキップ。
- `value` は各イベントハンドラが独自にパースする。

**Boards**:
- `code` が `BoardSize` の場合のみ `"WxH"` 形式の値が期待される（例: `"256x256"`）。
- `SpawnPoints` / `DifficultyModifier` / `MaxWaves` は値として文字列で保存される（現在型チェックなし）。

**全般**:
- Goals / GameOvers / Events / Boards は未知フィールドを警告のみで許容。

---

## 設計目的

- **実装ドリブン**: YAML スキーマは EventLoader.cs など実装コードに基づいて定義
- **回復フェーズ**: スキーマを緩く保ち、実装が拡張されても YAML 互換性を維持
- **拡張性**: 条件追加やイベント種別追加をユーザー側で自由に試せるよう、辞書ベース設計
- **型安全性**: YamlEventType enum（2026-01-28 統一）により、EventLoader のイベント dispatch は `nameof()` ベースで型チェック完備

---

## 既知の課題

### 課題 1: Boards 仕様未確定

- 現行: 値は文字列として保存・取得
- 課題: SpawnPoints / DifficultyModifier は値が保存されるが、ゲームロジックで消費されていない
- 将来: 実装側で型変換・消費される場合は validation を追加予定

### 課題 2: YAML パーサーのエラーハンドリング

- 簡単な try-catch のみ
- ファイル名・行番号・原因を詳細に表示する YamlValidator 実装予定

### 課題 3: ファイル重複（Resources vs StreamingAssets）

- 同期ミスのリスク
- StreamingAssets のみに統一化予定（Phase 1.2）

---

## 関連コード

| ファイル | 責務 |
|---------|------|
| [YamlSectionType.cs](../Assets/Scripts/Core/CommandProcessing/YamlSectionType.cs) | YamlEventType enum（15 イベントタイプ定義） |
| [EventLoader.cs](../Assets/Scripts/Game/Events/System/EventLoader.cs) | ActionEvent() による型安全な dispatch |
| [StageYamlRepository.cs](../Assets/Scripts/Data/Repositories/StageYamlRepository.cs) | YAML パース・コマンド変換 |
| [YamlCommandManager.cs](../Assets/Scripts/Core/CommandProcessing/YamlCommandManager.cs) | コマンド生成・factory パターン |
| [YamlCommandValidationTest.cs](../Assets/Scripts/UnitTest/YamlCommandValidationTest.cs) | バリデーション・イベントテスト |

---

## 関連ドキュメント

| ドキュメント | 目的 |
|------------|------|
| [AGENTS.md](../AGENTS.md) | 開発ガイドライン |
| [architecture.md](architecture.md) | データレイヤーの設計 |
| [introduction.md](introduction.md) | プロジェクト全体の目的 |

---

**最終更新**: 2026-01-28  
**担当**: Recovery Phase チーム  
**バージョン**: 2.0.0（実装統一版）
