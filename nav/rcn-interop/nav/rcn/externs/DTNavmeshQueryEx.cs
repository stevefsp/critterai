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
    public static class DTNavmeshQueryEx
    {

        // Source header: DetourNavmeshQueryEx.h

        [DllImport("cai-nav-rcn", EntryPoint = "dtnqFree")]
        public static extern void Free(ref IntPtr dtNavQuery);

        /// <summary>
        /// Solid (non-portal) wall segments.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="polyId"></param>
        /// <param name="filter">Applied only to neighbors?</param>
        /// <param name="segments"></param>
        /// <param name="segmentCount"></param>
        /// <param name="maxSegments"></param>
        /// <returns></returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqGetPolyWallSegments")]
        public static extern uint GetPolyWallSegments(IntPtr query
            , uint polyId
            , IntPtr filter
            , [In, Out] float[] segments
            , ref int segmentCount
            , int maxSegments);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindNearestPoly")]
        public static extern uint GetNearestPoly(IntPtr query
            , [In] float[] position
            , [In] float[] extents
		    , IntPtr filter
		    , ref uint nearestRef
            , [In, Out] float[] nearestPoint);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqClosestPointOnPoly")]
        public static extern uint GetNearestPoint(IntPtr query
            , uint polyId
            , [In] float[] position
            , [In, Out] float[] resultPoint);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqClosestPointOnPolyBoundary")]
        public static extern uint GetNearestBoundaryPoint(IntPtr query 
            , uint polyId
            , [In] float[] position
            , [In, Out] float[] resultPoint);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqQueryPolygons")]
        public static extern uint GetPolygons(IntPtr query
                , [In] float[] position
                , [In] float[] extents
                , IntPtr filter
                , [In, Out] uint[] resultPolyIds
                , ref int resultCount
                , int maxResult);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPolysAroundCircle")]
        public static extern uint FindPolygons(IntPtr query
                , uint startPolyId
                , [In] float[] position
                , float radius
                , IntPtr filter
                , [In, Out] uint[] resultPolyIds  // Optional
                , [In, Out] uint[] resultParentIds // Optional
                , [In, Out] float[] resultCosts // Optional
                , ref int resultCount
                , int maxResult);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPolysAroundShape")]
        public static extern uint FindPolygons(IntPtr query 
            , uint startPolyId
            , [In] float[] vertices
            , int vertexCount
	        , IntPtr filter
	        , [In, Out] uint[] resultPolyIds
            , [In, Out] uint[] resultParentIds
            , [In, Out] float[] resultCosts
	        , ref int resultCount
            , int maxResult);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqFindLocalNeighbourhood")]
        public static extern uint GetPolygonsLocal(IntPtr query
            , uint startPolyId
            , [In] float[] position
            , float radius
            , IntPtr filter
            , [In, Out] uint[] resultPolyIds
            , [In, Out] uint[] resultParentIds
            , ref int resultCount
            , int maxResult);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqGetPolyHeight")]
        public static extern uint GetPolyHeight(IntPtr query
            , uint polyId
            , [In] float[] position
            , ref float height);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindDistanceToWall")]
        public static extern uint FindDistanceToWall(IntPtr query
            , uint polyId
            , [In] float[] position
            , float searchRadius
	        , IntPtr filter
	        , ref float hitDistance
            , [In, Out] float[] hitPosition
            , [In, Out] float[] hitNormal);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPath")]
        public static extern uint FindPath(IntPtr query 
            , uint startPolyId
            , uint endPolyId
		    , [In] float[] startPosition
            , [In] float[] endPosition
		    , IntPtr filter
		    , [In, Out] uint[] resultPath
            , ref int pathCount
            , int maxPath);

        /// <summary>
        /// Returns TRUE if the polygon is in the current closed list.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="polyId"></param>
        /// <returns></returns>
        /// <remarks>
        /// <p>The closed list is the list of polygons that were fully evaluated
        /// during a find operation.</p>
        /// <p>All methods prefixed with "Find" and all sliced path methods are
        /// operations that can impact the closed list.  The content of the 
        /// list will persist until the next find operation is performed.</p>
        /// </remarks>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqIsInClosedList")]
        public static extern bool IsInClosedList(IntPtr query
            , uint polyId);
	
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqRaycast")]
        public static extern uint Raycast(IntPtr query
            , uint startPolyId
            , [In] float[] startPosition
            , [In] float[] endPosition
	        , IntPtr filter
	        , ref float hitParameter  // Very high number (> 1E38) if no hit.
            , [In, Out] float[] hitNormal
            , [In, Out] uint[] path
            , ref int pathCount
            , int maxPath);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindStraightPath")]
        public static extern uint GetStraightPath(IntPtr query
            , [In] float[] startPosition
            , [In] float[] endPosition
		    , [In] uint[] path
            , int pathSize
	        , [In, Out] float[] straightPathPoints
            , [In, Out] byte[] straightPathFlags
            , [In, Out] uint[] straightPathIds
	        , ref int straightPathCount
            , int maxStraightPath);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqMoveAlongSurface")]
        public static extern uint MoveAlongSurface(IntPtr query
            , uint startPolyId
            , [In] float[] startPosition
            , [In] float[] endPosition
            , IntPtr filter
            , [In, Out] float[] resultPosition
            , [In, Out] uint[] visitedPolyIds
            , ref int visitedCount
            , int maxVisited);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqInitSlicedFindPath")]
        public static extern uint InitSlicedFindPath(IntPtr query
            , uint startPolyId
            , uint endPolyId
            , [In] float[] startPosition
            , [In] float[] endPosition
            , IntPtr filter);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqUpdateSlicedFindPath")]
        public static extern uint UpdateSlicedFindPath(IntPtr query
            , int maxIterations
            , ref int actualIterations);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqFinalizeSlicedFindPath")]
        public static extern uint FinalizeSlicedFindPath(IntPtr query
            , [In, Out] uint[] path
            , ref int pathCount
            , int maxPath);
    }
}
