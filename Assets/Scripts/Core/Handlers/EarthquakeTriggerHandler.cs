using UnityEngine;
using CommonsUtility;
using Debug = UnityEngine.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// トリガーコライダーで検出した Player との接触時に、
    /// 地震イベントを発動するハンドラー
    /// 
    /// 使用例：地面（DEM）などに Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// </summary>
    public class EarthquakeTriggerHandler : TriggerHandler
    {
        private const float DEFAULT_MAGNITUDE = 1.8f;

        protected override void Awake()
        {
            base.Awake();
            SetDefaultTargetTag(GameEnum.UnitType.Player.ToString());
        }

        protected override void OnTargetEnter()
        {
            GameObject eventSystem = GameObjectTreat.GetEventSystem();
            if (eventSystem == null)
            {
                Debug.LogWarning("[EarthquakeTriggerHandler] EventSystem not found");
                return;
            }

            Earthquake earthquake = GameObjectTreat.GetOrAddComponent<Earthquake>(eventSystem);
            if (earthquake == null)
            {
                Debug.LogWarning("[EarthquakeTriggerHandler] Earthquake component not found");
                return;
            }

            earthquake.EventEarthQuake(DEFAULT_MAGNITUDE);
        }

        protected override void OnTargetExit()
        {
            // 何もしない
        }
    }
}
