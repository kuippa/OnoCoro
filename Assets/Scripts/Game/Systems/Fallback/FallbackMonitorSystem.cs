using UnityEngine;
using CommonsUtility;
using StarterAssets;
using Debug = CommonsUtility.Debug;

/// <summary>
/// フォールバック監視システム
/// プレイヤーが異常に低い Y 座標に到達した場合のセーフティネット
/// 高速 GameSpeed でトリガーをすり抜ける落下に対応
/// </summary>
public class FallbackMonitorSystem : MonoBehaviour
{
    private const float _FALLBACK_Y_THRESHOLD = -1000f;  // この Y 座標以下で自動復帰
    private const float _POPUP_DISTANCE = 50f;            // 復帰時の上げ幅
    
    private GameObject _playerArmature = null;
    private InputController _inputController = null;
    private bool _hasFallbackTriggered = false;

    private void Start()
    {
        _playerArmature = GameObject.Find(GameEnum.GameObjectNames.PLAYER_ARMATURE);
        if (_playerArmature == null)
        {
            Debug.LogWarning("[FallbackMonitorSystem] PlayerArmature が見つかりません");
        }

        _inputController = _playerArmature?.GetComponent<InputController>();
        if (_inputController == null)
        {
            Debug.LogWarning("[FallbackMonitorSystem] InputController が見つかりません");
        }
    }

    private void Update()
    {
        if (_playerArmature == null || _inputController == null)
        {
            return;
        }

        float playerY = _playerArmature.transform.position.y;

        // Y座標が閾値以下 → セーフティネット発動
        if (playerY < _FALLBACK_Y_THRESHOLD && !_hasFallbackTriggered)
        {
            OnPlayerExcessiveFall();
        }

        // プレイヤーが復帰したら reset フラグをリセット
        if (playerY >= _FALLBACK_Y_THRESHOLD && _hasFallbackTriggered)
        {
            _hasFallbackTriggered = false;
            Debug.Log("[FallbackMonitorSystem] フラグをリセット（プレイヤーが安全な高さに戻りました）");
        }
    }

    /// <summary>
    /// プレイヤーが異常に低い位置に落ちた場合のフォールバック処理
    /// </summary>
    private void OnPlayerExcessiveFall()
    {
        if (_playerArmature == null || _inputController == null)
        {
            return;
        }

        _hasFallbackTriggered = true;

        float currentY = _playerArmature.transform.position.y;
        Debug.LogWarning($"[FallbackMonitorSystem] プレイヤーが異常な低さに落下しました (Y={currentY:F1})。セーフティネットを発動します。");

        // Rigidbody をリセット
        ResetPlayerRigidbody();

        // プレイヤーの速度をリセット
        _inputController.ResetVelocity();

        // DEM 安全位置またはスポーン地点へ移動
        Vector3 recoveryPosition = GetRecoveryPosition();
        recoveryPosition.y += _POPUP_DISTANCE;

        Debug.Log($"[FallbackMonitorSystem] プレイヤーを Y+{_POPUP_DISTANCE} で復帰: {recoveryPosition}");
        _inputController.CharacterMoveToPosition(recoveryPosition);
    }

    /// <summary>
    /// プレイヤーを配置する安全な位置を取得
    /// </summary>
    private Vector3 GetRecoveryPosition()
    {
        if (_playerArmature == null)
        {
            return Vector3.zero;
        }

        Collider playerCollider = _playerArmature.GetComponent<Collider>();
        if (playerCollider != null)
        {
            // DEM で安全位置を取得
            Vector3 demPosition = DemController.GetClosestPointOnBounds(playerCollider, out bool succeeded);
            if (succeeded && demPosition != Vector3.zero)
            {
                return demPosition;
            }
        }

        // DEM 失敗時はセンター位置へ
        Vector3 demCenterPosition = DemController.GetDemCenterSafePosition(2f);
        if (demCenterPosition != Vector3.zero)
        {
            return demCenterPosition;
        }

        // 最終フォールバック：プレイヤーの現在位置（Y のみリセット）
        Vector3 currentPos = _playerArmature.transform.position;
        currentPos.y = 50f;  // 安全な高さ
        return currentPos;
    }

    /// <summary>
    /// プレイヤーの Rigidbody をリセット
    /// </summary>
    private void ResetPlayerRigidbody()
    {
        if (_playerArmature == null)
        {
            return;
        }

        Rigidbody rb = _playerArmature.GetComponent<Rigidbody>();
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.useGravity = true;

        Debug.Log("[FallbackMonitorSystem] プレイヤーの Rigidbody をリセット");
    }
}
