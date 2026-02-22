using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;
using UnityEngine.AI;

/// <summary>
/// 掃除機タワー（Sweeper）のバッテリー管理システム
/// 
/// 責務：
/// - バッテリー値の増減管理
/// - Dock（充電ステーション）への移動制御
/// - UI 更新（BatteryView）
/// - ChargeMode/SleepMode の UI 表示切り替え
/// 
/// 使用方法：
/// TowerSweeperCtrl に紐付け、CheckBattery() を Update() で呼び出す
/// </summary>
public class SweeperBatteryManager : MonoBehaviour
{
    // バッテリーパラメータ
    private const float _BATTERY_ORG_SIZE = 0.5f;
    private const float _BATTERY_DISTANCE = 1.8f;

    internal float _FULL_BATTERY = 100f;        // バッテリーの最大容量
    internal float _HP = 100f;                  // ヒットポイント（バッテリー）
    internal float _DECREASE_BATTERY = 1.0f;    // バッテリーの減少量
    internal float _CHARGE_BATTERY = 5f;        // バッテリーの回復量

    // 状態管理
    private bool _chargeMode = false;

    // 外部参照
    private GameObject _MyDeck = null;
    private GameObject _Active_Dock = null;
    private GameObject _Sleep_Dock = null;
    private NavMeshAgent _NavMeshAgent = null;
    private TowerSweeperCtrl _towerSweeperCtrl = null;

    /// <summary>
    /// バッテリーマネージャーを初期化
    /// </summary>
    /// <param name="deck">充電ステーション（Dock）GameObject</param>
    /// <param name="navMeshAgent">タワーの NavMeshAgent</param>
    /// <param name="towerSweeperCtrl">親の TowerSweeperCtrl</param>
    internal void Initialize(GameObject deck, NavMeshAgent navMeshAgent, TowerSweeperCtrl towerSweeperCtrl)
    {
        if (deck == null)
        {
            Debug.LogWarning("[SweeperBatteryManager] Deck is null");
            return;
        }
        if (navMeshAgent == null)
        {
            Debug.LogWarning("[SweeperBatteryManager] NavMeshAgent is null");
            return;
        }
        if (towerSweeperCtrl == null)
        {
            Debug.LogWarning("[SweeperBatteryManager] TowerSweeperCtrl is null");
            return;
        }

        _MyDeck = deck;
        _NavMeshAgent = navMeshAgent;
        _towerSweeperCtrl = towerSweeperCtrl;

        ChangeBatteryDockMode(false);
    }

    /// <summary>
    /// バッテリー状態をチェック（Update 内で呼び出し）
    /// バッテリーが足りなければ充電モードに移行
    /// </summary>
    /// <returns>行動可能な場合 true、充電中は false</returns>
    internal bool CheckBattery()
    {
        if (_HP <= 0 || _chargeMode)
        {
            ChargeBattery();
            return false;
        }
        DecreaseHP();
        return true;
    }

    /// <summary>
    /// バッテリーを時間経過で減らす
    /// </summary>
    private void DecreaseHP()
    {
        _HP -= _DECREASE_BATTERY;
        BatteryView();
    }

    /// <summary>
    /// 充電処理
    /// 1. Dock が遠い場合：Dock に向かって移動
    /// 2. Dock が近い場合：充電実行
    /// </summary>
    private void ChargeBattery()
    {
        if (_MyDeck == null)
        {
            Debug.LogWarning("[SweeperBatteryManager] Deck is null");
            return;
        }

        _chargeMode = true;
        float distance = Vector3.Distance(this.transform.position, _MyDeck.transform.position);

        // battery_bar を取得
        Transform battery_bar_transform = this.transform.Find("battery_bar");
        if (battery_bar_transform == null)
        {
            Debug.LogWarning("[SweeperBatteryManager] battery_bar オブジェクトが見つかりません");
            return;
        }
        GameObject battery_bar = battery_bar_transform.gameObject;

        // Dock が遠い場合：移動
        if (distance > _BATTERY_DISTANCE)
        {
            if (NavMeshManager.GetDestination(_NavMeshAgent) != _MyDeck.transform.position)
            {
                NavMeshManager.SetDestination(_MyDeck.transform.position, _NavMeshAgent);
            }
            GameObjectTreat.ColorChange(battery_bar, Color.red);
        }
        // Dock が近い場合：充電
        else
        {
            NavMeshManager.ClearDestination(_NavMeshAgent);
            if (_towerSweeperCtrl != null)
            {
                _towerSweeperCtrl.ClearIgnoreGarbageLists();
            }

            _HP += _CHARGE_BATTERY;
            BatteryView();
            ChangeBatteryDockMode(true);

            // バッテリー満タン
            if (_HP >= _FULL_BATTERY)
            {
                GameObjectTreat.ColorChange(battery_bar, Color.green);
                _HP = _FULL_BATTERY;
                _chargeMode = false;
                ChangeBatteryDockMode(false);
            }
        }
    }

    /// <summary>
    /// UI 上でバッテリーバーの高さを更新
    /// </summary>
    private void BatteryView()
    {
        Transform battery_bar_transform = this.transform.Find("battery_bar");
        if (battery_bar_transform == null)
        {
            Debug.LogWarning("[SweeperBatteryManager] battery_bar オブジェクトが見つかりません");
            return;
        }
        GameObject battery_bar = battery_bar_transform.gameObject;
        Vector3 battery_bar_scale = battery_bar.transform.localScale;
        battery_bar_scale.y = _BATTERY_ORG_SIZE * _HP / _FULL_BATTERY;
        battery_bar.transform.localScale = battery_bar_scale;
    }

    /// <summary>
    /// Dock の表示/非表示を切り替え
    /// mode = true: 充電中（ChargeMode 表示）
    /// mode = false: 待機中（SleepMode 表示）
    /// </summary>
    private void ChangeBatteryDockMode(bool mode)
    {
        if (_MyDeck == null)
        {
            Debug.LogWarning("[SweeperBatteryManager] Deck is null");
            return;
        }

        // 初回のみ ChargeMode/SleepMode を取得
        if (_Active_Dock == null || _Sleep_Dock == null)
        {
            Transform chargeMode_transform = _MyDeck.transform.Find("ChargeMode");
            if (chargeMode_transform != null)
            {
                _Active_Dock = chargeMode_transform.gameObject;
            }
            else
            {
                Debug.LogWarning("[SweeperBatteryManager] ChargeMode オブジェクトが見つかりません");
                return;
            }

            Transform sleepMode_transform = _MyDeck.transform.Find("SleepMode");
            if (sleepMode_transform != null)
            {
                _Sleep_Dock = sleepMode_transform.gameObject;
            }
            else
            {
                Debug.LogWarning("[SweeperBatteryManager] SleepMode オブジェクトが見つかりません");
                return;
            }
        }

        // UI を切り替え
        if (mode)
        {
            _Active_Dock.SetActive(true);
            _Sleep_Dock.SetActive(false);
        }
        else
        {
            _Active_Dock.SetActive(false);
            _Sleep_Dock.SetActive(true);
        }
    }

    /// <summary>
    /// 充電モード中かどうかを取得
    /// </summary>
    internal bool IsCharging => _chargeMode;
}
