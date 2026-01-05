// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// EventLogCtrl
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

	private const float _DEFAULT_ALPHA = 0.9f;

	private const int _MAX_LOG_LINES = 12;

	private const int _MAX_LOG_LINE_LENGTH = 20;

	private float _start_time;

	private float _window_alpha = 0.5f;

	private GameObject _eventLog;

	private GameObject _txtLog;

	public static EventLogCtrl Instance { get; private set; }

	private bool SetWindowAlpha(float alpha)
	{
		_window_alpha = alpha;
		if (_window_alpha <= 0.06f)
		{
			ToggleWindow(isOn: false);
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
		TextMeshProUGUI component = _txtLog.GetComponent<TextMeshProUGUI>();
		if (message.Length > 20)
		{
			message = message.Substring(0, 20) + "...";
		}
		component.text = component.text + message + Environment.NewLine;
		string[] array = component.text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
		if (array.Length > 12)
		{
			component.text = string.Join(Environment.NewLine, array.Skip(array.Length - 12));
		}
		SetWindowAlpha(0.9f);
		ToggleWindow();
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		_eventLog = base.transform.Find("EventLog").gameObject;
		_txtLog = _eventLog.transform.Find("txtLog").gameObject;
		_txtLog.GetComponent<TextMeshProUGUI>().text = "";
		SetWindowAlpha(0.9f);
		ToggleWindow(isOn: false);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (Time.time - _start_time > 0.1f && _window_alpha > 0.06f)
		{
			_start_time = Time.time;
			SetWindowAlpha(_window_alpha - 0.01f);
		}
	}
}
