using System;
using System.Globalization;
using UnityEngine;

namespace CommonsUtility
{
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
            TextAsset versiontxt = TextAssetLoader.LoadTextAsset(GlobalConst.BUILDDATE_RESOURCE_PATH);
            if (versiontxt == null)
            {
                return "BuildDate: null";
            }

            string app_versions = 
                "Version: " + 
                string.Join(".", versiontxt.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
            return app_versions;
        }

        internal static float GetScreenDp()
        {
            float f = Screen.dpi / 160f;
            float dp = Screen.height / f;
            return dp;
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

}

