using System;
using UnityEngine;

/// <summary>
/// 地震イベントを管理するクラス
///
/// [Season 3 W2 Task 1 で方式変更]
/// 旧実装: DEM・建物の transform を上下にテレポート変位させて地震を表現
///   → 上に載った物理オブジェクト（FireCube 等）やプレイヤーが地面をすり抜け、
///     奈落トラップで消去される問題があった（BUG-S3-002）
/// 新実装: 揺れオフセット値の計算のみを行い、カメラへの適用は
///   CameraShakeController（Presentation 層）が本クラスの静的プロパティを参照して行う。
///   ワールド（DEM・建物・オブジェクト）は一切動かさない。
/// </summary>
public class Earthquake : MonoBehaviour
{
    private float _time = 0.0f;
    private float _time_duration = 0.0f;
    private float _degree = 0.0f;
    private bool _is_earthquake = false;
    private float _total_duration = 3.5f;
    private float _interval = 0.02f;
    private float _magnitude = 1.0f;

    private const float _DURATION = 4.5f;
    private const float _INTERVAL = 0.02f;
    private const float _DEGREE_STEP = 30.0f;

    /// <summary>
    /// 現在の揺れ縦オフセット（CameraShakeController が毎フレーム参照）
    /// 揺れていないときは 0
    /// </summary>
    internal static float CurrentVerticalOffset { get; private set; } = 0f;

    /// <summary>
    /// 揺れ進行中かどうか（CameraShakeController が参照）
    /// </summary>
    internal static bool IsShaking { get; private set; } = false;

    /// <summary>
    /// 地震イベントを開始する
    /// </summary>
    /// <param name="magnitude">マグニチュード(振幅の大きさ)</param>
    /// <param name="duration">地震の継続時間(秒)</param>
    /// <param name="interval">振動の更新間隔(秒)</param>
    internal void EventEarthQuake(float magnitude, float duration = _DURATION, float interval = _INTERVAL)
    {
        _is_earthquake = true;
        _interval = interval;
        _total_duration = duration;
        _magnitude = magnitude;
        _degree = 0.0f;
        IsShaking = true;
    }

    /// <summary>
    /// P波(縦波)による揺れオフセットを更新
    /// 旧実装の DEM 変位と同じ波形（減衰付き Sin）をカメラ用オフセットとして公開する
    /// </summary>
    private void UpdateShakeOffset()
    {
        if (_total_duration > _time_duration)
        {
            CurrentVerticalOffset = CalcSin();
            _degree += _DEGREE_STEP;
        }
        else
        {
            EndShake();
        }
    }

    /// <summary>
    /// 揺れを終了し状態をリセット
    /// </summary>
    private void EndShake()
    {
        CurrentVerticalOffset = 0f;
        IsShaking = false;
        _degree = 0.0f;
        _time_duration = 0.0f;
        _is_earthquake = false;
    }

    /// <summary>
    /// Sin波を計算して振幅を返す
    /// </summary>
    /// <returns>減衰を考慮した振幅値</returns>
    private float CalcSin()
    {
        float ret = 0.0f;
        float val = 0.0f;
        val = (float)(_degree * Math.PI / 180.0f);
        ret = (float)Math.Sin(val);
        ret = CalcAmpDecay() * ret;
        return ret;
    }

    /// <summary>
    /// 振幅の減衰を計算
    /// 地震開始時と終了時で振幅が小さくなるように計算
    /// </summary>
    /// <returns>減衰係数</returns>
    private float CalcAmpDecay()
    {
        float ret = 0.0f;
        float sign = 1f;
        if (_time_duration > _total_duration / 2)
        {
            sign = -1f;
        }
        if (_time_duration != 0)
        {
            ret = sign * _magnitude * (float)Math.Pow(_time_duration / _total_duration, 2);
        }
        return ret;
    }

    private void Update()
    {
        if (_is_earthquake)
        {
            _time += Time.deltaTime;
            _time_duration += Time.deltaTime;
            if (_time > _interval)
            {
                _time = 0.0f;
                UpdateShakeOffset();
            }
        }
    }

    private void OnDestroy()
    {
        // シーン遷移中に揺れが残留しないよう静的状態をリセット
        CurrentVerticalOffset = 0f;
        IsShaking = false;
    }
}
