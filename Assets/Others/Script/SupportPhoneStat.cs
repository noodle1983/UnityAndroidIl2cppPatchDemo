using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SupportPhoneStat : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ReportEnterScene()); 
        
    }

    IEnumerator ReportEnterScene() 
    {
        string phoneId = SystemInfo.deviceUniqueIdentifier;
        string phoneType = SystemInfo.deviceModel;
        string androidVersion = SystemInfo.operatingSystem;
        int sceneNumber = SceneManager.GetActiveScene().buildIndex;
        int appVersion = 1;
        int random = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        string url = "http://noodle1983.gz01.bdysite.com/index.php/PatchRecord/login?";
        string param = "phone_id=" + Uri.EscapeDataString(phoneId)
             + "&phone_type=" + Uri.EscapeDataString(phoneType)
             + "&android_version=" + Uri.EscapeDataString(androidVersion)
             + "&enter_scene=" + sceneNumber
             + "&app_version=" + appVersion 
             + "&rand=" + random;
        string req = url + param;
        Debug.Log(req);

        UnityWebRequest www = UnityWebRequest.Get(req);
        www.SetRequestHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }
}
