// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// InfoWindowCtrl
using System;
using CommonsUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoWindowCtrl : MonoBehaviour
{
	private GameObject _infoWindow;

	[SerializeField]
	public Button _btnClose;

	public Button _btnOK;

	public Button _btnDelete;

	private UnitStruct? _unitStruct;

	internal void OnClickClose()
	{
		ToggleInfoWindow(isActive: false);
	}

	private void InitWindow()
	{
		if (_infoWindow == null)
		{
			_infoWindow = base.transform.Find("InfoWindow").gameObject;
		}
		if (_btnClose != null)
		{
			_btnClose.onClick.AddListener(OnClickClose);
		}
		if (_btnOK != null)
		{
			_btnOK.onClick.AddListener(OnClickClose);
		}
		if (_btnDelete != null)
		{
			_btnDelete.onClick.AddListener(OnClickDelete);
		}
		ToggleInfoWindow(isActive: false);
	}

	private void CallCircularIndicator(GameObject target)
	{
		Vector3 delIndicatorPosition = GetDelIndicatorPosition(target);
		MarkerIndicatorCtrl.CreateCircularIndicator(target, 5f, DeleteUnitProcess, delIndicatorPosition);
	}

	private Vector3 GetDelIndicatorPosition(GameObject target)
	{
		Vector3 position = target.transform.position;
		Transform transform = target.transform.Find("dispposi");
		if (transform != null)
		{
			position = transform.position;
		}
		return position;
	}

	private void DeleteUnitProcess(GameObject target)
	{
		if (target == null)
		{
			return;
		}
		if (target.tag == GameEnum.TagType.TowerSweeper.ToString())
		{
			TowerSweeper component = target.GetComponent<TowerSweeper>();
			if (component != null)
			{
				component.DeleteUnitProcess();
			}
		}
		else if (target.tag == GameEnum.TagType.WaterTurret.ToString())
		{
			WaterTurretCtrl component2 = target.GetComponent<WaterTurretCtrl>();
			if (component2 != null)
			{
				component2.DeleteUnitProcess();
			}
		}
		else
		{
			Debug.Log("DeleteUnitProcess " + target.name + " " + target.tag);
			int intScore = _unitStruct?.DeleteCost ?? 0;
			string score_type = _unitStruct?.ScoreType;
			ScoreCtrl.UpdateAndDisplayScore(intScore, score_type);
			GameObjectTreat.DestroyAll(target);
		}
	}

	private void OnClickDelete()
	{
		GameObject gameObject = GameObject.Find(_unitStruct?.UnitID);
		if (!(gameObject == null) && IsDeleteAbleUnit(_unitStruct))
		{
			CallCircularIndicator(gameObject);
			ToggleInfoWindow(isActive: false);
		}
	}

	private bool IsDeleteAbleUnit(UnitStruct? unitStruct)
	{
		if (GameObject.Find(unitStruct?.UnitID) == null)
		{
			return false;
		}
		if (unitStruct.HasValue || (unitStruct.HasValue && unitStruct.GetValueOrDefault().DeleteCost > 0))
		{
			return true;
		}
		return false;
	}

	internal void GetTargetUnit()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		int layerMask = ~LayerMask.GetMask(GameEnum.LayerType.AreaIgnoreRaycast.ToString());
		if (Physics.Raycast(ray, out var hitInfo, float.PositiveInfinity, layerMask))
		{
			UnitStruct? unitStruct = GetUnitStruct(hitInfo.collider.gameObject);
			int totalGarbageScore = ScoreCtrl.GetTotalGarbageScore(hitInfo.collider);
			if (SetInfo(unitStruct, totalGarbageScore))
			{
				ToggleInfoWindow(isActive: true);
			}
		}
	}

	internal bool SetInfo(UnitStruct? unitStruct, int get_score = 0)
	{
		if (!unitStruct.HasValue || unitStruct?.Name == "")
		{
			return false;
		}
		base.transform.Find("InfoWindow/title/tmpUnitName").GetComponent<TextMeshProUGUI>().text = unitStruct?.Name ?? "-";
		base.transform.Find("InfoWindow/info/tmpUnitID").GetComponent<TextMeshProUGUI>().text = "UnitID : " + unitStruct?.UnitID;
		base.transform.Find("InfoWindow/info/tmpUnitLv").GetComponent<TextMeshProUGUI>().text = "";
		TextMeshProUGUI component = base.transform.Find("InfoWindow/info/tmpUnitInfo").GetComponent<TextMeshProUGUI>();
		component.text = "INFO : " + unitStruct?.Info;
		if (get_score != 0)
		{
			component.text = component.text + Environment.NewLine + get_score + unitStruct?.ScoreType;
		}
		SetDeleteCost(unitStruct);
		return true;
	}

	private void SetDeleteCost(UnitStruct? unitStruct)
	{
		TextMeshProUGUI component = base.transform.Find("InfoWindow/delete/tmpDeleteCost").GetComponent<TextMeshProUGUI>();
		Button component2 = base.transform.Find("InfoWindow/delete/btnDelete").GetComponent<Button>();
		string text = SignedScore(unitStruct?.DeleteCost ?? 0);
		if (!unitStruct.HasValue || text == "" || !IsDeleteAbleUnit(unitStruct))
		{
			component.text = "";
			component2.gameObject.SetActive(value: false);
		}
		else
		{
			component.text = text + unitStruct?.ScoreType;
			component2.gameObject.SetActive(value: true);
		}
	}

	private static string SignedScore(int score)
	{
		if (score > 0)
		{
			return "+ " + score;
		}
		if (score < 0)
		{
			return "- " + Mathf.Abs(score);
		}
		return "";
	}

	internal void ToggleInfoWindow(bool isActive)
	{
		if (!(_infoWindow == null))
		{
			_infoWindow.SetActive(isActive);
		}
	}

	private GameObject GetParentObject(GameObject collider)
	{
		Transform parent = collider.transform.parent;
		if (parent == null)
		{
			return collider;
		}
		if (collider.tag == GameEnum.TagType.Garbage.ToString() || collider.tag == GameEnum.TagType.PowerCube.ToString() || collider.tag == GameEnum.TagType.FireCube.ToString())
		{
			return collider;
		}
		return parent.gameObject;
	}

	private UnitStruct? GetUnitStruct(GameObject collider)
	{
		UnitStruct? unitStruct = null;
		collider = GetParentObject(collider);
		string text = collider.tag;
		if (text == GameEnum.TagType.Garbage.ToString() || text == GameEnum.TagType.Ash.ToString())
		{
			unitStruct = collider.GetComponent<GarbageCube>().GetUnitStruct();
		}
		else if (text == GameEnum.TagType.PowerCube.ToString())
		{
			unitStruct = collider.GetComponent<PowerCube>().GetUnitStruct();
		}
		else if (text == GameEnum.TagType.TowerSweeper.ToString())
		{
			unitStruct = collider.GetComponent<Sweeper>().GetUnitStruct();
		}
		else if (text == GameEnum.TagType.FireCube.ToString())
		{
			unitStruct = collider.GetComponent<FireCube>().GetUnitStruct();
		}
		else if (text == GameEnum.TagType.WaterTurret.ToString())
		{
			unitStruct = collider.GetComponent<WaterTurret>().GetUnitStruct();
		}
		else if (text == GameEnum.TagType.EnemyLitters.ToString())
		{
			unitStruct = collider.GetComponent<Litter>().GetUnitStruct();
		}
		else if (text == GameEnum.TagType.DustBox.ToString())
		{
			unitStruct = collider.GetComponent<DustBox>().GetUnitStruct();
		}
		else if (text == GameEnum.TagType.StopPlate.ToString())
		{
			unitStruct = collider.GetComponent<StopPlate>().GetUnitStruct();
		}
		else if (text == GameEnum.TagType.SentryGuard.ToString())
		{
			unitStruct = collider.GetComponent<SentryGuard>().GetUnitStruct();
		}
		else
		{
			Debug.Log("GetUnitStruct default " + text + " " + collider.name);
		}
		_unitStruct = unitStruct;
		return unitStruct;
	}

	private void Awake()
	{
		InitWindow();
	}
}
