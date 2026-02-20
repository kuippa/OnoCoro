using System;
using System.Collections.Generic;
using System.IO;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.SceneManagement;
using YamlDotNet.RepresentationModel;

public class StageYamlRepository : MonoBehaviour
{
    // YamlDotNet for Unity
    // https://assetstore.unity.com/packages/tools/integration/yamldotnet-for-unity-36292

    // YAML セクションキーは YamlSectionType.cs の YamlSectionKeys を使用

    // pathmakers フィールドキー定数
    private const string _PATHMAKER_FIELD_NAME = "name";
    private const string _PATHMAKER_FIELD_POS = "pos";

    // gameovers / events のフィールドキーは YamlSectionType.cs の enum を使用
    // GameOverCommandFields.gameover_type / .threshold
    // TimedEventCommandFields.@event

    // イベントローダー
    private EventLoader _eventLoader = null;
    public static List<string> _ItemList = new List<string>();

    internal void LoadYamlData(string stageName)
    {
        StageGoalController.ResetStageState();

        YamlStream yaml = LoadStreamingAsset.LoadYamlFile(Path.GetFileName(stageName + LoadStreamingAsset.YAML_FILE_EXTENSION));
        if (yaml == null)
        {
            Debug.Log("yaml is null");
            return;
        }

        ActionStageNotice(yaml);
        SetTimerEventData(yaml);
        SetStageInitData(yaml);
        SetItemList(yaml);
        SetPathMakerList(yaml);
        SetGoalsRequirements(yaml);
        SetGameOversRequirements(yaml);
        SetBoardInitData(yaml);
    }

    /// <summary>
    /// YamlSequenceNode の各 MappingNode を Dictionary のリストに変換する共通ヘルパー
    /// </summary>
    private List<Dictionary<string, string>> BuildDictionaryListFromYaml(YamlStream yaml, string sectionKey)
    {
        YamlSequenceNode sequenceNode = GetYamlSequenceNode(yaml, sectionKey);
        List<Dictionary<string, string>> resultList = new List<Dictionary<string, string>>();

        if (sequenceNode == null)
        {
            return resultList;
        }

        foreach (YamlMappingNode mappingNode in sequenceNode)
        {
            Dictionary<string, string> rowData = new Dictionary<string, string>();
            foreach (KeyValuePair<YamlNode, YamlNode> entry in mappingNode.Children)
            {
                rowData.Add(
                    ((YamlScalarNode)entry.Key).Value,
                    ((YamlScalarNode)entry.Value).Value
                );
            }
            resultList.Add(rowData);
        }

        return resultList;
    }

    private void SetGoalsRequirements(YamlStream yaml)
    {
        List<Dictionary<string, string>> yamlDataList = BuildDictionaryListFromYaml(yaml, YamlSectionKeys.Goals);
        if (yamlDataList.Count == 0)
        {
            return;
        }

        YamlCommandManager.ParseGoalCommands(yamlDataList);

        Dictionary<string, string> goalsRequirement = new Dictionary<string, string>();
        foreach (Dictionary<string, string> goalData in yamlDataList)
        {
            foreach (KeyValuePair<string, string> entry in goalData)
            {
                goalsRequirement.Add(entry.Key, entry.Value);
            }
        }

        StageGoalController._dict_req = goalsRequirement;
        StageGoalController.StartCheckStageGoal(this);
    }

    private void SetGameOversRequirements(YamlStream yaml)
    {
        List<Dictionary<string, string>> yamlDataList = BuildDictionaryListFromYaml(yaml, YamlSectionKeys.GameOvers);
        if (yamlDataList.Count == 0)
        {
            return;
        }

        List<GameOverCommand> gameoverCommands = YamlCommandManager.ParseGameOverCommands(yamlDataList);
        if (gameoverCommands.Count == 0)
        {
            return;
        }

        Dictionary<string, string> gameoverRequirement = new Dictionary<string, string>();
        foreach (GameOverCommand command in gameoverCommands)
        {
            gameoverRequirement.Add(GameOverCommandFields.gameover_type.ToString(), command.Type.ToString());
            gameoverRequirement.Add(GameOverCommandFields.threshold.ToString(), command.Threshold.ToString());
        }

        StageGoalController._dict_fail = gameoverRequirement;
        StageGoalController.StartCheckStageFail(this);
    }

