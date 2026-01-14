// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// DemCtrl
using System.Linq;
using UnityEngine;

public static class DemCtrl
{
	private const float _POPUP_DISTANCE = 1f;

	private const float _MARGIN_DISTANCE = 3f;

	private const float _RAYCAST_DISTANCE = 200f;

	private static Vector3 _dem_center_pos = Vector3.zero;

	private static GameObject _dem = null;

	internal static Vector3 GetClosestPointOnBounds(Collider other)
	{
		Vector3 position = other.gameObject.transform.position;
		if (_dem == null)
		{
			_dem = GetDemObject();
			_dem_center_pos = GetDemPosition(_dem);
		}
		if (_dem == null)
		{
			return Vector3.zero;
		}
		Collider component = _dem.GetComponent<Collider>();
		if (component == null)
		{
			return Vector3.zero;
		}
		Vector3 closestPoint = component.ClosestPointOnBounds(position);
		float y = other.bounds.size.y;
		for (int i = 0; i < 20; i++)
		{
			closestPoint = AdjustPositionWithinBounds(closestPoint, _dem, y, i);
			if (HitDemPoint(ref closestPoint, other, y))
			{
				break;
			}
		}
		return closestPoint;
	}

	private static bool HitDemPoint(ref Vector3 closestPoint, Collider other, float objectHeight)
	{
		int layerMask = 1 << LayerMask.NameToLayer(GameEnum.LayerType.Ground.ToString());
		if (Physics.Raycast(closestPoint + Vector3.up * 200f, Vector3.down, out var hitInfo, float.PositiveInfinity, layerMask) && hitInfo.collider.gameObject.name != other.gameObject.name)
		{
			closestPoint = hitInfo.point;
			closestPoint.y += objectHeight + 1f;
			return true;
		}
		return false;
	}

	private static Vector3 AdjustPositionWithinBounds(Vector3 position, GameObject demObject, float objectHeight, int iterate = 0)
	{
		MeshFilter component = demObject.GetComponent<MeshFilter>();
		if (component == null)
		{
			return position;
		}
		if (_dem_center_pos == Vector3.zero)
		{
			_dem_center_pos = GetDemPosition(demObject);
		}
		Vector3 result = position;
		Vector3 size = component.mesh.bounds.size;
		result.x = Mathf.Clamp(result.x, _dem_center_pos.x - size.x / 2f + 3f * (float)iterate, _dem_center_pos.x + size.x / 2f - 3f * (float)iterate);
		result.z = Mathf.Clamp(result.z, _dem_center_pos.z - size.z / 2f + 3f * (float)iterate, _dem_center_pos.z + size.z / 2f - 3f * (float)iterate);
		float b = _dem_center_pos.y + size.y * 0.5f + objectHeight;
		result.y = Mathf.Max(result.y, b);
		result.y += 1f;
		return result;
	}

	internal static float GetDemHeight(GameObject demObject)
	{
		if (demObject == null)
		{
			return 0f;
		}
		MeshFilter component = demObject.GetComponent<MeshFilter>();
		if (component == null)
		{
			return 0f;
		}
		return component.mesh.bounds.size.y;
	}

	private static Vector3 GetDemPosition(GameObject dem)
	{
		if (dem == null)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		zero = dem.transform.localPosition;
		MeshRenderer component = dem.GetComponent<MeshRenderer>();
		if (component != null)
		{
			Vector3 center = component.bounds.center;
			zero.x += center.x;
			zero.y += center.y;
			zero.z += center.z;
		}
		return zero;
	}

	internal static GameObject GetDemObject()
	{
		GameObject result = null;
		GameObject[] array = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Ground.ToString());
		if (array == null || array.Length < 1)
		{
			GameObject[] array2 = Object.FindObjectsOfType<GameObject>();
			Debug.Log("GetDemObject dem is null " + array2.Length);
			array = array2.Where((GameObject obj) => obj.name.StartsWith("dem_") && obj.tag == GameEnum.TagType.Untagged.ToString()).ToArray();
		}
		if (array.Length >= 1)
		{
			array[0].tag = GameEnum.TagType.Ground.ToString();
			array[0].layer = LayerMask.NameToLayer(GameEnum.LayerType.Ground.ToString());
			result = array[0];
		}
		return result;
	}

	internal static Vector3 GetDemRndAbovePosition(float drop_distance)
	{
		Vector3 zero = Vector3.zero;
		if (_dem == null)
		{
			_dem = GetDemObject();
			_dem_center_pos = GetDemPosition(_dem);
		}
		MeshFilter component = _dem.GetComponent<MeshFilter>();
		if (component == null)
		{
			return zero;
		}
		Vector3 size = component.mesh.bounds.size;
		zero.x = Random.Range(_dem_center_pos.x - size.x / 2f + 3f, _dem_center_pos.x + size.x / 2f - 3f);
		zero.z = Random.Range(_dem_center_pos.z - size.z / 2f + 3f, _dem_center_pos.z + size.z / 2f - 3f);
		float y = _dem.transform.localScale.y;
		float y2 = _dem_center_pos.y + size.y * 0.5f * y + drop_distance;
		zero.y = y2;
		return zero;
	}

	internal static Vector3 GetDemAbovePosition(GameObject target, float above_distance)
	{
		Vector3 zero = Vector3.zero;
		if (_dem == null)
		{
			_dem = GetDemObject();
			_dem_center_pos = GetDemPosition(_dem);
		}
		MeshFilter component = _dem.GetComponent<MeshFilter>();
		if (component == null)
		{
			return zero;
		}
		_ = component.mesh.bounds.size;
		zero.x = target.transform.position.x;
		zero.z = target.transform.position.z;
		Vector3 closestPoint = _dem.GetComponent<Collider>().ClosestPointOnBounds(zero);
		Collider component2 = target.GetComponent<Collider>();
		float objectHeight = 0f;
		if (HitDemPoint(ref closestPoint, component2, objectHeight))
		{
			zero.y = closestPoint.y;
		}
		zero.y = closestPoint.y + above_distance - 1f;
		return zero;
	}
}
