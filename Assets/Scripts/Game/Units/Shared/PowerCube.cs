using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public class PowerCube : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
    public ItemStruct ItemStruct => _item_struct;
    internal ItemStruct _item_struct = new ItemStruct();
    public UnitStruct UnitStruct => _unit_struct;
    internal UnitStruct _unit_struct = new UnitStruct();

    // BaseScore の定義（デフォルト値）
    internal const float _DEFAULT_BASE_SCORE = 1000f;

    void Awake()
    {
        #if UNITY_EDITOR
        #endif
        _item_struct = new ItemStruct(
            "PowerCube"
            , "powerID"
            , "Tips powerCube"
            , "power Cube Info"
            , 0 // CreateCost
            , GlobalConst.SHORT_SCORE1_SCALE
            , 0.1f
            , 1
            , "imgs/icons/virus-covid-solid"
            , "imgs/icons/virus-covid-solid"
            , 2
        );

        _unit_struct = new UnitStruct(
            _item_struct.Name // name
            , _item_struct.ItemID   // UnitID
            , 1 // Lv
            , _item_struct.Info    // Info
            , 0 // UpdateCost
            , 0 // DeleteCost
            , (int)_DEFAULT_BASE_SCORE  // BaseScore
            , GlobalConst.SHORT_SCORE2_SCALE    // ScoreType
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

    /// <summary>
    /// BaseScore を設定します（YAML スポーン時用）
    /// キューブサイズは SpawnController 側で既に調整される
    /// ここではスコア値のみを更新
    /// </summary>
    internal void SetBaseScore(float baseScore)
    {
        if (baseScore <= 0)
        {
            return;
        }

        // UnitStruct の BaseScore を更新
        _unit_struct.BaseScore = (int)baseScore;
    }

}
