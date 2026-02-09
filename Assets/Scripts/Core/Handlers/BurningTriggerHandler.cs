using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// FireCube が複数タグのオブジェクトと接触した時の処理を管理するハンドラー
    /// 
    /// 監視対象：Garbage, FireCube, Water, Untagged（PLATEAU建物）
    /// 
    /// 使用例：FireCube プレファブに Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// </summary>
    internal class BurningTriggerHandler : MultiTagTriggerHandler
    {
        private Burning _burning = null;

        protected override void Awake()
        {
            base.Awake();
            
            // 監視対象タグを設定（enum対応）
            SetTargetTags(
                GameEnum.TagType.Garbage,
                GameEnum.TagType.FireCube,
                GameEnum.TagType.Water,
                GameEnum.TagType.Untagged
            );

            // 同じ GameObject に Burning コンポーネントを追加または取得
            _burning = GameObjectTreat.GetOrAddComponent<Burning>(gameObject);
            if (_burning == null)
            {
                Debug.LogWarning("[BurningTriggerHandler] Failed to get or add Burning component");
            }
        }

        protected override void OnTargetEnter(Collider other, GameEnum.TagType detectedTag)
        {
            if (_burning == null || other == null)
            {
                return;
            }

            switch (detectedTag)
            {
                case GameEnum.TagType.Garbage:
                    _burning.OnGarbageEnter(other);
                    break;

                case GameEnum.TagType.FireCube:
                    _burning.OnFireCubeEnter(other);
                    break;

                case GameEnum.TagType.Water:
                    _burning.OnWaterEnter(other);
                    break;

                case GameEnum.TagType.Untagged:
                    // PLATEAU 建物チェックは Burning 内で実施
                    _burning.OnBuildingEnter(other);
                    break;

                default:
                    Debug.LogWarning($"[BurningTriggerHandler] Unknown tag: {detectedTag}");
                    break;
            }
        }

        protected override void OnTargetExit(Collider other, GameEnum.TagType detectedTag)
        {
            if (_burning == null || other == null)
            {
                return;
            }

            switch (detectedTag)
            {
                case GameEnum.TagType.Garbage:
                    _burning.OnGarbageExit(other);
                    break;

                default:
                    break;
            }
        }
    }
}
