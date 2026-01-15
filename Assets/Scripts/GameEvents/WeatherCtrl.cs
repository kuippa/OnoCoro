// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// WeatherCtrl
using CommonsUtility;
using PlateauToolkit.Rendering;
using UnityEngine;

public class WeatherCtrl : MonoBehaviour
{
    private EnvironmentController _env;

    internal const float SUNNY_STRENGTH = 0f;

    internal const float SUNNY_CLOUD_STRENGTH = 0.25f;

    internal const float SUNNY_FOG_STRENGTH = 500f;

    internal const float RAINY_STRENGTH = 0.65f;

    internal const float RAINY_CLOUD_STRENGTH = 0.75f;

    internal const float RAINY_FOG_STRENGTH = 200f;

    internal const float SNOW_STRENGTH = 0.85f;

    internal const float TIME_ = 0.62f;

    internal void ChangeSolarAltitude(float time)
    {
        if (time < 0f)
        {
            time = 0f;
        }
        else if (time >= 1f)
        {
            time = 1f;
        }
        GetEnvironmentController().m_TimeOfDay = time;
    }

    internal void ChangeSnow(float snow = 0.85f)
    {
        GetEnvironmentController().m_Snow = snow;
    }

    internal void ChangeWeather(float strength = 0f, float cloudStrength = 0.25f, float fogStrength = 500f)
    {
        if (strength > 0f)
        {
            if (cloudStrength == 0.25f)
            {
                cloudStrength = 0.75f;
            }
            if (fogStrength == 500f)
            {
                fogStrength = 200f;
            }
            SetRainyEnv(strength, cloudStrength, fogStrength);
            GetRaining().SetRainMode(isRain: true);
        }
        else
        {
            SetSunnyEnv(strength, cloudStrength, fogStrength);
            GetRaining().SetRainMode(isRain: false);
        }
    }

    private Raining GetRaining()
    {
        GameObject eventSystem = GameObjectTreat.GetEventSystem();
        Raining raining = eventSystem.GetComponent<Raining>();
        if (raining == null)
        {
            raining = eventSystem.AddComponent<Raining>();
        }
        return raining;
    }

    internal void SetSunnyEnv(float strength = 0f, float cloudStrength = 0.25f, float fogStrength = 500f)
    {
        EnvironmentController environmentController = GetEnvironmentController();
        environmentController.m_Rain = strength;
        environmentController.m_Cloud = cloudStrength;
        environmentController.m_FogDistance = fogStrength;
        environmentController.m_Snow = 0f;
    }

    internal void SetRainyEnv(float strength = 0.65f, float cloudStrength = 0.75f, float fogStrength = 200f)
    {
        EnvironmentController environmentController = GetEnvironmentController();
        environmentController.m_Rain = strength;
        environmentController.m_Cloud = cloudStrength;
        environmentController.m_FogDistance = fogStrength;
    }

    internal float GetToggleRainStrength()
    {
        if (GetEnvironmentController().m_Rain > 0f)
        {
            return 0f;
        }
        return 0.65f;
    }

    private EnvironmentController GetEnvironmentController()
    {
        return _env;
    }

    private void Awake()
    {
        _env = GameObject.Find("Environment").GetComponent<EnvironmentController>();
        if (_env == null)
        {
            GameObject gameObject = new GameObject("Environment");
            _env = gameObject.AddComponent<EnvironmentController>();
            Debug.LogError("EnvironmentController is not found.");
        }
    }
}
