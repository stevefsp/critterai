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
        public const int MinAllowedTileSize 
            = IncrementalBuilder.MinAllowedTileSize;

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

        public bool GetInputGeometry(int tx, int tz
            , out NMGenTileParams tileConfig
            , out NMGenInputGeom geom)
        {
            if (tx < 0 || tz < 0 || tx >= mWidth || tz >= mDepth)
            {
                tileConfig = null;
                geom = null;
                return false;
            }

            float tcsFactor = mBaseConfig.TileSize * mBaseConfig.XZCellSize;
            float borderOffset = mBaseConfig.BorderSize * mBaseConfig.XZCellSize;

            // Note: The minimum bounds of the base configuration is
            // considered to be the origin of the mesh set.

            Vector3 bmin = mBoundsMin;
            Vector3 bmax = mBoundsMin;  // This is not an error.

            bmin.x += tx * tcsFactor - borderOffset;
            bmin.z += tz * tcsFactor - borderOffset;

            bmax.x += (tx + 1) * tcsFactor + borderOffset;
            bmax.y = mBoundsMax.y;
            bmax.z += (tz + 1) * tcsFactor + borderOffset;

            tileConfig = new NMGenTileParams(tx, tz, bmin, bmax);

            List<ChunkyTriMeshNode> nodes = new List<ChunkyTriMeshNode>();

            mChunkMesh.GetChunks(bmin.x, bmin.z, bmax.x, bmax.z, nodes);

            int triCount = 0;

            foreach (ChunkyTriMeshNode node in nodes)
            {
                triCount += node.count;
            }

            if (triCount == 0)
            {
                // There is nothing to build.  This is NOT a failure
                // since it is expected that not all tiles in a multi-tile mesh
                // will contain geometry.
                geom = null;
                return true;
            }

            int[] stris = mChunkMesh.Tris;
            byte[] sareas = mChunkMesh.Areas;

            int[] ltris = new int[triCount * 3];
            byte[] lareas = new byte[triCount];

            int iTri = 0;
            foreach (ChunkyTriMeshNode node in nodes)
            {
                for (int i = 0; i < node.count; i++)
                {
                    ltris[(iTri) * 3 + 0] = stris[(node.i + i) * 3 + 0];
                    ltris[iTri * 3 + 1] = stris[(node.i + i) * 3 + 1];
                    ltris[iTri * 3 + 2] = stris[(node.i + i) * 3 + 2];
                    lareas[iTri] = sareas[node.i + i];
                    iTri++;
                }
            }

            geom = NMGenInputGeom.UnsafeCreate(mVerts, ltris, lareas);
            return true;
        }

        public static TileSetDefinition Create(Vector3 meshBoundsMin
            , Vector3 meshBoundsMax
            , NMGenParams config
            , BuildFlags buildFlags
            , NMGenInputGeom geom)
        {
            if (config == null || !config.IsValid()
                || !Vector3Util.IsBoundsValid(meshBoundsMin, meshBoundsMax)
                || geom == null
                || config.tileSize < MinAllowedTileSize)
            {
                return null;
            }

            int w;
            int d;

            NMGen.DeriveSizeOfTileGrid(meshBoundsMin, meshBoundsMax
                , config.XZCellSize
                , config.tileSize
                , out w, out d);

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
