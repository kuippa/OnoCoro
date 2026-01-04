using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CommonsUtility;

public class ItemHolderCtrl : MonoBehaviour , IDragHandler, IEndDragHandler, IDropHandler
{
    const string _PLACEMENT_NAME = "Item_";
    ItemStruct _item = new ItemStruct();

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


    void Awake()
    {
		Button btn1 = GameObject.Find(this.name).GetComponent<Button>();
		// btn1.onClick.AddListener(OnTestClick);
        // ボタンのセレクトが解除されたら
        // btn1.OnDeselect.AddListener(OnTestClick);
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


    public void OnTestClick()
    {
        // Debug.Log("click " + this.name + " :" + this.transform.GetSiblingIndex());
        // Debug.Log("click " + this.name + " :" + this.transform.GetSiblingIndex() + " :" + _item.Name + " " + _item.Stack);
        // Debug.Log(_item.ItemID + " " + _item.Name + " " + _item.ToolTip + " " + _item.Info + " " + _item.CreateCost + " " + _item.CostType + " " + _item.CostTime + " " + _item.Stack + " " + _item.ItemIconPath + " " + _item.ItemImagePath + " " + _item.HolderIndex);

        // MarkerPointerCtrl.SetMarkerActive(true);
    }


}
