# PLATEAU SDK 統合・GIS データ処理

**目的**: PLATEAU SDK 使用方法・CityGML 解析・座標変換・パフォーマンス最適化

---

## PLATEAU SDK の役割

OnoCoro は **PLATEAU SDK** を使用して、日本の都市 3D データ（CityGML 形式）を処理・可視化します。

| 機能 | 説明 |
|------|------|
| **CityGML 解析** | 標準化 3D 都市データフォーマットの読み込み |
| **座標系変換** | 測地座標（緯度経度）↔ Unity ローカル座標 |
| **メッシュ生成** | GIS 属性から Unity メッシュへの自動変換 |
| **マテリアル割り当て** | 建物種別による自動マテリアル設定 |
| **LOD 管理** | 大規模データの詳細度管理 |

---

## CityGML の基本構造

### CityGML とは

[NOTE] **OGC（Open Geospatial Consortium）の国際標準**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<CityModel xmlns="http://www.opengis.net/citygml/2.0">
  <cityObjectMember>
    <bldg:Building gml:id="BLD001">
      <!-- 建物 ID -->
      
      <gml:boundedBy>
        <gml:Envelope srsName="urn:ogc:def:crs:EPSG::6697">
          <gml:lowerCorner>139.123 35.456 0</gml:lowerCorner>
          <gml:upperCorner>139.124 35.457 50</gml:upperCorner>
        </gml:Envelope>
      </gml:boundedBy>
      
      <bldg:function>6101</bldg:function>
      <!-- 建物種別コード: 6101 = 一般家庭 -->
      
      <bldg:lod2Solid>
        <!-- 3D メッシュデータ -->
      </bldg:lod2Solid>
    </bldg:Building>
  </cityObjectMember>
</CityModel>
```

### 建物種別コード（Function Code）

| コード | 日本語 | 描画色 |
|--------|--------|--------|
| **6101** | 住宅 | 白・クリーム色 |
| **6102** | 共同住宅 | 薄灰色 |
| **6201** | 商業施設 | 赤・オレンジ |
| **6202** | 事務所 | 濃灰色 |
| **6203** | 工場 | 茶色 |
| **6204** | 駅舎 | 黄色 |

---

## PLATEAU SDK 使用パターン

### Step 1: CityGML ロード

```csharp
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using Debug = CommonsUtility.Debug;

public class PlateauLoader : MonoBehaviour
{
    private PlateauCityModel _cityModel;
    
    public void LoadCityData(string datasetPath)
    {
        // [OK] null チェック
        if (string.IsNullOrEmpty(datasetPath))
        {
            Debug.LogWarning("Dataset path is empty");
            return;
        }
        
        try
        {
            // [OK] ロード
            _cityModel = new PlateauCityModel();
            _cityModel.LoadFromFile(datasetPath);
            
            if (_cityModel == null)
            {
                Debug.LogWarning($"Failed to load: {datasetPath}");
                return;
            }
            
            Debug.Log($"Successfully loaded: {_cityModel.Name}");
            ProcessCityData();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception loading city data: {ex.Message}");
        }
    }
    
    private void ProcessCityData()
    {
        // 建物データを処理
        foreach (Building building in _cityModel.Buildings)
        {
            CreateBuildingGameObject(building);
        }
    }
}
```

### Step 2: 座標変換

```csharp
using PLATEAU.CityInfo;

public class CoordinateTransform : MonoBehaviour
{
    // [OK] 東京を中心座標として使用
    private const float REFERENCE_LATITUDE = 35.6812f;
    private const float REFERENCE_LONGITUDE = 139.7671f;
    private const float REFERENCE_ALTITUDE = 0f;
    
    public Vector3 ConvertGeoToUnity(double lat, double lon, float altitude)
    {
        // 測地座標 → Unity ローカル座標への変換
        // 1度 ≈ 111.32 km（赤道近く）
        
        const float DEGREE_TO_METERS = 111320f;
        
        float x = (float)(lon - REFERENCE_LONGITUDE) * DEGREE_TO_METERS;
        float z = (float)(lat - REFERENCE_LATITUDE) * DEGREE_TO_METERS;
        float y = altitude - REFERENCE_ALTITUDE;
        
        return new Vector3(x, y, z);
    }
    
    public void ConvertGeoToUnityArray(
        double[] latitudes,
        double[] longitudes,
        float[] altitudes,
        Vector3[] unityPositions
    )
    {
        // [OK] 配列変換（大量データ向け）
        for (int i = 0; i < latitudes.Length; i++)
        {
            unityPositions[i] = ConvertGeoToUnity(
                latitudes[i],
                longitudes[i],
                altitudes[i]
            );
        }
    }
}
```

### Step 3: 建物 GameObject 生成

```csharp
public class BuildingMeshGenerator : MonoBehaviour
{
    private MaterialManager _materialManager;
    
