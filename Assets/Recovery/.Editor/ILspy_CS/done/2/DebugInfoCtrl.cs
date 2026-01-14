// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// DebugInfoCtrl
using System;
using CommonsUtility;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugInfoCtrl : MonoBehaviour
{
	private static string _app_version = "";

	private float _time;

	private float _fps;

	private const float _updateInterval = 1.5f;

	private string GetAppVer()
	{
		if (_app_version != "")
		{
			return _app_version;
		}
		_app_version = string.Concat("BuildDate: " + GameObjectTreat.GetAppBuildDate() + Environment.NewLine, "Application: ", GameObjectTreat.GetAppVersion());
		return _app_version;
	}

	private string GetAppFPS()
	{
		return string.Concat("" + "APP_FPS:" + Application.targetFrameRate, " FPS:", _fps.ToString());
	}

	private string GetAppDPI()
	{
		return "DP:" + Utility.GetScreenDp() + " DPI:" + Screen.dpi;
	}

	private string GetGameSpeed()
	{
		return "SIM SPEED:" + GameSpeedCtrl.GetGameSpeed();
	}

	private string GetWind()
	{
		return " Wind:" + WindCtrl.GetDirectionText() + " " + WindCtrl.GetWindSpeed() + "m/s";
	}

	private string GetAppMemory()
	{
		return string.Concat(string.Concat("" + "Total:" + GetMemoryString(Profiler.GetTotalReservedMemoryLong()), "Alloc:", GetMemoryString(Profiler.GetTotalAllocatedMemoryLong())), "Unuse:", GetMemoryString(Profiler.GetTotalUnusedReservedMemoryLong()));
	}

	private string GetMemoryString(long memory)
	{
		return $"{ConvBytesToMGBytes(memory):#,0} MB ";
	}

	private long ConvBytesToMGBytes(long memory)
	{
		return memory / 1048576;
	}

	private string GetBatteryStatusInfo()
	{
		string text = "";
		text += "BatteryStatus:";
		if (SystemInfo.batteryStatus == BatteryStatus.Charging)
		{
			text += BatteryStatus.Charging;
		}
		return text;
	}

	private string GetReachabilityInfo()
	{
		string result = "";
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			result = "[discon]";
		}
		else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
		{
			result = "[Carrier]";
		}
		else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
		{
			result = "[LAN/Wi-Fi]";
		}
		return result;
	}

	private string GetSceneName()
	{
		return "Scene:" + SceneManager.GetActiveScene().name;
	}

	private void InitDebugWindow()
	{
		if (GameConfig._APP_GAME_MODE != "debug")
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		GameObject gameObject = base.gameObject;
		string text = "";
		if (gameObject != null)
		{
			text = GetSceneName();
			text = text + Environment.NewLine + GetAppVer();
			text = text + Environment.NewLine + GetAppFPS();
			text = text + Environment.NewLine + GetAppDPI();
			text = text + Environment.NewLine + GetAppMemory();
			text = text + Environment.NewLine + GetBatteryStatusInfo();
			text = text + Environment.NewLine + GetReachabilityInfo();
			text = text + Environment.NewLine + GetGameSpeed();
			text = text + " " + GetWind();
			gameObject.GetComponentInChildren<Text>().text = text;
		}
		else
		{
			Debug.Log("DebugWindow is not found");
		}
	}

	private void Awake()
	{
		GameConfig.InitGameConfig();
		InitDebugWindow();
	}

	private void Update()
	{
		_time += Time.deltaTime;
		if (_time > 1.5f)
		{
			_fps = 1f / Time.deltaTime;
			InitDebugWindow();
			_time = 0f;
		}
	}
}
