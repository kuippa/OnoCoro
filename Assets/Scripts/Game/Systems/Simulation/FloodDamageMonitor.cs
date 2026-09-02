using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

/// <summary>
/// 潮位で水没した建物を倒壊させる監視（PLATEAU CityHack 2026）
///
/// 水面より一定以上深く沈んだ建物を、一定時間続いたら倒壊させる。
/// 倒壊は building_break と同じ PlateauInfoManager.SetBuildingToDoom を使う。
///
/// [実装方針] Update は使わずコルーチンで回している。
/// WaitForSeconds は Time.timeScale に従うため、GameSpeedManager による
/// 倍速・一時停止にそのまま追従する。Update だと自前で倍速を扱う必要があり、
/// 毎フレームの処理集中も招く。
///
/// [負荷対策]
/// 舞鶴は建物が 7928 棟あるため、以下で負荷を抑えている。
///   - 建物リストと底面 Y は一度収集したらキャッシュする
///     （PLATEAU の読み込み前だと 0 棟になるため、見つかるまでは再試行する）
///   - 判定は _CHECK_INTERVAL 間隔
///   - 倒壊は 1 秒あたり max_breaks_per_second 件までに制限する
/// </summary>
public class FloodDamageMonitor : MonoBehaviour
{
    /// <summary>水没判定を行う間隔（秒）</summary>
    private const float _CHECK_INTERVAL = 0.5f;

    private const string _PLATEAU_OBJECT_NAME = "Plateau";
    private const string _BUILDING_NAME_KEYWORD = "bldg_";

    /// <summary>建物収集を諦めるまでの試行回数（PLATEAU の読み込み待ち用）</summary>
    private const int _MAX_COLLECT_ATTEMPTS = 40;

    private const string _HOST_OBJECT_NAME = "FloodDamageMonitor";

    /// <summary>建物ごとの底面 Y と水没継続時間</summary>
    private class BuildingFloodState
    {
        public GameObject Building;
        public float BottomY;
        public float SubmergedSeconds;

        /// <summary>
        /// 倒壊済みか。SetBuildingToDoom を呼んでも建物は active のまま残るため、
        /// この印を付けないと同じ建物を何度も倒壊させ続け、
        /// 1 秒あたりの倒壊枠を食い潰して他の建物が永久に壊れなくなる
        /// </summary>
        public bool IsBroken;
    }

    private List<BuildingFloodState> _buildings = null;
    private PlateauInfoManager _plateauInfoManager = null;
    private PlateauBuildingInteractor _buildingInteractor = null;
    private WaterSurfaceManager _waterSurfaceManager = null;
    private int _collectAttempts = 0;

    /// <summary>
    /// ステージロード処理から呼んで監視役を用意する（シーン配置不要）。
    ///
    /// [重要] シーンをまたがせない。生成したホストはシーン遷移で破棄され、
    /// 次のステージで作り直される。建物キャッシュも一緒に捨てられるので、
    /// 前ステージの建物を掴んだままになる事故が起きない。
    ///
    /// 以前は RuntimeInitializeOnLoadMethod で起動時に一度だけ生成していたが、
    /// これはタイトルシーンで作られてステージロードで破棄されるため、
    /// ビルド版では監視役が居ない状態になっていた
    /// （エディタはステージシーンで直接 Play するので露見しなかった）。
    /// </summary>
    internal static void EnsureExists()
    {
        if (FindFirstObjectByType<FloodDamageMonitor>() != null)
        {
            return;
        }
        GameObject host = new GameObject(_HOST_OBJECT_NAME);
        host.AddComponent<FloodDamageMonitor>();
    }

    private void Start()
    {
        StartCoroutine(MonitorLoop());
    }

    /// <summary>
    /// 一定間隔で水没状態を更新する監視ループ
    /// </summary>
    private IEnumerator MonitorLoop()
    {
        WaitForSeconds interval = new WaitForSeconds(_CHECK_INTERVAL);

        while (true)
        {
            yield return interval;

            if (!FloodDamageSystem.IsEnabled)
            {
                continue;
            }
            CheckSubmergedBuildings(_CHECK_INTERVAL);
        }
    }

    private void CheckSubmergedBuildings(float elapsed)
    {
        EnsureBuildingsCollected();
        if (_buildings == null || _buildings.Count == 0)
        {
            return;
        }

        float submergeLine = GetOceanHeight() - FloodDamageSystem.DepthMeters;
        int breakQuota = Mathf.CeilToInt(FloodDamageSystem.MaxBreaksPerSecond * elapsed);

        foreach (BuildingFloodState state in _buildings)
        {
            if (state.IsBroken || state.Building == null || !state.Building.activeSelf)
            {
                continue;
            }

            // 底面が判定ラインより上なら、まだ沈んでいない
            if (state.BottomY > submergeLine)
            {
                state.SubmergedSeconds = 0f;
                continue;
            }

            // 水没時間は全建物ぶん進める。倒壊枠が尽きても計測は止めない
            // （止めるとリストの後ろの建物がいつまでも条件を満たさなくなる）
            state.SubmergedSeconds = state.SubmergedSeconds + elapsed;
            if (state.SubmergedSeconds < FloodDamageSystem.DurationSeconds)
            {
                continue;
            }

            if (breakQuota <= 0)
            {
                // 今回の枠は使い切った。条件は満たしたままなので次回倒壊する
                continue;
            }

            BreakBuilding(state);
            breakQuota = breakQuota - 1;
        }
    }

