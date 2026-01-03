using System;
using UnityEngine;
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
        pnlRebuild.SetActive(isDoomedBuilding);
        if (isDoomedBuilding)
        {
            GameObject txtRebuild = pnlRebuild.transform.Find("txtRebuild").gameObject;
            Text txtRebuildText = txtRebuild.GetComponent<Text>();
            txtRebuildText.text = " - " + rebuildCost.ToString() + GlobalConst.SHORT_SCORE1_SCALE;
        }

        GameObject pnlBreak = pnlInfo.transform.Find("pnlBreak").gameObject;
        pnlBreak.SetActive(!isDoomedBuilding);
        // if (!isDoomedBuilding)
        // {
        //     GameObject txtBreak = pnlRebuild.transform.Find("txtBreak").gameObject;
        //     Text txtBreakText = txtBreak.GetComponent<Text>();
        //     // txtBreakText.text = " - " + rebuildCost.ToString() + GlobalConst.SHORT_SCORE1_SCALE;
        // }

        GameObject pnlDelete = pnlInfo.transform.Find("pnlDelete").gameObject;
        pnlDelete.SetActive(false);
        if (GameConfig._APP_GAME_MODE == GlobalConst.GAME_MODE_DEBUG)
        {
                pnlDelete.SetActive(true);
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
            infoBox = Instantiate(Resources.Load("Prefabs/UI/UIBuildingInfo") as GameObject);
            infoBox.name = "UIBuildingInfo";
            _plateauInfoManager = this.gameObject.GetComponent<PlateauInfoManager>();
            _plateauInfoManager.InitPlateauInfoWindow();
        }

        _infoBox = infoBox;
        return infoBox;
    }

    private string FormatBuildingInfo(Dictionary<string, string> buildingInfo, float rebuildCost)
    {
        string formattedInfo = "";
        foreach (var pair in buildingInfo)
        {
            string langH = LangCtrl.GetLangVal(pair.Key);
            if (!string.IsNullOrEmpty(langH))
            {
                formattedInfo += $"{langH}: {pair.Value}{Environment.NewLine}";
                // formattedInfo += $"{langH}: {pair.Value}\n";
                // formattedInfo += $"{langH}: {pair.Key} {pair.Value}\n";
            }
            else
            {
                // formattedInfo += $"{pair.Key}: {pair.Value}\n";
                // Debug.Log($"{pair.Key}: {pair.Value}");
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
