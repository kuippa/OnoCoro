// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Earthquake
using System;
using UnityEngine;

public class Earthquake : MonoBehaviour
{
	private GameObject _dem;

	private float _time;

	private float _time_duration;

	private Vector3 _org_stage_vector;

	private float _degree;

	private bool _is_earthquake;

	private float _total_duration = 3.5f;

	private float _interval = 0.02f;

	private float _magnitude = 1f;

	private const float _DURATION = 4.5f;

	private const float _INTERVAL = 0.02f;

	internal void EventEarthQuake(float magnitude, float duration = 4.5f, float interval = 0.02f)
	{
		_is_earthquake = true;
		_interval = interval;
		_total_duration = duration;
		_magnitude = magnitude;
		_degree = 0f;
	}

	private void QualeP()
	{
		float num = CalcSin();
		if (_total_duration > _time_duration)
		{
			_dem.transform.position = new Vector3(_org_stage_vector.x, _org_stage_vector.y + num, _org_stage_vector.z);
			_degree += 30f;
			return;
		}
		_dem.transform.position = _org_stage_vector;
		_degree = 0f;
		_time_duration = 0f;
		_is_earthquake = false;
	}

	private float CalcSin()
	{
		float num = 0f;
		num = (float)Math.Sin((float)((double)_degree * Math.PI / 180.0));
		return CalcAmpDecay() * num;
	}

	private float CalcAmpDecay()
	{
		float result = 0f;
		float num = 1f;
		if (_time_duration > _total_duration / 2f)
		{
			num = -1f;
		}
		if (_time_duration != 0f)
		{
			result = num * _magnitude * (float)Math.Pow(_time_duration / _total_duration, 2.0);
		}
		return result;
	}

	private void Awake()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("Ground");
		if (gameObject != null)
		{
			_dem = gameObject.gameObject;
			_org_stage_vector = _dem.transform.position;
		}
		else
		{
			Debug.Log("Earthquake Awake ground is null");
		}
	}

	internal void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
		{
			Debug.Log("OnTriggerEnter " + other.name + " object:" + other.gameObject.name);
			EventEarthQuake(1.8f);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (_is_earthquake)
		{
			_time += Time.deltaTime;
			_time_duration += Time.deltaTime;
			if (_time > _interval)
			{
				_time = 0f;
				QualeP();
			}
		}
	}
}
