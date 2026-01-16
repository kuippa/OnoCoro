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
using System.Linq;

public class TowerSweeper : MonoBehaviour
{
    private GameObject _MyDeck = null;
    NavMeshAgent _NavMeshAgent = null;
    private GameObject _targetGarbage = null;    // 狙いのゴミ
    List<GameObject> _AimGarbageLists = new List<GameObject>();    // 狙いのゴミ候補
    List<GameObject> _IgnoreGarbageLists = new List<GameObject>();    // 無視するゴミ候補
    private float _lastTriggerStayTime;
    private const float _TRIGGER_STAY_INTERVAL = 0.02f; 
    private const float _TARGET_DEL_DISTANCE = 1.2f;    // 消し込みの最低距離 ヘッド2.2

    private double _time = 0;            // ループ経過時間
    public GameObject _Active_Dock = null;
    public GameObject _Sleep_Dock = null;

    bool _chargeMode = false;
    const float _BATTERY_ORG_SIZE = 0.5f;

    // [SerializeField]
    internal float _LOOP_TIME = 0.6f;      // ループ時間
    internal float _MOVE_SPEED = 3f;    // 移動速度
    // public float _ROTATE_ANGLE = 78;  // 回転角度
    internal float _FULL_BATTERY = 100f;  // バッテリーの最大容量
    internal float _HP = 100f;  // ヒットポイント（バッテリー）
    internal float _DECREASE_BATTERY = 1.0f;  // バッテリーの減少量
    internal float _CHARGE_BATTERY = 5f;  // バッテリーの回復量
    private const float _BATTERY_DISTANCE = 1.8f;  // バッテリーの充電距離

    private bool _isDelete = false; // 削除処理実行中かどうか

    public void CreateSweeperUnit(Vector3 setPoint)
    {
        this.tag = GameEnum.TagType.TowerSweeper.ToString();
        int idx = GameObjectTreat.IndexObjectByTag(this.tag);
        this.name = GameEnum.ModelsType.Sweeper.ToString() + idx.ToString();
        this.AddComponent<Sweeper>();
        this.GetComponent<Sweeper>()._item_struct.ItemID = this.name;
        this.GetComponent<Sweeper>()._unit_struct.UnitID = this.name;

        this.transform.position = setPoint;
        GameObject TowerDock = Instantiate(Resources.Load("Prefabs/WorkUnit/TowerDock")) as GameObject;
        setPoint.x = setPoint.x + this.transform.localScale.x / 2 + 0.1f;
        TowerDock.transform.position = setPoint;
        TowerDock.tag = GameEnum.TagType.TowerDock.ToString();
        TowerDock.name = TowerDock.tag + idx;
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
        if (currentTime - _lastTriggerStayTime <= _TRIGGER_STAY_INTERVAL)
        {
            return;
        }
        _lastTriggerStayTime = currentTime;

        // 視界に入ったゴミをターゲットにする
        if (other.tag == GameEnum.TagType.Garbage.ToString() || other.tag == GameEnum.TagType.Ash.ToString())
        {
            GameObject otherGameObject = other.gameObject;
            GameObjectTreat.DebugColorChange(otherGameObject, Color.red);
            SetTargetGarbage(otherGameObject);
        }
    }

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

        // ターゲットの更新
        UpdateTargetGarbage();
        if (_targetGarbage == null)
        {
            // ターゲットがない場合、周囲探索
            TowerMoveCtrl.LookAround(_NavMeshAgent, this.transform);
            return;
        }

        // 目的地の設定
        Vector3 destination = _targetGarbage.transform.position;
        if (IsSameDestination(destination))
        {
            return;
        }
        if (!SetNavMeshDestination(destination))
        {
            Debug.Log("SetNavMeshDestination false:" + destination);
            _targetGarbage = SetTargetGarbageIgnoreLists(_targetGarbage);
            return;
        }
        
