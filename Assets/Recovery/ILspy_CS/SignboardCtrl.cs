// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// SignboardCtrl
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignboardCtrl : MonoBehaviour
{
	private Canvas _cvsBoard;

	private TextMeshProUGUI _txtBoard;

	private GameObject _bloomQuad;

	private EventLoader _eventLoader = EventLoader.instance;

	[SerializeField]
	public bool _isBoardActive;

	public bool _isBoardBloom;

	public string _boardCD = "firstReadMeText";

	public string _boardText = "ReadMeText!よんでね！";

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
		{
			ToggleBoard(isBoardActive: true);
			ToggleBloom(isBoardBloom: true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
		{
			ToggleBoard();
			ToggleBloom();
		}
	}

	private void Start()
	{
		if (_eventLoader == null)
		{
			_eventLoader = EventLoader.instance;
		}
		if (_eventLoader != null)
		{
			_boardText = _eventLoader.GetBoardText(_boardCD);
			SetBoardText(_boardText);
		}
	}

	private void SetBoardText(string boardText)
	{
		if (_txtBoard == null)
		{
			GameObject gameObject = base.transform.Find("cvsBoard/backboardImage/tmpBoard").gameObject;
			if (gameObject != null)
			{
				_txtBoard = gameObject.GetComponent<TextMeshProUGUI>();
			}
		}
		if (_txtBoard != null)
		{
			_txtBoard.text = boardText;
		}
	}

	private void Awake()
	{
		GameObject gameObject = base.transform.Find("cvsBoard").gameObject;
		if (gameObject != null)
		{
			_cvsBoard = gameObject.GetComponent<Canvas>();
		}
		SetBoardText(_boardText);
		if (_cvsBoard != null)
		{
			_cvsBoard.gameObject.AddComponent<Button>().onClick.AddListener(ClickBoard);
		}
		_bloomQuad = base.transform.Find("board/BloomQuad").gameObject;
		if (_bloomQuad != null)
		{
			ToggleBloom(isBoardBloom: true);
		}
		ToggleBoard();
	}

	private void ClickBoard()
	{
		ToggleBoard();
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
