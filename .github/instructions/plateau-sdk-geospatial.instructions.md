---
title: OnoCoro PLATEAU SDK Geospatial Development
description: Best practices for PLATEAU SDK integration, CityGML data processing, coordinate transformation, and 3D geospatial rendering in OnoCoro
applyTo: "*.cs"
---

# OnoCoro PLATEAU SDK Geospatial Development

## 概要

OnoCoro は **PLATEAU SDK** を使用して、日本の都市 3D データ（CityGML 形式）を処理・可視化するゲームです。このドキュメントでは、PLATEAU SDK の適切な使用方法、座標系の理解、パフォーマンス最適化について説明します。

## 参考資料

- [AGENTS.md](../../AGENTS.md) - プロジェクト全体ルール
- [docs/coding-standards.md](../../docs/coding-standards.md) - C# 基準
- [docs/architecture.md](../../docs/architecture.md) - システム構成
- [unity-csharp-recovery.instructions.md](unity-csharp-recovery.instructions.md) - C# Recovery 実装

## PLATEAU SDK の役割

| 要素 | 説明 |
|------|------|
| **CityGML 解析** | 標準化された 3D 都市データフォーマットの読み込み・解析 |
| **座標系変換** | 測地座標系（緯度経度） ↔ Unity ローカル座標系への自動変換 |
| **メッシュ生成** | GIS 属性データから Unity メッシュへの変換 |
| **マテリアル割り当て** | 建物種別（住宅、商業など）による自動マテリアル設定 |
| **LOD 管理** | 大規模データの詳細度管理（パフォーマンス最適化） |

## 基本的な使用パターン

### ステップ 1: PLATEAU データの初期化

```csharp
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;

public class PlateauDataManager : MonoBehaviour
{
    private PlateauCityModel cityModel;
    private Dictionary<string, GameObject> buildingObjects = new Dictionary<string, GameObject>();
    
    public void LoadCityData(string datasetPath)
    {
        // ✅ CORRECT: PLATEAU SDK を使用した CityGML ロード
        if (string.IsNullOrEmpty(datasetPath))
        {
            Debug.LogWarning("Dataset path is empty");
            return;
        }
        
        try
        {
            cityModel = new PlateauCityModel();
            cityModel.LoadFromFile(datasetPath);
            
            if (cityModel == null)
            {
                Debug.LogWarning("Failed to load city model from: " + datasetPath);
                return;
            }
            
            Debug.Log($"Successfully loaded city model: {cityModel.Name}");
            ProcessCityData();
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Exception loading city data: {exception.Message}");
        }
    }
    
    private void ProcessCityData()
    {
        // 建物データの処理...
    }
}
```

### ステップ 2: 座標系の理解と変換

PLATEAU SDK では、**測地座標系**（WGS84）から Unity **ローカル座標系**への変換が必要です。

```csharp
using PLATEAU.Geometries;

public class CoordinateTransformer : MonoBehaviour
{
    // PLATEAU の参照座標（例：東京都心）
    private readonly GeoCoordinate REFERENCE_POINT = new GeoCoordinate(
        latitude: 35.6762f,      // 東京：35.6762°N
        longitude: 139.6503f,    // 東京：139.6503°E
        altitude: 0f
    );
    
    /// <summary>
    /// 測地座標（緯度経度高度）を Unity ローカル座標に変換
    /// </summary>
    public Vector3 GeoToUnityCoordinate(GeoCoordinate geoCoord)
    {
        if (geoCoord == null)
        {
            Debug.LogWarning("GeoCoordinate is null");
            return Vector3.zero;
        }
        
        // 緯度経度から相対位置を計算（簡略化版）
        float deltaLat = (float)(geoCoord.Latitude - REFERENCE_POINT.Latitude);
        float deltaLon = (float)(geoCoord.Longitude - REFERENCE_POINT.Longitude);
        float altitude = (float)geoCoord.Altitude;
        
        // 1度あたりのメートル数（緯度：約111km、経度：約91km@東京）
        const float LAT_TO_METER = 111320f;
        const float LON_TO_METER = 91350f; // 東京の値
        
        float unityX = deltaLon * LON_TO_METER;
        float unityZ = deltaLat * LAT_TO_METER;
        float unityY = altitude;
        
        return new Vector3(unityX, unityY, unityZ);
    }
    
    /// <summary>
    /// Unity ローカル座標を測地座標に逆変換
    /// </summary>
    public GeoCoordinate UnityToGeoCoordinate(Vector3 unityPos)
    {
        const float LAT_TO_METER = 111320f;
        const float LON_TO_METER = 91350f;
        
        double latitude = REFERENCE_POINT.Latitude + (unityPos.z / LAT_TO_METER);
        double longitude = REFERENCE_POINT.Longitude + (unityPos.x / LON_TO_METER);
        double altitude = unityPos.y;
        
        return new GeoCoordinate(latitude, longitude, altitude);
    }
}
```

### ステップ 3: 建物メッシュの生成と配置

