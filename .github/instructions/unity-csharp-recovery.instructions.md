---
title: Unity C# Recovery Phase Development
description: Coding standards and best practices for C# development during Recovery phase with null safety emphasis and AGENTS.md compliance
applyTo: "*.cs"
---

# Unity C# Recovery Phase Development

## 概要

Recovery フェーズでは、2年間のバックアップから復元されたコードを扱うため、**defensive programming** が必須です。すべてのコンポーネント参照は null チェックを含める必要があります。OnoCoro プロジェクトのコーディング基準に準拠した C# 実装を行ってください。

## 参考資料

- [AGENTS.md](../../AGENTS.md) - プロジェクト全体ルール
- [docs/coding-standards.md](../../docs/coding-standards.md) - 詳細 C# 基準
- [docs/recovery-workflow.md](../../docs/recovery-workflow.md) - Recovery フェーズマージガイドライン

## Core コーディング基準

### 1. 必須ブレース（Braces Required）

すべての制御文は `{}` を必ず使用してください。1行の記述は禁止です。

```csharp
// ❌ WRONG: ブレースなし
if (rainAbsorb == null)
    return;

// ✅ CORRECT: ブレース必須
if (rainAbsorb == null)
{
    return;
}
```

### 2. Null チェックパターン（Recovery Phase 必須）

Transform と GetComponent の結果は必ず null チェックしてください。

```csharp
// ✅ CORRECT: Transform null check + Component validation
Transform absorbCollider = transform.Find("absorbcollider");
if (absorbCollider == null)
{
    Debug.LogWarning($"absorbcollider not found on {gameObject.name}");
    return;
}

RainAbsorbCtrl rainAbsorb = absorbCollider.GetComponent<RainAbsorbCtrl>();
if (rainAbsorb == null)
{
    Debug.LogWarning($"RainAbsorbCtrl component not found on absorbcollider");
    return;
}
```

### 3. 定数の命名規則（Magic Numbers/Strings 禁止）

すべてのマジックナンバーと文字列は定数として定義してください。

```csharp
// ❌ WRONG: マジックナンバー
float scale = transform.localScale.x * 2.5f;
Collider collider = GetComponent("BoxCollider");

// ✅ CORRECT: 定数定義
private const float COLLIDER_SCALE_MULTIPLIER = 2.5f;
private const string ABSORB_COLLIDER_NAME = "absorbcollider";

float scale = transform.localScale.x * COLLIDER_SCALE_MULTIPLIER;
Collider collider = transform.Find(ABSORB_COLLIDER_NAME)?.GetComponent<Collider>();
```

**命名規則**:
- Private 定数: `_CONSTANT_NAME`
- Public 定数: `CONSTANT_NAME`

### 4. Early Return パターン（Nested If 禁止）

Guard clause を使用してネストを減らしてください。

```csharp
// ❌ WRONG: ネストが深い
public void ProcessRain()
{
    if (rainDropsCtrl != null)
    {
        if (rainAbsorbCtrl != null)
        {
            if (transform != null)
            {
                // 処理...
            }
        }
    }
}

// ✅ CORRECT: Early return で簡潔
public void ProcessRain()
{
    if (rainDropsCtrl == null)
    {
        return;
    }
    if (rainAbsorbCtrl == null)
    {
        return;
    }
    if (transform == null)
    {
        return;
    }
    
    // 処理...
}
```

### 5. 関数の長さ制限

1 つの関数は **最大 40 行**に収めてください。

```csharp
// ❌ WRONG: 50行以上
public void ChangeColliderSize(float newSize)
{
    // ... 40行以上の処理
}

// ✅ CORRECT: Helper メソッドに分割
public void ChangeColliderSize(float newSize)
{
    Collider collider = GetAbsorbCollider();
    if (collider == null)
    {
        return;
    }
    
    ApplyColliderScale(collider, newSize);
}

private Collider GetAbsorbCollider()
{
    Transform absorbCollider = transform.Find("absorbcollider");
    if (absorbCollider == null)
    {
        Debug.LogWarning($"absorbcollider not found");
        return null;
    }
    
    return absorbCollider.GetComponent<Collider>();
}

private void ApplyColliderScale(Collider collider, float newSize)
{
    // スケール適用処理...
}
```

### 6. 意味のある変数名（Abbreviation 禁止）

短縮形は避け、意図が明確な名前を使用してください。

```csharp
// ❌ WRONG: 短縮形
float sz = 2.5f;
int cnt = 0;
var rb = GetComponent<Rigidbody>();

// ✅ CORRECT: 明確な名前
float colliderSize = 2.5f;
int rainAbsorptionCount = 0;
Rigidbody rainRigidbody = GetComponent<Rigidbody>();
```

### 7. UnityEngine.Debug の明示的エイリアス

`Debug` の曖昧性を避けるため、常に明示的エイリアスを使用してください。

