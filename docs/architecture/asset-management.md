# アセット管理・Prefab ローディング

**目的**: PrefabManager 使用方法・キャッシング戦略・Prefab 追加手順の統一

---

## アセット管理ポリシー

### 基本原則

[WARN] **すべての Prefab ロードは PrefabManager 経由のみ**

```csharp
// [OK] PrefabManager 経由
GameObject prefab = PrefabManager.GetPrefab("TowerPrefab");

// [NG] 直接 Resources.Load（禁止）
GameObject prefab = Resources.Load<GameObject>("Prefabs/Tower/TowerPrefab");
```

### メリット

| メリット | 説明 |
|---------|------|
| **一元管理** | すべての Prefab パスを 1 箇所で定義 |
| **キャッシング** | 同じ Prefab は再度ロードしない |
| **保守性** | パス変更時に 1 箇所の修正で反映 |
| **Null チェック** | 存在しない Prefab を自動検出・ログ |

---

## PrefabManager の使用方法

### 基本的な Prefab 参照

```csharp
public class EnemySpawner : MonoBehaviour
{
    public void SpawnEnemy()
    {
        // [OK] PrefabManager 経由でロード
        GameObject enemyPrefab = PrefabManager.GetPrefab("EnemyLitterPrefab");
        
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemyLitterPrefab not found in PrefabManager");
            return;
        }
        
        // Instantiate
        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity
        );
        
        Debug.Log($"Spawned enemy at {spawnPosition}");
    }
}
```

### Prefab キャッシング（最適化）

[OK] **頻繁に使用する Prefab はフィールドにキャッシュ**

```csharp
public class TowerFactory : MonoBehaviour
{
    private GameObject _towerPrefab;
    
    private void Awake()
    {
        // 初回ロード（キャッシュに登録）
        _towerPrefab = PrefabManager.GetPrefab("TowerSentryGuardPrefab");
        
        if (_towerPrefab == null)
        {
            Debug.LogError("TowerSentryGuardPrefab not found");
            enabled = false;
            return;
        }
    }
    
    public void CreateTower(Vector3 position)
    {
        // 以降は _towerPrefab を使用（キャッシュから高速）
        Instantiate(_towerPrefab, position, Quaternion.identity);
    }
}
```

### 複数 Prefab の一括ロード

```csharp
public class GameManager : MonoBehaviour
{
    private GameObject _towerPrefab;
    private GameObject _enemyPrefab;
    private GameObject _bulletPrefab;
    
    private void Initialize()
    {
        // [OK] 必要な Prefab を全て先読み
        _towerPrefab = PrefabManager.GetPrefab("TowerSentryGuardPrefab");
        _enemyPrefab = PrefabManager.GetPrefab("EnemyLitterPrefab");
        _bulletPrefab = PrefabManager.GetPrefab("ProjectileBulletPrefab");
        
        // null チェック
        if (_towerPrefab == null || _enemyPrefab == null || _bulletPrefab == null)
        {
            Debug.LogError("One or more required prefabs not found");
            enabled = false;
            return;
        }
    }
}
```

---

## Prefab フォルダ構成

### 推奨ディレクトリ構成

```
Assets/Resources/Prefabs/
├── UI/
│   ├── ButtonPrefab.prefab
│   ├── PanelPrefab.prefab
│   └── MessageBoxPrefab.prefab
├── Units/
│   ├── Towers/
│   │   ├── TowerSentryGuardPrefab.prefab
│   │   └── TowerDefensePrefab.prefab
│   ├── Enemies/
│   │   ├── EnemyLitterPrefab.prefab
│   │   └── EnemyPlasticPrefab.prefab
│   ├── Items/
│   │   ├── CoinItemPrefab.prefab
│   │   └── HealthItemPrefab.prefab
│   └── Projectiles/
│       ├── ProjectileBulletPrefab.prefab
│       └── ProjectileExplosionPrefab.prefab
└── VFX/
    ├── ExplosionVFXPrefab.prefab
    ├── DamageIndicatorVFXPrefab.prefab
    └── ParticleEffectPrefab.prefab
```

### 命名規則

[OK] **Prefab 名は機能を明確に示す**

```csharp
// [OK] 明確な命名
TowerSentryGuardPrefab      // 監視塔タワー
EnemyLitterPrefab           // ゴミ敵
ProjectileBulletPrefab      // 発射物 - 通常弾
ExplosionVFXPrefab          // エフェクト - 爆発

// [NG] 曖昧な命名
Tower                        // ❌ どのタワー？
Enemy                        // ❌ どの敵？
Projectile                   // ❌ どのタイプ？
```

---

## Prefab 追加手順

新しい Prefab を PrefabManager に追加する場合：

### Step 1: Prefab 作成・配置

```
[1] Scene に GameObject を構築
[2] 確認・テスト
[3] Assets/Resources/Prefabs/ 下の適切なフォルダに保存
    例: Assets/Resources/Prefabs/Towers/NewTowerPrefab.prefab
```

### Step 2: PrefabManager に定義を追加

```csharp
public static class PrefabManager
{
    // [NEW] 新しい Prefab パスを追加
    private const string NEW_TOWER_PREFAB_PATH = "Prefabs/Towers/NewTowerPrefab";
    
    public static GameObject GetPrefab(string prefabName)
    {
        switch (prefabName)
        {
            // [EXISTING]
            case "TowerSentryGuardPrefab":
                return LoadPrefab("Prefabs/Towers/TowerSentryGuardPrefab");
            
            // [NEW]
            case "NewTowerPrefab":
                return LoadPrefab(NEW_TOWER_PREFAB_PATH);
            
            default:
                Debug.LogWarning($"Prefab '{prefabName}' not found in PrefabManager");
                return null;
        }
    }
}
```

