using System;
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.UI;
using UnityEngine.Profiling;    // Profiler
using UnityEngine.SceneManagement;
using CommonsUtility;

public class DebugInfoCtrl : MonoBehaviour
{
    private static string _app_version = "";
    private float _time = 0;
    private float _fps = 0;
    private const float _updateInterval = 1.5f;

    private string GetAppVer()
    {
        if (_app_version != "")
        {
            return _app_version;
        }
        string app_versions = "BuildDate: " + GameObjectTreat.GetAppBuildDate() + Environment.NewLine;
        app_versions += "Application: " + GameObjectTreat.GetAppVersion();
        _app_version = app_versions;
        return _app_version;
    }

    private string GetAppFPS()
    {
        string fps = "";
        fps += "APP_FPS:" + Application.targetFrameRate;
        fps += " FPS:" + _fps;
        return fps;
    }

    private string GetAppDPI()
    {
        return "DP:"+Utility.GetScreenDp() + " DPI:" + Screen.dpi;
    }

    private string GetGameSpeed()
    {
        return "SIM SPEED:" + GameSpeedManager.GetGameSpeed();
    }

    private string GetWind()
    {
        return " Wind:" + WindController.GetDirectionText() + " " + WindController.GetWindSpeed() + "m/s";
    }

    private string GetAppMemory()
    {
        // https://docs.unity3d.com/ScriptReference/Profiling.Profiler.html
        string mem = "";
        mem += "Total:" + GetMemoryString(Profiler.GetTotalReservedMemoryLong());
        mem += "Alloc:" + GetMemoryString(Profiler.GetTotalAllocatedMemoryLong());
        mem += "Unuse:" + GetMemoryString(Profiler.GetTotalUnusedReservedMemoryLong());
        return mem;
    }


    private string GetMemoryString(long memory)
    {
        string ret = "";
        ret = String.Format("{0:#,0} MB ", ConvBytesToMGBytes(memory));
        return ret;
    }

    private long ConvBytesToMGBytes(long memory)
    {
        return memory/(1024*1024);
    }

    // 充電状況
    private string GetBatteryStatusInfo()
    {
        // Unknown	The device's battery status cannot be determined. If battery status is not available on your target platform, SystemInfo.batteryStatus will return this value.
        // Charging	Device is plugged in and charging.
        // Discharging	Device is unplugged and discharging.
        // NotCharging	Device is plugged in, but is not charging.
        // Full	Device is plugged in and the battery is full.
        string batteryStatus = "";
        batteryStatus += "BatteryStatus:";
        // batteryStatus += "BatteryStatus:" + SystemInfo.batteryStatus.ToString();
        if (SystemInfo.batteryStatus == BatteryStatus.Charging)
        {
            batteryStatus += BatteryStatus.Charging.ToString();
        }
        return batteryStatus;        
    }

    // 通信環境
    private string GetReachabilityInfo()
    {
        string reachabilityText = "";
        // reachabilityText = Application.internetReachability.ToString();
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            reachabilityText = "[discon]";
        }
        //Check if the device can reach the internet via a carrier data network
        else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            reachabilityText = "[Carrier]";
        }
        //Check if the device can reach the internet via a LAN
        else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            reachabilityText = "[LAN/Wi-Fi]";
        }
        return reachabilityText;
    }

    private string GetSceneName()
    {
        return "Scene:" + SceneManager.GetActiveScene().name;
    }


    private void InitDebugWindow()
    {
        if (GameConfig._APP_GAME_MODE != GlobalConst.GAME_MODE_DEBUG)
        {
            this.gameObject.SetActive(false);
            return;
        }

        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        // Debug.Log("DebugInfoCtrl.InitDebugWindow()");

        // GameObject debugWindow = GameObject.Find("DebugWindow");
        GameObject debugWindow = this.gameObject;
        String debug_info = "";
        if (debugWindow != null)
        {
            // Debug.Log("DebugWindow is found");
            debug_info = GetSceneName();
            debug_info += Environment.NewLine + GetAppVer();
            debug_info += Environment.NewLine + GetAppFPS();
            debug_info += Environment.NewLine + GetAppDPI();
            debug_info += Environment.NewLine + GetAppMemory();
            debug_info += Environment.NewLine + GetBatteryStatusInfo();
            debug_info += Environment.NewLine + GetReachabilityInfo();
            debug_info += Environment.NewLine + GetGameSpeed();
            debug_info += " " + GetWind();
            debugWindow.GetComponentInChildren<Text>().text = debug_info;

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
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    private void Update()
    {
        // Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        _time += Time.deltaTime;
        if (_time > _updateInterval)
        {
            _fps = 1.0f / Time.deltaTime;
            InitDebugWindow();
            _time = 0;
        }
    }


}
