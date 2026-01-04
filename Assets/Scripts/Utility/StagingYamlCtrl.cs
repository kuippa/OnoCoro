using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityYamlData;
// using YamlDotNet.Serialization;
using YamlDotNet.RepresentationModel;
using System.Net.Http.Headers;
using UnityEngine.AI;
using Unity.VisualScripting;
using CommonsUtility;

public class StagingYamlCtrl : MonoBehaviour
{
    // YamlDotNet for Unity
    // https://assetstore.unity.com/packages/tools/integration/yamldotnet-for-unity-36292
    // Git hub YamlDotNet
    // https://github.com/aaubry/YamlDotNet


    // const string ex_yamlPath = "Assets/Resources/staging/ex.yaml";
    // const string yamlPath = "staging/ex.yaml";
    const string _YAML_FOLDER_PATH = "Assets/Resources/staging/";

    // イベントローダー
    private EventLoader _eventLoader = null;
    // public List<ItemStruct> _yamlItemList = new List<ItemStruct>();
    public static List<string> _ItemList = new List<string>();

    internal void LoadYamlData(string stageName)
    {
        var input = new StreamReader(_YAML_FOLDER_PATH + stageName + ".yaml");
        TextReader textReader = new StringReader(input.ReadToEnd());
        if (textReader == null)
        {
            return;
        }

        var yaml = new YamlStream();
        yaml.Load(textReader);
        if (yaml.Documents.Count == 0)
        {
            return;
        }

        ActionStageNotice(yaml);
        SetTimerEventData(yaml);
        SetStageInitData(yaml);
        SetItemList(yaml);
        SetGoalsRequirements(yaml);
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
                // string req = ((YamlScalarNode)entry.Value).Value;
                // if (string.IsNullOrEmpty(req))
                // {
                //     continue;
                // }
                goals_req.Add(
                    ((YamlScalarNode)entry.Key).Value
                    , ((YamlScalarNode)entry.Value).Value
                );
                // StageGoalCtrl._GoalsRequirementsList.Add(req);
                Debug.Log(((YamlScalarNode)entry.Key).Value + " : " + ((YamlScalarNode)entry.Value).Value);
            }
        }
        if (goals_req.Count > 0)
        {
            StageGoalCtrl._dict_req = goals_req;
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
                // Debug.Log(((YamlScalarNode)entry.Value).Value);
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

    private void ActionStageNotice(YamlStream yaml)
    {
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        foreach (var entry in mapping.Children)
        {
            if (YamlScalarNode.Equals((YamlScalarNode)entry.Key, new YamlScalarNode("stagenotice")))
            {
                GameObject UINotice = GameObject.Find("UINotice");
                UINotice.GetComponent<NoticeCtrl>().ShowNotice(((YamlScalarNode)entry.Value).Value);
                return;
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
            // Dictionary<string, string> staging_data = new Dictionary<string, string>();
            foreach (var entry in yamlevent.Children)
            {
                // Debug.Log(((YamlScalarNode)entry.Key).Value + " : " + ((YamlScalarNode)entry.Value).Value);
                if (TrySetScore(GlobalConst.SHORT_SCORE1_SCALE, ((YamlScalarNode)entry.Key).Value, ((YamlScalarNode)entry.Value).Value))
                {
                    continue;
                }
                if (TrySetScore(GlobalConst.SHORT_SCORE2_SCALE, ((YamlScalarNode)entry.Key).Value, ((YamlScalarNode)entry.Value).Value))
                {
                    continue;
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
            String board_code = "";
            // float event_time = -1f;
            foreach (var entry in yamlevent.Children)
            {
                if (((YamlScalarNode)entry.Key).Value == "code")
                {
                    board_code = ((YamlScalarNode)entry.Value).Value;
                    continue;
                }
                board_data.Add(board_code, ((YamlScalarNode)entry.Value).Value);
            }

        }
        if (board_data.Count > 0)
        {
            // foreach (var item in board_data)
            // {
            //     Debug.Log($"{item.Key}={item.Value}");
            // }
            // Debug.Log("board_data.Count " + board_data);
            // Debug.Log("_eventLoader.Count " + _eventLoader );
            // Debug.Log("_eventLoader._board_data.Count " + _eventLoader._board_data.Count );

            _eventLoader._board_data = new Dictionary<string, string>(board_data);
        }
        _eventLoader.InitBoardData();
    }


    private bool TrySetScore(string scoretype, string key, string val)
    {
        if (scoretype == key)
        {
            if (int.TryParse(val, out int intVal))
            {
                ScoreCtrl.InitScore(intVal, key);
            }
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
                    continue;
                }
                event_data.Add(((YamlScalarNode)entry.Key).Value, ((YamlScalarNode)entry.Value).Value);
            }
            if (event_time >= 0f)
            {
                // List<Dictionary<string, string>> event_list = new List<Dictionary<string, string>>();
                List<Dictionary<string, string>> event_list;
                if (_eventLoader._timer_events.ContainsKey(event_time))
                {
                    _eventLoader._timer_events.TryGetValue(event_time, out event_list);
                    if (event_list == null)
                    {
                        Debug.Log("event_list is null ");
                        event_list = new List<Dictionary<string, string>>();
                    }
                    // Debug.Log("event_list " + event_list);

                    event_list.Add(event_data);
                    // _timer_events の event_time を event_list で 上書き
                    _eventLoader._timer_events[event_time] = event_list;
                }
                else
                {
                    event_list = new List<Dictionary<string, string>>();
                    event_list.Add(event_data);
                    _eventLoader._timer_events.Add(event_time, event_list);
                    // Debug.Log("event_list new add : " + event_list);

                }
            }
        }
        _eventLoader.SetEventToTimer();
    }

    void Awake()
    {
        // クラス名を表示する
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        // this.gameObject.AddComponent<EventLoader>();
        _eventLoader = this.transform.parent.gameObject.AddComponent<EventLoader>();
        // _eventLoader = this.gameObject.GetComponent<EventLoader>();
        // _eventLoader = this.transform.parent.gameObject.GetComponent<EventLoader>();
        // _eventLoader._events = new Dictionary<string, object[]>();

        // 現在のシーン名を取得
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log("StagingYamlCtrl sceneName "+sceneName);
        LoadYamlData(sceneName);    // シーン名と同名のYamlデータを取得読み込み
    }



}
