using System;
using org.critterai.nav.rcn;
using org.critterai.interop;
using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NavmeshTileBuildData
    {
        /// <summary>
        /// The minimum allowed value for <see cref="XZCellSize"/> and 
        /// <see cref="YCellSize"/>.
        /// </summary>
        public const float MinCellSize = 0.01f;

        /// <summary>
        /// The absolute minimum allowed value for 
        /// <see cref="WalkableHeight"/>.
        /// </summary>
        /// <remarks>
        /// Dependancies between parameters may limit the minimum value 
        /// to a higher value.
        /// </remarks>
        public const float MinAllowedTraversableHeight = 3 * MinCellSize;

        #region dtNavMeshCreateParams Fields

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

        #region rcnNavMeshCreateParams Fields

        private bool mIsDisposed = true;  // Default is accurate.

        private int mMaxPolyVerts = 0;
        private int mMaxPolys = 0;

        private int mMaxDetailVerts = 0;
        private int mMaxDetailTris = 0;

        private int mMaxConns = 0;

        #endregion

        public int MaxPolyVerts { get { return mMaxPolyVerts; } }
        public int MaxPolys { get { return mMaxPolys; } }
        public int MaxDetailVerts { get { return mMaxDetailVerts; } }
        public int MaxDetailTris { get { return mMaxDetailTris; } }
        public int MaxConns { get { return mMaxConns; } }

        public int PolyVertCount { get { return mPolyVertCount; } }
        public int PolyCount { get { return mPolyCount; } }
        public int MaxVertsPerPoly { get { return mMaxVertsPerPoly; } }
        public int DetailVertCount { get { return mDetailVertCount; } }
        public int DetailTriCount { get { return mDetailTriCount; } }
        public int ConnCount { get { return mConnCount; } }
        public uint TileUserId { get { return mTileUserId; } }
        public int TileX { get { return mTileX; } }
        public int TileY { get { return mTileY; } }
        public int TileLayer { get { return mTileLayer; } }

        public float[] GetBoundsMin() { return (float[])mBoundsMin.Clone(); }
        public float[] GetBoundsMax() { return (float[])mBoundsMax.Clone(); }

        public float WalkableHeight
        {
            get { return mWalkableHeight; }
        }

        public float WalkableRadius
        {
            get { return mWalkableRadius; }
        }

        public float MaxTraversableStep
        {
            get { return mWalkableStep; }
        }

        public float XZCellSize { get { return mXZCellSize; } }
        public float YCellSize { get { return mYCellSize; } }

        public bool BVTreeEnabled { get { return mBVTreeEnabled; } }

        private bool IsValid(int maxPolyVerts
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

        ~NavmeshTileBuildData()
        {
            Dispose();
        }

        public bool IsDisposed
        {
            get { return mIsDisposed; }
        }

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

        public bool SetBase(int tileX
            , int tileY
            , int tileLayer
            , uint tileUserId
            , float[] boundsMin
            , float[] boundsMax
            , float xzCellSize
            , float yCellSize
            , float walkableHeight
            , float walkableStep
            , float walkableRadius
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
            mTileY = tileY;
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
        /// <returns>A copy of polygon and neighbor information.</returns>
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
        /// <returns>A copy of the area id for each polygon.</returns>
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
        /// <returns>A copy of the flags for each polygon.</returns>
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
        /// Returns a copy of the sub-mesh data. (baseVertexIndex,
        /// vertexCount, baseTriangleIndex, triangleCount) * MeshCount. 
        /// </summary>
        /// <remarks>
        /// <p>The maximum number of vertices per sub-mesh is 127.</p>
        /// <p>The maximum number of triangles per sub-mesh is 255.</p>
        /// <p>
        /// An example of iterating the triangles in a sub-mesh. (Where
        /// iMesh is the index for the sub-mesh within detailMesh.)
        /// </p>
        /// <p>
        /// <pre>
        ///     uint[] meshes = detailMesh.GetMeshes();
        ///     byte[] triangles = detailMesh.GetTriangles();
        ///     float[] vertices = detailMesh.GetVertices();
        /// 
        ///     int pMesh = iMesh * 4;
        ///     int pVertBase = meshes[pMesh + 0] * 3;
        ///     int pTriBase = meshes[pMesh + 2] * 4;
        ///     int tCount = meshes[pMesh + 3];
        ///     int vertX, vertY, vertZ;
        ///
        ///     for (int iTri = 0; iTri &lt; tCount; iTri++)
        ///     {
        ///        for (int iVert = 0; iVert &lt; 3; iVert++)
        ///        {
        ///            int pVert = pVertBase 
        ///                + (triangles[pTriBase + (iTri * 4 + iVert)] * 3);
        ///            vertX = vertices[pVert + 0];
        ///            vertY = vertices[pVert + 1];
        ///            vertZ = vertices[pVert + 2];
        ///            // Do something with the vertex.
        ///        }
        ///    }
        /// </pre>
        /// </p>
        /// </remarks>
        /// <returns>A copy of the sub-mesh data.</returns>
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
        /// Returns a copy of the detail triangles array. (vertIndexA,
        /// vertIndexB, vertIndexC, flag) * TriangleCount
        /// </summary>
        /// <remarks>
        /// <p>The triangles are grouped by sub-mesh. See GetMeshes()
        /// for how to obtain the base triangle and triangle count for
        /// each sub-mesh.</p>
        /// <p><b>Vertices</b></p>
        /// <p> The vertex indices in the triangle array are local to the 
        /// sub-mesh, not global.  To translate into an global index in the 
        /// vertices array, the values must be offset by the sub-mesh's base 
        /// vertex index.</p>
        /// <p>
        /// Example: If the baseVertexIndex for the sub-mesh is 5 and the
        /// triangle entry is (4, 8, 7, 0), then the actual indices for
        /// the vertices are (4 + 5, 8 + 5, 7 + 5).
        /// </p>
        /// <p><b>Flags</b></p>
        /// <p>The flags entry indicates which edges are internal and which
        /// are external to the sub-mesh.</p>
        /// 
        /// <p>Internal edges connect to other triangles within the same 
        /// sub-mesh.
        /// External edges represent portals to other sub-meshes or the null 
        /// region.</p>
        /// 
        /// <p>Each flag is stored in a 2-bit position.  Where position 0 is the
        /// lowest 2-bits and position 4 is the highest 2-bits:</p>
        /// 
        /// <p>Position 0: Edge AB (>> 0)<br />
        /// Position 1: Edge BC (>> 2)<br />
        /// Position 2: Edge CA (>> 4)<br />
        /// Position 4: Unused</p>
        /// 
        /// <p>Testing should be performed as follows:</p>
        /// <pre>
        /// if (((flag >> 2) & 0x3) == 0)
        /// {
        ///     // Edge BC is an external edge.
        /// }
        /// </pre>
        /// </remarks>
        /// <returns>A copy of the triangles array.</returns>
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
        /// The vertices that make up each detail sub-mesh. 
        /// (x, y, z) * DetailVertexCount.
        /// </summary>
        /// <remarks>
        /// <p>The vertices are grouped by sub-mesh and will contain duplicates
        /// since each sub-mesh is independantly defined.</p>
        /// <p>The first group of vertices for each sub-mesh are in the same 
        /// order as the vertices for the sub-mesh's associated polygon 
        /// mesh polygon.  These vertices are followed by any additional
        /// detail vertices.  So it the associated polygon has 5 vertices, the
        /// sub-mesh will have a minimum of 5 vertices and the first 5 
        /// vertices will be equivalent to the 5 polygon vertices.</p>
        /// <p>See GetMeshes() for how to obtain the base vertex and
        /// vertex count for each sub-mesh.</p>
        /// </remarks>
        /// <returns>The vertices that make up each sub-mesh.</returns>
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

        public int  GetConnRadii(float[] buffer)
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
    }
}
