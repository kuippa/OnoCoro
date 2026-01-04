using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEditor;
using CommonsUtility;

public class ItemAction : MonoBehaviour
{
    public static ItemAction instance = null;
    private static SpawnCtrl _spawnCtrl = null;
    private static ItemHolderCtrl _itemHolderCtrl = null;
    private static ItemStruct _item = new ItemStruct();

    internal static void GetSelectedItem()
    {
        MarkerPointerCtrl.SetMarkerActive(false);
        GameObject item_holder = GameObject.Find("ItemList");
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (item_holder == null || selected == null)
        {
            return;
        }

        if (selected.name.StartsWith("Item_"))
        {
            _itemHolderCtrl = selected.GetComponent<ItemHolderCtrl>();
            ItemStruct item = _itemHolderCtrl.GetItem();
            _item = item;
            if (LoupeCtrl.IsLoupe(item.Name))
            {
                return;
            }
            if (item.Name != null)
            {
                MarkerPointerCtrl.SetMarkerActive(true);
            }
            else
            {
                MarkerPointerCtrl.SetMarkerActive(false);
            }
            return;
        }

    }

    internal static void ActItemUse()
    {
        Vector2 mousePosision2 = Mouse.current.position.ReadValue();
        _spawnCtrl = SpawnCtrl.Instance;
        if (!_spawnCtrl.CallUnitByName(_item.Name))
        {
            return;
        }
        _item.Stack = -1;
        _itemHolderCtrl.AddItemToHolder(_item);
        MarkerPointerCtrl.SetMarkerActive(false);
    }

    internal static bool IsItemSelected()
    {
        if (_item.Name == null)
        {
            return false;
        }
        return true;
    }
    
    void OnDestory()
    {
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (instance == this)
        {
            instance = null;
        }
    }

    void Awake()
    {
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (instance == null)
        {
            instance = this;
        }
        // _spawnCtrl = GetSpawnCtrl();
        _spawnCtrl = SpawnCtrl.Instance;

    }


}
