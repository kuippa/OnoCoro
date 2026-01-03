using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.EventSystems;
using CommonsUtility;
using Unity.VisualScripting;

public class LoupeCtrl : MonoBehaviour
{
    public static LoupeCtrl instance = null;
    internal static bool _loupeMode = false;

    // private static ItemStruct _item = new ItemStruct();
    // private static ItemHolderCtrl _itemHolderCtrl = null;


    internal static bool IsLoupe()
    {
        return _loupeMode;
    }

    internal static bool IsLoupe(string item_name)
    {
        
        if (item_name == "Loupe")
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
        GameObject plateauInfo  = GameObject.Find("Plateau");
        if (plateauInfo == null)
        {
            return;
        }
        // Plateau 建物情報表示
        // bool boolplateau = plateauInfo.GetComponent<PlateauInfo>().GetPlateauInfo();

        bool boolplateau = plateauInfo.GetComponent<PlateauInfoManager>().IsPlateauObject();
        // Plateau 以外のユニットなどの情報を表示
        // if (!boolplateau)
        if (boolplateau)
        {
            plateauInfo.GetComponent<PlateauInfoManager>().DisplayPlateauInfo();
        }
        else
        {
            GameObject uiInfo = GameObject.Find("UIInfo");
            if (uiInfo != null)
            {
                uiInfo.GetComponent<InfoWindowCtrl>().GetTargetUnit();
                // uiInfo.GetComponent<InfoWindowCtrl>().ToggleInfoWindow(true);
            }
            return;
        }
    }

    void OnDestory()
    {
        Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (instance == this)
        {
            instance = null;
        }
    }

    void Awake()
    {
        Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (instance == null)
        {
            instance = this;
        }

        ItemStruct itemLoupe = new ItemStruct();
        itemLoupe = this.gameObject.AddComponent<Loupe>()._item_struct;
        ItemListCtrl itemListCtrl = this.gameObject.GetComponentInChildren<ItemListCtrl>();
        itemListCtrl.SetItemStruct(itemLoupe);
    }

}
