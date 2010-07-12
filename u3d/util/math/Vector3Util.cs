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
    /// Contains various static operations applicable to 3D vectors which are not provided
    /// by the Unity3D vector class.
    /// </summary>
    /// <remarks>
    /// <para>This class is optimized for speed.  To support this priority, no argument validation is
    /// performed.  E.g. No null checks, no divide by zero checks, etc.</para>
    /// <para>Static operations are thread safe.</para>
    /// </remarks>
    public static class Vector3Util
    {

        /// <summary>
        /// Performs a vector "right-handed" Cross product. (u x v)
        /// The resulting vector will be perpendicular to the plane 
        /// containing the two provided vectors.
        /// <para>Special Case: The result will be zero if the two vectors are parallel.</para>
        /// </summary>
        /// <param name="ux">The x-value of the vector (ux, uy, uz).</param>
        /// <param name="uy">The y-value of the vector (ux, uy, uz).</param>
        /// <param name="uz">The z-value of the vector (ux, uy, uz).</param>
        /// <param name="vx">The x-value of the vector (vx, vy, vz).</param>
        /// <param name="vy">The y-value of the vector (vx, vy, vz).</param>
        /// <param name="vz">The z-value of the vector (vx, vy, vz).</param>
        /// <returns>The cross product of the two vectors.</returns>
        public static Vector3 Cross(
                  float ux, float uy, float uz
                , float vx, float vy, float vz)
        {
            return new Vector3(uy * vz - uz * vy
                , -ux * vz + uz * vx
                , ux * vy - uy * vx);
        }

        /// <summary>
        /// Returns the square of the distance between the two provided points. (distance * distance)
        /// </summary>
        /// <param name="ax">The x-value of the point (ax, ay, az).</param>
        /// <param name="ay">The y-value of the point (ax, ay, az).</param>
        /// <param name="az">The z-value of the point (ax, ay, az). </param>
        /// <param name="bx">The x-value of the point (bx, by, bz).</param>
        /// <param name="by">The y-value of the point (bx, by, bz).</param>
        /// <param name="bz">The z-value of the point (bx, by, bz).</param>
        /// <returns> The square of the distance between the two provided points.</returns>
        public static float GetDistanceSq(float ax, float ay, float az
                , float bx, float by, float bz)
        {
            float dx = ax - bx;
            float dy = ay - by;
            float dz = az - bz;
            return (dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Returns the square of the length of the vector. (length * length)
        /// </summary>
        /// <param name="x">The x-value of the vector (x, y, z).</param>
        /// <param name="y">The y-value of the vector (x, y, z).</param>
        /// <param name="z">The z-value of the vector (x, y, z).</param>
        /// <returns>The square of the length of the vector.</returns>
        public static float GetLengthSq(float x, float y, float z)
        {
            return (x * x + y * y + z * z);
        }

        /// <summary>
        /// Returns the dot product of the provided vectors.
        /// </summary>
        /// <param name="ux">The x-value of the vector (ux, uy, uz).</param>
        /// <param name="uy">The y-value of the vector (ux, uy, uz).</param>
        /// <param name="uz">The z-value of the vector (ux, uy, uz).</param>
        /// <param name="vx">The x-value of the vector (vx, vy, vz).</param>
        /// <param name="vy">The y-value of the vector (vx, vy, vz).</param>
        /// <param name="vz">The z-value of the vector (vx, vy, vz).</param>
        /// <returns>The dot product of the provided vectors.</returns>
        public static float Dot(float ux, float uy, float uz
                , float vx, float vy, float vz)
        {
            return (ux * vx) + (uy * vy) + (uz * vz);
        }

        /// <summary>
        /// Determines whether or not the elements of the provided vectors are equal within
        /// the specified tolerance of each other.Each element of the vector is tested separately.
        /// </summary>
        /// <param name="ux">The x-value of the vector (ux, uy, uz).</param>
        /// <param name="uy">The y-value of the vector (ux, uy, uz).</param>
        /// <param name="uz">The z-value of the vector (ux, uy, uz).</param>
        /// <param name="vx">The x-value of the vector (vx, vy, vz).</param>
        /// <param name="vy">The y-value of the vector (vx, vy, vz).</param>
        /// <param name="vz">The z-value of the vector (vx, vy, vz).</param>
        /// <param name="tolerance">The tolerance for the test.  </param>
        /// <returns>TRUE if the the associated elements of each vector are within the specified tolerance
        /// of each other.  Otherwise FALSE.</returns>
        public static Boolean SloppyEquals(float ux, float uy, float uz
                , float vx, float vy, float vz
                , float tolerance)
        {
            tolerance = Math.Max(0, tolerance);
            if (vx < ux - tolerance || vx > ux + tolerance)
                return false;
            if (vy < uy - tolerance || vy > uy + tolerance)
                return false;
            if (vz < uz - tolerance || vz > uz + tolerance)
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether or not the elements of the provided vectors are equal within
        /// the specified tolerance of each other.Each element of the vector is tested separately.
        /// </summary>
        /// <param name="u">Vector u</param>
        /// <param name="v">Vector v</param>
        /// <param name="tolerance">The tolerance for the test.  </param>
        /// <returns>TRUE if the the associated elements of each vector are within the specified tolerance
        /// of each other.  Otherwise FALSE.</returns>
        public static Boolean SloppyEquals(Vector3 u, Vector3 v, float tolerance)
        {
            return SloppyEquals(u.x, u.y, u.z, v.x, v.y, v.z, tolerance);
        }

        /// <summary>
        /// Determines whether or not the elements of the provided vectors are equal within
        /// the specified tolerance of each other.Each element of the vector is tested separately.
        /// </summary>
        /// <param name="u">Vector u</param>
        /// <param name="vx">The x-value of the vector (vx, vy, vz).</param>
        /// <param name="vy">The y-value of the vector (vx, vy, vz).</param>
        /// <param name="vz">The z-value of the vector (vx, vy, vz).</param>
        /// <param name="tolerance">The tolerance for the test.  </param>
        /// <returns>TRUE if the the associated elements of each vector are within the specified tolerance
        /// of each other.  Otherwise FALSE.</returns>
        public static Boolean SloppyEquals(Vector3 u, float vx, float vy, float vz, float tolerance)
        {
            return SloppyEquals(u.x, u.y, u.z, vx, vy, vz, tolerance);
        }

        /// <summary>
        /// Traslates point A toward point B by the specified factor of the 
        /// distance between them.
        /// </summary>
        /// <remarks>
        /// <para>Examples:</para>
        /// <para>If the factor is 0.0, then the result will equal A.<br/>
        /// If the factor is 0.5, then the result will be the midpoint between A and B.<br/>
        /// If the factor is 1.0, then the result will equal B.<br/></para>
        /// </remarks>
        /// <param name="ax">The x-value of the point (ax, ay, az).</param>
        /// <param name="ay">The y-value of the point (ax, ay, az).</param>
        /// <param name="az">The z-value of the point (ax, ay, az). </param>
        /// <param name="bx">The x-value of the point (bx, by, bz).</param>
        /// <param name="by">The y-value of the point (bx, by, bz).</param>
        /// <param name="bz">The z-value of the point (bx, by, bz).</param>
        /// <param name="factor">The factor which governs the distance the point is translated
        /// from A toward B.</param>
        /// <returns>The point translated toward point B from point A.</returns>
        public static Vector3 TranslateToward(float ax, float ay, float az
                , float bx, float by, float bz
                , float factor)
        {
            return new Vector3(ax + (bx - ax) * factor
                , ay + (by - ay) * factor
                , az + (bz - az) * factor);
        }

        /// <summary>
        /// Flattens the vector array into a float array in the form (x, y, z).
        /// </summary>
        /// <param name="vectors">An array of vectors.</param>
        /// <returns>An array of flattened vectors.</returns>
        public static float[] Flatten(Vector3[] vectors)
        {
            float[] result = new float[vectors.Length * 3];
            for (int i = 0; i < vectors.Length; i++)
            {
                result[i * 3] = vectors[i].x;
                result[i * 3 + 1] = vectors[i].y;
                result[i * 3 + 2] = vectors[i].z;
            }
            return result;
        }
    }
}
