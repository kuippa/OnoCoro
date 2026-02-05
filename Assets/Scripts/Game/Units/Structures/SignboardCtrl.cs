using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Debug = CommonsUtility.Debug;

public class SignboardCtrl : MonoBehaviour
{
    /// <summary>立て看板の内部パスコンポーネント</summary>
    private const string BLOOM_PATH = "board/BloomQuad";
    
    private Canvas _uiBoard;
    private GameObject _uiBoardInstance;
    private TextMeshProUGUI _txtBoard;
    private GameObject _bloomQuad;
    private EventLoader _eventLoader = EventLoader.instance;

    [SerializeField]
    public bool _isBoardActive = false;
    public bool _isBoardBloom = false;
    public string _boardCD = "firstReadMeText";
    public string _boardText = "ReadMeText!よんでね！";

    /// <summary>
    /// 動的生成用の立て看板セットアップメソッド
    /// code と text を設定し、表示テキストを反映させます
    /// </summary>
    internal void SetupSignboard(string code, string text)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("[SignboardCtrl.SetupSignboard] code or text is null/empty");
            return;
        }
        
        this._boardCD = code;
        this._boardText = text;
        SetBoardText(this._boardText);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
        {
            ToggleBoard(true);
            ToggleBloom(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
        {
            ToggleBoard(false);
            ToggleBloom(false);
        }
    }

    void Start()
    {
        if (_eventLoader == null)
        {
            _eventLoader = EventLoader.instance;
        }
        
        if (_eventLoader == null)
        {
            return;
        }
        
        string eventText = _eventLoader.GetBoardText(_boardCD);
        
        if (!string.IsNullOrEmpty(eventText))
        {
            _boardText = eventText;
            SetBoardText(_boardText);
        }
    }

    private void SetBoardText(string boardText)
    {
        TextMeshProUGUI txtComponent = GetBoardTextComponent();
        if (txtComponent == null)
        {
            return;
        }
        
        txtComponent.text = boardText;
    }

    private TextMeshProUGUI GetBoardTextComponent()
    {
        if (_txtBoard != null)
        {
            return _txtBoard;
        }
        
        if (_uiBoardInstance == null)
        {
            Debug.LogWarning("[SignboardCtrl.GetBoardTextComponent] _uiBoardInstance が null です");
            return null;
        }
        
        _txtBoard = _uiBoardInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (_txtBoard == null)
        {
            Debug.LogWarning("[SignboardCtrl.GetBoardTextComponent] UIBoard 内に TextMeshProUGUI コンポーネントが見つかりません");
            return null;
        }
        
        return _txtBoard;
    }

    void Awake()
    {
        InitializeCanvasBoard();
        InitializeBloomQuad();
        SetBoardText(_boardText);
        ToggleBoard(false);
    }

    private void InitializeCanvasBoard()
    {
        GameObject uiBoardPrefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.UIBoard);
        if (uiBoardPrefab == null)
        {
            Debug.LogWarning("[SignboardCtrl.InitializeCanvasBoard] UIBoard プレファブが見つかりません");
            return;
        }
        
        _uiBoardInstance = Instantiate(uiBoardPrefab, transform);
        if (_uiBoardInstance == null)
        {
            Debug.LogWarning("[SignboardCtrl.InitializeCanvasBoard] UIBoard インスタンス化に失敗しました");
            return;
        }
        
        _uiBoard = _uiBoardInstance.GetComponent<Canvas>();
        if (_uiBoard == null)
        {
            Debug.LogWarning("[SignboardCtrl.InitializeCanvasBoard] UIBoard に Canvas コンポーネントが見つかりません");
            return;
        }
        
        Button button = _uiBoard.gameObject.GetComponent<Button>();
        if (button == null)
        {
            button = _uiBoard.gameObject.AddComponent<Button>();
        }
        if (button != null)
        {
            button.onClick.AddListener(ClickBoard);
        }
    }

    private void InitializeBloomQuad()
    {
        _bloomQuad = transform.Find(BLOOM_PATH)?.gameObject;
        if (_bloomQuad == null)
        {
            Debug.LogWarning($"[SignboardCtrl.InitializeBloomQuad] Bloom オブジェクトが見つかりません: {BLOOM_PATH}");
            return;
        }
    }

    private void ClickBoard()
    {
        ToggleBoard(false);
    }

    private void ToggleBoard(bool isBoardActive = false)
    {
        if (_uiBoard != null)
        {
            _uiBoard.gameObject.SetActive(isBoardActive);
        }
    }

    private void ToggleBloom(bool isBoardBloom = false)
    {
        if (_bloomQuad != null)
        {
            _bloomQuad.gameObject.SetActive(isBoardBloom);
        }
    }
}
