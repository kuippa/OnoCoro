// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// GarbageCubeBox
using UnityEngine;

public class GarbageCubeBox : MonoBehaviour, IItemStructProvider
{
	internal ItemStruct _item_struct;

	public ItemStruct ItemStruct => _item_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("GarbageCubeBox", "GarbageBoxID", "Tips GarbageCube Box", "GarbageCube box Info", 600, "BIT", 0.8f, 1, "imgs/icons/virus-covid-solid", "imgs/icons/virus-covid-solid", 2);
	}

	internal ItemStruct GetItemStruct()
	{
		return _item_struct;
	}
}
