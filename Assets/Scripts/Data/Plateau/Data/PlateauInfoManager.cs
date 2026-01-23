using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
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

    internal void SetBuildingToDoom(GameObject building)
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
            _buildingInteractor.SetBuildingToDoom(building);
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