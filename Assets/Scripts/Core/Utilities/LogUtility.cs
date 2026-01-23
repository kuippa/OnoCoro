using System;
using System.IO;
using UnityEngine;

/// <summary>
/// ログ出力を管理する汎用ユーティリティクラス
/// - エディタ環境では全レベルのログを出力
/// - ビルド環境ではWarning以上のみ出力
/// - Warning以上はファイルにも記録（ユーザーからのログ提出用）
/// </summary>
public static class LogUtility
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    // File and Message Constants
    private const string LOG_FILE_NAME = "game.log";
    private const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
    private const string MSG_CLEAR_LOG_FAILED = "Failed to clear log file: ";
    private const string MSG_WRITE_LOG_FAILED = "Failed to write log to file: ";
    private const string MSG_INIT_LOG_FAILED = "Failed to initialize log file: ";
    private const string LOG_HEADER_START = "=== Game Log Started ===";
    private const string LOG_HEADER_APP = "Application: ";
    private const string LOG_HEADER_VERSION = "Version: ";
    private const string LOG_HEADER_PLATFORM = "Platform: ";
    private const string LOG_HEADER_UNITY = "Unity Version: ";
    private const string LOG_HEADER_STARTED = "Started: ";
    private const string LOG_HEADER_SEPARATOR = "===========================";

    // エディタではすべて表示、ビルドでは警告以上のみ
#if UNITY_EDITOR
    private static readonly LogLevel MinLevel = LogLevel.Debug;
    private const bool EnableFileLogging = false; // エディタではファイル出力なし
#else
    private static readonly LogLevel MinLevel = LogLevel.Warning;
    private const bool EnableFileLogging = true; // ビルドではファイル出力あり
#endif

    private static string LogFilePath => Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);
    private static readonly object FileLock = new object();
    private static bool _isInitialized = false;

    /// <summary>
    /// デバッグレベルのログ出力（開発時の詳細確認用）
    /// </summary>
    public static void Debug(string message)
    {
        Log(LogLevel.Debug, message);
    }

    /// <summary>
    /// 情報レベルのログ出力（正常動作の記録用）
    /// </summary>
    public static void Info(string message)
    {
        Log(LogLevel.Info, message);
    }

    /// <summary>
    /// 警告レベルのログ出力（問題の可能性がある状況）
    /// </summary>
    public static void Warning(string message)
    {
        Log(LogLevel.Warning, message);
    }

    /// <summary>
    /// エラーレベルのログ出力（重大な問題）
    /// </summary>
    public static void Error(string message)
    {
        Log(LogLevel.Error, message);
    }

    /// <summary>
    /// ログファイルパスを取得（ユーザーがログを確認・提出する際に使用）
    /// </summary>
    public static string GetLogFilePath()
    {
        return LogFilePath;
    }

    /// <summary>
    /// ログファイルをクリア（新しいセッション開始時など）
    /// </summary>
    public static void ClearLogFile()
    {
        if (!EnableFileLogging) return;

        try
        {
            lock (FileLock)
            {
                if (File.Exists(LogFilePath))
                {
                    File.Delete(LogFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"{MSG_CLEAR_LOG_FAILED}{ex.Message}");
        }
    }

    private static void Log(LogLevel level, string message)
    {
        if (level < MinLevel) return;

        // Unity Console出力
        switch (level)
        {
            case LogLevel.Error:
                UnityEngine.Debug.LogError(message);
                break;
            case LogLevel.Warning:
                UnityEngine.Debug.LogWarning(message);
                break;
            default:
                UnityEngine.Debug.Log(message);
                break;
        }

        // ファイル出力（Warning以上のみ）
        if (EnableFileLogging && level >= LogLevel.Warning)
        {
            WriteToFile(level, message);
        }
    }

    private static void WriteToFile(LogLevel level, string message)
    {
        try
        {
            lock (FileLock)
            {
                // 初回のみヘッダーを書き込み
                if (!_isInitialized)
                {
                    InitializeLogFile();
                    _isInitialized = true;
                }

                string timestamp = DateTime.Now.ToString(DATETIME_FORMAT);
                string logEntry = $"[{level}] {timestamp} - {message}{Environment.NewLine}";

                File.AppendAllText(LogFilePath, logEntry);
            }
        }
        catch (Exception ex)
        {
            // ファイル書き込みエラーはUnityコンソールのみに出力
            UnityEngine.Debug.LogWarning($"{MSG_WRITE_LOG_FAILED}{ex.Message}");
        }
    }

    private static void InitializeLogFile()
    {
        try
        {
            string header = $"{LOG_HEADER_START}{Environment.NewLine}";
            header += $"{LOG_HEADER_APP}{Application.productName}{Environment.NewLine}";
            header += $"{LOG_HEADER_VERSION}{Application.version}{Environment.NewLine}";
            header += $"{LOG_HEADER_PLATFORM}{Application.platform}{Environment.NewLine}";
            header += $"{LOG_HEADER_UNITY}{Application.unityVersion}{Environment.NewLine}";
            header += $"{LOG_HEADER_STARTED}{DateTime.Now.ToString(DATETIME_FORMAT)}{Environment.NewLine}";
            header += $"{LOG_HEADER_SEPARATOR}{Environment.NewLine}{Environment.NewLine}";

            File.AppendAllText(LogFilePath, header);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"{MSG_INIT_LOG_FAILED}{ex.Message}");
        }
    }
}
