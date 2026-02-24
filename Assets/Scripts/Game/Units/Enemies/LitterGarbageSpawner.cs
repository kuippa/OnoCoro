using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

internal class LitterGarbageSpawner
{
    // Numeric Constants
    private const float _THROW_ANGLE_DEG = 65f;
    private const float _THROW_SPEED_DEFAULT = 4.43f;
    private const float _GRAVITY_ACCELERATION = 9.8f;
    private const float _ZERO_THRESHOLD = 0f;

    // Target State
    private Vector3 _targetPosition = Vector3.zero;
    private float _targetRadius = _ZERO_THRESHOLD;
    private int _numberOfMonitoring;

    internal LitterGarbageSpawner()
    {
        _targetPosition = Vector3.zero;
        _targetRadius = _ZERO_THRESHOLD;
        _numberOfMonitoring = 0;
    }

    /// <summary>
    /// ゴミの投げ出し対象タワーを設定
    /// </summary>
    internal void SetThrowTarget(GameObject targetObj = null)
    {
        if (targetObj == null)
        {
            _targetPosition = Vector3.zero;
            _targetRadius = _ZERO_THRESHOLD;
            return;
        }

        _targetPosition = targetObj.transform.position;

        TowerDustBoxCtrl dustBoxCtrl = targetObj.GetComponent<TowerDustBoxCtrl>();
        if (dustBoxCtrl != null)
        {
            _targetRadius = dustBoxCtrl.GetRadius();
        }
        else
        {
            _targetRadius = _ZERO_THRESHOLD;
        }
    }

    /// <summary>
    /// 監視カウントを更新（複数タワーから監視される場合）
    /// </summary>
    internal void UpdateMonitoringCount(int delta)
    {
        _numberOfMonitoring += delta;
    }

    /// <summary>
    /// 現在の監視カウント数を取得
    /// </summary>
    internal int GetMonitoringCount()
    {
        return _numberOfMonitoring;
    }

    /// <summary>
    /// ゴミの生成が可能かを判定
    /// </summary>
    internal bool CanSpawnGarbage(Vector3 litterPosition)
    {
        float targetDistance = GetTargetDistance(litterPosition);
        return !ShouldSkipLitterGeneration(targetDistance);
    }

    /// <summary>
    /// ゴミ生成を試行（Hand位置からの生成、物理計算、childCount更新を一括処理）
    /// コルーチンから定期的に呼び出されることを想定
    /// </summary>
    internal bool TryExecuteGarbageDrop(Transform handTransform, Vector3 litterPosition, Transform enemyTransform, int currentChildCount, out int newChildCount)
    {
        newChildCount = currentChildCount;

        if (handTransform == null)
        {
            Debug.LogWarning("Hand Transform is null");
            return false;
        }

        // 上限チェック
        if (currentChildCount >= Litter.MAX_GARBAGE_COUNT)
        {
            return false;
        }

        // ゴミ生成可能か判定
        if (!CanSpawnGarbage(litterPosition))
        {
            return false;
        }

        // ゴミキューブをスポーン
        return TrySpawnGarbage(handTransform.position, enemyTransform, currentChildCount, out newChildCount);
    }

    /// <summary>
    /// ゴミキューブをスポーンし、投げ出す物理計算を適用
    /// </summary>
    internal bool TrySpawnGarbage(Vector3 handPosition, Transform enemyTransform, int currentChildCount, out int newChildCount)
    {
        newChildCount = currentChildCount;

        if (currentChildCount >= Litter.MAX_GARBAGE_COUNT)
        {
            return false;
        }

        GameObject garbageCube = GarbageCubeCtrl.SpawnGarbageCube(handPosition, 1);
        if (garbageCube == null)
        {
            Debug.LogWarning($"Failed to spawn garbage cube at {handPosition}");
            return false;
        }

        Rigidbody rigidbody = garbageCube.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            Debug.LogWarning($"Rigidbody not found in spawned garbage cube");
            return false;
        }

        Vector3 velocity = CalculateThrowVelocity(handPosition, enemyTransform);
        rigidbody.linearVelocity = velocity;

        newChildCount = currentChildCount + 1;

        return true;
    }

    /// <summary>
    /// ゴミの投げ出し方向を計算（ターゲット方向または進行方向から左右）
    /// ターゲットがない場合は、敵の進行方向から左右45度にランダムに投げ捨て
    /// </summary>
    private Vector3 GetThrowOutDirection(Vector3 litterPosition, Transform enemyTransform = null)
    {
        if (_targetPosition != Vector3.zero)
        {
            return (_targetPosition - litterPosition).normalized;
        }

        // ターゲットがない場合、敵の進行方向から左右にランダムに投げ捨て
        if (enemyTransform != null)
        {
            Vector3 forwardDir = enemyTransform.forward;
            Vector3 rightDir = enemyTransform.right;
            
            // ランダムに左右どちらかを選ぶ（-1 = 左、+1 = 右）
            float randomSide = Random.value > 0.5f ? 1f : -1f;
            
            // 進行方向に左右45度加算した方向を返す
            Vector3 direction = (forwardDir + rightDir * randomSide * 0.7f).normalized;
            return direction;
        }

        return Vector3.forward;
    }

    /// <summary>
    /// ターゲットまでの距離を取得
    /// </summary>
    private float GetTargetDistance(Vector3 litterPosition)
    {
        if (_targetPosition == Vector3.zero)
        {
            return _ZERO_THRESHOLD;
        }

        return Vector3.Distance(litterPosition, _targetPosition);
    }

    /// <summary>
    /// 投げ出しに必要な速度を物理計算で算出
    /// 放物線運動の方程式に基づいて目標距離に到達する速度を計算
    /// </summary>
    private float GetThrowOutSpeed(Vector3 litterPosition)
    {
        if (_targetPosition == Vector3.zero)
        {
            return _THROW_SPEED_DEFAULT;
        }

        float targetDistance = GetTargetDistance(litterPosition);
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

    /// <summary>
    /// 投げ出しの速度ベクトルを計算
    /// 指定角度と方向に基づいて3D速度を計算
    /// </summary>
    private Vector3 CalculateThrowVelocity(Vector3 litterPosition, Transform enemyTransform = null)
    {
        float throwOutSpeed = GetThrowOutSpeed(litterPosition);
        Vector3 throwDirection = GetThrowOutDirection(litterPosition, enemyTransform);
        float angleRad = Mathf.PI * _THROW_ANGLE_DEG / 180f;

        Vector3 velocity = Vector3.zero;
        velocity.y = throwOutSpeed * Mathf.Sin(angleRad);
        velocity.x = throwOutSpeed * Mathf.Cos(angleRad) * throwDirection.x;
        velocity.z = throwOutSpeed * Mathf.Cos(angleRad) * throwDirection.z;

        return velocity;
    }

    /// <summary>
    /// ゴミ生成をスキップすべきかを判定
    /// ターゲット外にあり、かつ監視カウント > 0 の場合はスキップ
    /// </summary>
    private bool ShouldSkipLitterGeneration(float targetDistance)
    {
        bool isOutsideTargetRadius = targetDistance > _targetRadius || Mathf.Approximately(targetDistance, _ZERO_THRESHOLD);
        return isOutsideTargetRadius && _numberOfMonitoring > 0;
    }

    /// <summary>
    /// デバッグ用：現在の投げ出し設定情報を取得
    /// </summary>
    internal string GetDebugInfo(Vector3 litterPosition)
    {
        return $"Target: {_targetPosition}, Monitoring: {_numberOfMonitoring}, Distance: {GetTargetDistance(litterPosition):F2}";
    }
}
