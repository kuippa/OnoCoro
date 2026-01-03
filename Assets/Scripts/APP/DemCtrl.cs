using System.Linq;
using UnityEngine;

public static class DemCtrl
{
    private const float _POPUP_DISTANCE = 1f;   // 落ちたオブジェクトを上に持ち上げる距離
    private const float _MARGIN_DISTANCE = 3f;   // 内側によせる距離
    private static Vector3 _dem_center_pos = Vector3.zero;
    private static GameObject _dem = null;   // DEM(Digital Elevation Model) 航空レーザ測量 地形データ

    internal static Vector3 GetClosestPointOnBounds(Collider other)
    {
        Vector3 pos = other.gameObject.transform.position;
        if (_dem == null)
        {
            _dem = GetDemObject();
            _dem_center_pos = GetDemPosition(_dem);
        }
        Collider demcol = _dem.GetComponent<Collider>();
        if (demcol == null)
        {
            // Debug.Log("GetClosestPointOnBounds " + "demcol is null");
            return Vector3.zero;
        }

        Vector3 closestPoint = demcol.ClosestPointOnBounds(pos);
        float objectHeight = other.bounds.size.y;
        for (int i = 0; i < 20; i++)
        {
            // HITしなかった場合、内側に寄せてさらにhitするか試す
            closestPoint = AdjustPositionWithinBounds(closestPoint, _dem, objectHeight, i);        // 地面領域よりはみ出ている場合内側に寄せる
            if (HitDemPoint(ref closestPoint, other, objectHeight))
            {
                // 地面に当たった場合は処理を抜ける
                break;
            }
        }
        return closestPoint;
    }


    private static bool HitDemPoint(ref Vector3 closestPoint, Collider other, float objectHeight)
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


    private static Vector3 AdjustPositionWithinBounds(Vector3 position, GameObject demObject, float objectHeight, int iterate = 0)
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
        // Debug.Log("adjustedPosition:" + adjustedPosition + " groundHeight:" + groundHeight + " minHeight:" + minHeight);

        return adjustedPosition;
    }

    internal static float GetDemHeight(GameObject demObject)
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

    private static Vector3 GetDemPosition(GameObject dem)
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


    internal static GameObject GetDemObject()
    {
        GameObject ret_dem = null;
        GameObject[] dem = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Ground.ToString());
        if (dem == null || dem.Length < 1)
        {

            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            Debug.Log("GetDemObject " + "dem is null " + allObjects.Length);

            // Debug.Log("InitWindow " + "dem is null" + dem.Length);
            // DEMが見つからない(ステージの設定し忘れ)場合は、dem_を探す
            dem = allObjects.Where(
                obj => obj.name.StartsWith("dem_")
                && obj.tag == GameEnum.TagType.Untagged.ToString()
                ).ToArray();
            // Untagged は、デフォルトのタグなので FindGameObjectsWithTag では取得できない

            // dem = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Untagged.ToString()).Where(
            //     obj => obj.name.StartsWith("dem_")
            //     ).ToArray();
            
            // if (dem == null || dem.Length < 1)
            // {
            //     Debug.LogError("No objects with name starting with 'dem_' found.");
            // }
            // else
            // {
            //     Debug.Log("Found " + dem.Length + " objects with name starting with 'dem_'.");
            // }                
        }

        // 1つ目のDEMのみ処理する Groundは1つしか手動でつけない前提
        if (dem.Length >= 1)
        {
            dem[0].tag = GameEnum.TagType.Ground.ToString();
            dem[0].layer = LayerMask.NameToLayer(GameEnum.LayerType.Ground.ToString());
            ret_dem = dem[0];
            // Debug.Log("GetDemObject " + "dem is null " + dem.Length);
            // return null;
        }

        return ret_dem;
    }


    // RainDrop 用
    internal static Vector3 GetDemRndAbovePosition(float drop_distance)
    {
        Vector3 abovePos = Vector3.zero;
        if (_dem == null)
        {
            _dem = GetDemObject();
            _dem_center_pos = GetDemPosition(_dem);
        }
        MeshFilter meshFilter = _dem.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            // Debug.Log("GetDemAbarbPosition " + "meshFilter is null");
            return abovePos;
        }
        Vector3 meshSize = meshFilter.mesh.bounds.size;
        abovePos.x = Random.Range(
            _dem_center_pos.x - meshSize.x / 2 + _MARGIN_DISTANCE
            , _dem_center_pos.x + meshSize.x / 2 - _MARGIN_DISTANCE
            );

        abovePos.z = Random.Range(
            _dem_center_pos.z - meshSize.z / 2 + _MARGIN_DISTANCE
            , _dem_center_pos.z + meshSize.z / 2 - _MARGIN_DISTANCE
            );

        float groundHeight = _dem_center_pos.y + meshSize.y * 0.5f; // 地面の上面の高さ
        float minHeight = groundHeight + drop_distance;
        abovePos.y = minHeight;

        return abovePos;
    }

    // Puddle 用
    internal static Vector3 GetDemAbovePosition(GameObject target, float above_distance)
    {
        Vector3 abovePos = Vector3.zero;
        if (_dem == null)
        {
            _dem = GetDemObject();
            _dem_center_pos = GetDemPosition(_dem);
        }
        MeshFilter meshFilter = _dem.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            return abovePos;
        }
        Vector3 meshSize = meshFilter.mesh.bounds.size;
        
        abovePos.x = target.transform.position.x;
        abovePos.z = target.transform.position.z;

        Collider demcol = _dem.GetComponent<Collider>();
        Vector3 closestPoint = demcol.ClosestPointOnBounds(abovePos);

        Collider other = target.GetComponent<Collider>();
        // float objectHeight = other.bounds.size.y;
        float objectHeight = 0;
        if (HitDemPoint(ref closestPoint, other, objectHeight))
        {
            abovePos.y = closestPoint.y;
        }

        abovePos.y = closestPoint.y + above_distance - _POPUP_DISTANCE;

        // TODO:
        // なぜかy が 200 以上に持ち上がることがあるので、地面の高さより下にならないようにする



        return abovePos;
    }


}
