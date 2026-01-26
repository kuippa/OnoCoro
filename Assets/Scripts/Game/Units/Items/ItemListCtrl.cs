// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// ItemListCtrl
using UnityEngine;
using UnityEngine.UI;

public class ItemListCtrl : MonoBehaviour
{
    private const int _ITEM_LIST_MAX = 26;

    private int GetChildFreeHolderIndex(ItemStruct item_struct)
    {
        int result = -1;
        int childCount = this.gameObject.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            int appendableItemIndex = this.gameObject.transform.GetChild(i).gameObject.GetComponent<ItemHolderCtrl>().GetAppendableItemIndex(item_struct);
            if (appendableItemIndex != -1)
            {
                result = appendableItemIndex;
                break;
            }
        }
        return result;
    }

    internal void SetItemStruct(ItemStruct item_struct)
    {
        int childFreeHolderIndex = GetChildFreeHolderIndex(item_struct);
        if (childFreeHolderIndex != -1)
        {
            this.gameObject.transform.GetChild(childFreeHolderIndex).gameObject.GetComponent<ItemHolderCtrl>().AddItemToHolder(item_struct);
        }
    }

    private void Awake()
    {
        foreach (Transform item in this.gameObject.transform)
        {
            Object.Destroy(item.gameObject);
        }
        int childCount = this.gameObject.transform.childCount;
        int num = 26 - childCount;
        for (int i = 0; i < num; i++)
        {
            GameObject itemHolderPrefab = PrefabManager.ItemHolderPrefab;
            if (itemHolderPrefab == null)
            {
                Debug.LogWarning("ItemHolder prefab not found in PrefabManager");
                break;
            }
            GameObject gameObject = Object.Instantiate(itemHolderPrefab);
            gameObject.transform.SetParent(this.gameObject.transform);
            gameObject.GetComponent<ItemHolderCtrl>().ChangeSiblingIndex(gameObject.transform.GetSiblingIndex());
        }
        this.gameObject.AddComponent<LoupeCtrl>();
    }
}
