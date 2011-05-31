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
    /// Crowd agent corner data.
    /// </summary>
    /// <remarks><p>Minimal available documentation.</p></remarks>
    [StructLayout(LayoutKind.Sequential)]
    public class CrowdCornerData
    {
        /*
         * Design note:
         * 
         * Implemented as a class to permit use as a buffer.
         * 
         */

        /// <summary>
        /// The maximum number of corners the corner fields
        /// can hold.
        /// </summary>
        public const int MaxCorners = 4;

        /// <summary>
        /// The corner vertices of the agent's local path.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3 * MaxCorners)]
        public float[] verts = new float[3 * MaxCorners];

        /// <summary>
        /// The corner flags of the agent's local path.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCorners)]
        public WaypointFlag[] flags = new WaypointFlag[MaxCorners];

        /// <summary>
        /// The polygon references of the agent's local path.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCorners)]
        public uint[] polyRefs = new uint[MaxCorners];

        /// <summary>
        /// Nubmer of corners.
        /// </summary>
        public int cornerCount = 0;
    }
}
