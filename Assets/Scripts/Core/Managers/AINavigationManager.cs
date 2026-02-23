using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;
using UnityEngine.AI;

/// <summary>
/// AI キャラクター（Sweeper, NPC 等）のナビゲーション管理システム
/// 
/// 責務：
/// - NavMeshAgent の状態管理
/// - 目的地への移動制御
/// - パス状態確認・検証
/// - 経路計画と移動
/// 
/// 使用方法：
/// 1. Initialize(navMeshAgent, moveSpeed) で初期化
/// 2. MoveToTarget(destination, transform) で移動開始
/// 3. UpdateMovement(targetingService) で毎フレーム更新
/// 4. IsPathValid() でパス状態確認
/// 
/// 対応対象：Sweeper, NPC, Boss等のAIキャラクター
/// </summary>
public class AINavigationManager : MonoBehaviour
{
    // 移動パラメータ
    private const float _MOVE_SPEED = 3f;
    private const float _LOOK_AROUND_INTERVAL = 2f;
    private const float _LOOK_AROUND_ROTATION_ANGLE = 90f;  // 周囲探索の回転角度（小刻み）
    private const float _LOOK_AROUND_SPEED = 1f;  // 回転スムーズさ（大きいほど速い）

    // 外部参照
    private NavMeshAgent _NavMeshAgent = null;
    private Transform _myTransform = null;

    // 状態管理
    private Vector3 _currentDestination = Vector3.zero;
    private bool _hasValidPath = false;
    private Quaternion _targetLookAroundRotation = Quaternion.identity;
    private Quaternion _startLookAroundRotation = Quaternion.identity;  // 周囲探索の開始回転
    private bool _isLookingAround = false;
    private float _lookAroundElapsedTime = 0f;  // 周囲探索の経過時間

    /// <summary>
    /// ナビゲーションマネージャーを初期化
    /// </summary>
    /// <param name="navMeshAgent">NavMeshAgent コンポーネント</param>
    /// <param name="moveSpeed">移動速度（デフォルト値）</param>
    /// <param name="myTransform">自身の Transform</param>
    internal void Initialize(NavMeshAgent navMeshAgent, float moveSpeed, Transform myTransform)
    {
        if (navMeshAgent == null)
        {
            Debug.LogWarning("[AINavigationManager] NavMeshAgent is null");
            return;
        }
        if (myTransform == null)
        {
            Debug.LogWarning("[AINavigationManager] myTransform is null");
            return;
        }

        _NavMeshAgent = navMeshAgent;
        _myTransform = myTransform;
        _currentDestination = Vector3.zero;
        _hasValidPath = false;
    }

    /// <summary>
    /// 毎フレーム更新（周囲探索の回転を適用）
    /// </summary>
    private void Update()
    {
        UpdateLookAround();
        UpdateLookAroundTimer();
    }

    /// <summary>
    /// 周囲探索時の回転を更新
    /// _LOOK_AROUND_SPEED 秒で _LOOK_AROUND_ROTATION_ANGLE 角度だけ回転
    /// </summary>
    private void UpdateLookAround()
    {
        if (!_isLookingAround || _myTransform == null)
        {
            return;
        }

        float progress = Mathf.Clamp01(_lookAroundElapsedTime / _LOOK_AROUND_SPEED);
        _myTransform.rotation = Quaternion.Slerp(_startLookAroundRotation, _targetLookAroundRotation, progress);
    }

    /// <summary>
    /// 周囲探索タイマーを更新
    /// </summary>
    private void UpdateLookAroundTimer()
    {
        if (!_isLookingAround)
        {
            return;
        }

        _lookAroundElapsedTime += Time.deltaTime;
        if (_lookAroundElapsedTime >= _LOOK_AROUND_INTERVAL)
        {
            _isLookingAround = false;
            _lookAroundElapsedTime = 0f;
        }
    }

    /// <summary>
    /// 目的地に移動
    /// </summary>
    /// <param name="destination">目的地座標</param>
    /// <param name="myTransform">自身の Transform</param>
    /// <returns>移動開始成功時 true</returns>
    internal bool MoveToTarget(Vector3 destination, Transform myTransform)
    {
        if (_NavMeshAgent == null)
        {
            Debug.LogWarning("[AINavigationManager] NavMeshAgent is null");
            return false;
        }
        if (myTransform == null)
        {
            Debug.LogWarning("[AINavigationManager] myTransform is null");
            return false;
        }

        // 既に同じ目的地へ移動中
        if (destination == _currentDestination)
        {
            return true;
        }

        // 目的地を設定
        NavMeshManager.SetDestination(destination, _NavMeshAgent);

        // NavMesh 上にいるか確認
        if (!IsOnNavMesh())
        {
            Debug.LogWarning("[AINavigationManager] Not on NavMesh");
            return false;
        }

        _currentDestination = destination;
        return true;
    }


