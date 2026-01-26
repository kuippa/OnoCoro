# C# コーディング規約・Unity 設計標準

このドキュメントは、OnoCoro プロジェクトにおける C# 実装と Unity 設計の共通ルールを定義します。

**目的**: 実装品質の安定化・判断のブレ防止・保守性の向上

---

## 基本原則

1. **可読性を最優先** - コードは書くよりも読む時間の方が長い
2. **明示的であること** - 暗黙的な動作や省略形を避ける
3. **一貫性を保つ** - プロジェクト全体で統一されたスタイルを維持
4. **単純さを追求** - 複雑な処理は小さな単位に分割する

---

## 1. 名前空間の衝突回避

### Debug クラスの明示的なエイリアス

**必須**: `System.Diagnostics.Debug` と `UnityEngine.Debug` の衝突を避けるため、必ず using エイリアスを使用すること。

```csharp
using System.Diagnostics;
using Debug = UnityEngine.Debug;  // 必須

// OK: Unity のログ
Debug.Log("Unity のデバッグログ");
Debug.LogWarning("警告メッセージ");
Debug.LogError("エラーメッセージ");

// OK: System.Diagnostics は完全修飾名で使用
Process process = Process.Start("notepad.exe");
```

---

## 2. マジックナンバー・マジックストリングの禁止

### 基本ルール

**必須**: 数値・文字列リテラルは必ず名前付き定数として定義すること。

### 例外規定

以下の場合のみ直値の使用を許可:
- **一度しか使われない値**で、その後の処理が成立しないような必須のオブジェクト名
- 再利用や変更の予定がない値
- 定数化することで逆にコードが読みにくくなる場合

### 悪い例と良い例

```csharp
// ❌ NG: マジックナンバー・マジックストリング
if (count > 10)
{
    Debug.Log("Too many items");
}
GameObject canvas = GameObject.Find("PlayerCanvas");
float speed = 9.8f;
transform.position = new Vector3(0, 1.5f, 0);

// ✅ OK: 定数化
private const int MAX_ITEM_COUNT = 10;
private const string MSG_TOO_MANY_ITEMS = "Too many items";
private const string OBJ_PLAYER_CANVAS = "PlayerCanvas";
private const float GRAVITY_ACCELERATION = 9.8f;
private const float PLAYER_HEIGHT_OFFSET = 1.5f;

if (count > MAX_ITEM_COUNT)
{
    Debug.Log(MSG_TOO_MANY_ITEMS);
}
GameObject canvas = GameObject.Find(OBJ_PLAYER_CANVAS);
float speed = GRAVITY_ACCELERATION;
transform.position = new Vector3(0, PLAYER_HEIGHT_OFFSET, 0);
```

### 例外: 一度しか使わない場合

```csharp
// ✅ OK: 一度しか使わない初期化処理
private void Initialize()
{
    _loading = UIHelper.FindOrInstantiatePrefab("nowloading", path, missingObjects);
    UIHelper.RegisterButton("btnStart", OnClickStart, missingObjects);
}
```

### 定数の命名規則

| スコープ | 命名規則 | 例 |
|---------|---------|-----|
| プライベート定数 | `_CONSTANT_NAME` | `_MAX_RETRY_COUNT` |
| パブリック定数 | `CONSTANT_NAME` | `GRAVITY_ACCELERATION` |
| 定数フィールド | アンダースコア + 大文字スネークケース | `_SCENE_LOAD_DELAY` |

### 定数のグループ化

関連する定数は、コメントでグループ化すること:

```csharp
public class TitleStartCtrl : MonoBehaviour
{
    // File Constants
    private const string _STAGE_LIST_FILE_NAME = "stagelist.csv";
    private const string _YAML_FILE_EXTENSION = ".yaml";
    private const string _README_FILE_NAME = "README.md";
    
    // GameObject Names
    private const string _OBJ_LOADING = "nowloading";
    private const string _OBJ_BTN_START = "btnStart";
    private const string _OBJ_BTN_QUIT = "btnQuit";
    
    // Numeric Constants
    private const float _SCENE_LOAD_DELAY = 0.1f;
    private const int _MAX_RETRY_COUNT = 3;
    private const float _FADE_DURATION = 0.5f;
    
    // UI Messages
    private const string _MSG_LOADING = "読み込み中...";
    private const string _MSG_ERROR = "エラーが発生しました";
}
```

