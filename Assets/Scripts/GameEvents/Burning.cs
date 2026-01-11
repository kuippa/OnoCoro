using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonsUtility;
using UnityEngine;

public class Burning : MonoBehaviour
{
    private List<string> _list_near_garbage = new List<string>();

    private const float _INTERVAL_TIME = 0.1f;
    private const float _COUNT_TIME = 30f;
    private const int _FIRE_SPREAD = 100;
    private const int _MAX_NEAR_FLAME = 5;

    private const int _BURNING_LV_MIN = 1;
    private const int _BURNING_LV_MID = 2;
    private const int _BURNING_LV_MAX = 3;

    private float _t;
    private int _tAsh;
    private float _damage_buffer;

    private Dictionary<string, int> _dict_burn_count = new Dictionary<string, int>();
    private Dictionary<string, GameObject> _dict_burn_garbage = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _dict_ash = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _dict_burn_bldg = new Dictionary<string, GameObject>();


    internal void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == GameEnum.TagType.Garbage.ToString() && !_list_near_garbage.Contains(other.gameObject.name))
        {
            _list_near_garbage = _list_near_garbage.Where((string x) => x != other.gameObject.name).ToList();
        }
    }

    internal void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == GameEnum.TagType.Garbage.ToString())
        {
            if (!_list_near_garbage.Contains(other.gameObject.name))
            {
                _list_near_garbage.Add(other.gameObject.name);
                _dict_burn_garbage.Add(other.gameObject.name, other.gameObject);
            }
        }
        else if (other.gameObject.tag == GameEnum.TagType.FireCube.ToString())
        {
            if (other.gameObject.GetComponent<FireCube>()._unit_struct.Lv <= base.gameObject.GetComponent<FireCube>()._unit_struct.Lv)
            {
                base.gameObject.GetComponent<FireCube>()._unit_struct.Lv += other.gameObject.GetComponent<FireCube>()._unit_struct.Lv;
                ChangeFireLv(base.gameObject.GetComponent<FireCube>()._unit_struct.Lv);
                GameObjectTreat.DestroyAll(other.gameObject);
            }
        }
        else if (other.gameObject.tag == GameEnum.TagType.Untagged.ToString())
        {
            if (IsPlateauBuilding(other) && !_dict_burn_bldg.ContainsKey(other.gameObject.name))
            {
                _dict_burn_bldg.Add(other.gameObject.name, other.gameObject);
            }
        }
        else if (other.gameObject.tag == GameEnum.TagType.Water.ToString())
        {
            _damage_buffer += 0.2f;
            if (_damage_buffer > 1f)
            {
                _damage_buffer = 0f;
                base.gameObject.GetComponent<FireCube>()._unit_struct.Lv--;
                ChangeFireLv(base.gameObject.GetComponent<FireCube>()._unit_struct.Lv);
            }
        }
    }

    private bool IsPlateauBuilding(Collider obj)
    {
        return PlateauUtility.IsPlateauBuilding(obj);
    }



    private void BurnDownBuilding(GameObject obj = null)
    {
        GameObject obj2 = GameObject.Find("Plateau");
        PlateauInfoManager component = obj2.GetComponent<PlateauInfoManager>();
        if (obj2 == null || component == null)
        {
            return;
        }
        if (obj == null)
        {
            obj = _dict_burn_bldg.Where((KeyValuePair<string, GameObject> x) => x.Value != null).ToDictionary((KeyValuePair<string, GameObject> x) => x.Key, (KeyValuePair<string, GameObject> x) => x.Value).FirstOrDefault()
                .Value;
        }
        if (!(obj == null) && obj.activeSelf)
        {
            component.SetBuildingToDoom(obj);
        }
    }

    private void ChangeFireLv(int lv)
    {
        if (lv >= 3)
        {
            FireCubeCtrl.ChangeFireCubeSize(base.gameObject, 2);
        }
        else if (lv >= 2)
        {
            FireCubeCtrl.ChangeFireCubeSize(base.gameObject, 1);
        }
        else if (lv == 1)
        {
            FireCubeCtrl.ChangeFireCubeSize(base.gameObject, 0);
        }
        else if (lv < 1)
        {
            GameObjectTreat.DestroyAll(base.gameObject);
        }
    }

    private void Explode_Ash(GameObject target_ash)
    {
        if (!(target_ash == null))
        {
            target_ash.GetComponent<Renderer>().material.color = Color.red;
            Rigidbody rigidbody = target_ash.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = target_ash.AddComponent<Rigidbody>();
            }
            Vector3 normalized = (target_ash.transform.position - base.gameObject.transform.position).normalized;
            normalized += GetWindDirectionVector();
            normalized.y += 0.45f;
            float windSpeed = GetWindSpeed();
            rigidbody.AddForce(normalized * windSpeed, ForceMode.Impulse);
            _dict_ash.Remove(target_ash.name);
            StartCoroutine(InvokeWithGameObject(target_ash, 2f));
        }
    }

    private Vector3 GetWindDirectionVector()
    {
        return WindCtrl.GetWindDirectionVector();
    }

    private float GetWindSpeed()
    {
        return WindCtrl.GetWindSpeed();
    }

    private IEnumerator InvokeWithGameObject(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!(obj == null))
        {
            AshToFire(obj);
        }
    }

    private void AshToFire(GameObject target_ash)
    {
        Vector3 position = target_ash.transform.position;
        GameObject gameManagerObject = GameObjectTreat.GetGameManagerObject();
        FireCubeCtrl fireCubeCtrl = gameManagerObject.GetComponent<FireCubeCtrl>();
        if (fireCubeCtrl == null)
        {
            fireCubeCtrl = gameManagerObject.AddComponent<FireCubeCtrl>();
        }
        fireCubeCtrl.SpawnFireCubeAsync(position);
        GameObjectTreat.DestroyAll(target_ash);
    }

    private void GarbageToAsh(string name)
    {
        _list_near_garbage = _list_near_garbage.Where((string x) => x != name).ToList();
        if (_dict_burn_garbage.ContainsKey(name))
        {
            _dict_burn_garbage[name].GetComponent<Renderer>().material.color = Color.black;
            _dict_burn_garbage[name].tag = GameEnum.TagType.Ash.ToString();
            if (!_dict_ash.ContainsKey(name))
            {
                _dict_ash.Add(name, _dict_burn_garbage[name]);
            }
            _dict_burn_garbage.Remove(name);
        }
    }

    private float GetCountTime()
    {
        return 30f / GameSpeedCtrl.GetGameSpeed();
    }

    private float GetFireSpreadTime()
    {
        return 100f / GameSpeedCtrl.GetGameSpeed();
    }

    private void CountUpBurning(string name)
    {
        if (_dict_burn_count.ContainsKey(name))
        {
            _dict_burn_count[name]++;
            if ((float)_dict_burn_count[name] > GetCountTime())
            {
                GarbageToAsh(name);
                base.gameObject.GetComponent<FireCube>()._unit_struct.Lv++;
                ChangeFireLv(base.gameObject.GetComponent<FireCube>()._unit_struct.Lv);
                _dict_burn_count.Remove(name);
            }
        }
        else
        {
            _dict_burn_count.Add(name, 1);
        }
    } 

    private void CheckNearGarbage()
    {
        if (_list_near_garbage.Count <= 0 || _dict_burn_garbage.Count <= 0)
        {
            return;
        }
        float num = 10f;
        GameObject gameObject = null;
        string text = "";
        foreach (string item in _list_near_garbage)
        {
            if (!_dict_burn_garbage.ContainsKey(item))
            {
                continue;
            }
            gameObject = _dict_burn_garbage[item];
            if (gameObject == null)
            {
                _dict_burn_garbage.Remove(item);
                _dict_burn_count.Remove(item);
                continue;
            }
            float num2 = Vector3.Distance(base.gameObject.transform.position, gameObject.transform.position);
            if (num2 < num)
            {
                num = num2;
                text = item;
            }
        }
        CountUpBurning(text);
    }

    private void CheckNearAsh()
    {
        if (_dict_ash.Count <= 0 || base.gameObject.GetComponent<FireCube>()._unit_struct.Lv < 3)
        {
            return;
        }
        GameObject[] array = GameObject.FindGameObjectsWithTag(GameEnum.TagType.FireCube.ToString());
        if (array.Length == 0)
        {
            return;
        }
        float num = 10f;
        int num2 = 0;
        GameObject[] array2 = array;
        foreach (GameObject gameObject in array2)
        {
            if (Vector3.Distance(base.gameObject.transform.position, gameObject.transform.position) < num)
            {
                num2++;
            }
        }
        if (num2 >= 5)
        {
            return;
        }
        float num3 = 8f;
        float num4 = 0f;
        GameObject target_ash = null;
        List<string> list = new List<string>();
        foreach (string key in _dict_ash.Keys)
        {
            if (_dict_ash[key] == null)
            {
                list.Add(key);
                continue;
            }
            float num5 = Vector3.Distance(base.gameObject.transform.position, _dict_ash[key].transform.position);
            if (num5 > num4 && num5 < num3)
            {
                num4 = num5;
                target_ash = _dict_ash[key];
            }
        }
        if (list.Count > 0)
        {
            foreach (string item in list)
            {
                _dict_ash.Remove(item);
            }
        }
        Explode_Ash(target_ash);
    }

    private void Awake()
    {
    }

    private void Update()
    {
        _t += Time.deltaTime;
        if (_t > 0.1f)
        {
            _t = 0f;
            CheckNearGarbage();
            _tAsh++;
            if ((float)_tAsh > GetFireSpreadTime())
            {
                _tAsh = 0;
                CheckNearAsh();
                BurnDownBuilding();
            }
        }
    }
}
