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

    private float _time = 0.0f;

    private GameObject _eventSystem = null;

    private static GameObject _parent_holder = null;

    private const string _RAIN_PARENT_NAME = "Rains";

    internal const float INTERVAL_RAIN = 0.02f;

    internal const float ABOVE_POSITION = 280f;

    private const float RAINY_STRENGTH = 0.65f;    // 雨の強さ
    private const float RAINY_CLOUD_STRENGTH = 0.75f;    // 雨のときの雲の強さ
    private const float SUNNY_CLOUD_STRENGTH = 0.25f;    // 晴れのときの雲の強さ
    private const float RAINY_FOG_STRENGTH = 200f;   // 雨のときの霧の強さ
    // private const float RAINY_FOG_STRENGTH = 40f;   // 雨のときの霧の強さ
    private const float SUNNY_FOG_STRENGTH = 500f;
    // private const float INTERVAL_RAIN = 0.01f;  // 雨粒が落ちてくる間隔
    // private const float INTERVAL_RAIN = 0.05f;  // 雨粒が落ちてくる間隔
    // private const float INTERVAL_RAIN = 1.00f;  // 雨粒が落ちてくる間隔
    private GameObject _rain_holder = null;


    internal void SetRainMode(bool isRain)
    {
        if (isRain)
        {
            _is_rain = true;
            return;
        }
        _is_rain = false;
        DeleteAllRain();
        DeleteAllPuddle();
    }


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
        GameObject rainDropPrefab = PrefabManager.RainDropPrefab;
        Vector3 demRndAbovePosition = DemCtrl.GetDemRndAbovePosition(ABOVE_POSITION);
        Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
        GameObject obj = Object.Instantiate(rainDropPrefab, demRndAbovePosition, rotation);
        obj.tag = GameEnum.TagType.RainDrop.ToString();
        obj.name = GameEnum.TagType.RainDrop.ToString() + Time.time;
        Transform holderParentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_holder, _RAIN_PARENT_NAME);
        obj.transform.SetParent(holderParentTransform);
    }


    internal void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
        {
            _eventSystem = GameObjectTreat.GetEventSystem(_eventSystem);
            WeatherCtrl orAddComponent = GameObjectTreat.GetOrAddComponent<WeatherCtrl>(_eventSystem);
            float toggleRainStrength = orAddComponent.GetToggleRainStrength();
            orAddComponent.ChangeWeather(toggleRainStrength);
        }
    }

    private void Awake()
    {
    }

    private void Update()
    {
        if (_is_rain)
        {
            float num = INTERVAL_RAIN / GameSpeedCtrl.GetGameSpeed();
            _time += Time.deltaTime;
            if (_time > num)
            {
                _time = 0f;
                RainDrops();
            }
        }
    }

}
