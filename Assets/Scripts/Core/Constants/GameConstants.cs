using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
// using System.Collections.ObjectModel;   // ReadOnlyCollection

namespace CommonsUtility
{
    internal static class GlobalConst
    {
        internal const int APP_FPS = 60;          // FPS設定
        // internal const int APP_FPS = 75;          // FPS設定
        // internal const bool APP_DEBUGMODE = false; // デバッグモード ON/OFF
        internal const float APP_MINIMAM_FRAME = 0.06f; // アイテム作成時の最小フレーム

        internal const int UI_RAYCAST_MAX_DISTANCE = 20; // UIのレイキャストの最大距離

        internal const float TOOL_TIP_TIME = 0.7f; // ツールチップ表示時間

        internal const string SCORE1_SCALE = "BITRATE";
        internal const string SCORE2_SCALE = "CLOCK";
        internal const string SHORT_SCORE1_SCALE = "BIT";
        internal const string SHORT_SCORE2_SCALE = "CLK";

        internal const string UI_LV = "Lv : ";
        internal const string UI_UNIT_ID = "UnitID : ";
        internal const string UI_INFO = "INFO : ";

        // 言語設定
        internal const string LANG_JP = "JP";
        internal const string LANG_ENG = "ENG";

        // 改行コード
        // internal const string LINE_FEED = "\n";
        // internal static ReadOnlyCollection<string> LINE_FEEDS = Array.AsReadOnly<string>(new string[] { "\r\n", "\r", "\n" });
        // Environment.NewLine

        // ゲームモード
        internal const string GAME_MODE_NORMAL = "normal";
        internal const string GAME_MODE_DEBUG = "debug";
        internal const string GAME_MODE_GOD = "god";
        internal const string GAME_MODE_CHEAT = "cheat";

        // ゲームオブジェクトの設定値
        internal const float GARBAGE_BASE_SLICE_SIZE = 0.4f;    // 一回のスライスで切り取るサイズ
        internal const float GARBAGE_MINIMUM_SIZE = 0.2f;     // 最小サイズ    

        // ゲームオブジェクト名
        internal const string PLATEAU_OBJ_NAME = "Plateau";
        internal const string UI_INFO_OBJ_NAME = "UIInfo";

        // ログファイル設定
        internal const string _LOG_FILE_NAME = "onocoro.log";  // YYYYMMDDonocoro.log の形式になる

        // リソースパス - UI アイコン（Texture）
        // internal const string CURSOR_ICON_PATH = "imgs/icons/iconaddedlocal";  // カーソルアイコン
        internal const string CURSOR_ICON_PATH = "";  // カーソルアイコン（なし）

        // リソースパス - TextAsset
        internal const string BUILDDATE_RESOURCE_PATH = "BuildDate";  // ビルド日時テキスト（Resources相対パス）
        // internal const string BUILDDATE_FILE_PATH = "Assets/Resources/BuildDate.txt";  // ビルド日時テキスト（ファイルシステムパス）
        internal const string BUILDING_USAGE_XML_PATH = "xml/Building_usage";  // 建物用途定義XML

        // リソースパス - Material
        internal const string MATERIAL_BG_GREEN_PATH = "Materials/BG_Green";  // 背景用マテリアル（緑）
        internal const string MATERIAL_BG_RED_PATH = "Materials/BG_RED";  // 背景用マテリアル（赤）
        internal const string MATERIAL_PLATEAU_GENERIC_WOOD_PATH = "Materials/PlateauGenericWood";  // PLATEAU汎用木材マテリアル

        // リソースパス - UI Prefab
        internal const string UI_BUILDING_INFO_PREFAB_PATH = "Prefabs/UI/UIBuildingInfo";  // 建物情報UI




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