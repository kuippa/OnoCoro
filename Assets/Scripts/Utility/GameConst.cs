using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;


namespace CommonsUtility
{
    internal static class GlobalConst
    {
        internal const int APP_FPS = 60;          // FPS設定
        // internal const bool APP_DEBUGMODE = false; // デバッグモード ON/OFF
        internal const float APP_MINIMAM_FRAME = 0.06f; // アイテム作成時の最小フレーム

        internal const int UI_RAYCAST_MAX_DISTANCE = 20; // UIのレイキャストの最大距離

        internal const float TOOL_TIP_TIME = 0.7f; // ツールチップ表示時間

        internal static string SCORE1_SCALE = "BITRATE";
        internal static string SCORE2_SCALE = "CLOCK";
        internal static string SHORT_SCORE1_SCALE = "BIT";
        internal static string SHORT_SCORE2_SCALE = "CLK";

        internal static string UI_LV = "Lv : ";
        internal static string UI_UNIT_ID = "UnitID : ";
        internal static string UI_INFO = "INFO : ";

        // 言語設定
        internal static string LANG_JP = "JP";
        internal static string LANG_ENG = "ENG";


        // ゲームモード
        internal const string GAME_MODE_NORMAL = "normal";
        internal const string GAME_MODE_DEBUG = "debug";
        internal const string GAME_MODE_GOD = "god";
        internal const string GAME_MODE_CHEAT = "cheat";

        // シーンのパスと説明のリスト
        // internal static readonly string[,[]] SCENE_PATH_LIST = 
        // {
        //     "Scenes/Title",{GAME_MODE_NOMAL, "ノーマルモード"},
        // };
        // internal const string SCENE_TITLE = "Scenes/Title";

        internal static readonly Dictionary<string, string[]> _scene_path = new Dictionary<string, string[]>()
        {
            {"GameTutorial", new string[] {"Tutorial", "imgs/icons/income_fill72","stageinfo"}},
            // {"Scenes/StageSelect", new string[] {GAME_MODE_NOMAL, "ノーマルモード"}},
            // {"Scenes/Stage", new string[] {GAME_MODE_NOMAL, "ノーマルモード"}},
        };


    }

}