using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Threading;

public class BackgroundJob
{
    private bool isDone = false;  // access with lock
    private object result = null;  // lock by is_done
    private Func func = null;      // only access by bg thread after init
    private string jobName;

    public delegate void Func(BackgroundJob job);

    public BackgroundJob(Func func, string jobName)
    {
        //run on main thread
        this.func = func;
        this.jobName = jobName;
    }

    // run on main thread
    public bool IsDone() {  lock (this) { return isDone; }  }

    // run on bg thread
    public void SetDone() { lock (this) { isDone = true; } }

    //run on main thread
    public object GetResult() { return (IsDone()) ? result : null; }

    //run on bg thread
    public void SetResult(object _result) { result = _result ; }

    //run on bg thread
    public virtual void ExecJob() { func(this); }

    //run on bg thread
    public string GetJobName() { return jobName; }
}

public class BackgroundThread
{
    private Queue<BackgroundJob> waitingQueue = new Queue<BackgroundJob>();
    private Thread thread;

    private object runningJobNameMutex = new object();
    private string runningJobName;

    //run on main thread
    public BackgroundThread()
    {
        thread = new Thread(RunJob);
        thread.Start();
    }

    public string GetRunningJobName()
    {
        lock (runningJobNameMutex)
        {
            return runningJobName;
        }
    }

    //run on main thread
    public void Process(BackgroundJob job)
    {
        if (job == null) { return; }

        lock (waitingQueue)
        {
            bool jobQueueEmpty = waitingQueue.Count > 0;
            waitingQueue.Enqueue(job);
            Monitor.Pulse(waitingQueue);
        }

    }

    //run on main thread
    //run async run on bg thread
    //_call_back run on main thread
    public Coroutine ProcessAsync(BackgroundJob.Func runAsync, BackgroundJob.Func callbackResult, string jobName = "")
    {
        return BackgroundThreadGroup.Instance.StartCoroutine(ProcessAsyncCoroutine(runAsync, callbackResult, jobName));
    }

    //run on main thread
    public IEnumerator ProcessAsyncCoroutine(BackgroundJob.Func runAsync, BackgroundJob.Func callbackResult, string jobName = "")
    {
        BackgroundJob job = new BackgroundJob(runAsync, jobName);
        Process(job);
        while (!job.IsDone())
        {
            yield return null;
        }

        if (callbackResult != null)
        {
            callbackResult(job);
        }
    }

    //run on bg thread
    private void RunJob()
    {
        while (thread != null)
        {
            BackgroundJob job = null;
            lock (waitingQueue)
            {
                if (waitingQueue.Count > 0)
                {
                    job = waitingQueue.Dequeue();
                }
            }

            if (job != null)
            {
                lock(runningJobNameMutex)
                {
                    runningJobName = job.GetJobName();
                }
                job.ExecJob();
                job.SetDone();
            }
            else
            {
                lock (runningJobNameMutex)
                {
                    runningJobName = "No Job.";
                }
                lock (waitingQueue)
                {
                    Monitor.Wait(waitingQueue, 100);
                }
            }
        }
    }
}
