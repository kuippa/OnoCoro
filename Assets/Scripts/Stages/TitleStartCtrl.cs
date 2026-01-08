using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonsUtility;
using System;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;
using Debug = UnityEngine.Debug;

public class TitleStartCtrl : MonoBehaviour
{
    // UI Layout Constants
    private int _STAGE_SELECTOR_HEIGHT = 120;
    private int _STAGE_EDITOR_HEIGHT = 60;
    private int _STAGE_SELECTOR_MARGIN = 2;
    
    // File Constants
    private const string _YAML_FILE_EXTENSION = ".yaml";
    private const string _STAGE_LIST_FILE_NAME = "stagelist.csv";
    private const string _ABOUT_GAME_FILE_NAME = "aboutthisgame.txt";
    private const string _NOTICE_FILE_NAME = "notice.txt";
    
    // Prefab Paths
    private const string _PREFAB_PATH_LOADING = "Prefabs/UI/nowloading";
    
    // URL Constants
    private const string _BUG_REPORT_URL = "https://forms.gle/CdvySsxeXMDAigTQ9";
    
    // GameObject Names
    private const string _OBJ_LOADING = "nowloading";
    private const string _OBJ_BTN_GAME_CLOSE = "btnGameClose";
    private const string _OBJ_BTN_ABOUT_GAME = "btnAboutGame";
    private const string _OBJ_BTN_STAGE_EDITOR = "btnStageEditor";
    private const string _OBJ_BTN_SELECT_STAGE = "btnSelectStage";
    private const string _OBJ_BTN_YAML_EDIT = "btnYamlEdit";
    private const string _OBJ_BTN_BUG_REPORT = "btnBugReport";
    private const string _OBJ_PNL_STAGE_SELECTOR = "pnlStageSelector";
    private const string _OBJ_PNL_ABOUT_THIS_GAME = "pnlAboutThisGame";
    private const string _OBJ_PNL_BOTTOM_BAR = "pnlBottomBar";
    private const string _OBJ_PNL_YAML_EDITOR = "pnlYAMLEditor";
    private const string _OBJ_TXT_VERSION_INFO = "txtVersionInfo";
    private const string _OBJ_TMP_GAME_INFO = "tmpGameInfo";
    private const string _OBJ_TMP_NOTICE = "tmpNotice";
    private const string _OBJ_TMP_INPUT_FIELD = "tmpInputField";
    private const string _OBJ_TXT_YAML_PATH = "txtYamlPath";
    
   
    // Prefab Child Element Names
    private const string _CHILD_TXT_STAGE_DISPLAY_NAME = "txtStageDisplayName";
    private const string _CHILD_IMG_STAGE_ICON = "imgStageIcon";
    private const string _CHILD_TXT_STAGE_INFO = "txtStageInfo";
    private const string _CHILD_TXT_STAGE_PATH = "txtStagePath";
    private const string _CHILD_BTN_STAGE_PLAY = "btnStagePlay";
    
    // UI Child Paths (relative to parent)
    private const string _CHILD_PATH_SCROLLVIEW_CONTENT = "Scroll View/Viewport/Content";
    private const string _CHILD_PATH_SCROLLVIEW_SCROLLBAR = "Scroll View/Scrollbar Vertical";
    
    private const string _UI_STAGE_INFO_BOX = "UIStageInfoBox";
    // Editor Commands
    private const string _EDITOR_WINDOWS = "notepad.exe";
    private const string _EDITOR_MAC = "open";
    private const string _EDITOR_MAC_ARGS = "-a TextEdit ";
    private const string _EDITOR_LINUX = "xdg-open";
    
    // UI Messages
    private const string _MSG_STAGE_PATH_NULL = "StagePath is null";
    private const string _MSG_YAML_NULL = "yaml is null";
    private const string _MSG_TMP_INPUT_NOT_FOUND = "tmpInputField is not found";
    private const string _MSG_SCENE_PATH_NOT_FOUND = "Scene path not found: ";
    private const string _MSG_INVALID_LINE_FORMAT = "Invalid line format: ";
    private const string _MSG_ABOUT_GAME_NOT_FOUND = "aboutthisgame.txt is not found";
    private const string _MSG_ABOUT_GAME_ERROR = "ゲーム情報ファイルが見つかりませんでした。";
    private const string _MSG_ABOUT_GAME_LOADED = "aboutthisgame loaded";
    private const string _MSG_NOTICE_NOT_FOUND = "notice.txt is not found";
    private const string _MSG_NOTICE_ERROR = "お知らせファイルが見つかりませんでした。";
    private const string _MSG_NOTICE_LOADED = "notice loaded";
    private const string _MSG_FILE_OPEN_ERROR = "ファイルを開けませんでした: ";
    private const string _MSG_FILE_NOT_EXIST = "ファイルが存在しません: ";
    private const string _MSG_UNSUPPORTED_PLATFORM = "Unsupported platform";
    private const string _MSG_BUG_REPORT_LOG = "OnClickBugReport ";
    
