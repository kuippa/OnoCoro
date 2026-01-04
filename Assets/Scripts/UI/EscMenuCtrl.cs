using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using CommonsUtility;

public class EscMenuCtrl : MonoBehaviour
{
    private GameObject _esc_menu_window = null;

    void Awake()
    {
        // menuWindowを非表示にする
        _esc_menu_window = this.gameObject.transform.Find("menuWindow").gameObject;
        ToggleEscMenuWindow(false);

        GameObject txtBackToGame = this.gameObject.transform.Find("menuWindow/txtBackToGame").gameObject;
        if (txtBackToGame != null)
        {
            Button btn = txtBackToGame.GetComponent<Button>();
            btn.onClick.AddListener(OnClickBackToGame);
        }

        GameObject txtBackToTitlte = this.gameObject.transform.Find("menuWindow/txtBackToTitle").gameObject;
        if (txtBackToTitlte != null)
        {
            Button btn = txtBackToTitlte.GetComponent<Button>();
            btn.onClick.AddListener(OnClickBackToTitle);
        }

        GameObject txtBackToWindows = this.gameObject.transform.Find("menuWindow/txtBackToWindows").gameObject;
        if (txtBackToWindows != null)
        {
            Button btn = txtBackToWindows.GetComponent<Button>();
            btn.onClick.AddListener(OnClickBackToWindows);
        }

        GameObject txtOptions = this.gameObject.transform.Find("menuWindow/txtOptions").gameObject;
        if (txtOptions != null)
        {
            Button btn = txtOptions.GetComponent<Button>();
            btn.onClick.AddListener(OnClickOptions);
        }
    }

    public void OnClickOptions()
    {
        // オプション画面を呼び出す
        // TODO:
        // UnityEngine.SceneManagement.SceneManager.LoadScene("OptionScene");
        ToggleEscMenuWindow(false);

    }    

    public void OnClickBackToTitle()
    {
        // タイトル画面に戻る
        // TODO:
        // UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");

        ToggleEscMenuWindow(false);
    }

    public void OnClickBackToWindows()
    {
        GameCtrl gameCtrl = this.gameObject.AddComponent<GameCtrl>();
        gameCtrl.GameClose();
        // ToggleEscMenuWindow(false);

    }


    public void OnClickBackToGame()
    {
        ToggleEscMenuWindow(false);

    }


    public void ToggleEscMenuWindow(bool isOn)
    {
        if (_esc_menu_window != null)
        {
            // buttonのselectedを解除する
            EventSystem.current.SetSelectedGameObject(null);
            _esc_menu_window.SetActive(isOn);
        }
        // ゲーム内時間を一時停止
        Time.timeScale = isOn ? 0 : 1;
        // Time.timeScale = isOn ? 1 : 0;
        // Debug.Log("ToggleEscMenuWindow:" + isOn + Time.timeScale);
    }


}
