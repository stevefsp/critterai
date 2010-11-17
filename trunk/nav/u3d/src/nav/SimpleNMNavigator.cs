using System;
using org.critterai.nav.nmpath;

namespace org.critterai.nav
{
    /// <summary>
    /// Provides simple navigation control, including:
    /// Target position management by the ClientPathManager.
    /// Target velocity management by the SimpleVelocityManager.
    /// Movement control by a component provided at construction.
    /// </summary>
    /// <remarks>
    /// Directly depends on the following navigation data: 
    ///     movementEnabled
    /// Directly manages the following navigation data:
    ///     navState
    ///     hasBeenProcessed
    /// </remarks>
    public sealed class SimpleNMNavigator
    {
        private readonly ClientPathManager mPathManager;
        private readonly SimpleVelocityManager mVelocityManager;
        private readonly INavComponent mMovementController;
        private readonly NavigationData mNavData;

        public SimpleNMNavigator(NavigationData navData
            , INavComponent movementController)
        {
            //if (navData == null
            //    || movementController == null)
            //    throw new ArgumentNullException();

            mNavData = navData;
            mPathManager = new ClientPathManager(navData);
            mVelocityManager = new SimpleVelocityManager(navData);
            mMovementController = movementController;
        }

        public void SetPathPlanner(MasterPlanner.Planner pathPlanner)
        {
            mPathManager.pathPlanner = pathPlanner;
        }

        public NavigationState Update()
        {
            mNavData.hasBeenProcessed = true;
            if (!mNavData.movementEnabled)
            {
                // Special case.  Movement is not allowed.  Go inactive.
                Reset();
            }
            else if (mNavData.forceMovement)
            {
                // Special case: Skip planning.  Expect movement controller
                // to handle everything.
                // Dev Note: Must process movement before calling the exit
                // methods since the exit process may need the new position.
                ProcessMovement();
                mVelocityManager.Exit();
                mPathManager.Exit();
            }
            else if (mNavData.IsAtGoal)
            {
                // Special case: Already at goal.  Nothing to do.
                // Design note: Force movement handling (above) takes priority
                // over being at the goal.
                Reset();
                mNavData.navState = NavigationState.Complete;
            }
            else
            {
                // Perform normal navigation.
                if (NavUtil.IsComplete(mNavData.navState))
                    // Get ready to use again.
                    Reset();
                NavigationState childState = mPathManager.Update();
                if (childState == NavigationState.Failed)
                {
                    mNavData.navState = NavigationState.Failed;
                    mPathManager.Exit();
                }
                else
                {
                    childState = mVelocityManager.Update();
                    if (childState == NavigationState.Failed)
                    {
                        mNavData.navState = NavigationState.Failed;
                        mVelocityManager.Exit();
                        mPathManager.Exit();
                    }
                    else
                        ProcessMovement();
                }
            }
            return mNavData.navState;
        }

        private void ProcessMovement()
        {
            NavigationState childState = mMovementController.Update();
            if (childState == NavigationState.Failed)
                Reset();
            mNavData.navState = childState;
        }

        public void Reset()
        {
            mMovementController.Exit();
            mVelocityManager.Exit();
            mPathManager.Exit();
            mNavData.navState = NavigationState.Inactive;
        }

    }
}
