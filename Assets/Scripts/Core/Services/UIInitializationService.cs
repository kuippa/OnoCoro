using System.Collections;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// UI 初期化サービス（シーン横断的）
/// 
/// 責務:
/// - Canvas Scaler の統一初期化
/// - TextMeshPro フォント自動スケーリング
/// - すべての UI シーンで共通の初期化処理を提供
/// 
/// 使用方法:
/// // ゲームシーン（InitializationManager）
/// yield return UIInitializationService.InitializeUIForScene();
/// 
/// // タイトル画面（TitleStartController）
/// yield return UIInitializationService.InitializeUIForScene();
/// 
/// // 設定画面など、新規シーンでも共通利用可能
/// </summary>
internal static class UIInitializationService
{
    /// <summary>
    /// シーンの UI を初期化
    /// 
    /// 処理フロー:
    /// 1. Canvas Scaler の統一設定（UICanvasManager）
    /// 2. フォントのスケーリング適用（UIFontManager）
    /// 
    /// 実行順序:
    /// - Canvas Scaler 設定が先（フォント計算に必要）
    /// - フォント初期化が後（スケール係数が確定している）
    /// 
    /// 使用例:
    /// private IEnumerator Initialize()
    /// {
    ///     yield return UIInitializationService.InitializeUIForScene();
    ///     // シーン固有の処理...
    /// }
    /// </summary>
    internal static IEnumerator InitializeUIForScene()
    {
        // Debug.Log("[UIInitializationService] UI シーン初期化開始"); // [PROD] ログ抑制
        
        // Canvas Scaler 設定
        InitializeCanvasScalersInternal();
        yield return null;
        
        // フォント初期化
        yield return UIFontManager.InitializeFontSettings();
        
        // Debug.Log("[UIInitializationService] UI シーン初期化完了"); // [PROD] ログ抑制
    }

    /// <summary>
    /// Canvas Scaler 初期化の内部処理（エラーハンドリング付き）
    /// </summary>
    private static void InitializeCanvasScalersInternal()
    {
        try
        {
            // Debug.Log("[UIInitializationService] ステップ 1/2: Canvas Scaler 設定開始"); // [PROD] ログ抑制
            UICanvasManager.InitializeCanvasSettings();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[UIInitializationService] Canvas Scaler 設定エラー: {ex.Message}");
        }
    }
}
