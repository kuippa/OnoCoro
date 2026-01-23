using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class MessageBoxCtrl : MonoBehaviour
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

    void Awake()
    {
        _messageBox = this.gameObject.transform.Find("MessageBox").gameObject;
        Button btnOK = _messageBox.transform.Find("btnOK").gameObject.GetComponent<Button>();
        Button btnCancel = _messageBox.transform.Find("btnCancel").gameObject.GetComponent<Button>();
        _txtMessage = _messageBox.transform.Find("txtMessage").gameObject.GetComponent<TextMeshProUGUI>();
        btnOK.onClick.AddListener(() => OnButtonClick(true));
        btnCancel.onClick.AddListener(() => OnButtonClick(false));
        ToggleMsgBox(false);
    }

    void Start()
    {
        // MessageBoxCtrl messageBox = this.gameObject.GetComponent<MessageBoxCtrl>();
        // messageBox.Show("Hello, World!", (result) => 
        // {
        //     if (result)
        //     {
        //         Debug.Log("Yesが選択されました");
        //     }
        //     else
        //     {
        //         Debug.Log("Noが選択されました");
        //     }
        // });
    }

    void Update()
    {
        
    }
}
