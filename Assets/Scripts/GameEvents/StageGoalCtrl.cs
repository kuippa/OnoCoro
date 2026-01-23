using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class StageGoalCtrl
{
    internal static Dictionary<string, string> _dict_req = new Dictionary<string, string>();
    internal static Dictionary<string, string> _dict_fail = new Dictionary<string, string>();

    private static bool _is_stage_goal = false;
    private static bool _is_stage_fail = false;

    private const int CHECK_INTERVAL = 5;

    private static bool IsGoalTypeBuilding()
    {
        return _dict_req.ContainsKey("building");
    }

    internal static bool IsBuildingAllRepair()
    {
        if (IsGoalTypeBuilding())
        {
            return _dict_req["building"] == "repair_all";
        }
        return false;
    }

    private static async void ActionDelay(int delaytime, Action action)
    {
        await Task.Delay(delaytime);
        action();
    }

    private static void BackToStartPage()
    {
        MessageBoxCtrl messageBox = GameObject.Find("UIMessageBox").GetComponent<MessageBoxCtrl>();
        messageBox.Show("Stage Cleared, want to back start page?", (result) =>
        {
            if (result)
            {
                EscMenuCtrl escMenuCtrl = GameObject.Find("UIEscMenu").GetComponent<EscMenuCtrl>();
                escMenuCtrl.OnClickBackToTitle();
                Debug.Log("Yesが選択されました");
            }
            else
            {
                Debug.Log("Noが選択されました");
            }
        });
    }

    internal static void ActionStageGoal()
    {
        if (_is_stage_goal || _is_stage_fail)
        {
            return;
        }

        TelopCtrl telopCtrl = GameObject.Find("UITelop").GetComponent<TelopCtrl>();
        if (telopCtrl != null)
        {
            telopCtrl.ShowTelop("Stage Goal!! Clear");
            _is_stage_goal = true;
            ActionDelay(3000, () => BackToStartPage());
        }
    }

    internal static void ActionStageFail()
    {
        if (_is_stage_goal || _is_stage_fail)
        {
            return;
        }

        TelopCtrl telopCtrl = GameObject.Find("UITelop").GetComponent<TelopCtrl>();
        if (telopCtrl != null)
        {
            telopCtrl.ShowTelop("Stage Fail!! Game Over");
            _is_stage_fail = true;
            ActionDelay(3000, () => BackToStartPage());
        }
    }

    internal static void StartCheckStageGoal(MonoBehaviour caller)
    {
        if (caller == null)
        {
            Debug.LogWarning("MonoBehaviour caller is null in StartCheckStageGoal");
            return;
        }

        // notfailtime キーが存在しない場合はゴール条件として無視
        if (!_dict_req.ContainsKey("notfailtime"))
        {
            Debug.Log("Key 'notfailtime' not found in goals. Time-based goal check is skipped.");
            return;
        }

        if (!int.TryParse(_dict_req["notfailtime"], out int notfailtime))
        {
            Debug.LogWarning($"Failed to parse notfailtime value: {_dict_req["notfailtime"]}. Time-based goal check is skipped.");
            return;
        }

        Debug.Log($"Starting goal check with notfailtime: {notfailtime} seconds");
        caller.StartCoroutine(ProcessGoalCheck(notfailtime));
    }

    internal static void StartCheckStageFail(MonoBehaviour caller)
    {
        if (caller == null)
        {
            Debug.LogWarning("MonoBehaviour caller is null in StartCheckStageFail");
            return;
        }

        // garbage キーが存在しない場合はゲームオーバー条件として無視
        if (!_dict_fail.ContainsKey("garbage"))
        {
            Debug.Log("Key 'garbage' not found in gameovers. Garbage-based fail check is skipped.");
            return;
        }

        if (!int.TryParse(_dict_fail["garbage"], out int garbageCount))
        {
            Debug.LogWarning($"Failed to parse garbage value: {_dict_fail["garbage"]}. Garbage-based fail check is skipped.");
            return;
        }

        Debug.Log($"Starting fail check with garbage threshold: {garbageCount}");
        caller.StartCoroutine(ProcessFailCheck(garbageCount));
    }

    private static bool CheckGarbageCount(int garbageCount)
    {
        GameObject[] garbageObjects = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Garbage.ToString());
        Debug.Log("Garbages founds " + garbageObjects.Length + " " + garbageCount);
        if (garbageObjects.Length > garbageCount)
        {
            return true;
        }
        return false;
    }

    private static bool CheckGameStageTime(int goalTime)
    {
        GameTimerCtrl gameTimerCtrl = null;
        GameObject gameTimerObject = GameObject.Find("txtGameTime");
        if (gameTimerObject != null)
        {
            gameTimerCtrl = gameTimerObject.GetComponent<GameTimerCtrl>();
        }
        if (gameTimerCtrl == null)
        {
            Debug.Log("GameTimerCtrl is null");
            return false;
        }
        if (gameTimerCtrl._time > (float)goalTime)
        {
            Debug.Log("nowtime > goalTime");
            return true;
        }
        return false;
    }

    private static IEnumerator ProcessFailCheck(int garbageCount)
    {
        while (!_is_stage_fail)
        {
            yield return new WaitForSeconds(CHECK_INTERVAL);
            if (CheckGarbageCount(garbageCount))
            {
                ActionStageFail();
                break;
            }
        }
        yield return null;
    }

    private static IEnumerator ProcessGoalCheck(int notfailtime)
    {
        while (!_is_stage_goal)
        {
            yield return new WaitForSeconds(CHECK_INTERVAL);
            if (CheckGameStageTime(notfailtime))
            {
                ActionStageGoal();
                break;
            }
        }
        yield return null;
    }
}
