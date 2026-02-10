using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 雨吸収コントローラー（RainAbsorb）が RainDrop と建物を検出した時のハンドラー
    /// 
    /// 監視対象：RainDrop, Untagged（PLATEAU建物）
    /// 
    /// 責務：
    /// - RainDrop 進入時に吸収処理を実行
    /// - Untagged（建物）の沈下処理を実行
    /// 
    /// 使用例：RainAbsorbController GameObject に Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// </summary>
    internal class RainAbsorbTriggerHandler : MultiTagTriggerHandler
    {
        private RainAbsorbController _rainAbsorbController = null;

        protected override void Awake()
        {
            // MultiTagTriggerHandler は TriggerHandler._targetTag を使わず、独自の _targetTags を使用するため、
            // TriggerHandler.Awake() の warning を抑えるためにダミーの target tag を設定
            SetDefaultTargetTag(GameEnum.TagType.RainDrop.ToString());
            
            base.Awake();
            
            // 実際の監視対象タグを設定（enum対応）
            SetTargetTags(
                GameEnum.TagType.RainDrop,
                GameEnum.TagType.Untagged
            );

            // 同じ GameObject に RainAbsorbController コンポーネントを取得
            _rainAbsorbController = GetComponent<RainAbsorbController>();
            if (_rainAbsorbController == null)
            {
                Debug.LogWarning("[RainAbsorbTriggerHandler] Failed to get RainAbsorbController component"+ this.gameObject.name);
            }
        }

        protected override void OnTargetEnter(Collider other, GameEnum.TagType detectedTag)
        {
            if (_rainAbsorbController == null || other == null)
            {
                return;
            }

            switch (detectedTag)
            {
                case GameEnum.TagType.RainDrop:
                    _rainAbsorbController.OnRainDropEnter(other);
                    break;

                case GameEnum.TagType.Untagged:
                    // PLATEAU 建物チェックは RainAbsorbController 内で実施
                    _rainAbsorbController.OnBuildingEnter(other);
                    break;

                default:
                    Debug.LogWarning($"[RainAbsorbTriggerHandler] Unknown tag: {detectedTag}");
                    break;
            }
        }

        protected override void OnTargetExit(Collider other, GameEnum.TagType detectedTag)
        {
            // RainAbsorbController では現在 OnTriggerExit の処理はないため未実装
        }
    }
}