```csharp
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;

public class BuildingMeshGenerator : MonoBehaviour
{
    public GameObject CreateBuildingGameObject(CityObject cityObject, 
                                              string buildingName)
    {
        if (cityObject == null)
        {
            Debug.LogWarning("CityObject is null");
            return null;
        }
        
        // ✅ CORRECT: Null チェック + コンポーネント検証
        GameObject buildingGO = new GameObject(buildingName);
        if (buildingGO == null)
        {
            Debug.LogWarning("Failed to create GameObject for building");
            return null;
        }
        
        // メッシュ生成
        Mesh buildingMesh = GenerateMeshFromCityObject(cityObject);
        if (buildingMesh == null)
        {
            Debug.LogWarning($"Failed to generate mesh for {buildingName}");
            Destroy(buildingGO);
            return null;
        }
        
        // メッシュコンポーネント追加
        MeshFilter meshFilter = buildingGO.AddComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogWarning("Failed to add MeshFilter component");
            Destroy(buildingGO);
            return null;
        }
        meshFilter.mesh = buildingMesh;
        
        // レンダリング
        MeshRenderer meshRenderer = buildingGO.AddComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogWarning("Failed to add MeshRenderer component");
            Destroy(buildingGO);
            return null;
        }
        
        // マテリアル設定
        AssignBuildingMaterial(meshRenderer, cityObject);
        
        // コライダー追加（物理演算・タッチ検出用）
        MeshCollider meshCollider = buildingGO.AddComponent<MeshCollider>();
        if (meshCollider == null)
        {
            Debug.LogWarning("Failed to add MeshCollider");
        }
        
        return buildingGO;
    }
    
    private Mesh GenerateMeshFromCityObject(CityObject cityObject)
    {
        // PLATEAU SDK の メッシュ生成 API
        // 実装は PLATEAU SDK のドキュメント参照
        return null; // プレースホルダー
    }
    
    private void AssignBuildingMaterial(MeshRenderer renderer, 
                                       CityObject cityObject)
    {
        if (renderer == null || cityObject == null)
        {
            return;
        }
        
        // 建物種別に応じたマテリアル割り当て
        string buildingType = cityObject.ObjType;
        Material material = GetMaterialForBuildingType(buildingType);
        
        if (material != null)
        {
            renderer.material = material;
        }
    }
    
    private Material GetMaterialForBuildingType(string buildingType)
    {
        return buildingType switch
        {
            "Building" => Resources.Load<Material>("Materials/Residential"),
            "Building_Residential" => Resources.Load<Material>("Materials/Residential"),
            "Building_Commercial" => Resources.Load<Material>("Materials/Commercial"),
            "Building_Industrial" => Resources.Load<Material>("Materials/Industrial"),
            _ => Resources.Load<Material>("Materials/Default")
        };
    }
}
```

## パフォーマンス最適化

### メモリ効率化

大規模な CityGML データは数 GB に達する場合があります。以下のパターンでメモリを効率化してください。

```csharp
public class PlateauMemoryOptimizer : MonoBehaviour
{
    // 一度に処理する建物数の上限
    private const int BUILDINGS_PER_BATCH = 100;
    
    public void LoadBuildingsProgressively(List<CityObject> buildings)
    {
        if (buildings == null || buildings.Count == 0)
        {
            Debug.LogWarning("Buildings list is empty");
            return;
        }
        
        StartCoroutine(LoadBuildingsBatch(buildings));
    }
    
    private System.Collections.IEnumerator LoadBuildingsBatch(
        List<CityObject> buildings)
    {
        for (int i = 0; i < buildings.Count; i += BUILDINGS_PER_BATCH)
        {
            int endIndex = System.Math.Min(i + BUILDINGS_PER_BATCH, buildings.Count);
            
            for (int j = i; j < endIndex; j++)
            {
                CityObject building = buildings[j];
                
                // 建物メッシュ生成（UI ブロッキングなし）
                GameObject buildingGO = CreateBuildingObject(building);
                if (buildingGO == null)
                {
                    Debug.LogWarning($"Failed to create building at index {j}");
                    continue;
                }
            }
            
            // フレームスキップで UI レスポンス保持
            yield return null;
        }
        
        Debug.Log($"Successfully loaded {buildings.Count} buildings");
    }
    
    private GameObject CreateBuildingObject(CityObject building)
    {
        // 実装は BuildingMeshGenerator.CreateBuildingGameObject() 参照
        return null; // プレースホルダー
    }
}
```

### 描画最適化（Culling）

遠くの建物は描画をスキップしてパフォーマンスを向上させます。

```csharp
public class BuildingCullManager : MonoBehaviour
{
    private const float RENDER_DISTANCE = 500f; // メートル
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera not found");
            enabled = false;
            return;
        }
    }
    
    public void UpdateBuildingCulling(List<GameObject> buildings)
    {
        if (buildings == null || mainCamera == null)
        {
            return;
        }
        
        Vector3 cameraPos = mainCamera.transform.position;
        
        foreach (GameObject building in buildings)
        {
            if (building == null)
            {
                continue;
            }
            
            float distance = Vector3.Distance(building.transform.position, cameraPos);
            
            // 遠すぎる建物は描画をスキップ
            MeshRenderer renderer = building.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = (distance < RENDER_DISTANCE);
            }
        }
    }
}
```

