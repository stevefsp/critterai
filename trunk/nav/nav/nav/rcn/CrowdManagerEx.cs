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

namespace org.critterai.nav.rcn
{
    internal static class CrowdManagerEx
    {
        [DllImport("cai-nav-rcn", EntryPoint = "dtcDetourCrowdAlloc")]
        public static extern IntPtr Alloc(int maxAgents
            , float maxAgentRadius
            , IntPtr navmesh);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcDetourCrowdFree")]
        public static extern void FreeEx(IntPtr crowd);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcSetObstacleAvoidanceParams")]
        public static extern void SetAvoidanceParams(IntPtr crowd
            , int index
            , [In] CrowdAvoidanceParams obstacleParams);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetObstacleAvoidanceParams")]
        public static extern void GetAvoidanceParams(IntPtr crowd
            , int index
            , [In, Out] CrowdAvoidanceParams config);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcGetAgent")]
        public static extern IntPtr GetAgent(IntPtr crowd, int idx);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcGetAgentCount")]
        public static extern int GetAgentMaxCount(IntPtr crowd);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcAddAgent")]
        public static extern int AddAgent(IntPtr crowd
            , [In] float[] pos
            , ref CrowdAgentParams agentParams
            , ref IntPtr agent
            , ref CrowdAgentCoreState initialState);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcUpdateAgentParameters")]
        public static extern void UpdateAgentParameters(IntPtr crowd
            , int index
            , ref CrowdAgentParams agentParams);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcRemoveAgent")]
        public static extern void RemoveAgent(IntPtr crowd
            , int index);

        /* 
         * On the native side, the query filter is a constant pointer.  But I'm 
         * purposefully not protecting it on the managed side.  
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
        public static extern void Update(IntPtr crowd
            , float deltaTime
            , [In, Out] CrowdAgentCoreState[] coreStates);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcGetNavMeshQuery")]
        public static extern IntPtr GetQuery(IntPtr crowd);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcRequestMoveTarget")]
        public static extern bool RequestMoveTarget(IntPtr crowd
            , int agentIndex
            , uint polyRef
            , [In] float[] position);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtcAdjustMoveTarget")]
        public static extern bool AdjustMoveTarget(IntPtr crowd
            , int agentIndex
            , uint polyRef
            , [In] float[] position);
    }
}
