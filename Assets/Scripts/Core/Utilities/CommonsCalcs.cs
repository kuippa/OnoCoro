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

        /// <summary>
        /// Vector3 を文字列からパース。Y 座標が "auto" の場合は地面高さを自動検出 (Raycast)
        /// </summary>
        /// <param name="str">座標文字列。例: "-1.8, auto, 140.4" または "-1.8, 48.3, 140.4"</param>
        /// <returns>パラメータされたベクトル、または地面高さを反映した Vector3</returns>
        internal static Vector3 ParseVector3WithAutoHeight(string str)
        {
            string[] array = str.Trim('(', ')').Split(',');
            
            if (array.Length < 3)
            {
                Debug.LogWarning($"[ParseVector3WithAutoHeight] Invalid string format: {str}. Expected 3 components.");
                return Vector3.zero;
            }
            
            // X と Z を数値化
            if (!float.TryParse(array[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float x))
            {
                Debug.LogWarning($"[ParseVector3WithAutoHeight] Invalid X value: {array[0]}");
                return Vector3.zero;
            }
            
            if (!float.TryParse(array[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float z))
            {
                Debug.LogWarning($"[ParseVector3WithAutoHeight] Invalid Z value: {array[2]}");
                return Vector3.zero;
            }
            
            // Y を判定（"auto" または数値）
            string yValue = array[1].Trim();
            float y;
            
            if (yValue.Equals(YamlValueKeywords.AutoHeight, StringComparison.OrdinalIgnoreCase))
            {
                // Raycast で地面高さを検出
                y = GetGroundHeightAtPosition(x, z);
            }
            else
            {
                if (!float.TryParse(yValue, NumberStyles.Any, CultureInfo.InvariantCulture, out y))
                {
                    Debug.LogWarning($"[ParseVector3WithAutoHeight] Invalid Y value: {yValue}. Must be a number or '{YamlValueKeywords.AutoHeight}'.");
                    return Vector3.zero;
                }
            }
            
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// 指定座標の地面高さを Raycast で検出
        /// </summary>
        private static float GetGroundHeightAtPosition(float x, float z)
        {
            Vector3 rayOrigin = new Vector3(x, 500f, z);
            Vector3 rayDirection = Vector3.down;
            
            // PLATEAU地形および建物を対象にしたレイキャスト
            // （すべてのコライダーを対象にする場合は LayerMask.GetMask() を使わない）
            const int maxDistance = 1000;
            
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxDistance))
            {
                // 衝突した地面の高さを返す（少し浮かせる）
                const float floatOffset = 0.0f;
                return hit.point.y + floatOffset;
            }
            
            // 地面が見つからない場合（デバッグ警告）
            Debug.Log($"[GetGroundHeightAtPosition] Ground not found at ({x}, ?, {z}). Returning default height (0).");
            return 0f;
        }
    }

}

