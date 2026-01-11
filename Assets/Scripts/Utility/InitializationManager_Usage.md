# InitializationManager 使用ガイド

## 概要

`InitializationManager`は、Unityプロジェクトにおけるコンポーネントの初期化順序を制御するためのマネージャークラスです。`Awake`と`Start`のタイミングだけでは不十分な、複雑な依存関係を持つコンポーネントの初期化を確実に行うために使用します。

## 問題点

### 従来の問題

```csharp
// FireCubeCtrl.cs - 部品側
public class FireCubeCtrl : MonoBehaviour
{
    private void Awake()
    {
        // オブジェクトプールの初期化
        InitializePool();
    }
}

// UnitFireDisaster.cs - シーン側
public class UnitFireDisaster : MonoBehaviour
{
    private void Start()
    {
        // FireCubeCtrlのAwakeが完了していることを期待
        GameObject gameManagerObject = GameObjectTreat.GetGameManagerObject();
        FireCubeCtrl fireCubeCtrl = gameManagerObject.GetComponent<FireCubeCtrl>();
        
        // ❌ 動作が遅いマシンでは初期化が完了していない可能性がある
        fireCubeCtrl.SpawnFireCubeAsync(spawnPoint, 2);
    }
}
```

**問題点:**
- Startメソッドが呼ばれる時点で、他のコンポーネントのAwakeが完了している保証がない
- 非同期処理が完了していない可能性がある
- 動作が遅いマシンで不具合が発生する

## セットアップ

### 1. InitializationManagerの配置

```csharp
// シーンに空のGameObjectを作成し、"InitializationManager"と命名
// InitializationManagerスクリプトをアタッチ
```

### 2. Script Execution Orderの設定

**Unity Editor:**
1. `Edit` → `Project Settings` → `Script Execution Order`
2. `+`ボタンをクリック
3. `InitializationManager`を選択
4. 優先度を `-100` に設定（最優先で実行）

```
Script Execution Order:
├─ InitializationManager: -100
├─ FireCubeCtrl: 0 (デフォルト)
├─ GarbageCubeCtrl: 0 (デフォルト)
└─ UnitFireDisaster: 100
```

## 使用方法

### パターン1: 全体の初期化完了を待つ（シンプル）

最も基本的な使い方です。全ての初期化が完了するまで待機します。

```csharp
using System.Collections;
using UnityEngine;

public class UnitFireDisaster : MonoBehaviour
{
    private IEnumerator Start()
    {
        // InitializationManagerの初期化完了を待つ
        yield return new WaitUntil(() => InitializationManager.IsInitialized);
        
        // ここから安全に処理を開始できる
        ChangeDemMeshSize();
        int num = 50;
        float distance = 1f;
        float distance2 = 6f / Mathf.Sqrt(2f);
        
        SettingWalls(num, distance2);
        SettingWaterTurret(num - 1, distance);
        SettingCubes(num, distance);
        FireCubeCtrl.SpawnFireCube(new Vector3(0f, 1f, 0f));
    }
}
```

### パターン2: 部品側に初期化完了フラグを実装

部品側でより細かい制御が必要な場合。

```csharp
// FireCubeCtrl.cs - 部品側
public class FireCubeCtrl : MonoBehaviour
{
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;
    
    private List<GameObject> firePool = new List<GameObject>();
    
    private void Awake()
    {
        // 自身のみの初期化（他のオブジェクトに依存しない）
    }
    
    private IEnumerator Start()
    {
        // 依存する他のコンポーネントの初期化を待つ
        yield return new WaitUntil(() => InitializationManager.IsInitialized);
        
        // プールの初期化など、時間のかかる処理
        yield return InitializePool();
        
        // 初期化完了
        isInitialized = true;
    }
    
    private IEnumerator InitializePool()
    {
        // オブジェクトプールの初期化
        for (int i = 0; i < 100; i++)
        {
            GameObject obj = Instantiate(firePrefab);
            obj.SetActive(false);
            firePool.Add(obj);
            
            // 10個ごとに1フレーム待機（負荷分散）
            if (i % 10 == 0)
            {
                yield return null;
            }
        }
    }
}

// UnitFireDisaster.cs - シーン側
public class UnitFireDisaster : MonoBehaviour
{
    private IEnumerator Start()
    {
        // 特定のコンポーネントの初期化を待つ
        GameObject gameManagerObject = GameObjectTreat.GetGameManagerObject();
        FireCubeCtrl fireCubeCtrl = gameManagerObject.GetComponent<FireCubeCtrl>();
        
        if (fireCubeCtrl != null)
        {
            // ✅ FireCubeCtrlの初期化が確実に完了するまで待機
            yield return new WaitUntil(() => fireCubeCtrl.IsInitialized);
        }
        
        // 安全に処理を開始
        SettingCubes(50, 1f);
    }
}
```

