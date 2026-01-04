// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// StagingYamlCtrl
using System;
using System.Collections.Generic;
using System.IO;
using CommonsUtility;
using UnityEngine;
using UnityEngine.SceneManagement;
using YamlDotNet.RepresentationModel;

public class StagingYamlCtrl : MonoBehaviour
{
	private const string _YAML_FOLDER_PATH = "Assets/StreamingAssets/staging/";

	private EventLoader _eventLoader;

	public static List<string> _ItemList = new List<string>();

	internal void LoadYamlData(string stageName)
	{
		YamlStream yamlStream = LoadStreamingAsset.LoadYamlFile(Path.GetFileName(stageName + ".yaml"));
		if (yamlStream == null)
		{
			Debug.Log("yaml is null");
			return;
		}
		ActionStageNotice(yamlStream);
		SetTimerEventData(yamlStream);
		SetStageInitData(yamlStream);
		SetItemList(yamlStream);
		SetPathMakerList(yamlStream);
		SetGoalsRequirements(yamlStream);
		SetGameOversRequirements(yamlStream);
		SetBoardInitData(yamlStream);
	}

	private void SetGoalsRequirements(YamlStream yaml)
	{
		YamlSequenceNode yamlSequenceNode = GetYamlSequenceNode(yaml, "goals");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (yamlSequenceNode == null)
		{
			return;
		}
		foreach (YamlMappingNode item in yamlSequenceNode)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> child in item.Children)
			{
				dictionary.Add(((YamlScalarNode)child.Key).Value, ((YamlScalarNode)child.Value).Value);
				Debug.Log(((YamlScalarNode)child.Key).Value + " : " + ((YamlScalarNode)child.Value).Value);
			}
		}
		if (dictionary.Count > 0)
		{
			StageGoalCtrl._dict_req = dictionary;
			StageGoalCtrl.StartCheckStageGoal(this);
		}
	}

	private void SetGameOversRequirements(YamlStream yaml)
	{
		YamlSequenceNode yamlSequenceNode = GetYamlSequenceNode(yaml, "gameovers");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (yamlSequenceNode == null)
		{
			return;
		}
		foreach (YamlMappingNode item in yamlSequenceNode)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> child in item.Children)
			{
				dictionary.Add(((YamlScalarNode)child.Key).Value, ((YamlScalarNode)child.Value).Value);
				Debug.Log(((YamlScalarNode)child.Key).Value + " : " + ((YamlScalarNode)child.Value).Value);
			}
		}
		if (dictionary.Count > 0)
		{
			StageGoalCtrl._dict_fail = dictionary;
			StageGoalCtrl.StartCheckStageFail(this);
		}
	}

	internal static List<string> GetItemList()
	{
		return _ItemList;
	}

	private void SetPathMakerList(YamlStream yaml)
	{
		YamlSequenceNode yamlSequenceNode = GetYamlSequenceNode(yaml, "pathmakers");
		if (yamlSequenceNode == null)
		{
			return;
		}
		PathMakerCtrl.ResetPathMakerDict();
		foreach (YamlMappingNode item in yamlSequenceNode)
		{
			string key = "";
			Vector3 value = Vector3.zero;
			foreach (KeyValuePair<YamlNode, YamlNode> child in item.Children)
			{
				string value2 = ((YamlScalarNode)child.Key).Value;
				if (!string.IsNullOrEmpty(value2))
				{
					if (value2 == "name")
					{
						key = ((YamlScalarNode)child.Value).Value;
						key = key.Trim();
					}
					if (value2 == "pos")
					{
						value = Utility.StringToVector3(((YamlScalarNode)child.Value).Value);
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

	private void SetItemList(YamlStream yaml)
	{
		YamlSequenceNode yamlSequenceNode = GetYamlSequenceNode(yaml, "itemlists");
		if (yamlSequenceNode == null)
		{
			return;
		}
		foreach (YamlMappingNode item in yamlSequenceNode)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> child in item.Children)
			{
				string value = ((YamlScalarNode)child.Value).Value;
				if (!string.IsNullOrEmpty(value) && !_ItemList.Contains(value))
				{
					if (!Enum.IsDefined(typeof(GameEnum.ModelsType), value))
					{
						Debug.Log("itemname " + value + " は GameEnum.ModelsType に定義されていません");
					}
					else
					{
						_ItemList.Add(value);
					}
				}
			}
		}
	}

	private void ActionStageNotice(YamlStream yaml)
	{
		foreach (KeyValuePair<YamlNode, YamlNode> child in ((YamlMappingNode)yaml.Documents[0].RootNode).Children)
		{
			if (object.Equals((YamlScalarNode)child.Key, new YamlScalarNode("stagenotice")))
			{
				GameObject.Find("UINotice").GetComponent<NoticeCtrl>().ShowNotice(((YamlScalarNode)child.Value).Value);
				break;
			}
		}
	}

	private void SetStageInitData(YamlStream yaml)
	{
		YamlSequenceNode yamlSequenceNode = GetYamlSequenceNode(yaml, "stages");
		if (yamlSequenceNode == null)
		{
			return;
		}
		foreach (YamlMappingNode item in yamlSequenceNode)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> child in item.Children)
			{
				if (!TrySetScore("BIT", ((YamlScalarNode)child.Key).Value, ((YamlScalarNode)child.Value).Value))
				{
					TrySetScore("CLK", ((YamlScalarNode)child.Key).Value, ((YamlScalarNode)child.Value).Value);
				}
			}
		}
	}

	private void SetBoardInitData(YamlStream yaml)
	{
		YamlSequenceNode yamlSequenceNode = GetYamlSequenceNode(yaml, "boards");
		if (yamlSequenceNode == null)
		{
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (YamlMappingNode item in yamlSequenceNode)
		{
			string key = "";
			foreach (KeyValuePair<YamlNode, YamlNode> child in item.Children)
			{
				if (((YamlScalarNode)child.Key).Value == "code")
				{
					key = ((YamlScalarNode)child.Value).Value;
				}
				else
				{
					dictionary.Add(key, ((YamlScalarNode)child.Value).Value);
				}
			}
		}
		if (dictionary.Count > 0)
		{
			_eventLoader._board_data = new Dictionary<string, string>(dictionary);
		}
	}

	private bool TrySetScore(string scoretype, string key, string val)
	{
		if (scoretype == key && int.TryParse(val, out var result))
		{
			ScoreCtrl.InitScore(result, key);
		}
		return false;
	}

	private YamlSequenceNode GetYamlSequenceNode(YamlStream yaml, string key)
	{
		YamlMappingNode yamlMappingNode = (YamlMappingNode)yaml.Documents[0].RootNode;
		YamlScalarNode key2 = new YamlScalarNode(key);
		if (!yamlMappingNode.Children.ContainsKey(key2))
		{
			return null;
		}
		YamlNode yamlNode = yamlMappingNode.Children[key2];
		if (!(yamlNode is YamlSequenceNode))
		{
			return null;
		}
		return (YamlSequenceNode)yamlNode;
	}

	private void SetTimerEventData(YamlStream yaml)
	{
		YamlSequenceNode yamlSequenceNode = GetYamlSequenceNode(yaml, "events");
		if (yamlSequenceNode == null)
		{
			return;
		}
		foreach (YamlMappingNode item in yamlSequenceNode)
		{
			float num = -1f;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<YamlNode, YamlNode> child in item.Children)
			{
				if (((YamlScalarNode)child.Key).Value == "time")
				{
					num = float.Parse(((YamlScalarNode)child.Value).Value);
				}
				else
				{
					dictionary.Add(((YamlScalarNode)child.Key).Value, ((YamlScalarNode)child.Value).Value);
				}
			}
			if (!(num >= 0f))
			{
				continue;
			}
			List<Dictionary<string, string>> value;
			if (_eventLoader._timer_events.ContainsKey(num))
			{
				_eventLoader._timer_events.TryGetValue(num, out value);
				if (value == null)
				{
					Debug.Log("event_list is null ");
					value = new List<Dictionary<string, string>>();
				}
				value.Add(dictionary);
				_eventLoader._timer_events[num] = value;
			}
			else
			{
				value = new List<Dictionary<string, string>>();
				value.Add(dictionary);
				_eventLoader._timer_events.Add(num, value);
			}
		}
		_eventLoader.SetEventToTimer();
	}

	private void Awake()
	{
		_eventLoader = base.transform.parent.gameObject.AddComponent<EventLoader>();
		string text = SceneManager.GetActiveScene().name;
		Debug.Log("StagingYamlCtrl sceneName " + text);
		LoadYamlData(text);
	}
}
