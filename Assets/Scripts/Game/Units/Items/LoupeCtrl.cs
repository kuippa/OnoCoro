using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.EventSystems;
using CommonsUtility;
using Unity.VisualScripting;
using Debug = CommonsUtility.Debug;

public class LoupeCtrl : MonoBehaviour
{
    public static LoupeCtrl instance = null;
    internal static bool _loupeMode = false;

    private static ItemStruct _item = new ItemStruct();
    // private static ItemHolderCtrl _itemHolderCtrl = null;


    internal static bool IsLoupe()
    {
        return _loupeMode;
    }

    internal static bool IsLoupe(string item_name)
    {
        
        // if (item_name == "Loupe")
        if (item_name == _item.Name)
        {
            SetLoupeMode(true);
            return true;
        }
        return false;
    }

    internal static void SetLoupeMode(bool mode)
    {
        _loupeMode = mode;
    }
 
    internal static void ActLoupe()
    {
        _loupeMode = false;
        GameObject plateauInfo = GameObject.Find(GlobalConst.PLATEAU_OBJ_NAME);
        if (plateauInfo == null)
        {
            return;
        }

        PlateauInfoManager plateauInfoManager = plateauInfo.GetComponent<PlateauInfoManager>();
        if (plateauInfoManager == null)
        {
            return;
        }

        bool boolplateau = plateauInfoManager.IsPlateauObject();
        if (boolplateau)
        {
            plateauInfoManager.DisplayPlateauInfo();
        }
        else
        {
            GameObject uiInfo = GameObject.Find(GlobalConst.UI_INFO_OBJ_NAME);
            if (uiInfo != null)
            {
                uiInfo.GetComponent<InfoWindowCtrl>().GetTargetUnit();
            }
        }

        DeselectLoupe();
    }

    private static void DeselectLoupe()
    {
        string selectedItemName = ItemAction.GetSelectedItemName();
        if (selectedItemName != null && selectedItemName == _item.Name)
        {
            ItemAction.DeselectItem();
        }
    }

    void OnDestory()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        ItemStruct itemLoupe = new ItemStruct();
        itemLoupe = this.gameObject.AddComponent<Loupe>()._item_struct;
        ItemListCtrl itemListCtrl = this.gameObject.GetComponentInChildren<ItemListCtrl>();
        itemListCtrl.SetItemStruct(itemLoupe);
        _item = itemLoupe;
    }

}
