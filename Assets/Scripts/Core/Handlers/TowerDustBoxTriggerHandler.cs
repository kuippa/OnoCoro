using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// ダストボックスタワー（DustBox）が EnemyLitters を検出した時のハンドラー
    /// 
    /// 監視対象：EnemyLitters
    /// 
    /// 責務：
    /// - EnemyLitters 進入時に最初の1回だけ OnEnemyLitterEnterFirstTime を呼ぶ
    /// - EnemyLitters 離脱時に最後の離脱時のみ OnEnemyLitterExitAllLeft を呼ぶ
    /// 
    /// TowerDustBoxCtrl は個々のコライダー追跡も並行して実施
    /// </summary>
    internal class TowerDustBoxTriggerHandler : MonoBehaviour
    {
        private TowerDustBoxCtrl _towerDustBoxCtrl = null;
        private string _targetTagString = string.Empty;
        private bool _hasEnteredOnce = false;
        private int _currentEnemyLitterCount = 0;

        private void Awake()
        {
            _towerDustBoxCtrl = GetComponent<TowerDustBoxCtrl>();
            if (_towerDustBoxCtrl == null)
            {
                Debug.LogWarning("[TowerDustBoxTriggerHandler] Failed to get TowerDustBoxCtrl component");
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

            _currentEnemyLitterCount++;

            // TowerDustBoxCtrl で個々のコライダー追跡を行う
            _towerDustBoxCtrl.TrackEnemyLitterEnter(other);

            // 最初の進入時のみ
            if (!_hasEnteredOnce)
            {
                _hasEnteredOnce = true;
                _towerDustBoxCtrl.OnEnemyLitterEnterFirstTime();
            }
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

            _currentEnemyLitterCount--;

            // TowerDustBoxCtrl で個々のコライダー追跡を行う
            _towerDustBoxCtrl.TrackEnemyLitterExit(other);

            // 最後の離脱判定（すべて離脱した時）
            if (_currentEnemyLitterCount <= 0)
            {
                _currentEnemyLitterCount = 0;
                _hasEnteredOnce = false;
                _towerDustBoxCtrl.OnEnemyLitterExitAllLeft();
            }
        }
    }
}
