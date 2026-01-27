using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CommonsUtility;
using YamlDotNet.RepresentationModel;
using Debug = CommonsUtility.Debug;

/// <summary>
/// YAML ファイル実装テスト
/// Assets/Resources/staging 内の実際の YAML ファイルを読み込んで、
/// YamlCommandManager でパース・バリデーションし、エラー状況をテストする
/// 
/// 使用方法：
/// 1. このスクリプトを GameManager などにアタッチ
/// 2. Play ボタンで実行
/// 3. Console に各ファイルのバリデーション結果が表示される
/// 4. 使用後は Core/Editor/YamlFileValidationTest.cs に移動
/// </summary>
public class YamlFileValidationTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("YAML File Validation Test Started");
        Debug.Log("========================================");
        Debug.Log("");
        
        // staging フォルダ内のすべての YAML ファイルをテスト
        TestAllYamlFiles();
        
        Debug.Log("");
        Debug.Log("========================================");
        Debug.Log("YAML File Validation Test Completed");
        Debug.Log("========================================");
    }

    /// <summary>
    /// Assets/Resources/staging 内のすべての YAML ファイルをテスト
    /// </summary>
    private void TestAllYamlFiles()
    {
        string stagingPath = Path.Combine(Application.streamingAssetsPath, "staging");
        Debug.Log(stagingPath);

        if (!Directory.Exists(stagingPath))
        {
            Debug.LogError($"Staging folder not found: {stagingPath}");
            return;
        }
        
        string[] yamlFiles = Directory.GetFiles(stagingPath, "*.yaml");
        
        if (yamlFiles.Length == 0)
        {
            Debug.LogWarning("No YAML files found in staging folder");
            return;
        }
        
        Debug.Log($"Found {yamlFiles.Length} YAML files");
        Debug.Log("");
        
        int totalErrors = 0;
        int totalWarnings = 0;
        
        foreach (string filePath in yamlFiles)
        {
            string fileName = Path.GetFileName(filePath);
            Debug.Log($"[FILE] {fileName}");
            
            // YAML ファイルを読み込む
            YamlStream yaml = LoadYamlFile(filePath);
            if (yaml == null)
            {
                Debug.LogError($"  ✗ Failed to load YAML file");
                totalErrors++;
                Debug.Log("");
                continue;
            }
            
            // 各セクションをテスト
            int fileErrors = 0;
            int fileWarnings = 0;
            
            fileErrors += TestGoalsSection(yaml);
            fileWarnings += TestGameOversSection(yaml);
            fileErrors += TestEventsSection(yaml);
            fileWarnings += TestBoardsSection(yaml);
            
            if (fileErrors == 0 && fileWarnings == 0)
            {
                Debug.Log($"  ✓ All validations passed");
            }
            
            totalErrors += fileErrors;
            totalWarnings += fileWarnings;
            Debug.Log("");
        }
        
        Debug.Log($"========================================");
        Debug.Log($"Summary: {totalErrors} errors, {totalWarnings} warnings");
        Debug.Log($"========================================");
    }

    /// <summary>
    /// Goals セクションのテスト
    /// </summary>
    private int TestGoalsSection(YamlStream yaml)
    {
        YamlSequenceNode goalsNode = GetYamlSequenceNode(yaml, "goals");
        if (goalsNode == null)
        {
            return 0;
        }
        
        Debug.Log("  [goals]");
        int errorCount = 0;
        
        var yamlDataList = ConvertToYamlDataList(goalsNode);
        var goalCommands = YamlCommandManager.ParseGoalCommands(yamlDataList);
        
        for (int i = 0; i < goalCommands.Count; i++)
        {
            var validationResult = goalCommands[i].Validate();
            if (!validationResult.IsValid)
            {
                Debug.LogWarning($"    [goal #{i}] ✗ {string.Join(", ", validationResult.Errors)}");
                errorCount++;
            }
            else
            {
                Debug.Log($"    [goal #{i}] ✓ {goalCommands[i].Type} (threshold={goalCommands[i].Threshold})");
            }
        }
        
        return errorCount;
    }

    /// <summary>
    /// GameOvers セクションのテスト
    /// </summary>
    private int TestGameOversSection(YamlStream yaml)
    {
        YamlSequenceNode gameoversNode = GetYamlSequenceNode(yaml, "gameovers");
        if (gameoversNode == null)
        {
            return 0;
        }
        
        Debug.Log("  [gameovers]");
        int errorCount = 0;
        
        var yamlDataList = ConvertToYamlDataList(gameoversNode);
        var gameoverCommands = YamlCommandManager.ParseGameOverCommands(yamlDataList);
        
        for (int i = 0; i < gameoverCommands.Count; i++)
        {
            var validationResult = gameoverCommands[i].Validate();
            if (!validationResult.IsValid)
            {
                Debug.LogWarning($"    [gameover #{i}] ✗ {string.Join(", ", validationResult.Errors)}");
                errorCount++;
            }
            else
            {
                Debug.Log($"    [gameover #{i}] ✓ {gameoverCommands[i].Type} (threshold={gameoverCommands[i].Threshold})");
            }
        }
        
        return errorCount;
    }

    /// <summary>
    /// Events セクションのテスト
    /// </summary>
    private int TestEventsSection(YamlStream yaml)
    {
        YamlSequenceNode eventsNode = GetYamlSequenceNode(yaml, "events");
        if (eventsNode == null)
        {
            return 0;
        }
        
        Debug.Log("  [events]");
        int errorCount = 0;
        
        var yamlDataList = ConvertToYamlDataList(eventsNode);
        var timedEvents = YamlCommandManager.ParseTimedEventCommands(yamlDataList);
        
        int eventIndex = 0;
        foreach (var kvp in timedEvents)
        {
            float triggerTime = kvp.Key;
            List<EventCommand> commands = kvp.Value;
            
            foreach (var command in commands)
            {
                var validationResult = command.Validate();
                if (!validationResult.IsValid)
                {
                    Debug.LogWarning($"    [event #{eventIndex}] ✗ time={triggerTime}, {string.Join(", ", validationResult.Errors)}");
                    errorCount++;
                }
                else
                {
                    Debug.Log($"    [event #{eventIndex}] ✓ time={triggerTime}, type={command.Type}");
                }
                eventIndex++;
            }
        }
        
        return errorCount;
    }

    /// <summary>
    /// Boards セクションのテスト
    /// </summary>
    private int TestBoardsSection(YamlStream yaml)
    {
        YamlSequenceNode boardsNode = GetYamlSequenceNode(yaml, "boards");
        if (boardsNode == null)
        {
            return 0;
        }
        
        Debug.Log("  [boards]");
        int errorCount = 0;
        
        var yamlDataList = ConvertToYamlDataList(boardsNode);
        var boardCommands = YamlCommandManager.ParseBoardCommands(yamlDataList);
        
        for (int i = 0; i < boardCommands.Count; i++)
        {
            var validationResult = boardCommands[i].Validate();
            if (!validationResult.IsValid)
            {
                Debug.LogWarning($"    [board #{i}] ✗ {string.Join(", ", validationResult.Errors)}");
                errorCount++;
            }
            else
            {
                Debug.Log($"    [board #{i}] ✓ {boardCommands[i].ConfigType} = {boardCommands[i].Value}");
            }
        }
        
        return errorCount;
    }

    /// <summary>
    /// YamlSequenceNode を Dictionary<string, string> のリストに変換
    /// </summary>
    private List<Dictionary<string, string>> ConvertToYamlDataList(YamlSequenceNode sequenceNode)
    {
        var yamlDataList = new List<Dictionary<string, string>>();
        
        foreach (YamlMappingNode mappingNode in sequenceNode)
        {
            var yamlData = new Dictionary<string, string>();
            foreach (var entry in mappingNode.Children)
            {
                string key = ((YamlScalarNode)entry.Key).Value;
                string value = ((YamlScalarNode)entry.Value).Value;
                yamlData.Add(key, value);
            }
            yamlDataList.Add(yamlData);
        }
        
        return yamlDataList;
    }

    /// <summary>
    /// YAML ファイルを読み込む
    /// </summary>
    private YamlStream LoadYamlFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        try
        {
            StreamReader streamReader = new StreamReader(filePath);
            if (streamReader == null)
            {
                return null;
            }
            
            TextReader textReader = new StringReader(streamReader.ReadToEnd());
            if (textReader == null)
            {
                return null;
            }
            
            YamlStream yamlStream = new YamlStream();
            yamlStream.Load(textReader);
            
            if (yamlStream.Documents.Count == 0)
            {
                return null;
            }
            
            return yamlStream;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception loading YAML: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// YamlStream から指定したキーの YamlSequenceNode を取得
    /// </summary>
    private YamlSequenceNode GetYamlSequenceNode(YamlStream yaml, string key)
    {
        if (yaml == null || yaml.Documents.Count == 0)
        {
            return null;
        }
        
        YamlMappingNode rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
        if (rootNode == null)
        {
            return null;
        }
        
        foreach (var entry in rootNode.Children)
        {
            string nodeKey = ((YamlScalarNode)entry.Key).Value;
            if (nodeKey == key && entry.Value is YamlSequenceNode)
            {
                return (YamlSequenceNode)entry.Value;
            }
        }
        
        return null;
    }
}
