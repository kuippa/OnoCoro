using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitNPCact : MonoBehaviour
{

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("OnControllerColliderHit");
        if (hit.gameObject.tag == GameEnum.TagType.Garbage.ToString())
        {
            Debug.Log("OnControllerColliderHit Garbage");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Colliderのサイズを少し大きくすれば検知するが、
        // 接触判定がシビアすぎる。


        Debug.Log("OnCollisionEnter " + collision.gameObject.name);
        if (collision.gameObject.tag == GameEnum.TagType.Garbage.ToString())
        {
            Debug.Log("OnCollisionEnter Garbage");
            
            Destroy(collision.gameObject);

            // TODO 再帰的な子要素の削除


        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 重ね合わせのうえに少し余裕をまたせておけばColliderよりイベントを検出はしやすいが、それでもそれほど安定して呼び出せるわけでもない
        // 判定条件がややシビア


        Debug.Log("OnTriggerEnter " + other.gameObject.name);
        if (other.gameObject.tag == GameEnum.TagType.Garbage.ToString())
        {
            Debug.Log("OnTriggerEnter Garbage");
            Destroy(other.gameObject);

        }
    }


    void Awake()
    {
        // OnCollisionEnter
        // OnCollisionExit
        // OnCollisionStay
        // OnControllerColliderHit

        // OnTriggerEnter
        // OnTriggerExit
        // OnTriggerStay

    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
