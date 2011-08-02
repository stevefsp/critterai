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
    public sealed class PathCorridor
        : IManagedObject
    {
        private IntPtr mRoot;
        private int mMaxPathSize;
        private NavmeshQueryFilter mFilter;
        private NavmeshQuery mQuery;

        public int MaxPathSize { get { return mMaxPathSize; } }

        public AllocType ResourceType { get { return AllocType.External; } }

        public bool IsDisposed { get { return (mRoot == IntPtr.Zero); } }

        public NavmeshQuery Query
        {
            get { return mQuery; }
            set { mQuery = value; }
        }

        public NavmeshQueryFilter Fitler
        {
            get { return mFilter; }
            set { mFilter = value; }
        }

        public PathCorridor(int maxPathSize
            , NavmeshQuery query
            , NavmeshQueryFilter filter)
        {
            maxPathSize = Math.Max(1, maxPathSize);

            mRoot = PathCorridorEx.Alloc(maxPathSize);

            if (mRoot == IntPtr.Zero)
                mMaxPathSize = 0;
            else
                mMaxPathSize = maxPathSize;

            mQuery = query;
            mFilter = filter;
        }

        ~PathCorridor()
        {
            RequestDisposal();
        }

        public void RequestDisposal()
        {
            if (!IsDisposed)
            {
                PathCorridorEx.Free(mRoot);
                mRoot = IntPtr.Zero;
            }
        }

        public void Reset(uint polyRef, float[] position)
        {
            PathCorridorEx.Reset(mRoot, polyRef, position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>Behavior is undefined if the buffer sizes are not based
        /// on the same maximum corner count. E.g. The flag and poly buffers 
        /// are different sizes.</para>
        /// </remarks>
        /// <param name="cornerVerts"></param>
        /// <param name="cornerFlags"></param>
        /// <param name="cornerPolys"></param>
        /// <returns></returns>
        public int FindCorners(float[] cornerVerts
            , WaypointFlag[] cornerFlags
            , uint[] cornerPolys)
        {
            return PathCorridorEx.FindCorners(mRoot
                , cornerVerts
                , cornerFlags
                , cornerPolys
                , cornerPolys.Length
                , mQuery.root
                , mFilter.root);
        }

        public void OptimizePathVisibility(float[] next
            , float optimizationRange)
        {
            PathCorridorEx.OptimizePathVisibility(mRoot
                , next
                , optimizationRange
                , mQuery.root
                , mFilter.root);
        }

        public void OptimizePathTopology()
        {
            PathCorridorEx.OptimizePathTopology(mRoot
                , mQuery.root
                , mFilter.root);
        }

        public bool MoveOverConnection(uint connectionRef
            , uint[] endpointRefs
            , float[] startPosition
            , float[] endPosition)
        {
            return PathCorridorEx.MoveOverConnection(mRoot
                , connectionRef
                , endpointRefs
                , startPosition
                , endPosition
                , mQuery.root);
        }

        public float[] MovePosition(float[] desiredPosition, float[] position)
        {
            PathCorridorEx.MovePosition(mRoot
                , desiredPosition
                , mQuery.root
                , mFilter.root
                , position);
            return position;
        }

        public float[] MoveTarget(float[] desiredPosition, float[] position)
        {
            PathCorridorEx.MoveTargetPosition(mRoot
                , desiredPosition
                , mQuery.root
                , mFilter.root
                , position);
            return position;
        }

        public void SetCorridor(float[] target
            , uint[] path
            , int pathCount)
        {
            PathCorridorEx.SetCorridor(mRoot, target, path, pathCount);
        }

        public float[] GetPosition(float[] position)
        {
            PathCorridorEx.GetPosition(mRoot, position);
            return position;
        }

        public float[] GetTarget(float[] target)
        {
            PathCorridorEx.GetTarget(mRoot, target);
            return target;
        }

        public uint GetFirstPoly()
        {
            return PathCorridorEx.GetFirstPoly(mRoot);
        }

        public int GetPath(uint[] buffer)
        {
            return PathCorridorEx.GetPath(mRoot, buffer, buffer.Length);
        }

        public int GetPathCount()
        {
            return PathCorridorEx.GetPathCount(mRoot);
        }

        public bool GetData(PathCorridorData buffer)
        {
            return PathCorridorEx.GetData(mRoot, buffer);
        }
    }
}
