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
using org.critterai.geom;
using org.critterai.u3d;

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// Provides methods useful for debugging navigation.
    /// </summary>
    /// <remarks>
    /// <para>All draw methods use GL.  So they should generally 
    /// be called from within the OnRenderObject() method.</para></remarks>
    public static class NavDebug
    {
        /// <summary>
        /// The color to use when drawing corner visualizations.
        /// </summary>
        public static Color cornerColor = new Color(Color.blue.r
            , Color.blue.g
            , Color.blue.b
            , 0.66f);

        /// <summary>
        /// The scale to use for corner markers.
        /// </summary>
        public static float cornerScale = 0.2f;

        /// <summary>
        /// The color to use when drawing position visualizations.
        /// </summary>
        public static Color positionColor = new Color(Color.blue.r
            , Color.blue.g
            , Color.blue.b
            , 0.5f);

        /// <summary>
        /// The scale to use for position markers.
        /// </summary>
        public static float positionScale = 0.8f;

        /// <summary>
        /// The base color to use when drawing goal visualizations.
        /// </summary>
        public static Color goalColor = new Color(Color.green.r
            , Color.green.g
            , Color.green.b
            , 0.5f);

        /// <summary>
        /// The scale to use for goal marks.
        /// </summary>
        public static float goalScale = 0.8f;

        /// <summary>
        /// The color to use for drawing visualizations that overlay polygons.
        /// </summary>
        public static Color polygonOverlayColor = new Color(Color.yellow.r
            , Color.yellow.g
            , Color.yellow.b
            , 0.33f);

        /// <summary>
        /// Privides a human readable string representing the provided tile
        /// header.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns>A string representing the tile header.</returns>
        public static string ToString(NavmeshTileHeader header)
        {
            return string.Format("Tile: X: {2}, Z: {3}, Layer: {4}\n"
                + "Version: {1}, UserId: {5}\n"
                + "Polys: {6}, Verts: {7}\n"
                + "Detail: Meshes: {9}, Tris: {11}, Verts: {10}\n"
                + "Conns: {13}, ConnBase: {14}\n"
                + "Walkable: Height: {15}, Radius: {16}, Step: {17}\n"
                + "Bounds: Min: {18}, Max: {19}\n"
                + "MaxLinks: {8}, BVQuantFactor: {20}, BVNodes: {12}\n"
                + "Magic: {0}\n" 
                , header.magic, header.version
                , header.x, header.z, header.layer
                , header.userId
                , header.polyCount, header.vertCount
                , header.maxLinkCount
                , header.detailMeshCount
                , header.detailVertCount
                , header.detailTriCount
                , header.bvNodeCount
                , header.connCount, header.connBase
                , header.walkableHeight
                , header.walkableRadius
                , header.walkableStep
                , Vector3Util.ToString(header.boundsMin)
                , Vector3Util.ToString(header.boundsMax)
                , header.bvQuantFactor);
        }

        /// <summary>
        /// Draws a debug visualization of the navigation mesh.
        /// </summary>
        /// <param name="mesh">The mesh to draw.</param>
        public static void Draw(Navmesh mesh)
        {
            int count = mesh.GetMaxTiles();
            for (int i = 0; i < count; i++)
            {
                Draw(mesh.GetTile(i), null, null, 0, i);
            }
        }

        /// <summary>
        /// Draws a debug visualization of the navigation mesh with
        /// the closed nodes highlighted.
        /// </summary>
        /// <param name="mesh">The mesh to draw.</param>
        /// <param name="query">The query which provides the list of closed 
        /// nodes.</param>
        public static void Draw(Navmesh mesh, NavmeshQuery query)
        {
            int count = mesh.GetMaxTiles();
            for (int i = 0; i < count; i++)
            {
                Draw(mesh.GetTile(i), query, null, 0, i);
            }
        }
        
        /// <summary>
        /// Draws a debug visualization of the navigation mesh with the
        /// specified polygons highlighted.
        /// </summary>
        /// <param name="mesh">The mesh to draw.</param>
        /// <param name="markPolys">The reference ids of the polygons that
        /// should be highlighted.</param>
        /// <param name="polyCount">The number of polygons in the
        /// <paramref name="markPolys"/> array.</param>
        public static void Draw(Navmesh mesh, uint[] markPolys, int polyCount)
        {
            int count = mesh.GetMaxTiles();
            for (int i = 0; i < count; i++)
            {
                Draw(mesh.GetTile(i)
                    , null
                    , markPolys
                    , polyCount
                    , i);
            }
        }


        /// <summary>
        /// Draws a debug visualization of corner data.
        /// </summary>
        /// <param name="corners">The corners to draw.</param>
        public static void Draw(CornerData corners)
        {
            if (corners.cornerCount == 0)
                return;

            GLUtil.SimpleMaterial.SetPass(0);

            for (int i = 0; i < corners.cornerCount; i++)
            {
                DebugDraw.XMarker(Vector3Util.GetVector(corners.verts, i)
                    , cornerScale, cornerColor);
            }
        }

        /// <summary>
        /// Draws a debug visualization of a corridor.
        /// </summary>
        /// <param name="mesh">The navigation mesh associated with the corridor.
        /// </param>
        /// <param name="corridor">The corridor to draw.</param>
        public static void Draw(Navmesh mesh
            , PathCorridorData corridor)
        {
            if (corridor.pathCount == 0)
                return;

            GLUtil.SimpleMaterial.SetPass(0);

            float[] tileVerts = null;

            for (int iPoly = 0; iPoly < corridor.pathCount; iPoly++)
            {
                NavmeshTile tile;
                NavmeshPoly poly;
                mesh.GetTileAndPoly(corridor.path[iPoly], out tile, out poly);

                if (poly.Type == NavmeshPolyType.OffMeshConnection)
                    continue;

                NavmeshTileHeader header = tile.GetHeader();
                if (tileVerts == null
                    || tileVerts.Length < 3 * header.vertCount)
                {
                    // Resize.
                    tileVerts = new float[3 * header.vertCount];
                }

                tile.GetVerts(tileVerts);

                GL.Begin(GL.TRIANGLES);
                GL.Color(polygonOverlayColor);

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
            DebugDraw.XMarker(v, positionScale, positionColor);
            DebugDraw.Circle(v, positionScale, positionColor);
            DebugDraw.Circle(v, positionScale * 0.5f, positionColor);
            DebugDraw.Circle(v, positionScale * 0.25f, positionColor);

            v = Vector3Util.GetVector(corridor.target);
            DebugDraw.XMarker(v, goalScale, goalColor);
            DebugDraw.Circle(v, goalScale, goalColor);
            DebugDraw.Circle(v, goalScale * 0.5f, goalColor);
            DebugDraw.Circle(v, goalScale * 0.25f, goalColor);
        }

        /// <summary>
        /// Draws a debug visualization of an individual navmesh tile.
        /// </summary>
        /// <remarks>
        /// <para>The tile will be checked to see if it is in use before it is
        /// drawn.  So there is no need for client to do so.</para></remarks>
        private static void Draw(NavmeshTile tile
            , NavmeshQuery query
            , uint[] markPolys
            , int polyCount
            , int colorId)
        {
            NavmeshTileHeader header = tile.GetHeader();

            // Keep this check.  Less trouble for clients.
            if (header.polyCount < 1)
                return;

            GLUtil.SimpleMaterial.SetPass(0);

            uint polyBase = tile.GetBasePolyRef();

            NavmeshPoly[] polys = new NavmeshPoly[header.polyCount];
            tile.GetPolys(polys);
             
            float[] verts = new float[header.vertCount * 3];
            tile.GetVerts(verts);

            NavmeshDetailMesh[] meshes = 
                new NavmeshDetailMesh[header.detailMeshCount];
            tile.GetDetailMeshes(meshes);

            byte[] detailTris = new byte[header.detailTriCount * 4];
            tile.GetDetailTris(detailTris);

            float[] detailVerts = new float[header.detailVertCount * 3];
            tile.GetDetailVerts(detailVerts);

            float alpha = polygonOverlayColor.a;

            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < header.polyCount; i++)
            {
                NavmeshPoly poly = polys[i];

                if (poly.Type == NavmeshPolyType.OffMeshConnection)
                    continue;

                NavmeshDetailMesh mesh = meshes[i];

                Color color;
                uint polyRef = polyBase | (uint)i;
                if ((query != null && query.IsInClosedList(polyRef))
                    || IsInList(polyRef, markPolys, polyCount) != -1)
                {
                    color = polygonOverlayColor;
                }
                else
                {
                    if (colorId == -1)
                    {
                        if (poly.Area == 0)
                            color = new Color(0, 0.75f, 1, alpha);
                        else
                            color = ColorUtil.IntToColor(poly.Area, alpha);
                    }
                    else
                        color = ColorUtil.IntToColor(colorId, alpha);
                }

                GL.Color(color);

                for (int j = 0; j < mesh.triCount; j++)
                {
                    int pTri = (int)(mesh.triBase + j) * 4;

                    for (int k = 0; k < 3; k++)
                    {
                        // Note: iVert and pVert refer to different
                        // arrays.
                        int iVert = detailTris[pTri + k];
                        if (iVert < poly.vertCount)
                        {
                            int pVert = poly.indices[iVert] * 3;
                            GL.Vertex3(verts[pVert + 0]
                                , verts[pVert + 1]
                                , verts[pVert + 2]);
                        }
                        else
                        {
                            int pVert = (int)
                                (mesh.vertBase + iVert - poly.vertCount) * 3;
                            GL.Vertex3(detailVerts[pVert + 0]
                                , detailVerts[pVert + 1]
                                , detailVerts[pVert + 2]);
                        }
                    }
                }
            }
            GL.End();

            NavmeshLink[] links = new NavmeshLink[header.maxLinkCount];
            tile.GetLinks(links);

            DrawPolyBoundaries(header
                , polys
                , verts
                , meshes
                , detailTris
                , detailVerts
                , links
                , new Color(0, 0.2f, 0.25f, 0.13f)
                , true);

            DrawPolyBoundaries(header
                , polys
                , verts
                , meshes
                , detailTris
                , detailVerts
                , links
                , new Color(0.65f, 0.2f, 0, 0.9f)
                , false);

            // TODO: Add code to draw off-mesh connections
            // once the demo is implemented.

        }

        /// <summary>
        /// Returns the index of the polygon reference within the list, or
        /// -1 if it was not found.
        /// </summary>
        private static int IsInList(uint polyRef
            , uint[] polyList
            , int polyCount)
        {
            if (polyList == null)
                return -1;

            for (int i = 0; i < polyCount; i++)
            {
                if (polyList[i] == polyRef)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Draws the polygon boundary lines based on the height detail.
        /// </summary>
        private static void DrawPolyBoundaries(NavmeshTileHeader header
            , NavmeshPoly[] polys
            , float[] verts
            , NavmeshDetailMesh[] meshes
            , byte[] detailTris
            , float[] detailVerts
            , NavmeshLink[] links
            , Color color
            , bool inner)
        {
            const float thr = 0.01f * 0.01f;

            GL.Begin(GL.LINES);
            for (int i = 0; i < header.polyCount; i++)
            {
                NavmeshPoly poly = polys[i];

                if (poly.Type == NavmeshPolyType.OffMeshConnection)
                    continue;

                NavmeshDetailMesh mesh = meshes[i];
                float[] tv = new float[9];

                for (int j = 0, nj = (int)poly.vertCount; j < nj; j++)
                {
                    Color c = color;  // Color may change.
                    if (inner)
                    {
                        if (poly.neighborPolyRefs[j] == 0)
                            continue;
                        if ((poly.neighborPolyRefs[j]
                            & Navmesh.ExternalLink) != 0)
                        {
                            bool con = false;
                            for (uint k = poly.firstLink
                                ; k != Navmesh.NullLink
                                ; k = links[k].next)
                            {
                                if (links[k].edge == j)
                                {
                                    con = true;
                                    break;
                                }
                            }
                            if (con)
                                c = new Color(1, 1, 1, 0.2f);
                            else
                                c = new Color(0, 0, 0, 0.2f);
                        }
                        else
                            c = new Color(0, 0.2f, 0.25f, 0.13f);
                    }
                    else
                    {
                        if (poly.neighborPolyRefs[j] != 0)
                            continue;
                    }

                    GL.Color(c);

                    int pVertA = poly.indices[j] * 3;
                    int pVertB = poly.indices[(j + 1) % nj] * 3;

                    for (int k = 0; k < mesh.triCount; k++)
                    {
                        int pTri = (int)((mesh.triBase + k) * 4);
                        for (int m = 0; m < 3; m++)
                        {
                            int iVert = detailTris[pTri + m];
                            if (iVert < poly.vertCount)
                            {
                                int pv = poly.indices[iVert] * 3;
                                tv[m * 3 + 0] = verts[pv + 0];
                                tv[m * 3 + 1] = verts[pv + 1];
                                tv[m * 3 + 2] = verts[pv + 2];
                            }
                            else
                            {
                                int pv = (int)(mesh.vertBase 
                                    + (iVert - poly.vertCount)) * 3;
                                tv[m * 3 + 0] = detailVerts[pv + 0];
                                tv[m * 3 + 1] = detailVerts[pv + 1];
                                tv[m * 3 + 2] = detailVerts[pv + 2];
                            }
                        }
                        for (int m = 0, n = 2; m < 3; n = m++)
                        {
                            if (((detailTris[pTri + 3] >> (n * 2)) & 0x3) == 0)
                                // Skip inner detail edges.
                                continue;
                            float distN = Line2.GetPointLineDistanceSq(
                                tv[n * 3 + 0], tv[n * 3 + 2]
                                , verts[pVertA + 0], verts[pVertA + 2]
                                , verts[pVertB + 0], verts[pVertB + 2]);
                            float distM = Line2.GetPointLineDistanceSq(
                                tv[m * 3 + 0], tv[m * 3 + 2]
                                , verts[pVertA + 0], verts[pVertA + 2]
                                , verts[pVertB + 0], verts[pVertB + 2]);
                            if (distN < thr && distM < thr)
                            {
                                GL.Vertex3(tv[n * 3 + 0]
                                    , tv[n * 3 + 1]
                                    , tv[n * 3 + 2]);
                                GL.Vertex3(tv[m * 3 + 0]
                                    , tv[m * 3 + 1]
                                    , tv[m * 3 + 2]);
                            }
                        }
                    }
                }
            }
            GL.End();
        }

        /// <summary>
        /// Returns the 3D centroids of the provided navigation mesh polygons.
        /// </summary>
        /// <remarks>
        /// <para>If a polygon does not exist within the mesh, its associated
        /// centroid will not be altered.  So some centroid data will be
        /// invalid if <paramref name="polyCount"/> is not equal to the result
        /// count.</para>
        /// </remarks>
        /// <param name="mesh">The navigation mesh containing the polygons.
        /// </param>
        /// <param name="polyRefs">The reference id's of the polygons.</param>
        /// <param name="polyCount">The number of polygons.</param>
        /// <param name="centroids">The centroids for the polygons.
        /// [Length: >= polyCount] (Out)</param>
        /// <returns>The actual number of polygons found within the mesh.
        /// </returns>
        public static int GetCentroids(Navmesh mesh
            , uint[] polyRefs
            , int polyCount
            , Vector3[] centroids)
        {
            int resultCount = 0;
            int count = mesh.GetMaxTiles();
            for (int i = 0; i < count; i++)
            {
                resultCount += GetCentroids(mesh.GetTile(i)
                    , polyRefs
                    , polyCount
                    , centroids);
                if (resultCount == polyRefs.Length)
                    break;
            }
            return resultCount;
        }

        /// <summary>
        /// Gets the centroids for the polygons that are part of the tile.
        /// </summary>
        private static int GetCentroids(NavmeshTile tile
            , uint[] polyRefs
            , int polyCount
            , Vector3[] centroids)
        {
            NavmeshTileHeader header = tile.GetHeader();

            if (header.polyCount < 1)
                return 0;

            uint polyBase = tile.GetBasePolyRef();

            NavmeshPoly[] polys = new NavmeshPoly[header.polyCount];
            tile.GetPolys(polys);

            float[] verts = new float[header.vertCount * 3];
            tile.GetVerts(verts);

            int resultCount = 0;

            for (int i = 0; i < header.polyCount; i++)
            {
                uint polyRef = polyBase | (uint)i;

                int iResult = IsInList(polyRef, polyRefs, polyCount);

                if (iResult == -1)
                    continue;

                resultCount++;

                NavmeshPoly poly = polys[i];

                centroids[iResult] = GetCentroid(verts, poly.indices, poly.vertCount);
            }

            return resultCount;
        }

        /// <summary>
        /// Gets the centroid for a polygon.
        /// </summary>
        private static Vector3 GetCentroid(float[] verts
            , ushort[] indices
            , int vertCount)
        {
            // Reference:
            // http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
            Vector3 result = new Vector3();

            for (int i = 0; i < vertCount; i++)
            {
                int p = (ushort)indices[i] * 3;
                result.x += verts[p];
                result.y += verts[p + 1];
                result.z += verts[p + 2];
            }

            result.x /= vertCount;
            result.y /= vertCount;
            result.z /= vertCount;

            return result;
        }
    }
}
