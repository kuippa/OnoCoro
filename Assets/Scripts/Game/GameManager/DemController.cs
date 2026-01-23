using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class DemController
{
    // Raycast 判定結果
    private enum HitCheckResult
    {
        Success,    // メッシュがあってヒット成功
        MeshHole,   // メッシュ穴が原因でヒット失敗
        NoHit       // その他の理由でヒット失敗
    }

    private const float _POPUP_DISTANCE = 1f;   // 落ちたオブジェクトを上に持ち上げる距離
    private const float _MARGIN_DISTANCE = 3f;   // 内側によせる距離
    private const float _RAYCAST_DISTANCE = 200f;   // Raycast の距離
    private const int _MAX_ITERATION = 20;   // 穴回避の最大試行回数
    private static Vector3 _dem_center_pos = Vector3.zero;
    private static GameObject _dem = null;   // DEM(Digital Elevation Model) 航空レーザ測量 地形データ

    internal static Vector3 GetClosestPointOnBounds(Collider other)
    {
        Vector3 position = other.gameObject.transform.position;
        if (_dem == null)
        {
            _dem = GetDemObject();
            _dem_center_pos = GetDemPosition(_dem);
            if (_dem == null)
            {
                Debug.LogError("GetClosestPointOnBounds DEM not found");
                return Vector3.zero;
            }
        }
        Collider demcol = _dem.GetComponent<Collider>();
        if (demcol == null)
        {
            Debug.LogError("GetClosestPointOnBounds DEM has no Collider");
            return Vector3.zero;
        }

        Vector3 closestPoint = demcol.ClosestPointOnBounds(position);
        float objectHeight = other.bounds.size.y;
        for (int i = 0; i < _MAX_ITERATION; i++)
        {
            HitCheckResult result = TryHitDemPoint(ref closestPoint, other, objectHeight);
            if (result == HitCheckResult.Success)
            {
                break;
            }
            else if (result == HitCheckResult.MeshHole)
            {
                closestPoint = MoveToDemCenter(closestPoint, _MARGIN_DISTANCE);
            }
            else
            {
                closestPoint = AdjustPositionWithinBounds(closestPoint, _dem, objectHeight, i);
            }
        }  
        return closestPoint;
    }


    private static bool HitDemPoint(ref Vector3 closestPoint, Collider other, float objectHeight)
    {
        HitCheckResult result = TryHitDemPoint(ref closestPoint, other, objectHeight);
        return result == HitCheckResult.Success;
    }

    /// <summary>
    /// Raycast で地面をチェックし、判定結果を返す
    /// </summary>
    private static HitCheckResult TryHitDemPoint(ref Vector3 closestPoint, Collider other, float objectHeight)
    {
        RaycastHit hit;
        int layerMask = 1 << LayerMask.NameToLayer(GameEnum.LayerType.Ground.ToString());
        Vector3 rayStart = closestPoint + Vector3.up * _RAYCAST_DISTANCE;
        float rayDistance = _RAYCAST_DISTANCE;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance, layerMask))
        {
            if (hit.collider.gameObject.name != other.gameObject.name)
            {
                if (HasMeshAtHitPoint(hit))
                {
                    closestPoint = hit.point;
                    closestPoint.y += objectHeight + _POPUP_DISTANCE;
                    return HitCheckResult.Success;
                }
                else
                {
                    return HitCheckResult.MeshHole;
                }
            }
        }
        else
        {
            // Raycast が NO HIT だが、closestPoint が DEM bounds 内にあればメッシュ穴の可能性
            if (_dem != null)
            {
                Collider demCol = _dem.GetComponent<Collider>();
                if (demCol != null && demCol.bounds.Contains(closestPoint))
                {
                    // DEM の bounds 内にいるのに Raycast が NO HIT → メッシュ穴として扱う
                    return HitCheckResult.MeshHole;
                }
            }
        }
        return HitCheckResult.NoHit;
    }

    /// <summary>
    /// メッシュ穴を回避するため、位置を DEM 中心方向に移動
    /// </summary>
    private static Vector3 MoveToDemCenter(Vector3 position, float moveDistance)
    {
        if (_dem == null)
        {
            return position;
        }

        Vector3 dirToCenter = (_dem_center_pos - position).normalized;
        Vector3 movedPosition = position;
        movedPosition.x += dirToCenter.x * moveDistance;
        movedPosition.z += dirToCenter.z * moveDistance;

        return movedPosition;
    }

    /// <summary>
    /// Raycast のヒット点に実際のメッシュポリゴンがあるか判定
    /// メッシュマテリアルの穴を回避するため、三角形インデックスを確認
    /// これはAIが作ったがここには入ることはなさそう
    /// </summary>
    private static bool HasMeshAtHitPoint(RaycastHit hit)
    {
        // triangleIndex が -1 は未初期化（ポリゴンなし）
        if (hit.triangleIndex < 0)
        {
            return false;
        }

        MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            return false;
        }

        Mesh mesh = meshFilter.mesh;
        if (mesh == null)
        {
            return false;
        }

        int[] triangles = mesh.triangles;
        if (triangles == null || triangles.Length == 0)
        {
            return false;
        }

        // ヒットした三角形のインデックスを確認（triangleIndex * 3 で 3 頂点分のインデックス）
        int triangleIndex = hit.triangleIndex;
        int vertexIndexStart = triangleIndex * 3;

        // 配列の範囲内か確認
        if (vertexIndexStart + 2 >= triangles.Length)
        {
            return false;
        }

        // 頂点インデックスを取得
        int v0 = triangles[vertexIndexStart];
        int v1 = triangles[vertexIndexStart + 1];
        int v2 = triangles[vertexIndexStart + 2];

        Vector3[] vertices = mesh.vertices;
        if (vertices == null || vertices.Length == 0)
        {
            return false;
        }

        // 頂点インデックスが有効範囲か確認
        if (v0 >= vertices.Length || v1 >= vertices.Length || v2 >= vertices.Length)
        {
            return false;
        }

        // メッシュポリゴンが存在する
        return true;
    }


    private static Vector3 AdjustPositionWithinBounds(Vector3 position, GameObject demObject, float objectHeight, int iterate = 0)
    {
        MeshFilter meshFilter = demObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            return position;
        }
        if (_dem_center_pos == Vector3.zero)
        {
            _dem_center_pos = GetDemPosition(demObject);
        }

        Vector3 adjustedPosition = position;
        Vector3 meshSize = meshFilter.mesh.bounds.size;

        float xMin = _dem_center_pos.x - meshSize.x / 2 + _MARGIN_DISTANCE * iterate;
        float xMax = _dem_center_pos.x + meshSize.x / 2 - _MARGIN_DISTANCE * iterate;
        float zMin = _dem_center_pos.z - meshSize.z / 2 + _MARGIN_DISTANCE * iterate;
        float zMax = _dem_center_pos.z + meshSize.z / 2 - _MARGIN_DISTANCE * iterate;

        adjustedPosition.x = Mathf.Clamp(adjustedPosition.x, xMin, xMax);
        adjustedPosition.z = Mathf.Clamp(adjustedPosition.z, zMin, zMax);

        float groundHeight = _dem_center_pos.y + meshSize.y * 0.5f;
        float minHeight = groundHeight + objectHeight;
        adjustedPosition.y = Mathf.Max(adjustedPosition.y, minHeight);
        adjustedPosition.y += _POPUP_DISTANCE;

        return adjustedPosition;
    }

    internal static float GetDemHeight(GameObject demObject)
    {
        if (demObject == null)
        {
            return 0f;
        }
        MeshFilter meshFilter = demObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            return 0f;
        }
        Vector3 meshSize = meshFilter.mesh.bounds.size;
        float height = meshSize.y;
        return height;
    }

    private static Vector3 GetDemPosition(GameObject dem)
    {
        if (dem == null)
        {
            return Vector3.zero;
        }
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
        GameObject result = null;
        GameObject[] dem = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Ground.ToString());
        if (dem == null || dem.Length < 1)
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            
            // DEMが見つからない(ステージの設定し忘れ)場合は、dem_を探す
            dem = allObjects.Where(
                obj => obj.name.StartsWith("dem_")
                && obj.tag == GameEnum.TagType.Untagged.ToString()
                ).ToArray();
        }

        // 1つ目のDEMのみ処理する Groundは1つしか手動でつけない前提
        if (dem.Length >= 1)
        {
            dem[0].tag = GameEnum.TagType.Ground.ToString();
            dem[0].layer = LayerMask.NameToLayer(GameEnum.LayerType.Ground.ToString());
            result = dem[0];
        }
        else
        {
            Debug.LogError("GetDemObject NO DEM FOUND! dem.Length=" + dem.Length);
        }

        return result;
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

        float scaleY = _dem.transform.localScale.y;
        float groundHeight = _dem_center_pos.y + meshSize.y * 0.5f * scaleY; // 地面の上面の高さ（スケール考慮）
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
        float objectHeight = 0;
        if (HitDemPoint(ref closestPoint, other, objectHeight))
        {
            abovePos.y = closestPoint.y;
        }

        abovePos.y = closestPoint.y + above_distance - _POPUP_DISTANCE;

        return abovePos;
    }


}
