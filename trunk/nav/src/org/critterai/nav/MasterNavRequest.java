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

import static org.critterai.nav.NavRequestState.*;

/**
 * 
 * <p>Instances of this class are not thread-safe but can be used by multiple threads if only
 * a single thread has a reference to the instance and other threads are provided an
 * instance of {@link NavRequest} via a call to {@link #request()}</p>
 * @param <T> The result data of the request.
 */
public final class MasterNavRequest<T> 
{

	/*
	 * Design notes:  
	 * 
	 * Everything is synchronized on the state object.
	 */
	
	/**
	 * Provides access to navidation request state and data.
	 * <p>When polling to see if a request is complete, check {@link #isFinished()} rather than
	 * {@link #state()}.  Using {@link #state()} incurs synchronization costs.</p>
	 * <p>Instances of this class are thread-safe only if its associated {@link MasterNavRequest}
	 * object is used in a thread-safe manner.</p>
	 */
	public final class NavRequest
	{
		
		private NavRequest() { }

		/**
		 * The data associated with the request.
		 * This data is only guaranteed to be available (non-null)
		 * if the state of the object is {@link NavRequestState#COMPLETE}.
		 * @return The data associated with the request.
		 */
		public T data() 
		{
			synchronized (mState) 
			{
				return mData;
			}
		}
		
		/**
		 * Indicates whether or not the request is finished being processed.
		 * The request is considered finished if its state is either
		 * {@link NavRequestState#COMPLETE} or {@link NavRequestState#FAILED}.
		 * @return TRUE if the request is finished.  Otherwise FALSE.
		 */
		public boolean isFinished() { return mIsComplete; }
		
		/**
		 * The state of the object.
		 * <p>When polling to see if a request is complete, use {@link #isFinished()}.
	     * Using this operation incurs synchronization costs.</p>
		 * @return The state of the object.
		 */
		public NavRequestState state() 
		{ 
			synchronized (mState) 
			{
				return mState;
			}
		}

	}
	private T mData = null;
	
	private NavRequestState mState = PROCESSING;
	
	private final NavRequest mNavRequest = this.new NavRequest();
	
	private boolean mIsComplete = false;
	
	/**
	 * Default Constructor
	 */
	public MasterNavRequest() { }
	
	/**
	 * Constructor
	 * @param state The initial state of the object.
	 * @throws IllegalArgumentException If the state is null.
	 */
	public MasterNavRequest(NavRequestState state)
		throws IllegalArgumentException
	{
		if (mState == null)
			throw new IllegalArgumentException("State is null.");
		mState = state;
		if (mState != NavRequestState.PROCESSING)
			mIsComplete = true;
	}
	
	/**
	 * Constructor
	 * @param state The initial state of the object.
	 * @param data The objects data package.
	 * @throws IllegalArgumentException If the state is null.
	 */
	public MasterNavRequest(NavRequestState state, T data)
		throws IllegalArgumentException
	{
		if (mState == null)
			throw new IllegalArgumentException("State is null.");
		mState = state;
		mData = data;
		mIsComplete = (mState != NavRequestState.PROCESSING);
	}

	/**
	 * The data associated with the request.
	 * This data is only guaranteed to be available (non-null)
	 * if the state of the object is {@link NavRequestState#COMPLETE}.
	 * @return The data associated with the request.
	 */
	public T data() { return mData; }
	
	/**
	 * Returns a shared request object suitable for use by clients.
	 * (No new object creation.)
	 * @return A request object suitable for use by clients.
	 */
	public NavRequest request() { return mNavRequest; }

	/**
	 * Sets the state and data of the object.
	 * @param state The state of the object.
	 * @param data The object's data.
	 */
	public void set(NavRequestState state, T data)
	{
		synchronized (mState) 
		{
			mData = data;
			mState = state;	
		}
		mIsComplete = (mState != NavRequestState.PROCESSING);
	}
	
	/**
	 * Sets the object's data.
	 * @param data The objects's data.
	 */
	public void setData(T data) 
	{ 
		synchronized (mState) 
		{
			mData = data; 
		}
	}

	/**
	 * Sets the state of the object.
	 * @param state Sets the state of the object.
	 */
	public void setState(NavRequestState state) 
	{ 
		synchronized (mState) 
		{
			mState = state; 
		}
		mIsComplete = (mState != NavRequestState.PROCESSING);
	}
	
	/**
	 * The state of the object.
	 * @return The state of the object.
	 */
	public NavRequestState state()  { return mState; }
	
}
