/*
 * Copyright (c) 2010-2012 Stephen A. Pratt
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

namespace org.critterai.geom
{
    /// <summary>
    /// Provides circle utility methods.
    /// </summary>
    /// <remarks>
    /// <para>Static methods are thread safe.</para>
    /// </remarks>
    public static class Circle
    {
        /// <summary>
        /// Determines whether or not two circles intersect each other.
        /// </summary>
        /// <remarks>
        /// <para>Test is inclusive of the circle boundary.</para>
        /// <para>Containment of one circle by another is considered intersection.
        /// </para>
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
        /// <returns>TRUE if the circles intersect.
        /// </returns>
        public static bool Intersects(Vector2 a, float aradius, Vector2 b, float bradius)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            return (dx * dx + dy * dy) <= (aradius + bradius) * (aradius + bradius);
        }

        /// <summary>
        /// Determines whether or not a point is contained within a circle.
        /// </summary>
        /// <remarks>
        /// <para>The test is inclusive of the circle boundary</para></remarks>
        /// <param name="px">The x-value of point (px, py).</param>
        /// <param name="py">The y-value of point (px, py).</param>
        /// <param name="cx">The x-value of the circle's center point (cx, cy).
        /// </param>
        /// <param name="cy">The y-value of the circle's center point (cx, cy).
        /// </param>
        /// <param name="cr">The radius of the circle.</param>
        /// <returns>TRUE if the point is contained within the circle.
        /// </returns>
        public static bool Contains(Vector2 point, Vector2 circle, float radius)
        {
            float dx = point.x - circle.x;
            float dy = point.y - circle.y;
            return (dx * dx + dy * dy) <= radius * radius;
        }
    }
}
