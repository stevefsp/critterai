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
using System.Runtime.Serialization;
using org.critterai.interop;
using org.critterai.nmgen.rcn;

namespace org.critterai.nmgen
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
    /// <p>This class is not compatible with Unity serialization.</p>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public sealed class PolyMeshDetail
        : IManagedObject, ISerializable
    {
        /*
         * Design Notes:
         * 
         * The internal structure of this class will have to be transformed to
         * match the pattern used by PolyMesh in order to add the merge 
         * mesh functionality.  (Can't pass arrays of pointers across the
         * interop boundary.) Not changing the structure until then.
         * 
         */

        private const string DataKey = "d";

        private IntPtr mMeshes;
        private IntPtr mVerts;
        private IntPtr mTris;
        private int mMeshCount;
        private int mVertCount;
        private int mTriCount;

        private int mMaxMeshes;
        private int mMaxVerts;
        private int mMaxTris;
        private readonly AllocType mResourceType;

        /// <summary>
        /// The number of sub-meshes in the detail mesh.
        /// </summary>
        public int MeshCount { get { return mMeshCount; } }

        /// <summary>
        /// The total number of vertices in the detail mesh.
        /// </summary>
        public int VertCount { get { return mVertCount; } }

        /// <summary>
        /// The total number of triangles in the detail mesh.
        /// </summary>
        public int TriCount { get { return mTriCount; } }

        public int MaxMeshes { get { return mMaxMeshes; } }
        public int MaxVerts { get { return mMaxVerts; } }
        public int MaxTris { get { return mMaxTris; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mMeshes == IntPtr.Zero); } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return mResourceType; } }

        // Used for interop build calls.
        private PolyMeshDetail(AllocType resourceType)
        {
            mResourceType = resourceType;
        }

        private PolyMeshDetail(SerializationInfo info, StreamingContext context)
        {
            // Note: Version compatability is handled by the interop call.

            if (info.MemberCount != 1)
                return;

            byte[] rawData = (byte[])info.GetValue(DataKey, typeof(byte[]));
            PolyMeshDetailEx.Build(rawData, rawData.Length, this);
        }

        ~PolyMeshDetail()
        {
            RequestDisposal();
        }

        public bool Load(PolyMeshDetailData data)
        {
            if (IsDisposed
                || data == null
                || data.meshes == null
                || data.tris == null
                || data.verts == null
                || data.meshCount < 0 || data.meshCount > mMaxMeshes
                || data.triCount < 0 || data.triCount > mMaxTris
                || data.vertCount < 0 || data.vertCount > mMaxVerts
                || data.meshes.Length < data.meshCount * 4
                || data.tris.Length < data.triCount * 4
                || data.verts.Length < data.vertCount * 3)
            {
                return false;
            }

            mMeshCount = data.meshCount;
            mTriCount = data.triCount;
            mVertCount = data.vertCount;

            UtilEx.Copy(data.meshes, 0, mMeshes, mMeshCount * 4);
            Marshal.Copy(data.tris, 0, mTris, mTriCount * 4);
            Marshal.Copy(data.verts, 0, mVerts, mVertCount * 3);

            return true;
        }

        ///// <summary>
        ///// Returns a copy of the sub-mesh data. (baseVertexIndex,
        ///// vertexCount, baseTriangleIndex, triangleCount) * MeshCount 
        ///// </summary>
        ///// <remarks>
        ///// <p>The maximum number of vertices per sub-mesh is 127.</p>
        ///// <p>The maximum number of triangles per sub-mesh is 255.</p>
        ///// <p>
        ///// An example of iterating the triangles in a sub-mesh. (Where
        ///// iMesh is the index for the sub-mesh within detailMesh.)
        ///// </p>
        ///// <p>
        ///// <pre>
        /////     uint[] meshes = detailMesh.GetMeshes();
        /////     byte[] triangles = detailMesh.GetTriangles();
        /////     float[] vertices = detailMesh.GetVertices();
        ///// 
        /////     int pMesh = iMesh * 4;
        /////     int pVertBase = meshes[pMesh + 0] * 3;
        /////     int pTriBase = meshes[pMesh + 2] * 4;
        /////     int tCount = meshes[pMesh + 3];
        /////     int vertX, vertY, vertZ;
        /////
        /////     for (int iTri = 0; iTri &lt; tCount; iTri++)
        /////     {
        /////        for (int iVert = 0; iVert &lt; 3; iVert++)
        /////        {
        /////            int pVert = pVertBase 
        /////                + (triangles[pTriBase + (iTri * 4 + iVert)] * 3);
        /////            vertX = vertices[pVert + 0];
        /////            vertY = vertices[pVert + 1];
        /////            vertZ = vertices[pVert + 2];
        /////            // Do something with the vertex.
        /////        }
        /////    }
        ///// </pre>
        ///// </p>
        ///// </remarks>
        ///// <returns>A copy of the sub-mesh data.</returns>
        //public uint[] GetMeshes(bool includeBuffer)
        //{
        //    int len = (includeBuffer ? mMaxMeshes : mMeshCount) * 4;
        //    return (len > 0 ? UtilEx.ExtractArrayUInt(mMeshes, len) : null);
        //}

        ///// <summary>
        ///// The vertices that make up each sub-mesh. (x, y, z) * VertexCount.
        ///// </summary>
        ///// <remarks>
        ///// <p>The vertices are grouped by sub-mesh and will contain duplicates
        ///// since each sub-mesh is independantly defined.</p>
        ///// <p>The first group of vertices for each sub-mesh are in the same 
        ///// order as the vertices for the sub-mesh's associated polygon 
        ///// mesh polygon.  These vertices are followed by any additional
        ///// detail vertices.  So it the associated polygon has 5 vertices, the
        ///// sub-mesh will have a minimum of 5 vertices and the first 5 
        ///// vertices will be equivalent to the 5 polygon vertices.</p>
        ///// <p>See GetMeshes() for how to obtain the base vertex and
        ///// vertex count for each sub-mesh.</p>
        ///// </remarks>
        ///// <returns>The vertices that make up each sub-mesh.</returns>
        //public float[] GetVerts(bool includeBuffer)
        //{
        //    int len = (includeBuffer ? mMaxVerts : mVertCount) * 3;
        //    return (len > 0 ? UtilEx.ExtractArrayFloat(mVerts, len) : null);
        //}

        ///// <summary>
        ///// Returns a copy of the triangles array. (vertIndexA,
        ///// vertIndexB, vertIndexC, flag) * TriangleCount
        ///// </summary>
        ///// <remarks>
        ///// <p>The triangles are grouped by sub-mesh. See GetMeshes()
        ///// for how to obtain the base triangle and triangle count for
        ///// each sub-mesh.</p>
        ///// <p><b>Vertices</b></p>
        ///// <p> The vertex indices in the triangle array are local to the 
        ///// sub-mesh, not global.  To translate into an global index in the 
        ///// vertices array, the values must be offset by the sub-mesh's base 
        ///// vertex index.</p>
        ///// <p>
        ///// Example: If the baseVertexIndex for the sub-mesh is 5 and the
        ///// triangle entry is (4, 8, 7, 0), then the actual indices for
        ///// the vertices are (4 + 5, 8 + 5, 7 + 5).
        ///// </p>
        ///// <p><b>Flags</b></p>
        ///// <p>The flags entry indicates which edges are internal and which
        ///// are external to the sub-mesh.</p>
        ///// 
        ///// <p>Internal edges connect to other triangles within the same 
        ///// sub-mesh.
        ///// External edges represent portals to other sub-meshes or the null 
        ///// region.</p>
        ///// 
        ///// <p>Each flag is stored in a 2-bit position.  Where position 0 is the
        ///// lowest 2-bits and position 4 is the highest 2-bits:</p>
        ///// 
        ///// <p>Position 0: Edge AB (>> 0)<br />
        ///// Position 1: Edge BC (>> 2)<br />
        ///// Position 2: Edge CA (>> 4)<br />
        ///// Position 4: Unused</p>
        ///// 
        ///// <p>Testing should be performed as follows:</p>
        ///// <pre>
        ///// if (((flag >> 2) & 0x3) == 0)
        ///// {
        /////     // Edge BC is an external edge.
        ///// }
        ///// </pre>
        ///// </remarks>
        ///// <returns>A copy of the triangles array.</returns>
        //public byte[] GetTris(bool includeBuffer)
        //{
        //    int len = (includeBuffer ? mMaxTris : mTriCount) * 4;
        //    return (len > 0 ? UtilEx.ExtractArrayByte(mTris, len) : null);
        //}

        /// <summary>
        /// Gets a copy of the entire data structure of the mesh.
        /// </summary>
        /// <returns>A copy of the entire data structure of the mesh.
        /// </returns>
        public PolyMeshDetailData GetData(bool includeBuffer)
        {
            if (IsDisposed)
                return null;

            PolyMeshDetailData result = new PolyMeshDetailData(
                (includeBuffer ? mMaxVerts : mVertCount)
                , (includeBuffer ? mMaxTris : mTriCount)
                , (includeBuffer ? mMaxMeshes : mMeshCount));

            FillData(result);

            return result;
        }

        public PolyMeshDetailData GetData(PolyMeshDetailData buffer)
        {
            if (IsDisposed)
                return null;
            if (buffer == null)
                return GetData(true);

            if (!buffer.CanFit(mVertCount, mTriCount, mMeshCount))
                buffer.Reset(mMaxVerts, mMaxTris, mMaxMeshes);

            FillData(buffer);

            return buffer;

        }

        private void FillData(PolyMeshDetailData buffer)
        {
            buffer.vertCount = mVertCount;
            buffer.triCount = mTriCount;
            buffer.meshCount = mMeshCount;

            Marshal.Copy(mVerts, buffer.verts, 0, mVertCount * 3);
            Marshal.Copy(mTris, buffer.tris, 0, mTriCount * 4);
            UtilEx.Copy(mMeshes, buffer.meshes, mMeshCount * 4);
        }

        /// <summary>
        /// Frees all resources and marks the object as disposed.
        /// </summary>
        public void RequestDisposal()
        {
            if (!IsDisposed)
            {
                if (ResourceType == AllocType.Local)
                {
                    Marshal.FreeHGlobal(mMeshes);
                    Marshal.FreeHGlobal(mTris);
                    Marshal.FreeHGlobal(mVerts);
                }
                else if (ResourceType == AllocType.External)
                    PolyMeshDetailEx.FreeEx(this);

                mMeshes = IntPtr.Zero;
                mTris = IntPtr.Zero;
                mVerts = IntPtr.Zero;
                mMeshCount = 0;
                mVertCount = 0;
                mTriCount = 0;
                mMaxMeshes = 0;
                mMaxTris = 0;
                mMaxVerts = 0;
            }
        }

        public byte[] GetSerializedData(bool includeBuffer)
        {
            if (IsDisposed)
                return null;

            // Design note:  This is implemented using interop calls
            // bacause it is so much easier and faster to serialize in C++
            // than in C#.

            IntPtr ptr = IntPtr.Zero;
            int dataSize = 0;

            if (!PolyMeshDetailEx.GetSerializedData(this
                , includeBuffer
                , ref ptr
                , ref dataSize))
            {
                return null;
            }

            byte[] result = UtilEx.ExtractArrayByte(ptr, dataSize);

            PolyMeshDetailEx.FreeSerializationData(ref ptr);

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
             * Validation and versioning is handled by GetSerializedData().
             */

            byte[] rawData = GetSerializedData(true);

            if (rawData == null)
                return;

            info.AddValue(DataKey, rawData);
        }

        public PolyMeshDetail(byte[] serializedMesh)
        {
            mResourceType = AllocType.External;

            PolyMeshDetailEx.Build(serializedMesh
                , serializedMesh.Length
                , this);
        }

        public static PolyMeshDetail Build(BuildContext context
            , PolyMesh polyMesh
            , CompactHeightfield field
            , float detailSampleDistance
            , float detailMaxDeviation)
        {
            if (context == null || polyMesh == null)
                return null;

            PolyMeshDetail result = new PolyMeshDetail(AllocType.External);

            if (PolyMeshDetailEx.Build(context.root
                , ref polyMesh.root
                , field
                , detailSampleDistance
                , detailMaxDeviation
                , result))
            {
                return result;
            }

            return null;
        }

        public PolyMeshDetail(int maxVerts
            , int maxTris
            , int maxMeshes)
        {
            if (maxVerts < 3 || maxTris < 1 || maxMeshes < 1)
                return;

            mResourceType = AllocType.Local;

            mMaxVerts = maxVerts;
            mMaxTris = maxTris;
            mMaxMeshes = maxMeshes;

            int size = sizeof(float) * mMaxVerts * 3;
            mVerts = UtilEx.GetBuffer(size, true);

            size = sizeof(byte) * mMaxTris * 4;
            mTris = UtilEx.GetBuffer(size, true);

            size = sizeof(uint) * mMaxMeshes * 4;
            mMeshes = UtilEx.GetBuffer(size, true);
        }
    }
}
