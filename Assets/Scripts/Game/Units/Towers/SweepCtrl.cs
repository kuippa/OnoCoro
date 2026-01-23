using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using UnityEngine.InputSystem.XInput;

public class SweepCtrl : MonoBehaviour
{
    private float _lastTriggerStayTime;
    private const float _TRIGGER_STAY_INTERVAL = 0.02f;     // この感覚が長すぎるとゴミを消せずに残る

    private void ChangeCubeSize(Collider other)
    {
        Vector3 scale = other.transform.localScale;
        other.transform.localScale = ChangeScaleLimiter(scale);
    }

    private Vector3 ChangeScaleLimiter(Vector3 scale)
    {
        scale.x = CalcScale(scale.x);
        scale.y = CalcScale(scale.y);
        scale.z = CalcScale(scale.z);
        return scale;
    }

    private float CalcScale(float scale)
    {
        if (scale <= GlobalConst.GARBAGE_MINIMUM_SIZE)
        {
        }
        else
        {
            scale -= GlobalConst.GARBAGE_BASE_SLICE_SIZE;
            if (scale < GlobalConst.GARBAGE_MINIMUM_SIZE)
            {
                scale = GlobalConst.GARBAGE_MINIMUM_SIZE;
            }
        }
        return scale;
    }

    private GarbageCube GetGarbageCube(Collider other)
    {
        GarbageCube garbageCube = other.gameObject.GetComponent<GarbageCube>();
        if (garbageCube == null)
        {
            garbageCube = other.gameObject.AddComponent<GarbageCube>();
        }
        return garbageCube;
    }

    private void CalcScore(Collider other)
    {
        GarbageCube garbageCube = GetGarbageCube(other);
        UnitStruct unitStruct =  garbageCube.GetUnitStruct();
        int score = ScoreCtrl.GetSliceGarbageScore(other);
        ScoreCtrl.UpdateAndDisplayScore(score, unitStruct.ScoreType);
    }

    internal void SweepGarbage(Collider other)
    {
        // 射程に入ったターゲットを消去する
        if (other.tag == GameEnum.TagType.Garbage.ToString() || other.tag == GameEnum.TagType.Ash.ToString())
        {
            float scoreMag = ScoreCtrl.CalcGarbageMagnification(other.transform.localScale);
            CalcScore(other);
            if (scoreMag <= GlobalConst.GARBAGE_MINIMUM_SIZE * 3)
            {
                // 対象オブジェクトを消す
                GameObjectTreat.DestroyAll(other.gameObject);
            }
            else
            {
                ChangeCubeSize(other);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        // OnTriggerStay は毎フレーム呼ばれるので間隔をあける
        float currentTime = Time.time;
        if (currentTime - _lastTriggerStayTime < _TRIGGER_STAY_INTERVAL)
        {
            return;
        }
        _lastTriggerStayTime = currentTime;
        SweepGarbage(other);
    }

    void Awake()
    {
        #if UNITY_EDITOR
            Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
