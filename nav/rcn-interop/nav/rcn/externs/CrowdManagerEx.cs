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

namespace org.critterai.nav.rcn.externs
{
    /// <summary>
    /// Provides the method signatures for all interop method calls related
    /// to the Detour crowd manager class.
    /// </summary>
    /// <remarks>
    /// <p>Unless otherwise noted, all methods in the class require a fully
    /// initialized and ready to use instance of the crowd manager class.</p>
    /// </remarks>
    public static class CrowdManagerEx
    {
        /// <summary>
        /// The maximum number of avoidance configurations that can be
        /// associated with the manager.
        /// </summary>
        public const int MaxAvoidanceParams = 8;

        /// <summary>
        /// Allocates and initializes a new dtCrowd object.
        /// </summary>
        /// <remarks>The <see cref="FreeEx"/> method must be called
        /// on all pointers returned by this method before the last
        /// reference to the pointer is releases.  Otherwise a memory leak
        /// will occur.</remarks>
        /// <param name="maxAgents">The maximum number of agents
        /// the crowd manager will support.</param>
        /// <param name="maxAgentRadius">The maximum radius for agents
        /// that will use the crowd manager.</param>
        /// <param name="navmesh">The navigation mesh the crowd manager
        /// will use.</param>
        /// <returns>A pointer to the crowd manager, or a null pointer
        /// if the creation was not successful.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcDetourCrowdAlloc")]
        public static extern IntPtr Alloc(int maxAgents
            , float maxAgentRadius
            , IntPtr navmesh);

        /// <summary>
        /// Frees the unmanaged resources allocated by a successful call to
        /// <see cref="Alloc"/>.
        /// </summary>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcDetourCrowdFree")]
        public static extern void FreeEx(IntPtr crowd);

        /// <summary>
        /// Sets the avoidance configuration for a particular index.
        /// </summary>
        /// <remarks>
        /// Multiple avoidance configurations can be set for the manager with
        /// each agent assigned a configuration as appropriate.
        /// </remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="index">An index between zero and 
        /// <see cref="MaxAvoidanceParams"/>.</param>
        /// <param name="obstacleParams">The avoidance configuration.</param>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtcSetObstacleAvoidanceParams")]
        public static extern void SetAvoidanceParams(IntPtr crowd
            , int index
            , CrowdAvoidanceParams obstacleParams);

        /// <summary>
        /// Gets the avoidance configuration for a particular index.
        /// </summary>
        /// <remarks>
        /// Getting a configuration for an index that has not been set will
        /// return a configuration in an undefined state.
        /// </remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="index">An index between zero and 
        /// <see cref="MaxAvoidanceParams"/>.</param>
        /// <returns>An avoidance configuration.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetObstacleAvoidanceParams")]
        public static extern IntPtr GetAvoidanceParams(IntPtr crowd
            , int index);

        /// <summary>
        /// Gets a pointer to an agent managed by the dtCrowd object.
        /// </summary>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="idx">The agent index.</param>
        /// <returns></returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtcGetAgent")]
        public static extern IntPtr GetAgent(IntPtr crowd, int idx);

        /// <summary>
        /// Gets the maximum number of agents supported by the dtCrowd object.
        /// </summary>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <returns>The maximum number of agents supported by the
        /// dtCrowd object.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtcGetAgentCount")]
        public static extern int GetAgentMaxCount(IntPtr crowd);

        /// <summary>
        /// Adds an agent to the manager.
        /// </summary>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="position">The position of the agent.</param>
        /// <param name="agentParams">The agent configuration.</param>
        /// <returns>The index of the agent.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtcAddAgent")]
        public static extern int AddAgent(IntPtr crowd
            , [In] float[] pos
            , CrowdAgentParams agentParams);

        /// <summary>
        /// Updates the configuration for an agent.
        /// </summary>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="index">An index between zero and 
        /// <see cref="AgentMaxCount"/>.</param>
        /// <param name="agentParams">The new agent configuration.</param>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtcUpdateAgentParameters")]
        public static extern void UpdateAgentParameters(IntPtr crowd
            , int index
            , CrowdAgentParams agentParams);

        /// <summary>
        /// Removes an agent from the manager.
        /// </summary>
        /// <remarks>
        /// All associated references to the agent are immediately disposed.
        /// Continued use will result in undefined behavior.
        /// </remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="index">The index of the agent to remove.</param>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtcRemoveAgent")]
        public static extern void RemoveAgent(IntPtr crowd
            , int index);

        /* 
         * On the native side, this is a constant pointer.  But I'm 
         * purposefully not protecting it on this side of the border.  
         * I want the filter to be mutable and this is a dirty but quick way 
         * of doing it.
         */

