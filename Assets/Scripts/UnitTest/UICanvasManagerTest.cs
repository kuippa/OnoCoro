using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// プレファブ選択用 Enum
/// 
/// プルダウンメニューから選択可能な UI プレファブ一覧
/// </summary>
public enum UIPrefabPreset
{
    [InspectorName("EscMenu")]
    EscMenu,
    
    [InspectorName("TabMenu")]
    TabMenu,
    
    [InspectorName("DebugInfo")]
    DebugInfo,
    
    [InspectorName("Notice")]
    Notice,

    [InspectorName("UIItemCreate")]
    UIItemCreate,
    
}

/// <summary>
/// UICanvasManager テストスクリプト
/// 
/// 目的:
/// - UICanvasManager.ApplyStandardScalerSettings() の動作検証
/// - プレファブ読み込み時の Canvas Scaler 設定確認
/// - 複数解像度対応時のスケーリング検証
/// 
/// 使用方法:
/// 1. UnitTest フォルダに配置
/// 2. Scene に空の GameObject を作成
/// 3. このスクリプトをアタッチ
/// 4. Play Mode で実行
/// 
/// テスト内容:
/// - Canvas Scaler が正しく設定されるか
/// - Reference Resolution が CURRENT_RESOLUTION_PRESET と一致するか
/// - Canvas Scale が計算されるか
/// 
/// カスタマイズ:
/// - _prefabPreset でプレファブをプルダウンから選択
/// - _testPrefab でプレファブを直接指定（優先度高）
/// - REMOVE_FUNCTIONAL_SCRIPTS = true/false で機能スクリプト除去を制御
/// </summary>
public class UICanvasManagerTest : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════
    // テスト設定定数
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// プレファブプリセット（プルダウンメニュー）
    /// 
    /// Inspector でプルダウンから選択
    /// 対応するパスが自動的に解決される
    /// </summary>
    [SerializeField]
    private UIPrefabPreset _prefabPreset = UIPrefabPreset.EscMenu;

    /// <summary>
    /// テスト対象プレファブ（Editor から設定可能）
    /// 
    /// Inspector でプレファブを直接ドラッグ&ドロップして選択
    /// 設定されている場合は _prefabPreset より優先される
    /// </summary>
    [SerializeField]
    private GameObject _testPrefab = null;

    /// <summary>
    /// 機能スクリプトを削除するか
    /// 
    /// true = UIEscMenuCtrl などの機能スクリプトを削除
    ///        → UI 描画テストに集中できる（ボタン動作テスト不可）
    /// false = 機能スクリプトを保持
    ///        → UI 描画 + ボタン動作テスト可能
    ///        → コントローラーの初期化エラーはログに出力される
    /// </summary>
    private const bool REMOVE_FUNCTIONAL_SCRIPTS = false;

    /// <summary>
    /// ダミーマネージャーを作成するか
    /// 
    /// true = GameManager などのダミーマネージャーを作成
    ///        → コントローラー初期化エラーを軽減
    ///        → より多くの UI イベントが動作する可能性
    /// false = ダミーマネージャーなし
    ///        → コントローラー初期化時にエラーが出る可能性
    /// </summary>
    private const bool CREATE_DUMMY_MANAGERS = true;

    /// <summary>
    /// テスト結果をログに出力するか
    /// 
    /// false = 重要な情報のみ表示（Canvas階層診断、エラーメッセージのみ）
    /// true = 詳細なステップログをすべて表示
    /// </summary>
    private const bool ENABLE_DETAILED_LOGGING = false;

    /// <summary>
    /// テスト終了後にプレファブを削除するか
    /// 
    /// true = テスト完了後に削除（デフォルト）
    /// false = テスト完了後も表示を保持（UI 確認用）
    /// </summary>
    private const bool DELETE_PREFAB_AFTER_TEST = false;

    // ═══════════════════════════════════════════════════════════
    // テスト用フィールド
    // ═══════════════════════════════════════════════════════════

    private GameObject _testPrefabInstance = null;
    private Canvas _targetCanvas = null;
    private CanvasScaler _targetCanvasScaler = null;

    // ═══════════════════════════════════════════════════════════
    // テスト実行メソッド
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// テスト開始
    /// Play Mode で自動実行
    /// </summary>
    private void Start()
    {
        Debug.Log("[UICanvasManagerTest] ═════════════════════════════════════");
        Debug.Log("[UICanvasManagerTest] UICanvasManager Test Started");
        Debug.Log("[UICanvasManagerTest] ═════════════════════════════════════");

        StartCoroutine(RunTest());
    }

    /// <summary>
    /// テストコルーチン
    /// </summary>
    private IEnumerator RunTest()
    {
        // ステップ0: ダミーマネージャー設定（オプション）
        if (CREATE_DUMMY_MANAGERS)
        {
            yield return SetupDummyDependencies();
        }

        // ステップ1: プレファブ読み込み
        yield return LoadTestPrefab();

        if (_testPrefabInstance == null)
        {
            Debug.LogError("[UICanvasManagerTest] Failed to load prefab");
            yield break;
        }

        // ステップ1.5: Canvas 構造を診断
        LogCanvasHierarchy();

        // ステップ2: UICanvasManager の設定適用
        yield return ApplyCanvasSettings();

        // ステップ2.5: 設定後の Canvas 構造を診断
        Debug.Log("[UICanvasManagerTest] Canvas Hierarchy AFTER ApplyStandardScalerSettings:");
        LogCanvasHierarchy();

        // ステップ3: Canvas Scaler 設定検証
        yield return ValidateCanvasScalerSettings();

        // ステップ4: スケーリング計算検証
        yield return ValidateCanvasScale();

        // テスト完了
        Debug.Log("[UICanvasManagerTest] ═════════════════════════════════════");
        Debug.Log("[UICanvasManagerTest] Test Completed");
        Debug.Log("[UICanvasManagerTest] ═════════════════════════════════════");

        // テスト終了後、プレファブを削除するか制御
        if (DELETE_PREFAB_AFTER_TEST)
        {
            if (_testPrefabInstance != null)
            {
                Transform parent = _testPrefabInstance.transform.parent;
                if (parent != null)
                {
                    Destroy(parent.gameObject);
                }
                else
                {
                    Destroy(_testPrefabInstance);
                }
            }
        }
        else
        {
            LogTest("  DELETE_PREFAB_AFTER_TEST = false: UI remains visible for inspection");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 各テストステップ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// プリセット enum から Resources パスを取得
    /// </summary>
    private string GetPrefabPathFromPreset(UIPrefabPreset preset)
    {
        return preset switch
        {
            UIPrefabPreset.EscMenu => "Prefabs/UI/UIEscMenu",
            UIPrefabPreset.TabMenu => "Prefabs/UI/UITabMenu",
            UIPrefabPreset.DebugInfo => "Prefabs/UI/UIDebugInfo",
            UIPrefabPreset.Notice => "Prefabs/UI/UINotice",
            UIPrefabPreset.UIItemCreate => "Prefabs/UI/UIItemCreate",
            _ => "Prefabs/UI/UIEscMenu"
        };
    }
    /// 
    /// コントローラーの初期化に必要な依存オブジェクト（GameManager など）を
    /// 事前に作成しておくことで、コントローラー初期化エラーを軽減する
    /// </summary>
    private IEnumerator SetupDummyDependencies()
    {
        LogTest("Step 0: Setting up Dummy Dependencies");

        try
        {
            // ダミー GameManager を作成（Singleton パターン）
            // GameManager が存在しない場合にのみ作成
            if (GameObject.Find("_DummyGameManager") == null)
            {
                GameObject dummyGameManagerObj = new GameObject("_DummyGameManager");
                // ここで必要なマネージャーコンポーネントを追加することも可能
                LogTest("  Created dummy GameManager");
            }

            LogTest("  Dummy dependencies ready");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[UICanvasManagerTest] Error setting up dummy dependencies: {ex.Message}");
        }

        yield return null;
    }
    /// 
    /// 注意: REMOVE_FUNCTIONAL_SCRIPTS = true の場合：
    ///   機能スクリプトは Instantiate 直後に削除される（Awake() 実行前）
    /// 注意: REMOVE_FUNCTIONAL_SCRIPTS = false の場合：
    ///   コントローラーが Awake() で初期化エラーを起こす可能性がある
    ///   CREATE_DUMMY_MANAGERS = true で依存オブジェクトを作成してエラー軽減
    /// </summary>
    private IEnumerator LoadTestPrefab()
    {
        LogTest("Step 1: Loading Prefab");

        try
        {
            GameObject prefab = null;

            // 方法1: シリアライズフィールドから直接参照（Editor で設定）
            if (_testPrefab != null)
            {
                prefab = _testPrefab;
                LogTest($"  Using prefab from Inspector: {prefab.name}");
            }
            // 方法2: プリセット enum から Resources.Load
            else
            {
                string prefabPath = GetPrefabPathFromPreset(_prefabPreset);
                prefab = Resources.Load<GameObject>(prefabPath);
                LogTest($"  Using prefab from preset ({_prefabPreset}): {prefabPath}");
            }

            if (prefab == null)
            {
                Debug.LogError($"[UICanvasManagerTest] Prefab not found. Set _testPrefab in Inspector or check _prefabPreset");
                yield break;
            }

            // 非アクティブな親を作成（Awake() を遅延させるため）
            GameObject inactiveParent = new GameObject("_UITest_InactiveParent");
            inactiveParent.SetActive(false);

            // 非アクティブな親の下で Instantiate（Awake() は呼ばれない）
            _testPrefabInstance = Instantiate(prefab, inactiveParent.transform);

            // 機能スクリプト除去（Awake() 実行前、DestroyImmediate で即座削除）
            if (REMOVE_FUNCTIONAL_SCRIPTS)
            {
                RemoveFunctionalScripts(_testPrefabInstance);
            }

            // 親をアクティブ化（この時点で Awake() が呼ばれる）
            inactiveParent.SetActive(true);
            
            // UIItemCreate(Clone) も明示的にアクティブ化
            _testPrefabInstance.SetActive(true);
            
            // テスト用に親をスクリーン中央に配置（目視確認用）
            inactiveParent.transform.position = Vector3.zero;

            _targetCanvas = _testPrefabInstance.GetComponent<Canvas>();

            if (_targetCanvas == null)
            {
                Debug.LogError("[UICanvasManagerTest] Canvas component not found in prefab");
                yield break;
            }

            LogTest($"  Prefab loaded: {prefab.name}");
            LogTest($"  Canvas name: {_targetCanvas.name}");
            LogTest($"  Canvas active: {_targetCanvas.gameObject.activeInHierarchy}");

            // UI パネルが非表示の場合は表示する（EscMenuCtrl など）
            if (!REMOVE_FUNCTIONAL_SCRIPTS)
            {
                TryShowUIPanel();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[UICanvasManagerTest] Error loading prefab: {ex.Message}");
        }

        yield return null;
    }

    /// <summary>
    /// UI パネルを表示（コントローラー保持時のみ）
    /// 
    /// EscMenuCtrl などのコントローラーが Awake で パネルを非表示にしている場合、
    /// テスト用に明示的に表示する
    /// </summary>
    private void TryShowUIPanel()
    {
        if (_testPrefabInstance == null)
        {
            return;
        }

        // EscMenuCtrl の場合
        EscMenuCtrl escMenuCtrl = _testPrefabInstance.GetComponent<EscMenuCtrl>();
        if (escMenuCtrl != null)
        {
            try
            {
                // ToggleEscMenuWindow(true) で表示
                escMenuCtrl.GetType()
                    .GetMethod("ToggleEscMenuWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Invoke(escMenuCtrl, new object[] { true });
                LogTest("  EscMenuCtrl panel shown");
            }
            catch (System.Exception ex)
            {
                LogTest($"  Warning: Failed to show EscMenuCtrl panel: {ex.Message}");
            }
        }

        // TabMenuCtrl の場合（必要に応じて追加）
        // ...
    }

    /// <summary>
    /// ステップ2: 機能スクリプト除去
    /// 
    /// EscMenuCtrl, TabMenuCtrl などの機能スクリプトを削除
    /// DestroyImmediate を使用して即座削除（Awake() 実行前）
    /// → UI レイアウト・描画テストのみに集中
    /// </summary>
    private void RemoveFunctionalScripts(GameObject prefabInstance)
    {
        LogTest("Step 2: Removing Functional Scripts");

        // 除去対象のコンポーネント名リスト
        string[] functionalScriptNames = new string[]
        {
            "EscMenuCtrl",
            "TabMenuCtrl",
            "DebugInfoCtrl",
            "GameTimerCtrl",
            "NoticeCtrl",
            "SpawnMarkerPointerCtrl",
            "EventLogCtrl",
            "InfoWindowCtrl",
            "MessageBoxCtrl",
        };

        int removedCount = 0;

        foreach (string scriptName in functionalScriptNames)
        {
            // prefab 直下と子要素に対してコンポーネントを検索
            Component[] allComponents = prefabInstance.GetComponentsInChildren<Component>();

            foreach (Component component in allComponents)
            {
                // コンポーネントが指定された名前のスクリプトの場合
                if (component.GetType().Name == scriptName)
                {
                    LogTest($"  Removing: {scriptName}");
                    // DestroyImmediate で即座削除（Awake() 実行前）
                    DestroyImmediate(component);
                    removedCount++;
                }
            }
        }

        if (removedCount > 0)
        {
            LogTest($"  Total removed: {removedCount} script(s)");
        }
        else
        {
            LogTest("  No functional scripts found");
        }
    }

    /// <summary>
    /// ステップ3: UICanvasManager の設定適用
    /// </summary>
    private IEnumerator ApplyCanvasSettings()
    {
        LogTest("Step 3: Applying UICanvasManager Settings");

        try
        {
            UICanvasManager.ApplyStandardScalerSettings(_targetCanvas);
            _targetCanvasScaler = _targetCanvas.GetComponent<CanvasScaler>();

            LogTest("  Settings applied successfully");
            LogTest($"  Render Mode: {_targetCanvas.renderMode}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[UICanvasManagerTest] Error applying settings: {ex.Message}");
        }

        yield return null;
    }

    /// <summary>
    /// ステップ4: Canvas Scaler 設定検証
    /// </summary>
    private IEnumerator ValidateCanvasScalerSettings()
    {
        LogTest("Step 4: Validating Canvas Scaler Settings");

        if (_targetCanvasScaler == null)
        {
            Debug.LogError("[UICanvasManagerTest] CanvasScaler not found");
            yield break;
        }

        Vector2 expectedResolution = UICanvasManager.REFERENCE_RESOLUTION;
        Vector2 actualResolution = _targetCanvasScaler.referenceResolution;

        // 検証項目
        bool isScaleModeCorrect = _targetCanvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize;
        bool isResolutionCorrect = actualResolution == expectedResolution;
        bool isMatchCorrect = Mathf.Approximately(_targetCanvasScaler.matchWidthOrHeight, 0.5f);
        bool isScreenMatchCorrect = _targetCanvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        LogTest($"  UI Scale Mode: {_targetCanvasScaler.uiScaleMode} {(isScaleModeCorrect ? "[OK]" : "[FAIL]")}");
        LogTest($"  Reference Resolution: {actualResolution} (Expected: {expectedResolution}) {(isResolutionCorrect ? "[OK]" : "[FAIL]")}");
        LogTest($"  Match Width Or Height: {_targetCanvasScaler.matchWidthOrHeight:F2} {(isMatchCorrect ? "[OK]" : "[FAIL]")}");
        LogTest($"  Screen Match Mode: {_targetCanvasScaler.screenMatchMode} {(isScreenMatchCorrect ? "[OK]" : "[FAIL]")}");

        // 総合判定
        bool allSettingsCorrect = isScaleModeCorrect && isResolutionCorrect && isMatchCorrect && isScreenMatchCorrect;
        if (allSettingsCorrect)
        {
            LogTest("  ✓ All Canvas Scaler settings are correct");
        }
        else
        {
            LogTest("  ✗ Some settings are incorrect");
        }

        yield return null;
    }

    /// <summary>
    /// ステップ5: スケーリング計算検証
    /// </summary>
    private IEnumerator ValidateCanvasScale()
    {
        LogTest("Step 5: Validating Canvas Scale Calculation");

        float canvasScale = UICanvasManager.GetCurrentCanvasScale();
        Vector2 referenceResolution = UICanvasManager.REFERENCE_RESOLUTION;

        LogTest($"  Current Screen Resolution: {Screen.width}×{Screen.height}");
        LogTest($"  Reference Resolution: {referenceResolution}");
        LogTest($"  Calculated Canvas Scale: {canvasScale:F2}x");
        LogTest($"  Current Resolution Preset: {UICanvasManager.CURRENT_RESOLUTION_PRESET}");

        yield return null;
    }

    /// <summary>
    /// Canvas 階層構造を簡潔にログ出力
    /// UIItemCreate などの複雑な構造を診断するため（重要情報のみ）
    /// </summary>
    private void LogCanvasHierarchy()
    {
        if (_testPrefabInstance == null)
        {
            return;
        }

        Debug.Log("[UICanvasManagerTest] ═══════════════════════════════════");
        Debug.Log("[UICanvasManagerTest] Canvas Hierarchy Diagnostic");
        Debug.Log("[UICanvasManagerTest] ═══════════════════════════════════");

        // Canvas をすべて検索
        Canvas[] allCanvases = _testPrefabInstance.GetComponentsInChildren<Canvas>(includeInactive: true);
        Debug.Log($"[UICanvasManagerTest] Total Canvas count: {allCanvases.Length}");

        foreach (Canvas canvas in allCanvases)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            string scalerInfo = scaler != null ? $" (ScaleMode: {scaler.uiScaleMode})" : "";
            Debug.Log($"[UICanvasManagerTest] - Canvas: {canvas.gameObject.name}{scalerInfo}");
        }

        // TMPコンポーネント を全検索して、各々の親 Canvas を確認
        TextMeshProUGUI[] allTMPs = _testPrefabInstance.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
        Debug.Log($"[UICanvasManagerTest] Total TextMeshProUGUI count: {allTMPs.Length}");

        foreach (TextMeshProUGUI tmp in allTMPs)
        {
            Canvas parentCanvas = tmp.GetComponentInParent<Canvas>();
            string parentInfo = parentCanvas != null ? parentCanvas.gameObject.name : "NOT FOUND";
            Debug.Log($"[UICanvasManagerTest] - TMP: {tmp.gameObject.name} → Parent Canvas: {parentInfo}");
        }

        Debug.Log("[UICanvasManagerTest] ═══════════════════════════════════");
    }

    // ═══════════════════════════════════════════════════════════
    // ユーティリティメソッド
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// テストログを出力（ENABLE_DETAILED_LOGGING が true の場合のみ）
    /// </summary>
    private void LogTest(string message)
    {
        if (ENABLE_DETAILED_LOGGING)
        {
            Debug.Log($"[UICanvasManagerTest] {message}");
        }
    }
}
