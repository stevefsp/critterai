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
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn.externs
{
    /// <summary>
    /// Contains triangle meshes which represent detailed height data 
    /// associated with the polygons in a RCPolyMeshEx.
    /// </summary>
    /// <remarks>
    /// <p>The detail mesh is made up of sub-meshes which represent
    /// detailed triangulations of each polygon in its assoicated polygon
    /// mesh.  The sub-meshes share the same index as the polygons in the
    /// polygon mesh.  E.g. Polygon 5 in the polygon mesh is assoicated with
    /// sub-mesh 5 in the detail mesh.
    /// </p>
    /// <p>Each sub-mesh will, as a mimumum, contain the vertices of its
    /// associated polygon.  So it's xz-plane projection will be the same
    /// as its associated polygon.  But extra edge and internal vertices may be 
    /// added to increase height detail.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct PolyMeshDetailEx
    {
        private IntPtr mMeshes;	// Pointer to all mesh data.
        private IntPtr mVertices;			// Pointer to all vertex data.
        private IntPtr mTriangles;	// Pointer to all triangle data.
        private int mMeshCount;			// Number of meshes.
        private int mVertexCount;				// Number of total vertices.
        private int mTriangleCount;				// Number of triangles.

        public int MeshCount { get { return mMeshCount; } }
        public int VertexCount { get { return mVertexCount; } }
        public int TriangleCount { get { return mTriangleCount; } }

        public bool IsFullyAllocated
        {
            get
            {
                return !(mMeshes == IntPtr.Zero
                    || mTriangles == IntPtr.Zero
                    || mVertices == IntPtr.Zero);
            }
        }

        public bool HasAllocations
        {
            get
            {
                return (mMeshes != IntPtr.Zero
                    || mTriangles != IntPtr.Zero
                    || mVertices != IntPtr.Zero);
            }
        }

        public PolyMeshDetailEx(float[] vertices
            , byte[] triangles
            , uint[] meshes)
        {
            mMeshCount = (meshes == null ? 0 : meshes.Length / 4);
            mVertexCount = (vertices == null ? 0 : vertices.Length / 3);
            mTriangleCount = (triangles == null ? 0 : triangles.Length / 4);

            if (mMeshCount == 0 || meshes.Length % 4 != 0
                || mVertexCount == 0 || vertices.Length % 3 != 0
                || mTriangleCount == 0 || triangles.Length % 4 != 0)
            {
                mMeshCount = 0;
                mVertexCount = 0;
                mTriangleCount = 0;
                this.mMeshes = IntPtr.Zero;
                this.mVertices = IntPtr.Zero;
                this.mTriangles = IntPtr.Zero;
                return;
            }

            this.mMeshes = UtilEx.GetFilledBuffer(meshes, meshes.Length);
            this.mVertices = UtilEx.GetFilledBuffer(vertices, vertices.Length);
            this.mTriangles = 
                UtilEx.GetFilledBuffer(triangles, triangles.Length);
        }

        /// <summary>
        /// Returns a copy of the sub-mesh array in the form (baseVertexIndex,
        /// vertexCount, baseTriangleIndex, triangleCount). 
        /// </summary>
        /// <remarks>
        /// <p>The maximum number of vertices per sub-mesh is 127.</p>
        /// <p>The maximum number of triangles per sub-mesh is 255.</p>
        /// <p>
        /// An example of iterating the triangles in a sub-mesh. (Where
        /// iMesh is the index for the sub-mesh of the detail mesh
        /// detailMesh.)
        /// </p>
        /// <p>
        /// <pre>
        ///     uint[] meshes = detailMesh.GetMeshes();
        ///     byte[] triangles = detailMesh.GetTriangles();
        ///     float[] vertices = detailMesh.GetVertices();
        /// 
        ///     int pMesh = iMesh * 4;
        ///     int pVertBase = meshes[pMesh + 0] * 3;
        ///     int pTriBase = meshes[pMesh + 2] * 4;
        ///     int tCount = meshes[pMesh + 3];
        ///     int vertX, vertY, vertZ;
        ///
        ///     for (int iTri = 0; iTri < tCount; iTri++)
        ///     {
        ///        for (int iVert = 0; iVert < 3; iVert++)
        ///        {
        ///            int pVert = pVertBase 
        ///                + (triangles[pTriBase + (iTri * 4 + iVert)] * 3);
        ///            vertX = vertices[pVert + 0];
        ///            vertY = vertices[pVert + 1];
        ///            vertZ = vertices[pVert + 2];
        ///            // Do something with the vertex.
        ///        }
        ///    }
        /// </pre>
        /// </p>
        /// </remarks>
        /// <returns></returns>
        public uint[] GetMeshes()
        {
            return (mMeshes == IntPtr.Zero ? null
                : UtilEx.ExtractArrayUInt(mMeshes, mMeshCount * 4));
        }

        /// <summary>
        /// Returns a copy of the triangles array in the form (vertIndexA
        /// , vertIndexB, vertIndexC, flag).
        /// </summary>
        /// <remarks>
        /// <p>The triangles are grouped by sub-mesh. See GetMeshes()
        /// for how to obtain the base triangle and triangle count for
        /// each sub-mesh.</p>
        /// <p><b>Vertices</b></p>
        /// <p> The vertex indices in the triangle array are local to the 
        /// sub-mesh, not global.  To translate into an gobal index in the 
        /// vertices array, it must be offset by the sub-mesh's base vertex 
        /// index.</p>
        /// <p>
        /// Example: If the baseVertexIndex for the sub-mesh is 5 and the
        /// triangle entry is (4, 8, 7, 0), then the actual indices for
        /// the vertices are (5 + 4, 5 + 8, 5 + 7).
        /// </p>
        /// <p><b>Flags</b></p>
        /// <p>The flags entry indicates which edges are internal or external to
        /// the sub-mesh.</p>
        /// 
        /// <p>Internal edges connect to other triangles within the sub-mesh.
        /// External edges represent portals to other sub-meshes or the null 
        /// region.</p>
        /// 
        /// <p>Each flag is stored in a 2-bit position.  Where position 0 is the
        /// lowest 2-bits and position 4 is the highest 2-bits:</p>
        /// 
        /// <p>Position 0: Edge AB (>> 0)<br />
        /// Position 1: Edge BC (>> 2)<br />
        /// Position 2: Edge CA (>> 4)<br />
        /// Position 4: Unused</p>
        /// 
        /// <p>Testing should be performed as follows:</p>
        /// <pre>
        /// if (((flag >> 2) & 0x3) == 0)
        /// {
        ///     // Edge BC is an external edge.
        /// }
        /// </pre>
        /// </remarks>
        /// <returns></returns>
        public byte[] GetTriangles()
        {
            return (mTriangles == IntPtr.Zero ? null
                : UtilEx.ExtractArrayByte(mTriangles, mTriangleCount * 4));
        }

        /// <summary>
        /// The vertices that make up the detail mesh sub-meshes in the form
        /// (x, y, z).
        /// </summary>
        /// <remarks>
        /// <p>The vertices are grouped by sub-mesh and will contain duplicates
        /// for vertices shared by sub-meshes.</p>
        /// <p>The first sub-mesh vertices are in the same order as the 
        /// mesh's associated polygon mesh polygon, followed by any additional
        /// vertices.  So it the associated polygon has 5 vertices, the
        /// sub-mesh will have a minimum of 5 vertices and the first 5 
        /// vertices will be equivalent to the 5 polygon vertices.</p>
        /// <p>See GetMeshes() for how to obtain the base vertex and
        /// vertex count for each sub-mesh.</p>
        /// </remarks>
        /// <returns></returns>
        public float[] GetVertices()
        {
            return (mVertices == IntPtr.Zero ? null
                : UtilEx.ExtractArrayFloat(mVertices, mVertexCount * 3));
        }

        public static PolyMeshDetailEx Empty
        {
            get
            {
                PolyMeshDetailEx result = new PolyMeshDetailEx();
                result.mMeshCount = 0;
                result.mVertexCount = 0;
                result.mTriangleCount = 0;
                result.mMeshes = IntPtr.Zero;
                result.mVertices = IntPtr.Zero;
                result.mTriangles = IntPtr.Zero;
                return result;
            }
        }

        public static void Free(ref PolyMeshDetailEx detailMesh)
        {
            Marshal.FreeHGlobal(detailMesh.mMeshes);
            Marshal.FreeHGlobal(detailMesh.mVertices);
            Marshal.FreeHGlobal(detailMesh.mTriangles);
            detailMesh.mMeshes = IntPtr.Zero;
            detailMesh.mVertices = IntPtr.Zero;
            detailMesh.mTriangles = IntPtr.Zero;
            detailMesh.mMeshCount = 0;
            detailMesh.mVertexCount = 0;
            detailMesh.mTriangleCount = 0;
        }

        [DllImport("cai-nav-rcn", EntryPoint = "freeRCDetailMesh")]
        public static extern void FreeEx(ref PolyMeshDetailEx detailMesh);
    }
}
