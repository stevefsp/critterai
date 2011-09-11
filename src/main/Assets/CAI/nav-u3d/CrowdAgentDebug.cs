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
using org.critterai.u3d;
using UnityEngine;

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// Provides debug visualizations for <see cref="CrowdAgent"/> objects.
    /// </summary>
    /// <remarks>
    /// <para>All draw methods in this class use GL.  So they should generally 
    /// be called from within the OnRenderObject() method.</para>
    /// <para>The design of this class minimizes impact on garbage collection.
    /// </para>
    /// <para>Instances of this class are not thread-safe.</para>
    /// </remarks>
    public sealed class CrowdAgentDebug
    {
        /// <summary>
        /// The color to use when drawing neighbor visualizations.
        /// </summary>
        public static Color neighborColor = new Color(Color.yellow.r
            , Color.yellow.g
            , Color.yellow.b
            , 0.66f);

        /// <summary>
        /// The base color to use when drawing visualizations.
        /// </summary>
        public static Color baseColor = new Color(Color.blue.r
            , Color.blue.g
            , Color.blue.b
            , 0.66f);

        /// <summary>
        /// The color to use when drawing the agent velocity.
        /// </summary>
        public static Color velocityColor = new Color(Color.blue.r
            , Color.blue.g
            , Color.blue.b
            , 0.66f);

        /// <summary>
        /// the color to use when drawing the agent desired velocity.
        /// </summary>
        public static Color desiredVelocityColor = new Color(Color.cyan.r
            , Color.cyan.g
            , Color.cyan.b
            , 0.5f);

        /// <summary>
        /// The color to use when drawing corridor boundary visualizations.
        /// </summary>
        public static Color boundaryColor = new Color(Color.yellow.r
            , Color.yellow.g
            , Color.yellow.b
            , 0.66f);

        /// <summary>
        /// The color to use when drawing corner visualizations.
        /// </summary>
        public static Color cornerColor = new Color(Color.blue.r
            , Color.blue.g
            , Color.blue.b
            , 0.66f);

        /// <summary>
        /// the color to use when drawing corridor visualizations.
        /// </summary>
        public static Color corridorColor = new Color(Color.yellow.r
            , Color.yellow.g
            , Color.yellow.b
            , 0.33f);

        private Navmesh navmesh;

        // Various buffers.  (Reduces GC impact.)
        private CrowdNeighbor[] neighbors = 
            new CrowdNeighbor[CrowdNeighbor.MaxNeighbors];
        private LocalBoundaryData boundary = new LocalBoundaryData();
        private CrowdCornerData corners = new CrowdCornerData();
        private PathCorridorData corridor = new PathCorridorData();
        private float[] tileVerts;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="navmesh">The navigation mesh used by the agents.
        /// </param>
        public CrowdAgentDebug(Navmesh navmesh) 
        {
            this.navmesh = navmesh;
            tileVerts = new float[3 * navmesh.GetTile(0).GetHeader().vertCount];
        }

        /// <summary>
        /// Draws all agent debug information.
        /// </summary>
        /// <param name="agent">The agent to draw.</param>
        public void DrawAll(CrowdAgent agent)
        {
            // Order matters.
            DrawCorridor(agent);
            DrawNeighbors(agent);
            DrawLocalBoundary(agent);
            DrawCorners(agent);
            DrawBase(agent);
        }

        /// <summary>
        /// Draws the basic agent debug information.
        /// </summary>
        /// <param name="agent">The agent to draw.</param>
        public void DrawBase(CrowdAgent agent)
        {
            Vector3 pos = Vector3Util.GetVector(agent.Position);
            CrowdAgentParams config = agent.GetConfig();
            
            DebugDraw.Circle(pos, config.radius, baseColor);

            Vector3 v = Vector3Util.GetVector(agent.DesiredVelocity);
            DebugDraw.Arrow(pos + Vector3.up * config.height
                , pos + v + Vector3.up * config.height
                , 0, 0.05f, desiredVelocityColor);

            v = Vector3Util.GetVector(agent.Velocity);
            DebugDraw.Arrow(pos + Vector3.up * config.height
                , pos + v + Vector3.up * config.height
                , 0, 0.05f, velocityColor);
        }

        /// <summary>
        /// Draws agent neighbor information.
        /// </summary>
        /// <param name="agent">The agent to draw.</param>
        public void DrawNeighbors(CrowdAgent agent)
        {
            int neighborCount = agent.NeighborCount;

            if (neighborCount == 0)
                return;

            agent.GetNeighbors(neighbors);

            Vector3 pos = Vector3Util.GetVector(agent.Position);
            for (int i = 0; i < neighborCount; i++)
            {
                CrowdAgent n = agent.GetNeighbor(neighbors[i]);
                if (n == null)
                    // Not sure why this happens.  Bug in CrowdAgent?
                    continue;
                Vector3 npos = Vector3Util.GetVector(n.Position);
                DebugDraw.Arrow(pos, npos, 0, 0.05f, neighborColor);
                DebugDraw.Circle(npos, agent.GetConfig().radius, neighborColor);
            }
        }

        /// <summary>
        /// Draws agent local boundary information.
        /// </summary>
        /// <param name="agent">The agent to draw.</param>
        public void DrawLocalBoundary(CrowdAgent agent)
        {
            agent.GetBoundary(boundary);

            if (boundary.segmentCount == 0)
                return;

            DebugDraw.XMarker(Vector3Util.GetVector(boundary.center)
                , 0.1f, boundaryColor);

            GLUtil.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(boundaryColor);

            for (int i = 0; i < boundary.segmentCount; i++)
            {
                int p = i * 6;
                GL.Vertex3(boundary.segments[p + 0]
                    , boundary.segments[p + 1]
                    , boundary.segments[p + 2]);
                GL.Vertex3(boundary.segments[p + 3]
                    , boundary.segments[p + 4]
                    , boundary.segments[p + 5]);
            }

            GL.End();
        }

        /// <summary>
        /// Draws agent corner information.
        /// </summary>
        /// <param name="agent">The agent to draw.</param>
        public void DrawCorners(CrowdAgent agent)
        {
            agent.GetCornerData(corners);

            if (corners.cornerCount == 0)
                return;

            GLUtil.SimpleMaterial.SetPass(0);

            for (int i = 0; i < corners.cornerCount; i++)
            {
                DebugDraw.XMarker(Vector3Util.GetVector(corners.verts, i)
                    , 0.2f, cornerColor);
            }
        }

        /// <summary>
        /// Draws agent corridor information.
        /// </summary>
        /// <param name="agent">The agent to draw.</param>
        public void DrawCorridor(CrowdAgent agent)
        {
            agent.GetCorridor(corridor);

            if (corridor.pathCount == 0)
                return;

            GLUtil.SimpleMaterial.SetPass(0);

            for (int iPoly = 0; iPoly < corridor.pathCount; iPoly++ )
            {
                NavmeshTile tile;
                NavmeshPoly poly;
                navmesh.GetTileAndPoly(corridor.path[iPoly], out tile, out poly);

                if (poly.Type == NavmeshPolyType.OffMeshConnection)
                    continue;

                NavmeshTileHeader header = tile.GetHeader();
                if (tileVerts.Length < 3 * header.vertCount)
                    // Resize.
                    tileVerts = new float[3 * header.vertCount];

                tile.GetVerts(tileVerts);

                GL.Begin(GL.TRIANGLES);
                GL.Color(corridorColor);

                int pA = poly.indices[0] * 3;
                for (int i = 2; i < poly.vertCount; i++)
                {
                    int pB = poly.indices[i - 1] * 3;
                    int pC = poly.indices[i] * 3;

                    GL.Vertex3(tileVerts[pA + 0]
                        , tileVerts[pA + 1]
                        , tileVerts[pA + 2]);

                    GL.Vertex3(tileVerts[pB + 0]
                        , tileVerts[pB + 1]
                        , tileVerts[pB + 2]);

                    GL.Vertex3(tileVerts[pC + 0]
                        , tileVerts[pC + 1]
                        , tileVerts[pC + 2]);
                }
                GL.End();

                // Not drawing boundaries since it would obscure other agent
                // debug data.
            }

            Vector3 v = Vector3Util.GetVector(corridor.position);
            DebugDraw.XMarker(v, 0.8f, baseColor);

            v = Vector3Util.GetVector(corridor.target);
            DebugDraw.XMarker(v, 0.8f, baseColor);
            DebugDraw.Circle(v, 0.8f, baseColor);
            DebugDraw.Circle(v, 0.4f, baseColor);
            DebugDraw.Circle(v, 0.2f, baseColor);
        }
    }
}

