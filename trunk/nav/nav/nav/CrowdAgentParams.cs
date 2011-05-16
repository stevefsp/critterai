/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    /// <summary>
    /// Crowd agent configuration parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CrowdAgentParams
    {
        /*
         * Source: DetourCrowd.h dtCrowdAgentParams (struct)
         * 
         * Design notes:
         * 
         * Structure vs Class
         * 
         * I'm still uncertain whether this should be implemented as
         * a class or a structure.  But I've chosen structure for now.
         * 
         * I will need a structure in order to support passing 
         * the data as an interop array. An array will be required if I want
         * to support more efficent mass updates.  (Add/update multiple agents
         * at once.)
         * 
         * The significant downside is that a structure doesn't support 
         * Unity serialization.
         * 
         */

        /// <summary>
        /// Agent radius. (>= 0)
        /// </summary>
	    public float radius;

        /// <summary>
        /// Agent height. (> 0)
        /// </summary>
        public float height;

        /// <summary>
        /// Maximum allowed acceleration. (>= 0)
        /// </summary>
        public float maxAcceleration;

        /// <summary>
        /// Maximum allowed speed. (>= 0)
        /// </summary>
        public float maxSpeed;

        /// <summary>
        /// Defines how close a neighbor must be before it is considered
        /// in steering behaviors. (> 0)
        /// </summary>
        /// <remarks>
        /// The value is often based on the agent radius and/or
        /// and maximum speed.  E.g. radius * 8</remarks>
        public float collisionQueryRange;

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <remarks>
        /// This value is often based on the agent radius. E.g. radius * 30
        /// </remarks>
        public float pathOptimizationRange;

        /// <summary>
        /// How aggresive the agent manager should be at avoiding
        /// collisions with this agent.
        /// </summary>
        /// <remarks>
        /// A higher value will result in agents trying to stay farther away 
        /// from eachother, at the cost of more difficult steering in tight
        /// spaces.</remarks>
        public float separationWeight;

        /// <summary>
        /// Flags that impact steering behavior.
        /// </summary>
        public CrowdUpdateFlags updateFlags;

        /// <summary>
        /// The index of the avoidance parameters to use for the agent.
        /// </summary>
        /// <remarks>
        /// <p>The <see cref="CrowdManager"/> permits agents to use different
        /// avoidance configurations.  (See 
        /// <see cref="CrowManager.SetObstacleAvoidanceParams"/>.)  This value
        /// is the index of the configuration to use.</p>
        /// </remarks>
        /// <seealso cref="CrowdAvoidanceParams"/>
        public byte avoidanceType;

        // Must exist for marshalling.  Not used on managed side of boundary.
        // On the native side this is a void pointer for custom user data.
	    private IntPtr userData;

        ///// <summary>
        ///// Constructor
        ///// </summary>
        ///// <param name="radius">Agent radius (>=0)</param>
        ///// <param name="height">Agent height (>0)</param>
        ///// <param name="maxAcceleration">Maximum allowed acceleration. (>=0)
        ///// </param>
        ///// <param name="maxSpeed">Maximum allowed speed (>=0)</param>
        ///// <param name="collisionQueryRange">Defines how close a neighbor must 
        ///// be before it is considered in steering behaviors. (>0)</param>
        ///// <param name="pathOptimizationRange">TODO: Need documentation</param>
        ///// <param name="separationWeight">How aggresive the agent manager 
        ///// should be at avoiding collisions with this agent.</param>
        ///// <param name="updateFlags">Flags that impact steering behavior.
        ///// </param>
        ///// <param name="obstacleAvoidanceType">The index of the avoidance 
        ///// parameters to use for the agent.</param>
        //public CrowdAgentParams(float radius
        //    , float height
        //    , float maxAcceleration
        //    , float maxSpeed
        //    , float collisionQueryRange
        //    , float pathOptimizationRange
        //    , float separationWeight
        //    , CrowdUpdateFlags updateFlags
        //    , byte avoidanceType)
        //{
        //    this.radius = radius;
        //    this.height = height;
        //    this.maxAcceleration = maxAcceleration;
        //    this.maxSpeed = maxSpeed;
        //    this.collisionQueryRange = collisionQueryRange;
        //    this.pathOptimizationRange = pathOptimizationRange;
        //    this.separationWeight = separationWeight;
        //    this.updateFlags = updateFlags;
        //    this.avoidanceType = avoidanceType;
        //    this.userData = IntPtr.Zero;
        //}

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="config">The configuratioin to copy.</param>
        public CrowdAgentParams(CrowdAgentParams config)
        {
            this.radius = config.radius;
            this.height = config.height;
            this.maxAcceleration = config.maxAcceleration;
            this.maxSpeed = config.maxSpeed;
            this.collisionQueryRange = config.collisionQueryRange;
            this.pathOptimizationRange = config.pathOptimizationRange;
            this.separationWeight = config.separationWeight;
            this.updateFlags = config.updateFlags;
            this.avoidanceType = config.avoidanceType;
            this.userData = config.userData;
        }
    }
}
