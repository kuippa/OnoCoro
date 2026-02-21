using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.UI;
using UnityEngine.Events;
using CommonsUtility;
using UnityEngine.AI;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;   // NavMeshAgentを使うために必要
using System.Linq;

public class TowerSweeperCtrl : MonoBehaviour
{
    private GameObject _MyDeck = null;
    private NavMeshAgent _NavMeshAgent = null;
    private const float _TARGET_DEL_DISTANCE = 1.2f;    // 消し込みの最低距離 ヘッド2.2

    private double _time = 0;            // ループ経過時間

    // [SerializeField]
    internal float _LOOP_TIME = 0.6f;      // ループ時間
    internal float _MOVE_SPEED = 3f;    // 移動速度
    // public float _ROTATE_ANGLE = 78;  // 回転角度

    private bool _isDelete = false; // 削除処理実行中かどうか

    // マネージャー・サービス
    private SweeperBatteryManager _batteryManager = null;
    private SweeperTargetingService _targetingService = null;
    private AINavigationManager _navigationManager = null;

    public void CreateSweeperUnit(Vector3 setPoint)
    {
        this.tag = GameEnum.TagType.TowerSweeper.ToString();
        int idx = GameObjectTreat.IndexObjectByTag(this.tag);
        this.name = GameEnum.ModelsType.Sweeper.ToString() + idx.ToString();
        this.AddComponent<Sweeper>();
        this.GetComponent<Sweeper>()._item_struct.ItemID = this.name;
        this.GetComponent<Sweeper>()._unit_struct.UnitID = this.name;

        this.transform.position = setPoint;
        GameObject TowerDockPrefab = PrefabManager.TowerDockPrefab;
        if (TowerDockPrefab == null)
        {
            Debug.LogWarning("TowerDock prefab not found in PrefabManager");
            return;
        }
        GameObject TowerDock = Instantiate(TowerDockPrefab);
        setPoint.x = setPoint.x + this.transform.localScale.x / 2 + 0.1f;
        TowerDock.transform.position = setPoint;
        TowerDock.tag = GameEnum.TagType.TowerDock.ToString();
        TowerDock.name = TowerDock.tag + idx;
        _MyDeck = TowerDock;
        if (_NavMeshAgent != null)
        {
            _NavMeshAgent.enabled = true;
        }

        // バッテリーマネージャーを初期化
        _batteryManager = this.GetComponent<SweeperBatteryManager>();
        if (_batteryManager == null)
        {
            _batteryManager = this.gameObject.AddComponent<SweeperBatteryManager>();
        }
        _batteryManager.Initialize(_MyDeck, _NavMeshAgent, this);

        // ターゲティングサービスを初期化
        _targetingService = this.GetComponent<SweeperTargetingService>();
        if (_targetingService == null)
        {
            _targetingService = this.gameObject.AddComponent<SweeperTargetingService>();
        }
        _targetingService.Initialize(this.transform);

        // ナビゲーションマネージャーを初期化
        _navigationManager = this.GetComponent<AINavigationManager>();
        if (_navigationManager == null)
        {
            _navigationManager = this.gameObject.AddComponent<AINavigationManager>();
        }
        _navigationManager.Initialize(_NavMeshAgent, _MOVE_SPEED, this.transform);
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
    }

    /// <summary>
    /// Garbage タグの Collider が進入した時に呼ばれます
    /// TowerSweeperWatchTriggerHandler から呼び出されます
    /// </summary>
    /// <param name="other">進入した Collider</param>
    internal void OnGarbageEnter(Collider other)
    {
        if (other == null || other.gameObject == null)
        {
            return;
        }

        if (_targetingService == null)
        {
            Debug.LogWarning("[TowerSweeperCtrl] TargetingService is null");
            return;
        }

        GameObjectTreat.DebugColorChange(other.gameObject, Color.red);
        _targetingService.OnGarbageEnter(other);
    }

    /// <summary>
    /// Ash タグの Collider が進入した時に呼ばれます
    /// TowerSweeperWatchTriggerHandler から呼び出されます
    /// </summary>
    /// <param name="other">進入した Collider</param>
    internal void OnAshEnter(Collider other)
    {
        if (other == null || other.gameObject == null)
        {
            return;
        }

        if (_targetingService == null)
        {
            Debug.LogWarning("[TowerSweeperCtrl] TargetingService is null");
            return;
        }

        GameObjectTreat.DebugColorChange(other.gameObject, Color.red);
        _targetingService.OnAshEnter(other);
    }

    void MoveControl()
    {
        // バッテリーのチェック
        if (_batteryManager == null)
        {
            Debug.LogWarning("[TowerSweeperCtrl] BatteryManager is null");
            return;
        }
        if (!_batteryManager.CheckBattery())
        {
            return;
        }

        if (_navigationManager == null)
        {
            Debug.LogWarning("[TowerSweeperCtrl] NavigationManager is null");
            return;
        }

        if (_targetingService == null)
        {
            Debug.LogWarning("[TowerSweeperCtrl] TargetingService is null");
            return;
        }

        // ターゲットの取得
        GameObject targetGarbage = _targetingService.GetBestTarget();
        if (targetGarbage == null)
        {
            // ターゲットがない場合、周囲探索
            _navigationManager.LookAround();
            return;
        }

        // 目的地への移動
        Vector3 destination = targetGarbage.transform.position;
        if (!_navigationManager.MoveToTarget(destination, this.transform))
        {
            Debug.Log("MoveToTarget failed: " + destination);
            _targetingService.IgnoreCurrentTarget();
            return;
        }

        // 移動状態を更新
        _navigationManager.UpdateMovement(_targetingService);
    }

    internal void ClearIgnoreGarbageLists()
    {
        if (_targetingService != null)
        {
            _targetingService.ClearIgnoreList();
        }
    }

    private bool IsPowerState()
    {
        bool hasPower = ScoreCtrl.IsScorePositiveInt(0, "CLK");
        if (!hasPower)
        {
            SignPowerOutageController.GetOrCreateCirclePowerOutage(this.gameObject);
            return hasPower;
        }
        SignPowerOutageController.UnSignPowerOutage(this.gameObject);
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
        if (_time > (double)(_LOOP_TIME / GameSpeedManager.GetGameSpeed()) && !_isDelete && IsPowerState())
        {
            _time = 0;
            MoveControl();
        }
    }

}
