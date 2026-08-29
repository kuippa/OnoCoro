using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Debug = CommonsUtility.Debug;

public static class NavMeshManager
{
    // 推定値 - NavMesh 乱れに対応した値
    private const float _MIN_DISTANCE = 0.8f;
    private const float _MIN_VELOCITY = 0.8f;
    private const float _ROTATE_BUFFER_ANGLE = 5f;
    private const float _ROTATE_SPEED = 3f;
    private const float _MOVE_SPEED = 3f;
    private const float _ROTATE_ANGLE = 67f;

    // エージェントごとの意図した目的地を記録（キー：NavMeshAgent.GetInstanceID()）
    private static Dictionary<int, Vector3> _intendedDestinations = new Dictionary<int, Vector3>();
    
    /// <summary>
    /// ユニットが使うエージェントタイプの設定インデックス。
    /// NavMesh はこのタイプでベイクされている（Humanoid ではない）
    /// </summary>
    private const int _UNIT_AGENT_SETTING_INDEX = 1;

    /// <summary>
    /// エージェントタイプをベイク済み NavMesh に合わせる。
    ///
    /// プレファブに手で NavMeshAgent を付けると既定の Humanoid になるが、
    /// 本プロジェクトの NavMesh は別のエージェントタイプでベイクされているため、
    /// そのままだと NavMesh に乗れず速度 0 のまま一歩も動けない
    /// </summary>
    internal static void AlignAgentType(NavMeshAgent navMeshAgent, string ownerName = "")
    {
        if (navMeshAgent == null)
        {
            return;
        }

        int expectedTypeId = NavMesh.GetSettingsByIndex(_UNIT_AGENT_SETTING_INDEX).agentTypeID;
        if (navMeshAgent.agentTypeID == expectedTypeId)
        {
            return;
        }

        Debug.LogWarning(
            $"[NavMeshManager] {ownerName}: エージェントタイプが NavMesh と不一致のため補正します"
            + $"（{navMeshAgent.agentTypeID} → {expectedTypeId}）");

        // agentTypeID の変更は無効化してから行わないと反映されない
        bool wasEnabled = navMeshAgent.enabled;
        navMeshAgent.enabled = false;
        navMeshAgent.agentTypeID = expectedTypeId;
        navMeshAgent.enabled = wasEnabled;
    }