    public void CreateBuildingGameObject(Building building)
    {
        if (building == null)
        {
            Debug.LogWarning("Building is null");
            return;
        }
        
        // [STEP 1] GameObject 作成
        GameObject buildingObject = new GameObject(building.ID);
        
        // [STEP 2] 座標設定
        Vector3 position = ConvertBuildingPosition(building);
        buildingObject.transform.position = position;
        
        // [STEP 3] メッシュ生成
        Mesh mesh = GenerateMesh(building);
        if (mesh == null)
        {
            Debug.LogWarning($"Failed to generate mesh for {building.ID}");
            Destroy(buildingObject);
            return;
        }
        
        // [STEP 4] MeshFilter・MeshRenderer 設定
        MeshFilter meshFilter = buildingObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        
        MeshRenderer meshRenderer = buildingObject.AddComponent<MeshRenderer>();
        
        // [STEP 5] マテリアル割り当て
        Material material = _materialManager.GetMaterialByFunctionCode(
            building.FunctionCode
        );
        meshRenderer.material = material;
        
        // [STEP 6] Collider 追加（衝突判定用）
        MeshCollider collider = buildingObject.AddComponent<MeshCollider>();
        collider.convex = false;  // 複雑なメッシュの場合
        
        Debug.Log($"Building '{building.ID}' created at {position}");
    }
    
    private Mesh GenerateMesh(Building building)
    {
        // [OK] PLATEAU SDK のメッシュデータから Unity Mesh を生成
        Mesh mesh = new Mesh();
        mesh.name = building.ID;
        
        // 頂点データ・三角形インデックスを設定
        mesh.vertices = building.GetVertices();
        mesh.triangles = building.GetTriangles();
        mesh.uv = building.GetUVCoordinates();
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
}
```

---

## パフォーマンス最適化

### LOD（Level of Detail）システム

[OK] **大規模データは LOD を使用して詳細度を調整**

```csharp
public class LODManager : MonoBehaviour
{
    public enum LODLevel
    {
        LOD0,  // 超高詳細度（カメラ近い）
        LOD1,  // 高詳細度
        LOD2,  // 中詳細度（PLATEAU SDK 標準）
        LOD3,  // 低詳細度
        LOD4   // 超低詳細度（カメラ遠い）
    }
    
    public Mesh GetMeshByLOD(Building building, LODLevel lod)
    {
        switch (lod)
        {
            case LODLevel.LOD0:
                return building.GetLOD0Mesh();  // 最も詳細
            case LODLevel.LOD1:
                return building.GetLOD1Mesh();
            case LODLevel.LOD2:
                return building.GetLOD2Mesh();  // 推奨
            case LODLevel.LOD3:
                return building.GetLOD3Mesh();
            case LODLevel.LOD4:
                return building.GetLOD4Mesh();  // 最も簡略
            default:
                return building.GetLOD2Mesh();
        }
    }
}
```

### Culling（非表示オブジェクトのスキップ）

[OK] **カメラの視錐台外のオブジェクトをレンダリングしない**

```csharp
public class BuildingCullingManager : MonoBehaviour
{
    private Camera _mainCamera;
    
    private void Start()
    {
        _mainCamera = Camera.main;
    }
    
    private void Update()
    {
        // 視錐台カリング（Unity 自動）
        // MeshRenderer は自動的に視錐台外でレンダリングスキップ
        
        // カスタム距離カリング
        float cameraHeight = _mainCamera.transform.position.y;
        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
        {
            // 距離に応じて表示・非表示
            float distance = Vector3.Distance(
                renderer.transform.position,
                _mainCamera.transform.position
            );
            
            renderer.enabled = distance < 5000f;  // 5000m 以内のみ表示
        }
    }
}
```

### メッシュ結合（DrawCall 削減）

[OK] **複数の小さなメッシュを 1 つに結合してパフォーマンス向上**

```csharp
public class MeshCombiner : MonoBehaviour
{
    public void CombineMeshes(Building[] buildings)
    {
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        
        foreach (Building building in buildings)
        {
            // [STEP 1] 各建物のメッシュを取得
            Mesh mesh = GenerateMesh(building);
            
            // [STEP 2] 変換行列（位置・回転）を設定
            CombineInstance ci = new CombineInstance();
            ci.mesh = mesh;
            ci.transform = building.TransformMatrix;
            
            combineInstances.Add(ci);
        }
        
        // [STEP 3] メッシュを結合
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(
            combineInstances.ToArray(),
            mergeSubMeshes: true,
            useMatrices: true
        );
        
        // [STEP 4] MeshFilter に設定
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = combinedMesh;
        
        Debug.Log($"Combined {buildings.Length} buildings into single mesh");
    }
}
```

---

## GIS データのキャッシング

### データキャッシング戦略

[OK] **一度読み込んだ CityGML データをメモリにキャッシュ**

```csharp
public class GISDataCache : MonoBehaviour
{
    private static Dictionary<string, PlateauCityModel> _cache = 
        new Dictionary<string, PlateauCityModel>();
    
