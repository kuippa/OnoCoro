// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// WaterTurret
using UnityEngine;

public class WaterTurret : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
	internal ItemStruct _item_struct;

	internal UnitStruct _unit_struct;

	public ItemStruct ItemStruct => _item_struct;

	public UnitStruct UnitStruct => _unit_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("WaterTurret", "ID", "Tips WaterTurret", "WaterTurret Info", 150, "CLK", 0.6f, 1, "imgs/icons/hockey-puck-solid", "imgs/icons/hockey-puck-solid", 2);
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
