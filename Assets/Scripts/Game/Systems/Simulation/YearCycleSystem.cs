using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 年サイクルのフェーズ（Season 3 ターンベース化）
    /// </summary>
    internal enum YearCyclePhase
    {
        /// <summary>年サイクル非対象ステージ（従来のタイムライン駆動）</summary>
        Inactive,

        /// <summary>配置フェーズ（タイマー停止・施策配置待ち）</summary>
        Placement,

        /// <summary>年進行中（タイマー進行・イベント発火）</summary>
        YearRunning,

        /// <summary>全年完走（最終結果表示は W3 で実装）</summary>
        Finished
    }

    /// <summary>
    /// 年サイクル状態機械（Season 3 W1）
    ///
    /// Placement → YearRunning → (年末処理) → Placement(翌年) → ... → Finished
    /// タイマーの進行・停止の実体は GameTimerCtrl（Presentation 層）が担い、
    /// 本クラスは状態と年カウンタのみを管理する（Game 層から UI 層は参照しない）。
    /// 設計: docs/_tasklist/detailed/season3-w1-turnbased-detailed-plan.md
    /// </summary>
    internal static class YearCycleSystem
    {
        /// <summary>年末に除去する敵ユニットのタグ（配置フェーズ中の延焼進行を防ぐ）</summary>
        private static readonly string[] _YEAR_END_CLEANUP_TAGS = new string[]
        {
            nameof(GameEnum.TagType.FireCube)
        };

        private const int _FIRST_YEAR = 1;

        internal static YearCyclePhase CurrentPhase { get; private set; } = YearCyclePhase.Inactive;
        internal static int CurrentYear { get; private set; } = 0;

        private static EventLoader _eventLoader = null;

        /// <summary>
        /// フェーズ変更通知（UI 層が購読する。引数: 新フェーズ, 現在年）
        /// </summary>
        internal static event Action<YearCyclePhase, int> OnPhaseChanged;

        /// <summary>
        /// 年サイクルが有効か（years ステージで初期化済みか）
        /// </summary>
        internal static bool IsActive()
        {
            return CurrentPhase != YearCyclePhase.Inactive;
        }

        /// <summary>
        /// years ステージのロード後に呼ぶ。Year 1 の配置フェーズで開始する
        /// </summary>
        internal static void InitializeForStage(EventLoader eventLoader)
        {
            if (eventLoader == null || !eventLoader.HasYearEvents())
            {
                Debug.LogWarning("[YearCycleSystem.InitializeForStage] 年別イベントが未登録です");
                return;
            }

            _eventLoader = eventLoader;
            CurrentYear = _FIRST_YEAR;
            OnPhaseChanged = null;  // 前ステージの購読者を破棄（破棄済み UI への通知を防ぐ）
            ChangePhase(YearCyclePhase.Placement);
            Debug.Log($"[YearCycleSystem] 年サイクル開始: 全 {eventLoader.GetYearCount()} 年");
        }

        /// <summary>
        /// シミュレーション状態を破棄して Inactive に戻す（ステージロード時のリセット用）
        /// </summary>
        internal static void ResetSimulation()
        {
            CurrentYear = 0;
            _eventLoader = null;
            OnPhaseChanged = null;
            CurrentPhase = YearCyclePhase.Inactive;
            InvestmentLedger.Reset();  // 投資台帳もステージ単位でリセット（Season 3 W2）
        }

        /// <summary>
        /// 現在年を開始する（UI の Start Year ボタンから GameTimerCtrl 経由で呼ばれる）
        /// </summary>
        /// <returns>年の duration（秒）。開始できない場合は 0</returns>
        internal static float StartYear()
        {
            if (CurrentPhase != YearCyclePhase.Placement)
            {
                Debug.LogWarning($"[YearCycleSystem.StartYear] Placement フェーズではありません: {CurrentPhase}");
                return 0f;
            }

            if (_eventLoader == null || !_eventLoader.LoadYearEvents(CurrentYear))
            {
                return 0f;
            }

            ChangePhase(YearCyclePhase.YearRunning);
            Debug.Log($"[YearCycleSystem] Year {CurrentYear} 開始");
            return _eventLoader.GetYearDuration(CurrentYear);
        }

        /// <summary>
        /// 年の duration 経過時に呼ばれる（GameTimerCtrl から）
        /// 年末処理を行い、翌年の配置フェーズ or 全年完走へ遷移する
        /// </summary>
        internal static void OnYearTimeUp()
        {
            if (CurrentPhase != YearCyclePhase.YearRunning)
            {
                return;
            }

            CleanupYearEndUnits();

            if (_eventLoader != null && CurrentYear >= _eventLoader.GetYearCount())
            {
                ChangePhase(YearCyclePhase.Finished);
                Debug.Log("[YearCycleSystem] 全年完走（Finished）");
                return;
            }

            CurrentYear = CurrentYear + 1;
            ChangePhase(YearCyclePhase.Placement);
            Debug.Log($"[YearCycleSystem] Year {CurrentYear} 配置フェーズへ");
        }

        /// <summary>
        /// 年末の敵ユニット除去
        /// タイマー停止はイベント発火を止めるだけで敵の挙動は止まらないため、
        /// 配置フェーズ中に延焼が進まないよう残存する火災ユニットを取り除く。
        /// 配置済みタワー（施策）は除去しない（翌年への引き継ぎ要件）
        /// </summary>
        private static void CleanupYearEndUnits()
        {
            int removedCount = 0;
            foreach (string tagName in _YEAR_END_CLEANUP_TAGS)
            {
                GameObject[] targetUnits = GameObject.FindGameObjectsWithTag(tagName);
                foreach (GameObject unit in targetUnits)
                {
                    UnityEngine.Object.Destroy(unit);
                    removedCount = removedCount + 1;
                }
            }
            Debug.Log($"[YearCycleSystem] 年末処理: 残存敵ユニット {removedCount} 体を除去");
        }

        private static void ChangePhase(YearCyclePhase newPhase)
        {
            CurrentPhase = newPhase;
            if (OnPhaseChanged != null)
            {
                OnPhaseChanged.Invoke(newPhase, CurrentYear);
            }
        }
    }
}
