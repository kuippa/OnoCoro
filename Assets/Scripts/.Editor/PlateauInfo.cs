using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PLATEAU.CityInfo;
using PLATEAU.Native;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine.InputSystem.XInput;
using CommonsUtility;

// using System.Numerics;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif


public class PlateauInfo : MonoBehaviour
{
    public static PlateauInfo instance = null;

    const float _CLICK_LIMIT_DISTANCE = 250.0f;
    const float _CENTER_Y_OFFSET = 5.0f;
    const float _BUFFER_Y_OFFSET = 0.5f;

    const float _BLDG_HEIGHT_PAR_FLOOR = 3.0f;  // 1階の高さ(m)
    const float _BLDG_FLOOR_PAR_PERSON = 15.0f;  // 1人あたりの面積(m^2) 
    const float _BLDG_CORRIDOR_PAR_ROOM = 10.0f;  // 導線空間の面積(m^2) 

    const string _PLATEAU_OBJECT_TAG = "PlateauObject";
    private XMLparser _xmlParser;

    private Dictionary<string, Dictionary<string, string>> _dictBuilding = new Dictionary<string, Dictionary<string, string>>();
    private Dictionary<string, Material[]> _dictBuildingMaterial = new Dictionary<string, Material[]>();

    private List<GameObject> _doomedBuilding = new List<GameObject>();
    private GameObject _lastTargetObj = null;
    private GameObject _infoBox = null;

    // [SerializeField] private PLATEAUInstancedCityModel cityModel;

    // void ConvertLatLng()
    // {
    //     var geoReference = this.cityModel.GeoReference;
    //     // Unproject : PLATEAUInstancedCityModelから見た位置を緯度経度に変換
    //     // Project : その逆
    //     var latLon0 = geoReference.Unproject(new PlateauVector3d(0, 0, 0));
    //     Debug.Log($"(0,0,0)の緯度経度は {latLon0}" );
    //     var latLon0_1000 = geoReference.Unproject(new PlateauVector3d(0, 0, 100000));
    //     Debug.Log($"(0, 0, 100000)の緯度経度は {latLon0_1000}");
    //     var latlon1000_0 = geoReference.Unproject(new PlateauVector3d(100000, 0, 0));
    //     Debug.Log($"(100000, 0, 0)の緯度経度は {latlon1000_0}");
    //     var geoRef = geoReference.Project(new GeoCoordinate(35.6, 139, 0));
    //     Debug.Log($"経度緯度が 35.6, 139 は {geoRef}");
    // }

    // リファクタリング案
// PlateauInfoManager (メインクラス)
// PlateauObjectSelector
// PlateauDataExtractor
// PlateauUIManager
// PlateauBuildingInteractor

    private void CallCircularIndicator(GameObject target)
    {
        GameObject infoBox = GetInfoBox();
        infoBox.SetActive(false);

        GameObject UICircularIndicator = Instantiate(Resources.Load("Prefabs/UI/UICircularIndicator")) as GameObject;
        CircularIndicator indicator = UICircularIndicator.GetComponent<CircularIndicator>();
        
        GameObject indicator_object = new GameObject("Indicator");
        indicator_object.transform.SetParent(target.transform);
        Canvas indicator_canvas = indicator_object.AddComponent<Canvas>();
        
        Renderer renderer = target.GetComponent<Renderer>();
        Vector3 center = renderer.bounds.center;
        // center.y += _CENTER_Y_OFFSET  * 0.5f + _BUFFER_Y_OFFSET;
        center.y += _CENTER_Y_OFFSET * 2 + _BUFFER_Y_OFFSET;
        indicator_object.transform.position = center;

        indicator.StartIndicator(5f, () => {
                    CompleteReBuildProcess(target);
                    }, indicator_object);
    }

    private void CompleteReBuildProcess(GameObject target)
    {
        // TODO:GetCLKbyBuilding
        // 建物再建によるスコア加算（クロックを取得）

        SetMaterialToOriginal(_lastTargetObj);
        _doomedBuilding.Remove(_lastTargetObj);
        CloseInfoBox();
    }

