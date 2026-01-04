// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MessageBoxCtrl
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxCtrl : MonoBehaviour
{
	private GameObject _messageBox;

	private TextMeshProUGUI _txtMessage;

	private Action<bool> _callback;

	private void SetMessage(string message)
	{
		_txtMessage.text = message;
	}

	private void OnButtonClick(bool result)
	{
		Debug.Log($"Button clicked: {result}");
		_callback?.Invoke(result);
		ToggleMsgBox(isOn: false);
	}

	private void ToggleMsgBox(bool isOn)
	{
		_messageBox.SetActive(isOn);
	}

	internal void Show(string message, Action<bool> onResult)
	{
		SetMessage(message);
		_callback = onResult;
		ToggleMsgBox(isOn: true);
	}

	private void initWindowState()
	{
		_messageBox = base.gameObject.transform.Find("MessageBox").gameObject;
		Button component = _messageBox.transform.Find("btnOK").gameObject.GetComponent<Button>();
		Button component2 = _messageBox.transform.Find("btnCancel").gameObject.GetComponent<Button>();
		_txtMessage = _messageBox.transform.Find("txtMessage").gameObject.GetComponent<TextMeshProUGUI>();
		component.onClick.AddListener(delegate
		{
			OnButtonClick(result: true);
		});
		component2.onClick.AddListener(delegate
		{
			OnButtonClick(result: false);
		});
		ToggleMsgBox(isOn: false);
	}

	private void Awake()
	{
		initWindowState();
	}
}
