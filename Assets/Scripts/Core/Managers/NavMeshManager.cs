using UnityEngine;
using UnityEngine.AI;
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

    internal static NavMeshAgent GetNavMeshAgent(GameObject targetObject)
    {
        NavMeshAgent navMeshAgent = targetObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(1);
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

    internal static void SetDestination(Vector3 destination, NavMeshAgent NavMeshAgent)
    {
        if (!NavMeshAgent.isOnNavMesh)
        {
            // Debug.Log("SetNavMeshDestination isOnNavMesh false:" + NavMeshAgent.GetInstanceID() + " :" + NavMeshAgent.name);
            return;
        }
        if (NavMeshAgent.pathPending)
        {
            // Debug.Log("SetNavMeshDestination pathPending false:" + NavMeshAgent.GetInstanceID() + " :" + NavMeshAgent.name);
            return;
        }
        NavMeshAgent.destination = destination;
        NavMeshAgent.autoRepath = true;
    }

    internal static Quaternion GetRotateAngle(Vector3 relativePos)
    {
        if (relativePos == Vector3.zero)
        {
            return Quaternion.identity;
        }
        return Quaternion.LookRotation(new Vector3(relativePos.x, 0f, relativePos.z), Vector3.up);
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

    internal static bool SetNavMeshDestination(NavMeshAgent NavMeshAgent, Vector3 destination, Transform transform)
    {
        Vector3 relativePos = destination - transform.position;
        transform.localRotation = GetRotateAngle(relativePos);
        SetDestination(destination, NavMeshAgent);
        if (!IsOnNavMesh(NavMeshAgent))
        {
            return false;
        }
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
        if (!NavMeshAgent.pathPending && NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance && (!NavMeshAgent.hasPath || NavMeshAgent.velocity.sqrMagnitude <= 0.8f))
        {
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