---

## 3. 制御文の中括弧 {} を必須とする

### 基本ルール

**必須**: すべての `if`, `else`, `for`, `while`, `foreach`, `switch` に中括弧を使用すること。

単一行の文でも、必ず中括弧で囲むこと。

### 悪い例と良い例

```csharp
// ❌ NG: 中括弧なし
if (condition)
    DoSomething();

if (x > 0)
    x = 0;
else
    x = -1;

for (int i = 0; i < 10; i++)
    Debug.Log(i);

while (isRunning)
    Update();

// ✅ OK: 中括弧あり
if (condition)
{
    DoSomething();
}

if (x > 0)
{
    x = 0;
}
else
{
    x = -1;
}

for (int i = 0; i < 10; i++)
{
    Debug.Log(i);
}

while (isRunning)
{
    Update();
}
```

### 理由

- **バグ予防**: 後から行を追加する際の誤りを防ぐ
- **可読性**: 構造が明確になる
- **一貫性**: プロジェクト全体で統一されたスタイル

---

## 4. 三項演算子とnull条件演算子の使用禁止

### 基本ルール

**禁止**: `? :` (三項演算子) と `?.` (null条件演算子) は使用しないこと。

条件式は if-else で明示的に記述すること。

### 三項演算子の置き換え

```csharp
// ❌ NG: 三項演算子
int result = (x > 0) ? 10 : -10;
string message = isValid ? "Valid" : "Invalid";
GameObject obj = transform != null ? transform.gameObject : null;

// ✅ OK: if-else で明示
int result;
if (x > 0)
{
    result = 10;
}
else
{
    result = -10;
}

string message;
if (isValid)
{
    message = "Valid";
}
else
{
    message = "Invalid";
}

GameObject obj = null;
if (transform != null)
{
    obj = transform.gameObject;
}
```

### null条件演算子の置き換え

```csharp
// ❌ NG: null条件演算子
component?.DoSomething();
int? count = list?.Count;
string name = player?.name ?? "Unknown";

// ✅ OK: 明示的なnullチェック
if (component != null)
{
    component.DoSomething();
}

int count = 0;
if (list != null)
{
    count = list.Count;
}

string name = "Unknown";
if (player != null)
{
    name = player.name;
}
```

### 理由

- **可読性**: 条件が明確で、動作が理解しやすい
- **デバッグ**: ブレークポイントを設定しやすい
- **保守性**: 後から条件を追加しやすい

---

## 5. 関数の行数制限

### 基本ルール

**必須**: 1つの関数は40行以内に収めること。

40行を超える場合は、機能ごとに分割すること。

### 悪い例と良い例

```csharp
// ❌ NG: 100行以上の巨大な関数
void Awake()
{
    // GameObject の検索
    _loading = GameObject.Find("nowloading");
    _pnlStageSelector = GameObject.Find("pnlStageSelector");
    _pnlAboutThisGame = GameObject.Find("pnlAboutThisGame");
    // ... 50行の検索処理
    
    // ボタンの登録
    Button btnStart = _pnlStageSelector.transform.Find("btnStart").GetComponent<Button>();
    btnStart.onClick.AddListener(OnClickStart);
    // ... 30行のボタン登録
    
    // 初期化処理
    LoadStageData();
    InitializeUI();
    // ... 20行の初期化
}

// ✅ OK: 機能ごとに分割
void Awake()
{
    List<string> missingObjects = new List<string>();
    
    InitializeLoadingCanvas(missingObjects);
    InitializePanels(missingObjects);
    RegisterButtonListeners(missingObjects);
    InitializeStageContents(missingObjects);
    InitializeVersionInfo(missingObjects);
    
    CheckMissingObjects(missingObjects);
}

private void InitializeLoadingCanvas(List<string> missingObjects)
{
    _loading = UIHelper.FindGameObject("nowloading", missingObjects);
    if (_loading != null)
    {
        _loading.SetActive(false);
    }
}

private void InitializePanels(List<string> missingObjects)
{
    _pnlStageSelector = UIHelper.FindAndSetupPanel("pnlStageSelector", missingObjects);
    _pnlAboutThisGame = UIHelper.FindAndSetupPanel("pnlAboutThisGame", missingObjects, setActiveFalse: true);
}

private void RegisterButtonListeners(List<string> missingObjects)
{
    UIHelper.RegisterButton("btnStart", OnClickStart, missingObjects);
    UIHelper.RegisterButton("btnQuit", OnClickQuit, missingObjects);
}
```

