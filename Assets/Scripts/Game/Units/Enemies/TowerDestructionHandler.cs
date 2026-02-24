using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

internal class TowerDestructionHandler
{
    // Numeric Constants
    private const float _DESTRUCTION_RADIUS = 18.0f;  // タイムアウト時に破壊するタワーの対象範囲（メートル）

    internal TowerDestructionHandler()
    {
    }

    /// <summary>
    /// 指定位置の近隣にある最大maxTowerCount個のタワーを破壊
    /// タイムアウト時の相打ちペナルティー
    /// </summary>
    internal void DestroyNearbyTowers(Vector3 centerPosition, int maxTowerCount = 1, string agentName = "")
    {
        int destroyedCount = 0;
        
        foreach (string towerTag in GetTowerTags())
        {
            if (destroyedCount >= maxTowerCount)
            {
                break;
            }
            
            GameObject[] towersWithTag = GameObject.FindGameObjectsWithTag(towerTag);

            foreach (GameObject tower in towersWithTag)
            {
                if (destroyedCount >= maxTowerCount)
                {
                    break;
                }
                
                if (DestroyTowerIfInRange(tower, centerPosition, agentName))
                {
                    destroyedCount++;
                }
            }
        }
    }

    /// <summary>
    /// 範囲内のタワーを破壊（EventLog出力付き）
    /// 破壊成功時は true を返す
    /// </summary>
    private bool DestroyTowerIfInRange(GameObject tower, Vector3 centerPosition, string agentName)
    {
        if (tower == null)
        {
            return false;
        }

        float distanceToColliderEdge = GetDistanceToColliderEdge(tower, centerPosition);
        Debug.Log($"{agentName}: Checking tower {tower.name} at distance {distanceToColliderEdge:F2}m");

        if (distanceToColliderEdge <= _DESTRUCTION_RADIUS)
        {
            Debug.Log($"{agentName}: Destroying tower {tower.name} (distance: {distanceToColliderEdge:F2}m)");
            
            // EventLog に出力
            EventLogCtrl.Instance.ShowEventLog($"{agentName}がタワー{tower.name}を壊した");
            
            GameObjectTreat.DestroyAll(tower);
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// タワーのコライダー端までの距離を計算
    /// 距離 = コライダー中心までの距離 - コライダー半径
    /// </summary>
    private float GetDistanceToColliderEdge(GameObject tower, Vector3 centerPosition)
    {
        Collider collider = tower.GetComponent<Collider>();
        if (collider == null)
        {
            return float.MaxValue;
        }

        Vector3 colliderCenter = collider.bounds.center;
        float colliderRadius = GetColliderRadius(tower, collider);
        float distanceToColliderCenter = Vector3.Distance(centerPosition, colliderCenter);

        return distanceToColliderCenter - colliderRadius;
    }

    /// <summary>
    /// タワーのコライダー半径を計算（球体または AABB）
    /// </summary>
    private float GetColliderRadius(GameObject tower, Collider collider)
    {
        SphereCollider sphereCollider = tower.GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            return sphereCollider.radius * tower.transform.lossyScale.x;
        }

        // 球体でない場合は AABB の外接球半径を使用
        return collider.bounds.extents.magnitude;
    }

    /// <summary>
    /// 破壊対象となるタワーの種類一覧を取得
    /// </summary>
    private string[] GetTowerTags()
    {
        return new string[]
        {
            GameEnum.TagType.DustBox.ToString(),
            GameEnum.TagType.FireCube.ToString(),
            GameEnum.TagType.WaterTurret.ToString(),
            GameEnum.TagType.SentryGuard.ToString(),
            GameEnum.TagType.PowerCube.ToString(),
            GameEnum.TagType.StopPlate.ToString(),
            GameEnum.TagType.TowerSweeper.ToString()
        };
    }

    /// <summary>
    /// デバッグ用：破壊範囲情報を取得
    /// </summary>
    internal string GetDebugInfo()
    {
        return $"Destruction Radius: {_DESTRUCTION_RADIUS}m";
    }
}
