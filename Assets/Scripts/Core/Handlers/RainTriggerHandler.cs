using UnityEngine;
using CommonsUtility;
using Debug = UnityEngine.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// トリガーコライダーで検出した Player との接触時に、
    /// 天候を切り替えるハンドラー
    /// 
    /// 使用例：SimpleSwichBox などに Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// </summary>
    public class RainTriggerHandler : TriggerHandler
    {
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
                Debug.LogWarning("[RainTriggerHandler] EventSystem not found");
                return;
            }

            WeatherController controller = GameObjectTreat.GetOrAddComponent<WeatherController>(eventSystem);
            if (controller == null)
            {
                Debug.LogWarning("[RainTriggerHandler] WeatherController not found");
                return;
            }

            float toggleRainStrength = controller.GetToggleRainStrength();
            controller.ChangeWeather(toggleRainStrength);
        }

        protected override void OnTargetExit()
        {
            // 何もしない
        }
    }
}
