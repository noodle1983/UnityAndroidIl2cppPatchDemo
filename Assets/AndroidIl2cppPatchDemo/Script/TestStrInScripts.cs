using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestStrInScripts : MonoBehaviour
{
    public Text text;
    const string t = "string in scripts: base version";

    // Start is called before the first frame update
    void Start()
    {
        text.text = t;
    }
}
