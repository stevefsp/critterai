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
using org.critterai.nav.rcn;
using org.critterai.interop;
using System.Collections.Generic;

namespace org.critterai.nav
{

    // TODO: Need to find a more elegant and efficient way to handle
    // agent core state.

    /// <summary>
    /// Provides local steering behaviors for a group of agents.
    /// </summary>
    /// <remarks>
    /// <p>Behavior is undefined if methods are called after the object
    /// has been disposed.</p>
    /// TODO: DOC: Add use case details and warnings about local pathfinding.
    /// </remarks>
    public sealed class CrowdManager
        : ManagedObject
    {
        /*
         * WARNING:  The current design is based on not allowing
         * re-initialization of the native class after it is constructed.  
         * Re-evaluate this class if re-initailization is added.
         */

        /// <summary>
        /// The maximum number of avoidance configurations that can be
        /// associated with the manager.
        /// </summary>
        public const int MaxAvoidanceParams =
            CrowdManagerEx.MaxAvoidanceParams;

        /// <summary>
        /// A pointer to an instance of a dtCrowd class.
        /// </summary>
        internal IntPtr root;
        
        private NavmeshQueryFilter mFilter = null;
        private CrowdProximityGrid mGrid = null;
        private NavmeshQuery mQuery = null;

        private float mMaxAgentRadius;
        private Navmesh mNavmesh;
        
        internal CrowdAgent[] mAgents;

        // Needs to be a separate array since it is uses as an argument
        // in an interop call.
        internal CrowdAgentCoreState[] agentStates;

        public int MaxAgents { get { return mAgents.Length; } }
        public float MaxAgentRadius { get { return mMaxAgentRadius; } }
        public Navmesh Navmesh { get { return mNavmesh; } }

        /// <summary>
        /// The query filter used by the manager.
        /// </summary>
        /// <remarks>
        /// Updating the state of the filter will alter the steering 
        /// behaviors of the agents.
        /// </remarks>
        public NavmeshQueryFilter QueryFilter { get { return mFilter; } }

        /// <summary>
        /// The number of agents being managed.
        /// </summary>
        public int AgentCount 
        {
            get 
            {
                int result = 0;
                for (int i = 0; i < mAgents.Length; i++)
                {
                    result += (mAgents[i] == null) ? 0 : 1;
                }
                return result;
            } 
        }

        /// <summary>
        /// Indicates whether the object's resources have been disposed.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero || mNavmesh.IsDisposed); }
        }

        /// <summary>
        /// Creates a new crowd manager.
        /// </summary>
        /// <param name="maxAgents">The maximum number of agents that can
        /// be added to the manager.</param>
        /// <param name="maxAgentRadius">The maximum allowed agent radius.
        /// </param>
        /// <param name="navmesh">The navigation mesh to use for steering
        /// related queries.</param>
        /// <returns>A ready to use crowd manager, or null if one could
        /// not be created.</returns>
        public CrowdManager(int maxAgents
            , float maxAgentRadius
            , Navmesh navmesh)
            : base(AllocType.External)
        {
            if (navmesh.IsDisposed)
                return;

            maxAgents = Math.Max(1, maxAgents);
            maxAgentRadius = Math.Max(0, maxAgentRadius);

            root = CrowdManagerEx.Alloc(maxAgents
                , maxAgentRadius
                , navmesh.root);

            if (root == IntPtr.Zero)
                return;

            mMaxAgentRadius = maxAgentRadius;
            mNavmesh = navmesh;

            mAgents = new CrowdAgent[maxAgents];
            agentStates = new CrowdAgentCoreState[maxAgents];

            IntPtr ptr = CrowdManagerEx.GetQueryFilter(root);
            mFilter = new NavmeshQueryFilter(ptr, AllocType.ExternallyManaged);

            ptr = CrowdManagerEx.GetProximityGrid(root);
            mGrid = new CrowdProximityGrid(ptr);

            ptr = CrowdManagerEx.GetQuery(root);
            mQuery = new NavmeshQuery(ptr, true, AllocType.ExternallyManaged);
        }

