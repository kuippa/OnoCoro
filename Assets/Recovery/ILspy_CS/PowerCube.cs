// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PowerCube
using UnityEngine;

public class PowerCube : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
	internal ItemStruct _item_struct;

	internal UnitStruct _unit_struct;

	public ItemStruct ItemStruct => _item_struct;

	public UnitStruct UnitStruct => _unit_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("PowerCube", "powerID", "Tips powerCube", "power Cube Info", 0, "BIT", 0.1f, 1, "imgs/icons/virus-covid-solid", "imgs/icons/virus-covid-solid", 2);
		_unit_struct = new UnitStruct(_item_struct.Name, _item_struct.ItemID, 1, _item_struct.Info, 0, 0, 1000, "CLK");
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
