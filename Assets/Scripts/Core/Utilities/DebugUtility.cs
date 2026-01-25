using System;
using Debug = UnityEngine.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// Debug ログ統一管理ユーティリティ
    /// 
    /// 【使用例】
    /// - 通常ログ:
    ///   DebugUtility.Log("ゲーム開始");
    ///   // Output: [OnoCoro] ゲーム開始
    ///
    /// - 警告ログ:
    ///   DebugUtility.LogWarning("リソース不足");
    ///   // Output: [OnoCoro] リソース不足
    ///
    /// - エラーログ:
    ///   DebugUtility.LogError("初期化失敗");
    ///   // Output: [OnoCoro] 初期化失敗
    ///
    /// 【デバッグビルド/本番ビルドの自動分離】
    /// - DEBUGシンボルが定義されている時のみログ出力
    /// - 本番ビルドでは完全に削除される（パフォーマンス最適化）
    /// </summary>
    public static class DebugUtility
    {
        private const string PREFIX = "[OnoCoro]";

        /// <summary>
        /// 通常のログを出力（DEBUGビルドのみ）
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string message)
        {
            Debug.Log($"{PREFIX} {message}");
        }

        /// <summary>
        /// フォーマット指定でログを出力（DEBUGビルドのみ）
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogFormat(string format, params object[] args)
        {
            Debug.LogFormat($"{PREFIX} {format}", args);
        }

        /// <summary>
        /// 警告ログを出力（DEBUGビルドのみ）
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"{PREFIX} {message}");
        }

        /// <summary>
        /// フォーマット指定で警告ログを出力（DEBUGビルドのみ）
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogWarningFormat(string format, params object[] args)
        {
            Debug.LogWarningFormat($"{PREFIX} {format}", args);
        }

        /// <summary>
        /// エラーログを出力（DEBUGビルドのみ）
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogError(string message)
        {
            Debug.LogError($"{PREFIX} {message}");
        }

        /// <summary>
        /// フォーマット指定でエラーログを出力（DEBUGビルドのみ）
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogErrorFormat(string format, params object[] args)
        {
            Debug.LogErrorFormat($"{PREFIX} {format}", args);
        }

        /// <summary>
        /// 例外情報をログ出力（DEBUGビルドのみ）
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogException(Exception exception, string context = "")
        {
            string contextStr = string.IsNullOrEmpty(context) ? "" : $"[{context}] ";
            Debug.LogError($"{PREFIX} {contextStr}例外発生: {exception.Message}\n{exception.StackTrace}");
        }

        /// <summary>
        /// アサーション（条件確認）
        /// 条件が偽の場合、エラーログを出力（DEBUGビルドのみ）
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Debug.LogError($"{PREFIX} [Assert] {message}");
            }
        }

        /// <summary>
        /// 処理実行時間を測定してログ出力（DEBUGビルドのみ）
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogExecutionTime(string processName, System.Action action)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            action();
            watch.Stop();
            Debug.Log($"{PREFIX} [{processName}] 実行時間: {watch.ElapsedMilliseconds}ms");
        }
    }
}
