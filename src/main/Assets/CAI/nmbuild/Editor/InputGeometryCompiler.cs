/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using org.critterai.geom;
using System.Collections.Generic;
using org.critterai.nmgen;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    public class InputGeometryCompiler
    {
        private readonly List<Vector3> mVerts;
        private readonly List<int> mTris;
        private readonly List<byte> mAreas;

        public int VertCount { get { return mVerts.Count; } }
        public int TriCount { get { return mTris.Count / 3; } }

        public InputGeometryCompiler(int initVertCount, int initTriCount)
        {
            mVerts = new List<Vector3>(initVertCount);
            mTris = new List<int>(initTriCount * 3);
            mAreas = new List<byte>(initTriCount);
        }

        public void AddTriangle(Vector3 vertA, Vector3 vertB, Vector3 vertC, byte area)
        {
            mTris.Add(mVerts.Count);
            mVerts.Add(vertA);

            mTris.Add(mVerts.Count);
            mVerts.Add(vertB);

            mTris.Add(mVerts.Count);
            mVerts.Add(vertC);

            mAreas.Add(area);
        }

        //public bool AddTriangles(List<Vector3> verts
        //    , int vertCount
        //    , List<int> tris
        //    , List<byte> areas
        //    , int triCount)
        //{
        //    if (verts == null || tris == null)
        //        return false;

        //    return AddTriangles(verts.ToArray(), vertCount
        //        , tris.ToArray(), (areas == null ? null : areas.ToArray()), triCount);
        //}

        public bool AddTriangles(Vector3[] verts, int vertCount, int[] tris, byte[] areas, int triCount)
        {
            if (triCount < 1 || vertCount < 3
                || verts == null || verts.Length < vertCount
                || tris == null || tris.Length < triCount * 3
                || areas != null && areas.Length < triCount)
            {
                return false;
            }

            if (areas == null)
                areas = NMGen.CreateWalkableAreaBuffer(triCount);

            int iVertOffset = mVerts.Count;

            if (vertCount == verts.Length)
                mVerts.AddRange(verts);
            else
            {
                mVerts.Capacity += vertCount;

                for (int p = 0; p < vertCount; p++)
                {
                    mVerts.Add(verts[p]);
                }
            }

            int length = triCount * 3;

            mTris.Capacity += length;

            for (int p = 0; p < length; p++)
            {
                mTris.Add(tris[p] + iVertOffset);
            }

            if (areas.Length == triCount)
                mAreas.AddRange(areas);
            else
            {
                mAreas.Capacity += triCount;

                for (int i = 0; i < triCount; i++)
                {
                    mAreas.Add(areas[i]);
                }
            }

            return true;
        }


        /// <param name="areas">If null, the area will default to walkable for all triangles.</param>
        public bool AddTriangles(TriangleMesh mesh
            , byte[] areas)
        {
            if (mesh == null)
                return false;

            return AddTriangles(mesh.verts, mesh.vertCount, mesh.tris, areas, mesh.triCount);
        }

        public TriangleMesh GetGeometry(out byte[] areas)
        {
            if (mTris.Count == 0)
            {
                areas = null;
                return null;
            }

            areas = mAreas.ToArray();

            return new TriangleMesh(mVerts.ToArray(), mVerts.Count
                , mTris.ToArray(), mTris.Count / 3);
        }

        public void Reset()
        {
            mTris.Clear();
            mVerts.Clear();
            mAreas.Clear();
        }
    }
}
