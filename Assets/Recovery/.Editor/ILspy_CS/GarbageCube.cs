// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// GarbageCube
using UnityEngine;

public class GarbageCube : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
	internal ItemStruct _item_struct;

	internal UnitStruct _unit_struct;

	private const int _BASE_SCORE = 10;

	public ItemStruct ItemStruct => _item_struct;

	public UnitStruct UnitStruct => _unit_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("GarbageCube", "GarbageID", "Tips GarbageCube", "GarbageCube Info", 30, "BIT", 0.2f, 1, "imgs/icons/virus-covid-solid", "imgs/icons/virus-covid-solid", 2);
		_unit_struct = new UnitStruct(_item_struct.Name, _item_struct.ItemID, 1, _item_struct.Info, 0, 0, 10, "BIT");
	}

	internal static int GetBaseScore()
	{
		return 10;
	}

	internal ItemStruct GetItemStruct()
	{
		return _item_struct;
	}

	internal UnitStruct GetUnitStruct()
	{
		return _unit_struct;
	}
}