### 関数分割の指針

1. **単一責任の原則**: 1つの関数は1つの責任を持つ
2. **明確な命名**: 関数名で処理内容が理解できる
3. **適切な粒度**: 10-30行が理想的
4. **依存関係の明示**: 引数と戻り値で関係性を明確にする

---

## 6. Early Return パターンの徹底使用

### 基本ルール

**必須**: 条件チェックは Early Return（ガード句）パターンを使用し、ネストしたif文を避けること。

ネストの深さは2段階までに制限すること。

### 悪い例と良い例

```csharp
// ❌ NG: ネストしたif文
private void CloseWindow()
{
    if (transform.parent != null)
    {
        if (transform.parent.gameObject.name == "titlebar")
        {
            if (transform.parent.parent != null)
            {
                transform.parent.parent.gameObject.SetActive(false);
            }
            return;
        }
        transform.parent.gameObject.SetActive(false);
    }
    else
    {
        Debug.LogWarning("No parent found");
    }
}

// ✅ OK: Early Return パターン
private void CloseWindow()
{
    // ガード句: 異常系を先に処理
    if (transform.parent == null)
    {
        Debug.LogWarning("No parent found");
        return;
    }

    // タイトルバーの場合は親の親を非表示
    if (transform.parent.gameObject.name == "titlebar")
    {
        if (transform.parent.parent == null)
        {
            Debug.LogWarning("Grandparent not found");
            return;
        }
        transform.parent.parent.gameObject.SetActive(false);
        return;
    }

    // 通常の場合は親を非表示
    transform.parent.gameObject.SetActive(false);
}
```

### Early Return のメリット

1. **可読性**: 正常系のコードがフラットで読みやすい
2. **保守性**: 条件追加時にネストが深くならない
3. **認知負荷軽減**: 前提条件が明確で理解しやすい
4. **バグ予防**: else節の書き忘れを防げる

### 適用パターン

```csharp
// パターン1: nullチェック
public void ProcessData(Data data)
{
    if (data == null)
    {
        Debug.LogWarning("Data is null");
        return;
    }
    
    // 正常系の処理
}

// パターン2: 前提条件チェック
public void StartGame()
{
    if (!IsInitialized)
    {
        Debug.LogError("Not initialized");
        return;
    }
    
    if (IsGameRunning)
    {
        Debug.LogWarning("Game is already running");
        return;
    }
    
    // ゲーム開始処理
}
```

---

## 7. 変数名の命名規則

### 基本ルール

**必須**: 意味のある名前を使用し、一時的な名前を避けること。

変数名は、その変数が何を表すのかを明確に示すものにすること。

### 悪い例と良い例

```csharp
// ❌ NG: 意味不明な変数名
GameObject obj = GameObject.Find("Canvas");
Transform t = parent.transform.Find("Content");
int temp = CalculateValue();
string str = GetMessage();
Button btn = GetComponent<Button>();
List<int> list = new List<int>();

// ✅ OK: 意味のある変数名
GameObject canvasObject = GameObject.Find("Canvas");
Transform contentTransform = parent.transform.Find("Content");
int calculatedValue = CalculateValue();
string errorMessage = GetMessage();
Button startButton = GetComponent<Button>();
List<int> stageIndices = new List<int>();
```

