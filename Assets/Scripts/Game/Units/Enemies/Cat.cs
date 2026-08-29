using UnityEngine;

/// <summary>
/// 巨大猫のユニット定義（PLATEAU CityHack 2026）
///
/// Litter.cs と同じ役割で、ItemStruct / UnitStruct を保持するだけのデータ用コンポーネント。
/// 実際の移動・解体処理は EnemyCat が持つ。
/// </summary>
public class Cat : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
    // Cat の能力パラメータ
    internal const float DEMOLISH_INTERVAL = 1.5f;   // 建物を解体する間隔（秒）
    internal const float DEMOLISH_RADIUS = 12f;      // 解体対象を探す半径（m）
    internal const int MAX_DEMOLISH_COUNT = 20;      // 1 匹が解体できる建物数の上限

    internal ItemStruct _item_struct;

    internal UnitStruct _unit_struct;

    public ItemStruct ItemStruct => _item_struct;

    public UnitStruct UnitStruct => _unit_struct;

    private void Awake()
    {
        _item_struct = new ItemStruct(
            "Cat"
            , "CatID"
            , "巨大猫 Tips Cat"
            , "巨大猫 経路上の建物を踏み潰して更地にする。跡地には廃材が残る Cat Info"
            , -200
            , "CLK"
            , 3f
            , 1
            , "imgs/icons/spaghetti-monster-flying-solid"
            , "imgs/icons/spaghetti-monster-flying-solid"
            , 2);

        _unit_struct = new UnitStruct(
            _item_struct.Name
            , _item_struct.ItemID
            , 1
            , _item_struct.Info
            , 0
            , 0 // DeleteCost が 0 だと削除できないため、敵ユニットは DeleteCost を 0 に設定
            , 0
            , "CLK");
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
