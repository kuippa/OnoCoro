// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// RainDropsCtrl
using System.Collections;
using CommonsUtility;
using UnityEngine;

public class RainDropsCtrl : MonoBehaviour
{
	private Vector3 _rainDropPosition = Vector3.zero;

	private bool _is_rain = true;

	private GameObject _puddles_holder;

	private float _checkRadius = 10f;

	private int _requiredRainCount = 5;

	private int _check_count;

	private const float RAIN_CHECK = 5f;

	private const float STATIONARY_DISTANCE = 5f;

	private const float PUDDLE_ABOVE_DISTANCE = 0.25f;

	private const int PUDDLE_MAX_COUNT = 10;

	private const int MAX_CHECK_COUNT = 10;

	private bool MakePuddle()
	{
		if ((float)GameObject.FindGameObjectsWithTag(GameEnum.TagType.Puddle.ToString()).Length > 10f)
		{
			return false;
		}
		GameObject original = Resources.Load<GameObject>("Prefabs/WorkUnit/Puddle");
		Vector3 demAbovePosition = DemCtrl.GetDemAbovePosition(base.gameObject, 0.25f);
		Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
		GameObject obj = Object.Instantiate(original, demAbovePosition, rotation);
		obj.tag = GameEnum.TagType.Puddle.ToString();
		obj.name = GameEnum.TagType.Puddle.ToString() + Time.time;
		obj.transform.SetParent(GetPuddleHolder().transform);
		return true;
	}

	private GameObject GetPuddleHolder()
	{
		if (_puddles_holder == null)
		{
			_puddles_holder = GameObject.Find("puddles");
			if (_puddles_holder == null)
			{
				_puddles_holder = new GameObject("puddles");
			}
		}
		return _puddles_holder;
	}

	private bool CheckExistRainsInRadius()
	{
		bool result = false;
		Collider[] array = Physics.OverlapSphere(base.transform.position, _checkRadius);
		int num = 0;
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].CompareTag(GameEnum.TagType.RainDrop.ToString()))
			{
				num++;
			}
		}
		if (num >= _requiredRainCount)
		{
			result = true;
		}
		return result;
	}

	private void OnTriggerCheck()
	{
		_check_count++;
		if ((float)_check_count > 10f * GameSpeedCtrl.GetGameSpeed())
		{
			_is_rain = false;
			GameObjectTreat.DestroyAll(base.gameObject);
			return;
		}
		if (!base.gameObject.activeSelf)
		{
			_is_rain = false;
			return;
		}
		if (_rainDropPosition != Vector3.zero && Vector3.Distance(_rainDropPosition, base.gameObject.transform.position) <= 5f)
		{
			if (GameConfig._STAGE_PADDLE_MODE && CheckExistRainsInRadius())
			{
				if (MakePuddle())
				{
					_is_rain = false;
					GameObjectTreat.DestroyAll(base.gameObject);
				}
				return;
			}
			if (GameConfig._STAGE_RAIN_ABSORB_MODE)
			{
				ChangeColliderSize();
			}
		}
		_rainDropPosition = base.gameObject.transform.position;
	}

	private void ChangeColliderSize()
	{
		base.gameObject.transform.Find("absorbcollider").gameObject.GetComponent<RainAbsorbCtrl>().ChangeColliderSize(0.7f);
	}

	private IEnumerator RainDrops()
	{
		while (_is_rain)
		{
			yield return new WaitForSeconds(5f);
			OnTriggerCheck();
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
		StartCoroutine(RainDrops());
	}
}
