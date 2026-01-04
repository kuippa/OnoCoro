// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// SpawnMarkerPointerCtrl
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class SpawnMarkerPointerCtrl : MonoBehaviour
{
	public static SpawnMarkerPointerCtrl instance;

	private static GameObject _marker;

	private float _time;

	private const float _TIME_INTERVAL = 0.05f;

	private const float _MARKER_Y_OFFSET = 0.08f;

	private void SetPositionTMP(Vector3 pos)
	{
		GameObject gameObject = base.transform.Find("tmp_posi").gameObject;
		if (!(gameObject == null))
		{
			TextMeshPro component = gameObject.GetComponent<TextMeshPro>();
			if (!(component == null))
			{
				component.text = pos.x.ToString("F1") + ", " + pos.y.ToString("F1") + ", " + pos.z.ToString("F1");
			}
		}
	}

	private Quaternion GetMarkerRotation()
	{
		Quaternion quaternion = base.transform.rotation;
		if (quaternion == Quaternion.identity)
		{
			Vector3 playerPosition = GetPlayerPosition();
			Vector3 position = base.transform.position;
			Vector3 vector = playerPosition - position;
			vector.y = 0f;
			quaternion = Quaternion.LookRotation(vector * -1f);
		}
		return quaternion;
	}

	private void RayCastPointer()
	{
		Vector2 vector = Mouse.current.position.ReadValue();
		Ray ray = Camera.main.ScreenPointToRay(vector);
		int layerMask = ~LayerMask.GetMask(GameEnum.LayerType.AreaIgnoreRaycast.ToString());
		if (Physics.Raycast(ray, out var hitInfo, 20f, layerMask))
		{
			if (!(hitInfo.collider.gameObject.tag != GameEnum.LayerType.Ground.ToString()) && isNavMeshHit(hitInfo.point))
			{
				Vector3 point = hitInfo.point;
				point.y += 0.08f;
				base.transform.position = point;
				base.transform.rotation = GetMarkerRotation();
				SetPositionTMP(point);
				IsMarkerFarFromPlayer();
			}
		}
		else if (IsMarkerFarFromPlayer())
		{
			Debug.Log("IsMarkerFarFromPlayer: " + IsMarkerFarFromPlayer());
			SetMarkerActive(isActive: false);
		}
	}

	private static bool isNavMeshHit(Vector3 point)
	{
		if (NavMesh.SamplePosition(point, out var _, 10f, -1))
		{
			return true;
		}
		return false;
	}

	private static Vector3 GetPlayerPosition()
	{
		return GameObject.FindWithTag(GameEnum.UnitType.Player.ToString()).transform.position;
	}

	internal static Vector3 GetMarkerPosition()
	{
		if (_marker == null)
		{
			return GetPlayerPosition();
		}
		return _marker.transform.position;
	}

	internal static void SetMarkerActive(bool isActive)
	{
		if (!(_marker == null) && _marker.gameObject.activeSelf != isActive)
		{
			if (isActive)
			{
				_marker.transform.rotation = Quaternion.identity;
			}
			_marker.gameObject.SetActive(isActive);
		}
	}

	private static bool IsMarkerFarFromPlayer()
	{
		if (Vector3.Distance(GetMarkerPosition(), GetPlayerPosition()) > 20f)
		{
			return true;
		}
		return false;
	}

	internal static bool IsMarkerActive()
	{
		if (!ItemAction.IsItemSelected())
		{
			SetMarkerActive(isActive: false);
			return false;
		}
		return _marker.gameObject.activeSelf;
	}

	internal static Quaternion GetMarkerRotateAngle()
	{
		if (!IsMarkerActive())
		{
			return Quaternion.identity;
		}
		return _marker.transform.rotation;
	}

	internal static void RotateMarker(float moveVec)
	{
		if (moveVec < 0f)
		{
			_marker.transform.Rotate(0f, 30f, 0f);
		}
		else
		{
			_marker.transform.Rotate(0f, -30f, 0f);
		}
	}

	private void OnDestory()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		_marker = base.gameObject;
		SetMarkerActive(isActive: false);
	}

	private void Update()
	{
		_time += Time.deltaTime;
		if (_time > 0.05f)
		{
			if (IsMarkerActive())
			{
				RayCastPointer();
			}
			_time = 0f;
		}
	}
}
