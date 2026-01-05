// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// EscMenuCtrl
using CommonsUtility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscMenuCtrl : MonoBehaviour
{
	private GameObject _esc_menu_window;

	private GameObject _environment;

	private GameTimerCtrl _gameTimerCtrl;

	private void Awake()
	{
		_environment = GameObject.Find("Environment").gameObject;
		if (_environment == null)
		{
			Debug.Log("Environment is not found");
		}
		GameObject gameObject = GameObject.Find("txtGameTime");
		if (gameObject != null)
		{
			_gameTimerCtrl = gameObject.GetComponent<GameTimerCtrl>();
		}
		_esc_menu_window = base.gameObject.transform.Find("menuWindow").gameObject;
		ToggleEscMenuWindow(isOn: false);
		GameObject gameObject2 = base.gameObject.transform.Find("menuWindow/txtBackToGame").gameObject;
		if (gameObject2 != null)
		{
			gameObject2.GetComponent<Button>().onClick.AddListener(OnClickBackToGame);
		}
		GameObject gameObject3 = base.gameObject.transform.Find("menuWindow/txtBackToTitle").gameObject;
		if (gameObject3 != null)
		{
			gameObject3.GetComponent<Button>().onClick.AddListener(OnClickBackToTitle);
		}
		GameObject gameObject4 = base.gameObject.transform.Find("menuWindow/txtBackToWindows").gameObject;
		if (gameObject4 != null)
		{
			gameObject4.GetComponent<Button>().onClick.AddListener(OnClickBackToWindows);
		}
		GameObject gameObject5 = base.gameObject.transform.Find("menuWindow/txtOptions").gameObject;
		if (gameObject5 != null)
		{
			Button component = gameObject5.GetComponent<Button>();
			component.onClick.AddListener(OnClickOptions);
			component.interactable = false;
		}
	}

	public void OnClickOptions()
	{
		ToggleEscMenuWindow(isOn: false);
	}

	public void OnClickBackToTitle()
	{
		SceneManager.LoadScene("TitlteStart");
		ToggleEscMenuWindow(isOn: false);
	}

	public void OnClickBackToWindows()
	{
		base.gameObject.AddComponent<GameCtrl>().GameClose();
	}

	public void OnClickBackToGame()
	{
		ToggleEscMenuWindow(isOn: false);
	}

	public bool GetEscMenuWindowStatus()
	{
		bool result = false;
		if (_esc_menu_window != null)
		{
			result = _esc_menu_window.activeSelf;
		}
		return result;
	}

	public void ToggleEscMenuWindow(bool isOn)
	{
		if (_esc_menu_window != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
			_esc_menu_window.SetActive(isOn);
		}
		if (_gameTimerCtrl != null)
		{
			_gameTimerCtrl._isPaused = isOn;
		}
	}
}
