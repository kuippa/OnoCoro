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

        foreach (YamlNode node in sequenceNode)
        {
            YamlMappingNode mappingNode = node as YamlMappingNode;
            if (mappingNode == null)
            {
                Debug.LogWarning($"[YamlParserHelper.BuildDictionaryListFromYaml] 非MappingNode要素をスキップ: {node.GetType()}");
                continue;
            }

            Dictionary<string, string> rowData = new Dictionary<string, string>();
            bool isValidRow = true;
            
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
                    isValidRow = false;
                    break;
                }
            }
            
            // スカラー値取得に失敗した行はスキップ
            if (isValidRow)
            {
                resultList.Add(rowData);
            }
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

        // キーを線形探索で値を比較（ContainsKey は YamlDotNet の ノード参照比較のため失敗する可能性がある）
        foreach (var kvp in rootMapping.Children)
        {
            YamlScalarNode keyNode = kvp.Key as YamlScalarNode;
            if (keyNode != null && keyNode.Value == sectionKey)
            {
                YamlSequenceNode sequenceNode = kvp.Value as YamlSequenceNode;
                if (sequenceNode != null)
                {
                    return sequenceNode;
                }
            }
        }

        return null;
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
