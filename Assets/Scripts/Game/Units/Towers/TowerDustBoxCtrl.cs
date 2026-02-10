using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using CommonsUtility;

public class TowerDustBoxCtrl : MonoBehaviour
{
    private GameObject _parent;

    // 最初の1回だけ処理するようにガード条件を追加
    private HashSet<int> _triggeredEnemies = new HashSet<int>();

    internal float GetRadius()
    {
        return GetComponent<SphereCollider>().radius;
    }

    /// <summary>
    /// EnemyLitters が進入した時に呼ばれます
    /// TowerDustBoxTriggerHandler から呼び出されます（最初の1回のみ）
    /// </summary>
    internal void OnEnemyLitterEnterFirstTime()
    {
        // TowerDustBoxTriggerHandler により、最初の進入時のみ呼ばれる
    }

    /// <summary>
    /// EnemyLitters が離脱した時に呼ばれます
    /// TowerDustBoxTriggerHandler から呼び出されます（最後の離脱時のみ）
    /// </summary>
    internal void OnEnemyLitterExitAllLeft()
    {
        // TowerDustBoxTriggerHandler により、すべて離脱時のみ呼ばれる
    }

    /// <summary>
    /// EnemyLitters が進入するたびに呼ばれます
    /// 個々のコライダー参照を保持し、SetThrowOutDirection(gameObject) を実行
    /// </summary>
    internal void TrackEnemyLitterEnter(Collider other)
    {
        if (other == null || other.gameObject == null)
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

    /// <summary>
    /// EnemyLitters が離脱するたびに呼ばれます
    /// 個々のコライダー参照を削除し、SetThrowOutDirection() を実行
    /// </summary>
    internal void TrackEnemyLitterExit(Collider other)
    {
        if (other == null || other.gameObject == null)
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
        // TowerDustBoxTriggerHandler をアタッチ
        GameObjectTreat.GetOrAddComponent<TowerDustBoxTriggerHandler>(gameObject);
    }
}
