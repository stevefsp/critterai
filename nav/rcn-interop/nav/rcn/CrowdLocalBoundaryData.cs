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
    // TODO: DOC

    /// <summary>
    /// Crowd agent local boundary data.
    /// </summary>
    /// <remarks>
    /// <p>Must be initialized before use.</p>
    /// <p>This data is provided for debug purposes.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct CrowdLocalBoundaryData
    {
        public const int MaxSegments = 8;

        /// <summary>
        /// Undocumented.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] center;

        /// <summary>
        /// Undocumented
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6 * MaxSegments)]
        public float[] segments;

        /// <summary>
        /// Undocumented
        /// </summary>
        public int segmentCount;

        /// <summary>
        /// Initializes the structure before its first use.
        /// </summary>
        /// <remarks>
        /// Existing references are released and replaced.
        /// </remarks>
        public void Initialize()
        {
            center = new float[3];
            segments = new float[6 * MaxSegments];
            segmentCount = 0;
        }

        /// <summary>
        /// Resets all values to zero.
        /// </summary>
        public void Reset()
        {
            segmentCount = 0;
            Array.Clear(center, 0, center.Length);
            Array.Clear(segments, 0, segments.Length);
        }
    }
}
