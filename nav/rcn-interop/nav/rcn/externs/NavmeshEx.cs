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
    public static class NavmeshEx
    {
        // Source interface: DetourNavmeshEx.h

        public const int MaxAreas = 64;
        public const int MaxVertsPerPolygon = 6;

        [DllImport("cai-nav-rcn", EntryPoint = "rcnBuildStaticDTNavMesh")]
        public static extern NavmeshStatus BuildMesh(
            ref PolyMeshEx polyMesh
            , ref PolyMeshDetailEx detailMesh
            , float walkableHeight
            , float walkableRadius
            , float walkableClimb
            , ref NavWaypointsEx offMeshConnections
            , ref IntPtr resultMesh);

        [DllImport("cai-nav-rcn", EntryPoint = "freeDTNavMesh")]
        public static extern void FreeNavmesh(ref IntPtr dtNavMesh);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetParams")]
        public static extern void GetParams(IntPtr dtNavMesh
            , ref NavmeshParams parameters);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetMaxTiles")]
        public static extern int GetMaxTiles(IntPtr mesh);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmIsValidPolyRef")]
        public static extern bool IsValidPolyId(IntPtr mesh
            , uint polyId);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileInfo")]
        public static extern NavmeshStatus GetTileInfo(IntPtr mesh
            , int tileIndex
            , ref NavmeshTileData tileInfo);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTilePolys")]
        public static extern NavmeshStatus GetTilePolys(IntPtr mesh
            , int tileIndex
            , [In, Out] NavmeshPoly[] result
            , int resultSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileLinks")]
        public static extern NavmeshStatus GetTileLinks(IntPtr mesh
            , int tileIndex
            , [In, Out] NavmeshLink[] links
            , int linkSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyFlags")]
        public static extern NavmeshStatus GetPolyFlags(IntPtr mesh
            , uint polyId
            , ref ushort flags);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmSetPolyFlags")]
        public static extern NavmeshStatus SetPolyFlags(IntPtr mesh
            , uint polyId
            , ushort flags);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyArea")]
        public static extern NavmeshStatus GetPolyArea(IntPtr mesh
            , uint polyRef
            , ref byte area);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmSetPolyArea")]
        public static extern NavmeshStatus SetPolyArea(IntPtr mesh
            , uint polyRef
            , byte area);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileVerts")]
        public static extern NavmeshStatus GetTileVerts(IntPtr mesh
            , int tileIndex
            , [In, Out] float[] verts
            , int vertsSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileDetailMeshes")]
        public static extern NavmeshStatus GetTileDetailMeshes(IntPtr mesh
            , int tileIndex
            , [In, Out] NavmeshPolyDetail[] detailMeshes
            , int meshesSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileDetailVerts")]
        public static extern NavmeshStatus GetTileDetailVerts(IntPtr mesh
            , int tileIndex
            , [In, Out] float[] verts
            , int vertsSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileDetailTris")]
        public static extern NavmeshStatus GetTileDetailTris(IntPtr mesh
            , int tileIndex
            , [In, Out] byte[] tris
            , int trisSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileBVTree")]
        public static extern NavmeshStatus GetTileBVTree(IntPtr mesh
            , int tileIndex
            , [In, Out] BVNode[] nodes
            , int nodesSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileConnections")]
        public static extern NavmeshStatus GetTileOffMeshCons(IntPtr mesh
            , int tileIndex
            , [In, Out] NavmeshConnection[] conns
            , int connsSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetConnectionEndPoints")]
        public static extern NavmeshStatus GetOffMeshConEndpoints(
            IntPtr mesh
            , uint previousPolyId
            , uint polyId
            , [In, Out] float[] startPosition
            , [In, Out] float[] endPosition);
    }
}
