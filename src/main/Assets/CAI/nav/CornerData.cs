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
    /// Represents local corner data for a path within a path corridor. 
    /// (Generated during path straightening.)
    /// </summary>
    /// <remarks>
    /// <para>When path straightening occurs on a path corridor, the waypoints
    /// can include corners lying on the vertex of a polygon's solid wall
    /// segment.  These are the vertices included in this data structure.</para>
    /// <para>
    /// If path straightening does not result in any corners (e.g. path end
    /// point is visible) then the <see cref="cornerCount"/> will be zero.  So
    /// <see cref="cornerCount"/> can't be used to detect 'no path'.</para>
    /// <para>
    /// Certain methods which take objects of this type require a fixed buffer 
    /// size equal to <see cref="MarshalBufferSize"/>.  So be careful when 
    /// initializing and using objects of this type.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public class CornerData
    {
        /*
         * Design note:
         * 
         * Implemented as a class to permit use as a buffer.
         * 
         */

        /// <summary>
        /// The required maximum number of corners required to use with interop
        /// method calls.
        /// </summary>
        public const int MarshalBufferSize = 4;

        /// <summary>
        /// The corner vertices. [Form: (x, y, z) * <see cref="cornerCount"/>]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3 * MarshalBufferSize)]
        public float[] verts;

        /// <summary>
        /// The <see cref="WaypointFlag"/>'s for each corner.
        /// [Form: (flag) * <see cref="cornerCount"/>]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MarshalBufferSize)]
        public WaypointFlag[] flags;

        /// <summary>
        /// The navigation mesh polygon references for each corner.
        /// [Form: (polyRef) * <see cref="cornerCount"/>]
        /// </summary>
        /// <remarks>
        /// The reference is for the polygon being entered at the corner.
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MarshalBufferSize)]
        public uint[] polyRefs;

        /// <summary>
        /// Number of corners in the local path.
        /// [Limits: 0 &lt;= value &lt;= maxCorners]
        /// </summary>
        public int cornerCount = 0;

        /// <summary>
        /// Creates an object with buffers sized for use with interop method 
        /// calls. (Maximum Corners = <see cref="MarshalBufferSize"/>)
        /// </summary>
        public CornerData()
        {
            verts = new float[3 * MarshalBufferSize];
            flags = new WaypointFlag[MarshalBufferSize];
            polyRefs = new uint[MarshalBufferSize];
        }

        /// <summary>
        /// Creates an object with a non-standard buffer size.
        /// </summary>
        /// <param name="maxCorners">The maximum number of corners the
        /// buffers can hold.</param>
        public CornerData(int maxCorners)
        {
            verts = new float[3 * maxCorners];
            flags = new WaypointFlag[maxCorners];
            polyRefs = new uint[maxCorners];
        }
    }
}
