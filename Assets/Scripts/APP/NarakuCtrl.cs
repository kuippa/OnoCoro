using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using CommonsUtility;
using StarterAssets;
using UnityEngine.UIElements;
using UnityEngine.Events;

public class NarakuCtrl : MonoBehaviour
{
    private const float _POPUP_DISTANCE = 10f;   // 落ちたオブジェクトを上に持ち上げる距離
    private const float _MARGIN_DISTANCE = 20f;   // 内側によせる距離
    private const float _NARAKU_DISTANCE = 30f;  // 地面より下の奈落

    private static readonly Vector2 _NARAKU_BASIC_SIZE = new Vector2(1500f, 15f);   // ナラクの基本サイズ
    private Vector3 _dem_center_pos = Vector3.zero;

    // [SerializeField]
    // public int _naraku_idx = 0;

    private GameObject _dem = null;   // DEM(Digital Elevation Model) 航空レーザ測量 地形データ
    // private void OnTriggerStay(Collider other)
    // {
    //         Debug.Log("OnTriggerStay " + this.gameObject.name + " " + other.gameObject.name + " " + _time  + other.transform.position);
    // }
    // private void OnTriggerExit(Collider other)
    // {
    //         Debug.Log("OnTriggerExit " + this.gameObject.name + " " + other.gameObject.name + " " + _time  + other.transform.position);
    // }