### 例外的に許可される短い変数名

| 変数 | 用途 | 例 |
|------|------|-----|
| `i`, `j`, `k` | ループカウンタ | `for (int i = 0; i < count; i++)` |
| `x`, `y`, `z` | 座標 | `Vector3 position = new Vector3(x, y, z);` |
| `e` | イベント引数 | `void OnClick(Event e)` ※ `eventArgs`推奨 |

### 変数名の接頭辞・接尾辞

```csharp
// GameObject には適切な接尾辞を
GameObject loadingCanvas;        // "Canvas"がついているとUIだとわかる
GameObject playerCharacter;      // "Character"で実体がわかる
GameObject enemySpawner;         // "Spawner"で機能がわかる

// Transform には "Transform" を
Transform contentTransform;
Transform scrollbarTransform;
Transform cameraTransform;

// Component には型を示す名前を
Button closeButton;
Button startButton;
Text statusText;
Text scoreText;
Image backgroundImage;
Image iconImage;

// Collection には複数形を
List<Stage> stages;
Dictionary<string, GameObject> objectMap;
Queue<Action> actionQueue;
```

### フィールド変数の命名規則

**フィールド（変数）のみアンダースコアプレフィックスを使用する**

重要な区別：
- **フィールド** = 直接のデータ保持 → `_` プレフィックス必須
- **プロパティ** = インターフェース/抽象化層 → `_` プレフィックス不要

| 種類 | スコープ | 命名規則 | 例 | 用途 |
|-----|---------|---------|-----|------|
| **フィールド** | プライベート | `_fieldName` | `_currentScore` | クラス内のみ |
| **フィールド** | 内部 Static | `_CONSTANT_NAME` | `_APP_GAME_MODE` | 同じアセンブリ内 |
| **フィールド** | 内部 Instance | `_fieldName` | `_stageManager` | 同じアセンブリ内 |
| **フィールド** | 定数 | `_CONSTANT_NAME` | `_MAX_ITEMS` | 変更不可 |
| **プロパティ** | 任意 | `PropertyName` | `DebugLevel`, `LogFileName` | インターフェース |

#### 実装例

```csharp
public class ExampleController : MonoBehaviour
{
    // ✅ OK: プライベートフィールド（アンダースコア + キャメルケース）
    private GameObject _playerObject;
    private Transform _cameraTransform;
    private int _currentScore;
    
    // ✅ OK: 内部 Static フィールド（アンダースコア + 大文字スネークケース）
    internal static string _APP_GAME_MODE = GlobalConst.GAME_MODE_DEBUG;
    internal static bool _STAGE_PADDLE_MODE = false;
    
    // ✅ OK: 内部 Instance フィールド（アンダースコア + キャメルケース）
    internal static GameConfig _instance = null;
    
    // ✅ OK: 定数フィールド（アンダースコア + 大文字スネークケース）
    private const string _PLAYER_TAG = "Player";
    private const int _MAX_ITEMS = 100;
    
    // ✅ OK: プロパティはアンダースコアなし（パスカルケース）
    // プロパティは実装の詳細を隠したインターフェース - _は不要
    internal static DebugLevel DebugLevel { get; set; } = DebugLevel.All;
    internal static string LogFileName { get; set; } = GlobalConst._LOG_FILE_NAME;
    internal static string LogFilePath => System.IO.Path.Combine(Application.persistentDataPath, LogFileName);
    
    // ❌ NG: パブリックフィールド（避けるべき）
    public int maxHealth;  // → プロパティ化すべき
}
```

#### フィールド vs プロパティ の選択基準

**フィールド（`_` 付き）を使う場合：**
```csharp
// データを直接保持する変数
private int _health = 100;
private bool _isActive = true;
internal static bool _ENABLE_DEBUG = false;
```

