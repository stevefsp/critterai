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
    /// A structure used for marshalling triangle mesh data across the
    /// native interop boundary.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TriMesh3Ex
    {
        /// <summary>
        /// The number of vertices in the mesh.
        /// </summary>
        public int vertexCount;

        /// <summary>
        /// The number of triangles in the mesh.
        /// </summary>
        public int triangleCount;

        /// <summary>
        /// A pointer to the unmanged vertex array. (float) 
        /// (Size: 3 * vertexCount)
        /// </summary>
        public IntPtr vertices;

        /// <summary>
        /// A pointer to the unmanaged triangle array. (int)
        /// (Size: 3 * triangleCount)
        /// </summary>
        public IntPtr triangles;

        /// <summary>
        /// Gets a copy of the vertices. (x, y, z) * vertexCount
        /// </summary>
        /// <returns>A copy of the vertices.</returns>
        public float[] GetVertices()
        {
            if (vertexCount <= 0 || vertices == IntPtr.Zero)
                return null;
            float[] result = new float[vertexCount * 3];
            Marshal.Copy(vertices, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Gets a copy of the triangle indices.  
        /// (vertAIndex, vertBIndex, vertCIndex) * triangleCount
        /// </summary>
        /// <returns>A copy of the triangle indices.</returns>
        public int[] GetTriangles()
        {
            if (triangleCount <= 0 || triangles == IntPtr.Zero)
                return null;
            int[] result = new int[triangleCount * 3];
            Marshal.Copy(triangles, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// Any mesh constructed using a non-default constructor must be freed
        /// using the <see cref="Free"/> method.  Otherwise a memory leak will
        /// occur.</remarks>
        /// <param name="vertices">The mesh vertices. (x, y, z) * vertexCount
        /// </param>
        /// <param name="triangles">The mesh triangles.
        /// (vertAIndex, vertBIndex, vertCIndex) * triangleCount</param>
        public TriMesh3Ex(float[] vertices, int[] triangles)
        {
            if (vertices == null
                || vertices.Length < 3
                || vertices.Length % 3 != 0
                || triangles == null
                || triangles.Length < 3
                || triangles.Length % 3 != 0)
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
                UtilEx.GetFilledBuffer(triangles, triangles.Length);

            this.vertexCount = vertices.Length / 3;
            this.triangleCount = triangles.Length / 3;
        }

        /// <summary>
        /// Frees the unmanaged resources for a mesh created using a local
        /// non-default constructor.
        /// </summary>
        /// <remarks>
        /// <p>This method does not have to be called if the mesh was created
        /// using the default constructor.</p>
        /// <p>Behavior is undefined if this method is used on
        /// a mesh created by an interop method.</p>
        /// </remarks>
        /// <param name="detailMesh">The mesh to free.</param>
        public static void Free(ref TriMesh3Ex mesh)
        {
            Marshal.FreeHGlobal(mesh.vertices);
            Marshal.FreeHGlobal(mesh.triangles);
            mesh.vertexCount = 0;
            mesh.triangleCount = 0;
            mesh.vertices = IntPtr.Zero;
            mesh.triangles = IntPtr.Zero;
        }

        /// <summary>
        /// Frees the unmanaged resources for a mesh created using an
        /// native interop method.
        /// </summary>
        /// <remarks>
        /// Behavior is undefined if this method is called on a
        /// mesh created by a local constructor.</remarks>
        /// <param name="detailMesh">The mesh to free.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "rcnFreeMesh3")]
        public static extern void FreeEx(ref TriMesh3Ex mesh);
    };
}
