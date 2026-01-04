// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// GameTimerCtrl
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimerCtrl : MonoBehaviour
{
	public static GameTimerCtrl instance;

	public float _time;

	private double _time_stock;

	private float _buf_time;

	internal EventLoader _eventLoader = EventLoader.instance;

	internal bool _isPaused;

	private List<float> _eventTimeList = new List<float>();

	internal Dictionary<float, List<Dictionary<string, string>>> _timer_events = new Dictionary<float, List<Dictionary<string, string>>>();

	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private float _countdown_time = 300f;

	[SerializeField]
	private bool _countdown = true;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		if (_text == null)
		{
			_text = base.gameObject.GetComponent<TextMeshProUGUI>();
		}
	}

	private void SetTimeToText(float time)
	{
		if (_text != null)
		{
			string text = "";
			if (_countdown)
			{
				time = _countdown_time - time;
			}
			int num = Mathf.FloorToInt(time / 60f);
			int num2 = Mathf.FloorToInt(time - (float)(num * 60));
			int num3 = Mathf.FloorToInt((time - (float)(num * 60) - (float)num2) * 10f);
			text = $"{num:00}:{num2:00}.{num3:0}";
			_text.SetText(text);
		}
	}

	internal void SetTimerEvent()
	{
		if (!(_eventLoader != null))
		{
			return;
		}
		_timer_events = _eventLoader._timer_events;
		foreach (KeyValuePair<float, List<Dictionary<string, string>>> timer_event in _eventLoader._timer_events)
		{
			float key = timer_event.Key;
			_eventTimeList.Add(key);
		}
		_eventTimeList.Sort();
	}

	private bool ActionEvent(float time)
	{
		bool result = false;
		if (_eventLoader != null && _eventLoader._timer_events.TryGetValue(time, out var value))
		{
			foreach (Dictionary<string, string> item in value)
			{
				string value2 = "";
				string value3 = "";
				item.TryGetValue("event", out value2);
				item.TryGetValue("value", out value3);
				_eventLoader.ActionEvent(value2, value3);
			}
			return true;
		}
		return result;
	}

	private void Update()
	{
		if (_isPaused)
		{
			return;
		}
		_buf_time += Time.deltaTime;
		_time += Time.deltaTime;
		_time_stock += Time.timeAsDouble;
		if (_buf_time > 0.1f)
		{
			SetTimeToText(_time);
			_buf_time = 0f;
			if (_eventTimeList.Count > 0 && _time > _eventTimeList[0] && ActionEvent(_eventTimeList[0]))
			{
				_eventTimeList.RemoveAt(0);
			}
		}
	}
}
