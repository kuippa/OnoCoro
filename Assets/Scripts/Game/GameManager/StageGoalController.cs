using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public static class StageGoalController
{
    internal static Dictionary<string, string> _dict_req = new Dictionary<string, string>();
    internal static Dictionary<string, string> _dict_fail = new Dictionary<string, string>();

    private static bool _is_stage_goal = false;
    private static bool _is_stage_fail = false;

    private const int CHECK_INTERVAL = 5;

    /// <summary>
    /// ステージ開始時にフラグをリセット
    /// </summary>
    internal static void ResetStageState()
    {
        _is_stage_goal = false;
        _is_stage_fail = false;
    }

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
        try
        {
            MessageBoxCtrl messageBox = GameObject.Find("UIMessageBox").GetComponent<MessageBoxCtrl>();
            messageBox.Show("Stage Cleared, want to back start page?", (result) =>
            {
                if (result)
                {
                    EscMenuCtrl escMenuCtrl = GameObject.Find("UIEscMenu").GetComponent<EscMenuCtrl>();
                    escMenuCtrl.OnClickBackToTitle();
                }
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[StageGoalController.BackToStartPage] Exception: {ex.Message}");
        }
    }

    internal static void ActionStageGoal()
    {
        if (_is_stage_goal || _is_stage_fail)
        {
            return;
        }

        try
        {
            GameObject telopObj = GameObject.Find("UITelop");
            if (telopObj == null)
            {
                return;
            }

            TelopCtrl telopCtrl = telopObj.GetComponent<TelopCtrl>();
            if (telopCtrl == null)
            {
                return;
            }

            telopCtrl.ShowTelop("Stage Goal!! Clear");
            _is_stage_goal = true;
            ActionDelay(3000, () => BackToStartPage());
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[StageGoalController.ActionStageGoal] Exception: {ex.Message}");
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
            return;
        }

        // gameover_type キーが存在しない場合はスキップ
        if (!_dict_fail.ContainsKey(GameOverCommandFields.gameover_type.ToString()))
        {
            return;
        }

        string gameoverTypeStr = _dict_fail[GameOverCommandFields.gameover_type.ToString()];

        // GarbageOverflow 以外のゲームオーバータイプはここでは処理しない
        if (gameoverTypeStr != GameOverType.GarbageOverflow.ToString())
        {
            return;
        }

        // threshold の取得
        if (!_dict_fail.ContainsKey(GameOverCommandFields.threshold.ToString()))
        {
            return;
        }

        if (!int.TryParse(_dict_fail[GameOverCommandFields.threshold.ToString()], out int garbageCount))
        {
            return;
        }

        // ごみチェック開始時に UI を表示する
        PollutantManager.ActivateCountUI();

        caller.StartCoroutine(ProcessFailCheck(garbageCount));
    }

    private static bool CheckGarbageCount(int garbageCount)
    {
        GameObject[] garbageObjects = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Garbage.ToString());
        int currentCount = garbageObjects.Length;

        PollutantManager.SetDisplayCount(currentCount);

        if (currentCount > garbageCount)
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
