using System;
using System.Collections.Generic;
using System.IO;
using CommonsUtility;
using UnityEngine;
using UnityEngine.SceneManagement;
using YamlDotNet.RepresentationModel;

public class StagingYamlCtrl : MonoBehaviour
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
        Dictionary<string, string> goals_req = new Dictionary<string, string>();
        if (YSeqNode == null)
        {
            return;
        }

        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            foreach (var entry in yamlevent.Children)
            {
                goals_req.Add(
                    ((YamlScalarNode)entry.Key).Value
                    , ((YamlScalarNode)entry.Value).Value
                );
                Debug.Log(((YamlScalarNode)entry.Key).Value + " : " + ((YamlScalarNode)entry.Value).Value);
            }
        }
        if (goals_req.Count > 0)
        {
            StageGoalController._dict_req = goals_req;
            StageGoalController.StartCheckStageGoal(this);
        }
    }

    private void SetGameOversRequirements(YamlStream yaml)
    {
        YamlSequenceNode YSeqNode = GetYamlSequenceNode(yaml, "gameovers");
        Dictionary<string, string> gameover_req = new Dictionary<string, string>();
        if (YSeqNode == null)
        {
            return;
        }

        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            foreach (var entry in yamlevent.Children)
            {
                gameover_req.Add(
                    ((YamlScalarNode)entry.Key).Value
                    , ((YamlScalarNode)entry.Value).Value
                );
                Debug.Log(((YamlScalarNode)entry.Key).Value + " : " + ((YamlScalarNode)entry.Value).Value);
            }
        }
        if (gameover_req.Count > 0)
        {
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

        Dictionary<string, string> board_data = new Dictionary<string, string>();
        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            string board_code = "";
            foreach (var entry in yamlevent.Children)
            {
                if (((YamlScalarNode)entry.Key).Value == "code")
                {
                    board_code = ((YamlScalarNode)entry.Value).Value;
                }
                else
                {
                    board_data.Add(board_code, ((YamlScalarNode)entry.Value).Value);
                }
            }
        }
        if (board_data.Count > 0)
        {
            _eventLoader._board_data = new Dictionary<string, string>(board_data);
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
        foreach (YamlMappingNode yamlevent in YSeqNode)
        {
            float event_time = -1f;
            Dictionary<string, string> event_data = new Dictionary<string, string>();
            foreach (var entry in yamlevent.Children)
            {
                if (((YamlScalarNode)entry.Key).Value == "time")
                {
                    event_time = float.Parse(((YamlScalarNode)entry.Value).Value);
                }
                else
                {
                    event_data.Add(((YamlScalarNode)entry.Key).Value, ((YamlScalarNode)entry.Value).Value);
                }
            }
            if (!(event_time >= 0f))
            {
                continue;
            }
            List<Dictionary<string, string>> event_list;
            if (_eventLoader._timer_events.ContainsKey(event_time))
            {
                _eventLoader._timer_events.TryGetValue(event_time, out event_list);
                if (event_list == null)
                {
                    Debug.Log("event_list is null ");
                    event_list = new List<Dictionary<string, string>>();
                }
                event_list.Add(event_data);
                _eventLoader._timer_events[event_time] = event_list;
            }
            else
            {
                event_list = new List<Dictionary<string, string>>();
                event_list.Add(event_data);
                _eventLoader._timer_events.Add(event_time, event_list);
            }
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
