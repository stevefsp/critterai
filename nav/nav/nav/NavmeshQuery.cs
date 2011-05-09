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
using org.critterai.nav.rcn;
using org.critterai.interop;

namespace org.critterai.nav
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
        private bool mIsRestricted;

        /// <summary>
        /// If TRUE, certain methods are disabled.
        /// </summary>
        /// <remarks>
        /// Certain methods are generally not safe for use by multiple
        /// clients.  These methods will fail if the object is marked as 
        /// restricted.
        /// </remarks>
        public bool IsRestricted { get { return mIsRestricted; } }

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
        internal NavmeshQuery(IntPtr query, bool isConstant, AllocType type)
            : base(type)
        {
            mIsRestricted = isConstant;
            if (type == AllocType.Local)
            {
                root = IntPtr.Zero;
                // mLite = null;
            }
            else
            {
                root = query;
                // mLite = new NavmeshQueryLite(this);
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
        /// <remarks>
        /// This method is not protected by the <see cref="IsRestricted"/>
        /// block.</remarks>
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
        /// Finds the nearest polygon to the specified point.
        /// </summary>
        /// <param name="position">The center of the search location.</param>
        /// <param name="extents">The search distance along each axis in the
        /// form (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="nearestPolyRef">The id of the nearest polygon. Or
        /// zero if none could be found within the query box.</param>
        /// <param name="nearestPoint">The nearest point on the polygon
        /// in the form (x, y, z). (Optional Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPoly(float[] position
            , float[] extents
            , NavmeshQueryFilter filter
            , out uint resultPolyRef
            , float[] resultNearestPoint)
        {
            resultPolyRef = 0;
            return NavmeshQueryEx.GetNearestPoly(root
                , position
                , extents
                , filter.root
                , ref resultPolyRef
                , resultNearestPoint);
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
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="filter">The filter to apply to the query. Used to
        /// determine which polygon neighbors are considered accessible.</param>
        /// <param name="segmentVerts">An array to load the wall vertices into. 
        /// The vertices will be in the form (ax, ay, az, bx, by, bz).
        /// Length: [6 * maxSegments]</param>
        /// <param name="segmentCount">The number of segments returned
        /// in the segments array.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolySegments(uint polyRef
            , NavmeshQueryFilter filter
            , float[] resultSegments
            , out int segmentCount)
        {
            segmentCount = 0;
            return NavmeshQueryEx.GetPolyWallSegments(root
                , polyRef
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
        /// <p>If the segmentPolyRefs parameter is provided, then all polygon
        /// segments will be returned.  If the parameter is ommited, then only 
        /// the impassable segments are returned.</p>
        /// <p>
        /// A portal segment will be returned as impassable if the filter 
        /// argument results in the wall's neighbor polygon being marked as 
        /// excluded.</p>
        /// <p>The impassable segments of the polygon can be used for simple 2D
        /// collision detection.</p>
        /// </remarks>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="filter">The filter to apply to the query. Used to
        /// determine which polygon neighbors are considered accessible.</param>
        /// <param name="segmentVerts">An array to load the segment vertices 
        /// into. 
        /// The vertices will be in the form (ax, ay, az, bx, by, bz).
        /// Length: [6 * maxSegments]</param>
        /// <param name="segmentPolyRefs">Ids of the each segment's neighbor
        /// polygon. Or zero if the segment is considered impassible. 
        /// Length: [maxSegements] (Optional)</param>
        /// <param name="segmentCount">The number of segments returned</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolySegments(uint polyRef
            , NavmeshQueryFilter filter
            , float[] resultSegments
            , uint[] segmentPolyRefs
            , out int segmentCount)
        {
            segmentCount = 0;
            return NavmeshQueryEx.GetPolyWallSegments(root
                , polyRef
                , filter.root
                , resultSegments
                , segmentPolyRefs
                , ref segmentCount
                , resultSegments.Length / 2);
        }

        /// <summary>
        /// Finds the polygons that overlap the query box.
        /// </summary>
        /// <param name="position">The center of the query box in the form
        /// (x, y, z).</param>
        /// <param name="extents">The search distance along each axis in the
        /// form (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">The ids for the polygons that
        /// overlap the query box. (Out)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolygons(float[] position
            , float[] extents
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs
            , out int resultCount)
        {
            resultCount = 0;
            return NavmeshQueryEx.GetPolygons(root
                , position
                , extents
                , filter.root
                , resultPolyRefs
                , ref resultCount
                , resultPolyRefs.Length);
        }

        /// <summary>
        /// Finds the navigation polygons within the graph that touch the
        /// specified circle.
        /// </summary>
        /// <remarks>
        /// <p>The order of the result polygons is from least to highest
        /// cost. (A Dikstra search.)</p>
        /// </remarks>
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
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPolygons(uint startPolyRef
                , float[] position
                , float radius
                , NavmeshQueryFilter filter
                , uint[] resultPolyRefs   // Optional, must have one.
                , uint[] resultParentRefs // Optional
                , float[] resultCosts  // Optional
                , out int resultCount)
        {
            resultCount = 0;

            // Set max count to the smallest length.
            int maxCount = (resultPolyRefs == null ? 0 : resultPolyRefs.Length);
            maxCount = (resultParentRefs == null ? maxCount 
                : Math.Min(maxCount, resultParentRefs.Length));
            maxCount = (resultCosts == null ? maxCount
                : Math.Min(maxCount, resultCosts.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.FindPolygons(root
                , startPolyRef
                , position
                , radius
                , filter.root
                , resultPolyRefs
                , resultParentRefs
                , resultCosts
                , ref resultCount
                , maxCount);
        }

        /// <summary>
        /// Finds the navigation polygons within the graph that touch the
        /// specified convex polygon.
        /// </summary>
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
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPolygons(uint startPolyRef
                , float[] vertices
                , NavmeshQueryFilter filter
                , uint[] resultPolyRefs
                , uint[] resultParentRefs
                , float[] resultCosts
                , out int resultCount)
        {
            resultCount = 0;

            // Set max count to the smallest length.
            int maxCount = (resultPolyRefs == null ? 0 : resultPolyRefs.Length);
            maxCount = (resultParentRefs == null ? maxCount
                : Math.Min(maxCount, resultParentRefs.Length));
            maxCount = (resultCosts == null ? maxCount
                : Math.Min(maxCount, resultCosts.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.FindPolygons(root
                , startPolyRef
                , vertices
                , vertices.Length / 3
                , filter.root
                , resultPolyRefs
                , resultParentRefs
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
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolygonsLocal(uint startPolyRef
                , float[] position
                , float radius
                , NavmeshQueryFilter filter
                , uint[] resultPolyRefs
                , uint[] resultParentRefs
                , out int resultCount)
        {
            resultCount = 0;

            // Set max count to the smallest length.
            int maxCount = (resultPolyRefs == null ? 0 : resultPolyRefs.Length);
            maxCount = (resultParentRefs == null ? maxCount
                : Math.Min(maxCount, resultParentRefs.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.GetPolygonsLocal(root
                , startPolyRef
                , position
                , radius
                , filter.root
                , resultPolyRefs
                , resultParentRefs
                , ref resultCount
                , maxCount);
        }

        /// <summary>
        /// Finds the closest point on a navigation polygon.
        /// </summary>
        /// <remarks>
        /// <p>Uses the height detail to provide the most accurate information.
        /// </p></remarks>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="position">The position to search from in the form
        /// (x, y, z).</param>
        /// <param name="resultPoint">The closest point found in the form 
        /// (x, y, z) (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPoint(uint polyRef
            , float[] position
            , float[] resultPoint)
        {
            return NavmeshQueryEx.GetNearestPoint(root
                , polyRef
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
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="position">The point to check in the form (x, y, z).
        /// </param>
        /// <param name="resultPoint">The closest point. (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestBoundaryPoint(uint polyRef
            , float[] position
            , float[] resultPoint)
        {
            return NavmeshQueryEx.GetNearestBoundaryPoint(root
                , polyRef
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
        /// <param name="resultPolyRefs">The ids for the polygons that
        /// overlap the query box. (Out)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolyHeight(uint polyRef
            , float[] position
            , out float height)
        {
            height = 0;
            return NavmeshQueryEx.GetPolyHeight(root
                , polyRef
                , position
                , ref height);
        }

        /// <summary>
        /// Returns the distance from the specified position to the nearest
        /// polygon wall.
        /// </summary>
        /// <remarks>TODO: Confirm that wall refers to solid segment.</remarks>
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
        public NavStatus FindDistanceToWall(uint polyRef
            , float[] position
            , float searchRadius
            , NavmeshQueryFilter filter
            , out float distance
            , float[] closestPoint
            , float[] normal)
        {
            distance = 0;
            return NavmeshQueryEx.FindDistanceToWall(root
                , polyRef
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
        public NavStatus FindPath(uint startPolyRef
            , uint endPolyRef
            , float[] startPosition
            , float[] endPosition
            , NavmeshQueryFilter filter
            , uint[] resultPath
            , out int pathCount)
        {
            pathCount = 0;
            return NavmeshQueryEx.FindPath(root
                , startPolyRef
                , endPolyRef
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
        /// <param name="polyRef">The id of the polygon.</param>
        /// <returns>TRUE if the polgyon is in the current closed list.
        /// </returns>
        public bool IsInClosedList(uint polyRef)
        {
            return NavmeshQueryEx.IsInClosedList(root, polyRef);
        }

        /// <summary>
        /// Casts a 'walkability' ray along the surface of the navigation mesh
        /// from the start position toward the end position.
        /// </summary>
        /// <remarks>
        /// TODO: DOC: Add more information on the hit parameter.
        /// </remarks>
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
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus Raycast(uint startPolyRef
            , float[] startPosition
            , float[] endPosition
            , NavmeshQueryFilter filter
            , out float hitParameter  // Very high number (> 1E38) if no hit.
            , float[] hitNormal
            , uint[] path
            , out int pathCount)
        {
            pathCount = 0;
            hitParameter = 0;

            int maxCount = (path == null ? 0 : path.Length);

            return NavmeshQueryEx.Raycast(root
                , startPolyRef
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
        /// <param name="straightPathRefs">The id of the polygon that
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
            , uint[] straightPathRefs
            , out int straightPathCount)
        {
            straightPathCount = 0;

            int maxPath = straightPathPoints.Length / 3;
            maxPath = (straightPathFlags == null ? maxPath
                : Math.Min(straightPathFlags.Length, maxPath));
            maxPath = (straightPathRefs == null ? maxPath
                : Math.Min(straightPathRefs.Length, maxPath));

            if (maxPath < 1)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.GetStraightPath(root
                , startPosition
                , endPosition
                , path
                , pathSize
                , straightPathPoints
                , straightPathFlags
                , straightPathRefs
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
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus MoveAlongSurface(uint startPolyRef
            , float[] startPosition
            , float[] endPosition
            , NavmeshQueryFilter filter
            , float[] resultPosition
            , uint[] visitedPolyRefs
            , out int visitedCount)
        {
            visitedCount = 0;
            return NavmeshQueryEx.MoveAlongSurface(root
                , startPolyRef
                , startPosition
                , endPosition
                , filter.root
                , resultPosition
                , visitedPolyRefs
                , ref visitedCount
                , visitedPolyRefs.Length);
        }

        /// <summary>
        /// Initializes a sliced path find query.
        /// </summary>
        /// <remarks>
        /// <p>This method will fail if <see cref="IsRestricted"/> is TRUE.</p>
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
        /// <param name="startPolyRef">The id of the start polygon.</param>
        /// <param name="endPolyRef">The id of the end polygon.</param>
        /// <param name="startPosition">A position within the start polygon.
        /// </param>
        /// <param name="endPosition">A position within the end polygon.
        /// </param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus InitSlicedFindPath(uint startPolyRef
            , uint endPolyRef
            , float[] startPosition
            , float[] endPosition
            , NavmeshQueryFilter filter)
        {
            if (mIsRestricted)
                return NavStatus.Failure;
            return NavmeshQueryEx.InitSlicedFindPath(root
                , startPolyRef
                , endPolyRef
                , startPosition
                , endPosition
                , filter.root);
        }

        /// <summary>
        /// Continues a sliced path find query.
        /// </summary>
        /// <remarks>
        /// <p><p>This method will fail if <see cref="IsRestricted"/> is TRUE.
        /// </p></remarks>
        /// <param name="maxIterations">The maximum number of iterations
        /// to perform.</param>
        /// <param name="actualIterations">The actual number of iterations
        /// performed.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus UpdateSlicedFindPath(int maxIterations
            , out int actualIterations)
        {
            actualIterations = 0;
            if (mIsRestricted)
                return NavStatus.Failure;
            return NavmeshQueryEx.UpdateSlicedFindPath(root
                , maxIterations
                , ref actualIterations);
        }

        /// <summary>
        /// Finalizes and returns the results of the sliced path query.
        /// </summary>
        /// <remarks>
        /// <p>This method will fail if <see cref="IsRestricted"/> is TRUE.</p>
        /// </remarks>
        /// <param name="path">An ordered list of polygons representing the
        /// path.</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FinalizeSlicedFindPath(uint[] path
            , out int pathCount)
        {
            pathCount = 0;
            if (mIsRestricted)
                return NavStatus.Failure;
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
        public static NavStatus Build(Navmesh navmesh
            , int maximumNodes
            , out NavmeshQuery resultQuery)
        {
            IntPtr query = IntPtr.Zero;

            NavStatus status = NavmeshQueryEx.BuildNavmeshQuery(
                navmesh.root
                , maximumNodes
                , ref query);

            if (NavUtil.Succeeded(status))
                resultQuery = new NavmeshQuery(query
                    , false
                    , AllocType.External);
            else
                resultQuery = null;

            return status;
        }
    }
}
