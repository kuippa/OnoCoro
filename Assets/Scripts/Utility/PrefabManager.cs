// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PrefabManager
using UnityEngine;

public class PrefabManager
{
	private static GameObject _garbageCubePrefab;

	private static GameObject _fireCubePrefab;

	private static GameObject _rainDropPrefab;

	private static GameObject _dustBoxPrefab;

	private static GameObject _sentryGuardPrefab;

	private static GameObject _waterTurretPrefab;

	private static GameObject _stopPlatePrefab;

	private static GameObject _powerCubePrefab;

	private static GameObject _towerSweeperPrefab;

	private static GameObject _enemyLitterPrefab;

	private static GameObject _pathMakerPrefab;

	private static GameObject _pathBloomPrefab;

	// private static GameObject _UIStageSelectorPrefab;
	// private static GameObject _UIStageFileListPrefab;
	private static GameObject _UIStageInfoBoxPrefab;

	private static int _garbageCubeUID;

	private static int _fireCubeUID;

	private static int _dustBoxUID;

	private static int _sentryGuardUID;

	private static int _stopPlateUID;

	private static int _powerCubeUID;

	internal static GameObject UIStageInfoBoxPrefab
	{
		get
		{
			if (_UIStageInfoBoxPrefab == null)
			{
				_UIStageInfoBoxPrefab = Resources.Load<GameObject>("Prefabs/UI/UIStageInfoBox");
			}
			return _UIStageInfoBoxPrefab;
		}
	}

	internal static GameObject GarbageCubePrefab
	{
		get
		{
			if (_garbageCubePrefab == null)
			{
				_garbageCubePrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/GarbageCube");
			}
			return _garbageCubePrefab;
		}
	}

	internal static GameObject RainDropPrefab
	{
		get
		{
			if (_rainDropPrefab == null)
			{
				_rainDropPrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/RainDrop");
			}
			return _rainDropPrefab;
		}
	}

	internal static GameObject FireCubePrefab
	{
		get
		{
			if (_fireCubePrefab == null)
			{
				_fireCubePrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/FireCubeSimple");
			}
			return _fireCubePrefab;
		}
	}

	internal static GameObject PathMakerPrefab
	{
		get
		{
			if (_pathMakerPrefab == null)
			{
				_pathMakerPrefab = Resources.Load<GameObject>("Prefabs/Marker/path_marker");
			}
			return _pathMakerPrefab;
		}
	}

	internal static GameObject PathBloomPrefab
	{
		get
		{
			if (_pathBloomPrefab == null)
			{
				_pathBloomPrefab = Resources.Load<GameObject>("Prefabs/Marker/path_bloom");
			}
			return _pathBloomPrefab;
		}
	}

	internal static GameObject DustBoxPrefab
	{
		get
		{
			if (_dustBoxPrefab == null)
			{
				_dustBoxPrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/DustBox");
			}
			return _dustBoxPrefab;
		}
	}

	internal static GameObject SentryGuardPrefab
	{
		get
		{
			if (_sentryGuardPrefab == null)
			{
				_sentryGuardPrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/SentryGuard");
			}
			return _sentryGuardPrefab;
		}
	}

	internal static GameObject EnemyLitterPrefab
	{
		get
		{
			if (_enemyLitterPrefab == null)
			{
				_enemyLitterPrefab = Resources.Load<GameObject>("Prefabs/EnemyUnit/EnemyLitter");
			}
			return _enemyLitterPrefab;
		}
	}

	internal static GameObject WaterTurretPrefab
	{
		get
		{
			if (_waterTurretPrefab == null)
			{
				_waterTurretPrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/WaterTurret");
			}
			return _waterTurretPrefab;
		}
	}

	internal static GameObject StopPlatePrefab
	{
		get
		{
			if (_stopPlatePrefab == null)
			{
				_stopPlatePrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/StopPlate");
			}
			return _stopPlatePrefab;
		}
	}

	internal static GameObject PowerCubePrefab
	{
		get
		{
			if (_powerCubePrefab == null)
			{
				_powerCubePrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/PowerCube");
			}
			return _powerCubePrefab;
		}
	}

	internal static GameObject TowerSweeperPrefab
	{
		get
		{
			if (_towerSweeperPrefab == null)
			{
				_towerSweeperPrefab = Resources.Load<GameObject>("Prefabs/WorkUnit/TowerSweeper");
			}
			return _towerSweeperPrefab;
		}
	}

	internal static int GarbageCubeUID => _garbageCubeUID++;

	internal static int FireCubeUID => _fireCubeUID++;

	internal static int DustBoxUID => _dustBoxUID++;

	internal static int SentryGuardUID => _sentryGuardUID++;

	internal static int StopPlateUID => _stopPlateUID++;

	internal static int PowerCubeUID => _powerCubeUID++;

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

	/// <summary>
	/// プレハブを安全に取得します（nullの場合はエラーログ）
	/// </summary>
	/// <param name="prefab">取得するプレハブ</param>
	/// <param name="prefabName">プレハブ名（ログ用）</param>
	/// <param name="fallbackPath">フォールバック用のResourcesパス（オプション）</param>
	/// <returns>プレハブ、またはnull</returns>
	public static GameObject GetSafe(GameObject prefab, string prefabName, string fallbackPath = null)
	{
		if (prefab != null)
		{
			return prefab;
		}

		if (!string.IsNullOrEmpty(fallbackPath))
		{
			prefab = Resources.Load<GameObject>(fallbackPath);
			if (prefab != null)
			{
				#if UNITY_EDITOR
				Debug.Log($"[PrefabManager] {prefabName} をResourcesから読み込みました: {fallbackPath}");
				#endif
				return prefab;
			}
		}

		#if UNITY_EDITOR
		Debug.LogError($"[PrefabManager] {prefabName} プレハブが見つかりません。パス: {fallbackPath ?? "指定なし"}");
		#endif
		return null;
	}
}
