using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonsUtility;


public class GarbageCubeCtrl : MonoBehaviour
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

    private const int _MAX_CREATE_PER_FRAME = 200;  // 1フレームあたりの最大生成数
    internal const int _SIZE_NORMAL = 0;
    internal const int _SIZE_SMALL = 1;
    internal const int _SIZE_BIG = 2;
    private static GameObject _parent_holder;  // 親オブジェクトのキャッシュ
    private const string _GARBAGE_CUBE_PARENT_NAME = "GarbageCubes";
    private const float _SPREAD_RADIUS = 2.0f;
    private const float _GARBAGE_CUBE_SIZE = 0.3f;
    private const float _GARBAGE_CUBE_SIZE_SMALL = 0.08f;
    private const float _GARBAGE_CUBE_SIZE_BIG_MIN = 1.5f;
    internal const float _GARBAGE_CUBE_SIZE_BIG_MAX = 3.0f;  // お掃除ロボットの吸い込み範囲より大きくしては駄目

    // 非同期スポーン用のキューとフラグ
    private Queue<SpawnRequest> _spawnQueue = new Queue<SpawnRequest>();
    private bool _isProcessingQueue;


    private static Vector3 GetLocalScale(int sizeFlag)
    {
        Vector3 localScale = Vector3.zero;
        if (sizeFlag == _SIZE_SMALL)
        {
            localScale = new Vector3(
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_SMALL, _GARBAGE_CUBE_SIZE)
                , Utility.fRandomRange(_GARBAGE_CUBE_SIZE_SMALL, _GARBAGE_CUBE_SIZE)
                , Utility.fRandomRange(_GARBAGE_CUBE_SIZE_SMALL, _GARBAGE_CUBE_SIZE));
        }
        else if (sizeFlag == _SIZE_BIG)
        {
            localScale = new Vector3(
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_BIG_MIN, _GARBAGE_CUBE_SIZE_BIG_MAX)
                , Utility.fRandomRange(_GARBAGE_CUBE_SIZE_BIG_MIN, _GARBAGE_CUBE_SIZE_BIG_MAX)
                , Utility.fRandomRange(_GARBAGE_CUBE_SIZE_BIG_MIN, _GARBAGE_CUBE_SIZE_BIG_MAX));
        }
        else
        {
            localScale = new Vector3(_GARBAGE_CUBE_SIZE, _GARBAGE_CUBE_SIZE, _GARBAGE_CUBE_SIZE);
        }
        return localScale;
    }

    /// <summary>
    /// 非同期でゴミキューブをスポーンします（キューに追加）
    /// UnitFireDisasterなど大量生成時に使用
    /// </summary>
    internal void SpawnGarbageCubeAsync(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
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
                SpawnGarbageCube(request.Position, request.SizeFlag, request.IsSwayingPoint);
            }
            yield return waitForEndOfFrame;
        }
        _isProcessingQueue = false;
    }

    /// <summary>
    /// 即座にゴミキューブをスポーンします（同期版）
    /// </summary>
    internal static GameObject SpawnGarbageCube(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
    {
        // PrefabManagerが利用可能な場合はそちらを使用、なければResourcesから読み込み
        GameObject prefab = PrefabManager.GarbageCubePrefab;
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/GarbageCube");
        }
        prefab.transform.localScale = GetLocalScale(sizeFlag);
        Vector3 setPoint = spawnPoint;
        Quaternion setRotation = Quaternion.identity;
        if (isSwayingPoint)
        {
            setPoint.x += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setPoint.z += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setRotation = Quaternion.Euler(Utility.fRandomRange(0,360), Utility.fRandomRange(0,360), Utility.fRandomRange(0,360));

        }
        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        
        // プロパティ設定（リファクタリング版と既存版の統合）
        SetGarbageCubeProperties(unit);
        
        // 物理設定（リファクタリング版と既存版の統合）
        SetGarbageCubeRb(unit);

        return unit;
    }

    /// <summary>
    /// ゴミキューブのプロパティを設定します
    /// </summary>
    private static void SetGarbageCubeProperties(GameObject unit)
    {
        unit.tag = GameEnum.TagType.Garbage.ToString();
        
        // PrefabManagerが利用可能な場合はそちらを使用、なければIndexObjectByTagで取得
        int idx;
        if (PrefabManager.GarbageCubePrefab != null)
        {
            idx = PrefabManager.GarbageCubeUID;
        }
        else
        {
            idx = GameObjectTreat.IndexObjectByTag(GameEnum.TagType.Garbage.ToString());
        }
        
        unit.name = GameEnum.ModelsType.GarbageCube.ToString() + idx.ToString();
        
        GarbageCube garbageCube = unit.GetComponent<GarbageCube>();
        if (garbageCube == null)
        {
            garbageCube = unit.AddComponent<GarbageCube>();
        }
        garbageCube._item_struct.ItemID = unit.name;
        garbageCube._unit_struct.UnitID = unit.name;

        // 親オブジェクトの設定（キャッシュを使用）
        Transform parentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_holder, _GARBAGE_CUBE_PARENT_NAME);
        unit.transform.SetParent(parentTransform);
    }

    /// <summary>
    /// ゴミキューブのRigidbodyとColliderを設定します
    /// </summary>
    private static void SetGarbageCubeRb(GameObject unit)
    {
        Rigidbody rb = unit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = unit.AddComponent<Rigidbody>();
        }

        // 物理エンジンがオブジェクトの速度に関係なく、常に他のオブジェクトとの衝突を検出します。
        // これにより、高速で移動するオブジェクトが他のオブジェクトを通り抜けることがなくなります。
        // ただし、この設定はパフォーマンスに影響を与えるため、
        // 物理エンジンが他のオブジェクトとの衝突を検出する必要がない場合は、
        // Discrete または Continuous Speculative に設定することをお勧めします。
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // 以下はコメントアウト（必要に応じて有効化）
        // rb.linearVelocity = Vector3.zero;
        // rb.angularVelocity = Vector3.zero;
        // rb.collisionDetectionMode = CollisionDetectionMode.Continuous; 
        // rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;  // Default
        // rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        // if (GameConfig._APP_GAME_MODE == GlobalConst.GAME_MODE_DEBUG)
        // {
        //     rb.linearDamping = 2;
        // }

        if (unit.GetComponent<Collider>() == null)
        {
            unit.AddComponent<BoxCollider>();
        }
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