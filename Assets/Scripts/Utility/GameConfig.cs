using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonsUtility
{
    // ゲームコンフィグ用のシングルトン
    internal sealed class GameConfig: MonoBehaviour
    {

        private static GameConfig instance = null;

        // internal static string _APP_GAME_MODE = GlobalConst.GAME_MODE_NORMAL;
        internal static string _APP_GAME_MODE = GlobalConst.GAME_MODE_DEBUG;
        internal static string _APP_LANG = GlobalConst.LANG_JP;

        // public static bool eventWholeAction = false;   // 全体イベント実行中
        // public static bool AdsInAction = false;   // 広告実行中
        // public static bool eventInAction = false;   // 個別のイベント実行中
        // public static bool dialogRet = false;   // 確認画面YESNO

        private static void APPSettings()
        {
            Application.targetFrameRate = GlobalConst.APP_FPS;
        }

        void Awake()
        {
            #if UNITY_EDITOR
                Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            #endif

            #if UNITY_IPHONE || UNITY_IOS
            System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            #endif

            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

        }

        internal static GameConfig InitGameConfig()
        {
            #if UNITY_EDITOR
                // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                // Debug.Log("GameConfig.InitGameConfig()");
            #endif

            // Application.targetFrameRate = GlobalConst.APP_FPS;

            APPSettings();
            // GetPlayerInfo();

            return instance;
        }


    }
}