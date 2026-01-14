using System;
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
    public Button _btnDelete;
    private UnitStruct? _unitStruct = null;


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
        if (_btnClose != null)
        {
            _btnClose.onClick.AddListener(OnClickClose);
        }
        if (_btnOK != null)
        {
            _btnOK.onClick.AddListener(OnClickClose);
        }
        if (_btnDelete != null)
        {
            _btnDelete.onClick.AddListener(OnClickDelete);
        }

        ToggleInfoWindow(false);
    }

    private void CallCircularIndicator(GameObject target)
    {
        GameObject UICircularIndicator = Instantiate(Resources.Load("Prefabs/UI/UICircularIndicator")) as GameObject;
        CircularIndicator indicator = UICircularIndicator.GetComponent<CircularIndicator>();
        Transform indicatorTransform = target.transform.Find("Indicator"); 
        GameObject indicator_canvas = null;
        if (indicatorTransform != null)
        {
            indicator_canvas = target.transform.Find("Indicator").gameObject; 
        }
        else
        {
            GameObject indicator_object = new GameObject("Indicator");
            indicator_object.transform.SetParent(target.transform);
            Canvas canvas = indicator_object.AddComponent<Canvas>();
            indicator_canvas = canvas.gameObject;
            indicator_canvas.transform.position = target.transform.position + new Vector3(0, 1.5f, 0);
            // 裏表ひっくり返す
            indicator_canvas.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        // TODO: 子供クラスを持つオブジェクトか否かはif文分岐ではなくUnitStructのようなモデルで動的に判断するようにする
                
        if (target.tag == GameEnum.TagType.TowerSweeper.ToString())
        {
            target.GetComponent<TowerSweeper>().StartDeleteUnitProcess();
        }
        else if (target.tag == GameEnum.TagType.WaterTurret.ToString())
        {
            target.GetComponent<WaterTurretCtrl>().StartDeleteUnitProcess();
        }
        // TODO: 対象ごとに秒数調整

        indicator.StartIndicator(5f, () => {
                    DeleteUnitProcess(target);
                    }, indicator_canvas);
    }

    private void DeleteUnitProcess(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (target.tag == GameEnum.TagType.TowerSweeper.ToString())
        {
            TowerSweeper towerSweeper = target.GetComponent<TowerSweeper>();
            if (towerSweeper != null)
            {
                towerSweeper.DeleteUnitProcess();
            }
        }
        else if (target.tag == GameEnum.TagType.WaterTurret.ToString())
        {
            WaterTurretCtrl waterTurret = target.GetComponent<WaterTurretCtrl>();
            if (waterTurret != null)
            {
                waterTurret.DeleteUnitProcess();
            }
        }

    }

    private void OnClickDelete()
    {

        // Debug.Log("OnClickDelete" + _unitStruct?.Name + " " + _unitStruct?.UnitID);
        GameObject target = GameObject.Find(_unitStruct?.UnitID);
        if (target == null)
        {
            return;
        }

        if (target.tag == GameEnum.TagType.TowerSweeper.ToString()
            || target.tag == GameEnum.TagType.WaterTurret.ToString())
        {
            CallCircularIndicator(target);
            ToggleInfoWindow(false);
            return;
        }
    }

    internal void GetTargetUnit()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            UnitStruct? unitStruct = GetUnitStruct(hit.collider.gameObject);

            int total_score = ScoreCtrl.GetTotalGarbageScore(hit.collider);
            // Debug.Log("total_score " + total_score + unitStruct?.Name ?? " no name");

            if (SetInfo(unitStruct, total_score))
            {
                ToggleInfoWindow(true);
            }
            // マウスがヒットした地面の座標を取得
            // if (hit.collider.gameObject.layer != LayerMask.NameToLayer(EnemyEnum.LayerType.Ground.ToString()))
            // {
            //     // Debug.Log("hit.collider.gameObject.layer != LayerMask.NameToLayer(EnemyEnum.LayerType.Ground.ToString())");
            //     return;
            // }
            // // マーカーの座標をマウスの座標にする
            // transform.position = hit.point;
            
        }
    }

    internal bool SetInfo(UnitStruct? unitStruct, int get_score = 0)
    {
        if (unitStruct == null || unitStruct?.Name == "")
        {
            return false;
        }

        TextMeshProUGUI txtName = this.transform.Find("InfoWindow/title/tmpUnitName").GetComponent<TextMeshProUGUI>();
        txtName.text = unitStruct?.Name ?? "-";
        TextMeshProUGUI txtID = this.transform.Find("InfoWindow/info/tmpUnitID").GetComponent<TextMeshProUGUI>();
        txtID.text = GlobalConst.UI_UNIT_ID + unitStruct?.UnitID ?? "";
        TextMeshProUGUI txtLv = this.transform.Find("InfoWindow/info/tmpUnitLv").GetComponent<TextMeshProUGUI>();
        // txtLv.text = GlobalConst.UI_LV + unitStruct?.Lv.ToString() ?? "-";
        txtLv.text = "";
        TextMeshProUGUI txtInfo = this.transform.Find("InfoWindow/info/tmpUnitInfo").GetComponent<TextMeshProUGUI>();
        txtInfo.text = GlobalConst.UI_INFO + unitStruct?.Info ?? "";

        if (get_score != 0)
        {
            txtInfo.text += Environment.NewLine + get_score.ToString() + unitStruct?.ScoreType?? "";
        }
        // TextMeshProUGUI txtUpdate = this.transform.Find("InfoWindow/update/tmpUpdateCost").GetComponent<TextMeshProUGUI>();
        // txtUpdate.text = unitStruct.UpdateCost.ToString() + GlobalConst.SHORT_SCORE1_SCALE;
        SetDeleteCost(unitStruct);
        return true;
    }

    private void SetDeleteCost(UnitStruct? unitStruct)
    {
        TextMeshProUGUI tmpDeleteCost = this.transform.Find("InfoWindow/delete/tmpDeleteCost").GetComponent<TextMeshProUGUI>();
        Button btnDelete = this.transform.Find("InfoWindow/delete/btnDelete").GetComponent<Button>();
        int deleteCost = unitStruct?.DeleteCost ?? 0;
        string signedScore = SignedScore(deleteCost);
        if (unitStruct == null || deleteCost == 0 || signedScore == "")
        {
            tmpDeleteCost.text = "";
            btnDelete.gameObject.SetActive(false);
            return;
        }
        tmpDeleteCost.text = signedScore + unitStruct?.ScoreType ?? "type";
        btnDelete.gameObject.SetActive(true);
    }

    // 与えられたスコアが正の整数だった場合、＋文字列をつける
    private static string SignedScore(int score)
    {
        if (score > 0)
        {
            return "+ " + score;
        }
        else if (score < 0)
        {
            return "- " +  Mathf.Abs(score);
        }
        return "";
    }
    

    internal void ToggleInfoWindow(bool isActive)
    {
        if (_infoWindow == null)
        {
            return;
        }
        _infoWindow.SetActive(isActive);
    }

    private UnitStruct? GetUnitStruct(GameObject collider)
    {
        UnitStruct? unitStruct = null;
        string tag = collider.tag;
        // Debug.Log("collider.tag " + tag);
        // TODO : タグを持っているオブジェクトがGetUnitStructを持っている前提になっている。
        // 子どもオブジェクトにtagをもたせたらIndexObjectByTagの命名部分も破綻するし、rootしかもたないGetUnitStructも破綻する

        if (tag == GameEnum.TagType.Garbage.ToString() || tag == GameEnum.TagType.Ash.ToString())
        {
            unitStruct = collider.GetComponent<GarbageCube>().GetUnitStruct();
        }
        else if (tag == GameEnum.TagType.PowerCube.ToString())
        {
            unitStruct = collider.GetComponent<PowerCube>().GetUnitStruct();
        }
        // else if (tag == GameEnum.TagType.TowerSweeper.ToString() || tag == GameEnum.TagType.TowerDock.ToString())
        else if (tag == GameEnum.TagType.TowerSweeper.ToString())
        {
            unitStruct = collider.GetComponent<Sweeper>().GetUnitStruct();
        }
        else if (tag == GameEnum.TagType.FireCube.ToString())
        {
            unitStruct = collider.GetComponent<FireCube>().GetUnitStruct();
        }
        else if (tag == GameEnum.TagType.WaterTurret.ToString())
        {
            unitStruct = collider.GetComponent<WaterTurret>().GetUnitStruct();
        }
        else
        {
            Debug.Log("GetUnitStruct default " + tag);
        }
        _unitStruct = unitStruct;
        return unitStruct;
    }

    void Awake()
    {
        // Debug.Log("InfoWindowCtrl Awake");
        InitWindow();
        #if UNITY_EDITOR
        // debugPreview();
        #endif
    }

}
