using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class BackgroundThreadGroup : Singleton<BackgroundThreadGroup>
{
    public List<BackgroundThread> threadGroup = new List<BackgroundThread>();
    uint threadNumber = 0;
    uint threadCount = 10;

    //run on main thread
    public BackgroundThreadGroup()
    {
        threadNumber = threadCount;
        for(int i = 0; i < threadNumber; i++)
        {
            var backgroundThread = new BackgroundThread();
            threadGroup.Add(backgroundThread);
        }
    }

    //run on main thread
    public void Process(uint jobId, BackgroundJob job)
    {
        uint hash = jobId % threadNumber;
        threadGroup[(int)hash].Process(job);
    }

    //run on main thread
    //run async run on bg thread
    //_call_back run on main thread
    public Coroutine ProcessAsync(uint jobId, BackgroundJob.Func runAsync, BackgroundJob.Func callbackResult)
    {
        uint hash = jobId % threadNumber;
        var backgroundThread = threadGroup[(int)hash];
        return backgroundThread.ProcessAsync(runAsync, callbackResult);
    }

}
