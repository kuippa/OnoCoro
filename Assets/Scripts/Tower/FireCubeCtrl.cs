using UnityEngine;
using UnityEngine.UI;
using CommonsUtility;
using System;


public class FireCubeCtrl : MonoBehaviour
{
    internal const int _SIZE_NORMAL = 0;
    internal const int _SIZE_MEDIUM = 1;
    internal const int _SIZE_BIG = 2;

    internal const string _SIZE_NORMAL_NAME = "VFX_Fire_01_Small_Smoke";
    internal const string _SIZE_MEDIUM_NAME = "VFX_Fire_01_Medium_Smoke";
    internal const string _SIZE_BIG_NAME = "VFX_Fire_01_Big_Smoke";


    private const float _SPREAD_RADIUS = 1.0f;


    private const string _FIRE_CUBE_PARENT_NAME = "FireCubes";

    internal static void ChangeFireCubeSize(GameObject parentObject, int sizeFlag)
    {
        string sizeName = "";
        switch (sizeFlag)
        {
            case _SIZE_NORMAL:
                sizeName = _SIZE_NORMAL_NAME;
                break;
            case _SIZE_MEDIUM:
                sizeName = _SIZE_MEDIUM_NAME;
                break;
            case _SIZE_BIG:
                sizeName = _SIZE_BIG_NAME;
                break;
            default:
                sizeName = _SIZE_NORMAL_NAME;
                break;
        }
        
        // Transform[] childTransforms = parentObject.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == sizeName);
        Transform[] childTransforms = parentObject.GetComponentsInChildren<Transform>(true);
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

        // parentObject の コライダーの取得
        BoxCollider boxCollider = parentObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            float size = 1.5f + Mathf.Pow(2, sizeFlag);
            boxCollider.size = new Vector3(size, size, size);
        }
    }


    internal static GameObject SpawnFireCube(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
    {
		GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/FireCube");
        // prefab.transform.localScale = GetLocalScale(sizeFlag);
        prefab.transform.localScale = new Vector3(1f, 1f, 1f);
        Vector3 setPoint = spawnPoint;
        Quaternion setRotation = Quaternion.identity;
        if (isSwayingPoint)
        {
            setPoint.x += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setPoint.z += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setRotation = Quaternion.Euler(Utility.fRandomRange(0,360), 0, Utility.fRandomRange(0,360));

        }
        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        unit.tag = GameEnum.TagType.FireCube.ToString();

        int idx = GameObjectTreat.IndexObjectByTag(GameEnum.TagType.FireCube.ToString());
        unit.name = GameEnum.ModelsType.FireCube.ToString() + idx.ToString();

        unit.AddComponent<FireCube>();
        unit.GetComponent<FireCube>()._item_struct.ItemID = unit.name;
        unit.GetComponent<FireCube>()._unit_struct.UnitID = unit.name;

        Rigidbody rb = unit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = unit.AddComponent<Rigidbody>();
        }
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationY;        // Y軸方向回転を無効にする

        // if (unit.GetComponent<Collider>() == null)
        // {
        //     unit.AddComponent<BoxCollider>();
        // }
        Transform parentTransform = GameObjectTreat.GetParentTransform(_FIRE_CUBE_PARENT_NAME);
        unit.transform.SetParent(parentTransform);

        ChangeFireCubeSize(unit, sizeFlag);

        return unit;
    }



}
