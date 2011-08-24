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

namespace org.critterai.geom
{
    /// <summary>
    /// A basic indexed triangle mesh.
    /// </summary>
    /// <remarks>
    /// <para>The class arrays may represent buffers with unused space.</para></remarks>
    public sealed class TriangleMesh
    {
        /// <summary>
        /// Vertices [Form: (x, y, z) * vertCount]
        /// </summary>
        public float[] verts;

        /// <summary>
        /// Triangles [Form: (vertAIndex, vertBIndex, vertCIndex) * triCount]
        /// </summary>
        public int[] tris;

        /// <summary>
        /// The number of vertices.
        /// </summary>
        public int vertCount;

        /// <summary>
        /// The number of triangles.
        /// </summary>
        public int triCount;

        /// <summary>
        /// Constuctor.
        /// </summary>
        public TriangleMesh() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>This constructor assigns the provided references
        /// to the object.  (No copying.)</remarks>
        /// <param name="verts">The vertices. [Form: (x, y, z) * vertCount]
        /// </param>
        /// <param name="vertCount">The number of vertices.</param>
        /// <param name="tris">The triangles. 
        /// [Form: (vertAIndex, vertBIndex, vertCIndex) * triCount]</param>
        /// <param name="triCount">The number of triangles.</param>
        public TriangleMesh(float[] verts
            , int vertCount
            , int[] tris
            , int triCount)
        {
            this.verts = verts;
            this.vertCount = vertCount;
            this.tris = tris;
            this.triCount = triCount;
        }
    }
}
