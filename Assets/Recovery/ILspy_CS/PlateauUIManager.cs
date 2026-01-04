// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PlateauUIManager
using System;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using UnityEngine.UI;

public class PlateauUIManager : MonoBehaviour
{
	private GameObject _infoBox;

	internal static PlateauInfoManager _plateauInfoManager;

	internal void DisplayBuildingInfo(Dictionary<string, string> buildingInfo, float rebuildCost, bool isDoomedBuilding)
	{
		GameObject infoBox = GetInfoBox();
		string text = FormatBuildingInfo(buildingInfo, rebuildCost);
		GameObject obj = infoBox.transform.Find("pnlInfo").gameObject;
		obj.transform.Find("txtInfo").gameObject.GetComponent<Text>().text = text;
		GameObject gameObject = obj.transform.Find("pnlRebuild").gameObject;
		GameObject gameObject2 = obj.transform.Find("pnlBreak").gameObject;
		GameObject gameObject3 = obj.transform.Find("pnlDelete").gameObject;
		if (!IsDemLod(buildingInfo))
		{
			gameObject.SetActive(isDoomedBuilding);
			if (isDoomedBuilding)
			{
				gameObject.transform.Find("txtRebuild").gameObject.GetComponent<Text>().text = " - " + rebuildCost + "BIT";
			}
			gameObject2.SetActive(!isDoomedBuilding);
			gameObject3.SetActive(value: false);
			if (GameConfig._APP_GAME_MODE == "debug")
			{
				gameObject3.SetActive(value: true);
			}
		}
		else
		{
			gameObject.SetActive(value: false);
			gameObject2.SetActive(value: false);
			gameObject3.SetActive(value: false);
		}
		infoBox.SetActive(value: true);
	}

	internal void CloseInfoBox()
	{
		GetInfoBox().SetActive(value: false);
	}

	private void Awake()
	{
		InitBuildingInfo();
	}

	private void InitBuildingInfo()
	{
		if (_plateauInfoManager == null)
		{
			_plateauInfoManager = base.gameObject.GetComponent<PlateauInfoManager>();
		}
	}

	internal GameObject GetInfoBox()
	{
		if (_infoBox != null)
		{
			return _infoBox;
		}
		GameObject gameObject = GameObject.Find("UIBuildingInfo");
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/UIBuildingInfo") as GameObject);
			gameObject.name = "UIBuildingInfo";
			_plateauInfoManager = base.gameObject.GetComponent<PlateauInfoManager>();
			_plateauInfoManager.InitPlateauInfoWindow();
		}
		_infoBox = gameObject;
		return gameObject;
	}

	private bool IsDemLod(Dictionary<string, string> buildingInfo)
	{
		if (buildingInfo.ContainsKey("dem:lod"))
		{
			return true;
		}
		return false;
	}

	private string FormatBuildingInfo(Dictionary<string, string> buildingInfo, float rebuildCost)
	{
		string text = "";
		foreach (KeyValuePair<string, string> item in buildingInfo)
		{
			string langVal = LangCtrl.GetLangVal(item.Key);
			if (!string.IsNullOrEmpty(langVal))
			{
				text = text + langVal + ": " + item.Value + Environment.NewLine;
			}
		}
		return text;
	}

	internal void ShowInsufficientFundsMessage()
	{
		Debug.Log("再建コストが足りません");
		TelopCtrl component = GameObject.Find("UITelop").GetComponent<TelopCtrl>();
		if (component != null)
		{
			component.ShowTelop("Not enough the rebuild power", isSubTelop: true);
		}
	}
}
