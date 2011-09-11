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
using UnityEngine;

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// Provides a Unity friendly interface to the NavmeshQuery class.
    /// </summary>
    /// <remarks>
    /// <para>See the <see cref="NavmeshQuery"/> class for detailed descriptions 
    /// of the methods common to the two classes.</para>
    /// <para>While this is technically a convenience class, it is implemented
    /// in such a way that its features will have the minimum possible negative
    /// impact performance and memory.</para>
    /// </remarks>
    public class U3DNavmeshQuery
    {
        // Note: Purposefully not sealed.

        private float[] vbuffA = new float[3];
        private float[] vbuffB = new float[3];
        private float[] vbuffC = new float[3];

        private NavmeshQuery mRoot;

        /// <summary>
        /// The root object being used for querys.
        /// </summary>
        public NavmeshQuery RootQuery { get { return mRoot; } }

        /// <summary>
        /// TRUE if the object resources have been disposed and the object
        /// should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return mRoot.IsDisposed; } }

        /// <summary>
        /// If TRUE, certain methods are disabled.
        /// </summary>
        /// <remarks>
        /// <para>Certain methods are generally not safe for use by multiple
        /// clients.  These methods will fail if the object is marked as 
        /// restricted.</para>
        /// </remarks>
        public bool IsRestricted { get { return mRoot.IsRestricted; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rootQuery">The root object to use for queries.
        /// </param>
        public U3DNavmeshQuery(NavmeshQuery rootQuery)
        {
            mRoot = rootQuery;
        }

        /// <summary>
        /// Finds the polygons within the graph that touch the specified circle.
        /// </summary>
        /// <param name="start">The start location and polygon for the search.
        /// </param>
        /// <param name="radius">The radius of the search. 
        /// (From the start point.)</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">The reference ids of the polygons 
        /// touched by the circle. [Form: (polyRef) * resultCount] (Optional)
        /// </param>
        /// <param name="resultParentRefs">The reference ids of the parent 
        /// polygons for each result.  Zero if a result polygon has no parent. 
        /// [Form: (parentRef) * resultCount] (Optional)</param>
        /// <param name="resultCosts">The search cost from the center point to
        /// the polygon. [Form: (cost) * resultCount] (Optional)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPolys(NavmeshPoint start
            , float radius
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs
            , uint[] resultParentRefs
            , float[] resultCosts 
            , out int resultCount)
        {
            return mRoot.FindPolys(start.polyRef
                , Vector3Util.GetVector(start.point, vbuffA)
                , radius
                , filter
                , resultPolyRefs
                , resultParentRefs
                , resultCosts
                , out resultCount);
        }

        /// <summary>
        ///  Finds the non-overlapping navigation polygons in the local
        /// neighborhood around the specified point.
        /// </summary>
        /// <param name="start">The start location and polygon for the search.
        /// </param>
        /// <param name="radius">The radius of the search circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">The reference ids of the polygons 
        /// touched by the circle. [Form: (polyRef) * resultCount] (Optional)
        /// </param>
        /// <param name="resultParentRefs">The reference ids of the parent 
        /// polygons for each result.  Zero if a result polygon has no parent. 
        /// [Form: (parentRef) * resultCount] (Optional)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolysLocal(NavmeshPoint start
            , float radius
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs
            , uint[] resultParentRefs
            , out int resultCount)
        {
            return mRoot.GetPolysLocal(start.polyRef
                , Vector3Util.GetVector(start.point, vbuffA)
                , radius
                , filter
                , resultPolyRefs
                , resultParentRefs
                , out resultCount);
        }

        /// <summary>
        /// Finds the navigation polygons within the graph that touch the
        /// specified convex polygon.
        /// </summary>
        /// <param name="startPolyRef">The reference id of the polygon to start 
        /// the search at.</param>
        /// <param name="vertices">The vertices of the convex polygon.
        /// [Form (x, y, z) * vertCount]</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">The reference ids of the polygons 
        /// touched by the search polygon. [Form: (polyRef) * resultCount]
        /// (Optional)
        /// </param>
        /// <param name="resultParentRefs">The reference ids of the parent 
        /// polygons for each result.  Zero if a result polygon has no parent. 
        /// [Form: (parentRef) * resultCount] (Optional)</param>
        /// <param name="resultCosts">The search cost from the centroid point to
        /// the polygon. [Form: (cost) * resultCount] (Optional)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPolys(uint startPolyRef
                , float[] vertices
                , NavmeshQueryFilter filter
                , uint[] resultPolyRefs
                , uint[] resultParentRefs
                , float[] resultCosts
                , out int resultCount)
        {
            return mRoot.FindPolys(startPolyRef
                , vertices
                , filter
                , resultPolyRefs
                , resultParentRefs
                , resultCosts
                , out resultCount);
        }

        /// <summary>
        /// Finds the polygon nearest to the specified point.
        /// </summary>
        /// <param name="searchPoint">The center of the search box.</param>
        /// <param name="extents">The search distance along each axis.
        /// [Form: (x, y, z)]</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="result">The nearest point on the polygon.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the query.</returns>
        public NavStatus GetNearestPoly(Vector3 searchPoint
            , float[] extents
            , NavmeshQueryFilter filter
            , out NavmeshPoint result)
        {
            result = new NavmeshPoint();
            Vector3Util.GetVector(searchPoint, vbuffA);

            ResetVBuffB();

            NavStatus status = mRoot.GetNearestPoly(vbuffA
                , extents
                , filter
                , out result.polyRef
                , vbuffB);

            result.point = Vector3Util.GetVector(vbuffB);

            return status;
        }

        /// <summary>
        /// Finds the closest point on the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="sourcePoint">The position to search from.</param>
        /// <param name="resultPoint">The closest point on the polygon.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPoint(uint polyRef
            , Vector3 sourcePoint
            , out Vector3 resultPoint)
        {
            Vector3Util.GetVector(sourcePoint, vbuffA);

            ResetVBuffB();

            NavStatus status = mRoot.GetNearestPoint(polyRef
                , vbuffA
                , vbuffB);

            resultPoint = Vector3Util.GetVector(vbuffB);

            return status;
        }

        /// <summary>
        /// Returns a point on the boundary closest to the source point if the
        /// source point is outside the polygon's xz-column.
        /// </summary>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="sourcePoint">The point to check.</param>
        /// <param name="resultPoint">The closest point.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPointF(uint polyRef
            , Vector3 sourcePoint
            , out Vector3 resultPoint)
        {
            Vector3Util.GetVector(sourcePoint, vbuffA);

            ResetVBuffB();

            NavStatus status = mRoot.GetNearestPointF(polyRef
                , vbuffA
                , vbuffB);

            resultPoint = Vector3Util.GetVector(vbuffB);

            return status;
        }

        /// <summary>
        /// Returns the distance from the specified position to the nearest
        /// polygon wall.
        /// </summary>
        /// <param name="searchPoint">The center of the search circle.
        /// </param>
        /// <param name="searchRadius">The radius of the search circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="distance">Distance to nearest wall.</param>
        /// <param name="closestPoint">The nearest point on the wall.</param>
        /// <param name="normal">The normalized ray formed from the wall point
        /// to the source point.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindDistanceToWall(NavmeshPoint searchPoint
            , float searchRadius
            , NavmeshQueryFilter filter
            , out float distance
            , out Vector3 closestPoint
            , out Vector3 normal)
        {
            ResetVBuffB();
            ResetVBuffC();

            NavStatus status = mRoot.FindDistanceToWall(searchPoint.polyRef
                , Vector3Util.GetVector(searchPoint.point, vbuffA)
                , searchRadius
                , filter
                , out distance
                , vbuffB
                , vbuffC);

            closestPoint = Vector3Util.GetVector(vbuffB);
            normal = Vector3Util.GetVector(vbuffC);

            return status;
        }

        /// <summary>
        /// Gets the height of the polygon at the provided point using the
        /// detail mesh. (Most accurate.)
        /// </summary>
        /// <param name="point">The point within the polygon's xz-column.</param>
        /// <param name="height">The height at the surface of the polygon.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolyHeight(NavmeshPoint point
            , out float height)
        {
            return mRoot.GetPolyHeight(point.polyRef
                , Vector3Util.GetVector(point.point, vbuffA)
                , out height);
        }

        /// <summary>
        /// Casts a 'walkability' ray along the surface of the navigation mesh
        /// from the start point toward the end point.
        /// </summary>
        /// <param name="startPoint">The start location of the ray.</param>
        /// <param name="endPoint">The point to cast the ray toward.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="hitParameter">The hit parameter.  (>1E38 if no hit.)
        /// </param>
        /// <param name="hitNormal">The normal of the nearest wall hit.</param>
        /// <param name="path">The reference ids of the visited polygons. 
        /// [Form: (polyRef) * pathCount] (Optional)
        /// </param>
        /// <param name="pathCount">The number of visited polygons.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus Raycast(NavmeshPoint startPoint
            , Vector3 endPoint
            , NavmeshQueryFilter filter
            , out float hitParameter
            , out Vector3 hitNormal
            , uint[] path
            , out int pathCount)
        {
            ResetVBuffC();

            NavStatus status = mRoot.Raycast(startPoint.polyRef
                , Vector3Util.GetVector(startPoint.point, vbuffA)
                , Vector3Util.GetVector(endPoint, vbuffB)
                , filter
                , out hitParameter
                , vbuffC
                , path
                , out pathCount);

            hitNormal = Vector3Util.GetVector(vbuffC);

            return status;
        }

        /// <summary>
        /// Returns the wall segments for the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference id for the polygon.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultSegments">The segment vertex buffer for all 
        /// walls. [Form: (ax, ay, az, bx, by, bz) * segmentCount]</param>
        /// <param name="segmentCount">The number of segments returned
        /// in the segments array.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolySegments(uint polyRef
            , NavmeshQueryFilter filter
            , float[] resultSegments
            , out int segmentCount)
        {
            return mRoot.GetPolySegments(polyRef
                , filter
                , resultSegments
                , out segmentCount);
        }

        /// <summary>
        /// Returns the segments for the specified polygon, optionally
        /// excluding portals.
        /// </summary>
        /// <remarks>
        /// <para>If the segmentPolyRefs parameter is provided, then all polygon
        /// segments will be returned.  If the parameter is null, then only 
        /// the wall segments are returned.</para>
        /// </remarks>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultSegments">The segment vertex buffer for all
        /// segments.
        /// [Form: (ax, ay, az, bx, by, bz) * segmentCount]</param>
        /// <param name="segmentPolyRefs">Refernce ids of the each segment's 
        /// neighbor polygon. Or zero if the segment is considered impassable. 
        /// [Form: (polyRef) * segmentCount] (Optional)</param>
        /// <param name="segmentCount">The number of segments returned.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolySegments(uint polyRef
            , NavmeshQueryFilter filter
            , float[] resultSegments
            , uint[] segmentPolyRefs
            , out int segmentCount)
        {
            return mRoot.GetPolySegments(polyRef
                , filter
                , resultSegments
                , segmentPolyRefs
                , out segmentCount);
        }

        /// <summary>
        /// Finds the polygons that overlap the search box.
        /// </summary>
        /// <param name="searchPoint">The center of the query box.
        /// [Form: (x, y, z)]</param>
        /// <param name="extents">The search distance along each axis.
        /// [Form: (x, y, z)]</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">The reference ids of the polygons that
        /// overlap the query box. [Form: (polyRef) * resultCount] (Out)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolys(Vector3 searchPoint
            , float[] extents
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs
            , out int resultCount)
        {
            return mRoot.GetPolys(Vector3Util.GetVector(searchPoint, vbuffA)
                , extents
                , filter
                , resultPolyRefs
                , out resultCount);
        }

        /// <summary>
        /// Finds the polygon path from the start to the end polygon.
        /// </summary>
        /// <remarks>
        /// This method will fail if the polygon reference of either the start
        /// or end points is zero.</remarks>
        /// <param name="start">The start of the path.</param>
        /// <param name="end">The goal of the path.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath">An ordered list of reference ids in the
        /// path. (Start to end.) [Form: (polyRef) * pathCount]</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPath(NavmeshPoint start
            , NavmeshPoint end
            , NavmeshQueryFilter filter
            , uint[] resultPath
            , out int pathCount)
        {
            return mRoot.FindPath(start.polyRef
                , end.polyRef
                , Vector3Util.GetVector(start.point, vbuffA)
                , Vector3Util.GetVector(end.point, vbuffB)
                , filter
                , resultPath
                , out pathCount);
        }

        /// <summary>
        /// Finds the polygon path from the start to the end point.
        /// </summary>
        /// <remarks>
        /// <para>This method will attempt to snap the start and end points
        /// to the mesh if either has a polygon reference of zero.</para>
        /// </remarks>
        /// <param name="start">The start of the path.</param>
        /// <param name="end">The goal of the path.</param>
        /// <param name="extents">The search extents to use if the start
        /// and/or end points need to be located.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath">An ordered list of reference ids in the
        /// path. (Start to end.) [Form: (polyRef) * pathCount]</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPath(ref NavmeshPoint start
            , ref NavmeshPoint end
            , float[] extents
            , NavmeshQueryFilter filter
            , uint[] resultPath
            , out int pathCount)
        {
            NavStatus status = mRoot.FindPath(ref start.polyRef
                , ref end.polyRef
                , Vector3Util.GetVector(start.point, vbuffA)
                , Vector3Util.GetVector(end.point, vbuffB)
                , extents
                , filter
                , resultPath
                , out pathCount);

            start.point = Vector3Util.GetVector(vbuffA);
            end.point = Vector3Util.GetVector(vbuffB);

            return status;
        }

        /// <summary>
        /// Returns TRUE if the polygon refernce is in the current closed list.
        /// </summary>
        /// <remarks>
        /// <para>The closed list is the list of polygons that were fully evaluated
        /// during a find operation.</para>
        /// <para>All methods prefixed with "Find" and all sliced path methods
        /// generate a closed list.  The content of the list will persist 
        /// until the next find/sliced method is called.</para>
        /// </remarks>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <returns>TRUE if the polgyon is in the current closed list.
        /// </returns>
        public bool IsInClosedList(uint polyRef)
        {
            return mRoot.IsInClosedList(polyRef);
        }

        /// <summary>
        /// Returns the staight path from the start to the end point
        /// within the polygon corridor.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="path">The list of polygon references that represent the
        /// path corridor.</param>
        /// <param name="pathStart">The index within the path buffer of
        /// the polygon that contains the start point.</param>
        /// <param name="pathCount">The length of the path within the path
        /// buffer. (endPolyIndex - startPolyIndex)</param>
        /// <param name="straightPathPoints">Points describing the straight
        /// path. [Form: (x, y, z) * straightPathCount].</param>
        /// <param name="straightPathFlags">Flags describing each point.
        /// [Form: (flag) * striaghtPathCount] (Optional)</param>
        /// <param name="straightPathRefs">The reference id of the polygon that
        /// is being entered at the point position. 
        /// [Form: (polyRef) * straightPathCount] (Optional)</param>
        /// <param name="straightPathCount">The number of points in the
        /// straight path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetStraightPath(Vector3 startPoint
            , Vector3 endPoint
            , uint[] path
            , int pathStart
            , int pathCount
            , float[] straightPathPoints
            , WaypointFlag[] straightPathFlags
            , uint[] straightPathRefs
            , out int straightPathCount)
        {
            return mRoot.GetStraightPath(
                Vector3Util.GetVector(startPoint, vbuffA)
                , Vector3Util.GetVector(endPoint, vbuffB)
                , path
                , pathStart
                , pathCount
                , straightPathPoints
                , straightPathFlags
                , straightPathRefs
                , out straightPathCount);
        }

        /// <summary>
        /// Moves from the start to the end point constrained to
        /// the navigation mesh.
        /// </summary>
        /// <param name="start">The start position.</param>
        /// <param name="endPoint">The end position.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPoint">The result of the move.</param>
        /// <param name="visitedPolyRefs">The reference ids of the polygons
        /// visited during the move. [Form: (polyRef) * visitedCount]</param>
        /// <param name="visitedCount">The number of polygons visited during
        /// the move.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus MoveAlongSurface(NavmeshPoint start
            , Vector3 endPoint
            , NavmeshQueryFilter filter
            , out Vector3 resultPoint
            , uint[] visitedPolyRefs
            , out int visitedCount)
        {
            ResetVBuffC();

            NavStatus status = mRoot.MoveAlongSurface(start.polyRef
                , Vector3Util.GetVector(start.point, vbuffA)
                , Vector3Util.GetVector(endPoint, vbuffB)
                , filter
                , vbuffC
                , visitedPolyRefs
                , out visitedCount);

            resultPoint = Vector3Util.GetVector(vbuffC);

            return status;
        }

        /// <summary>
        /// Initializes a sliced path find query.
        /// </summary>
        /// <param name="start">The start position.</param>
        /// <param name="end">The goal position.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus InitSlicedFindPath(NavmeshPoint start
            , NavmeshPoint end
            , NavmeshQueryFilter filter)
        {
            return mRoot.InitSlicedFindPath(start.polyRef
                , end.polyRef
                , Vector3Util.GetVector(start.point, vbuffA)
                , Vector3Util.GetVector(end.point, vbuffC)
                , filter);
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
            , out int actualIterations)
        {
            return mRoot.UpdateSlicedFindPath(maxIterations
                , out actualIterations);
        }

        /// <summary>
        /// Finalizes and returns the results of the sliced path query.
        /// </summary>
        /// <remarks>
        /// <para>This method will fail if <see cref="IsRestricted"/> is TRUE.</para>
        /// </remarks>
        /// <param name="path">An ordered list of polygons representing the
        /// path. [Form: (polyRef) * pathCount]</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FinalizeSlicedFindPath(uint[] path
            , out int pathCount)
        {
            return mRoot.FinalizeSlicedFindPath(path, out pathCount);
        }

        private void ResetVBuffB()
        {
            vbuffB[0] = 0;
            vbuffB[1] = 0;
            vbuffB[2] = 0;
        }

        private void ResetVBuffC()
        {
            vbuffC[0] = 0;
            vbuffC[1] = 0;
            vbuffC[2] = 0;
        }
    }
}
