using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class GarbageCubeBox : MonoBehaviour, IItemStructProvider
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
            "GarbageCubeBox"
            , "GarbageBoxID"
            , "Tips GarbageCube Box"
            , "GarbageCube box Info"
            , 600
            , GlobalConst.SHORT_SCORE1_SCALE
            , 0.8f
            , 1
            , "imgs/icons/virus-covid-solid"
            , "imgs/icons/virus-covid-solid"
            , 2);

        // Debug.Log("GarbageCube.Awake() _item_struct.Name: " + _item_struct.Name);

// Assets/Resources/imgs/icons/virus-covid-solid.svg
    }

    internal ItemStruct GetItemStruct()
    {
        return _item_struct;
    }

}
