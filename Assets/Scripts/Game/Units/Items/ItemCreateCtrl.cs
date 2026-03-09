// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// ItemCreateCtrl
using System;
using System.Collections.Generic;
using CommonsUtility;
using TMPro;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.UI;

/// <summary>
/// アイテム作成ウィンドウの制御クラス
///
/// 責任範囲:
///   - ボタンリスナー登録（Awake で一度だけ）
///   - TabMenuCtrl からの表示/非表示切り替え（SwitchActive）
///   - 表示時に最新 itemlist で画面再構築（RebuildItemList + RefreshView）
///   - ページ切り替え・アイテム作成処理・ゲージ制御
///
/// 表示制御設計:
///   this.gameObject（UIItemCreate）は常にアクティブ → GameObject.Find で常に参照可能
///   表示/非表示は子の CreateWindow のみ制御する
/// </summary>
public class ItemCreateCtrl : MonoBehaviour
{
    // === 状態管理 ===
    private bool _create_start;
    private float _time;
    private float _timeLimit = 5f;
    private int _page;
    private List<ItemStruct> _itemList = new List<ItemStruct>();

    // === UI オブジェクト参照 ===
    // 子の CreateWindow を指す。this.gameObject（UIItemCreate）は常にアクティブを維持する
    private GameObject _UIItemCreate;
    private GameObject _itemInfo;

    [SerializeField]
    public Button _btnClose;
    public Button _btnOK;
    public Button _btnCreate;
    public Image _gauge;
    public Button _btnLeft;
    public Button _btnRight;

    // =============================================
    // ライフサイクル
    // =============================================

    private void Awake()
    {
        Transform createWindowTransform = this.gameObject.transform.Find("CreateWindow");
        if (createWindowTransform == null)
        {
            Debug.LogWarning($"[ItemCreateCtrl] CreateWindow not found on {this.gameObject.name}");
            _UIItemCreate = this.gameObject;
        }
        else
        {
            _UIItemCreate = createWindowTransform.gameObject;
        }

        // ItemInfo オブジェクトを初期化
        Transform itemInfoTransform = this.gameObject.transform.Find("ItemInfo");
        if (itemInfoTransform == null)
        {
            Debug.LogWarning($"[ItemCreateCtrl] ItemInfo not found on {this.gameObject.name}");
            _itemInfo = null;
        }
        else
        {
            _itemInfo = itemInfoTransform.gameObject;
        }

        RegisterButtonListeners();

        // 初期状態は非表示（SwitchActive(true) 呼び出し時に表示・画面構築する）
        _UIItemCreate.SetActive(false);
        if (_itemInfo != null)
        {
            _itemInfo.SetActive(false);
        }
    }

    private void Update()
    {
        if (_create_start && _time <= _timeLimit)
        {
            _time += Time.deltaTime;
            UpdateGaugeDisplay();
        }
    }

    // =============================================
    // 公開インターフェース（TabMenuCtrl から呼ばれる）
    // =============================================

    /// <summary>
    /// ウィンドウの表示/非表示を切り替える
    /// 表示時（isActive=true）: 最新 itemlist でリスト再構築 → SetActive(true) → 画面更新
    /// 非表示時（isActive=false）: SetActive(false) のみ
    /// </summary>
    public void SwitchActive(bool isActive)
    {
        if (_UIItemCreate == null)
        {
            Debug.LogWarning("[ItemCreateCtrl] _UIItemCreate is null");
            return;
        }

        if (!isActive)
        {
            _UIItemCreate.SetActive(false);
            if (_itemInfo != null)
            {
                _itemInfo.SetActive(false);
            }
            return;
        }

        _page = 0;
        RebuildItemList();

        if (_itemList.Count == 0)
        {
            Debug.LogWarning("[ItemCreateCtrl] itemList is empty, not showing window");
            return;
        }

        // SetActive(true) の後に RefreshView を呼ぶ (非アクティブ状態での UI 更新を回避)
        // → アイコン等の Sprite 系コンポーネントが確実に再描画される
        _UIItemCreate.SetActive(true);
        if (_itemInfo != null)
        {
            _itemInfo.SetActive(true);
        }
        RefreshView();
    }

