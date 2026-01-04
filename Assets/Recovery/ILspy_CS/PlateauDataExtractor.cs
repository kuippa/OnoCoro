// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PlateauDataExtractor
using System.Collections.Generic;
using PLATEAU.CityInfo;
using UnityEngine;

public class PlateauDataExtractor : MonoBehaviour
{
	private const float BLDG_HEIGHT_PER_FLOOR = 3f;

	private const float BLDG_FLOOR_PER_PERSON = 15f;

	private const float BLDG_CORRIDOR_PER_ROOM = 10f;

	private XMLparser _xmlParser;

	private Dictionary<string, Dictionary<string, string>> _dictBuilding = new Dictionary<string, Dictionary<string, string>>();

	private void Awake()
	{
	}

	internal Dictionary<string, string> TryGetBuildingInfo(GameObject building)
	{
		Dictionary<string, string> value = new Dictionary<string, string>();
		if (!_dictBuilding.TryGetValue(building.name, out value))
		{
			value = ExtractBuildingInfo(building);
			_dictBuilding.Add(building.name, value);
		}
		return value;
	}

	private Dictionary<string, string> ExtractBuildingInfo(GameObject building)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		PLATEAUCityObjectGroup component = building.GetComponent<PLATEAUCityObjectGroup>();
		if (component == null)
		{
			return dictionary;
		}
		foreach (CityObjectList.CityObject primaryCityObject in component.PrimaryCityObjects)
		{
			dictionary = ExtractAttributesFromCityObject(primaryCityObject, dictionary);
		}
		return CalculateAdditionalInfo(dictionary, building);
	}

	private Dictionary<string, string> ExtractAttributesFromCityObject(CityObjectList.CityObject cityObj, Dictionary<string, string> buildingInfo)
	{
		foreach (KeyValuePair<string, CityObjectList.Attributes.Value> item in cityObj.AttributesMap)
		{
			buildingInfo[item.Key] = item.Value.StringValue;
			if (item.Key == "bldg:usage")
			{
				string buildingUsage = GetBuildingUsage(item.Value.StringValue);
				buildingInfo["bldg:usagestr"] = buildingUsage;
			}
			foreach (KeyValuePair<string, CityObjectList.Attributes.Value> item2 in item.Value.AttributesMapValue)
			{
				buildingInfo[item2.Key] = item2.Value.StringValue;
			}
		}
		return buildingInfo;
	}

	private Dictionary<string, string> CalculateAdditionalInfo(Dictionary<string, string> buildingInfo, GameObject building)
	{
		if (buildingInfo.TryGetValue("bldg:measuredheight", out var value))
		{
			int num = Mathf.FloorToInt(float.Parse(value) / 3f);
			buildingInfo["bldg:floors"] = num.ToString();
			float supFloorSpace = GetSupFloorSpace(building);
			buildingInfo["bldg:floor"] = supFloorSpace.ToString("F2");
			int totalArea = Mathf.FloorToInt(supFloorSpace * (float)num);
			buildingInfo["bldg:totalarea"] = totalArea.ToString();
			buildingInfo["bldg:estimatedpersons"] = CalcPersonCapacity(totalArea).ToString();
			buildingInfo["bldg:rebuildcost"] = CalcRebuildCost(buildingInfo).ToString();
			buildingInfo["bldg:rebuildbouns"] = CalcRebuildBonus(buildingInfo).ToString();
		}
		return buildingInfo;
	}

	private string GetBuildingUsage(string usageCode)
	{
		_xmlParser = GetComponent<XMLparser>();
		if (_xmlParser == null)
		{
			_xmlParser = base.gameObject.AddComponent<XMLparser>();
		}
		return _xmlParser.GetBuildingUsage(usageCode);
	}

	private float GetSupFloorSpace(GameObject building)
	{
		Renderer component = building.GetComponent<Renderer>();
		if (component != null)
		{
			Vector3 size = component.bounds.size;
			return size.x * size.z;
		}
		return 0f;
	}

	private int CalcPersonCapacity(int totalArea)
	{
		int b = Mathf.FloorToInt(((float)totalArea - 10f) / 15f);
		return Mathf.Max(0, b);
	}

	internal float CalcRebuildCost(Dictionary<string, string> buildingInfo)
	{
		if (buildingInfo.TryGetValue("bldg:measuredheight", out var value) && buildingInfo.TryGetValue("bldg:floor", out var value2))
		{
			float num = float.Parse(value);
			float num2 = float.Parse(value2);
			float f = num * num2 / 10f;
			return Mathf.Max(1f, Mathf.Floor(f));
		}
		return 0f;
	}

	internal int CalcRebuildBonus(Dictionary<string, string> buildingInfo)
	{
		if (buildingInfo.TryGetValue("bldg:estimatedpersons", out var value))
		{
			int num = int.Parse(value);
			return Mathf.Max(1, Mathf.FloorToInt(num * 10));
		}
		return 0;
	}
}