**プロパティ（`_` なし）を使う場合：**
```csharp
// ゲッター・セッターで実装の詳細を隠す
internal static DebugLevel DebugLevel { get; set; } = DebugLevel.All;

// 計算による値
internal static string LogFilePath => System.IO.Path.Combine(
    Application.persistentDataPath,
    LogFileName
);

// バリデーション付き
public int Health
{
    get => _health;
    set => _health = Mathf.Clamp(value, 0, 100);
}
```
    internal static string _APP_GAME_MODE = GlobalConst.GAME_MODE_DEBUG;
    internal static bool _STAGE_PADDLE_MODE = false;
    
    // ✅ OK: 内部 Instance フィールド（アンダースコア + キャメルケース）
    internal static GameConfig _instance = null;
    
    // ✅ OK: 定数（アンダースコア + 大文字スネークケース）
    private const string _PLAYER_TAG = "Player";
    private const int _MAX_ITEMS = 100;
    
    // ✅ OK: プロパティはアンダースコアなし（パスカルケース）
    internal static string LogFileName { get; set; } = GlobalConst.LOG_FILE_NAME;
    public int Health { get; set; }
    
    // ❌ NG: パブリックフィールド（避けるべき）
    public int maxHealth;  // → プロパティ化すべき
}
```

#### スコープ可視性の根拠

- **フィールドへの `_` プレフィックス**: 目視でモジュールレベルの「変数」かローカル変数かが判断可能
- **プロパティへの `_` なし**: プロパティはインターフェース/抽象化層で、実装の詳細を隠すため
- **Recovery フェーズでの安全性**: グローバル状態への依存が明確になり、リグレッション防止
- **保守性向上**: フィールドとプロパティの役割が明確で、意図しないスコープ汚染を防止

#### 例外：ローカル変数

関数内のローカル変数はアンダースコアを使用しない

```csharp
void ProcessData()
{
    // ✅ OK: ローカル変数（アンダースコアなし）
    int count = 0;
    string message = "Processing";
    
    // ❌ NG: ローカル変数にアンダースコアはつけない
    int _count = 0;
}
```

---

## 8. ユーティリティクラスの設計原則

### 基本ルール

**必須**: 関連する機能は適切なユーティリティクラスに集約すること。

コードの重複を避け、保守性を高めるため、共通処理は静的ユーティリティクラスに集約すること。

### ユーティリティクラスの分類

| クラス | 責務 | 例 |
|--------|------|-----|
| `LoadStreamingAsset` | StreamingAssetsフォルダ関連操作 | ファイル読み込み、パス生成、存在チェック |
| `UIHelper` | UI操作 | GameObject検索、ボタン登録、スクロール制御 |
| `FileOperationUtility` | ファイル操作 | エディタで開く、画像読み込み |
| `StageDataManager` | ステージデータ管理 | シーンリスト取得、CSV読み込み |
| `LogUtility` | ログ管理 | レベル別ログ出力、ファイル出力 |

### LoadStreamingAsset の例

```csharp
public static class LoadStreamingAsset
{
    // 定数
    public const string YAML_FILE_EXTENSION = ".yaml";
    public const string STAGE_LIST_FILE_NAME = "stagelist.csv";
    private const string _STAGING_SUB_FOLDER = "Stages";
    
    // YAML関連の操作を集約
    public static string GetYamlFileName(string sceneName)
    {
        return Path.GetFileName(sceneName + YAML_FILE_EXTENSION);
    }
    
    public static bool YamlFileExists(string sceneName)
    {
        string yamlFileName = GetYamlFileName(sceneName);
        string yamlFilePath = StageFilePath(yamlFileName);
        return File.Exists(yamlFilePath);
    }
    
    internal static string StageFilePath(string fileName)
    {
        return Path.Combine(Application.streamingAssetsPath, _STAGING_SUB_FOLDER, fileName);
    }
    
    public static string AllTextStream(string fileName)
    {
        string filePath = StageFilePath(fileName);
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return string.Empty;
        }
        
        return File.ReadAllText(filePath);
    }
}
```

### UIHelper の例

```csharp
public static class UIHelper
{
    // Prefabパス定数
    public const string PREFAB_PATH_LOADING = "Prefabs/UI/nowloading";
    
