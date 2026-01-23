using System;
using UnityEngine;

/// <summary>
/// 地震イベントを管理するクラス
/// 地面(DEM)を振動させることで地震を表現する
/// </summary>
public class Earthquake : MonoBehaviour
{
    private GameObject _dem = null;
    private GameObject _bldg = null;
    private float _time = 0.0f;
    private float _time_duration = 0.0f;
    private Vector3 _org_stage_vector;
    private Vector3 _org_bldg_vector;
    private float _degree = 0.0f;
    private bool _is_earthquake = false;
    private float _total_duration = 3.5f;
    private float _interval = 0.02f;
    private float _magnitude = 1.0f;

    private const float _DURATION = 4.5f;
    private const float _INTERVAL = 0.02f;
    private const string _BLDG_IDENTIFIER = "_bldg_";  // 建物オブジェクト名の識別子

    /// <summary>
    /// 地震イベントを開始する
    /// </summary>
    /// <param name="magnitude">マグニチュード(振幅の大きさ)</param>
    /// <param name="duration">地震の継続時間(秒)</param>
    /// <param name="interval">振動の更新間隔(秒)</param>
    internal void EventEarthQuake(float magnitude, float duration = _DURATION, float interval = _INTERVAL)
    {
        _is_earthquake = true;
        _interval = interval;
        _total_duration = duration;
        _magnitude = magnitude;
        _degree = 0.0f;
        // Debug.Log("EventEarthQuake " + _degree);
    }

    /// <summary>
    /// P波(縦波)による振動を計算
    /// 上下方向に地面を振動させる
    /// </summary>
    private void QualeP()
    {
        float val = CalcSin();
        if (_total_duration > _time_duration)
        {
            // 地面を揺らす
            if (_dem != null)
            {
                _dem.transform.position = new Vector3(_org_stage_vector.x, _org_stage_vector.y + val, _org_stage_vector.z);
            }

            // 建物も同時に揺らす
            if (_bldg != null)
            {
                _bldg.transform.position = new Vector3(_org_bldg_vector.x, _org_bldg_vector.y + val, _org_bldg_vector.z);
            }

            _degree += 30.0f;
        }
        else
        {
            // 地面を元の位置に戻す
            if (_dem != null)
            {
                _dem.transform.position = _org_stage_vector;
            }

            // 建物を元の位置に戻す
            if (_bldg != null)
            {
                _bldg.transform.position = _org_bldg_vector;
            }

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


    /// <summary>
    /// Sin波を計算して振幅を返す
    /// </summary>
    /// <returns>減衰を考慮した振幅値</returns>
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

    /// <summary>
    /// 振幅の減衰を計算
    /// 地震開始時と終了時で振幅が小さくなるように計算
    /// </summary>
    /// <returns>減衰係数</returns>
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


    private void Awake()
    {
        // TODO:Naraku の demのところを別クラス化

        GameObject ground = GameObject.FindGameObjectWithTag("Ground");
        if (ground != null)
        {
            _dem = ground.gameObject;
            _org_stage_vector = _dem.transform.position;

            // _dem.transform.parent.parent.parent が建物オブジェクトの親
            if (_dem.transform.parent != null && 
                _dem.transform.parent.parent != null && 
                _dem.transform.parent.parent.parent != null)
            {
                _bldg = FindBuildingObjectInParent(_dem.transform.parent.parent.parent);
                if (_bldg != null)
                {
                    _org_bldg_vector = _bldg.transform.position;
                    Debug.Log($"Found building object for earthquake vibration: {_bldg.name}");
                }
                else
                {
                    Debug.LogWarning("Could not find building object containing '_bldg_' in DEM parent hierarchy");
                }
            }
            else
            {
                Debug.LogWarning("DEM parent hierarchy is incomplete (expected 3 levels deep)");
            }
        }
        else
        {
            Debug.Log("Earthquake Awake ground is null");
            return;
        }

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

    /// <summary>
    /// 親オブジェクトのチャイルドから、_bldg_を含む建物オブジェクトを探す
    /// </summary>
    /// <param name="parentTransform">親Transform</param>
    /// <returns>見つかった建物GameObject、見つからない場合はnull</returns>
    private GameObject FindBuildingObjectInParent(Transform parentTransform)
    {
        if (parentTransform == null)
        {
            return null;
        }

        foreach (Transform child in parentTransform)
        {
            if (child.name.Contains(_BLDG_IDENTIFIER))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
        {
            Debug.Log($"OnTriggerEnter {other.name} object:{other.gameObject.name}");

            // TODO:
            // マグニチュード2.5以上で、キャラなどが床抜けする

            // EventEarthQuake(0.6f);
            EventEarthQuake(1.8f);
            // EventEarthQuake(2.8f);
        }
    }


    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
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
