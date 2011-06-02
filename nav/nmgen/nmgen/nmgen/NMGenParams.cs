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
    /// <p>The is a convenience class that represents an aggregation of 
    /// settings used at different stages in the build process.  In some cases 
    /// the values are expected to be derived by the build process. In some
    /// cases settings are irrelavent.  It all depends on the particular
    /// build process being used/implemented.</p>
    /// <p>There is no such thing as a 'zero' configuration.  So the
    /// default constructor initializes all values to basic valid values.</p>
    /// <p>All fields are public in order to support Unity serialization.  
    /// But it is best to set the fields using the properties since
    /// they will enforce valid min/max limits.</p>
    /// <p>All properties and methods will auto-limit parameters
    /// to valid values. For example, if the <see cref="TileSize"/> property
    /// is set to -1, the field will be set to the minimum allowed value of 0.
    /// </p>
    /// <p>Fields are minimally documented.  See the property documentation
    /// for details.</p>
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

        /// <summary>
        /// Grid width.
        /// </summary>
        public int width = 0;

        /// <summary>
        /// Grid depth.
        /// </summary>
        public int depth = 0;

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
        /// Minimum bounds.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundsMin = new float[3];

        /// <summary>
        /// Maximum bounds.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundsMax = new float[3];

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
        public int minRegionArea = 25;

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
        /// The width of the heightfield along the x-axis.
        /// [Limit: >0] [Units: XZCellSize]
        /// </summary>
        /// <remarks><p>Often a derived value.</p></remarks>
        public int Width
        {
            get { return width; }
            set { width = Math.Max(0, value); }
        }

        /// <summary>
        /// The depth of the heightfield along the z-axis.
        /// [Limit: >=0] [Units: XZCellSize]
        /// </summary>
        /// <remarks><p>Often a derived value.</p></remarks>
        public int Depth
        {
            get { return depth; }
            set { depth = Math.Max(0, value); }
        }

        /// <summary>
        /// The width/depth size of the tile on the xz-plane.
        /// [Limit: >=0] [Units: CellSize]
        /// </summary>
        public int TileSize
        {
            get { return tileSize; }
            set { tileSize = Math.Max(0, value); }
        }

        /// <summary>
        /// Gets a copy of the minimum bounds of the grid's AABB.
        /// [Form: (x, y, z)] [Units: World]
        /// </summary>
        /// <returns>The maximum bounds of the grid.</returns>
        public float[] GetBoundsMin()
        {
            return (float[])boundsMin.Clone();
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
            boundsMin[0] = x;
            boundsMin[1] = y;
            boundsMin[2] = z;
        }

        /// <summary>
        /// Gets a copy of the maximum bounds of the grid's AABB.
        /// [Form: (x, y, z)] [Units: World]
        /// </summary>
        /// <returns>The maximum bounds of the grid.</returns>
        public float[] GetBoundsMax()
        {
            return (float[])boundsMax.Clone();
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
            boundsMax[0] = x;
            boundsMax[1] = y;
            boundsMax[2] = z;
        }

        /// <summary>
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// [Limit: >= <see cref="NMGen.MinCellSize"/>]
        /// [Units: World]
        /// </summary>
        /// <remarks>
        /// <p>Also the 'grid size' or 'voxel size'.</p>
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
        /// <p>Also the 'voxel size' for the y-axis.</p>
        /// </remarks>
        public float YCellSize
        {
            get { return yCellSize; }
            set { yCellSize = Math.Max(NMGen.MinCellSize, value); }
        }

        /// <summary>
        /// Minimum floor to 'ceiling' height that will still allow the
        /// floor area to be considered walkable. 
        /// [Limit: >= <see cref="NMGen.MinWalkableHeight"/>]
        /// [Units: YCellSize]
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
        /// Maximum ledge height that is considered to still be traversable.
        /// [Limit: >=0] [Units: YCellSize]
        /// </summary>
        /// <remarks>
        /// <p>Allows the mesh to flow over low lying obstructions such as
        /// curbs and up/down stairways.</p>
        /// <p>Usually set to how far up/down an agent can step.</p>
        /// </remarks>
        public int WalkableStep
        {
            get { return walkableStep; }
            set { walkableStep = Math.Max(0, value); }
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
        /// Represents the closest any part of a mesh should get to an
        /// obstruction in the source geometry.
        /// [Limit: >=0] [Units: XZCellSize]
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
        /// The closest the mesh should come to the xz-plane AABB of the
        /// source geometry.
        /// [Limit: >=0] [Units: XZCellSize]
        /// </summary>
        public int BorderSize
        {
            get { return borderSize; }
            set { borderSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum allowed length for edges on the border of the
        /// mesh. [Limit: >=0] [Units: XZCellSize]
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
        /// the source geometry. [Limit: >=0] [Units: World]
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
        /// mesh surface to the source geometry. (For height detail only.)
        /// [Limits: 0 or >= 0.9] [Units: World]
        /// </summary>
        public float DetailSampleDistance
        {
            get { return detailSampleDistance; }
            set { detailSampleDistance = value < 0.9f ? 0 : value; }
        }

        /// <summary>
        /// The maximum distance the mesh surface should deviate from the
        /// surface of the source geometry. (For height detail only.)
        /// [Limit: >=0] [Units: World]
        /// </summary>
        public float DetailMaxDeviation
        {
            get { return detailMaxDeviation; }
            set { detailMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// The minimum number of cells allowed to form isolated island meshes.
        /// [Limit: >=0] [Units: XZCellSize]
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
        /// [Limit: >=0] [Units: XZCellSize]
        /// </summary>
        public int MergeRegionArea
        {
            get { return mergeRegionArea; }
            set { mergeRegionArea = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum number of vertices allowed for polygons
        /// generated during the contour to polygon conversion process.
        /// [Limits: 3 &lt;= value &lt; 
        /// <see cref="NMGen.MaxAllowedVertsPerPoly"/>]
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

        /// <summary>
        /// Constructor.
        /// </summary>
        public NMGenParams() { }

        /// <summary>
        /// Derive the width/depth values from the bounds and cell size values.
        /// </summary>
        public void DeriveSizeOfGrid()
        {
            Width = (int)((boundsMax[0] - boundsMin[0]) / xzCellSize + 0.5f);
            Depth = (int)((boundsMax[2] - boundsMin[2]) / xzCellSize + 0.5f);
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone.</returns>
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
    }
}
