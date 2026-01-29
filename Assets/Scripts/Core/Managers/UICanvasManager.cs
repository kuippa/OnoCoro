using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = CommonsUtility.Debug;

/// <summary>
/// UI Canvas 一元管理（Singleton）
/// 全 Canvas の Scaler 設定と解像度対応を統一
/// 
/// 責務:
/// - Canvas Scaler の統一設定
/// - 複数解像度プリセットの管理（FullHD, HD, 2K など）
/// - 現在使用する解像度の切り替え
/// - 複数解像度対応時の中枢管理
/// 
/// 設計原則:
/// - CURRENT_RESOLUTION_PRESET で解像度を選択
/// - 変更するだけで全 Canvas に自動反映
/// - Canvas Scaler 設定の分散を防止
/// - 将来の解像度追加に対応可能
/// 
/// 参照: docs/ui-improvement-phase-1-4.md (Phase 1.4.4)
/// </summary>
internal class UICanvasManager : MonoBehaviour
{
    private static UICanvasManager _instance;

    // ═══════════════════════════════════════════════════════════
    // 解像度プリセット定義
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// UI 設計用の解像度プリセット
    /// </summary>
    public enum ResolutionPreset
    {
        /// <summary>HD (1280×720) - モバイル・小型画面対応</summary>
        HD = 0,

        /// <summary>Full HD (1920×1080) - スタンダード解像度（現在使用中）</summary>
        FullHD = 1,

        /// <summary>2K (2560×1440) - 高解像度画面対応</summary>
        TwoK = 2,

        /// <summary>iPad 4:3 (1024×768) - タブレット対応</summary>
        iPad = 3,
    }

    /// <summary>
    /// 解像度プリセットの解像度マッピング
    /// </summary>
    private static readonly Dictionary<ResolutionPreset, Vector2> RESOLUTION_MAP = 
        new Dictionary<ResolutionPreset, Vector2>()
        {
            { ResolutionPreset.HD, new Vector2(1280, 720) },
            { ResolutionPreset.FullHD, new Vector2(1920, 1080) },
            { ResolutionPreset.TwoK, new Vector2(2560, 1440) },
            { ResolutionPreset.iPad, new Vector2(1024, 768) },
        };

    // ═══════════════════════════════════════════════════════════
    // [重要] Canvas 設定定数
    // これらの値を変更するだけで、全 Canvas に自動反映される
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 現在使用する解像度プリセット
    /// 
    /// 複数解像度対応時は、ここの値を変更するだけで
    /// 全 Canvas に自動反映される
    /// 
    /// 例:
    /// - 現在: FullHD (1920×1080)
    /// - 将来: TwoK (2560×1440) に変更可能
    /// </summary>
    internal static readonly ResolutionPreset CURRENT_RESOLUTION_PRESET = ResolutionPreset.FullHD;

    /// <summary>
    /// 現在の基準解像度（CURRENT_RESOLUTION_PRESET から算出）
    /// </summary>
    internal static Vector2 REFERENCE_RESOLUTION => RESOLUTION_MAP[CURRENT_RESOLUTION_PRESET];

    /// <summary>
    /// Canvas Scaler の幅・高さマッチ設定
    /// 
    /// 0.0 = 幅優先（横長画面対応）
    /// 0.5 = 等比（幅・高さを等しく重視）← 推奨
    /// 1.0 = 高さ優先（縦長画面対応）
    /// </summary>
    internal static readonly float MATCH_WIDTH_OR_HEIGHT = 0.5f;

    /// <summary>
    /// デフォルト Render Mode
    /// </summary>
    internal static readonly RenderMode DEFAULT_RENDER_MODE = RenderMode.ScreenSpaceOverlay;

    /// <summary>
    /// シングルトンインスタンス取得
    /// </summary>
    internal static UICanvasManager Instance => _instance;

