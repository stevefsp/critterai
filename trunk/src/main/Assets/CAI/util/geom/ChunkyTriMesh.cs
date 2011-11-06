using System;
using System.Collections.Generic;

namespace org.critterai.geom
{
	public sealed class ChunkyTriMesh
	{
        private struct BoundsItem
        {
            public float xmin;
            public float zmin;
            public float xmax;
            public float zmax;
            public int i;
        }

        private class BoundsItemCompareX
            : IComparer<BoundsItem>
        {
            public int Compare(BoundsItem x, BoundsItem y)
            {
                if (x.xmin < y.xmin)
                    return -1;
                if (x.xmin > y.xmin)
                    return 1;
                return 0;
            }
        }

        private class BoundsItemCompareZ
            : IComparer<BoundsItem>
        {
            public int Compare(BoundsItem x, BoundsItem y)
            {
                if (x.zmin < y.zmin)
                    return -1;
                if (x.zmin > y.zmin)
                    return 1;
                return 0;
            }
        }

        public const int MinAllowedTrisPerChunk = 64;

        private static BoundsItemCompareX mCompareX = new BoundsItemCompareX();
        private static BoundsItemCompareZ mCompareZ = new BoundsItemCompareZ();

        private ChunkyTriMeshNode[] mNodes;
        private readonly int mNodeCount;
        private readonly int[] mTris;
        private readonly byte[] mAreas;
        private readonly int mTriCount;  // Convienience only.
        private readonly int mMaxTrisPerChunk;

        public int NodeCount { get { return mNodeCount; } }
        public int TriCount { get { return mTriCount; } }
        public int MaxTrisPerChunk { get { return mMaxTrisPerChunk; } }

        public int[] Triangles { get { return mTris; } }
        public byte[] Areas { get { return mAreas; } }

        public ChunkyTriMesh(float[] verts
            , int[] tris
            , byte[] areas
            , int triCount
            , int trisPerChunk)
        {
            trisPerChunk = Math.Max(MinAllowedTrisPerChunk, trisPerChunk);

            mTriCount = triCount;
            int nchunks = (mTriCount + trisPerChunk - 1) / trisPerChunk;

            mNodes = new ChunkyTriMeshNode[nchunks * 4];

            mTris = new int[mTriCount * 3];
            mAreas = new byte[mTriCount];

            // Build tree
            BoundsItem[] items = new BoundsItem[mTriCount];

            for (int i = 0; i < mTriCount; i++)
            {
                int pi = i * 3;
                // const int* t = &tris[i * 3];

                items[i].i = i;

                // Calc triangle XZ bounds.
                items[i].xmin = 
                    items[i].xmax = verts[tris[pi] * 3 + 0];
                items[i].zmin = 
                    items[i].zmax = verts[tris[pi] * 3 + 2];

                for (int j = 1; j < 3; j++)
                {
                    // const float* v = &verts[t[j] * 3];

                    float v = verts[tris[pi + j] * 3 + 0];

                    if (v < items[i].xmin)
                        items[i].xmin = v;

                    if (v > items[i].xmax)
                        items[i].xmax = v;

                    v = verts[tris[pi + j] * 3 + 2];

                    if (v < items[i].zmin)
                        items[i].zmin = v;

                    if (v > items[i].zmax)
                        items[i].zmax = v;
                }
            }

            int curTri = 0;
            int curNode = 0;
            Subdivide(items
                , 0
                , mTriCount
                , trisPerChunk
                , ref curNode
                , ref curTri
                , tris
                , areas);

            mNodeCount = curNode;

            // Calc max tris per node.
            mMaxTrisPerChunk = 0;
            for (int i = 0; i < mNodeCount; i++)
            {
                bool isLeaf = mNodes[i].i >= 0;
                if (!isLeaf) 
                    continue;

                if (mNodes[i].count > mMaxTrisPerChunk)
                    mMaxTrisPerChunk = mNodes[i].count;
            }
        }

