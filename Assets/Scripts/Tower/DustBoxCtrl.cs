using UnityEngine;

public class DustBoxCtrl : MonoBehaviour
{
    private GameObject _parent;

    internal float GetRadius()
    {
        return GetComponent<SphereCollider>().radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == GameEnum.TagType.EnemyLitters.ToString())
        {
            EnemyLitter component = other.gameObject.GetComponent<EnemyLitter>();
            component.SetThrowOutDirection(base.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == GameEnum.TagType.EnemyLitters.ToString())
        {
            EnemyLitter component = other.gameObject.GetComponent<EnemyLitter>();
            component.SetThrowOutDirection();
        }
    }

    private void Awake()
    {
    }
}
