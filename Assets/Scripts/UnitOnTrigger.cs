using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;   // NavMeshAgentを使うために必要
using UnityEditor;       // EditorApplicationを使うために必要

public class UnitOnTrigger : MonoBehaviour
{
    NavMeshAgent myNavMeshAgent;


    void OnTriggerEnter(Collider other)
    {
        // Debug.Log("UnitOnTrigger.OnTriggerEnter()" + other.name);

        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + "()" + other.name);
        #endif

        // 視界に入ったゴミを赤くする
        if (other.tag == GameEnum.EnemyType.Garbage.ToString() )
        {
            GameObject otherGameObject = other.gameObject;
            otherGameObject.GetComponent<Renderer>().material.color = Color.red;

            // Debug.Log(other.GetInstanceID() + " " + other.name + " is found");

            // Object obj = EditorUtility.InstanceIDToObject(other.GetInstanceID());
            // if (!obj)
            //     Debug.LogError("No object could be found with instance id: " + id);
            // else
            //     Debug.Log("Object's name: " + obj.name);
        }
 

        // 一番近いゴミを探索

        // 一番近いゴミに近寄る

        // 一番近いゴミを消す

        // 見てる角度を少しだけ変更
        this.gameObject.transform.Rotate(0, 10, 0);

        // TODO:
        // this.gameObjectですべてのユニットが同期してしまっている


    }

    void OnTriggerExit(Collider other)
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        // 視界から出たゴミを青くする
        if (other.tag == GameEnum.EnemyType.Garbage.ToString() )
        {
            GameObject otherGameObject = other.gameObject;
            otherGameObject.GetComponent<Renderer>().material.color = Color.blue;
        }

    }

    void Awake()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        myNavMeshAgent = this.GetComponent<NavMeshAgent>();

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (myNavMeshAgent.hasPath == false)
        // {
        //     // Debug.Log("path is not found");
        //     return;
        // }

        // if (!myNavMeshAgent.pathPending && myNavMeshAgent.remainingDistance < 0.5f && myNavMeshAgent.destination != Vector3.zero && myNavMeshAgent.isStopped == false)
        // {
        //     Debug.Log(this.name + " is reached");
        //     // myNavMeshAgent.destination = new Vector3(-10, 0, 0);
        //     // myNavMeshAgent.destination = myNavMeshAgent.po
        //     // navmeshagentの目的地に到達したらdestinationをクリアする
        //     myNavMeshAgent.destination = Vector3.zero;
        //     myNavMeshAgent.isStopped = true;
        //     myNavMeshAgent.ResetPath();
        //     // Debug.Log("Reached destination");
        // }
        
    }
}
