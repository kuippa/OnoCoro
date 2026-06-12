using System.Collections.Generic;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 投資台帳（Season 3 W2）
    ///
    /// 施策の配置コストを年ごとに記録する。W3 の被害率・ROI 計算の入力になる。
    /// 年は YearCycleSystem.CurrentYear を参照（年サイクル外の配置は year 0 に記録）。
    /// </summary>
    internal static class InvestmentLedger
    {
        /// <summary>年 → 投資合計</summary>
        private static readonly Dictionary<int, int> _yearTotals = new Dictionary<int, int>();

        /// <summary>年 → 施策タイプ別の配置数</summary>
        private static readonly Dictionary<int, Dictionary<GameEnum.ModelsType, int>> _yearPlacements
            = new Dictionary<int, Dictionary<GameEnum.ModelsType, int>>();

        /// <summary>
        /// 施策の配置を記録する
        /// </summary>
        internal static void RecordInvestment(GameEnum.ModelsType infraType, int cost)
        {
            int year = YearCycleSystem.CurrentYear;

            if (!_yearTotals.ContainsKey(year))
            {
                _yearTotals[year] = 0;
            }
            _yearTotals[year] = _yearTotals[year] + cost;

            if (!_yearPlacements.ContainsKey(year))
            {
                _yearPlacements[year] = new Dictionary<GameEnum.ModelsType, int>();
            }
            if (!_yearPlacements[year].ContainsKey(infraType))
            {
                _yearPlacements[year][infraType] = 0;
            }
            _yearPlacements[year][infraType] = _yearPlacements[year][infraType] + 1;

            Debug.Log($"[InvestmentLedger] Year {year}: {infraType} を配置（コスト {cost}）年合計 {_yearTotals[year]} / 総計 {GetGrandTotal()}");
        }

        /// <summary>
        /// 指定年の投資合計を取得
        /// </summary>
        internal static int GetYearTotal(int year)
        {
            if (_yearTotals.TryGetValue(year, out int total))
            {
                return total;
            }
            return 0;
        }

        /// <summary>
        /// 全年の投資総計を取得
        /// </summary>
        internal static int GetGrandTotal()
        {
            int grandTotal = 0;
            foreach (KeyValuePair<int, int> entry in _yearTotals)
            {
                grandTotal = grandTotal + entry.Value;
            }
            return grandTotal;
        }

        /// <summary>
        /// 指定年の施策タイプ別配置数を取得（未配置なら空辞書）
        /// </summary>
        internal static Dictionary<GameEnum.ModelsType, int> GetYearPlacements(int year)
        {
            if (_yearPlacements.TryGetValue(year, out Dictionary<GameEnum.ModelsType, int> placements))
            {
                return placements;
            }
            return new Dictionary<GameEnum.ModelsType, int>();
        }

        /// <summary>
        /// 台帳をリセット（ステージロード時）
        /// </summary>
        internal static void Reset()
        {
            _yearTotals.Clear();
            _yearPlacements.Clear();
        }
    }
}
