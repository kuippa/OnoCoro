// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// WaterTurretCtrl
using System;
using CommonsUtility;
using UnityEngine;

public class WaterTurretCtrl : MonoBehaviour
{
	private float _shoot_speed = 22f;

	private float _t;

	private const float _INTERVAL_TIME = 0.4f;

	private const float _MAX_SEARCH_RANGE = 30f;

	private GameObject _parent_holder;

	private const string _WATER_SPHERE_PARENT_NAME = "WaterSphere";

	private float _sinValue;

	private bool _reSearch;

	private GameObject _nozzle;

	private GameObject _target;

	private static int _waterUnitIndex;

	private bool _isDelete;

	private GameObject CreateWaterBullet()
	{
		GameObject original = Resources.Load<GameObject>("Prefabs/WorkUnit/WaterSphere");
		Vector3 position = _nozzle.transform.position;
		Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
		GameObject obj = UnityEngine.Object.Instantiate(original, position, rotation);
		obj.name = GameEnum.TagType.Water.ToString() + _waterUnitIndex++;
		obj.tag = GameEnum.TagType.Water.ToString();
		Transform holderParentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_holder, "WaterSphere");
		obj.transform.SetParent(holderParentTransform);
		return obj;
	}

	private void WaterShoot()
	{
		if (_sinValue > 360f)
		{
			_sinValue = 0f;
			_reSearch = true;
		}
		float num = Mathf.Sin(_sinValue * (MathF.PI / 180f));
		if (!(num < 0f))
		{
			GameObject gameObject = CreateWaterBullet();
			Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = gameObject.AddComponent<Rigidbody>();
			}
			rigidbody.useGravity = true;
			rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			Vector3 vector = new Vector3(0f, _target.transform.localScale.y - 0.2f, 0f);
			Vector3 vector2 = _target.transform.position - vector;
			Vector3 position = gameObject.transform.position;
			Vector3 vector3 = vector2 - position;
			float num2 = vector3.magnitude - (0.6f + 1f * num);
			float num3 = (45f + 20f * num) * (MathF.PI / 180f);
			float magnitude = Physics.gravity.magnitude;
			float num4 = Mathf.Sqrt(num2 * magnitude / Mathf.Sin(2f * num3));
			Vector3 linearVelocity = vector3.normalized * num4 * Mathf.Cos(num3);
			linearVelocity.y = num4 * Mathf.Sin(num3);
			rigidbody.linearVelocity = linearVelocity;
		}
	}

	private void LookRotateTarget()
	{
		if (!(_target == null) && !(_target.transform == null))
		{
			Vector3 forward = _target.transform.position - base.gameObject.transform.position;
			forward.y = 0f;
			Quaternion b = Quaternion.LookRotation(forward);
			float t = 1f;
			if (Quaternion.Angle(base.gameObject.transform.rotation, b) < 0.1f)
			{
				WaterShoot();
			}
			else
			{
				base.gameObject.transform.rotation = Quaternion.Slerp(base.gameObject.transform.rotation, b, t);
			}
		}
	}

	private bool SearchTargets()
	{
		if (_target != null && !_reSearch)
		{
			return true;
		}
		_reSearch = false;
		GameObject[] array = GameObject.FindGameObjectsWithTag(GameEnum.TagType.FireCube.ToString());
		if (array.Length == 0)
		{
			return false;
		}
		float num = 30f;
		_target = null;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			float num2 = Vector3.Distance(base.gameObject.transform.position, gameObject.transform.position);
			if (num2 < num)
			{
				num = num2;
				_target = gameObject;
				Debug.Log("target: " + _target.name);
			}
		}
		return true;
	}

	public void CreateWaterTurretUnit(Vector3 setPoint)
	{
		base.gameObject.tag = GameEnum.TagType.WaterTurret.ToString();
		int num = GameObjectTreat.IndexObjectByTag(base.gameObject.tag);
		base.gameObject.name = GameEnum.ModelsType.WaterTurret.ToString() + num;
		base.gameObject.AddComponent<WaterTurret>();
		base.gameObject.GetComponent<WaterTurret>()._item_struct.ItemID = base.name;
		base.gameObject.GetComponent<WaterTurret>()._unit_struct.UnitID = base.name;
		base.gameObject.transform.position = setPoint;
	}

	internal void StartDeleteUnitProcess()
	{
		_isDelete = true;
	}

	internal void DeleteUnitProcess()
	{
		UnitStruct unitStruct = GetComponent<WaterTurret>().GetUnitStruct();
		ScoreCtrl.UpdateAndDisplayScore(unitStruct.DeleteCost, unitStruct.ScoreType);
		GameObjectTreat.DestroyAll(base.gameObject);
	}

	private bool IsPowerState()
	{
		bool num = ScoreCtrl.IsScorePositiveInt(0, "CLK");
		if (!num)
		{
			SignPowerOutageCtrl.GetOrCreateCirclePowerOutage(base.gameObject);
			return num;
		}
		SignPowerOutageCtrl.UnSignPowerOutage(base.gameObject);
		return num;
	}

	private void OnDestroy()
	{
		GameObjectTreat.DestroyAll(base.gameObject);
	}

	private void Awake()
	{
		_nozzle = base.gameObject.transform.Find("ExtinguishingCylinder/nozzle").gameObject;
		GameObject gameObject = base.gameObject;
		gameObject.AddComponent<WaterTurret>();
		gameObject.GetComponent<WaterTurret>()._item_struct.ItemID = gameObject.name;
		gameObject.GetComponent<WaterTurret>()._unit_struct.UnitID = gameObject.name;
	}

	private void Update()
	{
		_t += Time.deltaTime;
		if (_t > 0.4f && !_isDelete)
		{
			_sinValue += _shoot_speed;
			_t = 0f;
			if (IsPowerState() && SearchTargets())
			{
				LookRotateTarget();
			}
		}
	}
}
