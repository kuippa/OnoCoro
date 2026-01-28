using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

/// <summary>
/// コンポーネント初期化の順序制御を行うマネージャー

/// Unity Editor メニュー → Edit (編集) → Project Settings (プロジェクト設定)
///  → Script Execution Order で -10 に設定。スクリプト中最優先で呼び出される。
/// 
/// 全ての Awake が完了した後に、制御された順序で初期化を実行します。
/// 
/// GamePrefabs（親オブジェクト）からの準備完了通知を受け取り、
/// EventLoader / YamlLoader などの依存コンポーネントに伝播させます。
/// 
/// 使用方法:
/// 1. GamePrefabs オブジェクト（シーンに配置済み）にこのスクリプトをアタッチ
/// 2. Script Execution Orderで最優先(-100)に設定
/// 3. GamePrefabs の準備完了後、NotifyGamePrefabsReady() を手動呼び出しするか、
///    親オブジェクトの OnEnable() 後に自動呼び出し
/// 4. 各コンポーネントで IsInitialized フラグを待機して処理を開始
/// 5. EventLoader / YamlLoader は OnGamePrefabsReady を購読
/// 
/// 詳細は InitializationManager_Usage.md / phase-1-5-initialization-order-design.md を参照
/// </summary>
internal class InitializationManager : MonoBehaviour
{
    private static InitializationManager _instance;
    
    /// <summary>
    /// 全初期化が完了したかどうか
    /// </summary>
    private bool _isInitialized = false;
    
    /// <summary>
    /// GamePrefabs の準備が完了したかどうか
    /// EventLoader / YamlLoader がこのフラグを確認して処理開始
    /// </summary>
    private static bool _isGamePrefabsReady = false;
    
    /// <summary>
    /// 各ステップの初期化状態
    /// </summary>
    private Dictionary<string, bool> _initializationSteps = new Dictionary<string, bool>();
    
    /// <summary>
    /// GamePrefabs 準備完了イベント
    /// EventLoader / YamlLoader はこのイベントを購読
    /// </summary>
    internal static event System.Action OnGamePrefabsReady;
    
    /// <summary>
    /// 全初期化が完了しているか
    /// </summary>
    internal static bool IsInitialized => _instance != null && _instance._isInitialized;
    
    /// <summary>
    /// GamePrefabs が準備完了しているか
    /// </summary>
    internal static bool IsGamePrefabsReady => _isGamePrefabsReady;
    
    /// <summary>
    /// シングルトンインスタンスの取得
    /// </summary>
    internal static InitializationManager Instance => _instance;
    
    /// <summary>
    /// 特定のステップが初期化完了しているか確認
    /// </summary>
    /// <param name="stepName">ステップ名</param>
    /// <returns>初期化完了している場合はtrue</returns>
    internal static bool IsStepInitialized(string stepName)
    {
        if (_instance == null)
        {
            return false;
        }
        
        return _instance._initializationSteps.ContainsKey(stepName) 
            && _instance._initializationSteps[stepName];
    }
    
    /// <summary>
    /// GamePrefabs が準備完了したことを通知
    /// GamePrefabs.cs から呼び出される
    /// </summary>
    internal static void NotifyGamePrefabsReady()
    {
        _isGamePrefabsReady = true;
        Debug.Log("[InitializationManager] GamePrefabs 初期化完了 - Event 発火");
        OnGamePrefabsReady?.Invoke();
    }
    
    /// <summary>
    /// GamePrefabs の準備完了を確認（安全弁）
    /// まだ初期化されていない場合は例外を投げる
    /// </summary>
    internal static void WaitForGamePrefabsReady()
    {
        if (!_isGamePrefabsReady)
        {
            throw new System.InvalidOperationException(
                "[InitializationManager] GamePrefabs がまだ初期化されていません");
        }
    }
    
    private void Awake()
    {
        // シングルトンパターン
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        // 初期化ステップの登録
        RegisterInitializationSteps();
    }
    
    /// <summary>
    /// 初期化ステップの登録
    /// </summary>
    private void RegisterInitializationSteps()
    {
        // Phase 1.5: GamePrefabs初期化レース条件の修正に特化
        // EventLoader/YamlLoader が GamePrefabs 準備完了まで待機する仕組み
        _initializationSteps["EventLoader"] = false;
        _initializationSteps["YamlLoader"] = false;
    }
    
