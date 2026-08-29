using System.Collections.Generic;
using CommonsUtility;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

namespace CommonsUtility
{
    /// <summary>
    /// ステージ YAML の demolition セクション（解体廃棄物の係数）を読み込む Provider
    /// （PLATEAU CityHack 2026）
    ///
    /// 建物種別ごとに「発生原単位(t/㎡)」と「見た目の瓦礫量」を調整できるようにする。
    /// セクションが無い場合は DemolitionSystem の既定値が使われる。
    ///
    /// YAML 例:
    ///   demolition:
    ///     - type: wood
    ///       tons_per_sqm: 0.5     # 発生量[t/㎡]。トラック台数などの数字に効く
    ///       debris_per_ton: 200   # 見た目の瓦礫量のみに効く
    ///       max_cubes: 2000       # 1 棟あたりのキューブ数上限（物理負荷の安全弁）
    ///       noncombustible_ratio: 0.4  # 廃棄物のうち不燃物の割合（0.0〜1.0）
    /// </summary>
    internal static class DemolitionYamlProvider
    {
        private const string _SECTION_KEY = "demolition";
        private const string _FIELD_TYPE = "type";
        private const string _FIELD_TONS_PER_SQM = "tons_per_sqm";
        private const string _FIELD_DEBRIS_PER_TON = "debris_per_ton";
        private const string _FIELD_MAX_CUBES = "max_cubes";
        private const string _FIELD_NONCOMBUSTIBLE_RATIO = "noncombustible_ratio";

        /// <summary>
        /// demolition セクションを読み込み DemolitionSystem に反映する。
        /// ステージロード時に呼ぶ（毎回既定値へ戻してから上書き）
        /// </summary>
        internal static void LoadDemolitionConfig(YamlStream yaml)
        {
            DemolitionSystem.ResetSpecsToDefaults();

            if (yaml == null)
            {
                return;
            }

            List<Dictionary<string, string>> rows =
                YamlParserHelper.BuildDictionaryListFromYaml(yaml, _SECTION_KEY);
            if (rows.Count == 0)
            {
                // セクション未定義なら既定値のまま（任意セクション）
                return;
            }

            int appliedCount = 0;
            foreach (Dictionary<string, string> row in rows)
            {
                if (TryApplyRow(row))
                {
                    appliedCount = appliedCount + 1;
                }
            }

            Debug.Log($"[DemolitionYamlProvider] 解体係数を {appliedCount} 件読み込みました");
        }

        /// <summary>
        /// 1 行ぶんの係数を DemolitionSystem に反映（未指定の項目は既定値を維持）
        /// </summary>
        private static bool TryApplyRow(Dictionary<string, string> row)
        {
            if (!row.TryGetValue(_FIELD_TYPE, out string type) || string.IsNullOrEmpty(type))
            {
                return false;
            }

            string normalizedType = type.Trim().ToLowerInvariant();
            DemolitionSpec spec = DemolitionSystem.GetSpec(normalizedType);

            if (row.TryGetValue(_FIELD_TONS_PER_SQM, out string tonsText)
                && float.TryParse(tonsText, out float tonsPerSqm) && tonsPerSqm > 0f)
            {
                spec.TonsPerSqm = tonsPerSqm;
            }
            if (row.TryGetValue(_FIELD_DEBRIS_PER_TON, out string debrisText)
                && float.TryParse(debrisText, out float debrisPerTon) && debrisPerTon > 0f)
            {
                spec.DebrisPerTon = debrisPerTon;
            }
            if (row.TryGetValue(_FIELD_MAX_CUBES, out string maxCubesText)
                && int.TryParse(maxCubesText, out int maxCubes) && maxCubes > 0)
            {
                spec.MaxCubes = maxCubes;
            }
            if (row.TryGetValue(_FIELD_NONCOMBUSTIBLE_RATIO, out string ratioText)
                && float.TryParse(ratioText, out float ratio) && ratio >= 0f && ratio <= 1f)
            {
                spec.NoncombustibleRatio = ratio;
            }

            DemolitionSystem.SetSpec(normalizedType, spec);
            return true;
        }
    }
}
