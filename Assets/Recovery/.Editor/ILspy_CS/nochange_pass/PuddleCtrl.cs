// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PuddleCtrl
using System;
using CommonsUtility;
using UnityEngine;

public class PuddleCtrl : MonoBehaviour
{
	private const float PUDDLE_INCREASE = 4f;

	private float GetPuddleDimension(GameObject puddle)
	{
		Vector3 localScale = puddle.transform.localScale;
		return localScale.x * localScale.z;
	}

	private void ChangePuddleSize()
	{
		float num = MathF.Sqrt(GetPuddleDimension(base.gameObject) + 4f);
		base.transform.localScale = new Vector3(num, 0f, num);
	}

	private Vector3 GetPuddleHighestPoint(GameObject puddle, GameObject other_puddle)
	{
		Vector3 position = puddle.transform.position;
		if (other_puddle.transform.position.y > position.y)
		{
			position.y = other_puddle.transform.position.y;
		}
		return position;
	}

	private void MergerPuddle(GameObject other_puddle)
	{
		float puddleDimension = GetPuddleDimension(base.gameObject);
		float puddleDimension2 = GetPuddleDimension(other_puddle);
		float num = MathF.Sqrt(puddleDimension + puddleDimension2);
		if (puddleDimension > puddleDimension2)
		{
			base.transform.localScale = new Vector3(num, 0f, num);
			GameObjectTreat.DestroyAll(other_puddle);
		}
		else
		{
			other_puddle.transform.localScale = new Vector3(num, 0f, num);
			GameObjectTreat.DestroyAll(base.gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == GameEnum.TagType.RainDrop.ToString())
		{
			ChangePuddleSize();
			GameObjectTreat.DestroyAll(other.gameObject);
		}
		else if (other.gameObject.tag == GameEnum.TagType.Puddle.ToString())
		{
			MergerPuddle(other.gameObject);
		}
	}
}
