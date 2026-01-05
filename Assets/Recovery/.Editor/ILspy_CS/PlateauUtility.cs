// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PlateauUtility
using PLATEAU.CityInfo;
using UnityEngine;

public static class PlateauUtility
{
	internal static bool IsPlateauBuilding(Collider obj)
	{
		if (obj.transform.parent == null || !obj.name.Contains("bldg_") || !obj.gameObject.activeSelf)
		{
			return false;
		}
		PLATEAUCityObjectGroup component = obj.gameObject.GetComponent<PLATEAUCityObjectGroup>();
		if (component == null)
		{
			return false;
		}
		if (GetCityObjectType(component) != "COT_Building")
		{
			return false;
		}
		return true;
	}

	internal static string GetCityObjectType(PLATEAUCityObjectGroup cityObjs)
	{
		string result = "";
		foreach (CityObjectList.CityObject primaryCityObject in cityObjs.PrimaryCityObjects)
		{
			result = primaryCityObject.CityObjectType.ToString();
		}
		return result;
	}
}
