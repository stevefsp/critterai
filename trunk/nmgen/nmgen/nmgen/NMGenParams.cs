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
using System.Runtime.InteropServices;

namespace org.critterai.nmgen
{
    /// <summary>
    /// Specifies a configuration to use when building navigation meshes.
    /// </summary>
    /// <remarks>
    /// <p>There is no such thing as a 'zero' configuration.  So the
    /// default constructor initializes all values to basic valid values.</p>
    /// <p>The values of various configuration settings can have significant 
    /// side effects on eachother.  See 
    /// <a href="http://www.critterai.org/nmgen_settings" target="_parent">
    /// Understanding Configuration Settings</a> for some helpful tips.</p>
    /// <p>All fields are public in order to support Unity serialization.  
    /// But it is best to set the fields using their properties since
    /// the properties will enforce valid min/max limits.</p>
    /// <p>A properties and methods will auto-limit parameters
    /// to valid values. For example, if the <see cref="TileSize"/>property
    /// is set to -1, the field will be set to the minimum allowed value of 1.
    /// An exception to this rule are the bounds parameters. The bounds
    /// are validated for length, but not content.  If a bounds is null or has
    /// a size of &lt; 3, then the original field value will simply not be 
    /// updated.</p>
    /// <p>Compatible with Unity serialization.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public sealed class NMGenParams
    {
        /*
         * Design note:
         * 
         * Would like this to be a structure.  But structures are not
         * serializable in Unity.  Don't want to have to switch to a
         * structure + class pattern unless I have to.
         */

        public int width = 0;
        public int depth = 0;
        public int tileSize = 0;

        public int borderSize = 0;
        public float xzCellSize = 0.2f;
        public float yCellSize = 0.2f;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundsMin = new float[3];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundsMax = new float[3];

        public float walkableSlope = 45.5f;
        public int walkableHeight = 8;
        public int walkableStep = 3;
        public int walkableRadius = 1;

        public int maxEdgeLength = 0;
        public float edgeMaxDeviation = 1;

        public int minRegionArea = 9;
        public int mergeRegionArea = 25;

        public int maxVertsPerPoly = 6;

        public float detailSampleDistance = 6;
        public float detailMaxDeviation = 1;

        public int Width
        {
            get { return width; }
            set { width = Math.Max(0, value); }
        }

        public int Depth
        {
            get { return depth; }
            set { depth = Math.Max(0, value); }
        }

        public int TileSize
        {
            get { return tileSize; }
            set { tileSize = Math.Max(0, value); }
        }

        public float[] GetBoundsMin()
        {
            return (float[])boundsMin.Clone();
        }

        public void SetBoundsMin(float x, float y, float z)
        {
            boundsMin[0] = x;
            boundsMin[1] = y;
            boundsMin[2] = z;
        }

        public float[] GetBoundsMax()
        {
            return (float[])boundsMax.Clone();
        }

        public void SetBoundsMax(float x, float y, float z)
        {
            boundsMax[0] = x;
            boundsMax[1] = y;
            boundsMax[2] = z;
        }

        /// <summary>
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// (>= <see cref="MinCellSize"/>)
        /// </summary>
        public float XZCellSize
        {
            get { return xzCellSize; }
            set { xzCellSize = Math.Max(NMGen.MinCellSize, value); }
        }

        /// <summary>
        /// The y-axis voxel size to use when sampling the source geometry.
        /// (>= <see cref="MinCellSize"/>)
        /// </summary>
        public float YCellSize
        {
            get { return yCellSize; }
            set { yCellSize = Math.Max(NMGen.MinCellSize, value); }
        }

        /// <summary>
        /// Minimum floor to 'ceiling' height that will still allow the
        /// floor area to be considered traversable.
        /// (>= 3)
        /// </summary>
        /// <remarks>
        /// <p>Permits detection of overhangs in the source geometry that make 
        /// the geometry below un-walkable.</p>
        /// <p>Usually the maximum client height.</p></remarks>
        public int WalkableHeight
        {
            get { return walkableHeight; }
            set { walkableHeight = Math.Max(NMGen.MinWalkableHeight, value); }
        }

        /// <summary>
        /// Maximum ledge height that is considered to still be
        /// traversable.
        /// (>= <see cref="LimitTraversableStep"/>)
        /// </summary>
        /// <remarks>
        /// <p>Allows the mesh to flow over curbs and up/down
        /// stairways.</p>
        /// <p>Usually set to how far up/down the client can step.</p>
        /// </remarks>
        public int WalkableStep
        {
            get { return walkableStep; }
            set { walkableStep = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum slope that is considered traversable. (In degrees.)
        /// (0 &lt;= value &lt;= <see cref="MaxAllowedSlope"/>)
        /// </summary>
        public float WalkableSlope
        {
            get { return walkableSlope; }
            set
            {
                walkableSlope = 
                    Math.Max(0, Math.Min(NMGen.MaxAllowedSlope, value));
            }
        }

        /// <summary>
        /// Represents the closest any part of a mesh should get to an
        /// obstruction in the source geometry.
        /// (>= <see cref="MinBorderSize"/>)
        /// </summary>
        /// <remarks>
        ///  Usually the client radius.
        /// </remarks>
        public int WalkableRadius
        {
            get { return walkableRadius; }
            set { walkableRadius = Math.Max(0, value);  }
        }

        /// <summary>
        /// The closest the mesh should come to the xz-plane's AABB of the
        /// source geometry.
        /// (>= 0)
        /// </summary>
        public int BorderSize
        {
            get { return borderSize; }
            set { borderSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum allowed length of triangle edges on the border of the
        /// mesh. (0 or >= <see cref="XZCellSize"/>)
        /// </summary>
        /// <remarks>
        /// <p>Extra vertices will be inserted if needed.</p>
        /// <p>A value of zero disabled this feature.</p>
        /// </remarks>
        public int MaxEdgeLength
        {
            get { return maxEdgeLength; }
            set { maxEdgeLength = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum distance the edges of the mesh should deviate from
        /// the source geometry. (>=0)
        /// </summary>
        /// <remarks>
        /// <p>Applies only to the xz-plane.</p>
        /// </remarks>
        public float EdgeMaxDeviation
        {
            get { return edgeMaxDeviation; }
            set { edgeMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// Sets the sampling distance to use when matching the
        /// mesh surface to the source geometry.
        /// (0 or >= 0.9)
        /// </summary>
        public float DetailSampleDistance
        {
            get { return detailSampleDistance; }
            set { detailSampleDistance = value < 0.9f ? 0 : value; }
        }

        /// <summary>
        /// The maximum distance the mesh surface should deviate from the
        /// surface of the source geometry. 
        /// (>= 0)
        /// </summary>
        public float DetailMaxDeviation
        {
            get { return detailMaxDeviation; }
            set { detailMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// The minimum number of cells allowed to form isolated island meshes.
        /// (>= 0)
        /// </summary>
        /// <remarks>
        /// <p>Prevents the formation of meshes that are too small to be
        /// of use.</p>
        /// </remarks>
        public int MinRegionArea
        {
            get { return minRegionArea; }
            set { minRegionArea = Math.Max(0, value); }
        }

        /// <summary>
        /// Any regions with an cell count smaller than this value will, 
        /// if possible, be merged with larger regions.
        /// (>= 0)
        /// </summary>
        public int MergeRegionArea
        {
            get { return mergeRegionArea; }
            set { mergeRegionArea = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum number of vertices allowed for polygons
        /// generated during the contour to polygon conversion process.
        /// (3 &lt;= value &lt; <see cref="MaxAllowedVertsPerPoly"/>)
        /// </summary>
        public int MaxVertsPerPoly
        {
            get { return maxVertsPerPoly; }
            set
            {
                // Must be between 3 and max allowed.
                maxVertsPerPoly = Math.Max(3
                    , Math.Min(NMGen.MaxAllowedVertsPerPoly, value));
            }
        }

        public NMGenParams() { }

        public void DerivedGridSize()
        {
            Width = (int)((boundsMax[0] - boundsMin[0]) / xzCellSize + 0.5f);
            Depth = (int)((boundsMax[2] - boundsMin[2]) / xzCellSize + 0.5f);
        }

        public NMGenParams Clone()
        {
            NMGenParams result = new NMGenParams();
            result.width = width;
            result.depth = depth;
            result.tileSize = tileSize;
            result.xzCellSize = xzCellSize;
            result.yCellSize = yCellSize;
            result.detailMaxDeviation = detailMaxDeviation;
            result.detailSampleDistance = detailSampleDistance;
            result.edgeMaxDeviation = edgeMaxDeviation;
            result.borderSize = borderSize;
            result.walkableSlope = walkableSlope;
            result.walkableStep = walkableStep;
            result.maxVertsPerPoly = maxVertsPerPoly;
            result.walkableRadius = walkableRadius;
            result.maxEdgeLength = maxEdgeLength;
            result.walkableHeight = walkableHeight;
            result.mergeRegionArea = mergeRegionArea;
            result.minRegionArea = minRegionArea;

            if (boundsMax == null)
                result.boundsMax = null;
            else
                result.boundsMax = GetBoundsMax();
            if (boundsMin == null)
                result.boundsMin = null;
            else
                result.boundsMin = GetBoundsMin();

            return result;
        }

        ///// <summary>
        ///// Constructor
        ///// </summary>
        ///// <remarks>
        ///// All min/max limits are automatically applied during construction.  
        ///// So the resulting structure may not contain the values specified 
        ///// during construction.  See the documentation for individual
        ///// properties for information on min/max limits.
        ///// </remarks>
        ///// <param name="xzCellSize">The xz-plane voxel size to use when 
        ///// sampling the source geometry.</param>
        ///// <param name="yCellSize">The y-axis voxel size to use when sampling 
        ///// the source geometry.</param>
        ///// <param name="minTraversableHeight">Minimum floor to 'ceiling' 
        ///// height that will still allow the floor area to be considered 
        ///// traversable.</param>
        ///// <param name="maxTraversableStep">Maximum ledge height that is
        ///// considered to still be traversable. </param>
        ///// <param name="maxTraversableSlope">The maximum slope that is 
        ///// considered traversable. (In degrees.)</param>
        ///// <param name="clipLedges">TRUE if ledge voxels should be considered 
        ///// un-walkable.</param>
        ///// <param name="traversableAreaBorderSize">Represents the closest 
        ///// any part of a mesh should get to an obstruction in the source 
        ///// geometry. (Usually the client radius.)</param>
        ///// <param name="heightfieldBorderSize">The closest the mesh should 
        ///// come to the xz-plane's AABB of the source geometry.</param>
        ///// <param name="smoothingThreshold">The amount of smoothing to be 
        ///// performed when generating the distance field used for deriving 
        ///// regions.</param>
        ///// <param name="minIslandRegionSize">The minimum number of cells 
        ///// allowed to form isolated island meshes.</param>
        ///// <param name="mergeRegionSize">Any regions with an cell count 
        ///// smaller than this value will, if possible, be merged with 
        ///// larger regions.</param>
        ///// <param name="maxEdgeLength">The maximum allowed length of 
        ///// triangle edges on the border of the mesh.</param>
        ///// <param name="edgeMaxDeviation">The maximum distance the edges of 
        ///// the mesh should deviate from the source geometry.</param>
        ///// <param name="maxVertsPerPoly">The maximum number of vertices 
        ///// allowed for polygons generated during the contour to polygon 
        ///// conversion process.</param>
        ///// <param name="contourSampleDistance">Sets the sampling distance 
        ///// to use when matching the mesh surface to the source geometry.
        ///// </param>
        ///// <param name="contourMaxDeviation">The maximum distance the mesh 
        ///// surface should deviate from the surface of the source geometry.
        ///// </param>
        //public NMGenParams(float[] boundsMin
        //    , float[] boundsMax
        //    , float xzCellSize
        //    , float yCellSize
        //    , int walkableHeight
        //    , int walkableStep
        //    , float walkableSlope
        //    , int walkableRadius
        //    , int heightfieldBorderSize
        //    , int minRegionArea
        //    , int mergeRegionArea
        //    , int maxEdgeLength
        //    , float edgeMaxDeviation
        //    , int maxVertsPerPoly
        //    , float detailSampleDistance
        //    , float detailMaxDeviation
        //    , int width
        //    , int depth
        //    , int tileSize)
        //{
        //    Width = width;
        //    Depth = depth;
        //    BoundsMin = boundsMin;
        //    BoundsMax = boundsMax;
        //    TileSize = tileSize;
        //    XZCellSize = xzCellSize;
        //    YCellSize = yCellSize;
        //    DetailMaxDeviation = detailMaxDeviation;
        //    DetailSampleDistance = detailSampleDistance;
        //    EdgeMaxDeviation = edgeMaxDeviation;
        //    HeightfieldBorderSize = heightfieldBorderSize;
        //    WalkableSlope = walkableSlope;
        //    WalkableStep = walkableStep;
        //    MaxVertsPerPoly = maxVertsPerPoly;
        //    WalkableRadius = walkableRadius;
        //    MaxEdgeLength = maxEdgeLength;
        //    WalkableHeight = walkableHeight;
        //    MergeRegionArea = mergeRegionArea;
        //    MinRegionArea = minRegionArea;

        //    // NMGenUtilEx.ApplyLimits(this);
        //}

        ///// <summary>
        ///// Converts an area in world units into a cell count.
        ///// </summary>
        ///// <param name="worldArea">The area in world units.</param>
        ///// <returns>The number of cells covered by the world area.</returns>
        //public int ToCellArea(float worldArea)
        //{
        //    return (int)Math.Ceiling(worldArea / (xzCellSize * xzCellSize));
        //}

        ///// <summary>
        ///// Converts the number of cells into an area in world units.
        ///// </summary>
        ///// <param name="cellArea">The number of cells.</param>
        ///// <param name="xzCellSize">The cell size.</param>
        ///// <returns></returns>
        //public float ToWorldArea(int cellArea)
        //{
        //    return cellArea * xzCellSize * xzCellSize;
        //}
    }
}
