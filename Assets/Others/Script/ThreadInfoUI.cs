using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ThreadInfoUI : MonoBehaviour
{
    public Text index;
    public Text desc;
    int id;
    BackgroundThread backgroundThread;

    private void Start()
    {
        this.index.text = id.ToString() + ".";

    }

    void Update()
    {
        if (backgroundThread == null || desc == null)
        {
            return;
        }

        desc.text = backgroundThread.GetRunningJobName() ;      
    }

    public void ResetRecord(int id, BackgroundThread backgroundThread)
    {
        this.id = id;
        this.backgroundThread = backgroundThread;
        if (desc == null)
        {
            return;
        }
        this.index.text = id.ToString() + ".";
    }
}
