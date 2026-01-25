using System;
using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

public class EnemyLitter : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private EnemyStatus _myStatus;
    private int _childCount;
    private Vector3[] _myPaths;

    // Timing Constants
    private const float _LITTER_CHECK_INTERVAL = 2.5f;  // デバッグ: 間隔を広げてゴミの軌跡を確認しやすく
    private const float _MOVING_CHECK_INTERVAL = 1.5f;
    private const float _THROW_ANGLE_DEG = 65f;

    // Numeric Constants
    private const int _MAX_LITTERS = 20;
    private const float _UNDEFINED_POSITION_VALUE = -99f;
    private const float _THROW_SPEED_DEFAULT = 4.43f;
    private const float _GRAVITY_ACCELERATION = 9.8f;
    private const float _ZERO_THRESHOLD = 0f;
    
    // GameObject Names
    private const string _CHILD_NAME_CAPSULE_HEAD = "CapsuleHead";
    private const string _CHILD_NAME_HAND = "Hand";
    
    // Internal State
    private Vector3 _undefinedPosition = new Vector3(_UNDEFINED_POSITION_VALUE, _UNDEFINED_POSITION_VALUE, _UNDEFINED_POSITION_VALUE);
    private Vector3 _targetPosition = Vector3.zero;
    private float _targetRadius;
    private int _numberOfMonitoring;
    private bool _littingMode = true;
    private bool _movingMode = true;

    internal static int _idx;

    internal void CreateLitterUnit(Vector3 position)
    {
        int indexByTag = GameObjectTreat.IndexObjectByTag(tag);
        name = tag + indexByTag;
        
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
            _targetPosition = Vector3.zero;
            _targetRadius = _ZERO_THRESHOLD;
            ChangeHeadColor(-1);  // Monitoring カウント -1
            return;
        }

        _targetPosition = targetObj.transform.position;
        _targetRadius = targetObj.GetComponent<TowerDustBoxCtrl>().GetRadius();
        ChangeHeadColor(1);  // Monitoring カウント +1
    }

    internal void ChangeHeadColor(int monit)
    {
        _numberOfMonitoring += monit;  // 複数箇所からの監視カウント
        
        Transform capsuleHeadTransform = transform.Find(_CHILD_NAME_CAPSULE_HEAD);
        if (capsuleHeadTransform == null)
        {
            Debug.LogWarning($"CapsuleHead not found in {name}");
            return;
        }
        
        Renderer headRenderer = capsuleHeadTransform.GetComponent<Renderer>();
        if (headRenderer == null)
        {
            Debug.LogWarning($"Renderer not found on CapsuleHead in {name}");
            return;
        }
        
        // 複数箇所からの監視カウントが1以上なら赤色、それ以外は緑色
        if (_numberOfMonitoring > 0)
        {
            headRenderer.material = MaterialManager.Material_BG_RED;
        }
        else
        {
            headRenderer.material = MaterialManager.Material_BG_Green;
        }
    }

    private Vector3 GetThrowOutDirection()
    {
        if (_targetPosition != Vector3.zero)
        {
            return (_targetPosition - transform.position).normalized;
        }
        return transform.forward;
    }

    private float GetTargetDistance()
    {
        if (_targetPosition == Vector3.zero)
        {
            return _ZERO_THRESHOLD;
        }

        return Vector3.Distance(transform.position, _targetPosition);
    }

    private float GetThrowOutSpeed()
    {
        if (_targetPosition == Vector3.zero)
        {
            return _THROW_SPEED_DEFAULT;
        }

        float targetDistance = GetTargetDistance();
        float angleRad = Mathf.PI * _THROW_ANGLE_DEG / 180f;
        float sinTwoAngle = Mathf.Sin(2f * angleRad);
        
        if (Mathf.Approximately(sinTwoAngle, 0))
        {
            return _THROW_SPEED_DEFAULT;
        }

        float speedSquared = targetDistance * _GRAVITY_ACCELERATION / sinTwoAngle;
        
        if (speedSquared < 0)
        {
            return _THROW_SPEED_DEFAULT;
        }

        return Mathf.Sqrt(speedSquared);
    }

    private bool MakeChildLitter()
    {
        if (_childCount >= _MAX_LITTERS)
        {
            _childCount = 0;
            _littingMode = false;
            return false;
        }

        float targetDistance = GetTargetDistance();
        if (ShouldSkipLitterGeneration(targetDistance))
        {
            return false;
        }

        return SpawnAndConfigureGarbageCube();
    }

    private bool ShouldSkipLitterGeneration(float targetDistance)
    {
        bool isOutsideTargetRadius = targetDistance > _targetRadius || Mathf.Approximately(targetDistance, _ZERO_THRESHOLD);
        return isOutsideTargetRadius && _numberOfMonitoring > 0;
    }

    private bool SpawnAndConfigureGarbageCube()
    {
        Transform handTransform = transform.Find(_CHILD_NAME_HAND);
        if (handTransform == null)
        {
            Debug.LogWarning($"Hand not found in {name}");
            return false;
        }

        GameObject garbageCube = GarbageCubeCtrl.SpawnGarbageCube(handTransform.position, 1);
        if (garbageCube == null)
        {
            Debug.LogWarning($"Failed to spawn garbage cube");
            return false;
        }

        Rigidbody rigidbody = garbageCube.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            Debug.LogWarning($"Rigidbody not found in spawned garbage cube");
            return false;
        }

        Vector3 velocity = CalculateThrowVelocity();
        rigidbody.linearVelocity = velocity;
        
        _childCount++;
        
        return true;
    }

    private Vector3 CalculateThrowVelocity()
    {
        float throwOutSpeed = GetThrowOutSpeed();
        Vector3 throwDirection = GetThrowOutDirection();
        float angleRad = Mathf.PI * _THROW_ANGLE_DEG / 180f;

        Vector3 velocity = Vector3.zero;
        velocity.y = throwOutSpeed * Mathf.Sin(angleRad);
        velocity.x = throwOutSpeed * Mathf.Cos(angleRad) * throwDirection.x;
        velocity.z = throwOutSpeed * Mathf.Cos(angleRad) * throwDirection.z;

        return velocity;
    }

    private Vector3 GetRandomSize(float min, float max)
    {
        return new Vector3(RandomRange(min, max), RandomRange(min, max), RandomRange(min, max));
    }

    private IEnumerator LitterDrops()
    {
        while (_littingMode)
        {
            yield return new WaitForSeconds(_LITTER_CHECK_INTERVAL);
            MakeChildLitter();
        }
    }

    private IEnumerator MoveAgent()
    {
        while (_movingMode)
        {
            yield return new WaitForSeconds(_MOVING_CHECK_INTERVAL);
            
            if (NavMeshManager.HasReachedDestination(_navMeshAgent) && !SetNextPath(_myPaths))
            {
                _movingMode = false;
            }
        }
    }

    private void AgentReachedGoal()
    {
        GameObjectTreat.DestroyAll(gameObject);
    }

    private void AgentJumpToStartPosition()
    {
        if (_myPaths == null || _myPaths.Length == 0)
        {
            Debug.LogWarning($"No paths available for {name}");
            return;
        }

        transform.position = _myPaths[0];
    }

    private bool SetNextPath(Vector3[] paths)
    {
        if (paths == null || paths.Length == 0)
        {
            AgentReachedGoal();
            return false;
        }

        Vector3 destination = paths[0];
        SetDestination(destination);
        
        List<Vector3> remainingPaths = new List<Vector3>(paths);
        remainingPaths.RemoveAt(0);
        _myPaths = remainingPaths.ToArray();
        
        return true;
    }

    internal void ReachDustBox(Vector3 dustBoxPos)
    {
        SetNextPath(_myPaths);
    }

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
        
        Debug.Log("AddPathAndInterrupt:" + path.ToString());
        SetNextPath(_myPaths);
    }

    internal void SetPaths(string[] markerNames = null)
    {
        Vector3[] paths;
        
        if (markerNames == null || markerNames.Length == 0)
        {
            paths = GetDefaultPaths();
        }
        else
        {
            paths = GetCustomPaths(markerNames);
        }

        _myPaths = paths;
    }

    private Vector3[] GetDefaultPaths()
    {
        return new Vector3[1] {new Vector3(0f, 0f, 0f) };
        // return new Vector3[4]
        // {
        //     GetMarkerPositionByName("path_marker_start"),
        //     GetMarkerPositionByName("path_marker_01"),
        //     GetMarkerPositionByName("path_marker_02"),
        //     GetMarkerPositionByName("path_marker_goal")
        // };
    }

    private Vector3[] GetCustomPaths(string[] markerNames)
    {
        List<Vector3> validPaths = new List<Vector3>();

        foreach (string markerName in markerNames)
        {
            Vector3 markerPosition = GetMarkerPositionByName(markerName.Trim());
            
            if (markerPosition == _undefinedPosition)
            {
                continue;
            }

            validPaths.Add(markerPosition);
        }

        return validPaths.ToArray();
    }

    private void SetDestination(Vector3 destination)
    {
        // TODO: キャラクターごとの移動スピードの制御
        // NavMeshManager.SetAgentSpeed(_navMeshAgent);
        NavMeshManager.ChangeAgentSpeed(_navMeshAgent, 1.2f, 6f);  
        
        if (NavMeshManager.IsSameDestination(_navMeshAgent, destination))
        {
            return;
        }

        bool isDestinationSet = NavMeshManager.SetNavMeshDestination(_navMeshAgent, destination, transform);
        if (!isDestinationSet)
        {
            Debug.Log("SetNavMeshDestination failed:" + destination.ToString());
        }
    }

    private Vector3 GetMarkerPositionByName(string markerName)
    {
        if (string.IsNullOrEmpty(markerName))
        {
            Debug.LogWarning("Marker name is null or empty");
            return _undefinedPosition;
        }

        GameObject markerObject = GameObject.Find(markerName);
        if (markerObject == null)
        {
            Debug.LogWarning("GetMarkerPositionByName cannot find:" + markerName);
            return _undefinedPosition;
        }

        return markerObject.transform.position;
    }

    private static float RandomRange(float min, float max)
    {
        return Utility.fRandomRange(min, max);
    }

    private void Awake()
    {
        _navMeshAgent = NavMeshManager.GetNavMeshAgent(gameObject);
        _idx++;
    }

    internal void InitUnitSpawn(string[] markerNames = null)
    {
        SetPaths(markerNames);
        AgentJumpToStartPosition();
        SetNextPath(_myPaths);
        StartCoroutine(LitterDrops());
        StartCoroutine(MoveAgent());
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    private void OnDestroy()
    {
    }
}
