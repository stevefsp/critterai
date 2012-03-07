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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using org.critterai.interop;
using org.critterai.nmgen.rcn;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
	public sealed class ChunkyTriMesh
        : IManagedObject
	{
        internal IntPtr verts;
        internal IntPtr tris;
        internal IntPtr areas;

        private ChunkyTriMeshNode[] mNodes;
        private readonly int mNodeCount;
        private readonly int mTriCount;
        private readonly int mVertCount;

        public int NodeCount { get { return mNodeCount; } }
        public int TriCount { get { return mTriCount; } }

        internal ChunkyTriMesh(Vector3[] verts
            , int vertCount
            , int[] tris
            , byte[] areas
            , int triCount
            , ChunkyTriMeshNode[] nodes
            , int nodeCount)
        {
            int size = sizeof(float) * vertCount * 3;
            this.verts = UtilEx.GetBuffer(size, false);
            float[] fverts = Vector3Util.Flatten(verts, vertCount); // Bleh.
            Marshal.Copy(fverts, 0, this.verts, vertCount * 3);

            size = sizeof(int) * tris.Length;
            this.tris = UtilEx.GetBuffer(size, false);
            Marshal.Copy(tris, 0, this.tris, tris.Length);

            size = sizeof(byte) * areas.Length;
            this.areas = UtilEx.GetBuffer(size, false);
            Marshal.Copy(areas, 0, this.areas, areas.Length);

            mTriCount = triCount;
            mVertCount = verts.Length;
            mNodes = nodes;
            mNodeCount = nodeCount;
        }

        public int GetChunks(float xmin, float zmin, float xmax, float zmax
            , List<ChunkyTriMeshNode> resultNodes)
        {
            if (tris == IntPtr.Zero || resultNodes == null)
                return 0;

            resultNodes.Clear();

            int result = 0;
            int i = 0;
            while (i < mNodeCount)
            {
                ChunkyTriMeshNode node = mNodes[i];

                bool overlap = node.Overlaps(xmin, zmin, xmax, zmax);

                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    resultNodes.Add(node);
                    result += node.count;
                }

                if (overlap || isLeafNode)
                    i++;
                else
                {
                    int escapeIndex = -node.i;
                    i += escapeIndex;
                }
            }

            return result;
        }

        public int ExtractMesh(out Vector3[] verts, out int[] tris, out byte[] areas)
        {
            if (this.tris == IntPtr.Zero)
            {
                verts = null;
                tris = null;
                areas = null;
                return 0;
            }

            float[] lverts = new float[mVertCount * 3];
            Marshal.Copy(this.verts, lverts, 0, mVertCount * 3);
            verts = Vector3Util.GetVectors(lverts);

            tris = new int[mTriCount * 3];
            Marshal.Copy(this.tris, tris, 0, mTriCount * 3);

            areas = new byte[mTriCount];
            Marshal.Copy(this.areas, areas, 0, mTriCount);

            return mTriCount;
        }

        public AllocType ResourceType { get { return AllocType.Local; } }
        public bool IsDisposed { get { return (tris == IntPtr.Zero); } }

        public void RequestDisposal()
        {
            if (tris != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(verts);
                Marshal.FreeHGlobal(tris);
                Marshal.FreeHGlobal(areas);
                verts = IntPtr.Zero;
                tris = IntPtr.Zero;
                areas = IntPtr.Zero;
            }
        }
    }
}
