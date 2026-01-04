using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonsUtility;
using UnityEngine.AI;
using System.ComponentModel;
using UnityEngine.Assertions.Must;
using Unity.VisualScripting;
// using StarterAssets;

public class ItemCreateCtrl : MonoBehaviour
{
    private bool _create_start = false;
    private float _time = 0.0f;
    private float _timeLimit = 5.0f;
    private int _page = 0;
    List<ItemStruct> _itemList = new List<ItemStruct>();

    [SerializeField]
    // インスペクター上からイベント割当のオブジェクトを設定済み
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
        bool ret = false;
        ItemStruct itemStruct = _itemList[_page];
        int createCost =  (int)itemStruct.CreateCost;
        if (ScoreCtrl.IsScorePositiveInt(itemStruct.CreateCost, itemStruct.CostType))
        {
            ScoreCtrl.CalcScore((int)createCost, itemStruct.CostType);
            return true;
        }
        return ret;
    }

    internal void OnClickClose()
    {
        this.gameObject.SetActive(false);
    }

    private void InitWindow()
    {
        if (_btnClose != null)
        {
            _btnClose.onClick.AddListener(OnClickClose);
        }

        if (_btnOK != null)
        {
            _btnOK.onClick.AddListener(OnClickClose);
        }

        if (_btnCreate != null)
        {
            _btnCreate.onClick.AddListener(OnClickCreate);
        }
        if (_gauge != null)
        {
            Image timeGaugeFill = _gauge;
            ResetGauge();
        }
        if (_btnLeft != null && _btnRight != null)
        {
            _btnLeft.onClick.AddListener(() => OnClickPage(-1));
            _btnRight.onClick.AddListener(() => OnClickPage(+1));
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
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        // TODO: StafingYamlCtrl より後に読み込まれることが担保されてないはず。あとで調整する/今はオブジェクト読み込み順
        List<ItemStruct> itemList = new List<ItemStruct>();
        List<string> itemNames = new List<string>();
        itemNames = StagingYamlCtrl.GetItemList();

        // itemNamesと一致するクラスを取得する
        foreach (string itemName in itemNames)
        {
            System.Type type = System.Type.GetType(itemName);
            if (type != null)
            {
                // Debug.Log("type:" + type);
                itemList = SetItemLists(type, itemList);
            }
        }

        // # TODO: ステージ yaml からの指定読み込みに変更する
        // itemList.Add(this.gameObject.AddComponent<GarbageCube>()._item_struct);
        // itemList.Add(this.gameObject.AddComponent<GarbageCubeBox>()._item_struct);
        // itemList.Add(this.gameObject.AddComponent<Sweeper>()._item_struct);
        // itemList.Add(this.gameObject.AddComponent<Litter>()._item_struct);

        // ルーペは、アイテム作成ウィンドウに表示しない
        // itemList.Add(this.gameObject.AddComponent<Loupe>()._item_struct);
        _itemList = itemList;
    }

    // private List<ItemStruct> SetItemLists(System.Type type, List<ItemStruct> itemList)
    // {
    //     // TODO: 可変クラスに対応したい// ここアホくさい
    //     switch (type.Name)
    //     {
    //         case "GarbageCube":
    //             itemList.Add((this.gameObject.AddComponent(type) as GarbageCube)._item_struct);
    //             break;
    //         case "GarbageCubeBox":
    //             itemList.Add((this.gameObject.AddComponent(type) as GarbageCubeBox)._item_struct);
    //             break;
    //         case "PowerCube":
    //             itemList.Add((this.gameObject.AddComponent(type) as PowerCube)._item_struct);
    //             break;
    //         case "Litter":
    //             itemList.Add((this.gameObject.AddComponent(type) as Litter)._item_struct);
    //             break;
    //         case "Sweeper":
    //             itemList.Add((this.gameObject.AddComponent(type) as Sweeper)._item_struct);
    //             break;
    //         default:
    //             Debug.Log("default SetItemLists " + type.Name);
    //             break;
    //     }

    //     return itemList;
    // }

    private List<ItemStruct> SetItemLists(System.Type type, List<ItemStruct> itemList)
    {
        var component = this.gameObject.AddComponent(type);
        if (component is IItemStructProvider provider)
        {
            itemList.Add(provider.ItemStruct);
        }
        else
        {
            Debug.LogWarning($"Type {type.Name} does not implement IItemStructProvider.");
        }
        return itemList;
    }



    private void SetItems()
    {
        List<ItemStruct> itemList = GetItemList();
        if (itemList.Count == 0)
        {
            SwitchActive(false);
            return;
        }

        SetItem(itemList[_page]);
        SetPagenate(itemList);
    }

    public void SwitchActive(bool isActive)
    {
        if (isActive && GetItemList().Count == 0)
        {
            this.gameObject.SetActive(false);
            // InitWindow();
            return;
        }
        this.gameObject.SetActive(isActive);
    }

    private void SetItem(ItemStruct itemStruct)
    {
        GameObject pnlMain = this.transform.Find("CreateWindow/mainarea/pnlMain").gameObject;
        GameObject txtName = pnlMain.transform.Find("txtName").gameObject;
        txtName.GetComponent<TextMeshProUGUI>().text = itemStruct.Name;
        GameObject txtCost = pnlMain.transform.Find("txtCost").gameObject;
        txtCost.GetComponent<TextMeshProUGUI>().text = 
            itemStruct.CreateCost.ToString() + itemStruct.CostType
            + "/"
            + itemStruct.CostTime.ToString() + "s";
        _timeLimit = itemStruct.CostTime;
        GameObject imgIcon = pnlMain.transform.Find("imgIcon").gameObject;
        imgIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>(itemStruct.ItemImagePath);
        GameObject txtDescription = imgIcon.transform.Find("txtAlt").gameObject;
        txtDescription.GetComponent<Text>().text = itemStruct.Info;

        if (ScoreCtrl.IsScorePositiveInt(itemStruct.CreateCost, itemStruct.CostType))
        {
            _btnCreate.interactable = true;
        }
        else
        {
            _btnCreate.interactable = false;
        }
    }

    private void SetPagenate(List<ItemStruct> itemList)
    {
        if (itemList.Count <= 1)
        {
            // Debug.Log("itemList.Count <= 1");
            SetPagenateIneractable(false);
        }
        else
        {
            SetPagenateIneractable(true);
        }
    }

    private void SetPagenateIneractable(bool isInteractable)
    {
        GameObject pnlLeft = this.transform.Find("CreateWindow/mainarea/pnlLeft").gameObject;
        Button btnLeft = pnlLeft.GetComponentInChildren<Button>();
        GameObject pnlRight = this.transform.Find("CreateWindow/mainarea/pnlRight").gameObject;
        Button btnRight = pnlRight.GetComponentInChildren<Button>();
        if (btnLeft != null && btnRight != null)
        {
            btnLeft.interactable = isInteractable;
            btnRight.interactable = isInteractable;
        }
    }


    public void OnClickPage(int page)
    {
        // Debug.Log("OnClickPage" + page + " _page:" + _page);
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
        // Debug.Log("OnClickPage :" + _page);
        SetItem(itemList[_page]);
    }

    private void TimeGauge()
    {
        if (_timeLimit == 0)
        {
            _timeLimit = GlobalConst.APP_MINIMAM_FRAME;
        }

        // ゲージの長さを変更
        float gaugeLength = _time / _timeLimit * 160;
        // Debug.Log("gaugeLength:" + gaugeLength + " _time:" + _time + " _timeLimit:" + _timeLimit);
        _gauge.rectTransform.sizeDelta = new Vector2(gaugeLength, _gauge.rectTransform.sizeDelta.y);
        // _gauge の 表示位置を変更
        _gauge.rectTransform.localPosition = new Vector3(-80 + gaugeLength / 2, _gauge.rectTransform.localPosition.y, _gauge.rectTransform.localPosition.z);

        // ゲージが満タンになったら
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
        // TODO:後でボタンコントロールの位置をまとめる
        if (!ScoreCtrl.IsScorePositiveInt(_itemList[_page].CreateCost, _itemList[_page].CostType))
        {
            _btnCreate.interactable = false;
        }

        // 親オブジェクトから子供UIItemを取得する
        GameObject parent_obj = this.gameObject.transform.parent.gameObject;
        // UIItem を取得する
        // GameObject ui_item = parent_obj.transform.Find("UIItem").gameObject;
        // // ItemBox を取得する
        // GameObject item_box = ui_item.transform.Find("ItemBox").gameObject;
        // // BG を取得する
        // GameObject bg = item_box.transform.Find("BG").gameObject;
        // // ItemList を取得する
        // GameObject item_list = bg.transform.Find("ItemList").gameObject;
        // // ItemListCtrl を取得する
        // ItemListCtrl itemListCtrl = item_list.GetComponent<ItemListCtrl>();

        ItemListCtrl itemListCtrl = parent_obj.GetComponentInChildren<ItemListCtrl>();
        // Debug.Log("_itemList[_page]" + _page + " " + _itemList[_page].Name + " " + _itemList[_page].CreateCost + " " + _itemList[_page].CostType + " " + _itemList[_page].CostTime + " " + _itemList[_page].Stack);
        itemListCtrl.SetItemStruct(_itemList[_page]);
    }


    private void ResetGauge()
    {
        _time = 0.0f;
        _gauge.rectTransform.sizeDelta = new Vector2(0, _gauge.rectTransform.sizeDelta.y);
        _btnCreate.interactable = true;
        SetPagenateIneractable(false);
    }


    void Awake()
    {
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        InitWindow();
        #if UNITY_EDITOR
        // debugPreview();
        #endif
    }


    // Update is called once per frame
    void Update()
    {
        if (_create_start && _time <= _timeLimit)
        {
            _time += Time.deltaTime;
            TimeGauge();
        }
    }
}
