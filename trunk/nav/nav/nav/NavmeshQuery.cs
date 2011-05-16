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
        /// <remarks>
        /// <p>If the search box does not intersect any polygons the search
        /// return success, but the result polyRef will be zero.  So always 
        /// check the result polyRef before using the nearest point data.</p>
        /// <p>The detail mesh is used to adjust the y-value of the nearest
        /// point result. So there is no need to call the 
        /// <see cref="GetPolyHeight"/> method.</p>
        /// </remarks>
        /// <param name="point">The center of the search volumn.</param>
        /// <param name="extents">The search distance along each axis in the
        /// form (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="nearestPolyRef">The id of the nearest polygon. Or
        /// zero if none could be found within the query box.</param>
        /// <param name="nearestPoint">The nearest point on the polygon
        /// in the form (x, y, z). (Optional Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPoly(float[] sourcePoint
            , float[] extents
            , NavmeshQueryFilter filter
            , out uint resultPolyRef
            , float[] resultPoint)
        {
            resultPolyRef = 0;
            return NavmeshQueryEx.GetNearestPoly(root
                , sourcePoint
                , extents
                , filter.root
                , ref resultPolyRef
                , resultPoint);
        }

        /// <summary>
        /// Returns the impassable segments for the specified polygon.
        /// </summary>
        /// <remarks>
        /// <p>
        /// A portal will be included in the set if the filter indicates 
        /// the neighbor is impassable.
        /// </p>
        /// <p>The impassable segments of the polygon can be used for simple 2D
        /// collision detection.</p>
        /// </remarks>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="segmentVerts">An array to load the wall vertices into. 
        /// (ax, ay, az, bx, by, bz) * maxSegments.</param>
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
        /// excluding portals.
        /// </summary>
        /// <remarks>
        /// <p>If the segmentPolyRefs parameter is provided, then all polygon
        /// segments will be returned.  If the parameter is ommited, then only 
        /// the impassable segments are returned.</p>
        /// <p>
        /// A portal will be included in the impassable set as impassable if the filter 
        /// indicates the neighbor is impassable.
        /// </p>
        /// <p>The impassable segments of the polygon can be used for simple 2D
        /// collision detection.</p>
        /// <p>The segments are polygon segments, not detail mesh segments.</p>
        /// </remarks>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="filter">The filter to apply to the query. Used to
        /// determine which polygon neighbors are considered accessible.</param>
        /// <param name="segmentVerts">The segment vertex buffer. 
        /// [Form: (ax, ay, az, bx, by, bz) * maxSegments]</param>
        /// <param name="segmentPolyRefs">Ids of the each segment's neighbor
        /// polygon. Or zero if the segment is considered impassable. 
        /// [Length: maxSegements] (Optional)</param>
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
        /// <remarks>
        /// <p>If no polygons are found, the method will return success with
        /// a result count of zero.</p>
        /// <p>If the result buffer is too small to hold the full result set, 
        /// the method will return success with the 
        /// <see cref="NavStatus.BufferTooSmall"/> flag set.  The method of 
        /// choosing which polygons from the full set are included in the 
        /// incomplete result set is undefined.</p>
        /// <p>The segments are polygon segments, not detail segments.</p>
        /// </remarks>
        /// <param name="position">The center of the query box in the form
        /// (x, y, z).</param>
        /// <param name="extents">The search distance along each axis in the
        /// form (x, y, z).</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">The references of the polygons that
        /// overlap the query box. (Out)</param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolys(float[] searchPoint
            , float[] extents
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs
            , out int resultCount)
        {
            resultCount = 0;
            return NavmeshQueryEx.GetPolys(root
                , searchPoint
                , extents
                , filter.root
                , resultPolyRefs
                , ref resultCount
                , resultPolyRefs.Length);
        }

        /// <summary>
        /// Finds the polygons within the graph that touch the specified circle.
        /// </summary>
        /// <remarks>
        /// <p>The order of the result set is from least to highest
        /// cost.</p>
        /// <p>At least one result buffer must be provided.</p>
        /// <p>The return status will include the 
        /// <see cref="NavStatus.BufferToSmall"/> flag if the provided buffers 
        /// are too small to hold the entire result set. (The operation will not
        /// fail.)</p>
        /// <p>The primary use case for this method is for performing
        /// Dijkstra searches.  This is because candidate polygons are found
        /// by searching the graph beginning at the start polygon.  If a 
        /// navmesh polygon is not found via the graph search,
        /// even if it intersects the search circle, it will not be included
        /// in the result set. Example scenario:</p>
        /// <p>polyA is the start polygon.<br/>
        /// polyB shares an edge with polyA. (Is adjacent.)<br/>
        /// polyC shares an edge with polyB, but not with polyA<br/>
        /// Even if the search circle overlaps polyC, it will not
        /// be included in the result set unless polyB is also in the set.
        /// </p>
        /// <p>The value of the position argument is used as the start point 
        /// for cost calculations.  It is not projected onto the surface of the
        /// mesh, so its y-value will effect the costs.</p>
        /// <p>Intersection tests occur in 2D with all polygons and the
        /// search circle projected onto the xz-plane.  So the y-value of the
        /// position argument does not effect intersection tests.</p>
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
        public NavStatus FindPolys(uint startPolyRef
                , float[] centerPoint
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

            return NavmeshQueryEx.FindPolys(root
                , startPolyRef
                , centerPoint
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
        /// <p>The order of the result set is from least to highest
        /// cost.</p>
        /// <p>At least one result buffer must be provided.</p>
        /// <p>The return status will include the 
        /// <see cref="NavStatus.BufferToSmall"/> flag if the provided buffers 
        /// are too small to hold the entire result set. (The operation will not
        /// fail.)</p>
        /// <p>The primary use case for this method is for performing
        /// Dijkstra searches.  This is because candidate polygons are found
        /// by searching the graph beginning at the start polygon.  If a 
        /// navmesh polygon is not found via the graph search,
        /// even if it intersects the search polygon, it will not be included
        /// in the result set. Example scenario:</p>
        /// <p>polyA is the start polygon.<br/>
        /// polyB shares an edge with polyA. (Is adjacent.)<br/>
        /// polyC shares an edge with polyB, but not with polyA<br/>
        /// Even if the search polygon overlaps polyC, it will not
        /// be included in the result set unless polyB is also in the set.
        /// </p>
        /// <p>The 3D centroid of the polygon is used as the start position for
        /// cost calculations.</p>
        /// <p>Intersection tests occur in 2D with all polygons projected
        /// onto the xz-plane.  So the y-values of the vertices
        /// do not effect intersection tests.</p>
        /// </remarks>
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
        public NavStatus FindPolys(uint startPolyRef
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

            return NavmeshQueryEx.FindPolys(root
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
        /// <p>This method is optimized for a small query radius and small number
        /// of polygons.</p>
        /// <p>The order of the result set is from least to highest
        /// cost.</p>
        /// <p>At least one result buffer must be provided.</p>
        /// <p>The return status will include the 
        /// <see cref="NavStatus.BufferToSmall"/> flag if the provided buffers 
        /// are too small to hold the entire result set. (The operation will not
        /// fail.)</p>
        /// <p>The primary use case for this method is for performing
        /// Dijkstra searches.  This is because candidate polygons are found
        /// by searching the graph beginning at the start polygon.  If a 
        /// navmesh polygon is not found via the graph search,
        /// even if it intersects the search circle, it will not be included
        /// in the result set. Example scenario:</p>
        /// <p>polyA is the start polygon.<br/>
        /// polyB shares an edge with polyA. (Is adjacent.)<br/>
        /// polyC shares an edge with polyB, but not with polyA<br/>
        /// Even if the search circle overlaps polyC, it will not
        /// be included in the result set unless polyB is also in the set.
        /// </p>
        /// <p>The value of the position argument is used as the start point 
        /// for cost calculations.  It is not projected onto the surface of the
        /// mesh, so its y-value will effect the costs.</p>
        /// <p>Intersection tests occur in 2D with all polygons and the
        /// search circle projected onto the xz-plane.  So the y-value of the
        /// position argument does not effect intersection tests.</p>
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
        public NavStatus GetPolysLocal(uint startPolyRef
                , float[] centerPoint
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

            return NavmeshQueryEx.GetPolysLocal(root
                , startPolyRef
                , centerPoint
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
        /// </p>
        /// <p>The source point does not have to be within the bounds of the
        /// navigation mesh.</p>
        /// </remarks>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="sourcePoint">The position to search from in the form
        /// (x, y, z).</param>
        /// <param name="resultPoint">The closest point found in the form 
        /// (x, y, z) (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPoint(uint polyRef
            , float[] sourcePoint
            , float[] resultPoint)
        {
            return NavmeshQueryEx.GetNearestPoint(root
                , polyRef
                , sourcePoint
                , resultPoint);
        }

        /// <summary>
        /// Returns a point on the boundary closest to the source point if the
        /// source point is is outside the polygon's xz-column.
        /// </summary>
        /// <remarks>
        /// <p>Much faster than <see cref="GetNearestPoint" />.</p>
        /// <p>If the provided point lies within the polygon's xz-column
        /// (above or below), then the source and result points will be equal
        /// </p>
        /// <p>The boundary point will be the polygon boundary, not the height
        /// corrected detail boundary.  Use <see cref="GetPolyHeight"/> if 
        /// needed.
        /// </p>
        /// <p>The source point does not have to be within the bounds of the
        /// navigation mesh.</p>
        /// </remarks>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="sourcePoint.">The point to check in the form (x, y, z).
        /// </param>
        /// <param name="resultPoint">The closest point. (Out)</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPointF(uint polyRef
            , float[] sourcePoint
            , float[] resultPoint)
        {
            return NavmeshQueryEx.GetNearestPointF(root
                , polyRef
                , sourcePoint
                , resultPoint);
        }

        /// <summary>
        /// Gets the height of the polygon at the provided point using the
        /// detail mesh. (Most accurate.)
        /// </summary>
        /// <remarks>The method will return falure if the provided point is
        /// outside the xz-column of the polygon.</remarks>
        /// <param name="polyRef">The polygon reference.</param>
        /// <param name="point">The point within the polygon's xz-column.
        /// </param>
        /// <param name="height">The height at the surface of the polygon.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolyHeight(uint polyRef
            , float[] point
            , out float height)
        {
            height = 0;
            return NavmeshQueryEx.GetPolyHeight(root
                , polyRef
                , point
                , ref height);
        }

        /// <summary>
        /// Returns the distance from the specified position to the nearest
        /// polygon wall.
        /// </summary>
        /// <remarks>
        /// <p>The closest point will be on the polygon wall.  It is not 
        /// height adjusted using the detail data. Use 
        /// <see cref="GetPolyHeight"/> if needed.</p>
        /// <p>The distance will equal the search radius if there is no wall 
        /// within the search radius.  In this case the values of closestPoint 
        /// and normal are undefined.</p>
        /// <p>The normal will become unpredicable if the distance is a
        /// very small number.</p>
        /// <p>Remember: A "wall" is considered a solid bounday.</p>
        /// </remarks>
        /// <param name="polyRef">The id of the polygon.</param>
        /// <param name="position">The center of the search query circle.
        /// </param>
        /// <param name="searchRadius">The radius of the query circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="distance">Distance to nearest wall.</param>
        /// <param name="closestPoint">The nearest point on the wall.</param>
        /// <param name="normal">The normal of the ray formed by the
        /// position and the closest wall point.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindDistanceToWall(uint polyRef
            , float[] sourcePoint
            , float searchRadius
            , NavmeshQueryFilter filter
            , out float distance
            , float[] closestPoint
            , float[] normal)
        {
            distance = 0;
            return NavmeshQueryEx.FindDistanceToWall(root
                , polyRef
                , sourcePoint
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
        /// <p>The start and end points are used to calculate
        /// traversal costs. (y-values matter.)</p>
        /// </remarks>
        /// <param name="startPolyRef">The id of the start polygon.</param>
        /// <param name="endPolyRef">The id of the end polygon.</param>
        /// <param name="startPoint">A position within the start polygon.
        /// </param>
        /// <param name="endPoint">A position within the end polygon.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath">An ordered list of polygon ids in the
        /// path. (Start to end.)</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <param name="maxPath">The maximum length of the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPath(uint startPolyRef
            , uint endPolyRef
            , float[] startPoint
            , float[] endPoint
            , NavmeshQueryFilter filter
            , uint[] resultPath
            , out int pathCount)
        {
            pathCount = 0;
            return NavmeshQueryEx.FindPath(root
                , startPolyRef
                , endPolyRef
                , startPoint
                , endPoint
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
            , float[] startPoint
            , float[] endPoint
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
                , startPoint
                , endPoint
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
        /// <p>This method peforms what is often called 'string pulling'.</p>
        /// <p>The start point is clamped to the first polygon in 
        /// the path, and the end point is clamped to the last polygon in the 
        /// path. So the start and end points should be within or very near
        /// the first and last polygons respectively.  The pathStart and
        /// pathCount parameters can be adjusted to restrict the usable portion
        /// of the the path to meet this requirement. (See the example use
        /// case below.)</p>
        /// <p>The returned polygon references represent the id of the polygon
        /// that is entered at the associated path point.  The id associated
        /// to end point will always be zero.</p>
        /// <p>Example use case for adjusting the straight path during
        /// locomotion:</p>
        /// <p>Senario: The path consists of polygons A, B, C, D, with the
        /// start point in A and the end point in D.</p>
        /// <p>The first call to the method will return straight waypoints for 
        /// the entire path:<br/>
        /// <code>
        /// query.GetStraightPath(startPoint, endPoint
        ///     , path
        ///     , 0, 4   // pathStart, pathCount
        ///     , straigthPath, null, null
        ///     , out straightCount);
        /// </code>
        /// </p>
        /// <p>If the agent moves into polygon B and needs to recaclulate its
        /// straight path for some reason, it can call the method as
        /// follows using the same path:<br/>
        /// <code>
        /// query.GetStraightPath(startPoint, endPoint
        ///     , path
        ///     , 1, 3   // pathStart, pathCount  &lt;- Note the changes here.
        ///     , straigthPath, null, null
        ///     , out straightCount);
        /// </code></p>
        /// </remarks>
        /// <param name="startPosition">The start position.</param>
        /// <param name="endPosition">The end position.</param>
        /// <param name="path">The list of polygon ids that represent the
        /// path corridor.</param>
        /// <param name="pathCount">The length of the path within the path
        /// buffer.</param>
        /// <param name="straightPathPoints">Points describing the straight
        /// path in the form (x, y, z).</param>
        /// <param name="straightPathFlags">Flags describing each point.
        /// (Optional)</param>
        /// <param name="straightPathRefs">The id of the polygon that
        /// is being entered at the point position. (Optional)</param>
        /// <param name="straightPathCount">The number of points in the
        /// straight path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetStraightPath(float[] startPoint
            , float[] endPoint
            , uint[] path
            , int pathStart
            , int pathCount
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
                , startPoint
                , endPoint
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
        /// <p>The result point will equal the end point if the end
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
            , float[] startPoint
            , float[] endPoint
            , NavmeshQueryFilter filter
            , float[] resultPoint
            , uint[] visitedPolyRefs
            , out int visitedCount)
        {
            visitedCount = 0;
            return NavmeshQueryEx.MoveAlongSurface(root
                , startPolyRef
                , startPoint
                , endPoint
                , filter.root
                , resultPoint
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
            , float[] startPoint
            , float[] endPoint
            , NavmeshQueryFilter filter)
        {
            if (mIsRestricted)
                return NavStatus.Failure;
            return NavmeshQueryEx.InitSlicedFindPath(root
                , startPolyRef
                , endPolyRef
                , startPoint
                , endPoint
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
