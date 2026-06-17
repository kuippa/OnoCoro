using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public class PlateauBuildingInteractor : MonoBehaviour
{
    private Dictionary<string, Material[]> _buildingMaterials = new Dictionary<string, Material[]>();
    internal List<GameObject> _doomedBuildings = new List<GameObject>();
    private bool _isGoalAlreadyTriggered = false;  // ゴール判定が実行済みかチェック    

    internal void DeleteBuilding(GameObject building)
    {
        RestoreBuildingMaterial(building);
        _doomedBuildings.Remove(building);
        Object.Destroy(building);
    }

    internal void SetBuildingToDoom(GameObject building, bool isFire = false)
    {
        StorageOriginalMaterial(building);
        if (!_doomedBuildings.Contains(building))
        {
            _doomedBuildings.Add(building);
        }
        ApplyDoomMaterial(building, isFire);
    }

    internal void RestoreBuildingMaterial(GameObject building)
    {
        if (building == null)
        {
            return;
        }
        
        SetMaterialToOriginal(building);
        _doomedBuildings.Remove(building);
        
        if (StageGoalController.IsBuildingAllRepair())
        {
            if (_doomedBuildings.Count == 0)
            {
                // 既にゴール判定が実行済みなら、二重呼び出しを防ぐ
                if (_isGoalAlreadyTriggered)
                {
                    return;
                }
                
                _isGoalAlreadyTriggered = true;
                StageGoalController.ActionStageGoal();
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

    // 火災で倒壊した建物の焦げ色（地震倒壊の木材色と区別する・教育表示）
    private static readonly Color _FIRE_DOOM_COLOR = new Color(0.18f, 0.10f, 0.08f);
    private static Material _fireDoomMaterial = null;

    /// <summary>
    /// 火災倒壊用の焦げ色マテリアル（テクスチャ無しのソリッド）。
    /// 地震倒壊の木材テクスチャ・マテリアルは色変更では見た目が変わらないため、
    /// 火災倒壊は専用マテリアルに丸ごと差し替えて区別する
    /// </summary>
    private static Material GetFireDoomMaterial()
    {
        if (_fireDoomMaterial == null)
        {
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            _fireDoomMaterial = new Material(shader);
            if (_fireDoomMaterial.HasProperty("_BaseColor"))
            {
                _fireDoomMaterial.SetColor("_BaseColor", _FIRE_DOOM_COLOR);
            }
            _fireDoomMaterial.color = _FIRE_DOOM_COLOR;
        }
        return _fireDoomMaterial;
    }

    private void ApplyDoomMaterial(GameObject building, bool isFire = false)
    {
        StartCoroutine(ApplyDoomMaterialCoroutine(building, isFire));
    }

    private IEnumerator ApplyDoomMaterialCoroutine(GameObject building, bool isFire)
    {
        // 火災倒壊は焦げ色の専用マテリアル、地震倒壊は従来の木材マテリアル
        Material source = isFire ? GetFireDoomMaterial() : MaterialManager.PlateauGenericWood;
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
