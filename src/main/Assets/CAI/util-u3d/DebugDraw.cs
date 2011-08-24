using UnityEngine;

namespace org.critterai.u3d
{
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
                        // Debug.Log(mDir[i * 2]);
                    }
                }
                return mDir;
            }
        }

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

        public static void Extents(Vector3 position
            , float[] extents
            , Color color)
        {
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
    }
}
