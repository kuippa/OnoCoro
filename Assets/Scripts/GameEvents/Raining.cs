using UnityEngine;
using PlateauToolkit.Rendering;
using System.Linq;
using CommonsUtility;

// using Microsoft.Win32.SafeHandles;
// using System.Numerics; // PlateauToolkit.Rendering  Environment を利用のために追加

// downpour 土砂降り
public class Raining : MonoBehaviour
{
    // TODO: ステージ内で一意の状態であるようにする
    private bool _is_rain = false; 

    private const float RAINY_STRENGTH = 0.65f;    // 雨の強さ
    private const float RAINY_CLOUD_STRENGTH = 0.75f;    // 雨のときの雲の強さ
    private const float SUNNY_CLOUD_STRENGTH = 0.25f;    // 晴れのときの雲の強さ
    private const float RAINY_FOG_STRENGTH = 200f;   // 雨のときの霧の強さ
    // private const float RAINY_FOG_STRENGTH = 40f;   // 雨のときの霧の強さ
    private const float SUNNY_FOG_STRENGTH = 500f;
    private const float INTERVAL_RAIN = 0.01f;  // 雨粒が落ちてくる間隔
    // private const float INTERVAL_RAIN = 0.05f;  // 雨粒が落ちてくる間隔
    // private const float INTERVAL_RAIN = 1.00f;  // 雨粒が落ちてくる間隔
    private float _time = 0.0f;
    private GameObject _rain_holder = null;


    internal void ToggleRain()
    {
        EnvironmentController env = GameObject.Find("Environment").GetComponent<EnvironmentController>();

        if (env.m_Rain > 0.0f)
        {
            env.m_Rain = 0.0f;
            env.m_Cloud = SUNNY_CLOUD_STRENGTH;
            env.m_FogDistance = SUNNY_FOG_STRENGTH;
            _is_rain = false;
            DeleteAllRain();
            DeleteAllPuddle();
        }
        else
        {
            env.m_Rain = RAINY_STRENGTH;
            env.m_Cloud = RAINY_CLOUD_STRENGTH;
            env.m_FogDistance = RAINY_FOG_STRENGTH;
            _is_rain = true;
        }

    }

    private void DeleteAllRain()
    {
        GameObject[] rainDrops = GameObject.FindGameObjectsWithTag(GameEnum.TagType.RainDrop.ToString());
        foreach (GameObject rainDrop in rainDrops)
        {
            GameObjectTreat.DestroyAll(rainDrop);
        }
    }

    private void DeleteAllPuddle()
    {
        GameObject[] puddles = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Puddle.ToString());
        foreach (GameObject puddle in puddles)
        {
            GameObjectTreat.DestroyAll(puddle);
        }
    }

    private void RainDrops()
    {
		GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/RainDrop");
        // prefab.transform.localScale = GetRandomSize(_LitterSizeMin, _LitterSizeMax);
        // Vector3 setPoint = this.transform.position;
        Vector3 setPoint = DemCtrl.GetDemRndAbovePosition(50f);
        // Vector3 setPoint = new UnityEngine.Vector3(-251, 48, -71);

        // Quaternion setRotation = Quaternion.Euler(rdNum(0,360), rdNum(0,360), rdNum(0,360));
        Quaternion setRotation = Quaternion.Euler(0, 0, 0);
        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        unit.tag = GameEnum.TagType.RainDrop.ToString();
        unit.name = GameEnum.TagType.RainDrop.ToString() + Time.time.ToString();

        // Rigidbody rigidbody = unit.GetComponent<Rigidbody>();
        // rigidbody.useGravity = true;
        // Rigidbody rb        = GetComponent<Rigidbody>();
        // Rigidbody rigidbody = unit.AddComponent<Rigidbody>();
        // rigidbody.AddForce(transform.forward * _LitteringPow);

        unit.transform.SetParent(GetRainHolder().transform);
    }

    private GameObject GetRainHolder()
    {
        if (_rain_holder == null)
        {
            _rain_holder = GameObject.Find("rains");
            if (_rain_holder == null)
            {
                // gameObject rains を追加する
                _rain_holder = new GameObject("rains");
            }
        }
        return _rain_holder;
    }


    internal void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
        {
            // Debug.Log("OnTriggerEnter " + other.name + " object:" + other.gameObject.name);
            ToggleRain();
            return;
        }
    }

    void Awake()
    {
        // _rain = GameObject.Find("Environment/Rain").gameObject;
    }

    void Update()
    {
        if (!_is_rain)
        {
            return;
        }

        _time += Time.deltaTime;
        if (_time > INTERVAL_RAIN)
        {
            _time = 0.0f;
            RainDrops();
        }

    }

}
