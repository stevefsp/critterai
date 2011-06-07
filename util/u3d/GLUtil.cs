/*
 * Copyright (c) 2011 Stephen A. Pratt
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

namespace org.critterai
{
    /// <summary>
    /// Provides various utility functions related to GL rendering.
    /// </summary>
    public static class GLUtil
    {
        private static Material mSimpleMaterial = null;

        /// <summary>
        /// Provides a shared material suitable for simple drawing operations.
        /// (Such as debug visualizations.)
        /// </summary>
        public static Material SimpleMaterial
        {
            get
            {
                if (!mSimpleMaterial)
                {
                    mSimpleMaterial = new Material(
                        "Shader \"Lines/Colored Blended\" {"
                        + "SubShader { Pass { "
                        + "	BindChannels { Bind \"Color\",color } "
                        + "	Blend SrcAlpha OneMinusSrcAlpha "
                        + "	ZWrite Off Cull Off Fog { Mode Off } "
                        + "} } }");
                    mSimpleMaterial.hideFlags = HideFlags.HideAndDontSave;
                    mSimpleMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
                }
                return mSimpleMaterial;
            }
        }

        /// <summary>
        /// Draws the provided triangle mesh in a manner suitable for 
        /// debug visualizations. 
        /// (Wiremesh with a low alpha transparent surface.)
        /// </summary>
        /// <remarks>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="vertices">The vertices.</param>
        /// <param name="triangles">The triangle indices.</param>
        /// <param name="drawColor">The base color to use. (Alpha is ignored.)
        /// </param>
        public static void DrawTriMesh(float[] vertices
            , int[] triangles
            , Color drawColor)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            drawColor.a = 0.25f;

            GL.Begin(GL.TRIANGLES);
            GL.Color(drawColor);
            for (int p = 0; p < triangles.Length; p += 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    GL.Vertex3(vertices[triangles[p + i] * 3 + 0]
                        , vertices[triangles[p + i] * 3 + 1]
                        , vertices[triangles[p + i] * 3 + 2]);
                }
            }
            GL.End();

            drawColor.a = 0.4f;

            for (int p = 0; p < triangles.Length; p += 3)
            {
                GL.Begin(GL.LINES);
                GL.Color(drawColor);
                for (int i = 0; i < 3; i++)
                {
                    GL.Vertex3(vertices[triangles[p + i] * 3 + 0]
                        , vertices[triangles[p + i] * 3 + 1]
                        , vertices[triangles[p + i] * 3 + 2]);
                }
                GL.Vertex3(vertices[triangles[p] * 3 + 0]
                    , vertices[triangles[p] * 3 + 1]
                    , vertices[triangles[p] * 3 + 2]);
                GL.End();
            }
        }
    }
}
