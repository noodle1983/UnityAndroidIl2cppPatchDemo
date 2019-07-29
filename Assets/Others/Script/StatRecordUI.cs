using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class StatRecordUI : MonoBehaviour
{
    public Text phoneType;
    public Text enterScene;

    StatRecord statRecord = null;
    void Start()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        if (statRecord == null)
        {
            phoneType.text = string.Empty;
            enterScene.text = string.Empty;
            return;
        }

        phoneType.text = statRecord.phone_type + "\n(" + statRecord.android_version + ")";
        int enterSceneBits = statRecord.enter_scene;
        StringBuilder enterSceneStr = new StringBuilder();
        for (int i = 0; i < 3; i++)
        {
            if ((enterSceneBits & (1 << i)) != 0)
            {
                enterSceneStr.Append(i.ToString() + ",");
            }
        }
        if (enterSceneStr.Length > 0) { enterSceneStr.Remove(enterSceneStr.Length - 1, 1); }
        enterScene.text = enterSceneStr.ToString();

        enterScene.color = ((enterSceneBits & (1 << 2)) != 0) ? Color.green : Color.yellow;
        phoneType.color = enterScene.color;
    }

    public void ResetRecord(StatRecord record)
    {
        this.statRecord = record;
        if (phoneType == null)
        {
            return;
        }

        RefreshUI();
    }
}