    // Numeric Constants
    private const float _SCENE_LOAD_DELAY = 0.1f;
    private const float _SCROLLBAR_TOP_VALUE = 1f;
    
    // Private Fields
    private GameObject _pnlStageSelector;
    private GameObject _pnlAboutThisGame;
    private GameObject _pnlYAMLEditor;
    private GameObject _pnlBottomBar;
    private GameObject _loading;
    private GameObject _StageScrollbar;
    private GameObject _EditorScrollbar;

    // GameTutorial

    private void OnClickGameClose()
    {
        // Debug.Log("OnClickGameClose");
        GameCtrl gameCtrl = this.gameObject.AddComponent<GameCtrl>();
        gameCtrl.GameClose();
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
        if (PrefabManager.UIStageInfoBoxPrefab == null)
        {
            Debug.LogError("UIStageInfoBoxPrefab が null です。PrefabManager が正しく初期化されているか確認してください。");
            return;
        }
        
        GameObject stageListItem = UnityEngine.Object.Instantiate(PrefabManager.UIStageInfoBoxPrefab, content.transform);
        stageListItem.name = _UI_STAGE_INFO_BOX;
        stageListItem.transform.Find(_CHILD_TXT_STAGE_DISPLAY_NAME).GetComponent<TextMeshProUGUI>().text = stageinfo[0];
        
        // StreamingAssetsから画像を読み込み
        Sprite iconSprite = LoadSpriteFromStreamingAssets(stageinfo[1]);
        if (iconSprite != null)
        {
            stageListItem.transform.Find(_CHILD_IMG_STAGE_ICON).GetComponent<Image>().sprite = iconSprite;
        }
        
        stageListItem.transform.Find(_CHILD_TXT_STAGE_INFO).GetComponent<TextMeshProUGUI>().text = stageinfo[2];
        
        Transform txtStagePathTransform = stageListItem.transform.Find(_CHILD_TXT_STAGE_PATH);
        if (txtStagePathTransform != null)
        {
            Text txtStagePathComponent = txtStagePathTransform.GetComponent<Text>();
            if (txtStagePathComponent != null)
            {
                txtStagePathComponent.text = scenepath;
                txtStagePathComponent.enabled = false;
            }
        }
        
        RegisterChildButton(stageListItem, _CHILD_BTN_STAGE_PLAY, OnClickSelectStage);
    }

    // private void SetStageEditorContents(GameObject content)
    // {
    //     ClearContetArea(content);
    //     Dictionary<string, string[]> sceneDict = GetSceneDict();
    //     foreach (KeyValuePair<string, string[]> item in sceneDict)
    //     {
    //         string key = item.Key;
    //         string[] value = item.Value;
    //         SetStageEditorContent(content, key, value);
    //     }
    //     int count = sceneDict.Count;
    //     content.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, _STAGE_EDITOR_HEIGHT * count + _STAGE_SELECTOR_MARGIN * (count - 1));
    // }

    // private void SetStageEditorContent(GameObject content, string scenepath, string[] stageinfo)
    // {
    //     GameObject gameObject = UnityEngine.Object.Instantiate(PrefabManager.UIStageFileListPrefab, content.transform);
    //     gameObject.name = _INSTANCE_UI_STAGE_FILE_LIST;
    //     gameObject.transform.Find(_CHILD_TXT_STAGE_DISPLAY_NAME).GetComponent<TextMeshProUGUI>().text = stageinfo[0];
    //     gameObject.transform.Find(_CHILD_TXT_STAGE_PATH).GetComponent<Text>().text = scenepath;
    //     Button btn = gameObject.GetComponent<Button>();
    //     btn.onClick.AddListener(delegate
    //     {
    //         OnClickSelectEditStage(btn);
    //     });
    // }

