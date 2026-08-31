using System;
using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using UnityEngine.AI;
using Debug = CommonsUtility.Debug;

/// <summary>
/// ゴミ敵キャラクター（ゴミを撒き散らすタイプの敵）
/// 期待される子オブジェクト: "CapsuleHead" (表示用), "Hand" (ゴミ生成用)
/// 期待されるコンポーネント: NavMeshAgent (移動制御用), CapsuleCollider or BoxCollider (位置計算用)
/// </summary>
public class EnemyLitter : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private EnemyStatus _myStatus;
    private Vector3[] _myPaths;
    private LitterGarbageSpawner _garbageSpawner;
    private LitterPathManager _pathManager;
    private LitterMovementController _movementController;
    private TowerDestructionHandler _towerDestructionHandler;
    private Renderer _headRenderer;
    private Transform _handTransform;
    private int _childCount;
    
    /// <summary>
    /// パス上のユニット識別用 （EventLoader.NotifyEnemyDeath 用）
    /// SpawnController から RegisterEnemyToPath の戻り値を設定される
    /// </summary>
    private string _pathMarkerSequence = "";

    // Movement Speed Constants
    private const float _AGENT_BASE_SPEED = 1.2f;     // NavMeshAgent の基本移動速度
    private const float _AGENT_MAX_SPEED = 6f;        // NavMeshAgent の最大速度制限
    
    // Tower Destruction Constants
    private const int _MAX_TOWER_DESTRUCTION_COUNT = 1;  // タイムアウト時に破壊するタワーの最大数

    // Numeric Constants
    private const float _UNDEFINED_POSITION_VALUE = -99f;
    private const float _ZERO_THRESHOLD = 0f;
    
    // GameObject Names
    private const string _CHILD_NAME_CAPSULE_HEAD = "CapsuleHead";
    private const string _CHILD_NAME_HAND = "Hand";
    
    // Internal State
    private Vector3 _undefinedPosition = new Vector3(_UNDEFINED_POSITION_VALUE, _UNDEFINED_POSITION_VALUE, _UNDEFINED_POSITION_VALUE);
    private bool _littingMode = true;
    private bool _movingMode = true;

    internal static int _idx;

    internal void CreateLitterUnit(Vector3 position)
    {
        int indexByTag = GameObjectTreat.IndexObjectByTag(this.gameObject.tag);
        name = this.gameObject.tag + indexByTag;
        
        _myStatus = GetEnemyStatus();
        _myStatus.SetEnemyName(name);
        
        transform.position = position;
    }

    private EnemyStatus GetEnemyStatus()
    {
        if (_myStatus == null)
        {
            _myStatus = new EnemyStatus();
        }
        return _myStatus;
    }

    internal void SetThrowOutDirection(GameObject targetObj = null)
    {
        if (targetObj == null)
        {
            _garbageSpawner.SetThrowTarget(null);
            ChangeHeadColor(-1);  // Monitoring カウント -1
            return;
        }

        _garbageSpawner.SetThrowTarget(targetObj);
        ChangeHeadColor(1);  // Monitoring カウント +1
    }

    internal void ChangeHeadColor(int monit)
    {
        _garbageSpawner.UpdateMonitoringCount(monit);
        
        if (_headRenderer == null)
        {
            return;
        }
        
        // 複数箇所からの監視カウントが1以上なら赤色、それ以外は緑色
        Material headMaterial = _garbageSpawner.GetMonitoringCount() > 0
            ? MaterialManager.BGRed
            : MaterialManager.BGGreen;
        _headRenderer.material = headMaterial;
    }


    private IEnumerator LitterDrops()
    {
        while (_littingMode)
        {
            yield return new WaitForSeconds(Litter.GARBAGE_DROP_INTERVAL);

            if (_handTransform == null)
            {
                _littingMode = false;
                yield break;
            }

            if (_garbageSpawner.TryExecuteGarbageDrop(_handTransform, transform.position, transform, _childCount, out int newChildCount))
            {
                _childCount = newChildCount;
            }
            else
            {
                // 生成失敗 → 上限に達したため生成モード終了
                _littingMode = false;
            }
        }
    }

    private IEnumerator MoveAgent()
    {
        while (_movingMode)
        {
            yield return new WaitForSeconds(LitterMovementController._MOVING_CHECK_INTERVAL);

            // 移動状態を更新＆タイムアウト判定
            float agentSpeed = _navMeshAgent.velocity.magnitude;
            if (_movementController.CheckAndUpdateMovementState(agentSpeed, name))
            {
                HandleMovementTimeout();
            }

            // 目的地到達判定
            if (_movementController.HasReachedDestination(_navMeshAgent))
            {
                _movingMode = SetNextPath(_myPaths);
            }
        }
    }

    /// <summary>
    /// 移動タイムアウト時の処理（タワー破壊→次のパス進行）
    /// </summary>
    private void HandleMovementTimeout()
    {
        Debug.Log($"{name}: パスに到達できずタイムアウト。近隣タワーを破壊します");
        _towerDestructionHandler.DestroyNearbyTowers(transform.position, _MAX_TOWER_DESTRUCTION_COUNT, name);

        _movementController.ResetStuckCounter();

        // タイムアウト後は、現在のパスをリセットして経路算定からやり直す
        NavMeshManager.SetDestinationFromIntended(_navMeshAgent);
    }

    private void AgentReachedGoal()
    {
        Debug.Log($"[EnemyLitter.AgentReachedGoal] {name} がゴールに到達しました。パス: {_pathMarkerSequence}");
        
        // 親から外す（パスルートオブジェクトの子要素から削除）
        Transform parent = transform.parent;
        transform.SetParent(null);
        Debug.Log($"[EnemyLitter.AgentReachedGoal] {name} を親から外しました");
        
        // パス完了時に明示的に EventLoader に通知
        // この時点で自分自身は子要素リストから削除されている
        if (!string.IsNullOrEmpty(_pathMarkerSequence))
        {
            EventLoader eventLoader = EventLoader.instance;
            if (eventLoader != null)
            {
                Debug.Log($"[EnemyLitter.AgentReachedGoal] {name} が NotifyEnemyDeath を呼び出します");
                eventLoader.NotifyEnemyDeath(_pathMarkerSequence);
            }
            else
            {
                Debug.LogWarning($"[EnemyLitter.AgentReachedGoal] EventLoader.instance が null です");
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyLitter.AgentReachedGoal] {name} の _pathMarkerSequence が空です");
        }
        
        GameObjectTreat.DestroyAll(gameObject);
    }

    /// <summary>
    /// パス上のユニット管理用マーカーシーケンスを設定
    /// SpawnController から呼び出される
    /// </summary>
    internal void SetPathMarkerSequence(string markerSequence)
    {
        _pathMarkerSequence = markerSequence;
        Debug.Log($"[EnemyLitter.SetPathMarkerSequence] {name} のパスマーカーシーケンスを設定: {markerSequence}");
    }

    private void AgentJumpToStartPosition()
    {
        if (_myPaths == null || _myPaths.Length == 0)
        {
            return;
        }

        Vector3 startPosition = _myPaths[0];
        float bottomOffset = GetBottomOffset();
        Vector3 warpPosition = new Vector3(startPosition.x, startPosition.y + bottomOffset, startPosition.z);

        // transform.position の直接書き換えでは NavMeshAgent が NavMesh に再登録されない。
        // Warp() を使うことで、エージェントが指定位置の NavMesh に即座にスナップされ
        // isOnNavMesh = true となり、直後の SetDestination が成功する。
        bool warped = _navMeshAgent.Warp(warpPosition);
        if (!warped)
        {
        }

        // 初期スポーン位置を SpawnOriginTracker に記録する。
        // NarakuTriggerHandler が池等で DEM 検出全失敗時にここへ戻すために使用する。
        SpawnOriginTracker spawnTracker = GameObjectTreat.GetOrAddComponent<SpawnOriginTracker>(gameObject);
        spawnTracker.SetSpawnOrigin(warpPosition);
    }

    /// <summary>
    /// ピボットからコライダー底面までのローカルオフセットを返す
    /// _myPaths[0] が底面位置になるよう transform.position を補正するために使用
    /// </summary>
    private float GetBottomOffset()
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            // CapsuleCollider: ピボットから底面 = height/2 - center.y
            return capsuleCollider.height / 2f - capsuleCollider.center.y;
        }

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            // BoxCollider: ピボットから底面 = size.y/2 - center.y
            return boxCollider.size.y / 2f - boxCollider.center.y;
        }

        return 0f;
    }

    private bool SetNextPath(Vector3[] paths)
    {
        if (paths == null || paths.Length == 0)
        {
            Debug.Log($"{name}: No more paths to follow. Agent has reached the final destination.");
            AgentReachedGoal();
            return false;
        }

        Vector3 destination = paths[0];
        SetDestination(destination);

        Debug.Log($"{name}: Next path set to {destination}. Remaining paths: {paths.Length - 1}");
        
        List<Vector3> remainingPaths = new List<Vector3>(paths);
        remainingPaths.RemoveAt(0);
        _myPaths = remainingPaths.ToArray();
        
        return true;
    }

    /// <summary>
    /// 移動パスに新しい経由地点を挿入し、現在の移動を中断・再開する
    /// 
    /// 指定された位置を現在のパス最前に挿入し、直ちにそこへ向かう。
    /// 例えばゲーム内イベント発生時に敵の進路を動的に変更する場合に使用する。
    /// </summary>
    /// <param name="path">新しく追加する経由地点の座標</param>
    internal void AddPathAndInterrupt(Vector3 path)
    {
        if (_myPaths == null)
        {
            return;
        }

        List<Vector3> pathList = new List<Vector3>(_myPaths);
        
        // Early Return: skip if path already exists
        if (pathList.Count > 0 && pathList[0] == path)
        {
            return;
        }

        if (NavMeshManager.IsSameDestination(_navMeshAgent, path))
        {
            return;
        }

        pathList.Insert(0, path);
        _myPaths = pathList.ToArray();
        
        SetNextPath(_myPaths);
    }

    internal void SetPaths(string[] markerNames = null)
    {
        _myPaths = _pathManager.GeneratePathsFromMarkers(markerNames);
    }

    private void SetDestination(Vector3 destination)
    {
        if (NavMeshManager.IsSameDestination(_navMeshAgent, destination))
        {
            return;
        }
        NavMeshManager.SetDestination(destination, _navMeshAgent);
    }


    private void Awake()
    {
        _navMeshAgent = NavMeshManager.GetNavMeshAgent(gameObject);
        if (_navMeshAgent == null)
        {
            enabled = false;
            return;
        }

        // CapsuleHead Renderer をキャッシュ
        Transform capsuleHeadTransform = transform.Find(_CHILD_NAME_CAPSULE_HEAD);
        if (capsuleHeadTransform == null)
        {
            enabled = false;
            return;
        }
        
        _headRenderer = capsuleHeadTransform.GetComponent<Renderer>();
        if (_headRenderer == null)
        {
            enabled = false;
            return;
        }

        // Hand Transform をキャッシュ
        _handTransform = transform.Find(_CHILD_NAME_HAND);
        if (_handTransform == null)
        {
            enabled = false;
            return;
        }

        _garbageSpawner = new LitterGarbageSpawner();
        _pathManager = new LitterPathManager();
        _movementController = new LitterMovementController();
        _towerDestructionHandler = new TowerDestructionHandler();
        _idx++;
    }

    internal void InitUnitSpawn(string[] markerNames = null)
    {
        SetPaths(markerNames);
        
        // パスが有効か確認
        if (_myPaths == null || _myPaths.Length == 0)
        {
            return;
        }
        AgentJumpToStartPosition();
        SetNextPath(_myPaths);
        
        // TODO: キャラクターごとの移動スピードの制御
        NavMeshManager.ChangeAgentSpeed(_navMeshAgent, _AGENT_BASE_SPEED, _AGENT_MAX_SPEED);

        StartCoroutine(LitterDrops());
        StartCoroutine(MoveAgent());
    }

}
