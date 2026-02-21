using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 掃除機タワー（Sweeper）の head オブジェクトが Garbage と Ash を削除する時の処理を管理するハンドラー
    /// 
    /// 監視対象：Garbage, Ash
    /// 役割：ゴミ削除実行（掃除処理）
    /// 
    /// 使用例：TowerSweeper > head 子オブジェクトに Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// 
    /// アタッチ手順：
    /// 1. TowerSweeper > head オブジェクトを選択
    /// 2. Collider (Is Trigger = ON) があることを確認
    /// 3. SweeperCtrl をアタッチ
    /// 4. SweeperCtrl.Awake() で GetOrAddComponent で自動アタッチ可能
    /// </summary>
    internal class TowerSweeperHeadTriggerHandler : MultiTagTriggerHandler
    {
        private SweeperCtrl _sweeperCtrl = null;

        protected override void Awake()
        {
            // 監視対象タグを設定（enum対応）- base.Awake() の前に設定必須
            SetTargetTags(
                GameEnum.TagType.Garbage,
                GameEnum.TagType.Ash
            );
            
            base.Awake();

            // 同じ GameObject に SweeperCtrl コンポーネントを取得
            _sweeperCtrl = GetComponent<SweeperCtrl>();
            if (_sweeperCtrl == null)
            {
                Debug.LogError($"[TowerSweeperHeadTriggerHandler] Failed to get SweeperCtrl component on {gameObject.name}. Ensure SweeperCtrl script is attached to this object.");
            }
        }

        protected override void OnTargetEnter(Collider other, GameEnum.TagType detectedTag)
        {
            // [NOTE] SweeperCtrl が OnTriggerStay で処理するため、ハンドラーは何もしない
            // ハンドラーの呼び出しを避けることで、二重処理（OnTriggerStay + OnTargetEnter）を防ぐ
        }

        protected override void OnTargetExit(Collider other, GameEnum.TagType detectedTag)
        {
            if (_sweeperCtrl == null || other == null)
            {
                return;
            }

            switch (detectedTag)
            {
                case GameEnum.TagType.Garbage:
                    // _sweeperCtrl.OnGarbageExit(other);
                    break;

                case GameEnum.TagType.Ash:
                    // _sweeperCtrl.OnAshExit(other);
                    break;

                default:
                    Debug.LogWarning($"[TowerSweeperHeadTriggerHandler] Unknown tag: {detectedTag}");
                    break;
            }
        }
    }
}
