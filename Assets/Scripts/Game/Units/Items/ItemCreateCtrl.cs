// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// ItemCreateCtrl
using System;
using System.Collections.Generic;
using CommonsUtility;
using TMPro;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.UI;

public class ItemCreateCtrl : MonoBehaviour
{
    private bool _create_start;

    private float _time;

    private float _timeLimit = 5f;

    private int _page;

    private List<ItemStruct> _itemList = new List<ItemStruct>();

    private static GameObject _UIItemCreate;

    [SerializeField]
    public Button _btnClose;

    public Button _btnOK;

    public Button _btnCreate;

    public Image _gauge;

    public Button _btnLeft;

    public Button _btnRight;


    internal void OnClickCreate()
    {
        ResetGauge();
        _btnCreate.interactable = false;
        if (PayItemCreateCost())
        {
            _create_start = true;
        }
        else
        {
            SetPagenate(_itemList);
        }
    }

    private bool PayItemCreateCost()
    {
        bool result = false;
        ItemStruct itemStruct = _itemList[_page];
        int intScore = itemStruct.CreateCost * -1;
        if (ScoreCtrl.IsScorePositiveInt(intScore, itemStruct.CostType))
        {
            ScoreCtrl.UpdateAndDisplayScore(intScore, itemStruct.CostType);
            return true;
        }
        return result;
    }

    internal void OnClickClose()
    {
        SwitchActive(isActive: false);
    }

    private void InitWindow()
    {
        _btnClose = this.gameObject.transform.Find("CreateWindow/titlebar/btnClose").GetComponent<Button>();
        if (_btnClose != null)
        {
            _btnClose.onClick.AddListener(OnClickClose);
        }
        else
        {
            Debug.Log("btnClose is null");
        }
        _btnOK = this.gameObject.transform.Find("CreateWindow/footer/btnOK").GetComponent<Button>();
        if (_btnOK != null)
        {
            _btnOK.onClick.AddListener(OnClickClose);
        }
        else
        {
            Debug.Log("btnOK is null");
        }
        _btnCreate = this.gameObject.transform.Find("CreateWindow/mainarea/pnlMain/btnCreate").GetComponent<Button>();
        if (_btnCreate != null)
        {
            _btnCreate.onClick.AddListener(OnClickCreate);
        }
        else
        {
            Debug.Log("btnCreate is null");
        }
        _gauge = this.gameObject.transform.Find("CreateWindow/mainarea/pnlMain/timeGauge/timeGaugeFill").GetComponent<Image>();
        if (_gauge != null)
        {
            _ = _gauge;
            ResetGauge();
        }
        else
        {
            Debug.Log("timeGaugeFill is null");
        }
        _btnLeft = this.gameObject.transform.Find("CreateWindow/mainarea/pnlLeft/btnLeft").GetComponent<Button>();
        _btnRight = this.gameObject.transform.Find("CreateWindow/mainarea/pnlRight/btnRight").GetComponent<Button>();
        if (_btnLeft != null && _btnRight != null)
        {
            _btnLeft.onClick.AddListener(delegate
            {
                OnClickPage(-1);
            });
            _btnRight.onClick.AddListener(delegate
            {
                OnClickPage(1);
            });
        }
        else
        {
            Debug.Log("btnLeft or btnRight is null");
        }
        SetItems();
    }

    private List<ItemStruct> GetItemList()
    {
        if (_itemList.Count == 0)
        {
            InitializeItemList();
        }

        return _itemList;
    }

    private void InitializeItemList()
    {
        List<ItemStruct> itemList = new List<ItemStruct>();
        new List<string>();
        foreach (string item in StageYamlRepository.GetItemList())
        {
            Type type = Type.GetType(item);
            if (type != null)
            {
                itemList = SetItemLists(type, itemList);
            }
        }
        _itemList = itemList;
    }

    private List<ItemStruct> SetItemLists(Type type, List<ItemStruct> itemList)
    {
        if (this.gameObject.AddComponent(type) is IItemStructProvider itemStructProvider)
        {
            itemList.Add(itemStructProvider.ItemStruct);
        }
        else
        {
            Debug.LogWarning("Type " + type.Name + " does not implement IItemStructProvider.");
        }
        return itemList;
    }



    private void SetItems()
    {
        List<ItemStruct> itemList = GetItemList();
        if (itemList.Count == 0)
        {
            SwitchActive(isActive: false);
            return;
        }
        SetItem(itemList[_page]);
        SetPagenate(itemList);
    }

