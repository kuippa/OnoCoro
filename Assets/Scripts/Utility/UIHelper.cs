using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// Unity UI操作に関する汎用ヘルパークラス
/// - GameObject検索とエラーハンドリング
/// - ボタンイベント登録
/// - スクロールバー制御
/// </summary>
public static class UIHelper
{
    // Prefab Paths
    public const string PREFAB_PATH_LOADING = "Prefabs/UI/nowloading";
    
    private const float SCROLLBAR_TOP_VALUE = 1f;
    
    /// <summary>
    /// GameObjectを検索し、見つからない場合はエラーログを出力
    /// </summary>
    /// <param name="path">GameObjectの名前またはパス</param>
    /// <param name="missingObjects">見つからないオブジェクトを追加するリスト</param>
    /// <param name="objectType">オブジェクトタイプ（エラーメッセージ用）</param>
    /// <param name="parent">親GameObject（指定時は親配下を検索）</param>
    /// <returns>見つかったGameObject、見つからない場合はnull</returns>
    public static GameObject FindGameObject(string path, List<string> missingObjects, string objectType = "", GameObject parent = null)
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
    
    /// <summary>
    /// ボタンを検索してイベントリスナーを登録
    /// </summary>
    /// <param name="buttonName">ボタンの名前</param>
    /// <param name="action">登録するアクション</param>
    /// <param name="missingObjects">見つからないオブジェクトを追加するリスト</param>
    /// <param name="parent">親GameObject（指定時は親配下を検索）</param>
    public static void RegisterButton(string buttonName, UnityEngine.Events.UnityAction action, List<string> missingObjects, GameObject parent = null)
    {
        GameObject buttonObj = FindGameObject(buttonName, missingObjects, "Button", parent);
        if (buttonObj != null)
        {
            Button buttonComponent = buttonObj.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(action);
            }
        }
    }
    
    /// <summary>
    /// 親オブジェクト配下の子ボタンを検索してイベントリスナーを登録
    /// </summary>
    /// <param name="parent">親GameObject</param>
    /// <param name="childButtonName">子ボタンの名前</param>
    /// <param name="action">登録するアクション（Buttonを引数に取る）</param>
    public static void RegisterChildButton(GameObject parent, string childButtonName, System.Action<Button> action)
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
    
    /// <summary>
    /// スクロールバーを最上部の位置に設定
    /// </summary>
    /// <param name="scrollbar">設定するScrollbarのGameObject</param>
    public static void SetScrollbarTopPosition(GameObject scrollbar)
    {
        if (scrollbar == null)
        {
            return;
        }
        
        Scrollbar scrollbarComponent = scrollbar.GetComponent<Scrollbar>();
        if (scrollbarComponent != null)
        {
            scrollbarComponent.value = SCROLLBAR_TOP_VALUE;
        }
    }
    
    /// <summary>
    /// 親オブジェクト配下のScrollRectコンポーネントを検索してリセット（最上部に設定）
    /// </summary>
    /// <param name="parentPanel">ScrollRectを含む親のGameObject（パネルなど）</param>
    public static void ResetScrollbarInPanel(GameObject parentPanel)
    {
        if (parentPanel == null || !parentPanel.activeSelf)
        {
            return;
        }
        
        ScrollRect scrollRect = parentPanel.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
    }
    
    /// <summary>
    /// パネルを検索してセットアップ（表示/非表示の設定）
    /// </summary>
    /// <param name="panelName">パネルの名前</param>
    /// <param name="missingObjects">見つからないオブジェクトを追加するリスト</param>
    /// <param name="setActiveFalse">falseに設定する場合はtrue</param>
    /// <returns>見つかったパネルのGameObject</returns>
    public static GameObject FindAndSetupPanel(string panelName, List<string> missingObjects, bool setActiveFalse = false)
    {
        GameObject panel = FindGameObject(panelName, missingObjects, "Panel");
        if (panel != null)
        {
            panel.SetActive(setActiveFalse);
        }
        return panel;
    }
    
    /// <summary>
    /// GameObjectを検索してコンポーネントを取得（nullチェック込み）
    /// </summary>
    /// <typeparam name="T">取得するコンポーネントの型</typeparam>
    /// <param name="objectName">GameObjectの名前またはパス</param>
    /// <param name="missingObjects">見つからないオブジェクトを追加するリスト</param>
    /// <param name="objectType">オブジェクトタイプ（エラーメッセージ用）</param>
    /// <param name="parent">親GameObject（指定時は親配下を検索）</param>
    /// <returns>見つかったコンポーネント、見つからない場合はnull</returns>
    public static T FindAndGetComponent<T>(string objectName, List<string> missingObjects, string objectType = "", GameObject parent = null) where T : Component
    {
        GameObject obj = FindGameObject(objectName, missingObjects, objectType, parent);
        if (obj == null)
        {
            return null;
        }
        
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            string typeInfo = string.IsNullOrEmpty(objectType) ? "" : $" ({objectType})";
            string fullPath = parent != null ? $"{parent.name}/{objectName}" : objectName;
            Debug.LogError($"Component {typeof(T).Name} not found on{typeInfo}: {fullPath}");
            missingObjects.Add($"{fullPath} [Missing {typeof(T).Name}]");
        }
        
        return component;
    }
    
    /// <summary>
    /// GameObjectを検索してTextMeshProUGUIコンポーネントを取得（頻出パターンの専用メソッド）
    /// </summary>
    /// <param name="objectName">GameObjectの名前またはパス</param>
    /// <param name="missingObjects">見つからないオブジェクトを追加するリスト</param>
    /// <param name="objectType">オブジェクトタイプ（エラーメッセージ用）</param>
    /// <param name="parent">親GameObject（指定時は親配下を検索）</param>
    /// <returns>見つかったTextMeshProUGUIコンポーネント、見つからない場合はnull</returns>
    public static TMPro.TextMeshProUGUI FindTextComponent(string objectName, List<string> missingObjects, string objectType = "Text", GameObject parent = null)
    {
        return FindAndGetComponent<TMPro.TextMeshProUGUI>(objectName, missingObjects, objectType, parent);
    }
    
    /// <summary>
    /// GameObjectを検索し、見つからない場合はPrefabから生成する
    /// </summary>
    /// <param name="objectName">検索するGameObjectの名前</param>
    /// <param name="prefabPath">Prefabのリソースパス（Resources.Loadで使用）</param>
    /// <param name="missingObjects">見つからないオブジェクトを追加するリスト</param>
    /// <param name="setActiveFalse">生成後にSetActive(false)を実行する場合はtrue</param>
    /// <returns>見つかったまたは生成されたGameObject、失敗した場合はnull</returns>
    public static GameObject FindOrInstantiatePrefab(string objectName, string prefabPath, List<string> missingObjects, bool setActiveFalse = false)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab != null)
            {
                obj = UnityEngine.Object.Instantiate(prefab);
                obj.name = objectName;
                Debug.Log($"Instantiated {objectName} from prefab: {prefabPath}");
            }
            else
            {
                Debug.LogError($"Prefab not found: {prefabPath}");
                missingObjects.Add($"{objectName} [Prefab: {prefabPath}]");
                return null;
            }
        }
        
        if (setActiveFalse)
        {
            obj.SetActive(false);
        }
        
        return obj;
    }
}