    // GameObject検索（エラーハンドリング付き）
    public static GameObject FindGameObject(string path, List<string> missingObjects, 
                                           string objectType = "", GameObject parent = null)
    {
        GameObject rootObject = (parent != null) ? parent : null;
        Transform searchRoot = (rootObject != null) ? rootObject.transform : null;
        
        GameObject foundObject = null;
        if (searchRoot != null)
        {
            foundObject = searchRoot.Find(path)?.gameObject;
        }
        else
        {
            foundObject = GameObject.Find(path);
        }
        
        if (foundObject == null)
        {
            string errorMessage = $"{objectType} '{path}' not found";
            missingObjects.Add(errorMessage);
            Debug.LogWarning(errorMessage);
        }
        
        return foundObject;
    }
    
    // ボタン登録
    public static void RegisterButton(string buttonName, UnityEngine.Events.UnityAction action, 
                                     List<string> missingObjects, GameObject parent = null)
    {
        GameObject buttonObject = FindGameObject(buttonName, missingObjects, "Button", parent);
        if (buttonObject != null)
        {
            Button button = buttonObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }
    }
    
    // スクロールバー制御
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
    
    // パネルセットアップ
    public static GameObject FindAndSetupPanel(string panelName, List<string> missingObjects, 
                                              bool setActiveFalse = false)
    {
        GameObject panel = FindGameObject(panelName, missingObjects, "Panel");
        if (panel != null && setActiveFalse)
        {
            panel.SetActive(false);
        }
        return panel;
    }
}
```

### 設計ルール

1. **単一責任の原則**: 1つのユーティリティクラスは1つの責務を持つ
2. **静的クラス**: ユーティリティは`static class`として定義
3. **定数の集約**: 関連する定数は該当ユーティリティクラスに配置
4. **名前空間**: 特定の名前空間は使わず、グローバルに配置（UnityのScriptAssembliesに準拠）

### 悪い例と良い例

```csharp
// ❌ NG: コントローラに直接実装
public class TitleStartCtrl : MonoBehaviour
{
    private void OnClickStageEditor(Button btnStageEditor)
    {
        // 直接Path.GetFileNameを使用
        string fileName = Path.GetFileName(sceneName + ".yaml");
        
        // 直接File.Existsを使用
        if (File.Exists(LoadStreamingAsset.StageFilePath(fileName)))
        {
            FileOperationUtility.OpenInEditor(LoadStreamingAsset.StageFilePath(fileName));
        }
    }
}

// ✅ OK: ユーティリティクラスに集約
public class TitleStartCtrl : MonoBehaviour
{
    private void OnClickStageEditor(Button btnStageEditor)
    {
        // LoadStreamingAssetに集約された操作を使用
        string fileName = LoadStreamingAsset.GetYamlFileName(sceneName);
        
        if (LoadStreamingAsset.YamlFileExists(sceneName))
        {
            string filePath = LoadStreamingAsset.StageFilePath(fileName);
            FileOperationUtility.OpenInEditor(filePath);
        }
    }
}
```

---

## 9. UIスクロールビューの取り扱い

### 基本ルール

**必須**: スクロール位置制御は `ScrollRect.normalizedPosition` を使用すること。

`Scrollbar.value` の直接操作は避けること。

### 基本原則

1. **ScrollRect.normalizedPositionを使用する**
   - `Scrollbar.value`の直接操作は避ける
   - `normalizedPosition = new Vector2(0, 1)`で最上部へ

2. **コンテンツ更新後にリセットする**
   - テキストやアイテムの設定後にスクロール位置を更新
   - コンテンツサイズが確定してから実行

3. **アクティブ状態をチェックする**
   - 非アクティブなパネルのScrollbarは操作しない
   - ユーティリティ関数内でチェックを行う

### UIHelper の実装

```csharp
// UIHelper.cs
public static void ResetScrollbarInPanel(GameObject parentPanel)
{
    // アクティブチェックを内包
    if (parentPanel == null || !parentPanel.activeSelf)
    {
        return;
    }
    
    // ScrollRectを使用（Scrollbar直接操作は不可）
    ScrollRect scrollRect = parentPanel.GetComponentInChildren<ScrollRect>(true);
    if (scrollRect != null)
    {
        scrollRect.normalizedPosition = new Vector2(0, 1); // x=0(左), y=1(上)
    }
}
```

### 呼び出しタイミング

```csharp
// ❌ NG: コンテンツ設定前にリセット
private void OnClickAboutGame()
{
    _pnlAboutThisGame.SetActive(value: true);
    UIHelper.ResetScrollbarInPanel(_pnlAboutThisGame); // 早すぎる
    
    textComponent.text = LoadStreamingAsset.AllTextStream("README.md");
}

