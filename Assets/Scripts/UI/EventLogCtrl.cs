using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventLogCtrl : MonoBehaviour
{
    private const float _FADE_TIME_INTERVAL = 0.1f;
    private const float _FADE_OUT_ALPHA = 0.06f;
    private const float _FADE_INTERVAL_ALPHA = 0.01f;
    // private const float _FADE_INTERVAL_ALPHA = 0.1f;
    private const float _DEFAULT_ALPHA = 0.9f;
    private const int _MAX_LOG_LINES = 12;
    private const int _MAX_LOG_LINE_LENGTH = 20;
    
    private float _start_time = 0f;
    private float _window_alpha = 0.5f;
    private GameObject _eventLog;
    private GameObject _txtLog;
    // private int _i = 0;

    public static EventLogCtrl Instance { get; private set; }

    private bool SetWindowAlpha(float alpha)
    {
        _window_alpha = alpha;
        if (_window_alpha <= _FADE_OUT_ALPHA)
        {
            ToggleWindow(isOn: false);
            // _i++;
            // ShowEventLog(_i  + "Log");
        }
        _eventLog.GetComponent<Image>().color = new Color(0f, 0f, 0f, _window_alpha);
        _txtLog.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, _window_alpha);
        return true;
    }

    private void ToggleWindow(bool isOn = true)
    {
        _eventLog.SetActive(isOn);
    }

    internal void ShowEventLog(string message)
    {
        ToggleWindow(isOn: false);
        TextMeshProUGUI txtMpro = _txtLog.GetComponent<TextMeshProUGUI>();
        
        // message の長さ調節
        if (message.Length > _MAX_LOG_LINE_LENGTH)
        {
            message = message.Substring(0, _MAX_LOG_LINE_LENGTH) + "...";
        }
        txtMpro.text += message + Environment.NewLine;

        // txtMpro.text が 6行以上開業コードを含むようになったら古い行を削除
        string[] lines = txtMpro.text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        if (lines.Length > _MAX_LOG_LINES)
        {
            txtMpro.text = string.Join(Environment.NewLine, lines.Skip(lines.Length - _MAX_LOG_LINES));
        }

        SetWindowAlpha(_DEFAULT_ALPHA);
        ToggleWindow();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        _eventLog = this.transform.Find("EventLog").gameObject;
        _txtLog = _eventLog.transform.Find("txtLog").gameObject;
        TextMeshProUGUI txtMpro = _txtLog.GetComponent<TextMeshProUGUI>();
        txtMpro.text = "";
        SetWindowAlpha(_DEFAULT_ALPHA);
        ToggleWindow(isOn: false);
    }

    void Start()
    {
        // ShowEventLog("EventLogCtrl Start test");
    }

    void Update()
    {
        if (Time.time - _start_time > _FADE_TIME_INTERVAL && _window_alpha > _FADE_OUT_ALPHA)
        {
            _start_time = Time.time;
            SetWindowAlpha(_window_alpha - _FADE_INTERVAL_ALPHA);
        }
    }
}
