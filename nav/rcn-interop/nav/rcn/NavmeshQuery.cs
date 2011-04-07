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
    public sealed class NavmeshQuery
        : ManagedObject
    {
        internal IntPtr root;

        private NavmeshQueryLite mLite;

        internal NavmeshQuery(IntPtr query, AllocType type)
            : base(type)
        {
            root = query;
            mLite = new NavmeshQueryLite(this);
        }

        ~NavmeshQuery()
        {
            RequestDisposal();
        }

        public override void RequestDisposal()
        {
            if (resourceType == AllocType.External)
                NavmeshQueryEx.FreeEx(ref root);
            root = IntPtr.Zero;
        }

        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        public NavmeshQueryLite LiteQuery
        {
            get { return mLite; }
        }

        public NavmeshStatus GetPolyWallSegments(uint polyId
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

        public NavmeshStatus GetNearestPoly(float[] position
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

        public NavmeshStatus GetPolygons(float[] position
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

        public NavmeshStatus FindPolygons(uint startPolyId
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
                return (NavmeshStatus.Failure | NavmeshStatus.InvalidParam);

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

        public NavmeshStatus FindPolygons(uint startPolyId
                , float[] vertices
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
                return (NavmeshStatus.Failure | NavmeshStatus.InvalidParam);

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

        public NavmeshStatus GetPolygonsLocal(uint startPolyId
                , float[] position
                , float radius
                , NavmeshQueryFilter filter
                , uint[] resultPolyIds   // Optional, must have one.
                , uint[] resultParentIds // Optional
                , ref int resultCount)
        {
            // Set max count to the smallest length.
            int maxCount = (resultPolyIds == null ? 0 : resultPolyIds.Length);
            maxCount = (resultParentIds == null ? maxCount
                : Math.Min(maxCount, resultParentIds.Length));

            if (maxCount == 0)
                return (NavmeshStatus.Failure | NavmeshStatus.InvalidParam);

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

        public NavmeshStatus GetNearestPoint(uint polyId
            , float[] position
            , float[] resultPoint)
        {
            return NavmeshQueryEx.GetNearestPoint(root
                , polyId
                , position
                , resultPoint);
        }

        public NavmeshStatus GetNearestBoundaryPoint(uint polyId
            , float[] position
            , float[] resultPoint)
        {
            return NavmeshQueryEx.GetNearestBoundaryPoint(root
                , polyId
                , position
                , resultPoint);
        }

        public NavmeshStatus GetPolyHeight(uint polyId
            , float[] position
            , ref float height)
        {
            return NavmeshQueryEx.GetPolyHeight(root
                , polyId
                , position
                , ref height);
        }

        public NavmeshStatus FindDistanceToWall(uint polyId
            , float[] position
            , float searchRadius
            , NavmeshQueryFilter filter
            , ref float hitDistance
            , float[] hitPosition
            , float[] hitNormal)
        {
            return NavmeshQueryEx.FindDistanceToWall(root
                , polyId
                , position
                , searchRadius
                , filter.root
                , ref hitDistance
                , hitPosition
                , hitNormal);
        }

        public NavmeshStatus FindPath(uint startPolyId
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

        public bool IsInClosedList(uint polyId)
        {
            return NavmeshQueryEx.IsInClosedList(root, polyId);
        }

        public NavmeshStatus Raycast(uint startPolyId
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

        public NavmeshStatus GetStraightPath(float[] startPosition
            , float[] endPosition
            , uint[] path
            , int pathSize
            , float[] straightPathPoints
            , byte[] straightPathFlags
            , uint[] straightPathIds
            , ref int straightPathCount)
        {
            int maxPath = straightPathPoints.Length / 3;
            maxPath = (straightPathFlags == null ? maxPath
                : Math.Min(straightPathFlags.Length, maxPath));
            maxPath = (straightPathIds == null ? maxPath
                : Math.Min(straightPathIds.Length, maxPath));

            if (maxPath < 1)
                return (NavmeshStatus.Failure | NavmeshStatus.InvalidParam);

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

        public NavmeshStatus MoveAlongSurface(uint startPolyId
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

        public NavmeshStatus InitSlicedFindPath(uint startPolyId
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

        public NavmeshStatus UpdateSlicedFindPath(int maxIterations
            , ref int actualIterations)
        {
            return NavmeshQueryEx.UpdateSlicedFindPath(root
                , maxIterations
                , ref actualIterations);
        }

        public NavmeshStatus FinalizeSlicedFindPath(uint[] path
            , ref int pathCount)
        {
            return NavmeshQueryEx.FinalizeSlicedFindPath(root
                , path
                , ref pathCount
                , path.Length);
        }

        public static NavmeshStatus BuildQuery(Navmesh navmesh
            , int maximumNodes
            , Object owner
            , out NavmeshQuery resultQuery)
        {
            IntPtr query = IntPtr.Zero;

            NavmeshStatus status = NavmeshQueryEx.BuildNavmeshQuery(
                navmesh.root
                , maximumNodes
                , ref query);

            if (NavmeshUtil.Succeeded(status))
                resultQuery = new NavmeshQuery(query
                    , AllocType.External);
            else
                resultQuery = null;

            return status;
        }
    }
}
