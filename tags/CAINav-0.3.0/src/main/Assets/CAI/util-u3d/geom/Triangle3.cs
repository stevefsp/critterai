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

namespace org.critterai.geom
{
    /// <summary>
    /// Provides various 3D triangle utility methods.
    /// </summary>
    /// <remarks>
    /// <para>Static methods are thread safe.</para>
    /// </remarks>
    public static class Triangle3 
    {
        /// <summary>
        /// Returns the area of the triangle ABC.
        /// WARNING: This is an costly method.
        /// </summary>
        /// <remarks>
        /// <para>If the value is only needed for comparison with other triangles, 
        /// then use  <see cref="GetAreaComp">GetAreaComp</see> instead. 
        /// </para>
        /// </remarks>
        /// <param name="ax">The x-value for vertex A in triangle ABC</param>
        /// <param name="ay">The y-value for vertex A in triangle ABC</param>
        /// <param name="az">The z-value for vertex A in triangle ABC</param>
        /// <param name="bx">The x-value for vertex B in triangle ABC</param>
        /// <param name="by">The y-value for vertex B in triangle ABC</param>
        /// <param name="bz">The z-value for vertex B in triangle ABC</param>
        /// <param name="cx">The x-value for vertex C in triangle ABC</param>
        /// <param name="cy">The y-value for vertex C in triangle ABC</param>
        /// <param name="cz">The z-value for vertex C in triangle ABC</param>
        /// <returns>The area of the triangle ABC.</returns>
        public static float GetArea(
                  float ax, float ay, float az
                , float bx, float by, float bz
                , float cx, float cy, float cz)
        {
            return (float)(Math.Sqrt(
                GetAreaComp(ax, ay, az, bx, by, bz, cx, cy, cz)) / 2);
        }
        
        /// <summary>
        /// Returns a value suitable for comparing the relative size of two 
        /// triangles. (E.g. Is triangleA larger than triangleB.  )
        /// </summary>
        /// <remarks>
        /// <para>The value returned by this method can be converted to area as 
        /// follows: Area = Math.sqrt(value)/2</para>
        /// <para>Useful for quickly comparing the size of triangles.</para>
        /// </remarks>
        /// <param name="ax">The x-value for vertex A in triangle ABC</param>
        /// <param name="ay">The y-value for vertex A in triangle ABC</param>
        /// <param name="az">The z-value for vertex A in triangle ABC</param>
        /// <param name="bx">The x-value for vertex B in triangle ABC</param>
        /// <param name="by">The y-value for vertex B in triangle ABC</param>
        /// <param name="bz">The z-value for vertex B in triangle ABC</param>
        /// <param name="cx">The x-value for vertex C in triangle ABC</param>
        /// <param name="cy">The y-value for vertex C in triangle ABC</param>
        /// <param name="cz">The z-value for vertex C in triangle ABC</param>
        /// <returns>A value suitable for comparing the relative size of two 
        /// triangles.</returns>
        public static float GetAreaComp(
                  float ax, float ay, float az
                , float bx, float by, float bz
                , float cx, float cy, float cz)
        {
            // References:
            // http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm#Modern%20Triangles
            
            // Get directional vectors.
            // A -> B
            float ux = bx - ax;
            float uy = by - ay;
            float uz = bz - az;
            // A -> C
            float vx = cx - ax;
            float vy = cy - ay;
            float vz = cz - az;

            // Cross product.
            float nx = uy * vz - uz * vy;
            float ny = -ux * vz + uz * vx;
            float nz = ux * vy - uy * vx;
            
            return Vector3Util.GetLengthSq(nx, ny, nz);
        }
        
        /// <summary>
        /// Returns the normal for the  triangle.
        /// WARNING: This is a costly method.
        /// </summary>
        /// <remarks>
        /// <para>The normal of a triangle is the vector perpendicular 
        /// to the triangle's plane with the direction determined by the 
        /// <a href="http://en.wikipedia.org/wiki/Right-hand_rule"
        /// target="_blank">right-handed rule</a>.</para>
        /// </remarks>
        /// <param name="ax">The x-value for vertex A in triangle ABC</param>
        /// <param name="ay">The y-value for vertex A in triangle ABC</param>
        /// <param name="az">The z-value for vertex A in triangle ABC</param>
        /// <param name="bx">The x-value for vertex B in triangle ABC</param>
        /// <param name="by">The y-value for vertex B in triangle ABC</param>
        /// <param name="bz">The z-value for vertex B in triangle ABC</param>
        /// <param name="cx">The x-value for vertex C in triangle ABC</param>
        /// <param name="cy">The y-value for vertex C in triangle ABC</param>
        /// <param name="cz">The z-value for vertex C in triangle ABC</param>
        /// <returns>The normal for the  triangle.</returns>
        public static Vector3 GetNormal(float ax, float ay, float az
                , float bx, float by, float bz
                , float cx, float cy, float cz)
        {
            // Reference: 
            // http://en.wikipedia.org/wiki/Surface_normal#Calculating_a_surface_normal
            // N = (B - A) x (C - A) with the final result normalized.
             
             Vector3 result = Vector3Util.Cross(bx - ax
                     , by - ay
                     , bz - az
                     , cx - ax
                     , cy - ay
                     , cz - az);
             result.Normalize();
            
            return result;
        }

        /// <summary>
        /// Returns the normal for the  triangle.
        /// WARNING: This is a costly method.
        /// </summary>
        /// <remarks>
        /// <para>The normal of a triangle is the vector perpendicular 
        /// to the triangle's plane with the direction determined by the 
        /// <a href="http://en.wikipedia.org/wiki/Right-hand_rule"
        /// target="_blank">right-handed rule</a>.</para>
        /// </remarks>
        /// <param name="vertices">An array of vertices which contains a 
        /// representation of triangles in the form 
        /// The wrap direction is  expected to be clockwise.
        /// [Form:(x, y, z) * 3]</param>
        /// <param name="startVertIndex">The index of the first vertex 
        /// in the triangle.</param>
        /// <returns>The normal for the  triangle.</returns>
        public static Vector3 GetNormal(float[] vertices, int startVertIndex)
        {
            int pStartVert = startVertIndex*3;
            return GetNormal(
                vertices[pStartVert]
                , vertices[pStartVert+1]
                , vertices[pStartVert+2]
                , vertices[pStartVert+3]
                , vertices[pStartVert+4]
                , vertices[pStartVert+5]
                , vertices[pStartVert+6]
                , vertices[pStartVert+7]
                , vertices[pStartVert+8]);
        }
    }
}
