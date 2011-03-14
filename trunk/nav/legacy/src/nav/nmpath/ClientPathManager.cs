using System;
using UnityEngine;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Maintains the target position based on the goal and a navigation 
    /// mesh path.
    /// </summary>
    /// <remarks>
    /// Depends on the following navigation data: 
    ///     position
    ///     goalPosition
    /// Manages the following navigation data: 
    ///     targetPosition
    /// </remarks>
    public sealed class ClientPathManager
        :  BaseNavComponent, INavComponent
    {

        public MasterPlanner.Planner pathPlanner = null;

        private MasterPlanner.Planner mActivePlanner;
        private MasterPath.Path mPath = null;
        private MasterNavRequest<MasterPath.Path>.NavRequest mPathRequest;
        private int mTargetFailures = 0;

        public ClientPathManager(NavigationData navData)
            : base(navData)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Will only return Failed or Active.  
        /// Will fail only if a path cannot be obtained.  Failure of a path
        /// will result in replanning.
        /// </remarks>
        /// <returns></returns>
        public NavigationState Update()
        {
            if (state == NavigationState.Inactive)
            {
                if (!Initialize())
                {
                    state = NavigationState.Failed;
                    return state;
                }
                else
                    state = NavigationState.Active;
            }
            else if (state != NavigationState.Active)
            {
                state = NavigationState.Failed;
                return state;
            }

            if (mActivePlanner.IsDisposed || mActivePlanner != pathPlanner)
            {
                // Need new planner.
                Cleanup();
                if (!Initialize())
                {
                    state = NavigationState.Failed;
                    return state;
                }
            }

            if (mPathRequest != null)
            {
                // A search is in progress.
                if (mPathRequest.IsFinished)
                {
                    // Search completed.
                    if (mPathRequest.State == NavRequestState.Complete)
                    {
                        // Search was a success.
                        mPath = mPathRequest.Data;
                    }
                    else
                    {
                        // Search failed.
                        Cleanup();
                        state = NavigationState.Failed;
                        return state;
                    }
                    mPathRequest = null;
                }
                else
                    // Search not yet complete.
                    return state;
            }

            // Will not get here if there is an active search.

            Vector3 pos = navData.position;
            if (mPath == null
                || mPath.IsDisposed
                || mPath.Goal != navData.goalPosition)
            {
                //if (mPath != null && mPath.IsDisposed)
                //{
                //    mPath = null;
                //}
                //else if (mPath != null && mPath.Goal != navData.goalPosition)
                //{
                //    mPath = null;
                //}
                //else
                //    mPath = null;
                // Need to perform a new search.
                mTargetFailures = 0;
                mPath = null;
                Vector3 goal = navData.goalPosition;
                mPathRequest = mActivePlanner.GetPath(pos.x, pos.y, pos.z
                    , goal.x, goal.y, goal.z);
                if (mPathRequest.IsFinished)
                {
                    // Unexpected immediate failure.
                    Cleanup();
                    state = NavigationState.Failed;
                    return state;
                }
            }
            else
            {
                // Process the current path.
                Vector3 u;
                if (mPath.GetTarget(pos.x, pos.y, pos.z, out u))
                {
                    navData.targetPosition = u;
                    mTargetFailures = 0;
                }
                else
                {
                    // Target failure.  Agent may no longer be 
                    // in valid position.
                    // Design note: Leaving original target in place
                    // while recovery is attempted.
                    mTargetFailures++;
                    if (mTargetFailures > 30)
                    {
                        // Could not recover from target failure.
                        // Force a new search.
                        mTargetFailures = 0;
                        navData.targetPosition = navData.position;
                        mPath = null;
                    }
                }
            }

            return state;
        }

        public void Exit()
        {
            Cleanup();
            state = NavigationState.Inactive;
        }

        private bool Initialize()
        {
            // Initialize does not perform cleanup.
            bool result = true;
            navData.targetPosition = navData.position;
            mActivePlanner = pathPlanner;
            if (mActivePlanner == null || mActivePlanner.IsDisposed)
                // Path planner is not valid.
                result = false;
            return result;
        }

        private void Cleanup()
        {
            // Design note: Don't alter behavior state within this method.
            if (mPathRequest != null && !mPathRequest.IsFinished)
                mActivePlanner.DiscardPathRequest(mPathRequest);
            mActivePlanner = null;
            mPathRequest = null;
            mPath = null;
            mTargetFailures = 0;
            navData.targetPosition = navData.position;
        }
    }
}
