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
using org.critterai.nmgen;
using org.critterai.nav;
using System.Collections.Generic;

namespace org.critterai.nmbuild
{
    public abstract class BuildTask<T>
        : IBuildTask
    {
        // Design note: Everything is locked on the messages list.

        private T mData;

        private BuildTaskState mState;
        private bool mIsFinished = false;
        private readonly int mPriority;

        private readonly List<string> mMessages = new List<string>();

        public int Priority { get { return mPriority; } }

        public BuildTask(int priority)
        {
            mPriority = priority;
        }

        public abstract bool IsThreadSafe { get; }

        protected abstract bool LocalRun();

        protected abstract bool GetResult(out T result);
        protected virtual void FinalizeTask() { }

        protected void AddMessage(string messsage)
        {
            lock (mMessages)
                mMessages.Add(messsage);
        }
        protected void AddMessages(string[] messages)
        {
            lock (mMessages)
                mMessages.AddRange(messages);
        }

        public BuildTaskState TaskState {  get {  lock (mMessages) return mState; } }
        public bool IsFinished { get { return mIsFinished; } }
        public string[] Messages { get {  lock (mMessages) return mMessages.ToArray(); } }
        public T Data { get { return mData; } }

        public void Run()
        {
            lock (mMessages)
            {
                if (mState != BuildTaskState.Inactive)
                    return;

                mState = BuildTaskState.InProgress;
            }

            try
            {
                while (LocalRun())
                {
                    lock (mMessages)
                    {
                        if (mState == BuildTaskState.Aborting)
                            break;
                    }
                }
                FinalizeRequest();
            }
            catch (System.Exception ex)
            {
                FinalizeRequest(ex);
            }
            finally
            {
                FinalizeTask();
            }
        }

        public void Abort(string reason)
        {
            lock (mMessages)
            {
                if (mIsFinished)
                    return;

                AddMessage(reason);

                if (mState == BuildTaskState.Inactive)
                {
                    mState = BuildTaskState.Aborting;
                    FinalizeRequest();
                }
                else
                    mState = BuildTaskState.Aborting;
            }
        }

        private void FinalizeRequest()
        {
            lock (mMessages)
            {
                if (mState == BuildTaskState.Aborting)
                {
                    // Don't care if the task actually finished.
                    // An abort takes presidence.
                    mState = BuildTaskState.Aborted;
                    mData = default(T);
                }
                else
                {
                    try
                    {
                        mState = GetResult(out mData) ? BuildTaskState.Complete : BuildTaskState.Aborted;
                    }
                    catch (System.Exception ex)
                    {
                        FinalizeRequest(ex);
                    }
                }

                mIsFinished = true;  // Always last.
            }
        }

        private void FinalizeRequest(System.Exception ex)
        {
            lock (mMessages)
            {
                mMessages.Add("Aborted on exception: " + ex.Message);
                mData = default(T);
                mState = BuildTaskState.Aborted;
                mIsFinished = true;
            }
        }
    }
}
