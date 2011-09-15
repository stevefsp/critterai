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
using org.critterai.interop;
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
    public class U3DPathCorridor
    {
        // Note: Purposefully not sealed.

        private float[] vbuffA = new float[3];
        private float[] vbuffB = new float[3];

        private PathCorridor mRoot;
        private U3DNavmeshQuery mQuery;

        /// <summary>
        /// The root object being used for querys.
        /// </summary>
        public PathCorridor RootCorridor { get { return mRoot; } }

        public int MaxPathSize { get { return mRoot.MaxPathSize; } }

        public AllocType ResourceType { get { return mRoot.ResourceType; } }

        public bool IsDisposed { get { return mRoot.IsDisposed; } }

        public U3DNavmeshQuery Query
        {
            get { return mQuery; }
            set 
            {
                mQuery = value;
                mRoot.Query = value.RootQuery; 
            }
        }

        public NavmeshQueryFilter Fitler
        {
            get { return mRoot.Fitler; }
            set { mRoot.Fitler = value; }
        }

        public U3DPathCorridor(int maxPathSize
            , U3DNavmeshQuery query
            , NavmeshQueryFilter filter)
        {
            mQuery = query;
            mRoot = new PathCorridor(maxPathSize, mQuery.RootQuery, filter);
        }

        public void RequestDisposal()
        {
            mRoot.RequestDisposal();
        }

        public void Reset(NavmeshPoint position)
        {
            mRoot.Reset(position.polyRef
                , Vector3Util.GetVector(position.point, vbuffA));
        }

        public int FindCorners(float[] cornerVerts
            , WaypointFlag[] cornerFlags
            , uint[] cornerPolys)
        {
            return mRoot.FindCorners(cornerVerts, cornerFlags, cornerPolys);
        }

        public void OptimizePathVisibility(Vector3 next
            , float optimizationRange)
        {
            mRoot.OptimizePathVisibility(Vector3Util.GetVector(next, vbuffA)
                , optimizationRange);
        }

        public void OptimizePathTopology()
        {
            mRoot.OptimizePathTopology();
        }

        public bool MoveOverConnection(uint connectionRef
            , uint[] endpointRefs
            , out Vector3 startPosition
            , out Vector3 endPosition)
        {
            ResetVBuffA();
            ResetVBuffB();

            bool result = mRoot.MoveOverConnection(connectionRef
                , endpointRefs
                , vbuffA
                , vbuffB);

            startPosition = Vector3Util.GetVector(vbuffA);
            endPosition = Vector3Util.GetVector(vbuffB);

            return result;
        }

        public Vector3 MovePosition(Vector3 desiredPosition)
        {
            ResetVBuffB();
            mRoot.MovePosition(Vector3Util.GetVector(desiredPosition, vbuffA)
                , vbuffB);
            return Vector3Util.GetVector(vbuffB);
        }

        public Vector3 MoveTarget(Vector3 desiredPosition)
        {
            ResetVBuffB();
            mRoot.MoveTarget(Vector3Util.GetVector(desiredPosition, vbuffA)
                , vbuffB);
            return Vector3Util.GetVector(vbuffB);
        }

        public void SetCorridor(Vector3 target
            , uint[] path
            , int pathCount)
        {
            mRoot.SetCorridor(Vector3Util.GetVector(target, vbuffA)
                , path
                , pathCount);
        }

        public Vector3 GetPosition()
        {
            return Vector3Util.GetVector(mRoot.GetPosition(vbuffA));
        }

        public Vector3 GetTarget()
        {
            return Vector3Util.GetVector(mRoot.GetTarget(vbuffA));
        }

        public uint GetFirstPoly()
        {
            return mRoot.GetFirstPoly();
        }

        public int GetPath(uint[] buffer)
        {
            return mRoot.GetPath(buffer);
        }

        public int GetPathCount()
        {
            return mRoot.GetPathCount();
        }

        public bool GetData(PathCorridorData buffer)
        {
            return GetData(buffer);
        }

        private void ResetVBuffA()
        {
            vbuffA[0] = 0;
            vbuffA[1] = 0;
            vbuffA[2] = 0;
        }

        private void ResetVBuffB()
        {
            vbuffB[0] = 0;
            vbuffB[1] = 0;
            vbuffB[2] = 0;
        }
    }
}
