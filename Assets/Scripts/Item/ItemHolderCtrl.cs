using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// アイテムホルダーの制御クラス
/// ドラッグ&ドロップ、アイテム選択、スタック管理を行う
/// </summary>
public class ItemHolderCtrl : MonoBehaviour, IDragHandler, IEndDragHandler, IDropHandler
{
    private const string _PLACEMENT_NAME = "Item_";
    private ItemStruct _item = new ItemStruct();

    private Vector2 _prevPos;

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 SetPos = new Vector3(eventData.position.x, eventData.position.y, this.transform.position.z);
        this.transform.position = SetPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log("OnEndDrag" + this.name + " :" + this.transform.GetSiblingIndex());
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            // Debug.Log(result.gameObject.name + " :" + result.gameObject.GetComponentInParent<ItemHolderCtrl>().name);
            if (this.name == result.gameObject.GetComponentInParent<ItemHolderCtrl>().name 
            || result.gameObject.GetComponentInParent<ItemHolderCtrl>() == null)
            {
                continue;
            }
            // _PLACEMENT_NAME で始まるオブジェクトの場合
            if (result.gameObject.GetComponentInParent<ItemHolderCtrl>().name.StartsWith(_PLACEMENT_NAME))
            {
                // Debug.Log("OnEndDrag" + this.name + " :" 
                //     + this.transform.GetSiblingIndex() + " : " 
                //     + result.gameObject.GetComponentInParent<ItemHolderCtrl>().name + " :" 
                //     + result.gameObject.GetComponentInParent<ItemHolderCtrl>().transform.GetSiblingIndex());
                int btrade_index = this.transform.GetSiblingIndex();
                int index = result.gameObject.GetComponentInParent<ItemHolderCtrl>().transform.GetSiblingIndex();
                ChangeSiblingIndex(index);
                result.gameObject.GetComponentInParent<ItemHolderCtrl>().ChangeSiblingIndex(btrade_index);
                break;
            }
        }
        LayoutRebuilder.MarkLayoutForRebuild(this.transform as RectTransform);
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Debug.Log("OnDrop" + this.name + " :" + this.transform.GetSiblingIndex());
    }


    private void Awake()
    {
        // EventTriggerを使ってSelectイベントを登録
        EventTrigger eventTrigger = this.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        entry.callback.AddListener(delegate
        {
            OnClickItemHolder();
        });
        eventTrigger.triggers.Add(entry);
    }

    private void ChangeIcon(Sprite sprite)
    {
        Image icon = this.transform.Find("pnlHolder").Find("Item_icon").GetComponentInChildren<Image>();
        icon.sprite = sprite;
        // 子要素のpnlHolderに画像が設定されていた場合アルファ値を255にする
        if (icon.sprite != null)
        {
            // 画像が設定されていた場合透過度を0にする
            Color color = icon.color;
            color.a = 1.0f;
            icon.color = color;
        }
        else
        {
            Color color = icon.color;
            color.a = 0.0f;
            icon.color = color;

        }
    }

    private int GetFreeItemIndex()
    {
        if (_item.Name == null)
        {
            return this.transform.GetSiblingIndex();
        }
        return -1;
    }

    internal int GetAppendableItemIndex(ItemStruct item)
    {
        int ret = GetFreeItemIndex();
        if (ret == -1)
        {
            // _item が item と同じ場合
            if (_item.Name == item.Name)
            {
                return this.transform.GetSiblingIndex();
            }
        }
        return ret;
    }

    private void SetAltText(ItemStruct item)
    {
        Transform unit = this.transform.Find("pnlHolder").Find("Item_icon").Find("txtAlt");
        if (unit != null)
        {
            Text text1 = unit.gameObject.GetComponent<Text>();
            text1.text = item.Info;
        }
    }

    internal void AddItemToHolder(ItemStruct item)
    {
        if (_item.Name == item.Name)
        {
            _item.AddStack(item.Stack);
            SetStackText();
            if (_item.Stack == 0)
            {
                _item = new ItemStruct();
                ChangeIcon(null);
                SetAltText(_item);
                SetActiveImageFlag(false);
                return;
            }
        }
        else
        {
            _item = item;
            Sprite sprite = Resources.Load<Sprite>(item.ItemIconPath);
            ChangeIcon(sprite);
            SetAltText(item);
        }
    }

    internal ItemStruct GetItem()
    {
        return _item;
    }

    internal void ChangeSiblingIndex(int index)
    {
        this.transform.SetSiblingIndex(index);
        this.name = _PLACEMENT_NAME + (this.transform.GetSiblingIndex()).ToString();
        this.transform.Find("txtIndex").GetComponentInChildren<Text>().text = (index+1).ToString();
        SetStackText();

        // TODO: デバッグのためにランダムで色を変える
        // this.GetComponentInChildren<Image>().color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f, 0.25f, 0.25f);
        // this.GetComponentInChildren<Image>().color = UnityEngine.Random.ColorHSV(0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 0.85f, 0.85f);

    }

    private void SetStackText()
    {
        if (_item.Stack > 1)
        {
            this.transform.Find("txtStack").GetComponentInChildren<Text>().text = _item.Stack.ToString();
        }
        else
        {
            this.transform.Find("txtStack").GetComponentInChildren<Text>().text = "";
        }
    }

    /// <summary>
    /// アイテムホルダーがクリックされた時の処理
    /// </summary>
    private void OnClickItemHolder()
    {
        int siblingIndex = this.transform.GetSiblingIndex();
        Debug.Log($"SelectItemByClick: {this.name} :{siblingIndex}");
        ItemAction.SelectItem(siblingIndex + 1);
    }

    /// <summary>
    /// アイテムを選択状態にします
    /// </summary>
    /// <returns>選択されたアイテム</returns>
    internal ItemStruct SelectItem()
    {
        ItemStruct item = GetItem();
        Button component = GetComponent<Button>();
        if (component == null)
        {
            return item;
        }

        if (item.Name != null)
        {
            if (!LoupeCtrl.IsLoupe(item.Name))
            {
                SpawnMarkerPointerCtrl.SetMarkerActive(true);
            }
            component.Select();
            SetActiveImageFlag(true);
        }
        else
        {
            SpawnMarkerPointerCtrl.SetMarkerActive(false);
            component.Select();
            component.OnDeselect(null);
            SetActiveImageFlag(false);
        }
        return item;
    }

    /// <summary>
    /// アクティブ画像フラグを設定します
    /// 選択されたアイテムのみactive_Imageを表示
    /// </summary>
    /// <param name="active">表示する場合true</param>
    private void SetActiveImageFlag(bool active)
    {
        GameObject[] allActiveImageObjects = GetAllActiveImageObjects();
        GameObject activeImage = this.transform.Find("pnlHolder").Find("active_Image").gameObject;

        foreach (GameObject obj in allActiveImageObjects)
        {
            if (obj == null)
            {
                continue;
            }

            Image component = obj.GetComponent<Image>();
            if (component == null)
            {
                continue;
            }

            if (activeImage == obj)
            {
                component.enabled = active;
            }
            else
            {
                component.enabled = false;
            }
        }
    }

    /// <summary>
    /// すべてのactive_Imageオブジェクトを取得します
    /// </summary>
    /// <returns>active_Imageオブジェクトの配列</returns>
    private GameObject[] GetAllActiveImageObjects()
    {
        return (from t in Object.FindObjectsOfType<Transform>()
                where t.name == "active_Image"
                select t.gameObject).ToArray();
    }
}
