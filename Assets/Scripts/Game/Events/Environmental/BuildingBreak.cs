using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingBreak : MonoBehaviour
{
    private List<GameObject> _buildingGameObject = new List<GameObject>();

    /// <summary>
    /// building_break の対象建物リストを取得（Season 3 W2: ヒートマップの倒壊予測に使用）
    /// EventBreakBuilding と同じ順序（先頭 N 棟が倒壊対象になる）
    /// </summary>
    internal List<GameObject> GetBreakTargets()
    {
        return _buildingGameObject;
    }

    internal void EventBreakBuilding(string event_value)
    {
        if (_buildingGameObject.Count == 0)
        {
            Debug.LogWarning("[BuildingBreak] 対象建物が 0 棟のため building_break をスキップ（シーンに bldg_ オブジェクトが無い可能性）");
            return;
        }
        GameObject obj = GameObject.Find("Plateau");
        PlateauInfoManager component = obj.GetComponent<PlateauInfoManager>();
        if (obj == null || component == null)
        {
            Debug.LogWarning("[BuildingBreak] Plateau / PlateauInfoManager が見つからないため building_break をスキップ");
            return;
        }

        // 倒壊判定用（被害集計で「実際に新規倒壊させた棟数」を数えるため）
        PlateauBuildingInteractor interactor = obj.GetComponent<PlateauBuildingInteractor>();

        int newlyCollapsed = 0;
        if (event_value == "all")
        {
            foreach (GameObject item in _buildingGameObject)
            {
                if (!(item == null) && item.activeSelf)
                {
                    if (interactor != null && !interactor.IsBuildingDoomed(item))
                    {
                        newlyCollapsed++;
                    }
                    component.SetBuildingToDoom(item);
                }
            }
            CommonsUtility.DamageReportSystem.AddQuakeCollapse(newlyCollapsed);
            return;
        }
        if (!int.TryParse(event_value, out var result))
        {
            Debug.Log("event_value is not number");
            return;
        }
        // [Season 3 W3 Task 4] 未倒壊建物を優先選択する。
        // 旧実装は先頭 N 棟を決定的に選び倒壊済みをスキップしないため、年々「新規倒壊」が
        // 減衰していた（同じ建物を再選択）。未倒壊だけを N 棟選ぶことで毎年新しい地域が被災する。
        // 出火位置の再現性は維持される（リスト順で先頭の未倒壊 N 棟）。
        int num = 0;
        foreach (GameObject item2 in _buildingGameObject)
        {
            if (num >= result)
            {
                break;
            }
            if (item2 == null || !item2.activeSelf)
            {
                continue;
            }
            if (interactor != null && interactor.IsBuildingDoomed(item2))
            {
                continue;  // 既に倒壊済み（地震/火災）はスキップして未倒壊を優先
            }
            component.SetBuildingToDoom(item2);
            newlyCollapsed++;
            num++;
        }
        CommonsUtility.DamageReportSystem.AddQuakeCollapse(newlyCollapsed);
        Debug.Log($"[BuildingBreak] {num} 棟を倒壊指定（うち新規 {newlyCollapsed} 棟・対象候補 {_buildingGameObject.Count} 棟）");
    }

    /// <summary>
    /// [building_break_fire 用] 未倒壊の建物を先頭から count 件 新規に倒壊させ、その建物を返す。
    /// EventBreakBuilding（building_break 本体）は変更せず、出火用に別メソッドとして用意。
    /// 呼び出し側（EventLoader）が返り値の各建物から出火させる（倒壊数＝出火数）
    /// </summary>
    internal System.Collections.Generic.List<GameObject> BreakBuildingsForFire(int count)
    {
        var broken = new System.Collections.Generic.List<GameObject>();
        if (_buildingGameObject.Count == 0)
        {
            Debug.LogWarning("[BuildingBreak] building_break_fire: 対象建物が 0 棟");
            return broken;
        }
        GameObject obj = GameObject.Find("Plateau");
        PlateauInfoManager component = obj != null ? obj.GetComponent<PlateauInfoManager>() : null;
        PlateauBuildingInteractor interactor = obj != null ? obj.GetComponent<PlateauBuildingInteractor>() : null;
        if (component == null)
        {
            Debug.LogWarning("[BuildingBreak] building_break_fire: PlateauInfoManager が見つからない");
            return broken;
        }

        int num = 0;
        foreach (GameObject item in _buildingGameObject)
        {
            if (num >= count)
            {
                break;
            }
            if (item == null || !item.activeSelf)
            {
                continue;
            }
            if (interactor != null && interactor.IsBuildingDoomed(item))
            {
                continue;  // 未倒壊優先（building_break と同じ）
            }
            component.SetBuildingToDoom(item);
            broken.Add(item);
            num++;
        }
        CommonsUtility.DamageReportSystem.AddQuakeCollapse(broken.Count);
        Debug.Log($"[BuildingBreak] building_break_fire: {broken.Count} 棟を倒壊させ出火対象に");
        return broken;
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
