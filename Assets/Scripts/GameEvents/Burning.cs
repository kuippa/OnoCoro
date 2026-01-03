using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class Burning : MonoBehaviour
{
    // 
    // [serializeField] public int _burnningSize = 0;

    private List<string> _list_near_garbage = new List<string>();
    private const float _INTERVAL_TIME = 0.10f; // 0.x秒ごとに評価
    private const float _COUNT_TIME = 30.0f; // * _INTERVAL_TIME 時間で燃焼
    private float _t = 0f;
    private Dictionary<string, int> _dict_burn_count = new Dictionary<string, int>();
    private Dictionary<string, GameObject> _dict_burn_garbage = new Dictionary<string, GameObject>();


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
    }

    private void ChangeFireLv(int lv)
    {
        // int lv = this.gameObject.GetComponent<FireCube>()._unit_struct.Lv;
        Debug.Log("ChangeFireLv " + lv);
        if (lv > 2)
        {
            FireCubeCtrl.ChangeFireCubeSize(this.gameObject, FireCubeCtrl._SIZE_BIG);
        }
        else if (lv > 1)
        {
            FireCubeCtrl.ChangeFireCubeSize(this.gameObject, FireCubeCtrl._SIZE_MEDIUM);
        }
        else
        {
        //     FireCubeCtrl.ChangeFireCubeSize(this.gameObject, FireCubeCtrl._SIZE_NORMAL);
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
                _list_near_garbage = _list_near_garbage.Where(x => x != name).ToList();
                if (_dict_burn_garbage.ContainsKey(name))
                {
                    _dict_burn_garbage[name].GetComponent<Renderer>().material.color = Color.black;
                    _dict_burn_garbage[name].tag = GameEnum.TagType.Ash.ToString();
                    _dict_burn_garbage.Remove(name);
                }
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
        if (_list_near_garbage.Count > 0)
        {
            foreach (string name in _list_near_garbage)
            {
                CountUpBurning(name);
            }
        }
    }

    void Awake()
    {
        // Debug.Log("Burnning Awake");
    }

    void Update()
    {
        // 一定間隔で子供要素を生成
        _t += Time.deltaTime;        
        if (_t > _INTERVAL_TIME)
        {
            _t = 0;
            CheckNearGarbage();
        }


    }
}
