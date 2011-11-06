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
using org.critterai.geom;

namespace org.critterai.nmgen
{
	public struct InputGeometry
	{
        public TriangleMesh mesh;
        public byte[] areas;
        public IAreaMarker[] areaMarkers;
        public float[] boundsMin;
        public float[] boundsMax;

        public void Reset()
        {
            mesh = null;
            areas = null;
            areaMarkers = null;
            boundsMin = null;
            boundsMax = null;
        }

        public void DeriveBounds()
        {
            if (boundsMin == null)
                boundsMin = new float[3];
            if (boundsMax == null)
                boundsMax = new float[3];

            // Only basic tests.  Let the rest cause an exception.
            if (mesh == null  || mesh.vertCount == 0)
                return;

            boundsMin[0] = mesh.verts[0];
            boundsMin[1] = mesh.verts[1];
            boundsMin[2] = mesh.verts[2];

            boundsMax[0] = mesh.verts[0];
            boundsMax[1] = mesh.verts[1];
            boundsMax[2] = mesh.verts[2];

            int length = mesh.vertCount * 3;
            for (int p = 3; p < length; p += 3)
            {
                boundsMin[0] = Math.Min(boundsMin[0], mesh.verts[p + 0]);
                boundsMin[1] = Math.Min(boundsMin[1], mesh.verts[p + 1]);
                boundsMin[2] = Math.Min(boundsMin[2], mesh.verts[p + 2]);

                boundsMax[0] = Math.Max(boundsMax[0], mesh.verts[p + 0]);
                boundsMax[1] = Math.Max(boundsMax[1], mesh.verts[p + 1]);
                boundsMax[2] = Math.Max(boundsMax[2], mesh.verts[p + 2]);
            }
        }
    }
}
