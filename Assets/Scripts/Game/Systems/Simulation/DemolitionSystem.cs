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

        /// <summary>4 階建て以上は木造でない可能性が高い（木造判定の除外しきい値）</summary>
        private const int _NON_WOOD_STOREYS_THRESHOLD = 4;

        /// <summary>
        /// 発生原単位（t/㎡）を決める。
        ///
        /// [2026-08-28 実データ調査の結果]
        /// 三鷹市データには建物構造（uro:buildingStructureType）が存在しなかった。
        /// 代わりに以下が取得できたため、これらを組み合わせて構造を推定する:
        ///   1. uro:fireproofStructureType（耐火 / 準耐火 / その他）… 構造の最有力な代理変数
        ///   2. bldg:storeysaboveground（地上階数）… 高層なら非木造
        ///   3. uro:districtsAndZonesType（用途地域）… 商業系は非木造が多い
        ///   4. bldg:usagestr（建物用途）… 最後のフォールバック
        /// </summary>
        private static float GetUnitTonsPerSqm(Dictionary<string, string> buildingInfo)
        {
            // 1. 建物構造が明示されていれば最優先（他地域のデータで入っている場合に備える）
            if (buildingInfo.TryGetValue("uro:buildingStructureType", out string structure)
                && !string.IsNullOrEmpty(structure))
            {
                if (structure.Contains("木"))
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

            int storeys = GetStoreysAboveGround(buildingInfo);

            // 2. 耐火構造種別から推定（三鷹データで取得できる本命）
            if (buildingInfo.TryGetValue("uro:fireproofStructureType", out string fireproof)
                && !string.IsNullOrEmpty(fireproof))
            {
                if (fireproof.Contains("耐火") && !fireproof.Contains("準耐火"))
                {
                    return _UNIT_TONS_CONCRETE;  // 耐火建築物 → RC/SRC 相当
                }
                if (fireproof.Contains("準耐火"))
                {
                    return _UNIT_TONS_STEEL;     // 準耐火建築物 → S 造相当
                }
                if (fireproof.Contains("その他"))
                {
                    // 非耐火 → 木造の可能性が高い。ただし高層なら非木造とみなす
                    if (storeys >= _NON_WOOD_STOREYS_THRESHOLD)
                    {
                        return _UNIT_TONS_STEEL;
                    }
                    return _UNIT_TONS_WOOD;
                }
            }

            // 3. 階数による判定（耐火情報が無い場合）
            if (storeys >= _NON_WOOD_STOREYS_THRESHOLD)
            {
                return _UNIT_TONS_CONCRETE;
            }

            // 4. 用途地域・建物用途からのフォールバック
            if (buildingInfo.TryGetValue("uro:districtsAndZonesType", out string zone)
                && !string.IsNullOrEmpty(zone))
            {
                if (zone.Contains("商業") || zone.Contains("工業"))
                {
                    return _UNIT_TONS_STEEL;
                }
                if (zone.Contains("低層住居"))
                {
                    return _UNIT_TONS_WOOD;
                }
            }

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
        /// 地上階数を取得する（実データ優先・無ければ計算値）
        /// </summary>
        private static int GetStoreysAboveGround(Dictionary<string, string> buildingInfo)
        {
            if (buildingInfo.TryGetValue("bldg:storeysaboveground", out string storeysStr)
                && int.TryParse(storeysStr, out int storeys) && storeys > 0)
            {
                return storeys;
            }
            if (buildingInfo.TryGetValue("bldg:floors", out string floorsStr)
                && int.TryParse(floorsStr, out int floors))
            {
                return floors;
            }
            return 0;
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
