using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using UnityEngine.AI;
using Debug = CommonsUtility.Debug;

/// <summary>
/// 巨大猫の敵ユニット（PLATEAU CityHack 2026）
///
/// EnemyLitter を下敷きにした、指定経路を移動する敵ユニット。
/// ゴミを撒く代わりに、近づいた建物を解体（更地化）して廃材を出す。
///
/// 期待されるコンポーネント: NavMeshAgent（移動制御用）
/// 期待される子オブジェクト: なし
///   ※ EnemyLitter は CapsuleHead / Hand が無いと自身を無効化するが、
///     猫は見た目のモデル構成を問わないため必須の子オブジェクトを持たせていない
/// </summary>
public class EnemyCat : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private EnemyStatus _myStatus;
    private Vector3[] _myPaths;
    private LitterPathManager _pathManager;
    private LitterMovementController _movementController;
    private PlateauInfoManager _plateauInfoManager;

    /// <summary>
    /// パス上のユニット識別用（EventLoader.NotifyEnemyDeath 用）
    /// SpawnController から RegisterEnemyToPath の戻り値を設定される
    /// </summary>
    private string _pathMarkerSequence = "";

    // 移動速度（猫は大きいので Litter より速くしている）
    private const float _AGENT_BASE_SPEED = 2.0f;
    private const float _AGENT_MAX_SPEED = 6f;

    private const string _PLATEAU_OBJECT_NAME = "Plateau";

    /// <summary>
    /// マーカー座標を NavMesh 上へ吸着させるときの探索半径(m)。
    /// 手で置いたマーカーが道からずれていても拾えるよう広めにしてある
    /// </summary>
    private const float _NAVMESH_SNAP_DISTANCE = 30f;

    private bool _movingMode = true;
    private bool _demolishMode = true;
    private int _demolishedCount = 0;

    internal static int _idx;

    private void Awake()
    {
        _navMeshAgent = NavMeshManager.GetNavMeshAgent(gameObject);
        if (_navMeshAgent == null)
        {
            Debug.LogWarning($"[EnemyCat] NavMeshAgent が無いため無効化します: {name}");
            enabled = false;
            return;
        }

        // プレファブに手で付けた NavMeshAgent は Humanoid になりがちで、
        // 別タイプでベイクされた NavMesh に乗れず動けなくなるため合わせる
        NavMeshManager.AlignAgentType(_navMeshAgent, name);

        _pathManager = new LitterPathManager();
        _movementController = new LitterMovementController();
        _idx++;
    }

    internal void CreateCatUnit(Vector3 position)
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

    /// <summary>
    /// パス上のユニット管理用マーカーシーケンスを設定（SpawnController から呼ぶ）
    /// </summary>
    internal void SetPathMarkerSequence(string markerSequence)
    {
        _pathMarkerSequence = markerSequence;
    }

    internal void InitUnitSpawn(string[] markerNames = null)
    {
        SetPaths(markerNames);

        if (_myPaths == null || _myPaths.Length == 0)
        {
            Debug.LogWarning($"[EnemyCat] パスマーカーが見つからないため初期化に失敗しました: {name}");
            return;
        }

        AgentJumpToStartPosition();

        // ここで NavMesh に乗れていないと以降まったく動かないため、先に切り分ける
        if (!_navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning(
                $"[EnemyCat] {name}: NavMesh に乗れていません（現在地 {transform.position}）。"
                + "NavMesh がベイクされているか、出発マーカーが陸上にあるか確認してください");
        }

        SetNextPath(_myPaths);
        NavMeshManager.ChangeAgentSpeed(_navMeshAgent, _AGENT_BASE_SPEED, _AGENT_MAX_SPEED);

        StartCoroutine(MoveAgent());
        StartCoroutine(DemolishNearbyBuildings());
    }

    internal void SetPaths(string[] markerNames = null)
    {
        _myPaths = _pathManager.GeneratePathsFromMarkers(markerNames);
    }

    // =============================================
    // 建物の解体
    // =============================================

    /// <summary>
    /// 一定間隔で周囲の建物を探し、最も近い 1 棟を解体する。
    ///
    /// 建物は NavMesh の障害物なので猫が物理的に接触するとは限らない。
    /// そのため衝突判定ではなく近接判定にしている（経路脇の建物も壊せる）。
    /// 1 回につき 1 棟に絞っているのは、瓦礫の生成負荷を分散させるため
    /// </summary>
    private IEnumerator DemolishNearbyBuildings()
    {
        while (_demolishMode)
        {
            yield return new WaitForSeconds(Cat.DEMOLISH_INTERVAL);

            if (_demolishedCount >= Cat.MAX_DEMOLISH_COUNT)
            {
                Debug.Log($"[EnemyCat] {name}: 解体上限 {Cat.MAX_DEMOLISH_COUNT} 棟に達したため解体を終了します");
                _demolishMode = false;
                yield break;
            }

            GameObject target = FindNearestBuilding();
            if (target == null)
            {
                continue;
            }
            DemolishBuilding(target);
        }
    }

    /// <summary>
    /// 半径内で最も近い PLATEAU 建物を返す（無ければ null）
    /// </summary>
    private GameObject FindNearestBuilding()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, Cat.DEMOLISH_RADIUS);
        GameObject nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            if (!PlateauUtility.IsPlateauBuilding(collider))
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance >= nearestDistance)
            {
                continue;
            }
            nearestDistance = distance;
            nearest = collider.gameObject;
        }
        return nearest;
    }

    private void DemolishBuilding(GameObject building)
    {
        PlateauInfoManager infoManager = GetPlateauInfoManager();
        if (infoManager == null)
        {
            Debug.LogWarning("[EnemyCat] PlateauInfoManager が見つからないため解体できません");
            _demolishMode = false;
            return;
        }

        float debrisTons = infoManager.DemolishBuilding(building);
        if (debrisTons <= 0f)
        {
            return;
        }
        _demolishedCount = _demolishedCount + 1;
    }

    private PlateauInfoManager GetPlateauInfoManager()
    {
        if (_plateauInfoManager != null)
        {
            return _plateauInfoManager;
        }

        GameObject plateauObject = GameObject.Find(_PLATEAU_OBJECT_NAME);
        if (plateauObject == null)
        {
            return null;
        }
        _plateauInfoManager = plateauObject.GetComponent<PlateauInfoManager>();
        return _plateauInfoManager;
    }

    // =============================================
    // 移動（EnemyLitter と同じ仕組み）
    // =============================================

    private IEnumerator MoveAgent()
    {
        while (_movingMode)
        {
            yield return new WaitForSeconds(LitterMovementController._MOVING_CHECK_INTERVAL);

            float agentSpeed = _navMeshAgent.velocity.magnitude;
            if (_movementController.CheckAndUpdateMovementState(agentSpeed, name))
            {
                HandleMovementTimeout();
            }

            if (_movementController.HasReachedDestination(_navMeshAgent))
            {
                _movingMode = SetNextPath(_myPaths);
            }
        }
    }

    /// <summary>
    /// 移動タイムアウト時の処理。
    /// Litter はタワーを破壊するが、猫は目の前の建物を壊して進む
    /// </summary>
    private void HandleMovementTimeout()
    {
        Debug.LogWarning($"[EnemyCat] {name}: 目的地へ到達できません。{DescribeAgentState()}");

        GameObject target = FindNearestBuilding();
        if (target != null)
        {
            Debug.Log($"[EnemyCat] {name}: 進めないため目の前の建物を解体します");
            DemolishBuilding(target);
        }

        _movementController.ResetStuckCounter();

        // 同じ目的地に粘っても届かないことがある（経路が分断されている等）。
        // 建物を壊しても駄目なら次の地点へ切り替えて、止まったままにしない
        _movingMode = SetNextPath(_myPaths);
    }

    /// <summary>
    /// 次の目的地を設定する。
    /// NavMesh に載らない地点は飛ばして先へ進む（マーカーが海上や建物内にある場合）
    /// </summary>
    private bool SetNextPath(Vector3[] paths)
    {
        List<Vector3> remainingPaths = new List<Vector3>();
        if (paths != null)
        {
            remainingPaths.AddRange(paths);
        }

        while (remainingPaths.Count > 0)
        {
            Vector3 destination = remainingPaths[0];
            remainingPaths.RemoveAt(0);

            if (TrySetDestination(destination))
            {
                _myPaths = remainingPaths.ToArray();
                return true;
            }
        }

        _myPaths = new Vector3[0];
        AgentReachedGoal();
        return false;
    }

    /// <summary>
    /// 目的地を NavMesh 上へ吸着させてから設定する。
    /// 近くに NavMesh が無ければ false（呼び出し側でその地点を飛ばす）
    /// </summary>
    private bool TrySetDestination(Vector3 destination)
    {
        if (!NavMeshManager.TrySnapToNavMesh(destination, out Vector3 snapped, _NAVMESH_SNAP_DISTANCE))
        {
            Debug.LogWarning(
                $"[EnemyCat] {name}: {destination} の周囲 {_NAVMESH_SNAP_DISTANCE}m に NavMesh が見つかりません。"
                + "この地点を飛ばします（マーカーが道から離れすぎている可能性）");
            return false;
        }

        if (NavMeshManager.IsSameDestination(_navMeshAgent, snapped))
        {
            return true;
        }
        NavMeshManager.SetDestination(snapped, _navMeshAgent);
        return true;
    }

    /// <summary>
    /// 詰まった理由を切り分けるための状態文字列
    /// </summary>
    private string DescribeAgentState()
    {
        return $"経路状態={_navMeshAgent.pathStatus} 残り距離={_navMeshAgent.remainingDistance:F1}m"
            + $" NavMesh上={_navMeshAgent.isOnNavMesh} 現在地={transform.position} 目的地={_navMeshAgent.destination}";
    }

    private void AgentReachedGoal()
    {
        Debug.Log($"[EnemyCat] {name} がゴールに到達しました（解体 {_demolishedCount} 棟）");
        _demolishMode = false;

        // 親（パスルートオブジェクト）から外してから通知する
        transform.SetParent(null);

        if (!string.IsNullOrEmpty(_pathMarkerSequence))
        {
            EventLoader eventLoader = EventLoader.instance;
            if (eventLoader != null)
            {
                eventLoader.NotifyEnemyDeath(_pathMarkerSequence);
            }
        }

        GameObjectTreat.DestroyAll(gameObject);
    }

    private void AgentJumpToStartPosition()
    {
        if (_myPaths == null || _myPaths.Length == 0)
        {
            return;
        }

        Vector3 startPosition = _myPaths[0];

        // 出発点が道から外れていると最初から動けないので NavMesh 上へ寄せる
        if (!NavMeshManager.TrySnapToNavMesh(startPosition, out startPosition, _NAVMESH_SNAP_DISTANCE))
        {
            Debug.LogWarning(
                $"[EnemyCat] {name}: 出発マーカー {_myPaths[0]} の周囲に NavMesh がありません。"
                + "そのまま配置しますが移動できない可能性があります");
        }

        float bottomOffset = GetBottomOffset();
        Vector3 warpPosition = new Vector3(startPosition.x, startPosition.y + bottomOffset, startPosition.z);

        // transform.position の直接書き換えでは NavMeshAgent が NavMesh に再登録されない。
        // Warp() で NavMesh にスナップさせてから SetDestination する
        _navMeshAgent.Warp(warpPosition);

        SpawnOriginTracker spawnTracker = GameObjectTreat.GetOrAddComponent<SpawnOriginTracker>(gameObject);
        spawnTracker.SetSpawnOrigin(warpPosition);
    }

    /// <summary>
    /// ピボットからコライダー底面までのローカルオフセットを返す。
    ///
    /// 猫のプレファブはモデルを子に持つ構成が多く、ルートにコライダーが無いことがある。
    /// そのため子オブジェクトも探す（EnemyLitter はルートのみ見ている）
    /// </summary>
    private float GetBottomOffset()
    {
        CapsuleCollider capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            return capsuleCollider.height / 2f - capsuleCollider.center.y;
        }

        BoxCollider boxCollider = GetComponentInChildren<BoxCollider>();
        if (boxCollider != null)
        {
            return boxCollider.size.y / 2f - boxCollider.center.y;
        }
        return 0f;
    }
}
