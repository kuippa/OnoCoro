using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// PowerCube の生成を専門に扱う Factory クラス
/// スポーン時のサイズ調整、BaseScore 設定などを一元管理します
/// </summary>
public static class PowerCubeFactory
{
    private const float _BASE_SCALE = 1.5f;
    private const float _SIZE_MULTIPLIER_MIN = 0.2f;    // 地面の起伏がある場所ではわずかに埋まることがあるので少し上
    private const float _SIZE_MULTIPLIER_MAX = 10f;
    private const float _SPAWN_HEIGHT_BUFFER = 0.1f;  // スポーン時のバッファー（地面からの高さ）

    /// <summary>
    /// PowerCube を生成します
    /// baseScore が指定されている場合、キューブサイズとスコアを調整します
    /// baseScore = -1f の場合はデフォルト (1000) を使用
    /// </summary>
    internal static GameObject SpawnPowerCube(Vector3 spawnPoint, float baseScore = -1f)
    {
        GameObject prefab = PrefabManager.PowerCubePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("PowerCube prefab not found in PrefabManager");
            return null;
        }

        // BaseScore に基づいてスケールを計算
        Vector3 scale = CalculateScale(baseScore);
        prefab.transform.localScale = scale;

        // ランダム回転を生成
        Quaternion spawnRotation = Quaternion.Euler(
            Utility.fRandomRange(0, 360),
            Utility.fRandomRange(0, 360),
            Utility.fRandomRange(0, 360)
        );

        // スポーン位置の Y を調整（回転を考慮）
        Vector3 adjustedSpawnPoint = spawnPoint;
        adjustedSpawnPoint.y += CalculateHeightOffset(scale, spawnRotation);

        // プレハブをインスタンス化
        GameObject instance = Object.Instantiate(prefab, adjustedSpawnPoint, spawnRotation);

        // コンポーネント設定
        SetPowerCubeProperties(instance, baseScore);

        return instance;
    }

    /// <summary>
    /// 回転を考慮した Y 座標オフセットを計算します
    /// 立方体の半サイズと上方向ベクトルの内積で最上部 Y を求めます
    /// </summary>
    private static float CalculateHeightOffset(Vector3 scale, Quaternion rotation)
    {
        // 回転後の上方向ベクトルを計算
        Vector3 upDir = rotation * Vector3.up;

        // 立方体の半サイズと上方向ベクトルの内積（絶対値を使用）
        // これにより、回転した立方体の最上部までの距離を正確に計算
        float maxYOffset = (scale.x / 2f) * Mathf.Abs(upDir.x) +
                           (scale.y / 2f) * Mathf.Abs(upDir.y) +
                           (scale.z / 2f) * Mathf.Abs(upDir.z);

        return maxYOffset + _SPAWN_HEIGHT_BUFFER;
    }

    /// <summary>
    /// BaseScore に基づくスケールを計算します
    /// </summary>
    private static Vector3 CalculateScale(float baseScore)
    {
        if (baseScore <= 0)
        {
            // デフォルトスケール
            return new Vector3(_BASE_SCALE, _BASE_SCALE, _BASE_SCALE);
        }

        // BaseScore に応じてサイズを調整（デフォルト 1000 を基準）
        float sizeMultiplier = baseScore / PowerCube._DEFAULT_BASE_SCORE;
        float clampedMultiplier = Mathf.Clamp(sizeMultiplier, _SIZE_MULTIPLIER_MIN, _SIZE_MULTIPLIER_MAX);
        float adjustedScale = _BASE_SCALE * clampedMultiplier;

        return new Vector3(adjustedScale, adjustedScale, adjustedScale);
    }

    /// <summary>
    /// PowerCube インスタンスのプロパティを設定します
    /// </summary>
    private static void SetPowerCubeProperties(GameObject instance, float baseScore)
    {
        if (instance == null)
        {
            Debug.LogWarning("[PowerCubeFactory] instance is null");
            return;
        }

        // UID と名前を設定
        int idx = PrefabManager.PowerCubeUID;
        instance.name = GameEnum.ModelsType.PowerCube.ToString() + idx.ToString();

        // PowerCube コンポーネントを取得または追加
        PowerCube powerCube = GameObjectTreat.GetOrAddComponent<PowerCube>(instance);
        if (powerCube == null)
        {
            Debug.LogWarning("[PowerCubeFactory] Failed to add PowerCube component");
            return;
        }

        // ItemStruct と UnitStruct の ID を設定
        powerCube._item_struct.ItemID = instance.name;
        powerCube._unit_struct.UnitID = instance.name;

        // BaseScore が指定されている場合は更新
        if (baseScore > 0)
        {
            powerCube.SetBaseScore(baseScore);
        }
    }
}
