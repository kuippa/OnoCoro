using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using UnityEngine.TextCore.Text;

public class GarbageCube : MonoBehaviour, IItemStructProvider, ICharacterStructProvider
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
            "GarbageCube"
            , "GarbageID"
            , "Tips GarbageCube"
            , "GarbageCube Info"
            , 30
            , GlobalConst.SHORT_SCORE1_SCALE
            , 0.2f
            , 1
            , "imgs/icons/virus-covid-solid"
            , "imgs/icons/virus-covid-solid"
            , 2);

        _chara_struct = new CharacterStruct(
            10
            ,10
            , GlobalConst.SHORT_SCORE1_SCALE
        );
    }

    internal ItemStruct GetItemStruct()
    {
        return _item_struct;
    }

    internal CharacterStruct GetCharacterStruct()
    {
        return _chara_struct;
    }

}