```csharp
using Debug = UnityEngine.Debug;

public class RainDropsCtrl : MonoBehaviour
{
    public void CheckComponents()
    {
        if (rainAbsorbCtrl == null)
        {
            Debug.LogWarning("RainAbsorbCtrl is missing");
        }
    }
}
```

### 8. ユーティリティクラスの活用

関連する機能を utility クラスに集約してください（例：PrefabManager）。

```csharp
// ✅ CORRECT: PrefabManager を使用
GameObject puddlePrefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.Puddle);
if (puddlePrefab == null)
{
    Debug.LogWarning("Puddle prefab not found in PrefabManager");
    return;
}

// Resources.Load の直接呼び出しは禁止
// GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/Puddle");  // ❌
```

## Ternary / Null-Coalescing 演算子の禁止

条件分岐は if-else 文で明示的に記述してください。

```csharp
// ❌ WRONG: Ternary operator
float value = condition ? 10f : 20f;
Transform transform = component?.transform;

// ✅ CORRECT: Explicit if-else
float value;
if (condition)
{
    value = 10f;
}
else
{
    value = 20f;
}

Transform childTransform = null;
if (component != null)
{
    childTransform = component.transform;
}
```

## コンポーネント依存性の明示

使用する子コンポーネントやシリアライズフィールドの依存性をコメントに明記してください。

```csharp
public class RainDropsCtrl : MonoBehaviour
{
    // シリアライズフィールド
    [SerializeField]
    private Transform rainAbsorbCtrl;
    
    private Rigidbody rainRigidbody;
    
    // 期待される子オブジェクト: "absorbcollider" (RainAbsorbCtrl コンポーネント必須)
    
    private void Start()
    {
        // Transform null チェック
        Transform absorbCollider = transform.Find("absorbcollider");
        if (absorbCollider == null)
        {
            Debug.LogWarning($"Missing child object 'absorbcollider' on {gameObject.name}");
            enabled = false;
            return;
        }
        
        // コンポーネント null チェック
        RainAbsorbCtrl rainAbsorb = absorbCollider.GetComponent<RainAbsorbCtrl>();
        if (rainAbsorb == null)
        {
            Debug.LogWarning($"Missing RainAbsorbCtrl on absorbcollider");
            enabled = false;
            return;
        }
        
        rainRigidbody = GetComponent<Rigidbody>();
        if (rainRigidbody == null)
        {
            Debug.LogWarning($"Missing Rigidbody on {gameObject.name}");
            enabled = false;
            return;
        }
    }
}
```

## Pre-Commit Checklist

コード提案前に以下を確認してください：

- [ ] すべての制御文に `{}` がある
- [ ] Magic numbers/strings はすべて定数化済み
- [ ] Ternary operator (`? :`) と null-coalescing (`?.`) なし
- [ ] Early return で nested if なし
- [ ] 関数の長さ ≤ 40 行
- [ ] 変数名が意味的で短縮形でない
- [ ] `UnityEngine.Debug` 明示的エイリアス使用
- [ ] すべての Transform.Find() と GetComponent() に null チェック
- [ ] PrefabManager 経由のアセット参照
- [ ] コンポーネント依存性がコメント記述済み

## Recovery Phase 特有の注意点

### Initialization 保持

古いコードの initialization は破棄せず、null チェックを追加して保持してください。

```csharp
// ✅ CORRECT: 古い初期化を保持し、null チェック追加
void Initialize()
{
    if (demoCtrl == null)
    {
        demoCtrl = FindObjectOfType<DemCtrl>();
    }
    if (demoCtrl == null)
    {
        Debug.LogWarning("DemCtrl not found");
        return;
    }
    
    // 元の初期化処理を続行...
}
```

### Use `this` Explicitly

Recovery phase では、`this` を明示的に使用してスコープを明確にしてください。

```csharp
// ✅ CORRECT: this 明示的使用
public void SetPosition(Vector3 position)
{
    this.transform.position = position;
}

private void OnCollisionEnter(Collider collision)
{
    if (collision.CompareTag("Water"))
    {
        this.enabled = false;
    }
}
```

## トラブルシューティング

### NullReferenceException が頻発する場合

1. すべての Transform.Find() に null チェックを追加
2. すべての GetComponent() に null チェックを追加
3. シリアライズフィールドの未設定を確認
4. 子オブジェクト名が正確に一致しているか確認

```csharp
// デバッグ用：子オブジェクト一覧を確認
void DebugChildObjects()
{
    foreach (Transform child in transform)
    {
        Debug.Log($"Child: {child.gameObject.name}");
    }
}
```

## 関連リソース

- **公式ドキュメント**: [docs/coding-standards.md](../../docs/coding-standards.md)
- **プロジェクトルール**: [AGENTS.md](../../AGENTS.md)
- **Recovery ワークフロー**: [docs/recovery-workflow.md](../../docs/recovery-workflow.md)
- **Architecture**: [docs/architecture.md](../../docs/architecture.md)

---

**Last Updated**: 2026-01-20  
**Project**: OnoCoro (Unity 6.3 Geospatial Visualization)
