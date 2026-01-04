using System;
using UnityEngine;
using UnityEngine.VFX;
using PlateauToolkit.Rendering; // PlateauToolkit.Rendering  Environment を利用のために追加

public class RainDrop : MonoBehaviour
{
    // GameObject _rain = null;
    private const float RAINY_STRENGTH = 0.65f;    // 雨の強さ
    private const float RAINY_CLOUD_STRENGTH = 0.75f;    // 雨のときの雲の強さ
    private const float SUNNY_CLOUD_STRENGTH = 0.25f;    // 晴れのときの雲の強さ

    private const float RAINY_FOG_STRENGTH = 40f;
    private const float SUNNY_FOG_STRENGTH = 500f;

// m_FogDistance
// m_Cloud
    internal void ToggleRain()
    {
        // bool isRain = _rain.activeSelf;
        // _rain.SetActive(!isRain);

        EnvironmentController env = GameObject.Find("Environment").GetComponent<EnvironmentController>();

        if (env.m_Rain > 0.0f)
        {
            env.m_Rain = 0.0f;
            env.m_Cloud = SUNNY_CLOUD_STRENGTH;
            env.m_FogDistance = SUNNY_FOG_STRENGTH;
        }
        else
        {
            env.m_Rain = RAINY_STRENGTH;
            env.m_Cloud = RAINY_CLOUD_STRENGTH;
            env.m_FogDistance = RAINY_FOG_STRENGTH;
        }

        // AudioSource はRain Strength で自動に止まる
        // AudioSource audio_rain = _rain.GetComponent<AudioSource>();
        // if (isRain)
        // {
        //     audio_rain.Stop();
        //     audio_rain.volume = 0.0f;
        // }
        // else
        // {
        //     audio_rain.Play();
        //     audio_rain.volume = 0.8f;
        // }
    }

    // private float GetRainStrength()
    // {
    //     // 0 ～ 1.0
    //     AudioSource audio_rain = _rain.GetComponent<AudioSource>();
    //     return audio_rain.volume;
    // }

    internal void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
        {
            Debug.Log("OnTriggerEnter " + other.name + " object:" + other.gameObject.name);

            ToggleRain();

            return;
        }

    }

    void Awake()
    {
        // _rain = GameObject.Find("Environment/Rain").gameObject;
    }


}
