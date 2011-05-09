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

namespace org.critterai.nav
{
    /// <summary>
    /// Path corridor data.
    /// </summary>
    /// <remarks>
    /// <p>Must be initialized before use.</p>
    /// <p>This data is provided for debug purposes.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public class PathCorridorData
    {
        /// <summary>
        /// The size of the path buffer.
        /// </summary>
        public const int MaxPathSize = 256;

        /// <summary>
        /// The current position within the path corridor in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] position = new float[3];

        /// <summary>
        /// The target position within the path corridor in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] target = new float[3];

        /// <summary>
        /// An ordered list of polygon ids representing the corridor.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPathSize)]
        public uint[] path = new uint[MaxPathSize];

        /// <summary>
        /// The number of polygons in the path.
        /// </summary>
        public int pathCount;

        public PathCorridorData() { }
    }
}
