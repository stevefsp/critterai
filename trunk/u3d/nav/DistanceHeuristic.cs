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
using System;

namespace org.critterai.nav
{
    /// <summary>
    /// Provides various distance mHeuristic operations used
    /// for A* searches.
    /// <para>Static operations are thread-safe.</para>
    /// </summary>
    public static class DistanceHeuristic 
    {

        /// <summary>
        /// Returns the distance estimate from point A and point B using the 
        /// requested heuristic.
        /// </summary>
        /// <param name="type">The heuristic to use.</param>
        /// <param name="ax">The x-value of point (ax, ay, az).</param>
        /// <param name="ay">The y-value of point (ax, ay, az).</param>
        /// <param name="az">The z-value of point (ax, ay, az).</param>
        /// <param name="bx">The x-value of point (bx, by, bz).</param>
        /// <param name="by">The y-value of point (bx, by, bz).</param>
        /// <param name="bz">The z-value of point (bx, by, bz).</param>
        /// <returns>The estimated distance from point A to point B.</returns>
        public static float GetHeuristicValue(DistanceHeuristicType type
                , float ax, float ay, float az
                , float bx, float by, float bz)
        {
            switch (type)
            {
            case DistanceHeuristicType.LongestAxis:
                return GetLongestAxis(ax, ay, az, bx, by, bz);
            case DistanceHeuristicType.Manhattan:
                return GetManhattan(ax, ay, az, bx, by, bz);
            }
            return 0;
        }
        
        /// <summary>
        /// Returns the longest axis distance from point A to point B.
        /// I.e. The longest of (ax - bx), (ay - by), or (az - bz).
        /// </summary>
        /// <param name="ax">The x-value of point (ax, ay, az).</param>
        /// <param name="ay">The y-value of point (ax, ay, az).</param>
        /// <param name="az">The z-value of point (ax, ay, az).</param>
        /// <param name="bx">The x-value of point (bx, by, bz).</param>
        /// <param name="by">The y-value of point (bx, by, bz).</param>
        /// <param name="bz">The z-value of point (bx, by, bz).</param>
        /// <returns>The longest axis-distance from point A to point B.</returns>
        public static float GetLongestAxis(float ax, float ay, float az, float bx, float by, float bz)
        {
            return Math.Max(Math.Max(Math.Abs(ax - bx), Math.Abs(ay - by)), Math.Abs(az - bz));
        }
        
        /// <summary>
        /// Determines the Manhattan distance from point A to point B.
        /// <para>See: <a href="http://en.wikipedia.org/wiki/Manhattan_distance" target="_blank">
        /// Manhattan Distance (Wikipedia)</a></para>
        /// </summary>
        /// <param name="ax">The x-value of point (ax, ay, az).</param>
        /// <param name="ay">The y-value of point (ax, ay, az).</param>
        /// <param name="az">The z-value of point (ax, ay, az).</param>
        /// <param name="bx">The x-value of point (bx, by, bz).</param>
        /// <param name="by">The y-value of point (bx, by, bz).</param>
        /// <param name="bz">The z-value of point (bx, by, bz).</param>
        /// <returns>The Manhattan distance from point A to point B.</returns>
        public static float GetManhattan(float ax, float ay, float az, float bx, float by, float bz)
        {
            return (Math.Abs(ax - bx) + Math.Abs(ay - by)) +  Math.Abs(az - bz);
        }
    }
}
