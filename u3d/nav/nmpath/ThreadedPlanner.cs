/*
 * Copyright (c) 2010 Stephen A. Pratt
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
using System.Threading;
using Planner = org.critterai.nav.nmpath.MasterPlanner.Planner;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// A navigator suitable for running on its own thread.
    /// </summary>
    /// <remarks>The navigator will only be thread safe if it is constructed in a thread safe
    /// manner, such as using the 
    /// <see cref="NavUtil.GetThreadedNavigator">NavUtil.GetThreadedNavigator</see>.
    /// <para>While running on its own thread, the navigator will still 
    /// utilize the <see cref="MasterNavigator.MaxProcessingTimeslice">
    /// MaxProcessingTimeslice</see> functionality
    /// of its <see cref="MasterNavigator">MasterNavigator</see> object.</para>
    /// </remarks>
    public sealed class ThreadedPlanner
    {
        /// <summary>
        /// This value is in ticks. I.e. DateTime.Ticks()
        /// </summary>
        private readonly long mFrameLength;

        private Boolean mIsDisposed = false;

        /// <summary>
        /// This value is in milliseconds.
        /// </summary>
        private readonly long mMaintenanceFrequency;

        /// <summary>
        /// This value is in ticks. I.e. DateTime.Ticks()
        /// </summary>
        private long mNextMaintenance;

        private Boolean mIsRunning = false;

        private readonly MasterPlanner mRoot;

        /// <summary>
        /// TRUE if the object has been disposed or is in the Process of being disposed.
        /// </summary>
        public Boolean IsDisposed { get { return mIsDisposed; } }

        /// <summary>
        /// Indicates whether or not the navigator is running.  E.g. Accepting and
        /// processing requests.
        /// </summary>
        public Boolean IsRunning { get { return mIsRunning; } }

        /// <summary>
        /// A Navigator for use by clients of the <see cref="MasterNavigator">MasterNavigator</see>.
        /// </summary>
        public Planner PathPlanner { get { return mRoot.PathPlanner; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="navigator">The navigator to manage.</param>
        /// <param name="frameLength">The length of the frame. In ticks as defined by DateTime.Ticks().
        /// The Process operation will not be called more frequently than this
        /// value.</param>
        /// <param name="maintenanceFrequency">The frequency that maintenance operations
        /// should be performed.  The value should normally be less than 
        /// {@Link MasterNavigator#mMaxPathAge}.  In milliseconds.</param>
        internal ThreadedPlanner(MasterPlanner navigator, int frameLength, long maintenanceFrequency)
        {
            //if (navigator == null || navigator.IsDisposed)
            //    throw new ArgumentException("Null or disposed navigator.");
            mRoot = navigator;
            mMaintenanceFrequency = Math.Max(0, maintenanceFrequency);
            mFrameLength = Math.Max(0, frameLength);
        }
        
        /// <summary>
        /// Disposes the navigator and and shuts down the thread.
        /// </summary>
        /// <remarks>
        /// Disposal will take at least 100ms, possibly longer, since there is a 
        /// controlled shutdown process that ensures that all active paths and requests
        /// are properly notified.
        /// </remarks>
        public void Dispose() { mIsDisposed = true; }
        
        /// <summary>
        /// Starts the navigator in threaded mode.
        /// <para>Has no effect if the navigator is already running or has been
        /// disposed.  In such cases, will return FALSE.</para>
        /// </summary>
        /// <returns>TRUE if the the navigator has successully transitioned to the
        /// running state.  Otherwise FALSE.</returns>
        public Boolean Start() 
        {
            if (mIsDisposed || mIsRunning)
                return false;
            try
            {
                ThreadStart d = new ThreadStart(this.Run);
                Thread t = new Thread(d);
                t.Start();
                mIsRunning = true;
            }
            finally { }
            return mIsRunning;
        }

        private void Run()
        {
            mNextMaintenance = DateTime.Now.Ticks + mMaintenanceFrequency * 10000;
            Boolean doMaintenance;
            long sleepLength = 0;
            while (!mIsDisposed)
            {
                long currTime = DateTime.Now.Ticks;
                doMaintenance = (mNextMaintenance < currTime ? true : false);
                if (doMaintenance)
                    mNextMaintenance = currTime + mMaintenanceFrequency * 10000;
                sleepLength = (mFrameLength - mRoot.Process(doMaintenance));
                if (sleepLength > 0)
                {
                    try
                    {
                        Thread.Sleep((int)Math.Min(int.MaxValue, sleepLength));
                    }
                    catch (Exception e)
                    {
                        if (e == null) { }
                        mIsDisposed = true;
                    }
                }
            }
            mRoot.Dispose();
            try
            {
                // Wait long enough to safely assume that
                // no new requests have been made of the
                // Nav.
                Thread.Sleep(100);
            }
            finally
            {
                mIsRunning = false;
                mRoot.Dispose();
            }
        }
        
    }
}
