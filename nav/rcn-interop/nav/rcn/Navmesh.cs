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
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
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
    /// <p>Behavior is undefined if objects of this type are used after 
    /// disposal.</p>
    /// </remarks>
    public sealed class Navmesh
        : ManagedObject
    {
        /// <summary>
        /// A flag that indicates a link is external.  (Context dependant.)
        /// </summary>
        public const ushort ExternalLink = NavmeshEx.ExternalLink;

        /// <summary>
        /// Indicates that a link is null. (Does not link to anything.)
        /// </summary>
        public const uint NullLink = NavmeshEx.ExternalLink;

        /// <summary>
        /// The maximum vertices per polygon.
        /// </summary>
        public const int MaxVertsPerPolygon = NavmeshEx.MaxVertsPerPolygon;

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
        internal Navmesh(IntPtr mesh)
            : base(AllocType.External)
        {
            root = mesh;
        }

        ~Navmesh()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Indicates whether or not the resources held by the object have
        /// been released.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Immediately releases all unmanaged resources controlled by the 
        /// object.
        /// </summary>
        public override void RequestDisposal()
        {
            if (root != IntPtr.Zero)
            {
                NavmeshEx.FreeNavmesh(ref root);
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

            NavmeshEx.GetParams(root, ref result);
            return result;
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

        /// <summary>
        /// Indicates whether or not the specified polygon id is valid for
        /// the navigation mesh.
        /// </summary>
        /// <param name="polyId">The polygon id to check.</param>
        /// <returns>TRUE if the provided polygon id is valid.</returns>
        public bool IsValidPolyId(uint polyId)
        {
            return NavmeshEx.IsValidPolyId(root, polyId);
        }

        /// <summary>
        /// Returns information for the specified tile.
        /// </summary>
        /// <param name="tileIndex">The zero based tile index.</param>
        /// <param name="info">The tile information.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetTileInfo(int tileIndex
            , out NavmeshTileData info)
        {
            info = new NavmeshTileData();
            info.Initialize();
            return NavmeshEx.GetTileInfo(root, tileIndex, ref info);
        }

        /// <summary>
        /// Returns the polygon information for a tile.
        /// </summary>
        /// <param name="tile">The tile information.</param>
        /// <param name="polys">The polygon information.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetTilePolys(NavmeshTileData tile
            , out NavmeshPoly[] polys)
        {
            polys = NavmeshPoly.GetInitializedArray(tile.polygonCount);

            NavStatus status = NavmeshEx.GetTilePolys(root
                , tile.tileIndex
                , polys
                , polys.Length);

            if (NavUtil.Failed(status))
                polys = null;

            return status;
        }

        /// <summary>
        /// Returns the polygon vertices for a tile.
        /// </summary>
        /// <param name="tile">The tile information.</param>
        /// <param name="verts">The polygon tile vertices.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetTileVerts(NavmeshTileData tile
            , out float[] verts)
        {
            verts = new float[tile.vertexCount * 3];

            NavStatus status = NavmeshEx.GetTileVerts(root
                , tile.tileIndex
                , verts
                , verts.Length);

            if (NavUtil.Failed(status))
                verts = null;

            return status;
        }

        /// <summary>
        /// Returns the detail polygon vertices for a tile.
        /// </summary>
        /// <param name="tile">The tile information.</param>
        /// <param name="verts">The detail polygon vertices in the form
        /// (x, y, z).</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetTileDetailVerts(NavmeshTileData tile
            , out float[] verts)
        {
            verts = new float[tile.detailVertCount * 3];

            NavStatus status = NavmeshEx.GetTileDetailVerts(root
                , tile.tileIndex
                , verts
                , verts.Length);

            if (NavUtil.Failed(status))
                verts = null;

            return status;
        }

        /// <summary>
        /// Returns the indices and flags of the detail triangles for a tile.
        /// </summary>
        /// <remarks>See the <see cref="PolyMeshDetail"/> class description
        /// for information on triangle flags.</remarks>
        /// <param name="tile">The tile information.</param>
        /// <param name="triangles">The indices for the detail triangles
        /// in the form (vertAIndex, vertBIndex, vertCIndex, flags).</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetTileDetailTris(NavmeshTileData tile
            , out byte[] triangles)
        {
            triangles = new byte[tile.detailTriCount * 4];

            NavStatus status = NavmeshEx.GetTileDetailTris(root
                , tile.tileIndex
                , triangles
                , triangles.Length);

            if (NavUtil.Failed(status))
                triangles = null;

            return status;
        }

        /// <summary>
        /// Returns link data for a tile.
        /// </summary>
        /// <param name="tile">The tile information.</param>
        /// <param name="links">The link information.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetTileLinks(NavmeshTileData tile
            , out NavmeshLink[] links)
        {
            links = new NavmeshLink[tile.maxLinkCount];

            NavStatus status = NavmeshEx.GetTileLinks(root
                , tile.tileIndex
                , links
                , links.Length);

            if (NavUtil.Failed(status))
                links = null;

            return status;
        }

        /// <summary>
        /// Returns bounding volume node information for a tile.
        /// </summary>
        /// <param name="tile">The tile information.</param>
        /// <param name="nodes">The bounding volume nodes.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetTileBVTree(NavmeshTileData tile
            , out BVNode[] nodes)
        {
            nodes = BVNode.GetInitializedArray(tile.bvNodeCount);

            NavStatus status = NavmeshEx.GetTileBVTree(root
                , tile.tileIndex
                , nodes
                , nodes.Length);

            if (NavUtil.Failed(status))
                nodes = null;

            return status;
        }

        /// <summary>
        /// Returns off-mesh connection information for a tile.
        /// </summary>
        /// <param name="tile">The tile information.</param>
        /// <param name="connections">The connection information.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetTileOffMeshCons(NavmeshTileData tile
            , out NavmeshConnection[] connections)
        {
            connections = 
                NavmeshConnection.GetInitializedArray(tile.offMeshConCount);

            NavStatus status = NavmeshEx.GetTileOffMeshCons(root
                , tile.tileIndex
                , connections
                , connections.Length);

            if (NavUtil.Failed(status))
                connections = null;

            return status;
        }

        /// <summary>
        /// Returns the detail mesh information for a tile.
        /// </summary>
        /// <param name="tile">The tile information.</param>
        /// <param name="detailMeshes">The detail mesh information.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetTileDetailMeshes(NavmeshTileData tile
            , out NavmeshPolyDetail[] detailMeshes)
        {
            detailMeshes = new NavmeshPolyDetail[tile.detailMeshCount];

            NavStatus status = NavmeshEx.GetTileDetailMeshes(root
                , tile.tileIndex
                , detailMeshes
                , detailMeshes.Length);

            if (NavUtil.Failed(status))
                detailMeshes = null;

            return status;
        }

        /// <summary>
        /// Returns the flags for the specified polygon.
        /// </summary>
        /// <param name="polyId">The polygon id.</param>
        /// <param name="flags">The polygon's flags.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetPolyFlags(uint polyId, out ushort flags)
        {
            flags = 0;
            return NavmeshEx.GetPolyFlags(root, polyId, ref flags);
        }

        /// <summary>
        /// Sets the flags for the specified polygon.
        /// </summary>
        /// <param name="polyId">The polyon id.</param>
        /// <param name="flags">The polygon's flags.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus SetPolyFlags(uint polyId, ushort flags)
        {
            return NavmeshEx.SetPolyFlags(root, polyId, flags);
        }

        /// <summary>
        /// Returns the area id of the specified polygon.
        /// </summary>
        /// <param name="polyId">The polyon id.</param>
        /// <param name="areaId">The polygon's area id.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetPolyAreaId(uint polyId, out byte areaId)
        {
            areaId = 0;
            return NavmeshEx.GetPolyArea(root, polyId, ref areaId);
        }

        /// <summary>
        /// Sets the area id of the specified polygon.
        /// </summary>
        /// <param name="polyId">The polyon id.</param>
        /// <param name="areaId">The polygon's area id.</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus SetPolyAreaId(uint polyId, byte areaId)
        {
            return NavmeshEx.SetPolyArea(root, polyId, areaId);
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
        /// endpoints. This is the polygon identified by startPolyId.</p>
        /// </remarks>
        /// <param name="startPolyId">The polygon id in which the
        /// start endpoint lies.</param>
        /// <param name="connectionPolyId">The off-mesh connection's polyon 
        /// id.</param>
        /// <param name="startPosition">The start position in the form
        /// (x, y, z).</param>
        /// <param name="endPosition">The end position in the form
        /// (x, y, z).</param>
        /// <returns>The status flags for the request.</returns>
        public NavStatus GetOffMeshConnEndpoints(uint startPolyId
            , uint connectionPolyId
            , float[] startPosition
            , float[] endPosition)
        {
            return NavmeshEx.GetOffMeshConEndpoints(root
                , startPolyId
                , connectionPolyId
                , startPosition
                , endPosition);
        }

        /// <summary>
        /// Gets the polygon id based on its polygon index within a tile.
        /// </summary>
        /// <param name="tileInfo">The tile information.</param>
        /// <param name="polyIndex">The polygon's index within the tile.</param>
        /// <returns>The status flags for the request.</returns>
        public static uint GetPolyId(NavmeshTileData tileInfo
            , int polyIndex)
        {
            return (tileInfo.basePolyId | (uint)polyIndex);
        }

        /// <summary>
        /// Builds a static, single tile navigation mesh from the provided
        /// data.
        /// </summary>
        /// <param name="polyMesh">Polygon mesh data.</param>
        /// <param name="detailMesh">Detail polygon mesh data. (Optional)
        /// </param>
        /// <param name="offMeshConnections">Off-mesh connection data.
        /// (Optional)</param>
        /// <param name="resultMesh">The resulting navigation mesh..</param>
        /// <returns>The status flags for the request.</returns>
        public static NavStatus BuildStaticMesh(PolyMesh polyMesh
            , PolyMeshDetail detailMesh
            , ref NavConnections offMeshConnections
            , out Navmesh resultMesh)
        {
            IntPtr navMesh = IntPtr.Zero;

            NavStatus status = NavmeshEx.BuildMesh(
                ref polyMesh.root
                , ref detailMesh.root
                , polyMesh.MinTraversableHeight
                , polyMesh.TraversableAreaBorderSize
                , polyMesh.MaxTraversableStep
                , ref offMeshConnections
                , ref navMesh);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(navMesh);
            else
                resultMesh = null;

            return status;
        }
    }
}
