using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class GarbageCubeBig : MonoBehaviour, IItemStructProvider
{
    internal ItemStruct _item_struct;
    
    public ItemStruct ItemStruct => _item_struct;

    private void Awake()
    // void Start()  // 必要に応じてStartに変更可能
    {
        #if UNITY_EDITOR
            // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        
        // 大きなゴミキューブの初期化
        _item_struct = new ItemStruct(
            "GarbageCubeBig",
            "GarbageCubeBigID",
            "Tips GarbageCube Big",
            "GarbageCube big Info",
            20,
            GlobalConst.SHORT_SCORE1_SCALE,  // "BIT"
            0.1f,
            1,
            "imgs/icons/virus-covid-solid",
            "imgs/icons/virus-covid-solid",
            2);
    }

    internal ItemStruct GetItemStruct()
    {
        return _item_struct;
    }

}
