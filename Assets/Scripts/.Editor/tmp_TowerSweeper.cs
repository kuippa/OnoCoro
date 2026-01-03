using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CommonsUtility;
using UnityEngine.AI;   // NavMeshAgentを使うために必要


public class TowerSweeper : MonoBehaviour
{

    // private GameObject myUnit;
    const string _TOWER_SWEEPER = "TowerSweeper";
    NavMeshAgent myNavMeshAgent;

    public void CreateSweeperUnit(Vector3 position)
    {
        this.tag = _TOWER_SWEEPER;
        int idx = GameObjectTreat.IndexObjectByTag(this.tag);
        this.name = this.tag + idx;
        // myStatus = new EnemyStatus();
        // myStatus.SetEnemyName(this.name);

        // myUnit = Instantiate(Resources.Load("Prefabs/TowerSweeper")) as GameObject;

        // myUnit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //#TODO:position の動的設定
        // myUnit.transform.position = new Vector3(0 + idx * 0.5f, 0.15f, -4);
        // myUnit.transform.localScale = new Vector3(0.8f, 0.1f, 0.4f);
        // myUnit.transform.SetParent(this.transform);

        this.transform.position = new Vector3(0 + idx * 0.5f, 0.15f, -4);
        // this.transform.localScale = new Vector3(0.8f, 0.1f, 1f);

        // this.transform.SetParent(this.transform);


        // Rigidbody rigidbody = myUnit.AddComponent<Rigidbody>();
        // rigidbody.useGravity = true;
        // // rigidbody.useGravity = false;
        // // rigidbody.mass = _LitterMass;
        // rigidbody.mass = 5.0f;
        // rigidbody.constraints = 
        //     // RigidbodyConstraints.FreezePositionX | 
        //     // RigidbodyConstraints.FreezePositionY | 
        //     // RigidbodyConstraints.FreezePositionZ | 
        //     RigidbodyConstraints.FreezeRotationX | 
        //     RigidbodyConstraints.FreezeRotationY | 
        //     RigidbodyConstraints.FreezeRotationZ;
        // myUnit.transform.SetParent(this.transform);

        // // 視界用のコライダー
        // CapsuleCollider capsuleCollider = myUnit.GetComponent<CapsuleCollider>();
        // capsuleCollider.isTrigger = true;
        // capsuleCollider.radius = 3.8f;
        // capsuleCollider.height = 3f;
        // capsuleCollider.center = new Vector3(2, 0, 0);
        // capsuleCollider.direction = 1;

        // // 衝突判定用のコライダー
        // BoxCollider boxCollider = myUnit.AddComponent<BoxCollider>();
        // boxCollider.isTrigger = false;
        // boxCollider.size = new Vector3(0.8f, 0.1f, 0.4f);
        // boxCollider.center = new Vector3(0, 0.05f, 0);

        // OnTriggerEnter をコード上から有効にできなかったのでプレファブに割当
        // UnitOnTrigger.cs

        // このコライダーにUnitOnTrigger.csを割当
        // capsuleCollider.gameObject.AddComponent<UnitOnTrigger>();
        // UnitOnTrigger unitOnTrigger = myUnit.AddComponent<UnitOnTrigger>();
        // unitOnTrigger.

        

        // コードからの制御でOnTriggerEnterを有効にする 方法がわからないな・・・
        
        // this.gameObject.AddComponent<UnitOnTrigger>();

        // Debug.Log("CreateSweeperUnit End");

        if (myNavMeshAgent == null)
        {
            Debug.Log("agent is not found");
        }
        else
        {
            Debug.Log("agent is found");
            // ナブメッシュエージェントでNPCを動かす
            // agent.SetDestination(new Vector3(-10, 0, 0));
            // objNPC.GetComponent<NavMeshAgent>().destination = new Vector3(-10, 0, 0);

        }


    }

