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
    public sealed class NavmeshQueryFilter
        : ManagedObject
    {
        /// <summary>
        /// The maximum number of areas that can be defined by the filter.
        /// </summary>
        public const int MaxAreas = NavmeshEx.MaxAreas;

        internal IntPtr root = IntPtr.Zero;
        private int mAreaCount;

        public NavmeshQueryFilter()
            : base(AllocType.Local)
        {
            root = NavmeshQueryFilterEx.Alloc();
            mAreaCount = MaxAreas;
        }

        internal NavmeshQueryFilter(IntPtr filter, AllocType type)
                : base(type)
        {
            root = filter;
            mAreaCount = MaxAreas;
        }

        public NavmeshQueryFilter(int areaCount)
            : base(AllocType.Local) 
        {
            root = NavmeshQueryFilterEx.Alloc();
            mAreaCount = Math.Min(MaxAreas, mAreaCount);
        }

        ~NavmeshQueryFilter()
        {
            RequestDisposal();
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
            get { return NavmeshQueryFilterEx.GetAreaCost(root, index); }
            set { NavmeshQueryFilterEx.SetAreaCost(root, index, value); }
        }

        /// <summary>
        /// Default: 0xffff
        /// </summary>
        public ushort IncludeFlags 
        { 
            get { return NavmeshQueryFilterEx.GetIncludeFlags(root); }
            set { NavmeshQueryFilterEx.SetIncludeFlags(root, value); }
        }

        /// <summary>
        /// Default: 0x0000
        /// </summary>
        public ushort ExcludeFlags
        {
            get { return NavmeshQueryFilterEx.GetExcludeFlags(root); }
            set { NavmeshQueryFilterEx.SetExcludeFlags(root, value); }
        }

        /// <summary>
        /// Indicates whether or not the memory allocated for the instance
        /// has been released.
        /// </summary>
        /// <returns>TRUE if memory has been released and the instance is
        /// no longer useable.  Otherwise FALSE.
        /// </returns>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        public override void RequestDisposal()
        {
            if (root != IntPtr.Zero 
                && resourceType == AllocType.Local)
            {
                NavmeshQueryFilterEx.Free(root);
                root = IntPtr.Zero;
            }
        }
    }
}
