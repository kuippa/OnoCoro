# AGENTS.md - AI Agent 最上位ルール

このファイルは、GitHub Copilot および AI Agent がこのプロジェクトで作業する際に従うべき最上位ルールを定義します。
---

## 重要: セッション情報の掲示（必須）

**すべての AI Agent は応答の最初に以下の情報を掲示する必要があります:**

```
**Model**: [モデル名（例: Claude Haiku 4.5）]
**Type**: [エージェントタイプ（Fixed / Auto）]
**Session**: [セッション状態（連続中 / 新規開始）]
```

**目的**:
- ユーザーが現在のエージェント状態を判断できるようにする
- セッションがリセットされた場合の判断根拠を提供する
- ドキュメント情報の読み込みが失われていないか確認可能にする

**例**:
```
**Model**: Claude Haiku 4.5
**Type**: Fixed
**Session**: 連続中（AGENTS.md, coding-standards.md 読み込み済み）
```

---
> **関連ドキュメント**:
> - [introduction.md](../docs/introduction.md) - プロジェクトの概要・目的・非目的
> - [architecture.md](../docs/architecture.md) - システム全体のアーキテクチャ
> - [coding-standards.md](../docs/coding-standards.md) - C#実装・Unity設計の詳細規約
> - [instructions.md](instructions.md) - プロジェクト管理・運用ガイド

---

## プロジェクト概要

**OnoCoro** は、Unity 6.3 で実装された地理情報可視化アプリケーション（タワーディフェンスゲーム）です。PLATEAU SDK（日本の都市3Dデータ標準フォーマット対応）を使用します。

**重要な背景**: このプロジェクトは SSD故障による2年前のバックアップから段階的に復旧されたものです。

詳細は [introduction.md](../docs/introduction.md) を参照してください。

---

## 技術スタック（固定・変更不可）

### 必須技術

| 技術 | バージョン | 用途 |
|------|-----------|------|
| **Unity** | 6.3 | ゲームエンジン |
| **C#** | 最新 | プログラミング言語 |
| **PLATEAU SDK** | 最新 | 地理データ処理 |
| **Cinemachine** | Unity標準 | カメラ制御 |
| **glTFast** | Unity標準 | 3Dモデルロード |
| **Unity Input System** | Unity標準 | 入力管理 |

### 禁止事項

- ❌ **Unity バージョンの変更を提案しない**
- ❌ **PLATEAU SDK の削除を提案しない**
- ❌ **外部フレームワーク（React, Vue, Angular等）の導入を提案しない**
- ❌ **JavaScript/TypeScript への移行を提案しない**
- ❌ **Python スクリプトでの Unity コード生成を提案しない**

---

## Windows PowerShell 環境（固定）

### 実行環境

このプロジェクトは **Windows 環境**を前提としています。

- ✅ **使用可能**: PowerShell コマンド
- ❌ **使用不可**: Linux/macOS の bash コマンド（`ls`, `grep`, `wc`, `du`, `sed`, `awk`, `cat`等）

### Windows PowerShell での代替コマンド

| Linux/macOS | Windows PowerShell |
|-------------|-------------------|
| `ls -la` | `Get-ChildItem -Force` |
| `grep pattern` | `Select-String "pattern"` |
| `wc -l` | `Measure-Object -Line` |
| `du -sh` | `(Get-ChildItem -Recurse \| Measure-Object -Property Length -Sum).Sum / 1GB` |
| `cat file` | `Get-Content file` |
| `find . -name "*.cs"` | `Get-ChildItem -Recurse -Filter "*.cs"` |
| `rm -rf folder` | `Remove-Item -Path folder -Recurse -Force` |

**重要**: Linux/macOS コマンドを提案しないこと。必ず PowerShell コマンドレットを使用すること。

---

## コーディング規約（厳守）

> **詳細**: 完全なコーディング規約は [coding-standards.md](../docs/coding-standards.md) を参照してください。
> ここでは要点のみを記載します。

