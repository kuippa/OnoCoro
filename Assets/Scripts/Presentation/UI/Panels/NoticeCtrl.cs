using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CommonsUtility;

/// <summary>
/// 通知ポップアップコントローラー
/// UIControllerBase を継承し、初期化フラグで状態を管理
/// </summary>
public class NoticeCtrl : UIControllerBase
{
    private GameObject _UINotice = null;
    private GameObject _notice_window = null;
    private GameObject _txtNotice = null;

    /// <summary>
    /// 参照取得と初期非表示
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        
        _UINotice = this.gameObject;
        _notice_window = _UINotice.transform.Find("noticeWindow").gameObject;
        _txtNotice = _notice_window.transform.Find("txtNotice").gameObject;

        // [重要] 初期非表示を Awake で実行
        ToggleNoticeWindow(false);
    }

    /// <summary>
    /// Initialize コルーチン
    /// </summary>
    protected override IEnumerator Initialize()
    {
        // 初期化処理なし
        yield return null;
    }

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
