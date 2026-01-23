using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コンポーネント初期化の順序制御を行うマネージャー
/// 全ての Awake が完了した後に、制御された順序で初期化を実行します。
/// 
/// 使用方法:
/// 1. シーンに空のGameObjectを作成し、このスクリプトをアタッチ
/// 2. Script Execution Orderで最優先(-100など)に設定することを推奨
/// 3. 各コンポーネントで IsInitialized フラグを待機して処理を開始
/// 
/// 詳細は InitializationManager_Usage.md を参照してください。
/// </summary>
public class InitializationManager : MonoBehaviour
{
    private static InitializationManager instance;
    
    /// <summary>
    /// 初期化が完了したかどうか
    /// </summary>
    private bool isInitialized = false;
    
    /// <summary>
    /// 各ステップの初期化状態
    /// </summary>
    private Dictionary<string, bool> initializationSteps = new Dictionary<string, bool>();
    
    /// <summary>
    /// 初期化が完了しているか
    /// </summary>
    public static bool IsInitialized => instance != null && instance.isInitialized;
    
    /// <summary>
    /// シングルトンインスタンスの取得
    /// </summary>
    public static InitializationManager Instance => instance;
    
    /// <summary>
    /// 特定のステップが初期化完了しているか確認
    /// </summary>
    /// <param name="stepName">ステップ名</param>
    /// <returns>初期化完了している場合はtrue</returns>
    public static bool IsStepInitialized(string stepName)
    {
        if (instance == null) return false;
        return instance.initializationSteps.ContainsKey(stepName) && instance.initializationSteps[stepName];
    }
    
    private void Awake()
    {
        // シングルトンパターン
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // 初期化ステップの登録
        RegisterInitializationSteps();
    }
    
    private void RegisterInitializationSteps()
    {
        // 必要な初期化ステップをここに登録
        // 例: initializationSteps["ResourceLoader"] = false;
        //     initializationSteps["AudioManager"] = false;
        //     initializationSteps["GameManager"] = false;
    }
    
    private IEnumerator Start()
    {
        // 全ての Awake が完了するまで1フレーム待機
        // これにより、全てのMonoBehaviourのAwakeメソッドが確実に実行される
        yield return null;
        
        // 順序制御された初期化処理を実行
        yield return InitializeAllComponents();
        
        // 初期化完了フラグを立てる
        isInitialized = true;
        
        Debug.Log("[InitializationManager] 全ての初期化が完了しました");
    }
    
    /// <summary>
    /// 全てのコンポーネントを順序制御して初期化
    /// </summary>
    private IEnumerator InitializeAllComponents()
    {
        // ステップ1: リソース読み込み系の初期化
        yield return InitializeResourceLoaders();
        
        // ステップ2: マネージャークラスの初期化
        yield return InitializeManagers();
        
        // ステップ3: UI系の初期化
        yield return InitializeUIComponents();
        
        // 必要に応じてステップを追加
    }
    
    /// <summary>
    /// リソースローダーの初期化
    /// </summary>
    private IEnumerator InitializeResourceLoaders()
    {
        Debug.Log("[InitializationManager] リソースローダーを初期化中...");
        
        // 例: ResourceLoaderの初期化
        // ResourceLoader loader = FindObjectOfType<ResourceLoader>();
        // if (loader != null)
        // {
        //     yield return loader.Initialize();
        //     MarkStepAsInitialized("ResourceLoader");
        // }
        
        yield return null;
    }
    
    /// <summary>
    /// 各種マネージャークラスの初期化
    /// </summary>
    private IEnumerator InitializeManagers()
    {
        Debug.Log("[InitializationManager] マネージャーを初期化中...");
        
        // 例: GameManagerの初期化
        // GameObject gameManagerObj = GameObjectTreat.GetGameManagerObject();
        // if (gameManagerObj != null)
        // {
        //     // 必要なコンポーネントの初期化を待つ
        //     var ctrl = gameManagerObj.GetComponent<SomeCtrl>();
        //     if (ctrl != null)
        //     {
        //         yield return new WaitUntil(() => ctrl.IsInitialized);
        //         MarkStepAsInitialized("GameManager");
        //     }
        // }
        
        yield return null;
    }
    
    /// <summary>
    /// UIコンポーネントの初期化
    /// </summary>
    private IEnumerator InitializeUIComponents()
    {
        Debug.Log("[InitializationManager] UIコンポーネントを初期化中...");
        
        // UI関連の初期化処理
        
        yield return null;
    }
    
    /// <summary>
    /// 初期化ステップを完了としてマーク
    /// </summary>
    /// <param name="stepName">ステップ名</param>
    private void MarkStepAsInitialized(string stepName)
    {
        if (initializationSteps.ContainsKey(stepName))
        {
            initializationSteps[stepName] = true;
            Debug.Log($"[InitializationManager] {stepName} の初期化が完了しました");
        }
    }
    
    /// <summary>
    /// シーン遷移時などに初期化状態をリセット
    /// </summary>
    public void ResetInitialization()
    {
        isInitialized = false;
        foreach (var key in new List<string>(initializationSteps.Keys))
        {
            initializationSteps[key] = false;
        }
        Debug.Log("[InitializationManager] 初期化状態をリセットしました");
    }
}