    public void SwitchActive(bool isActive)
    {
        if (GetItemList().Count == 0)
        {
            _UIItemCreate.SetActive(value: false);
            return;
        }
        _UIItemCreate.SetActive(isActive);
        SetCreateButtonInteractable(_itemList[_page]);
    }

    private void SetCreateButtonInteractable(ItemStruct itemStruct)
    {
        if (ScoreCtrl.IsScorePositiveInt(itemStruct.CreateCost * -1, itemStruct.CostType))
        {
            _btnCreate.interactable = true;
        }
        else
        {
            _btnCreate.interactable = false;
        }
    }

    private void SetItem(ItemStruct itemStruct)
    {
        GameObject gameObject = this.gameObject.transform.Find("CreateWindow/mainarea/pnlMain").gameObject;
        gameObject.transform.Find("txtName").gameObject.GetComponent<TextMeshProUGUI>().text = itemStruct.Name;
        gameObject.transform.Find("txtCost").gameObject.GetComponent<TextMeshProUGUI>().text = itemStruct.CreateCost + itemStruct.CostType + "/" + itemStruct.CostTime + "s";
        _timeLimit = itemStruct.CostTime;
        GameObject obj = gameObject.transform.Find("imgIcon").gameObject;
        Image imageComponent = obj.GetComponent<Image>();
        SpriteResourceLoader.SetSpriteToImage(imageComponent, itemStruct.ItemImagePath);
        obj.transform.Find("txtAlt").gameObject.GetComponent<Text>().text = itemStruct.Info;
        SetCreateButtonInteractable(itemStruct);
    }

    private void SetPagenate(List<ItemStruct> itemList)
    {
        if (itemList.Count <= 1)
        {
            SetPagenateIneractable(isInteractable: false);
        }
        else
        {
            SetPagenateIneractable(isInteractable: true);
        }
    }

    private void SetPagenateIneractable(bool isInteractable)
    {
        Button componentInChildren = this.gameObject.transform.Find("CreateWindow/mainarea/pnlLeft").gameObject.GetComponentInChildren<Button>();
        Button componentInChildren2 = this.gameObject.transform.Find("CreateWindow/mainarea/pnlRight").gameObject.GetComponentInChildren<Button>();
        if (componentInChildren != null && componentInChildren2 != null)
        {
            componentInChildren.interactable = isInteractable;
            componentInChildren2.interactable = isInteractable;
        }
    }


    public void OnClickPage(int page)
    {
        List<ItemStruct> itemList = GetItemList();
        _page += page;
        if (itemList.Count <= _page)
        {
            _page = 0;
        }
        else if (_page < 0)
        {
            _page = itemList.Count - 1;
        }
        SetItem(itemList[_page]);
    }

    private void TimeGauge()
    {
        if (_timeLimit == 0f)
        {
            _timeLimit = 0.06f;
        }
        float num = _time / _timeLimit * 160f;
        _gauge.rectTransform.sizeDelta = new Vector2(num, _gauge.rectTransform.sizeDelta.y);
        _gauge.rectTransform.localPosition = new Vector3(-80f + num / 2f, _gauge.rectTransform.localPosition.y, _gauge.rectTransform.localPosition.z);
        if (_time >= _timeLimit)
        {
            CreateComplete();
        }
    }

    private void CreateComplete()
    {
        _create_start = false;
        ResetGauge();
        SetPagenate(_itemList);
        if (!ScoreCtrl.IsScorePositiveInt(_itemList[_page].CreateCost, _itemList[_page].CostType))
        {
            _btnCreate.interactable = false;
        }
        this.gameObject.transform.parent.gameObject.GetComponentInChildren<ItemListCtrl>().SetItemStruct(_itemList[_page]);
    }


    private void ResetGauge()
    {
        _time = 0f;
        _gauge.rectTransform.sizeDelta = new Vector2(0f, _gauge.rectTransform.sizeDelta.y);
        _btnCreate.interactable = true;
        SetPagenateIneractable(isInteractable: false);
    }

    private void Awake()
    {
        _UIItemCreate = this.gameObject;
        InitWindow();
    }

    private void Update()
    {
        if (_create_start && _time <= _timeLimit)
        {
            _time += Time.deltaTime;
            TimeGauge();
        }
    }
}
