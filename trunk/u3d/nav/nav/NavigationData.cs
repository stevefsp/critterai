using UnityEngine;
using org.critterai.math;

namespace org.critterai.nav
{
    /// <summary>
    /// A blackboard for client navigation data.
    /// </summary>
    public sealed class NavigationData
        : DynamicObstacleData
    {

        public const float DefaultPositionTolerance = 0.005f;

        public float maximumSpeed = 0;

        /// <summary>
        /// The xz-plane tolerance the position must be within for the
        /// object to be considered at a location.  (Either target or goal.)
        /// </summary>
        public float xzTolerance = DefaultPositionTolerance;

        /// <summary>
        /// The y-plane tolerance the position must be within for the
        /// object to be considered at a location.  (Either target or goal.)
        /// </summary>
        public float yTolerance = DefaultPositionTolerance;

        /// <summary>
        /// Indicates whether or not assigned navigation manager should
        /// process the this client for movement.  I.e. The manager should
        /// update the target velocity.
        /// </summary>
        public bool movementEnabled = true;

        /// <summary>
        /// The overall state of navigation.
        /// </summary>
        public NavigationState navState = NavigationState.Inactive;

        /// <summary>
        /// Indicates whether the navigation data has been processed by
        /// the navigation loop.
        /// </summary>
        /// <remarks>
        /// This value is set to TRUE at the end of every iteration of the
        /// navigation loop.  It can be used by clients to detect when
        /// the state of the data is valid for a new goal.
        /// Example:
        /// 1.  Client sets a new goal.
        /// 2.  Client sets the value to FALSE.
        /// 3.  Navigation loop runs and sets the value to TRUE.
        /// 4.  Client checks the value.  If it is TRUe, it knows the
        ///     navigation state is valid for the new goal.  Otherwise
        ///     it knows the navigation state is still related to the old
        ///     goal.
        /// </remarks>
        public bool hasBeenProcessed = false;

        /// <summary>
        /// Indicates that the navigation control should ignore normal 
        /// restrictions and force movement to the current target position.
        /// (Usually used to fix navigation problems such as the agent moving
        /// off the navigation mesh or falling through geometry.)
        /// </summary>
        public bool forceMovement = false;

        /// <summary>
        /// The current target to move to.  (Expected to be in LOS. May be
        /// obstructed by dynamic obstacles.)
        /// </summary>
        public Vector3 targetPosition;

        /// <summary>
        /// The ultimate goal to move to.
        /// (May not have line of sight.)
        /// </summary>
        public Vector3 goalPosition;

        /// <summary>
        /// The desired rotation when the goal position is reached.
        /// </summary>
        public Quaternion goalRotation;

        /// <summary>
        /// The velocity the client needs to be set to in order to reach the goal.
        /// Managed by navigation system.
        /// </summary>
        public Vector3 targetVelocity = Vector3.zero;

        public NavigationData(Vector3 position, Quaternion rotation, float radius)
            : base(position, rotation, radius)
        {
            // Initialize target and goal to the current position. 
            goalPosition = position;
            targetPosition = position;
            goalRotation = rotation;
        }

        /// <summary>
        /// Indicates whether or not the position is within the specified
        /// tolerances of the target position.
        /// </summary>
        public bool IsAtTarget
        {
            get
            {
                return (Vector2Util.SloppyEquals(position.x, position.z
                            , targetPosition.x, targetPosition.z
                            , xzTolerance)
                        && MathUtil.SloppyEquals(position.y
                            , targetPosition.y
                            , yTolerance));
            }
        }

        /// <summary>
        /// Indicates whether or not the position is within the specified
        /// tolerances of the goal position.
        /// </summary>
        public bool IsAtGoal
        {
            get
            {
                return (Vector2Util.SloppyEquals(position.x, position.z
                            , goalPosition.x, goalPosition.z
                            , xzTolerance)
                        && MathUtil.SloppyEquals(position.y
                            , goalPosition.y
                            , yTolerance));
            }
        }

    }
}
