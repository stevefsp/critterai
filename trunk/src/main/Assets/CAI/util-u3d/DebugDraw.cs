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

namespace org.critterai.u3d
{
    /// <summary>
    /// Provides various drawing methods suitable for debug views.
    /// </summary>
    public static class DebugDraw
    {
        private const int CircleSegments = 40;
        private const float Epsilon = 0.001f;

        private static float[] mDir;

        private static float[] Dir
        {
            get
            {
                if (mDir == null)
                {
                    mDir = new float[CircleSegments * 2];
                    for (int i = 0; i < CircleSegments; i++)
                    {
                        float a = 
                            (float)i / (float)CircleSegments * Mathf.PI * 2;
                        mDir[i * 2] = Mathf.Cos(a);
                        mDir[i * 2 + 1] = Mathf.Sin(a);
                    }
                }
                return mDir;
            }
        }

        /// <summary>
        /// Draws a partially transparent diamond at the specified position.
        /// </summary>
        /// <remarks>
        /// <para>A <paramref name="scale"/> of 1.0 will result in a diamond with a
        /// width and height of 2.0. (Each diamond point will be 1.0 from 
        /// the position.)</para>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="position">The position.</param>
        /// <param name="scale">The scale of the diamond.</param>
        /// <param name="color">The color of the diamond.</param>
        public static void DiamondMarker(Vector3 position
            , float scale
            , Color color)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            Vector3 u = position + Vector3.up * scale;
            Vector3 d = position + Vector3.down * scale;
            Vector3 r = position + Vector3.right * scale;
            Vector3 l = position + Vector3.left * scale;
            Vector3 f = position + Vector3.forward * scale;
            Vector3 b = position + Vector3.back * scale;

            GL.Begin(GL.TRIANGLES);

            color.a = 0.6f;
            GL.Color(color);

            // Top

            GL.Vertex(u);
            GL.Vertex(r);
            GL.Vertex(f);

            GL.Vertex(u);
            GL.Vertex(f);
            GL.Vertex(l);

            GL.Vertex(u);
            GL.Vertex(l);
            GL.Vertex(b);

            GL.Vertex(u);
            GL.Vertex(b);
            GL.Vertex(r);

            // Bottom

            GL.Vertex(d);
            GL.Vertex(f);
            GL.Vertex(r);

            GL.Vertex(d);
            GL.Vertex(r);
            GL.Vertex(b);

            GL.Vertex(d);
            GL.Vertex(b);
            GL.Vertex(l);

            GL.Vertex(d);
            GL.Vertex(l);
            GL.Vertex(f);

            GL.End();

            GL.Begin(GL.LINES);

            color.a = 0.9f;
            GL.Color(color);

            // Top

            GL.Vertex(u);
            GL.Vertex(f);

            GL.Vertex(u);
            GL.Vertex(b);

            GL.Vertex(u);
            GL.Vertex(r);

            GL.Vertex(u);
            GL.Vertex(l);

            // Bottom

            GL.Vertex(u);
            GL.Vertex(f);

            GL.Vertex(u);
            GL.Vertex(b);

            GL.Vertex(u);
            GL.Vertex(r);

            GL.Vertex(u);
            GL.Vertex(l);

            // Center

            GL.Vertex(r);
            GL.Vertex(f);

            GL.Vertex(r);
            GL.Vertex(b);

            GL.Vertex(l);
            GL.Vertex(f);

            GL.Vertex(l);
            GL.Vertex(b);

            GL.End();
        }

        /// <summary>
        /// Draws an X-marker at the specified position.
        /// </summary>
        /// <remarks>
        /// <para>A <paramref name="scale"/> of 1.0 will result in a marker with a
        /// width and height of 2.0. (Each point of the x-marker will be 1.0 
        /// from the position.)</para>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="position">The position.</param>
        /// <param name="scale">The scale of the marker.</param>
        /// <param name="color">The color of the marker.</param>
        public static void XMarker(Vector3 position
            , float scale
            , Color color)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            GL.Color(color);

            GL.Vertex(position + Vector3.right * scale);
            GL.Vertex(position + Vector3.left * scale);
            GL.Vertex(position + Vector3.forward * scale);
            GL.Vertex(position + Vector3.back * scale);
            GL.Vertex(position + Vector3.up * scale * 2);
            GL.Vertex(position + Vector3.down * scale * 2);

