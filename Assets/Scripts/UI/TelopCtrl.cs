using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections; // 'IEnumerator' を使用するために 'System.Collections' を追加

public class TelopCtrl : MonoBehaviour
{
    private GameObject _UITelop = null;
    private GameObject _telop = null;
    private GameObject _txtTelop = null;
    private GameObject _subTelop = null;
    private GameObject _txtsubTelop = null;

    private const float _TELOP_DISPLAY_TIME = 2.5f;
    private const float _SUB_TELOP_DISPLAY_TIME = 2.5f;
    private float _telopDisplayTime = 0.0f;

    void Awake()
    {
        // Debug.Log("TelopCtrl Awake");
        _UITelop = this.gameObject;
        _telop = _UITelop.transform.Find("Telop").gameObject;   
        _txtTelop = _telop.transform.Find("txtTelop").gameObject;
        _subTelop = _UITelop.transform.Find("subTelop").gameObject;
        _txtsubTelop = _subTelop.transform.Find("txtSubTelop").gameObject;

        ToggleTelop(false);
        ToggleTelop(false, true);
        // ShowTelop("TelopCtrl Awake test");
        // ShowTelop("TelopCtrl Awake sub test", true);
    }

    internal void ShowTelop(string telop, bool isSubTelop = false)
    {
        GameObject targetTelop = _telop;
        GameObject txtObj = _txtTelop;
        _telopDisplayTime = _TELOP_DISPLAY_TIME;
        if (isSubTelop)
        {
            targetTelop = _subTelop;
            txtObj = _txtsubTelop;
            _telopDisplayTime = _SUB_TELOP_DISPLAY_TIME;
        }
        if (txtObj != null)
        {
            TextMeshProUGUI txtMpro = txtObj.GetComponent<TextMeshProUGUI>();
            txtMpro.text = telop;
            ToggleTelop(true, isSubTelop);
            StartCoroutine(InvokeWithDelay(_telopDisplayTime, () => ToggleTelop(false, isSubTelop)));
        }
    }

    private IEnumerator InvokeWithDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    private void ToggleTelop(bool isOn, bool isSubTelop = false)
    {
        GameObject targetTelop = _telop;
        if (isSubTelop)
        {
            targetTelop = _subTelop;
        }
        if (targetTelop != null)
        {
            targetTelop.SetActive(isOn);
        }
    }

    // void Update()
    // {
    //     if (_telopDisplayTime > 0.0f)
    //     {
    //         _telopDisplayTime -= Time.deltaTime;
    //         if (_telopDisplayTime <= 0.0f)
    //         {
    //             ToggleTelop(false);
    //         }
    //     }
    // }

}
