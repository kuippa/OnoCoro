using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CommonsUtility
{
    /// <summary>
    /// ゲーム内のタワーと敵のバランスデータを抽出するユーティリティ
    /// プレハブから ItemStruct / UnitStruct を取得して自動抽出
    /// </summary>
    internal static class BalanceDataExtractor
    {
        private const string _PREFAB_PATH_FORMAT = "Assets/Resources/Prefabs/{0}.prefab";

        /// <summary>
        /// すべてのタワー情報をプレハブから自動抽出
        /// </summary>
        internal static List<TowerBalanceData> ExtractAllTowers()
        {
            var towers = new List<TowerBalanceData>();
            var towerNames = new[] { "WaterTurret", "Sweeper", "DustBox", "SentryGuard" };

            foreach (var towerName in towerNames)
            {
                var towerData = ExtractTowerFromPrefab(towerName);
                if (towerData != null)
                {
                    towers.Add(towerData);
                }
            }

            return towers;
        }

        /// <summary>
        /// すべての敵情報をプレハブから自動抽出
        /// </summary>
        internal static List<EnemyBalanceData> ExtractAllEnemies()
        {
            var enemies = new List<EnemyBalanceData>();
            var enemyNames = new[] { "Litter", "FireCube" };

            foreach (var enemyName in enemyNames)
            {
                var enemyData = ExtractEnemyFromPrefab(enemyName);
                if (enemyData != null)
                {
                    enemies.Add(enemyData);
                }
            }

            return enemies;
        }

        /// <summary>
        /// 敵ごとのゴミ処理スコア情報
        /// </summary>
        internal static Dictionary<string, int> GetEnemyGarbageDropScores()
        {
            var scores = new Dictionary<string, int>
            {
                { "Litter", 10 * 20 }  // GarbageCube (score 10) × MAX_GARBAGE_COUNT (20)
            };

            return scores;
        }

        /// <summary>
        /// プレハブからタワー情報を抽出
        /// </summary>
        private static TowerBalanceData ExtractTowerFromPrefab(string towerName)
        {
            try
            {
                var prefabPaths = AssetDatabase.FindAssets(towerName + " t:prefab");
                if (prefabPaths.Length == 0)
                {
                    return CreateDefaultTowerData(towerName);
                }

                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabPaths[0]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null)
                {
                    return CreateDefaultTowerData(towerName);
                }

                // プレハブをインスタンス化して ItemStruct を取得
                var instance = Object.Instantiate(prefab);
                var itemProvider = instance.GetComponent<IItemStructProvider>();
                var unitProvider = instance.GetComponent<IUnitStructProvider>();

                if (itemProvider == null || unitProvider == null)
                {
                    Object.DestroyImmediate(instance);
                    return CreateDefaultTowerData(towerName);
                }

                var itemStruct = itemProvider.ItemStruct;
                var unitStruct = unitProvider.UnitStruct;

                var towerData = new TowerBalanceData
                {
                    TowerName = towerName,
                    CreateCost = itemStruct.CreateCost,
                    CostType = itemStruct.CostType,
                    CostTime = itemStruct.CostTime,
                    UpdateCost = unitStruct.UpdateCost,
                    DeleteCost = unitStruct.DeleteCost,
                    BaseScore = unitStruct.BaseScore,
                    EstimatedGarbageProcessCapacity = GetEstimatedGarbageProcessCapacity(towerName),
                    GarbageProcessNote = GetGarbageProcessNote(towerName)
                };

                Object.DestroyImmediate(instance);
                return towerData;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to extract tower data for {towerName}: {ex.Message}");
                return CreateDefaultTowerData(towerName);
            }
        }

        /// <summary>
        /// プレハブから敵情報を抽出
        /// </summary>
        private static EnemyBalanceData ExtractEnemyFromPrefab(string enemyName)
        {
            try
            {
                var prefabPaths = AssetDatabase.FindAssets(enemyName + " t:prefab");
                if (prefabPaths.Length == 0)
                {
                    return CreateDefaultEnemyData(enemyName);
                }

                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabPaths[0]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null)
                {
                    return CreateDefaultEnemyData(enemyName);
                }

                // プレハブをインスタンス化して ItemStruct を取得
                var instance = Object.Instantiate(prefab);
                var itemProvider = instance.GetComponent<IItemStructProvider>();

                if (itemProvider == null)
                {
                    Object.DestroyImmediate(instance);
                    return CreateDefaultEnemyData(enemyName);
                }

                var itemStruct = itemProvider.ItemStruct;

                var enemyData = new EnemyBalanceData
                {
                    EnemyName = enemyName,
                    CreateCost = itemStruct.CreateCost,
                    CostType = itemStruct.CostType,
                    BaseScore = 0,
                    GarbageDropCount = GetEnemyGarbageDropCount(enemyName),
                    GarbageDropNote = GetEnemyGarbageDropNote(enemyName)
                };

                Object.DestroyImmediate(instance);
                return enemyData;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to extract enemy data for {enemyName}: {ex.Message}");
                return CreateDefaultEnemyData(enemyName);
            }
        }

        /// <summary>
        /// デフォルトのタワーデータ（プレハブ見つからない時）
        /// </summary>
        private static TowerBalanceData CreateDefaultTowerData(string towerName)
        {
            return towerName switch
            {
                "WaterTurret" => new TowerBalanceData { TowerName = "WaterTurret", CreateCost = 150, CostType = "CLK", CostTime = 0.6f, UpdateCost = 0, DeleteCost = 150, BaseScore = 0, EstimatedGarbageProcessCapacity = GetEstimatedGarbageProcessCapacity("WaterTurret"), GarbageProcessNote = GetGarbageProcessNote("WaterTurret") },
                "Sweeper" => new TowerBalanceData { TowerName = "Sweeper", CreateCost = 100, CostType = "CLK", CostTime = 0.5f, UpdateCost = 0, DeleteCost = 100, BaseScore = 0, EstimatedGarbageProcessCapacity = GetEstimatedGarbageProcessCapacity("Sweeper"), GarbageProcessNote = GetGarbageProcessNote("Sweeper") },
                "DustBox" => new TowerBalanceData { TowerName = "DustBox", CreateCost = 50, CostType = "BIT", CostTime = 0.3f, UpdateCost = 0, DeleteCost = 50, BaseScore = 0, EstimatedGarbageProcessCapacity = GetEstimatedGarbageProcessCapacity("DustBox"), GarbageProcessNote = GetGarbageProcessNote("DustBox") },
                "SentryGuard" => new TowerBalanceData { TowerName = "SentryGuard", CreateCost = 80, CostType = "BIT", CostTime = 0.4f, UpdateCost = 0, DeleteCost = 80, BaseScore = 0, EstimatedGarbageProcessCapacity = GetEstimatedGarbageProcessCapacity("SentryGuard"), GarbageProcessNote = GetGarbageProcessNote("SentryGuard") },
                _ => new TowerBalanceData { TowerName = towerName, CreateCost = 0, CostType = "BIT", CostTime = 0f, UpdateCost = 0, DeleteCost = 0, BaseScore = 0 }
            };
        }

        /// <summary>
        /// デフォルトの敵データ（プレハブ見つからない時）
        /// </summary>
        private static EnemyBalanceData CreateDefaultEnemyData(string enemyName)
        {
            return enemyName switch
            {
                "Litter" => new EnemyBalanceData { EnemyName = "Litter", CreateCost = -200, CostType = "CLK", BaseScore = 100, GarbageDropCount = GetEnemyGarbageDropCount("Litter"), GarbageDropNote = GetEnemyGarbageDropNote("Litter") },
                "FireCube" => new EnemyBalanceData { EnemyName = "FireCube", CreateCost = -100, CostType = "BIT", BaseScore = 50, GarbageDropCount = GetEnemyGarbageDropCount("FireCube"), GarbageDropNote = GetEnemyGarbageDropNote("FireCube") },
                _ => new EnemyBalanceData { EnemyName = enemyName, CreateCost = 0, CostType = "BIT", BaseScore = 0 }
            };
        }

        /// <summary>
        /// 敵名からゴミ発生数（最大値）を取得
        /// </summary>
        private static int GetEnemyGarbageDropCount(string enemyName)
        {
            return enemyName switch
            {
                "Litter" => 20,
                "FireCube" => 0,
                _ => 0
            };
        }

        /// <summary>
        /// 敵名からゴミ発生説明を取得
        /// </summary>
        private static string GetEnemyGarbageDropNote(string enemyName)
        {
            return enemyName switch
            {
                "Litter" => "最大 20 個のゴミを散らかす",
                "FireCube" => "ゴミなし（火災を起こす）",
                _ => ""
            };
        }

        /// <summary>
        /// キャッシュをクリア（再抽出時）
        /// </summary>
        internal static void ClearCache()
        {
        }

        /// <summary>
        /// タワー名からゴミ処理能力を取得
        /// </summary>
        private static int GetEstimatedGarbageProcessCapacity(string towerName)
        {
            return towerName switch
            {
                "DustBox" => 15,
                "Sweeper" => 10,
                "WaterTurret" => 0,
                "SentryGuard" => 0,
                _ => 0
            };
        }

        /// <summary>
        /// タワー名からゴミ処理説明を取得
        /// </summary>
        private static string GetGarbageProcessNote(string towerName)
        {
            return towerName switch
            {
                "DustBox" => "ゴミ15を処理予定",
                "Sweeper" => "ゴミ10を処理予定",
                "WaterTurret" => "敵対応に特化",
                "SentryGuard" => "敵対応に特化",
                _ => ""
            };
        }
    }
}