            GL.End();
        }

        /// <summary>
        /// Draws the specified line segments.
        /// </summary>
        /// <remarks>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="segments">The line segments.
        /// [(ax, ay, ax, bx, by, bz) * <paramref name="segmentCount"/>]</param>
        /// <param name="segmentCount">The number of segments.</param>
        /// <param name="color">The color of the segments.</param>
        public static void Segments(float[] segments
            , int segmentCount
            , Color color)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int i = 0; i < segmentCount; i++)
            {
                GL.Vertex3(segments[i * 6 + 0]
                    , segments[i * 6 + 1]
                    , segments[i * 6 + 2]);
                GL.Vertex3(segments[i * 6 + 3]
                    , segments[i * 6 + 4]
                    , segments[i * 6 + 5]);
            }

            GL.End();
        }

        /// <summary>
        /// Draws an outlined and filled, partially transparent convex polygon.
        /// </summary>
        /// <remarks>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="verts">The polygon vertices.
        /// [(x, y, z) * <paramref name="vertCount"/>]</param>
        /// <param name="vertCount">The number of vertices.</param>
        /// <param name="color">The color of the polygon. 
        /// (The alpha is ignored.)</param>
        public static void ConvexPoly(float[] verts
            , int vertCount
            , Color color)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            GL.Begin(GL.TRIANGLES);

            color.a = 0.4f;
            GL.Color(color);

            for (int i = 2; i < vertCount; i++)
            {
                int pB = (i - 1) * 3;
                int pC = (i + 0) * 3;

                GL.Vertex3(verts[0], verts[1], verts[2]);
                GL.Vertex3(verts[pB + 0], verts[pB + 1], verts[pB + 2]);
                GL.Vertex3(verts[pC + 0], verts[pC + 1], verts[pC + 2]);
            }

            GL.End();

            GL.Begin(GL.LINES);

            color.a = 0.6f;
            GL.Color(color);

            for (int i = 1; i < vertCount; i++)
            {
                int pA = (i - 1) * 3;
                int pB = (i + 0) * 3;

                GL.Vertex3(verts[pA + 0], verts[pA + 1], verts[pA + 2]);
                GL.Vertex3(verts[pB + 0], verts[pB + 1], verts[pB + 2]);
            }

            GL.End();

        }

        /// <summary>
        /// Draws a wireframe bounding box representing the extents.
        /// </summary>
        /// <remarks>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="position">The center of the bounding box.</param>
        /// <param name="extents">The extents of the bounding box. 
        /// (Half-lengths.)
        /// </param>
        /// <param name="color">The color of the bounding box. 
        /// (The alpha is ignored.)</param>
        public static void Extents(Vector3 position
            , Vector3 extents
            , Color color)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            Vector3 ua = position + extents;
            Vector3 ub = new Vector3(
                  position.x - extents.x
                , position.y + extents.y
                , position.z + extents.z);
            Vector3 uc = new Vector3(
                  position.x - extents.x
                , position.y + extents.y
                , position.z - extents.z);
            Vector3 ud = new Vector3(
                  position.x + extents.x
                , position.y + extents.y
                , position.z - extents.z);

            Vector3 la = new Vector3(
                  position.x + extents.x
                , position.y - extents.y
                , position.z + extents.z);
            Vector3 lb = new Vector3(
                  position.x - extents.x
                , position.y - extents.y
                , position.z + extents.z);
            Vector3 lc =  position - extents;
            Vector3 ld = new Vector3(
                  position.x + extents.x
                , position.y - extents.y
                , position.z - extents.z);

            GL.Begin(GL.LINES);

            color.a = 0.6f;
            GL.Color(color);

            // Top
            GL.Vertex(ua);
            GL.Vertex(ub);
            GL.Vertex(ub);
            GL.Vertex(uc);
            GL.Vertex(uc);
            GL.Vertex(ud);
            GL.Vertex(ud);
            GL.Vertex(ua);

            // Bottom
            GL.Vertex(la);
            GL.Vertex(lb);
            GL.Vertex(lb);
            GL.Vertex(lc);
            GL.Vertex(lc);
            GL.Vertex(ld);
            GL.Vertex(ld);
            GL.Vertex(la);

            // Risers
            GL.Vertex(ua);
            GL.Vertex(la);
            GL.Vertex(ub);
            GL.Vertex(lb);
            GL.Vertex(uc);
            GL.Vertex(lc);
            GL.Vertex(ud);
            GL.Vertex(ld);

            GL.End();
        }

        /// <summary>
        /// Draws a wireframe bounding box representing an extents.
        /// </summary>
        /// <remarks>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="position">The center of the bounding box.</param>
        /// <param name="extents">The extents of the bounding box. 
        /// (Half-lengths.) [(x, y, z)]
        /// </param>
        /// <param name="color">The color of the bounding box. 
        /// (The alpha is ignored.)</param>
        public static void Extents(Vector3 position
            , float[] extents
            , Color color)
        {
            // TODO: EVAL: Re-evaluate after decision on float[3] to Vector3
            // refactor. Should it be retired or share code with the
            // Vector3 overload?

            GLUtil.SimpleMaterial.SetPass(0);

            Vector3 ua = new Vector3(
                  position.x + extents[0]
                , position.y + extents[1]
                , position.z + extents[2]);
            Vector3 ub = new Vector3(
                  position.x - extents[0]
                , position.y + extents[1]
                , position.z + extents[2]);
            Vector3 uc = new Vector3(
                  position.x - extents[0]
                , position.y + extents[1]
                , position.z - extents[2]);
            Vector3 ud = new Vector3(
                  position.x + extents[0]
                , position.y + extents[1]
                , position.z - extents[2]);

            Vector3 la = new Vector3(
                  position.x + extents[0]
                , position.y - extents[1]
                , position.z + extents[2]);
            Vector3 lb = new Vector3(
                  position.x - extents[0]
                , position.y - extents[1]
                , position.z + extents[2]);
            Vector3 lc = new Vector3(
                  position.x - extents[0]
                , position.y - extents[1]
                , position.z - extents[2]);
            Vector3 ld = new Vector3(
                  position.x + extents[0]
                , position.y - extents[1]
                , position.z - extents[2]);

            GL.Begin(GL.LINES);

            color.a = 0.6f;
            GL.Color(color);

            // Top
            GL.Vertex(ua);
            GL.Vertex(ub);
            GL.Vertex(ub);
            GL.Vertex(uc);
            GL.Vertex(uc);
            GL.Vertex(ud);
            GL.Vertex(ud);
            GL.Vertex(ua);

            // Bottom
            GL.Vertex(la);
            GL.Vertex(lb);
            GL.Vertex(lb);
            GL.Vertex(lc);
            GL.Vertex(lc);
            GL.Vertex(ld);
            GL.Vertex(ld);
            GL.Vertex(la);

            // Risers
            GL.Vertex(ua);
            GL.Vertex(la);
            GL.Vertex(ub);
            GL.Vertex(lb);
            GL.Vertex(uc);
            GL.Vertex(lc);
            GL.Vertex(ud);
            GL.Vertex(ld);

            GL.End();
        }

        /// <summary>
        /// Draws an unfilled circle on the xz-plane.
        /// </summary>
        /// <remarks>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="position">The position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="color">The colore of the circle's boundary.</param>
        public static void Circle(Vector3 position, float radius, Color color)
        {
            float[] dir = Dir;

            GLUtil.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            GL.Color(color);

	        for (int i = 0, j = CircleSegments - 1; i < CircleSegments; j = i++)
	        {
		        GL.Vertex3(position.x + dir[j*2+0] * radius
                    , position.y
                    , position.z + dir[j*2+1] * radius);
		        GL.Vertex3(position.x + dir[i*2+0] * radius
                    , position.y
                    , position.z + dir[i*2+1] * radius);
	        }

            GL.End();
        }

        /// <summary>
        /// Draws an arrow.
        /// </summary>
        /// <remarks>
        /// <para>The arrow head can be attached to end A and/or end B of the
        /// specified line segment.</para>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="pointA">The A end of the arrow.</param>
        /// <param name="pointB">The B end of the arrow.</param>
        /// <param name="headScaleA">The scale of the A end arrow head.
        /// (Or zero for no head.)</param>
        /// <param name="headScaleB">The scale of the B end arrow head.
        /// (Or zero for no head.)</param>
        /// <param name="color">The color of the arrow.</param>
        public static void Arrow(Vector3 pointA
            , Vector3 pointB
            , float headScaleA
            , float headScaleB
            , Color color)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            GL.Color(color);

            GL.Vertex(pointA);
            GL.Vertex(pointB);

            if (headScaleA > Epsilon)
                AppendArrowHead(pointA, pointB, headScaleA);
            if (headScaleB > Epsilon)
                AppendArrowHead(pointB, pointA, headScaleB);

            GL.End();
        }

        private static void AppendArrowHead(Vector3 start
            , Vector3 end
            , float headScale)
        {
            if (Vector3.SqrMagnitude(end - start) < Epsilon * Epsilon)
                return;

            Vector3 az = (end - start).normalized;
            Vector3 ax = Vector3.Cross(Vector3.up, az);
            // Vector3 ay = Vector3.Cross(az, ax).normalized;

            GL.Vertex(start);
            GL.Vertex3(
                  start.x + az.x * headScale + ax.x * headScale / 3
                , start.y + az.y * headScale + ax.y * headScale / 3
                , start.z + az.z * headScale + ax.z * headScale / 3);

            GL.Vertex(start);
            GL.Vertex3(
                  start.x + az.x * headScale - ax.x * headScale / 3
                , start.y + az.y * headScale - ax.y * headScale / 3
                , start.z + az.z * headScale - ax.z * headScale / 3);
        	 
        }

        /// <summary>
        /// Draws the provided triangle mesh in a manner suitable for 
        /// debug visualizations. 
        /// (Wiremesh with a partially transparent surface.)
        /// </summary>
        /// <remarks>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="vertices">The vertices. 
        /// [(x, y, z) * vertexCount]</param>
        /// <param name="triangles">The triangle indices. 
        /// [(vertAIndex, vertBIndex, vertCIndex) * triangleCount)</param>
        /// <param name="drawColor">The color of the mesh. (Alpha is ignored.)
        /// </param>
        [System.Obsolete("Use the other overload.")]
        public static void TriangleMesh(float[] verts
            , int[] tris
            , Color color)
        {
            TriangleMesh(verts, tris, tris.Length / 3, color);
        }

        /// <summary>
        /// Draws the provided triangle mesh in a manner suitable for 
        /// debug visualizations. 
        /// (Wiremesh with a partially transparent surface.)
        /// </summary>
        /// <remarks>
        /// <para>This method uses GL.  So it should usually be called within 
        /// OnRenderObject().</para>
        /// </remarks>
        /// <param name="vertices">The vertices. 
        /// [(x, y, z) * vertexCount]</param>
        /// <param name="triangles">The triangle indices. 
        /// [(vertAIndex, vertBIndex, vertCIndex) * 
        /// <typeparamref name="triCount"/>)</param>
        /// <param name="triCount">The number of triangles.</param>
        /// <param name="drawColor">The color of the mesh. (Alpha is ignored.)
        /// </param>
        public static void TriangleMesh(float[] verts
            , int[] tris
            , int triCount
            , Color color)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            int length = triCount * 3;

            color.a = 0.25f;

            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int p = 0; p < length; p += 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    GL.Vertex3(verts[tris[p + i] * 3 + 0]
                        , verts[tris[p + i] * 3 + 1]
                        , verts[tris[p + i] * 3 + 2]);
                }
            }
            GL.End();

            color.a = 0.4f;

            for (int p = 0; p < length; p += 3)
            {
                GL.Begin(GL.LINES);
                GL.Color(color);
                for (int i = 0; i < 3; i++)
                {
                    GL.Vertex3(verts[tris[p + i] * 3 + 0]
                        , verts[tris[p + i] * 3 + 1]
                        , verts[tris[p + i] * 3 + 2]);
                }
                GL.Vertex3(verts[tris[p] * 3 + 0]
                    , verts[tris[p] * 3 + 1]
                    , verts[tris[p] * 3 + 2]);
                GL.End();
            }
        }

        public static void Square(Vector3 origin
            , float width
            , float depth
            , Color color
            , bool fill)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            Vector3 a = origin;
            Vector3 b = origin;
            Vector3 c = origin;
            Vector3 d = origin;

            b.x += width;
            c.x += width;
            c.z += depth;
            d.z += depth;

            GL.Vertex(a);
            GL.Vertex(b);
            GL.Vertex(b);
            GL.Vertex(c);
            GL.Vertex(c);
            GL.Vertex(d);
            GL.Vertex(d);
            GL.Vertex(a);

            GL.End();

            if (fill)
            {
                GL.Begin(GL.TRIANGLES);

                color.a = 0.4f;
                GL.Color(color);

                GL.Vertex(a);
                GL.Vertex(b);
                GL.Vertex(c);

                GL.Vertex(a);
                GL.Vertex(c);
                GL.Vertex(d);

                GL.End();
            }
        }

        public static void Grid(Vector3 origin
            , float gridSize
            , int width
            , int depth
            , Color color)
        {
            GLUtil.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int x = 0; x < width + 1; x++)
            {
                Vector3 a = origin;

                a.x += x * gridSize;
                GL.Vertex(a);

                a.z += depth * gridSize;
                GL.Vertex(a);
            }

            for (int z = 0; z < depth + 1; z++)
            {
                Vector3 a = origin;

                a.z += z * gridSize;
                GL.Vertex(a);

                a.x += width * gridSize;
                GL.Vertex(a);
            }

            GL.End();
        }
    }
}