        ~CrowdManager()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Immediately frees all unmanaged resources allocated by the
        /// object.
        /// </summary>
        public override void RequestDisposal()
        {
            // There are no managed or local allocations.
            if (root != IntPtr.Zero)
            {
                mFilter.RequestDisposal();
                mFilter = null;
                mGrid.Dispose();
                mGrid = null;
                mQuery.RequestDisposal();
                mQuery = null;
                mNavmesh = null;
                mMaxAgentRadius = -1;
                agentStates = null;

                for (int i = 0; i < mAgents.Length; i++)
                {
                    if (mAgents[i] == null)
                        continue;
                    mAgents[i].Dispose();
                    mAgents[i] = null;
                }

                CrowdManagerEx.FreeEx(root);
                root = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Sets the avoidance configuration for a particular index.
        /// </summary>
        /// <remarks>
        /// Multiple avoidance configurations can be set for the manager with
        /// each agent assigned a configuration as appropriate.
        /// </remarks>
        /// <param name="index">An index between zero and 
        /// <see cref="MaxAvoidanceParams"/>.</param>
        /// <param name="obstacleParams">The avoidance configuration.</param>
        /// <returns>TRUE if the configuration is successfully set.</returns>
        public bool SetAvoidanceConfig(int index
            , CrowdAvoidanceParams obstacleParams)
        {
            if (IsDisposed || index < 0 || index >= MaxAvoidanceParams)
                return false;

            CrowdManagerEx.SetAvoidanceParams(root
                , index
                , obstacleParams);

            return true;
        }

        /// <summary>
        /// Gets the avoidance configuration for a particular index.
        /// </summary>
        /// <remarks>
        /// Getting a configuration for an index that has not been set will
        /// return a configuration in an undefined state.
        /// </remarks>
        /// <param name="index">An index between zero and 
        /// <see cref="MaxAvoidanceParams"/>.</param>
        /// <returns>An avoidance configuration.</returns>
        public CrowdAvoidanceParams GetAvoidanceConfig(int index)
        {
            if (!IsDisposed && index >= 0 && index < MaxAvoidanceParams)
            {
                CrowdAvoidanceParams result = new CrowdAvoidanceParams();
                CrowdManagerEx.GetAvoidanceParams(root
                    , index
                    , result);

                return result;             
            }
            return null;
        }

        // May contain nulls.
        public int GetAgentBuffer(CrowdAgent[] buffer)
        {
            if (IsDisposed || buffer.Length < mAgents.Length)
                return -1;

            int result = 0;
            for (int i = 0; i < mAgents.Length; i++)
            {
                buffer[i] = mAgents[i];
                result += (mAgents[i] == null) ? 0 : 1;
            }
            return result;
        }

        /// <summary>
        /// Adds an agent to the manager.
        /// </summary>
        /// <param name="position">The position of the agent.</param>
        /// <param name="agentParams">The agent configuration.</param>
        /// <returns>The index of the agent.</returns>
        public CrowdAgent AddAgent(float[] position
            , CrowdAgentParams agentParams)
        {
            if (IsDisposed || position == null || position.Length < 3)
                return null;

            IntPtr ptr = IntPtr.Zero;
            CrowdAgentCoreState initialState = new CrowdAgentCoreState();

            int index = CrowdManagerEx.AddAgent(root
                , position
                , ref agentParams
                , ref ptr
                , ref initialState);

            if (index == -1)
                return null;

            mAgents[index] = new CrowdAgent(this, ptr, index);
            agentStates[index] = initialState;

            return mAgents[index];
        }

        /// <summary>
        /// Removes an agent from the manager.
        /// </summary>
        /// <remarks>
        /// All associated references to the agent are immediately disposed.
        /// Continued use will result in undefined behavior.
        /// </remarks>
        /// <param name="index">The index of the agent to remove.</param>
        public void RemoveAgent(CrowdAgent agent)
        {
            for (int i = 0; i < mAgents.Length; i++)
            {
                if (mAgents[i] == agent)
                {
                    CrowdManagerEx.RemoveAgent(root, agent.managerIndex);
                    agent.Dispose();
                    mAgents[i] = null;
                    // Don't need to do anything about the core data array.
                }
            }
        }

        /// <summary>
        /// Updates the steering for all agents.
        /// </summary>
        /// <param name="deltaTime">The time in seconds to update the
        /// simulation.</param>
        public void Update(float deltaTime)
        {
            if (IsDisposed)
                return;

            CrowdManagerEx.Update(root, deltaTime, agentStates);
        }

        /// <summary>
        /// The extents used by the manager when it performs queries against
        /// the navigation mesh. (xAxis, yAxis, zAxis)
        /// </summary>
        /// <remarks>
        /// <p>All agents and targets should remain within these
        /// distances of the navigation mesh surface.  For example, if
        /// the yAxis extent is 0.5, then the agent should remain between
        /// 0.5 above and 0.5 below the surface of the mesh.</p>
        /// <p>The extents remain constant over the life of the object.</p>
        /// </remarks>
        /// <returns>The extents.</returns>
        public float[] GetQueryExtents()
        {
            if (IsDisposed)
                return null;
            float[] result = new float[3];
            CrowdManagerEx.GetQueryExtents(root, result);
            return result;
        }
        /// <summary>
        /// Undocumented.
        /// </summary>
        /// <returns>The velocity sample count.</returns>
        public int GetVelocitySampleCount()
        {
            if (IsDisposed)
                return -1;
            return CrowdManagerEx.GetVelocitySampleCount(root);
        }

        /// <summary>
        /// Undocumented.
        /// </summary>
        public CrowdProximityGrid ProximityGrid { get { return mGrid; } }

        /// <summary>
        /// The navmesh query used by the manager.
        /// </summary>
        /// <remarks>
        /// <p>The configuration of this query makes it only useful for
        /// local pathfinding queries. (Its search size is limited.) Also,
        /// it is marked as <see cref="NavmeshQuery.IsRestricted">
        /// restricted.</see></p>
        /// </remarks>
        public NavmeshQuery Query { get { return mQuery; } }
    }
}
