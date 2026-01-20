---
title: OnoCoro Prefab and Asset Management
description: Centralized asset management using PrefabManager, prefab loading patterns, and caching strategies for OnoCoro Recovery phase
applyTo: "*.cs"
---

# OnoCoro Prefab and Asset Management

## 概要

OnoCoro では、すべてのプレハブロードを **PrefabManager** 経由で管理します。`Resources.Load()` の直接呼び出しは禁止です。このドキュメントでは、PrefabManager の使用方法と、新しいプレハブ型の追加手順を説明します。

## 参考資料

- [AGENTS.md](../../AGENTS.md) - プロジェクト全体ルール
- [docs/coding-standards.md](../../docs/coding-standards.md) - C# 基準
- [unity-csharp-recovery.instructions.md](unity-csharp-recovery.instructions.md) - Recovery phase C# 実装

## PrefabManager の役割

| 機能 | 説明 |
|------|------|
| **一元化管理** | すべてのプレハブパスを 1 箇所で定義・管理 |
| **キャッシング** | 一度ロードしたプレハブを再利用（パフォーマンス向上） |
| **Null 検証** | プレハブ存在確認とエラーログ自動出力 |
| **保守性** | プレハブパス変更時に 1 箇所の修正で反映 |

## 基本的な使用方法

### 既存プレハブの参照

```csharp
using Debug = UnityEngine.Debug;

public class RainDropsCtrl : MonoBehaviour
{
    public void MakePuddle()
    {
        // ✅ CORRECT: PrefabManager 経由
        GameObject puddlePrefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.Puddle);
        if (puddlePrefab == null)
        {
            Debug.LogWarning("Puddle prefab not found in PrefabManager");
            return;
        }
        
        Instantiate(puddlePrefab, transform.position, Quaternion.identity);
    }
}

// ❌ WRONG: Resources.Load の直接呼び出し（禁止）
// GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/Puddle");
```

### プレハブの直接参照（キャッシング機能）

```csharp
public class GameManager : MonoBehaviour
{
    private GameObject puddlePrefab;
    
    private void Start()
    {
        // 1度ロード後、以降の呼び出しはキャッシュから返される
        puddlePrefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.Puddle);
        if (puddlePrefab == null)
        {
            Debug.LogWarning("Puddle prefab initialization failed");
            enabled = false;
            return;
        }
    }
    
    public void SpawnPuddle(Vector3 position)
    {
        if (puddlePrefab == null)
        {
            Debug.LogWarning("Puddle prefab is null");
            return;
        }
        
        Instantiate(puddlePrefab, position, Quaternion.identity);
    }
}
```

## 新しいプレハブ型の追加方法

### ステップ 1: PrefabType Enum に型を追加

```csharp
// PrefabManager.cs
public class PrefabManager : MonoBehaviour
{
    public enum PrefabType
    {
        Puddle,          // 既存
        Tower,           // 新規追加
        Enemy,           // 新規追加
        Particle         // 新規追加
    }
    
    // ...
}
```

### ステップ 2: パス辞書にマッピングを追加

```csharp
// PrefabManager.cs
private static readonly Dictionary<PrefabType, string> PrefabPaths = new Dictionary<PrefabType, string>
{
    { PrefabType.Puddle, "Prefabs/WorkUnit/Puddle" },
    { PrefabType.Tower, "Prefabs/Tower/BaseTower" },        // 新規
    { PrefabType.Enemy, "Prefabs/Enemy/BasicEnemy" },        // 新規
    { PrefabType.Particle, "Prefabs/Effects/RainParticle" } // 新規
};
```

### ステップ 3: プロパティを追加（Optional）

```csharp
// PrefabManager.cs
internal static GameObject PuddlePrefab => GetPrefab(PrefabType.Puddle);
internal static GameObject TowerPrefab => GetPrefab(PrefabType.Tower);     // 新規
internal static GameObject EnemyPrefab => GetPrefab(PrefabType.Enemy);     // 新規
internal static GameObject ParticlePrefab => GetPrefab(PrefabType.Particle); // 新規
```

### 完全な使用例

```csharp
// TowerManager.cs
using Debug = UnityEngine.Debug;

public class TowerManager : MonoBehaviour
{
    public void PlaceTower(Vector3 position)
    {
        // 方法 1: GetPrefab メソッド使用
        GameObject towerPrefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.Tower);
        if (towerPrefab == null)
        {
            Debug.LogWarning("Tower prefab not found");
            return;
        }
        
        Tower tower = Instantiate(towerPrefab, position, Quaternion.identity)
            .GetComponent<Tower>();
        if (tower == null)
        {
            Debug.LogWarning("Tower component not found on prefab");
            return;
        }
        
        tower.Initialize();
    }
    
    public void SpawnEnemy()
    {
        // 方法 2: プロパティ使用（プロパティが存在する場合）
        GameObject enemyPrefab = PrefabManager.EnemyPrefab;
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab not found");
            return;
        }
        
        Enemy enemy = Instantiate(enemyPrefab).GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogWarning("Enemy component not found on prefab");
            return;
        }
        
        enemy.SetStartPosition(transform.position);
    }
}
```

## パス命名規則

プレハブのパスは以下の規則に従ってください：

```
Prefabs/
├── WorkUnit/              ← ゲームロジック（Puddle など）
│   ├── Puddle
│   └── ...
├── Tower/                 ← タワーディフェンス要素
│   ├── BaseTower
│   └── SpecialTower
├── Enemy/                 ← 敵キャラクター
│   ├── BasicEnemy
│   └── BossEnemy
├── UI/                    ← UI パネル・要素
│   ├── MainMenuPanel
│   └── GameHUD
├── Effects/               ← パーティクル・エフェクト
│   ├── RainParticle
│   └── ExplosionEffect
└── Props/                 ← 環境オブジェクト
    ├── Tree
    └── Building
```

