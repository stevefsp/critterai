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

namespace org.critterai.nav.rcn
{
    internal static class NavmeshTileEx
    {
        // Source declarations: DetourNavmeshEx.h

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmBuildTileData")]
        public static extern bool BuildTileData(NavmeshTileBuildData sourceData
            , [In, Out] NavmeshTileData resultTile);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmBuildTileDataRaw")]
        public static extern bool BuildTileData([In] byte[] rawData
            , int dataSize
            , [In, Out] NavmeshTileData resultTile);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmFreeTileData")]
        public static extern void FreeEx(NavmeshTileData tileData);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileRef")]
        public static extern uint GetTileRef(IntPtr navmesh, IntPtr tile);


        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileStateSize")]
        public static extern int GetTileStateSize(IntPtr navmesh, IntPtr tile);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmStoreTileState")]
        public static extern NavStatus GetTileState(IntPtr navmesh
            , IntPtr tile
            , [In, Out] byte[] stateData
            , int dataSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmRestoreTileState")]
        public static extern NavStatus SetTileState(IntPtr navmesh
            , IntPtr tile
            , [In] byte[] stateData
            , int dataSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileHeader")]
        public static extern IntPtr GetTileHeader(IntPtr tile);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyRefBase")]
        public static extern uint GetBasePolyRef(IntPtr navmesh, IntPtr tile);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileVerts")]
        public static extern int GetTileVerts(IntPtr tile
            , [In, Out] float[] verts
            , int vertsSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTilePolys")]
        public static extern int GetTilePolys(IntPtr tile
            , [In, Out] NavmeshPoly[] polys
            , int polysSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileDetailVerts")]
        public static extern int GetTileDetailVerts(IntPtr tile
            , [In, Out] float[] verts
            , int vertsSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileDetailTris")]
        public static extern int GetTileDetailTris(IntPtr tile
            , [In, Out] byte[] tris
            , int trisSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileDetailMeshes")]
        public static extern int GetTileDetailMeshes(IntPtr tile
            , [In, Out] NavmeshDetailMesh[] detailMeshes
            , int meshesSize);


        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileLinks")]
        public static extern int GetTileLinks(IntPtr tile
            , [In, Out] NavmeshLink[] links
            , int linksSize);


        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileBVTree")]
        public static extern int GetTileBVTree(IntPtr tile
            , [In, Out] NavmeshBVNode[] nodes
            , int nodesSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileConnections")]
        public static extern int GetTileConnections(IntPtr tile
            , [In, Out] NavmeshConnection[] conns
            , int connsSize);









        //[DllImport("cai-nav-rcn", EntryPoint = "bobo")]
        //public static extern void bobo(NavmeshTileData data);

        ///// <summary>
        ///// Creates an ready to use single tile dtNavMesh object.
        ///// </summary>
        ///// <remarks>
        ///// The object created using this method must be freed before its
        ///// last reference is released by calling the <see cref="FreeEx"/> 
        ///// method.  Otherwise a memory leak will occur.
        ///// </remarks>
        ///// <param name="polyMesh">The source polygon mesh.</param>
        ///// <param name="detailMesh">The source polygon detail mesh.</param>
        ///// <param name="walkableHeight">The Minimum floor to 'ceiling' 
        ///// height used to build the polygon mesh.</param>
        ///// <param name="walkableRadius">The closest any part 
        ///// of the polygon mesh gets to obstructions in the source geometry.
        ///// </param>
        ///// <param name="walkableStep">The maximum traversable ledge 
        ///// height used to build the polygon mesh.</param>
        ///// <param name="offMeshConnections">Off-mesh connections
        ///// associated with the polygon mesh. (Optional)</param>
        ///// <param name="resultMesh">A pointer to an initialized dtNavmesh
        ///// object.</param>
        ///// <returns>The <see cref="NavStatus" /> flags for the build
        ///// request.
        ///// </returns>
        //[DllImport("cai-nav-rcn", EntryPoint = "dtnmBuildSingleTileMesh")]
        //public static extern NavStatus BuildMesh(
        //    NavmeshTileBuildData buildData
        //    , ref IntPtr resultMesh);

        ////[DllImport("cai-nav-rcn"
        ////    , EntryPoint = "dtnmBuildSingleTileMeshRaw")]
        ////public static extern NavStatus BuildMesh(TileData tileData
        ////    , ref IntPtr resultMesh);

        ///// <summary>
        ///// Frees all unmanaged resources held by the dtNavMesh object.
        ///// </summary>
        ///// <param name="navmesh">The dtNavMesh object to free.</param>
        //[DllImport("cai-nav-rcn", EntryPoint = "freeDTNavMesh")]
        //public static extern void FreeEx(ref IntPtr navmesh);

        ///// <summary>
        ///// Gets the configuration parameters used to initialize the
        ///// navigation mesh.
        ///// </summary>
        ///// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        ///// <param name="parameters"></param>
        //[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetParams")]
        //public static extern void GetParams(IntPtr navmesh
        //    , [In, Out] NavmeshParams config);

        ///// <summary>
        ///// The maximum number of tiles supported by the navigation mesh.
        ///// </summary>
        ///// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        ///// <returns>The maximum number of tiles supported by the navigation 
        ///// mesh.</returns>
        //[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetMaxTiles")]
        //public static extern int GetMaxTiles(IntPtr navmesh);

        ///// <summary>
        ///// Indicates whether or not the specified polygon id is valid for
        ///// the navigation mesh.
        ///// </summary>
        ///// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        ///// <param name="polyRef">The polygon id to check.</param>
        ///// <returns>TRUE if the provided polygon id is valid.</returns>
        //[DllImport("cai-nav-rcn", EntryPoint = "dtnmIsValidPolyRef")]
        //public static extern bool IsValidPolyRef(IntPtr navmesh
        //    , uint polyRef);

        /////// <summary>
        /////// Returns the polygon information for a tile.
        /////// </summary>
        /////// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /////// <param name="tileIndex">The tile index.</param>
        /////// <param name="resultPolys">An array of initialized polygon
        /////// structures which will hold the result. 
        /////// (Minimum size: tile.polygonCount)</param>
        /////// <param name="polyCount">The number of polygons in the result
        /////// array.</param>
        /////// <param name="polysSize">The size of the result array.</param>
        /////// <returns>The status flags for the request.</returns>
        ////[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTilePolys")]
        ////public static extern NavStatus GetTilePolys(IntPtr navmesh
        ////    , int tileIndex
        ////    , [In, Out] NavmeshPoly[] resultPolys
        ////    , ref int polyCount
        ////    , int polysSize);

        /////// <summary>
        /////// Returns link data for a tile.
        /////// </summary>
        /////// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /////// <param name="tileIndex">The tile index.</param>
        /////// <param name="resultLinks">An array of link structures which will
        /////// hold the result. (Minimum size: tile.maxLinkCount)</param>
        /////// <param name="linkCount">The number of links returned in the result
        /////// array.</param>
        /////// <param name="linksSize">The size of the result array.</param>
        /////// <returns>The status flags for the request.</returns>
        ////[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileLinks")]
        ////public static extern NavStatus GetTileLinks(IntPtr navmesh
        ////    , int tileIndex
        ////    , [In, Out] NavmeshLink[] resultLinks
        ////    , ref int linkCount
        ////    , int linksSize);

        ///// <summary>
        ///// Returns the flags for the specified polygon.
        ///// </summary>
        ///// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        ///// <param name="polyRef">The polygon id.</param>
        ///// <param name="flags">The polygon's flags.</param>
        ///// <returns>The status flags for the request.</returns>
        //[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyFlags")]
        //public static extern NavStatus GetPolyFlags(IntPtr navmesh
        //    , uint polyRef
        //    , ref ushort flags);

        ///// <summary>
        ///// Sets the flags for the specified polygon.
        ///// </summary>
        ///// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        ///// <param name="polyRef">The polyon id.</param>
        ///// <param name="flags">The polygon's flags.</param>
        ///// <returns>The status flags for the request.</returns>
        //[DllImport("cai-nav-rcn", EntryPoint = "dtnmSetPolyFlags")]
        //public static extern NavStatus SetPolyFlags(IntPtr navmesh
        //    , uint polyRef
        //    , ushort flags);

        ///// <summary>
        ///// Returns the area id of the specified polygon.
        ///// </summary>
        ///// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        ///// <param name="polyRef">The polyon id.</param>
        ///// <param name="area">The polygon's area id.</param>
        ///// <returns>The status flags for the request.</returns>
        //[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyArea")]
        //public static extern NavStatus GetPolyArea(IntPtr navmesh
        //    , uint polyRef
        //    , ref byte area);

        ///// <summary>
        ///// Sets the area id of the specified polygon.
        ///// </summary>
        ///// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        ///// <param name="polyRef">The polyon id.</param>
        ///// <param name="area">The polygon's area id.</param>
        ///// <returns>The status flags for the request.</returns>
        //[DllImport("cai-nav-rcn", EntryPoint = "dtnmSetPolyArea")]
        //public static extern NavStatus SetPolyArea(IntPtr navmesh
        //    , uint polyRef
        //    , byte area);

        /////// <summary>
        /////// Returns the polygon vertices for a tile.
        /////// </summary>
        /////// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /////// <param name="tileIndex">The tile index.</param>
        /////// <param name="resultVerts">The polygon vertices in the form
        /////// (x, y, z). (Minimum size: tile.vertexCount * 3)
        /////// E.g.: 3 * <see cref="MaxVertsPerPolygon"/>)</param>
        /////// <param name="vertCount">The number of vertices in the result
        /////// array.</param>
        /////// <param name="vertsSize">The size of the result array.</param>
        /////// <returns>The status flags for the request.</returns>
        ////[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileVerts")]
        ////public static extern NavStatus GetTileVerts(IntPtr navmesh
        ////    , int tileIndex
        ////    , [In, Out] float[] resultVerts
        ////    , ref int vertCount
        ////    , int vertsSize);

        /////// <summary>
        /////// Returns the detail mesh information for a tile.
        /////// </summary>
        /////// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /////// <param name="tileIndex">The tile index.</param>
        /////// <param name="resultMeshes">An array of detail mesh structures
        /////// which will hold the result. (Minimum size: tile.detailMeshCount)
        /////// </param>
        /////// <param name="meshCount">The number of meshes returned in the
        /////// result array.</param>
        /////// <param name="meshesSize">The size of the result array.</param>
        /////// <returns>The status flags for the request.</returns>
        ////[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileDetailMeshes")]
        ////public static extern NavStatus GetTileDetailMeshes(IntPtr navmesh
        ////    , int tileIndex
        ////    , [In, Out] NavmeshPolyDetail[] resultMeshes
        ////    , ref int meshCount
        ////    , int meshesSize);

        /////// <summary>
        /////// Returns the detail polygon vertices for a tile.
        /////// </summary>
        /////// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /////// <param name="tileIndex">The tile index.</param>
        /////// <param name="resultVerts">The detail polygon vertices in the form
        /////// (x, y, z). (Minimum size: tile.detailVertCount * 3)</param>
        /////// <param name="vertCount">The number of vertices return in the
        /////// result.</param>
        /////// <param name="vertsSize">The size of the result array.</param>
        /////// <returns>The status flags for the request.</returns>
        ////[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileDetailVerts")]
        ////public static extern NavStatus GetTileDetailVerts(IntPtr navmesh
        ////    , int tileIndex
        ////    , [In, Out] float[] resultVerts
        ////    , ref int vertCount
        ////    , int vertsSize);

        /////// <summary>
        /////// Returns the indices and flags of the detail triangles for a tile.
        /////// </summary>
        /////// <remarks>See the <see cref="PolyMeshDetail"/> class description
        /////// for information on triangle flags.</remarks>
        /////// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /////// <param name="tileIndex">The tile index.</param>
        /////// <param name="resultTris">The indices for the detail triangles
        /////// in the form (vertAIndex, vertBIndex, vertCIndex, flags).
        /////// (Minimum size: tile.detailTriCount * 4)</param>
        /////// <param name="trisCount">The number of triangles returned
        /////// in the result array.</param>
        /////// <param name="trisSize">The size of the result array.</param>
        /////// <returns>The status flags for the request.</returns>
        ////[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileDetailTris")]
        ////public static extern NavStatus GetTileDetailTris(IntPtr navmesh
        ////    , int tileIndex
        ////    , [In, Out] byte[] resultTris
        ////    , ref int trisCount
        ////    , int trisSize);

        /////// <summary>
        /////// Returns bounding volume node information for a tile.
        /////// </summary>
        /////// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /////// <param name="tileIndex">The tile index.</param>
        /////// <param name="resultNodes">An array of initialized bounding volumne
        /////// structures which will hold the result.
        /////// (Minimum size: tile.bvNodeCount)</param>
        /////// <param name="nodeCount">The number of nodes returned in the
        /////// result array.</param>
        /////// <param name="nodesSize">The size of the result array.</param>
        /////// <returns>The status flags for the request.</returns>
        ////[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileBVTree")]
        ////public static extern NavStatus GetTileBVTree(IntPtr navmesh
        ////    , int tileIndex
        ////    , [In, Out] NavmeshBVNode[] resultNodes
        ////    , ref int nodeCount
        ////    , int nodesSize);

        /////// <summary>
        /////// Returns off-mesh connection information for a tile.
        /////// </summary>
        /////// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /////// <param name="tileIndex">The tile information.</param>
        /////// <param name="resultConns">An initialized array of connection
        /////// structures which will be loaded with the result.
        /////// (Minimum size: tile.offMeshConCount)</param>
        /////// <param name="connCount">The number of connections returned in the 
        /////// result array.</param>
        /////// <param name="connsSize">The size of the result array.</param>
        /////// <returns>The status flags for the request.</returns>
        ////[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileConnections")]
        ////public static extern NavStatus GetTileOffMeshCons(IntPtr navmesh
        ////    , int tileIndex
        ////    , [In, Out] NavmeshConnection[] resultConns
        ////    , ref int connCount
        ////    , int connsSize);

        ///// <summary>
        ///// Gets the endpoints for an off-mesh connection, ordered by
        ///// "direction of travel".
        ///// </summary>
        ///// <remarks>
        ///// <p>Off-mesh connections are stored in the navigation mesh as
        ///// special 2-vertex polygons with a single edge.  At least one of the
        ///// vertices is expected to be inside a normal polygon. So an off-mesh
        ///// connection is "entered" from a normal polygon at one of its 
        ///// endpoints. This is the polygon identified by startPolyRef.</p>
        ///// </remarks>
        ///// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        ///// <param name="startPolyRef">The polygon id in which the
        ///// start endpoint lies.</param>
        ///// <param name="connectionPolyRef">The off-mesh connection's polyon 
        ///// id.</param>
        ///// <param name="startPosition">The start position in the form
        ///// (x, y, z).</param>
        ///// <param name="endPosition">The end position in the form
        ///// (x, y, z).</param>
        ///// <returns>The status flags for the request.</returns>
        //[DllImport("cai-nav-rcn", EntryPoint = "dtnmGetConnectionEndPoints")]
        //public static extern NavStatus GetOffMeshConEndpoints(
        //    IntPtr navmesh
        //    , uint previousPolyRef
        //    , uint polyRef
        //    , [In, Out] float[] startPosition
        //    , [In, Out] float[] endPosition);
    }
}