    private void OnClickSelectEditStage(Button btnStageSelect)
    {
        Text component = btnStageSelect.transform.Find(_CHILD_TXT_STAGE_PATH).gameObject.GetComponent<Text>();
        if (component == null || component.text == "")
        {
            Debug.Log(_MSG_STAGE_PATH_NULL);
            return;
        }
        string fileName = Path.GetFileName(component.text + _YAML_FILE_EXTENSION);
        Debug.Log("OnClickSelectEditStage: " + fileName);
        string text = LoadStreamingAsset.AllTextStream(fileName);
        if (text == null)
        {
            Debug.Log(_MSG_YAML_NULL);
            return;
        }
        _pnlYAMLEditor.SetActive(value: true);
        GameObject gameObject = GameObject.Find(_OBJ_TMP_INPUT_FIELD);
        if (gameObject == null)
        {
            Debug.LogWarning(_MSG_TMP_INPUT_NOT_FOUND);
            return;
        }
        gameObject.GetComponent<TMP_InputField>().text = text;
        GameObject.Find(_OBJ_TXT_YAML_PATH).GetComponent<Text>().text = LoadStreamingAsset.StageFilePath(fileName);
    }

    private void OnClickSelectStage(Button btnStageSelect)
    {
        btnStageSelect.interactable = false;
        
        if (btnStageSelect.transform.parent == null)
        {
            Debug.LogError("btnStageSelect の親が見つかりません");
            return;
        }
        
        Transform txtStagePathTransform = btnStageSelect.transform.parent.Find(_CHILD_TXT_STAGE_PATH);
        if (txtStagePathTransform == null)
        {
            Debug.LogError(_MSG_STAGE_PATH_NULL);
            return;
        }
        
        Text component = txtStagePathTransform.GetComponent<Text>();
        if (component == null || component.text == "")
        {
            Debug.Log(_MSG_STAGE_PATH_NULL);
            return;
        }
        
        string text = component.text;
        if (IsScenePathValid(text))
        {
            _loading.SetActive(value: true);
            StartCoroutine(LoadScene(text, _SCENE_LOAD_DELAY));
        }
        else
        {
            Debug.LogWarning(_MSG_SCENE_PATH_NOT_FOUND + text);
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
        string[] csvLines = LoadStreamingAsset.CsvLines(_STAGE_LIST_FILE_NAME);
        
        if (csvLines == null)
        {
            Debug.LogError($"ステージリストファイルが見つかりません: {_STAGE_LIST_FILE_NAME}");
            Debug.LogError($"期待されるパス: {LoadStreamingAsset.StageFilePath(_STAGE_LIST_FILE_NAME)}");
            return dictionary;
        }
        
        for (int i = 0; i < csvLines.Length; i++)
        {
            string[] csvColumns = LoadStreamingAsset.CsvCols(csvLines[i]);
            if (csvColumns.Length != 4)
            {
                Debug.LogWarning(_MSG_INVALID_LINE_FORMAT + csvLines[i]);
                continue;
            }
            dictionary.Add(csvColumns[0], new string[3]
            {
                csvColumns[1],
                csvColumns[2],
                csvColumns[3]
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
            // プロジェクト、ビルドプロファイルのシーンリストからの取得
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
                // dictionary.Add(text2, array2);   // シーンマネージャーに登録されているものをすべて表示する場合は有効化
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
        TextMeshProUGUI component = GameObject.Find(_OBJ_TMP_GAME_INFO).GetComponent<TextMeshProUGUI>();
        string text = LoadStreamingAsset.AllTextStream(_ABOUT_GAME_FILE_NAME, LoadStreamingAsset._PUBLIC_DOC_SUB_FOLDER);
        if (text == null)
        {
            Debug.LogWarning(_MSG_ABOUT_GAME_NOT_FOUND);
            component.text = _MSG_ABOUT_GAME_ERROR;
            return;
        }
        Debug.Log(_MSG_ABOUT_GAME_LOADED);
        component.text = text;
    }

    private bool OpenYamlByNotePad()
    {
        string text = GameObject.Find(_OBJ_TXT_YAML_PATH).GetComponent<Text>().text;
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
                Debug.LogError(_MSG_FILE_OPEN_ERROR + filepath + "\nエラー: " + ex.Message);
                return;
            }
        }
        Debug.LogWarning(_MSG_FILE_NOT_EXIST + filepath);
    }

    private void OpenFileInEditor(string filepath)
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            Process.Start(_EDITOR_WINDOWS, filepath);
        }
        else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            Process.Start(_EDITOR_MAC, _EDITOR_MAC_ARGS + filepath);
        }
        else if (Application.platform == RuntimePlatform.LinuxPlayer)
        {
            Process.Start(_EDITOR_LINUX, filepath);
        }
        else
        {
            Debug.LogError(_MSG_UNSUPPORTED_PLATFORM);
        }
    }

    private void OnClickBugReport()
    {
        Debug.Log(_MSG_BUG_REPORT_LOG + _BUG_REPORT_URL);
        Application.OpenURL(_BUG_REPORT_URL);
    }

    private void OnClickYamlEdit()
    {
        OpenYamlByNotePad();
        // _pnlStageEditor.SetActive(value: false);
        _pnlYAMLEditor.SetActive(value: false);
    }

    private void OnClickStageEditor()
    {
        SetScrollbarTopPosition(_EditorScrollbar);
        _pnlStageSelector.SetActive(value: false);
        // _pnlStageEditor.SetActive(!_pnlStageEditor.activeSelf);
    }

    private void SetScrollbarTopPosition(GameObject scrollbar)
    {
        if (!(scrollbar == null))
        {
            scrollbar.GetComponent<Scrollbar>().value = _SCROLLBAR_TOP_VALUE;
        }
    }

    private void OnClickStageSelect()
    {
        SetScrollbarTopPosition(_StageScrollbar);
        _pnlStageSelector.SetActive(!_pnlStageSelector.activeSelf);
    }

    void Awake()
    {
        List<string> missingObjects = new List<string>();
        
        InitializeLoadingCanvas(missingObjects);
        InitializePanels(missingObjects);
        RegisterButtonListeners(missingObjects);
        InitializeStageContents(missingObjects);
        InitializeVersionInfo(missingObjects);
        InitializeNotice(missingObjects);
        CheckMissingObjects(missingObjects);
    }

    private void InitializeLoadingCanvas(List<string> missingObjects)
    {
        _loading = GameObject.Find(_OBJ_LOADING);
        if (_loading == null)
        {
            GameObject loadingPrefab = Resources.Load<GameObject>(_PREFAB_PATH_LOADING);
            if (loadingPrefab != null)
            {
                _loading = UnityEngine.Object.Instantiate(loadingPrefab);
                _loading.name = _OBJ_LOADING;
            }
            else
            {
                missingObjects.Add(_OBJ_LOADING);
                return;
            }
        }
        _loading.SetActive(value: false);
    }

    private void InitializePanels(List<string> missingObjects)
    {
        _pnlStageSelector = FindAndSetupPanel(_OBJ_PNL_STAGE_SELECTOR, missingObjects);
        _pnlAboutThisGame = FindAndSetupPanel(_OBJ_PNL_ABOUT_THIS_GAME, missingObjects);
        // _pnlYAMLEditor = FindAndSetupPanel(_OBJ_PNL_YAML_EDITOR, missingObjects);
        _pnlBottomBar = FindAndSetupPanel(_OBJ_PNL_BOTTOM_BAR, missingObjects, true);
    }

    private GameObject FindAndSetupPanel(string panelName, List<string> missingObjects, Boolean setActiveFalse = false)
    {
        GameObject panel = FindGameObject(panelName, missingObjects, "Panel");
        if (panel != null)
        {
            panel.SetActive(value: setActiveFalse);
        }
        return panel;
    }

    private void RegisterButtonListeners(List<string> missingObjects)
    {
        RegisterButton(_OBJ_BTN_GAME_CLOSE, OnClickGameClose, missingObjects);
        RegisterButton(_OBJ_BTN_ABOUT_GAME, OnClickAboutGame, missingObjects);
        RegisterButton(_OBJ_BTN_SELECT_STAGE, OnClickStageSelect, missingObjects);
        // RegisterButton(_OBJ_BTN_YAML_EDIT, OnClickYamlEdit, missingObjects);
        RegisterButton(_OBJ_BTN_BUG_REPORT, OnClickBugReport, missingObjects);
    }

    private void RegisterButton(string buttonName, UnityEngine.Events.UnityAction action, List<string> missingObjects)
    {
        GameObject buttonObj = FindGameObject(buttonName, missingObjects, "Button");
        if (buttonObj != null)
        {
            Button buttonComponent = buttonObj.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(action);
            }
        }
    }

    private void RegisterChildButton(GameObject parent, string childButtonName, System.Action<Button> action)
    {
        Transform btnTransform = parent.transform.Find(childButtonName);
        if (btnTransform == null)
        {
            Debug.LogWarning($"{childButtonName} が見つかりません");
            return;
        }

        Button buttonComponent = btnTransform.GetComponent<Button>();
        if (buttonComponent == null)
        {
            Debug.LogWarning($"{childButtonName} にButtonコンポーネントが見つかりません");
            return;
        }

        buttonComponent.onClick.AddListener(delegate
        {
            action(buttonComponent);
        });
    }

    private void InitializeStageContents(List<string> missingObjects)
    {
        if (_pnlStageSelector == null)
        {
            Debug.LogError("[InitializeStageContents] _pnlStageSelectorがnullです");
            missingObjects.Add("pnlStageSelector (parent is null)");
            return;
        }

        // Content要素の初期化
        GameObject contentObject = FindGameObject(_CHILD_PATH_SCROLLVIEW_CONTENT, missingObjects, "Content", _pnlStageSelector);
        if (contentObject != null)
        {
            SetStageContents(contentObject);
        }
        
        // Scrollbar要素の初期化
        _StageScrollbar = FindGameObject(_CHILD_PATH_SCROLLVIEW_SCROLLBAR, missingObjects, "Scrollbar", _pnlStageSelector);
        if (_StageScrollbar != null)
        {
            SetScrollbarTopPosition(_StageScrollbar);
        }
    }

    private GameObject FindGameObject(string path, List<string> missingObjects, string objectType = "", GameObject parent = null)
    {
        GameObject obj = null;
        
        if (parent != null)
        {
            Transform transform = parent.transform.Find(path);
            if (transform != null)
            {
                obj = transform.gameObject;
            }
        }
        else
        {
            obj = GameObject.Find(path);
        }
        
        if (obj == null)
        {
            string typeInfo = string.IsNullOrEmpty(objectType) ? "" : $" ({objectType})";
            string fullPath = parent != null ? $"{parent.name}/{path}" : path;
            Debug.LogError($"GameObject not found{typeInfo}: {fullPath}");
            missingObjects.Add(parent != null ? $"{parent.name}/{path}" : path);
        }
        
        return obj;
    }

    private void InitializeVersionInfo(List<string> missingObjects)
    {
        GameObject txtVersionInfo = FindGameObject(_OBJ_TXT_VERSION_INFO, missingObjects, "VersionInfo", _pnlBottomBar);
        if (txtVersionInfo != null)
        {
            TextMeshProUGUI textComponent = txtVersionInfo.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.SetText(GameObjectTreat.GetAppBuildDate());
            }
        }
    }

    private void InitializeNotice(List<string> missingObjects)
    {
        GameObject tmpNotice = FindGameObject(_OBJ_TMP_NOTICE, missingObjects, "NoticeText");
        if (tmpNotice != null)
        {
            TextMeshProUGUI textComponent = tmpNotice.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                string noticeText = LoadStreamingAsset.AllTextStream(_NOTICE_FILE_NAME, LoadStreamingAsset._PUBLIC_DOC_SUB_FOLDER);
                if (noticeText == null)
                {
                    Debug.LogWarning(_MSG_NOTICE_NOT_FOUND);
                    textComponent.text = _MSG_NOTICE_ERROR;
                    return;
                }
                Debug.Log(_MSG_NOTICE_LOADED);
                textComponent.text = noticeText;
            }
        }
    }

    private void CheckMissingObjects(List<string> missingObjects)
    {
        if (missingObjects.Count > 0)
        {
            Debug.LogError("=== 以下のGameObjectがシーンに存在しません ===");
            foreach (string objName in missingObjects)
            {
                Debug.LogError("  - " + objName);
            }
            Debug.LogError("=== 合計 " + missingObjects.Count + " 個のオブジェクトが不足しています ===");
        }
    }

    private Sprite LoadSpriteFromStreamingAssets(string imagePath)
    {
        // 拡張子がすでに含まれているか確認
        string fullPath = imagePath;
        if (Path.HasExtension(imagePath))
        {
            // 拡張子付きの場合はそのまま使用
            fullPath = LoadStreamingAsset.StageFilePath(imagePath);
        }
        // else
        // {
        //     // 拡張子がない場合は.pngを追加
        //     fullPath = LoadStreamingAsset.StageFilePath(imagePath + ".png");
        // }
        
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"画像ファイルが見つかりません: {fullPath}");
            return null;
        }

        try
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                return sprite;
            }
            else
            {
                Debug.LogWarning($"画像の読み込みに失敗しました: {fullPath}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"画像読み込みエラー: {fullPath}\n{ex.Message}");
            return null;
        }
    }

}
