using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.EventSystems;
using CommonsUtility;
using Unity.VisualScripting;

public class ClickCtrl : MonoBehaviour
{
    // private GameObject _esc_menu_window = null;
    const float _click_limit_distance = GlobalConst.UI_RAYCAST_MAX_DISTANCE;

    private static bool _loupeMode = false;
    private static ItemStruct _item = new ItemStruct();
    private static ItemHolderCtrl _itemHolderCtrl = null;
    private static SpawnCtrl _spawnCtrl = null;

    void Awake()
    {
        _spawnCtrl = GetSpawnCtrl();
    }

    // TODO: EventSystem Ctrlに移動
    private static SpawnCtrl GetSpawnCtrl()
    {
        if (_spawnCtrl == null)
        {
            GameObject eventSystem = GameObject.Find("EventSystem");
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem");
            }
            _spawnCtrl = eventSystem.transform.GetComponent<SpawnCtrl>();
            if (_spawnCtrl == null)
            {
                _spawnCtrl = eventSystem.transform.AddComponent<SpawnCtrl>();
            }
        }
        return _spawnCtrl;
    }


    public static void OnRightClick(InputValue value)
    {
        ActLoupe();
        // Vector2 mousePosision = Mouse.current.position.ReadValue();
        // Ray PointRay = Camera.main.ScreenPointToRay(mousePosision);
        // RaycastHit hit;
        // if (Physics.Raycast(PointRay, out hit, _click_limit_distance))
        // {
        //     CreateBonFire(PointRay, hit);
        // }
    }


    public static void OnLeftClick(InputValue value)
    {
        // EventSystem.current.currentSelectedGameObject が 一個前の選択されているオブジェクトが入ってきているので、
        // マウス押下時ではなく、リリース時に発火するように変更。
        // Assets/Editor/package/InputSystem/StarterAssets.inputactions

        if (!IsNoticeWindow()){
            // UINotice が 表示されているとき
            // Debug.Log("IsNoticeWindow");
            return;
        }
        else if (_loupeMode)
        {
            ActLoupe();
            return;
        }
        else if (MarkerPointerCtrl.IsMarkerActive())
        {
            ActItemUse();
            return;
        }

        // // mouse Down とUPの二回走るので、ボタンの挙動側で TrigarBehavior Press Onlyに変更
        // Vector2 mousePosision = Mouse.current.position.ReadValue();
        // Ray PointRay = Camera.main.ScreenPointToRay(mousePosision);
        // RaycastHit hit;
        // if (Physics.Raycast(PointRay, out hit, _click_limit_distance))
        // {
        //     // CreateBonFire(PointRay, hit);
        // }
        // else
        // {
        //     // CreateCube(mousePosision);
        // }
        GetSelectedItem();
    }

    private static bool IsNoticeWindow()
    {
        GameObject UINotice = GameObject.Find("UINotice");
        if (UINotice != null)
        {
            bool isActive = UINotice.GetComponent<NoticeCtrl>().IsNoticeWindowActive();
            if (isActive)
            {
                // UINotice.GetComponent<NoticeCtrl>().ToggleNoticeWindow(!isActive);
                UINotice.GetComponent<NoticeCtrl>().ToggleNoticeWindow(false);
                // UINotice.SetActive(false);
                return false;
            }
        }
        return true;
    }

    // private void ToggleNoticeWindow(bool isOn, NoticeCtrl noticeCtrl)
    // {
    //     if (_esc_menu_window != null)
    //     {
    //         escMenuCtrl.ToggleEscMenuWindow(isOn);
    //     }
    // }


    private static void GetSelectedItem()
    {
        MarkerPointerCtrl.SetMarkerActive(false);
        GameObject item_holder = GameObject.Find("ItemList");
        if (item_holder == null)
        {
            return;
        }
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
        {
            return;
        }

        if (selected.name.StartsWith("Item_"))
        {
            _itemHolderCtrl = selected.GetComponent<ItemHolderCtrl>();
            ItemStruct item = _itemHolderCtrl.GetItem();
            if (item.Name == "Loupe")
            {
                _loupeMode = true;
                return;
            }
            _item = item;
            MarkerPointerCtrl.SetMarkerActive(true);
            return;
        }

    }


    private static void ActLoupe()
    {
        _loupeMode = false;
        GameObject plateauInfo  = GameObject.Find("Plateau");
        if (plateauInfo == null)
        {
            // Debug.Log("PlateauInfo is null");
            return;
        }
        bool boolplateau = plateauInfo.GetComponent<PlateauInfo>().GetPlateauInfo();
        if (!boolplateau)
        {
            GameObject uiInfo = GameObject.Find("UIInfo");
            if (uiInfo != null)
            {
                uiInfo.GetComponent<InfoWindowCtrl>().ToggleInfoWindow(true);
            }
            return;
        }
    }

    private static void ActItemUse()
    {
        // Debug.Log("BuildMode: " + _item.Name + " stack:" + _item.Stack);
        Vector2 mousePosision2 = Mouse.current.position.ReadValue();
        _spawnCtrl = GetSpawnCtrl();
        if (!_spawnCtrl.CallUnitByName(_item.Name))
        {
            return;
        }
        _item.Stack = -1;
        _itemHolderCtrl.AddItemToHolder(_item);
        MarkerPointerCtrl.SetMarkerActive(false);
    }

    private static void CreateCube(Vector2 mousePosision)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosision);	 // マウスをクリックしたときのメインカメラの位置
        // GameObject prefab = Resources.Load<GameObject>("Prefabs/Bonfire");
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Cube");
        Vector3 setPoint = worldPoint + Camera.main.transform.forward * 10;
        GameObject instance = Instantiate(prefab, setPoint, Quaternion.identity);
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
        // GameObject instance = Instantiate(prefab, setPoint, Quaternion.identity);
        // Debug.Log("Did Hit:" + hit.collider.gameObject.name + setPoint);
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

        // Not allowed to access vertices on mesh 'Combined Mesh
        // Debug.Log("uv: " + meshFilter.mesh.uv);
        // Debug.Log("uv2: " + meshFilter.mesh.uv2);
        // Debug.Log("uv3: " + meshFilter.mesh.uv3);

        // Vector3[] vertices = meshFilter.mesh.vertices;
        // for (var i = 0; i < vertices.Length; i++)
        // {
        // 	// vertices[i] += Vector3.up * Time.deltaTime;
        // 	Debug.Log("vertices[" + i + "]:" + vertices[i]);
        // }					

        // Vector3[] vertices = meshFilter.mesh.vertices;
        // Vector2[] uvs = new Vector2[vertices.Length];
        // for (int i = 0; i < uvs.Length; i++)
        // {
        // 	uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        // 	Debug.Log(uvs[i]);
        // }

    }



}