// ✅ OK: コンテンツ設定後にリセット
private void OnClickAboutGame()
{
    _pnlAboutThisGame.SetActive(value: true);
    
    string text = LoadStreamingAsset.AllTextStream("README.md");
    textComponent.text = text;
    
    UIHelper.ResetScrollbarInPanel(_pnlAboutThisGame); // コンテンツ確定後
}
```

### パネル切り替え時

```csharp
// ✅ OK: SetActive後にリセット、activeチェックは関数内で実施
private void OnClickStageSelect()
{
    _pnlStageSelector.SetActive(!_pnlStageSelector.activeSelf);
    UIHelper.ResetScrollbarInPanel(_pnlStageSelector); // 内部でactiveチェック
}
```

### 変数格納の不要化

従来のように`_StageScrollbar`などの変数にScrollbarを格納する必要はありません。

```csharp
// ❌ NG: 変数に格納して管理
private GameObject _StageScrollbar;
private const string _CHILD_PATH_SCROLLVIEW_SCROLLBAR = "Scroll View/Scrollbar Vertical";

void Awake()
{
    _StageScrollbar = UIHelper.FindGameObject(_CHILD_PATH_SCROLLVIEW_SCROLLBAR, missingObjects);
    UIHelper.SetScrollbarTopPosition(_StageScrollbar);
}

// ✅ OK: 親パネルから自動検索
void Awake()
{
    // コンテンツ設定後に親パネルを渡すだけ
    UIHelper.ResetScrollbarInPanel(_pnlStageSelector);
}
```

**メリット**:
- 変数管理不要（メモリ効率向上）
- パス定数不要（保守性向上）
- コードがシンプルで読みやすい
- `GetComponentInChildren(true)`で非アクティブな子も検索可能

---

## 10. コメント規約

### 基本ルール

**推奨**: コメントは最小限に、コードで意図を表現すること。

コードそのものが明確であれば、コメントは不要です。

### 悪い例と良い例

```csharp
// ❌ NG: コメントで説明が必要
// ユーザーの年齢が18歳以上かチェック
if (user.age >= 18)
{
    // 成人向けコンテンツを表示
    ShowAdultContent();
}

// ✅ OK: 定数名で意図が明確
private const int ADULT_AGE_THRESHOLD = 18;

if (user.age >= ADULT_AGE_THRESHOLD)
{
    ShowAdultContent();
}
```

### コメントが必要な場合

以下の場合は、コメントを記述すること:

1. **複雑なアルゴリズム**: 理解が困難な処理
2. **外部仕様への参照**: API仕様やドキュメントへのリンク
3. **将来の変更予定**: TODOやFIXME
4. **バグ回避**: Unity や外部ライブラリのバグ回避コード

```csharp
// 良いコメントの例

// FIXME: Unity 6.3 のバグ回避 - ScrollRect.normalizedPosition が正しく動作しないため遅延実行
IEnumerator ResetScrollPositionDelayed()
{
    yield return new WaitForEndOfFrame();
    scrollRect.normalizedPosition = new Vector2(0, 1);
}

// TODO: ステージエディタ実装後に削除
private void LoadStageDataFromCSV()
{
    // 暫定的にCSVから読み込み
}

