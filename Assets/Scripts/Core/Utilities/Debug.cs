using System;
using UnityEngine;

namespace CommonsUtility
{
    /// <summary>
    /// デバッグ出力レベルの定義
    /// ログ出力を制御するためのレベル指定
    /// </summary>
    public enum DebugLevel
    {
        /// <summary>
        /// 通常の開発時の設定。呼び出し元情報を付けつつ Log 以上を出力する。
        /// トレース（LogTrace）は出さないので、Console が追跡可能な量に保たれる
        /// </summary>
        Editor = 0,

        /// <summary>
        /// トレースを含めてすべて出力する。特定の不具合を追うときだけ一時的に使う。
        /// 常用すると Console が流れて必要な情報が追えなくなる
        /// </summary>
        Trace = 1,

        /// <summary>通常のログ以上を出力（トレースは出さない）</summary>
        Log = 2,

        /// <summary>警告ログ以上を出力</summary>
        Warning = 3,

        /// <summary>エラーのみ出力</summary>
        Error = 4,

        /// <summary>ログ出力なし</summary>
        None = 99
    }

    /// <summary>
    /// Debug ログ統一管理クラス
    /// using Debug = CommonsUtility.Debug; で既存コードと互換性を保つ
    /// 
    /// 内部的には UnityEngine.Debug に委譲し、デバッグレベルの制御やログ出力の管理を統一化
    /// GameConfig.DebugLevel で実行時に出力レベルを制御可能
    /// 
    /// 【使用例】
    /// - 通常ログ:
    ///   Debug.Log("ゲーム開始");
    ///   // Output: [OnoCoro] ゲーム開始
    ///
    /// - 警告ログ:
    ///   Debug.LogWarning("リソース不足");
    ///   // Output: [OnoCoro] リソース不足
    ///
    /// - エラーログ:
    ///   Debug.LogError("初期化失敗");
    ///   // Output: [OnoCoro] 初期化失敗
    /// </summary>
    public static class Debug
    {
        // private const string PREFIX = "[OnoCoro]";
        private const string PREFIX = "";

        /// <summary>
        /// 通常のログを出力（DebugLevel: Editor の場合は呼び出し元情報を含める）
        /// </summary>
        public static void Log(string message)
        {
            if (ShouldLog(DebugLevel.Log))
            {
                string formattedMessage = BuildLogMessage(message);
                UnityEngine.Debug.Log(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Log, formattedMessage);
            }
        }

        /// <summary>
        /// トレースログを出力（既定では出力されない）
        ///
        /// ループ内・毎フレーム・オブジェクト単位で出るような、
        /// 平常時には読む必要のない追跡用ログはこちらを使う。
        /// 見たいときだけ GameConfig.DebugLevel = DebugLevel.Trace にする
        /// </summary>
        public static void LogTrace(string message)
        {
            if (ShouldLog(DebugLevel.Trace))
            {
                string formattedMessage = BuildLogMessage(message);
                UnityEngine.Debug.Log(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Log, formattedMessage);
            }
        }

        /// <summary>
        /// フォーマット指定でログを出力（DebugLevel: Editor の場合は呼び出し元情報を含める）
        /// </summary>
        public static void LogFormat(string format, params object[] args)
        {
            if (ShouldLog(DebugLevel.Log))
            {
                string message = string.Format(format, args);
                string formattedMessage = BuildLogMessage(message);
                UnityEngine.Debug.Log(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Log, formattedMessage);
            }
        }

        /// <summary>
        /// 警告ログを出力（DebugLevel: Editor の場合は呼び出し元情報を含める）
        /// </summary>
        public static void LogWarning(string message)
        {
            if (ShouldLog(DebugLevel.Warning))
            {
                string formattedMessage = BuildLogMessage(message);
                UnityEngine.Debug.LogWarning(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Warning, formattedMessage);
            }
        }

        /// <summary>
        /// フォーマット指定で警告ログを出力（DebugLevel: Editor の場合は呼び出し元情報を含める）
        /// </summary>
        public static void LogWarningFormat(string format, params object[] args)
        {
            if (ShouldLog(DebugLevel.Warning))
            {
                string message = string.Format(format, args);
                string formattedMessage = BuildLogMessage(message);
                UnityEngine.Debug.LogWarning(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Warning, formattedMessage);
            }
        }

        /// <summary>
        /// エラーログを出力（DebugLevel: Editor の場合は呼び出し元情報を含める）
        /// </summary>
        public static void LogError(string message)
        {
            if (ShouldLog(DebugLevel.Error))
            {
                string formattedMessage = BuildLogMessage(message);
                UnityEngine.Debug.LogError(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Error, formattedMessage);
            }
        }

        /// <summary>
        /// フォーマット指定でエラーログを出力（DebugLevel: Editor の場合は呼び出し元情報を含める）
        /// </summary>
        public static void LogErrorFormat(string format, params object[] args)
        {
            if (ShouldLog(DebugLevel.Error))
            {
                string message = string.Format(format, args);
                string formattedMessage = BuildLogMessage(message);
                UnityEngine.Debug.LogError(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Error, formattedMessage);
            }
        }

