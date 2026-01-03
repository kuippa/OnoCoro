// using System.Diagnostics;
// using System.Reflection;
// using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class EnvironmentLight : MonoBehaviour
{
    private const int LIGHT_MODE_DEFAULT = 0;
    private const int LIGHT_MODE_DARK = 1;

    private const int SHADOW_MODE_DEFAULT = 0;

    private const int SHADOW_MODE_ULTRA = 0;    // 

    internal void SetLightOptions(int mode)
    {

    }

    private Light GetLight()
    {
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj != null)
        {
            return lightObj.GetComponent<Light>();
        }
        else
        {
            // FindObjectsOfType<Light>();が非推奨になったので、代替のFindObjectsByTypeを使用する
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            if (lights.Length > 0)
            {
                // ライトのうちディレクショナルライトのものを返す
                foreach (Light light in lights)
                {
                    if (light.type == LightType.Directional)
                    {
                        return light;
                    }
                }
                Debug.Log("Directional Light not found");
                return lights[0];   // なければ最初のライトを返す
            }
        }
        return null;
    }

    void Awake()
    {
        Light light = GetLight();
        

        // 反映されない
        // lightの影、解像度をウルトラに設定
        // Debug.Log("light.shadowResolution: " + light.shadowResolution);
        // light.shadowResolution = LightShadowResolution.VeryHigh;

    }



}
