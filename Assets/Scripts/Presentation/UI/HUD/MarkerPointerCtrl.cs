using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using Unity.VisualScripting;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MarkerPointerCtrl : MonoBehaviour
{
    public static MarkerPointerCtrl instance = null;
    private static GameObject _marker = null;
    private float _time = 0.0f;
    private const float _TIME_INTERVAL = 0.05f;
    private const float _MARKER_Y_OFFSET = 0.15f;

    private void RayCastPointer()
    {
        // マウスがヒットした地面の座標を取得
        Vector2 mousePosision = Mouse.current.position.ReadValue();
        Ray PointRay = Camera.main.ScreenPointToRay(mousePosision);
        RaycastHit hit;
        if (Physics.Raycast(PointRay, out hit, GlobalConst.UI_RAYCAST_MAX_DISTANCE))
        {
            if (hit.collider.gameObject.tag != GameEnum.LayerType.Ground.ToString())
            {
                // Debug.Log("hit.collider.gameObject.tag: " + hit.collider.gameObject.tag);
                return;
            }

            if (!isNavMeshHit(hit.point))
            {
                return;
            }

            // プレイヤー位置から距離が指定マス以上離れていたら抜ける
            Vector3 playerPos = GameObject.FindWithTag(GameEnum.UnitType.Player.ToString()).transform.position;
            if (Vector3.Distance(hit.point, playerPos) > GlobalConst.UI_RAYCAST_MAX_DISTANCE)
            {
                // Debug.Log("hit.point: " + hit.point + " Player.position: " + GameObject.FindWithTag(EnemyEnum.UnitType.Player.ToString()).transform.position);
                return;
            }
            Vector3 markerPos = hit.point;
            markerPos.y += _MARKER_Y_OFFSET;
            // マーカーの座標をマウスの座標にする
            this.transform.position = markerPos;
        }
    }

    private static bool isNavMeshHit(Vector3 point)
    {
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(point, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return true;
        }
        return false;
    }

    internal Vector3 GetMarkerPosition()
    {
        return transform.position;
    }

    internal Vector3 SetMarkerPosition(Vector3 pos)
    {
        return transform.position = pos;
    }

    internal static void SetMarkerActive(bool isActive)
    {
        _marker.gameObject.SetActive(isActive);
    }

    internal static bool IsMarkerActive()
    {
        // Item選択がOffの場合はマーカーを非表示にする
        if (!ItemAction.IsItemSelected())
        {
            SetMarkerActive(false);
            return false;
        }
        return _marker.gameObject.activeSelf;
    }

    void OnDestory()
    {
        Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (instance == this)
        {
            instance = null;
        }
    }


    void Awake()
    {
        Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (instance == null)
        {
            instance = this;
        }
        _marker = this.gameObject;
        SetMarkerActive(false);
    }

    void Update()
    {
        _time += Time.deltaTime;
        if (_time > _TIME_INTERVAL)
        {
            if (IsMarkerActive())
            {
                RayCastPointer();
            }
            _time = 0;
        }
    }
}
