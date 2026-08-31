using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

/// <summary>
/// イベントローダー
/// 
/// Phase 1.5 で IInitializable インターフェース対応
/// InitializationManager による初期化順序制御に統合
/// </summary>
public class EventLoader : MonoBehaviour, IInitializable
{
    public static EventLoader instance = null;

    private const string _PLAYER_ARMATURE_NAME = "PlayerArmature";

    // 倒壊建物からの出火 Y オフセット（PlateauCubeMaker のゴミ放出基準 center.y+0.5 に合わせる）
    private const float _DOOM_FIRE_Y_OFFSET = 0.5f;

    // ocean イベントで色まで指定するときの要素数（高さ + RGB）
    private const int _OCEAN_COLOR_PARAM_COUNT = 4;

    // ocean イベントで濁り（吸収距離）まで指定するときの要素数
    private const int _OCEAN_ABSORPTION_PARAM_COUNT = 5;

    // internal Dictionary<string, Dictionary<string, string>[]> _events = new Dictionary<string, Dictionary<string, string>[]>();
    // // ex. {notice,{time:10,value:"地震が発生しました。"}}
    // // イベント名,{イベントデータ配列}

    // internal Dictionary<float, Dictionary<string, string>> _timer_events = new Dictionary<float, Dictionary<string, string>>();
    // ex. {イベント発生時刻,{event: earthquake,value: 6}}

    internal Dictionary<float, List<Dictionary<string, string>>> _timer_events = new Dictionary<float, List<Dictionary<string, string>>>();

    /// <summary>
    /// 年別イベント辞書（Season 3 ターンベース化）
    /// 年番号 → （発火時刻 → イベントリスト）。YearScheduleYamlProvider が登録する。
    /// </summary>
    internal Dictionary<int, Dictionary<float, List<Dictionary<string, string>>>> _year_events
        = new Dictionary<int, Dictionary<float, List<Dictionary<string, string>>>>();

    /// <summary>
    /// 年別の duration（年の長さ・秒）。年番号 → 秒数
    /// </summary>
    internal Dictionary<int, float> _year_durations = new Dictionary<int, float>();

    /// <summary>
    /// 年別の想定火災延焼棟数（消火なしベースライン・W3 Task 4）。年番号 → 棟数。
    /// YAML の baseline 未指定の年は登録されず、GetYearBaseline が -1 を返す
    /// </summary>
    internal Dictionary<int, int> _year_baselines = new Dictionary<int, int>();

    internal Dictionary<string, string> _board_data = new Dictionary<string, string>();
    
    /// <summary>
    /// 座標付き立て看板データ（code → (text, pos)）
    /// 動的に生成される立て看板用（YAML の boards セクションで pos が指定されているもの）
    /// </summary>
    internal Dictionary<string, (string text, Vector3 pos)> _signboard_data = new Dictionary<string, (string, Vector3)>();

    /// <summary>
    /// ルート名（routenames） → マーカーシーケンス（CSV文字列）の辞書
    /// bloom_path や spawn_enemy_unit で前方互換として使用
    /// 例: route_wave1 → "path_marker_start_1, path_marker_1, path_marker_goal_1"
    /// </summary>
    internal Dictionary<string, string> _routeNameDict = new Dictionary<string, string>();

    /// <summary>
    /// パス上のユニット管理（off_bloom_path_complete 用）
    /// マーカーシーケンス（CSV文字列） → ルートGameObject
    /// パス上をナビゲート中のユニットを子要素として登録
    /// パス上のユニットがすべて消滅したら off_bloom_path を自動実行
    /// </summary>
    internal Dictionary<string, GameObject> _pathRootObjectDict = new Dictionary<string, GameObject>();

    /// <summary>
    /// ルート別名→実マーカーシーケンス マッピング（off_bloom_path_complete 用）
    /// routeNameDict の逆引きテーブル
    /// 例: route_wave1 のエイリアスで呼ぶときも実マーカーシーケンスで _pathRootObjectDict を検索できるようにする
    /// </summary>
    internal Dictionary<string, string> _routeNameToMarkerSequenceDict = new Dictionary<string, string>();

    /// <summary>
    /// ステージがデバッグモード（mode: debug）かどうかを示すフラグ
    /// EventLoaderYamlProvider.LoadStageInit() で stage の mode フィールドから設定される
    /// spawn_unit_debug イベント処理時に参照される
    /// </summary>
    internal bool _isDebugMode = false;

