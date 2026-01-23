using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using PLATEAU.CityInfo;
using System;

public class PlateauObjectSelector : MonoBehaviour
{

    private const float _CLICK_MAX_DISTANCE = 250.0f;
    private GameObject _selectedObject;


    private void Start()
    {
    }

    private void Update()
    {
        // if (Mouse.current.leftButton.wasPressedThisFrame)
        // {
        //     TrySelectObject();
        // }
    }

    private GameObject GetTargetObject()
    {
        _selectedObject = null;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return null;
        }

        Ray PointRay = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(PointRay, out hit, _CLICK_MAX_DISTANCE))
        {
            _selectedObject = hit.collider.gameObject;
            return _selectedObject;
        }
        return null;
    }
    internal bool IsPLATEAUObject()
    {
        GameObject obj = GetTargetObject();
        if (obj == null)
        {
            return false;
        }
        return obj.GetComponent<PLATEAUCityObjectGroup>() != null;
    }

    internal bool IsPLATEAUObject(GameObject obj)
    {
        return obj.GetComponent<PLATEAUCityObjectGroup>() != null;
    }

    internal GameObject GetSelectedObject()
    {
        return _selectedObject;
    }

    // デバッグ用：選択されたオブジェクトを視覚的に示す
    // private void HighlightSelectedObject(GameObject obj)
    // {
    //     // 実装は省略（例：一時的に色を変える、アウトラインを表示するなど）
    // }
}