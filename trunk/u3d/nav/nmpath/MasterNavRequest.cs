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

namespace org.critterai.nav.nmpath
{

    /// <summary>
    /// Represents a navigation request being processed by a navigator.
    /// <para>Instances of this class are not thread-safe but can be used by multiple threads if only
    /// a single thread has a reference to the instance and other threads are provided an
    /// instance of <see cref="NavRequest">NavRequest</see> via a call to 
    /// <see cref="Request">Request</see></para>
    /// </summary>
    /// <typeparam name="T">The type of the result data for the request.</typeparam>
    public sealed class MasterNavRequest<T> 
    {

        /*
         * Design notes:  
         * 
         * Everything is locked on the root object.
         */
        
        /// <summary>
        /// Provides access to the state and data related to a navigation request.
        /// </summary>
        /// <remarks>
        /// <para>When polling to see if a Request is complete, check <see cref="IsFinished">IsFinished</see>
        /// rather than <see cref="State">State</see>.  Using State incurs synchronization costs.</para>
        /// <para>Instances of this class are thread-safe only if its associated 
        /// MasterRequest.
        /// object is used in a thread-safe manner.</para>
        /// </remarks>
        public sealed class NavRequest
        {

            private readonly MasterNavRequest<T> mRoot;

            internal NavRequest(MasterNavRequest<T> root) 
            {
                mRoot = root;
            }

            /// <summary>
            /// The data associated with the request.
            /// This data is only guaranteed to be available (non-null)
            /// if the state of the object is <see cref="NavRequestState.Complete">Complete</see>.
            /// </summary>
            public T Data 
            {
                get
                {
                    lock (mRoot)
                    {
                        return mRoot.mData;
                    }
                }
            }
            
            /// <summary>
            /// Indicates whether or not the request is finished being processed.
            /// The request is considered finished if its State is either
            /// <see cref="NavRequestState.Complete">Complete</see> or 
            /// <see cref="NavRequestState.Failed">Failed</see>.
            /// </summary>
            public Boolean IsFinished { get { return mRoot.mIsComplete; } }
            
            /// <summary>
            /// The current state of the object.
            /// </summary>
            /// <remarks>
            /// When polling to see if a Request is complete, use <see cref="IsFinished">IsFinished</see>.
            /// Using this operation incurs synchronization costs.
            /// </remarks>
            public NavRequestState State
            {
                get
                {
                    lock (mRoot)
                    {
                        return mRoot.mState;
                    }
                }
            }

        }

        private T mData;

        private NavRequestState mState = NavRequestState.Processing;

        private readonly NavRequest mNavRequest;
        
        private Boolean mIsComplete = false;
        
        /// <summary>
        /// Default Constructor.  The state defaults to 
        /// <see cref="NavRequestState.Processing">Processing</see>
        /// </summary>
        public MasterNavRequest() 
        {
            mNavRequest = new NavRequest(this);
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">The initial State of the object.</param>
        public MasterNavRequest(NavRequestState state)
        {
            mNavRequest = new NavRequest(this);
            mState = state;
            if (mState != NavRequestState.Processing)
                mIsComplete = true;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">The initial state of the object.</param>
        /// <param name="data">The instance's data package.</param>
        public MasterNavRequest(NavRequestState state, T data)
        {
            mNavRequest = new NavRequest(this);
            mState = state;
            mData = data;
            mIsComplete = (mState != NavRequestState.Processing);
        }

        /// <summary>
        /// The data associated with the request.
        /// This data is only guaranteed to be available (non-null)
        /// if the state of the object is <see cref="NavRequestState.Complete">Complete</see>.
        /// </summary>
        public T Data { get { return mData; } }
        
        /// <summary>
        /// Returns a shared request object suitable for use by clients.
        /// </summary>
        public NavRequest Request { get { return mNavRequest; } }

        /// <summary>
        /// The State of the request.
        /// </summary>
        /// <value>The state of the request.</value>
        public NavRequestState State
        {
            get { return mState; }
            set
            {
                lock (this)
                {
                    mState = value;
                    mIsComplete = (mState != NavRequestState.Processing);
                }
            }
        }

        /// <summary>
        /// Sets the state and data of the request.
        /// </summary>
        /// <param name="state">The state of the request.</param>
        /// <param name="data">The request's data.</param>
        public void Set(NavRequestState state, T data)
        {
            lock (this) 
            {
                mData = data;
                mState = state;    
            }
            mIsComplete = (mState != NavRequestState.Processing);
        }
        
        /// <summary>
        /// Sets the request's data.
        /// </summary>
        /// <param name="data">The request's data.</param>
        public void SetData(T data) 
        { 
            lock (this) 
            {
                mData = data; 
            }
        }
        
    }

}
