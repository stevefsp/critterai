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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using org.critterai.math;

namespace org.critterai.mesh
{
    /// <summary>
    /// Provides some simple Wavefront utility methods.
    /// </summary>
    /// <remarks>
    /// <p>Only a small subset of Wavefront information is supported.</p>
    /// <p>Only the "v" and "f" entries are recognized. All others are ignored.
    /// </p>
    /// <p>The v entries are expected to be in one of the following forms:
    /// </p>
    /// <blockquote>
    /// "v x y z w"<br/>
    /// "v x y z"
    /// </blockquote>
    /// <p>The f entries are expected to be in one of the following forms: 
    /// </p>
    /// <blockquote>
    /// "f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3"<br/>
    /// "f v1 v2 v3"
    /// </blockquote>
    /// <p> Only the vertex portions of the entries are recognized,  
    /// and only positive indices supported.</p>
    /// <p>This class is optimized for speed.  To support this priority, no 
    /// argument validation is performed.  E.g. Minimal checks for poorly 
    /// formed data.</p>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public static class Wavefront
    {
        /// <summary>
        /// Creates a Wavefront format string from a set of vertices and 
        /// triangle indices.
        /// </summary>
        /// <param name="vertices">The vertices in the form (x, y, z)</param>
        /// <param name="triangles">The triangles in the form
        /// (vertAIndex, vertBIndex, vertCIndex).</param>
        /// <param name="reverseWrap">Revers the wrap direction of the
        /// triangles.</param>
        /// <param name="invertXAxis">Invert the x-axis values.</param>
        /// <returns>A string representing the mesh in Wavefront format.
        /// </returns>
        public static string TranslateTo(float[] vertices
            , int[] triangles
            , bool reverseWrap
            , bool invertXAxis)
        {
            StringBuilder sb = new StringBuilder();
            float xFactor = (invertXAxis ? -1 : 1);
            for (int p = 0; p < vertices.Length; p += 3)
            {
                sb.Append("v " 
                    + (vertices[p + 0] * xFactor) + " " 
                    + vertices[p + 1] + " " 
                    + vertices[p + 2] + "\n");
            }
            for (int p = 0; p < triangles.Length; p += 3)
            {
                // The +1 converts to a 1-based index.
                if (reverseWrap)
                {
                    sb.Append("f "
                        + (triangles[p + 0] + 1) + " "
                        + (triangles[p + 2] + 1) + " "
                        + (triangles[p + 1] + 1) + "\n");
                }
                else
                {
                    sb.Append("f "
                        + (triangles[p + 0] + 1) + " "
                        + (triangles[p + 1] + 1) + " "
                        + (triangles[p + 2] + 1) + "\n");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Translates a string in Wavefront format and loads the results
        /// into vertex and triangle arrays.
        /// </summary>
        /// <param name="wavefrontText"></param>
        /// <param name="bounds">An array of size 6 which will be loaed with 
        /// the AABB bounds of the resulting mesh in the form 
        /// (minX, minY, minZ, maxX, maxY, maxZ).  If null, the bounds will
        /// not be calculated.</param>
        /// <param name="vertices">The vertices from the wavefront data.
        /// </param>
        /// <param name="triangles">The triangles from the wavefront data.
        /// </param>
        public static void TranslateFrom(string wavefrontText
            , float[] bounds
            , out float[] vertices
            , out int[] triangles)
        {
            List<float> lverts = new List<float>();
            List<int> lindices = new List<int>();

            Regex nl = new Regex(@"\n");
            Regex r = new Regex(@"\s+");
            Regex rs = new Regex(@"\/");

            string[] lines = nl.Split(wavefrontText);
            int lineCount = 0;
            foreach (string line in lines)
            {
                lineCount++;
                string errPrefix = "Invalid vertex entry at line " 
                    + lineCount + ".";
                string s = line.Trim();
                string[] tokens = null;
                if (s.StartsWith("v "))
                {
                    tokens = r.Split(s);
                    for (int i = 1; i < 4; i++)
                    {
                        string token = tokens[i];
                        lverts.Add(float.Parse(token));
                    }
                }
                else if (s.StartsWith("f "))
                {
                    // This is a face entry.  Expecting one of:
                    // F  v1/vt1/vn1   v2/vt2/vn2   v3/vt3/vn3
                    // F  v1 v2 v3
                    tokens = r.Split(s);
                    for (int i = 1; i < 4; i++)
                    {
                        string token = tokens[i];
                        string[] subtokens = rs.Split(token);
                        // Subtraction converts from 1-based index to 
                        // zero-based index.
                        lindices.Add(int.Parse(subtokens[0]) - 1);
                    }
                }
            }

            vertices = lverts.ToArray();
            triangles = lindices.ToArray();

            if (bounds != null)
                Vector3Util.GetBounds(vertices, bounds);

            return;
        }
    }
}
