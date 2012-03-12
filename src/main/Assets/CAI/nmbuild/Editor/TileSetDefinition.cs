/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using org.critterai.nmgen;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    public sealed class TileSetDefinition
    {
        private readonly InputGeometry mGeometry;
        private readonly NMGenParams mBaseConfig;
        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;

        private int mWidth;
        private int mDepth;

        public int Width { get { return mWidth; } }
        public int Depth { get { return mDepth; } }
        public Vector3 BoundsMin { get { return mBoundsMin; } }
        public Vector3 BoundsMax { get { return mBoundsMax; } }
        public InputGeometry Geometry { get { return mGeometry; } } 

        private TileSetDefinition(int width, int depth
            , Vector3 bmin, Vector3 bmax
            , NMGenParams config
            , InputGeometry geom)
        {
            // Note: The constructor is private, which is why
            // the references are being stored.
            mBaseConfig = config.Clone();
            mGeometry = geom;
            mWidth = width;
            mDepth = depth;
            mBoundsMin = bmin;
            mBoundsMax = bmax;
        }

        public NMGenParams GetBaseConfig()
        {
            return mBaseConfig.Clone();
        }

        public float TileWorldSize { get { return mBaseConfig.TileWorldSize; } }

        public bool GetTileBounds(int tx, int tz
            , out Vector3 boundsMin, out Vector3 boundsMax)
        {
            boundsMin = mBoundsMin;
            boundsMax = mBoundsMin;

            if (tx < 0 || tz < 0 || tx >= mWidth || tz >= mDepth)
            {
                boundsMin = Vector3Util.Zero;
                boundsMax = Vector3Util.Zero;
                return false;
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

            return true;
        }

        public static TileSetDefinition Create(Vector3 boundsMin
            , Vector3 boundsMax
            , NMGenParams config
            , InputGeometry geom)
        {
            if (config == null || !config.IsValid()
                || !TriangleMesh.IsBoundsValid(boundsMin, boundsMax)
                || geom == null
                || config.tileSize <= 0)
            {
                return null;
            }

            int w;
            int d;

            NMGen.DeriveSizeOfTileGrid(boundsMin, boundsMax
                , config.XZCellSize
                , config.tileSize
                , out w, out d);

            if (w < 1 || d < 1)
                return null;

            return new TileSetDefinition(w, d
                , boundsMin, boundsMax
                , config
                , geom);
        }
    }
}