    public static PlateauCityModel GetCityData(string datasetPath)
    {
        // [STEP 1] キャッシュ確認
        if (_cache.ContainsKey(datasetPath))
        {
            Debug.Log($"Using cached data: {datasetPath}");
            return _cache[datasetPath];
        }
        
        // [STEP 2] キャッシュにない場合ロード
        PlateauCityModel cityModel = LoadCityData(datasetPath);
        
        if (cityModel != null)
        {
            // [STEP 3] キャッシュに登録
            _cache[datasetPath] = cityModel;
        }
        
        return cityModel;
    }
    
    private static PlateauCityModel LoadCityData(string datasetPath)
    {
        try
        {
            PlateauCityModel model = new PlateauCityModel();
            model.LoadFromFile(datasetPath);
            return model;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load GIS data: {ex.Message}");
            return null;
        }
    }
}
```

---

## 座標系の注意点

### EPSG コード（空間参照系）

[WARN] **日本のデータは複数の座標系混在**

| EPSG コード | 説明 | 用途 |
|-----------|------|------|
| **6677** | JGD2011 / Japan Plane Rectangular CS I（平面直角座標系 I） | 東日本 |
| **6678** | JGD2011 / Japan Plane Rectangular CS II | 西日本 |
| **4612** | JGD2000（地理座標系） | 緯度経度 |
| **6697** | JGD2011（地理座標系） | 緯度経度（最新） |

[OK] **PLATEAU SDK は自動で座標系を変換**

```csharp
// [OK] PLATEAU SDK が EPSG コード自動解釈
PlateauCityModel cityModel = new PlateauCityModel();
cityModel.LoadFromFile("citydata.gml");  // 自動で座標系変換

// 結果は Unity のワールド座標系に統一される
```

---

## トラブルシューティング

### "CityGML ファイルが読み込めない"

[STEP] **ファイル形式確認**

```csharp
// [OK] 検証
if (!File.Exists(datasetPath))
{
    Debug.LogError($"File not found: {datasetPath}");
    return;
}

// ファイルサイズ確認（10MB 以上の場合は要時間）
FileInfo fileInfo = new FileInfo(datasetPath);
Debug.Log($"File size: {fileInfo.Length / 1024 / 1024}MB");
```

### "建物メッシュがおかしい（反転、穴あき）"

[STEP] **法線（Normal）の再計算**

```csharp
// [OK] 法線を再計算
mesh.RecalculateNormals();
mesh.RecalculateTangents();

// 裏面カリングを確認
MeshRenderer renderer = GetComponent<MeshRenderer>();
renderer.material.SetFloat("_Cull", 2);  // Back culling
```

### "パフォーマンスが低い（FPS 低下）"

[STEP] **LOD・Culling 有効化確認**

```csharp
// [OK] プロファイラで DrawCall 確認
Debug.Log($"Total objects: {FindObjectsOfType<MeshRenderer>().Length}");

// LOD Group を使用
LODGroup lodGroup = gameObject.AddComponent<LODGroup>();
LOD[] lods = new LOD[2];
// LOD 0: 100% detail
// LOD 1: 50% detail
lodGroup.SetLODs(lods);
```

---

## チェックリスト

PLATEAU 統合実装時：

- [ ] **ファイルロード**: CityGML ファイルパス確認
- [ ] **null チェック**: PlateauCityModel・Building が null でないか
- [ ] **座標変換**: GEO 座標 → Unity 座標が正確か
- [ ] **メッシュ生成**: 法線・UV が正確に計算されているか
- [ ] **マテリアル**: 建物種別に応じた色が割り当てられているか
- [ ] **LOD**: 遠方でも LOD が切り替わっているか
- [ ] **Culling**: オフスクリーン オブジェクトがスキップされているか
- [ ] **パフォーマンス**: 1000+ 建物で 60 FPS 維持できるか

---

**関連資料**:
- [initialization-flow.md](initialization-flow.md) - GIS ロード初期化
- [../../AGENTS.md](../../AGENTS.md) - Null チェック基準
- [project-rules/coding-csharp.md](../project-rules/coding-csharp.md) - C# コーディング規約
