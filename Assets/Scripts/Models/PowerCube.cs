using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class PowerCube : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
    public ItemStruct ItemStruct => _item_struct;
    internal ItemStruct _item_struct = new ItemStruct();
    public UnitStruct UnitStruct => _unit_struct;
    internal UnitStruct _unit_struct = new UnitStruct();

    void Awake()
    {
        #if UNITY_EDITOR
            // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        _item_struct = new ItemStruct(
            "PowerCube"
            , "powerID"
            , "Tips powerCube"
            , "power Cube Info"
            , 0 // CreateCost
            , GlobalConst.SHORT_SCORE1_SCALE
            , 0.1f
            , 1
            , "imgs/icons/virus-covid-solid"
            , "imgs/icons/virus-covid-solid"
            , 2
        );

        _unit_struct = new UnitStruct(
            _item_struct.Name // name
            , _item_struct.ItemID   // UnitID
            , 1 // Lv
            , _item_struct.Info    // Info
            , 0 // UpdateCost
            , 0 // DeleteCost
            , 1000  // BaseScore
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
