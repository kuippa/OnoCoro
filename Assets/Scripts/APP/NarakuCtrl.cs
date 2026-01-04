using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using CommonsUtility;
using StarterAssets;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Runtime;
using NUnit.Framework;

public class NarakuCtrl : MonoBehaviour
{
    private const float _POPUP_DISTANCE = 1f;   // 落ちたオブジェクトを上に持ち上げる距離
    private const float _POPUP_PLAYER_DISTANCE = 30f;   // 落ちたプレイヤーを上に持ち上げる距離
    private const float _MARGIN_DISTANCE = 3f;   // 内側によせる距離
    private const float _NARAKU_DISTANCE = 50f;  // 奈落ごとの距離
    private static readonly Vector2 _NARAKU_BASIC_SIZE = new Vector2(1500f, 15f);   // ナラクの基本サイズ
    private Vector3 _dem_center_pos = Vector3.zero;

    private GameObject _dem = null;   // DEM(Digital Elevation Model) 航空レーザ測量 地形データ

    internal void OnTriggerEnter(Collider other)
    {
        // プレイヤーがナラクに入った
        if (other.gameObject.tag == GameEnum.TagType.Player.ToString())
        {
            // OnTriggerEnter進入時と脱出時の2回呼ばれるので、脱出時は処理しない
            // PlayerArmature がコライダーを２つもっていたのが原因。
            // 足元のコライダーが何故ついているかわからないので非アクティブに変更

            // Vector3 setPlayerPos = _dem_center_pos;
            Vector3 setPlayerPos = GetClosestPointOnBounds(other);
            // SetRigidbodyVelocity(other);

            Debug.Log("OnTriggerEnter Player " + other.gameObject.name + " " + setPlayerPos);
            setPlayerPos.y += _POPUP_PLAYER_DISTANCE;
            other.gameObject.GetComponent<InputController>().CharacterMoveToPosition(setPlayerPos);
            // other.gameObject.transform.position = setPlayerPos;
        }
        else if (other.gameObject.tag == GameEnum.TagType.Naraku.ToString())
        {
            // 
        }
        else if (other.gameObject.tag == GameEnum.TagType.Ground.ToString())
        {
            // Debug.Log("OnTriggerEnter Naraku:Ground;  " + other.gameObject.name);
            // 敵がナラクに入った
        }
        else
        {
            Debug.Log("OnTriggerEnter other.gameObject.tag " + other.gameObject.tag );
            // TODO:
            // コリジョン抜けする条件が不明
            // なぜだ・・・

            // 加速度をゼロにする
            SetRigidbodyVelocity(other);
            Vector3 setPos = GetClosestPointOnBounds(other);
            other.gameObject.transform.position = setPos;
        }
    }

