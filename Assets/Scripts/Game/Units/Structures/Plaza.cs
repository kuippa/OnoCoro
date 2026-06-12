using UnityEngine;
using CommonsUtility;

/// <summary>
/// 避難広場（防災施策・Season 3 W2）
///
/// クラス名は itemlists の表記 "Plaza" と一致させ、グローバル名前空間に置く
/// （ビルドメニューの Type.GetType() 解決のため。詳細は Hydrant.cs のコメント参照）。
/// W2 時点では投資記録のみ。W3 の結果計算で「人的被害の軽減」補正として使用する。
/// </summary>
public class Plaza : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
    internal ItemStruct _item_struct;
    internal UnitStruct _unit_struct;

    public ItemStruct ItemStruct => _item_struct;
    public UnitStruct UnitStruct => _unit_struct;

    private void Awake()
    {
        _item_struct = new ItemStruct(
            "Plaza",           // name（itemlists / SpawnController の分岐名と一致させる）
            "plazaID",         // ID
            "Tips Plaza",
            "避難広場: 周辺住民の避難先となり人的被害を軽減する防災施策（効果は年度結果に反映）",
            InfrastructureFactory.PLAZA_COST,    // CreateCost
            GlobalConst.SHORT_SCORE1_SCALE,      // CostType - "BIT"
            3f,                // CostTime
            1,                 // Stack
            "imgs/icons/building-solid",  // ItemIconPath
            "imgs/icons/building-solid",  // ItemImagePath
            3                  // HolderIndex
        );

        _unit_struct = new UnitStruct(
            _item_struct.Name,
            _item_struct.ItemID,
            1,                 // Lv
            _item_struct.Info,
            0,                 // UpdateCost
            0,                 // DeleteCost
            0,                 // BaseScore
            GlobalConst.SHORT_SCORE1_SCALE
        );
    }
}