**命名例**:
- ✅ `Prefabs/Tower/BaseTower`
- ✅ `Prefabs/Enemy/BasicEnemy`
- ✅ `Prefabs/Effects/RainParticle`
- ❌ `Prefabs/Tower` （ファイル名として曖昧）
- ❌ `Assets/Prefabs/tower` （大文字小文字混在）

## Null チェックパターン（必須）

PrefabManager からプレハブを取得したら、必ず null チェックしてください。

```csharp
// ✅ CORRECT: 2段階の null チェック
public void CreateGameObject()
{
    // ステップ 1: プレハブ自体の null チェック
    GameObject prefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.Tower);
    if (prefab == null)
    {
        Debug.LogWarning("Tower prefab not found in PrefabManager");
        return;
    }
    
    // ステップ 2: Instantiate 後のコンポーネント null チェック
    GameObject instance = Instantiate(prefab);
    Tower towerComponent = instance.GetComponent<Tower>();
    if (towerComponent == null)
    {
        Debug.LogWarning("Tower component missing on prefab instance");
        Destroy(instance);
        return;
    }
    
    // 処理続行...
    towerComponent.Initialize();
}
```

## 既存の Resources.Load() 呼び出しの移行

### 検出方法

```powershell
# PowerShell で Resources.Load を検出
Get-ChildItem -Recurse -Filter "*.cs" | Select-String "Resources\.Load"
```

### 移行パターン

```csharp
// ❌ 旧: Resources.Load 直接呼び出し
public class OldSystem : MonoBehaviour
{
    private void Start()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/Puddle");
        Instantiate(prefab);
    }
}

// ✅ 新: PrefabManager 経由
public class NewSystem : MonoBehaviour
{
    private void Start()
    {
        GameObject prefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.Puddle);
        if (prefab == null)
        {
            Debug.LogWarning("Puddle prefab not found");
            return;
        }
        Instantiate(prefab);
    }
}
```

## パフォーマンス最適化のヒント

### プレハブのプーリング

頻繁に生成・破棄されるオブジェクトは、事前にインスタンス化してプーリングしてください。

```csharp
public class PuddlePool : MonoBehaviour
{
    private Queue<GameObject> puddlePool = new Queue<GameObject>();
    private const int POOL_SIZE = 20;
    
    private void Start()
    {
        // 初期化時にプールを作成
        GameObject puddlePrefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.Puddle);
        if (puddlePrefab == null)
        {
            Debug.LogWarning("Cannot initialize puddle pool");
            return;
        }
        
        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject puddle = Instantiate(puddlePrefab);
            puddle.SetActive(false);
            puddlePool.Enqueue(puddle);
        }
    }
    
    public GameObject GetPuddle()
    {
        if (puddlePool.Count > 0)
        {
            GameObject puddle = puddlePool.Dequeue();
            puddle.SetActive(true);
            return puddle;
        }
        
        // プール枯渇時は新規作成（ログ出力推奨）
        Debug.LogWarning("Puddle pool exhausted, creating new instance");
        return Instantiate(PrefabManager.GetPrefab(PrefabManager.PrefabType.Puddle));
    }
    
    public void ReturnPuddle(GameObject puddle)
    {
        puddle.SetActive(false);
        puddlePool.Enqueue(puddle);
    }
}
```

## トラブルシューティング

### 「Prefab not found」エラーが出る場合

1. **プレハブパスを確認**
   ```csharp
   Debug.Log($"Looking for: Prefabs/WorkUnit/Puddle");
   ```

2. **Resources フォルダ配置を確認**
   ```
   Assets/
   └── Resources/
       └── Prefabs/
           └── WorkUnit/
               └── Puddle.prefab  ← ここに配置必須
   ```

3. **PrefabManager.cs の辞書エントリを確認**
   ```csharp
   { PrefabType.Puddle, "Prefabs/WorkUnit/Puddle" }  // 正確に一致？
   ```

### キャッシングが効いていない場合

キャッシュはスタティック辞書に保存されます。Unity エディタ再起動まで保持されます。

```csharp
// デバッグ用: キャッシュ状態確認
public class DebugPrefabManager : MonoBehaviour
{
    [ContextMenu("Log Cached Prefabs")]
    public void LogCachedPrefabs()
    {
        Debug.Log("Cached prefabs: " + PrefabManager.GetCacheCount());
    }
}
```

## Pre-Commit Checklist

PrefabManager 関連コード提案前に確認：

- [ ] すべてのプレハブロードが PrefabManager 経由
- [ ] Resources.Load の直接呼び出しなし
- [ ] GetPrefab() 結果に null チェック実装
- [ ] Instantiate 後のコンポーネント null チェック実装
- [ ] 新しいプレハブ型は PrefabType 追加 + 辞書マッピング + プロパティ追加
- [ ] パス名が Assets/Resources からの相対パス（先頭の "Assets/Resources/" なし）
- [ ] エラーログが意図的で、重要な情報を含む

## 関連リソース

- **Unity Resources.Load ドキュメント**: https://docs.unity3d.com/ScriptReference/Resources.Load.html
- **プロジェクト構成**: [docs/architecture.md](../../docs/architecture.md)
- **Recovery ワークフロー**: [docs/recovery-workflow.md](../../docs/recovery-workflow.md)

---

**Last Updated**: 2026-01-20  
**Project**: OnoCoro (Unity 6.3 Geospatial Visualization)