## CityGML データの属性アクセス

### 建物属性の取得

```csharp
public class CityObjectAttributeReader : MonoBehaviour
{
    public void PrintBuildingAttributes(CityObject cityObject)
    {
        if (cityObject == null)
        {
            Debug.LogWarning("CityObject is null");
            return;
        }
        
        // オブジェクト型
        Debug.Log($"Object Type: {cityObject.ObjType}");
        
        // 座標情報
        if (cityObject.TargetGeometry != null)
        {
            Debug.Log($"Geometry Type: {cityObject.TargetGeometry.GeometryType}");
        }
        
        // カスタム属性（建物の場合）
        if (cityObject.AttributesDictionary != null)
        {
            foreach (var attr in cityObject.AttributesDictionary)
            {
                Debug.Log($"{attr.Key}: {attr.Value}");
            }
        }
    }
    
    /// <summary>
    /// 建物の用途カテゴリを取得
    /// </summary>
    public string GetBuildingUsage(CityObject building)
    {
        if (building == null)
        {
            return "Unknown";
        }
        
        if (building.AttributesDictionary == null)
        {
            return "Unknown";
        }
        
        if (building.AttributesDictionary.TryGetValue("用途", out var usage))
        {
            return usage.ToString();
        }
        
        return "Unknown";
    }
}
```

## エラーハンドリング

### 一般的な例外処理パターン

```csharp
public class PlateauErrorHandler : MonoBehaviour
{
    public bool TryLoadPlateauData(string datasetPath, 
                                  out PlateauCityModel cityModel)
    {
        cityModel = null;
        
        // ステップ 1: パス検証
        if (string.IsNullOrEmpty(datasetPath))
        {
            Debug.LogError("Dataset path is null or empty");
            return false;
        }
        
        if (!System.IO.File.Exists(datasetPath))
        {
            Debug.LogError($"Dataset file not found: {datasetPath}");
            return false;
        }
        
        // ステップ 2: ロード実行
        try
        {
            cityModel = new PlateauCityModel();
            cityModel.LoadFromFile(datasetPath);
            
            if (cityModel == null)
            {
                Debug.LogError("cityModel is null after loading");
                return false;
            }
            
            return true;
        }
        catch (System.IO.IOException ioException)
        {
            Debug.LogError($"IO Exception: {ioException.Message}");
            return false;
        }
        catch (System.InvalidOperationException opException)
        {
            Debug.LogError($"Invalid Operation: {opException.Message}");
            return false;
        }
        catch (System.Exception generalException)
        {
            Debug.LogError($"Unexpected Exception: {generalException.GetType().Name} - {generalException.Message}");
            return false;
        }
    }
}
```

## 座標系の確認チェックリスト

PLATEAU データ処理時の座標系チェック：

- [ ] 参照点（基準座標）が明示的に定義されているか
- [ ] GeoCoordinate → Unity Vector3 変換が正確か
- [ ] 緯度経度の単位が度（degree）であることを確認
- [ ] 高度データが正しく Z 軸に割り当てられているか
- [ ] 地形レンダリングで Y 軸が上向きになっているか
- [ ] カメラが正しい向きで配置されているか

## Recovery Phase での PLATEAU 統合

Recovery フェーズで PLATEAU SDK コードを扱う際：

1. **既存座標変換ロジックを保持**
   - 参照点（Reference Point）は変更しない
   - テスト済みの計算式を破棄しない

2. **Null チェックの強化**
   ```csharp
   if (cityObject == null || cityObject.TargetGeometry == null)
   {
       Debug.LogWarning("Invalid city object");
       return;
   }
   ```

3. **メモリリーク防止**
   ```csharp
   if (oldMesh != null)
   {
       Resources.UnloadAsset(oldMesh);
   }
   ```

## パフォーマンスプロファイリング

```csharp
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class PlateauPerformanceMonitor : MonoBehaviour
{
    public void MonitorDataLoading(System.Action loadAction)
    {
        if (loadAction == null)
        {
            Debug.LogWarning("loadAction is null");
            return;
        }
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        loadAction.Invoke();
        
        stopwatch.Stop();
        Debug.Log($"Data loading took {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

## Pre-Commit Checklist

PLATEAU SDK 関連コード提案前：

- [ ] 座標変換が正確で、参照点が明記されているか
- [ ] すべての CityObject 参照に null チェックがあるか
- [ ] メッシュ生成後の MeshFilter/MeshRenderer 設定が完全か
- [ ] 大規模データは進行的ロード（Progressive Loading）対応か
- [ ] エラーハンドリングが包括的か
- [ ] メモリリークがないか（Resources.UnloadAsset 等）
- [ ] デバッグログが有用か

## 関連リソース

- **PLATEAU SDK ドキュメント**: https://github.com/Project-PLATEAU
- **CityGML 仕様**: https://www.ogc.org/standards/citygml
- **座標系参考**: https://www.gsi.go.jp/

---

**Last Updated**: 2026-01-20  
**Project**: OnoCoro (Unity 6.3 Geospatial Visualization)
