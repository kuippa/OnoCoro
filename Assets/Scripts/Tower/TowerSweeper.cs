using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CommonsUtility;
using UnityEngine.AI;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;   // NavMeshAgentを使うために必要


public class TowerSweeper : MonoBehaviour
{
    const string _TOWER_SWEEPER = "TowerSweeper";
    const string _TOWER_DECK = "TowerDeck";
    private GameObject _MyDeck = null;
    NavMeshAgent _NavMeshAgent = null;
    private GameObject _targetGarbage = null;    // 狙いのゴミ
    private GameObject _pre_targetGarbage = null;    // 前回からの狙いのゴミ
    List<GameObject> _AimGarbageLists = new List<GameObject>();    // 狙いのゴミ候補
    List<GameObject> _IgnoreGarbageLists = new List<GameObject>();    // 無視するゴミ候補
    private float _lastTriggerStayTime;
    private const float _TRIGGER_STAY_INTERVAL = 0.1f; 

    private double _time = 0;            // ループ経過時間
    private double _ignore_time = 0;      // 諦めリストにいれるまでの時間
    public GameObject _Active_Dock = null;
    public GameObject _Sleep_Dock = null;

    bool _chargeMode = false;
    const float _BATTERY_ORG_SIZE = 0.5f;

    // [SerializeField]
    internal float _LOOP_TIME = 0.2f;      // ループ時間
    internal float _IGNORE_TIME = 5.0f;      // 諦めるまでの時間
    internal float _MOVE_SPEED = 3f;    // 移動速度
    // public float _ROTATE_ANGLE = 78;  // 回転角度
    internal float _FULL_BATTERY = 100f;  // バッテリーの最大容量
    internal float _HP = 100f;  // ヒットポイント（バッテリー）
    internal float _DECREASE_BATTERY = 1.0f;  // バッテリーの減少量
    internal float _CHARGE_BATTERY = 5f;  // バッテリーの回復量
    private const float _BATTERY_DISTANCE = 1.8f;  // バッテリーの充電距離


    public void CreateSweeperUnit(Vector3 setPoint)
    {
        this.tag = _TOWER_SWEEPER;
        int idx = GameObjectTreat.IndexObjectByTag(this.tag);
        this.name = this.tag + idx;
        this.transform.position = setPoint;
        GameObject TowerDock = Instantiate(Resources.Load("Prefabs/WorkUnit/TowerDock")) as GameObject;
        setPoint.x = setPoint.x + this.transform.localScale.x / 2 + 0.1f;
        TowerDock.transform.position = setPoint;
        TowerDock.name = _TOWER_DECK + idx;
        _MyDeck = TowerDock;
        if (_NavMeshAgent != null)
        {
            _NavMeshAgent.enabled = true;
        }
        ChangeBatteryDockMode(false);


    }

