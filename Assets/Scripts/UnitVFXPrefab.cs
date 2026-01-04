using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitVFXPrefab : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        Debug.Log("Awake");
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Bonfire");
        Vector3 pos = new Vector3(0, 0, 5);
        // Vector3 pos = Vector3.zero;
        // GameObject instance = Instantiate(prefab) as GameObject;
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity);

        GameObject parent = GameObject.Find("byScript");
        instance.transform.SetParent(parent.transform);



        // Addressables.LoadAssetAsync<GameObject>("ExamplePrefab");
        

    }

}
