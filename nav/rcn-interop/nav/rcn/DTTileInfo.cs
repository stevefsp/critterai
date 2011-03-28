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

namespace org.critterai.nav.rcn
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DTTileInfo
    {

	    // public int magic;								// Magic number, used to identify the data.
        // public int version;							// Data version number.
        public int x;
        public int y;								// Location of the time on the grid.
        // public uint userId;					        // User ID of the tile.
        public int polygonCount;							// Number of polygons in the tile.
        public int vertexCount;							// Number of vertices in the tile.
        // public int maxLinkCount;						// Number of allocated links.
        // public int detailMeshCount;					// Number of detail meshes.
        // public int detailVertCount;					// Number of detail vertices.
        // public int detailTriCount;						// Number of detail triangles.
        // public int bvNodeCount;						// Number of BVtree nodes.
        // public int offMeshConCount;					// Number of Off-Mesh links.
        // public int offMeshBase;						// Index to first polygon which is Off-Mesh link.
        public float minTraversableHeight;					// Height of the agent.
        public float traversableAreaBorderSize;					// Radius of the agent
        public float maxTraversableStep;					// Max climb height of the agent.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundsMin;                           // Bounding box of the tile.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundsMax;	
				        
	    // float bvQuantFactor;					// BVtree quantization factor (world to bvnode coords)

        public uint basePolyId;

        public static DTTileInfo Initialized
        {
            get
            {
                DTTileInfo result = new DTTileInfo();
                result.boundsMax = new float[3];
                result.boundsMin = new float[3];
                return result;
            }
        }
    }
}
