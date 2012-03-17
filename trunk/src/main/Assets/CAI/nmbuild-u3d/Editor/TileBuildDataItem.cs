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
using org.critterai.nav;
using org.critterai.nmgen;

namespace org.critterai.nmbuild
{
    [System.Serializable]
	public sealed class BuildDataItem
	{
        /* Design notes: 
         * 
         * Odd design due to the need to support Unity serialization.
         * 
         */

        public const int EmptyId = -1;
        public const int ErrorId = -2;
        public const int QueuedId = -3;
        public const int InProgressId = -4;

        public int tileX;
        public int tileZ;

        // Arrays are set to zero length at construction.
        public byte[] polyMesh;
        public byte[] detailMesh;

        public byte[] bakedTile;
        public int bakedPolyCount;

        public byte[] workingTile;
        public int workingPolyCount;

        public TileBuildState TileState
        {
            get 
            {
                if (workingPolyCount < 0)
                {
                    // Special informational state.
                    switch (workingPolyCount)
                    {
                        case EmptyId:
                            return TileBuildState.Empty;
                        case QueuedId:
                            return TileBuildState.Queued;
                        case InProgressId:
                            return TileBuildState.InProgress;
                        case ErrorId:
                            return TileBuildState.Error;
                    }
                }

                if (workingTile.Length == 0)
                {
                    if (bakedTile.Length == 0)
                        return TileBuildState.NotBuilt;
                    else
                        return TileBuildState.Baked;
                }
                
                return TileBuildState.Built;
            }
        }

        internal BuildDataItem(int tx, int tz)
        {
            tileX = tx;
            tileZ = tz;
            Reset();
        }

        public void SetAsEmpty()
        {
            ClearUnbaked();
            workingPolyCount = EmptyId;
        }

        public void ClearUnbaked()
        {
            polyMesh = new byte[0];
            detailMesh = new byte[0];

            workingTile = new byte[0];
            workingPolyCount = 0;  // This clears special states.
        }

        public void SetAsFailed()
        {
            ClearUnbaked();
            workingPolyCount = ErrorId;
        }

        public void SetAsQueued()
        {
            ClearUnbaked();
            workingPolyCount = QueuedId;
        }

        public void SetAsInProgress()
        {
            ClearUnbaked();
            workingPolyCount = InProgressId;
        }

        public void SetWorkingData(PolyMesh polyMesh, PolyMeshDetail detailMesh)
        {
            if (polyMesh == null
                || polyMesh.PolyCount == 0)
            {
                SetAsEmpty();
                return;
            }

            this.polyMesh = polyMesh.GetSerializedData(false);

            if (detailMesh == null)
                this.detailMesh = new byte[0];
            else
                this.detailMesh = detailMesh.GetSerializedData(false);
        }

        public void SetWorkingData(NavmeshTileData tile, int polyCount)
        {
            if (tile == null || polyCount <= 0)
            {
                SetAsEmpty();
                return;
            }

            workingTile = tile.GetData();
            workingPolyCount = polyCount;
        }

        //[System.Obsolete]
        //public void SetWorkingData(TileBuildResult source)
        //{
        //    if (source == null
        //        || source.NoResult
        //        || source.PolyMesh == null
        //        || source.Tile == null
        //        || source.PolyCount <= 0)
        //    {
        //        SetAsEmpty();
        //        return;
        //    }

        //    polyMesh = source.PolyMesh.GetSerializedData(false);
        //    workingTile = source.Tile.GetData();
        //    workingPolyCount = source.PolyCount;

        //    if (source.DetailMesh == null)
        //        detailMesh = new byte[0];
        //    else
        //        detailMesh = source.DetailMesh.GetSerializedData(false);
        //}

        public bool SetAsBaked(byte[] tile, int polyCount)
        {
            if (tile == null || tile.Length == 0 || polyCount <= 0)
            {
                return false;
            }

            ClearUnbaked();
            bakedTile = tile;
            bakedPolyCount = polyCount;
            
            return true;
        }

        public bool SetAsBaked()
        {
            if (workingTile.Length > 0)
            {
                bakedTile = workingTile;
                bakedPolyCount = workingPolyCount;
                ClearUnbaked();
                return true;
            }
            workingPolyCount = 0; // Clears all special states.
            return false;
        }

        public void Reset()
        {
            polyMesh = new byte[0];
            detailMesh = new byte[0];

            workingTile = new byte[0];
            workingPolyCount = 0;

            bakedTile = new byte[0];
            bakedPolyCount = 0;
        }
	}
}
