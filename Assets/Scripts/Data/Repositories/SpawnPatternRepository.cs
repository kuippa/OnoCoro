using System;
using System.Collections.Generic;
using System.IO;
using CommonsUtility;
using Debug = CommonsUtility.Debug;
using YamlDotNet.RepresentationModel;

namespace CommonsUtility
{
    /// <summary>
    /// スポーンパターンリポジトリ（Season 3 W1）
    ///
    /// staging/patterns/*.yaml を読み込み、pattern_id → 相対時間イベントリストの
    /// 辞書を構築する。パターンは複数ステージ・複数年で再利用される。
    /// 仕様: docs/reference/yaml-format.md「Season 3 拡張」を参照
    /// </summary>
    internal static class SpawnPatternRepository
    {
        /// <summary>パターンファイル配置サブフォルダ（staging/ 配下）</summary>
        internal const string PATTERNS_SUB_FOLDER = "patterns";

        private const string _YAML_SEARCH_PATTERN = "*.yaml";

        /// <summary>pattern_id → パターン内イベントリスト（time は相対秒）</summary>
        private static readonly Dictionary<string, List<Dictionary<string, string>>> _patterns
            = new Dictionary<string, List<Dictionary<string, string>>>();

        /// <summary>
        /// patterns フォルダの全 YAML を読み込み直す
        /// ステージロードのたびに呼ばれる想定（ファイル数は少量のため全再読込で問題ない）
        /// </summary>
        internal static void LoadAllPatterns()
        {
            _patterns.Clear();

            string folderPath = LoadStreamingAsset.StageFilePath(PATTERNS_SUB_FOLDER);
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[SpawnPatternRepository.LoadAllPatterns] patterns フォルダがありません: {folderPath}");
                return;
            }

            string[] patternFiles = Directory.GetFiles(folderPath, _YAML_SEARCH_PATTERN);
            foreach (string filePath in patternFiles)
            {
                LoadSinglePatternFile(Path.GetFileName(filePath));
            }

            Debug.Log($"[SpawnPatternRepository.LoadAllPatterns] {_patterns.Count} 件のパターンを読み込みました");
        }

        /// <summary>
        /// パターン ID からイベントリストを取得
        /// </summary>
        internal static bool TryGetPattern(string patternId, out List<Dictionary<string, string>> patternEvents)
        {
            return _patterns.TryGetValue(patternId, out patternEvents);
        }

        /// <summary>
        /// 読み込み済みパターンをすべて破棄
        /// </summary>
        internal static void Clear()
        {
            _patterns.Clear();
        }

        /// <summary>
        /// パターンファイル 1 件を読み込み _patterns に登録
        /// </summary>
        private static void LoadSinglePatternFile(string fileName)
        {
            YamlStream yaml = null;
            try
            {
                yaml = LoadStreamingAsset.LoadYamlFile(Path.Combine(PATTERNS_SUB_FOLDER, fileName));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SpawnPatternRepository] パターン YAML のパースに失敗: {fileName} - {exception.Message}");
                return;
            }

            if (yaml == null)
            {
                Debug.LogWarning($"[SpawnPatternRepository] パターンファイルを読み込めません: {fileName}");
                return;
            }

            YamlMappingNode rootMapping = YamlParserHelper.GetRootMapping(yaml);
            string patternId = YamlParserHelper.GetChildScalar(rootMapping, YamlPatternKeys.PatternId);
            if (string.IsNullOrEmpty(patternId))
            {
                Debug.LogWarning($"[SpawnPatternRepository] {YamlPatternKeys.PatternId} がありません: {fileName}");
                return;
            }

            if (_patterns.ContainsKey(patternId))
            {
                Debug.LogWarning($"[SpawnPatternRepository] pattern_id が重複しています（後発を無視）: {patternId} in {fileName}");
                return;
            }

            List<Dictionary<string, string>> patternEvents
                = YamlParserHelper.BuildDictionaryListFromYaml(yaml, YamlPatternKeys.Events);
            if (patternEvents.Count == 0)
            {
                Debug.LogWarning($"[SpawnPatternRepository] events が空のパターンをスキップ: {patternId}");
                return;
            }

            _patterns.Add(patternId, patternEvents);
        }
    }
}
