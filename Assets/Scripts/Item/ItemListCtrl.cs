using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonsUtility;

public class ItemListCtrl : MonoBehaviour
{
    const int _ITEM_LIST_MAX = 26;

    private int GetChildFreeHolderIndex(ItemStruct item_struct)
    {
        int ret = -1;
        // 子供要素数を取得
        int childCount = this.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            GameObject unit = this.transform.GetChild(i).gameObject;
            int free_index = unit.GetComponent<ItemHolderCtrl>().GetAppendableItemIndex(item_struct);
            if (free_index != -1)
            {
                ret = free_index;
                break;
            }
            else
            {
                // Debug.Log("free_index: " + free_index + " i: " + i);
            }
        }
        return ret;
    }

    internal void SetItemStruct(ItemStruct item_struct)
    {
        int idx = GetChildFreeHolderIndex(item_struct);
        if (idx != -1)
        {
            GameObject unit = this.transform.GetChild(idx).gameObject;
            // Debug.Log("idx: " + idx + " unit: " + unit.name + " unit.GetSiblingIndex: " + unit.transform.GetSiblingIndex());
            unit.GetComponent<ItemHolderCtrl>().AddItemToHolder(item_struct);
        }
    }

    internal void SelectItem(int index)
    {
        int childCount = this.transform.childCount;
        if (childCount < index)
        {
            return;
        }
        int item_index = index - 1;

        GameObject item = this.transform.Find("Item_" + item_index).gameObject;
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            // itemの子要素が空ではなかった場合
            ItemStruct item_struct =  item.GetComponent<ItemHolderCtrl>().GetItem();
            if (item_struct.Name != null)
            {
                // Debug.Log($"item_struct: {item_struct.Name} {item_struct.Info}");
                MarkerPointerCtrl.SetMarkerActive(true);
                button.Select();
            }
            else
            {
                MarkerPointerCtrl.SetMarkerActive(false);
                button.Select();
                button.OnDeselect(null);
            }
        }
    }

    void Awake()
    {
        // 子要素をすべて削除
        foreach (Transform child in this.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // 子供要素数を取得
        int childCount = this.transform.childCount;
        int list_cnt = _ITEM_LIST_MAX - childCount;

        for (int i = 0; i < list_cnt; i++)
        {
            GameObject Item_holder_prefab = Resources.Load<GameObject>("Prefabs/UI/Item_holder");
            GameObject unit = Instantiate(Item_holder_prefab);
            unit.transform.SetParent(this.transform);
            unit.GetComponent<ItemHolderCtrl>().ChangeSiblingIndex(unit.transform.GetSiblingIndex());
        }

        // 虫眼鏡を作成
        LoupeCtrl loupeCtrl = this.gameObject.AddComponent<LoupeCtrl>();
        // bool ret = LoupeCtrl.IsLoupe();

        // ItemStruct itemLoupe = new ItemStruct();
        // itemLoupe = this.gameObject.AddComponent<Loupe>()._item_struct;
        // ItemListCtrl itemListCtrl = this.gameObject.GetComponentInChildren<ItemListCtrl>();
        // itemListCtrl.SetItemStruct(itemLoupe);

        
        // GameObjectTreat.DebugScriptList();


    }


}
