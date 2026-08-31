using CommonsUtility;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Debug = CommonsUtility.Debug;

public class WaterSurfaceManager : MonoBehaviour
{
	private GameObject _waterSurface;

	private GameObject _ocean;

	/// <summary>実行中の潮位補間（新しい指示が来たら止める）</summary>
	private Coroutine _tideCoroutine = null;

	private const float WATER_RISE_PAR_RAIN = 0.005f;

	private const float IGNORE_RAIN_SIZE = 0.1f;

	// private const float DEFAULT_DISTAL_WIND_SPEED = 152f;

	private void Start()
	{
		// SetDistalWindSpeed(DEFAULT_DISTAL_WIND_SPEED);
	}

	// 遠方風速を変更して波の高さを変える
	internal void SetDistalWindSpeed(float windSpeed)
	{
		GameObject waterSurface = GetWaterSurface();
		if (waterSurface == null)
		{
			return;
		}

		// watersurface の子オブジェクト "Ocean" を探す
		Transform oceanTransform = waterSurface.transform.Find("Ocean");
		if (oceanTransform == null)
		{
			return;
		}

		// Ocean オブジェクトから WaterSurface コンポーネントを取得
		Component waterComponent = oceanTransform.GetComponent("WaterSurface");
		if (waterComponent == null)
		{
			return;
		}

		// Simulation Swell の遠方風速（largeWindSpeed）を設定
		FieldInfo largeWindSpeedField = waterComponent.GetType().GetField("largeWindSpeed");
		if (largeWindSpeedField == null)
		{
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

	/// <summary>
	/// 海面を指定秒かけて目標の高さまで動かす。
	///
	/// 時刻指定のイベントで 10cm ずつ上げると、変化が階段状に見えてしまう。
	/// こちらは毎フレーム補間するので、0.01 刻みよりさらになめらかになる。
	/// Time.deltaTime を使うため倍速・一時停止にも追従する。
	///
	/// 実行中に新しい指示が来たら前の補間は破棄して上書きする
	/// </summary>
	internal void SetOceanHeightOverTime(float targetHeight, float duration)
	{
		if (_tideCoroutine != null)
		{
			StopCoroutine(_tideCoroutine);
			_tideCoroutine = null;
		}

		if (duration <= 0f)
		{
			SetOceanHeight(targetHeight);
			return;
		}
		_tideCoroutine = StartCoroutine(RaiseOceanRoutine(targetHeight, duration));
	}

	private IEnumerator RaiseOceanRoutine(float targetHeight, float duration)
	{
		float startHeight = GetOceanHeight();
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed = elapsed + Time.deltaTime;
			float progress = Mathf.Clamp01(elapsed / duration);
			SetOceanHeight(Mathf.Lerp(startHeight, targetHeight, progress));
			yield return null;
		}

		SetOceanHeight(targetHeight);
		_tideCoroutine = null;
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
	internal void SetWaterColor(Color color, float absorptionDistance = 0f)
	{
		Component waterComponent = GetOceanWaterComponent();
		if (waterComponent == null)
		{
			Debug.LogWarning("[WaterSurfaceManager] WaterSurface コンポーネントが見つからないため色を変更できません");
			return;
		}

		TrySetColorField(waterComponent, "refractionColor", color);
		TrySetColorField(waterComponent, "scatteringColor", color);

		// absorptionDistance は光が吸収されきるまでの距離(m)。
		// 既定 5.0 では澄んで水底が透けるため、浸水範囲が見分けにくい。
		// 小さくするほど濁って不透明になり、どこが浸かっているか一目で分かる
		if (absorptionDistance > 0f)
		{
			TrySetFloatField(waterComponent, "absorptionDistance", absorptionDistance);
		}

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
			Debug.LogWarning($"[WaterSurfaceManager] WaterSurface に {fieldName} がありません");
			return;
		}
		field.SetValue(waterComponent, color);
	}

	private void TrySetFloatField(Component waterComponent, string fieldName, float value)
	{
		FieldInfo field = waterComponent.GetType().GetField(fieldName);
		if (field == null || field.FieldType != typeof(float))
		{
			Debug.LogWarning($"[WaterSurfaceManager] WaterSurface に {fieldName} がありません");
			return;
		}
		field.SetValue(waterComponent, value);
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
