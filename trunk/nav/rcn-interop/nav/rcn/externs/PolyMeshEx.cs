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

namespace org.critterai.nav.rcn.externs
{
    /// <summary>
    /// Represents a polygon mesh suitable for use in building a dtNavMesh
    /// object.
    /// </summary>
    /// <remarks>
    /// <p>The polygon mesh is made up of convex, potentailly overlapping
    /// polygons of varying sizes.  Each polygon can have extra meta-data
    /// representing region and area membership, as well as user defined flags.
    /// </p>
    /// <p>An instance created using the default constructor is of no use.  The
    /// structure must be build using a non-default constructor or one of
    /// the interop methods that creates the structure in native code.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct PolyMeshEx
    {
        /// <summary>
        /// Represents an index that does not point to anything.
        /// </summary>
        public const ushort NullIndex = 0xffff;

        /// <summary>
        /// Represents the null region, a region which is not part of the
        /// polygon mesh.  (E.g. An edge on the boundary of the mesh connects
        /// to the null region.)
        /// </summary>
        public const byte NullRegion = 0;

        /// <summary>
        /// The default area id for externally built polygon meshes, indicating
        /// a walkable polygon.
        /// </summary>
        public const byte WalkableArea = 63;

        private IntPtr mVertices;
        private IntPtr mPolygons;
        private IntPtr mRegions;
        private IntPtr mFlags;
        private IntPtr mAreas;
        private int mVertexCount;
        private int mPolygonCount;	
        private int mMaxPolygons;	
        private int mMaxVertsPerPoly;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private readonly float[] mBoundsMin;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private readonly float[] mBoundsMax;

        private float mXZCellSize;
        private float mYCellSize;

        /// <summary>
        /// The number of vertices in the vertex array.
        /// </summary>
        public int VertexCount { get { return mVertexCount; } }

        /// <summary>
        /// The actual number of polygons defined by the mesh.
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
        /// the mesh's data structure contains unused (buffer) space.
        /// </remarks>
        public int MaxPolygons { get { return mMaxPolygons; } }

        /// <summary>
        /// The xz-plane size of the cells that form the mesh field.
        /// </summary>
        /// <remarks>
        /// <p>See the <see cref="GetVetices"/> method for details.
        /// </p>
        /// </remarks>
        public float XZCellSize { get { return mXZCellSize; } }

        /// <summary>
        /// The y-axis size of the cells that form the mesh field.
        /// </summary>
        /// <remarks>
        /// <p>See the <see cref="GetVetices"/> method for details.
        /// </p>
        /// </remarks>
        public float YCellSize { get { return mYCellSize; } }

        /// <summary>
        /// The world space minimum bounds of the mesh's axis-aligned
        /// bounding box. (x, y, z)
        /// </summary>
        public float[] BoundsMin 
        { 
            get 
            { 
                return mBoundsMin == null ? null : (float[])mBoundsMin.Clone();
            } 
        }

        /// <summary>
        /// The world space maximum bounds of the mesh's axis-aligned bounding
        /// box. (x, y, z)
        /// </summary>
        public float[] BoundsMax { 
            get 
            {
                return mBoundsMax == null ? null : (float[])mBoundsMax.Clone(); 
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Any mesh constructed using a non-default constructor must be freed
        /// using the <see cref="Free"/> method.  Otherwise a memory leak will
        /// occur.</remarks>
        /// <param name="xzCellSize">
        /// The xz-plane size of the cells that form the mesh field.</param>
        /// <param name="yCellSize">The y-axis size of the cells that form 
        /// the mesh field.</param>
        /// <param name="polygonCount">The actual number of polygons defined 
        /// by the mesh.</param>
        /// <param name="maxPolygons">The maximum number of polygons that can 
        /// be defined in the mesh.</param>
        /// <param name="maxVertsPerPoly">The maximum number of vertices per 
        /// polygon.</param>
        /// <param name="boundsMin">The world space minimum bounds of the 
        /// mesh's axis-aligned bounding box. (x, y, z)</param>
        /// <param name="boundsMax">The world space maximum bounds of the 
        /// mesh's axis-aligned bounding box. (x, y, z)</param>
        /// <param name="vertices">The mesh's vertex information.
        /// (x, y, z) * VertexCount</param>
        /// <param name="polygons">The mesh's polygon and neighbor
        /// information. (Size: 2 * MaxVertsPerPoly * MaxPolygons)</param>
        /// <param name="regions">The region id for each polygon. 
        /// (Size: MaxPolygons) (Optional)</param>
        /// <param name="flags">The flags for each polygon. (Size: MaxPolygons)
        /// (Optional)</param>
        /// <param name="areas">The area id for each polygon. 
        /// (Size: MaxPolygons) (Optional)</param>
        public PolyMeshEx(float xzCellSize
            , float yCellSize
            , int polygonCount
            , int maxPolygons
            , int maxVertsPerPoly
            , float[] boundsMin
            , float[] boundsMax
            , ushort[] vertices
            , ushort[] polygons
            , ushort[] regions
            , ushort[] flags
            , byte[] areaIds)
        {
            this.mPolygonCount = (polygons == null || polygonCount < 1 ?
                0 : polygonCount);
            this.mMaxPolygons = (maxPolygons < this.mPolygonCount ?
                0 : maxPolygons);

            if (this.mPolygonCount == 0 || this.mMaxPolygons == 0
                || xzCellSize <= 0 || yCellSize <= 0 || maxVertsPerPoly < 3
                || vertices == null
                || vertices.Length == 0 || vertices.Length % 3 != 0
                || boundsMin == null || boundsMin.Length != 3
                || boundsMax == null || boundsMax.Length != 3
                || polygons.Length % (maxVertsPerPoly * 2) != 0
                || polygons.Length < maxVertsPerPoly * 2 * this.mPolygonCount
                || (areaIds != null && areaIds.Length != this.mMaxPolygons)
                || (regions != null && regions.Length != this.mMaxPolygons)
                || (flags != null && flags.Length != this.mMaxPolygons))
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
                this.mXZCellSize = 0;
                this.mYCellSize = 0;

                return;
            }

            this.mVertexCount = vertices.Length / 3;
            this.mMaxVertsPerPoly = maxVertsPerPoly;
            this.mXZCellSize = xzCellSize;
            this.mYCellSize = yCellSize;

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
                    this.mMaxPolygons * sizeof(ushort), true);
            else
                this.mFlags =
                    UtilEx.GetFilledBuffer(flags, flags.Length);

            if (areaIds == null)
                this.mAreas = 
                    UtilEx.GetBuffer(this.mMaxPolygons * sizeof(byte), true);
            else
                this.mAreas =
                    UtilEx.GetFilledBuffer(areaIds, areaIds.Length);
        }

        /// <summary>
        /// Returns a copy of the vertex information. (x, y, z) *
        /// VertexCount
        /// </summary>
        /// <returns>
        /// <p>Values are in voxel space.  They can be converted to world space
        /// as follows:</p>
        /// <p>
        /// worldX = boundsMin[0] + x * XZCellSize
        /// worldY = boundsMin[1] + y * YCellSize
        /// worldZ = boundsMin[2] + z * XZCellSize
        /// </p></returns>
        public ushort[] GetVertices()
        {
            return (mVertexCount > 0 ? 
                UtilEx.ExtractArrayUShort(mVertices, mVertexCount * 3) : null);
        }

        /// <summary>
        /// Returns a copy of polygon and neighbor information. 
        /// </summary>
        /// <remarks>
        /// <p>Each entry is 2 * MaxVertsPerPoly in length.</p>
        /// <p>The first half of the entry contains the indices of the polygon.
        /// The first instance of <see cref="NullIndex"/> indicates the end of 
        /// the indices for the entry.</p>
        /// <p>The second half contains indices to neighbor polygons.  A
        /// value of <see cref="NullIndex"/> indicates no connection for the 
        /// associated edge. </p>
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
        /// <param name="includeBuffer">If TRUE, the entire array, including
        /// unused buffer space, is returned.</param>
        /// <returns>A copy of polygon and neighbor information.</returns>
        public ushort[] GetPolygons(bool includeBuffer)
        {
            int length = (includeBuffer ? mMaxPolygons * mMaxVertsPerPoly * 2
                : mPolygonCount * mMaxVertsPerPoly * 2);
            return (length > 0 ? 
                UtilEx.ExtractArrayUShort(mPolygons, length): null);
        }

        /// <summary>
        /// Returns a copy of the region id for each polygon.
        /// </summary>
        /// <remarks>
        /// This information is generated during the mesh build process.
        /// </remarks>
        /// <param name="includeBuffer">If TRUE, the entire array, including
        /// unused buffer space, is returned.</param>
        /// <returns>A copy of region id for each polygon.
        /// </returns>
        public ushort[] GetRegions(bool includeBuffer)
        {
            int length = (includeBuffer ? mMaxPolygons : mPolygonCount);
            return (mRegions == IntPtr.Zero ? null
                : UtilEx.ExtractArrayUShort(mRegions, length));
        }

        /// <summary>
        /// Returns a copy of the area id for each polygon.
        /// </summary>
        /// <remarks>
        /// <p>During the standard build process, all walkable polygons
        /// get the default value of <see cref="WalkableArea"/>.  
        /// This value can then be changed to meet user requirements.</p>
        /// </remarks>
        /// <param name="includeBuffer">If TRUE, the entire array, including
        /// unused buffer space, is returned.</param>
        /// <returns>A copy of area id for each polygon.</returns>
        public byte[] GetAreas(bool includeBuffer)
        {
            int length = (includeBuffer ? mMaxPolygons : mPolygonCount);
            return (mAreas == IntPtr.Zero ? null
                : UtilEx.ExtractArrayByte(mAreas, length));
        }

        /// <summary>
        /// Sets the user-defined area id for each polygon.
        /// </summary>
        /// <remarks>The size of the area array may be larger than
        /// <see cref="MaxPolygons"/>.  Only the data up to MaxPolygons
        /// will be copied.</remarks>
        /// <param name="areas">The area id for each polygon. 
        /// (Size: >= PolygonCount)</param>
        public void SetAreas(byte[] areas)
        {
            if (areas == null || areas.Length < mPolygonCount)
                return;
            int length = Math.Min(mMaxPolygons, areas.Length);
            Marshal.Copy(areas, 0, mAreas, length);
        }

        /// <summary>
        /// Returns a copy of the user-defined flags for each polygon.
        /// </summary>
        /// <param name="includeBuffer">If TRUE, the entire array, including
        /// unused buffer space, is returned.</param>
        /// <returns>A copy of the flags for each polygon.</returns>
        public ushort[] GetFlags(bool includeBuffer)
        {
            int length = (includeBuffer ? mMaxPolygons : mPolygonCount);
            return (mFlags == IntPtr.Zero ? null
                : UtilEx.ExtractArrayUShort(mFlags, length));
        }

        /// <summary>
        /// Sets the user-defined flags for each polygon.
        /// </summary>
        /// <remarks>The size of the flags array may be larger than
        /// <see cref="MaxPolygons"/>.  Only the data up to MaxPolygons
        /// will be copied.</remarks>
        /// <param name="flags">The flags for each polygon. 
        /// (Size: >= PolygonCount)</param>
        public void SetFlags(ushort[] flags)
        {
            if (flags == null || flags.Length < mPolygonCount)
                return;
            int length = Math.Min(mMaxPolygons, flags.Length);
            UtilEx.Copy(flags, 0, mFlags, length);
        }

        /// <summary>
        /// Frees the unmanaged resources for a mesh created using a local
        /// non-default constructor.
        /// </summary>
        /// <remarks>
        /// <p>This method does not have to be called if the mesh was created
        /// using the default constructor.</p>
        /// <p>Behavior is undefined if this method is used on
        /// a mesh created by an interop method.</p>
        /// </remarks>
        /// <param name="detailMesh">The mesh to free.</param>
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
            polyMesh.mYCellSize = 0;
            polyMesh.mXZCellSize = 0;
            polyMesh.mPolygonCount = 0;
            polyMesh.mVertexCount = 0;
            polyMesh.mMaxPolygons = 0;
            polyMesh.mMaxVertsPerPoly = 0;
        }

        /// <summary>
        /// Frees the unmanaged resources for a mesh created using an
        /// native interop method.
        /// </summary>
        /// <remarks>
        /// Behavior is undefined if this method is called on a
        /// mesh created by a local constructor.</remarks>
        /// <param name="detailMesh">The mesh to free.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "freeRCPolyMesh")]
        public static extern void FreeEx(ref PolyMeshEx polyMesh);
    }
}
