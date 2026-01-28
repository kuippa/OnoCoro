using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

public class MessageBoxCtrl : UIControllerBase
{
    private GameObject _messageBox = null;
    private TextMeshProUGUI _txtMessage = null;
    private Action<bool> _callback;

    private void SetMessage(string message)
    {
        _txtMessage.text = message;
    }

    private void OnButtonClick(bool result)
    {
        Debug.Log($"Button clicked: {result}");        
        _callback?.Invoke(result);
        ToggleMsgBox(false);
    }

    private void ToggleMsgBox(bool isOn)
    {
        _messageBox.SetActive(isOn);
    }

    internal void Show(string message, Action<bool> onResult)
    {
        SetMessage(message);
        _callback = onResult;
        ToggleMsgBox(true);
    }

    protected override void Awake()
    {
        base.Awake();
        
        _messageBox = this.gameObject.transform.Find("MessageBox").gameObject;
        Button btnOK = _messageBox.transform.Find("btnOK").gameObject.GetComponent<Button>();
        Button btnCancel = _messageBox.transform.Find("btnCancel").gameObject.GetComponent<Button>();
        _txtMessage = _messageBox.transform.Find("txtMessage").gameObject.GetComponent<TextMeshProUGUI>();
        btnOK.onClick.AddListener(() => OnButtonClick(true));
        btnCancel.onClick.AddListener(() => OnButtonClick(false));
        ToggleMsgBox(false);
    }

    protected override IEnumerator Initialize()
    {
        yield return null;
    }
}
