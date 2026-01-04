// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// NoticeCtrl
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NoticeCtrl : MonoBehaviour
{
	private GameObject _UINotice;

	private GameObject _notice_window;

	private GameObject _txtNotice;

	internal void ShowNotice(string notice)
	{
		if (_txtNotice != null)
		{
			_txtNotice.GetComponent<Text>().text = notice;
			ToggleNoticeWindow(isOn: true);
		}
	}

	private void Awake()
	{
		_UINotice = base.gameObject;
		_notice_window = _UINotice.transform.Find("noticeWindow").gameObject;
		_txtNotice = _notice_window.transform.Find("txtNotice").gameObject;
		ToggleNoticeWindow(isOn: false);
	}

	public bool IsNoticeWindowActive()
	{
		return _notice_window.activeSelf;
	}

	public void ToggleNoticeWindow(bool isOn)
	{
		if (_notice_window != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
			_notice_window.SetActive(isOn);
		}
		Time.timeScale = ((!isOn) ? 1 : 0);
	}
}
