// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// CommonsUtility.LangCtrl
using CommonsUtility;

internal static class LangCtrl
{
	internal static string GetLangVal(string key)
	{
		string result = "";
		if (LangConst._lang_const.TryGetValue(GameConfig._APP_LANG, out var value) && value.TryGetValue(key, out var value2))
		{
			result = value2;
		}
		return result;
	}
}
