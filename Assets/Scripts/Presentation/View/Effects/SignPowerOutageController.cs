using CommonsUtility;
using UnityEngine;

/// <summary>
/// 電力不足サインの制御クラス（静的クラス）
/// ユニットに電力不足マーカーを表示・非表示する機能を提供します
/// </summary>
public static class SignPowerOutageController
{

    /// <summary>
    /// 電力不足サインを削除します
    /// </summary>
    /// <param name="target">対象のGameObject</param>
    internal static void UnSignPowerOutage(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Transform transform = target.transform.Find("SignPowerOutage");
        if (transform != null)
        {
            GameObjectTreat.DestroyAll(transform.gameObject);
        }
    }

    /// <summary>
    /// 電力不足サインを取得または作成します（存在しない場合のみ作成）
    /// </summary>
    /// <param name="target">対象のGameObject</param>
    internal static void GetOrCreateCirclePowerOutage(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (target.transform.Find("SignPowerOutage") == null)
        {
            SignPowerOutage(target);
        }
    }

    /// <summary>
    /// 電力不足サインを生成してターゲットに配置します
    /// </summary>
    /// <param name="target">対象のGameObject</param>
    private static void SignPowerOutage(GameObject target)
    {
        GameObject gameObject = Object.Instantiate(PrefabManager.SignPowerOutagePrefab);
        gameObject.name = "SignPowerOutage";
        gameObject.transform.SetParent(target.transform);
        gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
    }

}
