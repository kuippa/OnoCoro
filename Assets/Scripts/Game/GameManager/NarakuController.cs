// using System.Collections;
// using System.Collections.Generic;
using System.Linq;
// using Unity.VisualScripting;
using UnityEngine;
using Debug = CommonsUtility.Debug;
// using UnityEngine.UI;
// using UnityEngine.Rendering;
using CommonsUtility;
using StarterAssets;
// using UnityEngine.UIElements;
// using UnityEngine.Events;
// using System.Runtime;
// using NUnit.Framework;
// using Unity.Android.Types;

public class NarakuController : MonoBehaviour
{
    private const float _POPUP_PLAYER_DISTANCE = 30f;   // 落ちたプレイヤーを上に持ち上げる距離
    private const float _NARAKU_DISTANCE = 50f;  // 奈落ごとの距離
    private static readonly Vector2 _NARAKU_BASIC_SIZE = new Vector2(1500f, 15f);   // ナラクの基本サイズ
    private Vector3 _dem_center_pos = Vector3.zero;
    private GameObject _dem = null;   // DEM(Digital Elevation Model) 航空レーザ測量 地形データ
    private WaterSurfaceCtrl _waterSurface = null;
    private GameObject _eventSystem = null;

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
            SetRigidbodyVelocity(other);

            // CharacterController を使用している場合、内部速度もリセット
            InputController inputCtrl = other.gameObject.GetComponent<InputController>();
            if (inputCtrl != null)
            {
                inputCtrl.ResetVelocity();
                // inputCtrl.SetVerticalVelocity(-5.0f);
                // inputCtrl.SetMoveSpeed(0.1f);
            }
            Debug.Log("OnTriggerEnter Player " + other.gameObject.name + " " + setPlayerPos);
            setPlayerPos.y += _POPUP_PLAYER_DISTANCE;
            other.gameObject.GetComponent<InputController>().CharacterMoveToPosition(setPlayerPos);
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
        else if (other.gameObject.tag == GameEnum.TagType.FireCube.ToString()
            || other.gameObject.tag == GameEnum.TagType.Ash.ToString())
        {
            GameObjectTreat.DestroyAll(other.gameObject);            
        }
        else if (other.gameObject.tag == GameEnum.TagType.RainDrop.ToString())
        {
            // Debug.Log("OnTriggerEnter Naraku:RainDrop;  " + other.gameObject.name);
            // 雨/水がナラクに入った
            // Debug.Log("OnTriggerEnter RainDrop " + other.gameObject.name);
            _eventSystem = GameObjectTreat.GetEventSystem(_eventSystem);
            _waterSurface = GameObjectTreat.GetOrAddComponent<WaterSurfaceCtrl>(_eventSystem);
            _waterSurface.RainDropIntoNaraku(other.gameObject);
            GameObjectTreat.DestroyAll(other.gameObject);
        }
        else if (other.gameObject.tag == GameEnum.TagType.Water.ToString())
        {
            // Debug.Log("OnTriggerEnter Naraku:Water;  " + other.gameObject.name);
            // 水がナラクに入った
            GameObjectTreat.DestroyAll(other.gameObject);
        }
        else
        {
            // その他のオブジェクトがナラクに入った ex. GarbageCube
            // Debug.Log("OnTriggerEnter other.gameObject.tag " + other.gameObject.tag );
            // 衝突判定を連続にしてもかなりの確率でコリジョン抜けをする

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

            if (GameConfig._APP_GAME_MODE == GlobalConst.GAME_MODE_DEBUG)
            {
                rb.linearDamping = 2;
            }
        }
    }

    private Vector3 GetClosestPointOnBounds(Collider other)
    {
        Vector3 closestPoint = DemController.GetClosestPointOnBounds(other);
        return closestPoint;
    }

    private void InitWindow()
    {
        GameObject naraku = this.gameObject;
        if (naraku == null)
        {
            // Debug.Log("InitWindow " + "naraku is null");
        }
        _dem = DemController.GetDemObject();
        ChangeMaterialUVToPlanar(_dem); // マテリアルのUVをPlanarに変更
        RectTransform naraku_rect = naraku.GetComponent<RectTransform>();
        Vector3 setPos = naraku_rect.anchoredPosition;
        naraku_rect.sizeDelta = _NARAKU_BASIC_SIZE;
        Transform jigoku = transform.parent;
        jigoku.position = _dem_center_pos;
        int naraku_idx = GetCurrentNarakuIndex();
        float dem_height = DemController.GetDemHeight(_dem);

        // Debug.Log("InitWindow " + "dem_height:" + dem_height + " _NARAKU_DISTANCE:" + _NARAKU_DISTANCE + " naraku_idx:" + naraku_idx + "_dem_center_pos.y:" + _dem_center_pos.y);
        setPos = new Vector3(_dem_center_pos.x, _dem_center_pos.y - dem_height - (_NARAKU_DISTANCE * naraku_idx), _dem_center_pos.z);
        naraku_rect.anchoredPosition = setPos;
    }

    private void ChangeMaterialUVToPlanar(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.Log("ChangeMaterialUVToPlanar " + "targetObject is null");
            return;
        }

        Renderer renderer = targetObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.Log("No Renderer component found on " + targetObject.name);
            return;
        }
        Material material = renderer.material;
        if (material == null)
        {
            Debug.Log("No material found on " + targetObject.name);
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
