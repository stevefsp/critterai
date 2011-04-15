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
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Provides pathfinding and related queries for a navigation mesh.
    /// </summary>
    /// <remarks>
    /// <p>Behavior is undefined if an object is used after 
    /// disposal.</p>
    /// </remarks>
    public sealed class NavmeshQuery
        : ManagedObject
    {
        /// <summary>
        /// A pointer to a dtQueryFilter object.
        /// </summary>
        internal IntPtr root;

        private readonly NavmeshQueryLite mLite;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <p>The allocation type may not be Local.</p>
        /// </remarks>
        /// <param name="query">The dtQueryFilter to use.</param>
        /// <param name="type">
        /// The allocation type of the dtQueryFilter (Can't be Local.)
        /// </param>
        internal NavmeshQuery(IntPtr query, AllocType type)
            : base(type)
        {
            if (type == AllocType.Local)
            {
                root = IntPtr.Zero;
                mLite = null;
            }
            else
            {
                root = query;
                mLite = new NavmeshQueryLite(this);
            }
        }

        ~NavmeshQuery()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Immediately frees all unmanaged resources if the object
        /// was created using <see cref="BuildQuery"/>.
        /// </summary>
        public override void RequestDisposal()
        {
            if (ResourceType == AllocType.External)
                NavmeshQueryEx.FreeEx(ref root);
            root = IntPtr.Zero;
        }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Obtains a lite version of the query object.
        /// </summary>
        public NavmeshQueryLite LiteQuery
        {
            get { return mLite; }
        }

        /// <summary>
        /// Returns the impassable segments for the specified polygon.
        /// </summary>
        /// <remarks>
        /// <p>
        /// A portal wall will be returned as impassable if the filter argument
        /// results in the segment's neighbor polygon being marked as excluded.
        /// </p>
        /// <p>The impassable segments of the polygon can be used for simple 2D
        /// collision detection.</p>
        /// </remarks>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="filter">The filter to apply to the query. Used to
        /// determine which polygon neighbors are considered accessible.</param>
        /// <param name="segmentVerts">An array to load the wall vertices into. 
        /// The vertices will be in the form (ax, ay, az, bx, by, bz).
        /// Length: [6 * maxSegments]</param>
        /// <param name="segmentCount">The number of segments returned
        /// in the segments array.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolySegments(uint polyId
            , NavmeshQueryFilter filter
            , float[] resultSegments
            , ref int segmentCount)
        {
            return NavmeshQueryEx.GetPolyWallSegments(root
                , polyId
                , filter.root
                , resultSegments
                , null
                , ref segmentCount
                , resultSegments.Length / 2);
        }

        /// <summary>
        /// Returns the segments for the specified polygon, optionally
        /// excluding portal segments.
        /// </summary>
        /// <remarks>
        /// <p>If the segmentPolyIds parameter is provided, then all polygon
        /// segments will be returned.  If the parameter is ommited, then only 
        /// the impassable segments are returned.</p>
        /// <p>
        /// A portal segment will be returned as impassable if the filter 
        /// argument results in the wall's neighbor polygon being marked as 
        /// excluded.</p>
        /// <p>The impassable segments of the polygon can be used for simple 2D
        /// collision detection.</p>
        /// </remarks>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="filter">The filter to apply to the query. Used to
        /// determine which polygon neighbors are considered accessible.</param>
        /// <param name="segmentVerts">An array to load the segment vertices 
        /// into. 
        /// The vertices will be in the form (ax, ay, az, bx, by, bz).
        /// Length: [6 * maxSegments]</param>
        /// <param name="segmentPolyIds">Ids of the each segment's neighbor
        /// polygon. Or zero if the segment is considered impassible. 
        /// Length: [maxSegements] (Optional)</param>
        /// <param name="segmentCount">The number of segments returned</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolySegments(uint polyId
            , NavmeshQueryFilter filter
            , float[] resultSegments
            , uint[] segmentPolyIds
            , ref int segmentCount)
        {
            return NavmeshQueryEx.GetPolyWallSegments(root
                , polyId
                , filter.root
                , resultSegments
                , segmentPolyIds
                , ref segmentCount
                , resultSegments.Length / 2);
        }

        /// <summary>
        /// Finds the nearest polygon to the specified point.
        /// </summary>
        /// <param name="position">The center of the search location.</param>
        /// <param name="extents">The search distance along each axis in the
        /// form (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="nearestPolyId">The id of the nearest polygon. Or
        /// zero if none could be found within the query box.</param>
        /// <param name="nearestPoint">The nearest point on the polygon
        /// in the form (x, y, z). (Optional Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPoly(float[] position
            , float[] extents
            , NavmeshQueryFilter filter
            , ref uint resultPolyId
            , float[] resultNearestPoint)
        {
            return NavmeshQueryEx.GetNearestPoly(root
                , position
                , extents
                , filter.root
                , ref resultPolyId
                , resultNearestPoint);
        }

        /// <summary>
        /// Finds the polygons that overlap the query box.
        /// </summary>
        /// <param name="position">The center of the query box in the form
        /// (x, y, z).</param>
        /// <param name="extents">The search distance along each axis in the
        /// form (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyIds">The ids for the polygons that
        /// overlap the query box. (Out)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolygons(float[] position
            , float[] extents
            , NavmeshQueryFilter filter
            , uint[] resultPolyIds
            , ref int resultCount)
        {
            return NavmeshQueryEx.GetPolygons(root
                , position
                , extents
                , filter.root
                , resultPolyIds
                , ref resultCount
                , resultPolyIds.Length);
        }

        /// <summary>
        /// Finds the navigation polygons within the graph that touch the
        /// specified circle.
        /// </summary>
        /// <remarks>
        /// <p>The order of the result polygons is from least to highest
        /// cost. (A Dikstra search.)</p>
        /// </remarks>
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
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPolygons(uint startPolyId
                , float[] position
                , float radius
                , NavmeshQueryFilter filter
                , uint[] resultPolyIds   // Optional, must have one.
                , uint[] resultParentIds // Optional
                , float[] resultCosts  // Optional
                , ref int resultCount)
        {
            // Set max count to the smallest length.
            int maxCount = (resultPolyIds == null ? 0 : resultPolyIds.Length);
            maxCount = (resultParentIds == null ? maxCount 
                : Math.Min(maxCount, resultParentIds.Length));
            maxCount = (resultCosts == null ? maxCount
                : Math.Min(maxCount, resultCosts.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.FindPolygons(root
                , startPolyId
                , position
                , radius
                , filter.root
                , resultPolyIds
                , resultParentIds
                , resultCosts
                , ref resultCount
                , maxCount);
        }

        /// <summary>
        /// Finds the navigation polygons within the graph that touch the
        /// specified convex polygon.
        /// </summary>
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
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPolygons(uint startPolyId
                , float[] vertices
                , NavmeshQueryFilter filter
                , uint[] resultPolyIds
                , uint[] resultParentIds
                , float[] resultCosts
                , ref int resultCount)
        {
            // Set max count to the smallest length.
            int maxCount = (resultPolyIds == null ? 0 : resultPolyIds.Length);
            maxCount = (resultParentIds == null ? maxCount
                : Math.Min(maxCount, resultParentIds.Length));
            maxCount = (resultCosts == null ? maxCount
                : Math.Min(maxCount, resultCosts.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.FindPolygons(root
                , startPolyId
                , vertices
                , vertices.Length / 3
                , filter.root
                , resultPolyIds
                , resultParentIds
                , resultCosts
                , ref resultCount
                , maxCount);
        }

        /// <summary>
        /// Finds the non-overlapping navigation polygons in the local
        /// neighborhood around the specified point.
        /// </summary>
        /// <remarks>
        /// This method is optimized for a small query radius and small number
        /// of polygons.
        /// </remarks>
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
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolygonsLocal(uint startPolyId
                , float[] position
                , float radius
                , NavmeshQueryFilter filter
                , uint[] resultPolyIds
                , uint[] resultParentIds
                , ref int resultCount)
        {
            // Set max count to the smallest length.
            int maxCount = (resultPolyIds == null ? 0 : resultPolyIds.Length);
            maxCount = (resultParentIds == null ? maxCount
                : Math.Min(maxCount, resultParentIds.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.GetPolygonsLocal(root
                , startPolyId
                , position
                , radius
                , filter.root
                , resultPolyIds
                , resultParentIds
                , ref resultCount
                , maxCount);
        }

        /// <summary>
        /// Finds the closest point on a navigation polygon.
        /// </summary>
        /// <remarks>
        /// <p>Uses the height detail to provide the most accurate information.
        /// </p></remarks>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="position">The position to search from in the form
        /// (x, y, z).</param>
        /// <param name="resultPoint">The closest point found in the form 
        /// (x, y, z) (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPoint(uint polyId
            , float[] position
            , float[] resultPoint)
        {
            return NavmeshQueryEx.GetNearestPoint(root
                , polyId
                , position
                , resultPoint);
        }

        /// <summary>
        /// Finds the closest point on the navigation polygon's xz-plane 
        /// boundary.
        /// </summary>
        /// <remarks>
        /// <p>Snaps the point to the polygon boundary if it is outside the
        /// polygon's xz-column.</p>
        /// <p>Does not change the y-value of the point.</p>
        /// <p>Much faster than <see cref="GetNearestPoint" />.</p>
        /// </remarks>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="position">The point to check in the form (x, y, z).
        /// </param>
        /// <param name="resultPoint">The closest point. (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestBoundaryPoint(uint polyId
            , float[] position
            , float[] resultPoint)
        {
            return NavmeshQueryEx.GetNearestBoundaryPoint(root
                , polyId
                , position
                , resultPoint);
        }

        /// <summary>
        /// Finds the polygons that overlap the query box.
        /// </summary>
        /// <param name="position">The center of the query box in the form
        /// (x, y, z).</param>
        /// <param name="extents">The search distance along each axis in the
        /// form (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyIds">The ids for the polygons that
        /// overlap the query box. (Out)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolyHeight(uint polyId
            , float[] position
            , ref float height)
        {
            return NavmeshQueryEx.GetPolyHeight(root
                , polyId
                , position
                , ref height);
        }

        /// <summary>
        /// Returns the distance from the specified position to the nearest
        /// polygon wall.
        /// </summary>
        /// <remarks>TODO: Confirm that wall refers to solid segment.</remarks>
        /// <param name="polyId">The id of the polygon.</param>
        /// <param name="position">The center of the search query circle.
        /// </param>
        /// <param name="searchRadius">The radius of the query circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="distance">Distance to nearest wall.</param>
        /// <param name="closestPoint">The nearest point on the wall.</param>
        /// <param name="normal">The normal of the ray from the 
        /// query position through the closest point on the wall.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindDistanceToWall(uint polyId
            , float[] position
            , float searchRadius
            , NavmeshQueryFilter filter
            , ref float distance
            , float[] closestPoint
            , float[] normal)
        {
            return NavmeshQueryEx.FindDistanceToWall(root
                , polyId
                , position
                , searchRadius
                , filter.root
                , ref distance
                , closestPoint
                , normal);
        }

        /// <summary>
        /// Finds the polygon path from the start to the end polygon.
        /// </summary>
        /// <remarks>
        /// <p>If the end polygon cannot be reached, then the last polygon
        /// is the nearest to the end polygon.</p>
        /// <p>The start and end positions are used to properly calculate
        /// traversal costs.</p>
        /// </remarks>
        /// <param name="startPolyId">The id of the start polygon.</param>
        /// <param name="endPolyId">The id of the end polygon.</param>
        /// <param name="startPosition">A position within the start polygon.
        /// </param>
        /// <param name="endPosition">A position within the end polygon.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath">An ordered list of polygon ids in the
        /// path. (Start to end.)</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <param name="maxPath">The maximum length of the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPath(uint startPolyId
            , uint endPolyId
            , float[] startPosition
            , float[] endPosition
            , NavmeshQueryFilter filter
            , uint[] resultPath
            , ref int pathCount)
        {
            return NavmeshQueryEx.FindPath(root
                , startPolyId
                , endPolyId
                , startPosition
                , endPosition
                , filter.root
                , resultPath
                , ref pathCount
                , resultPath.Length);
        }

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
        /// <param name="polyId">The id of the polygon.</param>
        /// <returns>TRUE if the polgyon is in the current closed list.
        /// </returns>
        public bool IsInClosedList(uint polyId)
        {
            return NavmeshQueryEx.IsInClosedList(root, polyId);
        }

        /// <summary>
        /// Casts a 'walkability' ray along the surface of the navigation mesh
        /// from the start position toward the end position.
        /// </summary>
        /// <remarks>
        /// TODO: DOC: Add more information on the hit parameter.
        /// </remarks>
        /// <param name="startPolyId">The id of the start polygon.</param>
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
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus Raycast(uint startPolyId
            , float[] startPosition
            , float[] endPosition
            , NavmeshQueryFilter filter
            , ref float hitParameter  // Very high number (> 1E38) if no hit.
            , float[] hitNormal
            , uint[] path
            , ref int pathCount)
        {
            int maxCount = (path == null ? 0 : path.Length);

            return NavmeshQueryEx.Raycast(root
                , startPolyId
                , startPosition
                , endPosition
                , filter.root
                , ref hitParameter
                , hitNormal
                , path
                , ref pathCount
                , maxCount);
        }

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
        /// <param name="straightPathIds">The id of the polygon that
        /// is being entered at the point position.</param>
        /// <param name="straightPathCount">The number of points in the
        /// straight path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetStraightPath(float[] startPosition
            , float[] endPosition
            , uint[] path
            , int pathSize
            , float[] straightPathPoints
            , WaypointFlag[] straightPathFlags
            , uint[] straightPathIds
            , ref int straightPathCount)
        {
            int maxPath = straightPathPoints.Length / 3;
            maxPath = (straightPathFlags == null ? maxPath
                : Math.Min(straightPathFlags.Length, maxPath));
            maxPath = (straightPathIds == null ? maxPath
                : Math.Min(straightPathIds.Length, maxPath));

            if (maxPath < 1)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.GetStraightPath(root
                , startPosition
                , endPosition
                , path
                , pathSize
                , straightPathPoints
                , straightPathFlags
                , straightPathIds
                , ref straightPathCount
                , maxPath);
        }

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
        /// <param name="startPolyId">The id of the start polygon.</param>
        /// <param name="startPosition">A position within the start
        /// polygon.</param>
        /// <param name="endPosition">The end position.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPosition">The result of the move in the
        /// form (x, y, z).</param>
        /// <param name="visitedPolyIds">The ids of the polygons
        /// visited during the move.</param>
        /// <param name="visitedCount">The number of polygons visited during
        /// the move.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus MoveAlongSurface(uint startPolyId
            , float[] startPosition
            , float[] endPosition
            , NavmeshQueryFilter filter
            , float[] resultPosition
            , uint[] visitedPolyIds
            , ref int visitedCount)
        {
            return NavmeshQueryEx.MoveAlongSurface(root
                , startPolyId
                , startPosition
                , endPosition
                , filter.root
                , resultPosition
                , visitedPolyIds
                , ref visitedCount
                , visitedPolyIds.Length);
        }

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
        /// <param name="startPolyId">The id of the start polygon.</param>
        /// <param name="endPolyId">The id of the end polygon.</param>
        /// <param name="startPosition">A position within the start polygon.
        /// </param>
        /// <param name="endPosition">A position within the end polygon.
        /// </param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus InitSlicedFindPath(uint startPolyId
            , uint endPolyId
            , float[] startPosition
            , float[] endPosition
            , NavmeshQueryFilter filter)
        {
            return NavmeshQueryEx.InitSlicedFindPath(root
                , startPolyId
                , endPolyId
                , startPosition
                , endPosition
                , filter.root);
        }

        /// <summary>
        /// Continues a sliced path find query.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations
        /// to perform.</param>
        /// <param name="actualIterations">The actual number of iterations
        /// performed.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus UpdateSlicedFindPath(int maxIterations
            , ref int actualIterations)
        {
            return NavmeshQueryEx.UpdateSlicedFindPath(root
                , maxIterations
                , ref actualIterations);
        }

        /// <summary>
        /// Finalizes and returns the results of the sliced path query.
        /// </summary>
        /// <param name="path">An ordered list of polygons representing the
        /// path.</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FinalizeSlicedFindPath(uint[] path
            , ref int pathCount)
        {
            return NavmeshQueryEx.FinalizeSlicedFindPath(root
                , path
                , ref pathCount
                , path.Length);
        }

        /// <summary>
        /// Creates a navigation mesh query for a navigation mesh.
        /// </summary>
        /// <param name="navmesh">A navigation mesh to query against.</param>
        /// <param name="maximumNodes">The maximum number of nodes
        /// allowed when performing searches.</param>
        /// <param name="resultQuery">A navigation mesh query object.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the build
        /// request.
        /// </returns>
        public static NavStatus BuildQuery(Navmesh navmesh
            , int maximumNodes
            , Object owner
            , out NavmeshQuery resultQuery)
        {
            IntPtr query = IntPtr.Zero;

            NavStatus status = NavmeshQueryEx.BuildNavmeshQuery(
                navmesh.root
                , maximumNodes
                , ref query);

            if (NavUtil.Succeeded(status))
                resultQuery = new NavmeshQuery(query
                    , AllocType.External);
            else
                resultQuery = null;

            return status;
        }
    }
}
