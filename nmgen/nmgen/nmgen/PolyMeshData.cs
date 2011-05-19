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

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents data for a <see cref="PolyMesh"/> object.
    /// </summary>
    /// <remarks>
    /// <p>This class is not compatible with Unity serialization.</p>
    /// </remarks>
    /// <seealso cref="PolyMesh"/>
    [Serializable]
    public sealed class PolyMeshData
    {
        /// <summary>
        /// Mesh vertices.
        /// [Form: (x, y, z) * vertCount]
        /// </summary>
        /// <remarks>
        /// <p>Minimum bounds and cell size is used to convert vertex 
        /// coordinates into world space.</p>
        /// <p>worldX = boundsMin[0] + vertX * xzCellSize<br/>
        /// worldY = boundsMin[1] + vertY * yCellSize<br/>
        /// worldZ = boundsMin[2] + vertZ * xzCellSize<br/>
        /// </p>
        /// </remarks>
        public ushort[] verts;

        /// <summary>
        /// Polygon and neighbor data. 
        /// [Length: >= polyCount * 2 * maxVertsPerPoly]
        /// </summary>
        /// <remarks>
        /// <p>Each entry is 2 * MaxVertsPerPoly in length.</p>
        /// <p>The first half of the entry contains the indices of the polygon.
        /// The first instance of <see cref="PolyMesh.NullIndex"/> indicates 
        /// the end of the indices for the entry.</p>
        /// <p>The second half contains indices to neighbor polygons.  A
        /// value of <see cref="NullIndex"/> indicates no connection for the 
        /// associated edge. (Solid wall.)</p>
        /// <p><b>Example:</b></p>
        /// <p>
        /// MaxVertsPerPoly = 6<br/>
        /// For the entry: (1, 3, 4, 8, NullIndex, NullIndex, 18, NullIndex
        /// , 21, NullIndex, NullIndex, NullIndex)</p>
        /// <p>
        /// (1, 3, 4, 8) defines a polygon with 4 vertices.<br />
        /// Edge 1->3 is shared with polygon 18.<br />
        /// Edge 4->8 is shared with polygon 21.<br />
        /// Edges 3->4 and 4->8 are border edges not shared with any other
        /// polygon.
        /// </p>
        /// </remarks>
        public ushort[] polys;

        /// <summary>
        /// The region id assigned to each polygon.
        /// [Length: >= polyCount]
        /// </summary>
        public ushort[] regions;

        /// <summary>
        /// The flags assigned to each polygon.
        /// [Length: >= polyCount]
        /// </summary>
        public ushort[] flags;

        /// <summary>
        /// The area id assigned to each polygon.
        /// [Length: >= polyCount]
        /// </summary>
        /// <remarks>
        /// <p>During the standard build process, all walkable polygons
        /// get the default value of <see cref="WalkableArea"/>.
        /// This value can then be changed to meet user requirements.</p>
        /// </remarks>
        public byte[] areas;

        /// <summary>
        /// The number of vertices.
        /// </summary>
        public int vertCount;

        /// <summary>
        /// The number of polygons.
        /// </summary>
        public int polyCount;

        /// <summary>
        /// The maximum vertices per polygon.
        /// </summary>
        public int maxVertsPerPoly;

        /// <summary>
        /// The minimum bounds of the mesh's AABB.
        /// [Form: (x, y, z)]
        /// </summary>
        public float[] boundsMin = new float[3];

        /// <summary>
        /// The maximum bounds of the mesh's AABB.
        /// [Form: (x, y, z)]
        /// </summary>
        public float[] boundsMax = new float[3];

        /// <summary>
        /// The xz-plane cell size.
        /// </summary>
        public float xzCellSize;

        /// <summary>
        /// The y-axis cell height.
        /// </summary>
        public float yCellSize;

        /// <summary>
        /// The AABB border size used to build the mesh. [Units: CellSize (XZ)]
        /// </summary>
        public int borderSize;

        /// <summary>
        /// The walkable height used to build the mesh.
        /// </summary>
        public float walkableHeight;

        /// <summary>
        /// The walkable raduius used to build the mesh.
        /// </summary>
        public float walkableRadius;

        /// <summary>
        /// The maximum walkable step used to build the mesh.
        /// </summary>
        public float walkableStep;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxVerts">The maximum vertices the vertex buffer
        /// can hold.</param>
        /// <param name="maxPolys">The maximum polygons the polygon buffer
        /// can hold.</param>
        /// <param name="maxVertsPerPoly">The maximum allowed vertices
        /// for a polygon.</param>
        public PolyMeshData(int maxVerts
            , int maxPolys
            , int maxVertsPerPoly)
        {
            if (maxVerts < 3
                || maxPolys < 1
                || maxVertsPerPoly < 3
                || maxVertsPerPoly > NMGen.MaxAllowedVertsPerPoly)
            {
                return;
            }

            polys = new ushort[maxPolys * 2 * maxVertsPerPoly];
            verts = new ushort[maxVerts * 3];
            areas = new byte[maxPolys];
            flags = new ushort[maxPolys];
            regions = new ushort[maxPolys];

            this.maxVertsPerPoly = maxVertsPerPoly;
        }

        /// <summary>
        /// Clears all object data and resizes the buffers.
        /// </summary>
        /// <param name="maxVerts">The maximum vertices the vertex buffer
        /// can hold.</param>
        /// <param name="maxPolys">The maximum polygons the polygon buffer
        /// can hold.</param>
        /// <param name="maxVertsPerPoly">The maximum allowed vertices
        /// for a polygon.</param>
        public void Resize(int maxVerts
            , int maxPolys
            , int maxVertsPerPoly)
        {
            Resize();

            if (maxVerts < 3
                || maxPolys < 1
                || maxVertsPerPoly < 3
                || maxVertsPerPoly > NMGen.MaxAllowedVertsPerPoly)
            {
                return;
            }

            polys = new ushort[maxPolys * 2 * maxVertsPerPoly];
            verts = new ushort[maxVerts * 3];
            areas = new byte[maxPolys];
            flags = new ushort[maxPolys];
            regions = new ushort[maxPolys];

            this.maxVertsPerPoly = maxVertsPerPoly;
        }

        private void Resize()
        {
            vertCount = 0;
            polyCount = 0;
            maxVertsPerPoly = 0;
            boundsMin = new float[3];
            boundsMax = new float[3];
            xzCellSize = 0;
            yCellSize = 0;
            borderSize = 0;
            walkableHeight = 0;
            walkableStep = 0;
            walkableRadius = 0;
            polys = null;
            verts = null;
            areas = null;
            flags = null;
            regions = null;
        }

        /// <summary>
        /// Checks the size of the buffers to see if they are large enough
        /// to hold the specified data.
        /// </summary>
        /// <param name="maxVerts">The maximum vertices the vertex buffer
        /// needs to hold.</param>
        /// <param name="maxPolys">The maximum polygons the polygon buffer
        /// needs to hold.</param>
        /// <param name="maxVertsPerPoly">The maximum allowed vertices
        /// for a polygon.</param>
        /// <returns>TRUE if all buffers are large enough to fit the data.
        /// </returns>
        public bool CanFit(int vertCount
            , int polyCount
            , int maxVertsPerPoly)
        {
            if (maxVertsPerPoly != this.maxVertsPerPoly
                || polys == null 
                || polys.Length < polyCount * 2 * maxVertsPerPoly
                || verts == null || verts.Length < vertCount * 3
                || areas == null || areas.Length < polyCount
                || flags == null || flags.Length < polyCount
                || regions == null || regions.Length < polyCount)
            {
                return false;
            }
            return true;
        }
    }
}
