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

        [DllImport("cai-nav-rcn", EntryPoint = "freeDTNavMesh")]
        public static extern void FreeEx(ref IntPtr navmesh);

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
        public static extern void DeriveTileLocation(IntPtr navmesh
            , [In] float[] position
            , ref int tx
            , ref int tz);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileAt")]
        public static extern IntPtr GetTile(IntPtr navmesh
            , int x
            , int z
            , int layer);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTilesAt")]
        public static extern int GetTiles(IntPtr navmesh
            , int x
            , int z
            , [In, Out] IntPtr[] tiles
            , int tilesSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileRefAt")]
        public static extern uint GetTileRef(IntPtr navMesh
            , int x
            , int z
            , int layer);

        // The other get tile id method is in NavmeshTileEx.

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileByRef")]
        public static extern IntPtr GetTileByRef(IntPtr navmesh
            , uint tileRef);

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

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmIsValidPolyRef")]
        public static extern bool IsValidPolyRef(IntPtr navmesh
            , uint polyRef);

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

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyFlags")]
        public static extern NavStatus GetPolyFlags(IntPtr navmesh
            , uint polyRef
            , ref ushort flags);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmSetPolyFlags")]
        public static extern NavStatus SetPolyFlags(IntPtr navmesh
            , uint polyRef
            , ushort flags);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyArea")]
        public static extern NavStatus GetPolyArea(IntPtr navmesh
            , uint polyRef
            , ref byte area);

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
