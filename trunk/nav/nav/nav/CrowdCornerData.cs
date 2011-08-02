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
    /// a <see cref="cornerCount"/> can't be used to detect 'no path'.</para>
    /// <para>
    /// Instances of this class are required by 
    /// <see cref="CrowdAgent.GetCornerData"/>.
    /// </para>
    /// <para>This class is used as an interop buffer.  Behavior is undefined
    /// if the size of the array fields are changed after construction.</para>
    /// </remarks>
    /// <seealso cref="CrowdAgent.GetCornerData"/>
    /// <seealso cref="CrowdAgent"/>
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
        /// <remarks>
        /// <para>Used to size instance buffers.</para>
        /// </remarks>
        public const int MaxCorners = 4;

        /// <summary>
        /// The corner vertices. [Form: (x, y, z) * <see cref="cornerCount"/>]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3 * MaxCorners)]
        public float[] verts = new float[3 * MaxCorners];

        /// <summary>
        /// The <see cref="WaypointFlag"/>'s for each corner.
        /// [Form: (flag) * <see cref="cornerCount"/>]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCorners)]
        public WaypointFlag[] flags = new WaypointFlag[MaxCorners];

        /// <summary>
        /// The navigation mesh polygon references for each corner.
        /// [Form: (polyRef) * <see cref="cornerCount"/>]
        /// </summary>
        /// <remarks>
        /// The reference is for the polygon being entered at the corner.
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCorners)]
        public uint[] polyRefs = new uint[MaxCorners];

        /// <summary>
        /// Number of corners in the local path.
        /// [Limits: 0 &lt;= value &lt;= <see cref="MaxCorners"/>]
        /// </summary>
        public int cornerCount = 0;
    }
}
