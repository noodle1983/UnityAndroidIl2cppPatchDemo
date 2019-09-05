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
        SceneManager.LoadScene(sceneName);
    }
    
}
