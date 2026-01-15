using UnityEngine;

public class SentryGuard : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
	internal ItemStruct _item_struct = new ItemStruct();

	internal UnitStruct _unit_struct = new UnitStruct();

	public ItemStruct ItemStruct => _item_struct;

	public UnitStruct UnitStruct => _unit_struct;

	private void Awake()
	{
		_item_struct = new ItemStruct(
			"SentryGuard"
			, "SentryGuard"
			, "監視員さん"
			, "ごみの投げ捨てを見られている間だけ思いとどまらせる。"
			, 20 // CreateCost
			, "BIT"
			, 1f
			, 1
			, "imgs/icons/spaghetti-monster-flying-solid"
			, "imgs/icons/spaghetti-monster-flying-solid"
			, 2
		);
		_unit_struct = new UnitStruct(
			_item_struct.Name // name
			, _item_struct.ItemID   // UnitID
			, 1 // Lv
			, _item_struct.Info    // Info
			, 0 // UpdateCost
			, _item_struct.CreateCost // DeleteCost
			, 0  // BaseScore
			, "CLK"    // ScoreType
		);
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
