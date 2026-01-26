using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// LogUtility の機能テスト用スクリプト
/// テストシーンに attach して使用
/// </summary>
public class LogUtilityTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== LogUtility テスト開始 ===");
        
        // デバッグ情報: GameConfig の確認
        DebugGameConfig();
        
        // デバッグ情報: パス確認
        DebugLogFilePath();
        
        // テスト1: ログファイルパス取得
        TestGetLogFilePath();
        
        // テスト2: 各ログレベルでの出力
        TestWriteLogByLevel();
        
        // テスト3: ログクリア
        // TestClearLogFile();
        
        // テスト4: 複数回の出力
        TestMultipleWrites();
        
        Debug.Log("=== LogUtility テスト完了 ===");
    }

    /// <summary>
    /// デバッグ情報: GameConfig の値を確認
    /// </summary>
    private void DebugGameConfig()
    {
        Debug.Log("--- GameConfig デバッグ情報 ---");
        Debug.Log($"GameConfig.LogLevel: {GameConfig.LogLevel}");
        Debug.Log($"GameConfig.DebugLevel: {GameConfig.DebugLevel}");
        Debug.Log($"GameConfig.LogFileName: {GameConfig.LogFileName}");
        Debug.Log($"GameConfig.LogFilePath: {GameConfig.LogFilePath}");
    }

    /// <summary>
    /// デバッグ情報: persistentDataPath の確認
    /// </summary>
    private void DebugLogFilePath()
    {
        Debug.Log("--- ログファイルパス デバッグ情報 ---");
        Debug.Log($"Application.persistentDataPath: {Application.persistentDataPath}");
        
        // GetLogFileName() の実装と同じロジックで確認
        string datePrefix = System.DateTime.Now.ToString("yyyyMMdd");
        string fileName = $"{datePrefix}_{GameConfig.LogFileName}";
        string fullPath = System.IO.Path.Combine(GameConfig.LogFilePath, fileName);
        Debug.Log($"予想されるログファイルパス: {fullPath}");
        
        // ディレクトリの確認
        string directory = System.IO.Path.GetDirectoryName(fullPath);
        bool directoryExists = System.IO.Directory.Exists(directory);
        Debug.Log($"ディレクトリ存在確認: {directory} → {directoryExists}");
    }

    /// <summary>
    /// テスト1: ログファイルパス取得
    /// </summary>
    private void TestGetLogFilePath()
    {
        Debug.Log("--- テスト1: ログファイルパス取得 ---");
        
        string logFilePath = LogUtility.GetLogFilePath();
        Debug.Log($"ログファイルパス（GetLogFilePath）: {logFilePath}");
        
        // ログを書き込み（パスの確認用）
        Debug.Log("テスト1: LogUtility.WriteLog() を実行");
        LogUtility.WriteLog(LogUtility.LogLevel.Log, "テスト1: パス確認ログ");
        Debug.Log("テスト1 完了");
    }

    /// <summary>
    /// テスト2: 各ログレベルでの出力
    /// </summary>
    private void TestWriteLogByLevel()
    {
        Debug.Log("--- テスト2: 各ログレベルでの出力 ---");
        
        // DebugLevel がどの値でも出力されるように、LogLevel.Log で書き込み
        LogUtility.WriteLog(LogUtility.LogLevel.Log, "ログレベル=Log のテスト出力");
        LogUtility.WriteLog(LogUtility.LogLevel.Warning, "ログレベル=Warning のテスト出力");
        LogUtility.WriteLog(LogUtility.LogLevel.Error, "ログレベル=Error のテスト出力");
        LogUtility.WriteLog(LogUtility.LogLevel.Editor, "ログレベル=Editor のテスト出力（エディタのみ）");
        
        Debug.Log("テスト2 完了: ログファイルに記録されました");
    }

    /// <summary>
    /// テスト3: ログクリア機能
    /// </summary>
    private void TestClearLogFile()
    {
        Debug.Log("--- テスト3: ログクリア前に一度ログを書き込み ---");
        
        LogUtility.WriteLog(LogUtility.LogLevel.Log, "クリア前のテストログ");
        
        Debug.Log("--- テスト3: ログファイルをクリア ---");
        LogUtility.ClearLogFile();
        
        Debug.Log("クリア完了。新しいセッションが開始されます");
    }

    /// <summary>
    /// テスト4: 複数回の出力テスト
    /// </summary>
    private void TestMultipleWrites()
    {
        Debug.Log("--- テスト4: 複数回の出力 ---");
        
        for (int i = 0; i < 5; i++)
        {
            LogUtility.WriteLog(LogUtility.LogLevel.Log, $"ループテスト {i + 1} 回目");
        }
        
        Debug.Log("テスト4 完了: 5つのログエントリが追加されました");
    }

    /// <summary>
    /// OnApplicationQuit で ログファイルを確実にクローズ
    /// </summary>
    private void OnApplicationQuit()
    {
        Debug.Log("=== アプリケーション終了: ログファイルをクローズ ===");
        LogUtility.CloseLogFile();
    }
}