    // =============================================
    // ボタンイベント
    // =============================================

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
            RefreshPagination();
        }
    }

    internal void OnClickClose()
    {
        SwitchActive(isActive: false);
    }

    public void OnClickPage(int page)
    {
        _page += page;
        if (_itemList.Count <= _page)
        {
            _page = 0;
        }
        else if (_page < 0)
        {
            _page = _itemList.Count - 1;
        }
        SetItemDisplay(_itemList[_page]);
    }

    // =============================================
    // 初期化
    // =============================================

    /// <summary>
    /// ボタンリスナーを登録する（Awake で一度だけ実行）
    /// データ取得・画面更新は行わない
    /// </summary>
    private void RegisterButtonListeners()
    {
        _btnClose = this.gameObject.transform.Find("CreateWindow/titlebar/btnClose").GetComponent<Button>();
        if (_btnClose != null)
        {
            _btnClose.onClick.AddListener(OnClickClose);
        }
        else
        {
            Debug.Log("[ItemCreateCtrl] btnClose is null");
        }

        _btnOK = this.gameObject.transform.Find("CreateWindow/footer/btnOK").GetComponent<Button>();
        if (_btnOK != null)
        {
            _btnOK.onClick.AddListener(OnClickClose);
        }
        else
        {
            Debug.Log("[ItemCreateCtrl] btnOK is null");
        }

        _btnCreate = this.gameObject.transform.Find("CreateWindow/mainarea/pnlMain/btnCreate").GetComponent<Button>();
        if (_btnCreate != null)
        {
            _btnCreate.onClick.AddListener(OnClickCreate);
        }
        else
        {
            Debug.Log("[ItemCreateCtrl] btnCreate is null");
        }

        _gauge = this.gameObject.transform.Find("CreateWindow/mainarea/pnlMain/timeGauge/timeGaugeFill").GetComponent<Image>();
        if (_gauge == null)
        {
            Debug.Log("[ItemCreateCtrl] timeGaugeFill is null");
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
            Debug.Log("[ItemCreateCtrl] btnLeft or btnRight is null");
        }

        // ゲージを初期化（インディケーターバーが最初から正しい状態で表示される）
        ResetGauge();
    }

    /// <summary>
    /// StageYamlRepository から最新の itemlist を再構築する
    /// SwitchActive(true) 時に呼ばれる
    /// </summary>
    private void RebuildItemList()
    {
        _itemList = new List<ItemStruct>();
        foreach (string itemName in StageYamlRepository.GetItemList())
        {
            Type type = Type.GetType(itemName);
            if (type == null)
            {
                continue;
            }
            if (this.gameObject.AddComponent(type) is IItemStructProvider provider)
            {
                _itemList.Add(provider.ItemStruct);
            }
            else
            {
                Debug.LogWarning($"[ItemCreateCtrl] {type.Name} does not implement IItemStructProvider.");
            }
        }
    }

    // =============================================
    // 画面更新
    // =============================================

    /// <summary>
    /// 現在の _itemList と _page で画面全体を更新する
    /// 必ず SetActive(true) の後に呼ぶこと（アイコン等の再描画を保証）
    /// </summary>
    private void RefreshView()
    {
        SetItemDisplay(_itemList[_page]);
        RefreshPagination();
    }

    private void SetItemDisplay(ItemStruct itemStruct)
    {
        GameObject pnlMain = this.gameObject.transform.Find("CreateWindow/mainarea/pnlMain").gameObject;
        pnlMain.transform.Find("txtName").GetComponent<TextMeshProUGUI>().text = itemStruct.Name;
        pnlMain.transform.Find("txtCost").GetComponent<TextMeshProUGUI>().text = itemStruct.CreateCost + itemStruct.CostType + "/" + itemStruct.CostTime + "s";
        _timeLimit = itemStruct.CostTime;

        GameObject imgIcon = pnlMain.transform.Find("imgIcon").gameObject;
        SpriteResourceLoader.SetSpriteToImage(imgIcon.GetComponent<Image>(), itemStruct.ItemImagePath);
        imgIcon.transform.Find("txtAlt").GetComponent<Text>().text = itemStruct.Info;

        // ItemInfo の Content にアイテム説明を表示
        if (_itemInfo != null)
        {
            TextMeshProUGUI contentText = _itemInfo.transform.Find("Scroll View/Viewport/Content/tmpItemInfo").GetComponent<TextMeshProUGUI>();
            if (contentText != null)
            {
                contentText.text = itemStruct.Info;
            }
            else
            {
                Debug.LogWarning("[ItemCreateCtrl] ItemInfo/tmpItemInfo component not found");
            }
        }
        else
        {
            Debug.LogWarning("[ItemCreateCtrl] _itemInfo is null");
        }

        RefreshCreateButton(itemStruct);
    }

    /// <summary>
    /// _itemList の件数に基づいてページネーションボタンの有効/無効を更新する
    /// </summary>
    private void RefreshPagination()
    {
        SetPaginationInteractable(_itemList.Count > 1);
    }

    private void SetPaginationInteractable(bool isInteractable)
    {
        Button btnLeft = this.gameObject.transform.Find("CreateWindow/mainarea/pnlLeft").GetComponentInChildren<Button>();
        Button btnRight = this.gameObject.transform.Find("CreateWindow/mainarea/pnlRight").GetComponentInChildren<Button>();
        if (btnLeft != null && btnRight != null)
        {
            btnLeft.interactable = isInteractable;
            btnRight.interactable = isInteractable;
        }
    }

    private void RefreshCreateButton(ItemStruct itemStruct)
    {
        if (_btnCreate == null)
        {
            return;
        }
        _btnCreate.interactable = ScoreCtrl.IsScorePositiveInt(itemStruct.CreateCost * -1, itemStruct.CostType);
    }

    // =============================================
    // アイテム作成ロジック
    // =============================================

    private bool PayItemCreateCost()
    {
        ItemStruct itemStruct = _itemList[_page];
        int costDelta = itemStruct.CreateCost * -1;
        if (!ScoreCtrl.IsScorePositiveInt(costDelta, itemStruct.CostType))
        {
            return false;
        }
        ScoreCtrl.UpdateAndDisplayScore(costDelta, itemStruct.CostType);
        return true;
    }

    private void CreateComplete()
    {
        _create_start = false;
        ResetGauge();
        RefreshPagination();
        RefreshCreateButton(_itemList[_page]);
        this.gameObject.transform.parent.gameObject.GetComponentInChildren<ItemListCtrl>().SetItemStruct(_itemList[_page]);
    }

    // =============================================
    // ゲージ制御
    // =============================================

    private void UpdateGaugeDisplay()
    {
        if (_timeLimit == 0f)
        {
            _timeLimit = 0.06f;
        }
        float width = _time / _timeLimit * 160f;
        _gauge.rectTransform.sizeDelta = new Vector2(width, _gauge.rectTransform.sizeDelta.y);
        _gauge.rectTransform.localPosition = new Vector3(-80f + width / 2f, _gauge.rectTransform.localPosition.y, _gauge.rectTransform.localPosition.z);
        if (_time >= _timeLimit)
        {
            CreateComplete();
        }
    }

    private void ResetGauge()
    {
        _time = 0f;
        _gauge.rectTransform.sizeDelta = new Vector2(0f, _gauge.rectTransform.sizeDelta.y);
        _btnCreate.interactable = true;
        // 作成中はページ切り替えを無効化する
        SetPaginationInteractable(false);
    }
}
