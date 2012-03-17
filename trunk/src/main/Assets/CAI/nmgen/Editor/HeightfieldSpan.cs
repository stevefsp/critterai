﻿/*
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

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents a span within a <see cref="Heightfield"/> object.
    /// </summary>
    /// <remarks>
    /// <para>Useful instances of this type can only by obtained from a <see cref="Heightfield"/> 
    /// object.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct HeightfieldSpan
    {
        private uint mPacked;  // [Area, Max, Min]

        /// <summary>
        /// The miniumum height of span.
        /// </summary>
        public ushort Min { get { return (ushort)(mPacked & 0x1fff ); } }

        /// <summary>
        /// The maximum height of the span.
        /// </summary>
        public ushort Max 
        { 
            get { return (ushort)((mPacked >> 13) & 0x1fff); } 
        }

        /// <summary>
        /// The area id assigned to the span.
        /// </summary>
        public byte Area { get { return (byte)(mPacked >> 26); } }
    }
}