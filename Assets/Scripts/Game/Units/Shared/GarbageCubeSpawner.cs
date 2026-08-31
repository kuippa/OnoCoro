using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// ゴミキューブの非同期スポーン管理を専門に扱う MonoBehaviour
/// GameManager にアタッチされ、フレーム分散でキューブを生成します
/// </summary>
public class GarbageCubeSpawner : MonoBehaviour
{
    // 非同期スポーン用の内部構造体
    private struct SpawnRequest
    {
        public Vector3 Position;
        public int SizeFlag;
        public bool IsSwayingPoint;
        public bool IsNoBurn;
        public bool IsBurst;

        public SpawnRequest(Vector3 pos, int size, bool sway, bool noBurn, bool burst)
        {
            Position = pos;
            SizeFlag = size;
            IsSwayingPoint = sway;
            IsNoBurn = noBurn;
            IsBurst = burst;
        }
    }

    private const int _MAX_CREATE_PER_FRAME = 200;  // 1フレームあたりの最大生成数

    // 非同期スポーン用のキューとフラグ
    private Queue<SpawnRequest> _spawnQueue = new Queue<SpawnRequest>();
    private bool _isProcessingQueue;

    /// <summary>
    /// 非同期でゴミキューブをスポーンします（キューに追加）
    /// UnitFireDisaster など大量生成時に使用
    /// </summary>
    internal void SpawnGarbageCubeAsync(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false, bool isNoBurn = false, bool isBurst = false)
    {
        _spawnQueue.Enqueue(new SpawnRequest(spawnPoint, sizeFlag, isSwayingPoint, isNoBurn, isBurst));
        
        if (!_isProcessingQueue)
        {
            StartCoroutine(ProcessSpawnQueue());
        }
    }

    /// <summary>
    /// スポーンキューを処理します（フレーム分散）
    /// 1フレームあたり最大 _MAX_CREATE_PER_FRAME 個の キューブ を生成
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
            // 1フレームで最大 _MAX_CREATE_PER_FRAME 個まで生成
            for (int i = 0; i < _MAX_CREATE_PER_FRAME; i++)
            {
                if (_spawnQueue.Count <= 0)
                {
                    break;
                }

                SpawnRequest request = _spawnQueue.Dequeue();
                GameObject spawned;
                if (request.IsNoBurn)
                {
                    spawned = GarbageCubeNoBurnFactory.SpawnGarbageCube(
                        request.Position,
                        request.SizeFlag,
                        request.IsSwayingPoint
                    );
                }
                else
                {
                    spawned = GarbageCubeFactory.SpawnGarbageCube(
                        request.Position,
                        request.SizeFlag,
                        request.IsSwayingPoint
                    );
                }

                // 解体由来の瓦礫だけ外向きに弾き飛ばす（通常のゴミ生成はその場に落とす）
                if (request.IsBurst)
                {
                    GarbageCubeFactory.ApplyBurstImpulse(spawned);
                }
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
