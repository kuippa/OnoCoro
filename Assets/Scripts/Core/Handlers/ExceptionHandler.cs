using System;
using OnoCoro.Core.Utilities;

namespace OnoCoro.Core.Handlers
{
    /// <summary>
    /// 例外処理統一管理クラス
    /// すべての例外をここで一元管理、ログ記録、UI通知などを統一
    ///
    /// 【使用例】
    /// - 基本的なエラーハンドリング:
    ///   try
    ///   {
    ///       spawnSystem.SpawnEnemy();
    ///   }
    ///   catch (System.Exception ex)
    ///   {
    ///       ExceptionHandler.Handle(ex, "EnemySpawn");
    ///   }
    ///
    /// - 安全実行（例外を自動キャッチ）:
    ///   ExceptionHandler.HandleSafe(() => 
    ///   {
    ///       spawnSystem.SpawnEnemy();
    ///   }, "EnemySpawn");
    ///
    /// - 戻り値付き安全実行:
    ///   var result = ExceptionHandler.HandleSafe<bool>(
    ///       () => gameManager.InitializeGame(),
    ///       false,  // デフォルト戻り値
    ///       "GameInitialize"
    ///   );
    ///
    /// - async/await パターン:
    ///   await ExceptionHandler.HandleSafeAsync(
    ///       async () => await networkManager.LoadStageAsync(),
    ///       "StageLoad"
    ///   );
    /// </summary>
    public static class ExceptionHandler
    {
        /// <summary>
        /// 例外を処理（ログ出力 + 通知）
        /// </summary>
        public static void Handle(Exception exception, string context = "")
        {
            if (exception == null)
            {
                return;
            }

            string contextStr = string.IsNullOrEmpty(context) ? "" : $"[{context}] ";
            string errorMessage = $"{contextStr}例外発生: {exception.Message}\nスタックトレース: {exception.StackTrace}";

            DebugUtility.LogError(errorMessage);

            // ここで追加処理を実装：
            // - エラーログファイルに記録
            // - 分析サーバーに送信
            // - UI通知（ダイアログ表示など）
            // LogToFile(errorMessage);
            // SendToAnalyticsServer(exception, context);
            // ShowErrorDialog(errorMessage);
        }

        /// <summary>
        /// アクションを安全に実行（例外を自動キャッチ）
        /// </summary>
        public static void HandleSafe(Action action, string context = "")
        {
            if (action == null)
            {
                DebugUtility.LogError("HandleSafe: action がnullです");
                return;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Handle(ex, context);
            }
        }

        /// <summary>
        /// 戻り値付きアクションを安全に実行
        /// </summary>
        public static T HandleSafe<T>(Func<T> action, T defaultValue, string context = "")
        {
            if (action == null)
            {
                DebugUtility.LogError("HandleSafe<T>: action がnullです");
                return defaultValue;
            }

            try
            {
                return action();
            }
            catch (Exception ex)
            {
                Handle(ex, context);
                return defaultValue;
            }
        }

        /// <summary>
        /// 非同期アクションを安全に実行
        /// </summary>
        public static async System.Threading.Tasks.Task HandleSafeAsync(
            Func<System.Threading.Tasks.Task> asyncAction,
            string context = "")
        {
            if (asyncAction == null)
            {
                DebugUtility.LogError("HandleSafeAsync: asyncAction がnullです");
                return;
            }

            try
            {
                await asyncAction();
            }
            catch (Exception ex)
            {
                Handle(ex, context);
            }
        }

        /// <summary>
        /// 戻り値付き非同期アクションを安全に実行
        /// </summary>
        public static async System.Threading.Tasks.Task<T> HandleSafeAsync<T>(
            Func<System.Threading.Tasks.Task<T>> asyncAction,
            T defaultValue,
            string context = "")
        {
            if (asyncAction == null)
            {
                DebugUtility.LogError("HandleSafeAsync<T>: asyncAction がnullです");
                return defaultValue;
            }

            try
            {
                return await asyncAction();
            }
            catch (Exception ex)
            {
                Handle(ex, context);
                return defaultValue;
            }
        }

        /// <summary>
        /// 例外ハンドラーを登録（アプリ全体の未処理例外をキャッチ）
        /// InitializationManager.cs などで初期化時に呼び出し
        /// </summary>
        public static void RegisterGlobalExceptionHandler()
        {
            System.AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = args.ExceptionObject as Exception;
                Handle(ex, "GlobalUnhandled");
            };

            UnityEngine.Application.logMessageReceivedThreaded += (logString, stackTrace, logType) =>
            {
                if (logType == UnityEngine.LogType.Exception)
                {
                    DebugUtility.LogError($"グローバル例外: {logString}\n{stackTrace}");
                }
            };
        }
    }
}