### パターン3: ステップごとの初期化制御（高度）

複数のステップに分けて初期化を制御する場合。

```csharp
// InitializationManager.cs の設定
private void RegisterInitializationSteps()
{
    initializationSteps["ResourceLoader"] = false;
    initializationSteps["AudioManager"] = false;
    initializationSteps["FireCubeCtrl"] = false;
    initializationSteps["GameManager"] = false;
}

private IEnumerator InitializeManagers()
{
    Debug.Log("[InitializationManager] マネージャーを初期化中...");
    
    // FireCubeCtrlの初期化
    GameObject gameManagerObj = GameObjectTreat.GetGameManagerObject();
    if (gameManagerObj != null)
    {
        FireCubeCtrl fireCubeCtrl = gameManagerObj.GetComponent<FireCubeCtrl>();
        if (fireCubeCtrl != null)
        {
            yield return new WaitUntil(() => fireCubeCtrl.IsInitialized);
            MarkStepAsInitialized("FireCubeCtrl");
        }
    }
    
    // GarbageCubeCtrlの初期化
    if (gameManagerObj != null)
    {
        GarbageCubeCtrl garbageCubeCtrl = gameManagerObj.GetComponent<GarbageCubeCtrl>();
        if (garbageCubeCtrl != null)
        {
            yield return new WaitUntil(() => garbageCubeCtrl.IsInitialized);
            MarkStepAsInitialized("GarbageCubeCtrl");
        }
    }
}

// 使用側
public class SomeComponent : MonoBehaviour
{
    private IEnumerator Start()
    {
        // 特定のステップの完了だけを待つ
        yield return new WaitUntil(() => 
            InitializationManager.IsStepInitialized("FireCubeCtrl"));
        
        // FireCubeCtrlだけが必要な処理を実行
        DoSomethingWithFireCube();
    }
}
```

### パターン4: 非同期処理の完了待ち

非同期メソッドを使用する場合。

```csharp
using System.Threading.Tasks;
using UnityEngine;

public class AsyncInitializer : MonoBehaviour
{
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;
    
    private async void Start()
    {
        // InitializationManagerの完了を待つ
        while (!InitializationManager.IsInitialized)
        {
            await Task.Yield();
        }
        
        // 非同期初期化処理
        await InitializeAsync();
        
        isInitialized = true;
    }
    
    private async Task InitializeAsync()
    {
        // Addressablesのロードなど
        var handle = Addressables.LoadAssetAsync<GameObject>("FireCube");
        await handle.Task;
        
        // データベースからの読み込みなど
        await LoadDataFromDatabase();
    }
}
```

## ベストプラクティス

### 1. Awakeでは自己完結型の初期化のみ

```csharp
private void Awake()
{
    // ✅ 良い例: 自分自身のフィールド初期化
    myTransform = transform;
    myRigidbody = GetComponent<Rigidbody>();
    
    // ❌ 悪い例: 他のオブジェクトへの依存
    // gameManager = GameObject.Find("GameManager");
}
```

### 2. Startで依存関係を解決

