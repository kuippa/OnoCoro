using CommonsUtility;
using UnityEngine;
using UnityEngine.AI;
using Debug = CommonsUtility.Debug;

internal class LitterMovementController
{
    // Timing Constants
    private const float _STUCK_TIMEOUT_DURATION = 10.0f;  // パスに到達できない場合のタイムアウト時間
    private const float _MIN_AGENT_SPEED = 0.3f;  // 移動が停止していると判定するエージェント速度（m/s）
    internal const float _MOVING_CHECK_INTERVAL = 1.0f;  // 移動チェック間隔（秒）

    // Internal State
    private float _stuckDurationCounter = 0f;  // パス設定から経過した時間（タイムアウト検出用）

    internal LitterMovementController()
    {
        _stuckDurationCounter = 0f;
    }

    /// <summary>
    /// 移動状態を更新（毎フレーム呼び出し）
    /// エージェント速度から詰まり状態を監視し、タイムアウトカウンターを更新
    /// </summary>
    private void UpdateMovementState(float agentSpeed, string agentName = "")
    {
        if (agentSpeed < _MIN_AGENT_SPEED)
        {
            // 移動が停止しているのでカウントアップ
            _stuckDurationCounter += _MOVING_CHECK_INTERVAL;
            Debug.Log($"{agentName}: 移動が停止しています (エージェント速度: {agentSpeed:F3}m/s) - 停止時間: {_stuckDurationCounter:F1}秒");
        }
        else
        {
            // 移動中はカウンターをリセット
            if (_stuckDurationCounter > 0)
            {
                Debug.Log($"{agentName}: 移動を再開しました (エージェント速度: {agentSpeed:F3}m/s)");
                _stuckDurationCounter = 0f;
            }
        }
    }

    /// <summary>
    /// 移動状態を更新してタイムアウト判定を行う（統合メソッド）
    /// </summary>
    internal bool CheckAndUpdateMovementState(float agentSpeed, string agentName = "")
    {
        UpdateMovementState(agentSpeed, agentName);
        return HasTimedOut();
    }

    /// <summary>
    /// タイムアウトしたかを判定
    /// </summary>
    internal bool HasTimedOut()
    {
        return _stuckDurationCounter >= _STUCK_TIMEOUT_DURATION;
    }

    /// <summary>
    /// 詰まりカウンターをリセット（新しいパスに移行時）
    /// </summary>
    internal void ResetStuckCounter()
    {
        _stuckDurationCounter = 0f;
    }

    /// <summary>
    /// 現在の詰まり時間を取得（デバッグ用）
    /// </summary>
    internal float GetStuckDuration()
    {
        return _stuckDurationCounter;
    }

    /// <summary>
    /// 目的地に到達したかを判定（直接距離チェック）
    /// </summary>
    internal bool HasReachedDestination(NavMeshAgent navMeshAgent)
    {
        if (navMeshAgent == null)
        {
            return false;
        }
        return NavMeshManager.HasReachedDestination(navMeshAgent);
    }


    /// <summary>
    /// デバッグ用：現在の移動状態情報を取得
    /// </summary>
    internal string GetDebugInfo(float agentSpeed)
    {
        return $"Speed: {agentSpeed:F3}m/s, StuckTime: {_stuckDurationCounter:F1}s, TimedOut: {HasTimedOut()}";
    }
}
