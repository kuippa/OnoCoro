using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

public class GarbageCubeBox : MonoBehaviour, IItemStructProvider
{
    internal ItemStruct _item_struct;
    
    public ItemStruct ItemStruct => _item_struct;

    private void Awake()
    // void Start()  // 必要に応じてStartに変更可能
    {
        #if UNITY_EDITOR
            // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            // Debug.Log("GarbageCube.Awake() _item_struct.Name: " + _item_struct.Name);
        #endif
        
        // ゴミ箱（ボックス）の初期化
        // アイコン: Assets/Resources/imgs/icons/virus-covid-solid.svg
        _item_struct = new ItemStruct(
            "GarbageCubeBox",
            "GarbageBoxID",
            "Tips GarbageCube Box",
            "GarbageCube box Info",
            600,
            GlobalConst.SHORT_SCORE1_SCALE,  // "BIT"
            0.8f,
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
