using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 1 年分の被害・投資・ROI 結果（Season 3 W3）
    /// </summary>
    internal struct YearResult
    {
        public int Year;
        public int QuakeCollapse;    // 地震倒壊（避けられない初期被害）
        public int FireSpread;       // 火災延焼（施策で減らせる被害・実測）
        public int AssumedSpread;    // 想定延焼被害（施策ゼロ時の想定）
        public int SavedBuildings;   // 救った棟数（想定 - 実測、0 でクランプ）
        public int Investment;       // この年の投資額
        public float Roi;            // 投資 100 あたりの救った棟数
        public float EvacuationCoverage;  // 避難カバー率（避難広場・別指標）
    }

    /// <summary>
    /// 被害・ROI 計算システム（Season 3 W3 Task 1）
    ///
    /// 火災焼失・地震倒壊はともに PlateauBuildingInteractor._doomedBuildings に入り
    /// 年をまたいで蓄積するため、年ごとの被害は差分で測る。
    /// 地震倒壊は BuildingBreak が各発火で報告する「実際に新規倒壊させた棟数」を積算し、
    /// 火災延焼は（年末の総増分 - 地震倒壊）で求める（多重地震でも正確に分離）。
    /// 設計: docs/_tasklist/detailed/season3-w3-result-roi-detailed-plan.md
    /// </summary>
    internal static class DamageReportSystem
    {
        private const string _PLATEAU_OBJECT_NAME = "Plateau";

        /// <summary>施策ゼロ時に 1 出火点あたり延焼する想定棟数（K=3 で開始・Task 4 で調整）</summary>
        private const int _FIRE_SPREAD_COEFFICIENT = 3;

        /// <summary>ROI 表示の基準投資単位（救った棟数 ÷ (投資額 / 単位)）</summary>
        private const float _ROI_INVESTMENT_UNIT = 100f;

        private static readonly List<YearResult> _results = new List<YearResult>();

        // 当年の集計用
        private static int _doomedCountAtYearStart = 0;
        private static int _quakeCollapseThisYear = 0;

        /// <summary>
        /// 年の開始時に呼ぶ（YearCycleSystem.StartYear から）。当年の集計を初期化
        /// </summary>
        internal static void OnYearStart()
        {
            _doomedCountAtYearStart = GetDoomedCount();
            _quakeCollapseThisYear = 0;
        }

        /// <summary>
        /// building_break が実際に新規倒壊させた棟数を加算（BuildingBreak から呼ぶ）
        /// 多重地震（本震+余震）でも各発火の実倒壊数を積み上げる
        /// </summary>
        internal static void AddQuakeCollapse(int newlyCollapsedCount)
        {
            if (newlyCollapsedCount <= 0)
            {
                return;
            }
            _quakeCollapseThisYear = _quakeCollapseThisYear + newlyCollapsedCount;
        }

        /// <summary>
        /// 年末に呼ぶ（YearCycleSystem.OnYearTimeUp から）。当年の結果を確定し保持
        /// </summary>
        internal static void OnYearEnd(int year)
        {
            int doomedCountAtYearEnd = GetDoomedCount();

            int totalNewDamage = Mathf.Max(0, doomedCountAtYearEnd - _doomedCountAtYearStart);
            int quakeCollapse = Mathf.Clamp(_quakeCollapseThisYear, 0, totalNewDamage);
            int fireSpread = totalNewDamage - quakeCollapse;

            // 想定延焼: YAML の baseline（消火なし実測）を優先、未指定なら K×N（W3 Task 4）
            int assumedSpread = GetBaselineSpread(year, quakeCollapse);
            int savedBuildings = Mathf.Max(0, assumedSpread - fireSpread);

            int investment = InvestmentLedger.GetYearTotal(year);
            float roi = CalcRoi(savedBuildings, investment);
            float evacuationCoverage = CalcEvacuationCoverage(year);

            YearResult result = new YearResult
            {
                Year = year,
                QuakeCollapse = quakeCollapse,
                FireSpread = fireSpread,
                AssumedSpread = assumedSpread,
                SavedBuildings = savedBuildings,
                Investment = investment,
                Roi = roi,
                EvacuationCoverage = evacuationCoverage
            };
            _results.Add(result);

            Debug.Log($"[DamageReportSystem] Year {year} 結果: 地震倒壊 {quakeCollapse} / 火災延焼 {fireSpread} / 想定 {assumedSpread} / 救った {savedBuildings} / 投資 {investment} / ROI {roi:F1}");
        }

        /// <summary>
        /// 指定年の結果を取得（無ければ false）
        /// </summary>
        internal static bool TryGetYearResult(int year, out YearResult result)
        {
            foreach (YearResult stored in _results)
            {
                if (stored.Year == year)
                {
                    result = stored;
                    return true;
                }
            }
            result = default(YearResult);
            return false;
        }

        /// <summary>
        /// 全年の累計サマリーを取得
        /// </summary>
        internal static YearResult GetSummary()
        {
            YearResult summary = new YearResult { Year = 0 };
            int investmentTotal = 0;
            foreach (YearResult stored in _results)
            {
                summary.QuakeCollapse = summary.QuakeCollapse + stored.QuakeCollapse;
                summary.FireSpread = summary.FireSpread + stored.FireSpread;
                summary.AssumedSpread = summary.AssumedSpread + stored.AssumedSpread;
                summary.SavedBuildings = summary.SavedBuildings + stored.SavedBuildings;
                investmentTotal = investmentTotal + stored.Investment;
            }
            summary.Investment = investmentTotal;
            summary.Roi = CalcRoi(summary.SavedBuildings, investmentTotal);
            summary.EvacuationCoverage = CalcEvacuationCoverage(-1);
            return summary;
        }

        /// <summary>
        /// 結果をすべて破棄（ステージロード時）
        /// </summary>
        internal static void Reset()
        {
            _results.Clear();
            _doomedCountAtYearStart = 0;
            _quakeCollapseThisYear = 0;
        }

        /// <summary>
        /// 想定火災延焼棟数（ベースライン）を取得
        /// YAML に baseline が指定されていればそれを使い、無ければ K×地震倒壊数
        /// </summary>
        private static int GetBaselineSpread(int year, int quakeCollapse)
        {
            if (EventLoader.instance != null)
            {
                int yamlBaseline = EventLoader.instance.GetYearBaseline(year);
                if (yamlBaseline >= 0)
                {
                    return yamlBaseline;
                }
            }
            return quakeCollapse * _FIRE_SPREAD_COEFFICIENT;
        }

        private static float CalcRoi(int savedBuildings, int investment)
        {
            if (investment <= 0)
            {
                return 0f;
            }
            return savedBuildings / (investment / _ROI_INVESTMENT_UNIT);
        }

        /// <summary>
        /// 避難広場（Plaza）の効果範囲が建物をカバーした割合（簡易・別指標）
        /// year < 0 のときは全 Plaza を対象（総括用）
        /// </summary>
        private static float CalcEvacuationCoverage(int year)
        {
            InfrastructureUnit[] units = Object.FindObjectsByType<InfrastructureUnit>(FindObjectsSortMode.None);
            int plazaCount = 0;
            foreach (InfrastructureUnit unit in units)
            {
                if (unit.InfraType == GameEnum.ModelsType.Plaza)
                {
                    plazaCount = plazaCount + 1;
                }
            }

            int totalBuildings = GetTotalBuildingCount();
            if (totalBuildings <= 0 || plazaCount == 0)
            {
                return 0f;
            }

            // 簡易モデル: Plaza 1 基が一定棟数をカバーすると仮定（Task 4 で精緻化）
            const int _PLAZA_COVER_BUILDINGS = 20;
            int covered = Mathf.Min(totalBuildings, plazaCount * _PLAZA_COVER_BUILDINGS);
            return (float)covered / totalBuildings * 100f;
        }

        private static int GetDoomedCount()
        {
            PlateauBuildingInteractor interactor = GetBuildingInteractor();
            if (interactor == null)
            {
                return 0;
            }
            return interactor._doomedBuildings.Count;
        }

        private static int GetTotalBuildingCount()
        {
            // PLATEAU 建物の概数（bldg_ を含むコライダー数）。Task 4 でキャッシュ化検討
            GameObject plateauObject = GameObject.Find(_PLATEAU_OBJECT_NAME);
            if (plateauObject == null)
            {
                return 0;
            }
            int count = 0;
            foreach (Transform child in plateauObject.GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains("bldg_"))
                {
                    count = count + 1;
                }
            }
            return count;
        }

        private static PlateauBuildingInteractor GetBuildingInteractor()
        {
            GameObject plateauObject = GameObject.Find(_PLATEAU_OBJECT_NAME);
            if (plateauObject == null)
            {
                return null;
            }
            return plateauObject.GetComponent<PlateauBuildingInteractor>();
        }
    }
}