### 1. 名前空間の衝突回避

**必須**: `Debug` クラスは必ず Unity のものを使用すること

```csharp
using System.Diagnostics;
using Debug = UnityEngine.Debug;  // 必須

// OK: Unity のログ
Debug.Log("Unity のデバッグログ");

// OK: System.Diagnostics は完全修飾名で
Process.Start("notepad.exe");
```

### 2. マジックナンバー・マジックストリングの禁止

**必須**: 数値・文字列リテラルは必ず定数化すること

**例外規定**: 以下の場合のみ直値を許可
- 一度しか使われない値で、その後の処理が成立しないような必須のオブジェクト名
- 再利用や変更の予定がない値

```csharp
// ❌ NG
if (count > 10)
{
    Debug.Log("Too many items");
}

// ✅ OK
private const int MAX_ITEM_COUNT = 10;
private const string MSG_TOO_MANY_ITEMS = "Too many items";

if (count > MAX_ITEM_COUNT)
{
    Debug.Log(MSG_TOO_MANY_ITEMS);
}

// ✅ OK（例外: 一度しか使わない）
private void Initialize()
{
    _loading = UIHelper.FindOrInstantiatePrefab("nowloading", path, missingObjects);
}
```

**定数の命名規則**:
- プライベート定数: `_CONSTANT_NAME` (アンダースコア + 大文字スネークケース)
- パブリック定数: `CONSTANT_NAME` (大文字スネークケース)

### 3. 制御文の中括弧 {} を必須とする

**必須**: すべての `if`, `else`, `for`, `while`, `foreach` に中括弧を使用すること

```csharp
// ❌ NG
if (condition)
    DoSomething();

// ✅ OK
if (condition)
{
    DoSomething();
}
```

### 4. 三項演算子とnull条件演算子の使用禁止

**禁止**: `? :` と `?.` は使用しないこと

```csharp
// ❌ NG
int result = (x > 0) ? 10 : -10;
component?.DoSomething();

// ✅ OK
int result;
if (x > 0)
{
    result = 10;
}
else
{
    result = -10;
}

if (component != null)
{
    component.DoSomething();
}
```

### 5. 関数の行数制限

**必須**: 1つの関数は40行以内に収めること

40行を超える場合は、機能ごとに分割すること。

```csharp
// ✅ OK: 分割された関数
void Awake()
{
    List<string> missingObjects = new List<string>();
    
    InitializeLoadingCanvas(missingObjects);
    InitializePanels(missingObjects);
    RegisterButtonListeners(missingObjects);
    
    CheckMissingObjects(missingObjects);
}

private void InitializeLoadingCanvas(List<string> missingObjects)
{
    // 10-20行程度の初期化処理
}
```

### 6. Early Return パターンの徹底使用

**必須**: ネストした `if` 文を避け、ガード句（Early Return）を使用すること

```csharp
// ❌ NG: ネストしたif
if (transform.parent != null)
{
    if (someCondition)
    {
        if (anotherCondition)
        {
            // 正常系の処理
        }
    }
}

// ✅ OK: Early Return
if (transform.parent == null)
{
    Debug.LogWarning("No parent found");
    return;
}

if (!someCondition)
{
    return;
}

if (!anotherCondition)
{
    return;
}

// 正常系の処理（ネストなし）
```

### 7. 変数名の命名規則

**必須**: 意味のある名前を使用し、一時的な名前を避けること

```csharp
// ❌ NG
GameObject obj;
Transform t;
int temp;
Button btn;

// ✅ OK
GameObject canvasObject;
Transform contentTransform;
int calculatedValue;
Button startButton;
```

**例外的に許可される短い変数名**:
- ループカウンタ: `i`, `j`, `k`
- 座標: `x`, `y`, `z`

### 8. ユーティリティクラスの使用

**必須**: 関連する機能は適切なユーティリティクラスに集約すること

