using UnityEngine;

/// <summary>
/// 環境光（Directional Light）の制御クラス
/// ライトモードとシャドウモードの設定を管理
/// TODO: シーンのディレクショナルライトの影、シャドーマップ、解像度の設定をウルトラに設定する実装が必要
/// TODO: 現在は未実装。将来的にシーン内のGameObjectにアタッチして使用する想定
/// </summary>
public class EnvironmentLightCtrl : MonoBehaviour
{
    private const int LIGHT_MODE_DEFAULT = 0;
    private const int LIGHT_MODE_DARK = 1;

    private const int SHADOW_MODE_DEFAULT = 0;
    private const int SHADOW_MODE_ULTRA = 0;

    /// <summary>
    /// ライトオプションを設定します
    /// </summary>
    /// <param name="mode">ライトモード（0: Default, 1: Dark）</param>
    internal void SetLightOptions(int mode)
    {
        // TODO: ライトモードに応じた設定を実装
        // TODO: LIGHT_MODE_DARK時の輝度調整、色温度変更などを実装
        // TODO: SHADOW_MODE_ULTRAに応じたシャドウ品質設定を実装
    }

    /// <summary>
    /// シーン内のDirectional Lightを取得します
    /// </summary>
    /// <returns>見つかったDirectional Light、なければ最初のLight</returns>
    private Light GetLight()
    {
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj != null)
        {
            return lightObj.GetComponent<Light>();
        }
        else
        {
            // 全Lightから検索（FindObjectsOfType非推奨のため、FindObjectsByTypeを使用）
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            if (lights.Length > 0)
            {
                // ディレクショナルライトを検索
                foreach (Light light in lights)
                {
                    if (light.type == LightType.Directional)
                    {
                        return light;
                    }
                }
                Debug.Log("Directional Light not found");
                return lights[0];   // 見つからない場合は最初のライトを返す
            }
        }
        return null;
    }

    private void Awake()
    {
        Light light = GetLight();

        // TODO: 以下のライト影設定は現在反映されない（Unity設定の制約）
        // TODO: lightの影、解像度をウルトラ（VeryHigh）に設定する実装を完成させる
        // TODO: シャドウマップの解像度設定方法を調査し、実装する
        // Debug.Log("light.shadowResolution: " + light.shadowResolution);
        // light.shadowResolution = LightShadowResolution.VeryHigh;
    }
}
