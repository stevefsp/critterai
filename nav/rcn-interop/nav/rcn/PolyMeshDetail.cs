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
using System.Collections.Generic;
using System.Text;
using org.critterai.nav.rcn.externs;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace org.critterai.nav.rcn
{
    [Serializable]
    public sealed class PolyMeshDetail
        : IDisposable, ISerializable
    {
        private const string VertKey = "v";
        private const string TrisKey = "t";
        private const string MeshKey = "m";

        internal PolyMeshDetailEx root;
        private bool mIsDisposed = false;
        private readonly bool mIsLocal;

        internal PolyMeshDetail(PolyMeshDetailEx detailMesh)
        {
            root = detailMesh;
            mIsLocal = false;
        }

        public PolyMeshDetail(float[] vertices
            , byte[] triangles
            , uint[] meshes)
        {
            mIsLocal = true;
            root = new PolyMeshDetailEx(vertices, triangles, meshes);
        }

        private PolyMeshDetail(SerializationInfo info
            , StreamingContext context)
        {
            mIsLocal = true;

            if (info.MemberCount != 3)
            {
                root = PolyMeshDetailEx.Empty;
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
            Dispose();
        }

        public void Dispose()
        {
            if (!mIsDisposed)
            {
                if (mIsLocal)
                    PolyMeshDetailEx.Free(ref root);
                else
                    PolyMeshDetailEx.FreeEx(ref root);
                mIsDisposed = true;
            }
        }

        public bool IsDisposed { get { return mIsDisposed; } }

        public int MeshCount { get { return root.MeshCount; } }
        public int VertexCount { get { return root.VertexCount; } }
        public int TriangleCount { get { return root.TriangleCount; } }

        public uint[] GetMeshes()
        {
            return root.GetMeshes();
        }

        public float[] GetVertices()
        {
            return root.GetVertices();
        }

        public byte[] GetTriangles()
        {
            return root.GetTriangles();
        }

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
