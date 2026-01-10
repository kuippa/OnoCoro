// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// CommonsUtility.Utility
using System;
using System.Globalization;
using UnityEngine;

public static class Utility
{
	internal static float fRandomRange(float min, float max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	internal static int fRandomRange(int min, int max)
	{
		return UnityEngine.Random.Range(min, max + 1);
	}

	internal static string GetAppVersion()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("BuildDate");
		if (textAsset == null)
		{
			return "BuildDate: null";
		}
		return string.Join(".", textAsset.text.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
	}

	internal static float GetScreenDp()
	{
		float num = Screen.dpi / 160f;
		return (float)Screen.height / num;
	}

	internal static Vector3 StringToVector3(string str)
	{
		string[] array = str.Trim('(', ')').Split(',');
		float x = float.Parse(array[0], CultureInfo.InvariantCulture);
		float y = float.Parse(array[1], CultureInfo.InvariantCulture);
		float z = float.Parse(array[2], CultureInfo.InvariantCulture);
		return new Vector3(x, y, z);
	}
}
