using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public class Loupe : MonoBehaviour, IItemStructProvider
{
    public ItemStruct ItemStruct => _item_struct;
    internal ItemStruct _item_struct = new ItemStruct();

    void Awake()
    // void Start()
    {
        #if UNITY_EDITOR
        #endif
        _item_struct = new ItemStruct(
            "Loupe"
            , "LoupeID"
            , "Tips Loupe"
            , "Loupe Info"
            , -1
            , GlobalConst.SHORT_SCORE1_SCALE
            , 0.0f
            , 1
            , "imgs/icons/search_fill24"
            , "imgs/icons/search_fill24"
            , 2);


// Assets/Resources/imgs/icons/virus-covid-solid.svg
    }

    internal ItemStruct GetItemStruct()
    {
        return _item_struct;
    }

}
