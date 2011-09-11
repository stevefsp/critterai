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
using UnityEngine;

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// A point within a navigation mesh.
    /// </summary>
    public struct NavmeshPoint
    {
        /// <summary>
        /// The reference id of the polygon the point belongs to. (Or zero
        /// if not known.)
        /// </summary>
        public uint polyRef;

        /// <summary>
        /// The location of the point.
        /// </summary>
        public Vector3 point;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="polyRef">The reference id of the polygon the point
        /// belongs to. (Or zero if not known.)</param>
        /// <param name="point">The location of the point.</param>
        public NavmeshPoint(uint polyRef, Vector3 point)
        {
            this.polyRef = polyRef;
            this.point = point;
        }

        /// <summary>
        /// Returns a standard string representation of point.
        /// </summary>
        /// <returns>A standard string representation of point.</returns>
        public override string ToString()
        {
            return string.Format("[{0:F3}, {1:F3}, {2:F3}] (Ref: {3})"
                , point.x, point.y, point.z, polyRef);
        }

    }
}
