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
    /// <summary>
    /// Provides local steering behaviors for a group of agents.
    /// </summary>
    /// <remarks>
    /// <p>It is important to remember that this class is meant to provide 
    /// 'local' movement.  It is not meant to provide automatic pathfinding 
    /// services over long distances. Agents should be given targets that are 
    /// no more than two or three straight path corners away.  Much further
    /// and the agent will move toward the target but may fail to find it.</p>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
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
        public const int MaxAvoidanceParams = 8;

        /// <summary>
        /// An instance of a dtCrowd object.
        /// </summary>
        internal IntPtr root;
        
        private NavmeshQueryFilter mFilter = null;
        private CrowdProximityGrid mGrid = null;
        private NavmeshQuery mQuery = null;

        private float mMaxAgentRadius;
        private Navmesh mNavmesh;
        
        internal CrowdAgent[] mAgents;
        // Needs to be a separate array since it is used as an argument
        // for an interop call.
        internal CrowdAgentCoreState[] agentStates;

        /// <summary>
        /// The maximum number agents that can be managed by the object.
        /// </summary>
        public int MaxAgents { get { return mAgents.Length; } }

        /// <summary>
        /// The maximum allowed agent radius supported by the object.
        /// </summary>
        public float MaxAgentRadius { get { return mMaxAgentRadius; } }

        /// <summary>
        /// The navigation mesh used by the object.
        /// </summary>
        public Navmesh Navmesh { get { return mNavmesh; } }

        /// <summary>
        /// The query filter used by the manager.
        /// </summary>
        /// <remarks>
        /// Updating the state of this filter will alter the steering 
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
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero || mNavmesh.IsDisposed); }
        }

        /// <summary>
        /// The agent buffer entry.
        /// </summary>
        /// <param name="index">The buffer index. 
        /// [Limit: 0 &lt;= value &lt; <see cref="MaxAgents"/>]
        /// </param>
        /// <returns>The agent buffer entry. (Null if no agent at the index.)
        /// </returns>
        public CrowdAgent this[int index]
        {
            get { return (IsDisposed ? null : mAgents[index]); }
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

        /// <summary>
        /// Destructor
        /// </summary>
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
        /// agents assigned to different avoidance behaviors via the
        /// <see cref="CrowdAgentParams"/> object.
        /// </remarks>
        /// <param name="index">The index. 
        /// [Limits: 0 &lt;= value &lt; <see cref="MaxAvoidanceParams"/>].
        /// </param>
        /// <param name="config">The avoidance configuration.</param>
        /// <returns>TRUE if the configuration is successfully set.</returns>
        public bool SetAvoidanceConfig(int index
            , CrowdAvoidanceParams config)
        {
            if (IsDisposed || index < 0 || index >= MaxAvoidanceParams)
                return false;

            CrowdManagerEx.SetAvoidanceParams(root, index, config);

            return true;
        }

        /// <summary>
        /// Gets the avoidance configuration for a particular index.
        /// </summary>
        /// <remarks>
        /// Getting a configuration for an index that has not been set will
        /// return a configuration in an undefined state.
        /// </remarks>
        /// <param name="index">The index 
        /// [Limits: 0 &lt;= value &lt; <see cref="MaxAvoidanceParams"/>].
        /// </param>
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

        // TODO: v0.4: Remove.
        ///// <summary>
        ///// Gets the agent buffer containing all agents currently being
        ///// managed by the manager.
        ///// </summary>
        ///// <remarks>
        ///// <p>The agents may be dispersed throughout the buffer with
        ///// null entries in between. Also, there is no quarentee that the order
        ///// of the agents will match the order they were added to the manager.
        ///// </p>
        ///// </remarks>
        ///// <param name="buffer">A buffer to load with the agent data.
        ///// [Size: >= MaxAgents]</param>
        ///// <returns>The number of agents in the buffer.</returns>
        //public int GetAgentBuffer(CrowdAgent[] buffer)
        //{
        //    if (IsDisposed || buffer.Length < mAgents.Length)
        //        return -1;

        //    int result = 0;
        //    for (int i = 0; i < mAgents.Length; i++)
        //    {
        //        buffer[i] = mAgents[i];
        //        result += (mAgents[i] == null) ? 0 : 1;
        //    }
        //    return result;
        //}

        /// <summary>
        /// Adds an agent to the manager.
        /// </summary>
        /// <param name="position">The current position of the agent within the
        /// navigation mesh.
        /// </param>
        /// <param name="agentParams">The agent configuration.</param>
        /// <returns>A reference to the agent object created by the manager,
        /// or null on error.</returns>
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
        /// The <see cref="CrowdAgent"/> object will be immediately disposed.
        /// Continued use will result in undefined behavior.
        /// </remarks>
        /// <param name="agent">The agent to remove.</param>
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
        /// Updates the steering and positions for all agents.
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
        /// <p>The extents remains constant over the life of the object.</p>
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
        /// Gets the velocity sample count.
        /// </summary>
        /// <returns>The velocity sample count.</returns>
        public int GetVelocitySampleCount()
        {
            if (IsDisposed)
                return -1;
            return CrowdManagerEx.GetVelocitySampleCount(root);
        }

        /// <summary>
        /// Gets the proximity grid.
        /// </summary>
        public CrowdProximityGrid ProximityGrid { get { return mGrid; } }

        /// <summary>
        /// The navmesh query used by the manager.
        /// </summary>
        /// <remarks>
        /// <p>The configuration of this query makes it useful only for
        /// local pathfinding queries. (Its search size is limited.) Also,
        /// it is marked as <see cref="NavmeshQuery.IsRestricted">
        /// restricted.</see></p>
        /// </remarks>
        public NavmeshQuery Query { get { return mQuery; } }
    }
}
