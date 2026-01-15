using UnityEngine;

public class SentryGuardCtrl : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == GameEnum.TagType.EnemyLitters.ToString())
		{
			other.gameObject.GetComponent<EnemyLitter>().ChangeHeadColor(1);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == GameEnum.TagType.EnemyLitters.ToString())
		{
			other.gameObject.GetComponent<EnemyLitter>().ChangeHeadColor(-1);
		}
	}

	private void Awake()
	{
	}
}
