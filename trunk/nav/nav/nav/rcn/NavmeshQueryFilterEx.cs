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

namespace org.critterai.nav.rcn
{
    internal static class NavmeshQueryFilterEx
    {
        [DllImport("cai-nav-rcn", EntryPoint = "dtqfAlloc")]
        public static extern IntPtr Alloc();

        [DllImport("cai-nav-rcn", EntryPoint = "dtqfFree")]
        public static extern void Free(IntPtr filter);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqfSetAreaCost")]
        public static extern void SetAreaCost(IntPtr filter
            , int index
            , float cost);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqfGetAreaCost")]
        public static extern float GetAreaCost(IntPtr filter
            , int index);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqfSetIncludeFlags")]
        public static extern void SetIncludeFlags(IntPtr filter
            , ushort flags);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqfGetIncludeFlags")]
        public static extern ushort GetIncludeFlags(IntPtr filter);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqfSetExcludeFlags")]
        public static extern void SetExcludeFlags(IntPtr filter
            , ushort flags);

        [DllImport("cai-nav-rcn", EntryPoint = "dtqfGetExcludeFlags")]
        public static extern ushort GetExcludeFlags(IntPtr filter);
    }
}
