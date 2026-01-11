using System;
using UnityEngine;

public static class WindCtrl
{
    private static float _wind_speed = 1f;
    private static float _wind_direction = 0f;

    internal static float GetWindSpeed()
    {
        return _wind_speed;
    }

    internal static void SetWindSpeed(float speed)
    {
        _wind_speed = speed;
    }

    internal static Vector3 GetWindDirectionVector()
    {
        float f = _wind_direction * (MathF.PI / 180f);
        return -new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
    }

    internal static void SetWindDirection(float direction)
    {
        _wind_direction = direction;
    }

    internal static string GetDirectionText()
    {
        double num = _wind_direction;
        num = (num % 360.0 + 360.0) % 360.0;
        string[] obj = new string[16]
        {
            "北", "北北東", "北東", "東北東", "東", "東南東", "南東", "南南東", "南", "南南西",
            "南西", "西南西", "西", "西北西", "北西", "北北西"
        };
        int num2 = (int)((num + 11.25) / 22.5) % 16;
        return obj[num2];
    }
}
