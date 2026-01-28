// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PrefabManager
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager
{
	public enum PrefabType
	{
		// WorkUnit
		GarbageCube,
		FireCube,
		DustBox,
		TowerSentryGuard,
		WaterTurret,
		StopPlate,
		PowerCube,
		TowerSweeper,
		WaterSphere,
		RainDrop,
		Puddle,
		EnemyLitter,
		TowerDock,
		
		// Environment
		WaterSurface,
		Bonfire,
		
		// Marker
		PathMaker,
		PathBloom,
		SignPowerOutage,
		CircularIndicator,
		
		// UI
		UIStageInfoBox,
		UIBuildingInfo,
		UIToolTips,
		ItemHolder,
		NowLoading
	}

	private static readonly Dictionary<PrefabType, string> _paths = new Dictionary<PrefabType, string>
	{
		// WorkUnit
		{ PrefabType.GarbageCube, "Prefabs/WorkUnit/GarbageCube" },
		{ PrefabType.FireCube, "Prefabs/WorkUnit/FireCube" },
		{ PrefabType.DustBox, "Prefabs/WorkUnit/TowerDustBox" },
		{ PrefabType.TowerSentryGuard, "Prefabs/WorkUnit/TowerSentryGuard" },
		{ PrefabType.WaterTurret, "Prefabs/WorkUnit/WaterTurret" },
		{ PrefabType.StopPlate, "Prefabs/WorkUnit/StopPlate" },
		{ PrefabType.PowerCube, "Prefabs/WorkUnit/PowerCube" },
		{ PrefabType.TowerSweeper, "Prefabs/WorkUnit/TowerSweeper" },
		{ PrefabType.WaterSphere, "Prefabs/WorkUnit/WaterSphere" },
		{ PrefabType.RainDrop, "Prefabs/WorkUnit/RainDrop" },
		{ PrefabType.Puddle, "Prefabs/WorkUnit/Puddle" },
		{ PrefabType.EnemyLitter, "Prefabs/EnemyUnit/EnemyLitter" },
		{ PrefabType.TowerDock, "Prefabs/WorkUnit/TowerDock" },
		
		// Environment
		{ PrefabType.WaterSurface, "Prefabs/Environment/watersurface" },
		{ PrefabType.Bonfire, "Prefabs/Bonfire" },
		
		// Marker
		{ PrefabType.PathMaker, "Prefabs/Marker/path_marker" },
		{ PrefabType.PathBloom, "Prefabs/Marker/path_bloom" },
		{ PrefabType.SignPowerOutage, "Prefabs/Marker/SignPowerOutage" },
		{ PrefabType.CircularIndicator, "Prefabs/Marker/CircularIndicator" },
		
		// UI
		{ PrefabType.UIStageInfoBox, "Prefabs/UI/UIStageInfoBox" },
		{ PrefabType.UIBuildingInfo, "Prefabs/UI/UIBuildingInfo" },
		{ PrefabType.UIToolTips, "Prefabs/UI/UIToolTips" },
		{ PrefabType.ItemHolder, "Prefabs/UI/Item_holder" },
		{ PrefabType.NowLoading, "Prefabs/UI/nowloading" }
	};

	private static readonly Dictionary<PrefabType, GameObject> _cache = new Dictionary<PrefabType, GameObject>();
	private static readonly Dictionary<PrefabType, int> _uidCounters = new Dictionary<PrefabType, int>();

	/// <summary>
	/// プレハブを取得します（キャッシュ機能付き）
	/// </summary>
	/// <param name="type">プレハブの種類</param>
	/// <returns>プレハブ、またはnull</returns>
	public static GameObject GetPrefab(PrefabType type)
	{
		if (!_cache.TryGetValue(type, out GameObject prefab) || prefab == null)
		{
			if (_paths.TryGetValue(type, out string path))
			{
				prefab = Resources.Load<GameObject>(path);
				_cache[type] = prefab;
				
				#if UNITY_EDITOR
				if (prefab == null)
				{
					Debug.LogWarning($"[PrefabManager] プレハブが見つかりません: {path}");
				}
				#endif
			}
		}
		return prefab;
	}

	/// <summary>
	/// 指定されたプレハブタイプの次のUIDを取得します
	/// </summary>
	/// <param name="type">プレハブの種類</param>
	/// <returns>ユニークID</returns>
	public static int GetNextUID(PrefabType type)
	{
		if (!_uidCounters.ContainsKey(type))
		{
			_uidCounters[type] = 0;
		}
		return _uidCounters[type]++;
	}

	// 後方互換性のためのプロパティ（既存コードとの互換性維持）
	internal static GameObject UIStageInfoBoxPrefab => GetPrefab(PrefabType.UIStageInfoBox);
	internal static GameObject UIBuildingInfoPrefab => GetPrefab(PrefabType.UIBuildingInfo);
	internal static GameObject GarbageCubePrefab => GetPrefab(PrefabType.GarbageCube);
	internal static GameObject RainDropPrefab => GetPrefab(PrefabType.RainDrop);
	internal static GameObject PuddlePrefab => GetPrefab(PrefabType.Puddle);
	internal static GameObject FireCubePrefab => GetPrefab(PrefabType.FireCube);
	internal static GameObject PathMakerPrefab => GetPrefab(PrefabType.PathMaker);
	internal static GameObject PathBloomPrefab => GetPrefab(PrefabType.PathBloom);
	internal static GameObject DustBoxPrefab => GetPrefab(PrefabType.DustBox);
	internal static GameObject TowerSentryGuardPrefab => GetPrefab(PrefabType.TowerSentryGuard);
	internal static GameObject EnemyLitterPrefab => GetPrefab(PrefabType.EnemyLitter);
	internal static GameObject WaterTurretPrefab => GetPrefab(PrefabType.WaterTurret);
	internal static GameObject StopPlatePrefab => GetPrefab(PrefabType.StopPlate);
	internal static GameObject PowerCubePrefab => GetPrefab(PrefabType.PowerCube);
	internal static GameObject TowerSweeperPrefab => GetPrefab(PrefabType.TowerSweeper);
	internal static GameObject WaterSpherePrefab => GetPrefab(PrefabType.WaterSphere);
	internal static GameObject WaterSurfacePrefab => GetPrefab(PrefabType.WaterSurface);
	internal static GameObject SignPowerOutagePrefab => GetPrefab(PrefabType.SignPowerOutage);
	internal static GameObject BonfirePrefab => GetPrefab(PrefabType.Bonfire);
	internal static GameObject TowerDockPrefab => GetPrefab(PrefabType.TowerDock);
	internal static GameObject CircularIndicatorPrefab => GetPrefab(PrefabType.CircularIndicator);
	internal static GameObject UIToolTipsPrefab => GetPrefab(PrefabType.UIToolTips);
	internal static GameObject ItemHolderPrefab => GetPrefab(PrefabType.ItemHolder);
	internal static GameObject NowLoadingPrefab => GetPrefab(PrefabType.NowLoading);

	internal static int GarbageCubeUID => GetNextUID(PrefabType.GarbageCube);
	internal static int FireCubeUID => GetNextUID(PrefabType.FireCube);
	internal static int DustBoxUID => GetNextUID(PrefabType.DustBox);
	internal static int TowerSentryGuardUID => GetNextUID(PrefabType.TowerSentryGuard);
	internal static int StopPlateUID => GetNextUID(PrefabType.StopPlate);
	internal static int PowerCubeUID => GetNextUID(PrefabType.PowerCube);
	internal static int TowerDockUID => GetNextUID(PrefabType.TowerDock);

	/// <summary>
	/// プレハブが利用可能かチェックします（nullチェック）
	/// </summary>
	/// <param name="prefab">チェックするプレハブ</param>
	/// <param name="prefabName">プレハブ名（ログ用）</param>
	/// <returns>利用可能な場合true</returns>
	public static bool IsAvailable(GameObject prefab, string prefabName)
	{
		if (prefab == null)
		{
			#if UNITY_EDITOR
			Debug.LogWarning($"[PrefabManager] {prefabName} プレハブが見つかりません。プレハブを復元後に機能します。");
			#endif
			return false;
		}
		return true;
	}
}
