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
using Array = System.Array;
using Math = System.Math;

namespace org.critterai.nmgen.u3d
{
    /// <summary>
    /// Represents a configuration for a navigation mesh build in Unity.
    /// </summary>
    /// <remarks>
    /// <para>See 
    /// <a href="http://www.critterai.org/projects/cainav/common/commonparams.html">
    /// Common Parameters</a> for details on the various properties. The primary
    /// difference between this class and the <see cref="NMGenParams"/> class
    /// is that some values in this class are in world units rather than cell
    /// units.</para>
    /// </remarks>
    [System.Serializable]
    public sealed class NMGenBuildParams
    {
        private static int mXZResolution = 1000;
        private static int mYResolution = 1000;
        private static int mDeviationFactor = 20;
        private static int mSampleResolution = 100;

        /// <summary>
        /// The xz-plane resolution to use when deriving a configuration.
        /// </summary>
        public static int XZResolution
        {
            get { return mXZResolution; }
            set { mXZResolution = Math.Max(1, value); }
        }

        /// <summary>
        /// The y-axis resolution to use when deriving a configuration.
        /// </summary>
        public static int YResolution
        {
            get { return mYResolution; }
            set { mYResolution = Math.Max(1, value); }
        }

        /// <summary>
        /// The detail deviation factor to use when deriving a configuration.
        /// </summary>
        public static int DetailDeviation
        {
            get { return mDeviationFactor; }
            set { mDeviationFactor = Math.Max(0, value); }
        }

        /// <summary>
        /// The sample resolution to use when deriving a configuration.
        /// </summary>
        public static int DetailSampleResolution
        {
            get { return mSampleResolution; }
            set { mSampleResolution = Math.Max(1, value); }
        }

        // Remember:  All locally stored fields are ignored
        // in the root.  So the root is not valid until the local
        // data is transferred into it.
        [SerializeField]
        private NMGenParams mRoot = new NMGenParams();

        [SerializeField]
        private float mWalkableRadius;
        [SerializeField]
        private float mMaxEdgeLength;
        [SerializeField]
        private float mMinRegionArea;
        [SerializeField]
        private float mMergeRegionArea;
        [SerializeField]
        private float mWalkableHeight;
        [SerializeField]
        private float mWalkableStep;

        [SerializeField]
        private BuildFlags mBuildFlags;

        /// <summary>
        /// The width of the heightfield along the x-axis.
        /// </summary>
        public int Width
        {
            get { return mRoot.Width; }
            set { mRoot.Width = value; }
        }

        /// <summary>
        /// The depth of the heightfield along the z-axis.
        /// </summary>
        public int Depth
        {
            get { return mRoot.Depth; }
            set { mRoot.Depth = value; }
        }

        /// <summary>
        /// The width/depth size of the tile on the xz-plane.
        /// </summary>
        public int TileSize
        {
            get { return mRoot.TileSize; }
            set { mRoot.TileSize = value; }
        }

        /// <summary>
        /// Gets a copy of the minimum bounds of the grid's AABB.
        /// </summary>
        /// <returns>The maximum bounds of the grid.</returns>
        public float[] GetBoundsMin()
        {
            return mRoot.GetBoundsMin();
        }

        /// <summary>
        /// Sets the minimum bounds of the grid's AABB.
        /// </summary>
        /// <remarks>The values are not validated against the maximum
        /// bounds.</remarks>
        /// <param name="x">The x-value of the bounds.</param>
        /// <param name="y">The y-value of the bounds.</param>
        /// <param name="z">The z-value of the bounds.</param>
        public void SetBoundsMin(float x, float y, float z)
        {
            mRoot.SetBoundsMin(x, y, z);
        }

        /// <summary>
        /// Gets a copy of the maximum bounds of the grid's AABB.
        /// </summary>
        /// <returns>The maximum bounds of the grid.</returns>
        public float[] GetBoundsMax()
        {
            return mRoot.GetBoundsMax();
        }

        /// <summary>
        /// Sets the minimum bounds of the grid's AABB.
        /// </summary>
        /// <remarks>The values are not validated against the minimum
        /// bounds.</remarks>
        /// <param name="x">The x-value of the bounds.</param>
        /// <param name="y">The y-value of the bounds.</param>
        /// <param name="z">The z-value of the bounds.</param>
        public void SetBoundsMax(float x, float y, float z)
        {
            mRoot.SetBoundsMax(x, y, z);
        }

        /// <summary>
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// </summary>
        /// <remarks>
        /// <para>Also the 'grid size' or 'voxel size'.</para>
        /// </remarks>
        public float XZCellSize
        {
            get { return mRoot.XZCellSize; }
            set { mRoot.XZCellSize = value; }
        }

        /// <summary>
        /// The y-axis voxel size to use when sampling the source geometry.
        /// [Limit: >= <see cref="NMGen.MinCellSize"/>]
        /// </summary>
        /// <remarks>
        /// <para>Also the 'voxel size' for the y-axis.</para>
        /// </remarks>
        public float YCellSize
        {
            get { return mRoot.YCellSize; }
            set { mRoot.YCellSize = value; }
        }