    private GameTimerCtrl _gameTimerCtrl = null;
    
    /// <summary>
    /// 初期化完了フラグ（IInitializable）
    /// InitializationManager が IsInitialized = true を待機して制御進行
    /// </summary>
    public bool IsInitialized { get; private set; } = false;

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



    internal string GetBoardText(string board_code)
    {
        string returndata = "";
        if (_board_data.Count > 0)
        {
            if (_board_data.ContainsKey(board_code))
            {
                returndata = _board_data[board_code];
                // Debug.Log($"[EventLoader.GetBoardText] FOUND: '{board_code}' -> '{returndata}'");
            }
            else
            {
                // Debug.LogWarning($"[EventLoader.GetBoardText] KEY NOT FOUND: '{board_code}' not in _board_data dictionary");
            }
        }
        else
        {
            // Debug.LogError($"[EventLoader.GetBoardText] _board_data is EMPTY (Count=0) - YAML data not loaded");
        }
        
        return returndata;
    }

    /// <summary>
    /// このステージが年サイクル（years セクション）を持つか
    /// </summary>
    internal bool HasYearEvents()
    {
        return _year_events.Count > 0;
    }

    /// <summary>
    /// 登録されている年数を取得（最終年判定に使用）
    /// </summary>
    internal int GetYearCount()
    {
        return _year_events.Count;
    }

    /// <summary>
    /// 指定年の duration（秒）を取得。未定義なら 0 を返す
    /// </summary>
    internal float GetYearDuration(int year)
    {
        if (_year_durations.TryGetValue(year, out float duration))
        {
            return duration;
        }
        return 0f;
    }

    /// <summary>
    /// 年別イベントを登録（YearScheduleYamlProvider から呼ばれる）
    /// </summary>
    internal void SetYearEvents(int year, Dictionary<float, List<Dictionary<string, string>>> timerEvents, float duration)
    {
        if (timerEvents == null)
        {
            return;
        }
        _year_events[year] = timerEvents;
        _year_durations[year] = duration;
    }

    /// <summary>
    /// 年別の想定火災延焼棟数（ベースライン）を登録（W3 Task 4）
    /// </summary>
    internal void SetYearBaseline(int year, int baseline)
    {
        _year_baselines[year] = baseline;
    }

    /// <summary>
    /// 年別の想定火災延焼棟数を取得（未登録なら -1）
    /// </summary>
    internal int GetYearBaseline(int year)
    {
        if (_year_baselines.TryGetValue(year, out int baseline))
        {
            return baseline;
        }
        return -1;
    }

    /// <summary>
    /// 年別イベントをすべて破棄（ステージロード時のリセット用）
    /// </summary>
    internal void ClearYearEvents()
    {
        _year_events.Clear();
        _year_durations.Clear();
        _year_baselines.Clear();
    }

