using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class WaterTurret : MonoBehaviour, IItemStructProvider, IUnitStructProvider
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
        
        // 放水タレットの初期化
        _item_struct = new ItemStruct(
            "WaterTurret",  // name
            "ID",           // ID
            "Tips WaterTurret",
            "WaterTurret Info",
            150,            // CreateCost
            GlobalConst.SHORT_SCORE2_SCALE,  // CostType - "CLK"
            0.6f,           // CostTime
            1,              // Stack
            "imgs/icons/hockey-puck-solid",  // ItemIconPath
            "imgs/icons/hockey-puck-solid",  // ItemImagePath
            2               // HolderIndex
        );

        _unit_struct = new UnitStruct(
            _item_struct.Name,       // name
            _item_struct.ItemID,     // UnitID
            1,                       // Lv
            _item_struct.Info,       // Info
            0,                       // UpdateCost
            _item_struct.CreateCost, // DeleteCost
            0,                       // BaseScore
            GlobalConst.SHORT_SCORE2_SCALE  // ScoreType - "CLK"
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
