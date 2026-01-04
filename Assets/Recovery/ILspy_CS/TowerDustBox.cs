// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// TowerDustBox
using System.Collections;
using CommonsUtility;
using UnityEngine;

public class TowerDustBox : MonoBehaviour
{
	private const float DUST_CHECK = 2.5f;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == GameEnum.TagType.Garbage.ToString())
		{
			StartCoroutine(DeleteDust(other));
		}
		else
		{
			_ = other.gameObject.tag == GameEnum.TagType.EnemyLitters.ToString();
		}
	}

	private IEnumerator DeleteDust(Collider other)
	{
		yield return new WaitForSeconds(2.5f);
		if (!(other == null))
		{
			UnitStruct unitStruct = other.gameObject.GetComponent<GarbageCube>().GetUnitStruct();
			ScoreCtrl.UpdateAndDisplayScore(ScoreCtrl.GetTotalGarbageScore(other), unitStruct.ScoreType);
			GameObjectTreat.DestroyAll(other.gameObject);
		}
	}
}
