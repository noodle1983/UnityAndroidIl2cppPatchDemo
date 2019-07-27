using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatRecordUIMgr : MonoBehaviour
{
    public GridLayoutGroup gridLayoutGroup;
    public StatRecordUI templateUI;

    List<StatRecordUI> statRecoredUIList = new List<StatRecordUI>();
    List<StatRecord> statRecordList = new List<StatRecord>();

    void Start()
    {
        statRecoredUIList.Add(templateUI);
        StartCoroutine(SupportPhoneStat.FetchSupportedStat(OnGotRecordData));
        RefreshUI();
        
    }

    void OnGotRecordData(StatRsp rsp)
    {
        if (rsp == null)
        {
            statRecordList.Clear();
            RefreshUI();
            return;
        }

        //Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        statRecordList = rsp.data;
        RefreshUI();

    }

    void RefreshUI()
    {
        statRecordList.Sort((a, b) => a.enter_scene.CompareTo(b.enter_scene));

        int recordIndex = 0;
        for(; recordIndex < statRecordList.Count; recordIndex++)
        {
            if (recordIndex >= statRecoredUIList.Count)
            {
                GameObject newObject = Object.Instantiate<GameObject>(templateUI.gameObject, gridLayoutGroup.transform);
                StatRecordUI recordUI = newObject.GetComponent<StatRecordUI>();
                statRecoredUIList.Add(recordUI);
            }
            StatRecordUI statRecordUI = statRecoredUIList[recordIndex];
            statRecordUI.ResetRecord(statRecordList[recordIndex]);
            statRecordUI.gameObject.SetActive(true);
        }

        for(; recordIndex < statRecoredUIList.Count; recordIndex++)
        {
            statRecoredUIList[recordIndex].gameObject.SetActive(false) ;
        }

        //gridLayoutGroup.
    }
}