    private string GetBuildingUsage(string key)
    {
        string ret = "";
        _xmlParser = this.GetComponent<XMLparser>();
        if (_xmlParser == null)
        {
            _xmlParser = this.gameObject.AddComponent<XMLparser>();
        }
        ret = _xmlParser.GetBuildingUsage(key);
        return ret;
    }

    private bool CheckObjectPlateau(GameObject gameObject)
    {
        bool ret = false;
        if (gameObject == null)
        {
            return ret;
        }

        // Debug.Log("gameObject.tag " + gameObject.tag);
        // if (gameObject.tag != _PLATEAU_OBJECT_TAG)
        // {
        //     Debug.Log("not PlateauObject");
        //     return ret;
        // }

        PLATEAUCityObjectGroup cityObjs = gameObject.GetComponent<PLATEAUCityObjectGroup>();
        if (cityObjs == null) {
            // Debug.Log("cityObjs is null");
            return ret;
        }

        ret = true;
        return ret;
    }

    internal bool GetPlateauInfo()
    {
        bool ret = GetTarget();
        return ret;
    }

    private bool GetTarget()
    {
        bool ret = false;
        GameObject gameObject = GetTargetObject();
        if (CheckObjectPlateau(gameObject) == false)
        {
            return ret;
        }
        _lastTargetObj = gameObject;

        Dictionary<string, string> dictInfo = new Dictionary<string, string>();
        if (_dictBuilding.TryGetValue(gameObject.name, out dictInfo))
        {
            // Debug.Log("dictInfo is already exist");
            // SetMaterialToOriginal(gameObject); 

        }
        else
        {
            dictInfo = GetTargetInfo(gameObject);
            _dictBuilding.Add(gameObject.name, dictInfo);

            // dispCubeMarker(gameObject, dictInfo);   // for debug
            // SetMaterialToDoom(gameObject);
        }
        DispInfo(dictInfo);
        ret = true;
        return ret;
    }

