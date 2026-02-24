using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

internal class LitterPathManager
{
    // Numeric Constants
    private const float _UNDEFINED_POSITION_VALUE = -99f;

    // Internal State
    private Vector3 _undefinedPosition = new Vector3(_UNDEFINED_POSITION_VALUE, _UNDEFINED_POSITION_VALUE, _UNDEFINED_POSITION_VALUE);

    internal LitterPathManager()
    {
    }

    /// <summary>
    /// マーカー名配列からパス配列を生成
    /// マーカーが見つからない場合はスキップされる
    /// </summary>
    internal Vector3[] GeneratePathsFromMarkers(string[] markerNames = null)
    {
        if (markerNames == null || markerNames.Length == 0)
        {
            return GetDefaultPaths();
        }

        return GetCustomPaths(markerNames);
    }

    /// <summary>
    /// デフォルトパスを取得（原点）
    /// </summary>
    private Vector3[] GetDefaultPaths()
    {
        return new Vector3[1] { new Vector3(0f, 0f, 0f) };
        // コメントアウト：
        // return new Vector3[4]
        // {
        //     GetMarkerPositionByName("path_marker_start"),
        //     GetMarkerPositionByName("path_marker_01"),
        //     GetMarkerPositionByName("path_marker_02"),
        //     GetMarkerPositionByName("path_marker_goal")
        // };
    }

    /// <summary>
    /// カスタムパスを生成（マーカー名から位置を解決）
    /// 見つからないマーカーはスキップ
    /// </summary>
    private Vector3[] GetCustomPaths(string[] markerNames)
    {
        List<Vector3> validPaths = new List<Vector3>();

        foreach (string markerName in markerNames)
        {
            Vector3 markerPosition = GetMarkerPositionByName(markerName.Trim());

            if (markerPosition == _undefinedPosition)
            {
                continue;
            }

            validPaths.Add(markerPosition);
        }

        return validPaths.ToArray();
    }

    /// <summary>
    /// マーカー名からワールド座標を取得
    /// 見つからない場合は _undefinedPosition を返す
    /// </summary>
    private Vector3 GetMarkerPositionByName(string markerName)
    {
        if (string.IsNullOrEmpty(markerName))
        {
            Debug.LogWarning("Marker name is null or empty");
            return _undefinedPosition;
        }

        GameObject markerObject = GameObject.Find(markerName);
        if (markerObject == null)
        {
            Debug.LogWarning("GetMarkerPositionByName cannot find: " + markerName);
            return _undefinedPosition;
        }

        return markerObject.transform.position;
    }

    /// <summary>
    /// パスが有効かを判定（null または空配列でないか）
    /// </summary>
    internal bool IsPathValid(Vector3[] paths)
    {
        return paths != null && paths.Length > 0;
    }

    /// <summary>
    /// デバッグ用：パス情報を文字列で取得
    /// </summary>
    internal string GetDebugInfo(Vector3[] paths)
    {
        if (paths == null)
        {
            return "Paths: null";
        }

        return $"Paths: {paths.Length} waypoints";
    }
}
