// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// TooltipInfoCtrl
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TooltipInfoCtrl : MonoBehaviour
{
	private float _idle_time;

	private const float _IDLE_TIME_LIMIT = 2f;

	private GameObject _textTooltip;

	private GameObject _ToolTipInfoWindow;

	private const string _TOOLTIP_TEXT = "WASD: move, Mouse: look around, Space: jump, Shift: run, Tab: Create menu, Esc: menu, \n\nF3: normal Simulation speed, F4: double speed, F5: 20times speed";

	private void Awake()
	{
		_ToolTipInfoWindow = base.transform.Find("ToolTipInfoWindow").gameObject;
		_textTooltip = _ToolTipInfoWindow.transform.Find("txtTooltip").gameObject;
		_textTooltip.GetComponent<Text>().text = "WASD: move, Mouse: look around, Space: jump, Shift: run, Tab: Create menu, Esc: menu, \n\nF3: normal Simulation speed, F4: double speed, F5: 20times speed";
		ToggleTooltip();
	}

	private void SetTooltipText(string tooltipText)
	{
		if (_textTooltip != null)
		{
			_textTooltip.GetComponent<Text>().text = tooltipText;
		}
	}

	private void ToggleTooltip(bool isTooltipActive = false)
	{
		if (_ToolTipInfoWindow != null)
		{
			_ToolTipInfoWindow.SetActive(isTooltipActive);
		}
	}

	private void Update()
	{
		_idle_time += Time.deltaTime;
		if (Input.anyKey || Mouse.current.delta.x.ReadValue() != 0f || Mouse.current.delta.y.ReadValue() != 0f)
		{
			_idle_time = 0f;
			ToggleTooltip();
		}
		if (_idle_time > 2f)
		{
			ToggleTooltip(isTooltipActive: true);
		}
	}
}
