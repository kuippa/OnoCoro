using System;
using System.Collections.Generic;
using System.IO;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.SceneManagement;
using YamlDotNet.RepresentationModel;

/// <summary>
/// YAML ステージデータの統合ローダー
/// 各Providerに責務を委譲し、ロード順序を制御する
/// </summary>
public class StageYamlRepository : MonoBehaviour
{
    private EventLoader _eventLoader = null;
    public static List<string> _ItemList = new List<string>();

    internal void LoadYamlData(string stageName)
    {
        StageGoalController.ResetStageState();

        YamlStream yaml = LoadStreamingAsset.LoadYamlFile(Path.GetFileName(stageName + LoadStreamingAsset.YAML_FILE_EXTENSION));
        if (yaml == null)
        {
            Debug.LogWarning("[StageYamlRepository.LoadYamlData] yaml is null");
            return;
        }

        // UI システム初期化
        UIYamlProvider.LoadStageNotice(yaml);

        // ゲーム環境セットアップ
        List<Dictionary<string, string>> itemLists = YamlParserHelper.BuildDictionaryListFromYaml(yaml, YamlSectionKeys.ItemLists);
        EnvironmentalYamlProvider.LoadItemLists(itemLists);

        EnvironmentalYamlProvider.LoadStageInit(yaml, _eventLoader);
        EnvironmentalYamlProvider.LoadTimerEvents(yaml, _eventLoader);

        // パス・ナビゲーションセットアップ
        RouteYamlProvider.LoadPathMakers(yaml);
        RouteYamlProvider.LoadRouteNames(yaml, _eventLoader);

        // ゲームオブジェクティブセットアップ
        ObjectiveYamlProvider.LoadGoals(yaml, _eventLoader);
        ObjectiveYamlProvider.LoadGameOvers(yaml, _eventLoader);

        // UI ボード・看板セットアップ
        UIYamlProvider.LoadBoardData(yaml, _eventLoader);
    }

    internal static List<string> GetItemList()
    {
        return _ItemList;
    }

    private void Awake()
    {
        _eventLoader = transform.parent.gameObject.AddComponent<EventLoader>();
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log("StagingYamlCtrl sceneName " + sceneName);
        LoadYamlData(sceneName);
    }
}
