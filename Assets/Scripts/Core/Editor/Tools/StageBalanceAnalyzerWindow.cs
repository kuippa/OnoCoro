using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CommonsUtility
{
    /// <summary>
    /// ステージバランス分析ツール
    /// Menu: Tools > OnoCoro > Stage Balance Analyzer
    /// </summary>
    internal class StageBalanceAnalyzerWindow : EditorWindow
    {
        private Vector2 _scrollPosition = Vector2.zero;
        private int _selectedTabIndex = 0;

        private List<TowerBalanceData> _towers = new List<TowerBalanceData>();
        private List<EnemyBalanceData> _enemies = new List<EnemyBalanceData>();
        private StageAnalysisResult _currentStageAnalysis = null;

        private const string _WINDOW_TITLE = "Stage Balance Analyzer";
        private const float _PADDING = 10f;
        private const float _ROW_HEIGHT = 20f;

        [MenuItem("Tools/Stage Balance Analyzer")]
        public static void ShowWindow()
        {
            GetWindow<StageBalanceAnalyzerWindow>(_WINDOW_TITLE);
        }

        private void OnEnable()
        {
            RefreshData();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("=== Stage Balance Analyzer ===", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Refresh Data", GUILayout.Width(100)))
                {
                    RefreshData();
                }
                EditorGUILayout.LabelField("タワーと敵のバランスデータを表示します");
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                _selectedTabIndex = GUILayout.SelectionGrid(
                    _selectedTabIndex,
                    new string[] { "Towers", "Enemies", "Stage", "Export" },
                    4,
                    EditorStyles.toolbarButton
                );
            }

            EditorGUILayout.Space();

            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;

                switch (_selectedTabIndex)
                {
                    case 0:
                        DrawTowersTab();
                        break;
                    case 1:
                        DrawEnemiesTab();
                        break;
                    case 2:
                        DrawStageAnalysisTab();
                        break;
                    case 3:
                        DrawExportTab();
                        break;
                }
            }
        }

        private void DrawTowersTab()
        {
            EditorGUILayout.LabelField("Tower List", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope("box"))
            {
                DrawTableHeader(new string[] { "Tower", "Create Cost", "Cost Type", "Update", "Delete", "Time", "ゴミ処理上限", "備考" });

                foreach (var tower in _towers)
                {
                    DrawTableRow(new string[]
                    {
                        tower.TowerName,
                        tower.CreateCost.ToString(),
                        tower.CostType,
                        tower.UpdateCost.ToString(),
                        tower.DeleteCost.ToString(),
                        tower.CostTime.ToString("F1"),
                        tower.EstimatedGarbageProcessCapacity > 0 ? tower.EstimatedGarbageProcessCapacity.ToString() : "-",
                        tower.GarbageProcessNote
                    });
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Total Towers: {_towers.Count}");
        }

        private void DrawEnemiesTab()
        {
            EditorGUILayout.LabelField("Enemy List", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope("box"))
            {
                DrawTableHeader(new string[] { "Enemy", "Spawn Cost", "Cost Type", "Score", "ゴミ発生数", "備考" });

                foreach (var enemy in _enemies)
                {
                    DrawTableRow(new string[]
                    {
                        enemy.EnemyName,
                        enemy.CreateCost.ToString(),
                        enemy.CostType,
                        enemy.BaseScore.ToString(),
                        enemy.GarbageDropCount > 0 ? enemy.GarbageDropCount.ToString() : "-",
                        enemy.GarbageDropNote
                    });
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Total Enemies: {_enemies.Count}");
        }

        private void DrawStageAnalysisTab()
        {
            EditorGUILayout.LabelField("Stage Analysis", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Load Current Scene", GUILayout.Height(30)))
            {
                _currentStageAnalysis = StageYamlAnalyzer.AnalyzeCurrentScene();
            }

            EditorGUILayout.Space();

            if (_currentStageAnalysis == null || string.IsNullOrEmpty(_currentStageAnalysis.StageName))
            {
                EditorGUILayout.HelpBox("no stage loaded yet", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField($"Stage: {_currentStageAnalysis.StageName}", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Initial Resources", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  BIT: {_currentStageAnalysis.InitialBIT}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  CLK: {_currentStageAnalysis.InitialCLK}", EditorStyles.miniLabel);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Enemy Summary", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  Total Count: {_currentStageAnalysis.TotalEnemiesCount}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  Total Drop Value (Enemy): {_currentStageAnalysis.TotalEnemyDropValue}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  Estimated Garbage Score: {_currentStageAnalysis.EstimatedGarbageScore}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  [Maximum Logical Score: {_currentStageAnalysis.MaximumLogicalScore}]", EditorStyles.boldLabel);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Available Towers", EditorStyles.miniLabel);
                foreach (var tower in _currentStageAnalysis.AvailableTowers)
                {
                    EditorGUILayout.LabelField($"  {tower.TowerName} ({tower.CostType} {tower.CreateCost})", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unified Timeline (敵スポーン & タワー配置)", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawUnifiedTimeline(_currentStageAnalysis);
        }

        private void DrawUnifiedTimeline(StageAnalysisResult analysis)
        {
            if (analysis.UnifiedTimeline == null || analysis.UnifiedTimeline.Count == 0)
            {
                EditorGUILayout.HelpBox("No timeline events available", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("残存ゴミ = タワー処理上限を超えたゴミ量（ルート別）。0ならタワーが十分に対応できている。", EditorStyles.miniLabel);
                EditorGUILayout.Space();

                DrawTableHeader(new string[] { "Time", "Route", "種別", "イベント", "コスト", "リソース", "残存(前)", "残存(後)", "処理上限" });

                foreach (var evt in analysis.UnifiedTimeline)
                {
                    string timeStr = evt.Time.ToString("F1") + "s";
                    string routeStr = string.IsNullOrEmpty(evt.RouteName) ? "-" : evt.RouteName;
                    string typeStr = evt.EventType == TimelineEventType.EnemySpawn ? "[敵]" : 
                                     evt.EventType == TimelineEventType.TowerDeploy ? "[塔]" : "[資]";
                    string eventStr = evt.EventName;
                    
                    // コスト表示（タワー配置の場合）
                    string costStr = "-";
                    if (evt.EventType == TimelineEventType.TowerDeploy && evt.TowerCreateCost > 0)
                    {
                        costStr = $"{evt.TowerCreateCost} {evt.TowerCostType}";
                    }
                    
                    // リソース取得表示（PowerCube の場合）
                    string resourceStr = "-";
                    if (evt.EventType == TimelineEventType.ResourceGain && evt.ResourceGain > 0)
                    {
                        resourceStr = $"+{evt.ResourceGain} {evt.ResourceGainType}";
                    }
                    
                    string before = evt.GarbageBeforeEvent.ToString();
                    string after = evt.GarbageAfterEvent.ToString();
                    string capacity = evt.TotalCapacityAtEvent.ToString();

                    DrawTableRow(new string[] { timeStr, routeStr, typeStr, eventStr, costStr, resourceStr, before, after, capacity });
                }
            }
        }

        private void DrawExportTab()
        {
            EditorGUILayout.LabelField("Export Data", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("タワーと敵のバランスデータを CSV (Tab 区切り) でコピーします", MessageType.Info);
            EditorGUILayout.Space();

            if (GUILayout.Button("Copy Towers to Clipboard (CSV)", GUILayout.Height(30)))
            {
                CopyTowersToClipboard();
                EditorUtility.DisplayDialog("Success", "Tower data copied to clipboard!", "OK");
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Copy Enemies to Clipboard (CSV)", GUILayout.Height(30)))
            {
                CopyEnemiesToClipboard();
                EditorUtility.DisplayDialog("Success", "Enemy data copied to clipboard!", "OK");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tips: Excel に貼り付けて分析できます（データ > テキストに列を分割）");
        }

        private void DrawTableHeader(string[] headers)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                foreach (var header in headers)
                {
                    EditorGUILayout.LabelField(header, EditorStyles.miniLabel, GUILayout.MinWidth(60));
                }
            }
        }

        private void DrawTableRow(string[] cells)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var cell in cells)
                {
                    EditorGUILayout.SelectableLabel(cell, GUILayout.Height(_ROW_HEIGHT), GUILayout.MinWidth(60));
                }
            }
        }

        private void CopyTowersToClipboard()
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Tower\tCreate Cost\tCost Type\tUpdate\tDelete\tTime");

            foreach (var tower in _towers)
            {
                csv.AppendLine($"{tower.TowerName}\t{tower.CreateCost}\t{tower.CostType}\t{tower.UpdateCost}\t{tower.DeleteCost}\t{tower.CostTime:F1}");
            }

            EditorGUIUtility.systemCopyBuffer = csv.ToString();
        }

        private void CopyEnemiesToClipboard()
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Enemy\tSpawn Cost\tCost Type\tScore");

            foreach (var enemy in _enemies)
            {
                csv.AppendLine($"{enemy.EnemyName}\t{enemy.CreateCost}\t{enemy.CostType}\t{enemy.BaseScore}");
            }

            EditorGUIUtility.systemCopyBuffer = csv.ToString();
        }

        private void RefreshData()
        {
            BalanceDataExtractor.ClearCache();
            _towers = BalanceDataExtractor.ExtractAllTowers();
            _enemies = BalanceDataExtractor.ExtractAllEnemies();
            Repaint();
        }
    }
}
