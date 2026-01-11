using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateauBuildingInteractor : MonoBehaviour
{
    private Dictionary<string, Material[]> _buildingMaterials = new Dictionary<string, Material[]>();
    internal List<GameObject> _doomedBuildings = new List<GameObject>();    

    internal void DeleteBuilding(GameObject building)
    {
        RestoreBuildingMaterial(building);
        _doomedBuildings.Remove(building);
        Object.Destroy(building);
    }

    internal void SetBuildingToDoom(GameObject building)
    {
        StorageOriginalMaterial(building);
        if (!_doomedBuildings.Contains(building))
        {
            _doomedBuildings.Add(building);
        }
        ApplyDoomMaterial(building);
    }

    internal void RestoreBuildingMaterial(GameObject building)
    {
        SetMaterialToOriginal(building);
        _doomedBuildings.Remove(building);
        if (StageGoalCtrl.IsBuildingAllRepair())
        {
            if (_doomedBuildings.Count == 0)
            {
                Debug.Log("IsBuildingAllRepair");
                StageGoalCtrl.ActionStageGoal();
            }
            else
            {
                Debug.Log("NotBuildingAllRepair" + _doomedBuildings.Count);
            }
        }
    }

    internal bool IsBuildingDoomed(GameObject building)
    {
        return _doomedBuildings.Contains(building);
    }

    private void StorageOriginalMaterial(GameObject building)
    {
        if (!_buildingMaterials.ContainsKey(building.name))
        {
            Renderer renderer = building.GetComponent<Renderer>();
            if (renderer != null)
            {
                _buildingMaterials[building.name] = renderer.materials;
            }
        }
    }

    private void ApplyDoomMaterial(GameObject building)
    {
        StartCoroutine(ApplyDoomMaterialCoroutine(building));
    }

    private IEnumerator ApplyDoomMaterialCoroutine(GameObject building)
    {
        Material source = Resources.Load("Materials/PlateauGenericWood") as Material;
        Renderer component = building.GetComponent<Renderer>();
        building.GetComponentsInChildren<Renderer>();
        if (component != null)
        {
            Material[] array = new Material[component.materials.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new Material(source);
            }
            component.materials = array;
            yield return null;
        }
    }

    private void SetMaterialToOriginal(GameObject building)
    {
        if (_buildingMaterials.TryGetValue(building.name, out Material[] materials))
        {
            Renderer renderer = building.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.materials = materials;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (Material[] value in _buildingMaterials.Values)
        {
            for (int i = 0; i < value.Length; i++)
            {
                Object.Destroy(value[i]);
            }
        }
        _buildingMaterials.Clear();
    }
}
