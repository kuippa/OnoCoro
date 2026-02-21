using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 掃除機タワー（Sweeper）の watch オブジェクトが Garbage と Ash を検出した時の処理を管理するハンドラー
    /// 
    /// 監視対象：Garbage, Ash
    /// 役割：ターゲット検出と登録
    /// 
    /// 使用例：TowerSweeper の watch 子オブジェクトに Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// 
    /// アタッチ手順：
    /// 1. TowerSweeper > watch オブジェクトを選択
    /// 2. Collider (Is Trigger = ON) があることを確認
    /// 3. このスクリプト TowerSweeperWatchTriggerHandler をアタッチ
    /// 4. TowerSweeperCtrl.Awake() で GetOrAddComponent で自動アタッチ可能
    /// </summary>
    internal class TowerSweeperWatchTriggerHandler : MultiTagTriggerHandler
    {
        private TowerSweeperCtrl _towerSweeperCtrl = null;

        protected override void Awake()
        {
            // 監視対象タグを設定（enum対応）- base.Awake() の前に設定必須
            SetTargetTags(
                GameEnum.TagType.Garbage,
                GameEnum.TagType.Ash
            );
            
            base.Awake();

            // 親オブジェクト（TowerSweeper）から TowerSweeperCtrl を取得
            _towerSweeperCtrl = GetComponentInParent<TowerSweeperCtrl>();
            if (_towerSweeperCtrl == null)
            {
                Debug.LogWarning("[TowerSweeperWatchTriggerHandler] Failed to get TowerSweeperCtrl from parent");
            }
        }

        protected override void OnTargetEnter(Collider other, GameEnum.TagType detectedTag)
        {
            if (_towerSweeperCtrl == null || other == null)
            {
                return;
            }

            switch (detectedTag)
            {
                case GameEnum.TagType.Garbage:
                    _towerSweeperCtrl.OnGarbageEnter(other);
                    break;

                case GameEnum.TagType.Ash:
                    _towerSweeperCtrl.OnAshEnter(other);
                    break;

                default:
                    Debug.LogWarning($"[TowerSweeperWatchTriggerHandler] Unknown tag: {detectedTag}");
                    break;
            }
        }

        protected override void OnTargetExit(Collider other, GameEnum.TagType detectedTag)
        {
            // TowerSweeper では現在 OnTriggerExit の処理はないため未実装
        }
    }
}
