// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// SignPowerOutageCtrl
using CommonsUtility;
using UnityEngine;

public static class SignPowerOutageCtrl
{
	private static GameObject _prefab;

	internal static void UnSignPowerOutage(GameObject target)
	{
		Transform transform = target.transform.Find("SignPowerOutage");
		if (transform != null)
		{
			GameObjectTreat.DestroyAll(transform.gameObject);
		}
	}

	internal static void GetOrCreateCirclePowerOutage(GameObject target)
	{
		if (target.transform.Find("SignPowerOutage") == null)
		{
			SignPowerOutage(target);
		}
	}

	private static void SignPowerOutage(GameObject target)
	{
		GameObject gameObject = Object.Instantiate(GetPrefab());
		gameObject.name = "SignPowerOutage";
		gameObject.transform.SetParent(target.transform);
		gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	private static GameObject GetPrefab()
	{
		if (_prefab == null)
		{
			_prefab = Resources.Load<GameObject>("Prefabs/Marker/SignPowerOutage");
		}
		return _prefab;
	}
}
