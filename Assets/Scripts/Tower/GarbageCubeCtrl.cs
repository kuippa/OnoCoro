// using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CommonsUtility;


public class GarbageCubeCtrl : MonoBehaviour
{
    internal const int _SIZE_NORMAL = 0;
    internal const int _SIZE_SMALL = 1;
    internal const int _SIZE_BIG = 2;
    private const string _GARBAGE_CUBE_PARENT_NAME = "GarbageCubes";
    private const float _SPREAD_RADIUS = 2.0f;
    private const float _GARBAGE_CUBE_SIZE = 0.3f;
    private const float _GARBAGE_CUBE_SIZE_SMALL = 0.08f;
    private const float _GARBAGE_CUBE_SIZE_BIG_MIN = 1.5f;
    internal const float _GARBAGE_CUBE_SIZE_BIG_MAX = 3.0f;  // お掃除ロボットの吸い込み範囲より大きくしては駄目


    private static Vector3 GetLocalScale(int sizeFlag)
    {
        Vector3 localScale = Vector3.zero;
        if (sizeFlag == _SIZE_SMALL)
        {
            localScale = new Vector3(
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_SMALL, _GARBAGE_CUBE_SIZE)
                , Utility.fRandomRange(_GARBAGE_CUBE_SIZE_SMALL, _GARBAGE_CUBE_SIZE)
                , Utility.fRandomRange(_GARBAGE_CUBE_SIZE_SMALL, _GARBAGE_CUBE_SIZE));
        }
        else if (sizeFlag == _SIZE_BIG)
        {
            localScale = new Vector3(
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_BIG_MIN, _GARBAGE_CUBE_SIZE_BIG_MAX)
                , Utility.fRandomRange(_GARBAGE_CUBE_SIZE_BIG_MIN, _GARBAGE_CUBE_SIZE_BIG_MAX)
                , Utility.fRandomRange(_GARBAGE_CUBE_SIZE_BIG_MIN, _GARBAGE_CUBE_SIZE_BIG_MAX));
        }
        else
        {
            localScale = new Vector3(_GARBAGE_CUBE_SIZE, _GARBAGE_CUBE_SIZE, _GARBAGE_CUBE_SIZE);
        }
        return localScale;
    }

    internal static GameObject SpawnGarbageCube(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
    {
		GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/GarbageCube");
        prefab.transform.localScale = GetLocalScale(sizeFlag);
        Vector3 setPoint = spawnPoint;
        Quaternion setRotation = Quaternion.identity;
        if (isSwayingPoint)
        {
            setPoint.x += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setPoint.z += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setRotation = Quaternion.Euler(Utility.fRandomRange(0,360), Utility.fRandomRange(0,360), Utility.fRandomRange(0,360));

        }
        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        unit.tag = GameEnum.TagType.Garbage.ToString();

        int idx = GameObjectTreat.IndexObjectByTag(GameEnum.TagType.Garbage.ToString());
        unit.name = GameEnum.ModelsType.GarbageCube.ToString() + idx.ToString();
        unit.AddComponent<GarbageCube>();
        unit.GetComponent<GarbageCube>()._item_struct.ItemID = unit.name;
        unit.GetComponent<GarbageCube>()._unit_struct.UnitID = unit.name;

        Rigidbody rb = unit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = unit.AddComponent<Rigidbody>();
        }
        // rb.linearVelocity = Vector3.zero;
        // rb.angularVelocity = Vector3.zero;

        // 物理エンジンがオブジェクトの速度に関係なく、常に他のオブジェクトとの衝突を検出します。
        // これにより、高速で移動するオブジェクトが他のオブジェクトを通り抜けることがなくなります。
        // ただし、この設定はパフォーマンスに影響を与えるため、
        // 物理エンジンが他のオブジェクトとの衝突を検出する必要がない場合は、
        // Discrete または Continuous Speculative に設定することをお勧めします。
        // rb.collisionDetectionMode = CollisionDetectionMode.Continuous; 
        
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        // rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;  // Default
        // rb.collisionDetectionMode = CollisionDetectionMode.Discrete; //

        // rb.linearVelocity = Vector3.zero;
        // rb.angularVelocity = Vector3.zero;
        // if (GameConfig._APP_GAME_MODE == GlobalConst.GAME_MODE_DEBUG)
        // {
        //     rb.linearDamping = 2;
        // }        


        if (unit.GetComponent<Collider>() == null)
        {
            // Debug.Log("Add Collider");
            unit.AddComponent<BoxCollider>();
        }
        Transform parentTransform = GameObjectTreat.GetParentTransform(_GARBAGE_CUBE_PARENT_NAME);
        unit.transform.SetParent(parentTransform);

        return unit;
    }



}
