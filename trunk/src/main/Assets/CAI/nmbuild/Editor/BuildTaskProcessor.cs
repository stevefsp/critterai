/*
 * Copyright (c) 2012 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Threading;
using org.critterai.nmgen;
using org.critterai.nav;

namespace org.critterai.nmbuild
{
    // The thread startup is delayed until the first request.
    public sealed class BuildTaskProcessor
    {
        /*
         * Design notes:
         * 
         * Everyting is locked on the task queue.
         * 
         * The processors are not created or their threads started until the first task is queued.
         * So the processor array will contain nulls until then.
         * This is important to the design in Unity.  Want to keep the threads in
         * the editor to a minimum until they are needed.
         */

        public const int HighPriority = 3000;
        public const int MediumPriority = 2000;
        public const int LowPriority = 1000;

        private static int mStandardSleep = 10;
        private static int mIdleSleep = 100;

        public static int IdleSleep
        {
            get { return mIdleSleep; }
            set { mIdleSleep = Math.Max(1, value); }
        }

        public static int StandardSleep
        {
            get { return mStandardSleep; }
            set { mStandardSleep = Math.Max(1, value);}
        }

        private class Processor
        {
            private readonly Object mSemaphore = new Object();
            private IBuildTask mTask;
            private bool mAbort = false;

            public void Abort()
            {
                mAbort = true;
            }

            public void RunTask(IBuildTask task)
            {
                lock (mSemaphore) { mTask = task; }
            }

            public void Run()
            {
                while (!mAbort)
                {
                    IBuildTask task = null;

                    lock (mSemaphore)
                    {
                        if (mTask != null && mTask.TaskState == BuildTaskState.Inactive)
                        {
                            task = mTask;
                            mTask = null;
                        }
                    }

                    if (task == null)
                        Thread.Sleep(100);
                    else
                    {
                        // Have to re-check.  State may have changed.
                        if (task.TaskState == BuildTaskState.Inactive)
                        {
                            /*
                             * Design note: 
                             * 
                             * This is a trade-off to deal with poorly written tasks. Technically, 
                             * the task is responsible for catching and reporting its own 
                             * exceptions.  But if it doesn't it can bring down the processor 
                             * thread.  
                             * 
                             * This method forces the task to abort so the main manager can detect 
                             * and discard it.  But if the task is so poorly written that the 
                             * abort doesn't work, or throws another exception, then the processor 
                             * will become a zombie, unresponsive to new requests from the manager.
                             */
                            try 
                            { 
                                task.Run(); 
                            } 
                            catch (Exception ex)
                            {
                                task.Abort("Exception detected by processor: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private const string AbortMessage = "Task processor shutdown.";

        private readonly List<IBuildTask> mTaskQueue = new List<IBuildTask>();

        // This sort order is correct.  The queue list is emptied from the end.
        private readonly PriorityComparer<IBuildTask> mComparer = new PriorityComparer<IBuildTask>(true);

        private readonly Processor[] mProcessors;
        private readonly IBuildTask[] mActiveTasks;

        private int mTaskCount;
        private bool mIsRunning;
        private bool mAbort;

        public int TaskCount { get { return mTaskCount; } }
        public bool IsRunning { get { return mIsRunning; } }
        public int MaxConcurrent { get { return mProcessors.Length; } }

        public BuildTaskProcessor(int maxConcurrent)
        {
            maxConcurrent = Math.Max(1, maxConcurrent);
            mProcessors = new Processor[maxConcurrent];
            mActiveTasks = new IBuildTask[maxConcurrent];
        }

        public void Abort()
        {
            lock (mTaskQueue)
            {
                if (mAbort)
                    return;

                mAbort = true;

                mTaskQueue.Clear();

                if (!mIsRunning)
                    mTaskCount = 0;
            }
        }

        public bool QueueTask(IBuildTask task)
        {
            if (task == null || task.TaskState != BuildTaskState.Inactive || !task.IsThreadSafe)
                return false;

            lock (mTaskQueue)
            {
                if (mAbort)
                    return false;

                if (mProcessors[0] == null)
                {
                    for (int i = 0; i < mProcessors.Length; i++)
                    {
                        mProcessors[i] = new Processor();
                        Thread t = new Thread(new ThreadStart(mProcessors[i].Run));
                        t.Start();
                    }
                }

                mTaskQueue.Add(task);

                mTaskCount = mTaskQueue.Count;
                foreach (IBuildTask item in mActiveTasks)
                {
                    if (item != null)
                        mTaskCount++;
                }

                return true;
            }
        }

        public void Run()
        {
            lock (mTaskQueue)
            {
                if (mAbort || mIsRunning)
                    return;

                mIsRunning = true;
            }

            while (!mAbort)
            {
                int activeCount = 0;

                lock (mTaskQueue)
                {
                    mTaskQueue.Sort(mComparer);

                    for (int i = 0; i < mActiveTasks.Length; i++)
                    {
                        IBuildTask task = mActiveTasks[i];

                        if (task == null || task.IsFinished)
                        {
                            // Task complete.  Clear it out.
                            mActiveTasks[i] = null;
                            task = null;
                        }
                        else
                        {
                            // Task still being processed.
                            activeCount++;
                            continue;
                        }

                        // Need to assign a new task to this slot.
                        while (task == null && mTaskQueue.Count > 0)
                        {
                            task = mTaskQueue[mTaskQueue.Count - 1];
                            mTaskQueue.RemoveAt(mTaskQueue.Count - 1);

                            if (task.TaskState != BuildTaskState.Inactive)
                                task = null;
                        }

                        if (task != null)
                        {
                            // Found a task to run.
                            activeCount++;
                            mActiveTasks[i] = task;
                            mProcessors[i].RunTask(task);
                        }
                    }

                    mTaskCount = mTaskQueue.Count + activeCount;
                }

                // Pause longer if there are no active tasks.
                if (activeCount > 0)
                    Thread.Sleep(10);
                else
                    Thread.Sleep(100);
            }

            mTaskCount = 0;

            foreach (Processor processor in mProcessors)
            {
                if (processor == null)
                    // The processors were never started.
                    break;

                processor.Abort();
            }

            mIsRunning = false;
        }
    }
}