        // float xmin, float zmin, float xmax, float zmax
        public void GetChunks(float xmin, float zmin, float xmax, float zmax
            , List<ChunkyTriMeshNode> resultNodes)
        {
            int i = 0;
            resultNodes.Clear();

            while (i < mNodeCount)
            {
                ChunkyTriMeshNode node = mNodes[i];

                bool overlap =  Overlaps(xmin, zmin, xmax, zmax, node);

                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                    resultNodes.Add(node);

                if (overlap || isLeafNode)
                    i++;
                else
                {
                    int escapeIndex = -node.i;
                    i += escapeIndex;
                }
            }
        }

        private void Subdivide(BoundsItem[] items
            , int imin
            , int imax
            , int trisPerChunk
            , ref int curNode
            , ref int curTri
            , int[] inTris
            , byte[] inAreas)
        {
            int inum = imax - imin;
            int icur = curNode;

            if (curNode > mNodes.Length)
                return;

            int inode = curNode++;

            if (inum <= trisPerChunk)
            {
                // Leaf
                DeriveExtents(items
                    , imin
                    , imax
                    , ref mNodes[inode]);

                // Copy triangles.
                mNodes[inode].i = curTri;
                mNodes[inode].count = inum;

                for (int i = imin; i < imax; ++i)
                {
                    int pi = items[i].i * 3;
                    int pd = curTri * 3;

                    mTris[pd + 0] = inTris[pi + 0];
                    mTris[pd + 1] = inTris[pi + 1];
                    mTris[pd + 2] = inTris[pi + 2];

                    mAreas[curTri] = inAreas[items[i].i];

                    curTri++;
                }
            }
            else
            {
                // Split
                DeriveExtents(items, imin, imax, ref mNodes[inode]);

                int axis =
                    (mNodes[inode].zmax - mNodes[inode].zmin
                        > mNodes[inode].xmax - mNodes[inode].xmin)
                        ? 1 : 0;
                
                if (axis == 0)
                {
                    // Sort along x-axis
                    Array.Sort<BoundsItem>(items , imin, inum , mCompareX);
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort<BoundsItem>(items, imin, inum, mCompareZ);
                }

                int isplit = imin + inum / 2;

                // Left
                Subdivide(items
                    , imin
                    , isplit
                    , trisPerChunk
                    , ref curNode
                    , ref curTri
                    , inTris
                    , inAreas);

                // Right
                Subdivide(items
                    , isplit
                    , imax
                    , trisPerChunk
                    , ref curNode
                    , ref curTri
                    , inTris
                    , inAreas);

                int iescape = curNode - icur;

                // Negative index means escape.
                mNodes[inode].i = -iescape;
            }
        }

        private static void DeriveExtents(BoundsItem[] items,
						int imin, int imax,
						ref ChunkyTriMeshNode node)
        {
	        node.xmin = items[imin].xmin;
	        node.zmin = items[imin].zmin;
        	
	        node.xmax = items[imin].xmax;
	        node.zmax = items[imin].zmax;
        	
	        for (int i = imin+1; i < imax; ++i)
	        {
                if (items[i].xmin < node.xmin)
                    node.xmin = items[i].xmin;

                if (items[i].zmin < node.zmin)
                    node.zmin = items[i].zmin;

                if (items[i].xmax > node.xmax)
                    node.xmax = items[i].xmax;

                if (items[i].zmax > node.zmax)
                    node.zmax = items[i].zmax;
	        }
        }

        public bool Overlaps(float xmin, float zmin, float xmax, float zmax
			, ChunkyTriMeshNode node)
        {
	        bool overlap = true;
	        overlap = (xmin > node.xmax || xmax < node.xmin) ? false : overlap;
	        overlap = (zmin > node.zmax || zmax < node.zmin) ? false : overlap;
	        return overlap;
        }
	}
}
