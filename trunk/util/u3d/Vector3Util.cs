/*
 * Copyright (c) 2010-2011 Stephen A. Pratt
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

namespace org.critterai
{
    /// <summary>
    /// Provides various 3D vector utility methods which are not provided
    /// by the Unity3D vector class.
    /// </summary>
    /// <remarks>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public static class Vector3Util
    {
        /// <summary>
        /// Performs a vector "right-handed"
        /// <a href="http://en.wikipedia.org/wiki/Cross_product" 
        /// target="_blank">cross product</a>. (u x v)
        /// </summary>
        /// <remarks>
        /// <p>The resulting vector will be perpendicular to the plane 
        /// containing the two provided vectors.</p>
        /// <p>Special Case: The result will be zero if the two vectors are 
        /// parallel.</p>
        /// </remarks>
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
        /// Returns the square of the distance between the two provided 
        /// points.
        /// </summary>
        /// <param name="ax">The x-value of the point (ax, ay, az).</param>
        /// <param name="ay">The y-value of the point (ax, ay, az).</param>
        /// <param name="az">The z-value of the point (ax, ay, az). </param>
        /// <param name="bx">The x-value of the point (bx, by, bz).</param>
        /// <param name="by">The y-value of the point (bx, by, bz).</param>
        /// <param name="bz">The z-value of the point (bx, by, bz).</param>
        /// <returns> The square of the distance between the two provided 
        /// points.</returns>
        public static float GetDistanceSq(float ax, float ay, float az
                , float bx, float by, float bz)
        {
            float dx = ax - bx;
            float dy = ay - by;
            float dz = az - bz;
            return (dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Returns the square of the length of the vector.
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
        /// Returns the
        /// <a href="http://en.wikipedia.org/wiki/Dot_product" target="_blank">
        /// dot product</a> of the provided vectors.
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
        /// Determines whether or not the elements of the provided vectors 
        /// are equal within the specified tolerance of each other.
        /// </summary>
        /// <param name="ux">The x-value of the vector (ux, uy, uz).</param>
        /// <param name="uy">The y-value of the vector (ux, uy, uz).</param>
        /// <param name="uz">The z-value of the vector (ux, uy, uz).</param>
        /// <param name="vx">The x-value of the vector (vx, vy, vz).</param>
        /// <param name="vy">The y-value of the vector (vx, vy, vz).</param>
        /// <param name="vz">The z-value of the vector (vx, vy, vz).</param>
        /// <param name="tolerance">The tolerance for the test.  </param>
        /// <returns>TRUE if the associated elements are  within the 
        /// specified tolerance of each other</returns>
        public static bool SloppyEquals(float ux, float uy, float uz
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
        /// Determines whether or not the elements of the provided vectors 
        /// are equal within the specified tolerance of each other. 
        /// </summary>
        /// <param name="u">Vector u</param>
        /// <param name="v">Vector v</param>
        /// <param name="tolerance">The tolerance for the test.</param>
        /// <returns>TRUE if the associated elements are  within the 
        /// specified tolerance of each other</returns>
        public static bool SloppyEquals(Vector3 u, Vector3 v, float tolerance)
        {
            return SloppyEquals(u.x, u.y, u.z, v.x, v.y, v.z, tolerance);
        }

        /// <summary>
        /// Determines whether or not the elements of the provided vectors 
        /// are equal within the specified tolerance of each other.
        /// </summary>
        /// <param name="u">Vector u</param>
        /// <param name="vx">The x-value of the vector (vx, vy, vz).</param>
        /// <param name="vy">The y-value of the vector (vx, vy, vz).</param>
        /// <param name="vz">The z-value of the vector (vx, vy, vz).</param>
        /// <param name="tolerance">The tolerance for the test.  </param>
        /// <returns>TRUE if the associated elements are  within the 
        /// specified tolerance of each other</returns>
        public static bool SloppyEquals(Vector3 u
            , float vx, float vy, float vz
            , float tolerance)
        {
            return SloppyEquals(u.x, u.y, u.z, vx, vy, vz, tolerance);
        }

        /// <summary>
        /// Determines whether or not the two vectors are within range
        /// of each other based on a xz-plane radius and a y-axis height.
        /// </summary>
        /// <remarks>
        /// <p>This is essentially a cylinder inclusion test where one 
        /// vector defines the centroid of the cylinder.</p>
        /// </remarks>
        /// <param name="u">A position.</param>
        /// <param name="v">A position.</param>
        /// <param name="radius">The allowed radius on the xz-plane.</param>
        /// <param name="height">The allowed y-axis delta.</param>
        /// <returns>TRUE if the two vectors are within the xz-radius and 
        /// y-height of each other.</returns>
        public static bool IsInRange(Vector3 u, Vector3 v
            , float radius, float height)
        {
            Vector3 d = v - u;
            return (d.x * d.x + d.z * d.z) < radius * radius
                && Mathf.Abs(d.y) < height;
        }

        /// <summary>
        /// Traslates point A toward point B by the specified factor of the 
        /// distance between them.
        /// </summary>
        /// <remarks>
        /// <p>Examples:</p>
        /// <p>If the factor is 0.0, then the result will equal A.<br/>
        /// If the factor is 0.5, then the result will be the midpoint 
        /// between A and B.<br/>
        /// If the factor is 1.0, then the result will equal B.<br/></p>
        /// </remarks>
        /// <param name="ax">The x-value of the point (ax, ay, az).</param>
        /// <param name="ay">The y-value of the point (ax, ay, az).</param>
        /// <param name="az">The z-value of the point (ax, ay, az). </param>
        /// <param name="bx">The x-value of the point (bx, by, bz).</param>
        /// <param name="by">The y-value of the point (bx, by, bz).</param>
        /// <param name="bz">The z-value of the point (bx, by, bz).</param>
        /// <param name="factor">The factor which governs the distance the 
        /// point is translated from A toward B.</param>
        /// <returns>The point translated toward point B from point A.
        /// </returns>
        public static Vector3 TranslateToward(float ax, float ay, float az
                , float bx, float by, float bz
                , float factor)
        {
            return new Vector3(ax + (bx - ax) * factor
                , ay + (by - ay) * factor
                , az + (bz - az) * factor);
        }

        /// <summary>
        /// Flattens the vector array into a float array in the form 
        /// (x, y, z).
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

        /// <summary>
        /// Creates an array of vectors from a flattend array of vectors
        /// in the form (x, y, z).
        /// </summary>
        /// <param name="flatVectors">An array of vectors in the form
        /// (x, y, z).</param>
        /// <returns>An array of vectors.</returns>
        public static Vector3[] GetVectors(float[] flatVectors)
        {
            int count = flatVectors.Length / 3;
            Vector3[] result = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                int p = i * 3;
                result[i] = new Vector3(flatVectors[p + 0]
                    , flatVectors[p + 1]
                    , flatVectors[p + 2]);
            }
            return result;
        }

        /// <summary>
        /// Creates a Unity vector from an entry in an array of vectors.
        /// </summary>
        /// <param name="vectors">An array of vectors.
        /// [(x, y, z) * vertorCount]</param>
        /// <param name="index">The index of the vector in the array.
        /// </param>
        /// <returns>The Unity vector for the specified array vector.</returns>
        public static Vector3 GetVector(float[] vectors, int index)
        {
            return new Vector3(vectors[index * 3 + 0]
                , vectors[index * 3 + 1]
                , vectors[index * 3 + 2]);
        }

        /// <summary>
        /// Fills an array vector based on the provided Unity vector.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="buffer">The buffer to copy the source
        /// vector into. [Length: >= 3] (Out)
        /// </param>
        /// <returns>A reference to the buffer.</returns>
        public static float[] GetVector(Vector3 vector, float[] buffer)
        {
            buffer[0] = vector.x;
            buffer[1] = vector.y;
            buffer[2] = vector.z;
            return buffer;
        }

        /// <summary>
        /// Returns a Unity vector created from an array vector.
        /// </summary>
        /// <param name="vector">The array vector.  [(x, y, z)] </param>
        /// <returns>A Unity vector created from the array vector.</returns>
        public static Vector3 GetVector(float[] vector)
        {
            return new Vector3(vector[0], vector[1], vector[2]);
        }

        /// <summary>
        /// Gets the minimum and maximum bounds of the AABB which contains the 
        /// array of vectors.
        /// </summary>
        /// <param name="vectors">An array of vectors.</param>
        /// <param name="minBounds">The mimimum bounds of the AABB.</param>
        /// <param name="maxBounds">The maximum bounds of the AABB.</param>
        public static void GetBounds(Vector3[] vectors
            , out Vector3 minBounds
            , out Vector3 maxBounds)
        {
            minBounds = vectors[0];
            maxBounds = vectors[0];
            for (int i = 1; i < vectors.Length; i++)
            {
                minBounds.x = Math.Min(minBounds.x, vectors[i].x);
                minBounds.y = Math.Min(minBounds.y, vectors[i].y);
                minBounds.z = Math.Min(minBounds.z, vectors[i].z);
                maxBounds.x = Math.Max(maxBounds.x, vectors[i].x);
                maxBounds.y = Math.Max(maxBounds.y, vectors[i].y);
                maxBounds.z = Math.Max(maxBounds.z, vectors[i].z);
            }
        }

        /// <summary>
        /// Gets the minimum and maximum bounds of the AABB which contains the 
        /// array of vectors.
        /// </summary>
        /// <param name="flatVectors">An flattened array of vectors in the form
        /// (x, y, z).</param>
        /// <param name="bounds">An array of length 6 to store the bounds 
        /// result in. Form: (minX, minY, minZ, maxX, maxY, maxZ).
        /// Can be null.
        /// </param>
        /// <returns>
        /// The bounds in the form (minX, minY, minZ, maxX, maxY, maxZ).
        /// Will be a reference to the bounds argument if one was provided.
        /// </returns>
        public static float[] GetBounds(float[] flatVectors
            , float[] bounds)
        {

            if (bounds == null)
                bounds = new float[6];

            bounds[0] = flatVectors[0];
            bounds[1] = flatVectors[1];
            bounds[2] = flatVectors[2];
            bounds[3] = flatVectors[0];
            bounds[4] = flatVectors[1];
            bounds[5] = flatVectors[2];  

            for (int p = 3; p < flatVectors.Length; p += 3)
            {
                bounds[0] = Math.Min(bounds[0], flatVectors[p + 0]);
                bounds[1] = Math.Min(bounds[1], flatVectors[p + 1]);
                bounds[2] = Math.Min(bounds[2], flatVectors[p + 2]);
                bounds[3] = Math.Max(bounds[3], flatVectors[p + 0]);
                bounds[4] = Math.Max(bounds[4], flatVectors[p + 1]);
                bounds[5] = Math.Max(bounds[5], flatVectors[p + 2]);
            }

            return bounds;
        }

        /// <summary>
        /// Returns the standard string representation of the provided array
        /// vector.
        /// </summary>
        /// <param name="vector">An array vector. [(x, y, z)]</param>
        /// <returns>A string representing the vector.</returns>
        public static string ToString(float[] vector)
        {
            return string.Format("[{0:F3}, {1:F3}, {2:F3}]"
                , vector[0], vector[1], vector[2]);
        }

        /// <summary>
        /// Returns a standard string representation of the provided Unity
        /// vector.
        /// </summary>
        /// <param name="vector">A vector.</param>
        /// <returns>A string representing the vector.</returns>
        public static string ToString(Vector3 vector)
        {
            return string.Format("[{0:F3}, {1:F3}, {2:F3}]"
                , vector.x, vector.y, vector.z);
        }
    }
}
