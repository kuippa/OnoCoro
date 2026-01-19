using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CommonsUtility;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

/// <summary>
/// Bloom Path システム（NavMesh ベースの動的パス表示）を管理
/// </summary>
public static class BloomPathCtrl
{
    internal static Dictionary<string, Vector3> _pathMakerDict = new Dictionary<string, Vector3>();
    internal static Dictionary<string, Vector3> _allPathMakerDict = new Dictionary<string, Vector3>();
    private static GameObject _parentHolder = null;
    
    private const string _PARENT_HOLDER_NAME = "nav_agents";
    private const float _NAVMESH_SEARCH_DISTANCE = 50f;
    private const int _AVOIDANCE_PRIORITY = 99;
    private const int _MAX_PATH_WAIT_FRAMES = 100;

    private static GameObject GetBloomPrefab()
    {
        return PrefabManager.PathBloomPrefab;
    }

    private static void AddPathMakerDictEntry(string markerName, Dictionary<string, Vector3> pathMakerDict)
    {
        if (pathMakerDict.ContainsKey(markerName))
        {
            return;
        }

        if (_allPathMakerDict.ContainsKey(markerName))
        {
            pathMakerDict.Add(markerName, _allPathMakerDict[markerName]);
            return;
        }

        GameObject markerObject = GameObject.Find(markerName);
        if (markerObject == null)
        {
            return;
        }

        Vector3 position = markerObject.transform.position;
        pathMakerDict.Add(markerName, position);
        PathMakerCtrl._pathMakerDict.Add(markerName, position);
    }

    private static Dictionary<string, Vector3> BuildPathMakerDict(string[] markerNames)
    {
        _pathMakerDict.Clear();
        for (int i = 0; i < markerNames.Length; i++)
        {
            string markerName = markerNames[i].Trim();
            AddPathMakerDictEntry(markerName, _pathMakerDict);
        }
        return _pathMakerDict;
    }

    private static GameObject AddNavAgent(Vector3 position)
    {
        GameObject navAgent = new GameObject();
        navAgent.layer = LayerMask.NameToLayer(GameEnum.LayerType.AreaIgnoreRaycast.ToString());
        navAgent.name = $"NavAgent_{position.x}_{position.y}_{position.z}";
        navAgent.transform.position = position;
        navAgent.transform.rotation = Quaternion.identity;
        Transform parentTransform = GameObjectTreat.GetHolderParentTransform(ref _parentHolder, _PARENT_HOLDER_NAME);
        navAgent.transform.SetParent(parentTransform);
        
        return navAgent;
    }

    private static NavMeshAgent AddNavMeshAgent(GameObject navAgentObject)
    {
        Vector3 sampledPosition = SampleNavMeshPosition(navAgentObject.transform.position);
        if (sampledPosition == Vector3.zero)
        {
            // Debug.LogError($"Failed to sample NavMesh for {navAgentObject.name}");
            return null;
        }
        navAgentObject.transform.position = sampledPosition;
        NavMeshAgent agent = navAgentObject.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = navAgentObject.AddComponent<NavMeshAgent>();
        }
        if (agent == null)
        {
            return null;
        }
        agent.Warp(sampledPosition);
        if (agent.isOnNavMesh == false)
        {
            return null;
        }
        return agent;
    }

    private static Vector3 SampleNavMeshPosition(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out var hit, _NAVMESH_SEARCH_DISTANCE, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector3.zero;
    }

    internal static void KickCoroutine(int pathIndex, string[] markerNames)
    {
        if (pathIndex >= _pathMakerDict.Count - 1)
        {
            return;
        }

        string currentMarkerName = markerNames[pathIndex].Trim();
        if (!_pathMakerDict.TryGetValue(currentMarkerName, out Vector3 startPosition))
        {
            return;
        }

        NavMeshAgent navAgent = AddNavMeshAgent(AddNavAgent(startPosition));
        if (navAgent == null)
        {
            // Debug.Log($" Failed to add NavMeshAgent for marker: {currentMarkerName}");
            return;
        }

        SetNavAgentDestination(navAgent, markerNames, pathIndex);
        navAgent.avoidancePriority = _AVOIDANCE_PRIORITY;
        string pathName = BuildPathName(markerNames, pathIndex);
        GameObject bloomPrefab = GetBloomPrefab();
        CoroutineRunner.Instance.StartCoroutine(PlaceMarkersAfterPathCalculation(navAgent, bloomPrefab, pathName));
    }

    private static void SetNavAgentDestination(NavMeshAgent navAgent, string[] markerNames, int pathIndex)
    {
        if (!navAgent.isOnNavMesh)
        {
            return;
        }

        string nextMarkerName = markerNames[pathIndex + 1].Trim();
        if (!_pathMakerDict.TryGetValue(nextMarkerName, out Vector3 destination))
        {
            return;
        }

        navAgent.SetDestination(destination);
    }

    private static string BuildPathName(string[] markerNames, int pathIndex)
    {
        string currentName = markerNames[pathIndex].Trim();
        string nextName = (pathIndex + 1 < markerNames.Length) ? markerNames[pathIndex + 1].Trim() : string.Empty;
        return $"{currentName}-{nextName}";
    }

    internal static void EventOffBloomPath(string eventValue)
    {
        string[] markerNames = eventValue.Split(',');
        
        if (markerNames.Length == 0 || markerNames[0].Trim() == GameEnum.PathMarkerNameParts.ALL)
        {
            ClearAllBloomPaths();
            return;
        }

        _allPathMakerDict = PathMakerCtrl.GetPathMakerDict();
        for (int i = 0; i < markerNames.Length; i++)
        {
            string markerName = markerNames[i].Trim();
            if (_allPathMakerDict.ContainsKey(markerName))
            {
                string pathName = BuildPathName(markerNames, i);
                PathMakerCtrl.DeletePathMarker(pathName);
            }
        }
    }

    private static void ClearAllBloomPaths()
    {
        _pathMakerDict.Clear();
        _allPathMakerDict.Clear();
        PathMakerCtrl.ResetPathMakerDict();
        PathMakerCtrl.DeleteAllPathMarkers();
    }

    internal static void EventBloomPath(string eventValue)
    {
        string[] markerNames = eventValue.Split(',');
        _allPathMakerDict = PathMakerCtrl.GetPathMakerDict();
        BuildPathMakerDict(markerNames);
        PathMakerCtrl.GetOrAddParentHolder();

        for (int i = 0; i < markerNames.Length; i++)
        {
            KickCoroutine(i, markerNames);
        }
    }

    private static IEnumerator PlaceMarkersAfterPathCalculation(NavMeshAgent agent, GameObject markerPrefab, string pathName)
    {
        if (agent == null)
        {
            yield break;
        }

        yield return WaitForPathCalculationCoroutine(agent, pathName);

        if (!agent.isOnNavMesh)
        {
            // Debug.LogWarning($"Agent is not on NavMesh for path {pathName}");
            yield break;
        }

        agent.isStopped = true;
        PathMakerCtrl.PlacePathMarkers(agent, markerPrefab, pathName);
    }

    private static IEnumerator WaitForPathCalculationCoroutine(NavMeshAgent agent, string pathName)
    {
        int waitCount = 0;
        while (agent.pathPending && waitCount < _MAX_PATH_WAIT_FRAMES)
        {
            yield return null;
            waitCount++;
        }

        if (waitCount >= _MAX_PATH_WAIT_FRAMES)
        {
            yield break;
        }
    }
}
