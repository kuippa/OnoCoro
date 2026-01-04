using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

namespace CommonsUtility
{
    public static class Utility
    {

        // floatの場合はmaxまで
        internal static float fRandomRange(float min, float max)
        {
            float num = UnityEngine.Random.Range(min, max);
            return num;
        }

        internal static int fRandomRange(int min, int max)
        {
            int ret = UnityEngine.Random.Range(min, max+1);   // int 指定の場合は min - max-1
            return ret;
        }

        internal static string GetAppVersion()
        {
            TextAsset versiontxt = Resources.Load<TextAsset>("BuildDate");
            if (versiontxt == null)
            {
                return "BuildDate: null";
            }

            string app_versions = 
                "Version: " + 
                string.Join(".",versiontxt.text.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None));
            return app_versions;
        }

        internal static float GetScreenDp()
        {
            float f = Screen.dpi / 160f;
            float dp = Screen.height / f;
            return dp;
        }


    }

}

