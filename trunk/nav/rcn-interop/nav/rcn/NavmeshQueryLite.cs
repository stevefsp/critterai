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

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Provides a navigation mesh query that is safer to share between
    /// multiple clients.
    /// </summary>
    /// <remarks>
    /// <p>Basically, this class only exposes query methods that complete 
    /// with each use. (No asynchronous methods.)</p>
    /// <p>References to this object should not be shared if the sliced path 
    /// feature of the root navigation query is being used.</p>
    /// <p>Behavior is undefined if objects of this type are used after 
    /// disposal.</p>
    /// </remarks>
    public sealed class NavmeshQueryLite
    {
        private NavmeshQuery mRoot;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rootQuery">The root query object.</param>
        public NavmeshQueryLite(NavmeshQuery rootQuery)
        {
            mRoot = rootQuery;
        }

        /// <summary>
        /// Indicates whether or not the resources held by the object have
        /// been released.
        /// </summary>
        public bool IsDisposed
        {
            get { return mRoot.IsDisposed; }
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
            , ref int segmentCount)
        {
            return mRoot.GetPolySegments(polyId
                , filter
                , resultSegments
                , null
                , ref segmentCount);
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
            return mRoot.GetPolySegments(polyId
                , filter
                , resultSegments
                , segmentPolyIds
                , ref segmentCount);
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
            return mRoot.GetNearestPoly(position
                , extents
                , filter
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
            return mRoot.GetPolygons(position
                , extents
                , filter
                , resultPolyIds
                , ref resultCount);
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
                , uint[] resultPolyIds
                , uint[] resultParentIds
                , float[] resultCosts
                , ref int resultCount)
        {
            return mRoot.FindPolygons(startPolyId
                , position
                , radius
                , filter
                , resultPolyIds
                , resultParentIds
                , resultCosts
                , ref resultCount);
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
            return mRoot.FindPolygons(startPolyId
                , vertices
                , filter
                , resultPolyIds
                , resultParentIds
                , resultCosts
                , ref resultCount);
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
            return mRoot.GetPolygonsLocal(startPolyId
                , position
                , radius
                , filter
                , resultPolyIds
                , resultParentIds
                , ref resultCount);
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
            return mRoot.GetNearestPoint(polyId
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
            return mRoot.GetNearestBoundaryPoint(polyId
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
            return mRoot.GetPolyHeight(polyId
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
            return mRoot.FindDistanceToWall(polyId
                , position
                , searchRadius
                , filter
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
            return mRoot.FindPath(startPolyId
                , endPolyId
                , startPosition
                , endPosition
                , filter
                , resultPath
                , ref pathCount);
        }

        /// <summary>
        /// Casts a 'walkability' ray along the surface of the navigation mesh
        /// from the start position toward the end position.
        /// </summary>
        /// <remarks>
        /// TODO: Add more information on the hit parameter.
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
            , ref float hitParameter
            , float[] hitNormal
            , uint[] path
            , ref int pathCount)
        {
            return mRoot.Raycast(startPolyId
                , startPosition
                , endPosition
                , filter
                , ref hitParameter
                , hitNormal
                , path
                , ref pathCount);
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
            return mRoot.GetStraightPath(startPosition
                , endPosition
                , path
                , pathSize
                , straightPathPoints
                , straightPathFlags
                , straightPathIds
                , ref straightPathCount);
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
            return mRoot.MoveAlongSurface(startPolyId
                , startPosition
                , endPosition
                , filter
                , resultPosition
                , visitedPolyIds
                , ref visitedCount);
        }
    }
}
