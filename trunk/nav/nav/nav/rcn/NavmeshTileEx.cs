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
    }
}
