using UnityEngine;

namespace CommonsUtility
{
    /// <summary>
    /// TextAsset リソース読み込みのユーティリティクラス
    /// TextAsset（CSV、XML、JSON、テキストファイルなど）の読み込みを一元化し、
    /// null チェックとデバッグログを提供します
    /// </summary>
    internal static class TextAssetLoader
    {
        private const string _LOG_PREFIX = "[TextAssetLoader]";

        /// <summary>
        /// パスから TextAsset をロードする
        /// </summary>
        /// <param name="resourcePath">Resources フォルダ相対パス（拡張子なし）</param>
        /// <returns>読み込まれた TextAsset、失敗時は null</returns>
        public static TextAsset LoadTextAsset(string resourcePath)
        {
            if (!ValidateResourcePath(resourcePath))
            {
                return null;
            }

            try
            {
                TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
                if (textAsset != null)
                {
                    return textAsset;
                }

                Debug.LogWarning($"{_LOG_PREFIX} Failed to load TextAsset from path: {resourcePath}");
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{_LOG_PREFIX} Exception while loading TextAsset '{resourcePath}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// パスから TextAsset をロードしてテキストを取得
        /// </summary>
        /// <param name="resourcePath">Resources フォルダ相対パス（拡張子なし）</param>
        /// <returns>読み込まれたテキスト、失敗時は空文字列</returns>
        public static string LoadTextAssetText(string resourcePath)
        {
            TextAsset textAsset = LoadTextAsset(resourcePath);
            if (textAsset == null)
            {
                return string.Empty;
            }

            return textAsset.text;
        }

        /// <summary>
        /// リソースパスの検証
        /// </summary>
        private static bool ValidateResourcePath(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                Debug.LogError($"{_LOG_PREFIX} Resource path is null, empty, or whitespace");
                return false;
            }

            // パスに不正な文字が含まれていないか確認
            if (resourcePath.Contains("\\"))
            {
                Debug.LogError($"{_LOG_PREFIX} Resource path contains backslash. Use forward slash only: {resourcePath}");
                return false;
            }

            if (resourcePath.StartsWith("/"))
            {
                Debug.LogWarning($"{_LOG_PREFIX} Resource path starts with slash. This may cause load failures: {resourcePath}");
            }

            return true;
        }

        /// <summary>
        /// デバッグ用: ロード結果を検証
        /// </summary>
        public static bool VerifyTextAsset(string resourcePath)
        {
            TextAsset textAsset = LoadTextAsset(resourcePath);
            bool isValid = textAsset != null;

            if (!isValid)
            {
                Debug.LogWarning($"{_LOG_PREFIX} Verify result for '{resourcePath}': FAILED");
            }

            return isValid;
        }

        /// <summary>
        /// デバッグ用: 実際にロード可能なパスを検索
        /// </summary>
        public static void DebugFindAvailableTextAssetPaths(string baseResourcePath)
        {
            if (!ValidateResourcePath(baseResourcePath))
            {
                return;
            }

            Debug.Log($"{_LOG_PREFIX} Debug Searching available TextAsset paths for: {baseResourcePath}");

            TextAsset textAsset = Resources.Load<TextAsset>(baseResourcePath);
            if (textAsset != null)
            {
                Debug.Log($"{_LOG_PREFIX} Debug [OK] Found: {baseResourcePath} -> {textAsset.name}");
                return;
            }

            Debug.LogError($"{_LOG_PREFIX} Debug [NG] No TextAsset found for: {baseResourcePath}");
        }
    }
}
