using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// ゴミキューブの生成を専門に扱う Factory クラス
/// サイズ設定、スケール計算、コンポーネント設定などを一元管理します
/// </summary>
public static class GarbageCubeFactory
{
    // サイズフラグ定数
    internal const int _SIZE_NORMAL = 0;
    internal const int _SIZE_SMALL = 1;
    internal const int _SIZE_BIG = 2;

    // サイズ定数
    private const float _GARBAGE_CUBE_SIZE = 0.3f;
    private const float _GARBAGE_CUBE_SIZE_SMALL = 0.08f;
    private const float _GARBAGE_CUBE_SIZE_BIG_MIN = 1.5f;
    internal const float _GARBAGE_CUBE_SIZE_BIG_MAX = 3.0f;

    // スポーン設定
    private const float _SPREAD_RADIUS = 2.0f;
    private const string _GARBAGE_CUBE_PARENT_NAME = "GarbageCubes";
    private static GameObject _parent_holder;

    /// <summary>
    /// 即座にゴミキューブをスポーンします（同期版）
    /// </summary>
    internal static GameObject SpawnGarbageCube(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
    {
        // PrefabManagerから取得
        GameObject prefab = PrefabManager.GarbageCubePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("GarbageCube prefab not found in PrefabManager");
            return null;
        }

        // [修正 2026-08-28] ここで prefab.transform.localScale を書き換えると、
        // Resources.Load が返す「プレファブ資産そのもの」を毎回書き換えてしまい、
        // ランダムなスケールがアセットに焼き付いて GarbageCube.prefab が破損していた
        // （git 上でも毎回 M 表示になっていた）。スケールは生成後のインスタンスに設定する
        Vector3 setPoint = spawnPoint;
        Quaternion setRotation = Quaternion.identity;
        
        if (isSwayingPoint)
        {
            setPoint.x += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setPoint.z += Utility.fRandomRange(-1 * _SPREAD_RADIUS, _SPREAD_RADIUS);
            setRotation = Quaternion.Euler(
                Utility.fRandomRange(0, 360),
                Utility.fRandomRange(0, 360),
                Utility.fRandomRange(0, 360)
            );
        }

        GameObject unit = Object.Instantiate(prefab, setPoint, setRotation);

        // スケールはインスタンスに設定する（プレファブ資産を汚さない）
        unit.transform.localScale = GetLocalScale(sizeFlag);

        // プロパティ設定
        SetGarbageCubeProperties(unit);

        // 物理設定
        SetGarbageCubeRb(unit);

        return unit;
    }

    /// <summary>
    /// サイズフラグに基づくローカルスケールを計算します
    /// （不燃ゴミ側とサイズを揃えるため GarbageCubeNoBurnFactory からも使う）
    /// </summary>
    internal static Vector3 GetLocalScale(int sizeFlag)
    {
        Vector3 localScale = Vector3.zero;
        
        if (sizeFlag == _SIZE_SMALL)
        {
            localScale = new Vector3(
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_SMALL, _GARBAGE_CUBE_SIZE),
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_SMALL, _GARBAGE_CUBE_SIZE),
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_SMALL, _GARBAGE_CUBE_SIZE)
            );
        }
        else if (sizeFlag == _SIZE_BIG)
        {
            localScale = new Vector3(
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_BIG_MIN, _GARBAGE_CUBE_SIZE_BIG_MAX),
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_BIG_MIN, _GARBAGE_CUBE_SIZE_BIG_MAX),
                Utility.fRandomRange(_GARBAGE_CUBE_SIZE_BIG_MIN, _GARBAGE_CUBE_SIZE_BIG_MAX)
            );
        }
        else
        {
            localScale = new Vector3(_GARBAGE_CUBE_SIZE, _GARBAGE_CUBE_SIZE, _GARBAGE_CUBE_SIZE);
        }
        
        return localScale;
    }

    /// <summary>
    /// ゴミキューブのプロパティを設定します
    /// </summary>
    private static void SetGarbageCubeProperties(GameObject unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("[GarbageCubeFactory] unit is null");
            return;
        }

        unit.tag = GameEnum.TagType.Garbage.ToString();

        // UID を取得
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

        // GarbageCube コンポーネント設定
        GarbageCube garbageCube = unit.GetComponent<GarbageCube>();
        if (garbageCube == null)
        {
            garbageCube = unit.AddComponent<GarbageCube>();
        }
        garbageCube._item_struct.ItemID = unit.name;
        garbageCube._unit_struct.UnitID = unit.name;

        // 親オブジェクト設定
        Transform parentTransform = GameObjectTreat.GetHolderParentTransform(
            ref _parent_holder, 
            _GARBAGE_CUBE_PARENT_NAME
        );
        unit.transform.SetParent(parentTransform);
    }

    /// <summary>
    /// ゴミキューブの Rigidbody と Collider を設定します
    /// </summary>
    private static void SetGarbageCubeRb(GameObject unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("[GarbageCubeFactory] unit is null");
            return;
        }

        Rigidbody rb = unit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = unit.AddComponent<Rigidbody>();
        }

        // 高速移動時の貫通を防止
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Collider がない場合は追加
        if (unit.GetComponent<Collider>() == null)
        {
            unit.AddComponent<BoxCollider>();
        }
    }
}