        // NavMeshの状態チェック
        NavMeshPathStatus pathStatus = _NavMeshAgent.pathStatus;
        switch (pathStatus)
        {
            case NavMeshPathStatus.PathComplete:
                HandlePathComplete();
                break;
            case NavMeshPathStatus.PathPartial:
                // Debug.Log("Path is partial. Waiting for complete path.");
                if (_NavMeshAgent.remainingDistance <= _TARGET_DEL_DISTANCE)
                {
                    TriggerSweepGarbage();
                }                
                break;
            case NavMeshPathStatus.PathInvalid:
                // Debug.LogWarning("Path is invalid. Ignoring current target.");
                _targetGarbage = SetTargetGarbageIgnoreLists(_targetGarbage);
                break;
            default:
                // Debug.LogError($"Unexpected NavMeshPathStatus: {pathStatus}");
                break;
        }
        // // ターゲットに移動中、もしくは経路探索中
        // if (!CheckNavMeshPathStatus(_NavMeshAgent))
        // {
        //     Debug.Log("CheckNavMeshPathStatus false:" + destination);
        //     return;
        // }
    }

    private void HandlePathComplete()
    {
        if (!_NavMeshAgent.hasPath)
        {
            // Debug.Log("Path is complete but agent has no path. Ignoring current target." + _targetGarbage.name);
            _targetGarbage = SetTargetGarbageIgnoreLists(_targetGarbage);
            return;
        }

        if (_NavMeshAgent.remainingDistance <= _TARGET_DEL_DISTANCE)
        {
            TriggerSweepGarbage();
        }
    }

    private void TriggerSweepGarbage()
    {
        GameObject head = this.transform.Find("head").gameObject;
        SweepCtrl sweepCtrl = head.GetComponent<SweepCtrl>();
        if (sweepCtrl != null)
        {
            sweepCtrl.SweepGarbage(_targetGarbage.GetComponent<Collider>());
        }
        else
        {
            // Debug.LogError("SweepCtrl component not found on head object.");
        }
    }

    private void UpdateTargetGarbage()
    {
        if (_targetGarbage == null || !IsValidTarget(_targetGarbage))
        {
            _targetGarbage = GetBestTargetFromList();
        }
    }

    private bool IsValidTarget(GameObject target)
    {
        return target != null && !_IgnoreGarbageLists.Contains(target);
    }

    private GameObject GetBestTargetFromList()
    {
        return _AimGarbageLists
            .Where(IsValidTarget)
            .OrderBy(g => Vector3.Distance(this.transform.position, g.transform.position))
            .FirstOrDefault();
    }

    private bool CheckBattery()
    {
        if (_HP <= 0 || _chargeMode)
        {
            ChargeBattery();
            return false;
        }
        DecreaseHP();
        return true;
    }

    private void ChargeBattery()
    {
        if (_MyDeck == null)
        {
            // Debug.Log("_MyDeck is null ");
            return;
        }
        _chargeMode = true;
        // deckとの距離が遠ければ、deckに向かって移動する
        float distance = Vector3.Distance(this.transform.position, _MyDeck.transform.position);
        GameObject battery_bar = this.transform.Find("battery_bar").gameObject;
        if ( distance > _BATTERY_DISTANCE)
        {
            if (TowerMoveCtrl.GetDestination(_NavMeshAgent) != _MyDeck.transform.position)
            {
                TowerMoveCtrl.SetDestination(_MyDeck.transform.position, _NavMeshAgent);
            }
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

    private bool IsSameDestination(Vector3 destination)
    {
        if (TowerMoveCtrl.GetDestination(_NavMeshAgent) == destination)
        {
            return true;
        }
        return false;
    }

    private bool IsOnNavMesh(NavMeshAgent NavMeshAgent)
    {
        if (!NavMeshAgent.isOnNavMesh)
        {
            Debug.Log("isOnNavMesh false:" + NavMeshAgent.GetInstanceID() + " :" + NavMeshAgent.name);
            return false;
        }
        return true;
    }

    private void DebugNavMeshAgent(NavMeshAgent NavMeshAgent)
    {
        Debug.Log("NavMeshAgent:" + NavMeshAgent.name 
            + " hasPath:" + NavMeshAgent.hasPath
            + " pathStatus:" + NavMeshAgent.pathStatus 
            + " remainingDistance:" + NavMeshAgent.remainingDistance 
            + " destination:" + NavMeshAgent.destination 
            + " pathPending:" + NavMeshAgent.pathPending
            // + " isPathStale:" + NavMeshAgent.isPathStale
            + _targetGarbage.transform.position
            + _targetGarbage.name
            );
    }

    private bool SetNavMeshDestination(Vector3 destination)
    {
        Vector3 relativePos = destination - this.transform.position;
        this.transform.localRotation = TowerMoveCtrl.GetRotateAngle(relativePos);
        TowerMoveCtrl.SetDestination(destination, _NavMeshAgent);
        if (!IsOnNavMesh(_NavMeshAgent))
        {
            return false;
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

    private bool IsPowerState()
    {
        bool hasPower = ScoreCtrl.IsScorePositiveInt(0, "CLK");
        if (!hasPower)
        {
            SignPowerOutageCtrl.GetOrCreateCirclePowerOutage(this.gameObject);
            return hasPower;
        }
        SignPowerOutageCtrl.UnSignPowerOutage(this.gameObject);
        return hasPower;
    }

    internal void StartDeleteUnitProcess()
    {
        _isDelete = true;
    }

    internal void DeleteUnitProcess()
    {
        // TODO: ユニットを灰色、半透明にする

        // ターゲットを消去する
        // Debug.Log("DeleteUnitProcess" + this.name);

        UnitStruct unitStruct = this.GetComponent<Sweeper>().GetUnitStruct();
        // if (ScoreCtrl.IsScorePositiveInt(unitStruct.DeleteCost, unitStruct.ScoreType))
        // {
            ScoreCtrl.UpdateAndDisplayScore((int)unitStruct.DeleteCost, unitStruct.ScoreType);
            // return true;
        // }
        GameObjectTreat.DestroyAll(_MyDeck);
        GameObjectTreat.DestroyAll(this.gameObject);
    }

    void Update()
    {
        _time += Time.deltaTime;
        if (_time > (double)(_LOOP_TIME / GameSpeedCtrl.GetGameSpeed()) && !_isDelete && IsPowerState())
        {
            _time = 0;
            MoveControl();
        }
    }

}
