public static class GameSpeedManager
{
    private static float _game_speed = 1f;

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
    }
}
