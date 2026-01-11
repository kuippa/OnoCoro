// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// StopPlate
using UnityEngine;

public class StopPlate : MonoBehaviour, IItemStructProvider
{
	internal ItemStruct _item_struct;

	internal UnitStruct _unit_struct;

	public ItemStruct ItemStruct => _item_struct;

	public UnitStruct UnitStruct => _unit_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("StopPlate", "StopPlateID", "Tips StopPlate", "StopPlate Info", 5, "BIT", 0.1f, 1, "imgs/icons/virus-covid-solid", "imgs/icons/virus-covid-solid", 10);
		_unit_struct = new UnitStruct(_item_struct.Name, _item_struct.ItemID, 1, _item_struct.Info, 0, _item_struct.CreateCost, 0, "CLK");
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
