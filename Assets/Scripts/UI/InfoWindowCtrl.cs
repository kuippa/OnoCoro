using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonsUtility;

public class InfoWindowCtrl : MonoBehaviour
{
    // public static InfoWindowCtrl instance = null;
    private GameObject _infoWindow = null;

    [SerializeField]
    public Button _btnClose;
    public Button _btnOK;


    internal void OnClickClose()
    {
        ToggleInfoWindow(false);
    }

    private void InitWindow()
    {
        if (_infoWindow == null)
        {
            // Debug.Log("InitWindow _infoWindow is null");
            _infoWindow = this.transform.Find("InfoWindow").gameObject;
        }


        // タグ UIinfo 以下のオブジェクトからbtnCloseを探して、OnClickCloseを登録 
        // GameObject[] objs = GameObject.FindGameObjectsWithTag("UIInfo");
        // foreach (GameObject obj in objs)
        // {
        //     Button btnClose = obj.transform.Find("btnClose").GetComponent<Button>();
        //     btnClose.onClick.AddListener(OnClickClose);
        //     Button btnOK = obj.transform.Find("btnOK").GetComponent<Button>();
        //     btnOK.onClick.AddListener(OnClickClose);
        // }

        if (_btnClose != null)
        {
            Button btnClose = _btnClose;
            // Button btnClose = GameObject.Find("btnClose").GetComponent<Button>();
            btnClose.onClick.AddListener(OnClickClose);
        }

        if (_btnOK != null)
        {
            Button btnOK = _btnOK;
            // Button btnOK = GameObject.Find("btnOK").GetComponent<Button>();
            btnOK.onClick.AddListener(OnClickClose);
        }

        ToggleInfoWindow(false);

    }

    internal void GetTargetUnit()
    {
        // raycastでターゲットを取得
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            // // マウスがヒットした地面の座標を取得
            // if (hit.collider.gameObject.layer != LayerMask.NameToLayer(EnemyEnum.LayerType.Ground.ToString()))
            // {
            //     return;
            // }
            // // マーカーの座標をマウスの座標にする
            // transform.position = hit.point;
        }
        // ターゲットの情報を取得
        // ターゲットの情報を表示

    }

    internal void SetInfo(UnitStruct structInfo)
    {
        TextMeshProUGUI txtName = GameObject.Find("tmpUnitName").GetComponent<TextMeshProUGUI>();
        txtName.text = structInfo.Name;
        TextMeshProUGUI txtID = GameObject.Find("tmpUnitID").GetComponent<TextMeshProUGUI>();
        txtID.text = GlobalConst.UI_UNIT_ID + structInfo.UnitID;
        TextMeshProUGUI txtLv = GameObject.Find("tmpUnitLv").GetComponent<TextMeshProUGUI>();
        txtLv.text = GlobalConst.UI_LV + structInfo.Lv.ToString();
        TextMeshProUGUI txtInfo = GameObject.Find("tmpUnitInfo").GetComponent<TextMeshProUGUI>();
        txtInfo.text = GlobalConst.UI_INFO + structInfo.Info;
        TextMeshProUGUI txtUpdate = GameObject.Find("tmpUpdateCost").GetComponent<TextMeshProUGUI>();
        txtUpdate.text = structInfo.UpdateCost.ToString() + GlobalConst.SHORT_SCORE1_SCALE;
        TextMeshProUGUI tmpDeleteCost = GameObject.Find("tmpDeleteCost").GetComponent<TextMeshProUGUI>();
        tmpDeleteCost.text = structInfo.DeleteCost.ToString() + GlobalConst.SHORT_SCORE2_SCALE;
    }

    internal void ToggleInfoWindow(bool isActive)
    {
        if (_infoWindow == null)
        {
            // Debug.Log("_infoWindow is null");
            return;
            // // TODO: ここで初期化するのはよくない
            // // Awake()が呼ばれていない？
            // InitWindow();
        }

        // Debug.Log("_infoWindow.SetActive(isActive);" + isActive);

        _infoWindow.SetActive(isActive);
    }

    // private void debugPreview()
    // {
    //     ToggleInfoWindow(true);
    //     SetInfo(GetUnitStruct());
    // }


    // internal UnitStruct GetUnitStruct()
    // {
    //     string info = "info texttttt \n power20 area 30 \n speed 10";

    //     UnitStruct unitStruct = new UnitStruct(
    //         "TowerSweeper",
    //         "TowerSweeper 1",
    //         1,
    //         info,
    //         -100,
    //         50
    //     );
    //     return unitStruct;   
    // }


    void Awake()
    {
        // Debug.Log("InfoWindowCtrl Awake");
        InitWindow();
        #if UNITY_EDITOR
        // debugPreview();
        #endif
    }

}
