using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class Sweeper : MonoBehaviour, IItemStructProvider
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
            "Sweeper"
            , "GarbageID"
            , "Tips Sweeper"
            , "Sweeper Info"
            , -200
            , GlobalConst.SHORT_SCORE2_SCALE
            , 0.6f
            , 1
            , "imgs/icons/hockey-puck-solid"
            , "imgs/icons/hockey-puck-solid"
            , 2);

        // Debug.Log("GarbageCube.Awake() _item_struct.Name: " + _item_struct.Name);

// Assets/Resources/imgs/icons/virus-covid-solid.svg
    }

    internal ItemStruct GetItemStruct()
    {
        return _item_struct;
    }

}
