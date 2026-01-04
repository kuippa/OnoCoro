using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using UnityEngine.TextCore.Text;

public class GarbageCube : MonoBehaviour, IItemStructProvider, IUnitStructProvider
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
            "GarbageCube" // name
            , "GarbageID"   // ID
            , "Tips GarbageCube"
            , "GarbageCube Info"
            , 30    // CreateCost
            , GlobalConst.SHORT_SCORE1_SCALE    // CostType
            , 0.2f  // CostTime
            , 1 // Stack
            , "imgs/icons/virus-covid-solid"    // ItemIconPath
            , "imgs/icons/virus-covid-solid"    // ItemImagePath
            , 2
        );

        _unit_struct = new UnitStruct(
            _item_struct.Name // name
            , _item_struct.ItemID   // UnitID
            , 1 // Lv
            , _item_struct.Info    // Info
            , 0 // UpdateCost
            , 0 // DeleteCost
            , 10  // BaseScore
            , GlobalConst.SHORT_SCORE1_SCALE    // ScoreType
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