    internal void OnTriggerEnter(Collider other)
    {

        // プレイヤーがナラクに入った
        if (other.gameObject.tag == "Player")
        {
            // OnTriggerEnter進入時と脱出時の2回呼ばれるので、脱出時は処理しない
            // PlayerArmature がコライダーを２つもっていたのが原因。
            // 足元のコライダーが何故ついているかわからないので非アクティブに変更

            // Debug.Log("-1 OnTriggerEnter Player " + this.gameObject.name + " " + other.gameObject.name + " " + _time  + other.transform.position);
            Debug.Log("OnTriggerEnter _dem_center_pos " + _dem_center_pos);
            Vector3 setPlayerPos = _dem_center_pos;
            setPlayerPos.y += _POPUP_DISTANCE;
            other.gameObject.GetComponent<InputController>().CharacterMoveToPosition(setPlayerPos);
        }
        else if (other.gameObject.tag == "Naraku")
        {
            // 
        }
        else if (other.gameObject.tag == "Ground")
        {
            // Debug.Log("OnTriggerEnter Naraku:Ground;  " + other.gameObject.name);
            // 敵がナラクに入った
        }
        else
        {
            // コリジョン抜けする条件が不明
            // なぜだ・・・

            Vector3 setPos = GetClosestPointOnBounds(other);
            other.gameObject.transform.position = setPos;

            // 加速度をゼロにする
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                // 動作確認のため、dragを増やしてゆっくり落ちるようにする
                if (GameConfig._APP_GAME_MODE == GlobalConst.GAME_MODE_DEBUG)
                {
                    rb.drag = 2;
                }
            }
        }
    }

    private Vector3 GetClosestPointOnBounds(Collider other)
    {
        Vector3 pos = other.gameObject.transform.position;
        Collider demcol = _dem.GetComponent<Collider>();
        Vector3 closestPoint = demcol.ClosestPointOnBounds(pos);
        // Vector3 closestPoint = col.ClosestPoint(pos);

        // // otherのy中心座標をyのサイズを加えて算出
        // MeshFilter ObjMeshFilter = other.gameObject.GetComponent<MeshFilter>();
        // // float y = other.gameObject.transform.position.y + ObjMeshFilter.mesh.bounds.size.y/2;
        // // TODO: 本当は回転角なども考慮する必要がある
        // float y = ObjMeshFilter.mesh.bounds.size.y/2;

        // ClosestPointOnBounds では直上の座標が取れないので、raycastで取得する
        // RaycastHit hit;
        // Vector3 setPoint = closestPoint;
        // if (Physics.Raycast(closestPoint, Vector3.up, out hit, 100f))
        // {
        //     setPoint = hit.point;
        // }

        // Debug.Log("ClosestPointOnBounds:" + closestPoint + " pos:" + pos);
        // Debug.Log("GetClosestPointOnBounds " + pos + " " + closestPoint + " " + y  + " " + setPoint);
        closestPoint.y += _POPUP_DISTANCE;
        // closestPoint.y += _POPUP_DISTANCE + y;

        // 地面領域よりはみ出ている場合内側に寄せる
        // MeshRenderer meshRenderer = _dem.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = demcol.gameObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            if (closestPoint.x <= _dem_center_pos.x - meshFilter.mesh.bounds.size.x / 2 + _MARGIN_DISTANCE)
            {
                closestPoint.x = _dem_center_pos.x - meshFilter.mesh.bounds.size.x / 2 + _MARGIN_DISTANCE;
            }
            else if (closestPoint.x >= _dem_center_pos.x + meshFilter.mesh.bounds.size.x / 2 - _MARGIN_DISTANCE)
            {
                closestPoint.x = _dem_center_pos.x + meshFilter.mesh.bounds.size.x / 2 - _MARGIN_DISTANCE;
            }
            if (closestPoint.z <= _dem_center_pos.z - meshFilter.mesh.bounds.size.z / 2 + _MARGIN_DISTANCE)
            {
                closestPoint.z = _dem_center_pos.z - meshFilter.mesh.bounds.size.z / 2 + _MARGIN_DISTANCE;
            }
            else if (closestPoint.z >= _dem_center_pos.z + meshFilter.mesh.bounds.size.z / 2 - _MARGIN_DISTANCE)
            {
                closestPoint.z = _dem_center_pos.z + meshFilter.mesh.bounds.size.z / 2 - _MARGIN_DISTANCE;
            }

            // 地面マップの厚みが持ち上げる距離よりも小さい場合、地面マップの厚み分持ち上げる
            if (_POPUP_DISTANCE < meshFilter.mesh.bounds.size.y / 2)
            {
                closestPoint.y += meshFilter.mesh.bounds.size.y / 2;
            }

            // if (closestPoint.y < closestPoint.y + meshFilter.mesh.bounds.size.y / 2 + _POPUP_DISTANCE)
            // {
            //     Debug.Log("closestPoint.y " + closestPoint.y + " " + _dem_center_pos.y + " " + meshFilter.mesh.bounds.size.y);
            //     closestPoint.y = closestPoint.y + meshFilter.mesh.bounds.size.y / 2 + _POPUP_DISTANCE;
            // }
        }
        return closestPoint;
    }

    private Vector3 GetDemPosition(GameObject dem)
    {
        Vector3 setPos = Vector3.zero;
        setPos = dem.transform.localPosition;

        // demがメッシュレンダーを持っているか確認
        MeshRenderer meshRenderer = dem.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Vector3 center = meshRenderer.bounds.center;
            // Debug.Log("dem center" + dem.name + " center:" + center + " setPos:" + setPos);
            setPos.x += center.x;
            setPos.y += center.y;
            setPos.z += center.z;
        }
        return setPos;
    }

    private void InitWindow()
    {
        GameObject[] dem = GameObject.FindGameObjectsWithTag("Ground");
        GameObject naraku = this.gameObject;
        if (dem != null && naraku != null)
        {
            RectTransform naraku_rect = naraku.GetComponent<RectTransform>();
            // Vector3 setPos = Vector3.zero;
            Vector3 setPos = naraku_rect.anchoredPosition;

            // 1つ目のDEMのみ処理する Groundは1つしか手動でつけない前提
            if (dem.Length >= 1)
            {
                _dem_center_pos = GetDemPosition(dem[0]);
                _dem = dem[0];
            }
            naraku_rect.sizeDelta = _NARAKU_BASIC_SIZE;
            // setPos = new Vector3(setPos.x, setPos.y - _NARAKU_DISTANCE - naraku_rect.anchoredPosition.y , setPos.z);
            setPos = new Vector3(setPos.x, setPos.y - _NARAKU_DISTANCE, setPos.z);
            // Debug.Log("InitWindow " + _dem_center_pos + " " + naraku_rect.anchoredPosition);
            naraku_rect.anchoredPosition = setPos;
        }
    }

    // internal GameObject CreateNaraku()
    // {
    //     GameObject[] jigoku = GameObject.FindGameObjectsWithTag("Naraku");
    //     int idx = jigoku.Length;

    //     // ナラクを作成する
    //     GameObject naraku = new GameObject("Naraku" + idx);
    //     naraku.tag = "Naraku";
    //     RectTransform naraku_rect = naraku.AddComponent<RectTransform>();
    //     naraku_rect.sizeDelta = _NARAKU_BASIC_SIZE;
    //     naraku_rect.localScale = new Vector3(300, 1, 300);
    //     Vector3 setPos = new Vector3(_center_pos.x, _center_pos.y - _NARAKU_DISTANCE * idx * idx, _center_pos.z);
    //     naraku_rect.anchoredPosition = setPos;
    //     MeshCollider collider = naraku.AddComponent<MeshCollider>();
    //     collider.convex = true;
    //     collider.isTrigger = true;
    //     naraku.transform.SetParent(this.transform.parent); 
    //     // naraku.parent(GameObject.Find("Canvas").transform);

    //     return naraku;
    // }


    void Awake()
    {
        InitWindow();
    }

    void Update()
    {
        // _time += Time.deltaTime;
        // TODO: 一定時間経過後、ナラク以下まで落ちているオブジェクトがあれば削除する
    }


}
