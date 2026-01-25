using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// アイテムの選択と使用を管理するクラス
/// </summary>
public class ItemAction : MonoBehaviour
{
    public static ItemAction instance = null;
    private static SpawnController _spawnCtrl = null;
    private static ItemHolderCtrl _itemHolderCtrl = null;
    private static ItemStruct _item = new ItemStruct();

    /// <summary>
    /// 番号指定でアイテムを選択します
    /// </summary>
    /// <param name="index">アイテムの番号（1から開始）</param>
    internal static void SelectItem(int index)
    {
        GameObject itemList = GetItemList();
        if (itemList == null)
        {
            return;
        }

        if (itemList.transform.childCount < index)
        {
            return;
        }

        GameObject itemObject = itemList.transform.Find($"Item_{index - 1}").gameObject;
        if (itemObject != null)
        {
            _itemHolderCtrl = itemObject.GetComponent<ItemHolderCtrl>();
            if (_itemHolderCtrl != null)
            {
                _item = _itemHolderCtrl.SelectItem();
            }
        }
    }

    /// <summary>
    /// ItemListオブジェクトを取得します
    /// </summary>
    /// <returns>ItemListオブジェクト、見つからない場合null</returns>
    private static GameObject GetItemList()
    {
        GameObject itemList = GameObject.Find("ItemList");
        if (itemList == null)
        {
            Debug.Log($"ItemAction GetItemList: item_list == null");
            return null;
        }
        return itemList;
    }

    /// <summary>
    /// 現在選択されているアイテムを取得します（UI選択から）
    /// </summary>
    internal static void GetSelectedItem()
    {
        SpawnMarkerPointerCtrl.SetMarkerActive(false);
        GameObject itemHolder = GetItemList();
        if (itemHolder == null)
        {
            return;
        }

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
        {
            return;
        }

        if (selected.name.StartsWith("Item_"))
        {
            _itemHolderCtrl = selected.GetComponent<ItemHolderCtrl>();
            if (_itemHolderCtrl != null)
            {
                ItemStruct item = _itemHolderCtrl.GetItem();
                _item = item;
                if (item.Name != null)
                {
                    SpawnMarkerPointerCtrl.SetMarkerActive(true);
                }
                else
                {
                    SpawnMarkerPointerCtrl.SetMarkerActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 選択されているアイテムの名前を取得します
    /// </summary>
    /// <returns>アイテム名</returns>
    internal static string GetSelectedItemName()
    {
        return GetSelectedItemStruct().Name;
    }

    /// <summary>
    /// 選択されているアイテム構造体を取得します
    /// </summary>
    /// <returns>選択中のアイテム</returns>
    private static ItemStruct GetSelectedItemStruct()
    {
        return _item;
    }

    /// <summary>
    /// アイテムのItemHolderCtrlを取得します
    /// </summary>
    /// <param name="item">対象のアイテム</param>
    /// <returns>ItemHolderCtrl</returns>
    private static ItemHolderCtrl GetItemHolderCtrl(ItemStruct item)
    {
        return _itemHolderCtrl;
    }

    /// <summary>
    /// 選択されているアイテムを使用します
    /// </summary>
    internal static void ActItemUse()
    {
        ItemStruct selectedItem = GetSelectedItemStruct();
        Mouse.current.position.ReadValue();
        _spawnCtrl = SpawnController.Instance;
        if (!_spawnCtrl.CallUnitByName(selectedItem.Name))
        {
            Debug.Log($"ItemAction ActItemUse: CallUnitByName failed: {selectedItem.Name}");
            return;
        }
        selectedItem.Stack = -1;
        ItemHolderCtrl itemHolderCtrl = GetItemHolderCtrl(selectedItem);
        if (itemHolderCtrl != null)
        {
            itemHolderCtrl.AddItemToHolder(selectedItem);
            SpawnMarkerPointerCtrl.SetMarkerActive(false);
        }
        else
        {
            Debug.Log($"ItemAction ActItemUse: item_holder == null");
        }
    }

    /// <summary>
    /// アイテムが選択されているかチェックします
    /// </summary>
    /// <returns>選択されている場合true</returns>
    internal static bool IsItemSelected()
    {
        ItemStruct selectedItem = GetSelectedItemStruct();
        if (selectedItem.Name == null)
        {
            return false;
        }
        _item = selectedItem;
        return true;
    }

    private void OnDestory()
    {
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Awake()
    {
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (instance == null)
        {
            instance = this;
        }
        _spawnCtrl = SpawnController.Instance;
    }
}
