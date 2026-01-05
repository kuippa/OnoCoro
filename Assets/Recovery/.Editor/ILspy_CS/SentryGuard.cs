// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// SentryGuard
using UnityEngine;

public class SentryGuard : MonoBehaviour, IItemStructProvider
{
	internal ItemStruct _item_struct;

	internal UnitStruct _unit_struct;

	public ItemStruct ItemStruct => _item_struct;

	public UnitStruct UnitStruct => _unit_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct("SentryGuard", "SentryGuard", "監視員さん", "ごみの投げ捨てを見られている間だけ思いとどまらせる。", 20, "BIT", 1f, 1, "imgs/icons/spaghetti-monster-flying-solid", "imgs/icons/spaghetti-monster-flying-solid", 2);
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
