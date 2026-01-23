using UnityEngine;

public class SentryGuardCtrl : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == GameEnum.TagType.EnemyLitters.ToString())
		{
			EnemyLitter enemyLitter = other.gameObject.GetComponent<EnemyLitter>();
			if (enemyLitter != null)
			{
				enemyLitter.ChangeHeadColor(1);  // Monitoring カウント +1
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == GameEnum.TagType.EnemyLitters.ToString())
		{
			EnemyLitter enemyLitter = other.gameObject.GetComponent<EnemyLitter>();
			if (enemyLitter != null)
			{
				enemyLitter.ChangeHeadColor(-1);  // Monitoring カウント -1
			}
		}
	}

	private void Awake()
	{
	}
}
