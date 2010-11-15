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
using org.critterai.math.geom;

namespace org.critterai.mesh
{
    /// <summary>
    /// Provides utility functions related to 3D triangle meshes.
    /// </summary>
    /// <remarks>
    /// <p> Only the vertex portions of the entries are recognized,  
    /// and only positive indices supported.</p>
    /// <p>This class is optimized for speed.  To support this priority, no 
    /// argument validation is performed.  E.g. No null reference checks, 
    /// minimal checks for poorly formed data.</p>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public static class TriangleMesh3Util
    {
        /// <summary>
        /// Gets the triangle wrap statistics for the xz-plane.
        /// </summary>
        /// <remarks>
        /// The triangles are projected onto the xz-plane then wrap direction
        /// determined.
        /// </remarks>
        /// <param name="vertices">The mesh vertices in the form (x, y, z).
        /// </param>
        /// <param name="triangles">The mesh triangles in the form
        /// (vertAIndex, vertBIndex, vertCIndex)</param>
        /// <param name="cwCount">The number of triangles wrapped in the
        /// clockwise direction.</param>
        /// <param name="ccwCount">The number of triangles wrapped in the
        /// counter-clockwise direction.</param>
        /// <param name="vertical">The number of triangles that are verticle.
        /// (I.e. Don't have a significant projection on the xz-plane.)</param>
        public static void GetWrapStatisicsXZ(float[] vertices, int[] triangles
            , out int cwCount, out int ccwCount, out int vertical)
        {
            const float tolerance = math.MathUtil.TOLERANCE_STD;
            cwCount = 0;
            ccwCount = 0;
            vertical = 0;
            int polyCount = triangles.Length / 3;
            for (int iPoly = 0; iPoly < polyCount; iPoly++)
            {
                int pVertA = triangles[iPoly*3+0]*3;
                int pVertB = triangles[iPoly*3+1]*3;
                int pVertC = triangles[iPoly*3+2]*3;
                float a = Triangle2.GetSignedAreaX2(
                      vertices[pVertA+0], vertices[pVertA+2]
                    , vertices[pVertB+0], vertices[pVertB+2]
                    , vertices[pVertC+0], vertices[pVertC+2]);
                if (a > tolerance)
                    ccwCount++;
                else if (a < -tolerance)
                    cwCount++;
                else
                    vertical++;
            }
        }

        /// <summary>
        /// Inverts the x-axis of the vertices.
        /// </summary>
        /// <param name="vertices">The mesh vertices in the form (x, y, z).
        /// </param>
        public static void InvertXAxis(float[] vertices)
        {
            for (int p = 0; p < vertices.Length; p += 3)
            {
                vertices[p] *= -1;
            }
        }

        /// <summary>
        /// Reverses the current wrap direction of all triangles.
        /// </summary>
        /// <param name="triangles">The mesh triangle indices in the form
        /// (vertAIndex, vertBIndex, vertCIndex).</param>
        public static void ReverseWrapDirection(int[] triangles)
        {
            // Note that the pointer is starting at one.  So it is actually
            // operating on the 2nd and 3rd vertices.
            for (int p = 1; p < triangles.Length; p += 3)
            {
                int t = triangles[p];
                triangles[p] = triangles[p + 1];
                triangles[p + 1] = t;
            }
        }
    }
}
