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

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Represents data for a <see cref="PolyMesh"/> object.
    /// </summary>
    /// <remarks>
    /// For information on the data within this structure, see the 
    /// documentation for <see cref="PolyMesh"/>.
    /// </remarks>
    /// <seealso cref="PolyMesh"/>
    public struct PolyMeshData
    {
        /// <summary>
        /// The mesh vertices.
        /// </summary>
        public ushort[] vertices;

        /// <summary>
        /// The mesh polygon and neighbor data.
        /// </summary>
        public ushort[] polygons;

        /// <summary>
        /// The polygon region ids.
        /// </summary>
        public ushort[] regions;

        /// <summary>
        /// The polygon flags.
        /// </summary>
        public ushort[] flags;

        /// <summary>
        /// The polygon area ids.
        /// </summary>
        public byte[] areaIds;

        /// <summary>
        /// The number of vertices.
        /// </summary>
        public int vertexCount;

        /// <summary>
        /// The number of polygons.
        /// </summary>
        public int polygonCount;

        /// <summary>
        /// The maximum number of polygons the buffer can hold.
        /// </summary>
        public int maxPolygons;

        /// <summary>
        /// The maximum vertices per polygon.
        /// </summary>
        public int maxVertsPerPoly;

        /// <summary>
        /// The minimum bounds of the mesh.
        /// </summary>
        public float[] boundsMin;

        /// <summary>
        /// The maximum bounds of the mesh.
        /// </summary>
        public float[] boundsMax;

        /// <summary>
        /// The xz-plane cell size.
        /// </summary>
        public float xzCellSize;

        /// <summary>
        /// the y-axis cell height.
        /// </summary>
        public float yCellSize;
    }
}
