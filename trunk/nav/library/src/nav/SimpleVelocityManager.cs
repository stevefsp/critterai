using System;
using UnityEngine;
using org.critterai.math;

namespace org.critterai.nav
{
    /// <summary>
    /// Maintains the target velocity based on the current position, target
    /// position, and maximum speed.
    /// </summary>
    /// <remarks>
    /// Depends on the following navigation data: 
    ///     position
    ///     targetPosition
    ///     maximumSpeed
    /// Manages the following navigation data: 
    ///     targetVelocity
    /// </remarks>
    public sealed class SimpleVelocityManager
        :  BaseNavComponent, INavComponent
    {
        public SimpleVelocityManager(NavigationData navData)
            : base(navData)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Will always return active.  There are no failure or complete
        /// states.
        /// Will set the target velocity to zero if position is considered
        /// at the target position.
        /// </remarks>
        /// <returns></returns>
        public NavigationState Update()
        {
            state = NavigationState.Active;

            Vector3 target = navData.targetPosition;
            Vector3 pos = navData.position;

            // Design note: y-axis needs more slop since geometry can force
            // a significant difference on that axis.
            if (navData.IsAtTarget)
            {
                navData.targetVelocity = Vector3.zero;
            }
            else
            {
                navData.targetVelocity = (target - pos).normalized
                   * navData.maximumSpeed;
            }

            return state;
        }

        public void Exit()
        {
            navData.targetVelocity = Vector3.zero;
            state = NavigationState.Inactive;
        }
    }
}
