// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// SpawnCtrl
using CommonsUtility;
using UnityEngine;

public class SpawnCtrl : MonoBehaviour
{
	public static SpawnCtrl _instance;

	public static SpawnCtrl Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindFirstObjectByType<SpawnCtrl>();
				if (_instance == null)
				{
					_instance = new GameObject("SpawnCtrl").AddComponent<SpawnCtrl>();
				}
			}
			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
	}

	private void OnDestory()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}

	internal bool CallUnitByName(string unitName, Vector3 spawnPoint = default(Vector3))
	{
		bool result = false;
		if (unitName == null || unitName == "")
		{
			return result;
		}
		if (unitName == GameEnum.ModelsType.GarbageCube.ToString())
		{
			result = SpawnGarbageCube(0f, spawnPoint);
		}
		else if (unitName == GameEnum.ModelsType.GarbageCubeBox.ToString())
		{
			result = SpawnGarbageCubeBox(spawnPoint);
		}
		else if (unitName == GameEnum.ModelsType.GarbageCubeBig.ToString())
		{
			result = SpawnGarbageCubeBig(spawnPoint);
		}
		else if (unitName == GameEnum.ModelsType.Sweeper.ToString())
		{
			result = SpawnTowerSweeper(0.25f, spawnPoint);
		}
		else if (unitName == GameEnum.ModelsType.PowerCube.ToString())
		{
			result = SpawnPowerCube(0f, spawnPoint);
		}
		else if (unitName == GameEnum.ModelsType.StopPlate.ToString())
		{
			result = SpawnStopPlate();
		}
		else if (unitName == GameEnum.ModelsType.FireCube.ToString())
		{
			result = SpawnFireCube(spawnPoint);
		}
		else if (unitName == GameEnum.ModelsType.WaterTurret.ToString())
		{
			result = SpawnWaterTurret(spawnPoint);
		}
		else if (unitName == GameEnum.ModelsType.DustBox.ToString())
		{
			result = SpawnDustBox(spawnPoint);
		}
		else if (unitName == GameEnum.ModelsType.SentryGuard.ToString())
		{
			result = SpawnSentryGuard(spawnPoint);
		}
		else
		{
			Debug.Log("default CallUnitByName " + unitName);
		}
		return result;
	}

	internal bool CallEnemyUnitByName(string unitName, string[] marker_names)
	{
		bool result = false;
		if (unitName == null || unitName == "")
		{
			return result;
		}
		if (unitName == GameEnum.ModelsType.Litter.ToString())
		{
			result = SpawnLitter(marker_names);
		}
		else
		{
			Debug.Log("default CallEnemyUnitByName " + unitName);
		}
		return result;
	}

	private bool SpawnSentryGuard(Vector3 spawnPoint = default(Vector3))
	{
		float dropbuffer = 0.05f;
		spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
		Quaternion spawnRotateAngle = GetSpawnRotateAngle();
		GameObject gameObject = Object.Instantiate(PrefabManager.SentryGuardPrefab, spawnPoint, spawnRotateAngle);
		int sentryGuardUID = PrefabManager.SentryGuardUID;
		gameObject.name = GameEnum.ModelsType.SentryGuard.ToString() + sentryGuardUID;
		SentryGuard orAddComponent = GameObjectTreat.GetOrAddComponent<SentryGuard>(gameObject);
		orAddComponent._item_struct.ItemID = gameObject.name;
		orAddComponent._unit_struct.UnitID = gameObject.name;
		return true;
	}

	private bool SpawnDustBox(Vector3 spawnPoint = default(Vector3))
	{
		float dropbuffer = 0.05f;
		Quaternion spawnRotateAngle = GetSpawnRotateAngle();
		spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
		GameObject gameObject = Object.Instantiate(PrefabManager.DustBoxPrefab, spawnPoint, spawnRotateAngle);
		int dustBoxUID = PrefabManager.DustBoxUID;
		gameObject.name = GameEnum.ModelsType.DustBox.ToString() + dustBoxUID;
		DustBox orAddComponent = GameObjectTreat.GetOrAddComponent<DustBox>(gameObject);
		orAddComponent._item_struct.ItemID = gameObject.name;
		orAddComponent._unit_struct.UnitID = gameObject.name;
		return true;
	}

	private bool SpawnLitter(string[] marker_names)
	{
		GameObject gameObject = Object.Instantiate(position: new Vector3(0f, 0f, 0f), original: PrefabManager.EnemyLitterPrefab, rotation: Quaternion.identity);
		EnemyLitter component = gameObject.GetComponent<EnemyLitter>();
		int idx = EnemyLitter._idx;
		gameObject.name = GameEnum.ModelsType.Litter.ToString() + idx;
		Litter orAddComponent = GameObjectTreat.GetOrAddComponent<Litter>(gameObject);
		orAddComponent._unit_struct.UnitID = gameObject.name;
		orAddComponent._item_struct.ItemID = gameObject.name;
		component.InitUnitSpawn(marker_names);
		return true;
	}

	private bool SpawnWaterTurret(Vector3 spawnPoint = default(Vector3))
	{
		float dropbuffer = 1.5f;
		GameObject waterTurretPrefab = PrefabManager.WaterTurretPrefab;
		spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
		Object.Instantiate(waterTurretPrefab).GetComponent<WaterTurretCtrl>().CreateWaterTurretUnit(spawnPoint);
		return true;
	}

	private bool SpawnFireCube(Vector3 spawnPoint = default(Vector3))
	{
		float dropbuffer = 1.5f;
		bool result = false;
		spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
		if (FireCubeCtrl.SpawnFireCube(spawnPoint) != null)
		{
			result = true;
		}
		return result;
	}

	private bool SpawnGarbageCubeBig(Vector3 spawnPoint = default(Vector3))
	{
		return SpawnGarbageCube(3f, spawnPoint, 2);
	}

	private bool SpawnGarbageCubeBox(Vector3 spawnPoint = default(Vector3))
	{
		return SpawnGarbageCubeBoxCoroutine(spawnPoint);
	}

	private bool SpawnGarbageCubeBoxCoroutine(Vector3 spawnPoint)
	{
		float num = 20f;
		for (int i = 0; (float)i < num; i++)
		{
			if (!SpawnGarbageCube(0.1f * (float)i, spawnPoint, 1, isSwayingPoint: true))
			{
				return false;
			}
		}
		return true;
	}

	private bool SpawnGarbageCube(float dropbuffer = 1.5f, Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
	{
		bool result = false;
		spawnPoint = GetSpawnPoint(dropbuffer, spawnPoint);
		if (GarbageCubeCtrl.SpawnGarbageCube(spawnPoint, sizeFlag, isSwayingPoint) != null)
		{
			result = true;
		}
		return result;
	}

	private bool SpawnStopPlate(float dropbuffer = 0.05f)
	{
		GameObject stopPlatePrefab = PrefabManager.StopPlatePrefab;
		Vector3 spawnPoint = GetSpawnPoint(dropbuffer);
		Quaternion markerRotateAngle = SpawnMarkerPointerCtrl.GetMarkerRotateAngle();
		GameObject gameObject = Object.Instantiate(stopPlatePrefab, spawnPoint, markerRotateAngle);
		int stopPlateUID = PrefabManager.StopPlateUID;
		gameObject.name = GameEnum.ModelsType.StopPlate.ToString() + stopPlateUID;
		StopPlate orAddComponent = GameObjectTreat.GetOrAddComponent<StopPlate>(gameObject);
		orAddComponent._item_struct.ItemID = gameObject.name;
		orAddComponent._unit_struct.UnitID = gameObject.name;
		return true;
	}

	private bool SpawnPowerCube(float dropbuffer = 0.25f, Vector3 setPoint = default(Vector3))
	{
		GameObject powerCubePrefab = PrefabManager.PowerCubePrefab;
		powerCubePrefab.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		setPoint = GetSpawnPoint(dropbuffer, setPoint);
		Quaternion rotation = Quaternion.Euler(rdNum(0f, 360f), rdNum(0f, 360f), rdNum(0f, 360f));
		GameObject gameObject = Object.Instantiate(powerCubePrefab, setPoint, rotation);
		int powerCubeUID = PrefabManager.PowerCubeUID;
		gameObject.name = GameEnum.ModelsType.PowerCube.ToString() + powerCubeUID;
		PowerCube orAddComponent = GameObjectTreat.GetOrAddComponent<PowerCube>(gameObject);
		orAddComponent._item_struct.ItemID = gameObject.name;
		orAddComponent._unit_struct.UnitID = gameObject.name;
		return true;
	}

	private bool SpawnTowerSweeper(float dropbuffer = 0.05f, Vector3 setPoint = default(Vector3))
	{
		setPoint = GetSpawnPoint(dropbuffer, setPoint);
		GameObject towerSweeperPrefab = PrefabManager.TowerSweeperPrefab;
		Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
		Object.Instantiate(towerSweeperPrefab, setPoint, rotation).GetComponent<TowerSweeper>().CreateSweeperUnit(setPoint);
		return true;
	}

	private Vector3 GetSpawnPoint(float dropbuffer = 0.05f, Vector3 setPoint = default(Vector3))
	{
		if (setPoint == default(Vector3))
		{
			setPoint = SpawnMarkerPointerCtrl.GetMarkerPosition();
		}
		setPoint.y += dropbuffer;
		return setPoint;
	}

	private Quaternion GetSpawnRotateAngle()
	{
		_ = Quaternion.identity;
		return SpawnMarkerPointerCtrl.GetMarkerRotateAngle();
	}

	private SpawnMarkerPointerCtrl GetSpawnMarkerCtrl()
	{
		GameObject gameObject = GameObject.FindWithTag(GameEnum.UIType.SpawnMarker.ToString());
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<SpawnMarkerPointerCtrl>();
	}

	private float rdNum(float min, float max)
	{
		return Utility.fRandomRange(min, max);
	}
}
