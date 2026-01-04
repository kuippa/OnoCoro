// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PathMakerCtrl
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
		Transform holderParentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_maker_holder, "path_markers");
		foreach (KeyValuePair<string, Vector3> item in _pathMakerDict)
		{
			GameObject gameObject = GameObject.Find(item.Key);
			if (gameObject == null)
			{
				GameObject pathMakerPrefab = PrefabManager.PathMakerPrefab;
				Vector3 value = item.Value;
				GameObject gameObject2 = Object.Instantiate(pathMakerPrefab, value, Quaternion.identity);
				gameObject2.name = item.Key;
				ChangeMarkerColorByName(gameObject2);
				gameObject2.transform.SetParent(holderParentTransform);
			}
			else
			{
				gameObject.transform.position = item.Value;
				ChangeMarkerColorByName(gameObject);
				gameObject.transform.SetParent(holderParentTransform);
			}
		}
	}

	private static void ChangeMarkerColorByName(GameObject target)
	{
		if (!(target == null))
		{
			GameObject gameObject = target.transform.Find("path_marker").gameObject;
			Renderer renderer = gameObject.GetComponent<Renderer>();
			if (renderer == null)
			{
				renderer = gameObject.AddComponent<MeshRenderer>();
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
		GameObjectTreat.GetHolderParentTransform(ref _parent_holder, "path_blooms");
	}

	internal static void PlacePathMarkers(NavMeshAgent NavMeshAgent, GameObject markerPrefab, string pathName)
	{
		if (NavMeshAgent.path == null || NavMeshAgent.path.corners.Length < 2)
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
		Vector3[] corners = NavMeshAgent.path.corners;
		for (int i = 0; i < corners.Length - 1; i++)
		{
			Vector3 vector = corners[i];
			Vector3 vector2 = corners[i + 1];
			int num = Mathf.FloorToInt(Vector3.Distance(vector, vector2) / 1.5f);
			for (int j = 0; j <= num; j++)
			{
				if ((i != 0 || j != 0) && (i != corners.Length - 2 || j != num))
				{
					float t = (float)j / (float)num;
					Vector3 position = Vector3.Lerp(vector, vector2, t);
					GameObject gameObject = Object.Instantiate(markerPrefab, position, Quaternion.identity);
					gameObject.name = "PathMarker_" + i + "_" + j;
					gameObject.tag = GameEnum.TagType.PathBloom.ToString();
					gameObject.transform.SetParent(holderParentTransform);
					gameObject.layer = LayerMask.NameToLayer(GameEnum.LayerType.AreaIgnoreRaycast.ToString());
					Quaternion rotation = Quaternion.LookRotation((vector2 - vector).normalized);
					gameObject.transform.rotation = rotation;
				}
			}
		}
	}
}
