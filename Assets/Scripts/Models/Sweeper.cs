using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class Sweeper : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
    public ItemStruct ItemStruct => _item_struct;
    internal ItemStruct _item_struct = new ItemStruct();
    public UnitStruct UnitStruct => _unit_struct;
    internal UnitStruct _unit_struct = new UnitStruct();

    void Awake()
    // void Start()
    {
        #if UNITY_EDITOR
            // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        _item_struct = new ItemStruct(
            "Sweeper"
            , "GarbageID"
            , "Tips Sweeper"
            , "Sweeper Info"
            , 200  // CreateCost
            , GlobalConst.SHORT_SCORE2_SCALE
            , 0.6f
            , 1
            , "imgs/icons/hockey-puck-solid"
            , "imgs/icons/hockey-puck-solid"
            , 2);

        _unit_struct = new UnitStruct(
            _item_struct.Name // name
            , _item_struct.ItemID   // UnitID
            , 1 // Lv
            , _item_struct.Info    // Info
            , 0 // UpdateCost
            , _item_struct.CreateCost * -1 // DeleteCost
            , 0  // BaseScore
            , GlobalConst.SHORT_SCORE2_SCALE    // ScoreType
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
