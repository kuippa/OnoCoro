using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Unity6 と HDRP にしたらVFXが動かなくなったので、このクラスは現在休眠中


public class Flame : MonoBehaviour
{
    private float _time = 0.0f;
    private float _interval = 0.0100f;

    // internal void EventEarthQuake(float magnitude, float duration = _DURATION ,float interval = _INTERVAL)
    // {
    //     _is_earthquake = true;
    //     _interval = interval;
    //     _total_duration = duration;
    //     _magnitude = magnitude;
    //     _degree = 0.0f;
    //     // Debug.Log("EventEarthQuake " + _degree);
    // }

    // TODO:
    // ごみを一定数次々燃やす
    internal void GarbageToFire()
    {
        // Debug.Log("GarbageToFire");
        GameObject[] garbage = GameObject.FindGameObjectsWithTag("Garbage");
        if (garbage.Length > 0)
        {
            // SetBonFire(garbage[0].transform.position);
            // GameObject prefab = Resources.Load<GameObject>("Prefabs/Bonfire");
            // GameObject instance = Instantiate(prefab, garbage[0].transform.position, Quaternion.identity);
            // instance.AddComponent<Rigidbody>();
            // instance.GetComponent<Rigidbody>().useGravity = true;

            for (int i = garbage.Length -1; i < garbage.Length; i--)
            {
                Debug.Log("GarbageToFire" + i );

                // 加速度があるか調べる
                Rigidbody rb = garbage[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // rb.velocity = Vector3.zero;
                    // rb.angularVelocity = Vector3.zero;
                    Debug.Log("GarbageToFire" + i + " "+ rb.linearVelocity + rb.angularVelocity);

                    // if (rb.velocity == Vector3.zero)
                    // if (rb.angularVelocity == Vector3.zero)
                    // {
                        SetBonFire(garbage[i].transform.position);
                        Destroy(garbage[i]);
                        // rb.useGravity = false;
                        break;  // 一つだけ燃やす
                    // }
                }

            }


        }

    }

    // 大火事用 TODO
    private void SetBonFire(Vector3 setPoint)
    {
        GameObject prefab = PrefabManager.BonfirePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("Bonfire prefab not found in PrefabManager");
            return;
        }
        setPoint.y += 0.5f;
        GameObject instance = Instantiate(prefab, setPoint, Quaternion.identity);
        instance.AddComponent<Rigidbody>();
        instance.GetComponent<Rigidbody>().useGravity = true;
        // instance.GetComponent<Rigidbody>().useGravity = false;
    }


    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        if (_time > _interval)
        {
            _time = 0.0f;
            GarbageToFire();
        }
    }
}
