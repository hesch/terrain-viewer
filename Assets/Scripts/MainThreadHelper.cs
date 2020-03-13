using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using UnityEngine;

public class ThreadHelperTask
{
    public Action action;
    public bool completed = false;
    public bool canceled = false;

    public ThreadHelperTask(Action a)
    {
        this.action = a;
    }

    public void wait()
    {
        int i = 0;
        while (!completed)
        {
            Thread.Sleep(10);
            i++;
            if (i > 1000 || canceled)
            {
                Debug.LogWarning("canceled wait");
                return;
            }
        }
    }
}

public class MainThreadHelper : MonoBehaviour
{

    private static MainThreadHelper _instance = null;

    private ConcurrentQueue<ThreadHelperTask> tasks = new ConcurrentQueue<ThreadHelperTask>();

    public MainThreadHelper()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Debug.LogError("There should be only one instance");
        }
    }

    public static MainThreadHelper instance()
    {
        if (_instance == null)
        {
            Debug.LogError("There should be an instance of MainThreadHelper in this Scene");
        }
        return _instance;
    }

    public ThreadHelperTask scheduleOnMainThread(Action action)
    {

        var task = new ThreadHelperTask(action);
        if (Thread.CurrentThread.ManagedThreadId == 1)
        {
            action();
            task.completed = true;
            return task;
        }
        tasks.Enqueue(task);
        return task;
    }

    public void cancelAllPendingTasks()
    {
        
        while (!tasks.IsEmpty)
        {
            ThreadHelperTask task;
            if (tasks.TryDequeue(out task))
            {
                Debug.Log("canceled Task");
                task.canceled = true;
            }
        }
    }

    public void Update()
    {
        ThreadHelperTask task;
        if (tasks.TryDequeue(out task))
        {
            if (!task.canceled)
            {
                task.action();
                task.completed = true;
            }
        }
    }
}