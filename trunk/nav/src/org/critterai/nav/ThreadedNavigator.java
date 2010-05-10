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
package org.critterai.nav;

import org.critterai.nav.MasterNavigator.Navigator;

/**
 * A navigator suitable for running on its own thread.
 * <p>The navigator will only be thread safe if it is constructed in a thread safe
 * manner, such as using 
 * {@link NavUtil#getThreadedNavigator(float[], int[], int, float, float, DistanceHeuristicType, long, int, int, int, long) getThreadedNavigator()}</p>
 * <p>While running on its own thread, the navigator will still 
 * utilize the {@link MasterNavigator#maxProcessingTimeslice} functionality
 * of its {@link MasterNavigator} object.</p>
 */
public final class ThreadedNavigator 
    implements Runnable 
    {

    /**
     * This value is in nanoseconds.
     */
    private final long mFrameLength;
    private boolean mIsDisposed = false;
    private final long mMaintenanceFrequency;
    private long mNextMaintenance;
    
    private final MasterNavigator mRoot;

    /**
     * Constructor
     * @param navigator The navigator to manage.
     * @param frameLength The length of the frame. In milliseconds.
     * The process operation will not be called more frequently than this
     * value.
     * @param maintenanceFrequency The frequency, in milliseconds, that maintenance operations
     * should be performed.  The value should normally be less than 
     * {@link MasterNavigator#maxPathAge}.
     * @throws IllegalArgumentException If the navigator is null.
     */
    ThreadedNavigator(MasterNavigator navigator, int frameLength, long maintenanceFrequency)
        throws IllegalArgumentException
    {
        if (navigator == null || navigator.isDisposed())
            throw new IllegalArgumentException("Null or disposed navigator.");
        mRoot = navigator;
        mMaintenanceFrequency = Math.max(0, maintenanceFrequency);
        mFrameLength = Math.max(0, frameLength)*1000000;  // Convert to nanoseconds.
    }
    
    /**
     * Disposes of the {@link MasterNavigator} and exits the {@link #run()} operation.
     * Disposal will take at least 100ms, possibly longer, since there is a 
     * controlled shutdown process that ensures that all active paths and requests
     * are properly notified.
     */
    public void dispose() { mIsDisposed = true; }
    
    /**
     * If TRUE, the object has been disposed or is in the process of being disposed.
     * @return TRUE if the object has been disposed or is in the process of being disposed.
     * Otherwise FALSE.
     */
    public boolean isDisposed() { return mRoot.isDisposed(); }
    
    /**
     * A {@link Navigator} for use by clients of the {@link MasterNavigator}.
     * @return A {@link Navigator} for use by clients of the {@link MasterNavigator}.
     */
    public Navigator navigator() { return mRoot.navigator(); }
    
    /**
     * {@inheritDoc}
     */
    @Override
    public void run() 
    {
        
        mNextMaintenance = System.currentTimeMillis() + mMaintenanceFrequency;
        boolean doMaintenance;
        long sleepLength = 0;
        while(!mIsDisposed)
        {
            long currTime = System.currentTimeMillis();
            doMaintenance = (mNextMaintenance < currTime ? true : false);
            if (doMaintenance)
                mNextMaintenance = currTime + mMaintenanceFrequency;
            sleepLength = (mFrameLength - mRoot.process(doMaintenance))/1000000;
            if (sleepLength > 0)
            {
                try
                {
                    Thread.sleep(sleepLength);
                }
                catch (InterruptedException e)     
                {
                    mIsDisposed = true;
                }    
            }
        }
        
        mRoot.dispose();
        try 
        { 
            // Wait long enough to safely assume that
            // no new requests have been made of the
            // navigator.
            Thread.sleep(100);
            mRoot.dispose();
        } 
        catch (InterruptedException e) { }
        
    }
    
}
