using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 危険予測ハイライト（簡易ヒートマップ・Season 3 W2 Task 5）
    ///
    /// 配置フェーズ（Placement）中に、次に開始する年の building_break で倒壊予定の
    /// 建物を警告色で強調し、「どこが危ないかを見てから投資する」体験を成立させる。
    /// Start Year で強調を解除する。
    ///
    /// 予測の根拠は年イベントの事前読み（BuildingBreak は対象リストの先頭 N 棟を
    /// 決定的に倒壊させるため、次年の break_count から対象を再現できる）。
    /// ワークショップでは「ハザードマップを見て投資する」体験として意図どおりの設計。
    /// </summary>
    internal static class HazardForecastSystem
    {
        private const string _PLATEAU_OBJECT_NAME = "Plateau";
        private const string _BREAK_ALL_KEYWORD = "all";

        private static readonly Color _WARNING_COLOR = new Color(1.0f, 0.45f, 0.1f, 1f);  // 警告オレンジ
        private const string _WARNING_SHADER_NAME = "HDRP/Lit";

        /// <summary>強調中の建物 → 元マテリアル配列（解除時の復元用）</summary>
        private static readonly Dictionary<GameObject, Material[]> _highlightedBuildings
            = new Dictionary<GameObject, Material[]>();

        private static Material _warningMaterial = null;

        /// <summary>
        /// 指定年の倒壊予定建物を警告色で強調する（Placement フェーズ開始時に呼ぶ）
        /// </summary>
        internal static void ApplyForecast(EventLoader eventLoader, int year)
        {
            ClearForecast();

            if (eventLoader == null)
            {
                return;
            }

            int breakCount = GetForecastBreakCount(eventLoader, year);
            if (breakCount == 0)
            {
                return;
            }

            List<GameObject> breakTargets = GetBreakTargets();
            if (breakTargets == null || breakTargets.Count == 0)
            {
                return;
            }

            int highlighted = HighlightTargets(breakTargets, breakCount);
            Debug.Log($"[HazardForecastSystem] Year {year} の倒壊予測 {breakCount} 棟中 {highlighted} 棟を強調表示");
        }

        /// <summary>
        /// 強調表示をすべて解除し元マテリアルに戻す（Start Year / リセット時に呼ぶ）
        /// </summary>
        internal static void ClearForecast()
        {
            foreach (KeyValuePair<GameObject, Material[]> entry in _highlightedBuildings)
            {
                if (entry.Key == null)
                {
                    continue;  // シーン遷移等で破棄済み
                }
                Renderer buildingRenderer = entry.Key.GetComponent<Renderer>();
                if (buildingRenderer != null)
                {
                    buildingRenderer.materials = entry.Value;
                }
            }
            _highlightedBuildings.Clear();
        }

        /// <summary>
        /// 対象リストの先頭から倒壊予定棟数ぶんを警告色にする（倒壊済みはスキップ）
        /// </summary>
        private static int HighlightTargets(List<GameObject> breakTargets, int breakCount)
        {
            PlateauBuildingInteractor interactor = GetBuildingInteractor();
            Material warningMaterial = GetWarningMaterial();
            int highlightedCount = 0;

            // building_break と同じく「未倒壊の建物を先頭から breakCount 件 新規に」塗る。
            // 先頭 N 番目までを見る方式だと、前年までに倒壊済みの建物が先頭にあると塗る数が
            // 不足し、実際に倒壊する建物（未倒壊優先で N 件）と一致しなかった（2026-06-18 修正）
            foreach (GameObject building in breakTargets)
            {
                if (highlightedCount >= breakCount)
                {
                    break;
                }
                if (building == null || !building.activeSelf)
                {
                    continue;
                }
                if (interactor != null && interactor.IsBuildingDoomed(building))
                {
                    continue;  // 既に倒壊済みはスキップ（未倒壊を N 件塗る）
                }

                Renderer buildingRenderer = building.GetComponent<Renderer>();
                if (buildingRenderer == null)
                {
                    continue;
                }

                _highlightedBuildings[building] = buildingRenderer.materials;
                Material[] warningMaterials = new Material[buildingRenderer.materials.Length];
                for (int slot = 0; slot < warningMaterials.Length; slot++)
                {
                    warningMaterials[slot] = warningMaterial;
                }
                buildingRenderer.materials = warningMaterials;
                highlightedCount = highlightedCount + 1;
            }

            return highlightedCount;
        }

        /// <summary>
        /// 指定年のイベントから building_break の合計棟数を取得（"all" は int.MaxValue）
        /// その年に倒壊する建物は本震＋余震など複数 break の合計なので、合計で予測する
        /// （未倒壊優先で各 break が新規 N 棟を倒すため、合計＝その年の地震倒壊予定数）
        /// </summary>
        private static int GetForecastBreakCount(EventLoader eventLoader, int year)
        {
            if (!eventLoader._year_events.TryGetValue(year, out Dictionary<float, List<Dictionary<string, string>>> yearEvents))
            {
                return 0;
            }

            int totalCount = 0;
            string eventField = TimedEventCommandFields.@event.ToString();
            foreach (List<Dictionary<string, string>> eventList in yearEvents.Values)
            {
                foreach (Dictionary<string, string> eventData in eventList)
                {
                    // building_break と building_break_fire（地震連動火災・2026-06-23 追加）の両方を
                    // 倒壊予測の対象にする。quake_fire の building_break_fire 化で予測が 0 件になり
                    // オレンジ予告が消えていた（2026-07-03 修正）
                    bool isBreakEvent = false;
                    if (eventData.TryGetValue(eventField, out string eventName))
                    {
                        isBreakEvent = eventName == YamlEventType.building_break.ToString()
                            || eventName == YamlEventType.building_break_fire.ToString();
                    }
                    if (!isBreakEvent)
                    {
                        continue;
                    }
                    if (!eventData.TryGetValue("value", out string breakValue))
                    {
                        continue;
                    }
                    if (breakValue.Trim() == _BREAK_ALL_KEYWORD)
                    {
                        return int.MaxValue;
                    }
                    if (int.TryParse(breakValue, out int count))
                    {
                        totalCount = totalCount + count;
                    }
                }
            }

            return totalCount;
        }

        private static List<GameObject> GetBreakTargets()
        {
            GameObject eventSystem = GameObjectTreat.GetEventSystem();
            if (eventSystem == null)
            {
                return null;
            }
            BuildingBreak buildingBreak = GameObjectTreat.GetOrAddComponent<BuildingBreak>(eventSystem);
            if (buildingBreak == null)
            {
                return null;
            }
            return buildingBreak.GetBreakTargets();
        }

        private static PlateauBuildingInteractor GetBuildingInteractor()
        {
            GameObject plateauObject = GameObject.Find(_PLATEAU_OBJECT_NAME);
            if (plateauObject == null)
            {
                return null;
            }
            return plateauObject.GetComponent<PlateauBuildingInteractor>();
        }

        private static Material GetWarningMaterial()
        {
            if (_warningMaterial == null)
            {
                Shader litShader = Shader.Find(_WARNING_SHADER_NAME);
                _warningMaterial = new Material(litShader);
                _warningMaterial.color = _WARNING_COLOR;
            }
            return _warningMaterial;
        }
    }
}
