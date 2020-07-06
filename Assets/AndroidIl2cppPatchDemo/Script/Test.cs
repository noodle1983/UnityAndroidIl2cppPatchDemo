using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    
    public Text tx;


    const string t = "new string";

    // Start is called before the first frame update
    void Start()
    {
        tx.text = t;
    }
}
