using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonsUtility;
using System;

/// <summary>
/// ユニットスポーン管理クラス
/// 各種ユニット（ゴミキューブ、炎キューブ、タワー等）の生成を制御します
/// </summary>
public class SpawnCtrl : MonoBehaviour 
{
    public static SpawnCtrl _instance = null;

    public static SpawnCtrl Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SpawnCtrl>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnCtrl");
                    _instance = go.AddComponent<SpawnCtrl>();
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

    private void OnDestory()
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
        GameObject gameObject = Instantiate(PrefabManager.DustBoxPrefab, spawnPoint, spawnRotateAngle);
        int dustBoxUID = PrefabManager.DustBoxUID;
        gameObject.name = GameEnum.ModelsType.DustBox.ToString() + dustBoxUID;
        DustBox orAddComponent = GameObjectTreat.GetOrAddComponent<DustBox>(gameObject);
        orAddComponent._item_struct.ItemID = gameObject.name;
        orAddComponent._unit_struct.UnitID = gameObject.name;
        return true;
    }

    private bool SpawnLitter(string[] marker_names)
    {
        GameObject gameObject = Instantiate(PrefabManager.EnemyLitterPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        EnemyLitter component = gameObject.GetComponent<EnemyLitter>();
        int idx = EnemyLitter._idx;
        gameObject.name = GameEnum.ModelsType.Litter.ToString() + idx;
        Litter orAddComponent = GameObjectTreat.GetOrAddComponent<Litter>(gameObject);
        orAddComponent._unit_struct.UnitID = gameObject.name;
        orAddComponent._item_struct.ItemID = gameObject.name;
        component.InitUnitSpawn(marker_names);
        return true;
    }

    private bool SpawnWaterTurret(Vector3 spawnPoint = default(Vector3))
    {
// TODO:
        // float dropbuffer = 1.5f;
        // GameObject waterTurretPrefab = PrefabManager.WaterTurretPrefab;
        // if (waterTurretPrefab == null)
        // {
        //     waterTurretPrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/WaterTurret");
        // }
        // spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
        // Instantiate(waterTurretPrefab).GetComponent<WaterTurretCtrl>().CreateWaterTurretUnit(spawnPoint);
        return true;
    }

    private bool SpawnFireCube(Vector3 spawnPoint = default(Vector3))
    {
        float dropbuffer = 1.5f;        
        bool result = false;
        spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
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
            prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/StopPlate");
        }
        Vector3 setPoint = GetSpawnPoint(dropbuffer);
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
            prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/PowerCube");
        }
        prefab.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        setPoint = GetSpawnPoint(dropbuffer, setPoint);
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
        GameObject prefab = PrefabManager.TowerSweeperPrefab;
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/TowerSweeper");
        }
        Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
        GameObject TowerSweeper = Instantiate(prefab, setPoint, rotation);
        TowerSweeper obj = TowerSweeper.GetComponent<TowerSweeper>();
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
        setPoint.y += dropbuffer;
        return setPoint;
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
