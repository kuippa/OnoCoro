using System.Collections;
using UnityEngine;

public class TowerSentryGuardCtrl : MonoBehaviour
{
    // Swing Constants
    private const float _SWING_INTERVAL = 0.5f;
    private const float _SWING_MAX_ANGLE = 20f;
    private const float _SWING_ANGLE_RATE = 2f;

    // Private Fields
    private bool _swing_mode = true;
    private bool _swing_direction = true;
    private float _swing_angle = 0f;
    private float _object_angle = 0f;

    void Start()
    {
        if (_swing_mode)
        {
            StartCoroutine(SwingMonitor());
        }
    }

    private IEnumerator SwingMonitor()
    {
        // 初期角度を記録
        _object_angle = this.gameObject.transform.eulerAngles.y;
        
        while (_swing_mode)
        {
            yield return new WaitForSeconds(_SWING_INTERVAL);
            SwingGameObject();
        }
    }

    private void SwingGameObject()
    {
        // 振り角度を更新
        if (_swing_direction)
        {
            _swing_angle += _SWING_ANGLE_RATE;
        }
        else
        {
            _swing_angle -= _SWING_ANGLE_RATE;
        }
        
        // 角度を適用
        Vector3 eulerAngles = this.gameObject.transform.eulerAngles;
        eulerAngles.y = _swing_angle + _object_angle;
        this.gameObject.transform.eulerAngles = eulerAngles;
        
        // 最大角度に達したら方向転換
        if (_swing_angle > _SWING_MAX_ANGLE || _swing_angle < -_SWING_MAX_ANGLE)
        {
            _swing_direction = !_swing_direction;
        }
    }
}
