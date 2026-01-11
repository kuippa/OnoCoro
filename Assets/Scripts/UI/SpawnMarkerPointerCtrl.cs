using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class SpawnMarkerPointerCtrl : MonoBehaviour
{
    public static SpawnMarkerPointerCtrl instance;
    private static GameObject _marker;
    private float _time;
    private const float _TIME_INTERVAL = 0.05f;
    private const float _MARKER_Y_OFFSET = 0.08f;
    private const float _MAX_RAYCAST_DISTANCE = 20f;

    private void SetPositionTMP(Vector3 pos)
    {
        GameObject tmpObject = transform.Find("tmp_posi")?.gameObject;
        if (tmpObject == null)
        {
            return;
        }

        TextMeshPro textComponent = tmpObject.GetComponent<TextMeshPro>();
        if (textComponent != null)
        {
            textComponent.text = $"{pos.x:F1}, {pos.y:F1}, {pos.z:F1}";
        }
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
        return GameObject.FindWithTag(GameEnum.UnitType.Player.ToString()).transform.position;
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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        _marker = gameObject;
        SetMarkerActive(false);
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
