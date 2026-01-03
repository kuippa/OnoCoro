using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class Litter : MonoBehaviour, IItemStructProvider
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
            "Litter"
            , "LitterID"
            , "Tips Litter"
            , "Litter Info"
            , -200
            , GlobalConst.SHORT_SCORE2_SCALE
            , 3.0f
            , 1
            , "imgs/icons/spaghetti-monster-flying-solid"
            , "imgs/icons/spaghetti-monster-flying-solid"
            , 2);

        // Debug.Log("Litter.Awake() _item_struct.Name: " + _item_struct.Name);
    }

    internal ItemStruct GetItemStruct()
    {
        return _item_struct;
    }

}
