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
using org.critterai.nmgen;
using org.critterai.nav;

namespace org.critterai.nmbuild
{
    public sealed class TileBuildTask
        : BuildTask<TileBuildAssets>
    {
        private readonly int mTileX;
        private readonly int mTileZ;
        private readonly bool mIsThreadSafe;
        private PolyMeshData mPolyData;
        private PolyMeshDetailData mDetailData;
        private ConnectionSet mConnections;

        public override bool IsThreadSafe { get { return mIsThreadSafe; } }

        public int TileX { get { return mTileX; } }
        public int TileZ { get { return mTileZ; } }

        private TileBuildTask(int tx, int tz
            , PolyMeshData polyData
            , PolyMeshDetailData detailData
            , ConnectionSet connections
            , bool isThreadSafe
            , int priority)
            : base(priority)
        {
            mTileX = tx;
            mTileZ = tz;
            mPolyData = polyData;
            mDetailData = detailData;
            mConnections = connections;
            mIsThreadSafe = isThreadSafe;
        }

        // Will only accept builders marked at threadsafe.
        public static TileBuildTask Create(int tx, int tz
            , PolyMeshData polyData
            , PolyMeshDetailData detailData
            , ConnectionSet conns
            , bool isThreadSafe
            , int priority)
        {
            if (tx < 0 || tz < 0
                || polyData == null || polyData.polyCount == 0
                || conns == null)
            {
                return null;
            }

            return new TileBuildTask(tx, tz, polyData, detailData, conns, isThreadSafe, priority);
        }

        protected override bool LocalRun() 
        { 
            // All the work is done in GetResult().
            return false; 
        }

        protected override bool GetResult(out TileBuildAssets result)
        {
            BuildContext logger = new BuildContext();

            result = new TileBuildAssets();

            NavmeshTileBuildData tbd =
                NMBuild.GetBuildData(mTileX, mTileZ
                , mPolyData
                , (mDetailData == null ? null : mDetailData)
                , mConnections
                , logger);

            AddMessages(logger.GetMessages());

            if (tbd == null)
                return false;

            NavmeshTileData td = new NavmeshTileData(tbd);

            if (td.Size == 0)
            {
                AddMessage(string.Format(
                    "Could not create {2} object. Cause unknown."
                    + " Tile: ({0},{1})"
                    , mTileX, mTileZ, td.GetType().Name));

                return false;
            }

            result = new TileBuildAssets(mTileX, mTileZ, td, tbd.PolyCount);

            return true;
        }

        protected override void FinalizeTask()
        {
            mPolyData = null;
            mDetailData = null;
            mConnections = null;
        }
    }
}
