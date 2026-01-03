using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CommonsUtility;


public class PlateauInfoManager : MonoBehaviour
{
    public static PlateauInfoManager _instance = null;

    // public static PlateauInfoManager Instance { get; private set; }

    private PlateauObjectSelector _objectSelector;
    private PlateauDataExtractor _dataExtractor;
    private PlateauUIManager _uiManager;
    private PlateauBuildingInteractor _buildingInteractor;

    const float _CENTER_Y_OFFSET = 5.0f;
    const float _BUFFER_Y_OFFSET = 0.5f;

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
        // else
        // {
        //     Destroy(gameObject);
        // }
        InitPlateauInfoWindow();
    }

    internal void InitPlateauInfoWindow()
    {
        _objectSelector = GetComponent<PlateauObjectSelector>()?? gameObject.AddComponent<PlateauObjectSelector>();
        _buildingInteractor = GetComponent<PlateauBuildingInteractor>()?? gameObject.AddComponent<PlateauBuildingInteractor>();
        _uiManager = GetComponent<PlateauUIManager>()?? gameObject.AddComponent<PlateauUIManager>();
        _dataExtractor = GetComponent<PlateauDataExtractor>()?? gameObject.AddComponent<PlateauDataExtractor>();
        // _buildingInteractor.OnBuildingRebuilt += HandleBuildingRebuilt;

        // _uiManager._plateauInfoManager = this._instance;
        GameObject infoBox = _uiManager.GetInfoBox();
        infoBox.SetActive(false);
        GameObject pnlInfo = infoBox.transform.Find("pnlInfo").gameObject;

        GameObject pnlRebuild = pnlInfo.transform.Find("pnlRebuild").gameObject;
        Button btnRebuild = pnlRebuild.transform.Find("btnRebuild").gameObject.GetComponent<Button>();
        btnRebuild.onClick.RemoveAllListeners();
        btnRebuild.onClick.AddListener(OnClickRebuildBtn);

        GameObject pnlBreak = pnlInfo.transform.Find("pnlBreak").gameObject;
        Button btnBreak = pnlBreak.transform.Find("btnBreak").gameObject.GetComponent<Button>();
        btnBreak.onClick.RemoveAllListeners();
        btnBreak.onClick.AddListener(OnClickBreakBtn);

        GameObject pnlDelete = pnlInfo.transform.Find("pnlDelete").gameObject;
        Button btnDelete = pnlDelete.transform.Find("btnDelete").gameObject.GetComponent<Button>();
        btnDelete.onClick.RemoveAllListeners();
        btnDelete.onClick.AddListener(OnClickDeleteBtn);
    }

    private void OnClickDeleteBtn()
    {
        // Debug.Log("OnClickDeleteBtn");
        // デバッグ用なのでいきなり削除
        GameObject gameObject = _objectSelector.GetSelectedObject();
        if (gameObject == null)
        {
            return;
        }
        Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(gameObject);
        float rebuildCost = _dataExtractor.CalcRebuildCost(buildingInfo);
        if (rebuildCost > 0)
        {
            // ScoreCtrl.UpdateAndDisplayScore((int)rebuildCost, GlobalConst.SHORT_SCORE1_SCALE);
            _buildingInteractor.DeleteBuilding(gameObject);
        }
    }

    private void OnClickBreakBtn()
    {
        // Debug.Log("OnClickBreakBtn");
        GameObject gameObject = _objectSelector.GetSelectedObject();
        bool isBuildingDoomed = _buildingInteractor.IsBuildingDoomed(gameObject);
        if (isBuildingDoomed)
        {
            return;
        }
        SetBuildingToDoom(gameObject);
        _uiManager.CloseInfoBox();

    }

    private void OnClickRebuildBtn()
    {
        Debug.Log("OnClickRebuildBtn");

        GameObject gameObject = _objectSelector.GetSelectedObject();
        Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(gameObject);
        float rebuildCost = _dataExtractor.CalcRebuildCost(buildingInfo);
        bool isBuildingDoomed = _buildingInteractor.IsBuildingDoomed(gameObject);
        // _uiManager.DisplayBuildingInfo(buildingInfo, rebuildCost, isBuildingDoomed);

        if (isBuildingDoomed)
        {
            if (PayRebuildCost((int)rebuildCost*-1))
            {
                CallCircularIndicator(gameObject);
            }
            else
            {
             
                // Debug.Log("再建コストが足りません");
                // ScoreCtrl.UpdateAndDisplayScore(3000, GlobalConst.SHORT_SCORE1_SCALE);
                _uiManager.ShowInsufficientFundsMessage();
            }
        }
    }

    private bool PayRebuildCost(int rebuildCost)
    {
        bool ret = false;
        if (ScoreCtrl.IsScorePositiveInt(rebuildCost, GlobalConst.SHORT_SCORE1_SCALE))
        {
            ScoreCtrl.UpdateAndDisplayScore((int)rebuildCost, GlobalConst.SHORT_SCORE1_SCALE);
            return true;
        }
        return ret;
    }

    private void CallCircularIndicator(GameObject target)
    {
        GameObject infoBox = _uiManager.GetInfoBox();
        infoBox.SetActive(false);

        GameObject UICircularIndicator = Instantiate(Resources.Load("Prefabs/UI/UICircularIndicator")) as GameObject;
        CircularIndicator indicator = UICircularIndicator.GetComponent<CircularIndicator>();
        
        GameObject indicator_object = new GameObject("Indicator");
        indicator_object.transform.SetParent(target.transform);
        Canvas indicator_canvas = indicator_object.AddComponent<Canvas>();
        
        Renderer renderer = target.GetComponent<Renderer>();
        Vector3 center = renderer.bounds.center;
        center.y += _CENTER_Y_OFFSET * 2 + _BUFFER_Y_OFFSET;
        indicator_object.transform.position = center;

        // TODO: 修復時間を建物サイズに応じて調整する

        indicator.StartIndicator(2f, () => {
                    CompleteReBuildProcess(target);
                    }, indicator_object);
    }

    private void CompleteReBuildProcess(GameObject target)
    {
        // 建物再建によるスコア加算（クロックを取得）
        Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(target);
        float rebuildBonus = _dataExtractor.CalcRebuildBonus(buildingInfo);
        ScoreCtrl.UpdateAndDisplayScore((int)rebuildBonus, GlobalConst.SHORT_SCORE2_SCALE);

        _buildingInteractor.RestoreBuildingMaterial(target);
        // _buildingInteractor._doomedBuildings.Remove(target);
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
        GameObject gameObject = _objectSelector.GetSelectedObject();
        Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(gameObject);
        float rebuildCost = _dataExtractor.CalcRebuildCost(buildingInfo);
        bool isBuildingDoomed = _buildingInteractor.IsBuildingDoomed(gameObject);
        _uiManager.DisplayBuildingInfo(buildingInfo, rebuildCost, isBuildingDoomed);
    }

    internal void SetBuildingToDoom(GameObject building)
    {
        Dictionary<string, string> buildingInfo = _dataExtractor.TryGetBuildingInfo(building);
        float rebuildBonus = _dataExtractor.CalcRebuildBonus(buildingInfo) * -1;
        float rebuildCost = _dataExtractor.CalcRebuildCost(buildingInfo);

        ScoreCtrl.UpdateAndDisplayScore((int)rebuildBonus, GlobalConst.SHORT_SCORE2_SCALE);

        // rebuildCost 分のゴミを放出する
        PlateauCubeMaker plateauCubeMaker = gameObject.GetComponent<PlateauCubeMaker>();
        if (plateauCubeMaker == null)
        {
            plateauCubeMaker = gameObject.AddComponent<PlateauCubeMaker>();
        }
        // plateauCubeMaker.DispCubeMarker(building, buildingInfo);
        plateauCubeMaker.BreakUpBuildingCube(building, (int)rebuildCost);
        _buildingInteractor.SetBuildingToDoom(building);
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