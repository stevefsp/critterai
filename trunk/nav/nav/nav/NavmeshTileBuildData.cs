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
using org.critterai.interop;
using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    /// <summary>
    /// Represents the data used to build a <see cref="NavmeshTile"/>.
    /// </summary>
    /// <remarks>
    /// <para>This class marshals data between the navigation mesh generation
    /// pipeline and the navigation system.  The NMGen PolyMesh and
    /// PolyMeshDetail classes provide details on data stucture and limits.
    /// </para>
    /// <para>The standard load process is as follows:</para>
    /// <ol>
    /// <li>Construct the object.</li>
    /// <li><see cref="LoadBase"/></li>
    /// <li><see cref="LoadPolys"/></li>
    /// <li><see cref="LoadDetail"/> (Optional.)</li>
    /// <li><see cref="LoadConns"/> (Optional.)</li>
    /// </ol>
    /// <para>The design permits re-use of the data buffers for multiple tile
    /// builds.  Just set the buffer sizes to the maximum size required then
    /// use the load methods to reload the data as needed.  The only restriction
    /// is that all polygon data must be have the same maximum vertices
    /// per polygon.</para>
    /// <para>Behavior is undefined if an object is used after disposal.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NavmeshTileBuildData
    {
        /// <summary>
        /// The minimum allowed cell size.
        /// </summary>
        public const float MinCellSize = 0.01f;

        /// <summary>
        /// The absolute minimum allowed value for <see cref="WalkableHeight"/>.
        /// </summary>
        public const float MinAllowedTraversableHeight = 3 * MinCellSize;

        #region dtNavMeshCreateParams Fields (private)

        /// <summary>
        /// ushort buffer.
        /// </summary>
        private IntPtr mPolyVerts = IntPtr.Zero;

        private int mPolyVertCount = 0;

        /// <summary>
        /// ushort buffer.
        /// </summary>
        private IntPtr mPolys = IntPtr.Zero;

        /// <summary>
        /// ushort buffer.
        /// </summary>
        private IntPtr mPolyFlags = IntPtr.Zero;

        /// <summary>
        /// byte buffer
        /// </summary>
        private IntPtr mPolyAreas = IntPtr.Zero;

        private int mPolyCount = 0;
        private int mMaxVertsPerPoly = 0;

        /// <summary>
        /// uint buffer
        /// </summary>
        /// <remarks>
        /// Note: Used by Detour to detect if there detail meshes
        /// are available.
        /// </remarks>
        private IntPtr mDetailMeshes = IntPtr.Zero;

        /// <summary>
        /// float buffer
        /// </summary>
        private IntPtr mDetailVerts = IntPtr.Zero;

        private int mDetailVertCount = 0;

        /// <summary>
        /// byte array
        /// </summary>
        private IntPtr mDetailTris = IntPtr.Zero;

        private int mDetailTriCount = 0;

        /// <summary>
        /// float array
        /// </summary>
        private IntPtr mConnVerts = IntPtr.Zero;

        /// <summary>
        /// float array
        /// </summary>
        private IntPtr mConnRadii = IntPtr.Zero;

        /// <summary>
        /// ushort array
        /// </summary>
        private IntPtr mConnFlags = IntPtr.Zero;

        /// <summary>
        /// byte array
        /// </summary>
        private IntPtr mConnAreas = IntPtr.Zero;

        /// <summary>
        /// byte array
        /// </summary>
        private IntPtr mConnDirs = IntPtr.Zero;

        /// <summary>
        /// uint array
        /// </summary>
        private IntPtr mConnUserIds = IntPtr.Zero;

        /// <summary>
        /// Connection Count
        /// </summary>
        /// <remarks>
        /// Note: Used by Detour to detect if there are any connections.
        /// </remarks>
        private int mConnCount = 0;

        private uint mTileUserId = 0;
        private int mTileX = 0;
        private int mTileY = 0;
        private int mTileLayer = 0;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private float[] mBoundsMin = new float[3];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private float[] mBoundsMax = new float[3];

        private float mWalkableHeight = 0;
        private float mWalkableRadius = 0;
        private float mWalkableStep = 0;
        private float mXZCellSize = 0;
        private float mYCellSize = 0;
        private bool mBVTreeEnabled = false;

        #endregion

        #region rcnNavMeshCreateParams Fields (private)

        private bool mIsDisposed = true;  // Default is accurate.

        private int mMaxPolyVerts = 0;
        private int mMaxPolys = 0;

        private int mMaxDetailVerts = 0;
        private int mMaxDetailTris = 0;

        private int mMaxConns = 0;

        #endregion

        /// <summary>
        /// The maximum allowed polygon vertices.
        /// </summary>
        public int MaxPolyVerts { get { return mMaxPolyVerts; } }

        /// <summary>
        /// The maximum allowed polygons.
        /// </summary>
        public int MaxPolys { get { return mMaxPolys; } }

        /// <summary>
        /// The maximum allowed detail vertices.
        /// </summary>
        public int MaxDetailVerts { get { return mMaxDetailVerts; } }

        /// <summary>
        /// The maximum allowed detail triangles.
        /// </summary>
        public int MaxDetailTris { get { return mMaxDetailTris; } }

        /// <summary>
        /// The maximum allowed off-mesh connections.
        /// </summary>
        public int MaxConns { get { return mMaxConns; } }

        /// <summary>
        /// The number of polygon vertices.
        /// </summary>
        public int PolyVertCount { get { return mPolyVertCount; } }

        /// <summary>
        /// The number of polygons.
        /// </summary>
        public int PolyCount { get { return mPolyCount; } }

        /// <summary>
        /// The maximum vertices per polygon.
        /// </summary>
        public int MaxVertsPerPoly { get { return mMaxVertsPerPoly; } }

        /// <summary>
        /// The number of detail vertices.
        /// </summary>
        public int DetailVertCount { get { return mDetailVertCount; } }

        /// <summary>
        /// The number of detail triangles.
        /// </summary>
        public int DetailTriCount { get { return mDetailTriCount; } }

        /// <summary>
        /// The number of off-mesh connections.
        /// </summary>
        public int ConnCount { get { return mConnCount; } }

        /// <summary>
        /// The user id of the tile.
        /// </summary>
        public uint TileUserId { get { return mTileUserId; } }

        /// <summary>
        /// The tile's grid x-location.
        /// </summary>
        public int TileX { get { return mTileX; } }

        /// <summary>
        /// The tile's grid z-location.
        /// </summary>
        public int TileZ { get { return mTileY; } }

        /// <summary>
        /// The tile's layer.
        /// </summary>
        public int TileLayer { get { return mTileLayer; } }

        /// <summary>
        /// The minimum bounds of the tile's AABB. 
        /// </summary>
        /// <remarks>Form: (x, y, z)</remarks>
        /// <returns>The mimumum bounds of the tile.</returns>
        public float[] GetBoundsMin() { return (float[])mBoundsMin.Clone(); }

        /// <summary>
        /// The maximum bounds of the tile's AABB. 
        /// </summary>
        /// <remarks>Form: (x, y, z)</remarks>
        /// <returns>The maximum bounds of the tile.</returns>
        public float[] GetBoundsMax() { return (float[])mBoundsMax.Clone(); }

        /// <summary>
        /// The walkable height to use for the tile.
        /// </summary>
        public float WalkableHeight { get { return mWalkableHeight; } }

        /// <summary>
        /// The walkable radius to use for the tile.
        /// </summary>
        public float WalkableRadius { get { return mWalkableRadius; } }

        /// <summary>
        /// The walkable step to use for the tile.
        /// </summary>
        public float MaxTraversableStep { get { return mWalkableStep; } }

        /// <summary>
        /// The xz-plane cell size of the tile.
        /// </summary>
        public float XZCellSize { get { return mXZCellSize; } }

        /// <summary>
        /// The y-axis cell size of the tile.
        /// </summary>
        public float YCellSize { get { return mYCellSize; } }

        /// <summary>
        /// TRUE if bounding volumn data should be generated for the tile.
        /// </summary>
        /// <remarks>This value is normally set to FALSE if the tile is small
        /// or layers are being used.</remarks>
        public bool BVTreeEnabled { get { return mBVTreeEnabled; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return mIsDisposed; } }

        private void InitializeBuffers(int maxPolyVerts
                , int maxPolys
                , int maxVertsPerPoly
                , int maxDetailVerts
                , int maxDetailTris
                , int maxConns)
        {
            mMaxPolyVerts = maxPolyVerts;
            mMaxPolys = maxPolys;
            mMaxDetailVerts = maxDetailVerts;
            mMaxDetailTris = maxDetailTris;
            mMaxConns = maxConns;

            mMaxVertsPerPoly = maxVertsPerPoly;

            // Polygons

            int size = maxPolyVerts * sizeof(ushort) * 3;
            mPolyVerts = UtilEx.GetBuffer(size, true);

            size = maxPolys * sizeof(ushort) * 2 * mMaxVertsPerPoly;
            mPolys = UtilEx.GetBuffer(size, true);

            size = maxPolys * sizeof(ushort);
            mPolyFlags = UtilEx.GetBuffer(size, true);

            size = maxPolys * sizeof(byte);
            mPolyAreas = UtilEx.GetBuffer(size, true);

            // Detail meshes

            if (maxDetailTris > 0)
            {
                size = maxDetailVerts * sizeof(float) * 3;
                mDetailVerts = UtilEx.GetBuffer(size, true);

                size = maxDetailTris * sizeof(byte) * 4;
                mDetailTris = UtilEx.GetBuffer(size, true);

                size = maxPolys * sizeof(uint) * 4;
                mDetailMeshes = UtilEx.GetBuffer(size, true);
            }

            // Connections

            if (maxConns > 0)
            {
                size = maxConns * sizeof(float) * 6;
                mConnVerts = UtilEx.GetBuffer(size, true);

                size = maxConns * sizeof(float);
                mConnRadii = UtilEx.GetBuffer(size, true);

                size = maxConns * sizeof(ushort);
                mConnFlags = UtilEx.GetBuffer(size, true);

                size = maxConns * sizeof(byte);
                mConnAreas = UtilEx.GetBuffer(size, true);
                mConnDirs = UtilEx.GetBuffer(size, true);

                size = maxConns * sizeof(uint);
                mConnUserIds = UtilEx.GetBuffer(size, true);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// <para>If invalid parameter values are supplied the resulting object
        /// will not be usable. (Buffers won't be initialized, all maximum
        /// size values set to zero.)</para>
        /// </remarks>
        /// <param name="maxPolyVerts">The maximum allowed polygon vertices.
        /// </param>
        /// <param name="maxPolys">The maximum allowed polygons.</param>
        /// <param name="maxVertsPerPoly">The maximum vertices per polygon.
        /// </param>
        /// <param name="maxDetailVerts">The maximum allowed detail vertices.
        /// </param>
        /// <param name="maxDetailTris">The maximum allowed detail vetices.
        /// </param>
        /// <param name="maxConns">The maximum allowed off-mesh connections.
        /// </param>
        public NavmeshTileBuildData(int maxPolyVerts
                , int maxPolys
                , int maxVertsPerPoly
                , int maxDetailVerts
                , int maxDetailTris
                , int maxConns)
        {
            if (!IsValid(maxPolyVerts
                , maxPolys
                , maxVertsPerPoly
                , maxDetailVerts
                , maxDetailTris
                , maxConns))
            {
                return;
            }

            InitializeBuffers(maxPolyVerts
                , maxPolys
                , maxVertsPerPoly
                , maxDetailVerts
                , maxDetailTris
                , maxConns);

            mIsDisposed = false;
        }

        /// <summary>
        /// Constructor. (No off-mesh connections.)
        /// </summary>
        /// <remarks>
        /// <para>If invalid parameter values are supplied the resulting object
        /// will not be usable. (Buffers won't be initialized, all maximum
        /// size values set to zero.)</para>
        /// </remarks>
        /// <param name="maxPolyVerts">The maximum allowed polygon vertices.
        /// </param>
        /// <param name="maxPolys">The maximum allowed polygons.</param>
        /// <param name="maxVertsPerPoly">The maximum vertices per polygon.
        /// </param>
        /// <param name="maxDetailVerts">The maximum allowed detail vertices.
        /// </param>
        /// <param name="maxDetailTris">The maximum allowed detail vetices.
        /// </param>
        public NavmeshTileBuildData(int maxPolyVerts
                , int maxPolys
                , int maxVertsPerPoly
                , int maxDetailVerts
                , int maxDetailTris)
        {
            if (!IsValid(maxPolyVerts
               , maxPolys
               , maxVertsPerPoly
               , maxDetailVerts
               , maxDetailTris
               , 0))
            {
                return;
            }

            InitializeBuffers(maxPolyVerts
                , maxPolys
                , maxVertsPerPoly
                , maxDetailVerts
                , maxDetailTris
                , 0);

            mIsDisposed = false;
        }

        /// <summary>
        /// Constructor. (No detail mesh.)
        /// </summary>
        /// <remarks>
        /// <para>If invalid parameter values are supplied the resulting object
        /// will not be usable. (Buffers won't be initialized, all maximum
        /// size values set to zero.)</para>
        /// </remarks>
        /// <param name="maxPolyVerts">The maximum allowed polygon vertices.
        /// </param>
        /// <param name="maxPolys">The maximum allowed polygons.</param>
        /// <param name="maxVertsPerPoly">The maximum vertices per polygon.
        /// </param>
        /// <param name="maxConns">The maximum allowed off-mesh connections.
        /// </param>
        public NavmeshTileBuildData(int maxPolyVerts
                , int maxPolys
                , int maxVertsPerPoly
                , int maxConns)
        {
            if (!IsValid(maxPolyVerts
              , maxPolys
              , maxVertsPerPoly
              , 0
              , 0
              , maxConns))
            {
                return;
            }

            InitializeBuffers(maxPolyVerts
                , maxPolys
                , maxVertsPerPoly
                , 0
                , 0
                , maxConns);

            mIsDisposed = false;
        }

        /// <summary>
        /// Constructor. (No detail mesh or off-mesh connections.)
        /// </summary>
        /// <remarks>
        /// <para>If invalid parameter values are supplied the resulting object
        /// will not be usable. (Buffers won't be initialized, all maximum
        /// size values set to zero.)</para>
        /// </remarks>
        /// <param name="maxPolyVerts">The maximum allowed polygon vertices.
        /// </param>
        /// <param name="maxPolys">The maximum allowed polygons.</param>
        /// <param name="maxVertsPerPoly">The maximum vertices per polygon.
        /// </param>
        public NavmeshTileBuildData(int maxPolyVerts
                , int maxPolys
                , int maxVertsPerPoly)
        {
            if (!IsValid(maxPolyVerts
                , maxPolys
                , maxVertsPerPoly
                , 0
                , 0
                , 0))
            {
                return;
            }

            InitializeBuffers(maxPolyVerts
                , maxPolys
                , maxVertsPerPoly
                , 0
                , 0
                , 0);

            mIsDisposed = false;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~NavmeshTileBuildData()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes of all resources and marks the object as disposed.
        /// </summary>
        public void Dispose()
        {
            if (mIsDisposed)
                return;

            // Polygons
            if (mPolys != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mPolyVerts);
                Marshal.FreeHGlobal(mPolys);
                Marshal.FreeHGlobal(mPolyFlags);
                Marshal.FreeHGlobal(mPolyAreas);
            }

            // Detail meshes
            if (mDetailMeshes != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mDetailVerts);
                Marshal.FreeHGlobal(mDetailTris);
                Marshal.FreeHGlobal(mDetailMeshes);
            }

            // Connections

            if (mConnVerts != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mConnVerts);
                Marshal.FreeHGlobal(mConnRadii);
                Marshal.FreeHGlobal(mConnFlags);
                Marshal.FreeHGlobal(mConnAreas);
                Marshal.FreeHGlobal(mConnDirs);
                Marshal.FreeHGlobal(mConnUserIds);
            }

            Array.Clear(mBoundsMin, 0, 3);
            Array.Clear(mBoundsMax, 0, 3);

            mMaxVertsPerPoly = 0;

            mPolyVerts = IntPtr.Zero;
            mPolys = IntPtr.Zero;
            mPolyFlags = IntPtr.Zero;
            mPolyAreas = IntPtr.Zero;

            mDetailVerts = IntPtr.Zero;
            mDetailTris = IntPtr.Zero;
            mDetailMeshes = IntPtr.Zero;

            mConnVerts = IntPtr.Zero;
            mConnRadii = IntPtr.Zero;
            mConnFlags = IntPtr.Zero;
            mConnAreas = IntPtr.Zero;
            mConnDirs = IntPtr.Zero;
            mConnUserIds = IntPtr.Zero;

            mPolyVertCount = 0;
            mPolyCount = 0;
            mDetailVertCount = 0;
            mDetailTriCount = 0;
            mConnCount = 0;
            mTileUserId = 0;
            mTileX = 0;
            mTileY = 0;
            mTileLayer = 0;
            mWalkableHeight = 0;
            mWalkableStep = 0;
            mWalkableRadius = 0;
            mXZCellSize = 0;
            mYCellSize = 0;
            mBVTreeEnabled = false;

            mMaxConns = 0;
            mMaxDetailTris = 0;
            mMaxDetailVerts = 0;
            mMaxPolys = 0;
            mMaxPolyVerts = 0;

            mIsDisposed = true;
        }

        /// <summary>
        /// Load the base configuration.
        /// </summary>
        /// <param name="tileX">The grid x-location.</param>
        /// <param name="tileZ">The grid y-location.</param>
        /// <param name="tileLayer">The layer.</param>
        /// <param name="tileUserId">The tile's user assigned id.</param>
        /// <param name="boundsMin">The minumum bounds. [Form: (x, y, z)]
        /// </param>
        /// <param name="boundsMax">The maximum bounds. [Form: (x, y, z)]
        /// </param>
        /// <param name="xzCellSize">The xz-plane cell size.</param>
        /// <param name="yCellSize">The y-axis cell size.</param>
        /// <param name="walkableHeight">The walkable height.</param>
        /// <param name="walkableRadius">The walkable radius.</param>
        /// <param name="walkableStep">The walkable step.</param>
        /// <param name="bvTreeEnabled">TRUE if bounding volumes should be
        /// used.</param>
        /// <returns>TRUE if the load succeeded.</returns>
        public bool LoadBase(int tileX
            , int tileZ
            , int tileLayer
            , uint tileUserId
            , float[] boundsMin
            , float[] boundsMax
            , float xzCellSize
            , float yCellSize
            , float walkableHeight
            , float walkableRadius
            , float walkableStep
            , bool bvTreeEnabled)
        {
            if (mIsDisposed
                || boundsMin == null || boundsMin.Length < 3
                || boundsMax == null || boundsMax.Length < 3
                || xzCellSize < MinCellSize
                || yCellSize < MinCellSize
                || walkableHeight < MinAllowedTraversableHeight
                || walkableStep < 0
                || walkableRadius < 0)
            {
                return false;
            }

            mTileX = tileX;
            mTileY = tileZ;
            mTileLayer = tileLayer;
            mTileUserId = tileUserId;

            Array.Copy(boundsMin, mBoundsMin, 3);
            Array.Copy(boundsMax, mBoundsMax, 3);
            mXZCellSize = xzCellSize;
            mYCellSize = yCellSize;

            mWalkableHeight = walkableHeight;
            mWalkableStep = walkableStep;
            mWalkableRadius = walkableRadius;

            mBVTreeEnabled = bvTreeEnabled;

            return true;
        }

        /// <summary>
        /// Load the polygon data.
        /// </summary>
        /// <param name="polyVerts">Polygon vertices.</param>
        /// <param name="vertCount">The number of vertices.</param>
        /// <param name="polys">Polygons.</param>
        /// <param name="polyFlags">Polygon flags.</param>
        /// <param name="polyAreas">Polygon areas ids.</param>
        /// <param name="polyCount">The number of polygons.</param>
        /// <returns>TRUE if the load succeeded.</returns>
        public bool LoadPolys(ushort[] polyVerts
            , int vertCount
            , ushort[] polys
            , ushort[] polyFlags
            , byte[] polyAreas
            , int polyCount)
        {
            if (mIsDisposed
                || vertCount < 3
                || polyCount < 1
                || polyVerts == null
                || polyVerts.Length < 3 * vertCount
                || vertCount > mMaxPolyVerts
                || polys == null
                || polys.Length < polyCount * 2 * mMaxVertsPerPoly
                || polyCount > mMaxPolys
                || polyFlags == null
                || polyFlags.Length < polyCount
                || polyAreas == null
                || polyAreas.Length < polyCount)
            {
                return false;
            }

            // Design note: Considered allowing null flags and areas, then
            // filling the arrays with default values.  But haven't found
            // a memset equivalent.  So don't see a memory/processing efficient
            // way to implement it.

            mPolyVertCount = vertCount;
            mPolyCount = polyCount;

            UtilEx.Copy(polyVerts, 0, mPolyVerts, vertCount * 3);
            UtilEx.Copy(polys, 0, mPolys, mPolyCount * 2 * mMaxVertsPerPoly);

            UtilEx.Copy(polyFlags, 0, mPolyFlags, mPolyCount);
            Marshal.Copy(polyAreas, 0, mPolyAreas, mPolyCount);

            return true;
        }

        /// <summary>
        /// Load the detail mesh data.
        /// </summary>
        /// <param name="verts">Detail vertices.</param>
        /// <param name="vertCount">The number of detail vertices.</param>
        /// <param name="tris">Detail triangles.</param>
        /// <param name="triCount">The number of detail triagles.</param>
        /// <param name="meshes">Detail meshes.</param>
        /// <param name="meshCount">The number of detail meshes.</param>
        /// <returns>TRUE if the load succeeded.</returns>
        public bool LoadDetail(float[] verts
            , int vertCount
            , byte[] tris
            , int triCount
            , uint[] meshes
            , int meshCount)
        {
            if (mIsDisposed
                || mDetailMeshes == IntPtr.Zero
                || vertCount < 3
                || triCount < 1
                || verts == null
                || verts.Length < vertCount * 3
                || vertCount > mMaxDetailVerts
                || tris == null
                || tris.Length < triCount * 4
                || triCount > mMaxDetailTris
                || meshes == null
                || meshes.Length < meshCount * 4
                || meshCount > mMaxPolys)
            {
                return false;
            }

            mDetailVertCount = vertCount;
            mDetailTriCount = triCount;

            // Necessary since polys may not be loaded yet.
            // The poly load will override this.
            if (mPolyCount == 0)
                mPolyCount = meshCount;

            Marshal.Copy(verts, 0, mDetailVerts, vertCount * 3);
            Marshal.Copy(tris, 0, mDetailTris, triCount * 4);
            UtilEx.Copy(meshes, 0, mDetailMeshes, meshCount * 4);

            return true;
        }

        /// <summary>
        /// Load the off-mesh connection data.
        /// </summary>
        /// <param name="connVerts">The connection vertices</param>
        /// <param name="connRadii">Connection radii.</param>
        /// <param name="connDirs">Connection direction data.</param>
        /// <param name="connAreas">Connection area ids.</param>
        /// <param name="connFlags">Connection flags.</param>
        /// <param name="connUserIds">Connection user ids.</param>
        /// <param name="connCount">The number of connections.</param>
        /// <returns>TRUE if the load succeeded.</returns>
        public bool LoadConns(float[] connVerts
            , float[] connRadii
            , byte[] connDirs
            , byte[] connAreas
            , ushort[] connFlags
            , uint[] connUserIds
            , int connCount)
        {
            if (mIsDisposed)
                return false;

            if (connCount == 0)
            {
                mConnCount = 0;
                return true;
            }

            if (mConnVerts == IntPtr.Zero
                || connCount < 0
                || connCount > mMaxConns
                || connVerts == null
                || connVerts.Length < connCount * 6
                || connRadii == null
                || connRadii.Length < connCount
                || connDirs == null
                || connDirs.Length < connCount
                || connAreas == null
                || connAreas.Length < connCount
                || connFlags == null
                || connFlags.Length < connCount
                || connUserIds == null
                || connUserIds.Length < connCount)
            {
                return false;
            }

            mConnCount = connCount;

            if (connCount > 0)
            {
                Marshal.Copy(connVerts, 0, mConnVerts, connCount * 6);
                Marshal.Copy(connRadii, 0, mConnRadii, connCount);
                Marshal.Copy(connDirs, 0, mConnDirs, connCount);
                Marshal.Copy(connAreas, 0, mConnAreas, connCount);
                UtilEx.Copy(connFlags, 0, mConnFlags, connCount);
                UtilEx.Copy(connUserIds, 0, mConnUserIds, connCount);
            }

            return true;
        }

        /// <summary>
        /// Returns a copy of the vertex information for each polygon.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of vertices returned, or -1 on error.</returns>
        public int GetPolyVerts(ushort[] buffer)
        {
            if (mPolyVerts == IntPtr.Zero
                || buffer == null
                || buffer.Length < mPolyVertCount * 3)
            {
                return -1;
            }
            UtilEx.Copy(mPolyVerts, buffer, mPolyVertCount * 3);
            return mPolyVertCount;
        }

        /// <summary>
        /// Returns a copy of polygon and neighbor information. 
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of polygons returned, or -1 on error.</returns>
        public int GetPolys(ushort[] buffer)
        {
            if (mPolys == IntPtr.Zero
                || buffer == null
                || buffer.Length < mPolyCount * 2 * mMaxVertsPerPoly)
            {
                return -1;
            }
            UtilEx.Copy(mPolys, buffer, mPolyCount * 2 * mMaxVertsPerPoly);
            return mPolyCount;
        }

        /// <summary>
        /// Returns a copy of the area id for each polygon.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of areas returned, or -1 on error.</returns>
        public int GetPolyAreas(byte[] buffer)
        {
            if (mPolyAreas == IntPtr.Zero
                || buffer == null
                || buffer.Length < mPolyCount)
            {
                return -1;
            }
            Marshal.Copy(mPolyAreas, buffer, 0, mPolyCount);
            return mPolyCount;
        }

        /// <summary>
        /// Returns a copy of the user-defined flags for each polygon.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of flags returned, or -1 on error.</returns>
        public int GetPolyFlags(ushort[] buffer)
        {
            if (mPolyFlags == IntPtr.Zero
                || buffer == null
                || buffer.Length < mPolyCount)
            {
                return -1;
            }
            UtilEx.Copy(mPolyFlags, buffer, mPolyCount);
            return mPolyCount;
        }

        /// <summary>
        /// Returns a copy of the sub-mesh data for the detail mesh.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of sub-meshes returned, or -1 on error.
        /// </returns>
        public int GetDetailMeshes(uint[] buffer)
        {
            if (mDetailMeshes == IntPtr.Zero
                || buffer == null
                || buffer.Length < mPolyCount * 4)
            {
                return -1;   
            }
            UtilEx.Copy(mDetailMeshes, buffer, mPolyCount * 4);
            return mPolyCount;
        }

        /// <summary>
        /// Returns a copy of the triangle/flag data for the detail mesh.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of triangles returned, or -1 on error.</returns>
        public int GetDetailTris(byte[] buffer)
        {
            if (mDetailTris == IntPtr.Zero
                || buffer == null
                || buffer.Length < mDetailTriCount * 4)
            {
                return -1;
            }
            Marshal.Copy(mDetailTris, buffer, 0, mDetailTriCount * 4);
            return mDetailTriCount;
        }

        /// <summary>
        /// Returns a copy of the vertex data for the detail mesh.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of vertices returned, or -1 on error.</returns>
        public int GetDetailVerts(float[] buffer)
        {
            if (mDetailVerts == IntPtr.Zero
                || buffer == null
                || buffer.Length < mDetailVertCount * 3)
            {
                return -1;
            }
            Marshal.Copy(mDetailVerts, buffer, 0, mDetailVertCount * 3);
            return mDetailVertCount;
        }

        /// <summary>
        /// Returns a copy of the off-mesh connection vertices.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of vertices returned, or -1 on error.</returns>
        public int GetConnVerts(float[] buffer)
        {
            if (mConnVerts == IntPtr.Zero
                || buffer == null
                || buffer.Length < mConnCount * 3 * 2)
            {
                return -1;
            }
            Marshal.Copy(mConnVerts, buffer, 0, mConnCount * 3 * 2);
            return mConnCount;
        }

        /// <summary>
        /// Returns a copy of the radii for off-mesh connections.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of radii returned, or -1 on error.</returns>
        public int GetConnRadii(float[] buffer)
        {
            if (mConnRadii == IntPtr.Zero
                || buffer == null
                || buffer.Length < mConnCount)
            {
                return -1;
            }
            Marshal.Copy(mConnRadii, buffer, 0, mConnCount);
            return mConnCount;
        }

        /// <summary>
        /// Returns a copy of the user-defined flags for off-mesh connections.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of flags returned, or -1 on error.</returns>
        public int GetConnFlags(ushort[] buffer)
        {
            if (mConnFlags == IntPtr.Zero
                || buffer == null
                || buffer.Length < mConnCount)
            {
                return -1;
            }
            UtilEx.Copy(mConnFlags, buffer, mConnCount);
            return mConnCount;
        }

        /// <summary>
        /// Returns a copy of the area ids for off-mesh connections.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of area ids returned, or -1 on error.</returns>
        public int GetConnAreas(byte[] buffer)
        {
            if (mConnAreas == IntPtr.Zero 
                || buffer == null
                || buffer.Length < mConnCount)
            {
                return -1;
            }
            Marshal.Copy(mConnAreas, buffer, 0, mConnCount);
            return mConnCount;
        }

        /// <summary>
        /// Returns a copy of the direction flags for off-mesh connections.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of direction flags returned, or -1 on error.
        /// </returns>
        public int GetConnDirs(byte[] buffer)
        {
            if (mConnDirs == IntPtr.Zero
                || buffer == null
                || buffer.Length < mConnCount)
            {
                return -1;
            }
            Marshal.Copy(mConnDirs, buffer, 0, mConnCount);
            return mConnCount;
        }

        /// <summary>
        /// The user-defined ids for off-mesh connections.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.</param>
        /// <returns>The number of user ids returned, or -1 on error.</returns>
        public int GetConnUserIds(uint[] buffer)
        {
            if (mConnUserIds == IntPtr.Zero
                || buffer == null
                || buffer.Length < mConnCount)
            {
                return -1;
            }
            UtilEx.Copy(mConnUserIds, buffer, mConnCount);
            return mConnCount;
        }

        private static bool IsValid(int maxPolyVerts
                , int maxPolys
                , int maxVertsPerPoly
                , int maxDetailVerts
                , int maxDetailTris
                , int maxConns)
        {
            if (maxPolyVerts < 3
                || maxPolys < 1
                || maxVertsPerPoly > Navmesh.MaxAllowedVertsPerPoly
                || maxVertsPerPoly < 3
                || maxDetailVerts < 0
                || maxDetailTris < 0
                || maxConns < 0)
            {
                return false;
            }

            if ((maxDetailVerts > 0 || maxDetailTris > 0)
                && (maxDetailVerts < 3 || maxDetailTris < 1))
            {
                return false;
            }

            return true;
        }
    }
}
