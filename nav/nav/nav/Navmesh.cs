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
using org.critterai.nav.rcn;
using org.critterai.interop;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    /// <summary>
    /// A navigation mesh based on convex polygons.
    /// </summary>
    /// <remarks>
    /// <p>The navigation mesh supports polygons that vary in number of 
    /// sides and overlap.
    /// </p>
    /// <p>While various methods refer to tiling, the tile functionality is
    /// not currently exposed.  So there is only one tile for the entire
    /// mesh.</p>
    /// <p>There are no asynchronous methods for this class.  So all methods
    /// will return a <see cref="NavStatus"/> including success or failure.</p>
    /// <p>Instances of this class are not serializable.  
    /// If you need serialization, then save the the source data used to create 
    /// the mesh.</p>
    /// <p>Behavior is undefined if an object is used after 
    /// disposal.</p>
    /// </remarks>
    [Serializable]
    public sealed class Navmesh
        : ManagedObject, ISerializable
    {
        private const long ClassVersion = 1L;

        private const string VersionKey = "v";
        private const string DataKey = "d";

        /// <summary>
        /// A flag that indicates a link is external.  (Context dependant.)
        /// </summary>
        public const ushort ExternalLink = NavmeshEx.ExternalLink;

        /// <summary>
        /// Indicates that a link is null. (Does not link to anything.)
        /// </summary>
        public const uint NullLink = NavmeshEx.NullLink;

        /// <summary>
        /// The maximum vertices per polygon.
        /// </summary>
        public const int MaxAllowedVertsPerPoly = NavmeshEx.MaxVertsPerPolygon;

        /// <summary>
        /// The maximum supported number of areas.
        /// </summary>
        public const int MaxAreas = NavmeshEx.MaxAreas;

        /// <summary>
        /// A pointer to the unmanaged dtNavMesh object.
        /// </summary>
        internal IntPtr root;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mesh">A pointer to an unmanaged dtNavMesh object.
        /// </param>
        private Navmesh(IntPtr mesh)
            : base(AllocType.External)
        {
            root = mesh;
        }

        private Navmesh(SerializationInfo info, StreamingContext context)
            : base(AllocType.External)
        {
            root = IntPtr.Zero;

            if (info.MemberCount != 2 || info.GetInt64("v") != ClassVersion)
                return;
            
            byte[] data = (byte[])info.GetValue(DataKey, typeof(byte[]));
            NavmeshEx.BuildMesh(data, data.Length, ref root);
        }

        ~Navmesh()
        {
            RequestDisposal();
        }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Request all unmanaged resources controlled by the object be 
        /// immediately freed and the object marked as disposed.
        /// </summary>
        public override void RequestDisposal()
        {
            if (root != IntPtr.Zero)
            {
                NavmeshEx.FreeEx(ref root);
                root = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Returns the configuration parameters used to initialize the
        /// navigation mesh.
        /// </summary>
        /// <returns>The configuration parameters used to initialize the
        /// navigation mesh.</returns>
        public NavmeshParams GetParams()
        {
            NavmeshParams result = new NavmeshParams();
            NavmeshEx.GetParams(root, result);
            return result;
        }

        public NavStatus AddTile(NavmeshTileData tileData
            , uint desiredTileRef
            , out uint resultTileRef)
        {
            if (tileData == null
                || tileData.IsOwned
                || tileData.Length == 0)
            {
                resultTileRef = 0;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            resultTileRef = 0;

            return NavmeshEx.AddTile(root
                , tileData
                , desiredTileRef
                , ref resultTileRef);
        }

        public NavStatus RemoveTile(uint tileRef)
        {
            return NavmeshEx.RemoveTile(root, tileRef);
        }

        public void CalculateTileLocation(float[] position
            , out int tx
            , out int ty)
        {
            tx = 0;
            ty = 0;
            NavmeshEx.CalculateTileLocation(root, position, ref tx, ref ty);
        }

        public NavmeshTile GetTile(int x, int y, int layer)
        {
            IntPtr tile = NavmeshEx.GetTile(root, x, y, layer);
            if (tile == IntPtr.Zero)
                return null;
            return new NavmeshTile(this, tile);
        }

        public int GetTiles(int x, int y, NavmeshTile[] resultTiles)
        {
            IntPtr[] tiles = new IntPtr[resultTiles.Length];

            int tileCount = NavmeshEx.GetTiles(root, x, y, tiles, tiles.Length);

            for (int i = 0; i < tileCount; i++)
            {
                resultTiles[i] = new NavmeshTile(this, tiles[i]);
            }

            return tileCount;
        }

        public uint GetTileRef(int x, int y, int layer)
        {
            return NavmeshEx.GetTileRef(root, x, y, layer);
        }

        public NavmeshTile GetTileByRef(uint id)
        {
            IntPtr tile = NavmeshEx.GetTileByRef(root, id);
            if (tile == IntPtr.Zero)
                return null;
            return new NavmeshTile(this, tile);
        }

        /// <summary>
        /// The maximum number of tiles supported by the navigation mesh.
        /// </summary>
        /// <returns>The maximum number of tiles supported by the navigation 
        /// mesh.</returns>
        public int GetMaxTiles()
        {
            return NavmeshEx.GetMaxTiles(root);
        }

        public NavmeshTile GetTile(int tileIndex)
        {
            IntPtr tile = NavmeshEx.GetTile(root, tileIndex);
            if (tile == IntPtr.Zero)
                return null;
            return new NavmeshTile(this, tile);
        }


        public NavStatus GetTileAndPoly(uint polyRef
            , out NavmeshTile tile
            , out NavmeshPoly poly)
        {
            IntPtr pTile = IntPtr.Zero;
            IntPtr pPoly = IntPtr.Zero;

            NavStatus status = NavmeshEx.GetTileAndPoly(root
                , polyRef
                , ref pTile
                , ref pPoly);

            if (NavUtil.Succeeded(status))
            {
                tile = new NavmeshTile(this, pTile);
                poly = (NavmeshPoly)Marshal.PtrToStructure(pPoly
                    , typeof(NavmeshPoly));
            }
            else
            {
                tile = null;
                poly = new NavmeshPoly();
            }

            return status;

        }

        /// <summary>
        /// Indicates whether or not the specified polygon id is valid for
        /// the navigation mesh.
        /// </summary>
        /// <param name="polyRef">The polygon id to check.</param>
        /// <returns>TRUE if the provided polygon id is valid.</returns>
        public bool IsValidPolyRef(uint polyRef)
        {
            return NavmeshEx.IsValidPolyRef(root, polyRef);
        }

        /// <summary>
        /// Gets the endpoints for an off-mesh connection, ordered by
        /// "direction of travel".
        /// </summary>
        /// <remarks>
        /// <p>Off-mesh connections are stored in the navigation mesh as
        /// special 2-vertex polygons with a single edge.  At least one of the
        /// vertices is expected to be inside a normal polygon. So an off-mesh
        /// connection is "entered" from a normal polygon at one of its 
        /// endpoints. This is the polygon identified by startPolyRef.</p>
        /// </remarks>
        /// <param name="startPolyRef">The polygon id in which the
        /// start endpoint lies.</param>
        /// <param name="connectionPolyRef">The off-mesh connection's polyon 
        /// id.</param>
        /// <param name="startPosition">The start position in the form
        /// (x, y, z).</param>
        /// <param name="endPosition">The end position in the form
        /// (x, y, z).</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetConnectionEndpoints(uint startPolyRef
            , uint connectionPolyRef
            , float[] startPosition
            , float[] endPosition)
        {
            return NavmeshEx.GetConnEndpoints(root
                , startPolyRef
                , connectionPolyRef
                , startPosition
                , endPosition);
        }

        public NavmeshConnection GetConnectionByRef(uint polyRef)
        {
            IntPtr conn = NavmeshEx.GetConnectionByRef(root, polyRef);

            NavmeshConnection result;
            if (conn == IntPtr.Zero)
                result = new NavmeshConnection();
            else
                result = (NavmeshConnection)Marshal.PtrToStructure(conn
                    , typeof(NavmeshConnection));

            return result;
        }

        /// <summary>
        /// Returns the flags for the specified polygon.
        /// </summary>
        /// <param name="polyRef">The polygon id.</param>
        /// <param name="flags">The polygon's flags.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetPolyFlags(uint polyRef, out ushort flags)
        {
            flags = 0;
            return NavmeshEx.GetPolyFlags(root, polyRef, ref flags);
        }

        /// <summary>
        /// Sets the flags for the specified polygon.
        /// </summary>
        /// <param name="polyRef">The polyon id.</param>
        /// <param name="flags">The polygon's flags.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus SetPolyFlags(uint polyRef, ushort flags)
        {
            return NavmeshEx.SetPolyFlags(root, polyRef, flags);
        }

        /// <summary>
        /// Returns the area id of the specified polygon.
        /// </summary>
        /// <param name="polyRef">The polyon id.</param>
        /// <param name="area">The polygon's area id.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetPolyArea(uint polyRef, out byte area)
        {
            area = 0;
            return NavmeshEx.GetPolyArea(root, polyRef, ref area);
        }

        /// <summary>
        /// Sets the area id of the specified polygon.
        /// </summary>
        /// <param name="polyRef">The polyon id.</param>
        /// <param name="area">The polygon's area id.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus SetPolyArea(uint polyRef, byte area)
        {
            return NavmeshEx.SetPolyArea(root, polyRef, area);
        }

        ///// <summary>
        ///// Returns information for the specified tile.
        ///// </summary>
        ///// <param name="tileIndex">The tile index.</param>
        ///// <param name="info">The tile information.</param>
        ///// <returns>The status flags for the request.</returns>
        //public NavStatus GetTileInfo(int tileIndex
        //    , out NavmeshTileDebugData info)
        //{
        //    info = new NavmeshTileDebugData();
        //    info.Initialize();
        //    return NavmeshEx.GetTileDebugData(root, tileIndex, ref info);
        //}

        ///// <summary>
        ///// Returns the polygon information for a tile.
        ///// </summary>
        ///// <param name="tileIndex">The tile index.</param>
        ///// <param name="resultPolys">An array of initialized polygon
        ///// structures which will hold the result. 
        ///// (Minimum size: tile.polygonCount)</param>
        ///// <param name="polyCount">The number of polygons in the result
        ///// array.</param>
        ///// <returns>The status flags for the request.</returns>
        //public NavStatus GetTilePolys(int tileIndex
        //    , NavmeshPoly[] resultPolys
        //    , out int resultCount)
        //{
        //    resultCount = 0;
        //    NavStatus status = NavmeshEx.GetTilePolys(root
        //        , tileIndex
        //        , resultPolys
        //        , ref resultCount
        //        , resultPolys.Length);

        //    if (NavUtil.Failed(status))
        //        resultCount = 0;

        //    return status;
        //}

        ///// <summary>
        ///// Returns the polygon vertices for a tile.
        ///// </summary>
        ///// <param name="tileIndex">The tile index.</param>
        ///// <param name="resultVerts">The polygon vertices in the form
        ///// (x, y, z). (Minimum size: tile.vertexCount * 3)
        ///// E.g.: 3 * <see cref="MaxVertsPerPolygon"/>)</param>
        ///// <param name="vertCount">The number of vertices in the result
        ///// array.</param>
        ///// <returns>The status flags for the request.</returns>
        //public NavStatus GetTileVerts(int tileIndex
        //    , float[] resultVerts
        //    , out int vertCount)
        //{
        //    vertCount = 0;

        //    NavStatus status = NavmeshEx.GetTileVerts(root
        //        , tileIndex
        //        , resultVerts
        //        , ref vertCount
        //        , resultVerts.Length);

        //    return status;
        //}

        ///// <summary>
        ///// Returns the detail polygon vertices for a tile.
        ///// </summary>
        ///// <param name="tileIndex">The tile index.</param>
        ///// <param name="resultVerts">The detail polygon vertices in the form
        ///// (x, y, z). (Minimum size: tile.detailVertCount * 3)</param>
        ///// <param name="vertCount">The number of vertices return in the
        ///// result.</param>
        ///// <returns>The status flags for the request.</returns>
        //public NavStatus GetTileDetailVerts(int tileIndex
        //    , float[] resultVerts
        //    , out int vertCount)
        //{
        //    vertCount = 0;

        //    NavStatus status = NavmeshEx.GetTileDetailVerts(root
        //        , tileIndex
        //        , resultVerts
        //        , ref vertCount
        //        , resultVerts.Length);

        //    return status;
        //}

        ///// <summary>
        ///// Returns the indices and flags of the detail triangles for a tile.
        ///// </summary>
        ///// <remarks>See the <see cref="PolyMeshDetail"/> class description
        ///// for information on triangle flags.</remarks>
        ///// <param name="tileIndex">The tile index.</param>
        ///// <param name="resultTris">The indices for the detail triangles
        ///// in the form (vertAIndex, vertBIndex, vertCIndex, flags).
        ///// (Minimum size: tile.detailTriCount * 4)</param>
        ///// <param name="trisCount">The number of triangles returned
        ///// in the result array.</param>
        ///// <returns>The status flags for the request.</returns>
        //public NavStatus GetTileDetailTris(int tileIndex
        //    , byte[] resultTriangles
        //    , out int triangleCount)
        //{
        //    triangleCount = 0;

        //    NavStatus status = NavmeshEx.GetTileDetailTris(root
        //        , tileIndex
        //        , resultTriangles
        //        , ref triangleCount
        //        , resultTriangles.Length);

        //    return status;
        //}

        ///// <summary>
        ///// Returns link data for a tile.
        ///// </summary>
        ///// <param name="tileIndex">The tile index.</param>
        ///// <param name="resultLinks">An array of link structures which will
        ///// hold the result. (Minimum size: tile.maxLinkCount)</param>
        ///// <param name="linkCount">The number of links returned in the result
        ///// array.</param>
        ///// <returns>The status flags for the request.</returns>
        //public NavStatus GetTileLinks(int tileIndex
        //    , NavmeshLink[] resultLinks
        //    , out int linkCount)
        //{
        //    linkCount = 0;

        //    NavStatus status = NavmeshEx.GetTileLinks(root
        //        , tileIndex
        //        , resultLinks
        //        , ref linkCount
        //        , resultLinks.Length);

        //    if (NavUtil.Failed(status))
        //        resultLinks = null;

        //    return status;
        //}

        ///// <summary>
        ///// Returns bounding volume node information for a tile.
        ///// </summary>
        ///// <param name="tileIndex">The tile index.</param>
        ///// <param name="resultNodes">An array of initialized bounding volumne
        ///// structures which will hold the result.
        ///// (Minimum size: tile.bvNodeCount)</param>
        ///// <param name="nodeCount">The number of nodes returned in the
        ///// result array.</param>
        ///// <returns>The status flags for the request.</returns>
        //public NavStatus GetTileBVTree(int tileIndex
        //    , NavmeshBVNode[] resultNodes
        //    , out int nodeCount)
        //{
        //    nodeCount = 0;

        //    NavStatus status = NavmeshEx.GetTileBVTree(root
        //        , tileIndex
        //        , resultNodes
        //        , ref nodeCount
        //        , resultNodes.Length);

        //    return status;
        //}

        ///// <summary>
        ///// Returns off-mesh connection information for a tile.
        ///// </summary>
        ///// <param name="tileIndex">The tile information.</param>
        ///// <param name="resultConns">An initialized array of connection
        ///// structures which will be loaded with the result.
        ///// (Minimum size: tile.offMeshConCount)</param>
        ///// <param name="connCount">The number of connections returned in the 
        ///// result array.</param>
        ///// <returns>The status flags for the request.</returns>
        //public NavStatus GetTileOffMeshCons(int tileIndex
        //    , NavmeshConnection[] resultConns
        //    , out int connCount)
        //{
        //    connCount = 0;

        //    NavStatus status = NavmeshEx.GetTileOffMeshCons(root
        //        , tileIndex
        //        , resultConns
        //        , ref connCount
        //        , resultConns.Length);

        //    return status;
        //}

        ///// <summary>
        ///// Returns the detail mesh information for a tile.
        ///// </summary>
        ///// <param name="tileIndex">The tile index.</param>
        ///// <param name="resultMeshes">An array of detail mesh structures
        ///// which will hold the result. (Minimum size: tile.detailMeshCount)
        ///// </param>
        ///// <param name="meshCount">The number of meshes returned in the
        ///// result array.</param>
        ///// <returns>The status flags for the request.</returns>
        //public NavStatus GetTileDetailMeshes(int tileIndex
        //    , NavmeshPolyDetail[] resultMeshes
        //    , out int meshCount)
        //{
        //    meshCount = 0;

        //    NavStatus status = NavmeshEx.GetTileDetailMeshes(root
        //        , tileIndex
        //        , resultMeshes
        //        , ref meshCount
        //        , resultMeshes.Length);

        //    return status;
        //}

        public byte[] GetSerializedMesh()
        {
            if (IsDisposed)
                return null;

            IntPtr data = IntPtr.Zero;
            int dataSize = 0;

            NavmeshEx.GetSerializedNavmesh(root, ref data, ref dataSize);

            if (dataSize == 0)
                return null;

            byte[] resultData = UtilEx.ExtractArrayByte(data, dataSize);

            NavmeshEx.FreeSerializedNavmeshEx(ref data);

            return resultData;
        }

        public void GetObjectData(SerializationInfo info
            , StreamingContext context)
        {
            if (IsDisposed)
                return;

            info.AddValue(VersionKey, ClassVersion);
            info.AddValue(DataKey, GetSerializedMesh());
        }

        /// <summary>
        /// Builds a static, single tile navigation mesh from the provided
        /// data.
        /// </summary>
        /// <returns>The status flags for the request.</returns>
        public static NavStatus Build(NavmeshTileBuildData buildData
            , out Navmesh resultMesh)
        {
            IntPtr navMesh = IntPtr.Zero;

            NavStatus status = NavmeshEx.BuildMesh(buildData
                , ref navMesh);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(navMesh);
            else
                resultMesh = null;

            return status;
        }

        public static NavStatus Build(byte[] serializedMesh
            , out Navmesh resultMesh)
        {
            if (serializedMesh == null || serializedMesh.Length < 1)
            {
                resultMesh = null;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            IntPtr root = IntPtr.Zero;

            NavStatus status = NavmeshEx.BuildMesh(serializedMesh
                , serializedMesh.Length
                , ref root);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(root);
            else
                resultMesh = null;

            return status;
        }

        public static NavStatus Build(NavmeshParams config
            , out Navmesh resultMesh)
        {
            if (config == null || config.maxTiles < 1)
            {
                resultMesh = null;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            IntPtr root = IntPtr.Zero;

            NavStatus status = NavmeshEx.BuildMesh(config, ref root);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(root);
            else
                resultMesh = null;

            return status;
        }
    }
}
