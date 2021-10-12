using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class AvatarThreadExecutor : SingletonModel<AvatarThreadExecutor>
{
    readonly System.Object mLock = new System.Object();
    readonly List<Action> mQueuedActions = new List<Action>();
    readonly List<Action> mExecutingActions = new List<Action>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        LoadInstance();
    }

    internal static void Queue(Action action)
    {
        if (action == null)
        {
            Debug.LogWarning("Action is null in Queue method.");
            return;
        }

        if (null != Instance)
        {
            lock (Instance.mLock)
            {
                Instance.mQueuedActions.Add(action);
            }
        }
    }

    void Update()
    {
        MoveQueuedActionsToExecuting();

        while (mExecutingActions.Count > 0)
        {
            Action action = mExecutingActions[0];
            mExecutingActions.RemoveAt(0);
            action();
        }
    }

    void MoveQueuedActionsToExecuting()
    {
        lock (mLock)
        {
            while (mQueuedActions.Count > 0)
            {
                Action action = mQueuedActions[0];
                mExecutingActions.Add(action);
                mQueuedActions.RemoveAt(0);
            }
        }
    }
}

