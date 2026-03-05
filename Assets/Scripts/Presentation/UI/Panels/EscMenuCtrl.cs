using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using CommonsUtility;

/// <summary>
/// ESC メニューコントローラー
/// UIControllerBase を継承し、初期化フラグで状態を管理
/// </summary>
public class EscMenuCtrl : UIControllerBase
{
    private GameObject _esc_menu_window = null;
    private GameTimerCtrl _gameTimerCtrl = null;

    /// <summary>
    /// 参照取得とメニュー初期非表示（元の Awake パターンを復帰）
    /// 初期化タイミングの問題を確認するため
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        
        GameTimerCtrl gameTimer = GameTimerCtrl.GetInstance();
        if (gameTimer != null)
        {
            _gameTimerCtrl = gameTimer;
        }

        // menuWindow 参照取得
        _esc_menu_window = this.gameObject.transform.Find("menuWindow").gameObject;
        
        // [重要] メニュー初期非表示を Awake で実行（元の実装）
        ToggleEscMenuWindow(false);
    }

    /// <summary>
    /// Initialize コルーチン - ボタンリスナー設定のみ
    /// </summary>
    protected override IEnumerator Initialize()
    {
        // [重要] ボタン設定を Initialize() に移動（非同期対応）
        SetupButtonListeners();
        
        yield return null;
    }

    /// <summary>
    /// ボタンリスナー設定
    /// Initialize() コルーチンから呼び出される
    /// </summary>
    private void SetupButtonListeners()
    {
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
        Debug.Log("OnClickOptions:: これはまだ未実装です");
        ToggleEscMenuWindow(false);
        UIUtility.ClearEventSystemSelection();
    }    

    public void OnClickBackToTitle()
    {
        // タイトル画面に戻る
        SceneLoaderManager.LoadScene(SceneLoaderManager.LoadSceneName.TitlteStart.ToString());
        ToggleEscMenuWindow(false);
        UIUtility.ClearEventSystemSelection();
    }

    public void OnClickBackToWindows()
    {
        GameManager gameManager = this.gameObject.AddComponent<GameManager>();
        gameManager.GameClose();
        // ToggleEscMenuWindow(false);

    }

    public void OnClickBackToGame()
    {
        ToggleEscMenuWindow(false);
        UIUtility.ClearEventSystemSelection();
    }

    public bool GetEscMenuWindowStatus()
    {
        bool ret = false;
        if (_esc_menu_window != null)
        {
            ret = _esc_menu_window.activeSelf;
        }        
        return ret;
    }

    public void ToggleEscMenuWindow(bool isOn)
    {
        if (_esc_menu_window != null)
        {
            // buttonのselectedを解除する（EventSystem が存在する場合のみ）
            UIUtility.ClearEventSystemSelection();
            _esc_menu_window.SetActive(isOn);
        }

        // ゲーム内時間を一時停止
        Time.timeScale = isOn ? 0.0001f : 1;

        // 時計の進行だけを止めているのでマウスカーソルや他のイベントは動いてしまう
        // TODO: 他のイベントも止める
        if (_gameTimerCtrl != null)
        {
            _gameTimerCtrl._isPaused = isOn;
            Debug.Log("_gameTimerCtrl._isPaused" + isOn);
        }
    }

    

}
