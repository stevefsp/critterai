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
    internal static class PathCorridorEx
    {
        [DllImport("cai-nav-rcn", EntryPoint = "rcnBuildDTNavQuery")]
        public static extern NavStatus BuildNavmeshQuery(IntPtr navmesh
            , int maxNodes
            , ref IntPtr resultQuery);

        [DllImport("cai-nav-rcn", EntryPoint = "dtpcAlloc")]
        public static extern IntPtr Alloc(int maxPath);

        [DllImport("cai-nav-rcn", EntryPoint = "dtpcFree")]
        public static extern void Free(IntPtr corridor);

        [DllImport("cai-nav-rcn", EntryPoint = "dtpcReset")]
        public static extern void Reset(IntPtr corridor
            , uint polyRef
            , [In] float[] position);

        [DllImport("cai-nav-rcn", EntryPoint = "dtpcFindCorners")]
        public static extern int FindCorners(IntPtr corridor
            , [In, Out] float[] cornerVerts
            , [In, Out] WaypointFlag[] cornerFlags
            , [In, Out] uint[] cornerPolys
            , int maxCorners
            , IntPtr navquery
            , IntPtr filter);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcOptimizePathVisibility")]
        public static extern void OptimizePathVisibility(IntPtr corridor
             , [In] float[] next
             , float pathOptimizationRange
             , IntPtr navquery
             , IntPtr filter);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcOptimizePathTopology")]
        public static extern bool OptimizePathTopology(IntPtr corridor
             , IntPtr navquery
             , IntPtr filter);
    	
	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcMoveOverOffmeshConnection")]
        public static extern bool MoveOverConnection(IntPtr corridor
             , uint offMeshConRef
             , [In, Out] uint[] refs // size 2
             , [In, Out] float[] startPos
             , [In, Out] float[] endPos
             , IntPtr navquery);
    	
	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcMovePosition")]
        public static extern void MovePosition(IntPtr corridor
            , [In] float[] npos
            , IntPtr navquery
            , IntPtr filter
            , [In, Out] float[] pos);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcMoveTargetPosition")]
        public static extern void MoveTargetPosition(IntPtr corridor
            , [In] float[] npos
            , IntPtr navquery
            , IntPtr filter
            , [In, Out] float[] pos);
    	
	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcSetCorridor")]
        public static extern void SetCorridor(IntPtr corridor
             , [In] float[] target
             , [In] uint[] path
             , int pathCount);
    	
	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcGetPos")]
        public static extern bool GetPosition(IntPtr corridor
             , [In] float[] position);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcGetTarget")]
        public static extern bool GetTarget(IntPtr corridor
             , [In] float[] target);
         	
	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcGetFirstPoly")]
        public static extern uint GetFirstPoly(IntPtr corridor);
    	
	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcGetPath")]
        public static extern int GetPath(IntPtr corridor
             , [In, Out] uint[] path
             , int maxPath);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtpcGetPathCount")]
        public static extern int GetPathCount(IntPtr corridor);

        [DllImport("cai-nav-rcn", EntryPoint = "dtpcGetData")]
        public static extern bool GetData(IntPtr corridor
            , [In, Out] PathCorridorData data);
    }
}
