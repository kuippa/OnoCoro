using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using UnityEngine.InputSystem.XInput;

public class SweepCtrl : MonoBehaviour
{
    private float _lastTriggerStayTime;
    private const float _TRIGGER_STAY_INTERVAL = 0.1f; 

    private const float _BASE_DECREASE_SIZE = 0.5f;
    private const float _MINIMUM_SIZE = 0.5f;


    void OnTriggerEnter(Collider other)
    {
        // #if UNITY_EDITOR
        //     Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        // #endif

        // 射程に入ったターゲットをロックする
        if (other.tag == GameEnum.EnemyType.Garbage.ToString() )
        {
            GameObjectTreat.DebugColorChange(other.gameObject, Color.white);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // #if UNITY_EDITOR
        //     Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        // #endif
        if (other.tag == GameEnum.EnemyType.Garbage.ToString() )
        {
            GameObjectTreat.DebugColorChange(other.gameObject, Color.magenta);
        }    
    }

    // スコア倍率を計算する
    private float CalcScoreMagnification(Vector3 ScaleMagnification)
    {
        float ret = 1f;
        // ret = ScaleMagnification.x * ScaleMagnification.y * ScaleMagnification.z;
        ret = ScaleMagnification.x + ScaleMagnification.y + ScaleMagnification.z;
        return ret;
    }

    private void ChangeCubeSize(Collider other, float scoreMag)
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
        if (scale <= _MINIMUM_SIZE)
        {
        }
        else
        {
            scale -= _BASE_DECREASE_SIZE;
            if (scale < _MINIMUM_SIZE)
            {
                scale = _MINIMUM_SIZE;
            }
        }
        return scale;
    }

    private void CalcScore(Collider other, float scoreMag)
    {
        GameObjectTreat.DebugColorChange(other.gameObject, Color.yellow);
        GarbageCube garbageCube = other.gameObject.GetComponent<GarbageCube>();
        CharacterStruct CharacterStruct =  garbageCube.GetCharacterStruct();
        int score = CharacterStruct.BaseScore;
        score = (int)(score * scoreMag);
        ScoreCtrl.CalcScore(score, CharacterStruct.ScoreType);
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

        // 射程に入ったターゲットを消去する
        if (other.tag == GameEnum.EnemyType.Garbage.ToString() )
        {
            // Debug.Log("SweepCtrl OnTriggerStay " + other.name);

            // TODO:: 与ダメージを計算する
            float scoreMag = CalcScoreMagnification(other.transform.localScale);
            CalcScore(other, scoreMag);

            if (scoreMag <= _MINIMUM_SIZE * 3)
            {
                // 対象オブジェクトを消す
                GameObjectTreat.DestroyAll(other.gameObject);
            }
            else
            {
                ChangeCubeSize(other, scoreMag);
            }
        }        
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
