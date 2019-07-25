using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Monetization;


//https://unityads.unity3d.com/help/unity/integration-guide-unity?&_ga=2.224601178.1107452368.1563611240-190348090.1563367334#basic-implementation
public class UnityAdsHelper : MonoBehaviour
{
    static string androidGameID = "3228535";
    static bool enableTestMode = true;
    static bool isInit = false;

    string myPlacementId = "";

    public string bannerPlacementId;

    void Start()
    {
        Debug.Log("Running for Unity Ads initialization...");

        string gameID = androidGameID;

        if (!Monetization.isSupported)
        {
            Debug.LogWarning("Unity Ads is not supported on the current runtime platform.");
            return;
        }
        
        if (string.IsNullOrEmpty(gameID))
        {
            Debug.LogError("The game ID value is not set. A valid game ID is required to initialize Unity Ads.");
            return;
        }

        if (!isInit)
        {
            isInit = true;
            StartCoroutine(Initialize());
        }

        if (!string.IsNullOrEmpty(bannerPlacementId))
        {
            StartCoroutine(ShowBannerWhenReady());
        }
    }

    private IEnumerator Initialize()
    {
        {
            float initStartTime = Time.time;
            Monetization.Initialize(androidGameID, enableTestMode);
            while (!Monetization.isInitialized) { yield return new WaitForSeconds(0.1f); }
            Debug.Log(string.Format("Unity Monetization was initialized in {0:F1} seconds.", Time.time - initStartTime));
        }

        //{
        //    float initStartTime = Time.time;
        //    Advertisement.Initialize(androidGameID, enableTestMode);
        //    while (!Advertisement.isInitialized) { yield return new WaitForSeconds(0.1f); }
        //    Debug.Log(string.Format("Unity Ads was initialized in {0:F1} seconds.", Time.time - initStartTime));
        //}
    }

    IEnumerator ShowBannerWhenReady()
    {
        while (!Monetization.isInitialized) { yield return new WaitForSeconds(0.1f); }
        while (!Advertisement.isInitialized) { yield return new WaitForSeconds(0.1f); }
        while (!Advertisement.IsReady(bannerPlacementId))
        {
            yield return new WaitForSeconds(0.5f);
        }
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Show(bannerPlacementId);
    }

    public void ShowAds(string placementId, ShowAdFinishCallback onFinish)
    {
        myPlacementId = placementId;
        StartCoroutine(ShowAdWhenReady(onFinish));
    }

    private IEnumerator ShowAdWhenReady(ShowAdFinishCallback onFinish)
    {
        if (string.IsNullOrEmpty(myPlacementId)) { yield break; }

        while (!Advertisement.isInitialized) { yield return new WaitForSeconds(0.1f); }
        while (!Monetization.IsReady(myPlacementId)) {  yield return new WaitForSeconds(0.1f); }

        ShowAdPlacementContent ad = null;
        ad = Monetization.GetPlacementContent(myPlacementId) as ShowAdPlacementContent;
        if (ad != null) { ad.Show(onFinish); }
    }
}
