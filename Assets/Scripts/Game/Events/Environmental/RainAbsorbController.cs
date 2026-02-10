using System;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

public class RainAbsorbController : MonoBehaviour
{
	private static float _BUILDING_BRAKE_SIZE = 20f;

	/// <summary>
	/// RainDrop が進入した時に呼ばれます
	/// RainAbsorbTriggerHandler から呼び出されます
	/// </summary>
	/// <param name="other">進入した Collider（RainDrop）</param>
	internal void OnRainDropEnter(Collider other)
	{
		if (other == null || other.gameObject == null)
		{
			return;
		}

		GameObject parentObject = GetParentObject();
		if (parentObject == null)
		{
			return;
		}

		Absorb(parentObject, other.gameObject);
	}

	/// <summary>
	/// 建物（Untagged）が進入した時に呼ばれます
	/// RainAbsorbTriggerHandler から呼び出されます
	/// </summary>
	/// <param name="other">進入した Collider（建物）</param>
	internal void OnBuildingEnter(Collider other)
	{
		if (other == null || other.gameObject == null)
		{
			return;
		}

		GameObject parentObject = GetParentObject();
		if (parentObject == null)
		{
			return;
		}

		// 雨吸収オブジェクトが十分に大きく、PLATEAU建物の場合のみ沈下処理
		if (parentObject.transform.localScale.x >= _BUILDING_BRAKE_SIZE && PlateauUtility.IsPlateauBuilding(other))
		{
			SinkingDownBuilding(other.gameObject);
		}
	}

	private void SinkingDownBuilding(GameObject obj = null)
	{
		GameObject gameObject = GameObject.Find("Plateau");
		if (gameObject != null)
		{
			gameObject.GetComponent<PlateauInfoManager>().SetBuildingToDoom(obj.gameObject);
		}
	}

	private void Absorb(GameObject rainDrop, GameObject other)
	{
		_ = Vector3.zero;
		if (other.transform.localScale.y >= rainDrop.transform.localScale.y)
		{
			other.transform.localScale = CalculateNewScale(rainDrop.transform.localScale, other.transform.localScale);
			AdjustPosition(other);
			GameObjectTreat.DestroyAll(rainDrop);
		}
		else
		{
			rainDrop.transform.localScale = CalculateNewScale(rainDrop.transform.localScale, other.transform.localScale);
			AdjustPosition(rainDrop);
			GameObjectTreat.DestroyAll(other);
			ChangeColliderSize(0.2f);
		}
	}

	private void AdjustPosition(GameObject obj)
	{
		obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y + obj.transform.localScale.y / 2f, obj.transform.position.z);
	}

	private Vector3 CalculateNewScale(Vector3 scale1, Vector3 scale2)
	{
		float num = Mathf.Pow(scale1.x, 3f);
		float num2 = Mathf.Pow(scale2.x, 3f);
		float num3 = Mathf.Pow(num + num2, 1f / 3f);
		return new Vector3(num3, num3, num3);
	}

	private GameObject GetParentObject()
	{
		return this.gameObject.transform.parent.gameObject;
	}

	internal void ChangeColliderSize(float size)
	{
		this.gameObject.GetComponent<SphereCollider>().radius = size;
	}

	private void Awake()
	{
		// RainAbsorbTriggerHandler をアタッチ
		GameObjectTreat.GetOrAddComponent<RainAbsorbTriggerHandler>(gameObject);
	}
}
