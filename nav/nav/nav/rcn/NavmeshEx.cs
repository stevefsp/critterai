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
    internal static class NavmeshEx
    {
        // Source declarations: DetourNavmeshEx.h

        /// <summary>
        /// A flag that indicates a link is external.  (Context dependant.)
        /// </summary>
        public const ushort ExternalLink = 0x8000;

        /// <summary>
        /// Indicates that a link is null. (Does not link to anything.)
        /// </summary>
        public const uint NullLink = 0xffffffff;

        /// <summary>
        /// The maximum supported number of areas.
        /// </summary>
        public const int MaxAreas = 64;

        /// <summary>
        /// The maximum vertices per polygon.
        /// </summary>
        public const int MaxVertsPerPolygon = 6;

        //[DllImport("cai-nav-rcn", EntryPoint = "bobo")]
        //public static extern void bobo(NavmeshTileData data);

        /// <summary>
        /// Creates an ready to use single tile dtNavMesh object.
        /// </summary>
        /// <remarks>
        /// The object created using this method must be freed before its
        /// last reference is released by calling the <see cref="FreeEx"/> 
        /// method.  Otherwise a memory leak will occur.
        /// </remarks>
        /// <param name="polyMesh">The source polygon mesh.</param>
        /// <param name="detailMesh">The source polygon detail mesh.</param>
        /// <param name="walkableHeight">The Minimum floor to 'ceiling' 
        /// height used to build the polygon mesh.</param>
        /// <param name="walkableRadius">The closest any part 
        /// of the polygon mesh gets to obstructions in the source geometry.
        /// </param>
        /// <param name="walkableStep">The maximum traversable ledge 
        /// height used to build the polygon mesh.</param>
        /// <param name="offMeshConnections">Off-mesh connections
        /// associated with the polygon mesh. (Optional)</param>
        /// <param name="resultMesh">A pointer to an initialized dtNavmesh
        /// object.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the build
        /// request.
        /// </returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnmBuildSingleTileMesh")]
        public static extern NavStatus BuildMesh(
            NavmeshTileBuildData buildData
            , ref IntPtr resultMesh);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmBuildDTNavMeshFromRaw")]
        public static extern NavStatus BuildMesh([In] byte[] rawMeshData
            , int dataSize
            , ref IntPtr resultNavMesh);

        [DllImport("cai-nav-rcn", EntryPoint = "rcnInitTiledDTNavMesh")]
        public static extern NavStatus BuildMesh(NavmeshParams config
            , ref IntPtr navmesh);

        /// <summary>
        /// Frees all unmanaged resources held by the dtNavMesh object.
        /// </summary>
        /// <param name="navmesh">The dtNavMesh object to free.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "freeDTNavMesh")]
        public static extern void FreeEx(ref IntPtr navmesh);

        /// <summary>
        /// Gets the configuration parameters used to initialize the
        /// navigation mesh.
        /// </summary>
        /// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /// <param name="parameters"></param>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetParams")]
        public static extern void GetParams(IntPtr navmesh
            , [In, Out] NavmeshParams config);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmAddTile")]
        public static extern NavStatus AddTile(IntPtr navmesh
            , [In, Out] NavmeshTileData tileData
            , uint lastRef
            , ref uint resultRef);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmRemoveTile")]
        public static extern NavStatus RemoveTile(IntPtr navmesh
            , uint tileRef);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmCalcTileLoc")]
        public static extern void CalculateTileLocation(IntPtr navmesh
            , [In] float[] position
            , ref int tx
            , ref int ty);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileAt")]
        public static extern IntPtr GetTile(IntPtr navmesh
            , int x
            , int y
            , int layer);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTilesAt")]
        public static extern int GetTiles(IntPtr navmesh
            , int x
            , int y
            , [In, Out] IntPtr[] tiles
            , int tilesSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileRefAt")]
        public static extern uint GetTileRef(IntPtr navMesh
            , int x
            , int y
            , int layer);

        // The other get tile id method is in NavmeshTileEx.

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileByRef")]
        public static extern IntPtr GetTileByRef(IntPtr navmesh
            , uint tileRef);

        /// <summary>
        /// The maximum number of tiles supported by the navigation mesh.
        /// </summary>
        /// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /// <returns>The maximum number of tiles supported by the navigation 
        /// mesh.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetMaxTiles")]
        public static extern int GetMaxTiles(IntPtr navmesh);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTile")]
        public static extern IntPtr GetTile(IntPtr navmesh
            , int tileIndex);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileAndPolyByRef")]
        public static extern NavStatus GetTileAndPoly(IntPtr navmesh
            , uint polyRef
            , ref IntPtr tile
            , ref IntPtr poly);

        /// <summary>
        /// Indicates whether or not the specified polygon id is valid for
        /// the navigation mesh.
        /// </summary>
        /// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /// <param name="polyRef">The polygon id to check.</param>
        /// <returns>TRUE if the provided polygon id is valid.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnmIsValidPolyRef")]
        public static extern bool IsValidPolyRef(IntPtr navmesh
            , uint polyRef);

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
        /// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /// <param name="startPolyRef">The polygon id in which the
        /// start endpoint lies.</param>
        /// <param name="connectionPolyRef">The off-mesh connection's polyon 
        /// id.</param>
        /// <param name="startPosition">The start position in the form
        /// (x, y, z).</param>
        /// <param name="endPosition">The end position in the form
        /// (x, y, z).</param>
        /// <returns>The status flags for the request.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetConnectionEndPoints")]
        public static extern NavStatus GetConnEndpoints(
            IntPtr navmesh
            , uint previousPolyRef
            , uint polyRef
            , [In, Out] float[] startPosition
            , [In, Out] float[] endPosition);


        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetOffMeshConnectionByRef")]
        public static extern IntPtr GetConnectionByRef(IntPtr navmesh
            , uint polyRef);

        /// <summary>
        /// Returns the flags for the specified polygon.
        /// </summary>
        /// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /// <param name="polyRef">The polygon id.</param>
        /// <param name="flags">The polygon's flags.</param>
        /// <returns>The status flags for the request.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyFlags")]
        public static extern NavStatus GetPolyFlags(IntPtr navmesh
            , uint polyRef
            , ref ushort flags);

        /// <summary>
        /// Sets the flags for the specified polygon.
        /// </summary>
        /// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /// <param name="polyRef">The polyon id.</param>
        /// <param name="flags">The polygon's flags.</param>
        /// <returns>The status flags for the request.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnmSetPolyFlags")]
        public static extern NavStatus SetPolyFlags(IntPtr navmesh
            , uint polyRef
            , ushort flags);

        /// <summary>
        /// Returns the area id of the specified polygon.
        /// </summary>
        /// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /// <param name="polyRef">The polyon id.</param>
        /// <param name="area">The polygon's area id.</param>
        /// <returns>The status flags for the request.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyArea")]
        public static extern NavStatus GetPolyArea(IntPtr navmesh
            , uint polyRef
            , ref byte area);

        /// <summary>
        /// Sets the area id of the specified polygon.
        /// </summary>
        /// <param name="navmesh">A pointer to a dtNavMesh object.</param>
        /// <param name="polyRef">The polyon id.</param>
        /// <param name="area">The polygon's area id.</param>
        /// <returns>The status flags for the request.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtnmSetPolyArea")]
        public static extern NavStatus SetPolyArea(IntPtr navmesh
            , uint polyRef
            , byte area);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetDTNavMeshRawData")]
        public static extern void GetSerializedNavmesh(IntPtr navmesh
            , ref IntPtr resultData
            , ref int dataSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmFreeDTNavMeshRawData")]
        public static extern void FreeSerializedNavmeshEx(ref IntPtr data);
    }
}
