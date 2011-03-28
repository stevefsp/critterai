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
        public struct RCNNavWaypointsEx
        {
            public const int MaxConnections = 256;

            private int mCount;

            /// <summary>
            /// Pointer to unmanaged float array in the form 
            /// (ax, ay, az, bx, by, bz).
            /// </summary>
            private IntPtr mVertices;

            /// <summary>
            /// Pointer to unmanaged float array in the form (radius)
            /// </summary>
            private IntPtr mRadii;

            /// <summary>
            /// Pointer to an unmanaged unsigned char (byte) array.
            /// </summary>
            private IntPtr mDirs;

            /// <summary>
            /// Unsigned char (byte).
            /// </summary>
            private IntPtr mAreas;

            /// <summary>
            /// Unsigned short array.
            /// </summary>
            private IntPtr mFlags;

            /// <summary>
            /// Unsigned int array.
            /// </summary>
            public IntPtr mIds;

            public int Count { get { return  mCount; } }

            public float[] GetVertices()
            {
                return UtilEx.ExtractArrayFloat(mVertices, mCount * 6);
            }

            public float[] GetRadii()
            {
                return UtilEx.ExtractArrayFloat(mRadii, mCount);
            }

            public byte[] GetDirs()
            {
                return UtilEx.ExtractArrayByte(mDirs, mCount);
            }

            public byte[] GetAreas()
            {
                return UtilEx.ExtractArrayByte(mAreas, mCount);
            }

            public ushort[] GetFlags()
            {
                return UtilEx.ExtractArrayUShort(mFlags, mCount);
            }

            public uint[] GetIds()
            {
                return UtilEx.ExtractArrayUInt(mIds, mCount);
            }

            public static RCNNavWaypointsEx Empty
            {
                get
                {
                    RCNNavWaypointsEx conns = new RCNNavWaypointsEx();
                    conns.mCount = 0;
                    conns.mVertices = IntPtr.Zero;
                    conns.mRadii = IntPtr.Zero;
                    conns.mDirs = IntPtr.Zero;
                    conns.mAreas = IntPtr.Zero;
                    conns.mFlags = IntPtr.Zero;
                    conns.mIds = IntPtr.Zero;
                    return conns;
                }
            }

            public RCNNavWaypointsEx(float[] vertices
                , float[] radii
                , byte[] dirs
                , byte[] areas
                , ushort[] flags
                , uint[] ids)
            {
                int count = (vertices == null ? 0 : vertices.Length / 6);
                if (count == 0 || count > MaxConnections
                    || vertices.Length % 6 != 0
                    || radii == null || radii.Length != count
                    || dirs == null || dirs.Length != count
                    || (areas != null && areas.Length != count)
                    || (flags != null && flags.Length != count)
                    || (ids != null && ids.Length != count))
                {
                    this.mCount = 0;
                    this.mVertices = IntPtr.Zero;
                    this.mRadii = IntPtr.Zero;
                    this.mDirs = IntPtr.Zero;
                    this.mAreas = IntPtr.Zero;
                    this.mFlags = IntPtr.Zero;
                    this.mIds = IntPtr.Zero;
                    return;
                }

                this.mCount = count;

                this.mVertices = 
                    UtilEx.GetFilledBuffer(vertices, vertices.Length);
                this.mRadii =
                    UtilEx.GetFilledBuffer(radii, radii.Length);
                this.mDirs =
                    UtilEx.GetFilledBuffer(dirs, dirs.Length);

                if (areas == null)
                    this.mAreas = UtilEx.GetBuffer(count, true);
                else
                    this.mAreas = UtilEx.GetFilledBuffer(areas, areas.Length);

                if (flags == null)
                    this.mFlags = 
                        UtilEx.GetBuffer(sizeof(ushort) * count, true);
                else
                    this.mFlags = UtilEx.GetFilledBuffer(flags, flags.Length);

                if (ids == null)
                    this.mIds = UtilEx.GetBuffer(sizeof(int) * count, true);
                else
                    this.mIds = UtilEx.GetFilledBuffer(ids, ids.Length);

            }

            public static void Free(ref RCNNavWaypointsEx conns)
            {
                conns.mCount = 0;
                Marshal.FreeHGlobal(conns.mVertices);
                Marshal.FreeHGlobal(conns.mRadii);
                Marshal.FreeHGlobal(conns.mDirs);
                Marshal.FreeHGlobal(conns.mAreas);
                Marshal.FreeHGlobal(conns.mFlags);
                Marshal.FreeHGlobal(conns.mIds);
                conns.mVertices = IntPtr.Zero;
                conns.mRadii = IntPtr.Zero;
                conns.mDirs = IntPtr.Zero;
                conns.mAreas = IntPtr.Zero;
                conns.mFlags = IntPtr.Zero;
                conns.mIds = IntPtr.Zero;
            }
        }



}
