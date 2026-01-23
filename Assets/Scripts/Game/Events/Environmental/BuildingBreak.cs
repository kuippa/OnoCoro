using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingBreak : MonoBehaviour
{
    private List<GameObject> _buildingGameObject = new List<GameObject>();

    internal void EventBreakBuilding(string event_value)
    {
        if (_buildingGameObject.Count == 0)
        {
            return;
        }
        GameObject obj = GameObject.Find("Plateau");
        PlateauInfoManager component = obj.GetComponent<PlateauInfoManager>();
        if (obj == null || component == null)
        {
            return;
        }
        if (event_value == "all")
        {
            foreach (GameObject item in _buildingGameObject)
            {
                if (!(item == null) && item.activeSelf)
                {
                    component.SetBuildingToDoom(item);
                }
            }
            return;
        }
        if (!int.TryParse(event_value, out var result))
        {
            Debug.Log("event_value is not number");
            return;
        }
        int num = 0;
        foreach (GameObject item2 in _buildingGameObject)
        {
            if (num >= result)
            {
                break;
            }
            component.SetBuildingToDoom(item2);
            num++;
        }
    }

    private void Awake()
    {
        GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootGameObjects.Length; i++)
        {
            foreach (GameObject item in from t in rootGameObjects[i].GetComponentsInChildren<Transform>(includeInactive: false)
                where t.gameObject.name.Contains("bldg_")
                select t.gameObject)
            {
                Collider component = item.GetComponent<Collider>();
                if (!(component == null) && PlateauUtility.IsPlateauBuilding(component))
                {
                    _buildingGameObject.Add(item);
                }
            }
        }
    }

    private void Update()
    {
    }
}
