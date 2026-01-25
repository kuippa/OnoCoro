using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    /// CSVファイルからステージ情報を読み込んで辞書を構築
    /// </summary>
    /// <returns>シーン名をキーとしたステージ情報の辞書</returns>
    public static Dictionary<string, string[]> GetSceneInfoDict()
    {
        Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
        string[] csvLines = LoadStreamingAsset.CsvLines(LoadStreamingAsset.STAGE_LIST_FILE_NAME);
        
        if (csvLines == null)
        {
            LogUtility.Error($"ステージリストファイルが見つかりません: {LoadStreamingAsset.STAGE_LIST_FILE_NAME}");
            return dictionary;
        }
        
        for (int i = 0; i < csvLines.Length; i++)
        {
            string[] csvColumns = LoadStreamingAsset.CsvCols(csvLines[i]);
            if (csvColumns.Length != 4)
            {
                LogUtility.Warning(MSG_INVALID_LINE_FORMAT + csvLines[i]);
                continue;
            }
            
            // 重複キーをスキップ
            if (dictionary.ContainsKey(csvColumns[0]))
            {
                LogUtility.Warning($"重複するシーン名をスキップ: {csvColumns[0]}");
                continue;
            }
            
            dictionary.Add(csvColumns[0], new string[3]
            {
                csvColumns[1],
                csvColumns[2],
                csvColumns[3]
            });
        }
        
        return dictionary;
    }

    /// <summary>
    /// ビルド設定のシーンとCSVのステージ情報をマージした辞書を構築
    /// </summary>
    /// <returns>表示用のステージ辞書</returns>
    public static Dictionary<string, string[]> GetSceneDict()
    {
        string[] sceneNames = GetSceneNames();
        string currentSceneName = SceneManager.GetActiveScene().name;
        Dictionary<string, string[]> sceneInfoDict = GetSceneInfoDict();
        Dictionary<string, string[]> result = new Dictionary<string, string[]>();
        string[] defaultStageInfo = new string[3] { "StageName", "StageIcon", "StageInfo" };
        
        foreach (string sceneName in sceneNames)
        {
            if (sceneInfoDict.ContainsKey(sceneName))
            {
                // 重複キーをスキップ
                if (!result.ContainsKey(sceneName))
                {
                    result.Add(sceneName, sceneInfoDict[sceneName]);
                }
            }
            else if (sceneName != currentSceneName && !result.ContainsKey(sceneName))
            {
                string[] fallbackInfo = (string[])defaultStageInfo.Clone();
                fallbackInfo[0] = sceneName;
                // result.Add(sceneName, fallbackInfo);   // シーンマネージャーに登録されているものをすべて表示する場合は有効化
            }
        }

        return result;
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
