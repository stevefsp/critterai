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
    /// <para>Used as a buffer for corridor data returned by other classes.
    /// </para>
    /// <para>This class is used as an interop buffer.  Behavior is undefined
    /// if the size of the array fields are changed after construction.</para>
    /// </remarks>
    /// <see cref="CrowdAgent.GetCorridor"/>
    /// <see cref="CrowdAgent"/>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class PathCorridorData
    {
        /// <summary>
        /// The maximum path size the class can hold.
        /// </summary>
        /// <remarks>
        /// <para>Used to size instance buffers.</para>
        /// </remarks>
        public const int MaxPathSize = 256;

        /// <summary>
        /// The current position within the path corridor. [Form: (x, y, z)]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] position = new float[3];

        /// <summary>
        /// The target position within the path corridor. [Form: (x, y, z)]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] target = new float[3];

        /// <summary>
        /// An ordered list of polygon references representing the corridor.
        /// </summary>
        /// <remarks>
        /// <para>[Form: (polyRef) * <see cref="pathCount"/></para>
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPathSize)]
        public uint[] path = new uint[MaxPathSize];

        /// <summary>
        /// The number of polygons in the path.
        /// </summary>
        public int pathCount;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PathCorridorData() { }
    }
}
