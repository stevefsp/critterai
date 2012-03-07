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
using System;
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
    /// <summary>
    /// Represents NMGen input geometry that has been validated and
    /// is ready to be used for a build.
    /// </summary>
	public sealed class InputGeometry
	{
        /*
         * Design notes:
         * 
         * This class has three main purposes:
         * 
         * It provides a single point for validation of the input geometry.  
         * It will only construct if there is input geometry and it is well formed.
         * 
         * It provides a thread friendly package.
         * 
         * It's internal structure can be optimized with minimal or no impact on
         * the public interface.  (It is likely I'll want to move the data
         * into unmanageed memory at some point.)
         * 
         * Due to peformance/memory concerns the internal data is protected by scoping 
         * access as internal. The issue with this design is the internal 
         * scope is ineffective for source distributions, such as in Unity. 
         * That is the reason  for the naming conventions.  They provide a 
         * warning to users.
         * 
         */

        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;

        private readonly ChunkyTriMesh mMesh;

        public Vector3 BoundsMin { get { return mBoundsMin; } }
        public Vector3 BoundsMax { get { return mBoundsMax; } }
        public int TriCount { get { return mMesh.TriCount; } }

        internal ChunkyTriMesh Mesh { get { return mMesh; } }

        internal InputGeometry(ChunkyTriMesh mesh, Vector3 bmin, Vector3 bmax)
        {
            mBoundsMin = bmin;
            mBoundsMax = bmax;
            mMesh = mesh;
        }

        public int ExtractMesh(out Vector3[] verts, out int[] tris, out byte[] areas)
        {
            return mMesh.ExtractMesh(out verts, out tris, out areas);
        }

        public int ExtractMesh(float xmin, float zmin, float xmax, float zmax
            , out Vector3[] verts, out int[] tris, out byte[] areas)
        {
            int[] ltris;
            byte[] lareas;

            mMesh.ExtractMesh(out verts, out ltris, out lareas);

            List<ChunkyTriMeshNode> nodes = new List<ChunkyTriMeshNode>();

            int triCount = mMesh.GetChunks(xmin, zmin, xmax, zmax, nodes);

            if (triCount == 0)
            {
                verts = null;
                tris = null;
                areas = null;
                return 0;
            }

            tris = new int[triCount * 3];
            areas = new byte[triCount];

            int i = 0;
            foreach (ChunkyTriMeshNode node in nodes)
            {
                for (int j = 0; j < node.count; j++, i++)
                {
                    tris[i * 3 + 0] = ltris[(node.i + j) * 3 + 0];
                    tris[i * 3 + 1] = ltris[(node.i + j) * 3 + 1];
                    tris[i * 3 + 2] = ltris[(node.i + j) * 3 + 2];
                    areas[i] = lareas[node.i + j];
                }
            }

            return triCount;
        }
    }
}