    internal static List<string> GetItemList()
    {
        return _ItemList;
    }

    private void SetItemList(YamlStream yaml)
    {
        List<Dictionary<string, string>> yamlDataList = BuildDictionaryListFromYaml(yaml, YamlSectionKeys.ItemLists);
        if (yamlDataList.Count == 0)
        {
            return;
        }

        foreach (Dictionary<string, string> rowData in yamlDataList)
        {
            foreach (KeyValuePair<string, string> entry in rowData)
            {
                string itemName = entry.Value;
                if (string.IsNullOrEmpty(itemName))
                {
                    continue;
                }
                if (_ItemList.Contains(itemName))
                {
                    continue;
                }
                if (!Enum.IsDefined(typeof(GameEnum.ModelsType), itemName))
                {
                    Debug.Log("itemname " + itemName + " は GameEnum.ModelsType に定義されていません");
                    continue;
                }
                _ItemList.Add(itemName);
            }
        }
    }

    private void SetPathMakerList(YamlStream yaml)
    {
        YamlSequenceNode sequenceNode = GetYamlSequenceNode(yaml, YamlSectionKeys.PathMakers);
        if (sequenceNode == null)
        {
            return;
        }

        PathMakerCtrl.ResetPathMakerDict();

        foreach (YamlMappingNode mappingNode in sequenceNode)
        {
            string markerName = "";
            Vector3 markerPosition = Vector3.zero;

            // ステップ 1: name と pos を抽出
            foreach (KeyValuePair<YamlNode, YamlNode> entry in mappingNode.Children)
            {
                string fieldKey = ((YamlScalarNode)entry.Key).Value;
                if (string.IsNullOrEmpty(fieldKey))
                {
                    continue;
                }

                if (fieldKey == _PATHMAKER_FIELD_NAME)
                {
                    markerName = ((YamlScalarNode)entry.Value).Value.Trim();
                }
                if (fieldKey == _PATHMAKER_FIELD_POS)
                {
                    markerPosition = Utility.ParseVector3WithAutoHeight(((YamlScalarNode)entry.Value).Value);
                }
            }

            // ステップ 2: 両方揃ったら Dictionary に追加
            if (string.IsNullOrEmpty(markerName) || markerPosition == Vector3.zero)
            {
                continue;
            }

            if (PathMakerCtrl._pathMakerDict.ContainsKey(markerName))
            {
                PathMakerCtrl._pathMakerDict[markerName] = markerPosition;
            }
            else
            {
                PathMakerCtrl._pathMakerDict.Add(markerName, markerPosition);
            }
        }

        PathMakerCtrl.CreateGameObjectByPathMakerDict();
    }

    private void ActionStageNotice(YamlStream yaml)
    {
        YamlMappingNode rootMapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        foreach (KeyValuePair<YamlNode, YamlNode> entry in rootMapping.Children)
        {
            if (!object.Equals((YamlScalarNode)entry.Key, new YamlScalarNode(YamlSectionKeys.StageNotice)))
            {
                continue;
            }

            GameObject.Find("UINotice").GetComponent<NoticeCtrl>().ShowNotice(((YamlScalarNode)entry.Value).Value);
            break;
        }
    }

    private void SetStageInitData(YamlStream yaml)
    {
        List<Dictionary<string, string>> yamlDataList = BuildDictionaryListFromYaml(yaml, YamlSectionKeys.Stages);
        if (yamlDataList.Count == 0)
        {
            return;
        }

        foreach (Dictionary<string, string> rowData in yamlDataList)
        {
            foreach (KeyValuePair<string, string> entry in rowData)
            {
                if (!TrySetScore(GlobalConst.SHORT_SCORE1_SCALE, entry.Key, entry.Value))
                {
                    TrySetScore(GlobalConst.SHORT_SCORE2_SCALE, entry.Key, entry.Value);
                }
            }
        }
    }

