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
    /// <summary>
    /// 建物種別ごとの解体係数（ステージ YAML の demolition セクションで調整する）
    /// </summary>
    internal struct DemolitionSpec
    {
        /// <summary>延床面積 1 ㎡あたりの解体廃棄物発生量（t/㎡）</summary>
        public float TonsPerSqm;

        /// <summary>廃棄物 1t あたりの瓦礫キューブ量（見た目の量。大きいほど多く散らばる）</summary>
        public float DebrisPerTon;

        /// <summary>
        /// 1 棟あたりに生成する瓦礫キューブ数の上限（物理負荷の安全弁）。
        /// ここに達すると TonsPerSqm / DebrisPerTon を増やしても見た目が変わらなくなる
        /// </summary>
        public int MaxCubes;

        /// <summary>
        /// 廃棄物のうち不燃物が占める割合（0.0〜1.0）。
        /// コンクリートがら・金属くず・ガラスなど燃やせないもの。
        /// 木造は可燃が多く、RC 造はほぼ不燃になる
        /// </summary>
        public float NoncombustibleRatio;

        public DemolitionSpec(float tonsPerSqm, float debrisPerTon, int maxCubes, float noncombustibleRatio)
        {
            TonsPerSqm = tonsPerSqm;
            DebrisPerTon = debrisPerTon;
            MaxCubes = maxCubes;
            NoncombustibleRatio = noncombustibleRatio;
        }
    }

    internal static class DemolitionSystem
    {
        // ===== 建物種別のキー（YAML の type と対応）=====
        internal const string TYPE_WOOD = "wood";          // 木造
        internal const string TYPE_STEEL = "steel";        // 鉄骨造(S)
        internal const string TYPE_CONCRETE = "concrete";  // RC/SRC 造
        internal const string TYPE_UNKNOWN = "unknown";    // 不明

        /// <summary>1 棟あたりの瓦礫キューブ数の上限の既定値（YAML の max_cubes で上書きできる）</summary>
        private const int _DEFAULT_MAX_CUBES = 1000;

        /// <summary>建物種別ごとの係数（既定値。ステージ YAML の demolition で上書きされる）</summary>
        private static Dictionary<string, DemolitionSpec> _specs = BuildDefaultSpecs();

        private static Dictionary<string, DemolitionSpec> BuildDefaultSpecs()
        {
            // 発生原単位は仮の値（パートナー回答で差し替える）。
            // DebrisPerTon は見た目の量で、50 は従来の 5 倍に相当する
            // 原単位・組成比は PLATEAU 技術資料 plateau_tech_doc_0015「5.7 災害廃棄物発生量の
            // 採用原単位」の値（横浜市災害廃棄物処理計画／環境省 災害廃棄物対策指針 技術資料）。
            //   木造   0.6 t/㎡ ・ 非木造 1.0 t/㎡ ・ 焼失 0.23 t/㎡
            //   種類別割合は木造・非木造とも 可燃 11% / 不燃 89%
            //
            // [NOTE] 木造でも不燃が 89% を占めるのは、重量比だとコンクリート殻・
            //   基礎・土砂が支配的なため。体積の見た目とは一致しない
            // [未実装] 焼失（0.23 t/㎡・不燃 99.9%）は被害要因別の区分で、
            //   本システムは構造別のみのため未対応。火災倒壊と結びつけるのは今後の課題
            return new Dictionary<string, DemolitionSpec>
            {
                { TYPE_WOOD, new DemolitionSpec(0.6f, 50f, _DEFAULT_MAX_CUBES, 0.89f) },
                { TYPE_STEEL, new DemolitionSpec(1.0f, 50f, _DEFAULT_MAX_CUBES, 0.89f) },
                { TYPE_CONCRETE, new DemolitionSpec(1.0f, 50f, _DEFAULT_MAX_CUBES, 0.89f) },
                { TYPE_UNKNOWN, new DemolitionSpec(0.6f, 50f, _DEFAULT_MAX_CUBES, 0.89f) },
            };
        }

        /// <summary>係数を上書き登録する（DemolitionYamlProvider から呼ぶ）</summary>
        internal static void SetSpec(string type, DemolitionSpec spec)
        {
            _specs[type] = spec;
        }

        /// <summary>係数を取得する（未定義なら unknown にフォールバック）</summary>
        internal static DemolitionSpec GetSpec(string type)
        {
            if (_specs.TryGetValue(type, out DemolitionSpec spec))
            {
                return spec;
            }
            return _specs[TYPE_UNKNOWN];
        }

        /// <summary>
        /// 建物種別キーを画面表示用の日本語名にする（情報ウィンドウ用）
        /// </summary>
        internal static string GetStructureDisplayName(string type)
        {
            if (type == TYPE_WOOD)
            {
                return "木造（推定）";
            }
            if (type == TYPE_STEEL)
            {
                return "鉄骨造（推定）";
            }
            if (type == TYPE_CONCRETE)
            {
                return "RC・SRC造（推定）";
            }
            return "不明";
        }

        /// <summary>係数を既定値へ戻す（ステージロード時）</summary>
        internal static void ResetSpecsToDefaults()
        {
            _specs = BuildDefaultSpecs();
        }

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
        internal static int TruckCount => CalcTruckCount(_totalTons);

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

            DemolitionSpec spec = GetSpec(ClassifyStructure(buildingInfo));
            return totalArea * spec.TonsPerSqm;
        }

        /// <summary>
        /// 建物の廃棄物量から、散布する瓦礫の目標量を求める（建物種別ごとの係数を反映）
        /// </summary>
        internal static int CalcDebrisAmount(Dictionary<string, string> buildingInfo, float debrisTons)
        {
            DemolitionSpec spec = GetSpec(ClassifyStructure(buildingInfo));
            return Mathf.CeilToInt(debrisTons * spec.DebrisPerTon);
        }

        /// <summary>
        /// 1 棟あたりの瓦礫キューブ数の上限を取得する（建物種別ごと）
        /// </summary>
        internal static int CalcMaxCubes(Dictionary<string, string> buildingInfo)
        {
            return GetSpec(ClassifyStructure(buildingInfo)).MaxCubes;
        }

        /// <summary>
        /// 建物種別に応じた不燃物の割合（0.0〜1.0）を取得する
        /// </summary>
        internal static float CalcNoncombustibleRatio(Dictionary<string, string> buildingInfo)
        {
            return GetSpec(ClassifyStructure(buildingInfo)).NoncombustibleRatio;
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
        /// 建物種別（wood / steel / concrete / unknown）を推定する。
        ///
        /// [2026-08-28 実データ調査の結果]
        /// 三鷹市データには建物構造（uro:buildingStructureType）が存在しなかった。
        /// 代わりに以下が取得できたため、これらを組み合わせて構造を推定する:
        ///   1. uro:fireproofStructureType（耐火 / 準耐火 / その他）… 構造の最有力な代理変数
        ///   2. bldg:storeysaboveground（地上階数）… 高層なら非木造
        ///   3. uro:districtsAndZonesType（用途地域）… 商業系は非木造が多い
        ///   4. bldg:usagestr（建物用途）… 最後のフォールバック
        /// </summary>
        internal static string ClassifyStructure(Dictionary<string, string> buildingInfo)
        {
            if (buildingInfo == null)
            {
                return TYPE_UNKNOWN;
            }

            // 1. 建物構造が明示されていれば最優先（他地域のデータで入っている場合に備える）
            if (buildingInfo.TryGetValue("uro:buildingStructureType", out string structure)
                && !string.IsNullOrEmpty(structure))
            {
                if (structure.Contains("木"))
                {
                    return TYPE_WOOD;
                }
                if (structure.Contains("鉄骨鉄筋") || structure.Contains("鉄筋") || structure.Contains("コンクリート"))
                {
                    return TYPE_CONCRETE;
                }
                if (structure.Contains("鉄骨") || structure.Contains("軽量"))
                {
                    return TYPE_STEEL;
                }
            }

            int storeys = GetStoreysAboveGround(buildingInfo);

            // 2. 耐火構造種別から推定（三鷹データで取得できる本命）
            if (buildingInfo.TryGetValue("uro:fireproofStructureType", out string fireproof)
                && !string.IsNullOrEmpty(fireproof))
            {
                if (fireproof.Contains("耐火") && !fireproof.Contains("準耐火"))
                {
                    return TYPE_CONCRETE;  // 耐火建築物 → RC/SRC 相当
                }
                if (fireproof.Contains("準耐火"))
                {
                    return TYPE_STEEL;     // 準耐火建築物 → S 造相当
                }
                if (fireproof.Contains("その他"))
                {
                    // 非耐火 → 木造の可能性が高い。ただし高層なら非木造とみなす
                    if (storeys >= _NON_WOOD_STOREYS_THRESHOLD)
                    {
                        return TYPE_STEEL;
                    }
                    return TYPE_WOOD;
                }
            }

            // 3. 階数による判定（耐火情報が無い場合）
            if (storeys >= _NON_WOOD_STOREYS_THRESHOLD)
            {
                return TYPE_CONCRETE;
            }

            // 4. 用途地域・建物用途からのフォールバック
            if (buildingInfo.TryGetValue("uro:districtsAndZonesType", out string zone)
                && !string.IsNullOrEmpty(zone))
            {
                if (zone.Contains("商業") || zone.Contains("工業"))
                {
                    return TYPE_STEEL;
                }
                if (zone.Contains("低層住居"))
                {
                    return TYPE_WOOD;
                }
            }

            if (buildingInfo.TryGetValue("bldg:usagestr", out string usage) && !string.IsNullOrEmpty(usage))
            {
                if (usage.Contains("住宅") && !usage.Contains("共同"))
                {
                    return TYPE_WOOD;
                }
                if (usage.Contains("共同住宅") || usage.Contains("商業") || usage.Contains("業務")
                    || usage.Contains("文教") || usage.Contains("公共"))
                {
                    return TYPE_CONCRETE;
                }
            }

            return TYPE_UNKNOWN;
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

        // =============================================
        // 画面上のゴミ実物からの集計（リザルト表示用）
        //
        // RecordDemolition の累計（_totalTons）は「解体した建物の算定値」だが、
        // こちらは「マップに実際に転がっているゴミ」を数える。
        // 解体由来だけでなく火災由来・プレイヤー由来のゴミも含む総量になる。
        // =============================================

        /// <summary>タグ Garbage（可燃）が付いたオブジェクトの個数を数える</summary>
        internal static int CountBurnableGarbageObjects()
        {
            GameObject[] garbageObjects =
                GameObject.FindGameObjectsWithTag(GameEnum.TagType.Garbage.ToString());
            return garbageObjects.Length;
        }

        /// <summary>タグ GarbageNoBurn（不燃）が付いたオブジェクトの個数を数える</summary>
        internal static int CountNoBurnGarbageObjects()
        {
            GameObject[] garbageObjects =
                GameObject.FindGameObjectsWithTag(GameEnum.TagType.GarbageNoBurn.ToString());
            return garbageObjects.Length;
        }

        /// <summary>可燃・不燃を合わせたゴミの個数</summary>
        internal static int CountGarbageObjects()
        {
            return CountBurnableGarbageObjects() + CountNoBurnGarbageObjects();
        }

        /// <summary>
        /// ゴミの個数を廃棄物量[t]に換算する。
        ///
        /// 散布時は「目標量 = 廃棄物量 t × DebrisPerTon」を
        /// 1 個あたり GarbageCube の基準スコアぶん消化して生成している。
        /// ここはその逆算にあたるため、見た目の調整つまみ（DebrisPerTon）を
        /// 変えても換算後のトン数は変わらない。
        ///
        /// [注意] max_cubes で打ち切られた場合は実際の解体量より少なく出る
        /// </summary>
        internal static float CalcTonsFromGarbageCount(int garbageCount)
        {
            if (garbageCount <= 0)
            {
                return 0f;
            }

            // 混在したゴミを一律換算するため、種別不明の係数を代表値として使う
            float debrisPerTon = GetSpec(TYPE_UNKNOWN).DebrisPerTon;
            if (debrisPerTon <= 0f)
            {
                return 0f;
            }

            float totalAmount = garbageCount * GarbageCube.GetBaseScore();
            return totalAmount / debrisPerTon;
        }

        /// <summary>廃棄物量[t]を 4t トラックの台数に換算する（切り上げ）</summary>
        internal static int CalcTruckCount(float tons)
        {
            if (tons <= 0f)
            {
                return 0;
            }
            return Mathf.CeilToInt(tons / _TRUCK_CAPACITY_TONS);
        }

        /// <summary>
        /// リザルト用: マップ上のゴミ総量から「N t / 4tトラック M 台分」を作る
        /// </summary>
        internal static string GetGarbageTruckText()
        {
            int burnableCount = CountBurnableGarbageObjects();
            int noBurnCount = CountNoBurnGarbageObjects();

            float burnableTons = CalcTonsFromGarbageCount(burnableCount);
            float noBurnTons = CalcTonsFromGarbageCount(noBurnCount);
            float totalTons = burnableTons + noBurnTons;

            return $"廃材 {totalTons:F0} t"
                + $"（可燃 {burnableTons:F0} t / 不燃 {noBurnTons:F0} t）\n"
                + $"4t トラック {CalcTruckCount(totalTons)} 台分";
        }
    }
}
