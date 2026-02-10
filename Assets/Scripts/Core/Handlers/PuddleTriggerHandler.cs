using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 水たまり（Puddle）が RainDrop と他の Puddle を検出した時のハンドラー
    /// 
    /// 監視対象：RainDrop, Puddle
    /// 
    /// 責務：
    /// - RainDrop 進入時に水たまりのサイズを増加
    /// - Puddle 進入時に水たまりをマージ
    /// 
    /// 使用例：PuddleController GameObject に Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// </summary>
    internal class PuddleTriggerHandler : MultiTagTriggerHandler
    {
        private PuddleController _puddleController = null;

        protected override void Awake()
        {
            base.Awake();
            
            // 監視対象タグを設定（enum対応）
            SetTargetTags(
                GameEnum.TagType.RainDrop,
                GameEnum.TagType.Puddle
            );

            // 同じ GameObject に PuddleController コンポーネントを取得
            _puddleController = GetComponent<PuddleController>();
            if (_puddleController == null)
            {
                Debug.LogWarning("[PuddleTriggerHandler] Failed to get PuddleController component");
            }
        }

        protected override void OnTargetEnter(Collider other, GameEnum.TagType detectedTag)
        {
            if (_puddleController == null || other == null)
            {
                return;
            }

            switch (detectedTag)
            {
                case GameEnum.TagType.RainDrop:
                    _puddleController.OnRainDropEnter(other);
                    break;

                case GameEnum.TagType.Puddle:
                    _puddleController.OnPuddleEnter(other);
                    break;

                default:
                    Debug.LogWarning($"[PuddleTriggerHandler] Unknown tag: {detectedTag}");
                    break;
            }
        }

        protected override void OnTargetExit(Collider other, GameEnum.TagType detectedTag)
        {
            // PuddleController では現在 OnTriggerExit の処理はないため未実装
        }
    }
}