    // ═══════════════════════════════════════════════════════════
    // Canvas Scaler 管理メソッド
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Canvas に標準 Scaler 設定を自動適用
    /// 
    /// 全ての UI Canvas でこのメソッドを呼び出すことで、
    /// Canvas Scaler 設定が統一される
    /// 
    /// 使用例:
    /// protected override IEnumerator Initialize()
    /// {
    ///     Canvas canvas = GetComponent<Canvas>();
    ///     UICanvasManager.ApplyStandardScalerSettings(canvas);
    ///     yield return null;
    /// }
    /// </summary>
    internal static void ApplyStandardScalerSettings(Canvas canvas)
    {
        if (canvas == null)
        {
            Debug.LogWarning("[UICanvasManager] Canvas is null");
            return;
        }

        // Render Mode を統一
        canvas.renderMode = DEFAULT_RENDER_MODE;

        // Canvas Scaler を取得または作成
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        // [重要] 以下の設定は REFERENCE_RESOLUTION を変更するだけで全 Canvas に反映される
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = MATCH_WIDTH_OR_HEIGHT;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        Debug.Log($"[UICanvasManager] Canvas '{canvas.name}' - Standard Scaler Settings Applied");
        Debug.Log($"[UICanvasManager]   Reference Resolution: {REFERENCE_RESOLUTION}");
        Debug.Log($"[UICanvasManager]   Match Width Or Height: {MATCH_WIDTH_OR_HEIGHT}");
    }

    /// <summary>
    /// 現在の Canvas スケール係数を取得（複数解像度対応）
    /// 
    /// 使用例:
    /// float canvasScale = UICanvasManager.GetCurrentCanvasScale();
    /// Debug.Log($"Current Canvas Scale: {canvasScale}x");
    /// </summary>
    internal static float GetCurrentCanvasScale()
    {
        float scaleY = Screen.height / REFERENCE_RESOLUTION.y;
        float scaleX = Screen.width / REFERENCE_RESOLUTION.x;

        // 等比スケーリング（MATCH_WIDTH_OR_HEIGHT = 0.5f に対応）
        return Mathf.Lerp(scaleX, scaleY, MATCH_WIDTH_OR_HEIGHT);
    }

    /// <summary>
    /// 複数解像度対応時に全 Canvas を更新
    /// 
    /// 注意: REFERENCE_RESOLUTION を変更した後、
    /// このメソッドを呼び出して全 Canvas を更新する
    /// 
    /// 使用例（将来の 2K 対応時）:
    /// // REFERENCE_RESOLUTION を 2560×1440 に変更した後
    /// UICanvasManager.UpdateAllCanvasesForResolution(new Vector2(2560, 1440));
    /// 
    /// ただし、実装上は REFERENCE_RESOLUTION 定数を変更するだけで
    /// 新規 Initialize() 時には自動反映されるため、
    /// 既存 Canvas の更新時のみ使用
    /// </summary>
    internal static void UpdateAllCanvasesForResolution(Vector2 newResolution)
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();

        int updatedCount = 0;
        foreach (Canvas canvas in allCanvases)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.referenceResolution = newResolution;
                updatedCount++;
            }
        }

        Debug.Log($"[UICanvasManager] Updated {updatedCount} Canvas(es) to resolution: {newResolution}");
    }

    /// <summary>
    /// 現在のスクリーン解像度情報をログ出力（デバッグ用）
    /// </summary>
    internal static void LogCurrentResolutionInfo()
    {
        float canvasScale = GetCurrentCanvasScale();
        Vector2 refResolution = REFERENCE_RESOLUTION;
        
        Debug.Log("[UICanvasManager] ─────────────────────────────────");
        Debug.Log($"[UICanvasManager] Current Preset: {CURRENT_RESOLUTION_PRESET}");
        Debug.Log($"[UICanvasManager] Reference Resolution: {refResolution}");
        Debug.Log($"[UICanvasManager] Current Screen Resolution: {Screen.width}×{Screen.height}");
        Debug.Log($"[UICanvasManager] Current Canvas Scale: {canvasScale:F2}x");
        Debug.Log("[UICanvasManager] ─────────────────────────────────");
    }

    // ═══════════════════════════════════════════════════════════
    // Singleton 管理
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Singleton 初期化
    /// </summary>
    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("[UICanvasManager] Singleton instance already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[UICanvasManager] Initialized (Singleton)");
        LogCurrentResolutionInfo();
    }

    /// <summary>
    /// シーン遷移時の参照クリア（オプション）
    /// </summary>
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
