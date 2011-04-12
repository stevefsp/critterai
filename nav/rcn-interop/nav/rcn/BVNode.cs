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
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Bounding volumn node.
    /// </summary>
    /// <remarks>
    /// <p>Must be initialized before use.</p>
    /// <p>This data is provided for debug purposes.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct BVNode
    {
        /// <summary>
        /// Minimimum bounds of the node's AABB in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort[] boundsMin;

        /// <summary>
        /// Maximum bounds of the node's AABB in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort[] boundsMax;		                    

        /// <summary>
        /// The node's index.  (Negative for escape indices.)
        /// </summary>
	    public int i;									

        /// <summary>
        /// Initializes the structure before its first use.
        /// </summary>
        /// <remarks>
        /// Existing references are released and replaced.
        /// </remarks>
        public void Initialize()
        {
            boundsMin = new ushort[3];
            boundsMax = new ushort[3];
            i = 0;
        }

        /// <summary>
        /// Rerturns an array of fully initialized nodes.
        /// </summary>
        /// <param name="length">The length of the array. (>0)</param>
        /// <returns>An array of fully initialized structures.</returns>
        public static BVNode[] GetInitializedArray(int length)
        {
            BVNode[] result = new BVNode[length];
            for (int i = 0; i < length; i++)
                result[i].Initialize();
            return result;
        }
    }
}
