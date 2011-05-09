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

        /// <summary>
        /// Creates a navigation mesh query for a navigation mesh.
        /// </summary>
        /// <remarks>
        /// <p>Any dtNavMesh query created by this method must be freed
        /// using <see cref="FreeEx"/></p>
        /// </remarks>
        /// <param name="dtNavMesh">A pointer to a fully initialized
        /// dtNavMesh object.</param>
        /// <param name="maxNodes">The maximum number of nodes
        /// allowed when performing searches.</param>
        /// <param name="dtQuery">A pointer to a dtNavMeshQuery 
        /// object.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the build
        /// request.
        /// </returns>
        [DllImport("cai-nav-rcn", EntryPoint = "rcnBuildDTNavQuery")]
        public static extern NavStatus BuildNavmeshQuery(IntPtr navmesh
            , int maxNodes
            , ref IntPtr resultQuery);

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
        /// Returns the segments for the specified polygon.
        /// </summary>
        /// <remarks>
        /// <p>If the segmentPolyRefs parameter is provided, then all polygon
        /// segments will be returned.  If the parameter is ommited, then only 
        /// the impassable walls are returned.</p>
        /// <p>
        /// A portal wall will be returned as impassable if the filter argument
        /// results in the wall's neighbor polygon being marked as excluded.</p>
        /// <p>The wall segments of the polygon can be used for simple 2D
        /// collision detection.</p>
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="filter">The filter to apply to the query. Used to
        /// determine which polygon neighbors are considered accessible.</param>
        /// <param name="segmentVerts">An array to load the wall vertices into. 
        /// The vertices will be in the form (ax, ay, az, bx, by, bz).
        /// Length: [6 * maxSegments]</param>
        /// <param name="segmentPolyRefs">Ids of the each segment's neighbor
        /// polygon. Or zero if the segment is considered impassible. 
        /// Length: [maxSegements] (Optional)</param>
        /// <param name="segmentCount">The number of wall segments returned
        /// in the segments array.</param>
        /// <param name="maxSegments">The maximum number of segments that
        /// can be returned.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqGetPolyWallSegments")]
        public static extern NavStatus GetPolyWallSegments(IntPtr query
            , uint polyRef
            , IntPtr filter
            , [In, Out] float[] segmentVerts
            , [In, Out] uint[] segmentPolyRefs
            , ref int segmentCount
            , int maxSegments);

        /// <summary>
        /// Finds the nearest polygon to the specified point.
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="position">The center of the search location.</param>
        /// <param name="extents">The search distance along each axis in the
        /// form (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="nearestPolyRef">The id of the nearest polygon. Or
        /// zero if none could be found within the query box. (Out)</param>
        /// <param name="nearestPoint">The nearest point on the polygon
        /// in the form (x, y, z). (Optional Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindNearestPoly")]
        public static extern NavStatus GetNearestPoly(IntPtr query
            , [In] float[] position
            , [In] float[] extents
		    , IntPtr filter
		    , ref uint nearestPolyRef
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
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="position">The position to search from in the form
        /// (x, y, z).</param>
        /// <param name="resultPoint">The closest point found in the form 
        /// (x, y, z) (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqClosestPointOnPoly")]
        public static extern NavStatus GetNearestPoint(IntPtr query
            , uint polyRef
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
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="position">The point to check in the form (x, y, z).
        /// </param>
        /// <param name="resultPoint">The closest point. (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqClosestPointOnPolyBoundary")]
        public static extern NavStatus GetNearestBoundaryPoint(IntPtr query 
            , uint polyRef
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
        /// <param name="resultPolyRefs">The ids for the polygons that
        /// overlap the query box. (Out)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <param name="maxResult">The maximum number of polygons to
        /// return.  (Must be less than or equal to the length of
        /// the result array.)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqQueryPolygons")]
        public static extern NavStatus GetPolygons(IntPtr query
                , [In] float[] position
                , [In] float[] extents
                , IntPtr filter
                , [In, Out] uint[] resultPolyRefs
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
        /// <param name="startPolyRef">The id of the polygon to start the
        /// search at.</param>
        /// <param name="position">The center of the query circle.</param>
        /// <param name="radius">The radius of the query circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">The ids of the polygons touched by
        /// the circle.</param>
        /// <param name="resultParentRefs">The ids of the parent polygons for 
        /// each result.  (Zero if a result polygon has no parent.) 
        /// (Optional)</param>
        /// <param name="resultCosts">The search cost for each result
        /// polygon. (From the search position to the polygon.) (Optional)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <param name="maxResult">The maximum polygons to return.
        /// (Must be less than or equal to the length of the result array.)
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPolysAroundCircle")]
        public static extern NavStatus FindPolygons(IntPtr query
                , uint startPolyRef
                , [In] float[] position
                , float radius
                , IntPtr filter
                , [In, Out] uint[] resultPolyRefs  // Optional
                , [In, Out] uint[] resultParentRefs // Optional
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
        /// <param name="startPolyRef">The id of the polygon to start the
        /// search at.</param>
        /// <param name="vertices">The convex polygon's vertices in the
        /// form (x, y, z).</param>
        /// <param name="vertexCount">The number of vertices.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">The ids of the polygons touched by
        /// the query polygon.</param>
        /// <param name="resultParentRefs">The ids of the parent polygons for 
        /// each result.  (Zero if a result polygon has no parent.) 
        /// (Optional)</param>
        /// <param name="resultCosts">The search cost for each result
        /// polygon. (From the search position to the polygon.) (Optional)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <param name="maxResult">The maximum polygons to return.
        /// (Must be less than or equal to the length of the result array.)
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindPolysAroundShape")]
        public static extern NavStatus FindPolygons(IntPtr query 
            , uint startPolyRef
            , [In] float[] verts
            , int vertCount
	        , IntPtr filter
	        , [In, Out] uint[] resultPolyRefs
            , [In, Out] uint[] resultParentRefs
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
        /// <param name="startPolyRef">The id of the polygon to start the
        /// search at.</param>
        /// <param name="position">The center of the query circle.</param>
        /// <param name="radius">The radius of the query circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">The ids of the polygons touched by
        /// the query circle.</param>
        /// <param name="resultParentRefs">The ids of the parent polygons for 
        /// each result.  (Zero if a result polygon has no parent.) 
        /// (Optional)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <param name="maxResult">The maximum polygons to return.
        /// (Must be less than or equal to the length of the result array.)
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqFindLocalNeighbourhood")]
        public static extern NavStatus GetPolygonsLocal(IntPtr query
            , uint startPolyRef
            , [In] float[] position
            , float radius
            , IntPtr filter
            , [In, Out] uint[] resultPolyRefs
            , [In, Out] uint[] resultParentRefs
            , ref int resultCount
            , int maxResult);

        /// <summary>
        /// Returns the height of the polygon at the specified point.
        /// </summary>
        /// <remarks>
        /// <p>This method uses the detail mesh when it is available.</p>
        /// TODO: Need to determine if this even works without a detail mesh.
        /// <p>TODO: Add doc on how off mesh connections are handled.</p>
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="position">The position within the column of the
        /// polygon.  (Within the xz-bounds.)</param>
        /// <param name="height">The heigth (y-value) on the surface of
        /// the polygon.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqGetPolyHeight")]
        public static extern NavStatus GetPolyHeight(IntPtr query
            , uint polyRef
            , [In] float[] position
            , ref float height);

        /// <summary>
        /// Returns the distance from the specified position to the nearest
        /// polygon wall.
        /// </summary>
        /// <remarks>TODO: Confirm that wall refers to solid segment.</remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="position">The center of the search query circle.
        /// </param>
        /// <param name="searchRadius">The radius of the query circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="distance">Distance to nearest wall.</param>
        /// <param name="closestPoint">The nearest point on the wall.</param>
        /// <param name="normal">The normal of the ray from the 
        /// query position through the closest point on the wall.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindDistanceToWall")]
        public static extern NavStatus FindDistanceToWall(IntPtr query
            , uint polyRef
            , [In] float[] position
            , float searchRadius
	        , IntPtr filter
	        , ref float distance
            , [In, Out] float[] closestPoint
            , [In, Out] float[] normal);

        /// <summary>
        /// Finds the polygon path from the start to the end polygon.
        /// </summary>
        /// <remarks>
        /// <p>If the end polygon cannot be reached, then the last polygon
        /// is the nearest to the end polygon.</p>
        /// <p>The start and end positions are used to properly calculate
        /// traversal costs.</p>
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyRef">The id of the start polygon.</param>
        /// <param name="endPolyRef">The id of the end polygon.</param>
        /// <param name="startPosition">A position within the start polygon.
        /// </param>
        /// <param name="endPosition">A position within the end polygon.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath">An ordered list of polygon ids in the
        /// path. (Start to end.)</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <param name="maxPath">The maximum length of the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
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

        /// <summary>
        /// Returns TRUE if the polygon is in the current closed list.
        /// </summary>
        /// <remarks>
        /// <p>The closed list is the list of polygons that were fully evaluated
        /// during a find operation.</p>
        /// <p>All methods prefixed with "Find" and all sliced path methods are
        /// operations that can generate a closed list.  The content of the 
        /// list will persist until the next find operation is performed.</p>
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <returns>TRUE if the polgyon is in the current closed list.
        /// </returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqIsInClosedList")]
        public static extern bool IsInClosedList(IntPtr query
            , uint polyRef);
	
        /// <summary>
        /// Casts a 'walkability' ray along the surface of the navigation mesh
        /// from the start position toward the end position.
        /// </summary>
        /// <remarks>
        /// TODO: DOC: Add more information on the hit parameter.
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyRef">The id of the start polygon.</param>
        /// <param name="startPosition">A position within the start polygon
        /// representing the start of the ray.</param>
        /// <param name="endPosition">The position to cast the ray toward.
        /// </param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="hitParameter">The hit parameter.  (Will be > 1E38
        /// if there was no hit.)</param>
        /// <param name="hitNormal">The normal of the nearest hit.</param>
        /// <param name="path">The ids of the visited polygons. (Optional)
        /// </param>
        /// <param name="pathCount">The number of visited polygons.</param>
        /// <param name="maxPath">The length of the path array.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
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

        /// <summary>
        /// Returns the staight path from the start to the end locations
        /// within the polygon corridor.
        /// </summary>
        /// <remarks>
        /// <p>Start and end positions will be clamped to the corridor.</p>
        /// <p>The returned polygon ids represent the id of the polygon
        /// that is entered at the associated path point.  The id associated
        /// to end point will always be zero.</p>
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="endPosition">The end position.</param>
        /// <param name="path">The list of polygon ids that represent the
        /// path corridor.</param>
        /// <param name="pathSize">The length of the path within the path
        /// array.</param>
        /// <param name="straightPathPoints">Points describing the straight
        /// path in the form (x, y, z).</param>
        /// <param name="straightPathFlags">Flags describing each point.
        /// (Optional)</param>
        /// <param name="straightPathRefs">The id of the polygon that
        /// is being entered at the point position.</param>
        /// <param name="straightPathCount">The number of points in the
        /// straight path.</param>
        /// <param name="maxStraightPath">The maximum length of the straight
        /// path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
	    [DllImport("cai-nav-rcn", EntryPoint = "dtqFindStraightPath")]
        public static extern NavStatus GetStraightPath(IntPtr query
            , [In] float[] startPosition
            , [In] float[] endPosition
		    , [In] uint[] path
            , int pathSize
	        , [In, Out] float[] straightPathPoints
            , [In, Out] WaypointFlag[] straightPathFlags
            , [In, Out] uint[] straightPathRefs
	        , ref int straightPathCount
            , int maxStraightPath);

        /// <summary>
        /// Moves from the start position to the end position constrained to
        /// the navigation mesh.
        /// </summary>
        /// <remarks>
        /// <p>The result position will equal the end position if the end position
        /// is reachable.</p>
        /// <p>This method is optimized for small delta movement and a small
        /// number of polygons.</p>
        /// <p>The result position is not projected to the surface of the
        /// navigation mesh.  If that is needed, use 
        /// <see cref="GetPolyHeight"/>.</p>
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyRef">The id of the start polygon.</param>
        /// <param name="startPosition">A position within the start
        /// polygon.</param>
        /// <param name="endPosition">The end position.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPosition">The result of the move in the
        /// form (x, y, z).</param>
        /// <param name="visitedPolyRefs">The ids of the polygons
        /// visited during the move.</param>
        /// <param name="visitedCount">The number of polygons visited during
        /// the move.</param>
        /// <param name="maxVisited">The length of the visitedPolyRefs array.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
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

        /// <summary>
        /// Initializes a sliced path find query.
        /// </summary>
        /// <remarks>
        /// <p>WARNING: Calling any other query methods besides the other
        /// sliced path methods before finalizing this query may result
        /// in corrupted data.</p>
        /// <p>The filter is stored and used for the duration of the query.</p>
        /// <p>The standard use case:</p>
        /// <ol>
        /// <li>Initialize the sliced path query</li>
        /// <li>Call <see cref="UpdateSlicedFindPath"/> until its status
        /// returns complete.</li>
        /// <li>Call <see cref="FinalizeSlicedFindPath"/> to get the path.</li>
        /// </ol>
        /// </remarks>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="startPolyRef">The id of the start polygon.</param>
        /// <param name="endPolyRef">The id of the end polygon.</param>
        /// <param name="startPosition">A position within the start polygon.
        /// </param>
        /// <param name="endPosition">A position within the end polygon.
        /// </param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqInitSlicedFindPath")]
        public static extern NavStatus InitSlicedFindPath(IntPtr query
            , uint startPolyRef
            , uint endPolyRef
            , [In] float[] startPosition
            , [In] float[] endPosition
            , IntPtr filter);

        /// <summary>
        /// Continues a sliced path find query.
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="maxIterations">The maximum number of iterations
        /// to perform.</param>
        /// <param name="actualIterations">The actual number of iterations
        /// performed.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqUpdateSlicedFindPath")]
        public static extern NavStatus UpdateSlicedFindPath(IntPtr query
            , int maxIterations
            , ref int actualIterations);

        /// <summary>
        /// Finalizes and returns the results of the sliced path query.
        /// </summary>
        /// <param name="query">A pointer to an initialized Detour navigation 
        /// mesh query.
        /// </param>
        /// <param name="path">An ordered list of polygons representing the
        /// path.</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <param name="maxPath">The maximum allowed path length.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqFinalizeSlicedFindPath")]
        public static extern NavStatus FinalizeSlicedFindPath(IntPtr query
            , [In, Out] uint[] path
            , ref int pathCount
            , int maxPath);
    }
}