### Step 3: 使用コード作成

```csharp
public class MyNewTowerFactory : MonoBehaviour
{
    private GameObject _newTowerPrefab;
    
    private void Awake()
    {
        // [NEW] PrefabManager から取得
        _newTowerPrefab = PrefabManager.GetPrefab("NewTowerPrefab");
        
        if (_newTowerPrefab == null)
        {
            Debug.LogError("NewTowerPrefab not found");
            enabled = false;
        }
    }
}
```

### Step 4: テスト・確認

```csharp
// [TEST] Prefab が正常にロードされるか確認
private void Start()
{
    GameObject tower = PrefabManager.GetPrefab("NewTowerPrefab");
    Debug.Log($"Tower prefab loaded: {tower.name}");  // "NewTowerPrefab"
    
    GameObject instance = Instantiate(tower);
    Debug.Log($"Tower instance created: {instance.name}");
}
```

---

## キャッシング戦略

### キャッシング仕組み

```csharp
public static class PrefabManager
{
    // [OK] Dictionary でキャッシング
    private static Dictionary<string, GameObject> _prefabCache = 
        new Dictionary<string, GameObject>();
    
    public static GameObject GetPrefab(string prefabName)
    {
        // [STEP 1] キャッシュを確認
        if (_prefabCache.ContainsKey(prefabName))
        {
            return _prefabCache[prefabName];  // キャッシュから返す
        }
        
        // [STEP 2] キャッシュにない場合ロード
        GameObject prefab = LoadPrefab(GetPrefabPath(prefabName));
        
        if (prefab != null)
        {
            // [STEP 3] キャッシュに登録
            _prefabCache[prefabName] = prefab;
        }
        
        return prefab;
    }
}
```

### パフォーマンスの利点

| 状況 | 無キャッシュ | キャッシュあり |
|------|-----------|------------|
| 1回目ロード | Disk 読み込み（遅い） | Disk 読み込み（遅い） |
| 2回目以降 | Disk 読み込み（遅い） | メモリ参照（高速） |
| メモリ | なし | Prefab サイズ分使用 |

[RECOMMENDED] **頻繁に使用する Prefab はキャッシング**

---

## Prefab Variant と AssetReference

### Prefab Variant（軽量な派生）

[OK] **同一タイプで細微な違いがある場合**

```
TowerSentryGuardPrefab (Base)
├─ TowerSentryGuardDamagedVariant (ダメージ状態)
└─ TowerSentryGuardIceVariant (冷却効果)
```

### AssetReference（遅延ロード）

[ALTERNATIVE] **必要になるまでロードを遅延させたい場合**

```csharp
public class LazyPrefabLoader : MonoBehaviour
{
    private AssetReference _towerReference;
    
    public async Task LoadTowerAsync()
    {
        // 非同期でロード
        GameObject tower = await _towerReference.LoadAssetAsync<GameObject>();
        Instantiate(tower);
    }
}
```

[NOTE] **OnoCoro では Addressables 未使用** - PrefabManager のみ使用

---

## トラブルシューティング

### "Prefab not found" エラー

[STEP] **パス確認**

```csharp
// [NG] 正確なパスでない
LoadPrefab("Prefabs/Tower");          // ❌ 拡張子なし
LoadPrefab("Prefabs/Towers");         // ❌ フォルダ名複数形

// [OK] 正確なパス
LoadPrefab("Prefabs/Towers/TowerSentryGuardPrefab");
```

### "Prefab が毎回ロードされ、パフォーマンスが悪い"

[STEP] **キャッシングの確認・フィールド保存**

```csharp
// [NG] 毎回ロード（パフォーマンス悪い）
public void SpawnTower()
{
    GameObject prefab = PrefabManager.GetPrefab("TowerPrefab");
    Instantiate(prefab);
}

// [OK] キャッシング＋フィールド保存（高速）
private GameObject _cachedTowerPrefab;

private void Start()
{
    _cachedTowerPrefab = PrefabManager.GetPrefab("TowerPrefab");
}

public void SpawnTower()
{
    Instantiate(_cachedTowerPrefab);
}
```

### "Prefab インスタンスの初期化が失敗する"

[STEP] **null チェック・Awake の確認**

```csharp
// [OK] Prefab + Instance の両方 null チェック
GameObject prefab = PrefabManager.GetPrefab("TowerPrefab");
if (prefab == null)
{
    Debug.LogError("Prefab not found");
    return;
}

GameObject instance = Instantiate(prefab);
if (instance == null)
{
    Debug.LogError("Failed to instantiate");
    return;
}

// Instance の初期化を待つ
TowerController controller = instance.GetComponent<TowerController>();
if (controller == null)
{
    Debug.LogError("TowerController not found on instance");
    Destroy(instance);
    return;
}
```

---

## チェックリスト

Prefab 管理実装時：

- [ ] **PrefabManager 経由**: Resources.Load 直接呼び出しなし
- [ ] **パス定義**: 定数で一元管理
- [ ] **null チェック**: ロード後に null 確認
- [ ] **キャッシング**: 頻繁に使用する Prefab をフィールド保存
- [ ] **フォルダ構成**: Assets/Resources/Prefabs/ 下に整理
- [ ] **命名規則**: 機能を明確に示す名前
- [ ] **テスト**: 各 Prefab が正常にロード・Instantiate されるか確認

---

**関連資料**:
- [initialization-flow.md](initialization-flow.md) - Phase 1 PrefabManager 初期化
- [ui-system.md](ui-system.md) - UI Prefab 管理
- [project-rules/naming-conventions.md](../project-rules/naming-conventions.md) - 命名規則
- [recovery-guidelines.md](recovery-guidelines.md) - null チェックのベストプラクティス
