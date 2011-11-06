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

namespace org.critterai.nmgen
{
	public class InputGeometryBuilder
	{
        private readonly List<float> mVerts;
        private readonly List<int> mTris;
        private readonly List<byte> mAreas;
        private readonly List<IAreaMarker> mAreaMarkers;

        public int VertCount { get { return mVerts.Count / 3; } }
        public int TriCount { get { return mTris.Count / 3; } }
        public int AreaMarkerCount { get { return mAreaMarkers.Count; } }

        public InputGeometryBuilder(int initVertCount
            , int initTriCount
            , int initAreaMarkerCount)
        {
            mVerts = new List<float>(initVertCount * 3);
            mTris = new List<int>(initTriCount * 3);
            mAreas = new List<byte>(initTriCount);
            mAreaMarkers = new List<IAreaMarker>(initAreaMarkerCount);
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

            int iVertOffset = mVerts.Count / 3;

            int length;

            if (mesh.vertCount * 3 == mesh.verts.Length)
                mVerts.AddRange(mesh.verts);
            else
            {
                length = mesh.vertCount * 3;

                mVerts.Capacity += length;

                for (int p = 0; p < length; p++)
                {
                    mVerts.Add(mesh.verts[p]);
                }
            }

            length = mesh.triCount * 3;

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

        public void AddAreaMarkers(params IAreaMarker[] markers)
        {
            mAreaMarkers.AddRange(markers);
        }

        public void AddAreaMarker(IAreaMarker marker)
        {
            mAreaMarkers.Add(marker);
        }

        public InputGeometry GetGeometry()
        {
            InputGeometry result = new InputGeometry();

            result.mesh = new TriangleMesh(mVerts.ToArray()
                , mVerts.Count / 3
                , mTris.ToArray()
                , mTris.Count / 3);

            result.areas = mAreas.ToArray();
            result.areaMarkers = mAreaMarkers.ToArray();

            result.DeriveBounds();

            return result;
        }

        public void Reset()
        {
            mTris.Clear();
            mVerts.Clear();
            mAreas.Clear();
            mAreaMarkers.Clear();
        }
    }
}
