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
    [StructLayout(LayoutKind.Sequential)]
    public struct NMGenParamsEx
    {
        public const float MaxAllowedSlope = 85.0f;
        public const int MaxAllowedSmoothing = 4;
        public const int MaxAllowedVertsPerPoly = 6;

        public float xzCellSize;
        public float yCellSize;
        public float minTraversableHeight;
        public float maxTraversableStep;
        public float maxTraversableSlope;
        public float traversableAreaBorderSize;
        public float heightfieldBorderSize;
        public float maxEdgeLength;
        public float edgeMaxDeviation;
        public float contourSampleDistance;
        public float contourMaxDeviation;
        public int smoothingThreshold;
        public int minIslandRegionSize;
        public int mergeRegionSize;
        public int maxVertsPerPoly;
        public bool clipLedges;

        public NMGenParamsEx(float xzCellSize
                , float yCellSize
                , float minTraversableHeight
                , float maxTraversableStep
                , float maxTraversableSlope
                , bool clipLedges
                , float traversableAreaBorderSize
                , float heightfieldBorderSize
                , int smoothingThreshold
                , int minIslandRegionSize
                , int mergeRegionSize
                , float maxEdgeLength
                , float edgeMaxDeviation
                , int maxVertsPerPoly
                , float contourSampleDistance
                , float contourMaxDeviation)
        {
            this.xzCellSize = xzCellSize;
            this.yCellSize = yCellSize;
            this.minTraversableHeight = minTraversableHeight;
            this.maxTraversableStep = maxTraversableStep;
            this.maxTraversableSlope = maxTraversableSlope;
            this.clipLedges = clipLedges;
            this.traversableAreaBorderSize = traversableAreaBorderSize;
            this.heightfieldBorderSize = heightfieldBorderSize;
            this.smoothingThreshold = smoothingThreshold;
            this.minIslandRegionSize = minIslandRegionSize;
            this.mergeRegionSize = mergeRegionSize;
            this.maxEdgeLength = maxEdgeLength;
            this.edgeMaxDeviation = edgeMaxDeviation;
            this.maxVertsPerPoly = maxVertsPerPoly;
            this.contourSampleDistance = contourSampleDistance;
            this.contourMaxDeviation = contourMaxDeviation;
        }

        [DllImport("cai-nav-rcn", EntryPoint = "rcnApplyNavMeshConfigLimits")]
        public static extern void ApplyStandardLimits(
            ref NMGenParamsEx config);
    }
}
