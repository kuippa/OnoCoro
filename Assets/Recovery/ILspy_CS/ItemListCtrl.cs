// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// ItemListCtrl
using UnityEngine;

public class ItemListCtrl : MonoBehaviour
{
	private const int _ITEM_LIST_MAX = 26;

	private int GetChildFreeHolderIndex(ItemStruct item_struct)
	{
		int result = -1;
		int childCount = base.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			int appendableItemIndex = base.transform.GetChild(i).gameObject.GetComponent<ItemHolderCtrl>().GetAppendableItemIndex(item_struct);
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
			base.transform.GetChild(childFreeHolderIndex).gameObject.GetComponent<ItemHolderCtrl>().AddItemToHolder(item_struct);
		}
	}

	private void Awake()
	{
		foreach (Transform item in base.transform)
		{
			Object.Destroy(item.gameObject);
		}
		int childCount = base.transform.childCount;
		int num = 26 - childCount;
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Item_holder"));
			gameObject.transform.SetParent(base.transform);
			gameObject.GetComponent<ItemHolderCtrl>().ChangeSiblingIndex(gameObject.transform.GetSiblingIndex());
		}
		base.gameObject.AddComponent<LoupeCtrl>();
	}
}
