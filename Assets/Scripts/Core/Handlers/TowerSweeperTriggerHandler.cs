using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 掃除機タワー（Sweeper）が Garbage と Ash を検出した時の処理を管理するハンドラー
    /// 
    /// 監視対象：Garbage, Ash
    /// 
    /// 使用例：TowerSweeper GameObject に Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// </summary>
    internal class TowerSweeperTriggerHandler : MultiTagTriggerHandler
    {
        private TowerSweeper _towerSweeper = null;

        protected override void Awake()
        {
            base.Awake();
            
            // 監視対象タグを設定（enum対応）
            SetTargetTags(
                GameEnum.TagType.Garbage,
                GameEnum.TagType.Ash
            );

            // 同じ GameObject に TowerSweeper コンポーネントを取得
            _towerSweeper = GetComponent<TowerSweeper>();
            if (_towerSweeper == null)
            {
                Debug.LogWarning("[TowerSweeperTriggerHandler] Failed to get TowerSweeper component");
            }
        }

        protected override void OnTargetEnter(Collider other, GameEnum.TagType detectedTag)
        {
            if (_towerSweeper == null || other == null)
            {
                return;
            }

            switch (detectedTag)
            {
                case GameEnum.TagType.Garbage:
                    _towerSweeper.OnGarbageEnter(other);
                    break;

                case GameEnum.TagType.Ash:
                    _towerSweeper.OnAshEnter(other);
                    break;

                default:
                    Debug.LogWarning($"[TowerSweeperTriggerHandler] Unknown tag: {detectedTag}");
                    break;
            }
        }

        protected override void OnTargetExit(Collider other, GameEnum.TagType detectedTag)
        {
            // TowerSweeper では現在 OnTriggerExit の処理はないため未実装
        }
    }
}
