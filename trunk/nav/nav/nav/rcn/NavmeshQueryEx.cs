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
    /// <summary>
    /// Provides interop method signatures related to the dtNavMeshQuery class.
    /// </summary>
    /// <remarks>
    /// <p>Unless otherwise noted, all methods in the class require a fully
    /// initialized and ready to use dtNavMeshQuery object.</p>
    /// <p>Many of the methods in this class require valid polygon ids.
    /// See <see cref="NavmeshEx"/> for information on polygon ids.</p>
    /// </remarks>
    internal static class NavmeshQueryEx
    {

        // Source header: DetourNavmeshQueryEx.h

        [DllImport("cai-nav-rcn", EntryPoint = "rcnBuildDTNavQuery")]
        public static extern NavStatus BuildNavmeshQuery(IntPtr navmesh
            , int maxNodes
            , ref IntPtr resultQuery);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnqFree")]
        public static extern void FreeEx(ref IntPtr query);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqGetPolyWallSegments")]
        public static extern NavStatus GetPolyWallSegments(IntPtr query
            , uint polyRef
            , IntPtr filter
            , [In, Out] float[] segmentVerts
            , [In, Out] uint[] segmentPolyRefs
            , ref int segmentCount
            , int maxSegments);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindNearestPoly")]
        public static extern NavStatus GetNearestPoly(IntPtr query
            , [In] float[] position
            , [In] float[] extents
		    , IntPtr filter
		    , ref uint nearestPolyRef
            , [In, Out] float[] nearestPoint);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqClosestPointOnPoly")]
        public static extern NavStatus GetNearestPoint(IntPtr query
            , uint polyRef
            , [In] float[] position
            , [In, Out] float[] resultPoint);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqClosestPointOnPolyBoundary")]
        public static extern NavStatus GetNearestPointF(IntPtr query 
            , uint polyRef
            , [In] float[] position
            , [In, Out] float[] resultPoint);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqQueryPolygons")]
        public static extern NavStatus GetPolys(IntPtr query
                , [In] float[] position
                , [In] float[] extents
                , IntPtr filter
                , [In, Out] uint[] resultPolyRefs
                , ref int resultCount
                , int maxResult);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPolysAroundCircle")]
        public static extern NavStatus FindPolys(IntPtr query
                , uint startPolyRef
                , [In] float[] position
                , float radius
                , IntPtr filter
                , [In, Out] uint[] resultPolyRefs  // Optional
                , [In, Out] uint[] resultParentRefs // Optional
                , [In, Out] float[] resultCosts // Optional
                , ref int resultCount
                , int maxResult);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPolysAroundShape")]
        public static extern NavStatus FindPolys(IntPtr query 
            , uint startPolyRef
            , [In] float[] verts
            , int vertCount
	        , IntPtr filter
	        , [In, Out] uint[] resultPolyRefs
            , [In, Out] uint[] resultParentRefs
            , [In, Out] float[] resultCosts
	        , ref int resultCount
            , int maxResult);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqFindLocalNeighbourhood")]
        public static extern NavStatus GetPolysLocal(IntPtr query
            , uint startPolyRef
            , [In] float[] position
            , float radius
            , IntPtr filter
            , [In, Out] uint[] resultPolyRefs
            , [In, Out] uint[] resultParentRefs
            , ref int resultCount
            , int maxResult);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqGetPolyHeight")]
        public static extern NavStatus GetPolyHeight(IntPtr query
            , uint polyRef
            , [In] float[] position
            , ref float height);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindDistanceToWall")]
        public static extern NavStatus FindDistanceToWall(IntPtr query
            , uint polyRef
            , [In] float[] position
            , float searchRadius
	        , IntPtr filter
	        , ref float distance
            , [In, Out] float[] closestPoint
            , [In, Out] float[] normal);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPath")]
        public static extern NavStatus FindPath(IntPtr query 
            , uint startPolyRef
            , uint endPolyRef
		    , [In] float[] startPosition
            , [In] float[] endPosition
		    , IntPtr filter
		    , [In, Out] uint[] resultPath
            , ref int pathCount
            , int maxPath);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqIsInClosedList")]
        public static extern bool IsInClosedList(IntPtr query
            , uint polyRef);
	
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqRaycast")]
        public static extern NavStatus Raycast(IntPtr query
            , uint startPolyRef
            , [In] float[] startPosition
            , [In] float[] endPosition
	        , IntPtr filter
	        , ref float hitParameter 
            , [In, Out] float[] hitNormal
            , [In, Out] uint[] path
            , ref int pathCount
            , int maxPath);

	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindStraightPath")]
        public static extern NavStatus GetStraightPath(IntPtr query
            , [In] float[] startPosition
            , [In] float[] endPosition
		    , [In] uint[] path
            , int pathStart
            , int pathSize
	        , [In, Out] float[] straightPathPoints
            , [In, Out] WaypointFlag[] straightPathFlags
            , [In, Out] uint[] straightPathRefs
	        , ref int straightPathCount
            , int maxStraightPath);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqMoveAlongSurface")]
        public static extern NavStatus MoveAlongSurface(IntPtr query
            , uint startPolyRef
            , [In] float[] startPosition
            , [In] float[] endPosition
            , IntPtr filter
            , [In, Out] float[] resultPosition
            , [In, Out] uint[] visitedPolyRefs
            , ref int visitedCount
            , int maxVisited);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqInitSlicedFindPath")]
        public static extern NavStatus InitSlicedFindPath(IntPtr query
            , uint startPolyRef
            , uint endPolyRef
            , [In] float[] startPosition
            , [In] float[] endPosition
            , IntPtr filter);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqUpdateSlicedFindPath")]
        public static extern NavStatus UpdateSlicedFindPath(IntPtr query
            , int maxIterations
            , ref int actualIterations);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqFinalizeSlicedFindPath")]
        public static extern NavStatus FinalizeSlicedFindPath(IntPtr query
            , [In, Out] uint[] path
            , ref int pathCount
            , int maxPath);
    }
}
