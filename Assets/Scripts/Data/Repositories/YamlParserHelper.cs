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
            Debug.LogWarning($"[YamlParserHelper.BuildDictionaryListFromYaml] '{sectionKey}' セクションが見つかりません");
            return resultList;
        }

        Debug.Log($"[YamlParserHelper.BuildDictionaryListFromYaml] '{sectionKey}' セクション: {sequenceNode.Children.Count} 要素を処理中");

        foreach (YamlNode node in sequenceNode)
        {
            YamlMappingNode mappingNode = node as YamlMappingNode;
            if (mappingNode == null)
            {
                Debug.LogWarning($"[YamlParserHelper.BuildDictionaryListFromYaml] 非MappingNode要素をスキップ: {node.GetType()}");
                continue;
            }

            Dictionary<string, string> rowData = new Dictionary<string, string>();
            foreach (KeyValuePair<YamlNode, YamlNode> entry in mappingNode.Children)
            {
                string keyValue = GetYamlScalarValue(entry.Key);
                string valueStr = GetYamlScalarValue(entry.Value);

                if (keyValue != null && valueStr != null)
                {
                    rowData.Add(keyValue, valueStr);
                }
                else
                {
                    Debug.LogWarning($"[YamlParserHelper.BuildDictionaryListFromYaml] スカラー値の取得失敗 - Key: {entry.Key.GetType()}, Value: {entry.Value.GetType()} (key='{keyValue}', value='{valueStr}')");
                }
            }
            resultList.Add(rowData);
        }

        Debug.Log($"[YamlParserHelper.BuildDictionaryListFromYaml] '{sectionKey}' セクション完了: {resultList.Count} 行をパース");
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

    /// <summary>
    /// YamlNode からスカラー値を安全に取得
    /// </summary>
    private static string GetYamlScalarValue(YamlNode node)
    {
        if (node == null)
        {
            return null;
        }

        YamlScalarNode scalarNode = node as YamlScalarNode;
        if (scalarNode != null)
        {
            return scalarNode.Value;
        }

        return null;
    }
}
