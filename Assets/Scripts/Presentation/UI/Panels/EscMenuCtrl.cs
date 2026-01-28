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
    private GameObject _environment = null;
    private GameTimerCtrl _gameTimerCtrl = null;

    /// <summary>
    /// 参照取得とメニュー初期非表示（元の Awake パターンを復帰）
    /// 初期化タイミングの問題を確認するため
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        
        _environment = GameObject.Find("Environment").gameObject;
        if (_environment == null)
        {
            Debug.Log("Environment is not found");
        }

        GameObject txtGameTime = GameObject.Find("txtGameTime");
        if (txtGameTime != null)
        {
            _gameTimerCtrl = txtGameTime.GetComponent<GameTimerCtrl>();
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
        EventSystem.current.SetSelectedGameObject(null);
    }    

    public void OnClickBackToTitle()
    {
        // タイトル画面に戻る
        // TODO:
        SceneLoaderManager.LoadScene(SceneLoaderManager.LoadSceneName.TitleScene.ToString());
        ToggleEscMenuWindow(false);
        EventSystem.current.SetSelectedGameObject(null);
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
        EventSystem.current.SetSelectedGameObject(null);
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
            // buttonのselectedを解除する
            EventSystem.current.SetSelectedGameObject(null);
            _esc_menu_window.SetActive(isOn);
        }

        // ゲーム内時間を一時停止
        // Environment
        // _environment.SetActive(isOn ? false : true);

        // Invalid AABB aabb でアニメーション関係でバグが発生するため、Time.timeScaleを使わない
        // // Physics.autoSimulation = isOn ? false : true;
        // Time.timeScale = isOn ? 0 : 1;
        // Time.fixedDeltaTime = 0.02f * Time.timeScale;
        // // Animator animator = GetComponent<Animator>();
        // // animator.speed = isOn ? 0 : 1;


        // GameTimerCtrl gameTimerCtrl = this.gameObject.AddComponent<GameTimerCtrl>();
        // gameTimerCtrl._buf_time = Time.time;

        // 時計の進行だけを止めているのでマウスカーソルや他のイベントは動いてしまう
        // TODO: 他のイベントも止める
        if (_gameTimerCtrl != null)
        {
            _gameTimerCtrl._isPaused = isOn;
            Debug.Log("_gameTimerCtrl._isPaused" + isOn);
        }

    }

    

}
