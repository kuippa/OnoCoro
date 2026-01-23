using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NoticeCtrl : MonoBehaviour
{
    private GameObject _UINotice = null;
    private GameObject _notice_window = null;
    private GameObject _txtNotice = null;

    internal void ShowNotice(string notice)
    {
        if (_txtNotice != null)
        {
            _txtNotice.GetComponent<Text>().text = notice;
            ToggleNoticeWindow(true);
        }
    }

    // private void OnClickPanel()
    // {
     
    //     _UINotice.SetActive(false);
    // }

    void Awake()
    {
        //    Debug.Log($"NoticeCtrl Awake called on {gameObject.name} (InstanceID: {gameObject.GetInstanceID()})");
        _UINotice = this.gameObject;
        // Debug.Log("NoticeCtrl Awake" + this.gameObject.name);
        // if (_UINotice.transform.Find("noticeWindow") == null)
        // {
        //     Debug.Log("NoticeCtrl noticeWindow is null");
        //     return;
        // }

        _notice_window = _UINotice.transform.Find("noticeWindow").gameObject;
        _txtNotice = _notice_window.transform.Find("txtNotice").gameObject;

        // _UINotice.SetActive(false);
        ToggleNoticeWindow(false);

        // GameObject noticeWindow = _UINotice.transform.Find("noticeWindow").gameObject;
        // noticeWindow.GetComponent<Button>().onClick.AddListener(OnClickPanel);
        // ShowNotice("NoticeCtrl Awake");
    }

    public bool IsNoticeWindowActive()
    {
        return _notice_window.activeSelf;
    }


    public void ToggleNoticeWindow(bool isOn)
    {
        if (_notice_window != null)
        {
            // buttonのselectedを解除する
            EventSystem.current.SetSelectedGameObject(null);
            _notice_window.SetActive(isOn);
        }
        // ゲーム内時間を一時停止
        Time.timeScale = isOn ? 0 : 1;
    }


}
