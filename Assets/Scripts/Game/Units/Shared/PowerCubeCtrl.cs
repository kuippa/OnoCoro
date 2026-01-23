using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using CommonsUtility;

public class PowerCubeCtrl : MonoBehaviour
{
    PowerCube _powerCube = null;

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("OnTriggerEnter" + other.gameObject.tag + " " + other.gameObject.name);

        if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
        {
            UnitStruct unitStruct = _powerCube.UnitStruct;
            int score =  (int)unitStruct.BaseScore;
            if (ScoreCtrl.IsScorePositiveInt(score, unitStruct.ScoreType))
            {
                ScoreCtrl.UpdateAndDisplayScore(score, unitStruct.ScoreType);
                GameObjectTreat.DestroyAll(this.gameObject);
                // return;
            }
            // Debug.Log("OnTriggerEnter" + other.gameObject.tag + " " + other.gameObject.name);
        }
    }

    void Awake()
    {
        _powerCube = this.gameObject.AddComponent<PowerCube>();

    }
}
