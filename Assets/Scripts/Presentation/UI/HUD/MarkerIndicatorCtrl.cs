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
        return gameObject2;
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
            TowerSweeper component = target.GetComponent<TowerSweeper>();
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
