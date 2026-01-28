using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TooltipInfoCtrl : MonoBehaviour
{
    private float _idle_time = 0f;
    private const float _IDLE_TIME_LIMIT = 2f;

    private GameObject _textTooltip;
    // private GameObject _UIRightBottom;
    private GameObject _ToolTipInfoWindow;

    void Awake()
    {
        // UIRightBottom
        //
        // _UIRightBottom = this.transform.parent.gameObject;
        // _ToolTipInfoWindow = this.transform.gameObject;
        _ToolTipInfoWindow = this.transform.Find("ToolTipInfoWindow").gameObject;
        _textTooltip = _ToolTipInfoWindow.transform.Find("txtTooltip").gameObject;
        // _textTooltip.GetComponent<Text>().text = "";
        _textTooltip.GetComponent<Text>().text = "WASD: move, Mouse: look around, Space: jump, Shift: run, Tab: Create menu, Esc: menu";
        ToggleTooltip(false);
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

    // Update is called once per frame
    void Update()
    {
        _idle_time += Time.deltaTime;
        // マウスが動いたかチェック
        if (Keyboard.current.anyKey.isPressed || Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            _idle_time = 0f;
            ToggleTooltip(false);
        }

        if (_idle_time > _IDLE_TIME_LIMIT)
        {
            ToggleTooltip(true);
            // _time = new Time();
        }

    }
}
