// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// FireCube
using UnityEngine;

public class FireCube : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
	internal ItemStruct _item_struct;

	internal UnitStruct _unit_struct;

	public ItemStruct ItemStruct => _item_struct;

	public UnitStruct UnitStruct => _unit_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("FireCube", "fireID", "Tips fireCube", "fireCube Info", 1, "BIT", 0.1f, 1, "imgs/icons/virus-covid-solid", "imgs/icons/virus-covid-solid", 5);
		_unit_struct = new UnitStruct(_item_struct.Name, _item_struct.ItemID, 1, _item_struct.Info, 0, 0, 10, "BIT");
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
