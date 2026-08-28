using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using CommonsUtility;

namespace CommonsUtility
{
    internal static class LanguageConstants
    {

        internal static readonly Dictionary<string, string> _jp_lang_const = new Dictionary<string, string>()
        {
            {"LV", "Lv : "},
            {"GAME_MODE_NORMAL", "ノーマルモード"},

            {"bldg:usagestr", "使用目的"},
            {"bldg:floors", "階数"},
            {"bldg:floor", "床面積[㎥]"},
            {"bldg:totalarea", "延べ面積[㎥]"},
            {"bldg:measuredheight", "建物高さ[m]"},
            {"uro:depth", "浸水時深さ[m]"},
            {"uro:duration", "浸水継続時間[h]"},
            {"uro:rankOrg", "浸水深さ区分"},
            {"uro:buildingID", "ビルID"},
            {"uro:prefecture", "都道府県"},
            {"uro:city", "市町村"},

            {"uro:areaClassificationType", "用途区域"},

            // 建物構造・地域地区（CityHack 2026: 解体廃棄物の発生原単位の係数に使用）
            // ※ PLATEAU の整備状況によって存在しない属性もある（無い場合は表示されないだけ）
            {"uro:buildingStructureType", "建物構造"},
            {"uro:buildingStructureOrgType", "建物構造（独自）"},
            {"uro:fireproofStructureType", "耐火構造種別"},
            {"uro:districtsAndZonesType", "地域地区"},
            {"uro:urbanPlanType", "区域区分"},
            {"uro:landUseType", "土地利用"},
            {"bldg:yearOfConstruction", "建築年"},
            {"bldg:class", "建物分類"},
            {"uro:totalFloorArea", "延床面積（実測）[㎡]"},
            {"uro:buildingFootprintArea", "建築面積[㎡]"},
            {"bldg:storeysaboveground", "地上階数"},
            {"bldg:storeysbelowground", "地下階数"},

            {"dem:lod", "道"},
            {"gml:name", "名称"},

            // 独自追加パラメーター
            // {"bldg:person", "想定人数[人]"},
            {"bldg:estimatedpersons", "想定人数[人]"},
            {"bldg:rebuildcost", "再建コスト[BIT]"},
            {"bldg:rebuildbouns", "再建ボーナス[CLK]"},

        };

        internal static readonly Dictionary<string, string> _eng_lang_const = new Dictionary<string, string>()
        {
            {"JP_LV", "Lv : "}
        };


        // static readonlyの場合定義順に注意
        internal static readonly Dictionary<string, Dictionary<string, string>> _lang_const = new Dictionary<string, Dictionary<string, string>>()
        {
            {"JP", _jp_lang_const},
            {"ENG", _eng_lang_const}
        };

    }
}