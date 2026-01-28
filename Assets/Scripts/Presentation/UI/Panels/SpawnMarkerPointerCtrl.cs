using TMPro;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

/// <summary>
/// スポーンマーカーポインターコントローラー
/// UIControllerBase を継承し、初期化フラグで状態を管理
/// </summary>
public class SpawnMarkerPointerCtrl : UIControllerBase
{
    public static SpawnMarkerPointerCtrl instance;
    private static GameObject _marker;
    private TMP_Text _tmpText;
    private float _time;
    
    // Time Constants
    private const float _TIME_INTERVAL = 0.05f;
    
    // Transform Constants
    private const float _MARKER_Y_OFFSET = 0.08f;
    private const float _MAX_RAYCAST_DISTANCE = 20f;
    
    // GameObject/Component Names
    private const string _CHILD_TMP_POSI = "tmp_posi";
    private const string _CHILD_CANVAS = "Canvas";
    
    // Log Constants
    private const string _LOG_PREFIX = "[SpawnMarker]";

    /// <summary>
    /// インスタンス参照設定とマーカー初期化
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        
        if (instance == null)
        {
            instance = this;
        }
        _marker = gameObject;
        SetMarkerActive(false);
    }

    /// <summary>
    /// Initialize コルーチン
    /// </summary>
    protected override IEnumerator Initialize()
    {
        // 初期化処理なし（Update で処理）
        yield return null;
    }

    private void SetPositionTMP(Vector3 pos)
    {
        _tmpText = GetTmpTextComponent(_tmpText);
        if (_tmpText == null)
        {
            return;
        }
        _tmpText.text = $"{pos.x:F1}, {pos.y:F1}, {pos.z:F1}";
    }

    private TMP_Text GetTmpTextComponent(TMP_Text tmpText)
    {
        if (tmpText == null)
        {
            Transform underCanvas = transform.Find($"{_CHILD_CANVAS}/{_CHILD_TMP_POSI}");
            if (underCanvas != null)
            {
                tmpText = underCanvas.GetComponent<TMP_Text>();
            }
        }
        return tmpText;
    }

    private Quaternion GetMarkerRotation()
    {
        Quaternion rotation = transform.rotation;
        if (rotation == Quaternion.identity)
        {
            Vector3 playerPos = GetPlayerPosition();
            Vector3 markerPos = transform.position;
            Vector3 direction = playerPos - markerPos;
            direction.y = 0f;
            rotation = Quaternion.LookRotation(direction * -1f);
        }
        return rotation;
    }

    private void RayCastPointer()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        int layerMask = ~LayerMask.GetMask(GameEnum.LayerType.AreaIgnoreRaycast.ToString());

        if (Physics.Raycast(ray, out RaycastHit hitInfo, _MAX_RAYCAST_DISTANCE, layerMask))
        {
            if (hitInfo.collider.gameObject.tag != GameEnum.LayerType.Ground.ToString())
            {
                return;
            }

            if (!isNavMeshHit(hitInfo.point))
            {
                return;
            }

            Vector3 markerPos = hitInfo.point;
            markerPos.y += _MARKER_Y_OFFSET;
            transform.position = markerPos;
            transform.rotation = GetMarkerRotation();
            SetPositionTMP(markerPos);
            IsMarkerFarFromPlayer();
        }
        else if (IsMarkerFarFromPlayer())
        {
            SetMarkerActive(false);
        }
    }

    private static bool isNavMeshHit(Vector3 point)
    {
        return NavMesh.SamplePosition(point, out NavMeshHit _, 10f, NavMesh.AllAreas);
    }

    private static Vector3 GetPlayerPosition()
    {
        GameObject player = GameObject.FindWithTag(GameEnum.UnitType.Player.ToString());
        if (player == null)
        {
            return Vector3.zero;
        }
        return player.transform.position;
    }

    internal static Vector3 GetMarkerPosition()
    {
        if (_marker == null)
        {
            return GetPlayerPosition();
        }
        return _marker.transform.position;
    }

    internal static void SetMarkerActive(bool isActive)
    {
        if (_marker == null || _marker.gameObject.activeSelf == isActive)
        {
            return;
        }

        if (isActive)
        {
            _marker.transform.rotation = Quaternion.identity;
        }
        _marker.gameObject.SetActive(isActive);
    }

    private static bool IsMarkerFarFromPlayer()
    {
        return Vector3.Distance(GetMarkerPosition(), GetPlayerPosition()) > _MAX_RAYCAST_DISTANCE;
    }

    internal static bool IsMarkerActive()
    {
        if (!ItemAction.IsItemSelected())
        {
            SetMarkerActive(false);
            return false;
        }
        return _marker.gameObject.activeSelf;
    }

    internal static Quaternion GetMarkerRotateAngle()
    {
        if (!IsMarkerActive())
        {
            return Quaternion.identity;
        }
        return _marker.transform.rotation;
    }

    internal static void RotateMarker(float moveVec)
    {
        if (moveVec < 0f)
        {
            _marker.transform.Rotate(0f, 30f, 0f);
        }
        else
        {
            _marker.transform.Rotate(0f, -30f, 0f);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if (_time > _TIME_INTERVAL)
        {
            if (IsMarkerActive())
            {
                RayCastPointer();
            }
            _time = 0f;
        }
    }
}
