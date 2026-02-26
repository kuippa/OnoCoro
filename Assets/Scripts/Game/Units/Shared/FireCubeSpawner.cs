using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// FireCube の非同期スポーン管理を担当するクラス
/// 大量生成時にフレーム分散させてパフォーマンス低下を防ぎます
/// </summary>
public class FireCubeSpawner : MonoBehaviour
{
    // 非同期スポーン用の内部構造体
    private struct SpawnRequest
    {
        public Vector3 Position;
        public int SizeFlag;
        public bool IsSwayingPoint;

        public SpawnRequest(Vector3 pos, int size, bool sway)
        {
            Position = pos;
            SizeFlag = size;
            IsSwayingPoint = sway;
        }
    }

    private const int _MAX_CREATE_PER_FRAME = 20;  // 1フレームあたりの最大生成数

    // 非同期スポーン用のキューとフラグ
    private Queue<SpawnRequest> _spawnQueue = new Queue<SpawnRequest>();
    private bool _isProcessingQueue;

    /// <summary>
    /// 非同期で炎キューブをスポーンします（キューに追加）
    /// UnitFireDisasterなど大量生成時に使用
    /// </summary>
    internal void SpawnFireCubeAsync(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
    {
        _spawnQueue.Enqueue(new SpawnRequest(spawnPoint, sizeFlag, isSwayingPoint));
        if (!_isProcessingQueue)
        {
            StartCoroutine(ProcessSpawnQueue());
        }
    }

    /// <summary>
    /// スポーンキューを処理します（フレーム分散）
    /// </summary>
    private IEnumerator ProcessSpawnQueue()
    {
        if (_isProcessingQueue)
        {
            yield break;
        }
        _isProcessingQueue = true;
        WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        while (_spawnQueue.Count > 0)
        {
            // 1フレームで最大_MAX_CREATE_PER_FRAME個まで生成
            for (int i = 0; i < _MAX_CREATE_PER_FRAME; i++)
            {
                if (_spawnQueue.Count <= 0)
                {
                    break;
                }
                SpawnRequest request = _spawnQueue.Dequeue();
                FireCubeFactory.SpawnFireCube(request.Position, request.SizeFlag, request.IsSwayingPoint);
            }
            yield return waitForEndOfFrame;
        }
        _isProcessingQueue = false;
    }

    /// <summary>
    /// 初期化時にキュー処理を開始
    /// </summary>
    private void Awake()
    {
        // キュー処理コルーチンを待機状態で開始
        StartCoroutine(ProcessSpawnQueue());
    }
}
