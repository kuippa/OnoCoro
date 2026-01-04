using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// txtGameTime にアタッチされている


public class GameTimerCtrl : MonoBehaviour
{
    public static GameTimerCtrl instance = null;
    public float _time = 0.0f;
    double _time_stock = 0.0f;
    float _buf_time = 0.0f;

    // internal EventLoader _eventLoader = null;
    internal EventLoader _eventLoader = EventLoader.instance;
    internal bool _isPaused = false; // 一時停止中かどうか

    // イベント発生時間リスト
    private List<float> _eventTimeList = new List<float>(); 
    // 時間形式のイベントリスト
    // internal Dictionary<float, Dictionary<string, string>> _timer_events = new Dictionary<float, Dictionary<string, string>>();
    internal Dictionary<float, List<Dictionary<string, string>>> _timer_events = new Dictionary<float, List<Dictionary<string, string>>>();


    [SerializeField] TextMeshProUGUI _text = null;
    [SerializeField] float _countdown_time = 300; // [Sec]
    [SerializeField] bool _countdown = true; // カウントダウンモード


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            // Destroy(this.gameObject);
        }

        if (_text == null)
        {
            _text = this.gameObject.GetComponent<TextMeshProUGUI>();
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
                // _time_stock = _countdown_time - _time_stock;
            }

            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time - minutes * 60);
            int mseconds = Mathf.FloorToInt((time - minutes * 60 - seconds) * 10);

            text = string.Format("{0:00}:{1:00}.{2:0}", minutes, seconds, mseconds);

            // if (this.name == "txtGameTime")
            // {
            //     text = string.Format("{0:00}:{1:00}.{2:0}", minutes, seconds, mseconds);
            // }
            // else
            // {
            //     text = String.Format("{00:00:00.0}", _time_stock);
            // }
            _text.SetText(text);
        }
    }

    // internal void SetEvent()
    // {
    //     if (_eventLoader != null)
    //     {
    //         foreach (var gevent in _eventLoader._events)
    //         {
    //             string event_name = gevent.Key;
    //             Debug.Log(gevent.Key);
    //             foreach (var event_data in _eventLoader._events[event_name])
    //             {
    //                 foreach (var entry in event_data)
    //                 {
    //                     Debug.Log(entry.Key + " : " + entry.Value);

    //                     if (entry.Key == "time")
    //                     {
    //                         Debug.Log(entry.Value);
    //                         _eventTimeList.Add(float.Parse(entry.Value));
    //                     }
    //                 }
    //             }
    //         }
    //         // _eventTimeList を時間でソートする
    //         _eventTimeList.Sort();
    //     }
    // }

    internal void SetTimerEvent()
    {
        if (_eventLoader != null)
        {
            _timer_events = _eventLoader._timer_events;
            foreach (var gevent in _eventLoader._timer_events)
            {
                float event_time = gevent.Key;
                // Debug.Log(gevent.Key);
                // Dictionary<string, string>[] event_data = gevent.Value;
                _eventTimeList.Add(event_time);
            }
            // _eventTimeList を時間でソートする
            _eventTimeList.Sort();
        }
    }


    private bool ActionEvent(float time)
    {
        bool ret = false;
        List<Dictionary<string, string>> event_data_list;
        // Dictionary<string, string> event_data;
        // その時刻に発生するイベントを実行する
        if (_eventLoader != null)
        {
            // if (_eventLoader._timer_events.TryGetValue(time, out event_data))
            if (_eventLoader._timer_events.TryGetValue(time, out event_data_list))
            {
                foreach (var event_data in event_data_list)
                {
                    // Debug.Log("ActionEventイベント発生:" + time + event_data);
                    string event_name = "";
                    string event_value = "";
                    event_data.TryGetValue("event", out event_name);
                    event_data.TryGetValue("value", out event_value);
                    _eventLoader.ActionEvent(event_name, event_value);
                }
                // string event_name = "";
                // string event_value = "";
                // event_data.TryGetValue("event", out event_name);
                // event_data.TryGetValue("value", out event_value);
                // _eventLoader.ActionEvent(event_name, event_value);
                return true;
            }
        }

        return ret;
    }

    void Update()
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
            _buf_time = 0.0f;

            // _eventTimeList の時間を超えた場合、イベントを実行する
            if (_eventTimeList.Count > 0)
            {
                if (_time > _eventTimeList[0])
                {
                    // Debug.Log("イベント発生" + _eventTimeList[0]);
                    if (ActionEvent(_eventTimeList[0]))
                    {
                        _eventTimeList.RemoveAt(0);
                    }
                }
            }
        }
    }
}
