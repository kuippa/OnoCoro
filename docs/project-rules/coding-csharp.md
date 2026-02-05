# C# コーディング規約

**目的**: 実装品質の安定化・判断のブレ防止・保守性向上

---

## 基本原則

[OK] **可読性を最優先** - コードは書くよりも読む時間が長い  
[OK] **明示的であること** - 暗黙的な動作や省略形を避ける  
[OK] **一貫性を保つ** - プロジェクト全体で統一されたスタイルを維持  
[OK] **単純さを追求** - 複雑な処理は小さな単位に分割する

---

## ブレース・制御文

### 必須ルール

[WARN] **すべての制御文に `{}` を使用** - 一行の if/for/while も例外なし

```csharp
// [NG] ブレースなし
if (value > 0)
    Debug.Log("正");

// [OK] ブレース必須
if (value > 0)
{
    Debug.Log("正");
}

// [OK] 複合条件も同様
for (int i = 0; i < count; i++)
{
    ProcessItem(i);
}

while (isRunning)
{
    UpdateFrame();
}
```

### 括弧の省略禁止

[WARN] **Optional なカッコも明示的に記述**

```csharp
// [NG] 括弧省略
for (int i = 0; i < 10; i++)
    items.Add(i);

// [OK] 括弧明示
for (int i = 0; i < 10; i++)
{
    items.Add(i);
}
```

---

## 演算子の制限

### 三項演算子（条件演算子）禁止

[WARN] **`? :` 構文は使用禁止** - if-else で明示的に記述すること

```csharp
// [NG] 三項演算子
int result = (value > 0) ? 1 : 0;
string status = (isActive) ? "ON" : "OFF";

// [OK] if-else
int result;
if (value > 0)
{
    result = 1;
}
else
{
    result = 0;
}

string status;
if (isActive)
{
    status = "ON";
}
else
{
    status = "OFF";
}
```

### Null 結合演算子（?.）禁止

[WARN] **`?.` は使用禁止** - 明示的な null チェックを使用

```csharp
// [NG] Null 結合演算子
int? length = items?.Count;
string name = person?.GetName();

// [OK] 明示的な null チェック
int? length = null;
if (items != null)
{
    length = items.Count;
}

string name = null;
if (person != null)
{
    name = person.GetName();
}
```

---

## ネストと関数長

### Early Return パターン（推奨）

[OK] **Guard clause を使用してネストを浅くする**

```csharp
// [NG] 深いネスト
private void ProcessTower(Tower tower)
{
    if (tower != null)
    {
        if (tower.IsActive)
        {
            if (tower.Fuel > 0)
            {
                // 実処理
                tower.Fire();
            }
        }
    }
}

// [OK] Early Return で平坦化
private void ProcessTower(Tower tower)
{
    if (tower == null)
    {
        return;
    }
    
    if (!tower.IsActive)
    {
        return;
    }
    
    if (tower.Fuel <= 0)
    {
        return;
    }
    
    // 実処理
    tower.Fire();
}
```

### 関数長の制限

[WARN] **関数は最大 40 行以内** - 長すぎる場合は分割すること

```csharp
// [NG] 60 行を超える関数
private void InitializeGameScene()
{
    // 20行... UI 初期化
    // 20行... ゲーム状態初期化
    // 20行... イベントリスナー登録
}

// [OK] 責務を分割
private void InitializeGameScene()
{
    InitializeUI();
    InitializeGameState();
    RegisterEventListeners();
}

private void InitializeUI()
{
    // 20行の UI 初期化
}

private void InitializeGameState()
{
    // 20行のゲーム状態初期化
}

private void RegisterEventListeners()
{
    // 20行のイベントリスナー登録
}
```

---

## 可読性

### 変数名の要件

[WARN] **意味のある名前を使用** - 省略形・一文字変数は避ける

```csharp
// [NG] 不明確な名前
var t = GetTower();
var p = transform.position;
int cnt = enemies.Count;
void Proc() { }
void Calc() { }

// [OK] 明確な名前
Tower tower = GetTower();
Vector3 position = transform.position;
int enemyCount = enemies.Count;
void ProcessEnemyWave() { }
void CalculatePathfinding() { }
```

### コメントの活用

[OK] **実装の「なぜ」をコメント化** - 「何をしているか」は code が語る

```csharp
// [NG] 無意味なコメント
i++;  // i をインクリメント

// [OK] 意味のあるコメント
_currentWaveIndex++;  // 次のウェーブ開始時に再スポーン判定を有効化
```

---

## Namespace 衝突回避

### Debug クラスの明示的なエイリアス

[WARN] **`System.Diagnostics.Debug` と `UnityEngine.Debug` の衝突を避けるため、必ず using エイリアスを使用**

```csharp
using System.Diagnostics;
using Debug = CommonsUtility.Debug;  // 必須

// [OK] Unity のログ
Debug.Log("Unity のデバッグログ");
Debug.LogWarning("警告メッセージ");

// [OK] System.Diagnostics は完全修飾名
Process process = Process.Start("notepad.exe");
```

---

## プリプロセッサディレクティブ

### 条件コンパイルの活用

[OK] **デバッグ専用コードは `#if DEBUG` で囲む**

```csharp
#if DEBUG
private void LogDebugInfo()
{
    Debug.Log($"Frame: {Time.frameCount}, FPS: {1f / Time.deltaTime}");
}
#endif

public void Update()
{
    #if DEBUG
    LogDebugInfo();
    #endif
    
    ProcessGameLogic();
}
```

---

## Checked Exception Handling

### 例外処理の明示性

[OK] **例外は明示的にキャッチしてハンドル** - 汎用的な catch は避ける

```csharp
// [NG] 汎用 catch
try
{
    LoadGameData();
}
catch (Exception ex)
{
    Debug.LogError("エラーが発生しました");
}

// [OK] 特定の例外をキャッチ
try
{
    LoadGameData();
}
catch (FileNotFoundException)
{
    Debug.LogError("ゲームデータファイルが見つかりません");
}
catch (JsonException)
{
    Debug.LogError("ゲームデータの形式が不正です");
}
```

---

## Pre-Commit チェックリスト

実装完了後、以下を確認してコミット：

- [ ] **魔法数字・文字列**: すべて定数化済み
- [ ] **ブレース**: すべての制御文に `{}` あり
- [ ] **演算子**: `? :` と `?.` なし
- [ ] **ネスト**: Early Return で平坦化
- [ ] **関数長**: すべて 40 行以内
- [ ] **変数名**: 意味のある名前のみ使用
- [ ] **Debug エイリアス**: using 文で明示的にエイリアス化
- [ ] **例外処理**: 特定の例外をキャッチ

---

**関連資料**:
- [naming-conventions.md](naming-conventions.md) - 変数・定数・クラス命名
- [unity-design-patterns.md](unity-design-patterns.md) - MonoBehaviour パターン
- [AGENTS.md](../AGENTS.md) - プロジェクト全体ルール
