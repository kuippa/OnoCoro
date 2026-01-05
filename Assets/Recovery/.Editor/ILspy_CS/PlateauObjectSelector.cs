// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PlateauObjectSelector
using PLATEAU.CityInfo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlateauObjectSelector : MonoBehaviour
{
	private const float _CLICK_MAX_DISTANCE = 250f;

	private GameObject _selectedObject;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private GameObject GetTargetObject()
	{
		_selectedObject = null;
		Vector2 vector = Mouse.current.position.ReadValue();
		if (EventSystem.current.IsPointerOverGameObject())
		{
			return null;
		}
		if (Physics.Raycast(Camera.main.ScreenPointToRay(vector), out var hitInfo, 250f))
		{
			_selectedObject = hitInfo.collider.gameObject;
			return _selectedObject;
		}
		return null;
	}

	internal bool IsPLATEAUObject()
	{
		GameObject targetObject = GetTargetObject();
		if (targetObject == null)
		{
			return false;
		}
		return targetObject.GetComponent<PLATEAUCityObjectGroup>() != null;
	}

	internal bool IsPLATEAUObject(GameObject obj)
	{
		return obj.GetComponent<PLATEAUCityObjectGroup>() != null;
	}

	internal GameObject GetSelectedObject()
	{
		return _selectedObject;
	}
}