    private IEnumerator Start()
    {
        // 全ての Awake が完了するまで1フレーム待機
        yield return null;
        
        Debug.Log("[InitializationManager] 初期化開始");
        
        // GamePrefabs（親オブジェクト）が準備完了したことを通知
        // このタイミングで EventLoader / YamlLoader が起動可能に
        NotifyGamePrefabsReady();
        
        // 順序制御された初期化処理を実行
        yield return InitializeAllComponents();
        
        // 全初期化完了フラグを立てる
        _isInitialized = true;
        Debug.Log("[InitializationManager] 全ての初期化が完了しました");
    }
    
    /// <summary>
    /// 全てのコンポーネントを順序制御して初期化
    /// GamePrefabs の準備完了後、その他のコンポーネント初期化を進める
    /// </summary>
    private IEnumerator InitializeAllComponents()
    {
        // ステップ1: リソース読み込み系の初期化（必要に応じて）
        yield return InitializeResourceLoaders();
        
        // ステップ2: マネージャークラスの初期化（FireCubeCtrl など）
        yield return InitializeManagers();
        
        // ステップ3: UI系の初期化（Phase 1.4 で実装）
        yield return InitializeUIComponents();
    }
    
    /// <summary>
    /// リソースローダーの初期化
    /// 現在は最小限（EventLoader/YamlLoader が別途管理）
    /// </summary>
    private IEnumerator InitializeResourceLoaders()
    {
        Debug.Log("[InitializationManager] リソース初期化中...");
        yield return null;
    }
    
    /// <summary>
    /// 各種マネージャークラスの初期化
    /// Phase 1.5 では GamePrefabs のコントローラーを動的に検出・監視
    /// 
    /// 動的管理により：
    /// - 新規コントローラー追加時に InitializationManager を修正不要
    /// - コントローラー削除時に自動的に反映
    /// - GetComponentsInChildren で実行時に検出
    /// </summary>
    private IEnumerator InitializeManagers()
    {
        Debug.Log("[InitializationManager] GamePrefabs のコントローラーを自動検出");
        
        // GamePrefabs オブジェクト取得
        // GameObjectTreat は Presentation/View にあるため注意
        GameObject gamePrefabsObj = GameObjectTreat.GetGameManagerObject();
        if (gamePrefabsObj == null)
        {
            Debug.LogWarning("[InitializationManager] GamePrefabs が見つかりません");
            yield break;
        }
        
        // [1] IInitializable を実装したすべてのコンポーネントを検出
        // GetComponentsInChildren は Awake 完了後であれば確実に検出可能
        IInitializable[] controllers = gamePrefabsObj.GetComponentsInChildren<IInitializable>();
        
        if (controllers.Length == 0)
        {
            Debug.LogWarning("[InitializationManager] IInitializable を実装したコンポーネントがありません");
            yield break;
        }
        
        Debug.Log($"[InitializationManager] {controllers.Length} 個のコントローラーを検出しました");
        
        // [2] 各コントローラーの初期化完了を個別に監視
        foreach (IInitializable controller in controllers)
        {
            // 個別に初期化完了を待機
            yield return new WaitUntil(() => controller.IsInitialized);
            
            string componentName = controller.GetComponentName();
            MarkStepAsInitialized(componentName);
            Debug.Log($"[InitializationManager] [OK] {componentName} 初期化完了");
        }
        
        Debug.Log("[InitializationManager] すべてのコントローラー初期化完了");
    }
    
    /// <summary>
    /// UIコンポーネントの初期化
    /// Phase 1.4 で UI 改善時に実装
    /// </summary>
    private IEnumerator InitializeUIComponents()
    {
        Debug.Log("[InitializationManager] UI初期化中...");
        yield return null;
    }
    
    /// <summary>
    /// 初期化ステップを完了としてマーク
    /// </summary>
    /// <param name="stepName">ステップ名</param>
    private void MarkStepAsInitialized(string stepName)
    {
        if (_initializationSteps.ContainsKey(stepName))
        {
            _initializationSteps[stepName] = true;
            Debug.Log($"[InitializationManager] {stepName} の初期化が完了しました");
        }
        else
        {
            Debug.LogWarning($"[InitializationManager] ステップ '{stepName}' が登録されていません");
        }
    }
    
    /// <summary>
    /// シーン遷移時などに初期化状態をリセット
    /// </summary>
    internal void ResetInitialization()
    {
        _isInitialized = false;
        _isGamePrefabsReady = false;
        
        foreach (string key in new List<string>(_initializationSteps.Keys))
        {
            _initializationSteps[key] = false;
        }
        
        Debug.Log("[InitializationManager] 初期化状態をリセットしました");
    }
}
