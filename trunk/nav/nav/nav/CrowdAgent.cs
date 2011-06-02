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
    /// Represents an agent managed by a <see cref="CrowdManager"/> object.
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
         * (Rather than pointers to instances.)  And interop can't seem to 
         * handle that. So I'm forced into a less efficient design.
         * 
         */

        private IntPtr root;  // dtCrowdAgent
        private CrowdManager mManager;
        internal int managerIndex;

        /// <summary>
        /// The <see cref="CrowdManager"/> the agent belongs to.
        /// </summary>
        public CrowdManager Manager
        {
            get { return mManager; }
        }

        /// <summary>
        /// The type of mesh polygon the agent is traversing.
        /// </summary>
        public CrowdAgentState State 
        { 
            get { return mManager.agentStates[managerIndex].state; } 
        }

        /// <summary>
        /// The number of neighbor agents. (In vicinity of agent.)
        /// </summary>
        public int NeighborCount 
        { 
            get { return mManager.agentStates[managerIndex].neighborCount; } 
        }

        /// <summary>
        /// The desired agent speed. (Derived) [Form: (x, y, z)]
        /// </summary>
        public float DesiredSpeed 
        { 
            get { return mManager.agentStates[managerIndex].desiredSpeed; } 
        }

        /// <summary>
        /// The current position of the agent. [Form: (x, y, z)]
        /// </summary>
        public float[] Position 
        { 
            get { return mManager.agentStates[managerIndex].position; } 
        }

        /// <summary>
        /// The reference id of the polygon where the position resides.
        /// </summary>
        public uint PositionPoly
        {
            get { return mManager.agentStates[managerIndex].positionPoly; }
        }

        /// <summary>
        /// The desired agent velocity. (Derived) [Form: (x, y, z)]
        /// </summary>
        public float[] DesiredVelocity 
        { 
            get { return mManager.agentStates[managerIndex].desiredVelocity; } 
        }

        /// <summary>
        /// The actual agent velocity. [Form: (x, y, z)]
        /// </summary>
        public float[] Velocity 
        { 
            get { return mManager.agentStates[managerIndex].velocity; } 
        }

        /// <summary>
        /// Indicates whether the object's resources have been disposed.
        /// </summary>
        public bool IsDisposed { get { return (root == IntPtr.Zero); } }

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

        /// <summary>
        /// Gets the current agent configuration.
        /// </summary>
        /// <returns>The current agent configuration.</returns>
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
        /// <param name="config">The new agent configuration.</param>
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
        /// needed. (Such as happens when following a moving target.)</p>
        /// </remarks>
        /// <param name="polyRef">The refernece id of the polygon containing
        /// the targetPoint.</param>
        /// <param name="targetPoint">The target position.</param>
        /// <returns>TRUE if the target was successfully set.</returns>
        public bool RequestMoveTarget(uint polyRef, float[] targetPoint)
        {
            if (IsDisposed)
                return false;
            return CrowdManagerEx.RequestMoveTarget(mManager.root
                , managerIndex
                , polyRef
                , targetPoint);
        }

        /// <summary>
        /// Adjusts the position of an agent's move target.
        /// </summary>
        /// <remarks
        /// ><p>This method expects small increments and is suitable
        /// to call every frame.  (Such as is required when the target is
        /// moving frequently.)</p></remarks>
        /// <param name="polyRef">The refernece id of the polygon containing
        /// the targetPoint.</param>
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

        /// <summary>
        /// Gets the corridor data related to the agent.
        /// </summary>
        /// <remarks>
        /// <p>Only available after after a <see cref="CrowdManager"/>
        /// update.</p>
        /// </remarks>
        /// <param name="buffer">
        /// The buffer to hold the corridor data. (Out)</param>
        public void GetCorridor(PathCorridorData buffer)
        {
            if (!IsDisposed)
                CrowdAgentEx.GetPathCorridor(root, buffer);
        }

        /// <summary>
        /// Gets the boundary data related to the agent.
        /// </summary>
        /// <remarks>
        /// <p>Only available after after a <see cref="CrowdManager"/>
        /// update.</p>
        /// </remarks>
        /// <param name="buffer">The buffer to hold the boundary
        /// data. (Out)</param>
        public void GetBoundary(LocalBoundaryData buffer)
        {
            if (!IsDisposed)
                CrowdAgentEx.GetLocalBoundary(root, buffer);
        }

        /// <summary>
        /// Gets data related to the current agent neighbors.
        /// 
        /// </summary>
        /// <remarks>
        /// <p>Only available after after a <see cref="CrowdManager"/>
        /// update.</p>
        /// </remarks>
        /// <param name="buffer">The buffer to load. 
        /// [Size: >= <see cref="CrowdNeighbor.MaxNeighbors"/>]</param>
        /// <returns>The number of neighbors in the buffer.</returns>
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

        /// <summary>
        /// Gets the agent associated with the specified neighbor.
        /// </summary>
        /// <param name="neighbor">The agent neighbor data.</param>
        /// <returns>The agent associated with the neighbor data.</returns>
        public CrowdAgent GetNeighbor(CrowdNeighbor neighbor)
        {
            if (IsDisposed || neighbor.index >= mManager.MaxAgents)
                return null;
            return mManager.mAgents[neighbor.index];
        }

        /// <summary>
        /// Gets the cordner data for the agent.
        /// </summary>
        /// <remarks>
        /// <p>Only available after after a <see cref="CrowdManager"/>
        /// update.</p>
        /// </remarks>
        /// <param name="buffer">The buffer to load with corner data.</param>
        public void GetCornerData(CrowdCornerData buffer)
        {
            if (!IsDisposed)
                CrowdAgentEx.GetCorners(root, buffer);
        }
    }
}
