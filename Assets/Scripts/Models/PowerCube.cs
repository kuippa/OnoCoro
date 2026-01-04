using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class PowerCube : MonoBehaviour, IItemStructProvider
{
    public ItemStruct ItemStruct => _item_struct;
    internal ItemStruct _item_struct = new ItemStruct();
    public CharacterStruct CharacterStruct => _chara_struct;
    internal CharacterStruct _chara_struct = new CharacterStruct();


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
            , 0
            , GlobalConst.SHORT_SCORE1_SCALE
            , 0.1f
            , 1
            , "imgs/icons/virus-covid-solid"
            , "imgs/icons/virus-covid-solid"
            , 2);

        _chara_struct = new CharacterStruct(
            10
            , 2000
            , GlobalConst.SHORT_SCORE2_SCALE
        );

        // this.Name = name;
        // this.ItemID = ItemID;
        // this.ToolTip = ToolTip;
        // this.Info = Info;
        // this.CreateCost = CreateCost;
        // this.CostType = CostType;
        // this.CostTime = CostTime;
        // this.Stack = Stack;
        // this.ItemIconPath = ItemIconPath;
        // this.ItemImagePath = ItemImagePath;
        // this.HolderIndex = HolderIndex;

    }

    internal ItemStruct GetItemStruct()
    {
        return _item_struct;
    }
}