    /// <summary>
    /// 毎フレーム移動状態を更新
    /// </summary>
    /// <param name="targetingService">ターゲティングサービス（ターゲット無視用）</param>
    /// <returns>移動継続中 true、停止・無視 false</returns>
    internal bool UpdateMovement(SweeperTargetingService targetingService)
    {
        if (_NavMeshAgent == null)
        {
            return false;
        }

        // パス状態をチェック
        NavMeshPathStatus pathStatus = _NavMeshAgent.pathStatus;
        switch (pathStatus)
        {
            case NavMeshPathStatus.PathComplete:
                // 正常に目的地へ向かっている
                return true;

            case NavMeshPathStatus.PathPartial:
                // 部分的なパス（目的地に到達不可能）
                return true;

            case NavMeshPathStatus.PathInvalid:
                // 無効なパス
                if (targetingService != null)
                {
                    targetingService.IgnoreCurrentTarget();
                }
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// 現在の目的地を取得
    /// </summary>
    internal Vector3 GetCurrentDestination()
    {
        return _currentDestination;
    }

    /// <summary>
    /// パスが有効かチェック
    /// </summary>
    internal bool IsPathValid()
    {
        if (_NavMeshAgent == null)
        {
            return false;
        }

        return _NavMeshAgent.hasPath && 
               (_NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete || 
                _NavMeshAgent.pathStatus == NavMeshPathStatus.PathPartial);
    }

    /// <summary>
    /// 周囲探索を開始（ターゲットなし時）
    /// </summary>
    internal void StartLookAround()
    {
        _isLookingAround = false;
        LookAround();
    }

    /// <summary>
    /// 周囲を見回る（内部実装）
    /// _LOOK_AROUND_SPEED 秒かけて目標角度に到達
    /// </summary>
    private void LookAround()
    {
        if (_myTransform == null)
        {
            // Debug.LogWarning("[AINavigationManager] myTransform is null");
            return;
        }

        if (_isLookingAround)
        {
            // Debug.Log("[AINavigationManager] Already looking around");
            return;  // 既に探索中
        }

        // 開始時の回転を記録
        _startLookAroundRotation = _myTransform.rotation;
        
        // 現在の Y 軸回転を取得し、目標角度を計算
        float currentYAngle = _myTransform.rotation.eulerAngles.y;
        _targetLookAroundRotation = NavMeshManager.CalculateLookAroundRotation(currentYAngle, _LOOK_AROUND_ROTATION_ANGLE);

        // Debug.Log("[AINavigationManager] Start LookAround: currentYAngle=" + currentYAngle + " targetYAngle=" + _targetLookAroundRotation.eulerAngles.y + " duration=" + _LOOK_AROUND_SPEED + "s");

        _isLookingAround = true;
        _lookAroundElapsedTime = 0f;  // タイマーをリセット
        _currentDestination = Vector3.zero;
        _hasValidPath = false;
    }

    /// <summary>
    /// 移動を停止
    /// </summary>
    internal void Stop()
    {
        if (_NavMeshAgent == null)
        {
            return;
        }

        NavMeshManager.ClearDestination(_NavMeshAgent);
        _currentDestination = Vector3.zero;
        _hasValidPath = false;
    }

    // ===== プライベートメソッド =====

    /// <summary>
    /// NavMesh 上にいるかを確認
    /// </summary>
    private bool IsOnNavMesh()
    {
        if (_NavMeshAgent == null)
        {
            return false;
        }

        if (!_NavMeshAgent.isOnNavMesh)
        {
            // Debug.Log("[AINavigationManager] Not on NavMesh: " + _NavMeshAgent.GetInstanceID());
            return false;
        }
        return true;
    }

    /// <summary>
    /// NavMeshAgent の状態をデバッグ出力
    /// </summary>
    private void DebugNavMeshAgent()
    {
        if (_NavMeshAgent == null)
        {
            return;
        }

        Debug.Log("[AINavigationManager] NavMeshAgent: " + _NavMeshAgent.name
            + " hasPath: " + _NavMeshAgent.hasPath
            + " pathStatus: " + _NavMeshAgent.pathStatus
            + " remainingDistance: " + _NavMeshAgent.remainingDistance
            + " destination: " + _NavMeshAgent.destination
            + " pathPending: " + _NavMeshAgent.pathPending);
    }
}
