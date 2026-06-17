using System;
using System.Collections.Generic;
using CommonsUtility;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

namespace CommonsUtility
{
    /// <summary>
    /// 防災施策のバランス値（staging/infrastructures.yaml）を読み込む Provider（W3 Task4）
    ///
    /// 全ステージ共通のゲームバランス調整ファイル。再コンパイルなしで施策の
    /// コスト・効果半径・鎮火力を調整できる。ファイルが無い/施策が未記載なら既定値が使われる。
    /// </summary>
    internal static class InfrastructureYamlProvider
    {
        private const string _CONFIG_FILE_NAME = "infrastructures.yaml";
        private const string _SECTION_KEY = "infrastructures";
        private const string _FIELD_TYPE = "type";
        private const string _FIELD_COST = "cost";
        private const string _FIELD_RADIUS = "radius";
        private const string _FIELD_POWER = "power";

        /// <summary>
        /// 共通バランスファイルを読み込み InfrastructureConfig に反映する。
        /// ステージロード時に呼ぶ（毎回既定値へ戻してから上書き）
        /// </summary>
        internal static void LoadSharedConfig()
        {
            InfrastructureConfig.ResetToDefaults();

            YamlStream yaml = null;
            try
            {
                yaml = LoadStreamingAsset.LoadYamlFile(_CONFIG_FILE_NAME);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[InfrastructureYamlProvider] {_CONFIG_FILE_NAME} のパースに失敗（既定値を使用）: {exception.Message}");
                return;
            }

            if (yaml == null)
            {
                // ファイルが無ければ既定値のまま（任意ファイル）
                return;
            }

            List<Dictionary<string, string>> rows = YamlParserHelper.BuildDictionaryListFromYaml(yaml, _SECTION_KEY);
            int appliedCount = 0;
            foreach (Dictionary<string, string> row in rows)
            {
                if (TryApplyRow(row))
                {
                    appliedCount = appliedCount + 1;
                }
            }

            Debug.Log($"[InfrastructureYamlProvider] 施策バランスを {appliedCount} 件読み込みました");
        }

        /// <summary>
        /// 1 行ぶんの施策バランスを InfrastructureConfig に反映
        /// </summary>
        private static bool TryApplyRow(Dictionary<string, string> row)
        {
            if (!row.TryGetValue(_FIELD_TYPE, out string typeText))
            {
                return false;
            }

            if (!Enum.TryParse(typeText, ignoreCase: true, out GameEnum.ModelsType type))
            {
                Debug.LogWarning($"[InfrastructureYamlProvider] 未知の施策タイプをスキップ: {typeText}");
                return false;
            }

            // 既定値をベースに、指定された項目だけ上書きする
            InfrastructureConfig.TryGet(type, out InfrastructureSpec spec);

            if (row.TryGetValue(_FIELD_COST, out string costText) && int.TryParse(costText, out int cost))
            {
                spec.Cost = cost;
            }
            if (row.TryGetValue(_FIELD_RADIUS, out string radiusText) && float.TryParse(radiusText, out float radius))
            {
                spec.Radius = radius;
            }
            if (row.TryGetValue(_FIELD_POWER, out string powerText) && float.TryParse(powerText, out float power))
            {
                spec.Power = power;
            }

            InfrastructureConfig.Set(type, spec);
            return true;
        }
    }
}