        /// <summary>
        /// Gets the query filter used by the dtCrowd object.
        /// </summary>
        /// <remarks>
        /// The object returned by this method is managed by its dtCrowd
        /// object.  So it should not be manually freed.
        /// </remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <returns>A pointer to the dtQueryFilter used by the
        /// dtCrowd object.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetFilter")]
        public static extern IntPtr GetQueryFilter(IntPtr crowd);

        /// <summary>
        /// The extents used by the manager when it performs queries against
        /// the navigation mesh.
        /// </summary>
        /// <remarks>
        /// <p>All agents and targets should remain within these
        /// distances of the navigation mesh surface.</p>
        /// <p>This value is immutable for the life of the manager.</p>
        /// </remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <returns>The extents.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetQueryExtents")]
        public static extern void GetQueryExtents(IntPtr crowd
            , [In, Out] float[] extents);

        /// <summary>
        /// Undocumented.
        /// </summary>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <returns>The velocity sample count.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetVelocitySampleCount")]
        public static extern int GetVelocitySampleCount(IntPtr crowd);


        /// <summary>
        /// Undocumented
        /// </summary>
        /// <remarks>
        /// The object returned by this method is managed by its dtCrowd
        /// object.  So it should not be manually freed.
        /// </remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <returns>A pointer to the dtCrowd object's dtProximityGrid object.
        /// </returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetGrid")]
        public static extern IntPtr GetProximityGrid(IntPtr crowd);

        /// <summary>
        /// Updates the steering for all agents.
        /// </summary>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="deltaTime">The time in seconds to update the
        /// simulation.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcUpdate")]
        public static extern void Update(IntPtr crowd, float deltaTime);

        /// <summary>
        /// A pointer to the dtNavMeshQuery object used by the dtCrowd object.
        /// </summary>
        /// <remarks>
        /// <p>WARNING: The pointer returned by this method looses its constant
        /// pointer property when it crosses the interop boundry.  
        /// Behavior is undefined if non-constant methods are utilized on
        /// the pointer.</p>
        /// <p>The object returned by this method is managed by its dtCrowd
        /// object.  So it should not be manually freed.</p>
        /// </remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <returns>The dtNavMeshQuery object used by the dtCrowd object.
        /// </returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetNavMeshQuery")]
        public static extern IntPtr GetQuery(IntPtr crowd);

        /// <summary>
        /// Sets the move target for an agent.
        /// </summary>
        /// <remarks>
        /// <p>This method is used when a new target is set.  Use 
        /// <see cref="AdjustMoveTarget"/> when only small adjustments are 
        /// needed. (Such as happens when following a moving target.</p>
        /// </remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="agentIndex">The index of the agent.</param>
        /// <param name="polyId">The id of the navmesh polygon where the 
        /// position is located.</param>
        /// <param name="position">The target position.</param>
        /// <returns>TRUE if the target was successfully set.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtcRequestMoveTarget")]
        public static extern bool RequestMoveTarget(IntPtr crowd
            , int agentIndex
            , uint polyId
            , [In] float[] position);

        /// <summary>
        /// Adjusts the position of an agent's move target.
        /// </summary>
        /// <remarks><p>This method expects small increments and is suitable
        /// to call every frame.  (Such as is required when the target is
        /// moving.)</p></remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="agentIndex">The index of the agent.</param>
        /// <param name="polyId">The id of the navmesh polygon where the 
        /// position is located.</param>
        /// <param name="position">The adjusted target position.</param>
        /// <returns>TRUE if the adjustment was successfully applied.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtcAdjustMoveTarget")]
        public static extern bool AdjustMoveTarget(IntPtr crowd
            , int agentIndex
            , uint polyId
            , [In] float[] position);

        /// <summary>
        /// Gets the up-to-date core (non-debug) state for all active agents.
        /// </summary>
        /// <remarks>
        /// <p>This is the most efficient method of getting core agent data
        /// from the crowd manager after an update.</p>
        /// <p>WARNING: All structures in the data array must be 
        /// initialized before passing it to this method.</p>
        /// </remarks>
        /// <param name="crowd">A pointer to a dtCrowd object.</param>
        /// <param name="agentData">An array containing <b>initialized</b> 
        /// agent data. (Should be sized to hold at least the expected number 
        /// of agents.)
        /// </param>
        /// <returns>The number of agents returned.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetAgentCoreData")]
        public static extern int GetAgentCoreData(IntPtr crowd
                , [In, Out] CrowdAgentCoreData[] agentData
                , int agentDataSize);

        /// <summary>
        /// Gets the current agent state.
        /// </summary>
        /// <param name="agent">A pointer to a dtCrowdAgent object.</param>
        /// <param name="data">An <b>initialized</b> data structure to load
        /// the state into.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetAgentDebugData")]
        public static extern void GetAgentDebugData(IntPtr agent
            , ref CrowdAgentDebugData data);
    }
}
