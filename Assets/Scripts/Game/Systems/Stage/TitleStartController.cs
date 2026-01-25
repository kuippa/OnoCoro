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

public class TitleStartController : MonoBehaviour
{
    // URL Constants
    private const string _BUG_REPORT_URL = "https://forms.gle/CdvySsxeXMDAigTQ9";
    private const float _STAGE_SELECTOR_HEIGHT = 100f;
    private const float _STAGE_SELECTOR_MARGIN = 5f;

    // GameObject Names (used multiple times)
    private const string _OBJ_PNL_STAGE_SELECTOR = "pnlStageSelector";
    private const string _OBJ_PNL_ABOUT_THIS_GAME = "pnlAboutThisGame";
    private const string _OBJ_PNL_BOTTOM_BAR = "pnlBottomBar";
    private const string _OBJ_PNL_STAGE_EDITOR = "pnlStageEditor";
    private const string _OBJ_TMP_YAML_PATH = "tmpYamlPath";
   
    // Prefab Child Element Names
    private const string _CHILD_TXT_STAGE_DISPLAY_NAME = "txtStageDisplayName";
    private const string _CHILD_IMG_STAGE_ICON = "imgStageIcon";
    private const string _CHILD_TXT_STAGE_INFO = "txtStageInfo";
    private const string _CHILD_TXT_STAGE_PATH = "txtStagePath";
    private const string _CHILD_BTN_STAGE_PLAY = "btnStagePlay";
    private const string _CHILD_BTN_STAGE_EDITOR = "btnStageEditor";

    
    // UI Child Paths (relative to parent)
    private const string _CHILD_PATH_SCROLLVIEW_CONTENT = "Scroll View/Viewport/Content";
    
    // UI Messages
    private const string _MSG_SCENE_PATH_NOT_FOUND = "Scene path not found: ";
    private const string _MSG_ABOUT_GAME_NOT_FOUND = "aboutthisgame.txt is not found";
    private const string _MSG_ABOUT_GAME_ERROR = "ゲーム情報ファイルが見つかりませんでした。";
    private const string _MSG_ABOUT_GAME_LOADED = "aboutthisgame loaded";
    private const string _MSG_NOTICE_NOT_FOUND = "notice.txt is not found";
    private const string _MSG_NOTICE_ERROR = "お知らせファイルが見つかりませんでした。";
    private const string _MSG_NOTICE_LOADED = "notice loaded";
    private const string _MSG_BUG_REPORT_LOG = "OnClickBugReport ";
    
    // Numeric Constants
    private const float _SCENE_LOAD_DELAY = 0.1f;
    
    // Private Fields
    private GameObject _pnlStageSelector;
    private GameObject _pnlAboutThisGame;
    private GameObject _pnlStageEditor;
    private GameObject _pnlStageEditorBottom;
    private GameObject _pnlBottomBar;
    private GameObject _loading;

    private void OnClickGameClose()
    {
        GameManager gameManager = this.gameObject.AddComponent<GameManager>();
        gameManager.GameClose();
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
        StageRepository.GetSceneNames();
        Dictionary<string, string[]> sceneDict = StageRepository.GetSceneDict();
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
            return;
        }
        GameObject stageListItem = UnityEngine.Object.Instantiate(PrefabManager.UIStageInfoBoxPrefab, content.transform);
        stageListItem.name = "UIStageInfoBox";
        stageListItem.transform.Find(_CHILD_TXT_STAGE_DISPLAY_NAME).GetComponent<TextMeshProUGUI>().text = stageinfo[0];
        
        // StreamingAssetsから画像を読み込み
        Sprite iconSprite = FileOperationUtility.LoadSpriteFromStreamingAssets(stageinfo[1]);
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
        