```csharp
private IEnumerator Start()
{
    // 初期化マネージャーの完了を待つ
    yield return new WaitUntil(() => InitializationManager.IsInitialized);
    
    // 他のオブジェクトへの依存を解決
    gameManager = GameObject.Find("GameManager");
    fireCubeCtrl = gameManager.GetComponent<FireCubeCtrl>();
}
```

### 3. 初期化完了フラグの実装

```csharp
public class MyComponent : MonoBehaviour
{
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;
    
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => InitializationManager.IsInitialized);
        
        // 初期化処理
        yield return DoInitialization();
        
        isInitialized = true;
    }
}
```

### 4. 負荷の高い処理は分散

```csharp
private IEnumerator InitializePool()
{
    for (int i = 0; i < 1000; i++)
    {
        // 処理
        CreatePoolObject(i);
        
        // 10個ごとにフレームを分散
        if (i % 10 == 0)
        {
            yield return null;
        }
    }
}
```

## トラブルシューティング

### Q1: InitializationManager.IsInitializedが常にfalse

**原因:**
- InitializationManagerがシーンに配置されていない
- Script Execution Orderが正しく設定されていない

**解決:**
1. シーンにInitializationManagerを配置
2. Script Execution Orderを確認

### Q2: 特定のコンポーネントだけ初期化が遅い

**原因:**
- そのコンポーネントで重い処理をしている
- 他のコンポーネントへの依存が多い

**解決:**
```csharp
// ステップごとの初期化で、そのコンポーネントだけ待つ
yield return new WaitUntil(() => 
    InitializationManager.IsStepInitialized("SlowComponent"));
```

### Q3: エディタでは動くがビルドで動かない

**原因:**
- Script Execution Orderがビルドに含まれていない
- 条件付きコンパイルの問題

**解決:**
- Project Settingsを確認
- `#if UNITY_EDITOR`を使っていないか確認

## サンプルコード集

### 完全な実装例

```csharp
// GarbageCubeCtrl.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCubeCtrl : MonoBehaviour
{
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;
    
    [SerializeField] private GameObject garbageCubePrefab;
    private List<GameObject> garbagePool = new List<GameObject>();
    private const int INITIAL_POOL_SIZE = 100;
    
    private void Awake()
    {
        // プレハブの参照取得のみ
        if (garbageCubePrefab == null)
        {
            garbageCubePrefab = Resources.Load<GameObject>("Prefabs/GarbageCube");
        }
    }
    
    private IEnumerator Start()
    {
        // 初期化マネージャーの完了を待つ
        yield return new WaitUntil(() => InitializationManager.IsInitialized);
        
        // オブジェクトプールの初期化
        yield return InitializePool();
        
        isInitialized = true;
        Debug.Log("[GarbageCubeCtrl] 初期化完了");
    }
    
    private IEnumerator InitializePool()
    {
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            GameObject obj = Instantiate(garbageCubePrefab, transform);
            obj.SetActive(false);
            garbagePool.Add(obj);
            
            // 10個ごとに1フレーム待機
            if (i % 10 == 0)
            {
                yield return null;
            }
        }
    }
    
    public void SpawnGarbageCubeAsync(Vector3 position)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[GarbageCubeCtrl] まだ初期化が完了していません");
            return;
        }
        
        GameObject cube = GetPooledObject();
        if (cube != null)
        {
            cube.transform.position = position;
            cube.SetActive(true);
        }
    }
    
    private GameObject GetPooledObject()
    {
        foreach (GameObject obj in garbagePool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        
        // プールが足りない場合は拡張
        GameObject newObj = Instantiate(garbageCubePrefab, transform);
        newObj.SetActive(false);
        garbagePool.Add(newObj);
        return newObj;
    }
}
```

## まとめ

- **Awake**: 自己完結型の初期化のみ
- **Start**: InitializationManagerの完了を待ってから依存解決
- **IsInitializedフラグ**: 各コンポーネントに実装
- **Script Execution Order**: InitializationManagerを最優先に設定
- **負荷分散**: yield return nullで処理を分散

これにより、動作が遅いマシンでも確実に初期化が完了してから処理を開始できます。
