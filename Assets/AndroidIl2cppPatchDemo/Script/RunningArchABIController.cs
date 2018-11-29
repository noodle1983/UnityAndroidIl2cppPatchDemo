using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class RunningArchABIController : MonoBehaviour {

    // Use this for initialization
    void Start () {
        UnityEngine.UI.Text archABILabel = GetComponent<UnityEngine.UI.Text>();
        if (archABILabel != null) { archABILabel.text = "ARCH_ABI: " + Bootstrap.get_arch_abi(); }
    }

  
}
