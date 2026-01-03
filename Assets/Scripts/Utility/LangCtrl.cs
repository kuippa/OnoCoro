using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;

namespace CommonsUtility
{
    internal static class LangCtrl
    {
        internal static string GetLangVal(string key)
        {
            string ret = "";
            if (LangConst._lang_const.TryGetValue(GameConfig._APP_LANG, out Dictionary<string, string> lang_dict))
            {
                if (lang_dict.TryGetValue(key, out string buf))
                {
                    ret = buf;
                }
            }
            return ret;
        }

    }

}