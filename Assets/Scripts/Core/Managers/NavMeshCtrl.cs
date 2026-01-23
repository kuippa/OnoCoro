using UnityEngine;
using UnityEngine.AI;

public static class NavMeshCtrl
{
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

    internal static void LookAround(NavMeshAgent NavMeshAgent, Transform transform)
    {
        float f = Mathf.Abs((transform.rotation.eulerAngles.y + 67f) % 360f);
        f = Mathf.RoundToInt(f);
        if (Mathf.Abs(transform.rotation.eulerAngles.y - f) > 5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, f, 0f), 3f);
        }
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
            Debug.Log("SetNavMeshDestination isOnNavMesh false:" + NavMeshAgent.GetInstanceID() + " :" + NavMeshAgent.name);
            return;
        }
        if (NavMeshAgent.pathPending)
        {
            Debug.Log("SetNavMeshDestination pathPending false:" + NavMeshAgent.GetInstanceID() + " :" + NavMeshAgent.name);
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

        NavMeshAgent.speed = 2.6f * GameSpeedCtrl.GetGameSpeed();
        NavMeshAgent.acceleration = 6f * GameSpeedCtrl.GetGameSpeed();
    }

    // TODO: キャラクターごとの移動スピードの制御
    internal static void ChangeAgentSpeed(NavMeshAgent NavMeshAgent, float speed, float acceleration)
    {
        NavMeshAgent.speed = speed * GameSpeedCtrl.GetGameSpeed();
        NavMeshAgent.acceleration = acceleration * GameSpeedCtrl.GetGameSpeed();
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
        if (!NavMeshAgent.pathPending && NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance && (!NavMeshAgent.hasPath || NavMeshAgent.velocity.sqrMagnitude <= 0.8f))
        {
            return true;
        }
        return false;
    }
}
