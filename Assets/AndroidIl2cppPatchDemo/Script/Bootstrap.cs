using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Bootstrap
{
#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("bootstrap")]
    public static extern string get_arch_abi();

    [DllImport("bootstrap")]
    public static extern string use_data_dir(string _data_path, string _apk_path);
#else
    public static string get_arch_abi() { return "armeabi-v7a"; }
    public static string use_data_dir(string _data_path, string _apk_path) { return ""; }
#endif

    public static void reboot_app()
    {
        using (AndroidJavaClass unity_player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject current_activity = unity_player.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject pm = current_activity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);
            //intent.Call<AndroidJavaObject>("setFlags", 0x20000000);//Intent.FLAG_ACTIVITY_SINGLE_TOP
            intent.Call<AndroidJavaObject>("setFlags", 0x04000000 | 0x00008000 | 0x10000000);//Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_CLEAR_TASK | Intent.FLAG_ACTIVITY_NEW_TASK

            AndroidJavaClass pending_intent = new AndroidJavaClass("android.app.PendingIntent");
            AndroidJavaObject content_intent = pending_intent.CallStatic<AndroidJavaObject>("getActivity", current_activity, 0, intent, 0x8000000); //PendingIntent.FLAG_UPDATE_CURRENT = 134217728 [0x8000000]
            AndroidJavaObject alarm_manager = current_activity.Call<AndroidJavaObject>("getSystemService", "alarm");
            AndroidJavaClass system = new AndroidJavaClass("java.lang.System");
            long current_time = system.CallStatic<long>("currentTimeMillis");
            alarm_manager.Call("set", 1, current_time + 1000, content_intent); // android.app.AlarmManager.RTC = 1 [0x1]

            Debug.LogError("alarm_manager set time " + current_time + 1000);
            current_activity.Call("finish");

            AndroidJavaClass process = new AndroidJavaClass("android.os.Process");
            int pid = process.CallStatic<int>("myPid");
            process.CallStatic("killProcess", pid);
        }
    }
}
