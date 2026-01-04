using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using CommonsUtility;
using UnityEditor;
using Unity.VisualScripting;
using System.Collections;   // IEnumerator を使うために追加

public class ClickCtrl : MonoBehaviour
{
    const float _click_limit_distance = GlobalConst.UI_RAYCAST_MAX_DISTANCE;

    private bool _isProcessingClick = false;
    private static ClickCtrl _instance;
    public static ClickCtrl Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ClickCtrl>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ClickCtrl");
                    _instance = go.AddComponent<ClickCtrl>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public static void OnRightClick(InputValue value)
    {
        if (!Instance._isProcessingClick)
        {
            Instance._isProcessingClick = true;
            Instance.StartCoroutine(Instance.ProcessRightClickNextFrame());
        }
    }

    private IEnumerator ProcessRightClickNextFrame()
    {
        // await Task.Yield(); // 次のフレームまで待機 C#の標準的な非同期プログラミングモデル
        yield return null; // 次のフレームまで待機

        if (!CheckAndCloseNoticeWindow()){
            Instance._isProcessingClick = false;
            yield break; // コルーチンを終了            
        }
        LoupeCtrl.ActLoupe();
        Instance._isProcessingClick = false;
    }

    // マウス押下時ではなく、リリース時に発火する
    public static void OnLeftClick(InputValue value)
    {
        if (!Instance._isProcessingClick)
        {
            Instance._isProcessingClick = true;
            Instance.StartCoroutine(Instance.ProcessLeftClickNextFrame());
        }
    }

    private IEnumerator ProcessLeftClickNextFrame()
    {
        // await Task.Yield(); // 次のフレームまで待機 C#の標準的な非同期プログラミングモデル
        yield return null; // 次のフレームまで待機

        if (!CheckAndCloseNoticeWindow()){
            Instance._isProcessingClick = false;
            yield break; // コルーチンを終了            
        }

        if (LoupeCtrl.IsLoupe())
        {
            LoupeCtrl.ActLoupe();
            Instance._isProcessingClick = false;
            yield break; // コルーチンを終了            
        }
        else if (MarkerPointerCtrl.IsMarkerActive())
        {
            ItemAction.ActItemUse();
            Instance._isProcessingClick = false;
            yield break; // コルーチンを終了            
        }
        ItemAction.GetSelectedItem();
        Instance._isProcessingClick = false;
    }

    private static bool CheckAndCloseNoticeWindow()
    {
        GameObject uiNotice = GameObject.Find("UINotice");
        if (uiNotice != null)
        {
            NoticeCtrl noticeCtrl = uiNotice.GetComponent<NoticeCtrl>();
            bool isActive = noticeCtrl.IsNoticeWindowActive();
            if (isActive)
            {
                noticeCtrl.ToggleNoticeWindow(false);
                return false;
            }
        }
        return true;
    }

    private static void CreateBonFire(Ray PointRay, RaycastHit hit)
    {
        Debug.DrawRay(PointRay.origin, PointRay.direction * 10, Color.red, 5f);
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Bonfire");
        Vector3 setPoint = hit.collider.gameObject.transform.position;
        MeshFilter meshFilter = hit.collider.gameObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            // メッシュの高さを取得してその上部に生成する
            setPoint += Vector3.up * meshFilter.mesh.bounds.size.y;
        }
        else
        {
            setPoint += Vector3.up;
        }
        Quaternion setRotate = hit.collider.gameObject.transform.localRotation;
        GameObject instance = Instantiate(prefab, setPoint, setRotate);
        Transform parent = hit.collider.gameObject.transform.parent;
        if (parent != null)
        {
            instance.transform.SetParent(parent.transform);
        }
    }

    private static void DebugMeshInfo(RaycastHit hit, Vector3 setPoint)
    {
        Debug.Log("localPosition:" + setPoint);
        Debug.Log("position:" + hit.collider.gameObject.transform.position);
        MeshFilter meshFilter = hit.collider.gameObject.GetComponent<MeshFilter>();
        Debug.Log("MeshFilter.bounds.size.y:" + meshFilter.mesh.bounds.size.y);

        var amarture = GameObject.Find("PlayerArmature").transform;
        Debug.Log("amarture :" + amarture.localPosition);
        Debug.Log("MeshFilter size:" + meshFilter.mesh.bounds.size);

        // m_Mesh
        Renderer renderer = hit.collider.gameObject.GetComponent<Renderer>();
        Debug.Log("Renderer size:" + renderer.bounds.size);
        Debug.Log("MeshFilter extents:" + meshFilter.mesh.bounds.extents);
        Debug.Log("MeshFilter center:" + meshFilter.mesh.bounds.center);
        Debug.Log("Renderer center:" + renderer.bounds.center);
        Debug.Log("MeshFilter max:" + meshFilter.mesh.bounds.max);
        Debug.Log("MeshFilter min:" + meshFilter.mesh.bounds.min);
        Debug.Log("Renderer max:" + renderer.bounds.max);
        Debug.Log("Renderer min:" + renderer.bounds.min);

        Vector3 localPosi = hit.collider.gameObject.transform.localPosition;
        Debug.Log("localPosition - center:" + (localPosi - meshFilter.mesh.bounds.center));
        Debug.Log("localPosition + center:" + (localPosi + meshFilter.mesh.bounds.center));
        Debug.Log("localPosition - size:" + (localPosi - meshFilter.mesh.bounds.size));
        Debug.Log("localPosition + size:" + (localPosi + meshFilter.mesh.bounds.size));
        //Contains	設定した point が、バウンディングボックスに含まれているか確認します
        Debug.Log("Contains:" + meshFilter.mesh.bounds.Contains(setPoint));
        // ClosestPoint	バウンディングボックスにもっとも近い点
        Debug.Log("ClosestPoint:" + meshFilter.mesh.bounds.ClosestPoint(setPoint));
        Debug.Log("Submeshes: " + meshFilter.mesh.subMeshCount);
        Debug.Log("isReadable: " + meshFilter.mesh.isReadable);
    }



}
