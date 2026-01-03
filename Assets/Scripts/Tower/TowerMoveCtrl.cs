using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class TowerMoveCtrl
{
    private const float _MIN_DISTANCE = 0.8f;  // 最小距離
    private const float _ROTATE_BUFFER_ANGLE = 5;
    private const float _ROTATE_SPEED = 3f;  // 回転速度
    private const float _ROTATE_ANGLE = 67;  // 回転角度(クオータニアンを用いた角度制限のとき)
    // private const float _ROTATE_ANGLE = 123;  // 回転角度(クオータニアンを用いた角度制限のとき)
    // public float _ROTATE_ANGLE = 273;  // 回転角度


    internal static Vector3 GetDestination(NavMeshAgent NavMeshAgent)
    {
        return NavMeshAgent.destination;
    }

    internal static void ClearDestination(NavMeshAgent NavMeshAgent)
    {
        // if (NavMeshAgent.isOnNavMesh)
        // {
        NavMeshAgent.destination = NavMeshAgent.transform.position;
        NavMeshAgent.isStopped = true;
        NavMeshAgent.ResetPath();
        // }
    }

    internal static void MoveControl(GameObject targetObject)
    {

    }


    internal static void LookAround(NavMeshAgent NavMeshAgent, Transform transform)
    {
        float rotate_org_angle = transform.rotation.eulerAngles.y;
        float rotate_angle = Mathf.Abs((rotate_org_angle + _ROTATE_ANGLE) % 360f);
        rotate_angle = (float)Mathf.RoundToInt(rotate_angle);
        if (Mathf.Abs(transform.rotation.eulerAngles.y - rotate_angle) > _ROTATE_BUFFER_ANGLE)
        {
            // 制限 - 単体のクォータ二オンでは、180度を超す回転を表すことができません。
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, rotate_angle, 0), _ROTATE_SPEED);
        }
        // MoveForward(NavMeshAgent, transform.position);
    }

    private static void MoveForward(NavMeshAgent NavMeshAgent, Vector3 unitPosition)
    {
        Vector3 destination  = GetDestination(NavMeshAgent);
        // if (destination.x == this.transform.position.x && destination.z == this.transform.position.z)
        // {
        //     destination = this.transform.position + this.transform.forward * _MOVE_SPEED;
        //     SetDestination(destination, NavMeshAgent);
        // }
        // else 
        if (Vector3.Distance(unitPosition, destination) < _MIN_DISTANCE)
        {
            ClearDestination(NavMeshAgent);
            // _moveMode = false;
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
            // pathPending	経路探索の準備ができているかどうか（読み取り専用）
            Debug.Log("SetNavMeshDestination pathPending false:" + NavMeshAgent.GetInstanceID() + " :" + NavMeshAgent.name);
            return;
        }

        NavMeshAgent.destination = destination;
        // autoRepath	エージェントが移動先に着いたり、途中で破棄された場合、新しいパスを取得する必要があるかどうか
        NavMeshAgent.autoRepath = true;
    }

    internal static Quaternion GetRotateAngle(Vector3 relativePos)
    {
        Quaternion rotation;
        if (relativePos == Vector3.zero)
        {
            rotation = Quaternion.identity;
        }
        else
        {
            rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        }
        return rotation;
    }



}
