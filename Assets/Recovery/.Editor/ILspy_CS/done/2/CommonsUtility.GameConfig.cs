// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// CommonsUtility.GameConfig
using CommonsUtility;
using UnityEngine;

internal sealed class GameConfig : MonoBehaviour
{
	private static GameConfig instance = null;

	internal static string _APP_GAME_MODE = "debug";

	internal static string _APP_LANG = "JP";

	internal static bool _STAGE_PADDLE_MODE = false;

	internal static bool _STAGE_RAIN_ABSORB_MODE = true;

	private static void APPSettings()
	{
		Application.targetFrameRate = 60;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	internal static GameConfig InitGameConfig()
	{
		APPSettings();
		return instance;
	}
}
