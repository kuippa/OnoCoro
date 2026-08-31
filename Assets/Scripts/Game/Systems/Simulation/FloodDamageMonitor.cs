using System.Collections.Generic;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// 潮位で水没した建物を倒壊させる監視（PLATEAU CityHack 2026）
///
/// 水面より一定以上深く沈んだ建物を、一定時間続いたら倒壊させる。
/// 倒壊は building_break と同じ PlateauInfoManager.SetBuildingToDoom を使う。
///
/// [負荷対策]
/// 舞鶴は建物が 7928 棟あるため、以下で負荷を抑えている。
///   - 建物リストと底面 Y は初回に一度だけ収集してキャッシュする
///   - 判定は毎フレームではなく一定間隔
///   - 倒壊は 1 秒あたりの上限までしか行わない（YAML の max_breaks_per_second）
/// </summary>
public class FloodDamageMonitor : MonoBehaviour
{
    /// <summary>水没判定を行う間隔（秒）</summary>
    private const float _CHECK_INTERVAL = 0.5f;

    private const string _PLATEAU_OBJECT_NAME = "Plateau";
    private const string _BUILDING_NAME_KEYWORD = "bldg_";

    /// <summary>建物ごとの底面 Y と水没継続時間</summary>
    private class BuildingFloodState
    {
        public GameObject Building;
        public float BottomY;
        public float SubmergedSeconds;
    }

    private List<BuildingFloodState> _buildings = null;
    private PlateauInfoManager _plateauInfoManager = null;
    private WaterSurfaceManager _waterSurfaceManager = null;

    private float _checkTimer = 0f;
    private float _breakBudgetTimer = 0f;
    private int _breaksThisSecond = 0;

    private const string _HOST_OBJECT_NAME = "FloodDamageMonitor";

    /// <summary>
    /// シーンロード時に自己生成する（シーン配置不要）。
    /// flood セクションが無いステージでは Update が即 return するので実害は無い
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<FloodDamageMonitor>() != null)
        {
            return;
        }
        GameObject host = new GameObject(_HOST_OBJECT_NAME);
        host.AddComponent<FloodDamageMonitor>();
    }

    private void Update()
    {
        if (!FloodDamageSystem.IsEnabled)
        {
            return;
        }

        UpdateBreakBudget();

        _checkTimer = _checkTimer + Time.deltaTime;
        if (_checkTimer < _CHECK_INTERVAL)
        {
            return;
        }
        float elapsed = _checkTimer;
        _checkTimer = 0f;

        CheckSubmergedBuildings(elapsed);
    }

    /// <summary>1 秒ごとに倒壊の残枠を戻す</summary>
    private void UpdateBreakBudget()
    {
        _breakBudgetTimer = _breakBudgetTimer + Time.deltaTime;
        if (_breakBudgetTimer < 1f)
        {
            return;
        }
        _breakBudgetTimer = 0f;
        _breaksThisSecond = 0;
    }

    private void CheckSubmergedBuildings(float elapsed)
    {
        EnsureBuildingsCollected();
        if (_buildings == null || _buildings.Count == 0)
        {
            return;
        }

        float waterY = GetWaterSurfaceHeight();
        float submergeLine = waterY - FloodDamageSystem.DepthMeters;

        foreach (BuildingFloodState state in _buildings)
        {
            if (state.Building == null || !state.Building.activeSelf)
            {
                continue;
            }

            // 底面が判定ラインより上なら、まだ沈んでいない
            if (state.BottomY > submergeLine)
            {
                state.SubmergedSeconds = 0f;
                continue;
            }

            state.SubmergedSeconds = state.SubmergedSeconds + elapsed;
            if (state.SubmergedSeconds < FloodDamageSystem.DurationSeconds)
            {
                continue;
            }

            if (_breaksThisSecond >= FloodDamageSystem.MaxBreaksPerSecond)
            {
                // 今秒の枠を使い切った。残りは次の秒に持ち越す
                return;
            }

            BreakBuilding(state);
        }
    }

    private void BreakBuilding(BuildingFloodState state)
    {
        PlateauInfoManager infoManager = GetPlateauInfoManager();
        if (infoManager == null)
        {
            return;
        }

        infoManager.SetBuildingToDoom(state.Building);
        state.SubmergedSeconds = 0f;
        _breaksThisSecond = _breaksThisSecond + 1;
        FloodDamageSystem.RecordFloodedBuilding();
    }

    /// <summary>
    /// 建物リストと底面 Y を一度だけ収集する。
    /// 底面 Y は Renderer の bounds から取る（建物は動かない前提でキャッシュ）
    /// </summary>
    private void EnsureBuildingsCollected()
    {
        if (_buildings != null)
        {
            return;
        }

        _buildings = new List<BuildingFloodState>();
        GameObject plateauObject = GameObject.Find(_PLATEAU_OBJECT_NAME);
        if (plateauObject == null)
        {
            return;
        }

        foreach (Transform child in plateauObject.GetComponentsInChildren<Transform>())
        {
            if (!child.gameObject.name.Contains(_BUILDING_NAME_KEYWORD))
            {
                continue;
            }

            Collider collider = child.GetComponent<Collider>();
            if (collider == null || !PlateauUtility.IsPlateauBuilding(collider))
            {
                continue;
            }

            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer == null)
            {
                continue;
            }

            BuildingFloodState state = new BuildingFloodState();
            state.Building = child.gameObject;
            state.BottomY = renderer.bounds.min.y;
            state.SubmergedSeconds = 0f;
            _buildings.Add(state);
        }

        Debug.Log($"[FloodDamageMonitor] 浸水監視の対象建物 {_buildings.Count} 棟を収集しました");
    }

    private float GetWaterSurfaceHeight()
    {
        if (_waterSurfaceManager == null)
        {
            _waterSurfaceManager = GameObjectTreat.GetOrAddComponent<WaterSurfaceManager>(
                GameObjectTreat.GetEventSystem());
        }
        return _waterSurfaceManager.GetWaterSurfaceHeightPublic();
    }

    private PlateauInfoManager GetPlateauInfoManager()
    {
        if (_plateauInfoManager != null)
        {
            return _plateauInfoManager;
        }

        GameObject plateauObject = GameObject.Find(_PLATEAU_OBJECT_NAME);
        if (plateauObject == null)
        {
            return null;
        }
        _plateauInfoManager = plateauObject.GetComponent<PlateauInfoManager>();
        return _plateauInfoManager;
    }
}
