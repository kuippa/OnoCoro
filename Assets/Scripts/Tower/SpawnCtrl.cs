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
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        // // ボタンを探してクリックイベントに登録する
        // // GameObject button = GameObject.Find("Button");
        // // GameObject button = GameObject.Find("btnSpawnUnit");
        // GameObject button = this.gameObject;

        // // buttonにイベントリスナーを登録する
        // button.GetComponent<Button>().onClick.AddListener(OnClickButton);

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


    // void OnClickButton()
    // {

    //     Debug.Log("OnClickButton()");
    //     // Dropdownから現在のオプションを取得する
    //     GameObject dropdown = GameObject.Find("Dropdown");
    //     if (dropdown == null)
    //     {
    //         Debug.Log("dropdown is null");
    //         return;
    //     }
    //     // Dropdown compdropdown = dropdown.GetComponent<Dropdown>();
    //     // if (compdropdown == null)
    //     // {
    //     //     Debug.Log("compdropdown is null");
    //     //     return;
    //     // }

    //     TMP_Dropdown compdropdown2 = dropdown.GetComponent<TMP_Dropdown>();
    //     if (compdropdown2 == null)
    //     {
    //         Debug.Log("compdropdown2 is null");
    //         return;
    //     }
    //     int option = compdropdown2.value;

    //     // int option = dropdown.GetComponent<Dropdown>().value;
    //     // ラベルを取得
    //     // string label = dropdown.GetComponent<Dropdown>().options[option].text;
    //     string label = compdropdown2.options[option].text;
    //     Debug.Log("option = " + option + ", label = " + label);

    //     // optionに応じて処理を分岐する
    //     if (CallUnitSpawn(option)){
    //         Debug.Log("CallUnitSpawn() is true");
    //     }


    // }

    // private bool CallUnitSpawn(int option)
    // {
    //     Debug.Log("CallUnitSpawn()");
    //     // 3D オブジェクトを生成する
    //     // GameObject unit = Instantiate(Resources.Load("Unit")) as GameObject;
    //     GameObject unit;
    //     switch (option)
    //     {
    //         case 0:
    //             // 3Dキューブを生成する
    //             CubeSpawner();
    //             break;
    //         case 1:
    //             // ゴミ収集人
    //             GarbageCubeCollector();
    //             break;
    //         case 2:
    //             // 3D 球を生成する
    //             unit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //             // GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //             unit.transform.position = new Vector3(0, 3, -4);
    //             Debug.Log("case 2");
    //             break;
    //         case 3: // ゴミ散らかし人
    //             Debug.Log("case 3");
    //             SpawnEnemyLitter();
    //             break;
    //         case 4: // ゴミ掃除人
    //             Debug.Log("case 4");
    //             SpawnTowerSweeper();
    //             break;
    //         default:
    //             Debug.Log("default");
    //             break;
    //     }
    //     return true;
    // }

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
            ret = SpawnPowerCube();
        }
        else
        {
            Debug.Log("default" + unitName);
        }
        return ret;
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
        if (spawnPoint == default(Vector3))
        {
            spawnPoint = GetSpawnPoint(dropbuffer);
        }
        else
        {
            spawnPoint.z += dropbuffer;
        }
        Vector3 setPoint = spawnPoint;
        GameObject garbageObj = GarbageCubeCtrl.SpawnGarbageCube(spawnPoint, sizeFlag, isSwayingPoint);
        if (garbageObj != null)
        {
            ret = true;
        }
        return ret;
    }

    private bool SpawnPowerCube(float dropbuffer = 0.25f)
    {
        bool ret = false;
		GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/PowerCube");
        prefab.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        Vector3 setPoint = GetSpawnPoint(dropbuffer);
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

    private Vector3 GetSpawnPoint(float dropbuffer = 0.05f)
    {
        GameObject spawnMarker = GameObject.FindWithTag(GameEnum.UIType.SpawnMarker.ToString());
        if (spawnMarker == null)
        {
            return new Vector3(0, 0, 0);
        }
        MarkerPointerCtrl markerPointerCtrl = spawnMarker.GetComponent<MarkerPointerCtrl>();
        Vector3 setPoint = markerPointerCtrl.GetMarkerPosition();
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
