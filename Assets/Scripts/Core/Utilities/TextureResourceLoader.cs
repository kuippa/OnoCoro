using UnityEngine;

namespace CommonsUtility
{
    /// <summary>
    /// Texture リソース読み込みのユーティリティクラス
    /// Texture2D（カーソル画像、テクスチャなど）の読み込みを一元化し、
    /// null チェックとデバッグログを提供します
    /// </summary>
    internal static class TextureResourceLoader
    {
        private const string _LOG_PREFIX = "[TextureResourceLoader]";

        /// <summary>
        /// パスから Texture2D をロードする
        /// </summary>
        /// <param name="resourcePath">Resources フォルダ相対パス（拡張子なし）</param>
        /// <returns>読み込まれた Texture2D、失敗時は null</returns>
        public static Texture2D LoadTexture(string resourcePath)
        {
            if (!ValidateResourcePath(resourcePath))
            {
                return null;
            }

            try
            {
                Texture2D texture = Resources.Load<Texture2D>(resourcePath);
                if (texture != null)
                {
                    return texture;
                }

                Debug.LogWarning($"{_LOG_PREFIX} Failed to load texture from path: {resourcePath}");
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{_LOG_PREFIX} Exception while loading texture '{resourcePath}': {ex.Message}");
                return null;
            }
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
        public static bool VerifyTexture(string resourcePath)
        {
            Texture2D texture = LoadTexture(resourcePath);
            bool isValid = texture != null;

            if (!isValid)
            {
                Debug.LogWarning($"{_LOG_PREFIX} Verify result for '{resourcePath}': FAILED");
            }

            return isValid;
        }

        /// <summary>
        /// デバッグ用: 実際にロード可能なパスを検索
        /// </summary>
        public static void DebugFindAvailableTexturePaths(string baseResourcePath)
        {
            if (!ValidateResourcePath(baseResourcePath))
            {
                return;
            }

            Debug.Log($"{_LOG_PREFIX} Debug Searching available texture paths for: {baseResourcePath}");

            Texture2D texture = Resources.Load<Texture2D>(baseResourcePath);
            if (texture != null)
            {
                Debug.Log($"{_LOG_PREFIX} Debug [OK] Found: {baseResourcePath} -> {texture.name}");
                return;
            }

            Debug.LogError($"{_LOG_PREFIX} Debug [NG] No texture found for: {baseResourcePath}");
        }
    }
}
