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
using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    /// <summary>
    /// Boundary data for crowd agents.
    /// </summary>
    ///<remarks><para>Minimal available documentation.</para></remarks>
    [StructLayout(LayoutKind.Sequential)]
    public class LocalBoundaryData
    {
        /*
         * Design note:
         * 
         * Implemented as a class to permit use as a buffer.
         * 
         */

        /// <summary>
        /// The maximum allowed segments.
        /// </summary>
        public const int MaxSegments = 8;

        /// <summary>
        /// Center. [Form: (x, y, z)]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] center = new float[3];

        /// <summary>
        /// Segments. [Form: (ax, ay, az, bx, by, bz) * segmentCount]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSegments * 6)]
        public float[] segments = new float[MaxSegments * 6];

        /// <summary>
        /// Segement count.
        /// </summary>
        public int segmentCount = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LocalBoundaryData() { }
    }
}
