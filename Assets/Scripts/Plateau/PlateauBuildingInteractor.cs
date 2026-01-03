using UnityEngine;
using System.Collections.Generic;
using System;

public class PlateauBuildingInteractor : MonoBehaviour
{

    private Dictionary<string, Material[]> _buildingMaterials = new Dictionary<string, Material[]>();
    internal List<GameObject> _doomedBuildings = new List<GameObject>();
    // public event Action<GameObject> OnBuildingRebuilt;

    // ... 他のフィールドと定数は前回のコードと同じ ...

    // public void InitiateRebuildProcess(GameObject building, Action onComplete)
    // {
    //     CallCircularIndicator(building, () => {
    //         RestoreBuildingMaterial(building);
    //         // OnBuildingRebuilt?.Invoke(building);
    //         onComplete?.Invoke();
    //     });
    // }    

    internal void DeleteBuilding(GameObject building)
    {
        RestoreBuildingMaterial(building);
        _doomedBuildings.Remove(building);
        Destroy(building);
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
            // すべての建物が修復済みか確認
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
        Material doomMaterial = Resources.Load("Materials/PlateauGenericWood") as Material;
        Renderer renderer = building.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = doomMaterial;
            }
            renderer.materials = materials;
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

    // private void CallCircularIndicator(GameObject building, Action onComplete)
    // {
    //     // CircularIndicatorの実装はプロジェクトの仕様に応じて調整が必要です
    //     GameObject indicator = Instantiate(Resources.Load("Prefabs/UI/UICircularIndicator")) as GameObject;
    //     CircularIndicator circularIndicator = indicator.GetComponent<CircularIndicator>();
        
    //     GameObject indicatorObject = new GameObject("Indicator");
    //     indicatorObject.transform.SetParent(building.transform);
        
    //     Renderer renderer = building.GetComponent<Renderer>();
    //     Vector3 center = renderer.bounds.center;
    //     center.y += 10f; // 適切な高さに調整
    //     indicatorObject.transform.position = center;

    //     circularIndicator.StartIndicator(5f, () => {
    //         onComplete?.Invoke();
    //         Destroy(indicator);
    //         Destroy(indicatorObject);
    //     }, indicatorObject);
    // }

    // private void OnBuildingRebuilt(GameObject rebuiltBuilding)
    // {
    //     Dictionary<string, string> updatedInfo = PlateauDataExtractor.ExtractBuildingInfo(rebuiltBuilding);
    //     PlateauUIManager.Instance.UpdateBuildingInfo(updatedInfo);
    //     // 他のシステムで必要な更新をトリガー
    // }

    private void OnDestroy()
    {
        foreach (var materials in _buildingMaterials.Values)
        {
            foreach (var material in materials)
            {
                Destroy(material);
            }
        }
        _buildingMaterials.Clear();
    }
}
