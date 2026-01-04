using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonsUtility;
using System;

public class SpawnCtrl : MonoBehaviour
{

    void Awake()
    {
        Debug.Log("SpawnCtrl.Awake()");
        // // ボタンを探してクリックイベントに登録する
        // // GameObject button = GameObject.Find("Button");
        // // GameObject button = GameObject.Find("btnSpawnUnit");
        // GameObject button = this.gameObject;

        // // buttonにイベントリスナーを登録する
        // button.GetComponent<Button>().onClick.AddListener(OnClickButton);
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

        // Sweeper sweeper = new Sweeper();
        // Debug.Log("SpawnCtrl.CallUnitByName() sweeper.GetItemStruct().Name:" + sweeper.GetItemStruct().Name);
        // string sweeperName = sweeper.GetItemStruct().Name;
        // ModelsEnum.ModelsType modelsType = (ModelsEnum.ModelsType)System.Enum.Parse(typeof(ModelsEnum.ModelsType), unitName);
        // ModelsEnum.ModelsType.GarbageCube.ToString();
        // Debug.Log("SpawnCtrl.CallUnitByName() unitName:" + unitName + Enum.GetName(typeof(ModelsEnum.ModelsType), unitName));


        bool ret = false;
        switch (unitName)
        {
            // case ModelsEnum.ModelsType.GarbageCube.ToString() :
            //     // ret = SpawnGarbageCubeCollector();
            //     break;
            // case "GarbageCubeCollector":
            //     ret = SpawnGarbageCubeCollector();
            //     break;

            // TODO: ここで、unitName に応じて処理を分岐する
            // 直値じゃなくてModelsEnum.ModelsType.GarbageCube.ToString() みたいな感じで
            // case ModelsEnum.ModelsType.GarbageCube.ToString():
            //     ret = SpawnGarbageCube();
            //     break;

            case "GarbageCube":
                ret = SpawnGarbageCube(0, spawnPoint);
                break;
            case "GarbageCubeBox":
                ret = SpawnGarbageCubeBox(spawnPoint);
                break;
            case "Sweeper":
                ret = SpawnTowerSweeper();
                break;
            case "PowerCube":
                ret = SpawnPowerCube();
                break;
            default:
                Debug.Log("default" + unitName);
                break;
        }
        return ret;
    }

    private bool SpawnGarbageCubeBox( Vector3 spawnPoint = default(Vector3))
    {
        bool ret = false;
        // float dropnumber = rdNum(20,60);
        float dropnumber = 10;
        for (int i = 0; i < dropnumber; i++)
        {
            // if (!SpawnGarbageCube(rdNum(1,4), spawnPoint))
            if (!SpawnGarbageCube(0.5f * i, spawnPoint))
            {
                return false;
            }
            // // 0.1 ～ 0.5 秒のランダムな時間だけ待つ
            // yield return new WaitForSeconds(rdNum(0.1f,0.5f));

        }
        ret = true;
        return ret;
    }

    // private IEnumerator waitSpawner(float waitTime)
    // {
    //     yield return new WaitForSeconds(waitTime);
    //     SpawnGarbageCube();
    // }

    private bool SpawnGarbageCube(float dropbuffer = 1.5f, Vector3 spawnPoint = default(Vector3))
    {
        bool ret = false;
		GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/GarbageCube");
        prefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        // prefab.transform.localScale = new Vector3(rdNum(0.1f,0.3f), rdNum(0.1f,0.3f), rdNum(0.1f,0.3f));
        // prefab.transform.localScale = new Vector3(rdNum(0.1f,3.3f), rdNum(0.1f,3.3f), rdNum(0.1f,3.3f));

        if (spawnPoint == default(Vector3))
        {
            spawnPoint = GetSpawnPoint(dropbuffer);
        }
        else
        {
            // spawnPoint.y += dropbuffer;
            spawnPoint.z += dropbuffer;
        }
        Vector3 setPoint = spawnPoint;
        // Vector3 setPoint = new Vector3(rdNum(-2,2), rdNum(0,2), -4);
        // Quaternion setRotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), rdNum(0,360));
        Quaternion setRotation = Quaternion.identity;

        // if (relativePos == Vector3.zero)
        // {
        //     rotation = Quaternion.identity;
        // }
        // else
        // {
        //     rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        // }

        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        unit.tag = GameEnum.EnemyType.Garbage.ToString();

        int countGarbage = GameObject.FindGameObjectsWithTag(GameEnum.EnemyType.Garbage.ToString()).Length;
        unit.name = ModelsEnum.ModelsType.GarbageCube.ToString() + countGarbage.ToString();
        unit.AddComponent<GarbageCube>();

        ret = true;
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
        unit.tag = GameEnum.EnemyType.Garbage.ToString();

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
            // Debug.Log("SpawnMarker is null");
            return new Vector3(0, 0, 0);
        }
        MarkerPointerCtrl markerPointerCtrl = spawnMarker.GetComponent<MarkerPointerCtrl>();
        Vector3 setPoint = markerPointerCtrl.GetMarkerPosition();
        setPoint.y += dropbuffer;
        return setPoint;
    }

// navMeshAgent.isOnNavMesh
// false
// navMeshAgent.isActiveAndEnabled
// false
// navMeshAgent.isPathStale
// false
// navMeshAgent.isStopped
// false


    private float rdNum(float min, float max)
    {
        float num = Utility.fRandomRange(min, max);
        // float num = Random.Range(min, max);
        // int num = Random.Range(min, max);
        return num;
    }
}
