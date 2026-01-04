// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// TelopCtrl
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TelopCtrl : MonoBehaviour
{
	private GameObject _UITelop;

	private GameObject _telop;

	private GameObject _txtTelop;

	private GameObject _subTelop;

	private GameObject _txtsubTelop;

	private const float _TELOP_DISPLAY_TIME = 2.5f;

	private const float _SUB_TELOP_DISPLAY_TIME = 2.5f;

	private float _telopDisplayTime;

	private void Awake()
	{
		_UITelop = base.gameObject;
		_telop = _UITelop.transform.Find("Telop").gameObject;
		_txtTelop = _telop.transform.Find("txtTelop").gameObject;
		_subTelop = _UITelop.transform.Find("subTelop").gameObject;
		_txtsubTelop = _subTelop.transform.Find("txtSubTelop").gameObject;
		ToggleTelop(isOn: false);
		ToggleTelop(isOn: false, isSubTelop: true);
	}

	internal void ShowTelop(string telop, bool isSubTelop = false)
	{
		_ = _telop;
		GameObject gameObject = _txtTelop;
		_telopDisplayTime = 2.5f;
		if (isSubTelop)
		{
			_ = _subTelop;
			gameObject = _txtsubTelop;
			_telopDisplayTime = 2.5f;
		}
		if (gameObject != null)
		{
			gameObject.GetComponent<TextMeshProUGUI>().text = telop;
			ToggleTelop(isOn: true, isSubTelop);
			StartCoroutine(InvokeWithDelay(_telopDisplayTime, delegate
			{
				ToggleTelop(isOn: false, isSubTelop);
			}));
		}
	}

	private IEnumerator InvokeWithDelay(float delay, Action action)
	{
		yield return new WaitForSeconds(delay);
		action();
	}

	private void ToggleTelop(bool isOn, bool isSubTelop = false)
	{
		GameObject gameObject = _telop;
		if (isSubTelop)
		{
			gameObject = _subTelop;
		}
		if (gameObject != null)
		{
			gameObject.SetActive(isOn);
		}
	}
}
