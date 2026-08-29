using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// 不燃ゴミキューブ（PLATEAU CityHack 2026）
///
/// GarbageCube と対になる不燃物。解体廃棄物のうち
/// コンクリートがら・金属くず・ガラスなど、燃やせないものを表す。
///
/// [なぜ分けるか]
/// 災害廃棄物の実務では可燃物と不燃物で処理先も処理費用も異なり、
/// 「何トン出るか」だけでなく「そのうち燃やせないものが何トンか」が
/// 仮置場の計画に効いてくるため。
///
/// [GarbageCube との違い]
/// スコアや構造は同じで、タグ（GarbageNoBurn）と見た目だけが異なる。
/// 燃える処理（Flame / Burning）は tag Garbage を対象にしているため、
/// タグを分けることで不燃ゴミは自動的に延焼対象から外れる。
/// </summary>
public class GarbageCubeNoBurn : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
    internal ItemStruct _item_struct = new ItemStruct();
    internal UnitStruct _unit_struct = new UnitStruct();

    private const int _BASE_SCORE = 10;

    public ItemStruct ItemStruct => _item_struct;
    public UnitStruct UnitStruct => _unit_struct;

    void Awake()
    {
        _item_struct = new ItemStruct(
            "GarbageCubeNoBurn" // name
            , "GarbageNoBurnID"   // ID
            , "Tips GarbageCubeNoBurn"
            , "不燃ゴミ（コンクリートがら・金属など）GarbageCubeNoBurn Info"
            , 30    // CreateCost
            , GlobalConst.SHORT_SCORE1_SCALE    // CostType
            , 0.2f  // CostTime
            , 1 // Stack
            , "imgs/icons/virus-covid-solid"    // ItemIconPath
            , "imgs/icons/virus-covid-solid"    // ItemImagePath
            , 2
        );

        _unit_struct = new UnitStruct(
            _item_struct.Name // name
            , _item_struct.ItemID   // UnitID
            , 1 // Lv
            , _item_struct.Info    // Info
            , 0 // UpdateCost
            , 0 // DeleteCost
            , 10  // BaseScore
            , GlobalConst.SHORT_SCORE1_SCALE    // ScoreType
        );
    }

    /// <summary>
    /// 基準スコア。可燃ゴミと同値にしてある
    /// （廃棄物量の換算式を可燃・不燃で共通に保つため）
    /// </summary>
    internal static int GetBaseScore()
    {
        return _BASE_SCORE;
    }

    internal ItemStruct GetItemStruct()
    {
        return _item_struct;
    }

    internal UnitStruct GetUnitStruct()
    {
        return _unit_struct;
    }
}
