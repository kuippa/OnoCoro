using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using Debug = UnityEngine.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// ステージ YAML ファイルを解析するユーティリティ
    /// 現在のシーン名から自動検出し、スケジュールされた敵生成イベントを抽出
    /// </summary>
    internal static class StageYamlAnalyzer
    {
        private const string _STAGING_FOLDER = "Assets/StreamingAssets/staging";
        private const string _SPAWN_ENEMY_EVENT = "spawn_enemy_unit";
        private const string _SPAWN_UNIT_DEBUG_EVENT = "spawn_unit_debug";

        /// <summary>
        /// 現在のシーン名から対応する YAML ファイルを探してパース
        /// </summary>
        internal static StageAnalysisResult AnalyzeCurrentScene()
        {
            string sceneName = GetCurrentSceneName();
            if (string.IsNullOrEmpty(sceneName))
            {
                return CreateEmpty("No scene loaded");
            }

            string yamlPath = FindYamlFile(sceneName);
            if (string.IsNullOrEmpty(yamlPath))
            {
                return CreateEmpty($"YAML not found for scene: {sceneName}");
            }

            return AnalyzeYamlFile(yamlPath);
        }

        /// <summary>
        /// 指定パスの YAML ファイルをパース
        /// </summary>
        internal static StageAnalysisResult AnalyzeYamlFile(string yamlPath)
        {
            if (!File.Exists(yamlPath))
            {
                return CreateEmpty($"File not found: {yamlPath}");
            }

            try
            {
                using (var reader = new StreamReader(yamlPath))
                {
                    var yaml = new YamlStream();
                    yaml.Load(reader);

                    if (yaml.Documents.Count == 0)
                    {
                        return CreateEmpty("YAML document is empty");
                    }

                    var rootNode = yaml.Documents[0].RootNode as YamlMappingNode;
                    if (rootNode == null)
                    {
                        return CreateEmpty("Invalid YAML structure");
                    }

                    var result = new StageAnalysisResult();

                    // ステージ名を抽出
                    result.StageName = ExtractScalarValue(rootNode, "stagename");

                    // 初期リソースを抽出
                    ExtractInitialResources(rootNode, result);

                    // 敵生成イベントを抽出
                    ExtractEnemySpawns(rootNode, result);

                    // 利用可能なタワー一覧を抽出
                    ExtractAvailableTowers(rootNode, result);

                    // 統合タイムラインを構築（spawn_unit_debug を含む）
                    BuildUnifiedTimeline(rootNode, result);

                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing YAML: {ex.Message}");
                return CreateEmpty($"Parse error: {ex.Message}");
            }
        }

        /// <summary>
        /// 初期リソース（BIT, CLK）を抽出
        /// </summary>
        private static void ExtractInitialResources(YamlMappingNode rootNode, StageAnalysisResult result)
        {
            var stagesKvp = rootNode.Children.FirstOrDefault(x => 
                (x.Key as YamlScalarNode)?.Value == "stages");
            var stagesNode = stagesKvp.Value as YamlSequenceNode;

            if (stagesNode == null)
            {
                return;
            }

            foreach (var stage in stagesNode.Children.OfType<YamlMappingNode>())
            {
                var bitValue = ExtractScalarValue(stage, "BIT");
                var clkValue = ExtractScalarValue(stage, "CLK");

                if (int.TryParse(bitValue, out int bit))
                {
                    result.InitialBIT = bit;
                }

                if (int.TryParse(clkValue, out int clk))
                {
                    result.InitialCLK = clk;
                }
            }
        }

        /// <summary>
        /// 敵生成イベント（spawn_enemy_unit）を抽出
        /// </summary>
        private static void ExtractEnemySpawns(YamlMappingNode rootNode, StageAnalysisResult result)
        {
            var eventsKvp = rootNode.Children.FirstOrDefault(x => 
                (x.Key as YamlScalarNode)?.Value == "events");
            var eventsNode = eventsKvp.Value as YamlSequenceNode;

            if (eventsNode == null)
            {
                return;
            }

            var spawnDict = new Dictionary<(float time, string enemyType, string route), int>();
            var garbageScores = BalanceDataExtractor.GetEnemyGarbageDropScores();

            foreach (var eventItem in eventsNode.Children.OfType<YamlMappingNode>())
            {
                string eventType = ExtractScalarValue(eventItem, "event");
                if (eventType != _SPAWN_ENEMY_EVENT)
                {
                    continue;
                }

                string timeStr = ExtractScalarValue(eventItem, "time");
                string value = ExtractScalarValue(eventItem, "value");

                if (!float.TryParse(timeStr, out float time))
                {
                    continue;
                }

                // value フォーマット: "EnemyType, route_name" または直接マーカー指定
                string enemyType = ExtractEnemyTypeFromValue(value);
                string routeName = ExtractRouteFromValue(value);
                if (string.IsNullOrEmpty(enemyType))
                {
                    continue;
                }

                var key = (time, enemyType, routeName);
                if (!spawnDict.ContainsKey(key))
                {
                    spawnDict[key] = 0;
                }
                spawnDict[key]++;
            }

            // 時系列にソート + 結果を設定
            int cumulativeGarbage = 0;
            foreach (var kvp in spawnDict.OrderBy(x => x.Key.time))
            {
                var spawnInfo = new EnemySpawnInfo
                {
                    SpawnTime = kvp.Key.time,
                    EnemyType = kvp.Key.enemyType,
                    RouteName = kvp.Key.route,
                    Count = kvp.Value
                };

                result.EnemySpawns.Add(spawnInfo);
                result.TotalEnemiesCount += kvp.Value;

                // 敵スコアとゴミスコアを計算
                int enemyScore = GetEstimatedDropValue(kvp.Key.enemyType);
                result.TotalEnemyDropValue += enemyScore * kvp.Value;

                // ゴミ処理スコアを追加
                if (garbageScores.ContainsKey(kvp.Key.enemyType))
                {
                    int garbageForThisWave = garbageScores[kvp.Key.enemyType] * kvp.Value / 10;
                    cumulativeGarbage += garbageForThisWave;
                    spawnInfo.CumulativeGarbageCount = cumulativeGarbage;
                    result.EstimatedGarbageScore += garbageScores[kvp.Key.enemyType] * kvp.Value;
                }
            }

            // 最大理論値 = 敵スコア + ゴミ処理スコア
            result.MaximumLogicalScore = result.TotalEnemyDropValue + result.EstimatedGarbageScore;
        }

        /// <summary>
        /// 利用可能なアイテム（タワー）を抽出
        /// </summary>
        private static void ExtractAvailableTowers(YamlMappingNode rootNode, StageAnalysisResult result)
        {
            var itemListsKvp = rootNode.Children.FirstOrDefault(x => 
                (x.Key as YamlScalarNode)?.Value == "itemlists");
            var itemListsNode = itemListsKvp.Value as YamlSequenceNode;

            if (itemListsNode == null)
            {
                return;
            }

            var allTowers = BalanceDataExtractor.ExtractAllTowers();
            var towerDict = allTowers.ToDictionary(t => t.TowerName);

            foreach (var item in itemListsNode.Children.OfType<YamlMappingNode>())
            {
                string itemName = ExtractScalarValue(item, "item");
                if (string.IsNullOrEmpty(itemName) || itemName.StartsWith("#"))
                {
                    continue;
                }

                if (towerDict.ContainsKey(itemName.Trim()))
                {
                    result.AvailableTowers.Add(towerDict[itemName.Trim()]);
                }
            }
        }

        
        /// <summary>
        /// YamlMappingNode からスカラー値を抽出（Int版）
        /// </summary>
        private static int ExtractIntValue(YamlMappingNode node, string key)
        {
            var kvp = node.Children.FirstOrDefault(x => 
                (x.Key as YamlScalarNode)?.Value == key);

            if (int.TryParse((kvp.Value as YamlScalarNode)?.Value ?? "", out int value))
            {
                return value;
            }

            return 0;
        }

        /// <summary>
        /// value フォーマットから敵タイプを抽出
        /// 例: "Litter, route_wave1" → "Litter"
        /// </summary>
        private static string ExtractEnemyTypeFromValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var parts = value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0].Trim() : null;
        }

        /// <summary>
        /// value フォーマットからルート名を抽出
        /// 例: "Litter, route_wave1" → "route_wave1"
        /// 例: "Litter, marker_a, marker_b, goal" → "marker_a, marker_b, goal"（直接指定）
        /// </summary>
        private static string ExtractRouteFromValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var parts = value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return string.Empty;
            }

            return parts[1].Trim();
        }

        /// <summary>
        /// 敵タイプから推定ドロップ値を取得
        /// </summary>
        private static int GetEstimatedDropValue(string enemyType)
        {
            return enemyType switch
            {
                "Litter" => 100,
                "FireCube" => 50,
                _ => 25
            };
        }

        /// <summary>
        /// YamlMappingNode からスカラー値を抽出
        /// </summary>
        private static string ExtractScalarValue(YamlMappingNode node, string key)
        {
            var kvp = node.Children.FirstOrDefault(x => 
                (x.Key as YamlScalarNode)?.Value == key);

            return (kvp.Value as YamlScalarNode)?.Value ?? string.Empty;
        }

        /// <summary>
        /// ステージ名から YAML ファイルを検索
        /// </summary>
        private static string FindYamlFile(string sceneName)
        {
            if (!Directory.Exists(_STAGING_FOLDER))
            {
                return null;
            }

            var yamlFiles = Directory.GetFiles(_STAGING_FOLDER, "*.yaml");

            // 完全一致を優先
            var exactMatch = yamlFiles.FirstOrDefault(f => 
                Path.GetFileNameWithoutExtension(f) == sceneName);

            if (exactMatch != null)
            {
                return exactMatch;
            }

            // 部分一致を探す
            return yamlFiles.FirstOrDefault(f => 
                Path.GetFileNameWithoutExtension(f).Contains(sceneName));
        }

        /// <summary>
        /// 現在のシーン名を取得
        /// </summary>
        private static string GetCurrentSceneName()
        {
            #if UNITY_EDITOR
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            return scene.name;
            #else
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            #endif
        }

        /// <summary>
        /// 空の分析結果を作成
        /// </summary>
        /// <summary>
        /// 敵スポーンとタワー配置を統合した時系列タイムラインを構築
        ///
        /// 設計方針：
        ///   - ルートごとに unprocessedGarbage と totalCapacity を独立管理
        ///   - タワー配置には route フィールドでどのルートをカバーするか指定
        ///   - 敵スポーン時：そのルートの unprocessedGarbage += 発生ゴミ数
        ///   - タワー配置時：そのルートの totalCapacity += タワー処理能力 × 台数
        /// </summary>
        private static void BuildUnifiedTimeline(YamlMappingNode rootNode, StageAnalysisResult result)
        {
            var timeline = new List<TimelineEvent>();

            // 敵スポーンイベントをリストに追加
            foreach (var spawn in result.EnemySpawns)
            {
                int garbageSpawned = spawn.Count * GetEnemyGarbageValue(spawn.EnemyType);
                timeline.Add(new TimelineEvent
                {
                    Time = spawn.SpawnTime,
                    EventType = TimelineEventType.EnemySpawn,
                    EventName = $"{spawn.EnemyType} ×{spawn.Count}",
                    Description = $"ゴミ発生: +{garbageSpawned}",
                    RouteName = spawn.RouteName,
                    EnemyType = spawn.EnemyType,
                    EnemyCount = spawn.Count,
                    GarbageSpawned = garbageSpawned
                });
            }

            // spawn_unit_debug イベントをタワー配置として抽出
            ExtractDebugUnitSpawns(rootNode, timeline);

            // PowerCube リソース取得イベントを抽出
            ExtractResourceGainEvents(rootNode, timeline);

            // タワー配置イベントを推奨戦略から追加（後方互換性）
            if (result.RecommendedStrategy != null)
            {
                foreach (var tower in result.RecommendedStrategy.Towers)
                {
                    int capacityAdded = GetTowerGarbageCapacity(tower.TowerName) * tower.Count;
                    timeline.Add(new TimelineEvent
                    {
                        Time = tower.DeployTime,
                        EventType = TimelineEventType.TowerDeploy,
                        EventName = $"{tower.TowerName} ×{tower.Count}",
                        Description = tower.Description,
                        RouteName = tower.RouteName,
                        TowerName = tower.TowerName,
                        TowerCount = tower.Count,
                        TowerCapacityAdded = capacityAdded
                    });
                }
            }

            // 時系列でソート（同時刻はタワー配置を先に処理）
            timeline = timeline
                .OrderBy(x => x.Time)
                .ThenBy(x => x.EventType == TimelineEventType.TowerDeploy ? 0 : 1)
                .ToList();

            // --- ルート別シミュレーション ---
            // キーはルート名。未指定の場合は空文字
            var routeGarbage = new Dictionary<string, int>();    // ルートごとの残存ゴミ数
            var routeCapacity = new Dictionary<string, int>();   // ルートごとのタワー処理上限

            foreach (var evt in timeline)
            {
                string route = evt.RouteName ?? string.Empty;

                if (!routeGarbage.ContainsKey(route))
                {
                    routeGarbage[route] = 0;
                }
                if (!routeCapacity.ContainsKey(route))
                {
                    routeCapacity[route] = 0;
                }

                evt.GarbageBeforeEvent = routeGarbage[route];
                evt.TotalCapacityAtEvent = routeCapacity[route];

                if (evt.EventType == TimelineEventType.TowerDeploy)
                {
                    // タワー配置：処理上限が増える。既存ゴミも即座に再計算
                    routeCapacity[route] += evt.TowerCapacityAdded;
                    routeGarbage[route] = Math.Max(0, routeGarbage[route] - evt.TowerCapacityAdded);
                }
                else if (evt.EventType == TimelineEventType.EnemySpawn)
                {
                    // 敵スポーン：ゴミが増える、処理上限を超えた分が残存
                    routeGarbage[route] += evt.GarbageSpawned;
                    routeGarbage[route] = Math.Max(0, routeGarbage[route] - routeCapacity[route]);
                }

                evt.GarbageAfterEvent = routeGarbage[route];
                evt.TotalCapacityAtEvent = routeCapacity[route];
            }

            result.UnifiedTimeline = timeline;
        }

        /// <summary>
        /// タワー名からゴミ処理能力を取得
        /// </summary>
        private static int GetTowerGarbageCapacity(string towerName)
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
        /// 敵タイプからゴミ値を取得
        /// </summary>
        private static int GetEnemyGarbageValue(string enemyType)
        {
            return enemyType switch
            {
                "Litter" => 20,
                "FireCube" => 0,
                _ => 0
            };
        }

        /// <summary>
        /// spawn_unit_debug イベントをタワー配置イベントとして抽出
        /// </summary>
        private static void ExtractDebugUnitSpawns(YamlMappingNode rootNode, List<TimelineEvent> timeline)
        {
            var eventsKvp = rootNode.Children.FirstOrDefault(x => 
                (x.Key as YamlScalarNode)?.Value == "events");
            var eventsNode = eventsKvp.Value as YamlSequenceNode;

            if (eventsNode == null)
            {
                return;
            }

            var allTowers = BalanceDataExtractor.ExtractAllTowers().ToDictionary(t => t.TowerName);

            foreach (var eventItem in eventsNode.Children.OfType<YamlMappingNode>())
            {
                string eventType = ExtractScalarValue(eventItem, "event");
                if (eventType != _SPAWN_UNIT_DEBUG_EVENT)
                {
                    continue;
                }

                string timeStr = ExtractScalarValue(eventItem, "time");
                string value = ExtractScalarValue(eventItem, "value");

                if (!float.TryParse(timeStr, out float time))
                {
                    continue;
                }

                // value フォーマット: "TowerName, posX, posY, posZ" など
                string towerName = ExtractTowerNameFromValue(value);
                if (string.IsNullOrEmpty(towerName) || !allTowers.ContainsKey(towerName))
                {
                    continue;
                }

                var towerData = allTowers[towerName];
                int capacityAdded = GetTowerGarbageCapacity(towerName);
                timeline.Add(new TimelineEvent
                {
                    Time = time,
                    EventType = TimelineEventType.TowerDeploy,
                    EventName = $"{towerName} (Debug)",
                    Description = "デバッグモードでの配置",
                    TowerName = towerName,
                    TowerCount = 1,
                    TowerCapacityAdded = capacityAdded,
                    TowerCreateCost = towerData.CreateCost,
                    TowerCostType = towerData.CostType
                });
            }
        }

        /// <summary>
        /// spawn_unit_debug value からタワー名を抽出
        /// フォーマット: "DustBox, -2, auto, 160"
        /// </summary>
        private static string ExtractTowerNameFromValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var parts = value.Split(',');
            if (parts.Length > 0)
            {
                return parts[0].Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// リソース取得イベント（PowerCube等）をタイムラインに抽出
        /// </summary>
        private static void ExtractResourceGainEvents(YamlMappingNode rootNode, List<TimelineEvent> timeline)
        {
            var eventsKvp = rootNode.Children.FirstOrDefault(x => 
                (x.Key as YamlScalarNode)?.Value == "events");
            var eventsNode = eventsKvp.Value as YamlSequenceNode;

            if (eventsNode == null)
            {
                return;
            }

            foreach (var eventItem in eventsNode.Children.OfType<YamlMappingNode>())
            {
                string eventType = ExtractScalarValue(eventItem, "event");
                if (eventType != "spawn_unit")
                {
                    continue;
                }

                string timeStr = ExtractScalarValue(eventItem, "time");
                string value = ExtractScalarValue(eventItem, "value");

                if (!float.TryParse(timeStr, out float time))
                {
                    continue;
                }

                // value フォーマット: "PowerCube, posX, posY, posZ, resourceAmount"
                // 例: "PowerCube, -2, auto, 155, 15"
                string unitName = ExtractResourceUnitNameFromValue(value);
                if (!unitName.Equals("PowerCube", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int resourceGain = ExtractResourceAmountFromValue(value);
                if (resourceGain <= 0)
                {
                    continue;
                }

                timeline.Add(new TimelineEvent
                {
                    Time = time,
                    EventType = TimelineEventType.ResourceGain,
                    EventName = $"{unitName} ×{resourceGain}",
                    Description = "リソース取得",
                    ResourceGain = resourceGain,
                    ResourceGainType = "CLK"  // PowerCube は CLK を供与
                });
            }
        }

        /// <summary>
        /// spawn_unit value からユニット名を抽出
        /// フォーマット: "UnitName, posX, posY, posZ, ..."
        /// </summary>
        private static string ExtractResourceUnitNameFromValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var parts = value.Split(',');
            if (parts.Length > 0)
            {
                return parts[0].Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// spawn_unit value からリソース量を抽出
        /// フォーマット: "PowerCube, posX, posY, posZ, resourceAmount"
        /// 最後の部分が数値であればそれがリソース量
        /// </summary>
        private static int ExtractResourceAmountFromValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            var parts = value.Split(',');
            if (parts.Length > 4)
            {
                if (int.TryParse(parts[4].Trim(), out int amount))
                {
                    return amount;
                }
            }

            return 0;
        }

        private static StageAnalysisResult CreateEmpty(string message)
        {
            Debug.LogWarning($"[StageYamlAnalyzer] {message}");
            return new StageAnalysisResult
            {
                StageName = message
            };
        }
    }
}
