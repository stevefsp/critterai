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
using org.critterai.interop;

namespace org.critterai.nmgen
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class Contour
        : IManagedObject
    {
        public const int RegionMask = 0xfff;

	    private IntPtr mVerts;			// int[vertCount * 4]
        private int mVertCount;
        private IntPtr mRawVerts;		// int[rawVertCount * 4]
        private int mRawVertCount;
        private ushort mRegion;
        private byte mArea;

        public int VertCount { get { return mVertCount; } }
        public int RawVertCount { get { return mRawVertCount; } }
        public ushort Region { get { return mRegion; } }
        public byte Area { get { return mArea; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mVerts == IntPtr.Zero); } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType 
        { 
            get { return AllocType.ExternallyManaged; }
        }

        internal Contour() { }

        internal void Reset()
        {
            mVertCount = 0;
            mRawVertCount = 0;
            mRegion = NMGen.NullRegion;
            mArea = NMGen.NullArea;
            mVerts = IntPtr.Zero;
            mRawVerts = IntPtr.Zero;
        }

        /// <summary>
        /// Has not effect on the object. (The object owner will handle
        /// disposal.)
        /// </summary>
        /// <remarks>
        /// <p>A <see cref="ContourSet"/> always ownes and manages objects
        /// of this type.</p>
        /// </remarks>
        public void RequestDisposal()
        {
            // Can't be disposed manually.  Owner will use reset.
        }

        public bool GetVerts(int[] buffer)
        {
            if (IsDisposed)
                return false;

            Marshal.Copy(mVerts, buffer, 0, mVertCount * 4);

            return true;
        }

        public bool GetRawVerts(int[] buffer)
        {
            if (IsDisposed)
                return false;

            Marshal.Copy(mRawVerts, buffer, 0, mVertCount * 4);

            return true;
        }
    }
}
