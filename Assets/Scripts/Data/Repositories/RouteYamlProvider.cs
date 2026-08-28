using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

/// <summary>
/// パスマーカー・ルート定義をパースするProvider
/// pathmakers セクション: マーカー位置定義
/// routenames セクション: パス名とマーカーシーケンスの関連づけ
/// </summary>
internal static class RouteYamlProvider
{
    private const string _PATHMAKER_FIELD_NAME = "name";
    private const string _PATHMAKER_FIELD_POS = "pos";

    /// <summary>
    /// pathmakers セクションをパースして PathMakerCtrl に登録
    /// </summary>
    internal static void LoadPathMakers(YamlStream yaml)
    {
        if (yaml == null)
        {
            Debug.LogWarning("[RouteYamlProvider.LoadPathMakers] yaml is null");
            return;
        }

        YamlSequenceNode sequenceNode = YamlParserHelper.GetYamlSequenceNode(yaml, YamlSectionKeys.PathMakers);
        if (sequenceNode == null)
        {
            return;
        }

        PathMakerCtrl.ResetPathMakerDict();

        foreach (YamlMappingNode mappingNode in sequenceNode)
        {
            string markerName = ExtractFieldValue(mappingNode, _PATHMAKER_FIELD_NAME);
            Vector3 markerPosition = Vector3.zero;

            string posString = ExtractFieldValue(mappingNode, _PATHMAKER_FIELD_POS);
            if (!string.IsNullOrEmpty(posString))
            {
                markerPosition = Utility.ParseVector3WithAutoHeight(posString);
            }

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

    /// <summary>
    /// routenames セクションをパースして EventLoader に登録
    /// 前方互換として bloom_path / spawn_enemy_unit で ルート名を指定できるようにする
    /// </summary>
    internal static void LoadRouteNames(YamlStream yaml, EventLoader eventLoader)
    {
        if (yaml == null)
        {
            Debug.LogWarning("[RouteYamlProvider.LoadRouteNames] yaml is null");
            return;
        }

        if (eventLoader == null)
        {
            Debug.LogWarning("[RouteYamlProvider.LoadRouteNames] eventLoader is null");
            return;
        }

        YamlSequenceNode sequenceNode = YamlParserHelper.GetYamlSequenceNode(yaml, YamlSectionKeys.RouteNames);
        if (sequenceNode == null)
        {
            return;
        }

        foreach (YamlMappingNode mappingNode in sequenceNode)
        {
            string routeName = ExtractFieldValue(mappingNode, "name");
            string markerSequence = ExtractFieldValue(mappingNode, "markers");

            if (string.IsNullOrEmpty(routeName) || string.IsNullOrEmpty(markerSequence))
            {
                continue;
            }

            // ルート名 → マーカーシーケンス（イベント処理用）
            if (eventLoader._routeNameDict.ContainsKey(routeName))
            {
                eventLoader._routeNameDict[routeName] = markerSequence;
            }
            else
            {
                eventLoader._routeNameDict.Add(routeName, markerSequence);
            }

            // ルート名の逆引きテーブル（off_bloom_path_complete 用）
            // routeName でも呼べるように実マーカーシーケンスにマッピング
            if (!eventLoader._routeNameToMarkerSequenceDict.ContainsKey(routeName))
            {
                eventLoader._routeNameToMarkerSequenceDict.Add(routeName, markerSequence);
            }

            Debug.LogTrace($"[RouteYamlProvider.LoadRouteNames] Registered route '{routeName}' → '{markerSequence}'");
        }
    }

    private static string ExtractFieldValue(YamlMappingNode mappingNode, string fieldName)
    {
        if (mappingNode == null || string.IsNullOrEmpty(fieldName))
        {
            return "";
        }

        foreach (KeyValuePair<YamlNode, YamlNode> entry in mappingNode.Children)
        {
            string key = ((YamlScalarNode)entry.Key).Value;
            if (key == fieldName)
            {
                return ((YamlScalarNode)entry.Value).Value.Trim();
            }
        }

        return "";
    }
}
