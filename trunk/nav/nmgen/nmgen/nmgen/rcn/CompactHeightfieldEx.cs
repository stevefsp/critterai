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

namespace org.critterai.nmgen.rcn
{
    internal static class CompactHeightfieldEx
    {
        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfBuildField")]
        public static extern bool Build(IntPtr context
            , int walkableHeight
            , int walkableStep
            , IntPtr sourceField
            , [In, Out] CompactHeightfield compactField);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfFreeFieldData")]
        public static extern void FreeDataEx(
            [In, Out] CompactHeightfield chf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfGetCellData")]
        public static extern bool GetCellData([In] CompactHeightfield chf
            , [In, Out] CompactCell[] cells
            , int cellsSize);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfGetSpanData")]
        public static extern bool GetSpanData([In] CompactHeightfield chf
            , [In, Out] CompactSpan[] spans
            , int spansSize);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfErodeWalkableArea")]
        public static extern bool ErodeWalkableArea(IntPtr context
            , int radius
            , [In, Out] CompactHeightfield chf);

        [DllImport("cai-nmgen-rcn"
            , EntryPoint = "nmcfMedianFilterWalkableArea")]
        public static extern bool ApplyMedianFilter(IntPtr context
            , [In, Out] CompactHeightfield chf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "rcMarkBoxArea")]
        public static extern bool MarkArea(IntPtr context
            , [In] float[] bmin
            , [In] float[] bmax
            , byte areaId
            , [In, Out] CompactHeightfield chf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfMarkConvexPolyArea")]
        public static extern bool MarkArea(IntPtr context
            , [In] float[] verts
            , int vertCount
            , float heightMin
            , float heightMax
            , byte areaId
            , [In, Out] CompactHeightfield chf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfMarkCylinderArea")]
        public static extern bool MarkArea(IntPtr context
            , [In] float[] position
            , float radius
            , float height
            , byte area
            , [In, Out] CompactHeightfield chf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfBuildDistanceField")]
        public static extern bool BuildDistanceField(IntPtr context
            , [In, Out] CompactHeightfield chf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfBuildRegions")]
        public static extern bool BuildRegions(IntPtr context
            , [In, Out] CompactHeightfield chf
            , int borderSize
            , int minRegionArea
            , int mergeRegionArea);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmcfBuildRegionsMonotone")]
        public static extern bool BuildRegionsMonotone(IntPtr context
            , [In, Out] CompactHeightfield chf
            , int borderSize
            , int minRegionArea
            , int mergeRegionArea);
    }
}
