using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Earthquake : MonoBehaviour
{
    private GameObject _dem = null;
    private float _time = 0.0f;
    private float _time_duration = 0.0f;
    private Vector3 _org_stage_vector;
    private float _degree = 0.0f;
    private bool _is_earthquake = false;
    private float _total_duration = 3.5f;
    private float _interval = 0.02f;
    private float _magnitude = 1.0f;

    private const float _DURATION = 4.5f;
    private const float _INTERVAL = 0.02f;

    internal void EventEarthQuake(float magnitude, float duration = _DURATION ,float interval = _INTERVAL)
    {
        _is_earthquake = true;
        _interval = interval;
        _total_duration = duration;
        _magnitude = magnitude;
        _degree = 0.0f;
        // Debug.Log("EventEarthQuake " + _degree);
    }

    // P波
    private void QualeP()
    {
        float val = CalcSin();
        if (_total_duration > _time_duration)
        {
            _dem.transform.position = new Vector3(_org_stage_vector.x, _org_stage_vector.y + val, _org_stage_vector.z);
            _degree += 30.0f;
        }
        else
        {
            // Debug.Log("QualeP end");
            _dem.transform.position = _org_stage_vector;
            _degree = 0.0f;
            _time_duration = 0.0f;
            _is_earthquake = false;
        }
    }


    // S波
    // 水平方向振動は摩擦係数が乗らないので見た目上機能しない
    // private void QuakeS()
    // {
    //     float val = CalcSin();
    //     if (_total_duration > _time_duration)
    //     {
    //         _dem.transform.position = new Vector3(_org_stage_vector.x + val, _org_stage_vector.y, _org_stage_vector.z + val);
    //         _degree += 30.0f;
    //     }
    //     else
    //     {
    //         Debug.Log("QualeP end");
    //         _dem.transform.position = _org_stage_vector;
    //         _degree = 0.0f;
    //         _time_duration = 0.0f;
    //         _is_earthquake = false;
    //     }
    // }


    private float CalcSin()
    {
        float ret = 0.0f;
        float val = 0.0f;
        val = (float)(_degree * Math.PI / 180.0f);
        ret = (float)Math.Sin(val);
        ret = CalcAmpDecay() * ret;
        // Debug.Log("CalcSin " + _degree + " val:" + val + " sin:" + Math.Sin(val) + " ret:" + ret);
        return ret;
    }

    private float CalcAmpDecay()
    {
        float ret = 0.0f;
        float sign = 1f;
        if (_time_duration > _total_duration/2)
        {
            sign = -1f;
            // return ret;
        }
        if (_time_duration != 0)
        {
            ret = sign * _magnitude * (float)Math.Pow(_time_duration/_total_duration, 2);
        }
        return ret;
    }


    void Awake()
    {
        // TODO:Naraku の demのところを別クラス化

        GameObject ground = GameObject.FindGameObjectWithTag("Ground");
        if (ground != null)
        {
            _dem = ground.gameObject;
        }
        else
        {
            Debug.Log("Earthquake Awake ground is null");
            return;
        }
        _org_stage_vector = _dem.transform.position;

        // Debug.Log(
        //   " 0:" + Math.Sin(0 * Math.PI / 180.0f) 
        // + " 45:" + Math.Sin(45 * Math.PI / 180.0f)
        // + " 90:" + Math.Sin(90 * Math.PI / 180.0f)
        // + " 135:" + Math.Sin(135 * Math.PI / 180.0f)
        // + " 180:" + Math.Sin(180 * Math.PI / 180.0f)
        // + " 225:" + Math.Sin(225 * Math.PI / 180.0f)
        // + " 270:" + Math.Sin(270 * Math.PI / 180.0f)
        // + " 315:" + Math.Sin(315 * Math.PI / 180.0f)
        // + " 360:" + Math.Sin(360 * Math.PI / 180.0f)
        // + " 405:" + Math.Sin(405 * Math.PI / 180.0f)
        // );
    }

    internal void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
        {
            Debug.Log("OnTriggerEnter " + other.name + " object:" + other.gameObject.name);

            // TODO:
            // マグニチュード2.5以上で、キャラなどが床抜けする

            // EventEarthQuake(0.6f);
            EventEarthQuake(1.8f);
            // EventEarthQuake(2.8f);



            return;
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_is_earthquake)
        {
            _time += Time.deltaTime;
            _time_duration += Time.deltaTime;
            if (_time > _interval)
            {
                _time = 0.0f;
                QualeP();
                // QuakeS();
            }
        }
    }
}