    /// <summary>
    /// 指定年のイベントを _timer_events に差し替える
    /// [NOTE] タイマー側（GameTimerCtrl）の時刻リセット・発火リスト再構築は
    /// 呼び出し側（Task 3: YearCycleSystem 経由）が SetEventToTimer 相当の処理で行うこと
    /// </summary>
    internal bool LoadYearEvents(int year)
    {
        if (!_year_events.TryGetValue(year, out Dictionary<float, List<Dictionary<string, string>>> yearEvents))
        {
            Debug.LogWarning($"[EventLoader.LoadYearEvents] year {year} のイベントが未登録です");
            return false;
        }

        _timer_events.Clear();
        foreach (KeyValuePair<float, List<Dictionary<string, string>>> entry in yearEvents)
        {
            _timer_events[entry.Key] = entry.Value;
        }
        return true;
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

        // イベント名は YamlEventType enum の値と一致
        // バリデーション: YamlCommandManager.CreateEventCommand() で enum.TryParse() チェック済み
        switch (event_name)
        {
            case nameof(YamlEventType.spawn_unit):
                SpawnUnit(event_value);
                break;
            case nameof(YamlEventType.spawn_enemy_unit):
                SpawnEnemyUnit(event_value);
                break;
            case nameof(YamlEventType.spawn_unit_debug):
                if (_isDebugMode)
                {
                    SpawnUnit(event_value);
                }
                break;
            case nameof(YamlEventType.notice):
                ShowNotice(event_value);
                break;
            case nameof(YamlEventType.earthquake):
                CallEarthquake(event_value);
                break;
            case nameof(YamlEventType.building_break):
                CallBuildingBreak(event_value);
                break;
            case nameof(YamlEventType.building_break_fire):
                CallBuildingBreakFire(event_value);
                break;
            case nameof(YamlEventType.building_demolish):
                CallBuildingDemolish(event_value);
                break;
            case nameof(YamlEventType.telop):
                CallTelopShow(event_value);
                break;
            case nameof(YamlEventType.subtelop):
                CallTelopShow(event_value, isSubTelop: true);
                break;
            case nameof(YamlEventType.wind):
                CallWind(event_value);
                break;
            case nameof(YamlEventType.weather):
                CallWeather(event_value);
                break;
            case nameof(YamlEventType.solar):
                CallSolar(event_value);
                break;
            case nameof(YamlEventType.watersurface):
                CallWaterSurface(event_value);
                break;
            case nameof(YamlEventType.ocean):
                CallOcean(event_value);
                break;
            case nameof(YamlEventType.bloom_path):
                CallBloomPath(event_value);
                break;
            case nameof(YamlEventType.off_bloom_path):
                CallBloomPath(event_value, isBloom: false);
                break;
            case nameof(YamlEventType.off_bloom_path_complete):
                CallOffBloomPathWhenCompleted(event_value);
                break;
            case nameof(YamlEventType.bloom_sakura):
                CallBloomSakura(event_value);
                break;
            case nameof(YamlEventType.volcano):
                // 未実装
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
            
            float y = 0f;
            string yString = array[1].Trim();
            if (yString.Equals("auto", System.StringComparison.OrdinalIgnoreCase))
            {
                float z = 0f;
                if (float.TryParse(array[2], out z))
                {
                    y = Utility.GetGroundHeightAtPosition(x, z);
                }
            }
            else
            {
                float result3 = 0f;
                if (float.TryParse(yString, out result3))
                {
                    y = result3;
                }
            }
            
            float result4 = 0f;
            float z_final = 0f;
            if (float.TryParse(array[2], out result4))
            {
                z_final = result4;
            }
            
            result = new Vector3(x, y, z_final);
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
                        // [BUG-S3-013/take3] 破壊済み建物の屋根（center）に湧くと延焼しない。
                        // 火種はゴミキューブのそば（＝建物周囲の perimeter・地面）か未破壊建物に隣接する必要がある。
                        // PlateauCubeMaker はゴミを半径 sqrt(ex^2+ez^2) の輪状に散布するため、
                        // 同じ perimeter 上の地面に火種を置き、ゴミ着火→アッシュ→延焼の連鎖を成立させる
                        Vector3 firePosition = GetDoomFirePerimeterPosition(component2.bounds.center, component2.bounds.extents);
                        Debug.Log($"[EventLoader] random_doom_building: 倒壊 {doomedBuildings.Count} 棟から {doomedBuildings[index].name} を選択 pos={firePosition}");
                        return firePosition;
                    }

                    // 倒壊建物が 0 棟の場合のフォールバック
                    // 従来は原点 (0,0,0) に湧いて「火災が見えない」状態になっていた（Season 3 W1 Task 5 で判明）
                    Debug.LogWarning("[EventLoader] random_doom_building: 倒壊建物が 0 棟のため random_position にフォールバック");
                    return DemController.GetDemRndAbovePosition(10f);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 倒壊建物の周囲（ゴミが散布される perimeter リング）の地面に出火位置を返す（W3 Task 4 take3）
    /// 屋根の上ではなく、ゴミキューブのそば・未破壊の隣接建物の近くに火種を置くことで延焼を連鎖させる。
    /// 角度は建物座標から決定的に導出（出火位置の再現性を維持）、Y は Ground レイヤーに接地
    /// </summary>
    private Vector3 GetDoomFirePerimeterPosition(Vector3 center, Vector3 extents)
    {
        const float _RAY_ORIGIN_HEIGHT = 500f;
        const float _RAY_MAX_DISTANCE = 1000f;

        float radius = Mathf.Sqrt(extents.x * extents.x + extents.z * extents.z);
        // 建物中心座標から決定的に角度を導出（同じ建物 → 同じ火種位置 = 再現性）
        float angleDegrees = Mathf.Repeat(Mathf.Abs(center.x) * 7f + Mathf.Abs(center.z) * 13f, 360f);
        float angle = angleDegrees * Mathf.Deg2Rad;
        float px = center.x + radius * Mathf.Cos(angle);
        float pz = center.z + radius * Mathf.Sin(angle);

        int groundLayerMask = 1 << LayerMask.NameToLayer(GameEnum.LayerType.Ground.ToString());
        Vector3 rayOrigin = new Vector3(px, _RAY_ORIGIN_HEIGHT, pz);
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _RAY_MAX_DISTANCE, groundLayerMask))
        {
            return new Vector3(px, hit.point.y + _DOOM_FIRE_Y_OFFSET, pz);
        }

