using System.Diagnostics;
using UnityEngine;
using Debug = CommonsUtility.Debug;

public class DustBox : MonoBehaviour, IItemStructProvider
{
    internal ItemStruct _item_struct;
    internal UnitStruct _unit_struct;

    public ItemStruct ItemStruct => _item_struct;
    public UnitStruct UnitStruct => _unit_struct;

    private void Awake()
    {
        _item_struct = new ItemStruct(
            "DustBox"
            , "DustBoxID"
            , "ゴミ箱"
            , "ごみが入ると消える。近くにあればそこに捨てようかなぐらいのモラルを引き起こす。"
            , 300
            , "BIT"
            , 1f
            , 1
            , "imgs/icons/spaghetti-monster-flying-solid"
            , "imgs/icons/spaghetti-monster-flying-solid"
            , 2
        );
        _unit_struct = new UnitStruct(
            _item_struct.Name
            , _item_struct.ItemID
            , 1
            , _item_struct.Info
            , 0
            , _item_struct.CreateCost
            , 0
            , "CLK"
        );
    }

    // private void OnEnable()
    // {
    //     // 右クリック・ルーペモードで検出されるようにタグを設定
    //     gameObject.tag = GameEnum.TagType.DustBox.ToString();
    // }

    internal ItemStruct GetItemStruct()
    {
        Debug.Log($"[DustBox] GetItemStruct called. Name: {_item_struct.Name}, ItemID: {_item_struct.ItemID}");
        return _item_struct;
    }

    internal UnitStruct GetUnitStruct()
    {
        Debug.Log($"[DustBox] GetUnitStruct called. Name: {_unit_struct.Name}, UnitID: {_unit_struct.UnitID}");
        return _unit_struct;
    }

    private void OnDestroy()
    {
        // NavMesh Carving をクリーンアップ（障害物除外を解除）
        NavMeshManager.DisableCarvingForObstacle(gameObject);
    }
}
