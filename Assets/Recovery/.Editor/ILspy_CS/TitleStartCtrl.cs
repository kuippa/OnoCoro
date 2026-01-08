// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// TitleStartCtrl
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CommonsUtility;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleStartCtrl : MonoBehaviour
{
	private int _STAGE_SELECTOR_HEIGHT = 120;

	private int _STAGE_EDITOR_HEIGHT = 60;

	private int _STAGE_SELECTOR_MARGIN = 2;

	private GameObject _pnlStageSelector;

	private GameObject _pnlStageEditor;

	private GameObject _pnlAboutThisGame;

	private GameObject _pnlYAMLEditor;

	private GameObject _loading;

	private GameObject _StageScrollbar;

	private GameObject _EditorScrollbar;

	private const string _YAML_FILE_EXTENSION = ".yaml";

	private const string _STAGE_LIST_FILE_NAME = "stagelist.csv";

	private void OnClickGameClose()
	{
		base.gameObject.AddComponent<GameCtrl>().GameClose();
	}

	private void ClearContetArea(GameObject content)
	{
		int childCount = content.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			GameObjectTreat.DestroyAll(content.transform.GetChild(i).gameObject);
		}
	}

	private void SetStageContents(GameObject content)
	{
		ClearContetArea(content);
		GetSceneNames();
		Dictionary<string, string[]> sceneDict = GetSceneDict();
		foreach (KeyValuePair<string, string[]> item in sceneDict)
		{
			string key = item.Key;
			string[] value = item.Value;
			SetStageContent(content, key, value);
		}
		int count = sceneDict.Count;
		content.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, _STAGE_SELECTOR_HEIGHT * count + _STAGE_SELECTOR_MARGIN * (count - 1));
	}

	private void SetStageContent(GameObject content, string scenepath, string[] stageinfo)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(PrefabManager.UIStageSelectorPrefab, content.transform);
		gameObject.name = "UIStageSelector";
		gameObject.transform.Find("txtStageDisplayName").GetComponent<TextMeshProUGUI>().text = stageinfo[0];
		gameObject.transform.Find("imgStageIcon").GetComponent<Image>().sprite = Resources.Load<Sprite>(stageinfo[1]);
		gameObject.transform.Find("txtStageInfo").GetComponent<TextMeshProUGUI>().text = stageinfo[2];
		gameObject.transform.Find("txtStagePath").GetComponent<Text>().text = scenepath;
		Button btn = gameObject.GetComponent<Button>();
		btn.onClick.AddListener(delegate
		{
			OnClickSelectStage(btn);
		});
	}

	private void SetStageEditorContents(GameObject content)
	{
		ClearContetArea(content);
		Dictionary<string, string[]> sceneDict = GetSceneDict();
		foreach (KeyValuePair<string, string[]> item in sceneDict)
		{
			string key = item.Key;
			string[] value = item.Value;
			SetStageEditorContent(content, key, value);
		}
		int count = sceneDict.Count;
		content.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, _STAGE_EDITOR_HEIGHT * count + _STAGE_SELECTOR_MARGIN * (count - 1));
	}

	private void SetStageEditorContent(GameObject content, string scenepath, string[] stageinfo)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(PrefabManager.UIStageFileListPrefab, content.transform);
		gameObject.name = "UIStageFileList";
		gameObject.transform.Find("txtStageDisplayName").GetComponent<TextMeshProUGUI>().text = stageinfo[0];
		gameObject.transform.Find("txtStagePath").GetComponent<Text>().text = scenepath;
		Button btn = gameObject.GetComponent<Button>();
		btn.onClick.AddListener(delegate
		{
			OnClickSelectEditStage(btn);
		});
	}

	private void OnClickSelectEditStage(Button btnStageSelect)
	{
		Text component = btnStageSelect.transform.Find("txtStagePath").gameObject.GetComponent<Text>();
		if (component == null || component.text == "")
		{
			UnityEngine.Debug.Log("StagePath is null");
			return;
		}
		string fileName = Path.GetFileName(component.text + _YAML_FILE_EXTENSION);
		string text = LoadStreamingAsset.AllTextStream(fileName);
		if (text == null)
		{
			UnityEngine.Debug.Log("yaml is null");
			return;
		}
		_pnlYAMLEditor.SetActive(value: true);
		GameObject gameObject = GameObject.Find("tmpInputField");
		if (gameObject == null)
		{
			UnityEngine.Debug.LogWarning("tmpInputField is not found");
			return;
		}
		gameObject.GetComponent<TMP_InputField>().text = text;
		GameObject.Find("txtYamlPath").GetComponent<Text>().text = LoadStreamingAsset.StageFilePath(fileName);
	}

	private void OnClickSelectStage(Button btnStageSelect)
	{
		btnStageSelect.interactable = false;
		Text component = btnStageSelect.transform.Find("txtStagePath").gameObject.GetComponent<Text>();
		if (component == null || component.text == "")
		{
			UnityEngine.Debug.Log("StagePath is null");
			return;
		}
		string text = component.text;
		if (IsScenePathValid(text))
		{
			_loading.SetActive(value: true);
			StartCoroutine(LoadScene(text, 0.1f));
		}
		else
		{
			UnityEngine.Debug.LogWarning("Scene path not found: " + text);
		}
	}

	private IEnumerator LoadScene(string scenename, float delay)
	{
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene(scenename, LoadSceneMode.Single);
	}

	private string RemoveQuotesSafely(string input)
	{
		if (input.StartsWith("\"") && input.EndsWith("\""))
		{
			return input.Substring(1, input.Length - 2);
		}
		return input;
	}

	private Dictionary<string, string[]> GetSceneInfoDict()
	{
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		string[] array = LoadStreamingAsset.CsvLines(_STAGE_LIST_FILE_NAME);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = LoadStreamingAsset.CsvCols(array[i]);
			if (array2.Length != 4)
			{
				UnityEngine.Debug.LogWarning("Invalid line format: " + array[i]);
				continue;
			}
			dictionary.Add(array2[0], new string[3]
			{
				array2[1],
				array2[2],
				array2[3]
			});
		}
		return dictionary;
	}

	private string[] GetSceneNames()
	{
		int sceneCountInBuildSettings = SceneManager.sceneCountInBuildSettings;
		string[] array = new string[sceneCountInBuildSettings];
		for (int i = 0; i < sceneCountInBuildSettings; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
			array[i] = fileNameWithoutExtension;
		}
		return array;
	}

	private Dictionary<string, string[]> GetSceneDict()
	{
		string[] sceneNames = GetSceneNames();
		string text = SceneManager.GetActiveScene().name;
		Dictionary<string, string[]> sceneInfoDict = GetSceneInfoDict();
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		string[] array = new string[3] { "StageName", "StageIcon", "StageInfo" };
		foreach (string text2 in sceneNames)
		{
			if (sceneInfoDict.ContainsKey(text2))
			{
				dictionary.Add(text2, sceneInfoDict[text2]);
			}
			else if (!(text2 == text) && !dictionary.ContainsKey(text2))
			{
				string[] array2 = (string[])array.Clone();
				array2[0] = text2;
				dictionary.Add(text2, array2);
			}
		}
		return dictionary;
	}

	private bool IsScenePathValid(string scenename)
	{
		string[] sceneNames = GetSceneNames();
		for (int i = 0; i < sceneNames.Length; i++)
		{
			if (scenename == sceneNames[i])
			{
				return true;
			}
		}
		return false;
	}

	private void OnClickAboutGame()
	{
		_pnlAboutThisGame.SetActive(value: true);
		TextMeshProUGUI component = GameObject.Find("tmpGameInfo").GetComponent<TextMeshProUGUI>();
		TextAsset textAsset = Resources.Load("public_doc/aboutthisgame") as TextAsset;
		if (textAsset == null)
		{
			UnityEngine.Debug.LogWarning("aboutthisgame.txt is not found");
			return;
		}
		UnityEngine.Debug.Log("aboutthisgame " + textAsset.text);
		component.text = textAsset.text;
	}

	private bool OpenYamlByNotePad()
	{
		string text = GameObject.Find("txtYamlPath").GetComponent<Text>().text;
		if (File.Exists(text))
		{
			OpenFileInEditor(text);
			return true;
		}
		return false;
	}

	private void OpenFileInDefaultProgram(string filepath)
	{
		if (File.Exists(filepath))
		{
			try
			{
				Process.Start(new ProcessStartInfo(filepath)
				{
					UseShellExecute = true
				});
				return;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("ファイルを開けませんでした: " + filepath + "\nエラー: " + ex.Message);
				return;
			}
		}
		UnityEngine.Debug.LogWarning("ファイルが存在しません: " + filepath);
	}

	private void OpenFileInEditor(string filepath)
	{
		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
		{
			Process.Start("notepad.exe", filepath);
		}
		else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
		{
			Process.Start("open", "-a TextEdit " + filepath);
		}
		else if (Application.platform == RuntimePlatform.LinuxPlayer)
		{
			Process.Start("xdg-open", filepath);
		}
		else
		{
			UnityEngine.Debug.LogError("Unsupported platform");
		}
	}

	private void OnClickBugReport()
	{
		string text = "https://forms.gle/CdvySsxeXMDAigTQ9";
		UnityEngine.Debug.Log("OnClickBugReport " + text);
		Application.OpenURL(text);
	}

	private void OnClickYamlEdit()
	{
		OpenYamlByNotePad();
		_pnlStageEditor.SetActive(value: false);
		_pnlYAMLEditor.SetActive(value: false);
	}

	private void OnClickStageSelect()
	{
		SetScrollbarTopPosition(_StageScrollbar);
		_pnlStageEditor.SetActive(value: false);
		_pnlStageSelector.SetActive(!_pnlStageSelector.activeSelf);
	}

	private void OnClickStageEditor()
	{
		SetScrollbarTopPosition(_EditorScrollbar);
		_pnlStageSelector.SetActive(value: false);
		_pnlStageEditor.SetActive(!_pnlStageEditor.activeSelf);
	}

	private void SetScrollbarTopPosition(GameObject scrollbar)
	{
		if (!(scrollbar == null))
		{
			scrollbar.GetComponent<Scrollbar>().value = 1f;
		}
	}

	private void Awake()
	{
		_loading = GameObject.Find("nowloading");
		_loading.SetActive(value: false);
		_loading.GetComponent<Canvas>().planeDistance = 100f;
		GameObject.Find("btnGameClose").GetComponent<Button>().onClick.AddListener(delegate
		{
			OnClickGameClose();
		});
		GameObject.Find("btnAboutGame").GetComponent<Button>().onClick.AddListener(delegate
		{
			OnClickAboutGame();
		});
		GameObject.Find("btnStageEditor").GetComponent<Button>().onClick.AddListener(delegate
		{
			OnClickStageEditor();
		});
		GameObject.Find("btnStage").GetComponent<Button>().onClick.AddListener(delegate
		{
			OnClickStageSelect();
		});
		GameObject.Find("btnYamlEdit").GetComponent<Button>().onClick.AddListener(delegate
		{
			OnClickYamlEdit();
		});
		GameObject.Find("btnBugReport").GetComponent<Button>().onClick.AddListener(delegate
		{
			OnClickBugReport();
		});
		GameObject stageContents = GameObject.Find("pnlStageSelector/Scroll View/Viewport/Content");
		SetStageContents(stageContents);
		_StageScrollbar = GameObject.Find("pnlStageSelector/Scroll View/Scrollbar Vertical");
		SetScrollbarTopPosition(_StageScrollbar);
		GameObject stageEditorContents = GameObject.Find("pnlStageEditor/Scroll View/Viewport/Content");
		SetStageEditorContents(stageEditorContents);
		_EditorScrollbar = GameObject.Find("pnlStageEditor/Scroll View/Scrollbar Vertical");
		SetScrollbarTopPosition(_EditorScrollbar);
		GameObject.Find("txtVersionInfo").GetComponent<TextMeshProUGUI>().text = "version:" + GameObjectTreat.GetAppBuildDate();
		_pnlStageSelector = GameObject.Find("pnlStageSelector");
		_pnlStageSelector.SetActive(value: false);
		_pnlStageEditor = GameObject.Find("pnlStageEditor");
		_pnlStageEditor.SetActive(value: false);
		_pnlAboutThisGame = GameObject.Find("pnlAboutThisGame");
		_pnlAboutThisGame.SetActive(value: false);
		_pnlYAMLEditor = GameObject.Find("pnlYAMLEditor");
		_pnlYAMLEditor.SetActive(value: false);
	}
}
