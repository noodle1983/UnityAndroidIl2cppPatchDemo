using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnityAssetStoreNavigator : MonoBehaviour {

    // Use this for initialization
    void Start () {
		
	}

    public void OnClickOpenURL()
    {
        Application.OpenURL("http://u3d.as/1mmM");
    }
    
}