    private void SetRigidbodyVelocity(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // rb.angularVelocity *= -1;

            // 動作確認のため、dragを増やしてゆっくり落ちるようにする
            if (GameConfig._APP_GAME_MODE == GlobalConst.GAME_MODE_DEBUG)
            {
                rb.linearDamping = 2;
            }
        }
    }

    private Vector3 GetClosestPointOnBounds(Collider other)
    {
        Vector3 pos = other.gameObject.transform.position;
        if (_dem == null)
        {
            // Debug.Log("GetClosestPointOnBounds " + "dem is null");
            _dem = GetDemObject();
            _dem_center_pos = GetDemPosition(_dem);
        }
        Collider demcol = _dem.GetComponent<Collider>();
        if (demcol == null)
        {
            // Debug.Log("GetClosestPointOnBounds " + "demcol is null");
        }

        Vector3 closestPoint = demcol.ClosestPointOnBounds(pos);
        float objectHeight = other.bounds.size.y;
        for (int i = 0; i < 20; i++)
        {
            closestPoint = AdjustPositionWithinBounds(closestPoint, _dem, objectHeight, i);        // 地面領域よりはみ出ている場合内側に寄せる
            // Debug.Log("GetClosestPointOnBounds " + "closestPoint:" + closestPoint + " pos:" + pos + " _dem_center_pos:" + _dem_center_pos);
            if (HitDemPoint(ref closestPoint, other, objectHeight))
            {
                // 地面に当たった場合は処理を抜ける
                break;
            }
            else
            {
                // Debug.Log("GetClosestPointOnBounds " + "hit not found" + " i:" + i);
            }
            // HITしなかった場合、内側に寄せてさらにhitするか試す
        }
        return closestPoint;
    }

    private bool HitDemPoint(ref Vector3 closestPoint, Collider other, float objectHeight)
    {
        // ClosestPointOnBounds では直上の座標が取れないので、raycastで上書き取得する
        RaycastHit hit;
        int layerMask = 1 << LayerMask.NameToLayer(GameEnum.LayerType.Ground.ToString());
        if (Physics.Raycast(closestPoint + Vector3.up * 200f, Vector3.down, out hit, Mathf.Infinity, layerMask))
        // 上向きにRayを飛ばしても地面との衝突判定がとれない。半透過？
        // if (Physics.Raycast(closestPoint + Vector3.up * 10f, Vector3.up, out hit, Mathf.Infinity, layerMask))
        {
            // Debug.Log("raycast hit:" + hit.point + " closestPoint:" + closestPoint + hit.collider.gameObject.name  + " : " + other.gameObject.name);
            if (hit.collider.gameObject.name != other.gameObject.name)
            {
                // Debug.Log("raycast override by hit:" + hit.point);
                closestPoint = hit.point;
                closestPoint.y += objectHeight + _POPUP_DISTANCE;
                return true;
            }
            else
            {
                // Debug.Log("raycast hit:" + hit.point + " closestPoint:" + closestPoint + hit.collider.gameObject.name  + " : " + other.gameObject.name);
            }
        }
        return false;
    }


    private Vector3 AdjustPositionWithinBounds(Vector3 position, GameObject demObject, float objectHeight, int iterate = 0)
    {
        MeshFilter meshFilter = demObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            // Debug.LogWarning("No MeshFilter component found on " + demObject.name);
            return position;
        }
        if (_dem_center_pos == Vector3.zero)
        {
            // Debug.Log("AdjustPositionWithinBounds " + "_dem_center_pos is zero");
            _dem_center_pos = GetDemPosition(demObject);
        }

        Vector3 adjustedPosition = position;
        Vector3 meshSize = meshFilter.mesh.bounds.size;

        // X軸の調整
        adjustedPosition.x = Mathf.Clamp(
            adjustedPosition.x,
            _dem_center_pos.x - meshSize.x / 2 + _MARGIN_DISTANCE * iterate,
            _dem_center_pos.x + meshSize.x / 2 - _MARGIN_DISTANCE * iterate
        );

        // Z軸の調整
        adjustedPosition.z = Mathf.Clamp(
            adjustedPosition.z,
            _dem_center_pos.z - meshSize.z / 2 + _MARGIN_DISTANCE * iterate,
            _dem_center_pos.z + meshSize.z / 2 - _MARGIN_DISTANCE * iterate
        );

        // Y軸の調整（地面マップの厚み考慮）
        float groundHeight = _dem_center_pos.y + meshSize.y * 0.5f; // 地面の上面の高さ
        float minHeight = groundHeight + objectHeight;
        adjustedPosition.y = Mathf.Max(adjustedPosition.y, minHeight);
        adjustedPosition.y += _POPUP_DISTANCE;
        Debug.Log("adjustedPosition:" + adjustedPosition + " groundHeight:" + groundHeight + " minHeight:" + minHeight);

        return adjustedPosition;
    }

    private float GetDemHeight(GameObject demObject)
    {
        MeshFilter meshFilter = demObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            return 0;
        }
        Vector3 meshSize = meshFilter.mesh.bounds.size;
        float height = meshSize.y;
        return height;
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
        // TODO: dem取得部分 関数化
        GameObject naraku = this.gameObject;
        if (naraku == null)
        {
            Debug.Log("InitWindow " + "naraku is null");
        }
        _dem = GetDemObject();
        ChangeMaterialUVToPlanar(_dem); // マテリアルのUVをPlanarに変更
        RectTransform naraku_rect = naraku.GetComponent<RectTransform>();
        Vector3 setPos = naraku_rect.anchoredPosition;
        naraku_rect.sizeDelta = _NARAKU_BASIC_SIZE;
        Transform jigoku = transform.parent;
        jigoku.position = _dem_center_pos;
        int naraku_idx = GetCurrentNarakuIndex();
        float dem_height = GetDemHeight(_dem);
        // Debug.Log("InitWindow " + "dem_height:" + dem_height + " _NARAKU_DISTANCE:" + _NARAKU_DISTANCE + " naraku_idx:" + naraku_idx + "_dem_center_pos.y:" + _dem_center_pos.y);
        setPos = new Vector3(_dem_center_pos.x, _dem_center_pos.y - dem_height - (_NARAKU_DISTANCE * naraku_idx), _dem_center_pos.z);
        naraku_rect.anchoredPosition = setPos;
    }

    private GameObject GetDemObject()
    {
        GameObject ret_dem;
        GameObject[] dem = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Ground.ToString());
        if (dem == null)
        {
            Debug.Log("GetDemObject " + "dem is null");
        }
        // 1つ目のDEMのみ処理する Groundは1つしか手動でつけない前提
        if (dem.Length < 1)
        {
            // Debug.Log("InitWindow " + "dem is null" + dem.Length);
            // DEMが見つからない(ステージの設定し忘れ)場合は、dem_を探す
            dem = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Untagged.ToString()).Where(
                obj => obj.name.StartsWith("dem_")
                ).ToArray();
            dem[0].tag = GameEnum.TagType.Ground.ToString();
        }
        dem[0].layer = LayerMask.NameToLayer(GameEnum.LayerType.Ground.ToString());
        ret_dem = dem[0];

        return ret_dem;
    }


    private void ChangeMaterialUVToPlanar(GameObject targetObject)
    {
        Renderer renderer = targetObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("No Renderer component found on " + targetObject.name);
            return;
        }
        Material material = renderer.material;
        if (material == null)
        {
            Debug.LogWarning("No material found on " + targetObject.name);
            return;
        }            

        // material.shader.name == "HDRP/Lit"
        // Debug.Log("Material shader name " + material.shader.name);
        // UV Mapping modeを変更
        // [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Planar, 4, Triplanar, 5)] _UVBase("UV Set for base", Float) = 0
        // _UVDetailsMappingMask

        var newUVMapping = 4f;  // 4はPlanar mappingを表す
        float currentUVMapping = material.GetFloat("_UVBase");
        // Debug.Log("Material UV mapping changed to Planar for " + targetObject.name + " currentUVMapping:" + currentUVMapping);
        if (currentUVMapping == newUVMapping)
        {
            return;
        }
        material.SetFloat("_UVBase", 4f);
        // currentUVMapping = material.GetFloat("_UVBase");
        // Debug.Log("Material UV mapping changed to Planar for " + targetObject.name + " currentUVMapping:" + currentUVMapping);
    }

    private void SetDemUV(GameObject dem)
    {
        if (_dem == null)
        {
            // Debug.Log("GetClosestPointOnBounds " + "dem is null");
            // _dem = GameObject.Find("Ground");
            // _dem_center_pos = GetDemPosition(_dem);
        }


        // DEMのUVを設定する
        // DEMのUVは、地形の高さによって変化する
        // DEMの高さを取得する
        // DEMの高さによって、UVのY座標を変更する
        // DEMの高さによって、UVのX座標を変更する
    }

    private int GetCurrentNarakuIndex()
    {
        // 全てのNarakuタグを持つオブジェクトを取得
        GameObject[] narakuObjects = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Naraku.ToString());
        // 現在のオブジェクトのインデックスを探す
        for (int i = 0; i < narakuObjects.Length; i++)
        {
            if (narakuObjects[i] == this.gameObject)
            {
                this.gameObject.name = "Naraku_" + i;
                return i + 1;
            }
        }
        return 1;
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
        // if (_dem == null || _dem_center_pos == Vector3.zero)
        // {
        //     Debug.Log("Awake " + "dem is null");
        //     _dem = GameObject.FindGameObjectsWithTag("Ground")[0];
        //     _dem_center_pos = GetDemPosition(_dem);
        // }
    }

    void Update()
    {
        // _time += Time.deltaTime;
        // TODO: 一定時間経過後、ナラク以下まで落ちているオブジェクトがあれば削除する
    }


}