        /// <summary>
        /// Minimum floor to 'ceiling' height that will still allow the
        /// floor area to be considered traversable.
        /// </summary>
        /// <remarks>
        /// <para>Usually the maximum client height.</para>
        /// </remarks>
        public float WalkableHeight
        {
            get { return mWalkableHeight; }
            set 
            { 
                mWalkableHeight =  Mathf.Max(
                    NMGen.MinWalkableHeight * NMGen.MinCellSize, value); 
            }
        }

        /// <summary>
        /// Maximum ledge height that is considered to still be
        /// traversable.
        /// </summary>
        /// <remarks>
        /// <para>Usually set to how far up/down the client can step.</para>
        /// </remarks>
        public float WalkableStep
        {
            get { return mWalkableStep; }
            set { mWalkableStep = Mathf.Max(0, value); }
        }

        /// <summary>
        /// The maximum slope that is considered walkable.
        /// </summary>
        public float WalkableSlope
        {
            get { return mRoot.WalkableSlope; }
            set { mRoot.WalkableSlope = value; }
        }

        /// <summary>
        /// Represents the closest any part of a mesh should get to an
        /// obstruction in the source geometry.
        /// </summary>
        /// <remarks>
        ///  Usually the client radius.
        /// </remarks>
        public float WalkableRadius
        {
            get { return mWalkableRadius; }
            set { mWalkableRadius = Mathf.Max(0, value); }
        }

        /// <summary>
        /// The closest the mesh should come to the xz-plane AABB of the
        /// source geometry.
        /// </summary>
        public int BorderSize
        {
            get { return mRoot.BorderSize; }
            set { mRoot.BorderSize = value; }
        }

        /// <summary>
        /// The maximum allowed length of triangle edges on the border of the
        /// mesh.
        /// </summary>
        /// <remarks>
        /// <para>Extra vertices will be inserted if needed.</para>
        /// <para>A value of zero disabled this feature.</para>
        /// </remarks>
        public float MaxEdgeLength
        {
            get { return mMaxEdgeLength; }
            set { mMaxEdgeLength = Mathf.Max(0, value); }
        }

        /// <summary>
        /// The maximum distance the edges of the mesh should deviate from
        /// the source geometry.
        /// </summary>
        /// <remarks>
        /// <para>Applies only to the xz-plane.</para>
        /// </remarks>
        public float EdgeMaxDeviation
        {
            get { return mRoot.EdgeMaxDeviation; }
            set { mRoot.EdgeMaxDeviation = value; }
        }

        /// <summary>
        /// Sets the sampling distance to use when matching the
        /// mesh surface to the source geometry.
        /// [Limits: 0 or >= 0.9]
        /// </summary>
        public float DetailSampleDistance
        {
            get { return mRoot.DetailSampleDistance; }
            set { mRoot.DetailSampleDistance = value; }
        }

        /// <summary>
        /// The maximum distance the mesh surface should deviate from the
        /// surface of the source geometry. 
        /// [Limit: >= 0]
        /// </summary>
        public float DetailMaxDeviation
        {
            get { return mRoot.DetailMaxDeviation; }
            set { mRoot.DetailMaxDeviation = value; }
        }

        /// <summary>
        /// The minimum number of cells allowed to form isolated island meshes.
        /// [Limit: >= 0]
        /// </summary>
        /// <remarks>
        /// <para>Prevents the formation of meshes that are too small to be
        /// of use.</para>
        /// </remarks>
        public float MinRegionArea
        {
            get { return mMinRegionArea; }
            set { mMinRegionArea = Mathf.Max(0, value); }
        }

        /// <summary>
        /// Any regions with an cell count smaller than this value will, 
        /// if possible, be merged with larger regions.
        /// [Limit: >= 0]
        /// </summary>
        public float MergeRegionArea
        {
            get { return mMergeRegionArea; }
            set { mMergeRegionArea = Mathf.Max(0, value); }
        }

        /// <summary>
        /// The maximum number of vertices allowed for polygons
        /// generated during the contour to polygon conversion process.
        /// </summary>
        public int MaxVertsPerPoly
        {
            get { return mRoot.MaxVertsPerPoly; }
            set
            {
                mRoot.MaxVertsPerPoly = value;
            }
        }

