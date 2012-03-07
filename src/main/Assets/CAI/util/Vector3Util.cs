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
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai
{
    /// <summary>
    /// Provides various 3D vector utility methods which are not provided
    /// by the Unity3D vector class.
    /// </summary>
    /// <remarks>
    /// <para>Static methods are thread safe.</para>
    /// </remarks>
    public static class Vector3Util
    {
        ///// <summary>
        ///// Performs a vector "right-handed"
        ///// <a href="http://en.wikipedia.org/wiki/Cross_product" 
        ///// target="_blank">cross product</a>. (u x v)
        ///// </summary>
        ///// <remarks>
        ///// <para>The resulting vector will be perpendicular to the plane 
        ///// containing the two provided vectors.</para>
        ///// <para>Special Case: The result will be zero if the two vectors are 
        ///// parallel.</para>
        ///// </remarks>
        ///// <param name="ux">The x-value of the vector (ux, uy, uz).</param>
        ///// <param name="uy">The y-value of the vector (ux, uy, uz).</param>
        ///// <param name="uz">The z-value of the vector (ux, uy, uz).</param>
        ///// <param name="vx">The x-value of the vector (vx, vy, vz).</param>
        ///// <param name="vy">The y-value of the vector (vx, vy, vz).</param>
        ///// <param name="vz">The z-value of the vector (vx, vy, vz).</param>
        ///// <returns>The cross product of the two vectors.</returns>


        public static Vector3 Cross(Vector3 u, Vector3 v)
        {
            return new Vector3(u.y * v.z - u.z * v.y
                , -u.x * v.z + u.z * v.x
                , u.x * v.y - u.y * v.x);
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
        public static float GetDistanceSq(Vector3 a, Vector3 b)
        {
            Vector3 d = a - b;
            return (d.x * d.x + d.y * d.y + d.z * d.z);
        }

        public static Vector3 Normalize(Vector3 v)
        {
            float m = (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            if (m < MathUtil.Tolerance)
                m = 1;

            v.x /= m;
            v.y /= m;
            v.z /= m;

            if (Math.Abs(v.x) < MathUtil.Tolerance)
                v.x = 0;
            if (Math.Abs(v.y) < MathUtil.Tolerance)
                v.y = 0;
            if (Math.Abs(v.z) < MathUtil.Tolerance)
                v.z = 0;

            return v;
        }

        /// <summary>
        /// Gets the distance between the specified points on the xz-plane. 
        /// (Ignores y-axis.)
        /// </summary>
        /// <param name="u">Vector u.</param>
        /// <param name="v">Vector v.</param>
        /// <returns>The distance between the specified points on the 
        /// xz-plane.</returns>
        public static float GetDistance2D(Vector3 u, Vector3 v)
        {
            float dx = v.x - u.x;
            float dz = v.z - u.z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        ///// <summary>
        ///// Returns the square of the length of the vector.
        ///// </summary>
        ///// <param name="x">The x-value of the vector (x, y, z).</param>
        ///// <param name="y">The y-value of the vector (x, y, z).</param>
        ///// <param name="z">The z-value of the vector (x, y, z).</param>
        ///// <returns>The square of the length of the vector.</returns>

        public static float GetLengthSq(Vector3 v)
        {
            return (v.x * v.x + v.y * v.y + v.z * v.z);
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
        public static float Dot(Vector3 u, Vector3 v)
        {
            return (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
        }

        /// <summary>
        /// Determines whether or not the provided vectors are equal within
        /// the specified tolerance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Change in beahvior:  Prior to version 0.4, the area of equality 
        /// for this method was an axis-aligned bounding box at the tip of
        /// the vector. As of version 0.4 the area of equality is a sphere.
        /// This change was made to improve performance.
        /// </para>
        /// </remarks>
        /// <param name="ux">The x-value of the vector (ux, uy, uz).</param>
        /// <param name="uy">The y-value of the vector (ux, uy, uz).</param>
        /// <param name="uz">The z-value of the vector (ux, uy, uz).</param>
        /// <param name="vx">The x-value of the vector (vx, vy, vz).</param>
        /// <param name="vy">The y-value of the vector (vx, vy, vz).</param>
        /// <param name="vz">The z-value of the vector (vx, vy, vz).</param>
        /// <param name="tolerance">The allowed tolerance. [Limit: >= 0]
        /// </param>
        /// <returns>True if the provided vectors are similar enough to be
        /// considered equal.
        /// </returns>
        public static bool SloppyEquals(Vector3 u, Vector3 v, float tolerance)
        {
            // Duplicating code for performance reasons.
            Vector3 d = u - v;
            return (d.x * d.x + d.y * d.y + d.z * d.z) <= tolerance * tolerance;
        }

        /// <summary>
        /// Determines whether or not the two points are within range
        /// of each other based on a xz-plane radius and a y-axis height.
        /// </summary>
        /// <remarks>
        /// <para>Essentially, one point defines the centroid of the cylinder 
        /// and the other in tested for inclusion.</para>
        /// </remarks>
        /// <param name="a">A position.</param>
        /// <param name="b">A position.</param>
        /// <param name="radius">The allowed radius on the xz-plane.</param>
        /// <param name="height">The allowed y-axis delta.</param>
        /// <returns>TRUE if the two vectors are within the xz-radius and 
        /// y-height of each other.</returns>
        public static bool IsInRange(Vector3 a, Vector3 b
            , float radius, float height)
        {
            Vector3 d = b - a;
            return (d.x * d.x + d.z * d.z) < radius * radius
                && Math.Abs(d.y) < height;
        }

        /// <summary>
        /// Traslates point A toward point B by the specified factor of the 
        /// distance between them.
        /// </summary>
        /// <remarks>
        /// <para>Examples:</para>
        /// <para>If the factor is 0.0, then the result will equal A.<br/>
        /// If the factor is 0.5, then the result will be the midpoint 
        /// between A and B.<br/>
        /// If the factor is 1.0, then the result will equal B.<br/></para>
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
        public static Vector3 TranslateToward(Vector3 a, Vector3 b, float factor)
        {
            return new Vector3(a.x + (b.x - a.x) * factor
                , a.y + (b.y - a.y) * factor
                , a.z + (b.z - a.z) * factor);
        }

        /// <summary>
        /// Flattens the vector array into a float array in the form 
        /// (x, y, z).
        /// </summary>
        /// <param name="vectors">An array of vectors.</param>
        /// <returns>An array of flattened vectors.</returns>
        public static float[] Flatten(Vector3[] vectors, int vertCount)
        {
            float[] result = new float[vertCount * 3];
            for (int i = 0; i < vertCount; i++)
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

        public static Vector3[] GetVectors(float[] source
            , int sourceIndex
            , Vector3[] target
            , int targetIndex
            , int count)
        {
            if (target == null)
                target = new Vector3[count + targetIndex];

            for (int i = 0; i < count; i++)
            {
                int p = (sourceIndex + i) * 3;
                target[targetIndex + i] = 
                    new Vector3(source[p + 0], source[p + 1], source[p + 2]);
            }

            return target;
        }

        /// <summary>
        /// Gets the minimum and maximum bounds of the AABB which contains the 
        /// array of vectors.
        /// </summary>
        /// <param name="vectors">An array of vectors.</param>
        /// <param name="minBounds">The mimimum bounds of the AABB.</param>
        /// <param name="maxBounds">The maximum bounds of the AABB.</param>
        public static void GetBounds(Vector3[] vectors
            , int vectorCount
            , out Vector3 boundsMin
            , out Vector3 boundsMax)
        {
            boundsMin = vectors[0];
            boundsMax = vectors[0];

            for (int i = 1; i < vectorCount; i++)
            {
                boundsMin.x = Math.Min(boundsMin.x, vectors[i].x);
                boundsMin.y = Math.Min(boundsMin.y, vectors[i].y);
                boundsMin.z = Math.Min(boundsMin.z, vectors[i].z);
                boundsMax.x = Math.Max(boundsMax.x, vectors[i].x);
                boundsMax.y = Math.Max(boundsMax.y, vectors[i].y);
                boundsMax.z = Math.Max(boundsMax.z, vectors[i].z);
            }
        }

        // TODO: Move to triangle mesh.  Vector3Util does not know about triangles.
        public static void GetBounds(Vector3[] verts
             , int[] tris
             , int triCount
             , out Vector3 boundsMin
             , out Vector3 boundsMax)
        {
            // TODO: Add unit test.

            boundsMin = verts[tris[0]];
            boundsMax = verts[tris[0]];

            for (int i = 1; i < triCount * 3; i++)
            {
                Vector3 v = verts[tris[i]];
                boundsMin.x = Math.Min(boundsMin.x, v.x);
                boundsMin.y = Math.Min(boundsMin.y, v.y);
                boundsMin.z = Math.Min(boundsMin.z, v.z);
                boundsMax.x = Math.Max(boundsMax.x, v.x);
                boundsMax.y = Math.Max(boundsMax.y, v.y);
                boundsMax.z = Math.Max(boundsMax.z, v.z);
            }
        }

        // TODO: Should this be here?
        public static bool IsBoundsValid(Vector3 boundsMin, Vector3 boundsMax)
        {
            return !(boundsMax.x < boundsMin.x
                || boundsMax.y < boundsMin.y
                || boundsMax.z < boundsMin.z);
        }

        public static Vector3 Zero
        {
            get { return new Vector3(0, 0, 0); }
        }

        /// <summary>
        /// Returns a standard string representation of the provided vector.
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
