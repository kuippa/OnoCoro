using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class FireCube : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
    internal ItemStruct _item_struct;
    internal UnitStruct _unit_struct;
    
    public ItemStruct ItemStruct => _item_struct;
    public UnitStruct UnitStruct => _unit_struct;

    private void Awake()
    {
        #if UNITY_EDITOR
            // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        
        // 炎キューブの初期化
        _item_struct = new ItemStruct(
            "FireCube",  // name
            "fireID",    // ID
            "Tips fireCube",
            "fireCube Info",
            1,           // CreateCost
            GlobalConst.SHORT_SCORE1_SCALE,  // CostType - "BIT"
            0.1f,        // CostTime
            1,           // Stack
            "imgs/icons/virus-covid-solid",  // ItemIconPath
            "imgs/icons/virus-covid-solid",  // ItemImagePath
            5            // HolderIndex
        );

        _unit_struct = new UnitStruct(
            _item_struct.Name,      // name
            _item_struct.ItemID,    // UnitID
            1,                      // Lv
            _item_struct.Info,      // Info
            0,                      // UpdateCost
            0,                      // DeleteCost
            10,                     // BaseScore
            GlobalConst.SHORT_SCORE1_SCALE  // ScoreType - "BIT"
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
