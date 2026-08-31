using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 浸水による建物被害の設定と累計（PLATEAU CityHack 2026）
    ///
    /// 潮位が上がって一定時間以上水没した建物を倒壊させる。
    /// 倒壊処理そのものは building_break と同じ（新しい壊し方は作らない）。
    ///
    /// 設定はステージ YAML の flood セクションから上書きする。
    /// </summary>
    internal static class FloodDamageSystem
    {
        /// <summary>水面からこの深さ(m)より下に建物の底面があれば水没とみなす</summary>
        private const float _DEFAULT_DEPTH_METERS = 0.5f;

        /// <summary>水没状態がこの秒数続いたら倒壊させる</summary>
        private const float _DEFAULT_DURATION_SECONDS = 3.0f;

        /// <summary>1 秒あたりに倒壊させる最大棟数（同時大量倒壊で固まらせないための安全弁）</summary>
        private const int _DEFAULT_MAX_BREAKS_PER_SECOND = 5;

        private static float _depthMeters = _DEFAULT_DEPTH_METERS;
        private static float _durationSeconds = _DEFAULT_DURATION_SECONDS;
        private static int _maxBreaksPerSecond = _DEFAULT_MAX_BREAKS_PER_SECOND;
        private static bool _isEnabled = false;

        private static int _floodedBuildingCount = 0;

        /// <summary>浸水判定を行うか（YAML の flood セクションがあるステージだけ有効）</summary>
        internal static bool IsEnabled => _isEnabled;

        internal static float DepthMeters => _depthMeters;
        internal static float DurationSeconds => _durationSeconds;
        internal static int MaxBreaksPerSecond => _maxBreaksPerSecond;

        /// <summary>浸水で倒壊させた建物数</summary>
        internal static int FloodedBuildingCount => _floodedBuildingCount;

        /// <summary>
        /// 設定を既定値へ戻す（ステージロード時）。
        /// flood セクションが無いステージでは無効のままにする
        /// </summary>
        internal static void ResetToDefaults()
        {
            _depthMeters = _DEFAULT_DEPTH_METERS;
            _durationSeconds = _DEFAULT_DURATION_SECONDS;
            _maxBreaksPerSecond = _DEFAULT_MAX_BREAKS_PER_SECOND;
            _isEnabled = false;
            _floodedBuildingCount = 0;
        }

        /// <summary>
        /// YAML から読み込んだ設定を反映して浸水判定を有効にする
        /// </summary>
        internal static void Configure(float depthMeters, float durationSeconds, int maxBreaksPerSecond)
        {
            if (depthMeters > 0f)
            {
                _depthMeters = depthMeters;
            }
            if (durationSeconds > 0f)
            {
                _durationSeconds = durationSeconds;
            }
            if (maxBreaksPerSecond > 0)
            {
                _maxBreaksPerSecond = maxBreaksPerSecond;
            }
            _isEnabled = true;

            Debug.Log($"[FloodDamageSystem] 浸水被害を有効化 深さ {_depthMeters}m / {_durationSeconds}秒 /"
                + $" 最大 {_maxBreaksPerSecond} 棟毎秒");
        }

        /// <summary>倒壊 1 棟ぶんを記録する</summary>
        internal static void RecordFloodedBuilding()
        {
            _floodedBuildingCount = _floodedBuildingCount + 1;
        }
    }
}
