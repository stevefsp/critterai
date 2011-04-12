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
    /// Navigation mesh tile data.
    /// </summary>
    /// <remarks>
    /// <p>Must be initialized before use.</p>
    /// <p>This data is provided for debug purposes.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshTileData
    {
	    private int magic;
	    private int version;

        /// <summary>
        /// The x-position of the tile within the tile grid. (x, z)
        /// </summary>
	    public int x;

        /// <summary>
        /// The z-position of the tile within the tile grid. (x, z)
        /// </summary>
        public int z;

        /// <summary>
        /// The layer of the tile.
        /// </summary>
        /// <remarks>
        /// Layering occurs on the y-axis. (Height)
        /// </remarks>
        public int layer;

        /// <summary>
        /// The user assigned id of the tile.
        /// </summary>
	    public uint userId;	

        /// <summary>
        /// The number of polygons in the tile.
        /// </summary>
	    public int polygonCount;
		
		/// <summary>
		/// The number of polygon vertices in the tile. 
		/// </summary>
	    public int vertexCount;

        /// <summary>
        /// The number of links allocated. 
        /// </summary>
	    public int maxLinkCount;	
		
		/// <summary>
		/// The number of detail meshes.
		/// </summary>
	    public int detailMeshCount;

        /// <summary>
        /// The number of unique detail vertices.  (In addition to the
        /// polygon vertices.)
        /// </summary>
	    public int detailVertCount;

        /// <summary>
        /// The number of detail triangles.
        /// </summary>
	    public int detailTriCount;

        /// <summary>
        /// The number of bounding volume nodes.
        /// </summary>
	    public int bvNodeCount;

        /// <summary>
        /// The number of off-mesh connections.
        /// </summary>
	    public int offMeshConCount;

        /// <summary>
        /// The index of the first polygon which is an off-mesh connection.
        /// </summary>
	    public int offMeshBase;

        /// <summary>
        /// The designed minimum floor to 'ceiling' height that will still 
        /// allow the floor area to be considered traversable.
        /// </summary>
        public float minTraversableHeight;

        /// <summary>
        /// Represents the closest any part of a mesh gets to an
        /// obstruction in the source geometry. (Usually the client radius.)
        /// </summary>
        public float traversableAreaBorderSize;

        /// <summary>
        /// The designed maximum ledge height that is considered to still be
        /// traversable. 
        /// </summary>
        public float maxTraversableStep;

        /// <summary>
        /// The minimum bounds of the tile's AABB in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundsMin;

        /// <summary>
        /// The maximum bounds of the tile's AABB in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundsMax;

        /// <summary>
        /// The bounding volumn quantization factor. (For converting from world
        /// to bounding volumn coordinates.)
        /// </summary>
        public float bvQuantFactor;

        private uint salt;						// Counter describing modifications to the tile.
        private uint linksFreeList;				// Index to next free link.

        /// <summary>
        /// The memory data size of the tile.
        /// </summary>
        public int dataSize;

        /// <summary>
        /// The tile ownership flags.
        /// </summary>
        public NavmeshTileFlags flags;

        /// <summary>
        /// The index of the tile within the navigation mesh.
        /// </summary>
        public int tileIndex;

        /// <summary>
        /// The id of the base polygon in the tile.
        /// </summary>
        public uint basePolyId;

        /// <summary>
        /// Initializes the structure before its first use.
        /// </summary>
        /// <remarks>
        /// Existing references are released and replaced.
        /// </remarks>
        public void Initialize()
        {
	        magic = 0;
	        version = 0;
	        x = 0;
            z = 0;
            layer = 0;
	        userId = 0;
	        polygonCount = 0;
	        vertexCount = 0;	
	        maxLinkCount = 0;	
	        detailMeshCount = 0;	
	        detailVertCount = 0;
	        detailTriCount = 0;	
	        bvNodeCount = 0;	
	        offMeshConCount = 0;
	        offMeshBase = 0;
            minTraversableHeight = 0;	
            traversableAreaBorderSize = 0;	
            maxTraversableStep = 0;		
            boundsMin = new float[3]; 
            boundsMax = new float[3];
            bvQuantFactor = 0;	
            salt = 0;
            linksFreeList = 0;	
            dataSize = 0;	
            flags = 0;
            tileIndex = 0;
            basePolyId = 0;
        }
    }
}
