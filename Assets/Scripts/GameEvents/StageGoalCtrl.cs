using UnityEngine;
// using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;   // Task
// using System.Collections; // 'IEnumerator' を使用するために 'System.Collections' を追加
// using System.Windows.Forms; // Control.Invoke

public static class StageGoalCtrl
{
    internal static Dictionary<string, string> _dict_req = new Dictionary<string, string>();
    private static bool _is_stage_goal = false;


//   - building: repair_all

    internal static bool IsGoalTypeBuilding()
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
        // TODO: メッセージの差し替え
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
        if (_is_stage_goal)
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



}
