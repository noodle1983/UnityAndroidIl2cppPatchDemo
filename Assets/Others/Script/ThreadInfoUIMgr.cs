using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreadInfoUIMgr : MonoBehaviour
{
    public GridLayoutGroup gridLayoutGroup;
    public ThreadInfoUI templateUI;

    List<ThreadInfoUI> threadInfoUIList = new List<ThreadInfoUI>();

    void Start()
    {
        threadInfoUIList.Add(templateUI);
        RefreshUI();
        
    }

    void RefreshUI()
    {
        var threadGroup = BackgroundThreadGroup.Instance.threadGroup;     
        for(int i = 0; i < threadGroup.Count; i++)
        {
            if (i >= threadInfoUIList.Count)
            {
                GameObject newObject = Object.Instantiate<GameObject>(templateUI.gameObject, gridLayoutGroup.transform);
                ThreadInfoUI recordUI = newObject.GetComponent<ThreadInfoUI>();
                threadInfoUIList.Add(recordUI);
            }
            ThreadInfoUI ThreadInfoUI = threadInfoUIList[i];
            ThreadInfoUI.ResetRecord(i, threadGroup[i]);
            ThreadInfoUI.gameObject.SetActive(true);
        }
    }
}
