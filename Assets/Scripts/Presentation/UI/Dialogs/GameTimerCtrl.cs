using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public class GameTimerCtrl : UIControllerBase
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
    internal float _countdown_time = 300; // [Sec]
    [SerializeField] bool _countdown = true; // カウントダウンモード

    /// <summary>
    /// GameTimerCtrl インスタンスを取得（キャッシング）
    /// GameObject.Find 負荷を軽減するため、複数呼び出し時に使用
    /// </summary>
    internal static GameTimerCtrl GetInstance()
    {
        if (instance == null)
        {
            GameObject txtGameTime = GameObject.Find("txtGameTime");
            if (txtGameTime != null)
            {
                instance = txtGameTime.GetComponent<GameTimerCtrl>();
            }
        }
        return instance;
    }

    protected override void Awake()
    {
        base.Awake();
        
        if (instance == null)
        {
            instance = this;
        }
        if (_text == null)
        {
            _text = this.gameObject.GetComponent<TextMeshProUGUI>();
        }
    }

    protected override IEnumerator Initialize()
    {
        // EventLoader の初期化待機（YAML ロード完了）
        int timeout = 0;
        
        while (!EventLoader.instance.IsInitialized && timeout < 100)
        {
            yield return new WaitForSeconds(0.1f);
            timeout++;
        }
        
        if (timeout >= 100)
        {
            Debug.LogWarning("[GameTimerCtrl.Initialize] EventLoader の初期化がタイムアウト (10秒以上)");
        }
        
        // イベント時間リストは StageYamlRepository -> EventLoader.SetEventToTimer() で設定済み
        // ここでは二重呼び出しを避けるため、SetTimerEvent() は呼ばない
        
        yield return null;
    }


    private void SetTimeToText(float time)
    {
        if (_text != null)
        {
            string text = "";
            bool isNegative = false;
            
            if (_countdown)
            {
                time = _countdown_time - time;
                
                // マイナス値の場合、フラグを立てて絶対値で計算
                if (time < 0)
                {
                    isNegative = true;
                    time = Mathf.Abs(time);
                }
            }

            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time - minutes * 60);
            int mseconds = Mathf.FloorToInt((time - minutes * 60 - seconds) * 10);

            text = string.Format("{0:00}:{1:00}.{2:0}", minutes, seconds, mseconds);
            
            // マイナスの場合は負号を付ける
            if (isNegative)
            {
                text = "-" + text;
            }
            
            _text.SetText(text);
        }
    }


    internal void SetTimerEvent()
    {
        if (_eventLoader != null)
        {
            _timer_events = _eventLoader._timer_events;
            foreach (var gevent in _eventLoader._timer_events)
            {
                float event_time = gevent.Key;
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
        if (_eventLoader != null)
        {
            if (_eventLoader._timer_events.TryGetValue(time, out event_data_list))
            {
                foreach (var event_data in event_data_list)
                {
                    string event_name = "";
                    string event_value = "";
                    event_data.TryGetValue("event", out event_name);
                    event_data.TryGetValue("value", out event_value);
                    
                    _eventLoader.ActionEvent(event_name, event_value);
                }
                
                return true;
            }
        }

        return ret;
    }

    void Update()
    {
        // [重要] システム全体の初期化が完了するまでタイマーを進めない
        // InitializationManager が全コンポーネント初期化完了まで待機
        if (!InitializationManager.IsInitialized)
        {
            return;
        }

        // [重要] GameTimerCtrl 自身の初期化も確認
        if (!IsInitialized)
        {
            return;
        }

        // 年サイクルステージの初回セットアップ（Season 3）
        TrySetupYearCycle();

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
                    if (ActionEvent(_eventTimeList[0]))
                    {
                        _eventTimeList.RemoveAt(0);
                    }
                }
            }

            CheckYearTimeUp();
        }
    }

    /// <summary>
    /// 年サイクルステージの初回セットアップ（Season 3）
    /// 全初期化完了後の最初の Update で 1 回だけ実行される。
    /// 年に依存しない初期設定イベント（天候・風等）を即時発火してから
    /// Year 1 の配置フェーズ（タイマー停止）に入る
    /// </summary>
    private void TrySetupYearCycle()
    {
        if (YearCycleSystem.IsActive())
        {
            return;
        }

        if (_eventLoader == null)
        {
            _eventLoader = EventLoader.instance;
        }

        if (_eventLoader == null || !_eventLoader.HasYearEvents())
        {
            return;
        }

        FireAllPendingEventsNow();
        YearCycleSystem.InitializeForStage(_eventLoader);
        _isPaused = true;
        _time = 0.0f;
        SetTimeToText(_time);
    }

    /// <summary>
    /// 発火待ちのタイマーイベントをすべて即時実行する（年サイクルの初期設定用）
    /// </summary>
    private void FireAllPendingEventsNow()
    {
        foreach (float eventTime in _eventTimeList)
        {
            ActionEvent(eventTime);
        }
        _eventTimeList.Clear();
    }

    /// <summary>
    /// 現在年を開始する（Start Year ボタン → YearPanelController から呼ばれる）
    /// 年イベントをタイマーに積み直し、duration をカウントダウンに設定して進行再開
    /// </summary>
    internal bool StartYearCycle()
    {
        float yearDuration = YearCycleSystem.StartYear();
        if (yearDuration <= 0f)
        {
            return false;
        }

        _eventTimeList.Clear();
        SetTimerEvent();  // EventLoader.LoadYearEvents 済みの _timer_events から再構築

        _time = 0.0f;
        _buf_time = 0.0f;
        _countdown_time = yearDuration;
        SetTimeToText(_time);
        _isPaused = false;
        return true;
    }

    /// <summary>
    /// 年の duration 経過を検出して年末処理へ渡す（Season 3）
    /// </summary>
    private void CheckYearTimeUp()
    {
        if (YearCycleSystem.CurrentPhase != YearCyclePhase.YearRunning)
        {
            return;
        }

        if (_time < _countdown_time)
        {
            return;
        }

        _isPaused = true;
        YearCycleSystem.OnYearTimeUp();
    }
}
