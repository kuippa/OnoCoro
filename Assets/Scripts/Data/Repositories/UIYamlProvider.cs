using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

/// <summary>
/// UI要素（ボード、立て看板）をパースするProvider
/// boards セクション: UI通知テキスト、立て看板データ
/// stagenotice: ステージ説明テロップ
/// </summary>
internal static class UIYamlProvider
{
    /// <summary>
    /// stagenotice をパースして UINotice に表示
    /// </summary>
    internal static void LoadStageNotice(YamlStream yaml)
    {
        if (yaml == null)
        {
            Debug.LogWarning("[UIYamlProvider.LoadStageNotice] yaml is null");
            return;
        }

        YamlMappingNode rootMapping = YamlParserHelper.GetRootMapping(yaml);
        if (rootMapping == null)
        {
            return;
        }

        foreach (KeyValuePair<YamlNode, YamlNode> entry in rootMapping.Children)
        {
            var key = (YamlScalarNode)entry.Key;
            if (key == null || key.Value != YamlSectionKeys.StageNotice)
            {
                continue;
            }

            string noticeText = ((YamlScalarNode)entry.Value).Value;
            GameObject uiNotice = GameObject.Find("UINotice");
            if (uiNotice == null)
            {
                Debug.LogWarning("[UIYamlProvider.LoadStageNotice] UINotice not found");
                return;
            }

            NoticeCtrl noticeCtrl = uiNotice.GetComponent<NoticeCtrl>();
            if (noticeCtrl == null)
            {
                Debug.LogWarning("[UIYamlProvider.LoadStageNotice] NoticeCtrl not found on UINotice");
                return;
            }

            noticeCtrl.ShowNotice(noticeText);
            break;
        }
    }

    /// <summary>
    /// boards セクションをパースして EventLoader に登録
    /// 座標なし: _board_data
    /// 座標あり: _signboard_data
    /// </summary>
    internal static void LoadBoardData(YamlStream yaml, EventLoader eventLoader)
    {
        if (yaml == null)
        {
            Debug.LogWarning("[UIYamlProvider.LoadBoardData] yaml is null");
            return;
        }

        if (eventLoader == null)
        {
            Debug.LogWarning("[UIYamlProvider.LoadBoardData] eventLoader is null");
            return;
        }

        YamlSequenceNode sequenceNode = YamlParserHelper.GetYamlSequenceNode(yaml, YamlSectionKeys.Boards);
        if (sequenceNode == null)
        {
            return;
        }

        Dictionary<string, string> boardData = new Dictionary<string, string>();
        Dictionary<string, (string text, Vector3 pos)> signboardData = new Dictionary<string, (string, Vector3)>();

        string codeField = BoardCommandFields.code.ToString();
        string textField = BoardCommandFields.text.ToString();
        string posField = BoardCommandFields.pos.ToString();

        foreach (YamlMappingNode mappingNode in sequenceNode)
        {
            string code = ExtractFieldValue(mappingNode, codeField);
            string text = ExtractFieldValue(mappingNode, textField);
            string posString = ExtractFieldValue(mappingNode, posField);

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
            eventLoader._board_data = boardData;
        }

        if (signboardData.Count > 0)
        {
            eventLoader._signboard_data = signboardData;
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
