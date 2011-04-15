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
using org.critterai.nav.rcn.externs;
using System.Runtime.Serialization;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Contains triangle meshes which represent detailed height data 
    /// associated with the polygons in its associated 
    /// <see cref="PolyMesh"/> object.
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
    /// <p>This object is not normally built manually.  Instead it is built
    /// using one of the methods in the <see cref="NMGenUtil"/> class.</p>
    /// <p><b>Unity Note:</b> The serialization method used by this class is not 
    /// compatible with Unity serialization.</p>
    /// <p>Behavior is undefined if an object is used after 
    /// disposal.</p>
    /// </remarks>
    [Serializable]
    public sealed class PolyMeshDetail
        : ManagedObject, ISerializable
    {
        private const string VertKey = "v";
        private const string TrisKey = "t";
        private const string MeshKey = "m";

        /// <summary>
        /// The root detail mesh structure.
        /// </summary>
        internal PolyMeshDetailEx root;
        private bool mIsDisposed = false;

        /// <summary>
        /// Unsafe constructor.
        /// </summary>
        /// <param name="detailMesh">An allocated detail mesh structure.</param>
        /// <param name="resouceType">The resource allocation type of
        /// the polygon mesh structure.</param>
        internal PolyMeshDetail(PolyMeshDetailEx detailMesh
            , AllocType resouceType)
            : base(resouceType)
        {
            root = detailMesh;
        }

        /// <summary>
        /// Manual constructor.
        /// </summary>
        /// <param name="vertices">The mesh vertices. 
        /// (See: <see cref="GetVertices"/>)</param>
        /// <param name="triangles">The mesh triangles.
        /// (See: <see cref="GetTriangles"/>)</param>
        /// <param name="meshes">The sub-mesh information.
        /// (See: <see cref="GetMeshes"/>)</param>
        public PolyMeshDetail(float[] vertices
            , byte[] triangles
            , uint[] meshes)
            : base(AllocType.Local)
        {
            root = new PolyMeshDetailEx(vertices, triangles, meshes);
        }

        private PolyMeshDetail(SerializationInfo info
            , StreamingContext context)
            : base(AllocType.Local)
        {
            if (info.MemberCount != 3)
            {
                root = new PolyMeshDetailEx();
                mIsDisposed = true;
                return;
            }

            float[] vertices =
                (float[])info.GetValue(VertKey, typeof(float[]));
            uint[] meshes =
                (uint[])info.GetValue(MeshKey, typeof(uint[]));
            byte[] triangles = 
                (byte[])info.GetValue(TrisKey, typeof(byte[]));
            
            root = new PolyMeshDetailEx(vertices, triangles, meshes);
        }

        ~PolyMeshDetail()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Request all unmanaged resources controlled by the object be 
        /// immediately freed and the object marked as disposed.
        /// </summary>
        /// <remarks>
        /// If the object was created using a public constructor the
        /// resources will be freed immediately.
        /// </remarks>
        public override void RequestDisposal()
        {
            if (!mIsDisposed)
            {
                if (ResourceType == AllocType.Local)
                    PolyMeshDetailEx.Free(ref root);
                else if (ResourceType == AllocType.External)
                    PolyMeshDetailEx.FreeEx(ref root);
                mIsDisposed = true;
            }
        }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed { get { return mIsDisposed; } }

        /// <summary>
        /// The number of sub-meshes in the detail mesh.
        /// </summary>
        public int MeshCount { get { return root.MeshCount; } }

        /// <summary>
        /// The total number of vertices in the detail mesh.
        /// </summary>
        public int VertexCount { get { return root.VertexCount; } }

        /// <summary>
        /// The total number of triangles in the detail mesh.
        /// </summary>
        public int TriangleCount { get { return root.TriangleCount; } }

        /// <summary>
        /// Returns a copy of the sub-mesh data. (baseVertexIndex,
        /// vertexCount, baseTriangleIndex, triangleCount) * MeshCount 
        /// </summary>
        /// <remarks>
        /// <p>The maximum number of vertices per sub-mesh is 127.</p>
        /// <p>The maximum number of triangles per sub-mesh is 255.</p>
        /// <p>
        /// An example of iterating the triangles in a sub-mesh. (Where
        /// iMesh is the index for the sub-mesh within detailMesh.)
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
        ///     for (int iTri = 0; iTri &lt; tCount; iTri++)
        ///     {
        ///        for (int iVert = 0; iVert &lt; 3; iVert++)
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
        /// <returns>A copy of the sub-mesh data.</returns>
        public uint[] GetMeshes()
        {
            return root.GetMeshes();
        }

        /// <summary>
        /// The vertices that make up each sub-mesh. (x, y, z) * VertexCount.
        /// </summary>
        /// <remarks>
        /// <p>The vertices are grouped by sub-mesh and will contain duplicates
        /// since each sub-mesh is independantly defined.</p>
        /// <p>The first group of vertices for each sub-mesh are in the same 
        /// order as the vertices for the sub-mesh's associated polygon 
        /// mesh polygon.  These vertices are followed by any additional
        /// detail vertices.  So it the associated polygon has 5 vertices, the
        /// sub-mesh will have a minimum of 5 vertices and the first 5 
        /// vertices will be equivalent to the 5 polygon vertices.</p>
        /// <p>See GetMeshes() for how to obtain the base vertex and
        /// vertex count for each sub-mesh.</p>
        /// </remarks>
        /// <returns>The vertices that make up each sub-mesh.</returns>
        public float[] GetVertices()
        {
            return root.GetVertices();
        }

        /// <summary>
        /// Returns a copy of the triangles array. (vertIndexA,
        /// vertIndexB, vertIndexC, flag) * TriangleCount
        /// </summary>
        /// <remarks>
        /// <p>The triangles are grouped by sub-mesh. See GetMeshes()
        /// for how to obtain the base triangle and triangle count for
        /// each sub-mesh.</p>
        /// <p><b>Vertices</b></p>
        /// <p> The vertex indices in the triangle array are local to the 
        /// sub-mesh, not global.  To translate into an global index in the 
        /// vertices array, the values must be offset by the sub-mesh's base 
        /// vertex index.</p>
        /// <p>
        /// Example: If the baseVertexIndex for the sub-mesh is 5 and the
        /// triangle entry is (4, 8, 7, 0), then the actual indices for
        /// the vertices are (4 + 5, 8 + 5, 7 + 5).
        /// </p>
        /// <p><b>Flags</b></p>
        /// <p>The flags entry indicates which edges are internal and which
        /// are external to the sub-mesh.</p>
        /// 
        /// <p>Internal edges connect to other triangles within the same 
        /// sub-mesh.
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
        /// <returns>A copy of the triangles array.</returns>
        public byte[] GetTriangles()
        {
            return root.GetTriangles();
        }

        /// <summary>
        /// Gets a copy of the entire data structure of the mesh.
        /// </summary>
        /// <returns>A copy of the entire data structure of the mesh.
        /// </returns>
        public PolyMeshDetailData GetData()
        {
            PolyMeshDetailData result = new PolyMeshDetailData();

            result.meshCount = MeshCount;
            result.meshes = GetMeshes();
            result.triangleCount = TriangleCount;
            result.triangles = GetTriangles();
            result.vertexCount = VertexCount;
            result.vertices = GetVertices();

            return result;
        }

        /// <summary>
        /// Gets serialization data for the object.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        public void GetObjectData(SerializationInfo info
            , StreamingContext context)
        {
            /*
             * Design Notes:
             * 
             * Default serialization security is OK.
             * 
             * The following fields are not serialized since they can
             * be derived from other data:
             *    vertexCount
             *    triangleCount
             *    meshCount
             */

            if (mIsDisposed)
                return;

            info.AddValue(VertKey, GetVertices());
            info.AddValue(TrisKey, GetTriangles());
            info.AddValue(MeshKey, GetMeshes());
        }

        /// <summary>
        /// Derives a flattened triangle mesh from the detail mesh data.
        /// </summary>
        /// <remarks>
        /// All duplicate vertices are merged.
        /// </remarks>
        /// <param name="vertices">The result vertices. (x, y, z) * vertexCount
        /// </param>
        /// <param name="triangles">The result triangles.
        /// (vertAIndex, vertBIndex, vertCIndex) * triangleCount</param>
        /// <returns></returns>
        public bool GetTriangleMesh(out float[] vertices, out int[] triangles)
        {
            TriMesh3Ex resultMesh = new TriMesh3Ex();
            vertices = null;
            triangles = null;
            bool success = NMGenUtilEx.FlattenDetailMesh(ref root, ref resultMesh);
            if (success)
            {
                vertices = resultMesh.GetVertices();
                triangles = resultMesh.GetTriangles();
                TriMesh3Ex.FreeEx(ref resultMesh);
            }
            return success;
        }
    }
}
