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

namespace org.critterai.nav
{
    /// <summary>
    /// Navigation mesh configuration parameters.
    /// </summary>
    /// <remarks>
    /// <p>Implemented as a class with public fields in order to support Unity
    /// serialization.  Care must be taken not to set the fields to invalid
    /// values.</p></remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NavmeshParams
    {
        /// <summary>
        /// The minimumn allowed tile size.
        /// </summary>
        public const float MinTileSize = 0.1f;

        /// <summary>
        /// The origin of the navigation mesh's tile space in the form
        /// (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	    public float[] origin = new float[3];

        /// <summary>
        /// The width of each tile. (Along the x-axis.)
        /// </summary>
        public float tileWidth = 0;

        /// <summary>
        /// The depth of each tile. (Along the z-axis.)
        /// </summary>
        public float tileDepth = 0;	

        /// <summary>
        /// The maximum number of tiles the navigation mesh can contain.
        /// </summary>
        public int maxTiles = 0;

        /// <summary>
        /// The maximum number of polygons each tile can contain.
        /// </summary>
        public int maxPolysPerTile = 0;

        /// <summary>
        /// The world-space origin of the navigation mesh. [Form: (x, y, z)]
        /// </summary>
        /// <returns>The world-space origin of the navigation mesh.</returns>
        public float[] GetOrigin()
        {
            return (float[])origin.Clone();
        }

        /// <summary>
        /// Constructs and initializes the structure.
        /// </summary>
        /// <param name="originX">The x-value of the tile space origin.</param>
        /// <param name="originY">The y-value of the tile space origin.</param>
        /// <param name="originZ">The z-value of the tile space origin.</param>
        /// <param name="tileWidth">The width of each tile. (Along the x-axis.)
        /// </param>
        /// <param name="tileDepth">The depth of each tile. (Along the z-axis.)
        /// </param>
        /// <param name="maxTiles">The maximum number of tiles the navigation 
        /// mesh can contain.</param>
        /// <param name="maxPolysPerTile">The maximum number of polygons each 
        /// tile can contain.</param>
        public NavmeshParams(float originX, float originY, float originZ
            , float tileWidth, float tileDepth
            , int maxTiles, int maxPolysPerTile)
        {
            origin[0] = originX;
            origin[1] = originY;
            origin[2] = originZ;
            this.tileWidth = Math.Max(MinTileSize, tileWidth);
            this.tileDepth = Math.Max(MinTileSize, tileDepth);
            this.maxTiles = Math.Max(1, maxTiles);
            this.maxPolysPerTile = Math.Max(3
                , Math.Min(Navmesh.MaxAllowedVertsPerPoly, maxPolysPerTile));
        }

        internal NavmeshParams() { }
    }
}
