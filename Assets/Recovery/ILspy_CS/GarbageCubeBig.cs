// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// GarbageCubeBig
using UnityEngine;

public class GarbageCubeBig : MonoBehaviour, IItemStructProvider
{
	internal ItemStruct _item_struct;

	public ItemStruct ItemStruct => _item_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("GarbageCubeBig", "GarbageCubeBigID", "Tips GarbageCube Big", "GarbageCube big Info", 20, "BIT", 0.1f, 1, "imgs/icons/virus-covid-solid", "imgs/icons/virus-covid-solid", 2);
	}

	internal ItemStruct GetItemStruct()
	{
		return _item_struct;
	}
}
