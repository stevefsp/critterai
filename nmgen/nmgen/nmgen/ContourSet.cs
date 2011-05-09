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
    public sealed class ContourSet
        : IManagedObject
    {

        internal ContourSetEx root;
        private Contour[] mContours = null;

        public int Width { get { return root.width; } }
        public int Depth { get { return root.depth; } }
        public float XZCellSize { get { return root.xzCellSize; } }
        public float YCellSize { get { return root.yCellSize; } }
        public int BorderSize { get { return root.borderSize; } }
        public int Count { get { return root.contourCount; } }

        public float[] GetBoundsMin()
        {
            return (float[])root.boundsMin.Clone();
        }

        public float[] GetBoundsMax()
        {
            return (float[])root.boundsMax.Clone();
        }

        public bool IsDisposed 
        {
            get { return (root.contours == IntPtr.Zero); } 
        }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        private ContourSet(ContourSetEx root)
        {
            this.root = root;
        }

        ~ContourSet()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Frees and marks as disposed all resources. (Including
        /// <see cref="Contour"/> objects.)
        /// </summary>
        public void RequestDisposal()
        {
            if (IsDisposed)
                return;

            ContourSetEx.FreeDataEx(root);

            if (mContours == null)
                return;

            for (int i = 0; i < mContours.Length; i++)
            {
                if (mContours[i] != null)
                    mContours[i].Reset();
            }
        }

        public Contour GetContour(int index)
        {
            if (IsDisposed || index < 0 || index >= root.contourCount)
                return null;

            if (mContours == null)
                mContours = new Contour[root.contourCount];

            if (mContours[index] != null)
                return mContours[index];

            Contour result = new Contour();

            ContourSetEx.GetContour(root, index, result);
            mContours[index] = result;

            return result;
        }

        public static ContourSet Build(BuildContext context
            , CompactHeightfield field
            , float edgeMaxDeviation
            , int maxEdgeLength
            , ContourBuildFlags flags)
        {
            ContourSetEx root = new ContourSetEx();

            if (!ContourSetEx.Build(context.root
                , field
                , edgeMaxDeviation
                , maxEdgeLength
                , root
                , flags))
            {
                return null;
            }

            ContourSet result = new ContourSet(root);

            return result;
        }

        
    }
}
