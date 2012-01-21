using System;
using System.Collections.Generic;
using System.Threading;
using org.critterai.nmgen.intern;

namespace org.critterai.nmgen
{
	public class ThreadedBuildManager
	{
        private readonly List<BuildTask> mActiveTasks;
        private readonly Queue<BuildTask> mTaskQueue = new Queue<BuildTask>();

        private readonly int mMaxActive;

        public int MaxActiveTasks { get { return mMaxActive; } }

        public int ActiveTaskCount { get { return mActiveTasks.Count; } }
        public int QueuedTaskCount { get { return mTaskQueue.Count; } }

        public ThreadedBuildManager(int maxActiveTasks)
        {
            mMaxActive = Math.Max(1, maxActiveTasks);
            mActiveTasks = new List<BuildTask>(mMaxActive);
        }

        ~ThreadedBuildManager()
        {
            AbortAll("Build manager disposed.");
        }

        public BuildRequest RequestBuild(IncrementalBuilder builder)
        {
            BuildTask task = BuildTask.Build(builder);

            if (task == null)
                return null;

            mTaskQueue.Enqueue(task);

            return task.Request;
        }

        public void RequestAbort(BuildRequest request, string reason)
        {
            foreach (BuildTask task in mActiveTasks)
            {
                if (task.Request == request)
                {
                    task.RequestAbort(reason);
                    return;
                }
            }

            foreach (BuildTask task in mTaskQueue)
            {
                if (task.Request == request)
                {
                    task.RequestAbort(reason);
                    return;
                }
            }
        }

        public void RequestAbort(List<BuildRequest> requests, string reason)
        {
            foreach (BuildTask task in mActiveTasks)
            {
                if (requests.Contains(task.Request))
                {
                    task.RequestAbort(reason);
                    return;
                }
            }

            foreach (BuildTask task in mTaskQueue)
            {
                if (requests.Contains(task.Request))
                {
                    task.RequestAbort(reason);
                    return;
                }
            }
        }

        public void AbortAll(string reason)
        {
            foreach (BuildTask task in mActiveTasks)
            {
                task.RequestAbort(reason);
            }
            mActiveTasks.Clear();


            while (mTaskQueue.Count > 0)
            {
                mTaskQueue.Dequeue().RequestAbort(reason);
            }
        }

        public void Update()
        {
            if (mActiveTasks.Count == 0 && mTaskQueue.Count == 0)
                // Quick exit.
                return;

            for (int i = mActiveTasks.Count - 1; i >= 0; i--)
            {
                BuildTask task = mActiveTasks[i];

                if (task.Request.IsFinished)
                    mActiveTasks.RemoveAt(i);
            }

            while (mActiveTasks.Count < mMaxActive && mTaskQueue.Count > 0)
            {
                if (mTaskQueue.Peek().Request.IsFinished)
                    mTaskQueue.Dequeue();  // Throw the task away.
                else
                {
                    BuildTask task = mTaskQueue.Dequeue();
                    try
                    {
                        Thread t = new Thread(task.Run);
                        t.Start();
                        mActiveTasks.Add(task);
                    }
                    catch (Exception ex)
                    {
                        task.RequestAbort(
                            "Aborted on exception at task start: " + ex.Message);
                    }
                }
            }
        }
    }
}
