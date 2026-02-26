using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// FireCube の生成を専門に扱う Factory クラス
/// スポーン時のサイズ調整、プロパティ設定などを一元管理します
/// </summary>
public static class FireCubeFactory
{
    public const int _SIZE_NORMAL = 0;
    public const int _SIZE_MEDIUM = 1;
    public const int _SIZE_BIG = 2;

    public const string _SIZE_NORMAL_NAME = "VFX_Fire_01_Small_Smoke";
    public const string _SIZE_MEDIUM_NAME = "VFX_Fire_01_Medium_Smoke";
    public const string _SIZE_BIG_NAME = "VFX_Fire_01_Big_Smoke";

    private const float _SPREAD_RADIUS = 1.0f;
    private static GameObject _parent_holder;  // 親オブジェクトのキャッシュ
    private const string _FIRE_CUBE_PARENT_NAME = "FireCubes";

    /// <summary>
    /// 即座に炎キューブをスポーンします（同期版）
    /// </summary>
    internal static GameObject SpawnFireCube(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
    {
        // PrefabManagerが利用可能な場合はそちらを使用
        GameObject prefab = PrefabManager.FireCubePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("PrefabManager.FireCubePrefab is null, loading from Resources.");
            return null;
        }

        prefab.transform.localScale = new Vector3(1f, 1f, 1f);
        Vector3 setPoint = spawnPoint;
        Quaternion setRotation = Quaternion.identity;

        if (isSwayingPoint)
        {
            setPoint.x += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setPoint.z += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setRotation = Quaternion.identity;  // 炎は回転させない
        }

        GameObject unit = Object.Instantiate(prefab, setPoint, setRotation);

        // プロパティ設定
        SetFireCubeProperties(unit);

        // 物理設定
        SetFireCubeRb(unit);

        // サイズ変更
        ChangeFireCubeSize(unit, sizeFlag);

        return unit;
    }

    /// <summary>
    /// 炎キューブのサイズを変更します
    /// </summary>
    internal static void ChangeFireCubeSize(GameObject parentObject, int sizeFlag)
    {
        string sizeName = "";
        // switch式で簡潔に記述
        sizeName = sizeFlag switch
        {
            _SIZE_NORMAL => _SIZE_NORMAL_NAME,
            _SIZE_MEDIUM => _SIZE_MEDIUM_NAME,
            _SIZE_BIG => _SIZE_BIG_NAME,
            _ => _SIZE_NORMAL_NAME,
        };

        // 子オブジェクトの炎エフェクトを切り替え
        Transform[] childTransforms = parentObject.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in childTransforms)
        {
            if (child.name == _SIZE_NORMAL_NAME || child.name == _SIZE_MEDIUM_NAME || child.name == _SIZE_BIG_NAME)
            {
                if (child.name == sizeName)
                {
                    child.gameObject.SetActive(true);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        // コライダーサイズの調整
        BoxCollider boxCollider = parentObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            float size = 1.5f + Mathf.Pow(2, sizeFlag);
            boxCollider.size = new Vector3(size, size, size);
        }
    }

    /// <summary>
    /// 炎キューブのプロパティを設定します
    /// </summary>
    private static void SetFireCubeProperties(GameObject unit)
    {
        unit.tag = GameEnum.TagType.FireCube.ToString();

        // PrefabManagerが利用可能な場合はそちらを使用、なければIndexObjectByTagで取得
        int idx;
        if (PrefabManager.FireCubePrefab != null)
        {
            idx = PrefabManager.FireCubeUID;
        }
        else
        {
            idx = GameObjectTreat.IndexObjectByTag(GameEnum.TagType.FireCube.ToString());
        }

        unit.name = GameEnum.ModelsType.FireCube.ToString() + idx.ToString();

        FireCube fireCube = unit.GetComponent<FireCube>();
        if (fireCube == null)
        {
            fireCube = unit.AddComponent<FireCube>();
        }
        fireCube._item_struct.ItemID = unit.name;
        fireCube._unit_struct.UnitID = unit.name;

        // 親オブジェクトの設定（キャッシュを使用）
        Transform parentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_holder, _FIRE_CUBE_PARENT_NAME);
        unit.transform.SetParent(parentTransform);
    }

    /// <summary>
    /// 炎キューブのRigidbodyを設定します
    /// </summary>
    private static void SetFireCubeRb(GameObject unit)
    {
        Rigidbody rb = unit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = unit.AddComponent<Rigidbody>();
        }
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationY;  // Y軸方向回転を無効にする
    }
}
