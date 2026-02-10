using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 番兵ガード（SentryGuard）が EnemyLitters を検出した時のハンドラー
    /// 
    /// 監視対象：EnemyLitters
    /// 
    /// 責務：
    /// - EnemyLitters 進入時に ChangeHeadColor(+1) を実行
    /// - EnemyLitters 離脱時に ChangeHeadColor(-1) を実行
    /// 
    /// 使用例：SentryGuardCtrl GameObject に Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// </summary>
    internal class SentryGuardTriggerHandler : MonoBehaviour
    {
        private SentryGuardCtrl _sentryGuardCtrl = null;
        private string _targetTagString = string.Empty;

        private void Awake()
        {
            _sentryGuardCtrl = GetComponent<SentryGuardCtrl>();
            if (_sentryGuardCtrl == null)
            {
                Debug.LogWarning("[SentryGuardTriggerHandler] Failed to get SentryGuardCtrl component");
            }

            _targetTagString = GameEnum.TagType.EnemyLitters.ToString();
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

            _sentryGuardCtrl.OnEnemyLitterEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == null || string.IsNullOrEmpty(_targetTagString))
            {
                return;
            }

            if (!other.CompareTag(_targetTagString))
            {
                return;
            }

            _sentryGuardCtrl.OnEnemyLitterExit(other);
        }
    }
}
