using UnityEngine;
using CommonsUtility;

/// <summary>
/// 消火栓（防災施策・Season 3 W2）
///
/// ビルドメニュー（ItemCreateCtrl）が YAML itemlists の名前から Type.GetType() で
/// 本クラスを解決して ItemStruct を取得するため、クラス名は itemlists の表記
/// "Hydrant" と一致させ、グローバル名前空間に置く（FireCube 等の既存 Units と同じ慣習）。
/// 配置後の効果（範囲内 FireCube の鎮火）は InfrastructureUnit が担う。
/// </summary>
public class Hydrant : MonoBehaviour, IItemStructProvider, IUnitStructProvider
{
    internal ItemStruct _item_struct;
    internal UnitStruct _unit_struct;

    public ItemStruct ItemStruct => _item_struct;
    public UnitStruct UnitStruct => _unit_struct;

    private void Awake()
    {
        _item_struct = new ItemStruct(
            "Hydrant",         // name（itemlists / SpawnController の分岐名と一致させる）
            "hydrantID",       // ID
            "Tips Hydrant",
            "消火栓: 周囲の火災を強力に鎮火する防災施策（範囲: 小・効果: 高）",
            InfrastructureConfig.GetCost(GameEnum.ModelsType.Hydrant),  // CreateCost（YAML 外部化）
            GlobalConst.SHORT_SCORE1_SCALE,      // CostType - "BIT"
            2f,                // CostTime
            1,                 // Stack
            "imgs/icons/fire-extinguisher-solid",  // ItemIconPath
            "imgs/icons/fire-extinguisher-solid",  // ItemImagePath
            1                  // HolderIndex
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
