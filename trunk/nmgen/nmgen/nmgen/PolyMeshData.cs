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
        /// The mesh vertices.
        /// </summary>
        public ushort[] verts;

        /// <summary>
        /// The mesh polygon and neighbor data.
        /// </summary>
        public ushort[] polys;

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
        /// The minimum bounds of the mesh.
        /// </summary>
        public float[] boundsMin = new float[3];

        /// <summary>
        /// The maximum bounds of the mesh.
        /// </summary>
        public float[] boundsMax = new float[3];

        /// <summary>
        /// The xz-plane cell size.
        /// </summary>
        public float xzCellSize;

        /// <summary>
        /// the y-axis cell height.
        /// </summary>
        public float yCellSize;

        /// <summary>
        /// The the closest any part of the polygon mesh gets to obstructions 
        /// in the source geometry.
        /// </summary>
        public int borderSize;

        /// <summary>
        /// The Minimum floor to 'ceiling' height used to build the polygon
        /// mesh.
        /// </summary>
        public float walkableHeight;

        /// <summary>
        /// The maximum traversable ledge height used to build the polygon
        /// mesh.
        /// </summary>
        public float walkableStep;

        // public PolyMeshData() { }

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

        public void Reset(int maxVerts
            , int maxPolys
            , int maxVertsPerPoly)
        {
            Reset();

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

        public void Reset()
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
            polys = null;
            verts = null;
            areas = null;
            flags = null;
            regions = null;
        }

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
