using UnityEngine;
using System.Collections.Generic;
using PLATEAU.CityInfo;
using PLATEAU.Native;

public class PlateauDataExtractor : MonoBehaviour
{
    private const float BLDG_HEIGHT_PER_FLOOR = 3.0f;
    private const float BLDG_FLOOR_PER_PERSON = 15.0f;
    private const float BLDG_CORRIDOR_PER_ROOM = 10.0f;

    private XMLparser _xmlParser;
    private Dictionary<string, Dictionary<string, string>> _dictBuilding = new Dictionary<string, Dictionary<string, string>>();    // 建物情報のキャッシュ

    private void Awake()
    {
    }

    internal Dictionary<string, string> TryGetBuildingInfo(GameObject building)
    {
        // string buildingId = building.GetInstanceID().ToString();
        Dictionary<string, string> dictInfo = new Dictionary<string, string>();
        if (_dictBuilding.TryGetValue(building.name, out dictInfo))
        {
            // Get from cache
            // Debug.Log("dictInfo is already exist");
            // SetMaterialToOriginal(gameObject); 
        }
        else
        {
            // Debug.Log("dictInfo is not exist");
            dictInfo = ExtractBuildingInfo(building);
            _dictBuilding.Add(building.name, dictInfo);
            
            // メッシュの位置を確認するためにマーカーを表示（デバッグ機能）
            // 右クリックでマーカーを表示
            // Debug.Log("DispCubeMarker BuildingInfo: " + dictInfo);
            // PlateauCubeMaker plateauCubeMaker = gameObject.GetComponent<PlateauCubeMaker>();
            // if (plateauCubeMaker == null)
            // {
            //     plateauCubeMaker = gameObject.AddComponent<PlateauCubeMaker>();
            // }
            // plateauCubeMaker.DispCubeMarker(building, dictInfo);

        }
        return dictInfo;
    }

    private Dictionary<string, string> ExtractBuildingInfo(GameObject building)
    {
        Dictionary<string, string> buildingInfo = new Dictionary<string, string>();
        PLATEAUCityObjectGroup cityObjs = building.GetComponent<PLATEAUCityObjectGroup>();
        if (cityObjs == null)
        {
            // Debug.LogWarning("No PLATEAUCityObjectGroup found on the building.");
            return buildingInfo;
        }

        foreach (var cityObj in cityObjs.PrimaryCityObjects)
        {
            buildingInfo = ExtractAttributesFromCityObject(cityObj, buildingInfo);
        }
        buildingInfo = CalculateAdditionalInfo(buildingInfo, building);

        // Debug.Log("BuildingInfo: " + buildingInfo);
        return buildingInfo;
    }

    private Dictionary<string, string> ExtractAttributesFromCityObject(PLATEAU.CityInfo.CityObjectList.CityObject cityObj, Dictionary<string, string> buildingInfo)
    {
        var attrMap = cityObj.AttributesMap;
        foreach (var pair in attrMap)
        {
            buildingInfo[pair.Key] = pair.Value.StringValue;

            if (pair.Key == "bldg:usage")
            {
                string buildingUsage = GetBuildingUsage(pair.Value.StringValue);
                buildingInfo["bldg:usagestr"] = buildingUsage;
                // Debug.Log("BuildingUsage: " + buildingUsage);
            }

            foreach (var pair2 in pair.Value.AttributesMapValue)
            {
                buildingInfo[pair2.Key] = pair2.Value.StringValue;
            }
        }
        return buildingInfo;
    }

    private Dictionary<string, string> CalculateAdditionalInfo(Dictionary<string, string> buildingInfo, GameObject building)
    {
        if (buildingInfo.TryGetValue("bldg:measuredheight", out string heightStr))
        {
            float height = float.Parse(heightStr);
            int floors = Mathf.FloorToInt(height / BLDG_HEIGHT_PER_FLOOR);
            buildingInfo["bldg:floors"] = floors.ToString();

            float floorSpace = GetSupFloorSpace(building);
            buildingInfo["bldg:floor"] = floorSpace.ToString("F2");

            int totalArea = Mathf.FloorToInt(floorSpace * floors);
            buildingInfo["bldg:totalarea"] = totalArea.ToString();

            int estimatedPersons = CalcPersonCapacity(totalArea);
            buildingInfo["bldg:estimatedpersons"] = estimatedPersons.ToString();

            buildingInfo["bldg:rebuildcost"] = CalcRebuildCost(buildingInfo).ToString();
            buildingInfo["bldg:rebuildbouns"] = CalcRebuildBonus(buildingInfo).ToString();
        }
        return buildingInfo;
    }

    private string GetBuildingUsage(string usageCode)
    {
        string ret = "";
        _xmlParser = this.GetComponent<XMLparser>();
        if (_xmlParser == null)
        {
            _xmlParser = this.gameObject.AddComponent<XMLparser>();
        }
        ret = _xmlParser.GetBuildingUsage(usageCode);
        return ret;
    }

    private float GetSupFloorSpace(GameObject building)
    {
        Renderer renderer = building.GetComponent<Renderer>();
        if (renderer != null)
        {
            Vector3 size = renderer.bounds.size;
            return size.x * size.z;
        }
        return 0f;
    }

    private int CalcPersonCapacity(int totalArea)
    {
        float availableArea = totalArea - BLDG_CORRIDOR_PER_ROOM;
        int estimatedPersons = Mathf.FloorToInt(availableArea / BLDG_FLOOR_PER_PERSON);
        return Mathf.Max(0, estimatedPersons);
    }

    internal float CalcRebuildCost(Dictionary<string, string> buildingInfo)
    {
        if (buildingInfo.TryGetValue("bldg:measuredheight", out string heightStr) &&
            buildingInfo.TryGetValue("bldg:floor", out string floorStr))
        {
            float height = float.Parse(heightStr);
            float floor = float.Parse(floorStr);
            float cost = (height * floor / 10.0f);
            return Mathf.Max(1, Mathf.Floor(cost));
        }
        return 0f;
    }

    internal int CalcRebuildBonus(Dictionary<string, string> buildingInfo)
    {
        if (buildingInfo.TryGetValue("bldg:estimatedpersons", out string estimatedPersons))
        {
            int persons = int.Parse(estimatedPersons);
            return Mathf.Max(1, Mathf.FloorToInt(persons * 10));
        }
        return 0;
    }
}