    private void StorageOriginalMaterial(GameObject gameObject)
    {
        Material[] materials = new Material[0];
        if (_dictBuildingMaterial.TryGetValue(gameObject.name, out materials) == false)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            // オリジナルのマテリアルを保持しておく
            if (renderer.materials.Length > 1)
            {
                // Debug.Log("material 複数パターン");
                _dictBuildingMaterial.Add(gameObject.name, renderer.materials);
                return;
            }
            else
            {
                Material setMaterial = (Material)renderer.material;
                if (materials == null) {
                    materials = new Material[1];
                }
                materials[0] = setMaterial;
                _dictBuildingMaterial.Add(gameObject.name,  materials);
            }
        }

    }

    private void ClickRebuildBtn()
    {
        // Debug.Log("ClickRebuildBtn" + _lastTargetObj.name);
        // gameObject が破壊されている場合は再生成する
        if (_lastTargetObj == null)
        {
            return;
        }
        if (!_doomedBuilding.Contains(_lastTargetObj))
        {
            return;
        }

        if (PayRebuildCost(_lastTargetObj))
        {
            CallCircularIndicator(_lastTargetObj);
        }
        else
        {
            // TODO:再建できない場合の処理
            Debug.Log("再建コストが足りません");
        }

    }

    private bool PayRebuildCost(GameObject targetObj)
    {
        bool ret = false;
        // 再建コストを計算して支払う
        Dictionary<string, string> dictInfo = new Dictionary<string, string>();
        if (_dictBuilding.TryGetValue(targetObj.name, out dictInfo))
        {
            int rebuildCost =  (int)CalcRebuildCost(dictInfo) * -1;
            
            if (ScoreCtrl.IsScorePositiveInt(rebuildCost, GlobalConst.SHORT_SCORE1_SCALE))
            {
                ScoreCtrl.UpdateAndDisplayScore((int)rebuildCost, GlobalConst.SHORT_SCORE1_SCALE);
                return true;
            }
        }
        return ret;
    }


    internal void SetMaterialToDoom(GameObject gameObject)
    {
        StorageOriginalMaterial(gameObject);
        if (!_doomedBuilding.Contains(gameObject))
        {
            _doomedBuilding.Add(gameObject);
        }

        // 破壊されたマテリアルに変更
        // Material doom_material = Resources.Load("Materials/PlateauDefaultVegetation") as Material;
        Material doom_material = Resources.Load("Materials/PlateauGenericWood") as Material;
        // Debug.Log("doom_material " + doom_material.name);
        if (gameObject.GetComponent<Renderer>().materials.Length > 1)
        {
            Material[] doom_materials = gameObject.GetComponent<Renderer>().materials;
            // Debug.Log("material 複数パターン" + gameObject.GetComponent<Renderer>().materials.Length + gameObject.name);
            for (int i = 0; i < gameObject.GetComponent<Renderer>().materials.Length; i++)
            {
                doom_materials[i] = doom_material;
                // gameObject.GetComponent<Renderer>().materials[i] = doom_material;
                // Debug.Log("i " + i + gameObject.GetComponent<Renderer>().materials[i].name);

            }
            gameObject.GetComponent<Renderer>().materials = doom_materials;
            // GameObjectTreat.DestroyMaterialsAll(doom_materials);
        }
        else
        {
            // Debug.Log("material 単数パターン");
            gameObject.GetComponent<Renderer>().material = doom_material;
        }
        // Destroy(doom_material);
    }

    private void SetMaterialToOriginal(GameObject gameObject)
    {
        Material[] materials = new Material[0];
        if (_dictBuildingMaterial.TryGetValue(gameObject.name, out materials))
        {
            // gameObject.GetComponent<Renderer>().material = material;
            if (materials.Length > 1)
            {
                // Material[] originMaterials = gameObject.GetComponent<Renderer>().materials;
                // for (int i = 0; i < materials.Length; i++)
                // {
                //     originMaterials[i] = materials[i];
                // }
                // gameObject.GetComponent<Renderer>().materials = originMaterials;
                gameObject.GetComponent<Renderer>().materials = materials;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material = materials[0];
            }
        }
        // GameObjectTreat.DestroyMaterialsAll(materials);
    }


    private float GetMesuredHeight(Dictionary<string, string> dictInfo)
    {
        float height = 0; 
        // bldg:measuredheight, value: 7.2
        if (dictInfo.ContainsKey("bldg:measuredheight"))
        {
            height = float.Parse(dictInfo["bldg:measuredheight"]);
        }

        return height;
    }

    private void dispCubeMarker(GameObject gameObject, Dictionary<string, string> dictInfo)
    {
            float height = GetMesuredHeight(dictInfo);
            GetMeshrenderInfo(gameObject, height);
    }

    private GameObject GetInfoBox()
    {
        if (_infoBox != null)
        {
            return _infoBox;
        }
        GameObject infoBox = GameObject.Find("UIBuildingInfo");
        if (infoBox == null)
        {
            infoBox = Instantiate(Resources.Load("Prefabs/UI/UIBuildingInfo") as GameObject);
            infoBox.name = "UIBuildingInfo";
        }

        GameObject pnlInfo = infoBox.transform.Find("pnlInfo").gameObject;
        GameObject pnlRebuild = pnlInfo.transform.Find("pnlRebuild").gameObject;
        Button btnRebuild = pnlRebuild.transform.Find("btnRebuild").gameObject.GetComponent<Button>();
        btnRebuild.onClick.RemoveAllListeners();
        btnRebuild.onClick.AddListener(ClickRebuildBtn);

        _infoBox = infoBox;
        return infoBox;
    }

    private void CloseInfoBox()
    {
        GameObject infoBox = GetInfoBox();
        // infoBox.SetActive(false);
        GameObjectTreat.DestroyAll(infoBox);
    }

    private void DispInfo(Dictionary<string, string> dictInfo)
    {
        GameObject infoBox = GetInfoBox();
        string buf = "";
        foreach (var pair in dictInfo)
        {
            string lang_h = LangCtrl.GetLangVal(pair.Key);
            if (lang_h == "")
            {
                // lang_h = pair.Key;
                // buf += $"{pair.Key}{lang_h}: {pair.Value}\n";
            }
            else
            {
                // buf += $"{lang_h}{pair.Key}: {pair.Value}\n";
                buf += $"{lang_h}: {pair.Value}{Environment.NewLine}";
            }
            // Debug.Log($"key: {pair.Key}, value: {pair.Value}");
        }
        float rebuildCost = CalcRebuildCost(dictInfo);
        buf += LangCtrl.GetLangVal("rebuildcost") +": "+ rebuildCost + Environment.NewLine;

        GameObject pnlInfo = infoBox.transform.Find("pnlInfo").gameObject;
        GameObject txtInfo = pnlInfo.transform.Find("txtInfo").gameObject;
        Text txtInfoText = txtInfo.GetComponent<Text>();
        txtInfoText.text = buf;

        GameObject pnlRebuild = pnlInfo.transform.Find("pnlRebuild").gameObject;
        if (!_doomedBuilding.Contains(_lastTargetObj))
        {
            pnlRebuild.SetActive(false);
            return;
        }
        else
        {
            pnlRebuild.SetActive(true);
        }

        GameObject txtRebuild = pnlRebuild.transform.Find("txtRebuild").gameObject;
        Text txtRebuildText = txtRebuild.GetComponent<Text>();
        txtRebuildText.text = " - " + rebuildCost.ToString() + GlobalConst.SHORT_SCORE1_SCALE;
    }

    private float CalcRebuildCost(Dictionary<string, string> dictInfo)
    {
        float ret = 0;
        if (dictInfo.ContainsKey("bldg:measuredheight") && dictInfo.ContainsKey("bldg:floor"))
        {
            float height = float.Parse(dictInfo["bldg:measuredheight"]);
            float floor = float.Parse(dictInfo["bldg:floor"]);
            ret = (height * floor / 10.0f);
            // 小数点以下切り捨て
            ret = Mathf.Floor(ret);
            if (ret < 1)
            {
                ret = 1;
            }
        }
        return ret;
    }


    private Dictionary<string, string> GetTargetInfo(GameObject targetObj)
    {
        Dictionary<string, string> dictInfo = new Dictionary<string, string>();
        PLATEAUCityObjectGroup cityObjs = targetObj.GetComponent<PLATEAUCityObjectGroup>();
        if (cityObjs == null) {
            // Debug.Log("cityObjs is null");
            return dictInfo;
        }

        Renderer renderer = targetObj.GetComponent<Renderer>();
        float floor = GetSupFloorSpace(renderer.bounds.extents);

        foreach (var cityObj in cityObjs.PrimaryCityObjects)
        {
            // Debug.Log(cityObj.GmlID + " name:" +  targetObj.name);
            // Debug.Log("CityObjectType:" + cityObj.CityObjectType);  // examples: CityObject::CityObjectsType::COT_Building | CityObject::CityObjectsType::COT_Room <- parses only Building and Room objects"

            var attrMap = cityObj.AttributesMap;
            // Debug.Log(attrMap); // PLATEAU.CityInfo.CityObjectList+Attributes
            // 3D都市モデル標準製品仕様書
            // https://www.mlit.go.jp/plateaudocument/#toc4
            // ex.
                // key: bldg:measuredheight, value: 7.8
                // key: bldg:usage, value: 411
                // key: gml:description, value: 1
                // key: uro:buildingDataQualityAttribute, value: {
                //     key: uro:lod1HeightType, value: 点群から取得_中央値
                // }
                // key: uro:buildingDetailAttribute, value: {
                //     key: uro:surveyYear, value: 2016
                // }
                // key: uro:buildingDisasterRiskAttribute, value: {
                //     key: uro:adminType, value: 国
                //     key: uro:depth, value: 0.05
                //     key: uro:duration, value: 0
                //     key: uro:rankOrg, value: 0.5m未満
                //     key: uro:scale, value: L2（想定最大規模）
                // }
                // key: uro:buildingIDAttribute, value: {
                //     key: uro:buildingID, value: 11326-bldg-41203
                //     key: uro:city, value: 群馬県桐生市
                //     key: uro:prefecture, value: 群馬県
                // }

            foreach (var pair in attrMap)
            {
                dictInfo.Add(pair.Key, pair.Value.StringValue);
                // 構造物使用目的
                if (pair.Key == "bldg:usage")
                {
                    string buildingusage = GetBuildingUsage(pair.Value.StringValue);
                    // Debug.Log($"buildingusage: {buildingusage}");
                    dictInfo.Add("bldg:usagestr", buildingusage);
                }
                if (pair.Key == "bldg:measuredheight")
                {
                    float height = float.Parse(pair.Value.StringValue);
                    int floors = Mathf.FloorToInt(height / _BLDG_HEIGHT_PAR_FLOOR);
                    dictInfo.Add("bldg:floors", floors.ToString());
                    dictInfo.Add("bldg:floor", floor.ToString());
                    int totalArea =  Mathf.FloorToInt(floor * floors);
                    dictInfo.Add("bldg:totalarea", totalArea.ToString());
                    int person = CalcPersonFromFloor(totalArea);
                    dictInfo.Add("bldg:person", person.ToString());
                }
                foreach (var pair2 in pair.Value.AttributesMapValue)
                {
                    // Debug.Log($"  key2: {pair2.Key}, value: {pair2.Value.StringValue}");
                    dictInfo.Add(pair2.Key, pair2.Value.StringValue);
                }
            }

        }
        // Debug.Log(dictInfo);
        return dictInfo;
    }

    // 居住人数の推定
    private int CalcPersonFromFloor(int totalArea)
    {
        // TODO:建築物の使用目的、郊外型、都市型から推定再計算

        // cf.住生活基本計画における居住面積水準
        // 居住面積水準[最低] 10m^2*世帯人数+10m^2
        // 居住面積水準[誘導/都市] 20m^2*世帯人数+15m^2
        // 居住面積水準[誘導/一般] 25m^2*世帯人数+25m^2
        // https://www.mhlw.go.jp/stf/shingi/2r98520000012t0i-att/2r98520000012t75.pdf

        float x = (totalArea - _BLDG_CORRIDOR_PAR_ROOM) / _BLDG_FLOOR_PAR_PERSON;
        int person = Mathf.FloorToInt(x);

        // 小さすぎる建物はマイナスになるので0にする
        if (person < 0)
        {
            person = 0;
        }

        // int person = Mathf.FloorToInt(totalArea / _BLDG_FLOOR_PAR_PERSON);
        return person;
    }

    private GameObject GetTargetObject()
    {
        Vector2 mousePosision = Mouse.current.position.ReadValue();
        // Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosision);

        // UIをraycastが貫通しないようにする
        // TODO:canvasを貫通してしまう
        // if (EventSystem.current.IsPointerOverGameObject() || EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Debug.Log("UI");
            return null;
        }

        Ray PointRay = Camera.main.ScreenPointToRay(mousePosision);
        RaycastHit hit;
        if (Physics.Raycast(PointRay, out hit, _CLICK_LIMIT_DISTANCE))
        {
            // Debug.Log("hit.collider " + hit.collider.gameObject.name);
            // SetMaterialColor(hit.collider.gameObject, Color.green);
            return hit.collider.gameObject;
        }
        return null;
    }

    private void SetMaterialColor(GameObject targetObj, Color color)
    {
        Renderer renderer = targetObj.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }
        if (renderer.materials.Length > 1)
        {
            // Debug.Log("material 複数パターン");
            return;
        }
        renderer.material.color = color;
    }

    private void SetCube(Vector3 setPosition, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = setPosition;
        SetMaterialColor(cube, color);
        cube.AddComponent<Rigidbody>();
        cube.GetComponent<Rigidbody>().useGravity = true;
        cube.tag = GameEnum.TagType.Garbage.ToString();
    }

    void GetMeshrenderInfo(GameObject targetObj, float height = _CENTER_Y_OFFSET)
    {
        MeshFilter meshFilter = targetObj.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Renderer renderer = targetObj.GetComponent<Renderer>();
            // Debug.Log(renderer.bounds);
            SetCubeMark(renderer);

            // CreateCubeRoundByAngle(renderer.bounds.center, renderer.bounds.extents, 16);
            CreateCubeRoundByArc(renderer.bounds.center, renderer.bounds.extents, 4);
        }

        // MeshColliderのbounds.center
        // geoReferenceのUnproject
    }

    private void SetCubeMark(Renderer renderer)
    {
        Vector3 center = renderer.bounds.center;
        SetCubeAtCenter(center, Color.blue);
        SetCubeAtCorner(renderer);
    }

    private void SetCubeAtCorner(Renderer renderer)
    {
        Vector3 center = renderer.bounds.center;
        // 四隅をポイント
        Vector3 extents1 = renderer.bounds.extents;
        extents1 = center + extents1;
        extents1.y = center.y;
        Vector3 extents2 = renderer.bounds.extents;
        extents2 = center - extents2;
        extents2.y = center.y;
        Vector3 extents3 = renderer.bounds.extents;
        extents3.x = center.x + extents3.x;
        extents3.z = center.z - extents3.z;
        extents3.y = center.y;
        Vector3 extents4 = renderer.bounds.extents;
        extents4.x = center.x - extents4.x;
        extents4.z = center.z + extents4.z;
        extents4.y = center.y;

        SetCube(extents1, Color.black);
        SetCube(extents2, Color.cyan);
        SetCube(extents3, Color.magenta);
        SetCube(extents4, Color.yellow);
    }

    // centerをポイント
    private void SetCubeAtCenter(Vector3 center, Color color, float height = _CENTER_Y_OFFSET)
    {
        center.y += height  * 0.5f + _BUFFER_Y_OFFSET;
        SetCube(center, color);
    }

    private float GetSupFloorSpace(Vector3 extents)
    {
        float x = extents.x;
        float z = extents.z;
        float ret = x * z;
        // ret 小数点以下切り捨て
        ret = Mathf.Floor(ret);
        return ret;
    }

    private float GetRadius(Vector3 extents)
    {
        float x = extents.x;
        float z = extents.z;
        float r = Mathf.Sqrt(x * x + z * z);
        return r;
    }

    private void CreateCubeRoundByArc(Vector3 center, Vector3 extents, int interval)
    {
        float r = GetRadius(extents);
        float arc = 2.0f * Mathf.PI * r;
        int step = Mathf.FloorToInt(arc / interval);
        CreateCubeRoundByAngle(center, extents, step);
    }

    private void CreateCubeRoundByAngle(Vector3 center, Vector3 extents, int step)
    {
        float r = GetRadius(extents);
        float angle = 360.0f / step;
        float radian = angle * Mathf.Deg2Rad;
        CreateCubeRound(center, r, radian, step);
    }

    private void CreateCubeRound(Vector3 center, float r, float radian, int step)
    {
        for (int i = 0; i < step; i++)
        {
            float x1 = r * Mathf.Cos(radian * i);
            float z1 = r * Mathf.Sin(radian * i);
            // Debug.Log($"x1: {x1}, z1: {z1}");
            // Vector3 pos = new Vector3(x1, center.y, z1);
            Vector3 pos = new Vector3(center.x + x1, center.y + _BUFFER_Y_OFFSET, center.z + z1);
            SetCube(pos, Color.white);
            // SetBonFire(pos);
        }
    }

    void OnDestroy()
    {
        Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        foreach (var dict in _dictBuildingMaterial) {
            GameObjectTreat.DestroyMaterialsAll(dict.Value);
            // foreach (var material in dict.Value)
            // {
            //     Destroy(material);
            // }
        }        
        _dictBuildingMaterial.Clear();

        if (instance == this)
        {
            instance = null;
        }

    }

    void Awake()
    {
        Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (instance == null)
        {
            instance = this;
        }
        // Debug.Log("PlateauInfo Awake");
        // _xmlParser = this.GetComponent<XMLparser>();
        // if (_xmlParser == null)
        // {
        //     _xmlParser = this.gameObject.AddComponent<XMLparser>();
        // }
    }

    void Start()
    {
        // ConvertLatLng();
        // GetMeshrenderInfo();
        // GetTargetInfo();

    }


    void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     GetTarget();
        // }
    }


}
