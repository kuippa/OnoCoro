using UnityEngine;
using CommonsUtility;

public static class GameSpeedManager
{
    private static float _game_speed = 1f;
    
    // デバッグ情報更新用のコールバック
    private static System.Action _onGameSpeedChanged = null;

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
        
        // SetGameSpeed が処理された直後にコールバック実行
        _onGameSpeedChanged?.Invoke();
    }

    /// <summary>
    /// ゲーム速度が変更されたときのコールバックを登録
    /// </summary>
    internal static void OnGameSpeedChanged(System.Action callback)
    {
        _onGameSpeedChanged += callback;
    }
}