| ユーティリティクラス | 責務 |
|-------------------|------|
| `LoadStreamingAsset` | StreamingAssetsフォルダ関連操作 |
| `UIHelper` | UI操作（GameObject検索、ボタン登録、スクロール制御） |
| `FileOperationUtility` | ファイル操作（エディタで開く、画像読み込み） |
| `StageDataManager` | ステージデータ管理 |
| `LogUtility` | ログ管理 |

```csharp
// ❌ NG: 直接実装
string fileName = Path.GetFileName(sceneName + ".yaml");
if (File.Exists(LoadStreamingAsset.StageFilePath(fileName)))
{
    // ...
}

// ✅ OK: ユーティリティ使用
string fileName = LoadStreamingAsset.GetYamlFileName(sceneName);
if (LoadStreamingAsset.YamlFileExists(sceneName))
{
    // ...
}
```

### 9. UIスクロールビューの取り扱い

**必須**: スクロール位置制御は `ScrollRect.normalizedPosition` を使用すること

```csharp
// ❌ NG: Scrollbar.value を直接操作
_scrollbar.value = 1f;

// ✅ OK: UIHelper を使用
UIHelper.ResetScrollbarInPanel(_pnlStageSelector);

// UIHelper の実装
public static void ResetScrollbarInPanel(GameObject parentPanel)
{
    if (parentPanel == null || !parentPanel.activeSelf)
    {
        return;
    }
    
    ScrollRect scrollRect = parentPanel.GetComponentInChildren<ScrollRect>(true);
    if (scrollRect != null)
    {
        scrollRect.normalizedPosition = new Vector2(0, 1); // x=0(左), y=1(上)
    }
}
```

---

## マージ作業時のドキュメント読み込み

**必須**: マージ・リファクタリング作業を開始する際は、以下のドキュメントを**必ず最初に読み込む**こと。

| ドキュメント | 場所 | 読み込みタイミング |
|-------------|------|------------------|
| AGENTS.md（このファイル） | `.github/AGENTS.md` | すべてのマージ・編集作業前 |
| coding-standards.md | `docs/coding-standards.md` | すべてのマージ・編集作業前 |
| architecture.md | `docs/architecture.md` | 新規クラス設計・大規模リファクタリング前 |
| introduction.md | `docs/introduction.md` | プロジェクト方針の確認時 |

**セッション開始時に読み込みが完了したら、応答に掲示すること**:
```
**Session**: 連続中（AGENTS.md, coding-standards.md 読み込み済み）
```

---

## Recovery フォルダからのマージルール

**重要**: リバースエンジニアリングで復元したコードをマージする際は、以下のルールを厳守すること

### 0. 機能的な変更がない場合はコードを変更しない（最優先）

**原則**: マージ作業で**機能的な変更がない場合は、元のコードを変更しないこと**。

軽微なリファクタリング（usingの削除、フォーマット調整など）であっても、機能的な変更がない場合は実施しない。

```csharp
// ❌ NG: 機能的な変更がないのに削除
// 変更前：
using System.Collections;
using UnityEngine;

// 変更後（リファクタリング）：
using UnityEngine;

// ⚠️ 理由: using削除は機能的な変更がないため、変更すべきでない

// ✅ OK: 機能的な改善がある場合のみマージ
// 例：nullチェックの追加、定数化、重要なバグ修正
```

**迷ったら**: マージをスキップし、ユーザーに判断を仰ぐこと。

### 1. 変数の初期化を維持する


デフォルト値であっても、明示的な初期化は**削除しない**こと。

```csharp
// ✅ OK: 初期化を維持
public static GameTimerCtrl instance = null;
public float _time = 0.0f;

// ❌ NG: 初期化を削除
public static GameTimerCtrl instance;
public float _time;
```

### 2. `this` を維持する（`base` を使わない）

**必須**: MonoBehaviourのメンバーアクセスには必ず`this`を使用すること

