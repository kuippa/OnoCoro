using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
// using UnityEditor.Experimental.GraphView;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class SignboardCtrl : MonoBehaviour
{
    private Canvas _cvsBoard;
    private TextMeshProUGUI _txtBoard;
    private GameObject _bloomQuad;
    private EventLoader _eventLoader = EventLoader.instance;

    [SerializeField]
    public bool _isBoardActive = false;
    public bool _isBoardBloom = false;
    public string _boardCD = "firstReadMeText";
    public string _boardText = "ReadMeText!よんでね！";


    private void OnTriggerEnter(Collider other)
    {
        // 接触側か、ボード側にもrigidbodyが必要
        // Debug.Log("OnTriggerEnter" + other.gameObject.tag);

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
        // Debug.Log("SignboardCtrl Start " + this.name + _boardCD);
        if (_eventLoader == null)
        {
            _eventLoader = EventLoader.instance;
        }
        if (_eventLoader != null)
        {
            // EventLoader eventLoader = gamePrefabs.GetComponent<EventLoader>();
            _boardText = _eventLoader.GetBoardText(_boardCD);
            SetBoardText(_boardText);
        }
        // else
        // {
        //     Debug.Log("SignboardCtrl Start _eventLoader is null");
        // }
    }

    private void SetBoardText(string boardText)
    {
        if (_txtBoard == null)
        {
            GameObject tmpBoard = this.transform.Find("cvsBoard/backboardImage/tmpBoard").gameObject;
            if (tmpBoard != null)
            {
                _txtBoard = tmpBoard.GetComponent<TextMeshProUGUI>();
            }
        }
        if (_txtBoard != null)
        {
            _txtBoard.text = boardText;
        }
    }

    void Awake()
    {
        // Debug.Log("SignboardCtrl Awake" + this.name + " " + _boardCD);
        GameObject objBoard = this.transform.Find("cvsBoard").gameObject;
        if (objBoard != null)
        {
            _cvsBoard = objBoard.GetComponent<Canvas>();
        }
        SetBoardText(_boardText);

        // ボードにイベントリスナーを追加
        if (_cvsBoard != null)
        {
            _cvsBoard.gameObject.AddComponent<Button>().onClick.AddListener(ClickBoard);
        }

        _bloomQuad = this.transform.Find("board/BloomQuad").gameObject;
        if (_bloomQuad != null)
        {
            ToggleBloom(true);
        }
        ToggleBoard(false);
    }

    private void ClickBoard()
    {
        ToggleBoard(false);
    }

    private void ToggleBoard(bool isBoardActive = false)
    {
        if (_cvsBoard != null)
        {
            _cvsBoard.gameObject.SetActive(isBoardActive);
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
