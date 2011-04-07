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

namespace org.critterai.nav.rcn
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CrowdLocalBoundaryData
    {
        public const int MaxSegments = 8;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] center;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6 * MaxSegments)]
        public float[] segments;

        public int segmentCount;

        public void Initialize()
        {
            center = new float[3];
            segments = new float[6 * MaxSegments];
            segmentCount = 0;
        }

        public void Reset()
        {
            segmentCount = 0;
            Array.Clear(center, 0, center.Length);
            Array.Clear(segments, 0, segments.Length);
        }

        public static CrowdLocalBoundaryData Initialized
        {
            get
            {
                CrowdLocalBoundaryData data = new CrowdLocalBoundaryData();
                data.Initialize();
                return data;
            }
        }
    }
}
