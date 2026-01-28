using UnityEngine;

namespace CommonsUtility
{
    /// <summary>
    /// カーソル管理クラス
    /// カーソルアイコンの読み込みと設定を一元管理します
    /// </summary>
    internal static class CursorManager
    {
        private const string _LOG_PREFIX = "[CursorManager]";
        
        // キャッシュ
        private static Texture2D _cachedCursorTexture = null;
        private static string _cachedCursorPath = "";

        /// <summary>
        /// ゲーム起動時に呼び出す初期化処理
        /// 初期カーソル状態を設定します
        /// </summary>
        public static void Initialize()
        {
            // 初期状態：マウスをロック + カーソルアイコンを設定
            SetCursorState(CursorLockMode.Locked);
            SetCursorIcon();
        }

        /// <summary>
        /// カーソルアイコンを設定
        /// リソースから読み込み、キャッシュして効率化
        /// </summary>
        public static void SetCursorIcon()
        {
            Texture2D cursorTexture = GetCachedCursorTexture();
            if (cursorTexture == null)
            {
                return;
            }

            ApplyCursorIcon(cursorTexture);
        }

        /// <summary>
        /// カーソルロック状態を設定（ゲームプレイ中の入力状態に応じた制御）
        /// </summary>
        /// <param name="isLooking">true=ルック中（ロック解除）、false=通常移動（ロック）</param>
        public static void SetCursorLockMode(bool isLooking)
        {
            // isLooking=true: ルック入力時 → マウスを表示
            // isLooking=false: 通常移動時 → マウスをロック
            CursorLockMode lockMode = isLooking ? CursorLockMode.None : CursorLockMode.Locked;
            
            SetCursorState(lockMode);
        }

        /// <summary>
        /// カーソルロック状態を直接設定（高度な制御用）
        /// 
        /// CursorLockMode.None: カーソルが表示され、ゲーム内外を自由に移動できます
        /// CursorLockMode.Locked: カーソルが消え、ゲームウィンドウの中央にロックされます（FPS用）
        /// CursorLockMode.Confined: カーソルがゲーム内のみに制限され、外側に出られません
        /// </summary>
        public static void SetCursorState(CursorLockMode lockMode)
        {
            if (Cursor.lockState == lockMode)
            {
                return;
            }

            Cursor.lockState = lockMode;

            // ロック解除時にカーソルアイコンを設定
            if (lockMode == CursorLockMode.None)
            {
                SetCursorIcon();
            }
        }

        /// <summary>
        /// キャッシュを考慮したカーソルテクスチャ取得
        /// </summary>
        private static Texture2D GetCachedCursorTexture()
        {
            string resourcePath = GameConfig.CursorIconPath;
            
            // リソースパスが指定されていない場合はデフォルトカーソルを使用
            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }
            
            // 既にキャッシュされている場合は使用
            if (_cachedCursorPath == resourcePath && _cachedCursorTexture != null)
            {
                return _cachedCursorTexture;
            }

            // 新しいテクスチャを読み込み
            Texture2D texture = TextureResourceLoader.LoadTexture(resourcePath);
            if (texture == null)
            {
                Debug.LogError($"{_LOG_PREFIX} Failed to load cursor texture: {resourcePath}");
                return null;
            }

            // キャッシュに保存
            _cachedCursorTexture = texture;
            _cachedCursorPath = resourcePath;

            return texture;
        }

        /// <summary>
        /// カーソルアイコンを実際に適用
        /// </summary>
        private static void ApplyCursorIcon(Texture2D cursorTexture)
        {
            if (cursorTexture == null)
            {
                Debug.LogError($"{_LOG_PREFIX} Cursor texture is null");
                return;
            }

            try
            {
                Vector2 hotSpot = Vector2.zero;
                CursorMode cursorMode = CursorMode.Auto;

                // カーソルがロックされていない場合のみ表示
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{_LOG_PREFIX} Exception while setting cursor: {ex.Message}");
            }
        }

        /// <summary>
        /// デバッグ用: キャッシュ情報を取得
        /// </summary>
        public static void DebugLogCacheInfo()
        {
            Debug.Log($"{_LOG_PREFIX} Cached path: {_cachedCursorPath}");
            Debug.Log($"{_LOG_PREFIX} Cached texture: {(_cachedCursorTexture != null ? _cachedCursorTexture.name : "null")}");
        }

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public static void ClearCache()
        {
            _cachedCursorTexture = null;
            _cachedCursorPath = "";
        }
    }
}
