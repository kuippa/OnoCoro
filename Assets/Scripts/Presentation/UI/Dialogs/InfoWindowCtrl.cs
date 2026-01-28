using System;
using System.Collections;
using CommonsUtility;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = CommonsUtility.Debug;
using UnityEngine.UI;

public class InfoWindowCtrl : UIControllerBase
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
        ToggleInfoWindow(isActive: false);
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

        ToggleInfoWindow(isActive: false);
    }

    private void CallCircularIndicator(GameObject target)
    {
        Vector3 delIndicatorPosition = GetDelIndicatorPosition(target);
        MarkerIndicatorCtrl.CreateCircularIndicator(target, 5f, DeleteUnitProcess, delIndicatorPosition);
    }

    private Vector3 GetDelIndicatorPosition(GameObject target)
    {
        Vector3 position = target.transform.position;
        Transform transform = target.transform.Find("dispposi");
        if (transform != null)
        {
            position = transform.position;
        }
        return position;
    }

    private void DeleteUnitProcess(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (target.tag == GameEnum.TagType.TowerSweeper.ToString())
        {
            TowerSweeper component = target.GetComponent<TowerSweeper>();
            if (component != null)
            {
                component.DeleteUnitProcess();
            }
        }
        else if (target.tag == GameEnum.TagType.WaterTurret.ToString())
        {
            WaterTurretCtrl component2 = target.GetComponent<WaterTurretCtrl>();
            if (component2 != null)
            {
                component2.DeleteUnitProcess();
            }
        }
        else
        {
            Debug.Log("DeleteUnitProcess " + target.name + " " + target.tag);
            int intScore = _unitStruct?.DeleteCost ?? 0;
            string score_type = _unitStruct?.ScoreType;
            ScoreCtrl.UpdateAndDisplayScore(intScore, score_type);
            GameObjectTreat.DestroyAll(target);
        }
    }

    private void OnClickDelete()
    {
        // Debug.Log("OnClickDelete" + _unitStruct?.Name + " " + _unitStruct?.UnitID);
        GameObject gameObject = GameObject.Find(_unitStruct?.UnitID);
        
        if (gameObject == null)
        {
            return;
        }
        
        if (IsDeleteAbleUnit(_unitStruct))
        {
            CallCircularIndicator(gameObject);
            ToggleInfoWindow(isActive: false);
        }
    }

    private bool IsDeleteAbleUnit(UnitStruct? unitStruct)
    {
        if (GameObject.Find(unitStruct?.UnitID) == null)
        {
            return false;
        }
        
        if (unitStruct.HasValue || (unitStruct.HasValue && unitStruct.GetValueOrDefault().DeleteCost > 0))
        {
            return true;
        }
        
        return false;
    }

    internal void GetTargetUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        int layerMask = ~LayerMask.GetMask(GameEnum.LayerType.AreaIgnoreRaycast.ToString());
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask))
        {
            UnitStruct? unitStruct = GetUnitStruct(hit.collider.gameObject);
            int total_score = ScoreCtrl.GetTotalGarbageScore(hit.collider);
            // Debug.Log("total_score " + total_score + unitStruct?.Name ?? " no name");

            if (SetInfo(unitStruct, total_score))
            {
                ToggleInfoWindow(isActive: true);
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
        if (!unitStruct.HasValue || unitStruct?.Name == "")
        {
            return false;
        }

        TextMeshProUGUI txtName = this.transform.Find("InfoWindow/title/tmpUnitName").GetComponent<TextMeshProUGUI>();
        txtName.text = unitStruct?.Name ?? "-";
        
        TextMeshProUGUI txtID = this.transform.Find("InfoWindow/info/tmpUnitID").GetComponent<TextMeshProUGUI>();
        txtID.text = "UnitID : " + (unitStruct?.UnitID ?? "");
        
        TextMeshProUGUI txtLv = this.transform.Find("InfoWindow/info/tmpUnitLv").GetComponent<TextMeshProUGUI>();
        // txtLv.text = GlobalConst.UI_LV + unitStruct?.Lv.ToString() ?? "-";
        txtLv.text = "";
        
        TextMeshProUGUI txtInfo = this.transform.Find("InfoWindow/info/tmpUnitInfo").GetComponent<TextMeshProUGUI>();
        txtInfo.text = "INFO : " + (unitStruct?.Info ?? "");

        if (get_score != 0)
        {
            txtInfo.text += Environment.NewLine + get_score.ToString() + (unitStruct?.ScoreType ?? "");
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
        
        string signedScore = SignedScore(unitStruct?.DeleteCost ?? 0);
        
        if (!unitStruct.HasValue || signedScore == "" || !IsDeleteAbleUnit(unitStruct))
        {
            tmpDeleteCost.text = "";
            btnDelete.gameObject.SetActive(value: false);
        }
        else
        {
            tmpDeleteCost.text = signedScore + (unitStruct?.ScoreType ?? "type");
            btnDelete.gameObject.SetActive(value: true);
        }
    }

    // 与えられたスコアが正の整数だった場合、＋文字列をつける
    private static string SignedScore(int score)
    {
        if (score > 0)
        {
            return "+ " + score;
        }
        
        if (score < 0)
        {
            return "- " + Mathf.Abs(score);
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

    private GameObject GetParentObject(GameObject collider)
    {
        Transform parent = collider.transform.parent;
        
        if (parent == null)
        {
            return collider;
        }
        
        if (collider.tag == GameEnum.TagType.Garbage.ToString() 
            || collider.tag == GameEnum.TagType.PowerCube.ToString() 
            || collider.tag == GameEnum.TagType.FireCube.ToString())
        {
            return collider;
        }
        
        return parent.gameObject;
    }

    private UnitStruct? GetUnitStruct(GameObject collider)
    {
        UnitStruct? unitStruct = null;
        collider = GetParentObject(collider);
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
        else if (tag == GameEnum.TagType.EnemyLitters.ToString())
        {
            unitStruct = collider.GetComponent<Litter>().GetUnitStruct();
        }
        else if (tag == GameEnum.TagType.DustBox.ToString())
        {
            unitStruct = collider.GetComponent<DustBox>().GetUnitStruct();
        }
        else if (tag == GameEnum.TagType.StopPlate.ToString())
        {
            unitStruct = collider.GetComponent<StopPlate>().GetUnitStruct();
        }
        else if (tag == GameEnum.TagType.SentryGuard.ToString())
        {
            unitStruct = collider.GetComponent<SentryGuard>().GetUnitStruct();
        }
        else
        {
            Debug.Log("GetUnitStruct default " + tag + " " + collider.name);
        }
        
        _unitStruct = unitStruct;
        return unitStruct;
    }

    protected override void Awake()
    {
        base.Awake();
        InitWindow();
    }

    protected override IEnumerator Initialize()
    {
        yield return null;
    }
}
