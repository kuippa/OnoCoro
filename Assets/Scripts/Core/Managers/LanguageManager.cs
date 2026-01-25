using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

namespace CommonsUtility
{
    internal static class LanguageManager
    {
        internal static string GetLangVal(string key)
        {
            string result = "";
            if (LanguageConstants._lang_const.TryGetValue(GameConfig._APP_LANG, out Dictionary<string, string> lang_dict))
            {
                if (lang_dict.TryGetValue(key, out string buf))
                {
                    result = buf;
                }
            }
            return result;
        }

    }

}