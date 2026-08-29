using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.UI;

public class PlateauInfoManager : MonoBehaviour
{
    public static PlateauInfoManager _instance;

    private PlateauObjectSelector _objectSelector;
    private PlateauDataExtractor _dataExtractor;
    private PlateauUIManager _uiManager;
    private PlateauBuildingInteractor _buildingInteractor;

    private const float _CENTER_Y_OFFSET = 5f;
    private const float _BUFFER_Y_OFFSET = 0.5f;

    // public static PlateauInfoManager Instance
    // {
    //     get
    //     {
    //         if (_instance == null)
    //         {
    //             _instance = FindFirstObjectByType<PlateauInfoManager>();
    //             if (_instance == null)
    //             {
    //                 GameObject go = new GameObject("Plateau");  // 本来はGameInterfaceの下にいる
    //                 _instance = go.AddComponent<PlateauInfoManager>();
    //             }
    //         }
    //         return _instance;
    //     }
    // }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        InitPlateauInfoWindow();
    }

    internal void InitPlateauInfoWindow()
    {
        _objectSelector = GetComponent<PlateauObjectSelector>() ?? base.gameObject.AddComponent<PlateauObjectSelector>();
        _buildingInteractor = GetComponent<PlateauBuildingInteractor>() ?? base.gameObject.AddComponent<PlateauBuildingInteractor>();
        _uiManager = GetComponent<PlateauUIManager>() ?? base.gameObject.AddComponent<PlateauUIManager>();
        _dataExtractor = GetComponent<PlateauDataExtractor>() ?? base.gameObject.AddComponent<PlateauDataExtractor>();
        GameObject infoBox = _uiManager.GetInfoBox();
        infoBox.SetActive(value: false);
        GameObject obj = infoBox.transform.Find("pnlInfo").gameObject;
        Button component = obj.transform.Find("pnlRebuild").gameObject.transform.Find("btnRebuild").gameObject.GetComponent<Button>();
        component.onClick.RemoveAllListeners();
        component.onClick.AddListener(OnClickRebuildBtn);
        Button component2 = obj.transform.Find("pnlBreak").gameObject.transform.Find("btnBreak").gameObject.GetComponent<Button>();
        component2.onClick.RemoveAllListeners();
        component2.onClick.AddListener(OnClickBreakBtn);
        Button component3 = obj.transform.Find("pnlDelete").gameObject.transform.Find("btnDelete").gameObject.GetComponent<Button>();
        component3.onClick.RemoveAllListeners();
        component3.onClick.AddListener(OnClickDeleteBtn);

        SetupDemolitionButton(obj);
    }

    /// <summary>情報ウィンドウに追加する解体パネルの名前（CityHack 2026）</summary>
    internal const string DEMOLITION_PANEL_NAME = "pnlDemolition";

    /// <summary>解体ボタンのラベル</summary>
    private const string _DEMOLITION_LABEL = "解体";

    /// <summary>レイアウトグループが無い場合に複製パネルをずらす量</summary>
    private const float _DEMOLITION_PANEL_OFFSET_Y = -36f;

    /// <summary>
    /// 建物情報ウィンドウに「解体」ボタンを実行時に追加する（CityHack 2026）。
    ///
    /// UI プレファブを編集せずに済むよう、既存の pnlBreak を複製してラベルと
    /// クリック処理だけ差し替える。表示制御は PlateauUIManager が pnlBreak と同条件で行う
    /// </summary>
    private void SetupDemolitionButton(GameObject pnlInfo)
    {
        Transform breakPanel = pnlInfo.transform.Find("pnlBreak");
        if (breakPanel == null)
        {
            Debug.LogWarning("[PlateauInfoManager] pnlBreak が見つからないため解体ボタンを追加できません");
            return;
        }

        // 既に追加済みなら配線し直すだけ（シーン再読込時の二重生成を防ぐ）
        Transform existing = pnlInfo.transform.Find(DEMOLITION_PANEL_NAME);
        GameObject demolitionPanel;
        if (existing != null)
        {
            demolitionPanel = existing.gameObject;
        }
        else
        {
            demolitionPanel = Object.Instantiate(breakPanel.gameObject, breakPanel.parent);
            demolitionPanel.name = DEMOLITION_PANEL_NAME;

            // 親にレイアウトグループが無いため位置を明示する。
            // 通常プレイでは非表示の pnlDelete（デバッグ専用）の位置を借りると、
            // 既存パネルと重ならず、デザイン済みの座標をそのまま使える
            if (breakPanel.parent.GetComponent<LayoutGroup>() == null)
            {
                RectTransform demolitionRect = demolitionPanel.GetComponent<RectTransform>();
                Transform deletePanel = pnlInfo.transform.Find("pnlDelete");
                RectTransform deleteRect = deletePanel != null ? deletePanel.GetComponent<RectTransform>() : null;

                if (demolitionRect != null && deleteRect != null)
                {
                    demolitionRect.anchoredPosition = deleteRect.anchoredPosition;
                }
                else if (demolitionRect != null)
                {
                    RectTransform breakRect = breakPanel.GetComponent<RectTransform>();
                    demolitionRect.anchoredPosition = breakRect.anchoredPosition
                        + new Vector2(0f, _DEMOLITION_PANEL_OFFSET_Y);
                }
            }

            SetPanelLabel(demolitionPanel, _DEMOLITION_LABEL);
        }

        Button demolitionButton = demolitionPanel.GetComponentInChildren<Button>();
        if (demolitionButton == null)
        {
            Debug.LogWarning("[PlateauInfoManager] 解体パネルにボタンが見つかりません");
            return;
        }
        demolitionButton.onClick.RemoveAllListeners();
        demolitionButton.onClick.AddListener(OnClickDemolishBtn);
    }

    /// <summary>複製したパネル内のテキストをすべて差し替える</summary>
    private void SetPanelLabel(GameObject panel, string label)
    {
        foreach (Text text in panel.GetComponentsInChildren<Text>(true))
        {
            text.text = label;
        }
        foreach (TMPro.TextMeshProUGUI tmp in panel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
        {
            tmp.text = label;
        }
    }

    /// <summary>
    /// 解体ボタン: 選択中の建物を解体（更地化＋瓦礫散布＋廃棄物の積算）する
    /// </summary>
    private void OnClickDemolishBtn()
    {
        GameObject selectedObject = _objectSelector.GetSelectedObject();
        if (selectedObject == null)
        {
            return;
        }

        if (DemolishBuilding(selectedObject) > 0f)
        {
            _uiManager.CloseInfoBox();
        }
    }

    private void OnClickDeleteBtn()
    {
        GameObject selectedObject = _objectSelector.GetSelectedObject();
        if (!(selectedObject == null))
        {
            Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(selectedObject);
            if (_dataExtractor.CalcRebuildCost(buildingInfo) > 0f)
            {
                _buildingInteractor.DeleteBuilding(selectedObject);
            }
        }
    }

    private void OnClickBreakBtn()
    {
        GameObject selectedObject = _objectSelector.GetSelectedObject();
        if (!_buildingInteractor.IsBuildingDoomed(selectedObject))
        {
            SetBuildingToDoom(selectedObject);
            _uiManager.CloseInfoBox();
        }
    }

    private void OnClickRebuildBtn()
    {
        Debug.Log("OnClickRebuildBtn");
        GameObject selectedObject = _objectSelector.GetSelectedObject();
        Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(selectedObject);
        float num = _dataExtractor.CalcRebuildCost(buildingInfo);
        if (_buildingInteractor.IsBuildingDoomed(selectedObject))
        {
            if (PayRebuildCost((int)num * -1))
            {
                CallCircularIndicator(selectedObject);
            }
            else
            {
                _uiManager.ShowInsufficientFundsMessage();
            }
        }
    }

    private bool PayRebuildCost(int rebuildCost)
    {
        bool result = false;
        if (ScoreCtrl.IsScorePositiveInt(rebuildCost))
        {
            ScoreCtrl.UpdateAndDisplayScore(rebuildCost);
            return true;
        }
        return result;
    }

    private void CallCircularIndicator(GameObject target)
    {
        _uiManager.GetInfoBox().SetActive(value: false);
        Vector3 center = target.GetComponent<Renderer>().bounds.center;
        center.y += 10.5f;
        MarkerIndicatorCtrl.CreateCircularIndicator(target, 2f, CompleteReBuildProcess, center);
    }

    private void CompleteReBuildProcess(GameObject target)
    {
        Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(target);
        ScoreCtrl.UpdateAndDisplayScore((int)(float)_dataExtractor.CalcRebuildBonus(buildingInfo), "CLK");
        _buildingInteractor.RestoreBuildingMaterial(target);
        _uiManager.CloseInfoBox();
    }

    // private void HandleBuildingRebuilt(GameObject rebuiltBuilding)
    // {
    //     Dictionary<string, string> updatedInfo = _dataExtractor.TryGetBuildingInfo(rebuiltBuilding);
    //     _uiManager.DisplayBuildingInfo(updatedInfo);
    //     // Trigger any other necessary updates in the system
    // }

    internal bool IsPlateauObject()
    {
        return _objectSelector.IsPLATEAUObject();
    }

    internal void DisplayPlateauInfo()
    {
        GameObject selectedObject = _objectSelector.GetSelectedObject();
        Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(selectedObject);
        float rebuildCost = _dataExtractor.CalcRebuildCost(buildingInfo);
        bool isDoomedBuilding = _buildingInteractor.IsBuildingDoomed(selectedObject);
        _uiManager.DisplayBuildingInfo(buildingInfo, rebuildCost, isDoomedBuilding);
    }

    /// <summary>
    /// 建物を解体する（CityHack 2026）。
    /// 属性から解体廃棄物量を算定 → 跡地に瓦礫を散布 → 建物を完全消去（更地化）→ 累計に記録。
    ///
    /// SetBuildingToDoom（マテリアル変更で「壊れた表現」にする既存動作）とは異なり、
    /// 建物そのものを消して更地にするのが解体イベントの挙動
    /// </summary>
    /// <returns>発生した解体廃棄物量（t）。解体できなかった場合は 0</returns>
    internal float DemolishBuilding(GameObject building)
    {
        if (building == null)
        {
            return 0f;
        }

        Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(building);
        float debrisTons = DemolitionSystem.CalcDebrisTons(buildingInfo);
        if (debrisTons <= 0f)
        {
            Debug.LogWarning($"[PlateauInfoManager.DemolishBuilding] 廃棄物量を算定できません: {building.name}");
            return 0f;
        }

        // 跡地に瓦礫を散布（建物を消す前に位置・サイズを使う）
        PlateauCubeMaker plateauCubeMaker = base.gameObject.GetComponent<PlateauCubeMaker>();
        if (plateauCubeMaker == null)
        {
            plateauCubeMaker = base.gameObject.AddComponent<PlateauCubeMaker>();
        }
        int debrisAmount = DemolitionSystem.CalcDebrisAmount(buildingInfo, debrisTons);
        int maxCubes = DemolitionSystem.CalcMaxCubes(buildingInfo);
        float noncombustibleRatio = DemolitionSystem.CalcNoncombustibleRatio(buildingInfo);
        plateauCubeMaker.ScatterDemolitionDebris(building, debrisAmount, maxCubes, noncombustibleRatio);

        // 更地化（建物を完全に消去）
        _buildingInteractor.DeleteBuilding(building);

        DemolitionSystem.RecordDemolition(debrisTons);
        Debug.Log($"[Demolition] {building.name}: {debrisTons:F1} t / {DemolitionSystem.GetSummaryText()}");
        return debrisTons;
    }

    internal void SetBuildingToDoom(GameObject building, bool isFire = false)
    {
        if (!_buildingInteractor.IsBuildingDoomed(building))
        {
            Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(building);
            float num = _dataExtractor.CalcRebuildBonus(buildingInfo) * -1;
            float num2 = _dataExtractor.CalcRebuildCost(buildingInfo);
            ScoreCtrl.UpdateAndDisplayScore((int)num, "CLK");
            PlateauCubeMaker plateauCubeMaker = base.gameObject.GetComponent<PlateauCubeMaker>();
            if (plateauCubeMaker == null)
            {
                plateauCubeMaker = base.gameObject.AddComponent<PlateauCubeMaker>();
            }
            plateauCubeMaker.BreakUpBuildingCube(building, (int)num2);
            _buildingInteractor.SetBuildingToDoom(building, isFire);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("PlateauInfoManager OnDestroy");
        if (_instance == this)
        {
            _instance = null;
        }
        // buildingInteractor.OnBuildingRebuilt -= HandleBuildingRebuilt;
    }
}
