using UnityEngine;
using CommonsUtility;

/// <summary>
/// 防火水槽（防災施策・Season 3 W2）
///
/// クラス名は itemlists の表記 "Cistern" と一致させ、グローバル名前空間に置く
/// （ビルドメニューの Type.GetType() 解決のため。詳細は Hydrant.cs のコメント参照）。
/// 配置後の効果（広範囲・弱めの延焼抑制）は InfrastructureUnit が担う。
/// </summary>
public class Cistern : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
    internal ItemStruct _item_struct;
    internal UnitStruct _unit_struct;

    public ItemStruct ItemStruct => _item_struct;
    public UnitStruct UnitStruct => _unit_struct;

    private void Awake()
    {
        _item_struct = new ItemStruct(
            "Cistern",         // name（itemlists / SpawnController の分岐名と一致させる）
            "cisternID",       // ID
            "Tips Cistern",
            "防火水槽: 広い範囲の延焼をゆるやかに抑える防災施策（範囲: 大・効果: 中）",
            InfrastructureConfig.GetCost(GameEnum.ModelsType.Cistern),  // CreateCost（YAML 外部化）
            GlobalConst.SHORT_SCORE1_SCALE,      // CostType - "BIT"
            2f,                // CostTime
            1,                 // Stack
            "imgs/icons/house-flood-water-solid",  // ItemIconPath
            "imgs/icons/house-flood-water-solid",  // ItemImagePath
            2                  // HolderIndex
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
