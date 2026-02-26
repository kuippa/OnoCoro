using System;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

/// <summary>
/// YAML パース共通ユーティリティクラス
/// YamlDotNet 操作の共通処理を一元管理
/// </summary>
internal static class YamlParserHelper
{
    /// <summary>
    /// YamlSequenceNode の各 MappingNode を Dictionary のリストに変換
    /// </summary>
    internal static List<Dictionary<string, string>> BuildDictionaryListFromYaml(YamlStream yaml, string sectionKey)
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

    /// <summary>
    /// YAML セクションから YamlSequenceNode を取得
    /// </summary>
    internal static YamlSequenceNode GetYamlSequenceNode(YamlStream yaml, string sectionKey)
    {
        if (yaml == null)
        {
            Debug.LogWarning("[YamlParserHelper.GetYamlSequenceNode] yaml is null");
            return null;
        }

        YamlMappingNode rootMapping = yaml.Documents[0].RootNode as YamlMappingNode;
        if (rootMapping == null)
        {
            Debug.LogWarning("[YamlParserHelper.GetYamlSequenceNode] rootMapping is not a YamlMappingNode");
            return null;
        }

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

    /// <summary>
    /// YAML ルートマッピングを取得
    /// </summary>
    internal static YamlMappingNode GetRootMapping(YamlStream yaml)
    {
        if (yaml == null)
        {
            return null;
        }

        return yaml.Documents[0].RootNode as YamlMappingNode;
    }
}
