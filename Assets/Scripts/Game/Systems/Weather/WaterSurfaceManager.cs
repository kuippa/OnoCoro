using CommonsUtility;
using System.Reflection;
using UnityEngine;
using Debug = CommonsUtility.Debug;

public class WaterSurfaceManager : MonoBehaviour
{
	private GameObject _waterSurface;

	private GameObject _ocean;

	private const float WATER_RISE_PAR_RAIN = 0.005f;

	private const float IGNORE_RAIN_SIZE = 0.1f;

	// private const float DEFAULT_DISTAL_WIND_SPEED = 152f;

	private void Start()
	{
		// Debug.Log("[WaterSurfaceCtrl] Start called");
		// SetDistalWindSpeed(DEFAULT_DISTAL_WIND_SPEED);
	}

	// 遠方風速を変更して波の高さを変える
	internal void SetDistalWindSpeed(float windSpeed)
	{
		GameObject waterSurface = GetWaterSurface();
		if (waterSurface == null)
		{
			// Debug.LogWarning("[WaterSurfaceCtrl] watersurface が null です");
			return;
		}

		// watersurface の子オブジェクト "Ocean" を探す
		Transform oceanTransform = waterSurface.transform.Find("Ocean");
		if (oceanTransform == null)
		{
			// Debug.LogWarning("[WaterSurfaceCtrl] 子オブジェクト 'Ocean' が見つかりません");
			return;
		}

		// Ocean オブジェクトから WaterSurface コンポーネントを取得
		Component waterComponent = oceanTransform.GetComponent("WaterSurface");
		if (waterComponent == null)
		{
			// Debug.LogWarning("[WaterSurfaceCtrl] WaterSurface コンポーネントが見つかりません");
			return;
		}

		// Simulation Swell の遠方風速（largeWindSpeed）を設定
		FieldInfo largeWindSpeedField = waterComponent.GetType().GetField("largeWindSpeed");
		if (largeWindSpeedField == null)
		{
			// Debug.LogWarning("[WaterSurfaceCtrl] largeWindSpeed フィールドが見つかりません");
			return;
		}

		largeWindSpeedField.SetValue(waterComponent, windSpeed);

		// HDRP に変更を通知して再計算させる
		MethodInfo markDirtyMethod = waterComponent.GetType().GetMethod("MarkDirty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (markDirtyMethod != null)
		{
			markDirtyMethod.Invoke(waterComponent, null);
		}
	}

	internal float GetDistalWindSpeed()
	{
		GameObject waterSurface = GetWaterSurface();
		if (waterSurface == null)
		{
			return 0f;
		}

		// watersurface の子オブジェクト "Ocean" を探す
		Transform oceanTransform = waterSurface.transform.Find("Ocean");
		if (oceanTransform == null)
		{
			return 0f;
		}

		// Ocean オブジェクトから WaterSurface コンポーネントを取得
		Component waterComponent = oceanTransform.GetComponent("WaterSurface");
		if (waterComponent == null)
		{
			return 0f;
		}

		FieldInfo largeWindSpeedField = waterComponent.GetType().GetField("largeWindSpeed");
		if (largeWindSpeedField == null)
		{
			return 0f;
		}

		object value = largeWindSpeedField.GetValue(waterComponent);
		if (value is float floatValue)
		{
			return floatValue;
		}

		return 0f;
	}

	private GameObject GetWaterSurface()
	{
		if (_waterSurface == null)
		{
			// シーンに既存の "watersurface" オブジェクトがあれば優先して使用する
			_waterSurface = GameObject.Find("watersurface");
			if (_waterSurface != null)
			{
				return _waterSurface;
			}

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

	/// <summary>
	/// 海面（子オブジェクト Ocean）のワールド高さを取得する。
	///
	/// [重要] 親の watersurface は Ocean / River / Water Foam Generator を
	/// まとめて持つホルダーで、その Y は海面の高さではない。
	/// 舞鶴では親 Y=6.97・Ocean の localY=-6.25 で、実際の海面は 0.72 になる。
	/// 浸水判定などはこちらを使うこと
	/// </summary>
	internal float GetOceanHeight()
	{
		Transform oceanTransform = GetOceanTransform();
		if (oceanTransform == null)
		{
			return GetWaterSurfaceHeight();
		}
		return oceanTransform.position.y;
	}

	/// <summary>
	/// 海面（Ocean）のワールド高さを設定する。
	/// 親ごと動かす SetWaterSurfaceHeight と違い、海面だけを上下させる
	/// </summary>
	internal void SetOceanHeight(float height)
	{
		Transform oceanTransform = GetOceanTransform();
		if (oceanTransform == null)
		{
			Debug.LogWarning("[WaterSurfaceManager] 子オブジェクト 'Ocean' が見つからないため海面を動かせません");
			return;
		}

		Vector3 position = oceanTransform.position;
		position.y = height;
		oceanTransform.position = position;
	}

	private Transform GetOceanTransform()
	{
		GameObject waterSurface = GetWaterSurface();
		if (waterSurface == null)
		{
			return null;
		}
		return waterSurface.transform.Find("Ocean");
	}

	/// <summary>
	/// 水面の色を変更する（HDRP WaterSurface の屈折色・散乱色）。
	///
	/// HDRP のバージョン差でフィールド名が変わりうるため、
	/// 既存の largeWindSpeed と同様にリフレクションで触っている。
	/// 見つからないフィールドは黙って飛ばす（演出なので落とさない）
	/// </summary>
	internal void SetWaterColor(Color color)
	{
		Component waterComponent = GetOceanWaterComponent();
		if (waterComponent == null)
		{
			Debug.LogWarning("[WaterSurfaceManager] WaterSurface コンポーネントが見つからないため色を変更できません");
			return;
		}

		TrySetColorField(waterComponent, "refractionColor", color);
		TrySetColorField(waterComponent, "scatteringColor", color);

		MethodInfo markDirtyMethod = waterComponent.GetType().GetMethod(
			"MarkDirty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (markDirtyMethod != null)
		{
			markDirtyMethod.Invoke(waterComponent, null);
		}
	}

	private void TrySetColorField(Component waterComponent, string fieldName, Color color)
	{
		FieldInfo field = waterComponent.GetType().GetField(fieldName);
		if (field == null || field.FieldType != typeof(Color))
		{
			return;
		}
		field.SetValue(waterComponent, color);
	}

	/// <summary>
	/// 子オブジェクト "Ocean" の WaterSurface コンポーネントを取得する
	/// </summary>
	private Component GetOceanWaterComponent()
	{
		GameObject waterSurface = GetWaterSurface();
		if (waterSurface == null)
		{
			return null;
		}

		Transform oceanTransform = waterSurface.transform.Find("Ocean");
		if (oceanTransform == null)
		{
			return null;
		}
		return oceanTransform.GetComponent("WaterSurface");
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
