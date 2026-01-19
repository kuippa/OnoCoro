using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TowerDustBoxCtrl : MonoBehaviour
{
    private GameObject _parent;

    // 最初の1回だけ処理するようにガード条件を追加
    private HashSet<int> _triggeredEnemies = new HashSet<int>();

    internal float GetRadius()
    {
        return GetComponent<SphereCollider>().radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != GameEnum.TagType.EnemyLitters.ToString())
        {
            return;
        }

        int instanceId = other.gameObject.GetInstanceID();
        if (_triggeredEnemies.Contains(instanceId))
        {
            return;  // 既に処理済みなのでスキップ
        }

        EnemyLitter component = other.gameObject.GetComponent<EnemyLitter>();
        if (component != null)
        {
            component.SetThrowOutDirection(gameObject);
            _triggeredEnemies.Add(instanceId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != GameEnum.TagType.EnemyLitters.ToString())
        {
            return;
        }

        int instanceId = other.gameObject.GetInstanceID();
        _triggeredEnemies.Remove(instanceId);
        
        EnemyLitter component = other.gameObject.GetComponent<EnemyLitter>();
        if (component != null)
        {
            component.SetThrowOutDirection();
        }
    }

    private void Awake()
    {
    }
}
