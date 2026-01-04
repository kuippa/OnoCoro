// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// TowerSentryGuard
using System.Collections;
using UnityEngine;

public class TowerSentryGuard : MonoBehaviour
{
	private const bool IS_SWING = true;

	private const float SWING_INTERVAL = 0.5f;

	private const float SWING_MAX_ANGLE = 20f;

	private const float SWING_ANGLE_RATE = 2f;

	private bool _swing_mode = true;

	private bool _swing_direction = true;

	private float _swing_angle;

	private float _object_angle;

	private void SwingGameObject()
	{
		if (_swing_direction)
		{
			_swing_angle += 2f;
		}
		else
		{
			_swing_angle -= 2f;
		}
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles.y = _swing_angle + _object_angle;
		base.transform.eulerAngles = eulerAngles;
		if (_swing_angle > 20f || _swing_angle < -20f)
		{
			_swing_direction = !_swing_direction;
		}
	}

	private IEnumerator Swing_monit()
	{
		_object_angle = base.transform.eulerAngles.y;
		while (_swing_mode)
		{
			yield return new WaitForSeconds(0.5f);
			SwingGameObject();
		}
	}

	private void Start()
	{
		if (_swing_mode)
		{
			StartCoroutine(Swing_monit());
		}
	}
}
