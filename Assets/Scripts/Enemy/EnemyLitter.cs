using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonsUtility;

public class EnemyLitter : MonoBehaviour
{
    private EnemyStatus myStatus;
    // private GameObject myUnit = new GameObject();
    // private GameObject myUnit;
    // 時間間隔を保持する変数
    private float _t = 0f;
    private int _child_count = 0;

    [SerializeField]
    public float _TimeDistance = 0.08f;
    public float _LitteringPow = 300f;
    public int _MaxLitters = 50;
    public float _LitterSizeMin = 0.05f;
    public float _LitterSizeMax = 0.10f;
    public float _LitterMass = 80f;
    public bool _LittingMode = true;   // true: 一定時間ごとに生成する false: 生成しない

    public void CreateLitterUnit(Vector3 position)
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif

        this.tag = GameEnum.TagType.EnemyLitters.ToString();
        int idx = GameObjectTreat.IndexObjectByTag(this.tag);
        this.name = this.tag + idx;
        myStatus = new EnemyStatus();
        myStatus.SetEnemyName(this.name);

        // this.transform.position = new Vector3(0 + idx * 0.5f, 0.5f, -6);
        this.transform.position = new Vector3(position.x + idx * 0.5f, position.y, position.z);
        this.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        Vector3 setPoint = this.transform.position;

        Quaternion setRotation = Quaternion.Euler(0, rdNum(0,360), 0);
        this.transform.rotation = setRotation;


    }

    private bool MakeChildLitter()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        // 子供要素数がXX以上ならこれ以上生成をおこなわない
        // if (myUnit.transform.childCount >= _MaxLitters)
        if (_child_count >= _MaxLitters)
        {
            _child_count = 0;
            _LittingMode = false;
            return false;
        }
        myStatus = new EnemyStatus();
        myStatus.SetEnemyName(GameEnum.TagType.Garbage.ToString());
		GameObject prefab = Resources.Load<GameObject>("Prefabs/GarbageCube");
        prefab.transform.localScale = GetRandomSize(_LitterSizeMin, _LitterSizeMax);
        Vector3 setPoint = this.transform.position;
        Quaternion setRotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), rdNum(0,360));
        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        // unit.tag = "Garbage";
        unit.tag = GameEnum.TagType.Garbage.ToString();
        Rigidbody rigidbody = unit.GetComponent<Rigidbody>();
        // rigidbody.useGravity = true;
        // Rigidbody rb        = GetComponent<Rigidbody>();
        // Rigidbody rigidbody = unit.AddComponent<Rigidbody>();
        rigidbody.AddForce(transform.forward * _LitteringPow);
        unit.transform.SetParent(this.transform);

        _child_count++;
        return true;
    }

    private Vector3 GetRandomSize(float min, float max)
    {
        Vector3 size = new Vector3(
                rdNum(min, max), 
                rdNum(min, max), 
                rdNum(min, max)
                );
        return size;
    }

    void Awake()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        // GameConfig.InitGameConfig();

        // // this.tag = "EnemyLitters";
        // this.tag = GameEnum.TagType.EnemyLitters.ToString();
        // int idx = GameObjectTreat.IndexObjectByTag(this.tag);
        // this.name = this.tag + idx;
        // myStatus = new EnemyStatus();
        // myStatus.SetEnemyName(this.name);
		// // GameObject prefab = Resources.Load<GameObject>("Prefabs/EnemyLitter");
        // // myUnit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        // // prefab.transform.localScale = GetRandomSize(_LitterSizeMin, _LitterSizeMax);
        // // myUnit.transform.position = new Vector3(0 + idx * 0.5f, 0.5f, -6);
        // // myUnit.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        // // Vector3 setPoint = myUnit.transform.position;

        // // prefab.transform.position = new Vector3(0 + idx * 0.5f, 0.5f, -6);
        // // prefab.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        // // Vector3 setPoint = prefab.transform.position;

        // this.transform.position = new Vector3(0 + idx * 0.5f, 0.5f, -6);
        // this.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        // Vector3 setPoint = this.transform.position;



        // // Quaternion setRotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), rdNum(0,360));
        // // Quaternion setRotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), 0);
        // Quaternion setRotation = Quaternion.Euler(0, rdNum(0,360), 0);
        // // Quaternion setRotation = Quaternion.Euler(0, 0, 0);
        // // GameObject unit = Instantiate(prefab, setPoint, setRotation);
        // // myUnit = Instantiate(prefab, setPoint, setRotation);
        // this.transform.rotation = setRotation;

        // Rigidbody rigidbody = myUnit.AddComponent<Rigidbody>();
        // rigidbody.useGravity = true;
        // rigidbody.mass = _LitterMass;
        // rigidbody.constraints = 
        //     // RigidbodyConstraints.FreezePositionX | 
        //     // RigidbodyConstraints.FreezePositionY | 
        //     // RigidbodyConstraints.FreezePositionZ | 
        //     RigidbodyConstraints.FreezeRotationX | 
        //     RigidbodyConstraints.FreezeRotationY | 
        //     RigidbodyConstraints.FreezeRotationZ;

        // myUnit.transform.SetParent(this.transform);

        // 衝突判定用のコライダーを追加


    }

    private static float rdNum(float min, float max)
    {
        return Utility.fRandomRange(min, max);
    }

    void Update()
    {
        // 一定間隔で子供要素を生成
        _t += Time.deltaTime;        
        if (_t > _TimeDistance && _LittingMode)
        {
            MakeChildLitter();
            #if UNITY_EDITOR
                // Debug.Log(_t.ToString("0.00"));
            #endif
            _t = 0;
        }

    }


    void OnDestroy()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
    }

}
