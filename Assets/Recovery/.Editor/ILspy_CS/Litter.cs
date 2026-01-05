// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Litter
using UnityEngine;

public class Litter : MonoBehaviour, IItemStructProvider
{
	internal ItemStruct _item_struct;

	internal UnitStruct _unit_struct;

	public ItemStruct ItemStruct => _item_struct;

	public UnitStruct UnitStruct => _unit_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("Litter", "LitterID", "ほかしもん Tips Litter", "ほかしもん ごみを巻き散らかす、まくゴミには数に限りがある Litter Info", -200, "CLK", 3f, 1, "imgs/icons/spaghetti-monster-flying-solid", "imgs/icons/spaghetti-monster-flying-solid", 2);
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
