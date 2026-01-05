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

	private static GameObject _UIStageSelectorPrefab;

	private static GameObject _UIStageFileListPrefab;

	private static int _garbageCubeUID;

	private static int _fireCubeUID;

	private static int _dustBoxUID;

	private static int _sentryGuardUID;

	private static int _stopPlateUID;

	private static int _powerCubeUID;

	internal static GameObject UIStageFileListPrefab
	{
		get
		{
			if (_UIStageFileListPrefab == null)
			{
				_UIStageFileListPrefab = Resources.Load<GameObject>("Prefabs/UI/UIStageFileList");
			}
			return _UIStageFileListPrefab;
		}
	}

	internal static GameObject UIStageSelectorPrefab
	{
		get
		{
			if (_UIStageSelectorPrefab == null)
			{
				_UIStageSelectorPrefab = Resources.Load<GameObject>("Prefabs/UI/UIStageSelector");
			}
			return _UIStageSelectorPrefab;
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
}
