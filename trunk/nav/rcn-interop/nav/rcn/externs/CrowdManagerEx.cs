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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn.externs
{
    public static class CrowdManagerEx
    {
        public const int MaxObstacleAvoidanceParams = 8;
        // public const int MaxAgents = 128;

        [DllImport("cai-nav-rcn", EntryPoint = "dtcDetourCrowdAlloc")]
        public static extern IntPtr Alloc(int maxAgents
            , float maxAgentRadius
            , IntPtr dtNavmesh);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcDetourCrowdFree")]
        public static extern void FreeEx(IntPtr crowd);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcSetObstacleAvoidanceParams")]
        public static extern void SetObstacleAvoidanceParams(IntPtr crowd
            , int index
            , CrowdAvoidanceParams obstacleParams);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetObstacleAvoidanceParams")]
        public static extern IntPtr GetObstacleAvoidanceParams(IntPtr crowd
            , int index);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcGetAgent")]
        public static extern IntPtr GetAgent(IntPtr crowd, int idx);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcGetAgentCount")]
        public static extern int GetAgentMaxCount(IntPtr crowd);
    	
	    [DllImport("cai-nav-rcn", EntryPoint = "dtcAddAgent")]
        public static extern int AddAgent(IntPtr crowd
            , [In] float[] pos
            , CrowdAgentParams agentParams);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcUpdateAgentParameters")]
        public static extern void UpdateAgentParameters(IntPtr crowd
            , int index
            , CrowdAgentParams agentParams);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcRemoveAgent")]
        public static extern void RemoveAgent(IntPtr crowd
            , int index);

        /* 
         * On the native side, this is a constant pointer.  But I'm 
         * purposefully not protecting it on this side of the border.  
         * I want the filter to be mutable and this is a dirty but quick way 
         * of doing it.
         */
        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetFilter")]
        public static extern IntPtr GetQueryFilter(IntPtr crowd);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetQueryExtents")]
        public static extern void GetQueryExtents(IntPtr crowd
            , [In, Out] float[] extents);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetVelocitySampleCount")]
        public static extern int GetVelocitySampleCount(IntPtr crowd);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetGrid")]
        public static extern IntPtr GetProximityGrid(IntPtr crowd);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcUpdate")]
        public static extern void Update(IntPtr crowd, float deltaTime);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// WARNING: The pointer returned by this method looses its constant
        /// pointer property when it crosses the interop boundry.  
        /// Behavior is undefined if non-constant methods are utilized on
        /// the pointer.</remarks>
        /// <param name="crowd"></param>
        /// <returns></returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetNavMeshQuery")]
        public static extern IntPtr GetQuery(IntPtr crowd);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcRequestMoveTarget")]
        public static extern bool RequestMoveTarget(IntPtr crowd
            , int agentIndex
            , uint polyId
            , [In] float[] position);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcAdjustMoveTarget")]
        public static extern bool AdjustMoveTarget(IntPtr crowd
            , int agentIndex
            , uint polyId
            , [In] float[] position);

        [DllImport("cai-nav-rcn", EntryPoint = "dtpcGetPathCorridorData")]
        public static extern void GetPathCorridorData(IntPtr corridor
            , ref PathCorridorData data);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetAgentCoreData")]
        public static extern int GetAgentCoreData(IntPtr crowd
                , [In, Out] CrowdAgentCoreData[] agentData
                , int agentDataSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetAgentDebugData")]
        public static extern void GetAgentDebugData(IntPtr agent
            , ref DTCrowdAgentDebugData agentData);
    }
}
