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
using System.Globalization;

namespace org.critterai.geom
{
    /// <summary>
    /// Provides some limited Wavefront utility methods. (Very limited.)
    /// </summary>
    /// <remarks>
    /// <para>Only a small subset of Wavefront information is supported.</para>
    /// <para>Only the "v" and "f" entries are recognized. All others are ignored.
    /// </para>
    /// <para>The v entries are expected to be in one of the following forms:
    /// </para>
    /// <blockquote>
    /// "v x y z w"<br/>
    /// "v x y z"
    /// </blockquote>
    /// <para>The f entries are expected to be in one of the following forms: 
    /// </para>
    /// <para>(Note that only triangles are supported.  Quads are not
    /// supported.)</para>
    /// <blockquote>
    /// "f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3"<br/>
    /// "f v1 v2 v3"
    /// </blockquote>
    /// <para> Only the vertex portions of the entries are recognized,  
    /// and only positive indices supported.</para>
    /// <para>Static methods are thread safe.</para>
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
                    + (vertices[p + 0] * xFactor)
                        .ToString(CultureInfo.InvariantCulture) + " "
                    + vertices[p + 1]
                        .ToString(CultureInfo.InvariantCulture) + " "
                    + vertices[p + 2]
                        .ToString(CultureInfo.InvariantCulture) + "\n");
            }
            for (int p = 0; p < triangles.Length; p += 3)
            {
                // The +1 converts to a 1-based index.
                if (reverseWrap)
                {
                    sb.Append("f "
                        + (triangles[p + 0] + 1)
                            .ToString(CultureInfo.InvariantCulture) + " "
                        + (triangles[p + 2] + 1)
                            .ToString(CultureInfo.InvariantCulture) + " "
                        + (triangles[p + 1] + 1)
                            .ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else
                {
                    sb.Append("f "
                        + (triangles[p + 0] + 1)
                            .ToString(CultureInfo.InvariantCulture) + " "
                        + (triangles[p + 1] + 1)
                            .ToString(CultureInfo.InvariantCulture) + " "
                        + (triangles[p + 2] + 1)
                            .ToString(CultureInfo.InvariantCulture) + "\n");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Translates a string in Wavefront format and loads the results
        /// into vertex and triangle arrays.
        /// </summary>
        /// <param name="wavefrontText">The wavefront format text defining
        /// the vertices and indices.</param>
        /// <param name="outBounds">An array of size 6 which will be loaed with 
        /// the AABB bounds of the resulting mesh in the form 
        /// (minX, minY, minZ, maxX, maxY, maxZ).  If null, the bounds will
        /// not be calculated.</param>
        /// <param name="vertices">The vertices from the wavefront data.
        /// </param>
        /// <param name="triangles">The triangles from the wavefront data.
        /// </param>
        public static void TranslateFrom(string wavefrontText
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
                string s = line.Trim();
                string[] tokens = null;
                if (s.StartsWith("v "))
                {
                    tokens = r.Split(s);
                    for (int i = 1; i < 4; i++)
                    {
                        string token = tokens[i];
                        lverts.Add(
                            float.Parse(token, CultureInfo.InvariantCulture));
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
                        lindices.Add(int.Parse(subtokens[0]
                            , CultureInfo.InvariantCulture) - 1);
                    }
                }
            }

            vertices = lverts.ToArray();
            triangles = lindices.ToArray();

            return;
        }
    }
}