// 参照: PLATEAU SDK ドキュメント - https://example.com/docs
private void LoadCityGMLData(string filePath)
{
    // CityGML 2.0 形式のパース
}
```

---

## 11. Unity 固有の規約

### MonoBehaviour の使用

```csharp
// ✅ OK: this を使用
void Awake()
{
    _text = this.gameObject.GetComponent<TextMeshProUGUI>();
    _transform = this.transform;
}

// ❌ NG: base を使用しない
void Awake()
{
    _text = base.gameObject.GetComponent<TextMeshProUGUI>();
}
```

### Coroutine の命名

```csharp
// ✅ OK: Coroutineには"Coroutine"接尾辞
private IEnumerator LoadSceneCoroutine()
{
    yield return new WaitForSeconds(0.1f);
    SceneManager.LoadScene(sceneName);
}

// または"Async"接尾辞（非同期処理の場合）
private IEnumerator LoadDataAsync()
{
    yield return StartCoroutine(FetchDataFromServer());
}
```

### インスペクタ公開フィールド

```csharp
// ✅ OK: SerializeFieldでプライベートを公開
[SerializeField]
private float _moveSpeed = 5.0f;

[SerializeField]
private GameObject _targetObject;

// ❌ NG: publicフィールド（避けるべき）
public float moveSpeed = 5.0f;
public GameObject targetObject;
```

---

## 12. Recovery フォルダからのマージルール

### 基本ルール

リバースエンジニアリングで復元したコードをマージする際は、以下のルールを厳守すること。

### 1. 変数の初期化を維持する

デフォルト値であっても、明示的な初期化は**削除しない**こと。

```csharp
// ✅ OK: 初期化を維持
public static GameTimerCtrl instance = null;
public float _time = 0.0f;
private bool _isPaused = false;

// ❌ NG: 初期化を削除
public static GameTimerCtrl instance;
public float _time;
private bool _isPaused;
```

### 2. `this` を維持する（`base` を使わない）

```csharp
// ✅ OK: this を使用
_text = this.gameObject.GetComponent<TextMeshProUGUI>();

// ❌ NG: base に変更
_text = base.gameObject.GetComponent<TextMeshProUGUI>();
```

### 3. コメントを削除しない

既存のコメント（コメントアウトされたコードを含む）は、マージ時に**削除しない**こと。

```csharp
// ✅ OK: コメントを維持
void Awake()
{
    if (instance == null)
    {
        instance = this;
        // DontDestroyOnLoad(this.gameObject);  // 将来的に必要になるかもしれない
    }
    // else
    // {
    //     // Destroy(this.gameObject);
    // }
}
```

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

---

## チェックリスト

コードレビューやコード提案の際は、以下を確認すること:

- [ ] **定数化**: マジックナンバー・マジックストリングがないか
- [ ] **中括弧**: すべての制御文に`{}`があるか
- [ ] **三項演算子**: `? :`や`?.`を使用していないか
- [ ] **Early Return**: ネストした`if`を避け、ガード句を使用しているか
- [ ] **関数の長さ**: 40行以内に収まっているか
- [ ] **変数名**: 意味のある名前になっているか
- [ ] **フィールド名**: モジュールレベル変数に`_`プレフィックスがあるか、ローカル変数にはないか
- [ ] **ユーティリティの使用**: 共通処理を適切なユーティリティクラスに集約しているか
- [ ] **ScrollRect**: スクロール位置制御は`normalizedPosition`を使用しているか
- [ ] **コメント**: 不要なコメントがないか、必要なコメントがあるか

---

## 参考資料

- [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Unity Coding Standards](https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity)
- [Clean Code by Robert C. Martin](https://www.amazon.co.jp/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)

---

## 改訂履歴

| 日付 | バージョン | 変更内容 |
|------|----------|---------|
| 2026-01-26 | 1.1.0 | モジュールレベル変数の命名規則を追加（アンダースコアプレフィックス必須） |
| 2026-01-15 | 1.0.0 | 初版作成 |