    private void BreakBuilding(BuildingFloodState state)
    {
        PlateauInfoManager infoManager = GetPlateauInfoManager();
        if (infoManager == null)
        {
            return;
        }

        // [重要] 地震や火災で既に倒壊している建物は数えない。
        // 数えてしまうと浸水倒壊が水増しされ、
        // 「総被害 - 地震倒壊 - 浸水倒壊」で求める火災延焼が 0 に潰れる
        bool isAlreadyDoomed = IsBuildingAlreadyDoomed(state.Building);

        // 瓦礫は既定で出さない。1 棟あたり最大 200 個の Rigidbody が生成され蓄積するため、
        // 広域浸水では数万個に達して FPS が落ちる（水中なので見た目の損失も無い）
        infoManager.SetBuildingToDoom(state.Building, isFire: false,
            isSpawnDebris: FloodDamageSystem.IsDebrisEnabled);
        state.IsBroken = true;

        if (isAlreadyDoomed)
        {
            return;
        }

        FloodDamageSystem.RecordFloodedBuilding();
        DamageReportSystem.AddFloodCollapse(1);
    }

    /// <summary>
    /// 既に倒壊扱いの建物か（BuildingBreak の新規倒壊判定と同じ方法）
    /// </summary>
    private bool IsBuildingAlreadyDoomed(GameObject building)
    {
        PlateauBuildingInteractor interactor = GetBuildingInteractor();
        if (interactor == null)
        {
            return false;
        }
        return interactor.IsBuildingDoomed(building);
    }

    /// <summary>
    /// 建物リストと底面 Y を収集する（建物は動かない前提でキャッシュ）。
    ///
    /// [注意] 建物は "Plateau" オブジェクトの配下にあるとは限らないため、
    /// BuildingBreak と同じくシーンの全ルートオブジェクトを走査する
    /// </summary>
    private void EnsureBuildingsCollected()
    {
        if (_buildings != null)
        {
            return;
        }

        List<BuildingFloodState> collected = new List<BuildingFloodState>();
        foreach (GameObject rootObject in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            CollectBuildingsUnder(rootObject, collected);
        }

        if (collected.Count == 0)
        {
            // PLATEAU の読み込み前かもしれないのでキャッシュせず次回に回す。
            // 建物が無いステージで走査を繰り返しても無駄なので回数で打ち切る
            _collectAttempts = _collectAttempts + 1;
            if (_collectAttempts >= _MAX_COLLECT_ATTEMPTS)
            {
                _buildings = collected;
                Debug.LogWarning(
                    $"[FloodDamageMonitor] 対象建物が {_MAX_COLLECT_ATTEMPTS} 回試しても見つからないため監視を諦めます"
                    + "（シーンに bldg_ オブジェクトが無い可能性）");
            }
            return;
        }

        _buildings = collected;
        Debug.Log($"[FloodDamageMonitor] 浸水監視の対象建物 {_buildings.Count} 棟を収集しました");
    }

    private void CollectBuildingsUnder(GameObject rootObject, List<BuildingFloodState> collected)
    {
        foreach (Transform child in rootObject.GetComponentsInChildren<Transform>(includeInactive: false))
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

            BuildingFloodState state = new BuildingFloodState();
            state.Building = child.gameObject;

            // 底面はコライダーの境界から取る（Renderer が無い建物でも拾えるようにする）
            state.BottomY = collider.bounds.min.y;
            state.SubmergedSeconds = 0f;
            state.IsBroken = false;
            collected.Add(state);
        }
    }

    /// <summary>
    /// 海面の高さ。親の watersurface は Ocean / River などを束ねるホルダーで、
    /// その Y は海面の高さではないため必ず Ocean のワールド Y を見る
    /// </summary>
    private float GetOceanHeight()
    {
        if (_waterSurfaceManager == null)
        {
            _waterSurfaceManager = GameObjectTreat.GetOrAddComponent<WaterSurfaceManager>(
                GameObjectTreat.GetEventSystem());
        }
        return _waterSurfaceManager.GetOceanHeight();
    }

    private PlateauBuildingInteractor GetBuildingInteractor()
    {
        if (_buildingInteractor != null)
        {
            return _buildingInteractor;
        }

        GameObject plateauObject = GameObject.Find(_PLATEAU_OBJECT_NAME);
        if (plateauObject == null)
        {
            return null;
        }
        _buildingInteractor = plateauObject.GetComponent<PlateauBuildingInteractor>();
        return _buildingInteractor;
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
