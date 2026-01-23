using System.Collections.Generic;
using System.Linq;
using CommonsUtility;
using UnityEngine;

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

        switch (event_name)
        {
            case "spawn_unit":
                SpawnUnit(event_value);
                break;
            case "spawn_enemy_unit":
                SpawnEnemyUnit(event_value);
                break;
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
                CallTelopShow(event_value, isSubTelop: true);
                break;
            case "wind":
                CallWind(event_value);
                break;
            case "weather":
                CallWeather(event_value);
                break;
            case "solar":
                CallSolar(event_value);
                break;
            case "watersurface":
                CallWaterSurface(event_value);
                break;
            case "bloom_path":
                CallBloomPath(event_value);
                break;
            case "off_bloom_path":
                CallBloomPath(event_value, isBloom: false);
                break;
            case "volcano":
                break;
            default:
                Debug.Log("ActionEvent: else");
                break;
        }
    }

    private Vector3 tryGetPosition(string event_value)
    {
        Vector3 result = new Vector3(0f, 0f, 0f);
        
        if (string.IsNullOrEmpty(event_value))
        {
            return result;
        }
        
        string[] array = event_value.Split(',');
        
        if (array.Length >= 3)
        {
            float result2 = 0f;
            float x = 0f;
            if (float.TryParse(array[0], out result2))
            {
                x = result2;
            }
            
            float result3 = 0f;
            float y = 0f;
            if (float.TryParse(array[1], out result3))
            {
                y = result3;
            }
            
            float result4 = 0f;
            float z = 0f;
            if (float.TryParse(array[2], out result4))
            {
                z = result4;
            }
            
            result = new Vector3(x, y, z);
        }
        else
        {
            event_value = event_value.Trim();
            
            if (event_value == "random_position")
            {
                return DemController.GetDemRndAbovePosition(10f);
            }
            
            if (event_value == "random_doom_building")
            {
                PlateauBuildingInteractor component = GameObject.Find("Plateau").GetComponent<PlateauBuildingInteractor>();
                if (component != null)
                {
                    List<GameObject> doomedBuildings = component._doomedBuildings;
                    if (doomedBuildings.Count > 0)
                    {
                        int index = Random.Range(0, doomedBuildings.Count);
                        Renderer component2 = doomedBuildings[index].GetComponent<Renderer>();
                        Vector3 center = component2.bounds.center;
                        Vector3 extents = component2.bounds.extents;
                        return center + extents;
                    }
                }
            }
        }
        
        return result;
    }

    private string TryGetCol0(string event_value)
    {
        string result = "";
        
        if (string.IsNullOrEmpty(event_value))
        {
            return result;
        }
        
        string[] array = event_value.Split(',');
        if (array.Length >= 1)
        {
            result = array[0];
        }
        
        return result;
    }

    private string TryGetColValue(string event_value)
    {
        string result = "";
        
        if (string.IsNullOrEmpty(event_value))
        {
            return result;
        }
        
        string[] array = event_value.Split(',');
        if (array.Length >= 2)
        {
            result = string.Join(",", array.Skip(1).ToArray());
        }
        
        return result;
    }

    private void SpawnUnit(string event_value)
    {
        string unit_name = TryGetCol0(event_value);
        Vector3 spawnPoint = tryGetPosition(TryGetColValue(event_value));
        
        GameObject eventSystem = GameObjectTreat.GetEventSystem();
        SpawnCtrl spawnCtrl = eventSystem.GetComponent<SpawnCtrl>();
        if (spawnCtrl == null)
        {
            spawnCtrl = eventSystem.AddComponent<SpawnCtrl>();
        }
        
        spawnCtrl.CallUnitByName(unit_name, spawnPoint);
        EventLogCtrl.Instance.ShowEventLog("SpawnUnit:" + unit_name);
    }

    private void SpawnEnemyUnit(string event_value)
    {
        string unit_name = TryGetCol0(event_value);
        string[] marker_names = event_value.Split(',').Skip(1).ToArray();
        
        GameObject eventSystem = GameObjectTreat.GetEventSystem();
        SpawnCtrl spawnCtrl = eventSystem.GetComponent<SpawnCtrl>();
        if (spawnCtrl == null)
        {
            spawnCtrl = eventSystem.AddComponent<SpawnCtrl>();
        }
        
        spawnCtrl.CallEnemyUnitByName(unit_name, marker_names);
        EventLogCtrl.Instance.ShowEventLog("SpawnEnemyUnit:" + unit_name);
    }

    private void ShowNotice(string event_value)
    {
        GameObject UINotice = GameObject.Find("UINotice");
        if (UINotice != null)
        {
            UINotice.GetComponent<NoticeCtrl>().ShowNotice(event_value);
        }
    }

    private void CallWind(string event_value)
    {
        float windSpeed = float.Parse(TryGetCol0(event_value));
        float windDirection = float.Parse(TryGetColValue(event_value));
        
        WindCtrl.SetWindSpeed(windSpeed);
        WindCtrl.SetWindDirection(windDirection);
    }

    private void CallWeather(string event_value)
    {
        WeatherCtrl orAddComponent = GameObjectTreat.GetOrAddComponent<WeatherCtrl>(GameObjectTreat.GetEventSystem());
        string weather_type = TryGetCol0(event_value);
        string weather_params = TryGetColValue(event_value);
        string[] array = weather_params.Split(',');
        
        if (array.Length == 3)
        {
            float strength = float.Parse(array[0]);
            float cloudStrength = float.Parse(array[1]);
            float fogStrength = float.Parse(array[2]);
            
            orAddComponent.ChangeWeather(strength, cloudStrength, fogStrength);
            
            if (weather_type == "snow")
            {
                orAddComponent.ChangeSnow();
            }
        }
        else
        {
            Debug.Log("CallWeather: else " + weather_type + ":" + weather_params);
        }
    }

    private void CallSolar(string event_value)
    {
        GameObjectTreat.GetOrAddComponent<WeatherCtrl>(GameObjectTreat.GetEventSystem()).ChangeSolarAltitude(float.Parse(event_value));
    }

    private void CallWaterSurface(string event_value)
    {
        GameObjectTreat.GetOrAddComponent<WaterSurfaceCtrl>(GameObjectTreat.GetEventSystem()).SetWaterSurfaceHeight(float.Parse(event_value));
    }

    private void CallEarthquake(string event_value)
    {
        GameObject eventSystem = GameObjectTreat.GetEventSystem();
        Earthquake earthquake = eventSystem.GetComponent<Earthquake>();
        if (earthquake == null)
        {
            earthquake = eventSystem.AddComponent<Earthquake>();
        }
        
        earthquake.EventEarthQuake(float.Parse(event_value));
    }

    private bool CheckEventValueFormat(string event_value)
    {
        return true;
    }

    private void CallBuildingBreak(string event_value)
    {
        GameObject eventSystem = GameObjectTreat.GetEventSystem();
        BuildingBreak buildingBreak = eventSystem.GetComponent<BuildingBreak>();
        if (buildingBreak == null)
        {
            buildingBreak = eventSystem.AddComponent<BuildingBreak>();
        }
        
        buildingBreak.EventBreakBuilding(event_value);
        EventLogCtrl.Instance.ShowEventLog("BuildingBreak:" + event_value);
    }

    private void CallTelopShow(string event_value, bool isSubTelop = false)
    {
        TelopCtrl telopCtrl = GameObject.Find("UITelop").GetComponent<TelopCtrl>();
        if (telopCtrl != null)
        {
            telopCtrl.GetComponent<TelopCtrl>().ShowTelop(event_value, isSubTelop);
        }
    }

    private void CallBloomPath(string event_value, bool isBloom = true)
    {
        if (isBloom)
        {
            BloomPathController.EventBloomPath(event_value);
        }
        else
        {
            BloomPathController.EventOffBloomPath(event_value);
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

        GameObject txtGameTime = GameObject.Find("txtGameTime");
        if (txtGameTime != null)
        {
            _gameTimerCtrl = txtGameTime.GetComponent<GameTimerCtrl>();
            // txtGameTime.GetComponent<GameTimerCtrl>()._time = 0.0f;
        }
    }
}
