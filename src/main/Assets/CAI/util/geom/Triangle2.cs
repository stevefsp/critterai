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

namespace org.critterai.geom
{
    /// <summary>
    /// Provides various 2D triangle utility methods.
    /// </summary>
    /// <remarks>
    /// <para>Static methods are thread safe.</para>
    /// </remarks>
    public static class Triangle2
    {
        /// <summary>
        /// Returns TRUE if the point (px, py) is contained by the triangle.
        /// (Inclusive)
        /// </summary>
        /// <remarks>
        /// <para>The test is inclusive.  So points on the vertices or edges
        /// of the triangle are considered to be contained by the triangle.</para>
        /// </remarks>
        /// <param name="px">The x-value for the point to test. (px, py)</param>
        /// <param name="py">The y-value for the poitn to test. (px, py)</param>
        /// <param name="ax">The x-value for vertex A in triangle ABC</param>
        /// <param name="ay">The y-value for vertex A in triangle ABC</param>
        /// <param name="bx">The x-value for vertex B in triangle ABC</param>
        /// <param name="by">The y-value for vertex B in triangle ABC</param>
        /// <param name="cx">The x-value for vertex C in triangle ABC</param>
        /// <param name="cy">The y-value for vertex C in triangle ABC</param>
        /// <returns>TRUE if the point (x, y) is contained by the triangle ABC.
        /// </returns>
        public static bool Contains(float px, float py
                , float ax, float ay
                , float bx, float by
                , float cx, float cy)
        {
            float dirABx = bx - ax;
            float dirABy = by - ay;
            float dirACx = cx - ax;
            float dirACy = cy - ay;
            float dirAPx = px - ax;
            float dirAPy = py - ay;

            float dotABAB = Vector2Util.Dot(dirABx, dirABy, dirABx, dirABy);
            float dotACAB = Vector2Util.Dot(dirACx, dirACy, dirABx, dirABy);
            float dotACAC = Vector2Util.Dot(dirACx, dirACy, dirACx, dirACy);
            float dotACAP = Vector2Util.Dot(dirACx, dirACy, dirAPx, dirAPy);
            float dotABAP = Vector2Util.Dot(dirABx, dirABy, dirAPx, dirAPy);

            float invDenom = 1 / (dotACAC * dotABAB - dotACAB * dotACAB);
            float u = (dotABAB * dotACAP - dotACAB * dotABAP) * invDenom;
            float v = (dotACAC * dotABAP - dotACAB * dotACAP) * invDenom;

            // Altered this slightly from the reference so that points on the 
            // vertices and edges are considered to be inside the triangle.
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        /// <summary>
        /// The absolute value of the returned value is two times the area of 
        /// the triangle ABC.
        /// </summary>
        /// <remarks>
        /// <para>A positive return value indicates:</para>
        /// <ul>
        /// <li>Counterclockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the right of line AC, looking from A toward C.
        /// </li>
        /// </ul>
        /// <para>A negative value indicates:</para>
        /// <ul>
        /// <li>Clockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the left of line AC, looking from A toward C.
        /// </li>
        /// </ul>
        /// <para>A value of zero indicates that all points are collinear or 
        /// represent the same point.</para>
        /// <para>This is a low cost method.</para>
        /// </remarks>
        /// <param name="ax">The x-value for vertex A in triangle ABC</param>
        /// <param name="ay">The y-value for vertex A in triangle ABC</param>
        /// <param name="bx">The x-value for vertex B in triangle ABC</param>
        /// <param name="by">The y-value for vertex B in triangle ABC</param>
        /// <param name="cx">The x-value for vertex C in triangle ABC</param>
        /// <param name="cy">The y-value for vertex C in triangle ABC</param>
        /// <returns>The absolute value of the returned value is two times the 
        /// area of the triangle ABC.</returns>
        public static float GetSignedAreaX2(float ax, float ay
            , float bx, float by
            , float cx, float cy)
        {
            // References:
            // http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm#Modern%20Triangles
            // http://mathworld.wolfram.com/TriangleArea.html 
            // (Search for "signed".)
            return (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
        }

        /// <summary>
        /// The absolute value of the returned value is two times the area of 
        /// the triangle ABC.
        /// </summary>
        /// <remarks>
        /// <para>A positive return value indicates:</para>
        /// <ul>
        /// <li>Counterclockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the right of line AC, looking from A toward C.
        /// </li>
        /// </ul>
        /// <para>A negative value indicates:</para>
        /// <ul>
        /// <li>Clockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the left of line AC, looking from A toward C.
        /// </li>
        /// </ul>
        /// <para>A value of zero indicates that all points are collinear or 
        /// represent the same point.</para>
        /// <para>This is a low cost method.</para>
        /// </remarks>
        /// <param name="ax">The x-value for vertex A in triangle ABC</param>
        /// <param name="ay">The y-value for vertex A in triangle ABC</param>
        /// <param name="bx">The x-value for vertex B in triangle ABC</param>
        /// <param name="by">The y-value for vertex B in triangle ABC</param>
        /// <param name="cx">The x-value for vertex C in triangle ABC</param>
        /// <param name="cy">The y-value for vertex C in triangle ABC</param>
        /// <returns>The absolute value of the returned value is two times the
        /// area of the triangle ABC.</returns>
        public static int GetSignedAreaX2(int ax, int ay
            , int bx, int by
            , int cx, int cy)
        {
            // References:
            // http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm#Modern%20Triangles
            // http://mathworld.wolfram.com/TriangleArea.html 
            // (Search for "signed".)
            return (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
        }
    }
}