    internal static NavMeshAgent GetNavMeshAgent(GameObject targetObject)
    {
        NavMeshAgent navMeshAgent = targetObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(_UNIT_AGENT_SETTING_INDEX);
            navMeshAgent = targetObject.AddComponent<NavMeshAgent>();
            navMeshAgent.enabled = false;
            navMeshAgent.agentTypeID = settingsByIndex.agentTypeID;
            navMeshAgent.speed = 3f;
            navMeshAgent.angularSpeed = 80f;
            navMeshAgent.autoBraking = true;
            navMeshAgent.radius = 0.5f;
            navMeshAgent.height = 2f;
            navMeshAgent.areaMask = 1;
            navMeshAgent.stoppingDistance = 0.55f;
            navMeshAgent.enabled = true;
        }
        return navMeshAgent;
    }

    internal static Vector3 GetDestination(NavMeshAgent NavMeshAgent)
    {
        return NavMeshAgent.destination;
    }

    internal static void ClearDestination(NavMeshAgent NavMeshAgent)
    {
        if (NavMeshAgent.isOnNavMesh)
        {
            NavMeshAgent.destination = NavMeshAgent.transform.position;
            NavMeshAgent.isStopped = true;
            NavMeshAgent.ResetPath();
        }
    }

    internal static void MoveControl(GameObject targetObject)
    {
    }

    /// <summary>
    /// 周囲探索時の目標回転角度を計算（Y軸回転）
    /// </summary>
    /// <param name="currentYAngle">現在の Y 軸回転角度</param>
    /// <param name="rotationAngle">追加回転角度</param>
    /// <returns>目標回転 Quaternion</returns>
    internal static Quaternion CalculateLookAroundRotation(float currentYAngle, float rotationAngle)
    {
        float targetYAngle = Mathf.Repeat(currentYAngle + rotationAngle, 360f);
        // Debug.Log("[NavMeshManager] CalculateLookAroundRotation: currentYAngle=" + currentYAngle + " rotationAngle=" + rotationAngle + " targetYAngle=" + targetYAngle);
        return Quaternion.Euler(0, targetYAngle, 0);
    }

    private static void MoveForward(NavMeshAgent NavMeshAgent, Vector3 unitPosition)
    {
        Vector3 destination = GetDestination(NavMeshAgent);
        if (Vector3.Distance(unitPosition, destination) < 0.8f)
        {
            ClearDestination(NavMeshAgent);
        }
    }

    internal static void SetDestinationFromIntended(NavMeshAgent NavMeshAgent)
    {
        if (NavMeshAgent == null)
        {
            return;
        }
        
        int agentId = NavMeshAgent.GetInstanceID();
        if (!_intendedDestinations.ContainsKey(agentId))
        {
            return;
        }
        
        SetDestination(_intendedDestinations[agentId], NavMeshAgent);
    }

    internal static void SetDestination(Vector3 destination, NavMeshAgent NavMeshAgent)
    {
        if (!IsOnNavMesh(NavMeshAgent))
        {
            return;
        }

        if (NavMeshAgent.pathPending)
        {
            return;
        }
        
        // 意図した目的地を記録（NavMeshAgent.destination がセット失敗しても保持）
        int agentId = NavMeshAgent.GetInstanceID();
        _intendedDestinations[agentId] = destination;
        
        NavMeshAgent.destination = destination;
        NavMeshAgent.autoRepath = true;
    }

    internal static void SetAgentSpeed(NavMeshAgent NavMeshAgent)
    {
        // TODO: 固定値をキャラクター別にもつ

        NavMeshAgent.speed = 2.6f * GameSpeedManager.GetGameSpeed();
        NavMeshAgent.acceleration = 6f * GameSpeedManager.GetGameSpeed();
    }

    // TODO: キャラクターごとの移動スピードの制御
    internal static void ChangeAgentSpeed(NavMeshAgent NavMeshAgent, float speed, float acceleration)
    {
        NavMeshAgent.speed = speed * GameSpeedManager.GetGameSpeed();
        NavMeshAgent.acceleration = acceleration * GameSpeedManager.GetGameSpeed();
    }

    internal static bool IsSameDestination(NavMeshAgent NavMeshAgent, Vector3 destination)
    {
        if (GetDestination(NavMeshAgent) == destination)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 指定座標に最も近い NavMesh 上の点を探す。
    ///
    /// マーカーを道の中心から少し外れた位置に置いても経路を作れるようにするため。
    /// 海上や建物の中など、周囲に NavMesh が無い場所では false を返す
    /// </summary>
    internal static bool TrySnapToNavMesh(Vector3 position, out Vector3 snapped, float maxDistance)
    {
        snapped = position;
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            return false;
        }
        snapped = hit.position;
        return true;
    }

    private static bool IsOnNavMesh(NavMeshAgent NavMeshAgent)
    {
        if (!NavMeshAgent.isOnNavMesh)
        {
            return false;
        }
        return true;
    }

    internal static bool HasReachedDestination(NavMeshAgent NavMeshAgent)
    {
        if (NavMeshAgent == null)
        {
            return false;
        }
        if (!NavMeshAgent.isOnNavMesh)
        {
            return false;
        }

        int agentId = NavMeshAgent.GetInstanceID();
        
        if (NavMeshAgent.pathPending)
        {
            return false;
        }

        // [重要] パスがない = 経路計算失敗 → 到達ではなく失敗
        if (!NavMeshAgent.hasPath)
        {
            if (!(NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance))
            {
                return false;
            }
        }

        // 意図した目的地が記録されているか確認
        if (!_intendedDestinations.ContainsKey(agentId))
        {
            return false;
        }

        Vector3 destination = GetDestination(NavMeshAgent);
        Vector3 intendedDestination = _intendedDestinations[agentId];

        // 意図した目的地と実際の目的地の距離がstoppingDistance以上なら到達していない
        if (Vector3.Distance(intendedDestination, destination) > NavMeshAgent.stoppingDistance)
        {
            // Debug.Log("HasReachedDestination: Destination mismatch (intended: " 
            //     + intendedDestination + ", actual: " + destination + ") for " 
            //     + NavMeshAgent.name + " Distance: " + Vector3.Distance(intendedDestination, destination) 
            //     + " " + NavMeshAgent.stoppingDistance);
            return false;
        }

        if (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
        {
            Debug.Log("HasReachedDestination reached:" + destination + " :" + NavMeshAgent.name);
            return true;
        }

        return false;
    }

    /// <summary>
    /// NavMesh Carving を有効化（障害物をリアルタイムで NavMesh から除外）
    /// </summary>
    /// <param name="obstacleObject">Carving を適用するオブジェクト</param>
    internal static void EnableCarvingForObstacle(GameObject obstacleObject)
    {
        if (obstacleObject == null)
        {
            Debug.LogWarning("Obstacle object is null");
            return;
        }

        NavMeshObstacle obstacle = obstacleObject.GetComponent<NavMeshObstacle>();
        if (obstacle == null)
        {
            obstacle = obstacleObject.AddComponent<NavMeshObstacle>();
        }

        obstacle.carving = true;
        obstacle.shape = NavMeshObstacleShape.Box;
        Debug.Log($"[NavMeshManager] Carving enabled for: {obstacleObject.name}");
    }

    /// <summary>
    /// NavMesh Carving を無効化
    /// </summary>
    /// <param name="obstacleObject">Carving を無効化するオブジェクト</param>
    internal static void DisableCarvingForObstacle(GameObject obstacleObject)
    {
        if (obstacleObject == null)
        {
            return;
        }

        NavMeshObstacle obstacle = obstacleObject.GetComponent<NavMeshObstacle>();
        if (obstacle != null)
        {
            obstacle.carving = false;
            Debug.Log($"[NavMeshManager] Carving disabled for: {obstacleObject.name}");
        }
    }
}
