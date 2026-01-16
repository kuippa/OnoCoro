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
    // Constants - UI Object Names
    private const string _PLACEMENT_NAME = "Item_";
    private const string _PNL_HOLDER = "pnlHolder";
    private const string _ITEM_ICON = "Item_icon";
    private const string _ACTIVE_IMAGE = "active_Image";
    private const string _TXT_STACK = "txtStack";
    private const string _TXT_INDEX = "txtIndex";
    private const string _TXT_ALT = "txtAlt";

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

        // UI復元: active_Imageが存在しない場合は動的に生成
        EnsureActiveImageExists();
    }

    /// <summary>
    /// active_Imageオブジェクトが存在することを確認。存在しなければ生成
    /// </summary>
    private void EnsureActiveImageExists()
    {
        Transform pnlHolder = SafeFindChild(_PNL_HOLDER);
        if (pnlHolder == null)
        {
            return;
        }

        GameObject activeImageObj = pnlHolder.Find(_ACTIVE_IMAGE).gameObject;
        if (activeImageObj == null)
        {
            return;
        }

        Image image = activeImageObj.GetComponent<Image>();
        image.enabled = false; // 初期状態: 非表示（選択されていない）
    }

    /// <summary>
    /// Transform.Find で null をチェックする補助メソッド
    /// </summary>
    private Transform SafeFindChild(string childName)
    {
        return this.transform.Find(childName);
    }

    /// <summary>
    /// 指定した Transform の子要素を Find する補助メソッド
    /// </summary>
    private Transform SafeFindChild(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }
        return parent.Find(childName);
    }

    private void ChangeIcon(Sprite sprite)
    {
        Transform pnlHolder = SafeFindChild(_PNL_HOLDER);
        if (pnlHolder == null)
        {
            return;
        }

        Image icon = SafeFindChild(pnlHolder, _ITEM_ICON)?.GetComponentInChildren<Image>();
        if (icon == null)
        {
            return;
        }

        icon.sprite = sprite;
        Color color = icon.color;
        color.a = (icon.sprite != null) ? 1f : 0f;
        icon.color = color;
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
        Transform pnlHolder = SafeFindChild(_PNL_HOLDER);
        Transform txtAlt = SafeFindChild(SafeFindChild(pnlHolder, _ITEM_ICON), _TXT_ALT);
        
        if (txtAlt != null)
        {
            Text text = txtAlt.GetComponent<Text>();
            if (text != null)
            {
                text.text = item.Info;
            }
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
            Sprite sprite = SpriteResourceLoader.LoadSprite(item.ItemIconPath);
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
        
        Text indexText = SafeFindChild(_TXT_INDEX)?.GetComponentInChildren<Text>();
        if (indexText != null)
        {
            indexText.text = (index + 1).ToString();
        }

        SetStackText();
    }

    private void SetStackText()
    {
        Text stackText = SafeFindChild(_TXT_STACK)?.GetComponentInChildren<Text>();
        if (stackText == null)
        {
            return;
        }

        stackText.text = (_item.Stack > 1) ? _item.Stack.ToString() : "";
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
        Transform pnlHolder = SafeFindChild(_PNL_HOLDER);
        Transform activeImageTransform = SafeFindChild(pnlHolder, _ACTIVE_IMAGE);
        
        if (activeImageTransform == null)
        {
            return;
        }

        GameObject activeImage = activeImageTransform.gameObject;
        GameObject[] allActiveImageObjects = GetAllActiveImageObjects();

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

            component.enabled = (activeImage == obj) && active;
        }
    }

    /// <summary>
    /// すべてのactive_Imageオブジェクトを取得します
    /// </summary>
    /// <returns>active_Imageオブジェクトの配列</returns>
    private GameObject[] GetAllActiveImageObjects()
    {
        return (from t in Object.FindObjectsOfType<Transform>()
                where t.name == _ACTIVE_IMAGE
                select t.gameObject).ToArray();
    }
}
