// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// ItemAction
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemAction : MonoBehaviour
{
	public static ItemAction instance;

	private static SpawnCtrl _spawnCtrl;

	private static ItemHolderCtrl _itemHolderCtrl;

	private static ItemStruct _item;

	internal static void SelectItem(int index)
	{
		GameObject itemList = GetItemList();
		if (!(itemList == null) && itemList.transform.childCount >= index)
		{
			_itemHolderCtrl = itemList.transform.Find("Item_" + (index - 1)).gameObject.GetComponent<ItemHolderCtrl>();
			if (_itemHolderCtrl != null)
			{
				_item = _itemHolderCtrl.SelectItem();
			}
		}
	}

	private static GameObject GetItemList()
	{
		GameObject gameObject = GameObject.Find("ItemList");
		if (gameObject == null)
		{
			Debug.Log("ItemAction GetItemList: item_list == null" + gameObject?.name);
			return null;
		}
		return gameObject;
	}

	internal static string GetSelectedItemName()
	{
		return GetSelectedItem().Name;
	}

	private static ItemStruct GetSelectedItem()
	{
		return _item;
	}

	private static ItemHolderCtrl GetItemHolderCtrl(ItemStruct item)
	{
		return _itemHolderCtrl;
	}

	internal static void ActItemUse()
	{
		ItemStruct selectedItem = GetSelectedItem();
		Mouse.current.position.ReadValue();
		_spawnCtrl = SpawnCtrl.Instance;
		if (!_spawnCtrl.CallUnitByName(selectedItem.Name))
		{
			Debug.Log("ItemAction ActItemUse: CallUnitByName failed: " + selectedItem.Name);
			return;
		}
		selectedItem.Stack = -1;
		ItemHolderCtrl itemHolderCtrl = GetItemHolderCtrl(selectedItem);
		if (itemHolderCtrl != null)
		{
			itemHolderCtrl.AddItemToHolder(selectedItem);
			SpawnMarkerPointerCtrl.SetMarkerActive(isActive: false);
		}
		else
		{
			Debug.Log("ItemAction ActItemUse: item_holder == null" + itemHolderCtrl?.name);
		}
	}

	internal static bool IsItemSelected()
	{
		ItemStruct selectedItem = GetSelectedItem();
		if (selectedItem.Name == null)
		{
			return false;
		}
		_item = selectedItem;
		return true;
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
		_spawnCtrl = SpawnCtrl.Instance;
	}
}
