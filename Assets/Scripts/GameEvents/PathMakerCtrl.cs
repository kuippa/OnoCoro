using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Path Marker システム（静的なマーカーの配置と管理）
/// </summary>
public static class PathMakerCtrl
{
    private static GameObject _parentMakerHolder = null;
    private static GameObject _parentBloomHolder = null;

    internal static Dictionary<string, Vector3> _pathMakerDict = new Dictionary<string, Vector3>();

    private const string _PARENT_MAKER_HOLDER_NAME = "path_markers";
    private const string _PARENT_BLOOM_HOLDER_NAME = "path_blooms";
    private const float _MARKER_Y_OFFSET = 0.06f;
    private const float _MARKER_BLOOM_Y_OFFSET = 0.12f;
    private const float _MARKER_INTERVAL = 1.5f;

    /// <summary>マーカー Y 座標オフセット定数を取得（BloomPath 等で使用）</summary>
    internal static float GetMarkerYOffset()
    {
        return _MARKER_Y_OFFSET;
    }

    internal static void ResetPathMakerDict()
    {
        _pathMakerDict.Clear();
    }

    internal static Dictionary<string, Vector3> GetPathMakerDict()
    {
        return _pathMakerDict;
    }

    internal static void CreateGameObjectByPathMakerDict()
    {
        Transform parentTransform = GameObjectTreat.GetHolderParentTransform(ref _parentMakerHolder, _PARENT_MAKER_HOLDER_NAME);
        
        foreach (KeyValuePair<string, Vector3> markerEntry in _pathMakerDict)
        {
            CreateOrUpdatePathMarker(markerEntry.Key, markerEntry.Value, parentTransform);
        }
    }

    private static void CreateOrUpdatePathMarker(string markerName, Vector3 basePosition, Transform parentTransform)
    {
        Vector3 adjustedPosition = AdjustMarkerPosition(basePosition);
        GameObject existingMarker = GameObject.Find(markerName);

        if (existingMarker == null)
        {
            CreateNewPathMarker(markerName, adjustedPosition, parentTransform);
        }
        else
        {
            UpdateExistingPathMarker(existingMarker, adjustedPosition, parentTransform);
        }
    }

    private static Vector3 AdjustMarkerPosition(Vector3 position)
    {
        position.y += _MARKER_Y_OFFSET;
        return position;
    }

    private static void CreateNewPathMarker(string markerName, Vector3 position, Transform parentTransform)
    {
        GameObject pathMakerPrefab = PrefabManager.PathMakerPrefab;
        GameObject newMarker = Object.Instantiate(pathMakerPrefab, position, Quaternion.identity);
        newMarker.name = markerName;
        ChangeMarkerColorByName(newMarker);
        newMarker.transform.SetParent(parentTransform);
    }

    private static void UpdateExistingPathMarker(GameObject marker, Vector3 position, Transform parentTransform)
    {
        marker.transform.position = position;
        ChangeMarkerColorByName(marker);
        marker.transform.SetParent(parentTransform);
    }

    private static void ChangeMarkerColorByName(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        GameObject markerChild = target.transform.Find("path_marker").gameObject;
        if (markerChild == null)
        {
            return;
        }

        Renderer renderer = markerChild.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = markerChild.AddComponent<MeshRenderer>();
        }

