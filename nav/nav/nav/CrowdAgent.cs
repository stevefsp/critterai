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
using org.critterai.nav.rcn;
using org.critterai.interop;

namespace org.critterai.nav
{
    /// <summary>
    /// Represents an agent is being managed by a <see cref="CrowdManager"/>
    /// object.
    /// </summary>
    /// <remarks>
    /// <p>Objects of this type can only be obtained from a 
    /// <see cref="CrowdManager"/> object.</p>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
    /// </remarks>
    /// <seealso cref="CrowdManager"/>
    public sealed class CrowdAgent
    {
        /*
         * Design notes:
         * 
         * The difficulty in the design of the agent is that the
         * structure on the native side contains embedded classes.
         * (Not pointers to instances.)  And interop can't seem to handle
         * that. So I'm forced into a less efficient design.
         * 
         */

        private IntPtr root;
        private CrowdManager mManager;
        internal int managerIndex;

        public CrowdAgentState State 
        { 
            get { return mManager.agentStates[managerIndex].state; } 
        }
        public int NeighborCount 
        { 
            get { return mManager.agentStates[managerIndex].neighborCount; } 
        }
        public float DesiredSpeed 
        { 
            get { return mManager.agentStates[managerIndex].desiredSpeed; } 
        }
        public float[] Position 
        { 
            get { return mManager.agentStates[managerIndex].position; } 
        }
        public float[] DesiredVelocity 
        { 
            get { return mManager.agentStates[managerIndex].desiredVelocity; } 
        }
        public float[] Velocity 
        { 
            get { return mManager.agentStates[managerIndex].velocity; } 
        }

        /// <summary>
        /// Indicates whether the object's resources have been disposed.
        /// </summary>
        public bool IsDisposed { get { return (root == IntPtr.Zero); } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">A pointer to an allocated native 
        /// dtCrowdAgent object.
        /// </param>
        internal CrowdAgent(CrowdManager manager
            , IntPtr agent
            , int mangerIndex)
        {
            mManager = manager;
            root = agent;
            managerIndex = mangerIndex;
        }

        internal void Dispose()
        {
            root = IntPtr.Zero;
            mManager = null;
            managerIndex = -1;
        }

        public CrowdAgentParams GetConfig()
        {
            CrowdAgentParams config = new CrowdAgentParams();
            if (!IsDisposed)
                CrowdAgentEx.GetParams(root, ref config);
            return config;
        }

        /// <summary>
        /// Updates the configuration for an agent.
        /// </summary>
        /// <param name="agentParams">The new agent configuration.</param>
        public void SetConfig(CrowdAgentParams config)
        {
            if (!IsDisposed)
                CrowdManagerEx.UpdateAgentParameters(mManager.root
                    , managerIndex
                    , ref config);
        }

        /// <summary>
        /// Sets the move target for an agent.
        /// </summary>
        /// <remarks>
        /// <p>This method is used when a new target is set.  Use 
        /// <see cref="AdjustMoveTarget"/> when only small adjustments are 
        /// needed. (Such as happens when following a moving target.</p>
        /// </remarks>
        /// <param name="agentIndex">The index of the agent.</param>
        /// <param name="polyRef">The id of the navmesh polygon where the 
        /// position is located.</param>
        /// <param name="position">The target position.</param>
        /// <returns>TRUE if the target was successfully set.</returns>
        public bool RequestMoveTarget(uint polyRef, float[] position)
        {
            if (IsDisposed)
                return false;
            return CrowdManagerEx.RequestMoveTarget(mManager.root
                , managerIndex
                , polyRef
                , position);
        }

        /// <summary>
        /// Adjusts the position of an agent's move target.
        /// </summary>
        /// <remarks><p>This method expects small increments and is suitable
        /// to call every frame.  (Such as is required when the target is
        /// moving.)</p></remarks>
        /// <param name="agentIndex">The index of the agent.</param>
        /// <param name="polyRef">The id of the navmesh polygon where the 
        /// position is located.</param>
        /// <param name="position">The adjusted target position.</param>
        /// <returns>TRUE if the adjustment was successfully applied.</returns>
        public bool AdjustMoveTarget(uint polyRef, float[] position)
        {
            if (IsDisposed)
                return false;
            return CrowdManagerEx.AdjustMoveTarget(mManager.root
                , managerIndex
                , polyRef
                , position);
        }

        public void GetCorridor(PathCorridorData buffer)
        {
            if (!IsDisposed)
                CrowdAgentEx.GetPathCorridor(root, buffer);
        }

        public void GetBoundary(LocalBoundaryData buffer)
        {
            if (!IsDisposed)
                CrowdAgentEx.GetLocalBoundary(root, buffer);
        }

        public int GetNeighbors(CrowdNeighbor[] buffer)
        {
            if (IsDisposed
                || buffer == null
                || buffer.Length < CrowdNeighbor.MaxNeighbors)
            {
                return -1;
            }

            int count = CrowdAgentEx.GetNeighbors(root
                , buffer
                , buffer.Length);

            return count;
        }

        public CrowdAgent GetNeighbor(CrowdNeighbor neighbor)
        {
            if (IsDisposed || neighbor.index >= mManager.MaxAgents)
                return null;
            return mManager.mAgents[neighbor.index];
        }

        public void GetCornerData(CrowdCornerData buffer)
        {
            if (!IsDisposed)
                CrowdAgentEx.GetCorners(root, buffer);
        }
    }
}
