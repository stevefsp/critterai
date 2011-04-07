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
using System.Collections.Generic;
using System.Text;

namespace org.critterai.nav.rcn
{
    public sealed class NavmeshQueryLite
    {
        private NavmeshQuery mRoot;

        public NavmeshQueryLite(NavmeshQuery rootQuery)
        {
            mRoot = rootQuery;
        }

        public bool IsDisposed
        {
            get { return mRoot.IsDisposed; }
        }

        public NavmeshStatus GetPolyWallSegments(uint polyId
            , NavmeshQueryFilter filter
            , float[] resultSegments
            , uint[] segmentPolyIds
            , ref int segmentCount)
        {
            return mRoot.GetPolyWallSegments(polyId
                , filter
                , resultSegments
                , segmentPolyIds
                , ref segmentCount);
        }

        public NavmeshStatus GetNearestPoly(float[] position
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

        public NavmeshStatus GetPolygons(float[] position
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

        public NavmeshStatus FindPolygons(uint startPolyId
                , float[] position
                , float radius
                , NavmeshQueryFilter filter
                , uint[] resultPolyIds   // Optional, must have one.
                , uint[] resultParentIds // Optional
                , float[] resultCosts  // Optional
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

        public NavmeshStatus FindPolygons(uint startPolyId
                , float[] vertices
                , NavmeshQueryFilter filter
                , uint[] resultPolyIds   // Optional, must have one.
                , uint[] resultParentIds // Optional
                , float[] resultCosts  // Optional
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

        public NavmeshStatus GetPolygonsLocal(uint startPolyId
                , float[] position
                , float radius
                , NavmeshQueryFilter filter
                , uint[] resultPolyIds   // Optional, must have one.
                , uint[] resultParentIds // Optional
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

        public NavmeshStatus GetNearestPoint(uint polyId
            , float[] position
            , float[] resultPoint)
        {
            return mRoot.GetNearestPoint(polyId
                , position
                , resultPoint);
        }

        public NavmeshStatus GetNearestBoundaryPoint(uint polyId
            , float[] position
            , float[] resultPoint)
        {
            return mRoot.GetNearestBoundaryPoint(polyId
                , position
                , resultPoint);
        }

        public NavmeshStatus GetPolyHeight(uint polyId
            , float[] position
            , ref float height)
        {
            return mRoot.GetPolyHeight(polyId
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
            return mRoot.FindDistanceToWall(polyId
                , position
                , searchRadius
                , filter
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
            return mRoot.FindPath(startPolyId
                , endPolyId
                , startPosition
                , endPosition
                , filter
                , resultPath
                , ref pathCount);
        }

        public bool IsInClosedList(uint polyId)
        {
            return mRoot.IsInClosedList(polyId);
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
            return mRoot.Raycast(startPolyId
                , startPosition
                , endPosition
                , filter
                , ref hitParameter
                , hitNormal
                , path
                , ref pathCount);
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
            return mRoot.GetStraightPath(startPosition
                , endPosition
                , path
                , pathSize
                , straightPathPoints
                , straightPathFlags
                , straightPathIds
                , ref straightPathCount);
        }

        public NavmeshStatus MoveAlongSurface(uint startPolyId
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
