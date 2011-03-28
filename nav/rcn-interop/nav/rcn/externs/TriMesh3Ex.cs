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
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn.externs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TriMesh3Ex
    {
        public int vertexCount;
        public int triangleCount;
        public IntPtr vertices;
        public IntPtr triangles;

        public float[] GetVertices()
        {
            if (vertexCount <= 0 || vertices == IntPtr.Zero)
                return null;
            float[] result = new float[vertexCount * 3];
            Marshal.Copy(vertices, result, 0, result.Length);
            return result;
        }

        public int[] GetTriangles()
        {
            if (triangleCount <= 0 || triangles == IntPtr.Zero)
                return null;
            int[] result = new int[triangleCount * 3];
            Marshal.Copy(triangles, result, 0, result.Length);
            return result;
        }

        public TriMesh3Ex(float[] vertices, int[] indices)
        {
            if (vertices == null
                || vertices.Length < 3
                || vertices.Length % 3 != 0
                || indices == null
                || indices.Length < 3
                || indices.Length % 3 != 0)
            {
                this.vertexCount = 0;
                this.triangleCount = 0;
                this.vertices = IntPtr.Zero;
                this.triangles = IntPtr.Zero;
                return;
            }

            this.vertices = 
                UtilEx.GetFilledBuffer(vertices, vertices.Length);

            this.triangles =
                UtilEx.GetFilledBuffer(indices, indices.Length);

            this.vertexCount = vertices.Length / 3;
            this.triangleCount = indices.Length / 3;
        }

        public static void Free(ref TriMesh3Ex mesh)
        {
            Marshal.FreeHGlobal(mesh.vertices);
            Marshal.FreeHGlobal(mesh.triangles);
            mesh.vertexCount = 0;
            mesh.triangleCount = 0;
            mesh.vertices = IntPtr.Zero;
            mesh.triangles = IntPtr.Zero;
        }

        [DllImport("cai-nav-rcn", EntryPoint = "rcnFreeMesh3")]
        public static extern void FreeEx(ref TriMesh3Ex mesh);
    };
}
