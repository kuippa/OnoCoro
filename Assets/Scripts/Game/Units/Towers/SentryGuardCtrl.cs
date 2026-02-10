using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public class SentryGuardCtrl : MonoBehaviour
{
	/// <summary>
	/// EnemyLitters が進入した時に呼ばれます
	/// SentryGuardTriggerHandler から呼び出されます
	/// </summary>
	/// <param name="other">進入した Collider</param>
	internal void OnEnemyLitterEnter(Collider other)
	{
		if (other == null || other.gameObject == null)
		{
			return;
		}

		EnemyLitter enemyLitter = other.gameObject.GetComponent<EnemyLitter>();
		if (enemyLitter != null)
		{
			enemyLitter.ChangeHeadColor(1);  // Monitoring カウント +1
		}
	}

	/// <summary>
	/// EnemyLitters が離脱した時に呼ばれます
	/// SentryGuardTriggerHandler から呼び出されます
	/// </summary>
	/// <param name="other">離脱した Collider</param>
	internal void OnEnemyLitterExit(Collider other)
	{
		if (other == null || other.gameObject == null)
		{
			return;
		}

		EnemyLitter enemyLitter = other.gameObject.GetComponent<EnemyLitter>();
		if (enemyLitter != null)
		{
			enemyLitter.ChangeHeadColor(-1);  // Monitoring カウント -1
		}
	}

	private void Awake()
	{
		// SentryGuardTriggerHandler をアタッチ
		GameObjectTreat.GetOrAddComponent<SentryGuardTriggerHandler>(gameObject);
	}
}
