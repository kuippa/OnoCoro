using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSpawn : MonoBehaviour
{
    
    void Awake()
    {
        Debug.Log("UnitSpawn.Awake()");

        // ボタンを探してクリックイベントに登録する
        // GameObject button = GameObject.Find("Button");
        // GameObject button = GameObject.Find("btnSpawnUnit");
        GameObject button = this.gameObject;

        // buttonにイベントリスナーを登録する
        button.GetComponent<Button>().onClick.AddListener(OnClickButton);


        // Spawn a unit
        // GameObject unit = Instantiate(Resources.Load("Unit")) as GameObject;
        // unit.transform.position = new Vector3(0, 0, 0);
    }


    void OnClickButton()
    {

        Debug.Log("OnClickButton()");
        // Dropdownから現在のオプションを取得する
        GameObject dropdown = GameObject.Find("Dropdown");
        if (dropdown == null)
        {
            Debug.Log("dropdown is null");
            return;
        }
        // Dropdown compdropdown = dropdown.GetComponent<Dropdown>();
        // if (compdropdown == null)
        // {
        //     Debug.Log("compdropdown is null");
        //     return;
        // }

        TMP_Dropdown compdropdown2 = dropdown.GetComponent<TMP_Dropdown>();
        if (compdropdown2 == null)
        {
            Debug.Log("compdropdown2 is null");
            return;
        }
        int option = compdropdown2.value;

        // int option = dropdown.GetComponent<Dropdown>().value;
        // ラベルを取得
        // string label = dropdown.GetComponent<Dropdown>().options[option].text;
        string label = compdropdown2.options[option].text;
        Debug.Log("option = " + option + ", label = " + label);

        // optionに応じて処理を分岐する
        if (CallUnitSpawn(option)){
            Debug.Log("CallUnitSpawn() is true");
        }


    }

    private bool CallUnitSpawn(int option)
    {
        Debug.Log("CallUnitSpawn()");
        // 3D オブジェクトを生成する
        // GameObject unit = Instantiate(Resources.Load("Unit")) as GameObject;
        GameObject unit;
        switch (option)
        {
            case 0:
                // 3Dキューブを生成する
                CubeSpawner();
                break;
            case 1:
                // ゴミ収集人
                GarbageCubeCollector();
                break;
            case 2:
                // 3D 球を生成する
                unit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                // GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                unit.transform.position = new Vector3(0, 3, -4);
                Debug.Log("case 2");
                break;
            case 3: // ゴミ散らかし人
                Debug.Log("case 3");
                SpawnEnemyLitter();
                break;
            case 4: // ゴミ掃除人
                Debug.Log("case 4");
                SpawnTowerSweeper();
                break;
            default:
                Debug.Log("default");
                break;
        }
        return true;
    }

    private void SpawnEnemyLitter()
    {
        // Vector3 setPoint = new Vector3(0.5f, 1.65f, -4);
        Vector3 setPoint = GetSpawnPoint();
        GameObject EnemyLitter = Instantiate(Resources.Load("Prefabs/EnemyLitter")) as GameObject;
        EnemyLitter obj = EnemyLitter.GetComponent<EnemyLitter>();
        obj.CreateLitterUnit(setPoint);


    }

    private void SpawnTowerSweeper()
    {
        // Vector3 setPoint = new Vector3(0.5f, 2.65f, -4);
        Vector3 setPoint = GetSpawnPoint();
        GameObject TowerSweeper = Instantiate(Resources.Load("Prefabs/TowerSweeper")) as GameObject;
        TowerSweeper obj = TowerSweeper.GetComponent<TowerSweeper>();
        obj.CreateSweeperUnit(setPoint);
    }

    private Vector3 GetSpawnPoint()
    {
        MarkerPointerCtrl markerPointerCtrl = GameObject.FindWithTag(GameEnum.UIType.SpawnMarker.ToString()).GetComponent<MarkerPointerCtrl>();
        // MarkerPointerCtrl markerPointerCtrl = GameObject.Find("MarkerPointer").GetComponent<MarkerPointerCtrl>();
        Vector3 setPoint = markerPointerCtrl.GetMarkerPosition();

        // Vector3 setPoint = new Vector3(rdNum(-2,2), rdNum(0,2), -4);
        return setPoint;
    }


    private bool CubeSpawner()
    {
        Debug.Log("CubeSpawner()");
		GameObject prefab = Resources.Load<GameObject>("Prefabs/GarbageCube");
        prefab.transform.localScale = new Vector3(rdNum(0.1f,0.3f), rdNum(0.1f,0.3f), rdNum(0.1f,0.3f));
        Vector3 setPoint = new Vector3(rdNum(-2,2), rdNum(0,2), -4);
        Quaternion setRotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), rdNum(0,360));
        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        // unit.transform.position = new Vector3(rdNum(-2,2), rdNum(0,2), -4);
        // unit.transform.rotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), rdNum(0,360));
        // unit.transform.localScale = new Vector3(rdNum(0.1f,0.5f), rdNum(0.1f,0.5f), rdNum(0.1f,0.5f));
        return true;
    }

    private bool GarbageCubeCollector()
    {
        Debug.Log("GarbageCubeCollector()");

        GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        unit.transform.position = new Vector3(rdNum(-2,2), 1, -5);
        // unit.transform.rotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), rdNum(0,360));
        // unit.transform.localScale = new Vector3(rdNum(0.1f,0.5f), rdNum(0.1f,0.5f), rdNum(0.1f,0.5f));


        return true;
    }


    private float rdNum(float min, float max)
    {
        float num = Random.Range(min, max);
        // int num = Random.Range(min, max);
        return num;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
