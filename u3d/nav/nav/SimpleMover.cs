using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace org.critterai.nav
{
    /// <summary>
    /// Updates the navigation position based on the target velocity.
    /// </summary>
    /// <remarks>
    /// Depends on the following navigation data: 
    ///     targetVelocity
    ///     goalPosition
    ///     goalRotation
    ///     forceMovement
    /// Manages the following navigation data: 
    ///     position
    ///     rotation (Only set when the goal is reached.)
    ///     forceMovement (Sets to false after update.)
    ///     velocity
    ///     navState
    /// </remarks>
    public abstract class SimpleMover
        : BaseNavComponent, INavComponent
    {
        protected float deltaTime = 0;

        public SimpleMover(NavigationData navData)
            : base(navData)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Will stay active until there is a failure or the goal has
        /// been reached.
        /// Failure will occur on instructions from the concrete implementation
        /// or if the Update is called after completion/failure without first
        /// calling the exit operation.
        /// </remarks>
        /// <returns></returns>
        public NavigationState Update()
        {
            if (state == NavigationState.Inactive)
            {
                if (Initialize())
                    state = NavigationState.Active;
                else
                {
                    state = NavigationState.Failed;
                    return state;
                }
            }
            else if (state != NavigationState.Active)
            {
                state = NavigationState.Failed;
                return state;
            }

            if (!PreUpdate())
            {
                state = NavigationState.Failed;
                return state;
            }

            if (navData.forceMovement)
            {
                navData.forceMovement = false;
                navData.position = navData.goalPosition;
                navData.rotation = navData.goalRotation;
                navData.velocity = Vector3.zero;
            }
            else
            {
                Vector3 scaledVelocity = navData.targetVelocity * deltaTime;
                Vector3 goalVector = (navData.goalPosition - navData.position);
                if (scaledVelocity.sqrMagnitude >= goalVector.sqrMagnitude)
                {
                    // Can reach goal this time step.
                    navData.velocity = goalVector / deltaTime;
                    navData.position = navData.goalPosition;
                    navData.rotation = navData.goalRotation;
                }
                else
                {
                    navData.position += scaledVelocity;
                    navData.velocity = navData.targetVelocity;
                }
            }

            if (!ApplyMovement())
                state = NavigationState.Failed;
            else if (navData.IsAtGoal)
            {
                navData.rotation = navData.goalRotation;
                state = NavigationState.Complete;
            }

            return state;
        }

        /// <summary>
        /// Called during the Update operation when the component transitions
        /// from inactive to active.
        /// </summary>
        /// <remarks>
        /// The navigation state of the mover should not be changed from within 
        /// this operation.
        /// </remarks>
        /// <returns>TRUE if the state can transition to Active.  FALSE if
        /// initialization failed and the state should transition to Failed.
        /// </returns>
        protected abstract bool Initialize();

        /// <summary>
        /// Perform exit operations.  Called at the beginning of the Exit
        /// operation.
        /// </summary>
        protected abstract void LocalExit();

        /// <summary>
        /// Called at the beginning of every update during which the state
        /// is active.
        /// It is expected that the deltaTime will be supplied with the
        /// correct value during this operation.
        /// </summary>
        /// <remarks>
        /// The navigation state of the mover should not be changed from within 
        /// this operation.
        /// </remarks>
        /// <returns>TRUE if the update can continue.  FALSE if the update
        /// should halt on failure.</returns>
        protected abstract bool PreUpdate();

        /// <summary>
        /// Transfer the position data from the navigation data to whatever
        /// is using the data.  Called after the new position has been
        /// calculated.
        /// </summary>
        /// <remarks>
        /// The navigation state of the mover should not be changed from within 
        /// this operation.
        /// WARNING: A failure of this operation does not automatically result 
        /// in the rollback of navidation data changes that occurred earlier 
        /// in the update process. E.g. Any changes made to the navigation 
        /// data's position field will remain in place.
        /// </remarks>
        /// <returns>TRUE if the movement was successully applied.  FALSE if
        /// the update should halt on a failure.</returns>
        protected abstract bool ApplyMovement();

        public void Exit()
        {
            LocalExit();
            navData.velocity = Vector3.zero;
            state = NavigationState.Inactive;
        }


    }
}
