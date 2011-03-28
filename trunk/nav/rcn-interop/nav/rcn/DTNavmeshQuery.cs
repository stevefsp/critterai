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
    public sealed class DTNavmeshQuery
        : IDisposable
    {
        internal IntPtr root;

        internal DTNavmeshQuery(IntPtr query)
        {
            root = query;
        }

        ~DTNavmeshQuery()
        {
            Dispose();
        }

        public bool IsDisposed()
        {
            return (root == IntPtr.Zero);
        }

        public void Dispose()
        {
            if (root != IntPtr.Zero)
            {
                DTNavmeshQueryEx.Free(ref root);
                root = IntPtr.Zero;
            }
        }

        public DTStatus GetPolyWallSegments(uint polyId
            , DTQueryFilter filter
            , float[] resultSegments
            , ref int segmentCount)
        {
            return (DTStatus)DTNavmeshQueryEx.GetPolyWallSegments(root
                , polyId
                , filter.root
                , resultSegments
                , ref segmentCount
                , resultSegments.Length / 2);
        }

        public DTStatus GetNearestPoly(float[] position
            , float[] extents
            , DTQueryFilter filter
            , ref uint resultPolyId
            , float[] resultNearestPoint)
        {
            return (DTStatus)DTNavmeshQueryEx.GetNearestPoly(root
                , position
                , extents
                , filter.root
                , ref resultPolyId
                , resultNearestPoint);
        }

        public DTStatus GetPolygons(float[] position
            , float[] extents
            , DTQueryFilter filter
            , uint[] resultPolyIds
            , ref int resultCount)
        {
            return (DTStatus)DTNavmeshQueryEx.GetPolygons(root
                , position
                , extents
                , filter.root
                , resultPolyIds
                , ref resultCount
                , resultPolyIds.Length);
        }

        public DTStatus FindPolygons(uint startPolyId
                , float[] position
                , float radius
                , DTQueryFilter filter
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
                return (DTStatus.Failure | DTStatus.InvalidParam);

            return (DTStatus)DTNavmeshQueryEx.FindPolygons(root
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

        public DTStatus FindPolygons(uint startPolyId
                , float[] vertices
                , DTQueryFilter filter
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
                return (DTStatus.Failure | DTStatus.InvalidParam);

            return (DTStatus)DTNavmeshQueryEx.FindPolygons(root
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

        public DTStatus GetPolygonsLocal(uint startPolyId
                , float[] position
                , float radius
                , DTQueryFilter filter
                , uint[] resultPolyIds   // Optional, must have one.
                , uint[] resultParentIds // Optional
                , ref int resultCount)
        {
            // Set max count to the smallest length.
            int maxCount = (resultPolyIds == null ? 0 : resultPolyIds.Length);
            maxCount = (resultParentIds == null ? maxCount
                : Math.Min(maxCount, resultParentIds.Length));

            if (maxCount == 0)
                return (DTStatus.Failure | DTStatus.InvalidParam);

            return (DTStatus)DTNavmeshQueryEx.GetPolygonsLocal(root
                , startPolyId
                , position
                , radius
                , filter.root
                , resultPolyIds
                , resultParentIds
                , ref resultCount
                , maxCount);
        }

        public DTStatus GetNearestPoint(uint polyId
            , float[] position
            , float[] resultPoint)
        {
            return (DTStatus)DTNavmeshQueryEx.GetNearestPoint(root
                , polyId
                , position
                , resultPoint);
        }

        public DTStatus GetNearestBoundaryPoint(uint polyId
            , float[] position
            , float[] resultPoint)
        {
            return (DTStatus)DTNavmeshQueryEx.GetNearestBoundaryPoint(root
                , polyId
                , position
                , resultPoint);
        }

        public DTStatus GetPolyHeight(uint polyId
            , float[] position
            , ref float height)
        {
            return (DTStatus)DTNavmeshQueryEx.GetPolyHeight(root
                , polyId
                , position
                , ref height);
        }

        public DTStatus FindDistanceToWall(uint polyId
            , float[] position
            , float searchRadius
            , DTQueryFilter filter
            , ref float hitDistance
            , float[] hitPosition
            , float[] hitNormal)
        {
            return (DTStatus)DTNavmeshQueryEx.FindDistanceToWall(root
                , polyId
                , position
                , searchRadius
                , filter.root
                , ref hitDistance
                , hitPosition
                , hitNormal);
        }

        public DTStatus FindPath(uint startPolyId
            , uint endPolyId
            , float[] startPosition
            , float[] endPosition
            , DTQueryFilter filter
            , uint[] resultPath
            , ref int pathCount)
        {
            return (DTStatus)DTNavmeshQueryEx.FindPath(root
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
            return DTNavmeshQueryEx.IsInClosedList(root, polyId);
        }

        public DTStatus Raycast(uint startPolyId
            , float[] startPosition
            , float[] endPosition
            , DTQueryFilter filter
            , ref float hitParameter  // Very high number (> 1E38) if no hit.
            , float[] hitNormal
            , uint[] path
            , ref int pathCount)
        {
            int maxCount = (path == null ? 0 : path.Length);

            return (DTStatus)DTNavmeshQueryEx.Raycast(root
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

        public DTStatus GetStraightPath(float[] startPosition
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
                return (DTStatus.Failure | DTStatus.InvalidParam);

            return (DTStatus)DTNavmeshQueryEx.GetStraightPath(root
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

        public DTStatus MoveAlongSurface(uint startPolyId
            , float[] startPosition
            , float[] endPosition
            , DTQueryFilter filter
            , float[] resultPosition
            , uint[] visitedPolyIds
            , ref int visitedCount)
        {
            return (DTStatus)DTNavmeshQueryEx.MoveAlongSurface(root
                , startPolyId
                , startPosition
                , endPosition
                , filter.root
                , resultPosition
                , visitedPolyIds
                , ref visitedCount
                , visitedPolyIds.Length);
        }

        public DTStatus InitSlicedFindPath(uint startPolyId
            , uint endPolyId
            , float[] startPosition
            , float[] endPosition
            , DTQueryFilter filter)
        {
            return (DTStatus)DTNavmeshQueryEx.InitSlicedFindPath(root
                , startPolyId
                , endPolyId
                , startPosition
                , endPosition
                , filter.root);
        }

        public DTStatus UpdateSlicedFindPath(int maxIterations
            , ref int actualIterations)
        {
            return (DTStatus)DTNavmeshQueryEx.UpdateSlicedFindPath(root
                , maxIterations
                , ref actualIterations);
        }

        public DTStatus FinalizeSlicedFindPath(uint[] path
            , ref int pathCount)
        {
            return (DTStatus)DTNavmeshQueryEx.FinalizeSlicedFindPath(root
                , path
                , ref pathCount
                , path.Length);
        }
    }
}