    private void SetBoardInitData(YamlStream yaml)
    {
        YamlSequenceNode sequenceNode = GetYamlSequenceNode(yaml, YamlSectionKeys.Boards);
        if (sequenceNode == null)
        {
            return;
        }

        Dictionary<string, string> boardData = new Dictionary<string, string>();
        Dictionary<string, (string text, Vector3 pos)> signboardData = new Dictionary<string, (string text, Vector3 pos)>();

        string codeField = BoardCommandFields.code.ToString();
        string textField = BoardCommandFields.text.ToString();
        string posField = BoardCommandFields.pos.ToString();

        foreach (YamlMappingNode mappingNode in sequenceNode)
        {
            string code = "";
            string text = "";
            string posString = "";

            foreach (KeyValuePair<YamlNode, YamlNode> entry in mappingNode.Children)
            {
                string fieldKey = ((YamlScalarNode)entry.Key).Value;
                string fieldValue = ((YamlScalarNode)entry.Value).Value;

                if (fieldKey == codeField)
                {
                    code = fieldValue;
                }
                else if (fieldKey == textField)
                {
                    text = fieldValue;
                }
                else if (fieldKey == posField)
                {
                    posString = fieldValue;
                }
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(text))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(posString))
            {
                signboardData[code] = (text, Utility.StringToVector3(posString));
            }
            else
            {
                boardData[code] = text;
            }
        }

        if (boardData.Count > 0)
        {
            _eventLoader._board_data = boardData;
        }

        if (signboardData.Count > 0)
        {
            _eventLoader._signboard_data = signboardData;
        }
    }

    private bool TrySetScore(string scoreType, string key, string value)
    {
        if (scoreType != key)
        {
            return false;
        }

        if (!int.TryParse(value, out int intValue))
        {
            return false;
        }

        ScoreCtrl.InitScore(intValue, key);
        return true;
    }

    private YamlSequenceNode GetYamlSequenceNode(YamlStream yaml, string sectionKey)
    {
        YamlMappingNode rootMapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        YamlScalarNode targetKey = new YamlScalarNode(sectionKey);

        if (!rootMapping.Children.ContainsKey(targetKey))
        {
            return null;
        }

        YamlNode sectionNode = rootMapping.Children[targetKey];
        if (!(sectionNode is YamlSequenceNode))
        {
            return null;
        }

        return (YamlSequenceNode)sectionNode;
    }

    private void SetTimerEventData(YamlStream yaml)
    {
        List<Dictionary<string, string>> yamlDataList = BuildDictionaryListFromYaml(yaml, YamlSectionKeys.Events);
        if (yamlDataList.Count == 0)
        {
            return;
        }

        Dictionary<float, List<EventCommand>> eventCommandsByTime = YamlCommandManager.ParseTimedEventCommands(yamlDataList);
        if (eventCommandsByTime.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<float, List<EventCommand>> timeEntry in eventCommandsByTime)
        {
            float eventTime = timeEntry.Key;
            List<Dictionary<string, string>> eventList = new List<Dictionary<string, string>>();

            foreach (EventCommand eventCommand in timeEntry.Value)
            {
                Dictionary<string, string> eventDict = new Dictionary<string, string>(eventCommand.Parameters);
                eventDict[TimedEventCommandFields.@event.ToString()] = eventCommand.Type.ToString();
                eventList.Add(eventDict);
            }

            _eventLoader._timer_events[eventTime] = eventList;
        }

        _eventLoader.SetEventToTimer();
    }

    void Awake()
    {
        _eventLoader = transform.parent.gameObject.AddComponent<EventLoader>();
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log("StagingYamlCtrl sceneName " + sceneName);
        LoadYamlData(sceneName);
    }
}
