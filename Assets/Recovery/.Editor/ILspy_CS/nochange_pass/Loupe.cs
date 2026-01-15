// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Loupe
using UnityEngine;

public class Loupe : MonoBehaviour, IItemStructProvider
{
	internal ItemStruct _item_struct;

	public ItemStruct ItemStruct => _item_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("Loupe", "LoupeID", "Tips Loupe", "Loupe Info", -1, "BIT", 0f, 1, "imgs/icons/search_fill24", "imgs/icons/search_fill24", 2);
	}

	internal ItemStruct GetItemStruct()
	{
		return _item_struct;
	}
}
