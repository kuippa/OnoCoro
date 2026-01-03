using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Diagnostics;
// 2023/11/3 Zoom開催 Plateau のイベントで５分LT用のスクリプト

public class LightningTalkCtrl : MonoBehaviour
{
    private void MoveScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "UnitLT":
                SceneManager.LoadScene("TitlteStart");
                break;
            case "TitlteStart":
                SceneManager.LoadScene("Playground");
                break;
            case "Playground":
                SceneManager.LoadScene("GameTutorial");
                break;
            case "GameTutorial":
                SceneManager.LoadScene("UnitSpawn");
                break;
            case "UnitSpawn":
                SceneManager.LoadScene("群馬県桐生市");
                break;

            default:
                SceneManager.LoadScene("TitlteStart");
                break;
        }
    }

    void OnClickToNextScene()
    {
        // ゲームに戻る
        // Debug.Log("OnClickToNextScene" + SceneManager.GetActiveScene().name);
        MoveScene();

    }

    void Awake()
    {
        // イベントリスナーでメソッドを登録
        Button btn = this.gameObject.GetComponentInChildren<Button>();
        btn.onClick.AddListener(OnClickToNextScene);
        // このオブジェクトを破棄しないようにする
        DontDestroyOnLoad(this.gameObject);
    } 

}
