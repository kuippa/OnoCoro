// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// StageGoalCtrl
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
		GameObject.Find("UIMessageBox").GetComponent<MessageBoxCtrl>().Show("Stage Cleared, want to back start page?", delegate(bool result)
		{
			if (result)
			{
				GameObject.Find("UIEscMenu").GetComponent<EscMenuCtrl>().OnClickBackToTitle();
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
		TelopCtrl component = GameObject.Find("UITelop").GetComponent<TelopCtrl>();
		if (component != null)
		{
			component.ShowTelop("Stage Goal!! Clear");
			_is_stage_goal = true;
			ActionDelay(3000, delegate
			{
				BackToStartPage();
			});
		}
	}

	internal static void ActionStageFail()
	{
		if (_is_stage_goal || _is_stage_fail)
		{
			return;
		}
		TelopCtrl component = GameObject.Find("UITelop").GetComponent<TelopCtrl>();
		if (component != null)
		{
			component.ShowTelop("Stage Fail!! Game Over");
			_is_stage_fail = true;
			ActionDelay(3000, delegate
			{
				BackToStartPage();
			});
		}
	}

	internal static void StartCheckStageGoal(MonoBehaviour caller)
	{
		int notfailtime = int.Parse(_dict_req["notfailtime"]);
		caller.StartCoroutine(ProcessGoalCheck(notfailtime));
	}

	internal static void StartCheckStageFail(MonoBehaviour caller)
	{
		int garbageCount = int.Parse(_dict_fail["garbage"]);
		caller.StartCoroutine(ProcessFailCheck(garbageCount));
	}

	private static bool CheckGarbageCount(int garbageCount)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Garbage.ToString());
		Debug.Log("Garbages founds " + array.Length + " " + garbageCount);
		if (array.Length > garbageCount)
		{
			return true;
		}
		return false;
	}

	private static bool CheckGameStageTime(int goalTime)
	{
		GameTimerCtrl gameTimerCtrl = null;
		GameObject gameObject = GameObject.Find("txtGameTime");
		if (gameObject != null)
		{
			gameTimerCtrl = gameObject.GetComponent<GameTimerCtrl>();
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
			yield return new WaitForSeconds(5f);
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
			yield return new WaitForSeconds(5f);
			if (CheckGameStageTime(notfailtime))
			{
				ActionStageGoal();
				break;
			}
		}
		yield return null;
	}
}