```csharp
// ✅ OK: this.gameObject と this.gameObject.transform を明示
_text = this.gameObject.GetComponent<TextMeshProUGUI>();
Button btn = this.gameObject.transform.Find("Panel/Button").GetComponent<Button>();
this.gameObject.SetActive(false);

// ❌ NG: base を使用
_text = base.gameObject.GetComponent<TextMeshProUGUI>();

// ❌ NG: this を省略
_text = gameObject.GetComponent<TextMeshProUGUI>();

// ❌ NG: this.transform を直接使用（this.gameObject.transform とすること）
Button btn = this.transform.Find("Panel/Button").GetComponent<Button>();
```

**理由**: 
- `this`の明示により、インスタンスメンバーであることが明確になる
- `this.gameObject.transform`とすることで、GameObjectを経由してTransformにアクセスしていることが明確になる
- `base`はC#の予約語だが、MonoBehaviourでは意味が曖昧
- `gameObject`の省略形は可読性を下げる

### 3. コメントを削除しない

既存のコメント（コメントアウトされたコードを含む）は、マージ時に**削除しない**こと。

### 4. 明確な差分がない場合はマージをパスする

**マージをスキップする基準**:
- コード構造が実質的に同じで、差分が軽微な場合
- リファクタリングによる改善効果が小さい場合
- 既存コードが十分に動作しており、変更リスクの方が高い場合

**マージすべき重要な差分**:
- 新しいメソッドや機能の追加
- 重要なバグ修正（nullチェック、ロジック修正など）
- パフォーマンス改善
- 未実装機能の実装（TODOの解消）

**迷ったら**: マージをスキップし、ユーザーに判断を仰ぐこと。

---

## Git ワークフロー

### ブランチ戦略

- `main`: 安定版リリース用ブランチ
- `develop`: 開発用ブランチ
- `feature/*`: 機能追加用ブランチ
- `bugfix/*`: バグ修正用ブランチ

### コミットメッセージ規約

```
<type>(<scope>): <subject>

<body>

<footer>
```

**type の種類**:
- `feat`: 新機能
- `fix`: バグ修正
- `docs`: ドキュメント
- `style`: コード整形（意味の変更なし）
- `refactor`: コード整理
- `perf`: パフォーマンス改善
- `test`: テストコードの追加・修正
- `chore`: ビルドやツール設定の変更

---

## AI Agent が提案を行う前のチェックリスト

コードを提案する際は、以下を必ず確認すること:

- [ ] **定数化**: マジックナンバー・マジックストリングがないか
- [ ] **中括弧**: すべての制御文に`{}`があるか
- [ ] **三項演算子**: `? :`や`?.`を使用していないか
- [ ] **Early Return**: ネストした`if`を避け、ガード句を使用しているか
- [ ] **関数の長さ**: 40行以内に収まっているか
- [ ] **変数名**: 意味のある名前になっているか
- [ ] **ユーティリティの使用**: 共通処理を適切なユーティリティクラスに集約しているか
- [ ] **ScrollRect**: スクロール位置制御は`normalizedPosition`を使用しているか
- [ ] **PowerShell**: Linux/macOSコマンドを使用していないか

**規約違反がある場合は、修正してから提案すること。**

---

## データ保護ルール

このプロジェクトは SSD 故障からの復旧プロジェクトです。

### 必須事項

- ✅ **Git に頻繁にコミットすること**
- ✅ **大容量ファイルを追加する前に相談すること**
- ✅ **`.gitignore` の運用ルールを遵守すること**

### 禁止事項

- ❌ **Library, Temp, Obj フォルダを Git に追加しない**
- ❌ **100MB 以上のファイルを追加しない**（事前相談必須）
- ❌ **バイナリ形式（.blend, .fbx, .psd等）を安易に追加しない**

---

## 最後に

このルールは、AI Agent が暴走せず、プロジェクトの一貫性を保つために設けられています。
ルールに違反する提案や、技術スタックの変更を伴う提案は行わないでください。

不明な点がある場合は、ユーザーに確認を求めてください。
