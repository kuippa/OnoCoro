using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = CommonsUtility.Debug;

/// <summary>
/// ステージデータの管理を行うクラス
/// - シーン名の取得
/// - CSVからのステージ情報読み込み
/// - ステージ辞書の構築
/// - シーンパスの検証
/// </summary>
public static class StageRepository
{
    // UI Messages
    private const string MSG_INVALID_LINE_FORMAT = "Invalid line format: ";
    
    /// <summary>
    /// ビルド設定に登録されているシーン名を取得
    /// </summary>
    /// <returns>シーン名の配列</returns>
    public static string[] GetSceneNames()
    {
        int sceneCountInBuildSettings = SceneManager.sceneCountInBuildSettings;
        string[] sceneNames = new string[sceneCountInBuildSettings];
        
        for (int i = 0; i < sceneCountInBuildSettings; i++)
        {
            // プロジェクト、ビルドプロファイルのシーンリストからの取得
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            sceneNames[i] = fileNameWithoutExtension;
        }
        
        return sceneNames;
    }

    /// <summary>
    /// CSVファイルからステージ情報を読み込んでリストを構築（CSV順序を保持）
    /// </summary>
    /// <returns>シーン名とステージ情報のタプルリスト（CSV読み込み順）</returns>
    internal static List<(string SceneName, string[] StageInfo)> GetSceneInfoList()
    {
        var list = new List<(string, string[])>();
        string[] csvLines = LoadStreamingAsset.CsvLines(LoadStreamingAsset.STAGE_LIST_FILE_NAME);
        
        if (csvLines == null)
        {
            Debug.LogError($"ステージリストファイルが見つかりません: {LoadStreamingAsset.STAGE_LIST_FILE_NAME}");
            return list;
        }
        
        var seenSceneNames = new HashSet<string>();
        
        for (int i = 0; i < csvLines.Length; i++)
        {
            string[] csvColumns = LoadStreamingAsset.CsvCols(csvLines[i]);
            if (csvColumns.Length != 4)
            {
                Debug.LogWarning(MSG_INVALID_LINE_FORMAT + csvLines[i]);
                continue;
            }
            
            // 重複キーをスキップ
            if (seenSceneNames.Contains(csvColumns[0]))
            {
                Debug.LogWarning($"重複するシーン名をスキップ: {csvColumns[0]}");
                continue;
            }
            
            seenSceneNames.Add(csvColumns[0]);
            list.Add((csvColumns[0], new string[3]
            {
                csvColumns[1],
                csvColumns[2],
                csvColumns[3]
            }));
        }
        
        return list;
    }

    /// <summary>
    /// 指定されたシーン名がビルド設定に存在するか検証
    /// </summary>
    /// <param name="sceneName">検証するシーン名</param>
    /// <returns>存在する場合true</returns>
    public static bool IsScenePathValid(string sceneName)
    {
        string[] sceneNames = GetSceneNames();
        
        for (int i = 0; i < sceneNames.Length; i++)
        {
            if (sceneName == sceneNames[i])
            {
                return true;
            }
        }
        
        return false;
    }
}
