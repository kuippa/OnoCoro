// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Raining
using CommonsUtility;
using UnityEngine;

public class Raining : MonoBehaviour
{
	private bool _is_rain;

	private float _time;

	private GameObject _eventSystem;

	private static GameObject _parent_holder;

	private const string _RAIN_PARENT_NAME = "Rains";

	internal const float INTERVAL_RAIN = 0.02f;

	internal const float ABOVE_POSITION = 280f;

	internal void SetRainMode(bool isRain)
	{
		if (isRain)
		{
			_is_rain = true;
			return;
		}
		_is_rain = false;
		DeleteAllRain();
		DeleteAllPuddle();
	}

	private void DeleteAllRain()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag(GameEnum.TagType.RainDrop.ToString());
		for (int i = 0; i < array.Length; i++)
		{
			GameObjectTreat.DestroyAll(array[i]);
		}
	}

	private void DeleteAllPuddle()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Puddle.ToString());
		for (int i = 0; i < array.Length; i++)
		{
			GameObjectTreat.DestroyAll(array[i]);
		}
	}

	private void RainDrops()
	{
		GameObject rainDropPrefab = PrefabManager.RainDropPrefab;
		Vector3 demRndAbovePosition = DemCtrl.GetDemRndAbovePosition(280f);
		Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
		GameObject obj = Object.Instantiate(rainDropPrefab, demRndAbovePosition, rotation);
		obj.tag = GameEnum.TagType.RainDrop.ToString();
		obj.name = GameEnum.TagType.RainDrop.ToString() + Time.time;
		Transform holderParentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_holder, "Rains");
		obj.transform.SetParent(holderParentTransform);
	}

	internal void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
		{
			_eventSystem = GameObjectTreat.GetEventSystem(_eventSystem);
			WeatherCtrl orAddComponent = GameObjectTreat.GetOrAddComponent<WeatherCtrl>(_eventSystem);
			float toggleRainStrength = orAddComponent.GetToggleRainStrength();
			orAddComponent.ChangeWeather(toggleRainStrength);
		}
	}

	private void Awake()
	{
	}

	private void Update()
	{
		if (_is_rain)
		{
			float num = 0.02f / GameSpeedCtrl.GetGameSpeed();
			_time += Time.deltaTime;
			if (_time > num)
			{
				_time = 0f;
				RainDrops();
			}
		}
	}
}
