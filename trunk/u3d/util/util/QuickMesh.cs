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
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace org.critterai.util
{
    /// <summary>
    /// Creates a mesh from a file or <see cref="UnityEngine.TextAsset">TextAsset</see>
    /// that is in a simplified wavefront format.
    /// </summary>
    /// <remarks>Only the "v" and "f" entries are recognized.  All others are ignored.
    /// <para>The v entries are expected to be in one of the following forms: "v x y z w"
    /// or "v x y z"</para>
    /// <para>The f entries are expected to be in one of the following forms: 
    /// "f  v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3" or "f v1 v2 v3" Only the vertex portions of the
    /// entries are recognized.  Also, only positive indices are supported.  The vertex indices
    /// cannot be negative.</para></remarks>
    public sealed class QuickMesh
    {
        /// <summary>
        /// The mesh verts in the form (x, y, z).
        /// </summary>
        public readonly float[] verts;

        /// <summary>
        /// The mesh indices in the form (v1, v2, v3).
        /// </summary>
        public readonly int[] indices;

        /// <summary>
        /// The minimum bounds of the loaded mesh.
        /// </summary>
        public readonly Vector3 minBounds = new Vector3();

        /// <summary>
        /// The maximum bounds of the loaded mesh.
        /// </summary>
        public readonly Vector3 maxBounds = new Vector3();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fullText">The text asset in wavefront format.</param>
        /// <param name="keepWrapDirection">If TRUE, the wrap direction of the trianges is
        /// maintained.  Otherwise they are reversed.  E.g. If the wrap direction in the text assest
        /// is counter-clockwise, setting this value to TRUE will convert the triangles to clockwise
        /// wrapping.</param>
        public QuickMesh(TextAsset fullText, Boolean keepWrapDirection)
        {
            buildMesh(fullText.text, keepWrapDirection, out verts, out indices);
            buildMinMax(out minBounds, out maxBounds);
        }

        private void buildMinMax(out Vector3 minBounds, out Vector3 maxBounds)
        {
            minBounds = new Vector3(verts[0], verts[1], verts[2]);
            maxBounds = new Vector3(verts[0], verts[1], verts[2]);
            for (int pVert = 3; pVert < verts.Length; pVert += 3)
            {
                minBounds.x = System.Math.Min(minBounds.x, verts[pVert + 0]);
                minBounds.y = System.Math.Min(minBounds.y, verts[pVert + 1]);
                minBounds.z = System.Math.Min(minBounds.z, verts[pVert + 2]);
                maxBounds.x = System.Math.Max(maxBounds.x, verts[pVert + 0]);
                maxBounds.y = System.Math.Max(maxBounds.y, verts[pVert + 1]);
                maxBounds.z = System.Math.Max(maxBounds.z, verts[pVert + 2]);
            }
        }

        private void buildMesh(String fullText, Boolean keepWrapDirection, out float[] verts, out int[] indices)
        {
            List<float> lverts = new List<float>();
            List<int> lindices = new List<int>();

            Regex nl = new Regex(@"\n");
            Regex r = new Regex(@"\s+");
            Regex rs = new Regex(@"\/");

            String[] lines = nl.Split(fullText);
            int lineCount = 0;
            foreach (String line in lines)
            {
                lineCount++;
                String errPrefix = "Invalid vertex entry at line " + lineCount + ".";
                String s = line.Trim();
                String[] tokens = null;
                if (s.StartsWith("v "))
                {
                    // Vertex entry.  Expecting one of: 
                    // v x y z w
                    // v x y z
                    tokens = r.Split(s);
                    if (tokens.Length < 4)
                        throw new Exception(errPrefix + "Too few fields.");
                    for (int i = 1; i < 4; i++)
                    {
                        String token = tokens[i];
                        try
                        {
                            lverts.Add(float.Parse(token));
                        }
                        catch (Exception e)
                        {
                            if (e == null) { }
                            throw new Exception(errPrefix + " Field is not a valid float.");
                        }
                    }
                }
                else if (s.StartsWith("f "))
                {
                    // This is a face entry.  Expecting one of:
                    // F  v1/vt1/vn1   v2/vt2/vn2   v3/vt3/vn3
                    // F  v1 v2 v3
                    errPrefix = "Invalid face entry at line " + lineCount + ".";
                    tokens = r.Split(s);
                    if (tokens.Length < 4)
                        throw new Exception(errPrefix + "Too few fields.");
                    for (int i = 1; i < 4; i++)
                    {
                        String token = tokens[i];
                        String[] subtokens = rs.Split(token);
                        try
                        {
                            // Subtraction converts from 1-based index to zero-based index.
                            lindices.Add(int.Parse(subtokens[0]) - 1);
                        }
                        catch (Exception e)
                        {
                            if (e == null) { }
                            throw new Exception(errPrefix + " Field is not a valid integer.");
                        }
                    }
                }
            }

            verts = lverts.ToArray();
            indices = lindices.ToArray();

            // Default wrap direction for wavefront files is CCW.  We
            // want CW.
            if (!keepWrapDirection)
            {
                // Convert from counterclockwise to clockwise.
                for (int p = 1; p < indices.Length; p += 3)
                {
                    int t = indices[p];
                    indices[p] = indices[p + 1];
                    indices[p + 1] = t;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">The path of the wavefront formatted file to load.</param>
        /// <param name="keepWrapDirection">If TRUE, the wrap direction of the trianges is
        /// maintained.  Otherwise they are reversed.  E.g. If the wrap direction in the text assest
        /// is counter-clockwise, setting this value to TRUE will convert the triangles to clockwise
        /// wrapping.</param>
        public QuickMesh(String path, Boolean keepWrapDirection)
        {
            StreamReader reader = File.OpenText(path);

            buildMesh(reader.ReadToEnd(), keepWrapDirection, out verts, out indices);
            buildMinMax(out minBounds, out maxBounds);
        }
    }
}
