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
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <para>Implemented as a class with public fields in order to support Unity
    /// serialization.  Care must be taken not to set the fields to invalid
    /// values.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NMGenTileParams
    {
        public int x;
        public int z;

        /// <summary>
        /// Minimum bounds.
        /// </summary>
        public Vector3 boundsMin;

        /// <summary>
        /// Maximum bounds.
        /// </summary>
        public Vector3 boundsMax;

        public int Z
        {
            get { return z; }
            set { z = Math.Max(0, value); }
        }

        public int X
        {
            get { return x; }
            set { x = Math.Max(0, value); }
        }

        /// <summary>
        /// Gets a copy of the minimum bounds of the grid's AABB.
        /// [Form: (x, y, z)] [Units: World]
        /// </summary>
        /// <remarks><para>This value is usually derived during the build process.
        ///  See <see cref="DeriveBounds"/>.</para></remarks>
        /// <returns>The maximum bounds of the grid.</returns>
        public Vector3 BoundsMin 
        { 
            get { return boundsMin; }
            set { boundsMin = value; }
        }

        /// <summary>
        /// Gets a copy of the maximum bounds of the grid's AABB.
        /// [Form: (x, y, z)] [Units: World]
        /// </summary>
        /// <remarks><para>This value is usually derived during the build 
        /// process. See <see cref="DeriveBounds"/>.
        /// </para></remarks>
        /// <returns>The maximum bounds of the grid.</returns>
        public Vector3 BoundsMax
        {
            get { return boundsMax; }
            set { boundsMax = value; }
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public NMGenTileParams(int tx, int tz, Vector3 boundsMin, Vector3 boundsMax) 
        {
            X = tx;
            Z = tz;
            BoundsMin = boundsMin;
            BoundsMax = boundsMax;
        }

        //public bool IsValid()
        //{
        //    return !(x < 0
        //        || z < 0
        //        || Vector3Util.IsBoundsValid(boundsMin, boundsMax));
        //}

        ///// <summary>
        ///// Forces all field values to within the mandatory limits.
        ///// </summary>
        //public void Clean()
        //{
        //    X = x;
        //    Z = z;
        //}
    }
}
