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
using org.critterai.geom;
using org.critterai.interop;
using org.critterai.nmgen.rcn;

namespace org.critterai.nmgen
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class Heightfield
        : IManagedObject
    {
        /*
         * Design notes:
         * 
         * I ran into complications with implementing this class with a data
         * layout matching the native structure. The cause appeared to be the 
         * pointer to a pointer field in the native structure. So I switched
         * the the root pattern with some duplication of data on this size
         * of the boundary for performance reasons.
         * 
         * The AddSpan method is not supported because of a bug in Recast.
         * http://code.google.com/p/recastnavigation/issues/detail?id=167
         * 
         * 
         */

        private int mWidth = 0;
        private int mDepth = 0;

        private float[] mBoundsMin = new float[3];
        private float[] mBoundsMax = new float[3];

        private float mXZCellSize = 0;
        private float mYCellSize = 0;

        internal IntPtr root;

        public int Width { get { return mWidth; } }
        public int Depth { get { return mDepth; } }
        public float[] GetBoundsMin() { return (float[])mBoundsMin.Clone(); }
        public float[] GetBoundsMax() { return (float[])mBoundsMax.Clone(); }
        public float XZCellSize { get { return mXZCellSize; } }
        public float YCellSize { get { return mYCellSize; } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return root == IntPtr.Zero; } }

        public Heightfield(int width
            , int depth
            , float[] boundsMin
            , float[] boundsMax
            , float xzCellSize
            , float yCellSize)
        {
            root = HeightfieldEx.Alloc(width
                , depth
                , boundsMin
                , boundsMax
                , xzCellSize
                , yCellSize);

            if (root == IntPtr.Zero)
                return;

            mWidth = width;
            mDepth = depth;
            mBoundsMin = (float[])boundsMin.Clone();
            mBoundsMax = (float[])boundsMax.Clone();
            mYCellSize = yCellSize;
            mXZCellSize = xzCellSize;
        }

        ~Heightfield()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Frees all resources and marks object as disposed.
        /// </summary>
        public void RequestDisposal() 
        {
            if (!IsDisposed)
            {
                HeightfieldEx.FreeEx(root);
                root = IntPtr.Zero;
                mWidth = 0;
                mDepth = 0;
                mXZCellSize = 0;
                mYCellSize = 0;
                Array.Clear(mBoundsMin, 0, 3);
                Array.Clear(mBoundsMax, 0, 3);
            }
        }

        public int GetSpanCount()
        {
            if (IsDisposed)
                return 0;
            return HeightfieldEx.GetSpanCount(root);
        }

        /// <summary>
        /// Gets an buffer that is sized to fit the maximum
        /// number of spans in a column for the field.
        /// </summary>
        /// <returns></returns>
        public HeightfieldSpan[] GetSpanBuffer()
        {
            if (IsDisposed)
                return null;

            int size = HeightfieldEx.GetMaxSpansInColumn(root);
            return new HeightfieldSpan[size];
        }

        public int GetSpans(int widthIndex
            , int depthIndex
            , HeightfieldSpan[] buffer)
        {
            if (IsDisposed)
                return -1;

            return HeightfieldEx.GetSpans(root
                , widthIndex
                , depthIndex
                , buffer
                , buffer.Length);
        }

        public bool FlagLowObstaclesWalkable(BuildContext context
            , int walkableStep)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.FlagLowObstaclesWalkable(context.root
                , walkableStep
                , root);
        }

        public bool FlagLedgeSpansNotWalkable(BuildContext context
            , int walkableHeight
            , int walkableStep)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.FlagLedgeSpansNotWalkable(context.root
                , walkableHeight
                , walkableStep
                , root);
        }

        public bool FlagLowHeightSpansNotWalkable(BuildContext context
            , int walkableHeight)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.FlagLowHeightSpansNotWalkable(context.root
                , walkableHeight
                , root);
        }

        public bool AddTriangle(BuildContext context
            , float[] verts
            , byte area
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.AddTriangle(context.root
                , verts
                , area
                , root
                , flagMergeThreshold);
        }

        public bool AddTriangles(BuildContext context
            , TriangleMesh mesh
            , byte[] areas
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.AddTriangles(context.root
                , mesh.verts
                , mesh.vertCount
                , mesh.tris
                , areas
                , mesh.triCount
                , root
                , flagMergeThreshold);
        }

        public bool AddTriangles(BuildContext context
            , float[] verts
            , ushort[] tris
            , byte[] areas
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.AddTriangles(context.root
                , verts
                , verts.Length / 3
                , tris
                , areas
                , tris.Length / 3
                , root
                , flagMergeThreshold);
        }

        public bool AddTriangles(BuildContext context
            , float[] verts
            , byte[] areas
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.AddTriangles(context.root
                , verts
                , areas
                , verts.Length / 9
                , root
                , flagMergeThreshold);
        }
    }
}
