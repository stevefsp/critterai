/*
 * Copyright (c) 2010 Stephen A. Pratt
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

namespace org.critterai.math.geom
{
    /// <summary>
    /// Provides circle utility methods.
    /// </summary>
    /// <remarks>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public static class Circle
    {
        /*
         * Design note:
         * 
         * No "This class is optimized" warning is needed for this class
         * since it can't throw expections.
         * 
         */

        /// <summary>
        /// Determines whether or not two circles intersect eachother.
        /// </summary>
        /// <remarks>
        /// <p>Test is inclusive of the circle boundary.</p>
        /// </p>
        /// <p>Containment of one circle by another is considered intersection.
        /// </p>
        /// </remarks>
        /// <param name="ax">
        /// The x-value of circle A's center point. (ax, ay)
        /// </param>
        /// <param name="ay">
        /// The y-value of circle A's center point. (ax, ay)
        /// </param>
        /// <param name="ar">
        /// The radius of circle A.
        /// </param>
        /// <param name="bx">
        /// The x-value of circle B's center point. (bx, by)
        /// </param>
        /// <param name="by">
        /// The y-value of circle B's center point. (bx, by)
        /// </param>
        /// <param name="br">
        /// The radius of Circle B.
        /// </param>
        /// <returns>TRUE if the circles intersect.  Otherwise FALSE.
        /// </returns>
        public static bool Intersects(float ax, float ay, float ar
            , float bx, float by, float br)
        {
            return (Vector2Util.GetDistanceSq(ax, ay, bx, by) 
                <= (ar + br) * (ar + br));
        }

        /// <summary>
        /// Determines whether or not a point is contained within a circle.
        /// </summary>
        /// <remarks>
        /// <p>The test is inclusive of the circle boundary</p></remarks>
        /// <param name="px">The x-value of point (px, py).</param>
        /// <param name="py">The y-value of point (px, py).</param>
        /// <param name="cx">The x-value of the circle's center point (cx, cy).
        /// </param>
        /// <param name="cy">The y-value of the circle's center point (cx, cy).
        /// </param>
        /// <param name="cr">The radius of the circle.</param>
        /// <returns></returns>
        public static bool Contains(float px, float py
            , float cx, float cy
            , float cr)
        {
            return (Vector2Util.GetDistanceSq(px, py, cx, cy) <= cr * cr);
        }
    }
}
