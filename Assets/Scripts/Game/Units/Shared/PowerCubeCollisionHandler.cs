using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
// using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// PowerCube と Player の衝突時の相互作用を処理するハンドラー
/// プレハブにアタッチされ、衝突検出時にスコア処理を実行します
/// </summary>
public class PowerCubeCollisionHandler : MonoBehaviour
{
    PowerCube _powerCube = null;

    /// <summary>
    /// Player が進入した時に呼ばれます
    /// PowerCubeTriggerHandler から呼び出されます
    /// </summary>
    /// <param name="other">進入した Collider（Player）</param>
    internal void OnPlayerEnter(Collider other)
    {
        if (other == null || other.gameObject == null)
        {
            return;
        }

        if (_powerCube == null)
        {
            Debug.LogWarning("[PowerCubeCollisionHandler] PowerCube component is null");
            return;
        }

        UnitStruct unitStruct = _powerCube.UnitStruct;
        int score = (int)unitStruct.BaseScore;
        if (ScoreCtrl.IsScorePositiveInt(score, unitStruct.ScoreType))
        {
            ScoreCtrl.UpdateAndDisplayScore(score, unitStruct.ScoreType);
            GameObjectTreat.DestroyAll(this.gameObject);
        }
    }

    void Awake()
    {
        _powerCube = this.gameObject.AddComponent<PowerCube>();

        // PowerCubeTriggerHandler をアタッチ
        GameObjectTreat.GetOrAddComponent<PowerCubeTriggerHandler>(gameObject);
    }
}