        Color markerColor = GetMarkerColorByName(target.name);
        renderer.material.color = markerColor;
    }

    private static Color GetMarkerColorByName(string markerName)
    {
        if (markerName.Contains(GameEnum.PathMarkerNameParts.START))
        {
            return Color.green;
        }

        if (markerName.Contains(GameEnum.PathMarkerNameParts.GOAL))
        {
            return Color.red;
        }

        return Color.blue;
    }

    internal static void DeletePathMarker(string pathName)
    {
        GameObjectTreat.DestroyAll(GameObject.Find(pathName));
    }

    internal static void DeleteAllPathMarkers()
    {
        GameObjectTreat.DestroyAll(_parentBloomHolder);
    }

    internal static void GetOrAddParentHolder()
    {
        GameObjectTreat.GetHolderParentTransform(ref _parentBloomHolder, _PARENT_BLOOM_HOLDER_NAME);
    }

    internal static void PlacePathMarkers(NavMeshAgent navMeshAgent, GameObject markerPrefab, string pathName)
    {
        if (!ValidatePathForMarkerPlacement(navMeshAgent))
        {
            return;
        }

        Transform parentTransform = SetupMarkerParent(pathName);
        Vector3[] pathCorners = navMeshAgent.path.corners;
        PlaceMarkersAlongPath(pathCorners, markerPrefab, parentTransform);
    }

    private static bool ValidatePathForMarkerPlacement(NavMeshAgent navMeshAgent)
    {
        if (navMeshAgent == null || navMeshAgent.path == null)
        {
            Debug.LogWarning("NavMeshAgent path is null");
            return false;
        }

        if (navMeshAgent.path.corners.Length < 2)
        {
            Debug.LogWarning("NavMeshAgent path has insufficient corners");
            return false;
        }

        return true;
    }

    private static Transform SetupMarkerParent(string pathName)
    {
        GameObject parentObject = null;
        Transform parentTransform = GameObjectTreat.GetHolderParentTransform(ref parentObject, pathName);
        
        if (_parentBloomHolder == null)
        {
            GetOrAddParentHolder();
        }
        parentTransform.SetParent(_parentBloomHolder.transform);
        
        return parentTransform;
    }

    private static void PlaceMarkersAlongPath(Vector3[] pathCorners, GameObject markerPrefab, Transform parentTransform)
    {
        for (int i = 0; i < pathCorners.Length - 1; i++)
        {
            PlaceMarkersOnSegment(pathCorners[i], pathCorners[i + 1], i, markerPrefab, parentTransform);
        }
    }

    private static void PlaceMarkersOnSegment(Vector3 startPoint, Vector3 endPoint, int segmentIndex, GameObject markerPrefab, Transform parentTransform)
    {
        int segmentCount = Mathf.FloorToInt(Vector3.Distance(startPoint, endPoint) / _MARKER_INTERVAL);

        for (int j = 0; j <= segmentCount; j++)
        {
            // if (ShouldSkipMarker(segmentIndex, j, segmentCount))
            // {
            //     continue;
            // }
            PlaceMarkerAtPosition(startPoint, endPoint, j, segmentCount, segmentIndex, markerPrefab, parentTransform);
        }
    }

    private static bool ShouldSkipMarker(int segmentIndex, int markerIndex, int segmentCount)
    {
        // スキップ条件：最初のセグメントの最初のマーカー、または最後のセグメントの最後のマーカー
        if (segmentIndex == 0 && markerIndex == 0)
        {
            return true;
        }

        return false;  // Note: Last segment endpoint is handled separately
    }

    private static void PlaceMarkerAtPosition(Vector3 startPoint, Vector3 endPoint, int markerIndex, int segmentCount, int segmentIndex, GameObject markerPrefab, Transform parentTransform)
    {
        float interpolation = (float)markerIndex / (float)segmentCount;
        Vector3 markerPosition = Vector3.Lerp(startPoint, endPoint, interpolation);
        markerPosition.y = _MARKER_BLOOM_Y_OFFSET;  // Bloom マーカーは別のオフセット値を使用

        GameObject marker = Object.Instantiate(markerPrefab, markerPosition, Quaternion.identity);
        marker.name = $"PathMarker_{segmentIndex}_{markerIndex}";
        marker.tag = GameEnum.TagType.PathBloom.ToString();
        marker.layer = LayerMask.NameToLayer(GameEnum.LayerType.AreaIgnoreRaycast.ToString());

        Quaternion rotation = Quaternion.LookRotation((endPoint - startPoint).normalized);
        marker.transform.rotation = rotation;
        marker.transform.SetParent(parentTransform);
    }
}
