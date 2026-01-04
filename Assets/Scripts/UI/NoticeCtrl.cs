using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NoticeCtrl : MonoBehaviour
{
    GameObject _UINotice = null;
    GameObject _notice_window = null;

    internal void ShowNotice(string notice)
    {
        GameObject txtNotice = _notice_window.transform.Find("txtNotice").gameObject;
        // Debug.Log(txtNotice.name);
        if (txtNotice != null)
        {
            txtNotice.GetComponent<Text>().text = notice;
            ToggleNoticeWindow(true);
        }
        // _UINotice.SetActive(true);

    }

    // private void OnClickPanel()
    // {
     
    //     _UINotice.SetActive(false);
    // }


    void Awake()
    {
        _UINotice = this.gameObject;
        _notice_window = _UINotice.transform.Find("noticeWindow").gameObject;
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
