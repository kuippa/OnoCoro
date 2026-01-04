using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonsUtility;
using System;

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

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void OnDestory()
    {
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (_instance == this)
        {
            _instance = null;
        }
    }

    // private void SpawnEnemyLitter()
    // {
    //     // Vector3 setPoint = new Vector3(0.5f, 1.65f, -4);
    //     Vector3 setPoint = GetSpawnPoint();
    //     GameObject EnemyLitter = Instantiate(Resources.Load("Prefabs/EnemyLitter")) as GameObject;
    //     EnemyLitter obj = EnemyLitter.GetComponent<EnemyLitter>();
    //     obj.CreateLitterUnit(setPoint);
    // }

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
            ret = SpawnTowerSweeper();
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
        else if (unitName == GameEnum.ModelsType.Extinguishing.ToString())
        {
            ret = SpawnExtinguishing(spawnPoint);
        }
        else
        {
            Debug.Log("default" + unitName);
        }
        return ret;
    }

    private bool SpawnExtinguishing(Vector3 spawnPoint = default(Vector3))
    {
        float dropbuffer = 1.5f;        
        bool result = false;
        spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
        GameObject Extinguishing = Instantiate(Resources.Load("Prefabs/WorkUnit/Extinguishing")) as GameObject;
        ExtinguishingCtrl obj = Extinguishing.GetComponent<ExtinguishingCtrl>();
        obj.CreateExtinguishingUnit(spawnPoint);
        result = true;
        return result;
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
		GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/StopPlate");
        Vector3 setPoint = GetSpawnPoint(dropbuffer);
        Quaternion setRotation = SpawnMarkerPointerCtrl.GetMarkerRotateAngle();
        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        int idx = GameObjectTreat.IndexObjectByTag(GameEnum.TagType.StopPlate.ToString());
        unit.name = GameEnum.ModelsType.StopPlate.ToString() + idx.ToString();
        unit.AddComponent<StopPlate>();
        unit.GetComponent<StopPlate>()._item_struct.ItemID = unit.name;

        ret = true;
        return ret;
    }

    private bool SpawnPowerCube(float dropbuffer = 0.25f, Vector3 setPoint = default(Vector3))
    {
        bool ret = false;
		GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/PowerCube");
        prefab.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        setPoint = GetSpawnPoint(dropbuffer, setPoint);
        Quaternion setRotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), rdNum(0,360));
        GameObject unit = Instantiate(prefab, setPoint, setRotation);

        int idx = GameObjectTreat.IndexObjectByTag(GameEnum.TagType.PowerCube.ToString());
        unit.name = GameEnum.ModelsType.PowerCube.ToString() + idx.ToString();
        unit.AddComponent<PowerCube>();
        unit.GetComponent<PowerCube>()._item_struct.ItemID = unit.name;
        unit.GetComponent<PowerCube>()._unit_struct.UnitID = unit.name;

        ret = true;
        return ret;
    }


    private bool SpawnTowerSweeper()
    {
        bool ret = false;
        Vector3 setPoint = GetSpawnPoint();
        GameObject TowerSweeper = Instantiate(Resources.Load("Prefabs/WorkUnit/TowerSweeper")) as GameObject;
        TowerSweeper obj = TowerSweeper.GetComponent<TowerSweeper>();
        obj.CreateSweeperUnit(setPoint);
        ret = true;
        return ret;
    }

    private Vector3 GetSpawnPoint(float dropbuffer = 0.05f, Vector3 setPoint = default(Vector3))
    {
        GameObject spawnMarker = GameObject.FindWithTag(GameEnum.UIType.SpawnMarker.ToString());
        if (spawnMarker == null)
        {
            setPoint.y += dropbuffer;
            return setPoint;
        }
        SpawnMarkerPointerCtrl markerPointerCtrl = spawnMarker.GetComponent<SpawnMarkerPointerCtrl>();
        setPoint = markerPointerCtrl.GetMarkerPosition();
        setPoint.y += dropbuffer;
        return setPoint;
    }

    private float rdNum(float min, float max)
    {
        float num = Utility.fRandomRange(min, max);
        // float num = Random.Range(min, max);
        // int num = Random.Range(min, max);
        return num;
    }
}
