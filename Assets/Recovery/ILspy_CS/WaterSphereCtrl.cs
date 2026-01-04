// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// WaterSphereCtrl
using System.Collections;
using CommonsUtility;
using UnityEngine;

public class WaterSphereCtrl : MonoBehaviour
{
	private const float _FLOWING_TIME = 15f;

	private IEnumerator InvokeWithGameObject(GameObject obj, float delay)
	{
		yield return new WaitForSeconds(delay);
		WaterFlowing(obj);
	}

	private void WaterFlowing(GameObject target)
	{
		GameObjectTreat.DestroyAll(target);
	}

	private void Start()
	{
		StartCoroutine(InvokeWithGameObject(base.gameObject, 15f));
	}
}
