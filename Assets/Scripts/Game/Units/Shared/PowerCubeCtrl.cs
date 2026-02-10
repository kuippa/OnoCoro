using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
// using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public class PowerCubeCtrl : MonoBehaviour
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
            Debug.LogWarning("[PowerCubeCtrl] PowerCube component is null");
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