        /// <summary>
        /// Flags used to control optional build steps. 
        /// </summary>
        public BuildFlags BuildFlags
        {
            get { return mBuildFlags; }
            set { mBuildFlags = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public NMGenBuildParams()
        {
            UpdateLocals();
            mBuildFlags = BuildFlags.LowHeightSpansNotWalkable
                | BuildFlags.LowObstaclesWalkable
                | BuildFlags.TessellateWallEdges
                | BuildFlags.ApplyPolyFlags;
        }

        /// <summary>
        /// Sets the configuration to match the provided configuration.
        /// </summary>
        /// <param name="config">The configuration to match.</param>
        public void SetConfig(NMGenParams config)
        {
            if (config == null)
                return;

            mRoot = config.Clone();
            Clean(mRoot);
            UpdateLocals();
        }

        private void UpdateLocals()
        {
            float xz = mRoot.XZCellSize;
            float y = mRoot.YCellSize;
            float a = xz * xz;

            mMaxEdgeLength = mRoot.MaxEdgeLength * xz;
            mMergeRegionArea = mRoot.MergeRegionArea * a;
            mMinRegionArea = mRoot.MinRegionArea * a;
            mWalkableHeight = mRoot.WalkableHeight * y;
            mWalkableRadius = mRoot.WalkableRadius * xz;
            mWalkableStep = mRoot.WalkableStep * y;
        }

        /// <summary>
        /// Attempts the derive the best configuration for the provided
        /// source geometry AABB.
        /// </summary>
        /// <param name="minX">The minimum x of the AABB.</param>
        /// <param name="minY">The minimum y of the AABB.</param>
        /// <param name="minZ">The minimum z of the AABB.</param>
        /// <param name="maxX">The maximum x of the AABB.</param>
        /// <param name="maxY">The maximum y of the AABB.</param>
        /// <param name="maxZ">The maximum z of the AABB.</param>
        public void Derive(float minX, float minY, float minZ
            , float maxX, float maxY, float maxZ)
        {
            float maxXZLength = Mathf.Max(maxX - minX, maxZ - minZ);

            // Default to 1000 x 1000 resolution.
            XZCellSize = Mathf.Max(0.05f, maxXZLength / mXZResolution);
            YCellSize = Mathf.Max(0.05f, (maxY - minY) / mYResolution);

            // Default sample at 100 resolution. (Or minimum effective of 0.9f);
            DetailSampleDistance = 
                Mathf.Max(0.9f, maxXZLength / mSampleResolution);

            DetailMaxDeviation = YCellSize * mDeviationFactor;
        }

        /// <summary>
        /// Generates a <see cref="NMGenParams"/> based on the the object 
        /// values.
        /// </summary>
        /// <returns>A configuration based on the object values.</returns>
        public NMGenParams GetConfig()
        {
            NMGenParams result = mRoot.Clone();

            float y = mRoot.YCellSize;
            float xz = mRoot.XZCellSize;
            float a = xz * xz;

            result.MaxEdgeLength = Mathf.CeilToInt(mMaxEdgeLength / xz);
            result.MergeRegionArea = Mathf.CeilToInt(mMergeRegionArea / a);
            result.MinRegionArea = Mathf.CeilToInt(mMinRegionArea / a);
            result.WalkableHeight = Mathf.CeilToInt(mWalkableHeight / y);
            result.WalkableRadius = Mathf.CeilToInt(mWalkableRadius / xz);
            result.WalkableStep = Mathf.FloorToInt(mWalkableStep / y);

            return result;
        }

        /// <summary>
        /// Duplicates the object.
        /// </summary>
        /// <returns>A duplicate of the object.</returns>
        public NMGenBuildParams Clone()
        {
            NMGenBuildParams result = new NMGenBuildParams();
            result.mRoot = mRoot;
            result.mMaxEdgeLength = mMaxEdgeLength;
            result.mMergeRegionArea = mMergeRegionArea;
            result.mMinRegionArea = mMinRegionArea;
            result.mWalkableHeight = mWalkableHeight;
            result.mWalkableRadius = mWalkableRadius;
            result.mWalkableStep = mWalkableStep;
            result.mBuildFlags = mBuildFlags;
            return result;
        }

        /// <summary>
        /// Forces all field values to within the mandatory limits.
        /// </summary>
        /// <param name="config">The configuration to clean.</param>
        public static void Clean(NMGenParams config)
        {
            if (config == null)
                return;

            config.XZCellSize = config.xzCellSize;
            config.WalkableHeight = config.walkableHeight;
            config.YCellSize = config.yCellSize;

            config.Depth = config.depth;
            config.DetailMaxDeviation = config.detailMaxDeviation;
            config.DetailSampleDistance = config.detailSampleDistance;
            config.EdgeMaxDeviation = config.edgeMaxDeviation;
            config.BorderSize = config.borderSize;
            config.MaxEdgeLength = config.maxEdgeLength;
            config.MaxVertsPerPoly = config.MaxVertsPerPoly;
            config.MergeRegionArea = config.mergeRegionArea;
            config.MinRegionArea = config.minRegionArea;
            config.TileSize = config.tileSize;
            config.WalkableRadius = config.walkableRadius;
            config.WalkableSlope = config.walkableSlope;
            config.WalkableStep = config.walkableStep;
            config.Width = config.width;

            if (config.boundsMin == null || config.boundsMin.Length < 3)
                config.boundsMin = new float[3];
            else if (config.boundsMin.Length > 3)
            {
                float[] a = new float[3];
                Array.Copy(config.boundsMin, a, 3);
                config.boundsMin = a;
            }

            if (config.boundsMax == null || config.boundsMax.Length < 3)
                config.boundsMax = new float[3];
            else if (config.boundsMax.Length > 3)
            {
                float[] a = new float[3];
                Array.Copy(config.boundsMax, a, 3);
                config.boundsMax = a;
            }
        }
    }
}
