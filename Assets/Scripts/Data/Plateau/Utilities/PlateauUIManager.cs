using System;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using CommonsUtility;


public class PlateauUIManager : MonoBehaviour
{
    private GameObject _infoBox;
    internal static PlateauInfoManager _plateauInfoManager = null;
    
    internal void DisplayBuildingInfo(Dictionary<string, string> buildingInfo, float rebuildCost, bool isDoomedBuilding)
    {
        GameObject infoBox = GetInfoBox();
        string infoText = FormatBuildingInfo(buildingInfo, rebuildCost);

        GameObject pnlInfo = infoBox.transform.Find("pnlInfo").gameObject;
        GameObject txtInfo = pnlInfo.transform.Find("txtInfo").gameObject;
        Text txtInfoText = txtInfo.GetComponent<Text>();
        txtInfoText.text = infoText;
        GameObject pnlRebuild = pnlInfo.transform.Find("pnlRebuild").gameObject;
        GameObject pnlBreak = pnlInfo.transform.Find("pnlBreak").gameObject;
        GameObject pnlDelete = pnlInfo.transform.Find("pnlDelete").gameObject;

        bool isDemLod = IsDemLod(buildingInfo);
        if (!isDemLod)
        {
            pnlRebuild.SetActive(isDoomedBuilding);
            if (isDoomedBuilding)
            {
                GameObject txtRebuild = pnlRebuild.transform.Find("txtRebuild").gameObject;
                Text txtRebuildText = txtRebuild.GetComponent<Text>();
                txtRebuildText.text = " - " + rebuildCost.ToString() + GlobalConst.SHORT_SCORE1_SCALE;
            }

            pnlBreak.SetActive(!isDoomedBuilding);

            // 解体パネル（実行時に PlateauInfoManager が追加）も破壊前の建物にのみ表示する
            Transform demolitionPanel = pnlInfo.transform.Find(PlateauInfoManager.DEMOLITION_PANEL_NAME);
            if (demolitionPanel != null)
            {
                demolitionPanel.gameObject.SetActive(!isDoomedBuilding);
            }
            // if (!isDoomedBuilding)
            // {
            //     GameObject txtBreak = pnlRebuild.transform.Find("txtBreak").gameObject;
            //     Text txtBreakText = txtBreak.GetComponent<Text>();
            //     // txtBreakText.text = " - " + rebuildCost.ToString() + GlobalConst.SHORT_SCORE1_SCALE;
            // }

            // [CityHack 2026] 解体パネルは pnlDelete の位置を借りているため、
            // デバッグモードで両方を出すと重なる。解体ボタンを優先して Delete は隠す
            pnlDelete.SetActive(false);
            bool hasDemolitionPanel = pnlInfo.transform.Find(PlateauInfoManager.DEMOLITION_PANEL_NAME) != null;
            if (GameConfig._APP_GAME_MODE == GlobalConst.GAME_MODE_DEBUG && !hasDemolitionPanel)
            {
                    pnlDelete.SetActive(true);
            }
        }
        else
        {
            pnlRebuild.SetActive(false);
            pnlBreak.SetActive(false);
            pnlDelete.SetActive(false);

            // 道路等（DEM）では解体パネルも隠す
            Transform demolitionPanelOnDem = pnlInfo.transform.Find(PlateauInfoManager.DEMOLITION_PANEL_NAME);
            if (demolitionPanelOnDem != null)
            {
                demolitionPanelOnDem.gameObject.SetActive(false);
            }
        }
        infoBox.SetActive(true);
    }

    internal void CloseInfoBox()
    {
        GameObject infoBox = GetInfoBox();
        infoBox.SetActive(false);
        // GameObjectTreat.DestroyAll(infoBox);
    }

    void Awake()
    {
        InitBuildingInfo();
    }

    private void InitBuildingInfo()
    {
        if (_plateauInfoManager == null)
        {
            _plateauInfoManager = this.gameObject.GetComponent<PlateauInfoManager>();
        }

    }


    internal GameObject GetInfoBox()
    {
        if (_infoBox != null)
        {
            return _infoBox;
        }
        
        GameObject infoBox = GameObject.Find("UIBuildingInfo");
        if (infoBox == null)
        {
            GameObject prefab = PrefabManager.UIBuildingInfoPrefab;
            if (prefab == null)
            {
                Debug.LogWarning("[PlateauUIManager] UIBuildingInfo prefab not found");
                return null;
            }
            infoBox = Instantiate(prefab);
            infoBox.name = "UIBuildingInfo";
            _plateauInfoManager = this.gameObject.GetComponent<PlateauInfoManager>();
            _plateauInfoManager.InitPlateauInfoWindow();
        }

        _infoBox = infoBox;
        return infoBox;
    }

    private bool IsDemLod(Dictionary<string, string> buildingInfo)
    {
        // "key": "dem:lod",
        // dem:lodモードの場合はtrueを返す
        if (buildingInfo.ContainsKey("dem:lod"))
        {
            return true;
        }
        return false;
    }


    private string FormatBuildingInfo(Dictionary<string, string> buildingInfo, float rebuildCost)
    {
        string formattedInfo = "";
        foreach (var pair in buildingInfo)
        {
            string langH = LanguageManager.GetLangVal(pair.Key);
            if (!string.IsNullOrEmpty(langH))
            {
                formattedInfo += $"{langH}: {pair.Value}{Environment.NewLine}";
                // formattedInfo += $"{langH}: {pair.Value}\n";
                // formattedInfo += $"{langH}: {pair.Key} {pair.Value}\n";
            }
            else
            {
                // formattedInfo += $"{pair.Key}: {pair.Value}\n";
            }
        }
        // formattedInfo += LangCtrl.GetLangVal("rebuildcost") + ": " + rebuildCost + "\n";
        return formattedInfo;
    }

    internal void ShowInsufficientFundsMessage()
    {
        // TODO:再建できない場合の処理
        Debug.Log("再建コストが足りません");
        TelopCtrl telopCtrl = GameObject.Find("UITelop").GetComponent<TelopCtrl>();
        if (telopCtrl != null)
        {
            telopCtrl.ShowTelop("Not enough the rebuild power", true);
        }

    }

}
