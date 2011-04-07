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
    /// to the Detour navigation mesh query class.
    /// </summary>
    /// <remarks>
    /// <p>Unless otherwise noted, all methods in the class require a fully
    /// initialized and ready to use instance of the Detour navigation mesh 
    /// query class.</p>
    /// <p>Many of the methods in this class require valid polygon ids.
    /// See <see cref="DTNavmeshEx"/> for information on polygon ids.</p>
    /// </remarks>
    public static class NavmeshQueryEx
    {

        // Source header: DetourNavmeshQueryEx.h

        [DllImport("cai-nav-rcn", EntryPoint = "rcnBuildDTNavQuery")]
        public static extern NavmeshStatus BuildNavmeshQuery(IntPtr dtNavMesh
            , int maxNodes
            , ref IntPtr dtQuery);

        /// <summary>
        /// Free the unmanaged memory allocated during creation of a Detour
        /// navigation mesh query.
        /// </summary>
        /// <remarks>
        /// WARNING: A memory leak will occur if this method is not called 
        /// before the last reference to the query is released.
        /// </remarks>
        /// <param name="query">A pointer to the Detour navigation
        /// mesh query.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnqFree")]
        public static extern void FreeEx(ref IntPtr query);

        /// <summary>
        /// Returns the wall segments of the specified polygon.
        /// </summary>
        /// <remarks>
        /// <p>If the segmentPolyIds parameter is provided, then all polygon
        /// walls will be returned.  If the parameter is ommited, then only 
        /// the impassible walls arereturn.</p>
        /// <p>
        /// A portal wall will be returns as impassible if the filter argument
        /// results in the wall's neighbor polygon being marked as excluded.</p>
        /// <p>The wall segments of the polygon can be used for simple 2D
        /// collision detection.</p>
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="filter">The filter to apply to the query. Used to
        /// determine which polygon neighbors are considered accessible.</param>
        /// <param name="segmentVerts">An array to load the wall vertices into. 
        /// The vertices will be in the form (ax, ay, az, bx, by, bz).
        /// Length: [6 * maxSegments]</param>
        /// <param name="segmentPolyIds">Ids of the each segment's neighbor
        /// polygon. Or zero if the segment is considered impassible. 
        /// Length: [maxSegements] (Optional)</param>
        /// <param name="segmentCount">The number of wall segments returned
        /// in the segments array.</param>
        /// <param name="maxSegments">The maximum number of segments that
        /// can be returned.
        /// </param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqGetPolyWallSegments")]
        public static extern NavmeshStatus GetPolyWallSegments(IntPtr query
            , uint polyId
            , IntPtr filter
            , [In, Out] float[] segmentVerts
            , [In, Out] uint[] segmentPolyIds
            , ref int segmentCount
            , int maxSegments);

        /// <summary>
        /// Finds the nearest polygon to the specified point.
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="position">The center of the search location.</param>
        /// <param name="extents">The distance to search from the 
        /// position along each axis (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="nearestPolyId">The id of the nearest polygon. Or
        /// zero if none could be found within the query box. (Out)</param>
        /// <param name="nearestPoint">The nearest point on the polygon.
        /// (Optional Out)</param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindNearestPoly")]
        public static extern NavmeshStatus GetNearestPoly(IntPtr query
            , [In] float[] position
            , [In] float[] extents
		    , IntPtr filter
		    , ref uint nearestPolyId
            , [In, Out] float[] nearestPoint);

        /// <summary>
        /// Finds the closest point on a navigation polygon.
        /// </summary>
        /// <remarks>
        /// <p>Uses the height detail to provide the most accurate information.
        /// </p></remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="position">The position to search from in the form
        /// (x, y, z).</param>
        /// <param name="resultPoint">The closest point found in the form 
        /// (x, y, z) (Out)</param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqClosestPointOnPoly")]
        public static extern NavmeshStatus GetNearestPoint(IntPtr query
            , uint polyId
            , [In] float[] position
            , [In, Out] float[] resultPoint);

        /// <summary>
        /// Finds the closest point on the navigation polygon's xz-plane 
        /// boundary.
        /// </summary>
        /// <remarks>
        /// <p>Snaps the point to the polygon boundary if it is outside the
        /// polygon's xz-column.</p>
        /// <p>Does not change the y-value of the point.</p>
        /// <p>Much faster than <see cref="GetNearestPoint" />.</p></remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="position">The point to check in the form (x, y, z).
        /// </param>
        /// <param name="resultPoint">The closest point. (Out)</param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqClosestPointOnPolyBoundary")]
        public static extern NavmeshStatus GetNearestBoundaryPoint(IntPtr query 
            , uint polyId
            , [In] float[] position
            , [In, Out] float[] resultPoint);

        /// <summary>
        /// Finds the polygons that overlap the query box.
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="position">The center of the query box in the form
        /// (x, y, z).</param>
        /// <param name="extents">The search distance along each axis in the
        /// form (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyIds">The ids for the polygons that
        /// overlap the query box. (Out)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <param name="maxResult">The maximum number of polygons to
        /// return.  (Must be less than or equal to the length of
        /// the result array.)</param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqQueryPolygons")]
        public static extern NavmeshStatus GetPolygons(IntPtr query
                , [In] float[] position
                , [In] float[] extents
                , IntPtr filter
                , [In, Out] uint[] resultPolyIds
                , ref int resultCount
                , int maxResult);

        /// <summary>
        /// Finds the navigation polygons within the graph that touch the
        /// specified circle.
        /// </summary>
        /// <remarks>
        /// <p>The order of the result polygons is from least to highest
        /// cost. (A Dikstra search.)</p></remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyId">The id of the polygon to start the
        /// search at.</param>
        /// <param name="position">The center of the query circle.</param>
        /// <param name="radius">The radius of the query circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyIds">The ids of the polygons touched by
        /// the circle.</param>
        /// <param name="resultParentIds">The ids of the parent polygons for 
        /// each result.  (Zero if a result polygon has no parent.) 
        /// (Optional)</param>
        /// <param name="resultCosts">The search cost for each result
        /// polygon. (From the search position to the polygon.) (Optional)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <param name="maxResult">The maximum polygons to return.
        /// (Must be less than or equal to the length of the result array.)
        /// </param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPolysAroundCircle")]
        public static extern NavmeshStatus FindPolygons(IntPtr query
                , uint startPolyId
                , [In] float[] position
                , float radius
                , IntPtr filter
                , [In, Out] uint[] resultPolyIds  // Optional
                , [In, Out] uint[] resultParentIds // Optional
                , [In, Out] float[] resultCosts // Optional
                , ref int resultCount
                , int maxResult);

        /// <summary>
        /// Finds the navigation polygons within the graph that touch the
        /// specified convex polygon.
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyId">The id of the polygon to start the
        /// search at.</param>
        /// <param name="vertices">The convex polygon's vertices in the
        /// form (x, y, z).</param>
        /// <param name="vertexCount">The number of vertices.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyIds">The ids of the polygons touched by
        /// the query polygon.</param>
        /// <param name="resultParentIds">The ids of the parent polygons for 
        /// each result.  (Zero if a result polygon has no parent.) 
        /// (Optional)</param>
        /// <param name="resultCosts">The search cost for each result
        /// polygon. (From the search position to the polygon.) (Optional)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <param name="maxResult">The maximum polygons to return.
        /// (Must be less than or equal to the length of the result array.)
        /// </param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPolysAroundShape")]
        public static extern NavmeshStatus FindPolygons(IntPtr query 
            , uint startPolyId
            , [In] float[] vertices
            , int vertexCount
	        , IntPtr filter
	        , [In, Out] uint[] resultPolyIds
            , [In, Out] uint[] resultParentIds
            , [In, Out] float[] resultCosts
	        , ref int resultCount
            , int maxResult);

        /// <summary>
        /// Finds the non-overlapping navigation polygons in the local
        /// neighborhood around the specified point.
        /// </summary>
        /// <remarks>
        /// This method is optimized for a small query radius and small number
        /// of polygons.
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyId">The id of the polygon to start the
        /// search at.</param>
        /// <param name="position">The center of the query circle.</param>
        /// <param name="radius">The radius of the query circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyIds">The ids of the polygons touched by
        /// the query circle.</param>
        /// <param name="resultParentIds">The ids of the parent polygons for 
        /// each result.  (Zero if a result polygon has no parent.) 
        /// (Optional)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <param name="maxResult">The maximum polygons to return.
        /// (Must be less than or equal to the length of the result array.)
        /// </param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqFindLocalNeighbourhood")]
        public static extern NavmeshStatus GetPolygonsLocal(IntPtr query
            , uint startPolyId
            , [In] float[] position
            , float radius
            , IntPtr filter
            , [In, Out] uint[] resultPolyIds
            , [In, Out] uint[] resultParentIds
            , ref int resultCount
            , int maxResult);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="position"></param>
        /// <param name="height"></param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqGetPolyHeight")]
        public static extern NavmeshStatus GetPolyHeight(IntPtr query
            , uint polyId
            , [In] float[] position
            , ref float height);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="position"></param>
        /// <param name="searchRadius"></param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="hitDistance"></param>
        /// <param name="hitPosition"></param>
        /// <param name="hitNormal"></param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindDistanceToWall")]
        public static extern NavmeshStatus FindDistanceToWall(IntPtr query
            , uint polyId
            , [In] float[] position
            , float searchRadius
	        , IntPtr filter
	        , ref float hitDistance
            , [In, Out] float[] hitPosition
            , [In, Out] float[] hitNormal);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyId"></param>
        /// <param name="endPolyId"></param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath"></param>
        /// <param name="pathCount"></param>
        /// <param name="maxPath"></param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPath")]
        public static extern NavmeshStatus FindPath(IntPtr query 
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
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyId">The id of the polygon.</param>
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
	
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyId"></param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="hitParameter"></param>
        /// <param name="hitNormal"></param>
        /// <param name="path"></param>
        /// <param name="pathCount"></param>
        /// <param name="maxPath"></param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqRaycast")]
        public static extern NavmeshStatus Raycast(IntPtr query
            , uint startPolyId
            , [In] float[] startPosition
            , [In] float[] endPosition
	        , IntPtr filter
	        , ref float hitParameter  // Very high number (> 1E38) if no hit.
            , [In, Out] float[] hitNormal
            , [In, Out] uint[] path
            , ref int pathCount
            , int maxPath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <param name="path"></param>
        /// <param name="pathSize"></param>
        /// <param name="straightPathPoints"></param>
        /// <param name="straightPathFlags"></param>
        /// <param name="straightPathIds"></param>
        /// <param name="straightPathCount"></param>
        /// <param name="maxStraightPath"></param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindStraightPath")]
        public static extern NavmeshStatus GetStraightPath(IntPtr query
            , [In] float[] startPosition
            , [In] float[] endPosition
		    , [In] uint[] path
            , int pathSize
	        , [In, Out] float[] straightPathPoints
            , [In, Out] byte[] straightPathFlags
            , [In, Out] uint[] straightPathIds
	        , ref int straightPathCount
            , int maxStraightPath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyId"></param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPosition"></param>
        /// <param name="visitedPolyIds"></param>
        /// <param name="visitedCount"></param>
        /// <param name="maxVisited"></param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqMoveAlongSurface")]
        public static extern NavmeshStatus MoveAlongSurface(IntPtr query
            , uint startPolyId
            , [In] float[] startPosition
            , [In] float[] endPosition
            , IntPtr filter
            , [In, Out] float[] resultPosition
            , [In, Out] uint[] visitedPolyIds
            , ref int visitedCount
            , int maxVisited);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyId"></param>
        /// <param name="endPolyId"></param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqInitSlicedFindPath")]
        public static extern NavmeshStatus InitSlicedFindPath(IntPtr query
            , uint startPolyId
            , uint endPolyId
            , [In] float[] startPosition
            , [In] float[] endPosition
            , IntPtr filter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="maxIterations"></param>
        /// <param name="actualIterations"></param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqUpdateSlicedFindPath")]
        public static extern NavmeshStatus UpdateSlicedFindPath(IntPtr query
            , int maxIterations
            , ref int actualIterations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="path"></param>
        /// <param name="pathCount"></param>
        /// <param name="maxPath"></param>
        /// <returns>The <see cref="DTStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqFinalizeSlicedFindPath")]
        public static extern NavmeshStatus FinalizeSlicedFindPath(IntPtr query
            , [In, Out] uint[] path
            , ref int pathCount
            , int maxPath);
    }
}