        UIHelper.RegisterChildButton(stageListItem, _CHILD_BTN_STAGE_PLAY, OnClickStagePlay);
        UIHelper.RegisterChildButton(stageListItem, _CHILD_BTN_STAGE_EDITOR, OnClickStageEditor);
        // YAMLファイルがない場合はエディタボタンを無効化
        SetEditorButtonState(stageListItem, scenepath);
    }

    private void SetEditorButtonState(GameObject stageListItem, string scenepath)
    {
        bool yamlExists = LoadStreamingAsset.YamlFileExists(scenepath);
        if (!yamlExists)
        {
            Transform btnEditorTransform = stageListItem.transform.Find(_CHILD_BTN_STAGE_EDITOR);
            if (btnEditorTransform != null)
            {
                Button btnEditor = btnEditorTransform.GetComponent<Button>();
                if (btnEditor != null)
                {
                    btnEditor.interactable = false;
                }
            }
        }
    }

    private void OnClickStageEditor(Button btnStageEditor)
    {
        if (btnStageEditor.transform.parent == null)
        {
            return;
        }
        
        Transform txtStagePathTransform = btnStageEditor.transform.parent.Find(_CHILD_TXT_STAGE_PATH);
        if (txtStagePathTransform == null)
        {
            return;
        }
        
        Text component = txtStagePathTransform.GetComponent<Text>();
        if (component == null || component.text == "")
        {
            return;
        }
        
        string fileName = LoadStreamingAsset.GetYamlFileName(component.text);
        LogUtility.Debug("OnClickStageEdit: " + fileName);
        string text = LoadStreamingAsset.AllTextStream(fileName);
        if (text == null)
        {
            return;
        }
        _pnlStageEditor.SetActive(value: true);

        GameObject gameObject = GameObject.Find("tmpYAMLView");
        if (gameObject == null)
        {
            return;
        }
        gameObject.GetComponent<TextMeshProUGUI>().text = text;
        GameObject yamlPathObject = GameObject.Find(_OBJ_TMP_YAML_PATH);
        if (yamlPathObject != null)
        {
            TMP_InputField inputField = yamlPathObject.GetComponent<TMP_InputField>();
            if (inputField != null)
            {
                inputField.text = LoadStreamingAsset.StageFilePath(fileName);
                inputField.readOnly = true;
                inputField.interactable = true;
            }
        }
    }

    private void OnClickStagePlay(Button btnStagePlay)
    {
        btnStagePlay.interactable = false;
        if (btnStagePlay.transform.parent == null)
        {
            return;
        }
        Transform txtStagePathTransform = btnStagePlay.transform.parent.Find(_CHILD_TXT_STAGE_PATH);
        if (txtStagePathTransform == null)
        {
            return;
        }
        Text component = txtStagePathTransform.GetComponent<Text>();
        if (component == null || component.text == "")
        {
            return;
        }
        string text = component.text;
        if (StageRepository.IsScenePathValid(text))
        {
            _loading.SetActive(value: true);
            StartCoroutine(SceneLoaderManager.LoadSceneWithDelay(text, _SCENE_LOAD_DELAY));
        }
        else
        {
            LogUtility.Warning(_MSG_SCENE_PATH_NOT_FOUND + text);
        }
    }

    private void OnClickAboutGame()
    {
        _pnlAboutThisGame.SetActive(value: true);
        TextMeshProUGUI component = GameObject.Find("tmpGameInfo").GetComponent<TextMeshProUGUI>();
        string text = LoadStreamingAsset.AllTextStream(LoadStreamingAsset.ABOUT_GAME_FILE_NAME, LoadStreamingAsset._PUBLIC_DOC_SUB_FOLDER);
        if (text == null)
        {
            LogUtility.Warning(_MSG_ABOUT_GAME_NOT_FOUND);
            component.text = _MSG_ABOUT_GAME_ERROR;
            return;
        }
        LogUtility.Debug(_MSG_ABOUT_GAME_LOADED);
        component.text = text;
        UIHelper.ResetScrollbarInPanel(_pnlAboutThisGame);
    }

    private void InitializeTextFromFile(TextMeshProUGUI textComponent, string fileName, string notFoundMessage, string errorMessage, string loadedMessage)
    {
        if (textComponent == null) return;
        
        string text = LoadStreamingAsset.AllTextStream(fileName, LoadStreamingAsset._PUBLIC_DOC_SUB_FOLDER);
        if (text == null)
        {
            LogUtility.Warning(notFoundMessage);
            textComponent.text = errorMessage;
            return;
        }
        LogUtility.Debug(loadedMessage);
        textComponent.text = text;
    }

    private bool OpenYamlByNotePad()
    {
        GameObject yamlPathObject = GameObject.Find("tmpYamlPath");
        if (yamlPathObject == null)
        {
            return false;
        }
        
        TMP_InputField inputField = yamlPathObject.GetComponent<TMP_InputField>();
        if (inputField == null || string.IsNullOrEmpty(inputField.text))
        {
            return false;
        }
        
        string path = inputField.text;
        if (File.Exists(path))
        {
            FileOperationUtility.OpenFileInEditor(path);
            return true;
        }
        return false;
    }

    private void OnClickBugReport()
    {
        LogUtility.Debug(_MSG_BUG_REPORT_LOG + _BUG_REPORT_URL);
        Application.OpenURL(_BUG_REPORT_URL);
    }

    private void OnClickYamlEdit()
    {
        OpenYamlByNotePad();
        _pnlStageEditor.SetActive(value: false);
    }

    private void OnClickStageEditor()
    {
        _pnlStageSelector.SetActive(value: false);
        _pnlStageEditor.SetActive(!_pnlStageEditor.activeSelf);
        UIHelper.ResetScrollbarInPanel(_pnlStageEditor);
    }

    private void OnClickStageSelect()
    {
        _pnlStageSelector.SetActive(!_pnlStageSelector.activeSelf);
        UIHelper.ResetScrollbarInPanel(_pnlStageSelector);
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
        _loading = UIHelper.FindOrInstantiatePrefab("nowloading", UIHelper.PREFAB_PATH_LOADING, missingObjects, setActiveFalse: true);
    }

    private void InitializePanels(List<string> missingObjects)
    {
        _pnlStageSelector = UIHelper.FindAndSetupPanel(_OBJ_PNL_STAGE_SELECTOR, missingObjects);
        _pnlAboutThisGame = UIHelper.FindAndSetupPanel(_OBJ_PNL_ABOUT_THIS_GAME, missingObjects);
        _pnlStageEditor = UIHelper.FindAndSetupPanel(_OBJ_PNL_STAGE_EDITOR, missingObjects);
        _pnlBottomBar = UIHelper.FindAndSetupPanel(_OBJ_PNL_BOTTOM_BAR, missingObjects, true);
        if (_pnlStageEditor != null)
        {
            _pnlStageEditorBottom = UIHelper.FindGameObject("pnlStageEditorBottom", missingObjects, "Panel", _pnlStageEditor);
        }
    }

    private void RegisterButtonListeners(List<string> missingObjects)
    {
        UIHelper.RegisterButton("btnGameClose", OnClickGameClose, missingObjects);
        UIHelper.RegisterButton("btnAboutGame", OnClickAboutGame, missingObjects);
        UIHelper.RegisterButton("btnSelectStage", OnClickStageSelect, missingObjects);
        UIHelper.RegisterButton("btnBugReport", OnClickBugReport, missingObjects, _pnlBottomBar);
        UIHelper.RegisterButton("btnYamlEdit", OnClickYamlEdit, missingObjects, _pnlStageEditorBottom);
    }

    private void InitializeStageContents(List<string> missingObjects)
    {
        if (_pnlStageSelector == null)
        {
            missingObjects.Add("pnlStageSelector (parent is null)");
            return;
        }

        // Content要素の初期化
        GameObject contentObject = UIHelper.FindGameObject(_CHILD_PATH_SCROLLVIEW_CONTENT, missingObjects, "Content", _pnlStageSelector);
        if (contentObject != null)
        {
            SetStageContents(contentObject);
        }
        
        // Scrollbar要素の初期化
        UIHelper.ResetScrollbarInPanel(_pnlStageSelector);
    }

    private void InitializeVersionInfo(List<string> missingObjects)
    {
        TextMeshProUGUI textComponent = UIHelper.FindTextComponent("txtVersionInfo", missingObjects, "VersionInfo", _pnlBottomBar);
        if (textComponent != null)
        {
            textComponent.SetText(GameObjectTreat.GetAppBuildDate());
        }
    }

    private void InitializeNotice(List<string> missingObjects)
    {
        TextMeshProUGUI textComponent = UIHelper.FindTextComponent("tmpNotice", missingObjects, "NoticeText");
        InitializeTextFromFile(textComponent, LoadStreamingAsset.NOTICE_FILE_NAME, _MSG_NOTICE_NOT_FOUND, _MSG_NOTICE_ERROR, _MSG_NOTICE_LOADED);
    }

    private void CheckMissingObjects(List<string> missingObjects)
    {
        if (missingObjects.Count > 0)
        {
            LogUtility.Error("=== 以下のGameObjectがシーンに存在しません ===");
            foreach (string objName in missingObjects)
            {
                LogUtility.Error("  - " + objName);
            }
        }
    }

}
