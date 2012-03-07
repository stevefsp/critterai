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
    public struct NMGenAssets
    {
        private readonly int tileX;
        private readonly int tileZ;

        private readonly PolyMesh mPolyMesh;
        private readonly PolyMeshDetail mDetailMesh;

        private readonly Heightfield mHeightfield;
        private readonly CompactHeightfield mCompactField;
        private readonly ContourSet mContours;

        public int TileX { get { return tileX; } }
        public int TileZ { get { return tileZ; } } 
        public PolyMesh PolyMesh { get { return mPolyMesh; } }
        public PolyMeshDetail DetailMesh { get { return mDetailMesh; } }
        public Heightfield Heightfield { get { return mHeightfield; } }
        public CompactHeightfield CompactField { get { return mCompactField; } }
        public ContourSet Contours { get { return mContours; } } 

        public bool NoResult { get { return (mPolyMesh == null); } }

        public NMGenAssets(int tx, int tz
            , PolyMesh polyMesh
            , PolyMeshDetail detailMesh
            , Heightfield heightfield
            , CompactHeightfield compactField
            , ContourSet contours)
        {
            tileX = tx;
            tileZ = tz;

            if (polyMesh == null || polyMesh.PolyCount == 0)
            {
                mPolyMesh = null;
                mDetailMesh = null;
                mHeightfield = null;
                mCompactField = null;
                mContours = null;
            }
            else
            {
                mPolyMesh = polyMesh;
                mDetailMesh = detailMesh;  // OK to be null.
                mHeightfield = heightfield;
                mCompactField = compactField;
                mContours = contours;
            }
        }

        public NMGenAssets(int tx, int tz, PolyMesh polyMesh, PolyMeshDetail detailMesh)
        {
            tileX = tx;
            tileZ = tz;

            if (polyMesh == null || polyMesh.PolyCount == 0)
            {
                mPolyMesh = null;
                mDetailMesh = null;
            }
            else
            {
                mPolyMesh = polyMesh;
                mDetailMesh = detailMesh;  // OK to be null.
            }

            mHeightfield = null;
            mCompactField = null;
            mContours = null;
        }
    }
}
