// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// BloomPathCtrl
using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using UnityEngine.AI;

public static class BloomPathCtrl
{
	internal static Dictionary<string, Vector3> _pathMakerDict = new Dictionary<string, Vector3>();

	internal static Dictionary<string, Vector3> _all_pathMakerDict = new Dictionary<string, Vector3>();

	private static GameObject _parent_holder = null;

	private const string _PARENT_HOLDER_NAME = "nav_agents";

	private static GameObject GetBloomPrefab()
	{
		return PrefabManager.PathBloomPrefab;
	}

	private static Dictionary<string, Vector3> AddPathMakerDict(string[] values)
	{
		_pathMakerDict.Clear();
		for (int i = 0; i < values.Length; i++)
		{
			string text = values[i].Trim();
			if (_pathMakerDict.ContainsKey(text))
			{
				continue;
			}
			if (_all_pathMakerDict.ContainsKey(text))
			{
				_pathMakerDict.Add(text, _all_pathMakerDict[text]);
				continue;
			}
			GameObject gameObject = GameObject.Find(text);
			if (gameObject != null)
			{
				Vector3 position = gameObject.transform.position;
				_pathMakerDict.Add(text, position);
				PathMakerCtrl._pathMakerDict.Add(text, position);
			}
			else
			{
				Debug.LogWarning("SetPathMakerDict: " + text + "は存在しません。");
			}
		}
		return _pathMakerDict;
	}

	private static GameObject AddNavAgent(Vector3 setPosition)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.transform.position = setPosition;
		gameObject.layer = LayerMask.NameToLayer(GameEnum.LayerType.AreaIgnoreRaycast.ToString());
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.enabled = false;
		}
		gameObject.name = "NavAgent_" + setPosition.x + "_" + setPosition.y + "_" + setPosition.z;
		BoxCollider component2 = gameObject.GetComponent<BoxCollider>();
		if (component2 != null)
		{
			component2.isTrigger = true;
			component2.enabled = false;
		}
		Transform holderParentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_holder, "nav_agents");
		gameObject.transform.SetParent(holderParentTransform);
		return gameObject;
	}

	private static NavMeshAgent AddNavMeshAgent(GameObject nav_agent)
	{
		NavMeshAgent navMeshAgent = nav_agent.AddComponent<NavMeshAgent>();
		if (NavMesh.SamplePosition(navMeshAgent.transform.position, out var hit, 10f, -1))
		{
			navMeshAgent.Warp(hit.position);
		}
		return navMeshAgent;
	}

	internal static void KickCoroutine(int idx, string[] values)
	{
		if (_pathMakerDict.Count > idx + 1 && _pathMakerDict.TryGetValue(values[idx].Trim(), out var value))
		{
			NavMeshAgent navMeshAgent = AddNavMeshAgent(AddNavAgent(value));
			if (_pathMakerDict.TryGetValue(values[idx + 1].Trim(), out value) && navMeshAgent.isOnNavMesh)
			{
				navMeshAgent.SetDestination(value);
			}
			if (navMeshAgent != null)
			{
				navMeshAgent.avoidancePriority = 99;
			}
			string pathName = GetPathName(values, idx);
			GameObject bloomPrefab = GetBloomPrefab();
			CoroutineRunner.Instance.StartCoroutine(PlaceMarkersAfterPathCalculation(navMeshAgent, bloomPrefab, pathName));
		}
	}

	private static string GetPathName(string[] values, int idx)
	{
		string text = values[idx].Trim() + "-";
		if (values.Length > idx + 1)
		{
			text += values[idx + 1].Trim();
		}
		return text;
	}

	internal static void EventOffBloomPath(string event_value)
	{
		string[] array = event_value.Split(',');
		if (array.Length == 0 || array[0].Trim() == "all")
		{
			_pathMakerDict.Clear();
			_all_pathMakerDict.Clear();
			PathMakerCtrl.ResetPathMakerDict();
			PathMakerCtrl.DeleteAllPathMarkers();
			return;
		}
		_all_pathMakerDict = PathMakerCtrl.GetPathMakerDict();
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (_all_pathMakerDict.ContainsKey(text))
			{
				PathMakerCtrl.DeletePathMarker(GetPathName(array, i));
			}
			else
			{
				Debug.LogWarning("EventOffBloomPath: " + text + "は存在しません。");
			}
		}
	}

	internal static void EventBloomPath(string event_value)
	{
		string[] array = event_value.Split(',');
		_all_pathMakerDict = PathMakerCtrl.GetPathMakerDict();
		_pathMakerDict = AddPathMakerDict(array);
		PathMakerCtrl.GetOrAddParentHolder();
		for (int i = 0; i < array.Length; i++)
		{
			KickCoroutine(i, array);
		}
	}

	private static IEnumerator PlaceMarkersAfterPathCalculation(NavMeshAgent agent, GameObject markerPrefab, string pathName)
	{
		while (agent.pathPending)
		{
			yield return null;
		}
		agent.isStopped = true;
		PathMakerCtrl.PlacePathMarkers(agent, markerPrefab, pathName);
	}
}
