using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildingBreak : MonoBehaviour
{
    private List<GameObject> _buildingGameObject = new List<GameObject>();


    internal void EventBreakBuilding(string event_value)
    {
        if (_buildingGameObject.Count == 0)
        {
            return;
        }
        GameObject _plateauInfo = null;
        _plateauInfo = GameObject.Find("Plateau");
        PlateauInfo plateauInfo = _plateauInfo.GetComponent<PlateauInfo>();
        if (_plateauInfo == null || plateauInfo == null)
        {
            // Debug.Log("PlateauInfo is null");
            return;
        }

        if (event_value == "all")
        {
            foreach (var obj in _buildingGameObject)
            {
                if (obj == null || obj.activeSelf == false)
                {
                    continue;
                }
                plateauInfo.SetMaterialToDoom(obj);
            }
            return;
        }
        else
        {
            // event_value が 数字なら、その数の建物を破壊する
            if (!Int32.TryParse(event_value, out int n))
            {
                Debug.Log("event_value is not number");
                return;
            }
            int i = 0;
            foreach (var obj in _buildingGameObject)
            {
                if (i > n)
                {
                    return;
                } 
                plateauInfo.SetMaterialToDoom(obj);
                i++;
            }
            return;
        }

        // Debug.Log(_buildingGameObject[0].name);
        // _plateauInfo.GetComponent<PlateauInfo>().SetMaterialToDoom(_buildingGameObject[0]);        

    }



    void Awake()
    {
        // .gml 以下、LOD2 以上
        // マップ上に存在するオブジェクトから名前にbldg_が含まれるものを取得
        var findAllObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in findAllObjects)
        {
            if (obj.transform.parent != null)
            {
                if (obj.transform.parent.name.Contains("LOD2") && obj.name.Contains("bldg_"))
                {
                    _buildingGameObject.Add(obj);
                    // Debug.Log(obj.name);
                }
            }
        }

    }

    void Update()
    {
        // if (_is_earthquake)
        // {
        //     _time += Time.deltaTime;
        //     _time_duration += Time.deltaTime;
        //     if (_time > _interval)
        //     {
        //         _time = 0.0f;
        //         QualeP();
        //         // QuakeS();
        //     }
        // }
    }
}
