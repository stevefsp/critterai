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
using UnityEngine;

namespace org.critterai.geom
{
    /// <summary>
    /// Provides various 3D polygon utility methods.
    /// </summary>
    /// <remarks>
    /// <p>Unless otherwise noted, methods expect all polygon vertices to
    /// be co-planar.</p>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public static class Polygon3 
    {
        /// <summary>
        /// Determines whether a polygon is convex.
        /// </summary>
        /// <remarks>
        /// <p>Behavior is undefined if vertices are not coplanar.</p>
        /// <p>If the area of the triangle formed by the first three vertices 
        /// of the polygon is too small  to detect on both the (x, z) and 
        /// (x, y) planes, then this method may improperly return FALSE.</p>
        /// </remarks>
        /// <param name="vertices">An array of vertices which contains a 
        /// representation of polygons with an  arbitrary number of sides 
        /// in the form (x1, y1, z1, x2, y2, z2, ..., xn, yn, zn).  
        /// Wrap direction does not matter.</param>
        /// <param name="startVertIndex">The index of the first vertex 
        /// in the polygon.</param>
        /// <param name="vertCount">The number of vertices in the polygon.
        /// </param>
        /// <returns>TRUE if the polygon is convex.</returns>
        public static bool IsConvex(float[] vertices
            , int startVertIndex
            , int vertCount)
        {  
            if (vertCount < 3)
                return false;
            if (vertCount == 3)
                // It is a triangle, so it must be convex.
                return true;  
            
            // At this point we know that the polygon has at least 4 sides.
            
            /*
             *  Process will be to step through the sides, 3 vertices at a time.
             *  As long the signed area for the triangles formed by each set of
             *  vertices is the same (negative or positive), then the polygon 
             *  is convex.
             *  
             *  Using a shortcut by projecting onto the (x, z) or (x, y) plane 
             *  for all calculations. For a proper polygon, if the 2D 
             *  projection is convex, the 3D polygon is convex.
             *  
             *  There is one special case: A polygon that is vertical.  
             *  I.e. 2D on the (x, z) plane.
             *  This is detected during the first test.
             */

            int offset = 2;  // Start by projecting to the (x, z) plane.
            
            int pStartVert = startVertIndex*3;
            
            float initDirection = Triangle2.GetSignedAreaX2(
                vertices[pStartVert]
                , vertices[pStartVert+2]
                , vertices[pStartVert+3]
                , vertices[pStartVert+5]
                , vertices[pStartVert+6]
                , vertices[pStartVert+8]);
            
            if (initDirection > -2 * MathUtil.Epsilon 
                    && initDirection < 2 * MathUtil.Epsilon)
            {
                // The polygon is on or very close to the vertical plane.  
                // Switch to projecting on the (x, y) plane.
                offset = 1;
                initDirection = Triangle2.GetSignedAreaX2(
                    vertices[pStartVert]
                    , vertices[pStartVert+1]
                    , vertices[pStartVert+3]
                    , vertices[pStartVert+4]
                    , vertices[pStartVert+6]
                    , vertices[pStartVert+7]);
                // Design note: This is meant to be a strict zero test.
                if (initDirection == 0)
                    // Some sort of problem.  Should very rarely ever get here.
                    return false;  
            }
            
            int vertLength = (startVertIndex+vertCount)*3;
            for (int vertAPointer = pStartVert+3
                ; vertAPointer < vertLength
                ; vertAPointer += 3)
            {
                int vertBPointer = vertAPointer+3;
                if (vertBPointer >= vertLength) 
                    // Wrap it back to the start.
                    vertBPointer = pStartVert;  
                int vertCPointer = vertBPointer+3;
                if (vertCPointer >= vertLength)
                    // Wrap it back to the start.
                    vertCPointer = pStartVert;
                float direction = Triangle2.GetSignedAreaX2(
                          vertices[vertAPointer]
                        , vertices[vertAPointer+offset]
                        , vertices[vertBPointer]
                        , vertices[vertBPointer+offset]
                        , vertices[vertCPointer]
                        , vertices[vertCPointer+offset]);
                if (!(initDirection < 0 
                    && direction < 0) 
                    && !(initDirection > 0 
                    && direction > 0))
                    // The sign of the current direction is not the same as 
                    // the sign of the initial direction.  Can't be convex.
                    return false;
            }
            
            return true;
            
        }
        
        /// <summary>
        /// Returns the 
        /// <a href="http://en.wikipedia.org/wiki/Centroid" target="_blank">
        /// centroid</a> of a convex polygon.
        /// </summary>
        /// <remarks>
        /// <p>Behavior is undefined if the polygon is not convex.</p>
        /// <p>Behavior is undefined if the vector being overwritten in the 
        /// out array is a vertex in the polygon.  (Can only happen if the 
        /// vertices and out arrays are the same object.)</p>
        /// </remarks>
        /// <param name="vertices">An array of vertices which contains a 
        /// representation of a polygon with an  arbitrary number of sides 
        /// in the form (x1, y1, z1, x2, y2, z2, ..., xn, yn, zn).  
        /// Wrap direction does not matter.</param>
        /// <param name="startVertIndex">The index of the first vertex in the 
        /// polygon.</param>
        /// <param name="vertCount">The number of vertices in the polygon.
        /// </param>
        /// <param name="result">The array to store the result in.</param>
        /// <param name="resultVectorIndex">The vector index in the out array
        /// to store the result in.  (The stride is expected to be three.  
        /// So the insertion point will be outVectorIndex*3.)</param>
        /// <returns>A reference to the result argument.</returns>
        public static float[] GetCentroid(float[] vertices
                , int startVertIndex
                , int vertCount
                , float[] result
                , int resultVectorIndex)
        {
            // Reference: 
            // http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
            
            int vertLength = (startVertIndex+vertCount)*3;
            int pOut = resultVectorIndex*3;
            result[pOut] = 0;
            result[pOut+1] = 0;
            result[pOut+2] = 0;
            for (int pVert = startVertIndex*3; pVert < vertLength; pVert += 3)
            {
                result[pOut] += vertices[pVert];
                result[pOut+1] += vertices[pVert+1];
                result[pOut+2] += vertices[pVert+2];
            }

            result[pOut] /= vertCount;
            result[pOut+1] /= vertCount;
            result[pOut+2] /= vertCount;
            
            return result;
        }
        
        /// <summary>
        /// Returns the 
        /// <a href="http://en.wikipedia.org/wiki/Centroid" target="_blank">
        /// centroid</a> of a convex polygon.
        /// </summary>
        /// <param name="vertices">An array of vertices which contains a 
        /// representation of a polygon with an arbitrary number of sides in 
        /// the form (x1, y1, z1, x2, y2, z2, ..., xn, yn, zn).  
        /// Wrap direction does not matter.</param>
        /// <param name="startVertIndex">The index of the first vertex in 
        /// the polygon.</param>
        /// <param name="vertCount">The number of vertices in the polygon.
        /// </param>
        /// <returns>The centroid of a polygon.</returns>
        public static Vector3 GetCentroid(float[] vertices
                , int startVertIndex
                , int vertCount)
        {
            // Reference:
            // http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
            
            Vector3 result = new Vector3();
            int vertLength = (startVertIndex+vertCount)*3;
            for (int pVert = startVertIndex*3; pVert < vertLength; pVert += 3)
            {
                result.x += vertices[pVert];
                result.y += vertices[pVert+1];
                result.z += vertices[pVert+2];
            }

            result.x /= vertCount;
            result.y /= vertCount;
            result.z /= vertCount;
            
            return result;
        }
        
        /// <summary>
        /// Returns the 
        /// <a href="http://en.wikipedia.org/wiki/Centroid" target="_blank">
        /// centroid</a> of a convex polygon.
        /// </summary>
        /// <param name="vertices">An list of vertices which represent a 
        /// polygon with an  arbitrary number of sides in the form 
        /// (x1, y1, z1, x2, y2, z2, ..., xn, yn, zn).</param>
        /// <returns>The centroid of a convex polygon.</returns>
        public static Vector3 GetCentroid(params float[] vertices)
        {
            // Reference: 
            // http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
            
            Vector3 result = new Vector3();
            
            int vertCount = 0;
            for (int pVert = 0; pVert < vertices.Length; pVert += 3)
            {
                result.x += vertices[pVert];
                result.y += vertices[pVert+1];
                result.z += vertices[pVert+2];
                vertCount++;
            }

            result.x /= vertCount;
            result.y /= vertCount;
            result.z /= vertCount;

            return result;
        }
    }
}
