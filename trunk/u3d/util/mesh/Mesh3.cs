/*
 * Copyright (c) 2010 Stephen A. Pratt
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

namespace org.critterai.mesh
{
    /// <summary>
    /// A data structure representing a simple 3D polygon mesh with
    /// only vertex and index information. (No normals, uv, or other
    /// extra data.)
    /// </summary>
    /// <remarks>
    /// <p>This class is optimized for speed.  To support this priority, no 
    /// argument validation is performed.  E.g. No null reference checks, 
    /// minimal checks for poorly formed data.</p>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public class Mesh3
    {
        /// <summary>
        /// The number of vertices per polygon.
        /// </summary>
        public readonly int vertsPerPolygon;

        /// <summary>
        /// The mesh's verticies in the form (x, y, z).
        /// </summary>
        public float[] vertices = null;

        /// <summary>
        /// The mesh's indices in the form 
        /// (vertIndex1, vertIndex2, vertIndex3, ..., vertIndexN)
        /// </summary>
        /// <remarks>The stride of the array is determined by the value
        /// of <see cref="vertsPerPolygon"/></remarks>
        public int[] indices = null;

        /// <summary>
        /// The number of vertices in the mesh.
        /// </summary>
        public int VertexCount
        {
            get { return (vertices == null ? 0 : vertices.Length / 3); }
        }

        /// <summary>
        /// The number of polygons in the mesh.
        /// </summary>
        public int PolyCount
        {
            get 
            { 
                return (indices == null ? 0 : 
                    indices.Length / vertsPerPolygon);
            }
        }

        /// <summary>
        /// Constructs an instance with no vertices or indices defined.
        /// </summary>
        /// <param name="vertsPerPolygon">The number of vertices per
        /// polygon. Limited to a a minimum value of 3.</param>
        public Mesh3(int vertsPerPolygon) 
        {
            this.vertsPerPolygon = Math.Max(3, vertsPerPolygon);
        }

        /// <summary>
        /// Constructs an instance with the vertices and indices arrays 
        /// initialized.
        /// </summary>
        /// <param name="vertexCount">The number of vertices that will be
        /// loaded into the mesh.</param>
        /// <param name="indicesCount">The number of indices that will be
        /// loaded into the mesh.</param>
        /// <param name="vertsPerPolygon">The number of vertices per
        /// polygon. Limited to a a minimum value of 3.</param>
        public Mesh3(int vertsPerPolygon
            , int vertexCount
            , int indicesCount)
        {
            this.vertsPerPolygon = Math.Max(3, vertsPerPolygon);
            vertices = new float[vertexCount * 3];
            indices = new int[indicesCount * this.vertsPerPolygon];
        }

        /// <summary>
        /// Constructor which creates an instance loaded with the specified 
        /// vertices and indices.
        /// </summary>
        /// <remarks>
        /// The arrays are stored by reference, not by copy.
        /// </remarks>
        /// <param name="vertices">The vertices to associate to the mesh.
        /// </param>
        /// <param name="indices">The indices to associate to the mesh.</param>
        /// <param name="vertsPerPolygon">The number of vertices per
        /// polygon. Limited to a a minimum value of 3.</param>
        public Mesh3(int vertsPerPolygon, float[] vertices
            , int[] indices)
        {
            this.vertsPerPolygon = Math.Max(3, vertsPerPolygon);
            this.vertices = vertices;
            this.indices = indices;
        }
    }
}
