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
    // Git hub YamlDotNet
    // https://github.com/aaubry/YamlDotNet

    // イベントローダー
    private EventLoader _eventLoader = null;
    // public List<ItemStruct> _yamlItemList = new List<ItemStruct>();
    public static List<string> _ItemList = new List<string>();

    internal void LoadYamlData(string stageName)
    {
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

    private void SetGoalsRequirements(YamlStream yaml)
    {
        YamlSequenceNode YSeqNode = GetYamlSequenceNode(yaml, "goals");
        if (YSeqNode == null)
        {
            return;
        }

        // Dictionary<string, string> のリストに変換
        var yamlDataList = new List<Dictionary<string, string>>();
        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            var goalData = new Dictionary<string, string>();
            foreach (var entry in yamlevent.Children)
            {
                goalData.Add(
                    ((YamlScalarNode)entry.Key).Value,
                    ((YamlScalarNode)entry.Value).Value
                );
            }
            yamlDataList.Add(goalData);
        }

        // YamlCommandManager 経由で GoalCommand に変換
        var goalCommands = YamlCommandManager.ParseGoalCommands(yamlDataList);
        
        if (goalCommands.Count > 0)
        {
            // GoalCommand を Dictionary に変換して StageGoalController に渡す
            var goals_req = new Dictionary<string, string>();
            foreach (var cmd in goalCommands)
            {
                goals_req.Add("goal_type", cmd.Type.ToString());
                goals_req.Add("threshold", cmd.Threshold.ToString());
                if (!string.IsNullOrEmpty(cmd.Description))
                {
                    goals_req.Add("description", cmd.Description);
                }
            }
            
            StageGoalController._dict_req = goals_req;
            StageGoalController.StartCheckStageGoal(this);
        }
    }

    private void SetGameOversRequirements(YamlStream yaml)
    {
        YamlSequenceNode YSeqNode = GetYamlSequenceNode(yaml, "gameovers");
        if (YSeqNode == null)
        {
            return;
        }

        // Dictionary<string, string> のリストに変換
        var yamlDataList = new List<Dictionary<string, string>>();
        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            var gameoverData = new Dictionary<string, string>();
            foreach (var entry in yamlevent.Children)
            {
                gameoverData.Add(
                    ((YamlScalarNode)entry.Key).Value,
                    ((YamlScalarNode)entry.Value).Value
                );
            }
            yamlDataList.Add(gameoverData);
        }

        // YamlCommandManager 経由で GameOverCommand に変換
        var gameoverCommands = YamlCommandManager.ParseGameOverCommands(yamlDataList);
        
        if (gameoverCommands.Count > 0)
        {
            // GameOverCommand を Dictionary に変換して StageGoalController に渡す
            var gameover_req = new Dictionary<string, string>();
            foreach (var cmd in gameoverCommands)
            {
                gameover_req.Add("gameover_type", cmd.Type.ToString());
                gameover_req.Add("threshold", cmd.Threshold.ToString());
            }
            
            StageGoalController._dict_fail = gameover_req;
            StageGoalController.StartCheckStageFail(this);
        }
    }

    internal static List<string> GetItemList()
    {
        return _ItemList;
    }

    private void SetItemList(YamlStream yaml)
    {
        YamlSequenceNode YSeqNode = GetYamlSequenceNode(yaml, "itemlists");
        if (YSeqNode == null)
        {
            return;
        }

        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            foreach (var entry in yamlevent.Children)
            {
                string itemname = ((YamlScalarNode)entry.Value).Value;
                if (string.IsNullOrEmpty(itemname))
                {
                    continue;
                }

                // _ItemListにすでに格納されているか確認
                if (_ItemList.Contains(itemname))
                {
                    continue;
                }

                // itemname が GameEnum.ModelsType に定義されているか確認
                if (!Enum.IsDefined(typeof(GameEnum.ModelsType), itemname))
                {
                    Debug.Log("itemname " + itemname + " は GameEnum.ModelsType に定義されていません");
                    continue;
                }
                _ItemList.Add(itemname);
            }
        }
    }

    private void SetPathMakerList(YamlStream yaml)
    {
        YamlSequenceNode YSeqNode = GetYamlSequenceNode(yaml, "pathmakers");
        if (YSeqNode == null)
        {
            return;
        }
        PathMakerCtrl.ResetPathMakerDict();
        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            string key = "";
            Vector3 value = Vector3.zero;
            foreach (var entry in yamlevent.Children)
            {
                string keyName = ((YamlScalarNode)entry.Key).Value;
                if (!string.IsNullOrEmpty(keyName))
                {
                    if (keyName == "name")
                    {
                        key = ((YamlScalarNode)entry.Value).Value;
                        key = key.Trim();
                    }
                    if (keyName == "pos")
                    {
                        value = Utility.StringToVector3(((YamlScalarNode)entry.Value).Value);
                    }
                    if (PathMakerCtrl._pathMakerDict.ContainsKey(key))
                    {
                        PathMakerCtrl._pathMakerDict[key] = value;
                    }
                    else
                    {
                        PathMakerCtrl._pathMakerDict.Add(key, value);
                    }
                }
            }
        }
        PathMakerCtrl.CreateGameObjectByPathMakerDict();
    }

    private void ActionStageNotice(YamlStream yaml)
    {
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        foreach (var entry in mapping.Children)
        {
            if (object.Equals((YamlScalarNode)entry.Key, new YamlScalarNode("stagenotice")))
            {
                GameObject.Find("UINotice").GetComponent<NoticeCtrl>().ShowNotice(((YamlScalarNode)entry.Value).Value);
                break;
            }
        }
    }

    private void SetStageInitData(YamlStream yaml)
    {
        YamlSequenceNode YSeqNode = GetYamlSequenceNode(yaml, "stages");
        if (YSeqNode == null)
        {
            return;
        }

        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            foreach (var entry in yamlevent.Children)
            {
                if (!TrySetScore(GlobalConst.SHORT_SCORE1_SCALE, ((YamlScalarNode)entry.Key).Value, ((YamlScalarNode)entry.Value).Value))
                {
                    TrySetScore(GlobalConst.SHORT_SCORE2_SCALE, ((YamlScalarNode)entry.Key).Value, ((YamlScalarNode)entry.Value).Value);
                }
            }
        }
    }


    private void SetBoardInitData(YamlStream yaml)
    {
        YamlSequenceNode YSeqNode = GetYamlSequenceNode(yaml, "boards");
        if (YSeqNode == null)
        {
            return;
        }

        // Dictionary<string, string> のリストに変換
        var yamlDataList = new List<Dictionary<string, string>>();
        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            var boardData = new Dictionary<string, string>();
            foreach (var entry in yamlevent.Children)
            {
                boardData.Add(
                    ((YamlScalarNode)entry.Key).Value,
                    ((YamlScalarNode)entry.Value).Value
                );
            }
            yamlDataList.Add(boardData);
        }

        // YamlCommandManager 経由で BoardCommand に変換
        var boardCommands = YamlCommandManager.ParseBoardCommands(yamlDataList);
        
        if (boardCommands.Count > 0)
        {
            // BoardCommand を Dictionary に変換して _eventLoader に渡す
            var board_data = new Dictionary<string, string>();
            foreach (var cmd in boardCommands)
            {
                board_data[cmd.ConfigType.ToString()] = cmd.Value;
            }
            
            _eventLoader._board_data = board_data;
        }
    }


    private bool TrySetScore(string scoretype, string key, string val)
    {
        if (scoretype == key && int.TryParse(val, out int intVal))
        {
            ScoreCtrl.InitScore(intVal, key);
            return true;
        }
        return false;
    }


    private YamlSequenceNode GetYamlSequenceNode(YamlStream yaml, string key)
    {
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        YamlScalarNode YSNode = new YamlScalarNode(key);
        if (!mapping.Children.ContainsKey(YSNode))
        {
            return null;
        }
        YamlNode YSeqNode = mapping.Children[YSNode];
        if (!(YSeqNode is YamlSequenceNode))
        {
            return null;
        }
        return (YamlSequenceNode)YSeqNode;
    }

    private void SetTimerEventData(YamlStream yaml)
    {
        YamlSequenceNode YSeqNode = GetYamlSequenceNode(yaml, "events");
        if (YSeqNode == null)
        {
            return;
        }

        // Dictionary<string, string> のリストに変換
        var yamlDataList = new List<Dictionary<string, string>>();
        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            var eventData = new Dictionary<string, string>();
            foreach (var entry in yamlevent.Children)
            {
                eventData.Add(
                    ((YamlScalarNode)entry.Key).Value,
                    ((YamlScalarNode)entry.Value).Value
                );
            }
            yamlDataList.Add(eventData);
        }

        // YamlCommandManager 経由で EventCommand に変換（時間キー付き Dictionary で返される）
        var eventCommandsByTime = YamlCommandManager.ParseTimedEventCommands(yamlDataList);
        
        if (eventCommandsByTime.Count > 0)
        {
            // EventCommand を Dictionary<float, List<Dictionary<string, string>>> に変換して _eventLoader に渡す
            foreach (var timeEntry in eventCommandsByTime)
            {
                float eventTime = timeEntry.Key;
                var eventList = new List<Dictionary<string, string>>();
                
                foreach (var eventCmd in timeEntry.Value)
                {
                    var eventDict = new Dictionary<string, string>(eventCmd.Parameters);
                    eventList.Add(eventDict);
                }
                
                _eventLoader._timer_events[eventTime] = eventList;
            }
            
            _eventLoader.SetEventToTimer();
        }
    }

    void Awake()
    {
        _eventLoader = transform.parent.gameObject.AddComponent<EventLoader>();
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log("StagingYamlCtrl sceneName " + sceneName);
        LoadYamlData(sceneName);
    }



}
