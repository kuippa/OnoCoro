using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 解体廃棄物の算定・積算システム（PLATEAU CityHack 2026）
    ///
    /// 建物を解体（更地化）した際に発生する廃棄物量を
    /// 「延床面積 × 構造別の発生原単位(t/㎡)」で算定し、累計して 4t トラック換算で表示する。
    ///
    /// [重要] 原単位は現在すべて仮の値。パートナー（解体業）から実務値を受領し次第、
    /// infrastructures.yaml と同じ方式で外部ファイル化して差し替える（T3）。
    /// 設計: docs/_tasklist/detailed/cityhack2026-demolition-plan.md
    /// </summary>
    internal static class DemolitionSystem
    {
        // ===== 発生原単位（t/㎡）※ 仮の値。パートナー回答で差し替える =====
        private const float _UNIT_TONS_WOOD = 0.5f;      // 木造
        private const float _UNIT_TONS_STEEL = 0.7f;     // 鉄骨造(S)
        private const float _UNIT_TONS_CONCRETE = 1.1f;  // RC/SRC 造
        private const float _UNIT_TONS_UNKNOWN = 0.6f;   // 不明（木造と鉄骨の中間）

        /// <summary>混合廃棄物の見かけ比重（t/m³）※ 体積換算用・仮の値</summary>
        private const float _DEBRIS_DENSITY = 0.4f;

        /// <summary>4t トラック 1 台あたりの積載量（t）</summary>
        private const float _TRUCK_CAPACITY_TONS = 4.0f;

        /// <summary>瓦礫キューブ 1 個が表す廃棄物量（t）※ 見た目の量を決める</summary>
        private const float _TONS_PER_DEBRIS_CUBE = 3.0f;

        /// <summary>1 棟あたりの瓦礫キューブ上限（処理落ち防止）</summary>
        private const int _MAX_DEBRIS_CUBES_PER_BUILDING = 60;

        private static float _totalTons = 0f;
        private static int _demolishedBuildingCount = 0;

        /// <summary>累計の解体廃棄物量（t）</summary>
        internal static float TotalTons => _totalTons;

        /// <summary>解体した建物数</summary>
        internal static int DemolishedBuildingCount => _demolishedBuildingCount;

        /// <summary>累計の廃棄物体積（m³）</summary>
        internal static float TotalVolumeCubicMeters => _totalTons / _DEBRIS_DENSITY;

        /// <summary>4t トラック換算の台数（切り上げ）</summary>
        internal static int TruckCount => Mathf.CeilToInt(_totalTons / _TRUCK_CAPACITY_TONS);

        /// <summary>
        /// 建物属性から解体廃棄物量（t）を算定する。
        /// 延床面積（bldg:totalarea）× 構造別の発生原単位
        /// </summary>
        internal static float CalcDebrisTons(Dictionary<string, string> buildingInfo)
        {
            if (buildingInfo == null)
            {
                return 0f;
            }

            float totalArea = GetTotalFloorArea(buildingInfo);
            if (totalArea <= 0f)
            {
                return 0f;
            }

            float unitTons = GetUnitTonsPerSqm(buildingInfo);
            return totalArea * unitTons;
        }

        /// <summary>
        /// 延床面積を取得する。実測値（uro:totalFloorArea）を優先し、
        /// 無ければ計算値（bldg:totalarea = 底面積 × 階数）を使う
        /// </summary>
        private static float GetTotalFloorArea(Dictionary<string, string> buildingInfo)
        {
            if (buildingInfo.TryGetValue("uro:totalFloorArea", out string measuredArea)
                && float.TryParse(measuredArea, out float measured) && measured > 0f)
            {
                return measured;
            }
            if (buildingInfo.TryGetValue("bldg:totalarea", out string calcArea)
                && float.TryParse(calcArea, out float calculated))
            {
                return calculated;
            }
            return 0f;
        }

        /// <summary>
        /// 構造種別から発生原単位（t/㎡）を決める。
        /// PLATEAU の構造属性を優先し、無ければ建物用途から推定する
        /// </summary>
        private static float GetUnitTonsPerSqm(Dictionary<string, string> buildingInfo)
        {
            string structure = GetStructureText(buildingInfo);
            if (!string.IsNullOrEmpty(structure))
            {
                if (structure.Contains("木造") || structure.Contains("木質"))
                {
                    return _UNIT_TONS_WOOD;
                }
                if (structure.Contains("鉄骨鉄筋") || structure.Contains("鉄筋") || structure.Contains("コンクリート"))
                {
                    return _UNIT_TONS_CONCRETE;
                }
                if (structure.Contains("鉄骨") || structure.Contains("軽量"))
                {
                    return _UNIT_TONS_STEEL;
                }
            }

            // フォールバック: 建物用途からの推定（住宅系は木造が多い / 施設系は非木造が多い）
            if (buildingInfo.TryGetValue("bldg:usagestr", out string usage) && !string.IsNullOrEmpty(usage))
            {
                if (usage.Contains("住宅") && !usage.Contains("共同"))
                {
                    return _UNIT_TONS_WOOD;
                }
                if (usage.Contains("共同住宅") || usage.Contains("商業") || usage.Contains("業務")
                    || usage.Contains("文教") || usage.Contains("公共"))
                {
                    return _UNIT_TONS_CONCRETE;
                }
            }

            return _UNIT_TONS_UNKNOWN;
        }

        /// <summary>
        /// 建物の構造種別を表す文字列を取得（複数の属性名に対応）
        /// </summary>
        private static string GetStructureText(Dictionary<string, string> buildingInfo)
        {
            string[] structureKeys = new string[]
            {
                "uro:buildingStructureType",
                "uro:buildingStructureOrgType",
                "uro:fireproofStructureType"
            };

            foreach (string key in structureKeys)
            {
                if (buildingInfo.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            return "";
        }

        /// <summary>
        /// 解体を記録して累計に加算する
        /// </summary>
        internal static void RecordDemolition(float tons)
        {
            if (tons <= 0f)
            {
                return;
            }
            _totalTons = _totalTons + tons;
            _demolishedBuildingCount = _demolishedBuildingCount + 1;
        }

        /// <summary>
        /// 廃棄物量（t）から散布する瓦礫キューブ数を求める（上限あり）
        /// </summary>
        internal static int CalcDebrisCubeCount(float tons)
        {
            int count = Mathf.CeilToInt(tons / _TONS_PER_DEBRIS_CUBE);
            return Mathf.Clamp(count, 1, _MAX_DEBRIS_CUBES_PER_BUILDING);
        }

        /// <summary>
        /// 累計をリセット（ステージロード時）
        /// </summary>
        internal static void Reset()
        {
            _totalTons = 0f;
            _demolishedBuildingCount = 0;
        }

        /// <summary>
        /// 発表・HUD 用のサマリー文字列
        /// </summary>
        internal static string GetSummaryText()
        {
            return $"解体 {_demolishedBuildingCount} 棟 / 廃棄物 {_totalTons:F0} t / 4tトラック {TruckCount} 台分";
        }
    }
}
