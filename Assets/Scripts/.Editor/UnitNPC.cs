using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;   // NavMeshAgentを使うために必要

public class UnitNPC : MonoBehaviour
{
    NavMeshAgent myNavMeshAgent;

    // NavMeshAgent 到達したかどうかのイベント
    // https://docs.unity3d.com/ja/current/ScriptReference/NavMeshAgent-onDestinationReached.html
    // こんなURLは存在しなかった・・・copilotめ
    void OnDestinationReached()
    {
        Debug.Log("OnDestinationReached");

    }


    void Awake()
    {
        // 
        GameObject objNPC = GameObject.Find("Sphere1");
        

        if (objNPC != null)
        {
            Debug.Log("NPC is found");
            // NavMeshAgentをobjNPCに追加
            // objNPC.AddComponent<NavMeshAgent>();

            myNavMeshAgent = objNPC.GetComponent<NavMeshAgent>();
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
        else
        {
            Debug.Log("NPC is not found");
        }
    }



    void SetDestinationToMousePosition()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            // Debug.Log("myNavMeshAgent.pathStatus " +  myNavMeshAgent.pathStatus);
            Debug.Log("myNavMeshAgent " + hit.point);
            Debug.Log("Destination " + myNavMeshAgent.destination);
            Debug.Log("pathPending " + myNavMeshAgent.pathPending);
            Debug.Log("pathStatus " + myNavMeshAgent.pathStatus);
            Debug.Log("PathInvalid " + NavMeshPathStatus.PathInvalid);
            Debug.Log("position " + myNavMeshAgent.updatePosition);
            // myNavMeshAgent.destination = hit.point;
            myNavMeshAgent.SetDestination(hit.point);

            Debug.Log("Destination af:" + myNavMeshAgent.destination);
            // Debug.Log("myNavMeshAgent.pathStatus af: " +  myNavMeshAgent.pathStatus);

            // if (myNavMeshAgent.pathPending == true)
            // {
            //     Debug.Log("myNavMeshAgent " + hit.point);
            //     myNavMeshAgent.SetDestination(hit.point);

            // }
            // else
            // {
            //     Debug.Log("pathPending false ");
            //     // myNavMeshAgent.SetDestination(hit.point);
            // }

        }
    }

    void Start()
    {
        // myNavMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // agent.destination = goal.position; 
        if (Input.GetMouseButtonDown(1))
        {
            SetDestinationToMousePosition();
            // if (!agent.pathPending && agent.remainingDistance < 0.5f)
        }        
        // myNavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete

        if (myNavMeshAgent.hasPath == false)
        {
            // Debug.Log("path is not found");
            return;
        }

        if (!myNavMeshAgent.pathPending && myNavMeshAgent.remainingDistance < 0.5f && myNavMeshAgent.destination != Vector3.zero && myNavMeshAgent.isStopped == false)
        {
            Debug.Log(this.name + " is reached");
            // myNavMeshAgent.destination = new Vector3(-10, 0, 0);
            // myNavMeshAgent.destination = myNavMeshAgent.po
            // navmeshagentの目的地に到達したらdestinationをクリアする
            myNavMeshAgent.destination = Vector3.zero;
            myNavMeshAgent.isStopped = true;
            myNavMeshAgent.ResetPath();
            // Debug.Log("Reached destination");
        }
    }
}
