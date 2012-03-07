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
using System;
using System.Collections.Generic;
using org.critterai.geom;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// Defines a set of NMGen tiles and provides builders for its tiles.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public sealed class TileSetDefinition
    {
        private static int MaxTrianglesPerChunk = 2048;

        private readonly ChunkyTriMesh mChunkMesh;

        private readonly NMGenParams mBaseConfig;
        private readonly BuildFlags mBuildFlags;
        private readonly Vector3[] mVerts;
        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;

        private int mWidth;
        private int mDepth;

        public int Width { get { return mWidth; } }
        public int Depth { get { return mDepth; } }
        public Vector3 BoundsMin { get { return mBoundsMin; } }
        public Vector3 BoundsMax { get { return mBoundsMax; } }
        public BuildFlags BuildFlags { get { return mBuildFlags; } }

        private TileSetDefinition(int width, int depth
            , Vector3 bmin, Vector3 bmax
            , NMGenParams config
            , BuildFlags buildFlags
            , ChunkyTriMesh source
            , Vector3[] verts)
        {
            // Note: The constructor is private, which is why
            // the references are being stored.
            mBaseConfig = config.Clone();
            mChunkMesh = source;
            mVerts = verts;
            mBuildFlags = buildFlags;
            mWidth = width;
            mDepth = depth;
            mBoundsMin = bmin;
            mBoundsMax = bmax;
        }

        public NMGenParams GetBaseConfig()
        {
            return mBaseConfig.Clone();
        }

        public void GetTileBounds(int tx, int tz
            , out Vector3 boundsMin, out Vector3 boundsMax)
        {
            boundsMin = mBoundsMin;
            boundsMax = mBoundsMin;

            if (tx < 0 || tz < 0 || tx >= mWidth || tz >= mDepth)
            {
                boundsMin = Vector3Util.Zero;
                boundsMax = Vector3Util.Zero;
                return;
            }

            float tcsFactor = mBaseConfig.TileSize * mBaseConfig.XZCellSize;
            float borderOffset = mBaseConfig.BorderSize * mBaseConfig.XZCellSize;

            // Note: The minimum bounds of the base configuration is
            // considered to be the origin of the mesh set.

            boundsMin = mBoundsMin;
            boundsMax = mBoundsMin;  // This is not an error.

            boundsMin.x += tx * tcsFactor - borderOffset;
            boundsMin.z += tz * tcsFactor - borderOffset;

            boundsMax.x += (tx + 1) * tcsFactor + borderOffset;
            boundsMax.y = mBoundsMax.y;
            boundsMax.z += (tz + 1) * tcsFactor + borderOffset;
        }

        private void GetInputGeometryPartial(Vector3 bmin
            , Vector3 bmax
            , out TriangleMesh mesh
            , out byte[] areas)
        {
            List<ChunkyTriMeshNode> nodes = new List<ChunkyTriMeshNode>();

            mChunkMesh.GetChunks(bmin.x, bmin.z, bmax.x, bmax.z, nodes);

            int triCount = 0;

            foreach (ChunkyTriMeshNode node in nodes)
            {
                triCount += node.count;
            }

            if (triCount == 0)
            {
                mesh = new TriangleMesh();
                areas = new byte[0];
                return;
            }

            mesh = new TriangleMesh(mVerts.Length, triCount);
            mesh.triCount = triCount;
            mesh.vertCount = mVerts.Length;

            areas = new byte[triCount];

            int[] stris = mChunkMesh.Tris;
            byte[] sareas = mChunkMesh.Areas;

            int iTri = 0;
            foreach (ChunkyTriMeshNode node in nodes)
            {
                for (int i = 0; i < node.count; i++)
                {
                    mesh.tris[(iTri) * 3 + 0] = stris[(node.i + i) * 3 + 0];
                    mesh.tris[iTri * 3 + 1] = stris[(node.i + i) * 3 + 1];
                    mesh.tris[iTri * 3 + 2] = stris[(node.i + i) * 3 + 2];
                    areas[iTri] = sareas[node.i + i];
                    iTri++;
                }
            }
        }

        public void GetInputGeometry(Vector3 bmin, Vector3 bmax
            , out TriangleMesh mesh
            , out byte[] areas)
        {
            GetInputGeometryPartial(bmin, bmax, out mesh, out areas);
            mesh.verts = (Vector3[])mVerts.Clone();
        }

        public bool GetInputGeometry(int tx, int tz
            , out NMGenTileParams tileConfig
            , out InputGeometry geom)
        {
            if (tx < 0 || tz < 0 || tx >= mWidth || tz >= mDepth)
            {
                tileConfig = null;
                geom = null;
                return false;
            }

            // Note: The minimum bounds of the base configuration is
            // considered to be the origin of the mesh set.

            Vector3 bmin;
            Vector3 bmax;

            GetTileBounds(tx, tz, out bmin, out bmax);

            tileConfig = new NMGenTileParams(tx, tz, bmin, bmax);

            TriangleMesh mesh;
            byte[] areas;

            GetInputGeometryPartial(bmin, bmax, out mesh, out areas);

            if (mesh.triCount == 0)
            {
                // There is nothing to build.  This is NOT a failure
                // since it is expected that not all tiles in a multi-tile mesh
                // will contain geometry.
                geom = null;
                return true;
            }

            geom = InputGeometry.UnsafeCreate(mVerts, mesh.tris, areas);
            return true;
        }

        public static TileSetDefinition Create(Vector3 meshBoundsMin
            , Vector3 meshBoundsMax
            , NMGenParams config
            , BuildFlags buildFlags
            , InputGeometry geom)
        {
            if (config == null || !config.IsValid()
                || !Vector3Util.IsBoundsValid(meshBoundsMin, meshBoundsMax)
                || geom == null
                || config.tileSize < 0)
            {
                return null;
            }

            int w;
            int d;

            NMGen.DeriveSizeOfTileGrid(meshBoundsMin, meshBoundsMax
                , config.XZCellSize
                , config.tileSize
                , out w, out d);

            if (w < 1 || d < 1)
                return null;

            ChunkyTriMesh cmesh = new ChunkyTriMesh(geom.UnsafeVerts
                , geom.UnsafeTris
                , geom.UnsafeAreas
                , geom.UnsafeTris.Length / 3
                , MaxTrianglesPerChunk);

            Vector3[] verts = geom.UnsafeVerts;

            return new TileSetDefinition(w, d
                , meshBoundsMin, meshBoundsMax
                , config, buildFlags
                , cmesh, verts);
        }
    }
}
