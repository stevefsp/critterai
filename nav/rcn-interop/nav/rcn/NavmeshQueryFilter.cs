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
    /// Defines area traversal cost and restrictions for navigation querys.
    /// </summary>
    /// <remarks>
    /// <p>WARNING: Behavior is undefined if an area
    /// index is out of range.  The error may result in a runtime error, or
    /// it may operate as if there is no problem whatsoever.  E.g. Setting
    /// and getting myFilter[myFilter.AreaCount] may get and set the value
    /// value normally.  Do not write code that depends on this behavior since 
    /// it may change in future releases.</p>
    /// <p>Behavior is undefined if an object is used after 
    /// disposal.</p>
    /// </remarks>
    public sealed class NavmeshQueryFilter
        : ManagedObject
    {
        /// <summary>
        /// The maximum number of areas that can be defined by the filter.
        /// </summary>
        public const int MaxAreas = NavmeshEx.MaxAreas;

        /// <summary>
        /// A pointer to the unmanaged dtQueryFilter object.
        /// </summary>
        internal IntPtr root = IntPtr.Zero;
        private int mAreaCount;

        /// <summary>
        /// Constructor for a filter with the maximum number of areas.
        /// </summary>
        public NavmeshQueryFilter()
            : base(AllocType.Local)
        {
            root = NavmeshQueryFilterEx.Alloc();
            mAreaCount = MaxAreas;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filter">A pointer to an existing unmanaged 
        /// dtQueryFilter object.
        /// </param>
        /// <param name="type">The allocation type of the dtQueryFilter
        /// object.</param>
        internal NavmeshQueryFilter(IntPtr filter, AllocType type)
                : base(type)
        {
            root = filter;
            mAreaCount = MaxAreas;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="areaCount">The number of used areas.</param>
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
        /// The number of areas defined by the filter.
        /// </summary>
        public int AreaCount 
        { 
            get { return mAreaCount; }
            set { mAreaCount = Math.Min(MaxAreas, value); }
        }

        /// <summary>
        /// The traversal cost for each area, indexed by area id.
        /// (Default: 1.0)
        /// </summary>
        /// <param name="index">The area id.</param>
        /// <returns>The traversal cost for the area.</returns>
        public float this[int index]
        {
            get { return NavmeshQueryFilterEx.GetAreaCost(root, index); }
            set { NavmeshQueryFilterEx.SetAreaCost(root, index, value); }
        }

        /// <summary>
        /// The flags for polygons that should be included in the query.
        /// (Default: 0xffff)
        /// </summary>
        /// <remarks>
        /// <p>A navigation mesh polygon must have at least one of these flags
        /// set in order to be considered accessible during a search.</p>
        /// <p>If a navigation mesh polygon does not have at least one flag
        /// set, it will never be considered accessable.</p>
        /// </remarks>
        public ushort IncludeFlags 
        { 
            get { return NavmeshQueryFilterEx.GetIncludeFlags(root); }
            set { NavmeshQueryFilterEx.SetIncludeFlags(root, value); }
        }

        /// <summary>
        /// The flags for polygons that should be excluded from the query.
        /// (Default: 0x0000)
        /// </summary>
        /// <remarks>If a polygon has any of these flags set it will be
        /// considered inaccessable.</remarks>
        public ushort ExcludeFlags
        {
            get { return NavmeshQueryFilterEx.GetExcludeFlags(root); }
            set { NavmeshQueryFilterEx.SetExcludeFlags(root, value); }
        }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Request all unmanaged resources controlled by the object be 
        /// immediately freed and the object marked as disposed.
        /// </summary>
        /// <remarks>
        /// If the object was created using a public constructor the
        /// resources will be freed immediately.
        /// </remarks>
        public override void RequestDisposal()
        {
            if (root != IntPtr.Zero 
                && ResourceType == AllocType.Local)
            {
                NavmeshQueryFilterEx.Free(root);
                root = IntPtr.Zero;
            }
        }
    }
}
