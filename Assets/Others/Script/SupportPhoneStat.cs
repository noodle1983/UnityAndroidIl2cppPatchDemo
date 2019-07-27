using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class StatRecord
{
    public string phone_type;
    public string android_version;
    public int app_version;
    public int enter_scene;
}

public class StatRsp
{
    public int code;
    public List<StatRecord> data;
}

public class SupportPhoneStat : MonoBehaviour
{
    static readonly string HOST = "http://unitypatchdemo.noodle1983.cn";
    static SupportPhoneStat instance;

    public delegate void OnResponse(StatRsp rsp);

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        StartCoroutine(ReportEnterScene());      
    }

    IEnumerator ReportEnterScene() 
    {
        string phoneId = SystemInfo.deviceUniqueIdentifier;
        string phoneType = SystemInfo.deviceModel;
        string androidVersion = SystemInfo.operatingSystem;
        int sceneNumber = 1;
        int appVersion = 1;
        int random = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        string url = HOST + "/index.php/PatchRecord/login?";
        string param = "phone_id=" + Uri.EscapeDataString(phoneId)
             + "&phone_type=" + Uri.EscapeDataString(phoneType)
             + "&android_version=" + Uri.EscapeDataString(androidVersion)
             + "&enter_scene=" + sceneNumber
             + "&app_version=" + appVersion 
             + "&rand=" + random;
        string req = url + param;

        UnityWebRequest www = UnityWebRequest.Get(req);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
        //GetSupportedStat((rsp) => { if (rsp != null) { Debug.Log(JsonConvert.SerializeObject(rsp)); } });
    }

    public static void GetSupportedStat(OnResponse onRsp)
    {
        if (instance == null) 
        {
            Debug.LogError("SupportPhoneStat instance = null");
            onRsp(null); return; 
        }
        instance.StartCoroutine(FetchSupportedStat(onRsp)); 
    }

    public static IEnumerator FetchSupportedStat(OnResponse onRsp)
    {
        string req = HOST + "/index.php/PatchRecord/dump_stat";
        UnityWebRequest www = UnityWebRequest.Get(req);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            onRsp(null);
            yield break;
        }

        Debug.Log(www.downloadHandler.text);
        StatRsp rsp = JsonConvert.DeserializeObject<StatRsp>(www.downloadHandler.text);
        onRsp(rsp);
    }
}
