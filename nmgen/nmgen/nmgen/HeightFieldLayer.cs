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
    public sealed class HeightFieldLayer
        : IManagedObject
    {

        // Field layout: rcHeightfieldLayer

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private float[] mBoundsMin = new float[3];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private float[] mBoundsMax = new float[3];

        private float mXZCellSize;
        private float mYCellSize;

        private int mWidth;
        private int mDepth;
        private int mXMin;   // Bounding box of usable data.
        private int mXMax;
        private int mYMin;
        private int mYMax;
        private int mHeightMin;
        private int mHeightMax;
	
        // Height min/max
	    private IntPtr mHeights;			// byte[depth*width]
        private IntPtr mAreas;				// byte[depth*width]
        private IntPtr mCons;	            // byte[depth*width]

        public int Width { get { return mWidth; } }
        public int Depth { get { return mDepth; } }
        public float[] GetBoundsMin() { return (float[])mBoundsMin.Clone(); }
        public float[] GetBoundsMax() { return (float[])mBoundsMax.Clone(); }
        public float XZCellSize { get { return mXZCellSize; } }
        public float YCellSize { get { return mYCellSize; } }

        public int HeightMin { get { return mHeightMin; } }
        public int HeightMax { get { return mHeightMax; } }

        public int XMin { get { return mXMin; } }
        public int XMax { get { return mXMax; } }
        public int ZMin { get { return mYMin; } }
        public int ZMax { get { return mYMax; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mHeights == IntPtr.Zero); } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType
        {
            get { return AllocType.ExternallyManaged; }
        }

        internal HeightFieldLayer() { }

        internal void Reset()
        {
            Array.Clear(mBoundsMin, 0, 3);
            Array.Clear(mBoundsMax, 0, 3);

            mXZCellSize = 0;
            mYCellSize = 0;
            mWidth = 0;
            mDepth = 0;
            mXMin = 0;
            mXMax = 0;
            mYMin = 0;
            mYMax = 0;
            mHeightMin = 0;
            mHeightMax = 0;

            mHeights = IntPtr.Zero;
            mAreas = IntPtr.Zero;
            mCons = IntPtr.Zero;
        }

        /// <summary>
        /// Has not effect on the object. (The object owner will handle
        /// disposal.)
        /// </summary>
        /// <remarks>
        /// <p>A <see cref="HeightfieldLayerSet"/> always owns and manages 
        /// objects of this type.</p>
        /// </remarks>
        public void RequestDisposal()
        {
            // Always externally managed.  So don't do anything.
        }

        public bool GetHeightData(byte[] buffer)
        {
            if (IsDisposed || buffer.Length < mWidth * mDepth)
                return false;

            Marshal.Copy(mHeights, buffer, 0, mWidth * mDepth);

            return true;
        }

        public bool GetAreaData(byte[] buffer)
        {
            if (IsDisposed || buffer.Length < mWidth * mDepth)
                return false;

            Marshal.Copy(mAreas, buffer, 0, mWidth * mDepth);

            return true;
        }

        public bool GetConnectionData(byte[] buffer)
        {
            if (IsDisposed || buffer.Length < mWidth * mDepth)
                return false;

            Marshal.Copy(mCons, buffer, 0, mWidth * mDepth);

            return true;
        }
    }
}
