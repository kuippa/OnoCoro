using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

public class WaterSurfaceCtrl : MonoBehaviour
{
	private GameObject _waterSurface;

	private GameObject _ocean;

	private const float WATER_RISE_PAR_RAIN = 0.005f;

	private const float IGNORE_RAIN_SIZE = 0.1f;

	private GameObject GetWaterSurface()
	{
		if (_waterSurface == null)
		{
			GameObject waterSurfacePrefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.WaterSurface);
			if (waterSurfacePrefab == null)
			{
				Debug.LogWarning("[WaterSurfaceCtrl] watersurface プレファブが見つかりません");
				_waterSurface = GameObjectTreat.GetOrNewGameObject(_waterSurface, "watersurface");
				return _waterSurface;
			}
			
			_waterSurface = Instantiate(waterSurfacePrefab);
			_waterSurface.name = "watersurface";
		}
		return _waterSurface;
	}

	private float GetWaterSurfaceHeight()
	{
		return GetWaterSurface().transform.position.y;
	}

	internal void SetWaterSurfaceHeight(float height)
	{
		Vector3 position = GetWaterSurface().transform.position;
		position.y = height;
		GetWaterSurface().transform.position = position;
	}

	internal void RainDropIntoNaraku(GameObject rainDrop)
	{
		if (!(rainDrop.transform.localScale.y <= 0.1f))
		{
			float waterSurfaceHeight = GetWaterSurfaceHeight();
			waterSurfaceHeight += 0.005f * GameSpeedManager.GetGameSpeed();
			SetWaterSurfaceHeight(waterSurfaceHeight);
		}
	}
}
