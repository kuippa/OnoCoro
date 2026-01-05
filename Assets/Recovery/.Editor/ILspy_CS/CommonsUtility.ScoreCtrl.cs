// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// CommonsUtility.ScoreCtrl
using CommonsUtility;
using TMPro;
using UnityEngine;

public class ScoreCtrl : MonoBehaviour
{
	public static ScoreCtrl instance;

	private static int _intScore;

	private static int _intScoreCLK;

	private const string _SCORE_FIELD1 = "txtScore1";

	private const string _SCORE_FIELD2 = "txtScore2";

	private static void DisplayScore(int intScore, string score_field, string score_scale)
	{
		GameObject gameObject = GameObject.Find(score_field);
		if (!(gameObject == null))
		{
			string text = $"{intScore:#,0}" + score_scale;
			gameObject.GetComponent<TextMeshProUGUI>().SetText(text);
		}
	}

	internal static void InitScore(int intScore = 0, string score_type = "BIT")
	{
		if (score_type == "BIT")
		{
			_intScore = intScore;
			DisplayScore(intScore, "txtScore1", "BIT");
		}
		else if (score_type == "CLK")
		{
			_intScoreCLK = intScore;
			DisplayScore(intScore, "txtScore2", "CLK");
		}
	}

	internal static bool IsScorePositiveInt(int intScore, string score_type = "BIT")
	{
		if (score_type == "BIT")
		{
			if (_intScore + intScore < 0)
			{
				return false;
			}
		}
		else if (score_type == "CLK" && _intScoreCLK + intScore < 0)
		{
			return false;
		}
		return true;
	}

	internal static float CalcGarbageMagnification(Vector3 ScaleMagnification)
	{
		return ScaleMagnification.x + ScaleMagnification.y + ScaleMagnification.z;
	}

	internal static int GetSliceGarbageScore(Collider target)
	{
		int result = 0;
		if (target.tag != GameEnum.TagType.Garbage.ToString())
		{
			return result;
		}
		return target.gameObject.GetComponent<GarbageCube>().GetUnitStruct().BaseScore;
	}

	internal static int GetTotalGarbageScore(Collider target)
	{
		int result = 0;
		if (target.tag != GameEnum.TagType.Garbage.ToString())
		{
			return result;
		}
		int baseScore = target.gameObject.GetComponent<GarbageCube>().GetUnitStruct().BaseScore;
		if (CalcGarbageMagnification(target.transform.localScale) <= 0.6f)
		{
			return baseScore;
		}
		return Mathf.Max(GetCountSliceGarbage(target.transform.localScale.x), GetCountSliceGarbage(target.transform.localScale.y), GetCountSliceGarbage(target.transform.localScale.z)) * baseScore;
	}

	private static int GetCountSliceGarbage(float size)
	{
		float num = 0.4f;
		size -= 0.2f;
		return Mathf.CeilToInt(size / num) + 1;
	}

	private static void ShowScoreEventLog(int intScore, string score_type)
	{
		EventLogCtrl.Instance.ShowEventLog("Score:" + intScore + score_type);
	}

	internal static void UpdateAndDisplayScore(int intScore, string score_type = "BIT")
	{
		if (score_type == "BIT")
		{
			_intScore += intScore;
			DisplayScore(_intScore, "txtScore1", "BIT");
			ShowScoreEventLog(intScore, score_type);
		}
		else if (score_type == "CLK")
		{
			_intScoreCLK += intScore;
			DisplayScore(_intScoreCLK, "txtScore2", "CLK");
			ShowScoreEventLog(intScore, score_type);
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
	}
}
