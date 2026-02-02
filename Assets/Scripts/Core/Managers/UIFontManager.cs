using System.Collections;
using CommonsUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = CommonsUtility.Debug;

/// <summary>
/// フォント初期化を管理するマネージャー
/// 
/// シーン内のすべての TextMeshProUGUI コンポーネントを検出して、
/// Canvas Scaler の設定に応じて自動フォントスケーリングを適用する
/// 
/// 責務:
/// - TextMeshProUGUI の自動検出（アクティブ/非アクティブ問わず）
/// - Canvas Scaler の ScaleWithScreenSize モード確認
/// - Canvas スケール係数計算と UIFont 自動検出
/// - フォントサイズの自動スケーリング適用
/// 
/// 使用方法:
/// InitializationManager から StartCoroutine(UIFontManager.InitializeFontSettings()) で呼び出される
/// </summary>
internal static class UIFontManager
{
    /// <summary>
    /// フォント初期化を実行
    /// 
    /// 処理フロー:
    /// 1. シーン内のすべての TextMeshProUGUI を検出（非アクティブも含む）
    /// 2. 各テキストの親 Canvas の ScaleMode を確認
    /// 3. ScaleWithScreenSize のみにスケーリングを適用
    /// 4. ApplyScaledFontSize() で自動スケーリング
    /// </summary>
    internal static IEnumerator InitializeFontSettings()
    {
        // Debug.Log("[UIFontManager] フォント初期化開始");
        
        try
        {
            // シーン内のすべての TextMeshProUGUI を検出（非アクティブなオブジェクトも含む）
            // UIEscMenu など、Hierarchy で非表示のメニュー UI も対象に含める
            TextMeshProUGUI[] allTextMeshPros = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>(includeInactive: true);
            
            if (allTextMeshPros.Length == 0)
            {
                // Debug.LogWarning("[UIFontManager] TextMeshProUGUI コンポーネントがシーン内に見つかりません");
                yield break;
            }
            
            int processedCount = 0;
            
            // 各 TextMeshProUGUI に対して、現在のフォントサイズから最も近い UIFont を見つけてスケーリング適用
            foreach (TextMeshProUGUI textMesh in allTextMeshPros)
            {
                if (textMesh == null)
                {
                    continue;
                }
                
                // 親の Canvas を探す（非アクティブな親も含める）
                // GetComponentInParent は非アクティブな親を検索できないため、手動でトレース
                Canvas parentCanvas = FindParentCanvasIncludeInactive(textMesh.transform);
                
                // Debug.Log($"[UIFontManager] TMP: {textMesh.gameObject.name}, Found Parent Canvas: {(parentCanvas != null ? parentCanvas.gameObject.name : "NULL")}");
                // Debug.Log($"[UIFontManager] TMP: {textMesh.gameObject.name}, Found Parent Canvas: {(parentCanvas != null ? parentCanvas.gameObject.name : "NULL")}");
                // Debug.Log($"[UIFontManager] TMP: {textMesh.gameObject.name}, Found Parent Canvas: {(parentCanvas != null ? parentCanvas.gameObject.name : "NULL")}"); // [PROD] ログ抑制
                
                // Canvas Scaler が ScaleWithScreenSize の場合のみスケーリングを適用
                // （UICanvasManager の設定を尊重する防御層）
                // parentCanvas が null の場合もスキップ（Canvas がない = 独立したキャンバスシステム）
                if (parentCanvas == null)
                {
                    // Debug.Log($"[UIFontManager] フォント初期化スキップ: {textMesh.gameObject.name} (親 Canvas が見つかりません)");
                    continue;
                }
                
                CanvasScaler canvasScaler = parentCanvas.GetComponent<CanvasScaler>();
                
                // ScaleMode が ScaleWithScreenSize でない場合、またはCanvasScalerがない場合はスキップ
                if (canvasScaler == null || canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    // Debug.Log($"[UIFontManager] フォント初期化スキップ: {textMesh.gameObject.name} (Canvas '{parentCanvas.name}' の ScaleMode は {(canvasScaler != null ? canvasScaler.uiScaleMode.ToString() : "未設定")})");
                    continue;
                }
                
                // 現在のフォントサイズから最も近い UIFont を見つけてスケーリング
                ApplyScaledFontSize(textMesh);
                processedCount++;
            }
            
            // Debug.Log($"[UIFontManager] フォント初期化完了: {processedCount}/{allTextMeshPros.Length} コンポーネント処理");
        }
        catch (System.Exception ex)
        {
            // Debug.LogError($"[UIFontManager] フォント初期化エラー: {ex.Message}");
        }
        
        yield return null;
    }

