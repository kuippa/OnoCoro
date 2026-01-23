using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CommonsUtility;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class UnitFireDisaster : MonoBehaviour
{
	private const int _ZONE_SIZE = 50;

	private void SettingCubes(int zone, float distance)
	{
		for (int i = -1 * zone; i < zone; i++)
		{
			for (int j = -1 * zone; j < zone; j++)
			{
				if (i != 0 || j != 0)
				{
					Vector3 spawnPoint = new Vector3((float)i * distance, 1f, (float)j * distance);
					GameObject gameManagerObject = GameObjectTreat.GetGameManagerObject();
					GarbageCubeCtrl garbageCubeCtrl = gameManagerObject.GetComponent<GarbageCubeCtrl>();
					if (garbageCubeCtrl == null)
					{
						garbageCubeCtrl = gameManagerObject.AddComponent<GarbageCubeCtrl>();
					}
					garbageCubeCtrl.SpawnGarbageCubeAsync(spawnPoint);
				}
			}
		}
	}

	private void SettingFireCubes(int zone, float distance)
	{
		for (int i = -1 * zone; i <= zone; i++)
		{
			for (int j = -1 * zone; j <= zone; j++)
			{
				if (i != 0 || j != 0)
				{
					Vector3 spawnPoint = new Vector3((float)i * distance, 1f, (float)j * distance);
					GameObject gameManagerObject = GameObjectTreat.GetGameManagerObject();
					FireCubeCtrl fireCubeCtrl = gameManagerObject.GetComponent<FireCubeCtrl>();
					if (fireCubeCtrl == null)
					{
						fireCubeCtrl = gameManagerObject.AddComponent<FireCubeCtrl>();
					}
					fireCubeCtrl.SpawnFireCubeAsync(spawnPoint, 2);
				}
			}
		}
	}

	private void GetPooledObject(List<GameObject> pool)
	{
		for (int i = 0; i < pool.Count; i++)
		{
			if (!pool[i].activeInHierarchy)
			{
				pool[i].SetActive(value: true);
			}
		}
	}

	private void SettingWalls(int max, float distance)
	{
		int num = max / 2;
		int num2 = max / 2;
		GameObject original = PrefabManager.StopPlatePrefab;
		for (int i = 0; i < num; i++)
		{
			Vector3 position = new Vector3((float)num + (float)i * distance + distance / 2f, 0f, (float)num2 - (float)i * distance - distance / 2f);
			if (!((float)num + (float)i * distance > (float)max) && !((float)num2 - (float)i * distance < 0f))
			{
				Quaternion rotation = Quaternion.Euler(0f, 45f, 0f);
				Object.Instantiate(original, position, rotation);
				continue;
			}
			break;
		}
	}

	private void SettingWaterTurret(int max, float distance)
	{
		GameObject waterTurretPrefab = PrefabManager.WaterTurretPrefab;
		if (waterTurretPrefab == null)
		{
			#if UNITY_EDITOR
			Debug.LogWarning("[UnitFireDisaster] WaterTurretプレハブが見つからないためスキップします");
			#endif
			return;
		}

		int num = max / 2;
		int num2 = max / 2;
		for (int i = 0; i < num; i++)
		{
			Vector3 setPoint = new Vector3((float)num + (float)i * distance + 2/1.4f*distance, 0f, (float)num2 - (float)i * distance + distance/2f);
			if (!((float)num + (float)i * distance > (float)max) && !((float)num2 - (float)i * distance < 0f))
			{
				GameObject instance = Object.Instantiate(waterTurretPrefab);
				WaterTurretCtrl component = instance.GetComponent<WaterTurretCtrl>();
				if (component != null)
				{
					#if UNITY_EDITOR
					Debug.Log("[UnitFireDisaster] Creating WaterTurret at " + setPoint);
					#endif
					component.CreateWaterTurretUnit(setPoint);
				}
				else
				{
					#if UNITY_EDITOR
					Debug.LogError("[UnitFireDisaster] WaterTurretCtrlコンポーネントが見つかりません");
					#endif
				}
			}
			else
			{
				break;
			}
		}
	}

	private void ChangeDemMeshSize()
	{
		GameObject gameObject = GameObject.Find("dem_");
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		if (!(component == null))
		{
			Bounds bounds = component.mesh.bounds;
			Vector3 size = new Vector3(0f, 0f, 0f);
			size.x = gameObject.transform.localScale.x;
			size.z = gameObject.transform.localScale.z;
			bounds.size = size;
			component.mesh.bounds = bounds;
		}
	}

	private void Start()
	{
		ChangeDemMeshSize();
		int num = 50;
		float distance = 1f;
		float distance2 = 6f / Mathf.Sqrt(2f);
		SettingWalls(num, distance2);
		SettingWaterTurret(num - 1, distance2);
		SettingCubes(num, distance);
		FireCubeCtrl.SpawnFireCube(new Vector3(0f, 1f, 0f));
	}

	private void Update()
	{
	}
}