    void OnDestroy()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        GameObjectTreat.DestroyAll(this.gameObject);
    }

    void Awake()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        // GameConfig.InitGameConfig();

        // NavMeshAgentを取得
        _NavMeshAgent = this.GetComponent<NavMeshAgent>();
        if (_NavMeshAgent == null)
        {
            Debug.Log("NavMeshAgent is null");
            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(1);
            _NavMeshAgent = this.gameObject.AddComponent<NavMeshAgent>();
            _NavMeshAgent.enabled = false;
            _NavMeshAgent.agentTypeID = settings.agentTypeID;
            _NavMeshAgent.speed = _MOVE_SPEED;
            _NavMeshAgent.angularSpeed = 80;
            _NavMeshAgent.autoBraking = true;
            _NavMeshAgent.radius = 0.5f;
            _NavMeshAgent.height = 2f;
            _NavMeshAgent.areaMask = 1;
            // _NavMeshAgent.stoppingDistance = 0.1f;  // stoppingDistance	目標地点のどれぐらい手前で停止するかの距離
            _NavMeshAgent.enabled = true;
        }
        // _NavMeshAgent.autoRepath = true;    // autoRepath	エージェントが移動先に着いたり、途中で破棄された場合、新しいパスを取得する必要があるかどうか

        // currentOffMeshLinkData	現在の OffMeshLinkData
        ChangeBatteryDockMode(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // #if UNITY_EDITOR
        //     Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        //     // Debug.Log(this.transform.position + " " + other.name);
        // #endif
        float currentTime = Time.time;
        if (currentTime - _lastTriggerStayTime < _TRIGGER_STAY_INTERVAL)
        {
            return;
        }
        _lastTriggerStayTime = currentTime;

        // 視界に入ったゴミを赤くする
        if (other.tag == GameEnum.EnemyType.Garbage.ToString() )
        {
            GameObject otherGameObject = other.gameObject;
            GameObjectTreat.DebugColorChange(otherGameObject, Color.red);
            SetTargetGarbage(otherGameObject);
        }
    }

    // void OnTriggerExit(Collider other)
    // {
    //     // 視界から出たゴミを灰色にする
    //     if (other.tag == GameEnum.EnemyType.Garbage.ToString() )
    //     {
    //         GameObjectTreat.DebugColorChange(other.gameObject, Color.gray);
    //     }
    // }


    private void SetTargetGarbage(GameObject other)
    {
        // エリア内にあるゴミをリストに追加
        if (!_AimGarbageLists.Contains(other))
        {
            _AimGarbageLists.Add(other);
        }
        // thisからの距離が近ければtargetGarbageを更新
        if (CompareDistance(other))
        {
            GameObjectTreat.DebugColorChange(_targetGarbage, Color.green);
            _targetGarbage = other.gameObject;
        }
        else
        {
            GameObjectTreat.DebugColorChange(_targetGarbage, Color.blue);
        }
    }

    // 与えられた引数の距離を比較する
    private bool CompareDistance(GameObject compareObject)
    {
        // 無視リストに含まれている場合は、比較しない
        if (_IgnoreGarbageLists.Contains(compareObject))
        {
            return false;
        }

        if (_targetGarbage == null)
        {
            _targetGarbage = compareObject.gameObject;
        }

        if (Vector3.Distance(this.transform.position, _targetGarbage.transform.position) 
            >= Vector3.Distance(this.transform.position, compareObject.transform.position))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void MoveControl()
    {
        // バッテリーのチェック
        if (!CheckBattery())
        {
            return;
        }
        else
        {
            // ターゲットの確認
            if (!CheckTargetGarbage())
            {
                // 目的地の設定
                if (!SetNavMeshDestination())
                {
                    return;
                }
            }
            else
            {
                // 周囲探索
                TowerMoveCtrl.LookAround(_NavMeshAgent, this.transform);
            }
        }
    }

    private bool CheckTargetGarbage()
    {
        GameObject targetGarbage =  GetTargetGarbage();
        if (targetGarbage != null)
        {
            _targetGarbage = targetGarbage;
            return false;
        }
        return true;
    }

    private GameObject GetTargetGarbage()
    {
        GameObject targetGarbage = _targetGarbage;
        if (targetGarbage == null)
        {
            if (_AimGarbageLists.Count > 0)
            {
                // ターゲットがない場合、リストの先頭をターゲットにする
                targetGarbage = SetTargetFromAimGarbageLists();
            }
        }
        return targetGarbage;
    }

    private bool CheckBattery()
    {
        if (_HP > 0 && !_chargeMode)
        {
            DecreaseHP();
        }

        if (_HP <= 0 || _chargeMode)
        {
            ChargeBattery();
            return false;
        }
        return true;
    }

    private void ChargeBattery()
    {
        if (_MyDeck == null)
        {
            Debug.Log("_MyDeck is null ");
            return;
        }
        _chargeMode = true;
        // deckとの距離が遠ければ、deckに向かって移動する
        float distance = Vector3.Distance(this.transform.position, _MyDeck.transform.position);
        GameObject battery_bar = this.transform.Find("battery_bar").gameObject;

        if ( distance > _BATTERY_DISTANCE)
        {
            TowerMoveCtrl.SetDestination(_MyDeck.transform.position, _NavMeshAgent);
            GameObjectTreat.ColorChange(battery_bar, Color.red);            
        }
        else
        {
            TowerMoveCtrl.ClearDestination(_NavMeshAgent);
            ClearIgnoreGarbageLists();
            _HP += _CHARGE_BATTERY;
            BatteryView();
            ChangeBatteryDockMode(true);
            if (_HP >= _FULL_BATTERY)
            {
                GameObjectTreat.ColorChange(battery_bar, Color.green);            
                _HP = _FULL_BATTERY;
                _chargeMode = false;
                ChangeBatteryDockMode(false);
            }
        }
    }

    private void ChangeBatteryDockMode(bool mode)
    {
        if (_MyDeck == null)
        {
            // Debug.Log("_MyDeck is null ");
            return;
        }

        if (_Active_Dock == null || _Sleep_Dock == null)
        {
            // Debug.Log("_MyDeck.name " + _MyDeck.name);

            _Active_Dock = _MyDeck.transform.Find("ChargeMode").gameObject;
            _Sleep_Dock = _MyDeck.transform.Find("SleepMode").gameObject;
        }
        if (mode)
        {
            _Active_Dock.SetActive(true);
            _Sleep_Dock.SetActive(false);    
        }
        else
        {
            _Active_Dock.SetActive(false);
            _Sleep_Dock.SetActive(true);
        }


    }

    private bool SetNavMeshDestination()
    {
        Vector3 destination = _targetGarbage.transform.position;
        // destination = new Vector3(destination.x, this.transform.position.y, destination.z);
        Vector3 relativePos = destination - this.transform.position;
        this.transform.localRotation = TowerMoveCtrl.GetRotateAngle(relativePos);
        TowerMoveCtrl.SetDestination(destination, _NavMeshAgent);

        if (!_NavMeshAgent.isOnNavMesh)
        {
            Debug.Log("SetNavMeshDestination isOnNavMesh false:" + _NavMeshAgent.GetInstanceID() + " :" + _NavMeshAgent.name);
            _targetGarbage = SetTargetGarbageIgnoreLists(_targetGarbage);
            TowerMoveCtrl.ClearDestination(_NavMeshAgent);
            return false;
        }

        // 経路探索未完了
        if (_NavMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.Log("PathPartialキューブ:" + _targetGarbage.name 
                + " hasPath:" + _NavMeshAgent.hasPath   // エージェントが経路を持っているかどうか（読み取り専用）
                + " pathStatus:" + _NavMeshAgent.pathStatus 
                + " remainingDistance:" + _NavMeshAgent.remainingDistance 
                + " destination:" + _NavMeshAgent.destination 
                + " pathPending:" + _NavMeshAgent.pathPending   // 経路探索の準備ができているかどうか（読み取り専用）
                + " isPathStale:" + _NavMeshAgent.isPathStale);
            return false;
        }
        // 経路探索 無効
        else if (_NavMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("PathInvalidキューブ:" + _targetGarbage.name 
                + " hasPath:" + _NavMeshAgent.hasPath   // エージェントが経路を持っているかどうか（読み取り専用）
                + " pathStatus:" + _NavMeshAgent.pathStatus 
                + " remainingDistance:" + _NavMeshAgent.remainingDistance 
                + " destination:" + _NavMeshAgent.destination 
                + " pathPending:" + _NavMeshAgent.pathPending   // 経路探索の準備ができているかどうか（読み取り専用）
                + " isPathStale:" + _NavMeshAgent.isPathStale);
            return false;
        }

        // 経路探索 完了
        else if (_NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            // CalculatePath 	エージェントが目標値までにたどり着く道程を計算し、path 引数に格納します
            if (!_NavMeshAgent.hasPath)
            {
                Debug.Log("到達できないキューブ:" + _targetGarbage.name 
                    + " hasPath:" + _NavMeshAgent.hasPath   // エージェントが経路を持っているかどうか（読み取り専用）
                    + " pathStatus:" + _NavMeshAgent.pathStatus 
                    + " remainingDistance:" + _NavMeshAgent.remainingDistance 
                    + " destination:" + _NavMeshAgent.destination 
                    + " pathPending:" + _NavMeshAgent.pathPending   // 経路探索の準備ができているかどうか（読み取り専用）
                    + " isPathStale:" + _NavMeshAgent.isPathStale);

                _targetGarbage = SetTargetGarbageIgnoreLists(_targetGarbage);
                return false;
            }
            else
            {
                Debug.Log("pathはある:" + _targetGarbage.name 
                    + " hasPath:" + _NavMeshAgent.hasPath   // エージェントが経路を持っているかどうか（読み取り専用）
                    + " pathStatus:" + _NavMeshAgent.pathStatus 
                    + " remainingDistance:" + _NavMeshAgent.remainingDistance 
                    + " destination:" + _NavMeshAgent.destination 
                    + " pathPending:" + _NavMeshAgent.pathPending   // 経路探索の準備ができているかどうか（読み取り専用）
                    + " isPathStale:" + _NavMeshAgent.isPathStale);
                return true;
            }
        }
        else
        {
            Debug.Log("_NavMeshAgent.pathStatus:" + _NavMeshAgent.pathStatus);
        }

        return true;
    }

    private GameObject SetTargetGarbageIgnoreLists(GameObject targetGarbage)
    {
        // Debug.Log("SetTargetGarbageIgnoreLists " + targetGarbage);
        _IgnoreGarbageLists.Add(targetGarbage);
        GameObjectTreat.DebugColorChange(targetGarbage, Color.black);
        targetGarbage = null;
        return targetGarbage;
    }

    private void ClearIgnoreGarbageLists()
    {

        foreach (GameObject ignoreGarbage in _IgnoreGarbageLists)
        {
            GameObjectTreat.DebugColorChange(ignoreGarbage, Color.yellow);
        }

        _IgnoreGarbageLists.Clear();
    }

    private GameObject SetTargetFromAimGarbageLists()
    {
        GameObject targetGarbage = null;
        for (int i = 0; i < _AimGarbageLists.Count; i++)
        {
            if (_AimGarbageLists[i] == null || _IgnoreGarbageLists.Contains(_AimGarbageLists[i]))
            {
                _AimGarbageLists.RemoveAt(i);
            }
            else
            {
                if (CompareDistance(_AimGarbageLists[i]))
                {
                    targetGarbage = _AimGarbageLists[i];
                }
            }
        }
        return targetGarbage;
    }

    // HPを時間経過で減らす
    private void DecreaseHP()
    {
        _HP -= _DECREASE_BATTERY;
        // return;
        BatteryView();
    }

    private void BatteryView()
    {
        GameObject battery_bar = this.transform.Find("battery_bar").gameObject;
        Vector3 battery_bar_scale = battery_bar.transform.localScale;
        battery_bar_scale.y = _BATTERY_ORG_SIZE * _HP / _FULL_BATTERY;
        battery_bar.transform.localScale = battery_bar_scale;

    }

    void FixedUpdate()
    {
        _time += Time.deltaTime;
        if (_time > _LOOP_TIME)
        {
            _time = 0;
            MoveControl();
            if (_targetGarbage != null)
            {
                if (_pre_targetGarbage == _targetGarbage && !_NavMeshAgent.isStopped)
                {
                    _ignore_time += Time.deltaTime;
                    if (_ignore_time > _IGNORE_TIME)
                    {
                        // ターゲットを無視リストに追加する
                        // _targetGarbage = SetTargetGarbageIgnoreLists(_targetGarbage);
                    }
                }
                else
                {
                    _ignore_time = 0;
                    _pre_targetGarbage = _targetGarbage;
                }
            }
        }
    }

}
