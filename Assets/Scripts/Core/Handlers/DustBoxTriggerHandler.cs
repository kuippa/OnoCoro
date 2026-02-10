using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// ダストボックス（DustBox）が Garbage を検出した時のハンドラー
    /// 
    /// 監視対象：Garbage
    /// 
    /// 責務：
    /// - Garbage との接触時に遅延削除処理を開始
    /// 
    /// 使用例：DustBoxCtrl GameObject に Collider と共にアタッチ
    /// GameObject の Collider は Is Trigger = true にしてください
    /// </summary>
    internal class DustBoxTriggerHandler : MonoBehaviour
    {
        private DustBoxCtrl _dustBoxCtrl = null;
        private string _targetTagString = string.Empty;

        private void Awake()
        {
            _dustBoxCtrl = GetComponent<DustBoxCtrl>();
            if (_dustBoxCtrl == null)
            {
                Debug.LogWarning("[DustBoxTriggerHandler] Failed to get DustBoxCtrl component");
            }

            _targetTagString = GameEnum.TagType.Garbage.ToString();
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

            _dustBoxCtrl.OnGarbageEnter(other);
        }
    }
}
