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
using org.critterai.math;

namespace org.critterai.mesh
{
    /// <summary>
    /// A data structure representing a 3D polygon mesh with
    /// minimum and maximum bounds data.
    /// </summary>
    /// <remarks>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public class BoundedMesh3
        : Mesh3
    {
        /// <summary>
        /// The AABB bounds of the mesh in the form 
        /// (minX, minY, minZ, maxX, maxY, maxZ).
        /// </summary>
        public readonly float[] bounds = new float[6];

        /// <summary>
        /// Constructs an instance with no vertices or indices defined.
        /// </summary>
        /// <param name="vertsPerPolygon">The number of vertices per
        /// polygon. Limited to a a minimum value of 3.</param>
        public BoundedMesh3(int vertsPerPolygon) : base(vertsPerPolygon) { }

        /// <summary>
        /// Constructor which creates an instance loaded with the specified 
        /// vertices and indices.
        /// </summary>
        /// <remarks>
        /// The arrays are stored by reference, not by copy.
        /// </remarks>
        /// <param name="vertsPerPolygon">The number of vertices per
        /// polygon. Limited to a a minimum value of 3.</param>
        /// <param name="vertices">The vertices to associate to the mesh.
        /// </param>
        /// <param name="indices">The indices to associate to the mesh.
        /// </param>
        public BoundedMesh3(int vertsPerPolygon
                , float[] vertices
                , int[] indices)
            : base(vertsPerPolygon, vertices, indices)
        {
            RebuildBounds();
        }

        /// <summary>
        /// Re-calculates the minimum and maximum bounds of the mesh.
        /// </summary>
        public void RebuildBounds()
        {
            Vector3Util.GetBounds(vertices, bounds);
        }
    }
}
