using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// パワーキューブ（PowerCube）が Player を検出した時のハンドラー
    /// 
    /// 監視対象：Player
    /// 
    /// 責務：
    /// - Player との接触時にスコア加算と自身の破棄を実行
    /// 
    /// 使用例：PowerCubeCollisionHandler GameObject に Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// </summary>
    internal class PowerCubeTriggerHandler : MonoBehaviour
    {
        private PowerCubeCollisionHandler _powerCubeCollisionHandler = null;
        private string _targetTagString = string.Empty;

        private void Awake()
        {
            _powerCubeCollisionHandler = GetComponent<PowerCubeCollisionHandler>();
            if (_powerCubeCollisionHandler == null)
            {
                Debug.LogWarning("[PowerCubeTriggerHandler] Failed to get PowerCubeCollisionHandler component");
            }

            _targetTagString = GameEnum.UnitType.Player.ToString();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || string.IsNullOrEmpty(_targetTagString))
            {
                return;
            }

            if (!other.CompareTag(_targetTagString))
            {
                return;
            }

            _powerCubeCollisionHandler.OnPlayerEnter(other);
        }
    }
}
