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
using UnityEngine;

namespace org.critterai.math
{
    /// <summary>
    /// Contains various static operations applicable to 2D vectors which are not provided
    /// by the Unity3D vector class.
    /// </summary>
    /// <remarks>
    /// <para>This class is optimized for speed.  To support this priority, no argument validation is
    /// performed.  E.g. No null checks, no divide by zero checks, etc.</para>
    /// <para>Static operations are thread safe.</para>
    /// </remarks>
    public static class Vector2Util
    {

        private const float EPSILON_STD = MathUtil.EPSILON_STD;

        /// <summary>
        /// Returns the dot product of the provided vectors.
        /// See: <a href="http://en.wikipedia.org/wiki/Dot_product" target="_blank">
        /// Wikipedia- Dot Product</a>
        /// </summary>
        /// <remarks></remarks>
        /// <param name="ux">The x-value of the vector (ux, uy).</param>
        /// <param name="uy">The y-value of the vector (ux, uy).</param>
        /// <param name="vx">The x-value of the vector (vx, vy).</param>
        /// <param name="vy">The y-value of the vector (vx, vy).</param>
        /// <returns>The dot product of the provided vectors.</returns>
        public static float Dot(float ux, float uy, float vx, float vy)
        {
            return (ux * vx) + (uy * vy);
        }

        /// <summary>
        /// Derives the normalized direction vector for the vector pointing from point A (ax, ay) to
        /// point B (bx, by).
        /// <para>WARNING: This is a costly operation.</para>
        /// </summary>
        /// <param name="ax">The x-value for the starting point A (ax, ay).</param>
        /// <param name="ay">The y-value for the starting point A (ax, ay).</param>
        /// <param name="bx">The x-value for the end point B (bx, by).</param>
        /// <param name="by">The y-value for the end point B (bx, by).</param>
        /// <returns>The normalized direction vector for the vector pointing from point A to B.</returns>
        public static Vector2 GetDirectionAB(float ax, float ay
                , float bx, float by)
        {
            // Subtract.
            Vector2 result = new Vector2(bx - ax, by - ay);
            
            // Normalize.
            float length = (float)Math.Sqrt((result.x * result.x) + (result.y * result.y));
            if (length <= EPSILON_STD) 
                length = 1;
            
            result.x /= length;
            result.y /= length;
            
            if (Math.Abs(result.x) < EPSILON_STD) 
                result.x = 0;
            if (Math.Abs(result.y) < EPSILON_STD) 
                result.y = 0;    
            
            return result;
        }

        /// <summary>
        /// Determines whether or not the elements of the provided vectors are equal within
        /// the specified tolerance.  Each element of the vector is tested separately.
        /// </summary>
        /// <param name="ux">The x-value of the vector (ux, uy).</param>
        /// <param name="uy">The y-value of the vector (ux, uy).</param>
        /// <param name="vx">The x-value of the vector (vx, vy).</param>
        /// <param name="vy">The y-value of the vector (vx, vy).</param>
        /// <param name="tolerance">The tolerance for the test.</param>
        /// <returns>TRUE if the the associated elements of each vector are within the specified tolerance
        /// of each other.  Otherwise FALSE.</returns>
        public static Boolean SloppyEquals(float ux, float uy, float vx, float vy, float tolerance)
        {
            tolerance = Math.Max(0, tolerance);
            return !(Math.Abs(ux - vx) > tolerance || Math.Abs(uy - vy) > tolerance);
        }

        /// <summary>
        /// Determines whether or not the elements of the provided vectors are equal within
        /// the specified tolerance.  Each element of the vector is tested separately.
        /// </summary>
        /// <param name="u">Vector v</param>
        /// <param name="v">Vector u</param>
        /// <param name="tolerance">The tolerance for the test. </param>
        /// <returns>TRUE if the the associated elements of each vector are within the specified tolerance
        /// of each other.  Otherwise FALSE.</returns>
        public static Boolean SloppyEquals(Vector2 u, Vector2 v, float tolerance)
        {
            return SloppyEquals(u.x, u.y, v.x, v.y, tolerance);
        }

        /// <summary>
        /// Determines whether or not the elements of the provided vectors are equal within
        /// the specified tolerance.  Each element of the vector is tested separately.
        /// </summary>
        /// <param name="u">Vector v</param>
        /// <param name="vx">The x-value of the vector (vx, vy).</param>
        /// <param name="vy">The y-value of the vector (vx, vy).</param>
        /// <param name="tolerance">The tolerance for the test.</param>
        /// <returns>TRUE if the the associated elements of each vector are within the specified tolerance
        /// of each other.  Otherwise FALSE.</returns>
        public static Boolean SloppyEquals(Vector2 u, float vx, float vy, float tolerance)
        {
            return SloppyEquals(u.x, u.y, vx, vy, tolerance);
        }

        /// <summary>
        /// Returns the square of the distance between the two provided points. (distance * distance)
        /// </summary>
        /// <param name="ax">The x-value of the point (ax, ay).</param>
        /// <param name="ay">The y-value of the point (ax, ay).</param>
        /// <param name="bx">The x-value of the point (bx, by).</param>
        /// <param name="by">The y-value of the point (bx, by).</param>
        /// <returns>The square of the distance between the two provided points.</returns>
        public static float GetDistanceSq(float ax, float ay, float bx, float by)
        {
            float dx = ax - bx;
            float dy = ay - by;
            return (dx * dx + dy * dy);
        }

        /// <summary>
        /// Normalizes the provided vector such that its length is equal to one.
        /// <para>WARNING: This is a costly operation.</para>
        /// </summary>
        /// <param name="x">The x-value of the vector (x, y).</param>
        /// <param name="y">The y-value of the vector (x, y).</param>
        /// <returns>The normalized vector.</returns>
        public static Vector2 Normalize(float x, float y) 
        {
            float length = (float)Math.Sqrt(x * x + y * y);
            if (length <= EPSILON_STD) 
                length = 1;
            
            x /= length;
            y /= length;
            
            if (Math.Abs(x) < EPSILON_STD) 
                x = 0;
            if (Math.Abs(y) < EPSILON_STD) 
                y = 0;
            
            return new Vector2(x, y);
        }

        /// <summary>
        /// Scales the vector to the provided length.
        /// <para>WARNING: This is a costly operation.</para>
        /// </summary>
        /// <param name="x">The x-value of the vector (x, y).</param>
        /// <param name="y">The y-value of the vector (x, y).</param>
        /// <param name="length">The length to scale the vector to.</param>
        /// <returns>The vector scaled to the provided length.</returns>
        public static Vector2 ScaleTo(float x, float y, float length) 
        {
            if (length == 0 || (x == 0 && y == 0))
                return Vector2.zero;
            float factor = (length / (float)(Math.Sqrt(x * x + y * y)));
            return new Vector2(x * factor, y * factor);
        }

        /// <summary>
        /// Scales the vector to the provided length. Returns a new vector. The
        /// original vector is not mutated.
        /// <para>WARNING: This is a costly operation.</para>
        /// </summary>
        /// <param name="v">The vector to scale.</param>
        /// <param name="length">The length to scale the vector to.</param>
        /// <returns>A new vector scaled to the provided length.</returns>
        public static Vector2 ScaleTo(Vector2 v, float length) 
        {
            return ScaleTo(v.x, v.y, length);
        }
    }
}
