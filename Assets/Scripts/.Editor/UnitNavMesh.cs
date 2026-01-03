using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitNavMesh : MonoBehaviour
{

    public Transform target;
    private NavMeshPath path;
    private float elapsed = 0.0f;

    public NavMeshData data;
    NavMeshDataInstance[] instances = new NavMeshDataInstance[2];    

    // Start is called before the first frame update
    void Start()
    {
        // // Add an instance of navmesh data
        // instances[0] = NavMesh.AddNavMeshData(data);
        // // instances[0] = NavMesh.AddNavMeshData(data, new Vector3(0, 5, 0), Quaternion.AngleAxis(90, Vector3.up));        


        // // Add another instance of the same navmesh data - displaced and rotated
        // // instances[1] = NavMesh.AddNavMeshData(data, new Vector3(0, 500, 0), Quaternion.AngleAxis(90, Vector3.up));        
        // instances[1] = NavMesh.AddNavMeshData(data, new Vector3(500, 0, 0), Quaternion.AngleAxis(90, Vector3.up));        
        // // instances[1] = NavMesh.AddNavMeshData(data, new Vector3(0, 0, 0), new Vector3(30, 30, 0));

        path = new NavMeshPath();
        elapsed = 0.0f;        
    }

    public void OnDisable()
    {
        // instances[0].Remove();
        // instances[1].Remove();
    }

    // Update is called once per frame
    void Update()
    {

        // Update the way to the goal every second.
        elapsed += Time.deltaTime;
        if (elapsed > 1.0f)
        {
            elapsed -= 1.0f;
            // NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
        }
        for (int i = 0; i < path.corners.Length - 1; i++)
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);        
    }
}
