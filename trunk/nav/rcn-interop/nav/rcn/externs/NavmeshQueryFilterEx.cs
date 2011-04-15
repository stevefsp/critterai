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

namespace org.critterai.nav.rcn.externs
{
    /// <summary>
    /// Provides interop method signatures related to the dtQueryFilter class.
    /// </summary>
    public static class NavmeshQueryFilterEx
    {
        // Source header: DetourNavmeshQueryEx.h

        /// <summary>
        /// Allocates and returns a pointer to a dtQueryFilter object.
        /// </summary>
        /// <remarks>
        /// Any objects allocated with this method must be freed using the
        /// <see cref="Free"/> method before the last reference is released.
        /// Otherwise a memory leak will occur.
        /// </remarks>
        /// <returns>A pointer to a dtQueryFilter object.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqfAlloc")]
        public static extern IntPtr Alloc();

        /// <summary>
        /// Frees the unmanaged resources of a dtQueryFilter object created
        /// using the <see cref="Alloc"/> method.
        /// </summary>
        /// <param name="filter">A pointer to a dqQueryFilter object.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqfFree")]
        public static extern void Free(IntPtr filter);

        /// <summary>
        /// Sets the traversal cost for the area id.
        /// </summary>
        /// <param name="filter">A pointer to a dqQueryFilter object.</param>
        /// <param name="index">The index of the area. 
        /// </param>
        /// <param name="cost">The cost of traversing the area. (>= 0)</param>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqfSetAreaCost")]
        public static extern void SetAreaCost(IntPtr filter
            , int index
            , float cost);

        /// <summary>
        /// Gets the traversal cost for an area id
        /// </summary>
        /// <param name="filter">A pointer to a dqQueryFilter object.</param>
        /// <param name="index">The inex of the area. 
        /// </param>
        /// <returns>The traversal cost.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqfGetAreaCost")]
        public static extern float GetAreaCost(IntPtr filter
            , int index);

        /// <summary>
        /// Sets the include flags for the filter.
        /// </summary>
        /// <param name="filter">A pointer to a dqQueryFilter object.</param>
        /// <param name="flags">The new flags.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqfSetIncludeFlags")]
        public static extern void SetIncludeFlags(IntPtr filter
            , ushort flags);

        /// <summary>
        /// Gets the include flags for the filter.
        /// </summary>
        /// <param name="filter">A pointer to a dqQueryFilter object.</param>
        /// <returns>The include flags.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqfGetIncludeFlags")]
        public static extern ushort GetIncludeFlags(IntPtr filter);

        /// <summary>
        /// Sets the exclude flags for the filter.
        /// </summary>
        /// <param name="filter">A pointer to a dqQueryFilter object.</param>
        /// <param name="flags">The exclude flags.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqfSetExcludeFlags")]
        public static extern void SetExcludeFlags(IntPtr filter
            , ushort flags);

        /// <summary>
        /// Gets the include flags for the filter.
        /// </summary>
        /// <param name="filter">A pointer to a dqQueryFilter object.</param>
        /// <returns>The exclude flags for the filter.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "dtqfGetExcludeFlags")]
        public static extern ushort GetExcludeFlags(IntPtr filter);
    }
}
