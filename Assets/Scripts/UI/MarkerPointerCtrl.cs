using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class MarkerPointerCtrl : MonoBehaviour
{
    public static MarkerPointerCtrl instance = null;
    private static GameObject _marker = null;

    private int i = 0;

    private void RayCastPointer()
    {
        // マウスがヒットした地面の座標を取得
        Vector2 mousePosision = Mouse.current.position.ReadValue();
        Ray PointRay = Camera.main.ScreenPointToRay(mousePosision);
        RaycastHit hit;
        if (Physics.Raycast(PointRay, out hit, GlobalConst.UI_RAYCAST_MAX_DISTANCE))
        {
            // if (hit.collider.gameObject.layer != LayerMask.NameToLayer(EnemyEnum.LayerType.Ground.ToString()))
            // {
            //     Debug.Log("hit.collider.gameObject.layer: " + hit.collider.gameObject.layer);
            //     return;
            // }
            // if (hit.collider.gameObject.name.Contains("dem_"))
            // gameObjectのタグがground以外だったら抜ける
            if (hit.collider.gameObject.tag != GameEnum.LayerType.Ground.ToString())
            {
                // Debug.Log("hit.collider.gameObject.tag: " + hit.collider.gameObject.tag);
                return;
            }

            // プレイヤー位置から距離が指定マス以上離れていたら抜ける
            Vector3 playerPos = GameObject.FindWithTag(GameEnum.UnitType.Player.ToString()).transform.position;
            // Debug.Log("hit.point: " + hit.point + "  playerPos: " + playerPos);
            if (Vector3.Distance(hit.point, playerPos) > GlobalConst.UI_RAYCAST_MAX_DISTANCE)
            {
                // Debug.Log("hit.point: " + hit.point + " Player.position: " + GameObject.FindWithTag(EnemyEnum.UnitType.Player.ToString()).transform.position);
                return;
            }
            Vector3 markerPos = hit.point;
            markerPos.y += 0.25f;
            // マーカーの座標をマウスの座標にする
            this.transform.position = markerPos;
        }

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
        return _marker.gameObject.activeSelf;
    }


    void Awake()
    {
        // Debug.Log("MarkerPointerCtrl Awake");
        if (instance == null)
        {
            instance = this;
        }
        _marker = this.gameObject;
        SetMarkerActive(false);
    }

    void Update()
    {
        if (IsMarkerActive() && i > 5)
        {
            i = 0;
            RayCastPointer();
        }
        i++;

    }
}
