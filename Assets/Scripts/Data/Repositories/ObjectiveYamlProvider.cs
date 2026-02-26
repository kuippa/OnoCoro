using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

/// <summary>
/// ゲーム目標・勝敗条件をパースするProvider
/// goals セクション: ステージの勝利条件
/// gameovers セクション: ゲームオーバー条件
/// </summary>
internal static class ObjectiveYamlProvider
{
    /// <summary>
    /// goals セクションをパースして StageGoalController に登録
    /// </summary>
    internal static void LoadGoals(YamlStream yaml)
    {
        if (yaml == null)
        {
            Debug.LogWarning("[ObjectiveYamlProvider.LoadGoals] yaml is null");
            return;
        }

        List<Dictionary<string, string>> yamlDataList = YamlParserHelper.BuildDictionaryListFromYaml(yaml, YamlSectionKeys.Goals);
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
        StageGoalController.StartCheckStageGoal(null);
    }

    /// <summary>
    /// gameovers セクションをパースして StageGoalController に登録
    /// </summary>
    internal static void LoadGameOvers(YamlStream yaml)
    {
        if (yaml == null)
        {
            Debug.LogWarning("[ObjectiveYamlProvider.LoadGameOvers] yaml is null");
            return;
        }

        List<Dictionary<string, string>> yamlDataList = YamlParserHelper.BuildDictionaryListFromYaml(yaml, YamlSectionKeys.GameOvers);
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
        StageGoalController.StartCheckStageFail(null);
    }
}