        /// <summary>
        /// 例外情報をログ出力（DebugLevel: Editor の場合は呼び出し元情報を含める）
        /// </summary>
        public static void LogException(Exception exception, string context = "")
        {
            if (ShouldLog(DebugLevel.Error))
            {
                string contextStr = string.IsNullOrEmpty(context) ? "" : $"[{context}] ";
                string message = $"{contextStr}例外発生: {exception.Message}\n{exception.StackTrace}";
                string formattedMessage = BuildLogMessage(message);
                UnityEngine.Debug.LogError(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Error, formattedMessage);
            }
        }

        /// <summary>
        /// アサーション（条件確認）
        /// 条件が偽の場合、エラーログを出力（DebugLevel: Editor の場合は呼び出し元情報を含める）
        /// </summary>
        public static void Assert(bool condition, string message)
        {
            if (ShouldLog(DebugLevel.Error) && !condition)
            {
                string assertMessage = $"[Assert] {message}";
                string formattedMessage = BuildLogMessage(assertMessage);
                UnityEngine.Debug.LogError(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Error, formattedMessage);
            }
        }

        /// <summary>
        /// 処理実行時間を測定してログ出力（DebugLevel: Editor の場合は呼び出し元情報を含める）
        /// </summary>
        public static void LogExecutionTime(string processName, System.Action action)
        {
            if (ShouldLog(DebugLevel.Log))
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                action();
                watch.Stop();
                string message = $"[{processName}] 実行時間: {watch.ElapsedMilliseconds}ms";
                string formattedMessage = BuildLogMessage(message);
                UnityEngine.Debug.Log(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Log, formattedMessage);
            }
        }

        /// <summary>
        /// タイムスタンプと起動からの経過時間付きでログを出力
        /// Bootstrap フェーズの計測用に使用
        /// 出力例: [10:23:45.123] [5.234s] PLATEAU初期化完了
        /// </summary>
        public static void LogWithMilliseconds(string message)
        {
            if (ShouldLog(DebugLevel.Log))
            {
                string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
                float elapsedSeconds = Time.realtimeSinceStartup;
                string formattedMessage = BuildLogMessage($"[{timestamp}] [{elapsedSeconds:F3}s] {message}");
                UnityEngine.Debug.Log(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Log, formattedMessage);
            }
        }

        /// <summary>
        /// ミリ秒精度のタイムスタンプ付きでログを出力
        /// 時刻のみが必要な場合はこちらを使用
        /// </summary>
        public static void LogWithTimestamp(string message)
        {
            if (ShouldLog(DebugLevel.Log))
            {
                string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
                string formattedMessage = BuildLogMessage($"[{timestamp}] {message}");
                UnityEngine.Debug.Log(formattedMessage);
                LogUtility.WriteLog(LogUtility.LogLevel.Log, formattedMessage);
            }
        }

        /// <summary>
        /// ログメッセージを構築（Console と ファイル出力で共通使用）
        /// PREFIX + caller情報 + メッセージを結合
        /// </summary>
        private static string BuildLogMessage(string message)
        {
            string caller = GameConfig.DebugLevel == DebugLevel.Editor ? $"[{GetCallerInfo()}] " : "";
            return $"{PREFIX} {caller}{message}";
        }

        /// <summary>
        /// 呼び出し元のメソッド情報を取得
        /// CommonsUtility.Debug クラス以外の最初のメソッドを探す
        /// </summary>
        private static string GetCallerInfo()
        {
            var stackTrace = new System.Diagnostics.StackTrace(true);
            
            // CommonsUtility.Debug クラス以外の最初のメソッドを探す
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();
                var type = method?.DeclaringType;
                
                // Debug クラス以外の場合がそこが呼び出し元
                if (type?.Name != "Debug" || type?.Namespace != "CommonsUtility")
                {
                    return $"{type?.Name}.{method?.Name}";
                }
            }
            
            return "Unknown";
        }

        /// <summary>
        /// シーンビュー内にレイを描画（UnityEngine.Debug.DrawRay のラッパー）
        /// </summary>
        public static void DrawRay(UnityEngine.Vector3 start, UnityEngine.Vector3 direction, UnityEngine.Color color = default, float duration = 0f, bool depthTest = true)
        {
            UnityEngine.Debug.DrawRay(start, direction, color, duration, depthTest);
        }

        /// <summary>
        /// 現在のデバッグレベルでログ出力すべきかを判定
        /// 
        /// DebugLevel のenumにおける数値順序に準拠：
        /// Editor(0)→Log以上＋呼び出し元情報 / Trace(1)→全出力 > Log(2) > Warning(3) > Error(4) > None(99)→沈黙
        /// </summary>
        private static bool ShouldLog(DebugLevel requiredLevel)
        {
            DebugLevel currentLevel = GameConfig.DebugLevel;

            // None の場合は常に出力しない
            if (currentLevel == DebugLevel.None)
            {
                return false;
            }

            // Editor は「呼び出し元情報つきで Log 以上」。
            // トレースまで出すと Console が流れて必要な情報が追えなくなるため、
            // トレースを見たいときは明示的に DebugLevel.Trace を指定する
            if (currentLevel == DebugLevel.Editor)
            {
                return requiredLevel >= DebugLevel.Log;
            }

            // Log 以上のレベルでは、requiredLevel に基づいて判定
            // Log(1) 時は requiredLevel >= 1 で常にtrue（全レベル出力）
            // Warning(2) 時は requiredLevel >= 2（Warning以上）
            // Error(3) 時は requiredLevel >= 3（Errorのみ）
            return requiredLevel >= currentLevel;
        }
    }
}

