using System;
using UnityEngine;

public static class MarkerIndicatorCtrl
{
    internal static void CreateCircularIndicator(GameObject target, float duration, Action<GameObject> callback, Vector3 position)
    {
        if (target == null)
        {
            Debug.LogError("Target GameObject is null");
            return;
        }
        GameObject gameObject = PrefabManager.CircularIndicatorPrefab;
        if (gameObject == null)
        {
            Debug.LogError("Failed to load CircularIndicator prefab");
            return;
        }
        GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
        CircularIndicator component = gameObject2.GetComponent<CircularIndicator>();
        if (component == null)
        {
            Debug.LogError("CircularIndicator component not found on prefab");
            UnityEngine.Object.Destroy(gameObject2);
            return;
        }
        GameObject orCreateIndicatorCanvas = GetOrCreateIndicatorCanvas(target, position);
        StartDeleteProcess(target);
        component.StartIndicator(duration, delegate
        {
            try
            {
                callback?.Invoke(target);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error in callback execution: " + ex.Message);
            }
        }, orCreateIndicatorCanvas);
    }

    private static GameObject GetOrCreateIndicatorCanvas(GameObject target, Vector3 position)
    {
        Transform transform = target.transform.Find("Indicator");
        if (transform != null)
        {
            return transform.gameObject;
        }
        GameObject gameObject = new GameObject("Indicator");
        gameObject.transform.SetParent(target.transform);
        GameObject gameObject2 = gameObject.AddComponent<Canvas>().gameObject;
        gameObject2.transform.position = position;
        gameObject2.transform.rotation = target.transform.rotation;

        // targetのサイズを考慮して、直上Y座標、全面Z座標に配置するためのオフセットを計算
        Vector3 offset = CalculateIndicatorOffset(target);
        gameObject2.transform.localPosition += offset;

        return gameObject2;
    }

    private static Vector3 CalculateIndicatorOffset(GameObject target)
    {
        if (target == null)
        {
            return Vector3.zero;
        }

        // // Rendererから対象のサイズを取得
        // Renderer renderer = target.GetComponent<Renderer>();
        // if (renderer == null)
        // {
        //     Debug.LogWarning($"Renderer component not found on {target.name}");
        //     return Vector3.zero;
        // }

        // Bounds bounds = renderer.bounds;
        // Vector3 targetScale = target.transform.localScale;

        // // Y軸: 対象の上部から上方へオフセット
        // float yOffset = bounds.extents.y * targetScale.y;

        // // Z軸: 対象の前面へオフセット（Z方向前面）
        // float zOffset = bounds.extents.z * targetScale.z;

        // Y軸: 対象の上部から上方へオフセット
        float yOffset = 1.5f;

        // Z軸: 対象の前面へオフセット（Z方向前面）
        float zOffset = -2f;


        // X軸: 対象の中央（オフセットなし）
        return new Vector3(0f, yOffset, zOffset);
    }

    private static void StartDeleteProcess(GameObject target)
    {
        if (target == null)
        {
            return;
        }
        string tag = target.tag;
        if (tag == GameEnum.TagType.TowerSweeper.ToString())
        {
            TowerSweeperCtrl component = target.GetComponent<TowerSweeperCtrl>();
            if (component != null)
            {
                component.StartDeleteUnitProcess();
            }
        }
        else if (tag == GameEnum.TagType.WaterTurret.ToString())
        {
            WaterTurretCtrl component2 = target.GetComponent<WaterTurretCtrl>();
            if (component2 != null)
            {
                component2.StartDeleteUnitProcess();
            }
        }
    }
}
