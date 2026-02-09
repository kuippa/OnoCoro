using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// トリガーコライダーの Enter/Exit を安全に処理するベースクラス
    /// 複数 Collider からの重複発火を防止し、冪等性を保証します
    /// 
    /// 使用例：Player タグのみを対象にする場合
    /// public class PlayerSensorCtrl : TriggerHandler
    /// {
    ///     protected override void OnTargetEnter() { /* 処理 */ }
    ///     protected override void OnTargetExit() { /* 処理 */ }
    /// }
    /// 
    /// 実行時にタグを変更：
    /// var handler = GetComponent&lt;TriggerHandler&gt;();
    /// handler.SetTargetTag(GameEnum.UnitType.Player.ToString());
    /// </summary>
    public abstract class TriggerHandler : MonoBehaviour
    {
    private HashSet<Collider> _objectsInTrigger = new HashSet<Collider>();
        private Collider _triggerCollider = null;
        private Coroutine _delayedExitCoroutine = null;
        private string _targetTag = string.Empty;
        private float _delayedExitTime = DEFAULT_DELAYED_EXIT_TIME;

        protected const float DEFAULT_DELAYED_EXIT_TIME = 0.05f;

        /// <summary>
        /// 対象タグを実行時に設定します
        /// </summary>
        /// <param name="tag">監視対象のタグ名</param>
        public void SetTargetTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                Debug.LogWarning($"[{nameof(TriggerHandler)}] Target tag cannot be null or empty");
                return;
            }

            _targetTag = tag;
        }

        /// <summary>
        /// 現在の対象タグを取得します
        /// </summary>
        /// <returns>監視中のタグ名</returns>
        public string GetTargetTag()
        {
            return _targetTag;
        }

        /// <summary>
        /// 初期対象タグを設定します。子クラスの Awake で呼び出してください
        /// </summary>
        /// <param name="defaultTag">デフォルトの対象タグ</param>
        protected void SetDefaultTargetTag(string defaultTag)
        {
            if (string.IsNullOrEmpty(defaultTag))
            {
                Debug.LogWarning($"[{nameof(TriggerHandler)}] Default tag cannot be null or empty");
                return;
            }

            _targetTag = defaultTag;
        }

        /// <summary>
        /// 離脱時の遅延実行時間を設定します
        /// </summary>
        /// <param name="delayTime">遅延時間（秒）</param>
        public void SetDelayedExitTime(float delayTime)
        {
            if (delayTime < 0f)
            {
                Debug.LogWarning($"[{nameof(TriggerHandler)}] Delay time cannot be negative");
                return;
            }

            _delayedExitTime = delayTime;
        }

        /// <summary>
        /// 現在設定されている離脱時の遅延実行時間を取得します
        /// </summary>
        /// <returns>遅延時間（秒）</returns>
        public float GetDelayedExitTime()
        {
            return _delayedExitTime;
        }

        /// <summary>
        /// 遅延実行時間（秒）。オーバーライドで変更可能
        /// </summary>
        protected virtual float DelayedExitDelay => _delayedExitTime;

        /// <summary>
        /// 対象オブジェクトが最初に進入した時のみ呼ばれます
        /// 複数 Collider がある場合は最初の1回だけ実行されることを保証します
        /// </summary>
        protected abstract void OnTargetEnter();

        /// <summary>
        /// 対象オブジェクトがすべて離脱した時のみ呼ばれます
        /// </summary>
        protected abstract void OnTargetExit();

        protected virtual void Awake()
        {
            _triggerCollider = GetComponent<Collider>();
            
            if (_triggerCollider == null)
            {
                // Collider がない場合も Update は動作させる
                // （EventSystem にアタッチされて Update 機能だけを使う場合など）
                return;
            }

            if (!_triggerCollider.isTrigger)
            {
                Debug.LogWarning($"[{nameof(TriggerHandler)}] Collider is not a trigger on {gameObject.name}");
            }

            if (string.IsNullOrEmpty(_targetTag))
            {
                Debug.LogWarning($"[{nameof(TriggerHandler)}] Target tag not set on {gameObject.name}. Call SetDefaultTargetTag() in child Awake()");
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other == null || string.IsNullOrEmpty(_targetTag))
            {
                return;
            }

            if (!other.CompareTag(_targetTag))
            {
                return;
            }

            CancelDelayedExit();

            bool isFirstObject = _objectsInTrigger.Count == 0;
            _objectsInTrigger.Add(other);

            if (isFirstObject)
            {
                OnTargetEnter();
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other == null || string.IsNullOrEmpty(_targetTag))
            {
                return;
            }

            if (!other.CompareTag(_targetTag))
            {
                return;
            }

            _objectsInTrigger.Remove(other);

            if (_objectsInTrigger.Count == 0)
            {
                StartDelayedExit();
            }
        }

        private void CancelDelayedExit()
        {
            if (_delayedExitCoroutine != null)
            {
                StopCoroutine(_delayedExitCoroutine);
                _delayedExitCoroutine = null;
            }
        }

        private void StartDelayedExit()
        {
            CancelDelayedExit();
            _delayedExitCoroutine = StartCoroutine(DelayedExit());
        }

        private System.Collections.IEnumerator DelayedExit()
        {
            yield return new WaitForSeconds(DelayedExitDelay);
            OnTargetExit();
            _objectsInTrigger.Clear();
            _delayedExitCoroutine = null;
        }
    }
}
