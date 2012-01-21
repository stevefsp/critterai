﻿/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
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
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// Provides core pathfinding functionality for navigation meshes.
    /// </summary>
    /// <remarks>
    /// <para>In the context of this class: A wall is a polygon segment that
    /// is considered impassable.  A portal is a passable segment between
    /// polygons.</para>
    /// <para>For methods that support undersized buffers, if the buffer is too
    /// small to hold the entire result set the return status of the method will 
    /// include the <see cref="NavStatus.BufferTooSmall"/> flag.</para>
    /// <para>Behavior is undefined if an object is used after disposal.</para>
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
        /// <para>Certain methods are generally not safe for use by multiple
        /// clients.  These methods will fail if the object is marked as 
        /// restricted.</para>
        /// </remarks>
        public bool IsRestricted { get { return mIsRestricted; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="query">The dtQueryFilter to use.</param>
        /// <param name="isConstant">If TRUE, all find and sliced methods
        /// are disabled.</param>
        /// <param name="type">
        /// The allocation type of the dtQueryFilter 
        /// (<see cref="AllocType.Local"/> is not supported.)
        /// </param>
        internal NavmeshQuery(IntPtr query, bool isConstant, AllocType type)
            : base(type)
        {
            mIsRestricted = isConstant;
            if (type == AllocType.Local)
                root = IntPtr.Zero;
            else
                root = query;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~NavmeshQuery()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Marks the object as disposed and immediately frees all 
        /// unmanaged resources for locally owned objects.
        /// </summary>
        /// <remarks>
        /// <para>This method is not projected by the <see cref="IsRestricted"/>
        /// feature.</para>
        /// </remarks>
        public override void RequestDisposal()
        {
            if (ResourceType == AllocType.External)
                NavmeshQueryEx.dtnqFree(ref root);
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
        /// Finds the polygon nearest to the specified point.
        /// </summary>
        /// <remarks>
        /// <para>If the search box does not intersect any polygons the search
        /// will return success, but the result polyRef will be zero.  So always 
        /// check the result polyRef before using the nearest point data.</para>
        /// <warning>This function is not suitable for large area searches.  If the search
        /// extents overlaps more than 128 polygons it may return an invalid result.
        /// </warning>
        /// <para>The detail mesh is used to correct the y-value of the nearest
        /// point.</para>
        /// </remarks>
        /// <param name="searchPoint">The center of the search box.</param>
        /// <param name="extents">The search distance along each axis.
        /// [Form: (x, y, z)]</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRef">The reference id of the nearest polygon. Or
        /// zero if none could be found within the search box.</param>
        /// <param name="resultPoint">The nearest point on the polygon.
        /// [Form: (x, y, z)] (Optional Out)</param>
        /// <returns>The <see cref="NavStatus"/> flags for the query.</returns>
        public NavStatus GetNearestPoly(Vector3 searchPoint
            , Vector3 extents
            , NavmeshQueryFilter filter
            , out NavmeshPoint result)
        {
            result = NavmeshPoint.Zero;
            return NavmeshQueryEx.dtqFindNearestPoly(root
                , ref searchPoint
                , ref extents
                , filter.root
                , ref result);
        }

        /// <summary>
        /// Returns the wall segments for the specified polygon.
        /// </summary>
        /// <remarks>
        /// <para>A segment that is normally a portal will be included in the
        /// result set if the filter results in the neighbor polygon
        /// being considered impassable.
        /// </para>
        /// <para>The vertex buffer must be sized for the maximum segments per
        /// polygon of the source navigation mesh.
        /// Usually: 6 * <see cref="Navmesh.MaxAllowedVertsPerPoly"/></para>
        /// <para>The segments can be used for simple 2D collision detection.</para>
        /// </remarks>
        /// <param name="polyRef">The reference id for the polygon.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultSegments">The segment vertex buffer for all 
        /// walls. [Form: (ax, ay, az, bx, by, bz) * segmentCount]</param>
        /// <param name="segmentCount">The number of segments returned
        /// in the segments array.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolySegments(uint polyRef
            , NavmeshQueryFilter filter
            , Vector3[] resultSegments
            , out int segmentCount)
        {
            segmentCount = 0;
            return NavmeshQueryEx.dtqGetPolyWallSegments(root
                , polyRef
                , filter.root
                , resultSegments
                , null
                , ref segmentCount
                , resultSegments.Length / 2);
        }

        /// <summary>
        /// Returns the segments for the specified polygon, optionally
        /// excluding portals.
        /// </summary>
        /// <remarks>
        /// <para>If the segmentPolyRefs parameter is provided, then all polygon
        /// segments will be returned.  If the parameter is null, then only 
        /// the wall segments are returned.</para>
        /// <para>A segment that is normally a portal will be included in the
        /// result set as a wall if the filter results in the neighbor polygon
        /// being considered impassable.
        /// </para>
        /// <para>The vertex and polyRef buffers must be sized for the maximum 
        /// segments per polygon of the source navigation mesh.
        /// Usually: <see cref="Navmesh.MaxAllowedVertsPerPoly"/></para>
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
            , Vector3[] resultSegments
            , uint[] segmentPolyRefs
            , out int segmentCount)
        {
            segmentCount = 0;
            return NavmeshQueryEx.dtqGetPolyWallSegments(root
                , polyRef
                , filter.root
                , resultSegments
                , segmentPolyRefs
                , ref segmentCount
                , resultSegments.Length / 2);
        }

        /// <summary>
        /// Finds the polygons that overlap the search box.
        /// </summary>
        /// <remarks>
        /// <para>If no polygons are found, the method will return success with
        /// a result count of zero.</para>
        /// <para>If the result buffer is too small to hold the entire result set
        /// the buffer will be filled to capacity.  The method of 
        /// choosing which polygons from the full set are included in the 
        /// partial result set is undefined.</para>
        /// </remarks>
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
            , Vector3 extents
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs
            , out int resultCount)
        {
            resultCount = 0;
            return NavmeshQueryEx.dtqQueryPolygons(root
                , ref searchPoint
                , ref extents
                , filter.root
                , resultPolyRefs
                , ref resultCount
                , resultPolyRefs.Length);
        }

        /// <summary>
        /// Finds the polygons within the graph that touch the specified circle.
        /// </summary>
        /// <remarks>
        /// <para>At least one result buffer must be provided.</para>
        /// <para>The order of the result set is from least to highest
        /// cost to reach the polygon.</para>
        /// <para>The primary use case for this method is for performing
        /// Dijkstra searches.  Candidate polygons are found
        /// by searching the graph beginning at the start polygon.</para>
        /// <para>If a polygon is not found via the graph search,
        /// even if it intersects the search circle, it will not be included
        /// in the result set. Example scenario:</para>
        /// <para>polyA is the start polygon.<br/>
        /// polyB shares an edge with polyA. (Is adjacent.)<br/>
        /// polyC shares an edge with polyB, but not with polyA<br/>
        /// Even if the search circle overlaps polyC, it will not
        /// be included in the result set unless polyB is also in the set.
        /// </para>
        /// <para>The value of the center point is used as the start point 
        /// for cost calculations.  It is not projected onto the surface of the
        /// mesh, so its y-value will effect the costs.</para>
        /// <para>Intersection tests occur in 2D.  All polygons and the
        /// search circle are projected onto the xz-plane.  So the y-value of 
        /// the center point does not effect intersection tests.</para>
        /// <para>If the buffers are to small to hold the entire result set, they
        /// will be filled to capacity.</para>
        /// </remarks>
        /// <param name="startPolyRef">The reference id of the polygon to 
        /// start the search at.</param>
        /// <param name="centerPoint">The center of the query circle.</param>
        /// <param name="radius">The radius of the query circle.</param>
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
            resultCount = 0;

            // Set max count to the smallest length.
            int maxCount = (resultPolyRefs == null ? 0 : resultPolyRefs.Length);
            maxCount = (resultParentRefs == null ? maxCount 
                : Math.Min(maxCount, resultParentRefs.Length));
            maxCount = (resultCosts == null ? maxCount
                : Math.Min(maxCount, resultCosts.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.dtqFindPolysAroundCircle(root
                , start.polyRef
                , ref start.point
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
        /// <remarks>
        /// <para>The order of the result set is from least to highest
        /// cost.</para>
        /// <para>At least one result buffer must be provided.</para>
        /// <para>The primary use case for this method is for performing
        /// Dijkstra searches.  Candidate polygons are found
        /// by searching the graph beginning at the start polygon.</para>
        /// <para>The same intersection test restrictions that apply to 
        /// the circle version of this method apply to this method.</para>
        /// <para>The 3D centroid of the polygon is used as the start position for
        /// cost calculations.</para>
        /// <para>Intersection tests occur in 2D.  All polygons are projected
        /// onto the xz-plane.  So the y-values of the vertices do not effect
        /// intersection tests.</para>
        /// <para>If the buffers are is too small to hold the entire result set,
        /// they will be filled to capacity.</para>
        /// </remarks>
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
            , Vector3[] vertices
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

            return NavmeshQueryEx.dtqFindPolysAroundShape(root
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
        /// <para>This method is optimized for a small query radius and small 
        /// number of result polygons.</para>
        /// <para>The order of the result set is from least to highest cost.</para>
        /// <para>At least one result buffer must be provided.</para>
        /// <para>The primary use case for this method is for performing
        /// Dijkstra searches.  Candidate polygons are found
        /// by searching the graph beginning at the start polygon.</para>
        /// <para>The same intersection test restrictions that apply to 
        /// the FindPoly mehtods apply to this method.</para>
        /// <para>The value of the center point is used as the start point 
        /// for cost calculations.  It is not projected onto the surface of the
        /// mesh, so its y-value will effect the costs.</para>
        /// <para>Intersection tests occur in 2D.  All polygons and the
        /// search circle are projected onto the xz-plane.  So the y-value of 
        /// the center point does not effect intersection tests.</para>
        /// <para>If the buffers are is too small to hold the entire result set,
        /// they will be filled to capacity.</para>
        /// </remarks>
        /// <param name="startPolyRef">The reference id of the polygon to 
        /// start the search at.</param>
        /// <param name="centerPoint">The center of the search circle.</param>
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
            resultCount = 0;

            // Set max count to the smallest length.
            int maxCount = (resultPolyRefs == null ? 0 : resultPolyRefs.Length);
            maxCount = (resultParentRefs == null ? maxCount
                : Math.Min(maxCount, resultParentRefs.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.dtqFindLocalNeighbourhood(root
                , start.polyRef
                , ref start.point
                , radius
                , filter.root
                , resultPolyRefs
                , resultParentRefs
                , ref resultCount
                , maxCount);
        }

        /// <summary>
        /// Finds the closest point on the specified polygon.
        /// </summary>
        /// <remarks>
        /// <para>Uses the height detail to provide the most accurate information.
        /// </para>
        /// <para>The source point does not have to be within the bounds of the
        /// navigation mesh.</para>
        /// </remarks>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="sourcePoint">The position to search from.
        /// [Form: (x, y, z)]</param>
        /// <param name="resultPoint">The closest point on the polygon.
        /// [Form: (x, y, z)] (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPoint(uint polyRef
            , Vector3 sourcePoint
            , out Vector3 resultPoint)
        {
            resultPoint = Vector3Util.Zero;
            return NavmeshQueryEx.dtqClosestPointOnPoly(root
                , polyRef
                , ref sourcePoint
                , ref resultPoint);
        }

        /// <summary>
        /// Returns a point on the boundary closest to the source point if the
        /// source point is outside the polygon's xz-column.
        /// </summary>
        /// <remarks>
        /// <para>Much faster than <see cref="GetNearestPoint" />.</para>
        /// <para>If the provided point lies within the polygon's xz-column
        /// (above or below), then the source and result points will be equal
        /// </para>
        /// <para>The boundary point will be the polygon boundary, not the height
        /// corrected detail boundary.  Use <see cref="GetPolyHeight"/> if 
        /// needed.
        /// </para>
        /// <para>The source point does not have to be within the bounds of the
        /// navigation mesh.</para>
        /// </remarks>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="sourcePoint">The point to check. [Form: (x, y, z)]
        /// </param>
        /// <param name="resultPoint">The closest point. [Form: (x, y, z)] 
        /// (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPointF(uint polyRef
            , Vector3 sourcePoint
            , out Vector3 resultPoint)
        {
            resultPoint = Vector3Util.Zero;
            return NavmeshQueryEx.dtqClosestPointOnPolyBoundary(root
                , polyRef
                , ref sourcePoint
                , ref resultPoint);
        }

        /// <summary>
        /// Gets the height of the polygon at the provided point using the
        /// detail mesh. (Most accurate.)
        /// </summary>
        /// <remarks>The method will return falure if the provided point is
        /// outside the xz-column of the polygon.</remarks>
        /// <param name="polyRef">The polygon reference.</param>
        /// <param name="point">The point within the polygon's xz-column.
        /// [Form: (x, y, z)]
        /// </param>
        /// <param name="height">The height at the surface of the polygon.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolyHeight(NavmeshPoint point
            , out float height)
        {
            height = 0;
            return NavmeshQueryEx.dtqGetPolyHeight(root
                , point
                , ref height);
        }

        /// <summary>
        /// Returns the distance from the specified position to the nearest
        /// polygon wall.
        /// </summary>
        /// <remarks>
        /// <para>The closest point is not height adjusted using the detail data. 
        /// Use <see cref="GetPolyHeight"/> if needed.</para>
        /// <para>The distance will equal the search radius if there is no wall 
        /// within the radius.  In this case the values of closestPoint 
        /// and normal are undefined.</para>
        /// <para>The normal will become unpredicable if the distance is a
        /// very small number.</para>
        /// </remarks>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="searchPoint">The center of the search circle.
        /// </param>
        /// <param name="searchRadius">The radius of the search circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="distance">Distance to nearest wall.</param>
        /// <param name="closestPoint">The nearest point on the wall.
        /// [Form: (x, y, z)] (Out)</param>
        /// <param name="normal">The normalized ray formed from the wall point
        /// to the source point. [Form: (x, y, z)] (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindDistanceToWall(NavmeshPoint searchPoint
            , float searchRadius
            , NavmeshQueryFilter filter
            , out float distance
            , out Vector3 closestPoint
            , out Vector3 normal)
        {
            distance = 0;
            closestPoint = Vector3Util.Zero;
            normal = Vector3Util.Zero;
            return NavmeshQueryEx.dtqFindDistanceToWall(root
                , searchPoint
                , searchRadius
                , filter.root
                , ref distance
                , ref closestPoint
                , ref normal);
        }

        /// <summary>
        /// Finds the polygon path from the start to the end polygon.
        /// </summary>
        /// <remarks>
        /// <para>If the end polygon cannot be reached, then the last polygon
        /// is the nearest one found to the end polygon.</para>
        /// <para>If the path buffer is to small to hold the result, it will
        /// be filled as far as possible from the start polygon toward the
        /// end polygon.</para>
        /// <para>The start and end points are used to calculate
        /// traversal costs. (y-values matter.)</para>
        /// </remarks>
        /// <param name="startPolyRef">The reference id of the start polygon.
        /// </param>
        /// <param name="endPolyRef">The reference id of the end polygon.
        /// </param>
        /// <param name="startPoint">A point within the start polygon.
        /// [(x, y, z)]</param>
        /// <param name="endPoint">A point within the end polygon.
        /// [(x, y, z)]</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath">An ordered list of reference ids in the
        /// path. (Start to end.) [(polyRef) * pathCount] [Out]</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPath(NavmeshPoint start
            , NavmeshPoint end
            , NavmeshQueryFilter filter
            , uint[] resultPath
            , out int pathCount)
        {
            pathCount = 0;
            return NavmeshQueryEx.dtqFindPath(root
                , start
                , end
                , filter.root
                , resultPath
                , ref pathCount
                , resultPath.Length);
        }

        /// <summary>
        /// Finds the polygon path from the start to the end polygon, searching
        /// for points not on the navigation mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is useful if the polygon reference of either the
        /// <paramref name="start"/> or <paramref name="end"/> point is not 
        /// known. If both points have a polygon reference of zero, then this
        /// method is equivalent to the following:</para>
        /// <ol>
        /// <li>Call <see cref="GetNearestPoly"/> using the 
        /// <paramref name="start"/> point.</li>
        /// <li>Call <see cref="GetNearestPoly"/> using the 
        /// <paramref name="end"/> point.</li>
        /// <li>Call the normal find path using the two new 
        /// start and end points.
        /// </li>
        /// </ol>
        /// <para><em>A point search will only be performed for points with a
        /// polygon reference of zero.</em> If a point search is reqired, the
        /// point and its polygon reference parameter become output parameters.
        /// The point will be snapped to the navigation mesh.</para>
        /// <para>This method may return a partial result, even if there
        /// is a failure.  If there is no failure, it will at least
        /// perform the required point searches.  It will then perform the find
        /// path operation if the point searches successful.</para>
        /// <para>Checking the return results:</para>
        /// <ul>
        /// <li>If the <paramref name="pathCount"/> is greater than zero, then
        /// the path and all required point searches succeeded.</li>
        /// <li>If the overall operation failed, but a point with 
        /// an input polygon reference of zero has an output polygon reference
        /// that is non-zero, then that point's search succeeded.</li>
        /// </ul>
        /// <para>For the path results:</para>
        /// <para>If the end polygon cannot be reached, then the last polygon
        /// is the nearest one found to the end polygon.</para>
        /// <para>If the path buffer is to small to hold the result, it will
        /// be filled as far as possible from the start polygon toward the
        /// end polygon.</para>
        /// <para>The start and end points are used to calculate
        /// traversal costs. (y-values matter.)</para>
        /// </remarks>
        /// <param name="startPolyRef">The reference id of the start polygon.
        /// (An input value of zero triggers a search.)</param>
        /// <param name="endPolyRef">The reference id of the end polygon.
        /// (An input value of zero triggers a search.)</param>
        /// <param name="startPoint">A point within the start polygon.
        /// ([(x, y, z)] [In] (Also Out if <paramref name="startPolyRef"/>
        /// is zero.)]</param>
        /// <param name="endPoint">A point within the end polygon.
        /// [(x, y, z)]  [In] (Also Out if <paramref name="endPolyRef"/>
        /// is zero.)]</param>
        /// <param name="extents">The search extents to use if the start
        /// or end point polygon reference is zero.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath">An ordered list of reference ids in the
        /// path. (Start to end.) [(polyRef) * pathCount] [Out]</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPath(ref NavmeshPoint start
            , ref NavmeshPoint end
            , Vector3 extents
            , NavmeshQueryFilter filter
            , uint[] resultPath
            , out int pathCount)
        {
            pathCount = 0;
            return NavmeshQueryEx.dtqFindPathExt(root
                , ref start
                , ref end
                , ref extents
                , filter.root
                , resultPath
                , ref pathCount
                , resultPath.Length);
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
            return NavmeshQueryEx.dtqIsInClosedList(root, polyRef);
        }

        /// <summary>
        /// Returns true if the polygon reference is valid and passes the 
        /// filter restrictions.
        /// </summary>
        /// <param name="polyRef">A polygon reference id.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <returns>True if the polygon reference is valid and passes the 
        /// filter restrictions.</returns>
        public bool IsValidPolyRef(uint polyRef, NavmeshQueryFilter filter)
        {
            return NavmeshQueryEx.dtqIsValidPolyRef(root, polyRef, filter.root);
        }

        /// <summary>
        /// Casts a 'walkability' ray along the surface of the navigation mesh
        /// from the start point toward the end point.
        /// </summary>
        /// <remarks>
        /// <para>This method is meant to be used for quick short distance checks.
        /// </para>
        /// <para>If the path buffer is too small to hold the result, it will
        /// be filled as far as possible from the start point toward the
        /// end point.</para>
        /// <para><b>Using the Hit Paramter</b></para>
        /// <para>If the hit parameter is a very high value (>1E38), then
        /// the ray has hit the end point.  In this case the path represents a 
        /// valid corridor to the end point and the value of hitNormal is 
        /// undefined.
        /// </para>
        /// <para>If the hit parameter is zero, then the start point is on the
        /// border that was hit and the value of hitNormal is undefined.</para>
        /// <para>If <c>0 &lt; hitParameter &lt; 1.0 </c>
        /// then the following applies:</para>
        /// <code>
        /// distanceToHitBorder = distanceToEndPoint * hitParameter<br/>
        /// hitPoint = startPoint + (endPoint - startPoint) * hitParameter
        /// </code>
        /// <para><b>Use Case Restriction</b></para>
        /// <para>The raycast ignores the y-value of the end point.  (2D check)
        /// This places significant limits on how it can be used.
        /// Example scenario:</para>
        /// <para>Consider a scene where there is a main floor with a second
        /// floor balcony that hangs over the main floor.  So the first floor
        /// mesh extends below the balcony mesh.  The start point is somewhere 
        /// on the first floor.  The end point is on the balcony.</para>
        /// <para>The raycast will search toward the end point along the first 
        /// floor mesh.  If it reaches the end point's xz-coordinates it will 
        /// indicate 'no hit', meaning it reached the end point.
        /// </para>
        /// </remarks>
        /// <param name="startPolyRef">The reference id of the start polygon.
        /// </param>
        /// <param name="startPoint">A point within the start polygon
        /// representing the start of the ray. [Form: (x, y, z)]</param>
        /// <param name="endPoint">The point to cast the ray toward.
        /// [Form: (x, y, z)]
        /// </param>
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
            pathCount = 0;
            hitParameter = 0;
            hitNormal = Vector3Util.Zero;

            int maxCount = (path == null ? 0 : path.Length);

            return NavmeshQueryEx.dtqRaycast(root
                , startPoint
                , ref endPoint
                , filter.root
                , ref hitParameter
                , ref hitNormal
                , path
                , ref pathCount
                , maxCount);
        }

        /// <summary>
        /// Returns the staight path from the start to the end point
        /// within the polygon corridor.
        /// </summary>
        /// <remarks>
        /// <para>This method peforms what is often called 'string pulling'.</para>
        /// <para>If the provided result buffers are too small for the entire
        /// result set, they will be filled as far as possible from the 
        /// start point toward the end point.</para>
        /// <para>The start point is clamped to the first polygon in 
        /// the path, and the end point is clamped to the last. So the start 
        /// and end points should be within or very near the first and last 
        /// polygons respectively.  The pathStart and pathCount parameters can 
        /// be adjusted to restrict the usable portion of the the path to 
        /// meet this requirement. (See the example below.)</para>
        /// <para>The returned polygon references represent the reference id of 
        /// the polygon that is entered at the associated path point.  The 
        /// reference id associated with the end point will always be zero.</para>
        /// <para>Example use case for adjusting the straight path during
        /// locomotion:</para>
        /// <para>Senario: The path consists of polygons A, B, C, D, with the
        /// start point in A and the end point in D.</para>
        /// <para>The first call to the method will return straight waypoints for 
        /// the entire path:<br/>
        /// <code>
        /// query.GetStraightPath(startPoint, endPoint
        ///     , path
        ///     , 0, 4   // pathStart, pathCount
        ///     , straigthPath, null, null
        ///     , out straightCount);
        /// </code>
        /// </para>
        /// <para>If the agent moves into polygon B and needs to recaclulate its
        /// straight path for some reason, it can call the method as
        /// follows using the original path buffer:<br/>
        /// <code>
        /// query.GetStraightPath(startPoint, endPoint
        ///     , path
        ///     , 1, 3   // pathStart, pathCount  &lt;- Note the changes here.
        ///     , straigthPath, null, null
        ///     , out straightCount);
        /// </code></para>
        /// </remarks>
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
            , Vector3[] straightPathPoints
            , WaypointFlag[] straightPathFlags
            , uint[] straightPathRefs
            , out int straightPathCount)
        {
            straightPathCount = 0;

            int maxPath = straightPathPoints.Length;
            maxPath = (straightPathFlags == null ? maxPath
                : Math.Min(straightPathFlags.Length, maxPath));
            maxPath = (straightPathRefs == null ? maxPath
                : Math.Min(straightPathRefs.Length, maxPath));

            if (maxPath < 1)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.dtqFindStraightPath(root
                , ref startPoint
                , ref endPoint
                , path
                , pathStart
                , pathCount
                , straightPathPoints
                , straightPathFlags
                , straightPathRefs
                , ref straightPathCount
                , maxPath);
        }

        /// <summary>
        /// Moves from the start to the end point constrained to
        /// the navigation mesh.
        /// </summary>
        /// <remarks>
        /// <para>This method is optimized for small delta movement and a small
        /// number of polygons. If used for too great a distance, the
        /// result set will form an incomplete path.</para>
        /// <para>The result point will equal the end point if the end
        /// is reached. Otherwise the closest reachable point will be 
        /// returned.</para>
        /// <para>The result position is not projected to the surface of the
        /// navigation mesh.  If that is needed, use 
        /// <see cref="GetPolyHeight"/>.</para>
        /// <para>This method treats the end point in the same manner as the
        /// <see cref="Raycast"/> method.  (As a 2D point.) 
        /// See that method's documentation for details on the impact.</para>
        /// <para>If the result buffer is too small to hold the entire result
        /// set, it will be filled as far as possible from the start point
        /// toward the end point.</para>
        /// </remarks>
        /// <param name="startPolyRef">The reference id of the start polygon.
        /// </param>
        /// <param name="startPoint">A position within the start
        /// polygon. [Form: (x, y, x)]</param>
        /// <param name="endPoint">The end position. [Form: (x, y, z)]
        /// </param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPoint">The result point form the move.
        /// [Form: (x, y, x)]</param>
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
            visitedCount = 0;
            resultPoint = Vector3Util.Zero;

            return NavmeshQueryEx.dtqMoveAlongSurface(root
                , start
                , ref endPoint
                , filter.root
                , ref resultPoint
                , visitedPolyRefs
                , ref visitedCount
                , visitedPolyRefs.Length);
        }

        /// <summary>
        /// Initializes a sliced path find query.
        /// </summary>
        /// <remarks>
        /// <para>This method will fail if <see cref="IsRestricted"/> is TRUE.</para>
        /// <para>WARNING: Calling any other query methods besides the other
        /// sliced path methods before finalizing this query may result
        /// in corrupted data.</para>
        /// <para>The filter is stored and used for the duration of the query.</para>
        /// <para>The standard use case:</para>
        /// <ol>
        /// <li>Initialize the sliced path query</li>
        /// <li>Call <see cref="UpdateSlicedFindPath"/> until its status
        /// returns complete.</li>
        /// <li>Call <see cref="FinalizeSlicedFindPath"/> to get the path.</li>
        /// </ol>
        /// </remarks>
        /// <param name="startPolyRef">The reference id of the start polygon.
        /// </param>
        /// <param name="endPolyRef">The reference id of the end polygon.
        /// </param>
        /// <param name="startPoint">A point within the start polygon.
        /// [Form: (x, y, x)] </param>
        /// <param name="endPoint">A point within the end polygon.
        /// [Form: (x, y, x)]</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus InitSlicedFindPath(NavmeshPoint start
            , NavmeshPoint end
            , NavmeshQueryFilter filter)
        {
            if (mIsRestricted)
                return NavStatus.Failure;
            return NavmeshQueryEx.dtqInitSlicedFindPath(root
                , start
                , end
                , filter.root);
        }

        /// <summary>
        /// Continues a sliced path find query.
        /// </summary>
        /// <remarks>
        /// <para>This method will fail if <see cref="IsRestricted"/> is TRUE.</para>
        /// </remarks>
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
            return NavmeshQueryEx.dtqUpdateSlicedFindPath(root
                , maxIterations
                , ref actualIterations);
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
            pathCount = 0;
            if (mIsRestricted)
                return NavStatus.Failure;
            return NavmeshQueryEx.dtqFinalizeSlicedFindPath(root
                , path
                , ref pathCount
                , path.Length);
        }

        /// <summary>
        /// Creates a new navigation mesh query based on the provided 
        /// navigation mesh.
        /// </summary>
        /// <param name="navmesh">A navigation mesh to query against.</param>
        /// <param name="maximumNodes">The maximum number of nodes
        /// allowed when performing A* and Dijkstra searches.</param>
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

            NavStatus status = NavmeshQueryEx.dtnqBuildDTNavQuery(
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
