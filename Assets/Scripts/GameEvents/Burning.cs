using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommonsUtility;
using System.Runtime.InteropServices;
// using System.Diagnostics;
using PLATEAU.CityInfo;
using PLATEAU.Native;


public class Burning : MonoBehaviour
{
    // 
    // [serializeField] public int _burnningSize = 0;


    private List<string> _list_near_garbage = new List<string>();
    private const float _INTERVAL_TIME = 0.10f; // 0.x秒ごとに評価
    private const float _COUNT_TIME = 30.0f; // * _INTERVAL_TIME 時間で燃焼
    private const int _MAX_NEAR_FLAME = 6; // 

    private const int _BURNING_LV_MIN = 1; // 
    private const int _BURNING_LV_MID = 2; // 
    private const int _BURNING_LV_MAX = 3; // 

    private float _t = 0f;
    private int _tAsh = 0;
    private float _damage_buffer = 0f;
    private Dictionary<string, int> _dict_burn_count = new Dictionary<string, int>();
    private Dictionary<string, GameObject> _dict_burn_garbage = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _dict_ash = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _dict_burn_bldg = new Dictionary<string, GameObject>();


    internal void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == GameEnum.TagType.Garbage.ToString())
        {
            if (!_list_near_garbage.Contains(other.gameObject.name))
            {
                // _list_near_garbage.Remove(other.gameObject.name);
                _list_near_garbage = _list_near_garbage.Where(x => x != other.gameObject.name).ToList();
            }
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
        // Untagged は建物チェック
        else if (other.gameObject.tag == GameEnum.TagType.Untagged.ToString())
        {
            // Debug.Log("Burning OnTriggerEnter Untagged: " + other.gameObject.name);
            if (IsPlateauBuilding(other))
            {
                // Debug.Log("Burning OnTriggerEnter PlateauBuilding: " + other.gameObject.name);
                if (! _dict_burn_bldg.ContainsKey(other.gameObject.name))
                {
                    _dict_burn_bldg.Add(other.gameObject.name, other.gameObject);
                    return;
                }
                // BurnDownBuilding(other.gameObject);
            }
        }
        else if (other.gameObject.tag == GameEnum.TagType.Water.ToString())
        {
            _damage_buffer += 0.2f;
            if (_damage_buffer > 1.0f)
            {
                _damage_buffer = 0f;
                this.gameObject.GetComponent<FireCube>()._unit_struct.Lv -= 1;
                ChangeFireLv(this.gameObject.GetComponent<FireCube>()._unit_struct.Lv);
            }
        }
    }

    private bool IsPlateauBuilding(Collider obj)
    {

        if (obj.transform.parent != null)
        {
            if (obj.name.Contains("_LOD2_") && obj.name.Contains("bldg_"))
            {
                PLATEAUCityObjectGroup cityObjs = obj.gameObject.GetComponent<PLATEAUCityObjectGroup>();
                if (cityObjs == null) {
                    // Debug.Log("cityObjs null ");
                    return false;
                }
                return true;
            }
        }
        // Debug.Log("transform.parent null ");
        return false;
    }



    private void BurnDownBuilding(GameObject obj = null)
    {
        GameObject plateau = GameObject.Find("Plateau");
        PlateauInfoManager plateauInfo = plateau.GetComponent<PlateauInfoManager>();
        if (plateau == null || plateauInfo == null)
        {
            // Debug.Log("PlateauInfo is null");
            return;
        }
        if (obj == null)
        {
            // 一番近い _dict_burn_bldg を取得
            obj = _dict_burn_bldg.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value).FirstOrDefault().Value;
        }

        if (obj == null || obj.activeSelf == false)
        {
            // Debug.Log("PlateauInfo activeSelf");
            return;
        }
        // Debug.Log("PlateauInfo SetBuildingToDoom");
        plateauInfo.SetBuildingToDoom(obj);

    }

    private void ChangeFireLv(int lv)
    {
        // int lv = this.gameObject.GetComponent<FireCube>()._unit_struct.Lv;
        // Debug.Log("ChangeFireLv " + lv);
        if (lv >= _BURNING_LV_MAX)
        {
            FireCubeCtrl.ChangeFireCubeSize(this.gameObject, FireCubeCtrl._SIZE_BIG);
        }
        else if (lv >= _BURNING_LV_MID)
        {
            FireCubeCtrl.ChangeFireCubeSize(this.gameObject, FireCubeCtrl._SIZE_MEDIUM);
        }
        else if (lv == _BURNING_LV_MIN)
        {
            FireCubeCtrl.ChangeFireCubeSize(this.gameObject, FireCubeCtrl._SIZE_NORMAL);
        }
        else if (lv < _BURNING_LV_MIN)
        {
            // 消火
            GameObjectTreat.DestroyAll(this.gameObject);
            // FireCubeCtrl.ChangeFireCubeSize(this.gameObject, FireCubeCtrl._SIZE_SMALL);
        }
    }

    private void Explode_Ash(GameObject target_ash)
    {
        // 炭を吹き飛ばして炎症を拡大
        // Debug.Log("Explode_Ash");
        if (target_ash == null)
        {
            return;
        }

        target_ash.GetComponent<Renderer>().material.color = Color.red;
        Rigidbody rb = target_ash.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = target_ash.AddComponent<Rigidbody>();
        }
        Vector3 direction = (target_ash.transform.position - this.gameObject.transform.position).normalized;

        direction.y += 0.45f; // 少し放物線を描く
        float force = 5f; // 力の大きさを調整
        // rb.AddForce(direction * force, ForceMode.VelocityChange);
        rb.AddForce(direction * force, ForceMode.Impulse);
        _dict_ash.Remove(target_ash.name);     
        // Debug.Log("Explode_Ash " + target_ash.name + " " + _dict_ash.Count);

        // x秒後に炭を炎に変換
        StartCoroutine(InvokeWithGameObject(target_ash, 2f));

    }

    private IEnumerator InvokeWithGameObject(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj == null)
        {
            yield break;
        }
        AshToFire(obj);
    }

    private void AshToFire(GameObject target_ash)
    {
        // 炭を炎にする
        Vector3 setPoint = target_ash.transform.position;
        GameObject garbageObj = FireCubeCtrl.SpawnFireCube(setPoint, FireCubeCtrl._SIZE_NORMAL, false);
        if (garbageObj != null)
        {
            GameObjectTreat.DestroyAll(target_ash);
        }

    }

    private void GarbageToAsh(string name)
    {
        // ゴミを灰にする

        // 延焼対象ゴミから削除
        _list_near_garbage = _list_near_garbage.Where(x => x != name).ToList();
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

    private void CountUpBurning(string name)
    {
 
        if (_dict_burn_count.ContainsKey(name))
        {
            _dict_burn_count[name] += 1;
            if (_dict_burn_count[name] > _COUNT_TIME)
            {
                // Debug.Log(name + " is burning");
                GarbageToAsh(name);
                this.gameObject.GetComponent<FireCube>()._unit_struct.Lv += 1;
                ChangeFireLv(this.gameObject.GetComponent<FireCube>()._unit_struct.Lv);
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
        // 
        if (_list_near_garbage.Count > 0 && _dict_burn_garbage.Count > 0)
        {
            // 周辺のゴミをいっぺんに燃やす
            // foreach (string name in _list_near_garbage)
            // {
            //     CountUpBurning(name);
            // }

            // 周辺のゴミを近いものから燃やす
            float minDistance = 10f;
            GameObject target = null;
            string targetName = "";
            foreach (string name in _list_near_garbage)
            {
                if (_dict_burn_garbage.ContainsKey(name))
                {
                    target = _dict_burn_garbage[name];
                    if (target == null)
                    {
                        // Debug.Log("target is null");
                        // 他の処理によりゴミがすでに削除されている場合
                        _dict_burn_garbage.Remove(name);
                        _dict_burn_count.Remove(name);
                        continue;
                    }

                    float distance = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        targetName = name;
                    }
                }
            }
            CountUpBurning(targetName);
        }
    }

    private void CheckNearAsh()
    {
        if (_dict_ash.Count > 0 && this.gameObject.GetComponent<FireCube>()._unit_struct.Lv >= _BURNING_LV_MAX)
        {
            // 周辺の他の火災の数を取得
            GameObject[] targets = GameObject.FindGameObjectsWithTag(GameEnum.TagType.FireCube.ToString());
            if (targets.Length == 0)
            {
                // 火災がない場合は延焼しない
                return;
            }

            float minDistance = 10f;
            int cntFire = 0;
            foreach (GameObject target in targets)
            {
                float distance = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
                if (distance < minDistance)
                {
                    cntFire += 1;
                }
            }
            if (cntFire >= _MAX_NEAR_FLAME)
            {
                // 規定圏内近隣に炎がx個以上ある場合は延焼しない
                // Debug.Log("Too many FireCube");
                return;
            }

            // 一番遠い灰を一つ取得
            float limitDistance = 8f;
            float maxDistance = 0f;
            GameObject target_ash = null;
            
            List<string> list_remove = new List<string>();
            foreach (string name in _dict_ash.Keys)
            {
                if (_dict_ash[name] == null)
                {
                    list_remove.Add(name);
                    continue;
                }
                float distance = Vector3.Distance(this.gameObject.transform.position, _dict_ash[name].transform.position);
                if (distance > maxDistance && distance < limitDistance)
                {
                    maxDistance = distance;
                    target_ash = _dict_ash[name];
                }
            }
            if (list_remove.Count > 0)
            {
                foreach (string name in list_remove)
                {
                    _dict_ash.Remove(name);
                }
            }


            Explode_Ash(target_ash);
        }
    }

    void Awake()
    {
        // Debug.Log("Burnning Awake");
    }

    void Update()
    {
        _t += Time.deltaTime;        
        if (_t > _INTERVAL_TIME)
        {
            _t = 0;
            CheckNearGarbage();
            _tAsh += 1;
            if (_tAsh > 100)
            {
                _tAsh = 0;
                CheckNearAsh();

                BurnDownBuilding(); // 建物炎症チェック
            }
        }


    }
}
