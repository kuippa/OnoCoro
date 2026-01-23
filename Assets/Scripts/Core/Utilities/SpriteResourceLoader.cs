using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sprite リソース読み込みのユーティリティクラス
/// SVG -> Sprite 変換に対応し、null チェックとデバッグログを提供
///
/// SVG インポート手順の詳細については、Assets/Documentation/svg-image-import-guide.md を参照してください。
/// Asset Type を "UI SVGImage" に設定し、Image コンポーネントで "Sprite Mesh を使用" チェックボックスを有効にしてください。
/// 
/// NOTE: 古い環境や異なる SVG インポート設定を使用する場合：
///   - LoadSprite() メソッド内の ConvertSvgPathToSpritePath() ロジックを変更してください
///   - _SVG_SPRITE_SUFFIX の値（現在: "Sprite"）を環境に合わせて調整してください
///   - 例：古い Unity バージョンでは Sprite サフィックスが異なる場合があります
/// </summary>
public static class SpriteResourceLoader
{
    // SVG インポート後の Sprite 命名規則
    // 古い環境では異なる場合があります（環境に応じて変更可能）
    private const string _SVG_SPRITE_SUFFIX = "Sprite";
    private const string _LOG_PREFIX = "[SpriteResourceLoader]";

    /// <summary>
    /// パスから Sprite をロードする
    /// SVG ファイルの場合、自動的に生成された Sprite を参照
    /// </summary>
    /// <param name="resourcePath">Resources フォルダ相対パス（拡張子なし）</param>
    /// <returns>読み込まれた Sprite、失敗時は null</returns>
    public static Sprite LoadSprite(string resourcePath)
    {
        if (!ValidateResourcePath(resourcePath))
        {
            return null;
        }

        try
        {
            // Asset Type が "UI SVGImage" の場合、元のパスで自動生成される Sprite をロード可能
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                return sprite;
            }

            // パターン2：古い環境や異なる設定の場合、サフィックス付きで再試行
            string spritePathWithSuffix = ConvertSvgPathToSpritePath(resourcePath);
            sprite = Resources.Load<Sprite>(spritePathWithSuffix);
            if (sprite != null)
            {
                return sprite;
            }

            Debug.LogWarning($"{_LOG_PREFIX} Failed to load sprite from path: {resourcePath}");
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{_LOG_PREFIX} Exception while loading sprite '{resourcePath}': {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// SVG パスを Sprite パスに変換
    /// 例: "imgs/icons/spaghetti-monster-flying-solid" -> 
    ///      "imgs/icons/spaghetti-monster-flying-solidSprite"
    /// </summary>
    private static string ConvertSvgPathToSpritePath(string resourcePath)
    {
        // SVG ファイルとして認識される場合、Sprite サフィックスを追加
        // Asset Type が "UI SVGImage" の場合、自動生成される Sprite の命名規則
        return resourcePath + _SVG_SPRITE_SUFFIX;
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
    /// Sprite を Image コンポーネントに設定
    /// SVG Sprite Mesh 対応
    /// エラー時は Image の透明度を設定して視覚的にフィードバック
    /// </summary>
    public static void SetSpriteToImage(Image image, string resourcePath)
    {
        if (!ValidateImageComponent(image))
        {
            return;
        }

        if (!ValidateResourcePath(resourcePath))
        {
            SetImageToErrorState(image);
            return;
        }

        Sprite sprite = LoadSprite(resourcePath);
        if (sprite == null)
        {
            Debug.LogWarning($"{_LOG_PREFIX} Cannot set sprite to image: {resourcePath}");
            SetImageToErrorState(image);
            return;
        }

        try
        {
            image.sprite = sprite;
            
            // スプライト設定成功時は透明度を戻す
            Color color = image.color;
            color.a = 1f;
            image.color = color;
            image.enabled = true;

            // SVG スプライトメッシュを自動有効化
            EnableSpriteMeshForImage(image);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{_LOG_PREFIX} Exception while setting sprite to image: {ex.Message}");
            SetImageToErrorState(image);
        }
    }

    /// <summary>
    /// Image コンポーネントの検証
    /// </summary>
    private static bool ValidateImageComponent(Image image)
    {
        if (image == null)
        {
            Debug.LogError($"{_LOG_PREFIX} Image component is null");
            return false;
        }

        if (!image.isActiveAndEnabled)
        {
            Debug.LogWarning($"{_LOG_PREFIX} Image component is not active or enabled");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Image をエラー状態に設定
    /// 透明度を下げて読み込み失敗を視覚的に示す
    /// </summary>
    private static void SetImageToErrorState(Image image)
    {
        if (image == null)
        {
            return;
        }

        try
        {
            // エラー状態：透明度 0.3f
            Color color = image.color;
            color.a = 0.3f;
            image.color = color;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{_LOG_PREFIX} Exception while setting error state: {ex.Message}");
        }
    }

    /// <summary>
    /// SVG スプライトメッシュを自動有効化
    /// Unity 6.3 VectorGraphics パッケージの機能
    /// </summary>
    private static void EnableSpriteMeshForImage(Image image)
    {
        if (image == null)
        {
            return;
        }

        try
        {
            // Unity 6.3 VectorGraphics: useSpriteMesh プロパティ
            var property = image.GetType().GetProperty("useSpriteMesh");
            if (property != null && property.CanWrite)
            {
                property.SetValue(image, true);
                return;
            }

            // フォールバック: プロパティが見つからない場合
            Debug.LogWarning($"{_LOG_PREFIX} useSpriteMesh property not found on Image component");
        }
        catch (System.Exception ex)
        {
            // プロパティが存在しない環境では無視（Reflection エラーは最小限に記録）
            Debug.LogWarning($"{_LOG_PREFIX} Sprite Mesh property configuration failed: {ex.Message}");
        }
    }

    /// <summary>
    /// デバッグ用: ロード結果を検証
    /// </summary>
    public static bool VerifySprite(string resourcePath)
    {
        Sprite sprite = LoadSprite(resourcePath);
        bool isValid = sprite != null;

        if (!isValid)
        {
            Debug.LogWarning($"{_LOG_PREFIX} Verify result for '{resourcePath}': FAILED");
        }
        
        return isValid;
    }

    /// <summary>
    /// デバッグ用: 実際にロード可能なパスを検索
    /// Sprite名の命名規則が環境により異なる場合に使用
    /// </summary>
    public static void DebugFindAvailableSpritePaths(string baseResourcePath)
    {
        if (!ValidateResourcePath(baseResourcePath))
        {
            return;
        }

        Debug.Log($"{_LOG_PREFIX}:Debug Searching available sprite paths for: {baseResourcePath}");

        // パターン1: サフィックスなし（元のパス）
        Sprite sprite1 = Resources.Load<Sprite>(baseResourcePath);
        if (sprite1 != null)
        {
            Debug.Log($"{_LOG_PREFIX}:Debug ✓ Found: {baseResourcePath} -> {sprite1.name}");
            return;
        }

        // パターン2: Sprite サフィックス付き
        string pathWithSprite = baseResourcePath + _SVG_SPRITE_SUFFIX;
        Sprite sprite2 = Resources.Load<Sprite>(pathWithSprite);
        if (sprite2 != null)
        {
            Debug.Log($"{_LOG_PREFIX}:Debug ✓ Found: {pathWithSprite} -> {sprite2.name}");
            return;
        }

        // パターン3: ファイル名の末尾の "Sprite" を削除したパターン
        // （SVGが既に "Sprite" を含む場合）
        if (baseResourcePath.EndsWith(_SVG_SPRITE_SUFFIX))
        {
            string pathWithoutSuffix = baseResourcePath.Substring(0, baseResourcePath.Length - _SVG_SPRITE_SUFFIX.Length);
            Sprite sprite3 = Resources.Load<Sprite>(pathWithoutSuffix);
            if (sprite3 != null)
            {
                Debug.Log($"{_LOG_PREFIX}:Debug ✓ Found: {pathWithoutSuffix} -> {sprite3.name}");
                return;
            }
        }

        Debug.LogError($"{_LOG_PREFIX}:Debug ✗ No available sprite found for: {baseResourcePath}");
        Debug.LogError($"{_LOG_PREFIX}:Debug Tried paths:");
        Debug.LogError($"{_LOG_PREFIX}:Debug   1. {baseResourcePath}");
        Debug.LogError($"{_LOG_PREFIX}:Debug   2. {pathWithSprite}");
        if (baseResourcePath.EndsWith(_SVG_SPRITE_SUFFIX))
        {
            Debug.LogError($"{_LOG_PREFIX}:Debug   3. {baseResourcePath.Substring(0, baseResourcePath.Length - _SVG_SPRITE_SUFFIX.Length)}");
        }
    }
}
