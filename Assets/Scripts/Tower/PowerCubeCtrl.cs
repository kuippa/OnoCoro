using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using CommonsUtility;

public class PowerCubeCtrl : MonoBehaviour
{
    [SerializeField] public float _Power = 0f;

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("OnTriggerEnter" + other.gameObject.tag + " " + other.gameObject.name);

        if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
        {
            // ItemStruct itemStruct = GetComponent<PowerCube>().ItemStruct;
            CharacterStruct characterStruct = GetComponent<PowerCube>().CharacterStruct;

            // ItemStruct itemStruct = _itemList[_page];
            int score =  (int)characterStruct.BaseScore;
            // if (ScoreCtrl.IsScorePositiveInt(score, characterStruct.ScoreType))
            // {
                ScoreCtrl.CalcScore(score, characterStruct.ScoreType);
                GameObjectTreat.DestroyAll(this.gameObject);
                return;
            // }
            // Debug.Log("OnTriggerEnter" + other.gameObject.tag + " " + other.gameObject.name);

            // ToggleBoard(true);
            // ToggleBloom(true);
        }
    }

    void Awake()
    {
        this.gameObject.AddComponent<PowerCube>();

    }
}
