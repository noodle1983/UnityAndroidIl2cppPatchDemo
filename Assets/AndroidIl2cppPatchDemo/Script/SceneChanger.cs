using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Monetization;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour {
    public string sceneName = "0";
    public MessageBoxUI messageBox;

    // Use this for initialization
    void Start () {
		
	}

    public void OnClickLoadScene()
    {
        if (sceneName != "0" && Monetization.isSupported)
        {
            GetComponent<UnityAdsHelper>().ShowAds("rewardedVideo", (onFinish) =>
            {
                if (onFinish == UnityEngine.Monetization.ShowResult.Finished)
                {
                    SceneManager.LoadScene(sceneName);
                }
            });
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
    
}
