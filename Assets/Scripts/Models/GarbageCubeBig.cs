using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class GarbageCubeBig : MonoBehaviour, IItemStructProvider
{
    public ItemStruct ItemStruct => _item_struct;
    internal ItemStruct _item_struct = new ItemStruct();

    void Awake()
    // void Start()
    {
        #if UNITY_EDITOR
            // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        _item_struct = new ItemStruct(
            "GarbageCubeBig"
            , "GarbageCubeBigID"
            , "Tips GarbageCube Big"
            , "GarbageCube big Info"
            , 20
            , GlobalConst.SHORT_SCORE1_SCALE
            , 0.1f
            , 1
            , "imgs/icons/virus-covid-solid"
            , "imgs/icons/virus-covid-solid"
            , 2);
    }

    internal ItemStruct GetItemStruct()
    {
        return _item_struct;
    }

}
