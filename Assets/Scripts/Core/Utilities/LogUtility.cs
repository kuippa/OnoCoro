using System;
using System.IO;
using UnityEngine;
using CommonsUtility;

/// <summary>
/// ログファイル管理ユーティリティクラス
/// - テキストをログファイルに記録（Warning レベル以上のみ）
/// - ビルド環境でのファイル出力機能を提供
/// - エディタでのファイル出力は無効
/// </summary>
public static class LogUtility
{
    public enum LogLevel
    {
        /// <summary>エディタ環境でのみ最詳細ログをファイル出力</summary>
        Editor = 0,

        /// <summary>すべてのログをファイル出力</summary>
        Log = 1,

        /// <summary>警告ログ以上をファイル出力</summary>
        Warning = 2,

        /// <summary>エラーのみファイル出力</summary>
        Error = 3,

        /// <summary>ファイル出力なし</summary>
        None = 99
    }

    // ログ出力フォーマット定数
    private const string _DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
    private const string _MSG_CLEAR_LOG_FAILED = "Failed to clear log file: ";
    private const string _MSG_WRITE_LOG_FAILED = "Failed to write log to file: ";
    private const string _MSG_INIT_LOG_FAILED = "Failed to initialize log file: ";
    
    // ログファイルヘッダー定数
    private const string _LOG_HEADER_START = "=== Game Log Started ===";
    private const string _LOG_HEADER_APP = "Application: ";
    private const string _LOG_HEADER_VERSION = "Version: ";
    private const string _LOG_HEADER_PLATFORM = "Platform: ";
    private const string _LOG_HEADER_UNITY = "Unity Version: ";
    private const string _LOG_HEADER_STARTED = "Started: ";
    private const string _LOG_HEADER_SEPARATOR = "===========================";

    private static readonly object _fileLock = new object();
    private static StreamWriter _logWriter = null;
    private static bool _isInitialized = false;

    /// <summary>
    /// ログをファイルに記録
    /// enum の数値に基づいて出力判定を実施
    /// 外部は本メソッドのみを使用
    /// </summary>
    public static void WriteLog(LogLevel level, string message)
    {
        LogLevel currentLogLevel = GameConfig.LogLevel;
        // None の場合は出力しない
        if (currentLogLevel == LogLevel.None)
        {
            return;
        }

        // Log/Warning/Error の場合は、level >= currentLogLevel で出力判定
        // 例: currentLogLevel=Warning(2) の場合、Warning(2)以上のログのみファイル出力
        if (level >= currentLogLevel)
        {
            WriteToFile(level, message);
        }
    }

    /// <summary>
    /// ログファイルパスを取得（ユーザーがログを確認・提出する際に使用）
    /// </summary>
    public static string GetLogFilePath()
    {
        return GameConfig.LogFilePath;
    }

    /// <summary>
    /// ログファイルをクリア（新しいセッション開始時など）
    /// </summary>
    public static void ClearLogFile()
    {
        try
        {
            lock (_fileLock)
            {
                // ファイルポインタが開いていれば閉じる
                if (_logWriter != null)
                {
                    _logWriter.Flush();
                    _logWriter.Close();
                    _logWriter.Dispose();
                    _logWriter = null;
                    _isInitialized = false;
                }

                // ログファイルを削除
                if (File.Exists(GetLogFileName()))
                {
                    File.Delete(GetLogFileName());
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"{_MSG_CLEAR_LOG_FAILED}{ex.Message}");
        }
    }

    /// <summary>
    /// ログファイルをクローズ（アプリケーション終了時に呼び出す）
    /// </summary>
    public static void CloseLogFile()
    {
        try
        {
            lock (_fileLock)
            {
                if (_logWriter != null)
                {
                    _logWriter.Flush();
                    _logWriter.Close();
                    _logWriter.Dispose();
                    _logWriter = null;
                    _isInitialized = false;
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"Failed to close log file: {ex.Message}");
        }
    }

    /// <summary>
    /// ログをファイルに書き込み（内部メソッド）
    /// StreamWriter を使用してファイルポインタを保持し、効率化
    /// </summary>
    private static void WriteToFile(LogLevel level, string message)
    {
        try
        {
            lock (_fileLock)
            {
                // 初回のみ初期化（StreamWriter を開く）
                if (_logWriter == null && !_isInitialized)
                {
                    InitializeLogFile();
                    _isInitialized = true;
                }

                // ファイルポインタが有効な場合は書き込み
                if (_logWriter != null)
                {
                    string timestamp = DateTime.Now.ToString(_DATETIME_FORMAT);
                    string logEntry = $"[{level}] {timestamp} - {message}{Environment.NewLine}";
                    _logWriter.Write(logEntry);
                    _logWriter.Flush();  // バッファをディスクに書き込み
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"{_MSG_WRITE_LOG_FAILED}{ex.Message}");
        }
    }

    /// <summary>
    /// ログファイルを初期化（StreamWriter を開いてヘッダー書き込み）
    /// </summary>
    private static void InitializeLogFile()
    {
        try
        {
            string logFilePath = GetLogFileName();

            // ログディレクトリがなければ作成
            string directory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // StreamWriter を開く（追記モード、自動フラッシュ有効）
            _logWriter = new StreamWriter(logFilePath, true);
            _logWriter.AutoFlush = false;  // 手動でFlush制御

            // ヘッダー書き込み
            string header = $"{_LOG_HEADER_START}{Environment.NewLine}";
            header += $"{_LOG_HEADER_APP}{UnityEngine.Application.productName}{Environment.NewLine}";
            header += $"{_LOG_HEADER_VERSION}{UnityEngine.Application.version}{Environment.NewLine}";
            header += $"{_LOG_HEADER_PLATFORM}{UnityEngine.Application.platform}{Environment.NewLine}";
            header += $"{_LOG_HEADER_UNITY}{UnityEngine.Application.unityVersion}{Environment.NewLine}";
            header += $"{_LOG_HEADER_STARTED}{DateTime.Now.ToString(_DATETIME_FORMAT)}{Environment.NewLine}";
            header += $"{_LOG_HEADER_SEPARATOR}{Environment.NewLine}{Environment.NewLine}";

            _logWriter.Write(header);
            _logWriter.Flush();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"{_MSG_INIT_LOG_FAILED}{ex.Message}");
            _logWriter = null;
        }
    }

    private static string GetLogFileName()
    {
        string datePrefix = DateTime.Now.ToString("yyyyMMdd");
        string fileName = $"{datePrefix}_{GameConfig.LogFileName}";
        string combinedPath = System.IO.Path.Combine(GameConfig.LogFilePath, fileName);
        
        // スラッシュを統一（Unity の慣習は / を使用）
        // char separator = System.IO.Path.DirectorySeparatorChar;
        return combinedPath.Replace("\\", "/");
    }

}