    /// <summary>
    /// Canvas スケールに対応したフォントサイズを自動適用
    /// 現在のフォントサイズから最も近い UIFont を見つけてスケーリング
    /// （初期化時に既存フォントサイズを保持しつつスケーリングする際に使用）
    /// </summary>
    private static void ApplyScaledFontSize(TextMeshProUGUI textMeshPro)
    {
        if (textMeshPro == null)
        {
            // Debug.LogWarning("[UIFontManager] TextMeshProUGUI is null");
            return;
        }
        
        float originalSize = textMeshPro.fontSize;
        
        // 現在のフォントサイズから最も近い UIFont を見つける
        UIFont closestFont = FindClosestUIFont(textMeshPro.fontSize);
        
        float scaleFactor = GetCanvasScaleFactor();
        int newFontSize = Mathf.RoundToInt((int)closestFont * scaleFactor);
        textMeshPro.fontSize = newFontSize;
        
        // Debug.Log($"[UIFontManager] ApplyScaledFontSize - Object: {textMeshPro.gameObject.name}, Original: {originalSize}px, Closest UIFont: {closestFont} ({(int)closestFont}px), Scale: {scaleFactor:F2}x, Final: {newFontSize}px");
    }

    /// <summary>
    /// 現在の Canvas スケール係数を取得（UI デザイン基準 → Canvas Scaler Reference Resolution への比率）
    /// UI デザイン時の 800×600 → 現在の Canvas Reference Resolution へのスケーリング比率
    /// </summary>
    private static float GetCanvasScaleFactor()
    {
        // UI デザイン基準解像度 (800×600)
        Vector2 baseResolution = UICanvasManager.UI_DESIGN_BASE_RESOLUTION;
        
        // Canvas Scaler の現在の Reference Resolution
        Vector2 referenceResolution = UICanvasManager.REFERENCE_RESOLUTION;
        
        // Canvas Scaler がどのようにスケーリングしているかの比率を計算
        float scaleX = referenceResolution.x / baseResolution.x;
        float scaleY = referenceResolution.y / baseResolution.y;
        
        // 等比スケーリング
        float scale = Mathf.Lerp(scaleX, scaleY, UICanvasManager.MATCH_WIDTH_OR_HEIGHT);
        
        // Debug.Log($"[UIFontManager] Canvas Scale Factor: {scale:F2}x (Base: {baseResolution}, Ref: {referenceResolution})");
        
        return scale;
    }

    /// <summary>
    /// 指定されたフォントサイズに最も近い UIFont 値を見つける
    /// </summary>
    private static UIFont FindClosestUIFont(float fontSize)
    {
        UIFont[] allFonts = System.Enum.GetValues(typeof(UIFont)) as UIFont[];
        
        UIFont closestFont = UIFont.H4;
        float minDifference = float.MaxValue;
        
        foreach (UIFont font in allFonts)
        {
            float fontValue = (float)font;
            float difference = Mathf.Abs(fontSize - fontValue);
            
            if (difference < minDifference)
            {
                minDifference = difference;
                closestFont = font;
            }
        }
        
        // Debug.Log($"[UIFontManager] FindClosestUIFont - Input: {fontSize}px, Closest: {closestFont} ({(int)closestFont}px), Difference: {minDifference}px");
        
        return closestFont;
    }

    /// <summary>
    /// 非アクティブな親オブジェクトも含めて、親 Canvas を検索
    /// 
    /// Unity の GetComponentInParent は非アクティブな親を検索できないため、手動実装
    /// UIItemCreate などのように全階層が非アクティブなケースに対応
    /// </summary>
    private static Canvas FindParentCanvasIncludeInactive(Transform transform)
    {
        if (transform == null)
        {
            return null;
        }
        
        // 自分自身のキャンバスをチェック
        Canvas canvas = transform.GetComponent<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }
        
        // 親をたどって Canvas を探す
        Transform current = transform.parent;
        while (current != null)
        {
            canvas = current.GetComponent<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }
            current = current.parent;
        }
        
        return null;
    }
}
