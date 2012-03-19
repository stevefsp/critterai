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
    /// Specifies a configuration to use when building navigation mesh data.
    /// </summary>
    /// <remarks>
    /// <para>This class represents an aggregation of settings used at different stages in the 
    /// build process.  Not all parameters are used in all build processes.</para>
    /// <para>There is no such thing as a 'zero' configuration.  So the default constructor 
    /// initializes all values to basic valid values.</para>
    /// <para>All properties and methods will auto-clamp fields to valid values. For example, 
    /// if the <see cref="TileSize"/> property is set to -1, the property will clamp the value
    /// to 0.</para>
    /// <para>Field members are minimally documented.  See the  property member documentation 
    /// for details.</para>
    /// <para>Implemented as a class with public fields in order to support Unity serialization.  
    /// Care must be taken not to set the fields to invalid values.</para>
    /// </remarks>
    /// <seealso cref="NMGenTileParams"/>
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public sealed class NMGenParams
    {
        /// <summary>
        /// Tile size.
        /// </summary>
        public int tileSize = 0;

        /// <summary>
        /// Border size.
        /// </summary>
        public int borderSize = 0;

        /// <summary>
        /// XZ-plane cell size.
        /// </summary>
        public float xzCellSize = 0.2f;

        /// <summary>
        /// Y-axis cell size.
        /// </summary>
        public float yCellSize = 0.1f;

        /// <summary>
        /// Maximum walkable slope.
        /// </summary>
        public float walkableSlope = 45.5f;

        /// <summary>
        /// Walkable height.
        /// </summary>
        public int walkableHeight = 19;

        /// <summary>
        /// Walkable step.
        /// </summary>
        public int walkableStep = 3;

        /// <summary>
        /// Walkable radius.
        /// </summary>
        public int walkableRadius = 2;

        /// <summary>
        /// Maximum edge length.
        /// </summary>
        public int maxEdgeLength = 0;

        /// <summary>
        /// Maximum edge deviation.
        /// </summary>
        public float edgeMaxDeviation = 3;

        /// <summary>
        /// Minimum region area.
        /// </summary>
        public int minRegionArea = 400;

        /// <summary>
        /// Merge region area.
        /// </summary>
        public int mergeRegionArea = 75;

        /// <summary>
        /// Maximum vertices per polygon.
        /// </summary>
        public int maxVertsPerPoly = 6;

        /// <summary>
        /// Detail sample distance.
        /// </summary>
        public float detailSampleDistance = 6;

        /// <summary>
        /// Detail maximum deviation.
        /// </summary>
        public float detailMaxDeviation = 1;

        /// <summary>
        /// Options to use when building the contour set.
        /// </summary>
        /// <seealso cref="ContourSet"/>
        public ContourBuildFlags contourOptions = 
            ContourBuildFlags.TessellateAreaEdges | ContourBuildFlags.TessellateWallEdges;

        /// <summary>
        /// If true, use monotone region generation.
        /// </summary>
        public bool useMonotone = false;

        /// <summary>
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// [Limit: >= <see cref="NMGen.MinCellSize"/>]
        /// [Units: World]
        /// </summary>
        /// <remarks>
        /// <para>Also known as the 'grid size' and 'voxel size'.</para>
        /// </remarks>
        public float XZCellSize
        {
            get { return xzCellSize; }
            set { xzCellSize = Math.Max(NMGen.MinCellSize, value); }
        }

        /// <summary>
        /// The y-axis voxel size to use when sampling the source geometry.
        /// [Limit >= <see cref="NMGen.MinCellSize"/>]
        /// </summary>
        /// <remarks>
        /// <para>Also known at the the 'voxel size' for the y-axis.</para>
        /// </remarks>
        public float YCellSize
        {
            get { return yCellSize; }
            set { yCellSize = Math.Max(NMGen.MinCellSize, value); }
        }

        /// <summary>
        /// The width/depth size of the tile on the xz-plane. 
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        /// <para>A value of zero indicates no-tiles.  Small values are not of much use.  
        /// In general, non-zero values should be between 100 and 1000.</para>
        /// </remarks>
        public int TileSize
        {
            get { return tileSize; }
            set { tileSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The tile size in world units.
        /// </summary>
        public float TileWorldSize
        {
            get { return tileSize * xzCellSize; }
        }

        /// <summary>
        /// The closest the mesh should come to the xz-plane AABB of the source geometry.
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        /// <para>The border size exists primarily to support tiled meshes.</para>
        /// </remarks>
        public int BorderSize
        {
            get { return borderSize; }
            set { borderSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum slope that is considered walkable.
        /// [Limits: 0 &lt;= value &lt;= <see cref="NMGen.MaxAllowedSlope"/>]
        /// [Units: Degrees]
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
        /// Minimum floor to 'ceiling' height that will still allow the  floor area to be 
        /// considered walkable. 
        /// [Limit: >= <see cref="NMGen.MinWalkableHeight"/>]
        /// [Units: YCellSize]
        /// </summary>
        /// <remarks>
        /// <para>Permits detection of overhangs in the source geometry that make the geometry 
        /// below un-walkable.</para>
        /// <para>Usually the maximum client height.</para></remarks>
        public int WalkableHeight
        {
            get { return walkableHeight; }
            set { walkableHeight = Math.Max(NMGen.MinWalkableHeight, value); }
        }

        /// <summary>
        /// The walkable height in world units.
        /// </summary>
        public float WorldWalkableHeight
        {
            get { return walkableHeight * yCellSize; }
        }

        /// <summary>
        /// Derives <see cref="WalkableHeight"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>Snapping to the base unit type will occur.</para>
        /// <para>The <see cref="YCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldHeight">The walkable height in world units.</param>
        public void SetWalkableHeight(float worldHeight)
        {
            WalkableHeight = (int)Math.Ceiling(worldHeight / yCellSize);
        }

        /// <summary>
        /// Maximum ledge height that is considered to still be traversable.
        /// [Limit: >=0] 
        /// [Units: YCellSize]
        /// </summary>
        /// <remarks>
        /// <para>Allows the mesh to flow over low lying obstructions such as curbs and up/down 
        /// stairways.</para>
        /// <para>Usually set to how far up/down an agent can step.</para>
        /// </remarks>
        public int WalkableStep
        {
            get { return walkableStep; }
            set { walkableStep = Math.Max(0, value); }
        }

        /// <summary>
        /// The walkable set in world units.
        /// </summary>
        public float WorldWalkableStep
        {
            get { return walkableStep * yCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="WalkableStep"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>Snapping to the base unit type will occur.</para>
        /// <para>The <see cref="YCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldStep">The walkable radius in world units.</param>
        public void SetWalkableStep(float worldStep)
        {
            WalkableStep = (int)Math.Floor(worldStep / yCellSize);
        }

        /// <summary>
        /// Represents the closest any part of a mesh should get to an obstruction in the source 
        /// geometry.
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        ///  Usually the client radius.
        /// </remarks>
        public int WalkableRadius
        {
            get { return walkableRadius; }
            set { walkableRadius = Math.Max(0, value); }
        }

        /// <summary>
        /// The walkable radius in world units.
        /// </summary>
        public float WorldWalkableRadius
        {
            get { return walkableRadius * xzCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="WalkableRadius"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>Snapping to the base unit type will occur.</para>
        /// <para>The <see cref="XZCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldRadius">The walkable radius in world units.</param>
        public void SetWalkableRadius(float worldRadius)
        {
            WalkableRadius = (int)Math.Ceiling(worldRadius / xzCellSize);
        }

        /// <summary>
        /// If true, use monotone region generation.
        /// </summary>
        /// <seealso cref="CompactHeightfield.BuildRegionsMonotone"/>
        public bool UseMonotone
        {
            get { return useMonotone; }
            set { useMonotone = value; }
        }

        /// <summary>
        /// The minimum number of cells allowed to form isolated island meshes.
        /// [Limit: >=0]
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        /// <para>Prevents the formation of meshes that are too small to be of use.</para>
        /// </remarks>
        public int MinRegionArea
        {
            get { return minRegionArea; }
            set { minRegionArea = Math.Max(0, value); }
        }

        /// <summary>
        /// The minimum region area in world units.
        /// </summary>
        public float WorldMinRegionArea
        {
            get { return minRegionArea * xzCellSize * xzCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="MinRegionArea"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>Snapping to the base unit type will occur.</para>
        /// <para>The <see cref="XZCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldArea">The minimum region area in world units.</param>
        public void SetMinRegionArea(float worldArea)
        {
            MinRegionArea = (int)Math.Ceiling(worldArea / (xzCellSize * xzCellSize));
        }

        /// <summary>
        /// Any regions with an cell count smaller than this value will, if possible, be merged 
        /// with larger regions.
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        public int MergeRegionArea
        {
            get { return mergeRegionArea; }
            set { mergeRegionArea = Math.Max(0, value); }
        }

        /// <summary>
        /// The merge region area in world units.
        /// </summary>
        public float WorldMergeRegionArea
        {
            get { return mergeRegionArea * xzCellSize * xzCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="MergeRegionArea"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>Snapping to the base unit type will occur.</para>
        /// <para>The <see cref="XZCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldArea">The merge region area in world units.</param>
        public void SetMergeRegionArea(float worldArea)
        {
            MergeRegionArea = (int)Math.Ceiling(worldArea / (xzCellSize * xzCellSize));
        }

        /// <summary>
        /// The maximum allowed length for edges on the border of the mesh. 
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        /// <para>Extra vertices will be inserted if needed.</para>
        /// <para>A value of zero disabled this feature.</para>
        /// </remarks>
        public int MaxEdgeLength
        {
            get { return maxEdgeLength; }
            set { maxEdgeLength = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum edge length in world units.
        /// </summary>
        public float WorldMaxEdgeLength
        {
            get { return maxEdgeLength * xzCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="WalkableStep"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>Snapping to the base unit type will occur.</para>
        /// <para>The <see cref="XZCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldLength">The walkable radius in world units.</param>
        public void SetMaxEdgeLength(float worldLength)
        {
            MaxEdgeLength = (int)Math.Ceiling(worldLength / xzCellSize);
        }

        /// <summary>
        /// The maximum distance the edges of the mesh should deviate from the source geometry. 
        /// [Limit: >=0] 
        /// [Units: World]
        /// </summary>
        /// <remarks>
        /// <para>Applies only to the xz-plane.</para>
        /// </remarks>
        public float EdgeMaxDeviation
        {
            get { return edgeMaxDeviation; }
            set { edgeMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// Options to use when building the contour set.
        /// </summary>
        /// <seealso cref="ContourSet"/>
        public ContourBuildFlags ContourOptions
        {
            get { return contourOptions; }
            set { contourOptions = value; }
        }

        /// <summary>
        /// Sets the sampling distance to use when matching the mesh surface to the source 
        /// geometry. (For height detail only.)
        /// [Limits: 0 or >= 0.9] 
        /// [Units: World]
        /// </summary>
        public float DetailSampleDistance
        {
            get { return detailSampleDistance; }
            set { detailSampleDistance = value < 0.9f ? 0 : value; }
        }

        /// <summary>
        /// The maximum distance the mesh surface should deviate from the surface of the source 
        /// geometry. (For height detail only.)
        /// [Limit: >=0] 
        /// [Units: World]
        /// </summary>
        public float DetailMaxDeviation
        {
            get { return detailMaxDeviation; }
            set { detailMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum number of vertices allowed for polygons generated during the contour 
        /// to polygon conversion process.
        /// [Limits: 3 &lt;= value &lt; <see cref="NMGen.MaxAllowedVertsPerPoly"/>]
        /// </summary>
        public int MaxVertsPerPoly
        {
            get { return maxVertsPerPoly; }
            set { maxVertsPerPoly = Math.Max(3, Math.Min(NMGen.MaxAllowedVertsPerPoly, value)); }
        }

        /// <summary>
        /// Validates the parameter values.
        /// </summary>
        /// <returns>True if the parameter values meet the manditory limits.</returns>
        public bool IsValid()
        {
            return !(tileSize < 0
                || xzCellSize < NMGen.MinCellSize
                || yCellSize < NMGen.MinCellSize
                || walkableHeight < NMGen.MinWalkableHeight
                || walkableRadius < 0
                || walkableRadius < 0
                || walkableSlope < 0
                || walkableSlope > NMGen.MaxAllowedSlope
                || walkableStep < 0
                || borderSize < 0
                || maxEdgeLength < 0
                || edgeMaxDeviation < 0
                || (detailSampleDistance != 0 && detailSampleDistance < 0.9f)
                || detailMaxDeviation < 0
                || minRegionArea < 0
                || mergeRegionArea < 0
                || maxVertsPerPoly > NMGen.MaxAllowedVertsPerPoly);
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone.</returns>
        public NMGenParams Clone()
        {
            NMGenParams result = new NMGenParams();
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
            result.tileSize = tileSize;
            result.contourOptions = contourOptions;
            result.useMonotone = useMonotone;

            return result;
        }

        /// <summary>
        /// Forces all field values to within the mandatory limits.
        /// </summary>
        public void Clean()
        {
            XZCellSize = xzCellSize;
            WalkableHeight = walkableHeight;
            YCellSize = yCellSize;

            DetailMaxDeviation = detailMaxDeviation;
            DetailSampleDistance = detailSampleDistance;
            EdgeMaxDeviation = edgeMaxDeviation;
            BorderSize = borderSize;
            MaxEdgeLength = maxEdgeLength;
            MaxVertsPerPoly = MaxVertsPerPoly;
            MergeRegionArea = mergeRegionArea;
            MinRegionArea = minRegionArea;
            WalkableRadius = walkableRadius;
            WalkableSlope = walkableSlope;
            WalkableStep = walkableStep;

            TileSize = tileSize;
        }
    }
}
