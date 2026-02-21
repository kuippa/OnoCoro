using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;
using System.Linq;

/// <summary>
/// 掃除機タワー（Sweeper）のターゲティング管理システム
/// 
/// 責務：
/// - ゴミ検出・ターゲット候補リストの管理
/// - 最近いゴミのソート・距離計算
/// - 無視リスト（到達不可）の管理
/// - デバッグカラー表示
/// 
/// 使用方法：
/// 1. Initialize(transform, _TRIGGER_STAY_INTERVAL) で初期化
/// 2. OnGarbageEnter/OnAshEnter/OnGarbageExit/OnAshExit() をハンドラーから呼び出し
/// 3. GetBestTarget() で次の移動先候補を取得
/// 4. ClearIgnoreList() で無視リストをリセット（バッテリー充電時）
/// </summary>
public class SweeperTargetingService : MonoBehaviour
{
    // デバッグカラー用
    private const float _TRIGGER_STAY_INTERVAL = 0.02f;
    private float _lastTriggerStayTime = 0f;

    // ターゲット管理
    private GameObject _targetGarbage = null;
    private List<GameObject> _AimGarbageLists = new List<GameObject>();
    private List<GameObject> _IgnoreGarbageLists = new List<GameObject>();

    // 外部参照
    private Transform _myTransform = null;

    /// <summary>
    /// ターゲティングサービスを初期化
    /// </summary>
    /// <param name="myTransform">親の Transform（距離計算用）</param>
    internal void Initialize(Transform myTransform)
    {
        if (myTransform == null)
        {
            Debug.LogWarning("[SweeperTargetingService] myTransform is null");
            return;
        }
        _myTransform = myTransform;
    }

    /// <summary>
    /// 現在のターゲットを取得
    /// </summary>
    internal GameObject GetCurrentTarget()
    {
        return _targetGarbage;
    }

    /// <summary>
    /// 最適なターゲットを取得
    /// </summary>
    /// <returns>最近いゴミ。なければ null</returns>
    internal GameObject GetBestTarget()
    {
        UpdateTargetGarbage();
        return _targetGarbage;
    }

    /// <summary>
    /// Garbage タグの Collider が進入した時（ハンドラーから呼び出し）
    /// </summary>
    internal void OnGarbageEnter(Collider other)
    {
        if (other == null || other.gameObject == null)
        {
            return;
        }

        if (!IsRateLimited())
        {
            return;
        }

        SetTargetGarbage(other.gameObject);
    }

    /// <summary>
    /// Ash タグの Collider が進入した時 （ハンドラーから呼び出し）
    /// </summary>
    internal void OnAshEnter(Collider other)
    {
        if (other == null || other.gameObject == null)
        {
            return;
        }

        if (!IsRateLimited())
        {
            return;
        }

        SetTargetGarbage(other.gameObject);
    }

    /// <summary>
    /// Garbage タグの Collider が離脱した時
    /// </summary>
    internal void OnGarbageExit(Collider other)
    {
        // 現在、OnTriggerExit の処理は不要
        if (other == null || other.gameObject == null)
        {
            return;
        }

        if (_AimGarbageLists.Contains(other.gameObject))
        {
            _AimGarbageLists.Remove(other.gameObject);
        }
    }

    /// <summary>
    /// Ash タグの Collider が離脱した時
    /// </summary>
    internal void OnAshExit(Collider other)
    {
        // 現在、OnTriggerExit の処理は不要
        if (other == null || other.gameObject == null)
        {
            return;
        }

        if (_AimGarbageLists.Contains(other.gameObject))
        {
            _AimGarbageLists.Remove(other.gameObject);
        }
    }

    /// <summary>
    /// 無視リストをクリア（バッテリー充電完了時）
    /// </summary>
    internal void ClearIgnoreList()
    {
        foreach (GameObject ignoreGarbage in _IgnoreGarbageLists)
        {
            GameObjectTreat.DebugColorChange(ignoreGarbage, Color.yellow);
        }
        _IgnoreGarbageLists.Clear();
    }

    /// <summary>
    /// ターゲットをリセット
    /// </summary>
    internal void ResetTarget()
    {
        _targetGarbage = null;
    }

    // ===== プライベートメソッド =====

    /// <summary>
    /// レート制限チェック（連続呼び出しを避ける）
    /// </summary>
    private bool IsRateLimited()
    {
        float currentTime = Time.time;
        if (currentTime - _lastTriggerStayTime <= _TRIGGER_STAY_INTERVAL)
        {
            return false;
        }
        _lastTriggerStayTime = currentTime;
        return true;
    }

    /// <summary>
    /// ターゲットリストからゴミを設定
    /// </summary>
    private void SetTargetGarbage(GameObject other)
    {
        if (_myTransform == null)
        {
            Debug.LogWarning("[SweeperTargetingService] myTransform is null");
            return;
        }

        // エリア内にあるゴミをリストに追加
        if (!_AimGarbageLists.Contains(other))
        {
            _AimGarbageLists.Add(other);
        }

        // thisからの距離が近ければ targetGarbage を更新
        if (CompareDistance(other))
        {
            if (_targetGarbage != null)
            {
                GameObjectTreat.DebugColorChange(_targetGarbage, Color.green);
            }
            _targetGarbage = other;
        }
        else
        {
            if (_targetGarbage != null)
            {
                GameObjectTreat.DebugColorChange(_targetGarbage, Color.blue);
            }
        }
    }

    /// <summary>
    /// 与えられた引数の距離を比較する
    /// </summary>
    private bool CompareDistance(GameObject compareObject)
    {
        if (_myTransform == null)
        {
            return false;
        }

        // 無視リストに含まれている場合は、比較しない
        if (_IgnoreGarbageLists.Contains(compareObject))
        {
            return false;
        }

        if (_targetGarbage == null)
        {
            _targetGarbage = compareObject;
            return true;
        }

        float currentDistance = Vector3.Distance(_myTransform.position, _targetGarbage.transform.position);
        float compareDistance = Vector3.Distance(_myTransform.position, compareObject.transform.position);

        return currentDistance >= compareDistance;
    }

    /// <summary>
    /// ターゲットの有効性をチェック
    /// </summary>
    private bool IsValidTarget(GameObject target)
    {
        return target != null && !_IgnoreGarbageLists.Contains(target);
    }

    /// <summary>
    /// リストから最適なターゲットを取得
    /// </summary>
    private GameObject GetBestTargetFromList()
    {
        if (_myTransform == null)
        {
            return null;
        }

        return _AimGarbageLists
            .Where(IsValidTarget)
            .OrderBy(g => Vector3.Distance(_myTransform.position, g.transform.position))
            .FirstOrDefault();
    }

    /// <summary>
    /// ターゲットを更新（無効→最善を選択）
    /// </summary>
    private void UpdateTargetGarbage()
    {
        if (_targetGarbage == null || !IsValidTarget(_targetGarbage))
        {
            _targetGarbage = GetBestTargetFromList();
        }
    }

    /// <summary>
    /// ターゲットを無視リストに移動
    /// </summary>
    internal GameObject IgnoreCurrentTarget()
    {
        if (_targetGarbage == null)
        {
            return null;
        }

        _IgnoreGarbageLists.Add(_targetGarbage);
        GameObjectTreat.DebugColorChange(_targetGarbage, Color.black);
        _targetGarbage = null;
        return null;
    }
}
