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
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Provides local steering behaviors for a group of agents.
    /// </summary>
    /// <remarks>
    /// <p>Behavior is undefined if object methods are called after the object
    /// has been disposed.</p>
    /// TODO: DOC: Add use case details and warnings about local pathfinding.
    /// </remarks>
    public sealed class CrowdManager
        : IDisposable
    {
        /*
         * WARNING:  The current design is based on not allowing
         * re-initialization of the native class after it is constructed.  
         * Re-evaluate this class if initailization is added.
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
        
        /// <summary>
        /// References to the local wrapper classes for crowd agents.
        /// </summary>
        private CrowdAgent[] mAgents = null;

        private int mAgentCount = 0;
        private NavmeshQueryFilter mFilter = null;
        private CrowdProximityGrid mGrid = null;
        private NavmeshQuery mQuery = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dtCrowd">A pointer to an instance of a dtCrowd class.
        /// </param>
        internal CrowdManager(IntPtr dtCrowd)
        {
            root = dtCrowd;

            mAgents = new CrowdAgent[CrowdManagerEx.GetAgentMaxCount(root)];

            IntPtr ptr = CrowdManagerEx.GetQueryFilter(root);
            if (ptr != IntPtr.Zero)
                mFilter = 
                    new NavmeshQueryFilter(ptr, AllocType.ExternallyManaged);

            ptr = CrowdManagerEx.GetProximityGrid(root);
            mGrid = new CrowdProximityGrid(ptr);

            ptr = CrowdManagerEx.GetQuery(root);

            mQuery = 
                new NavmeshQuery(ptr, AllocType.ExternallyManaged);

        }

        ~CrowdManager()
        {
            Dispose();
        }

        /// <summary>
        /// Indicates whether the object's resources have been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Immediately frees all unmanaged resources allocated by the
        /// object.
        /// </summary>
        public void Dispose()
        {
            if (root != IntPtr.Zero)
            {
                foreach (CrowdAgent agent in mAgents)
                {
                    if (agent != null)
                        agent.Dispose();
                }
                mAgents = null;
                mAgentCount = 0;

                mFilter.RequestDisposal();
                mGrid.RequestDisposal();
                mQuery.RequestDisposal();

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
        public bool SetAvoidanceParams(int index
            , CrowdAvoidanceParams obstacleParams)
        {
            if (index < 0 || index >= MaxAvoidanceParams)
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
        public CrowdAvoidanceParams GetAvoidanceParams(int index)
        {
            if (index >= 0 && index < MaxAvoidanceParams)
            {
                IntPtr ptr = CrowdManagerEx.GetAvoidanceParams(root, index);

                if (ptr != IntPtr.Zero)
                    return (CrowdAvoidanceParams)Marshal.PtrToStructure(
                        ptr, typeof(CrowdAvoidanceParams));                
            }
            return new CrowdAvoidanceParams();
        }

        /// <summary>
        /// The number of agents being managed.
        /// </summary>
        public int AgentCount
        {
            get { return mAgentCount; }
        }

        /// <summary>
        /// Gets agent information for the specified index.
        /// </summary>
        /// <remarks>
        /// <p>The index is one of the values returned by the 
        /// <see cref="AddAgent"/> method.  The index is valid until
        /// <see cref="RemoveAgent"/> is called.</p>
        /// <p>Use the <see cref="GetAllAgents"/> method to get all agents.</p>
        /// <p>Getting an agent for an index that is currently inactive will
        /// return an agent in an undefined state.</p>
        /// </remarks>
        /// <param name="index">An index between zero and 
        /// <see cref="AgentMaxCount"/>.</param>
        /// <returns>The agent for the specified index.</returns>
        public CrowdAgent GetAgent(int index)
        {
            return mAgents[index];
        }

        /// <summary>
        /// The maximum agents that can be managed by the object.
        /// </summary>
        public int AgentMaxCount
        {
            get { return mAgents.Length; }
        }

        /// <summary>
        /// Undocumented.
        /// </summary>
        public CrowdProximityGrid ProximityGrid
        {
            get { return mGrid; }
        }

        /// <summary>
        /// Updates the steering for all agents.
        /// </summary>
        /// <param name="deltaTime">The time in seconds to update the
        /// simulation.</param>
        public void Update(float deltaTime)
        {
            CrowdManagerEx.Update(root, deltaTime);
        }

        /// <summary>
        /// The extents used by the manager when it performs queries against
        /// the navigation mesh.
        /// </summary>
        /// <remarks>
        /// <p>All agents and targets should remain within these
        /// distances of the navigation mesh surface.</p>
        /// <p>This value is immutable for the life of the manager.</p>
        /// </remarks>
        /// <returns>The extents.</returns>
        public float[] GetQueryExtents()
        {
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
            return CrowdManagerEx.GetVelocitySampleCount(root);
        }

        /// <summary>
        /// Adds an agent to the manager.
        /// </summary>
        /// <param name="position">The position of the agent.</param>
        /// <param name="agentParams">The agent configuration.</param>
        /// <returns>The index of the agent.</returns>
        public int AddAgent(float[] position, CrowdAgentParams agentParams)
        {
            if (position == null || position.Length != 3)
                return -1;

            int index = CrowdManagerEx.AddAgent(root, position, agentParams);

            if (index == -1)
                return index;

            IntPtr agent = CrowdManagerEx.GetAgent(root, index);

            mAgents[index] = new CrowdAgent(agent);
            mAgentCount++;

            return index;
        }

        /// <summary>
        /// Gets all active agents.
        /// </summary>
        /// <returns>An array of all active agents.</returns>
        public CrowdAgent[] GetAllAgents()
        {
            if (mAgentCount == 0)
                return null;

            CrowdAgent[] result = new CrowdAgent[mAgentCount];
            int count = 0;

            foreach (CrowdAgent agent in mAgents)
            {
                if (agent != null)
                    result[count++] = agent;
            }

            return result;
        }

        /// <summary>
        /// Updates the configuration for an agent.
        /// </summary>
        /// <param name="index">An index between zero and 
        /// <see cref="AgentMaxCount"/>.</param>
        /// <param name="agentParams">The new agent configuration.</param>
        public void UpdateAgentParameters(int index
            , CrowdAgentParams agentParams)
        {
            CrowdManagerEx.UpdateAgentParameters(root, index, agentParams);
        }

        /// <summary>
        /// Removes an agent from the manager.
        /// </summary>
        /// <remarks>
        /// All associated references to the agent are immediately disposed.
        /// Continued use will result in undefined behavior.
        /// </remarks>
        /// <param name="index">The index of the agent to remove.</param>
        public void RemoveAgent(int index)
        {
            if (index < 0 || index >= mAgents.Length
                    || mAgents[index] == null)
                return;

            mAgents[index].Dispose();
            mAgents[index] = null;
            mAgentCount--;

            CrowdManagerEx.RemoveAgent(root, index);
        }

        /// <summary>
        /// The query filter used by the manager.
        /// </summary>
        /// <remarks>
        /// Updating the state of the filter will alter the steering 
        /// behaviors of the agents.
        /// </remarks>
        public NavmeshQueryFilter QueryFilter
        {
            get { return mFilter; }
        }

        /// <summary>
        /// The navmesh query used by the manager.
        /// </summary>
        /// <remarks>
        /// <p>This configuration of this query makes it only useful for
        /// local pathfinding queries. (Its search size is limited.)</p>
        /// </remarks>
        public NavmeshQueryLite Query
        {
            get { return mQuery.LiteQuery; }
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
        public static CrowdManager Build(int maxAgents
            , float maxAgentRadius
            , Navmesh navmesh)
        {
            if (navmesh.IsDisposed)
                return null;

            IntPtr root = CrowdManagerEx.Alloc(Math.Max(1, maxAgents)
                , Math.Max(0, maxAgentRadius)
                , navmesh.root);

            if (root == IntPtr.Zero)
                return null;

            return new CrowdManager(root);
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
        /// <param name="polyId">The id of the navmesh polygon where the 
        /// position is located.</param>
        /// <param name="position">The target position.</param>
        /// <returns>TRUE if the target was successfully set.</returns>
        public bool RequestMoveTarget(int agentIndex
            , uint polyId
            , float[] position)
        {
            return CrowdManagerEx.RequestMoveTarget(root
                , agentIndex
                , polyId
                , position);
        }

        /// <summary>
        /// Adjusts the position of an agent's move target.
        /// </summary>
        /// <remarks><p>This method expects small increments and is suitable
        /// to call every frame.  (Such as is required when the target is
        /// moving.)</p></remarks>
        /// <param name="agentIndex">The index of the agent.</param>
        /// <param name="polyId">The id of the navmesh polygon where the 
        /// position is located.</param>
        /// <param name="position">The adjusted target position.</param>
        /// <returns>TRUE if the adjustment was successfully applied.</returns>
        public bool AdjustMoveTarget(int agentIndex
            , uint polyId
            , float[] position)
        {
            return CrowdManagerEx.AdjustMoveTarget(root
                , agentIndex
                , polyId
                , position);
        }

        /// <summary>
        /// Gets the up-to-date core (non-debug) state for all active agents.
        /// </summary>
        /// <remarks>
        /// <p>This is the most efficient method of getting core agent data
        /// from the crowd manager after an update.</p>
        /// <p>WARNING: All structures in the data array must be 
        /// initialized before passing it to this method.</p>
        /// </remarks>
        /// <param name="agentData">An array containing <b>initialized</b> 
        /// agent data. (Should be sized to hold at least the expected number 
        /// of agents.)
        /// </param>
        /// <returns>The number of agents returned.</returns>
        public int GetAgentCoreData(CrowdAgentCoreData[] agentData)
        {
            return CrowdManagerEx.GetAgentCoreData(root
                , agentData
                , agentData.Length);
        }

    }
}
