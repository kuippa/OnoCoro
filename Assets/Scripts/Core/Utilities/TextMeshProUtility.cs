using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// TextMeshPro 拡張メソッド（UIFont enum 対応）
    /// 
    /// TextMeshPro のフォントサイズと表示スタイルを簡潔に設定するための
    /// ユーティリティクラスです。UIFont enum（H1～H7）を使用することで、
    /// ハードコーディングなしにフォントサイズを一元管理できます。
    /// 
    /// 責務:
    /// - UIFont enum（H1～H7）を使用したフォントサイズ設定
    /// - テキストスタイル（サイズ + 配置）の一括適用
    /// - TextMeshPro の操作を簡潔に
    /// 
    /// 使用例:
    /// // 単純にサイズだけ設定
    /// myText.SetFontSize(UIFont.H3);
    /// 
    /// // スタイル一括設定（サイズ + 配置）
    /// myText.ApplyUIStyle(UIFont.H1, TextAlignmentOptions.Center);
    /// 
    /// // スケーリング対応（Canvas Scaler Reference Resolution に自動対応）
    /// myText.SetScaledFontSize(UIFont.H2);
    /// </summary>
    internal static class TextMeshProUtility
    {
        /// <summary>
        /// TextMeshPro のフォントサイズを UIFont enum（H1～H7）で設定
        /// 
        /// ハードコーディングなしにフォントサイズを統一管理します。
        /// 
        /// 例:
        /// textMeshPro.SetFontSize(UIFont.H3);  // 36px に設定
        /// 
        /// パラメータ:
        /// textMeshPro: 対象となる TextMeshProUGUI コンポーネント
        /// fontLevel: 設定するフォントサイズレベル（UIFont enum: H1～H7）
        /// </summary>
        internal static void SetFontSize(this TextMeshProUGUI textMeshPro, UIFont fontLevel)
        {
            if (textMeshPro == null)
            {
                Debug.LogWarning("[TextMeshProUtility] TextMeshProUGUI is null");
                return;
            }

            textMeshPro.fontSize = (int)fontLevel;
        }

        /// <summary>
        /// TextMeshPro のスタイル（フォントサイズ + 配置）を一括設定
        /// 
        /// ハードコーディングなしにテキストスタイルを統一管理します。
        /// 
        /// 例:
        /// textMeshPro.ApplyUIStyle(UIFont.H1, TextAlignmentOptions.Center);
        /// 
        /// パラメータ:
        /// textMeshPro: 対象となる TextMeshProUGUI コンポーネント
        /// fontLevel: 設定するフォントサイズレベル（UIFont enum）
        /// alignment: テキスト配置（デフォルト: Center）
        /// </summary>
        internal static void ApplyUIStyle(
            this TextMeshProUGUI textMeshPro,
            UIFont fontLevel,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            if (textMeshPro == null)
            {
                Debug.LogWarning("[TextMeshProUtility] TextMeshProUGUI is null");
                return;
            }

            textMeshPro.fontSize = (int)fontLevel;
            textMeshPro.alignment = alignment;
        }

        /// <summary>
        /// Canvas スケールに対応したフォントサイズを適用
        /// （800×600 基準値から自動スケーリング）
        /// 
        /// UIFontManager.InitializeFontSettings() で自動適用されるため、
        /// 通常は使用不要です。
        /// 手動で後からフォント設定する場合に使用してください。
        /// </summary>
        internal static void SetScaledFontSize(this TextMeshProUGUI textMeshPro, UIFont fontLevel)
        {
            if (textMeshPro == null)
            {
                Debug.LogWarning("[TextMeshProUtility] TextMeshProUGUI is null");
                return;
            }
            
            float scaleFactor = GetCanvasScaleFactor();
            textMeshPro.fontSize = Mathf.RoundToInt((int)fontLevel * scaleFactor);
        }

        /// <summary>
        /// Canvas スケールに対応したスタイルを一括適用
        /// </summary>
        internal static void ApplyScaledUIStyle(
            this TextMeshProUGUI textMeshPro,
            UIFont fontLevel,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            if (textMeshPro == null)
            {
                Debug.LogWarning("[TextMeshProUtility] TextMeshProUGUI is null");
                return;
            }
            
            float scaleFactor = GetCanvasScaleFactor();
            textMeshPro.fontSize = Mathf.RoundToInt((int)fontLevel * scaleFactor);
            textMeshPro.alignment = alignment;
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
            
            return scale;
        }
    }
}
