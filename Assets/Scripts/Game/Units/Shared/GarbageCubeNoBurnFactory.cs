using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// 不燃ゴミキューブの生成を専門に扱う Factory クラス（PLATEAU CityHack 2026）
///
/// GarbageCubeFactory と同じ手順で生成し、タグとコンポーネントだけを不燃側にする。
/// サイズ計算は GarbageCubeFactory と共通の値を使い、可燃・不燃で見た目の
/// 大きさが揃うようにしている（総量換算を両者で共通にするため）。
/// </summary>
public static class GarbageCubeNoBurnFactory
{
    private const float _SPREAD_RADIUS = 2.0f;
    private const string _PARENT_NAME = "GarbageCubesNoBurn";
    private static GameObject _parent_holder;

    /// <summary>
    /// 不燃ゴミキューブをスポーンする（同期版）
    ///
    /// プレファブが未作成の場合は可燃ゴミのプレファブで代用する。
    /// 見た目は可燃と同じになるが、タグと集計は不燃として扱われるため
    /// プレファブ待ちで機能全体が止まらないようにしている
    /// </summary>
    internal static GameObject SpawnGarbageCube(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
    {
        GameObject prefab = PrefabManager.GarbageCubeNoBurnPrefab;
        if (prefab == null)
        {
            prefab = PrefabManager.GarbageCubePrefab;
            if (prefab == null)
            {
                Debug.LogWarning("[GarbageCubeNoBurnFactory] プレファブが見つかりません（GarbageCubeNoBurn / GarbageCube とも）");
                return null;
            }
        }

        Vector3 setPoint = spawnPoint;
        Quaternion setRotation = Quaternion.identity;

        if (isSwayingPoint)
        {
            setPoint.x += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setPoint.z += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setRotation = Quaternion.Euler(
                Utility.fRandomRange(0, 360),
                Utility.fRandomRange(0, 360),
                Utility.fRandomRange(0, 360)
            );
        }

        GameObject unit = Object.Instantiate(prefab, setPoint, setRotation);

        // スケールはインスタンスに設定する（プレファブ資産を汚さない）
        unit.transform.localScale = GarbageCubeFactory.GetLocalScale(sizeFlag);

        SetProperties(unit);
        SetRigidbody(unit);

        return unit;
    }

    private static void SetProperties(GameObject unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("[GarbageCubeNoBurnFactory] unit is null");
            return;
        }

        unit.tag = GameEnum.TagType.GarbageNoBurn.ToString();

        int idx = PrefabManager.GarbageCubeNoBurnUID;
        unit.name = GameEnum.ModelsType.GarbageCubeNoBurn.ToString() + idx.ToString();

        // 可燃ゴミのプレファブで代用した場合、GarbageCube が付いたままだと
        // 可燃側として扱われてしまうので取り除く
        GarbageCube burnable = unit.GetComponent<GarbageCube>();
        if (burnable != null)
        {
            Object.Destroy(burnable);
        }

        GarbageCubeNoBurn noBurn = unit.GetComponent<GarbageCubeNoBurn>();
        if (noBurn == null)
        {
            noBurn = unit.AddComponent<GarbageCubeNoBurn>();
        }
        noBurn._item_struct.ItemID = unit.name;
        noBurn._unit_struct.UnitID = unit.name;

        Transform parentTransform = GameObjectTreat.GetHolderParentTransform(
            ref _parent_holder,
            _PARENT_NAME
        );
        unit.transform.SetParent(parentTransform);
    }

    private static void SetRigidbody(GameObject unit)
    {
        if (unit == null)
        {
            return;
        }

        Rigidbody rb = unit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = unit.AddComponent<Rigidbody>();
        }

        // 高速移動時の貫通を防止
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (unit.GetComponent<Collider>() == null)
        {
            unit.AddComponent<BoxCollider>();
        }
    }
}
