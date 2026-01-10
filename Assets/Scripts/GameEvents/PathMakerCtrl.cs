using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using UnityEngine.AI;

public static class PathMakerCtrl
{
    private static GameObject _parent_holder = null;
    private static GameObject _parent_maker_holder = null;

    internal static Dictionary<string, Vector3> _pathMakerDict = new Dictionary<string, Vector3>();

    private const string _PARENT_MAKER_HOLDER_NAME = "path_markers";
    private const string _PARENT_HOLDER_NAME = "path_blooms";
    private const float MARKER_INTERVAL = 1.5f;

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
        Transform holderParentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_maker_holder, _PARENT_MAKER_HOLDER_NAME);
        foreach (KeyValuePair<string, Vector3> item in _pathMakerDict)
        {
            GameObject existingObject = GameObject.Find(item.Key);
            if (existingObject == null)
            {
                GameObject pathMakerPrefab = PrefabManager.PathMakerPrefab;
                Vector3 position = item.Value;
                GameObject newMarker = Object.Instantiate(pathMakerPrefab, position, Quaternion.identity);
                newMarker.name = item.Key;
                ChangeMarkerColorByName(newMarker);
                newMarker.transform.SetParent(holderParentTransform);
            }
            else
            {
                existingObject.transform.position = item.Value;
                ChangeMarkerColorByName(existingObject);
                existingObject.transform.SetParent(holderParentTransform);
            }
        }
    }

    private static void ChangeMarkerColorByName(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        GameObject markerChild = target.transform.Find("path_marker").gameObject;
        Renderer renderer = markerChild.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = markerChild.AddComponent<MeshRenderer>();
        }

        if (target.name.Contains("start"))
        {
            renderer.material.color = Color.green;
        }
        else if (target.name.Contains("goal"))
        {
            renderer.material.color = Color.red;
        }
        else
        {
            renderer.material.color = Color.blue;
        }
    }

    internal static void DeletePathMarker(string pathName)
    {
        Debug.Log("DeletePathMarker: " + pathName);
        GameObjectTreat.DestroyAll(GameObject.Find(pathName));
    }

    internal static void DeleteAllPathMarkers()
    {
        GameObjectTreat.DestroyAll(_parent_holder);
    }

    internal static void GetOrAddParentHolder()
    {
        GameObjectTreat.GetHolderParentTransform(ref _parent_holder, _PARENT_HOLDER_NAME);
    }

    internal static void PlacePathMarkers(NavMeshAgent navMeshAgent, GameObject markerPrefab, string pathName)
    {
        if (navMeshAgent.path == null || navMeshAgent.path.corners.Length < 2)
        {
            Debug.LogWarning("NavMeshAgentの経路が無効です。");
            return;
        }

        GameObject parent = null;
        Transform holderParentTransform = GameObjectTreat.GetHolderParentTransform(ref parent, pathName);
        if (_parent_holder == null)
        {
            GetOrAddParentHolder();
        }
        holderParentTransform.SetParent(_parent_holder.transform);

        Vector3[] corners = navMeshAgent.path.corners;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 startPoint = corners[i];
            Vector3 endPoint = corners[i + 1];
            int segmentCount = Mathf.FloorToInt(Vector3.Distance(startPoint, endPoint) / MARKER_INTERVAL);

            for (int j = 0; j <= segmentCount; j++)
            {
                // 開始点と終了点は除外
                if ((i != 0 || j != 0) && (i != corners.Length - 2 || j != segmentCount))
                {
                    float t = (float)j / (float)segmentCount;
                    Vector3 position = Vector3.Lerp(startPoint, endPoint, t);
                    GameObject marker = Object.Instantiate(markerPrefab, position, Quaternion.identity);
                    marker.name = "PathMarker_" + i + "_" + j;
                    marker.tag = GameEnum.TagType.PathBloom.ToString();
                    marker.transform.SetParent(holderParentTransform);
                    marker.layer = LayerMask.NameToLayer(GameEnum.LayerType.AreaIgnoreRaycast.ToString());

                    Quaternion rotation = Quaternion.LookRotation((endPoint - startPoint).normalized);
                    marker.transform.rotation = rotation;
                }
            }
        }
    }
}
