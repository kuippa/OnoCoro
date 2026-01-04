using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using UnityEngine.Rendering;
using CommonsUtility;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class EventLoader : MonoBehaviour
{
    public static EventLoader instance = null;

    // internal Dictionary<string, Dictionary<string, string>[]> _events = new Dictionary<string, Dictionary<string, string>[]>();
    // // ex. {notice,{time:10,value:"地震が発生しました。"}}
    // // イベント名,{イベントデータ配列}

    // internal Dictionary<float, Dictionary<string, string>> _timer_events = new Dictionary<float, Dictionary<string, string>>();
    // ex. {イベント発生時刻,{event: earthquake,value: 6}}

    internal Dictionary<float, List<Dictionary<string, string>>> _timer_events = new Dictionary<float, List<Dictionary<string, string>>>();


    internal Dictionary<string, string> _board_data = new Dictionary<string, string>();


    private GameTimerCtrl _gameTimerCtrl = null;

    // internal void debug_events()
    // {
    //     foreach (var gevent in _events)
    //     {
    //         string event_name = gevent.Key;
    //         Debug.Log(gevent.Key);
    //         foreach (var event_data in _events[event_name])
    //         {
    //             foreach (var entry in event_data)
    //             {
    //                 Debug.Log(entry.Key + " : " + entry.Value);
    //             }
    //         }
    //     }

    //     //  10秒後にイベントを実行
    //     Invoke("testInvoke", 10.0f);
    // }

    internal void InitBoardData()
    {
        if (_board_data.Count > 0)
        {

            // BoardData読み込み済か確認
            // Debug.Log("BoardData読み込み済" + _board_data.First().Key + " : " + _board_data.First().Value);
            // Debug.Log("BoardData読み込み済LAST" + _board_data.Last().Key + " : " + _board_data.Last().Value);
            // GamePrefabs

        }
        // _board_data.Add(board_name, board_text);

    }

    internal string GetBoardText(string board_code)
    {
        string returndata = "";
        if (_board_data.Count > 0)
        {
            if (_board_data.ContainsKey(board_code))
            {
                // Debug.Log("GetBoardText " + board_code + " : " + _board_data[board_code]);
                returndata = _board_data[board_code];
            }
        }
        return returndata;
    }

    internal void SetEventToTimer()
    {
        if (_gameTimerCtrl != null)
        {
            // _gameTimerCtrl._time = 0.0f;
            _gameTimerCtrl._eventLoader = instance;
            _gameTimerCtrl.SetTimerEvent();
        }
    }

    internal void ActionEvent(string event_name, string event_value)
    {
        Debug.Log("ActionEvent " + event_name + " : " + event_value);

        // TODO:
        // event_name の値によって処理をswitch分岐
        switch (event_name)
        {
            case "spawn_unit":
                SpawnUnit(event_value);
                break;
                //
            case "notice":
                ShowNotice(event_value);
                break;
            case "earthquake":
                CallEarthquake(event_value);
                break;
            case "building_break":
                CallBuildingBreak(event_value);
                break;
            case "telop":
                CallTelopShow(event_value);
                break;
            case "subtelop":
                CallTelopShow(event_value, true);
                break;
            case "volcano":
                break;
            default :
                Debug.Log("ActionEvent: else");
                break;
        }
    }


    private Vector3 tryGetPosition(string event_value)
    {
        Vector3 position = new Vector3(0, 0, 0);
        if (event_value.Contains(","))
        {
            position = new Vector3(float.Parse(event_value.Split(',')[1]), float.Parse(event_value.Split(',')[2]), float.Parse(event_value.Split(',')[3]));
        }
        return position;
    }

    private void SpawnUnit(string event_value)
    {

        // CallUnitByName
        // 召喚位置を value から取得
        // event_value を カンマで分割
        // 0: unit_name
        // 1: position_x
        // 2: position_y
        // 3: position_z
        
        string unit_name = event_value.Split(',')[0];
        Vector3 position = tryGetPosition(event_value);
        
        Debug.Log("SpawnUnit " + unit_name + " : " + position);

        // TODO: ClickCtrlのGetSpawnCtrlと統合

        SpawnCtrl spawnCtrl = null;
        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem");
        }
        spawnCtrl = eventSystem.transform.GetComponent<SpawnCtrl>();
        if (spawnCtrl == null)
        {
            spawnCtrl = eventSystem.transform.AddComponent<SpawnCtrl>();
        }
        // eventSystem.transform.AddComponent<SpawnCtrl>();
// 
        spawnCtrl.CallUnitByName(unit_name, position);


        // GameObject UINotice = GameObject.Find("UINotice");
        // if (UINotice != null)
        // {
        //     UINotice.GetComponent<NoticeCtrl>().ShowNotice(event_value);
        // }
    }

    private void ShowNotice(string event_value)
    {
        GameObject UINotice = GameObject.Find("UINotice");
        if (UINotice != null)
        {
            UINotice.GetComponent<NoticeCtrl>().ShowNotice(event_value);
        }
    }

    private void CallEarthquake(string event_value)
    {
        GameObject EventSystem = GameObject.Find("EventSystem");
        if (EventSystem != null)
        {
            Earthquake earthquake = EventSystem.GetComponent<Earthquake>();
            if (earthquake == null)
            {
                earthquake = EventSystem.AddComponent<Earthquake>();
            }
            earthquake.EventEarthQuake(float.Parse(event_value));
        }
    }

    private void CallBuildingBreak(string event_value)
    {
        GameObject EventSystem = GameObject.Find("EventSystem");
        if (EventSystem != null)
        {
            BuildingBreak buildingBreak = EventSystem.GetComponent<BuildingBreak>();
            if (buildingBreak == null)
            {
                buildingBreak = EventSystem.AddComponent<BuildingBreak>();
            }
            buildingBreak.EventBreakBuilding(event_value);
        }
    }

    private void CallTelopShow(string event_value, bool isSubTelop = false)
    {
        TelopCtrl telopCtrl = GameObject.Find("UITelop").GetComponent<TelopCtrl>();
        if (telopCtrl != null)
        {
            telopCtrl.GetComponent<TelopCtrl>().ShowTelop(event_value, isSubTelop);
        }
    }



    // private void testInvoke()
    // {
    //     float tes = GameTimerCtrl.instance._time;
    //     Debug.Log(tes);
    // }

    // private float GetGameTime()
    // {

    //     if (_gameTimerCtrl != null)
    //     {
    //         return _gameTimerCtrl._time = 0.0f;
    //     }
    //     return 0.0f;
    // }


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        // _time へのアクセス
        GameObject txtGameTime = GameObject.Find("txtGameTime");
        if (txtGameTime != null)
        {
            _gameTimerCtrl = txtGameTime.GetComponent<GameTimerCtrl>();
            // txtGameTime.GetComponent<GameTimerCtrl>()._time = 0.0f;
        }

    }

}

