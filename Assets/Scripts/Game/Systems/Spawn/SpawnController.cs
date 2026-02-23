using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.UI;
using TMPro;
using CommonsUtility;
using System;

/// <summary>
/// ユニットスポーン管理クラス
/// 各種ユニット（ゴミキューブ、炎キューブ、タワー等）の生成を制御します
/// </summary>
public class SpawnController : MonoBehaviour 
{
    public static SpawnController _instance = null;

    // PathMaker 回避距離（pathmakers のポイントからこの距離以内にスポーンしない）
    private const float _PATH_MARKER_EXCLUSION_RADIUS = 1.0f;

    public static SpawnController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SpawnController>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnController");
                    _instance = go.AddComponent<SpawnController>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void OnDestroy()
    {
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// ユニット名でユニットを生成します
    /// </summary>
    internal bool CallUnitByName(string unitName, Vector3 spawnPoint = default(Vector3))
    {
        bool ret = false;
        if (unitName == null || unitName == "")
        {
            return ret;
        }
        else if (unitName == GameEnum.ModelsType.GarbageCube.ToString())
        {
            ret = SpawnGarbageCube(0, spawnPoint);
        }
        else if (unitName == GameEnum.ModelsType.GarbageCubeBox.ToString())
        {
            ret = SpawnGarbageCubeBox(spawnPoint);
        }
        else if (unitName == GameEnum.ModelsType.GarbageCubeBig.ToString())
        {
            ret = SpawnGarbageCubeBig(spawnPoint);
        }
        else if (unitName == GameEnum.ModelsType.Sweeper.ToString())
        {
            ret = SpawnTowerSweeper(0.25f, spawnPoint);
        }
        else if (unitName == GameEnum.ModelsType.PowerCube.ToString())
        {
            ret = SpawnPowerCube(0, spawnPoint);
        }
        else if (unitName == GameEnum.ModelsType.StopPlate.ToString())
        {
            ret = SpawnStopPlate();
        }
        else if (unitName == GameEnum.ModelsType.FireCube.ToString())
        {
            ret = SpawnFireCube(spawnPoint);
        }
        else if (unitName == GameEnum.ModelsType.WaterTurret.ToString())
        {
            ret = SpawnWaterTurret(spawnPoint);
        }
        else if (unitName == GameEnum.ModelsType.DustBox.ToString())
        {
            ret = SpawnDustBox(spawnPoint);
        }
        else if (unitName == GameEnum.ModelsType.SentryGuard.ToString())
        {
            ret = SpawnSentryGuard(spawnPoint);
        }
        else
        {
            Debug.Log("default CallUnitByName: " + unitName);
        }
        return ret;
    }

    /// <summary>
    /// 敵ユニット名で敵を生成します
    /// </summary>
    internal bool CallEnemyUnitByName(string unitName, string[] marker_names)
    {
        bool result = false;
        if (unitName == null || unitName == "")
        {
            return result;
        }
        if (unitName == GameEnum.ModelsType.Litter.ToString())
        {
            result = SpawnLitter(marker_names);
        }
        else
        {
            Debug.Log("default CallEnemyUnitByName: " + unitName);
        }
        return result;
    }

    private bool SpawnSentryGuard(Vector3 spawnPoint = default(Vector3))
    {
        float dropbuffer = 0.05f;
        spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
        if (spawnPoint == Vector3.zero)
        {
            Debug.LogWarning("[SpawnController] SentryGuard: pathmaker 除外エリア内でスポーン不可");
            return false;
        }
        Quaternion spawnRotateAngle = GetSpawnRotateAngle();
        GameObject gameObject = Instantiate(PrefabManager.TowerSentryGuardPrefab, spawnPoint, spawnRotateAngle);
        int sentryGuardUID = PrefabManager.TowerSentryGuardUID;
        gameObject.name = GameEnum.ModelsType.SentryGuard.ToString() + sentryGuardUID;
        SentryGuard orAddComponent = GameObjectTreat.GetOrAddComponent<SentryGuard>(gameObject);
        orAddComponent._item_struct.ItemID = gameObject.name;
        orAddComponent._unit_struct.UnitID = gameObject.name;
        return true;
    }

    private bool SpawnDustBox(Vector3 spawnPoint = default(Vector3))
    {
        float dropbuffer = 0.05f;
        Quaternion spawnRotateAngle = GetSpawnRotateAngle();
        spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
        if (spawnPoint == Vector3.zero)
        {
            Debug.LogWarning("[SpawnController] DustBox: pathmaker 除外エリア内でスポーン不可");
            return false;
        }
        GameObject gameObject = Instantiate(PrefabManager.DustBoxPrefab, spawnPoint, spawnRotateAngle);
        int dustBoxUID = PrefabManager.DustBoxUID;
        gameObject.name = GameEnum.ModelsType.DustBox.ToString() + dustBoxUID;
        DustBox orAddComponent = GameObjectTreat.GetOrAddComponent<DustBox>(gameObject);
        orAddComponent._item_struct.ItemID = gameObject.name;
        orAddComponent._unit_struct.UnitID = gameObject.name;

        // NavMesh Carving を有効化（経路計算から除外）
        NavMeshManager.EnableCarvingForObstacle(gameObject);

        return true;
    }

    private bool SpawnLitter(string[] marker_names)
    {
        // 最初のパスマーカー位置を取得
        Vector3 spawnPosition = GetFirstMarkerPosition(marker_names);
        
        GameObject gameObject = Instantiate(PrefabManager.EnemyLitterPrefab, spawnPosition, Quaternion.identity);
        EnemyLitter component = gameObject.GetComponent<EnemyLitter>();
        int idx = EnemyLitter._idx;
        gameObject.name = GameEnum.ModelsType.Litter.ToString() + idx;
        Litter orAddComponent = GameObjectTreat.GetOrAddComponent<Litter>(gameObject);
        orAddComponent._unit_struct.UnitID = gameObject.name;
        orAddComponent._item_struct.ItemID = gameObject.name;
        component.InitUnitSpawn(marker_names);
        return true;
    }

    private Vector3 GetFirstMarkerPosition(string[] marker_names)
    {
        if (marker_names == null || marker_names.Length == 0)
        {
            Debug.LogWarning("SpawnLitter: marker_names is null or empty. Using default position (0, 0, 0)");
            return new Vector3(0f, 0f, 0f);
        }

        GameObject markerObject = GameObject.Find(marker_names[0].Trim());
        if (markerObject == null)
        {
            Debug.LogWarning($"SpawnLitter: First marker '{marker_names[0]}' not found. Using default position (0, 0, 0)");
            return new Vector3(0f, 0f, 0f);
        }

        // マーカー位置から Y 軸方向に 3.0 オフセット（マーカーの手前）
        // これにより、初期化直後に即座に到達判定されることを防ぐ
        Vector3 markerPosition = markerObject.transform.position;
        Vector3 spawnPosition = markerPosition + Vector3.down * 3.0f;
        
        Debug.Log($"SpawnLitter: Marker '{marker_names[0]}' at {markerPosition}, spawning Litter at {spawnPosition}");
        return spawnPosition;
    }

    private bool SpawnWaterTurret(Vector3 spawnPoint = default(Vector3))
    {
        float dropbuffer = 1.5f;
        GameObject waterTurretPrefab = PrefabManager.WaterTurretPrefab;
        if (waterTurretPrefab == null)
        {
            Debug.LogWarning("WaterTurret prefab not found in PrefabManager");
            return false;
        }
        
        spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
        if (spawnPoint == Vector3.zero)
        {
            Debug.LogWarning("[SpawnController] WaterTurret: pathmaker 除外エリア内でスポーン不可");
            return false;
        }
        Quaternion spawnRotation = Quaternion.identity;
        
        // Instantiate 結果をチェック
        GameObject waterTurretInstance = Instantiate(waterTurretPrefab, spawnPoint, spawnRotation);
        if (waterTurretInstance == null)
        {
            Debug.LogWarning("Failed to instantiate WaterTurret prefab");
            return false;
        }
        
        // GetComponent 結果をチェック
        WaterTurretCtrl turretCtrl = waterTurretInstance.GetComponent<WaterTurretCtrl>();
        if (turretCtrl == null)
        {
            Debug.LogWarning("WaterTurretCtrl component not found on instantiated WaterTurret prefab");
            Destroy(waterTurretInstance);
            return false;
        }
        
        // CreateWaterTurretUnit を呼ぶ
        turretCtrl.CreateWaterTurretUnit(spawnPoint);
        return true;
    }

    private bool SpawnFireCube(Vector3 spawnPoint = default(Vector3))
    {
        float dropbuffer = 1.5f;        
        bool result = false;
        spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
        if (spawnPoint == Vector3.zero)
        {
            Debug.LogWarning("[SpawnController] FireCube: pathmaker 除外エリア内でスポーン不可");
            return false;
        }
        GameObject garbageObj = FireCubeCtrl.SpawnFireCube(spawnPoint, FireCubeCtrl._SIZE_NORMAL, false);
        if (garbageObj != null)
        {
            result = true;
        }
        return result;
    }

    private bool SpawnGarbageCubeBig(Vector3 spawnPoint = default(Vector3))
    {
        bool result = SpawnGarbageCube(GarbageCubeCtrl._GARBAGE_CUBE_SIZE_BIG_MAX, spawnPoint, GarbageCubeCtrl._SIZE_BIG, false);
        return result;
    }

    private bool SpawnGarbageCubeBox(Vector3 spawnPoint = default(Vector3))
    {
        bool result = SpawnGarbageCubeBoxCoroutine(spawnPoint);
        return result;
    }

    private bool SpawnGarbageCubeBoxCoroutine(Vector3 spawnPoint)
    {
        float dropnumber = 20;
        for (int i = 0; i < dropnumber; i++)
        {
            if (!SpawnGarbageCube(0.1f * i, spawnPoint, GarbageCubeCtrl._SIZE_SMALL, true))
            {
                return false;
            }
            // await Task.Delay(5); // ミリ秒待機
        }

        return true;
    }

    // private IEnumerator waitSpawner(float waitTime)
    // {
    //     yield return new WaitForSeconds(waitTime);
    //     SpawnGarbageCube();
    // }

    private bool SpawnGarbageCube(float dropbuffer = 1.5f, Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
    {
        bool ret = false;
        spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
        if (spawnPoint == Vector3.zero)
        {
            Debug.LogWarning("[SpawnController] GarbageCube: pathmaker 除外エリア内でスポーン不可");
            return false;
        }
        GameObject garbageObj = GarbageCubeCtrl.SpawnGarbageCube(spawnPoint, sizeFlag, isSwayingPoint);
        if (garbageObj != null)
        {
            ret = true;
        }
        return ret;
    }

    private bool SpawnStopPlate(float dropbuffer = 0.05f)
    {
        bool ret = false;
        GameObject prefab = PrefabManager.StopPlatePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("StopPlate prefab not found in PrefabManager");
            return ret;
        }
        Vector3 setPoint = GetSpawnPoint(dropbuffer);
        if (setPoint == Vector3.zero)
        {
            Debug.LogWarning("[SpawnController] StopPlate: pathmaker 除外エリア内でスポーン不可");
            return false;
        }
        Quaternion setRotation = SpawnMarkerPointerCtrl.GetMarkerRotateAngle();
        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        int idx = PrefabManager.StopPlateUID;
        unit.name = GameEnum.ModelsType.StopPlate.ToString() + idx.ToString();
        StopPlate stopPlate = GameObjectTreat.GetOrAddComponent<StopPlate>(unit);
        stopPlate._item_struct.ItemID = unit.name;
        stopPlate._unit_struct.UnitID = unit.name;

        ret = true;
        return ret;
    }

    private bool SpawnPowerCube(float dropbuffer = 0.25f, Vector3 setPoint = default(Vector3))
    {
        bool ret = false;
        GameObject prefab = PrefabManager.PowerCubePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("PowerCube prefab not found in PrefabManager");
            return false;
        }
        prefab.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        setPoint = GetSpawnPoint(dropbuffer, setPoint);
        if (setPoint == Vector3.zero)
        {
            Debug.LogWarning("[SpawnController] PowerCube: pathmaker 除外エリア内でスポーン不可");
            return false;
        }
        Quaternion setRotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), rdNum(0,360));
        GameObject unit = Instantiate(prefab, setPoint, setRotation);

        int idx = PrefabManager.PowerCubeUID;
        unit.name = GameEnum.ModelsType.PowerCube.ToString() + idx.ToString();
        PowerCube powerCube = GameObjectTreat.GetOrAddComponent<PowerCube>(unit);
        powerCube._item_struct.ItemID = unit.name;
        powerCube._unit_struct.UnitID = unit.name;

        ret = true;
        return ret;
    }


    private bool SpawnTowerSweeper(float dropbuffer = 0.05f, Vector3 setPoint = default(Vector3))
    {
        bool ret = false;
        setPoint = GetSpawnPoint(dropbuffer, setPoint);
        if (setPoint == Vector3.zero)
        {
            Debug.LogWarning("[SpawnController] TowerSweeper: pathmaker 除外エリア内でスポーン不可");
            return false;
        }
        GameObject prefab = PrefabManager.TowerSweeperPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("TowerSweeper prefab not found in PrefabManager");
            return false;
        }
        Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
        GameObject TowerSweeper = Instantiate(prefab, setPoint, rotation);
        TowerSweeperCtrl obj = TowerSweeper.GetComponent<TowerSweeperCtrl>();
        if (obj == null)
        {
            Debug.LogError("[SpawnController] TowerSweeperCtrl component not found on TowerSweeper prefab instance");
            GameObjectTreat.DestroyAll(TowerSweeper);
            return false;
        }
        obj.CreateSweeperUnit(setPoint);
        ret = true;
        return ret;
    }

    private Vector3 GetSpawnPoint(float dropbuffer = 0.05f, Vector3 setPoint = default(Vector3))
    {
        if (setPoint == default(Vector3))
        {
            setPoint = SpawnMarkerPointerCtrl.GetMarkerPosition();
        }

        // PathMaker のポイントとの距離をチェック（除外エリア判定）
        if (IsWithinPathMarkerExclusionZone(setPoint))
        {
            Debug.LogWarning("[SpawnController] スポーン位置が pathmark 敵経路上にあります。スポーンをキャンセルします。");
            return Vector3.zero;
        }

        setPoint.y += dropbuffer;
        return setPoint;
    }

    /// <summary>
    /// スポーン位置が pathmakers の除外エリア内にあるかを判定
    /// </summary>
    private bool IsWithinPathMarkerExclusionZone(Vector3 spawnPoint)
    {
        Dictionary<string, Vector3> pathMakers = PathMakerCtrl.GetPathMakerDict();
        if (pathMakers == null || pathMakers.Count == 0)
        {
            return false;
        }

        foreach (Vector3 markerPos in pathMakers.Values)
        {
            float distance = Vector3.Distance(spawnPoint, markerPos);
            if (distance < _PATH_MARKER_EXCLUSION_RADIUS)
            {
                Debug.Log($"[SpawnController] pathmaker 除外エリア内: 距離 {distance:F2}m");
                // NG_point を表示（1秒後に自動で消える）
                SpawnMarkerPointerCtrl.ShowNGPoint();
                return true;
            }
        }

        return false;
    }

    private Quaternion GetSpawnRotateAngle()
    {
        return SpawnMarkerPointerCtrl.GetMarkerRotateAngle();
    }

    private SpawnMarkerPointerCtrl GetSpawnMarkerCtrl()
    {
        GameObject gameObject = GameObject.FindWithTag(GameEnum.UIType.SpawnMarker.ToString());
        if (gameObject == null)
        {
            return null;
        }
        return gameObject.GetComponent<SpawnMarkerPointerCtrl>();
    }

    private float rdNum(float min, float max)
    {
        float num = Utility.fRandomRange(min, max);
        return num;
    }
}
