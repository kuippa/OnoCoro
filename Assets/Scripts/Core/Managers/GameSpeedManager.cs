using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public static class GameSpeedManager
{
    private static float _game_speed = 1f;
    
    // デバッグ情報更新用のコールバック
    private static System.Action _onGameSpeedChanged = null;

    // Physics 調整用定数
    private const int _DEFAULT_SOLVER_ITERATIONS = 6;
    private const int _HIGH_SPEED_SOLVER_ITERATIONS = 15;
    private const float _HIGH_SPEED_THRESHOLD = 5f;

    internal static float GetGameSpeed()
    {
        if (_game_speed == 0f)
        {
            _game_speed = 0.0001f;
        }
        return _game_speed;
    }

    internal static void SetGameSpeed(float speed)
    {
        _game_speed = speed;
        if (GlobalConst.GAME_SPEED_SIMULATION_MODE == false)
        {
            Time.timeScale = GetGameSpeed();
        }

        // 高速モード時は物理演算の精度を上げる
        AdjustPhysicsForGameSpeed(speed);
        
        // SetGameSpeed が処理された直後にコールバック実行
        _onGameSpeedChanged?.Invoke();
    }

    /// <summary>
    /// GameSpeed に応じて物理演算パラメータを調整
    /// </summary>
    private static void AdjustPhysicsForGameSpeed(float speed)
    {
        if (speed >= _HIGH_SPEED_THRESHOLD)
        {
            // 高速モード：ソルバーイテレーション数を増やす（衝突検出精度向上）
            Physics.defaultSolverIterations = _HIGH_SPEED_SOLVER_ITERATIONS;
            Debug.Log($"[GameSpeedManager] 高速モード ({speed:F1}x): Physics.defaultSolverIterations = {_HIGH_SPEED_SOLVER_ITERATIONS}");
        }
        else
        {
            // 通常モード：デフォルト値に戻す
            Physics.defaultSolverIterations = _DEFAULT_SOLVER_ITERATIONS;
        }
    }

    /// <summary>
    /// ゲーム速度が変更されたときのコールバックを登録
    /// </summary>
    internal static void OnGameSpeedChanged(System.Action callback)
    {
        _onGameSpeedChanged += callback;
    }
}

