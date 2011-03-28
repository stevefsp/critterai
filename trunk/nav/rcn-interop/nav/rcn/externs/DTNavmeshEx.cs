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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn.externs
{
    public static class DTNavmeshEx
    {
        // Source interface: DetourNavmeshEx.h

        public const int MaxAreas = 64;
        public const int MaxVertsPerPolygon = 6;

        [DllImport("cai-nav-rcn", EntryPoint = "freeDTNavMesh")]
        public static extern void FreeNavmesh(ref IntPtr dtNavMesh);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetParams")]
        public static extern void GetParams(IntPtr dtNavMesh
            , ref DTNavMeshParams parameters);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetMaxTiles")]
        public static extern int GetMaxTiles(IntPtr pNavMesh);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmIsValidPolyRef")]
        public static extern bool IsValidPolyId(IntPtr pNavMesh
            , uint polyId);

        //[DllImport("cai-nav-rcn"
        //    , EntryPoint = "dtnmGetOffMeshConnectionPolyEndPoints")]
        //public static extern uint GetOffMeshConnectionPolyEndPoints(
        //    IntPtr pNavMesh
        //    , uint prevPolyId
        //    , uint polyId
        //    , ref float[] startPos
        //    , ref float[] endPos);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetTileInfo")]
        public static extern uint GetTileInfo(IntPtr pNavMesh
        , int tileIndex
        , ref DTTileInfo tileInfo);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyInfo")]
        public static extern uint GetPolyInfo(IntPtr pNavMesh
            , uint polyId
            , ref DTPolyInfo polyInfo);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyFlags")]
        public static extern uint GetPolyFlags(IntPtr pNavMesh
                , uint polyId
                , ref ushort flags);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmSetPolyFlags")]
        public static extern uint SetPolyFlags(IntPtr pNavMesh
        , uint polyId
        , ushort flags);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmGetPolyArea")]
        public static extern uint GetPolyArea(IntPtr pNavMesh
            , uint polyRef
            , ref byte area);

        [DllImport("cai-nav-rcn", EntryPoint = "dtnmSetPolyArea")]
        public static extern uint SetPolyArea(IntPtr pNavMesh
            , uint polyRef
            , byte area);
    }
}