        // Ground 未ヒット時はゴミ放出と同じ height 基準でフォールバック
        return new Vector3(px, center.y + _DOOM_FIRE_Y_OFFSET, pz);
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
        string[] allParams = event_value.Split(',');
        
        // パラメータ数に応じて処理
        // PowerCube の場合：PowerCube, X, Y, Z または PowerCube, X, Y, Z, BaseScore
        Vector3 spawnPoint = tryGetPosition(TryGetColValue(event_value));
        
        SpawnController spawnCtrl = GameObjectTreat.GetSpawnController();
        if (spawnCtrl == null)
        {
            return;
        }
        
        // 5個以上のパラメータがある場合、BaseScore が指定されている可能性
        // （0: UnitName, 1: X, 2: Y, 3: Z, 4: BaseScore(optional)）
        if (allParams.Length >= 5 && unit_name == GameEnum.ModelsType.PowerCube.ToString())
        {
            if (float.TryParse(allParams[4].Trim(), out float baseScore))
            {
                spawnCtrl.CallUnitByNameWithBaseScore(unit_name, spawnPoint, baseScore);
                EventLogCtrl.Instance.ShowEventLog($"SpawnUnit:{unit_name} (BaseScore: {baseScore})");
                return;
            }
        }
        
        // 通常のスポーン（BaseScore なし）
        spawnCtrl.CallUnitByName(unit_name, spawnPoint);
        EventLogCtrl.Instance.ShowEventLog("SpawnUnit:" + unit_name);
    }

    private void SpawnEnemyUnit(string event_value)
    {
        string unit_name = TryGetCol0(event_value);
        string[] marker_names = event_value.Split(',').Skip(1).Select(m => m.Trim()).ToArray();
        
        // routenames の互換処理: 最初のマーカーがルート名かチェック
        if (marker_names.Length > 0)
        {
            string firstMarker = marker_names[0];
            if (_routeNameDict.ContainsKey(firstMarker))
            {
                // ルート定義が見つかった → マーカー名を分解
                string markerSequence = _routeNameDict[firstMarker];
                marker_names = markerSequence.Split(',').Select(m => m.Trim()).ToArray();
            }
            else
            {
                // 直接指定の場合も、正規化形式に統一
                string markerSequence = string.Join(", ", marker_names);
                marker_names = markerSequence.Split(',').Select(m => m.Trim()).ToArray();
            }
        }
        
        SpawnController spawnCtrl = GameObjectTreat.GetSpawnController();
        if (spawnCtrl == null)
        {
            return;
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
        
        WindController.SetWindSpeed(windSpeed);
        WindController.SetWindDirection(windDirection);
    }

    private void CallWeather(string event_value)
    {
        WeatherController orAddComponent = GameObjectTreat.GetOrAddComponent<WeatherController>(GameObjectTreat.GetEventSystem());
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
        GameObjectTreat.GetOrAddComponent<WeatherController>(GameObjectTreat.GetEventSystem()).ChangeSolarAltitude(float.Parse(event_value));
    }

    /// <summary>
    /// 水面ホルダー（watersurface 親）の高さを変える従来イベント。
    /// 海面だけを動かしたい場合は ocean を使うこと（三鷹大沢が本イベントを使用中のため据え置き）
    /// </summary>
    private void CallWaterSurface(string event_value)
    {
        GameObjectTreat.GetOrAddComponent<WaterSurfaceManager>(GameObjectTreat.GetEventSystem()).SetWaterSurfaceHeight(float.Parse(event_value));
    }

    /// <summary>
    /// 海面（Ocean）イベント。
    ///   value: 高さ                        … 海面のワールド Y(m)
    ///   value: 高さ, R, G, B               … 併せて海面の色も変える（各 0〜1）
    ///   value: 高さ, R, G, B, 吸収距離     … さらに濁り具合も変える（m・小さいほど濁る）
    /// </summary>
    private void CallOcean(string event_value)
    {
        string[] parts = event_value.Split(',');

        WaterSurfaceManager waterSurfaceManager =
            GameObjectTreat.GetOrAddComponent<WaterSurfaceManager>(GameObjectTreat.GetEventSystem());

        if (!float.TryParse(parts[0].Trim(), out float height))
        {
            Debug.LogWarning($"[EventLoader] ocean: 高さを解釈できません '{event_value}'");
            return;
        }
        waterSurfaceManager.SetOceanHeight(height);

        if (parts.Length < _OCEAN_COLOR_PARAM_COUNT)
        {
            return;
        }

        if (!float.TryParse(parts[1].Trim(), out float r)
            || !float.TryParse(parts[2].Trim(), out float g)
            || !float.TryParse(parts[3].Trim(), out float b))
        {
            Debug.LogWarning($"[EventLoader] ocean: 色を解釈できません '{event_value}'");
            return;
        }

        // 吸収距離は省略可。指定が無ければ現在の透明度を維持する
        float absorptionDistance = 0f;
        if (parts.Length >= _OCEAN_ABSORPTION_PARAM_COUNT)
        {
            float.TryParse(parts[4].Trim(), out absorptionDistance);
        }

        waterSurfaceManager.SetWaterColor(new Color(r, g, b, 1f), absorptionDistance);
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

    /// <summary>
    /// building_break_fire: 建物を N 棟倒壊させ、倒壊した各建物から出火させる。
    /// 出火数が倒壊数に一致し、地震連動火災の延焼が倒壊規模を反映する
    /// </summary>
    private void CallBuildingBreakFire(string event_value)
    {
        GameObject eventSystem = GameObjectTreat.GetEventSystem();
        BuildingBreak buildingBreak = eventSystem.GetComponent<BuildingBreak>();
        if (buildingBreak == null)
        {
            buildingBreak = eventSystem.AddComponent<BuildingBreak>();
        }

        if (!int.TryParse(event_value.Trim(), out int count) || count <= 0)
        {
            Debug.LogWarning($"[EventLoader] building_break_fire: 棟数が不正 '{event_value}'");
            return;
        }

        List<GameObject> brokenBuildings = buildingBreak.BreakBuildingsForFire(count);
        SpawnController spawnCtrl = GameObjectTreat.GetSpawnController();
        if (spawnCtrl == null)
        {
            return;
        }

        int fireCount = 0;
        foreach (GameObject building in brokenBuildings)
        {
            if (building == null)
            {
                continue;
            }
            Renderer buildingRenderer = building.GetComponent<Renderer>();
            if (buildingRenderer == null)
            {
                continue;
            }
            Vector3 firePosition = GetDoomFirePerimeterPosition(buildingRenderer.bounds.center, buildingRenderer.bounds.extents);
            spawnCtrl.CallUnitByName(GameEnum.ModelsType.FireCube.ToString(), firePosition);
            fireCount = fireCount + 1;
        }
        Debug.Log($"[EventLoader] building_break_fire: {brokenBuildings.Count} 棟倒壊・{fireCount} 箇所から出火");
        EventLogCtrl.Instance.ShowEventLog("地震で " + brokenBuildings.Count + " 棟倒壊・出火");
    }

    /// <summary>
    /// building_demolish: 建物を N 棟 解体（更地化＋瓦礫散布）する（CityHack 2026）。
    /// value に "all" を指定すると対象建物すべてを解体する
    /// </summary>
    private void CallBuildingDemolish(string event_value)
    {
        GameObject eventSystem = GameObjectTreat.GetEventSystem();
        BuildingBreak buildingBreak = eventSystem.GetComponent<BuildingBreak>();
        if (buildingBreak == null)
        {
            buildingBreak = eventSystem.AddComponent<BuildingBreak>();
        }

        string trimmed = event_value.Trim();
        int count = 0;
        if (trimmed == GameEnum.PathMarkerNameParts.ALL)
        {
            count = buildingBreak.GetBreakTargets().Count;
        }
        else if (!int.TryParse(trimmed, out count) || count <= 0)
        {
            Debug.LogWarning($"[EventLoader] building_demolish: 棟数が不正 '{event_value}'");
            return;
        }

        int demolished = buildingBreak.DemolishBuildings(count);
        if (EventLogCtrl.Instance != null)
        {
            EventLogCtrl.Instance.ShowEventLog($"解体 {demolished} 棟 / {DemolitionSystem.GetSummaryText()}");
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

    private void CallBloomPath(string event_value, bool isBloom = true)
    {
        // routenames の互換処理: event_value がルート名かチェック
        string resolvedValue = event_value;
        string trimmedValue = event_value.Trim();
        if (_routeNameDict.ContainsKey(trimmedValue))
        {
            // ルート定義が見つかった → マーカー文字列に置き換える
            resolvedValue = _routeNameDict[trimmedValue];
        }
        
        if (isBloom)
        {
            BloomPathController.EventBloomPath(resolvedValue);
        }
        else
        {
            BloomPathController.EventOffBloomPath(resolvedValue);
        }
    }

    /// <summary>
    /// パス上のユニットが全て消滅したら off_bloom_path を実行
    /// ユニット登録用のルートオブジェクトの子要素をチェック
    /// </summary>
    private void CallOffBloomPathWhenCompleted(string event_value)
    {
        // routenames の互換処理: event_value がルート名かチェック
        string resolvedValue = event_value;
        string trimmedValue = event_value.Trim();
        if (_routeNameDict.ContainsKey(trimmedValue))
        {
            // ルート定義が見つかった → マーカー文字列に置き換える
            resolvedValue = _routeNameDict[trimmedValue];
        }

        // NotifyEnemyDeath と同じロジックで確認
        NotifyEnemyDeath(resolvedValue);
    }

    /// <summary>
    /// 敵ユニットをパス追跡ルートに登録
    /// ユニットオブジェクトをパス用ルートオブジェクトの子要素として追加
    /// ユニット死亡時に自動的に子要素から削除される
    /// 戻り値: マーカーシーケンス（CSV文字列）
    /// </summary>
    internal string RegisterEnemyToPath(string[] marker_names, GameObject enemy_unit)
    {
        if (marker_names == null || marker_names.Length == 0 || enemy_unit == null)
        {
            return "";
        }

        // マーカーシーケンスを CSV 文字列に変換（ルートオブジェクト識別用）
        string markerSequence = string.Join(", ", marker_names);

        // パス用ルートオブジェクトを取得または作成
        if (!_pathRootObjectDict.TryGetValue(markerSequence, out GameObject pathRootObject))
        {
            // 初めてのユニット → ルートオブジェクトを作成
            pathRootObject = new GameObject($"PathRoot_{markerSequence}");
            pathRootObject.transform.SetParent(transform);
            _pathRootObjectDict[markerSequence] = pathRootObject;
            Debug.Log($"[EventLoader.RegisterEnemyToPath] パス用ルートオブジェクト作成: {markerSequence}");
        }

        // ユニットをルートオブジェクトの子要素として登録
        enemy_unit.transform.SetParent(pathRootObject.transform);
        Debug.LogTrace($"[EventLoader.RegisterEnemyToPath] ユニット '{enemy_unit.name}' をパス '{markerSequence}' に登録（現在の子要素数: {pathRootObject.transform.childCount})");
        
        return markerSequence;
    }

    /// <summary>
    /// 敵ユニット削除時に呼び出す
    /// パス追跡中のユニットが全て消滅したかをチェック
    /// 全て消滅していたら off_bloom_path を実行
    /// </summary>
    internal void NotifyEnemyDeath(string markerSequence)
    {
        if (string.IsNullOrEmpty(markerSequence))
        {
            Debug.Log($"[EventLoader.NotifyEnemyDeath] markerSequence が空の場合、処理をスキップ");
            return;
        }

        // routeName で呼ばれた場合、実マーカーシーケンスに変換
        string resolvedMarkerSequence = markerSequence;
        if (_routeNameToMarkerSequenceDict.ContainsKey(markerSequence))
        {
            resolvedMarkerSequence = _routeNameToMarkerSequenceDict[markerSequence];
            Debug.LogTrace($"[EventLoader.NotifyEnemyDeath] routeName '{markerSequence}' を実マーカーシーケンス '{resolvedMarkerSequence}' に変換");
        }

        // パス用ルートオブジェクトを取得
        if (!_pathRootObjectDict.TryGetValue(resolvedMarkerSequence, out GameObject pathRootObject))
        {
            Debug.Log($"[EventLoader.NotifyEnemyDeath] パス '{resolvedMarkerSequence}' のルートオブジェクトが見つかりません");
            return;
        }

        // パス上のユニット数をカウント（子要素 = スポーン済みユニット）
        int childCount = pathRootObject.transform.childCount;
        
        Debug.LogTrace($"[EventLoader.NotifyEnemyDeath] パス '{resolvedMarkerSequence}' の現在の子要素数: {childCount}");

        // 子要素の詳細ログ（デバッグ用）
        for (int i = 0; i < childCount; i++)
        {
            Transform child = pathRootObject.transform.GetChild(i);
            Debug.LogTrace($"  └─ [{i}] {child.gameObject.name}");
        }

        if (childCount > 0)
        {
            Debug.LogTrace($"[EventLoader.NotifyEnemyDeath] パス '{resolvedMarkerSequence}' にまだ {childCount} 個のユニットがいるため、off_bloom_path は実行しません");
            return;
        }

        // パスが空 → off_bloom_path を実行
        Debug.Log($"[EventLoader.NotifyEnemyDeath] パス '{resolvedMarkerSequence}' が空になったため、off_bloom_path を実行");
        BloomPathController.EventOffBloomPath(resolvedMarkerSequence);
    }

    private void CallBloomSakura(string event_value)
    {
        OrnamentSystem ornamentSystem = GameObjectTreat.GetOrAddComponent<OrnamentSystem>(GameObjectTreat.GetEventSystem());
        if (ornamentSystem == null)
        {
            Debug.LogWarning("[EventLoader.CallBloomSakura] OrnamentSystem の取得に失敗しました");
            return;
        }

        ornamentSystem.BloomSakura(event_value);
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

        _gameTimerCtrl = GameTimerCtrl.GetInstance();
        // txtGameTime.GetComponent<GameTimerCtrl>()._time = 0.0f;

        // [重要] Awake では IsInitialized = false
        IsInitialized = false;

        // PollutantManager を最初に初期化
        PollutantManager.Initialize();
    }
    
    /// <summary>
    /// 初期化コルーチン
    /// InitializationManager が呼び出し、完了を待機
    /// 
    /// ゲーム進行制御：
    /// - IsInitialized = true まで、EventLoader の機能を制御
    /// - ゲーム開始時にこのフラグが完了するまで待機
    /// </summary>
    void Start()
    {
        // StageYamlRepository が既に _board_data と _signboard_data を初期化済み
        // PollutantManager は Awake() で既に初期化済み
        
        // 座標付き立て看板を生成
        SignboardManager.CreateSignboards(_signboard_data);
        
        // PlayerArmature の初期スポーン位置を SpawnOriginTracker に記録する
        RegisterPlayerSpawnOrigin();
        
        IsInitialized = true;
    }
    
    /// <summary>
    /// PlayerArmature に SpawnOriginTracker を付与し、初期位置を記録する。
    /// NarakuTriggerHandler が池等で DEM 検出全失敗時にここへ戻す帰還先として参照する。
    /// </summary>
    private void RegisterPlayerSpawnOrigin()
    {
        GameObject playerArmature = GameObject.Find(_PLAYER_ARMATURE_NAME);
        if (playerArmature == null)
        {
            Debug.LogWarning("[EventLoader.RegisterPlayerSpawnOrigin] PlayerArmature がシーン上に見つかりません");
            return;
        }
        
        SpawnOriginTracker spawnTracker = GameObjectTreat.GetOrAddComponent<SpawnOriginTracker>(playerArmature);
        if (spawnTracker == null)
        {
            Debug.LogWarning("[EventLoader.RegisterPlayerSpawnOrigin] SpawnOriginTracker の取得に失敗しました");
            return;
        }
        
        spawnTracker.SetSpawnOrigin(playerArmature.transform.position);
        Debug.Log($"[EventLoader] PlayerArmature スポーン原点を登録しました: {playerArmature.transform.position}");
    }
    
    /// <summary>
    /// IInitializable インターフェース実装
    /// コンポーネント名を取得（ログ出力用）
    /// </summary>
    public string GetComponentName()
    {
        return this.GetType().Name;
    }
}
