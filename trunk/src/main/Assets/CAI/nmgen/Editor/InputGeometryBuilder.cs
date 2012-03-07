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
using org.critterai.geom;
using System.Collections.Generic;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    public class InputGeometryBuilder
    {
        private readonly List<Vector3> mVerts;
        private readonly List<int> mTris;
        private readonly List<byte> mAreas;

        public int VertCount { get { return mVerts.Count; } }
        public int TriCount { get { return mTris.Count / 3; } }

        public InputGeometryBuilder(int initVertCount, int initTriCount)
        {
            mVerts = new List<Vector3>(initVertCount);
            mTris = new List<int>(initTriCount * 3);
            mAreas = new List<byte>(initTriCount);
        }

        public InputGeometryBuilder(InputGeometry source)
        {
            mVerts = new List<Vector3>(source.UnsafeVerts);
            mTris = new List<int>(source.UnsafeTris);
            mAreas = new List<byte>(source.UnsafeAreas);
        }

        public bool AddTriangles(TriangleMesh mesh
            , byte[] areas)
        {
            if (mesh == null
                || mesh.triCount == 0
                || !TriangleMesh.Validate(mesh, false)
                || areas != null && areas.Length < mesh.triCount)
            {
                return false;
            }

            if (areas == null)
                areas = NMGen.BuildWalkableAreaBuffer(mesh.triCount);

            int iVertOffset = mVerts.Count;

            if (mesh.vertCount == mesh.verts.Length)
                mVerts.AddRange(mesh.verts);
            else
            {
                mVerts.Capacity += mesh.vertCount;

                for (int p = 0; p < mesh.vertCount; p++)
                {
                    mVerts.Add(mesh.verts[p]);
                }
            }

            int length = mesh.triCount * 3;

            mTris.Capacity += length;

            for (int p = 0; p < length; p++)
            {
                mTris.Add(mesh.tris[p] + iVertOffset);
            }

            if (areas.Length == mesh.triCount)
                mAreas.AddRange(areas);
            else
            {
                mAreas.Capacity += mesh.triCount;

                for (int i = 0; i < mesh.triCount; i++)
                {
                    mAreas.Add(areas[i]);
                }
            }

            return true;
        }

        public InputGeometry GetGeometry()
        {
            if (mTris.Count == 0)
                return null;

            return InputGeometry.Create(mVerts.ToArray(), mVerts.Count
                , mTris.ToArray()
                , mAreas.ToArray()
                , mAreas.Count);
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
