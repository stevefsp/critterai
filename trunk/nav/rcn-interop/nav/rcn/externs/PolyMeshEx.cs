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

namespace org.critterai.nav.rcn.externs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PolyMeshEx
    {
        public const ushort NullIndex = 0xffff;
        public const byte NullRegion = 0;
        public const byte WalkableArea = 63;

        private IntPtr mVertices;	// Vertices of the mesh, 3 elements per vertex.
        private IntPtr mPolygons;	// Polygons of the mesh, nvp*2 elements per polygon.
        private IntPtr mRegions;	// Region ID of the polygons.
        private IntPtr mFlags;	// Per polygon flags.
        private IntPtr mAreas;	// Area ID of polygons.
        private int mVertexCount;				// Number of vertices.
        private int mPolygonCount;				// Number of polygons.
        private int mMaxPolygons;			// Number of allocated polygons.
        private int mMaxVertsPerPoly;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private readonly float[] mBoundsMin;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private readonly float[] mBoundsMax;

        private float mCellSize;
        private float mCellHeight;

        /// <summary>
        /// The number of vertices in the vertex array.
        /// </summary>
        public int VertexCount { get { return mVertexCount; } }

        /// <summary>
        /// The actual number of polygons defined in the mesh.
        /// </summary>
        public int PolygonCount { get { return mPolygonCount; } }

        /// <summary>
        /// The maximum number of vertices per polygon.  Individual polygons
        /// can vary in size from three to this number of vertices.
        /// </summary>
        public int MaxVertsPerPoly { get { return mMaxVertsPerPoly; } }

        /// <summary>
        /// The maximum number of polygons that can be defined in the
        /// mesh.
        /// </summary>
        /// <remarks>
        /// If this value is greater than PolygonCount, then
        /// the mesh's data structure contains unused space.
        /// </remarks>
        public int MaxPolygons { get { return mMaxPolygons; } }

        /// <summary>
        /// The xz-plane size of the cells that form the mesh field.
        /// </summary>
        /// <remarks>
        /// <p>See the remarks for GetVertices for details.
        /// </p>
        /// </remarks>
        public float CellSize { get { return mCellSize; } }

        /// <summary>
        /// The y-plane size of the cells that form the mesh field.
        /// </summary>
        /// <remarks>
        /// <p>See the remarks for GetVertices for details.
        /// </p>
        /// </remarks>
        public float CellHeight { get { return mCellHeight; } }

        /// <summary>
        /// The world space minimum bounds of the mesh's axis-aligned
        /// bounding box.
        /// </summary>
        public float[] BoundsMin { get { return (float[])mBoundsMin.Clone(); } }

        /// <summary>
        /// The world space maximum bounds of the mesh's axis-aligned bounding
        /// box.
        /// </summary>
        public float[] BoundsMax { get { return (float[])mBoundsMax.Clone(); } }

        private PolyMeshEx(bool empty)
        {
            // Never allocate memory in this constructor.
            this.mVertices = IntPtr.Zero;
            this.mPolygons = IntPtr.Zero;
            this.mRegions = IntPtr.Zero;
            this.mFlags = IntPtr.Zero;
            this.mAreas = IntPtr.Zero;
            this.mVertexCount = 0;
            this.mPolygonCount = 0;
            this.mMaxPolygons = 0;
            this.mMaxVertsPerPoly = 0;
            this.mBoundsMin = new float[3];
            this.mBoundsMax = new float[3];
            this.mCellSize = 0;
            this.mCellHeight = 0;
        }

        public PolyMeshEx(float cellSize
            , float cellHeight
            , int polygonCount
            , int maxPolygons
            , int maxVertsPerPoly
            , float[] boundsMin
            , float[] boundsMax
            , ushort[] vertices
            , ushort[] polygons
            , ushort[] regions
            , ushort[] flags
            , byte[] areas)
        {
            this.mPolygonCount = (polygons == null || polygonCount < 1 ?
                0 : polygonCount);
            this.mMaxPolygons = (maxPolygons < this.mPolygonCount ?
                0 : maxPolygons);

            if (this.mPolygonCount == 0 || this.mMaxPolygons == 0
                || cellSize <= 0 || cellHeight <= 0 || maxVertsPerPoly < 3
                || vertices == null
                || vertices.Length == 0 || vertices.Length % 3 != 0
                || boundsMin == null || boundsMin.Length != 3
                || boundsMax == null || boundsMax.Length != 3
                || polygons.Length % (maxVertsPerPoly * 2) != 0
                || polygons.Length < maxVertsPerPoly * 2 * this.mPolygonCount
                || (areas != null && areas.Length != this.mMaxPolygons)
                || (regions != null && regions.Length != this.mMaxPolygons)
                || (flags != null && flags.Length != this.mPolygonCount))
            {
                this.mVertices = IntPtr.Zero;
                this.mPolygons = IntPtr.Zero;
                this.mRegions = IntPtr.Zero;
                this.mFlags = IntPtr.Zero;
                this.mAreas = IntPtr.Zero;
                this.mVertexCount = 0;
                this.mPolygonCount = 0;
                this.mMaxPolygons = 0;
                this.mMaxVertsPerPoly = 0;
                this.mBoundsMin = new float[3];
                this.mBoundsMax = new float[3];
                this.mCellSize = 0;
                this.mCellHeight = 0;

                return;
            }

            this.mVertexCount = vertices.Length / 3;
            this.mMaxVertsPerPoly = maxVertsPerPoly;
            this.mCellSize = cellSize;
            this.mCellHeight = cellHeight;

            this.mBoundsMin = new float[3];
            this.mBoundsMax = new float[3];
            for (int i = 0; i < 3; i++)
            {
                this.mBoundsMin[i] = boundsMin[i];
                this.mBoundsMax[i] = boundsMax[i];
            }

            this.mVertices =
                UtilEx.GetFilledBuffer(vertices, vertices.Length);
            this.mPolygons =
                UtilEx.GetFilledBuffer(polygons, polygons.Length);

            if (regions == null)
                this.mRegions = UtilEx.GetBuffer(
                    this.mMaxPolygons * sizeof(ushort), true);
            else
                this.mRegions =
                    UtilEx.GetFilledBuffer(regions, regions.Length);

            if (flags == null)
                this.mFlags = UtilEx.GetBuffer(
                    this.mPolygonCount * sizeof(ushort), true);
            else
                this.mFlags =
                    UtilEx.GetFilledBuffer(flags, flags.Length);

            if (areas == null)
                this.mAreas =
                    UtilEx.GetBuffer(this.mMaxPolygons, true);
            else
                this.mAreas =
                    UtilEx.GetFilledBuffer(areas, areas.Length);
        }

        public bool IsFullyAllocated
        {
            get 
            {
                return !(mVertices == IntPtr.Zero
                    || mPolygons == IntPtr.Zero
                    || mAreas == IntPtr.Zero
                    || mRegions == IntPtr.Zero
                    || mFlags == IntPtr.Zero); 
            }
        }

        public bool HasAllocations
        {
            get
            {
                return (mVertices != IntPtr.Zero
                    || mPolygons != IntPtr.Zero
                    || mAreas != IntPtr.Zero
                    || mRegions != IntPtr.Zero
                    || mFlags != IntPtr.Zero);
            }
        }

        public bool HasVertices
        {
            get { return !(mVertices == IntPtr.Zero || mVertexCount == 0); }
        }

        public bool HasPolygons
        {
            get { return !(mPolygons == IntPtr.Zero || mPolygonCount == 0); }
        }

        /// <summary>
        /// Returns a copy of the mesh's vertex information in the form 
        /// (x, y, z).
        /// </summary>
        /// <returns>
        /// Values are in voxel space.  They can be converted to world space
        /// as follows:
        /// <p>
        /// worldX = boundsMin[0] + x * CellSize
        /// worldY = boundsMin[1] + y * CellHeight
        /// worldZ = boundsMin[2] + z * CellSize
        /// </p></returns>
        public ushort[] GetVertices()
        {
            return (HasVertices ? 
                UtilEx.ExtractArrayUShort(mVertices, mVertexCount * 3) : null);
        }

        /// <summary>
        /// Returns a copy of the mesh's polygon and neighbor information. 
        /// (Stride: 2 * MaxVertsPerPoly)
        /// </summary>
        /// <remarks>
        /// <p>Each entry is 2 * MaxVertsPerPoly with the first 
        /// MaxVertsPerPoly.</p>
        /// <p>The first half of the entry contains the indices of the polygon.
        /// The first instance of NullIndex indicates the end of the indices
        /// for the entry.</p>
        /// <p>The second half contains indices to neighbor polygons.  A
        /// value of NullIndex indicates no connection for the associated edge.
        /// </p>
        /// <p><b>Example:</b></p>
        /// <p>
        /// MaxVertsPerPoly = 6<br/>
        /// For the entry: (1, 3, 4, 8, NullIndex, NullIndex, 18, NullIndex
        /// , 21, NullIndex, NullIndex, NullIndex)</p>
        /// <p>
        /// (1, 3, 4, 8) defines a 4 vertex the polygon.<br />
        /// Edge 1->3 is shared with polygon 18.<br />
        /// Edge 4->8 is shared with polygon 21.<br />
        /// Edges 3->4 and 4->8 are bordner edges not shared with any other
        /// polygon.
        /// </p>
        /// </remarks>
        public ushort[] GetPolygons(bool includeBuffer)
        {
            int length = (includeBuffer ? mMaxPolygons * mMaxVertsPerPoly * 2
                : mPolygonCount * mMaxVertsPerPoly * 2);
            return (HasPolygons ? 
                UtilEx.ExtractArrayUShort(mPolygons, length): null);
        }

        public ushort[] GetRegions(bool includeBuffer)
        {
            int length = (includeBuffer ? mMaxPolygons : mPolygonCount);
            return (mRegions == IntPtr.Zero ? null
                : UtilEx.ExtractArrayUShort(mRegions, length));
        }

        public byte[] GetAreas(bool includeBuffer)
        {
            int length = (includeBuffer ? mMaxPolygons : mPolygonCount);
            return (mAreas == IntPtr.Zero ? null
                : UtilEx.ExtractArrayByte(mAreas, length));
        }

        public ushort[] GetFlags()
        {
            return (mFlags == IntPtr.Zero ? null
                : UtilEx.ExtractArrayUShort(mFlags, mPolygonCount));
        }

        public static PolyMeshEx Empty
        {
            get { return new PolyMeshEx(true); }
        }

        public static void Free(ref PolyMeshEx polyMesh)
        {
            Marshal.FreeHGlobal(polyMesh.mAreas);
            Marshal.FreeHGlobal(polyMesh.mFlags);
            Marshal.FreeHGlobal(polyMesh.mPolygons);
            Marshal.FreeHGlobal(polyMesh.mRegions);
            Marshal.FreeHGlobal(polyMesh.mVertices);
            polyMesh.mAreas = IntPtr.Zero;
            polyMesh.mFlags = IntPtr.Zero;
            polyMesh.mPolygons = IntPtr.Zero;
            polyMesh.mRegions = IntPtr.Zero;
            polyMesh.mVertices = IntPtr.Zero;
            Array.Clear(polyMesh.mBoundsMax, 0, polyMesh.mBoundsMax.Length);
            Array.Clear(polyMesh.mBoundsMin, 0, polyMesh.mBoundsMin.Length);
            polyMesh.mCellHeight = 0;
            polyMesh.mCellSize = 0;
            polyMesh.mPolygonCount = 0;
            polyMesh.mVertexCount = 0;
            polyMesh.mMaxPolygons = 0;
            polyMesh.mMaxVertsPerPoly = 0;
        }

        [DllImport("cai-nav-rcn", EntryPoint = "freeRCPolyMesh")]
        public static extern void FreeEx(ref PolyMeshEx polyMesh);
    }
}
