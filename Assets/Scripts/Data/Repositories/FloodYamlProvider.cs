using System.Collections.Generic;
using CommonsUtility;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

namespace CommonsUtility
{
    /// <summary>
    /// ステージ YAML の flood セクション（浸水による建物被害）を読み込む Provider
    /// （PLATEAU CityHack 2026）
    ///
    /// セクションが無いステージでは浸水判定そのものが無効になる。
    ///
    /// YAML 例:
    ///   flood:
    ///     - depth: 0.5                  # 水面からこの深さ(m)より下に底面があれば水没
    ///       duration: 3.0               # 水没がこの秒数続いたら倒壊
    ///       max_breaks_per_second: 5    # 1 秒あたりの倒壊上限（負荷の安全弁）
    ///       debris: false               # 瓦礫を出すか（既定 false・負荷が大きい）
    /// </summary>
    internal static class FloodYamlProvider
    {
        private const string _SECTION_KEY = "flood";
        private const string _FIELD_DEPTH = "depth";
        private const string _FIELD_DURATION = "duration";
        private const string _FIELD_MAX_BREAKS = "max_breaks_per_second";
        private const string _FIELD_DEBRIS = "debris";

        /// <summary>
        /// flood セクションを読み込み FloodDamageSystem に反映する
        /// </summary>
        internal static void LoadFloodConfig(YamlStream yaml)
        {
            FloodDamageSystem.ResetToDefaults();

            if (yaml == null)
            {
                return;
            }

            List<Dictionary<string, string>> rows =
                YamlParserHelper.BuildDictionaryListFromYaml(yaml, _SECTION_KEY);
            if (rows.Count == 0)
            {
                // セクション未定義なら浸水被害は無効のまま（任意セクション）
                return;
            }

            Dictionary<string, string> row = rows[0];

            float depth = 0f;
            if (row.TryGetValue(_FIELD_DEPTH, out string depthText))
            {
                float.TryParse(depthText, out depth);
            }

            float duration = 0f;
            if (row.TryGetValue(_FIELD_DURATION, out string durationText))
            {
                float.TryParse(durationText, out duration);
            }

            int maxBreaks = 0;
            if (row.TryGetValue(_FIELD_MAX_BREAKS, out string maxBreaksText))
            {
                int.TryParse(maxBreaksText, out maxBreaks);
            }

            // 瓦礫は既定で出さない（1棟あたり最大200個のRigidbodyが蓄積し重くなるため）
            bool isDebrisEnabled = false;
            if (row.TryGetValue(_FIELD_DEBRIS, out string debrisText))
            {
                bool.TryParse(debrisText.Trim(), out isDebrisEnabled);
            }

            FloodDamageSystem.Configure(depth, duration, maxBreaks, isDebrisEnabled);
        }
    }
}
