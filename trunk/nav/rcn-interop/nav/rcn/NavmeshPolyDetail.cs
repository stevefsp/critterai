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
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Data for a polygon's detailed mesh. (Within a navigation mesh tile.)
    /// </summary>
    /// <remarks>
    /// <p>This data is provided for debug purposes.</p>
    /// <p>All indices refer to the vertex and triangle data in the
    /// polygon's <see cref="NavmeshTileData">tile</see>.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshPolyDetail
    {

        /// <summary>
        /// The index of the base vertex for the detail mesh.
        /// </summary>
	    public uint vertexBase;

        /// <summary>
        /// The index of the base triangle for the detail mesh.
        /// </summary>
        public uint triangleBase;
		
		/// <summary>
		/// The number of vertices in the detail mesh.
		/// </summary>
        public byte vertexCount;
		
		/// <summary>
		/// The number of triangles in the detail mesh.
		/// </summary>
        public byte triangleCount;
    }
}
