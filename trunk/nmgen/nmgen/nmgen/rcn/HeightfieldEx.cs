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
    internal static class HeightfieldEx
    {
        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfAllocField")]
        public static extern IntPtr Alloc(int width
            , int depth
            , [In] float[] boundsMin
            , [In] float[] boundsMax
            , float xzCellSize
            , float yCellSize);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfFreeField")]
        public static extern void FreeEx(IntPtr hf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfRasterizeTriangle")]
        public static extern bool AddTriangle(IntPtr context
            , [In] float[] verts
            , byte area
            , IntPtr hf
            , int flagMergeThreshold);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfRasterizeTriMesh")]
        public static extern bool AddTriangles(IntPtr context
            , [In] float[] verts
            , int vertCount
            , [In] int[] tris
            , [In] byte[] areas
            , int triCount
            , IntPtr hf
            , int flagMergeThreshold);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfRasterizeTriMeshShort")]
        public static extern bool AddTriangles(IntPtr context
            , [In] float[] verts
            , int vertCount
            , [In] ushort[] tris
            , [In] byte[] areas
            , int triCount
            , IntPtr hf
            , int flagMergeThreshold);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfRasterizeTriangles")]
        public static extern bool AddTriangles(IntPtr context
            , [In] float[] verts
            , [In] byte[] areas
            , int triCount
            , IntPtr hf
            , int flagMergeThreshold);

        [DllImport("cai-nmgen-rcn"
            , EntryPoint = "nmhfFilterLowHangingWalkableObstacles")]
        public static extern bool FlagLowObstaclesWalkable(IntPtr context
            , int walkableStep
            , IntPtr hf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfFilterLedgeSpans")]
        public static extern bool FlagLedgeSpansNotWalkable(IntPtr context
            , int walkableHeight
            , int walkableStep
            , IntPtr hf);

        [DllImport("cai-nmgen-rcn"
            , EntryPoint = "nmhfFilterWalkableLowHeightSpans")]
        public static extern bool FlagLowHeightSpansNotWalkable(IntPtr context
            , int walkableHeight
            , IntPtr hf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfGetHeightFieldSpanCount")]
        public static extern int GetSpanCount(IntPtr hf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfGetMaxSpansInColumn")]
        public static extern int GetMaxSpansInColumn(IntPtr hf);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmhfGetSpans")]
        public static extern int GetSpans(IntPtr hf
            , int widthIndex
            , int depthIndex
            , [In, Out] HeightfieldSpan[] spanBuffer
            , int bufferSize);
    }
}
