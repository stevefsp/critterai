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
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Defines area traversal cost and restrictions on the navigation query.
    /// </summary>
    /// <remarks>
    /// <p>Currently, behavior is undefined if an area
    /// index is out of range.  The error may result in a runtime error, or
    /// it may operate as if there is no problem whatsoever.  E.g. Setting
    /// and getting myFilter[myFilter.AreaCount + 1] may get and set the value
    /// value normally.  Do not write code that depends on this behavior since 
    /// it may change in future releases.</p>
    /// </remarks>
    public sealed class DTQueryFilter
        : IDisposable
    {
        /// <summary>
        /// The maximum number of areas that can be defined by the filter.
        /// </summary>
        public const int MaxAreas = DTNavmeshEx.MaxAreas;

        internal IntPtr root = IntPtr.Zero;
        private int mAreaCount;

        public DTQueryFilter()
        {
            root = DTQueryFilterEx.Alloc();
            mAreaCount = MaxAreas;
        }

        public DTQueryFilter(int areaCount)
        {
            root = DTQueryFilterEx.Alloc();
            mAreaCount = Math.Min(MaxAreas, mAreaCount);
        }

        ~DTQueryFilter()
        {
            Dispose();
        }

        /// <summary>
        /// The number of areas being used by the filter.
        /// </summary>
        public int AreaCount 
        { 
            get { return mAreaCount; }
            set { mAreaCount = Math.Min(MaxAreas, value); }
        }

        public float this[int index]
        {
            get { return DTQueryFilterEx.GetAreaCost(root, index); }
            set { DTQueryFilterEx.SetAreaCost(root, index, value); }
        }

        /// <summary>
        /// Default: 0xffff
        /// </summary>
        public ushort IncludeFlags 
        { 
            get { return DTQueryFilterEx.GetIncludeFlags(root); }
            set { DTQueryFilterEx.SetIncludeFlags(root, value); }
        }

        /// <summary>
        /// Default: 0x0000
        /// </summary>
        public ushort ExcludeFlags
        {
            get { return DTQueryFilterEx.GetExcludeFlags(root); }
            set { DTQueryFilterEx.SetExcludeFlags(root, value); }
        }

        /// <summary>
        /// Indicates whether or not the memory allocated for the instance
        /// has been released.
        /// </summary>
        /// <returns>TRUE if memory has been released and the instance is
        /// no longer useable.  Otherwise FALSE.
        /// </returns>
        public bool IsDisposed()
        {
            return (root == IntPtr.Zero);
        }

        /// <summary>
        /// Manually and immediately de-allocates the memory for the instance.  
        /// </summary>
        /// <remarks>
        /// Attempting to use the instance after disposal will result in
        /// undefined behavior.
        /// <p>Calling this method is optional.</p>
        /// </remarks>
        public void Dispose()
        {
            if (root != IntPtr.Zero)
            {
                DTQueryFilterEx.Free(root);
                root = IntPtr.Zero;
            }
        }
    }
}
