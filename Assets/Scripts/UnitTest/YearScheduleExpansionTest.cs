using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// YearScheduleYamlProvider のスロット置換・schedule 展開ロジックのテスト（Season 3 W1）
///
/// 使用方法：
/// 1. このスクリプトを任意の GameObject にアタッチ
/// 2. Play ボタンで実行
/// 3. Console に結果が表示される
/// </summary>
public class YearScheduleExpansionTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("YearSchedule Expansion Test Started");
        Debug.Log("========================================");

        TestReplaceSlots();
        Debug.Log("");
        TestExpandScheduleEntryTimeOffset();

        Debug.Log("========================================");
        Debug.Log("YearSchedule Expansion Test Completed");
        Debug.Log("========================================");
    }

    /// <summary>
    /// スロット置換のテスト（予約キー pattern/at が置換されないことも確認）
    /// </summary>
    private void TestReplaceSlots()
    {
        Debug.Log("[TEST] ReplaceSlots");

        Dictionary<string, string> bindings = new Dictionary<string, string>
        {
            { "pattern", "quake_fire" },
            { "at", "5" },
            { "intensity", "1.6" },
            { "spot", "-184, 40, -52" }
        };

        // ケース 1: 単一スロット置換
        {
            string result = YearScheduleYamlProvider.ReplaceSlots("{intensity}", bindings);
            LogTestResult("slot intensity", result == "1.6", result);
        }

        // ケース 2: 文字列中のスロット置換
        {
            string result = YearScheduleYamlProvider.ReplaceSlots("FireCube, {spot}", bindings);
            LogTestResult("slot in value string", result == "FireCube, -184, 40, -52", result);
        }

        // ケース 3: 予約キーは置換されない
        {
            string result = YearScheduleYamlProvider.ReplaceSlots("{pattern} {at}", bindings);
            LogTestResult("reserved keys not replaced", result == "{pattern} {at}", result);
        }

        // ケース 4: 未束縛スロットはそのまま残る
        {
            string result = YearScheduleYamlProvider.ReplaceSlots("{unknown}", bindings);
            LogTestResult("unbound slot kept", result == "{unknown}", result);
        }
    }

    /// <summary>
    /// schedule 展開の at オフセット加算テスト
    /// SpawnPatternRepository の実ファイル（patterns/quake_fire.yaml）を使用
    /// </summary>
    private void TestExpandScheduleEntryTimeOffset()
    {
        Debug.Log("[TEST] ExpandScheduleEntry (patterns/quake_fire.yaml が必要)");

        SpawnPatternRepository.LoadAllPatterns();

        Dictionary<string, string> scheduleEntry = new Dictionary<string, string>
        {
            { "pattern", "quake_fire" },
            { "at", "10" },
            { "intensity", "1.6" },
            { "break_count", "5" }
        };

        List<Dictionary<string, string>> resultEvents = new List<Dictionary<string, string>>();
        YearScheduleYamlProvider.ExpandScheduleEntry(scheduleEntry, resultEvents, 1);

        LogTestResult("expanded events not empty", resultEvents.Count > 0, $"count={resultEvents.Count}");

        if (resultEvents.Count > 0)
        {
            // quake_fire の先頭イベントは time: 1 → at: 10 加算で 11 になる
            string firstTime = resultEvents[0]["time"];
            LogTestResult("first event time = at + relative", firstTime == "11", firstTime);

            // {intensity} スロットが置換されている
            string firstValue = resultEvents[0]["value"];
            LogTestResult("intensity slot bound", firstValue == "1.6", firstValue);
        }
    }

    private void LogTestResult(string caseName, bool isPassed, object detail)
    {
        if (isPassed)
        {
            Debug.Log($"  [PASS] {caseName}");
        }
        else
        {
            Debug.LogError($"  [FAIL] {caseName} (actual: {detail})");
        }
    }
}
