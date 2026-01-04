// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// EnvironmentLight
using UnityEngine;

public class EnvironmentLight : MonoBehaviour
{
	private const int LIGHT_MODE_DEFAULT = 0;

	private const int LIGHT_MODE_DARK = 1;

	private const int SHADOW_MODE_DEFAULT = 0;

	private const int SHADOW_MODE_ULTRA = 0;

	internal void SetLightOptions(int mode)
	{
	}

	private Light GetLight()
	{
		GameObject gameObject = GameObject.Find("Directional Light");
		if (gameObject != null)
		{
			return gameObject.GetComponent<Light>();
		}
		Light[] array = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
		if (array.Length != 0)
		{
			Light[] array2 = array;
			foreach (Light light in array2)
			{
				if (light.type == LightType.Directional)
				{
					return light;
				}
			}
			Debug.Log("Directional Light not found");
			return array[0];
		}
		return null;
	}

	private void Awake()
	{
		GetLight();
	}
}