    void OnDestroy()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        // DestroyAll(myUnit);
        GameObjectTreat.DestroyAll(this.gameObject);
    }

    void Awake()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        GameConfig.InitGameConfig();
        // CreateSweeperUnit();
    }

    void OnTriggerEnter(Collider other)
    {
        // 一つのゲームオブジェクトに２つのトリガーイベントがある場合区別できるか？

        // できなそう


        // Triggerイベントを発生させたオブジェクトの名前をログに出力する
        

        // 視界に入ったゴミを赤くする
        if (other.tag == EnemyEnum.TagType.Garbage.ToString() )
        {
            GameObject otherGameObject = other.gameObject;
            otherGameObject.GetComponent<Renderer>().material.color = Color.red;

            // Debug.Log(other.GetInstanceID() + " " + other.name + " is found");

            // Object obj = EditorUtility.InstanceIDToObject(other.GetInstanceID());
            // if (!obj)
            //     Debug.LogError("No object could be found with instance id: " + id);
            // else
            //     Debug.Log("Object's name: " + obj.name);
            SetTargetGarbage(otherGameObject);
        }


        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            // Debug.Log("Tower OnTriggerEnter");
        #endif
    }

    GameObject targetGarbage = null;

    void SetTargetGarbage(GameObject other)
    {
        if (targetGarbage == null)
        {
            targetGarbage = other.gameObject;
        }
        // thisからの距離が近ければtargetGarbageを更新
        if (Vector3.Distance(this.transform.position, targetGarbage.transform.position) 
            > Vector3.Distance(this.transform.position, other.transform.position))
        {
            targetGarbage.GetComponent<Renderer>().material.color = Color.gray;
            targetGarbage = other.gameObject;
        }
        targetGarbage.GetComponent<Renderer>().material.color = Color.yellow;



    }

    void MoveControl()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        // MoveForward();
        // return;

        if (targetGarbage == null)
        {
            // RotateAngle(45f);
            return;
        }

        Vector3 targetPosition = targetGarbage.transform.position;
        // targetPosition.y = this.transform.position.y;
        // this.transform.LookAt(targetPosition);
        this.transform.LookAt(targetPosition);

        // this.GetComponent<NavMeshAgent>().destination = targetGarbage.transform.position;
        this.GetComponent<NavMeshAgent>().destination = targetPosition;
        // RotateControl();

        // // destination に到着したら
        // if (Vector3.Distance(this.transform.position, targetPosition) < 1.5f)
        // {
        //     // Debug.Log("Arrived");
        //     // targetGarbage.GetComponent<Renderer>().material.color = Color.gray;

        //     // 範囲内のゴミを全て回収
        //     if (targetGarbage.tag == EnemyEnum.TagType.Garbage.ToString())
        //     {

        //     }

        //     GameObjectTreat.DestroyAll(targetGarbage);
        // }

        // Debug.Log("MoveControl");
        // this.GetComponent<NavMeshAgent>().destination = targetGarbage.transform.position;
    }

    void MoveForward()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif

        // this.transform.position += this.transform.forward * 0.1f;
        this.transform.position += this.transform.forward * 0.4f;
    }


    // thisの向きをtargetGarbageに向ける
    void RotateControl()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif

        if (targetGarbage == null || targetGarbage.transform.position == null)
        {
            return;
        }

        Vector3 targetPosition = targetGarbage.transform.position;
        targetPosition.y = this.transform.position.y;
        this.transform.LookAt(targetPosition);
    }

    // thisの向きをdegree度回転させる
    private void RotateAngle(float degree)
    {
        // #if UNITY_EDITOR
        //     Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        // #endif

        // ゆっくり回転させる
        // this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(0, degree, 0), Time.deltaTime * 1.0f);
        // this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(0, degree, 0), Time.deltaTime * 50.1f);

        this.transform.Rotate(0, degree, 0);
    }

    void OnTriggerExit(Collider other)
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        // Debug.Log("OnTriggerExit");
        // 視界から出たゴミを青くする
        if (other.tag == GameEnum.TagType.Garbage.ToString() )
        {
            GameObject otherGameObject = other.gameObject;
            otherGameObject.GetComponent<Renderer>().material.color = Color.blue;
        }
    }

    private void FixedUpdate()
    {
        // #if UNITY_EDITOR
        //     Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        // #endif
        // Debug.Log("FixedUpdate");
    }

    private double time = 0;

    // 時間経過でゴミを見失う
    void Update()
    {
        time += Time.deltaTime;

        // ゆっくり回転させる
        // this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(0, 180, 0), Time.deltaTime * 1.0f);

        if (time > 1)
        {
            time = 0;
            MoveControl();
        }

    }

}
