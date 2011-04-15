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
using org.critterai.nav.rcn.externs;
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Specifies a configuration to use when building navigation meshes.
    /// </summary>
    /// <remarks>
    /// <p>There is no such thing as a 'zero' configuration.  Use the 
    /// <see cref="GetDefault"/> method to get a new instance with the default
    /// settings.</p>
    /// <p>The values of various configuration settings can have significant 
    /// side effects on eachother.  See 
    /// <a href="http://www.critterai.org/nmgen_settings" target="_parent">
    /// Understanding Configuration Settings</a> for some helpful tips.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct NMGenParams
    {
        /// <summary>
        /// The prefix tag for all machine configuration strings.
        /// </summary>
        /// <seealso cref="ToMachineString"/>
        /// <seealso cref="FromMachineString"/>
        public const string MachineTag = "NMGC:";

        /// <summary>
        /// The maximum allowed value for <see cref="MaxTraversableSlope"/>.
        /// </summary>
        public const float MaxAllowedSlope = 85.0f;

        /// <summary>
        /// The maximum allowed value for <see cref="SmoothingThreshold"/>.
        /// </summary>
        public const int MaxAllowedSmoothing = 4;

        /// <summary>
        /// The maximum allowed value for <see cref="MaxVertsPerPoly"/>.
        /// </summary>
        public const int MaxAllowedVertsPerPoly = 6;

        /// <summary>
        /// The minimum allowed value for <see cref="XZCellSize"/> and 
        /// <see cref="YCellSize"/>.
        /// </summary>
        public const float MinCellSize = 0.01f;

        /// <summary>
        /// The absolute minimum allowed value for 
        /// <see cref="MinTraversableHeight"/>.
        /// </summary>
        /// <remarks>
        /// Dependancies between parameters may limit the minimum value 
        /// to a higher value.
        /// </remarks>
        public const float LimitTraversableHeight = 3 * MinCellSize;

        /// <summary>
        /// The absolute mininum value allowed value for 
        /// <see cref="MaxTraversableStep"/>.
        /// </summary>
        /// <remarks>
        /// Dependancies between parameters may limit the minimum value 
        /// to a higher value.
        /// </remarks>
        public const float LimitTraversableStep = 3 * MinCellSize;

        /// <summary>
        /// The minimum allowed value for 
        /// <see cref="TraversableAreaBorderSize"/> .
        /// </summary>
        public const float MinBorderSize = 0;

        // Default values.  (Only available for non-derived settings.)
        // See GetDefault() for other values.
        private const float DefaultXZCellSize = 0.2f;
        private const float DefaultMinTraversableHeight = 1.5f;
        private const float DefaultMaxTraversableStep = 0.25f;
        private const float DefaultMaxTraversableSlope = 45.5f;
        private const float DefaultHeightfieldBorderSize = 0;
        private const int DefaultSmoothingThreshold = 1;
        private const int DefaultMaxVertsPerPoly = 6;
        private const bool DefaultClipLedges = false;
        private const float DefaultMaxEdgeLength = 0;

        private float mXZCellSize;
        private float mYCellSize;
        private float mMinTraversableHeight;
        private float mMaxTraversableStep;
        private float mMaxTraversableSlope;
        private float mTraversableAreaBorderSize;
        private float mHeightfieldBorderSize;
        private float mMaxEdgeLength;
        private float mEdgeMaxDeviation;
        private float mContourSampleDistance;
        private float mContourMaxDeviation;
        private int mSmoothingThreshold;
        private int mMinIslandRegionSize;
        private int mMergeRegionSize;
        private int mMaxVertsPerPoly;
        private bool mClipLedges;

        /// <summary>
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// (>= <see cref="MinCellSize"/>)
        /// </summary>
        public float XZCellSize
        {
            get { return mXZCellSize; }
            set { mXZCellSize = Math.Max(MinCellSize, value); }
        }

        /// <summary>
        /// The y-axis voxel size to use when sampling the source geometry.
        /// (>= <see cref="MinCellSize"/>)
        /// </summary>
        public float YCellSize
        {
            get { return mYCellSize; }
            set { mYCellSize = Math.Max(MinCellSize, value); }
        }

        /// <summary>
        /// Minimum floor to 'ceiling' height that will still allow the
        /// floor area to be considered traversable.
        /// (>= 3 * <see cref="YCellSize"/>)
        /// </summary>
        /// <remarks>
        /// <p>Permits detection of overhangs in the source geometry that make 
        /// the geometry below un-walkable.</p>
        /// <p>Usually the maximum client height.</p></remarks>
        public float MinTraversableHeight
        {
            get { return mMinTraversableHeight; }
            set 
            {
                // Must be >= 3 times the yCellSize.
                mMinTraversableHeight = Math.Max(Math.Max(LimitTraversableHeight
                        , 3 * mYCellSize)
                        , value); 
            }
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
        public float MaxTraversableStep
        {
            get { return mMaxTraversableStep; }
            set { mMaxTraversableStep = Math.Max(LimitTraversableStep, value); }
        }

        /// <summary>
        /// The maximum slope that is considered traversable. (In degrees.)
        /// (0 &lt;= value &lt;= <see cref="MaxAllowedSlope"/>)
        /// </summary>
        public float MaxTraversableSlope
        {
            get { return mMaxTraversableSlope; }
            set
            {
                mMaxTraversableSlope = 
                    Math.Max(0, Math.Min(MaxAllowedSlope, value));
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
        public float TraversableAreaBorderSize
        {
            get { return mTraversableAreaBorderSize; }
            set 
            { 
                mTraversableAreaBorderSize =  Math.Max(MinBorderSize, value); 
            }
        }

        /// <summary>
        /// The closest the mesh should come to the xz-plane's AABB of the
        /// source geometry.
        /// (>= 0)
        /// </summary>
        public float HeightfieldBorderSize
        {
            get { return mHeightfieldBorderSize; }
            set { mHeightfieldBorderSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum allowed length of triangle edges on the border of the
        /// mesh. (0 or >= <see cref="XZCellSize"/>)
        /// </summary>
        /// <remarks>
        /// <p>Extra vertices will be inserted if needed.</p>
        /// <p>A value of zero disabled this feature.</p>
        /// </remarks>
        public float MaxEdgeLength
        {
            get { return mMaxEdgeLength; }
            set 
            { 
                // Any value less than the xzCell size is irrelavant.  In
                // such cases, disabled feature by snapping to zero.
                mMaxEdgeLength = (value < mXZCellSize ? 0 : value);
            }
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
            get { return mEdgeMaxDeviation; }
            set { mEdgeMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// Sets the sampling distance to use when matching the
        /// mesh surface to the source geometry.
        /// (0 or >= 0.9)
        /// </summary>
        public float ContourSampleDistance
        {
            get { return mContourSampleDistance; }
            set { mContourSampleDistance = value < 0.9f ? 0 : value; }
        }

        /// <summary>
        /// The maximum distance the mesh surface should deviate from the
        /// surface of the source geometry. 
        /// (>= 0)
        /// </summary>
        public float ContourMaxDeviation
        {
            get { return mContourMaxDeviation; }
            set { mContourMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// The amount of smoothing to be performed when generating the distance 
        /// field used for deriving regions.
        /// (0 &lt;= value &lt;= <see cref="MaxAllowedSmoothing"/>)
        /// </summary>
        public int SmoothingThreshold
        {
            get { return mSmoothingThreshold; }
            set
            {
                // Must be between 0 and max allowed.
                mSmoothingThreshold = Math.Max(0, 
                        Math.Min(MaxAllowedSmoothing, value));
            }
        }

        /// <summary>
        /// The minimum number of cells allowed to form isolated island meshes.
        /// (>= 0)
        /// </summary>
        /// <remarks>
        /// <p>Prevents the formation of meshes that are too small to be
        /// of use.</p>
        /// </remarks>
        public int MinIslandRegionSize
        {
            get { return mMinIslandRegionSize; }
            set { mMinIslandRegionSize = Math.Max(0, value); }
        }

        /// <summary>
        /// Any regions with an cell count smaller than this value will, 
        /// if possible, be merged with larger regions.
        /// (>= 0)
        /// </summary>
        public int MergeRegionSize
        {
            get { return mMergeRegionSize; }
            set { mMergeRegionSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum number of vertices allowed for polygons
        /// generated during the contour to polygon conversion process.
        /// (3 &lt;= value &lt; <see cref="MaxAllowedVertsPerPoly"/>)
        /// </summary>
        public int MaxVertsPerPoly
        {
            get { return mMaxVertsPerPoly; }
            set
            {
                // Must be between 3 and max allowed.
                mMaxVertsPerPoly = Math.Max(3
                    , Math.Min(MaxAllowedVertsPerPoly, value));
            }
        }

        /// <summary>
        /// TRUE if ledge voxels should be considered un-walkable.
        /// </summary>
        public bool ClipLedges
        {
            get { return mClipLedges; }
            set { mClipLedges = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// All min/max limits are automatically applied during construction.  
        /// So the resulting structure may not contain the values specified 
        /// during construction.  See the documentation for individual
        /// properties for information on min/max limits.
        /// </remarks>
        /// <param name="xzCellSize">The xz-plane voxel size to use when 
        /// sampling the source geometry.</param>
        /// <param name="yCellSize">The y-axis voxel size to use when sampling 
        /// the source geometry.</param>
        /// <param name="minTraversableHeight">Minimum floor to 'ceiling' 
        /// height that will still allow the floor area to be considered 
        /// traversable.</param>
        /// <param name="maxTraversableStep">Maximum ledge height that is
        /// considered to still be traversable. </param>
        /// <param name="maxTraversableSlope">The maximum slope that is 
        /// considered traversable. (In degrees.)</param>
        /// <param name="clipLedges">TRUE if ledge voxels should be considered 
        /// un-walkable.</param>
        /// <param name="traversableAreaBorderSize">Represents the closest 
        /// any part of a mesh should get to an obstruction in the source 
        /// geometry. (Usually the client radius.)</param>
        /// <param name="heightfieldBorderSize">The closest the mesh should 
        /// come to the xz-plane's AABB of the source geometry.</param>
        /// <param name="smoothingThreshold">The amount of smoothing to be 
        /// performed when generating the distance field used for deriving 
        /// regions.</param>
        /// <param name="minIslandRegionSize">The minimum number of cells 
        /// allowed to form isolated island meshes.</param>
        /// <param name="mergeRegionSize">Any regions with an cell count 
        /// smaller than this value will, if possible, be merged with 
        /// larger regions.</param>
        /// <param name="maxEdgeLength">The maximum allowed length of 
        /// triangle edges on the border of the mesh.</param>
        /// <param name="edgeMaxDeviation">The maximum distance the edges of 
        /// the mesh should deviate from the source geometry.</param>
        /// <param name="maxVertsPerPoly">The maximum number of vertices 
        /// allowed for polygons generated during the contour to polygon 
        /// conversion process.</param>
        /// <param name="contourSampleDistance">Sets the sampling distance 
        /// to use when matching the mesh surface to the source geometry.
        /// </param>
        /// <param name="contourMaxDeviation">The maximum distance the mesh 
        /// surface should deviate from the surface of the source geometry.
        /// </param>
        public NMGenParams(float xzCellSize
                , float yCellSize
                , float minTraversableHeight
                , float maxTraversableStep
                , float maxTraversableSlope
                , bool clipLedges
                , float traversableAreaBorderSize
                , float heightfieldBorderSize
                , int smoothingThreshold
                , int minIslandRegionSize
                , int mergeRegionSize
                , float maxEdgeLength
                , float edgeMaxDeviation
                , int maxVertsPerPoly
                , float contourSampleDistance
                , float contourMaxDeviation)
        {

            this.mXZCellSize = 0;
            this.mYCellSize = 0;
            this.mClipLedges = false;
            this.mContourMaxDeviation = 0;
            this.mContourSampleDistance = 0;
            this.mEdgeMaxDeviation = 0;
            this.mHeightfieldBorderSize = 0;
            this.mMaxEdgeLength = 0;
            this.mMaxTraversableSlope = 0;
            this.mMaxTraversableStep = 0;
            this.mMaxVertsPerPoly = 0;
            this.mMinTraversableHeight = 0;
            this.mSmoothingThreshold = 0;
            this.mTraversableAreaBorderSize = 0;
            this.mMergeRegionSize = 0;
            this.mMinIslandRegionSize = 0;

            XZCellSize = xzCellSize;
            YCellSize = yCellSize;
            ClipLedges = clipLedges;
            ContourMaxDeviation = contourMaxDeviation;
            ContourSampleDistance = contourSampleDistance;
            EdgeMaxDeviation = edgeMaxDeviation;
            HeightfieldBorderSize = heightfieldBorderSize;
            MaxTraversableSlope = maxTraversableSlope;
            MaxTraversableStep = maxTraversableStep;
            MaxVertsPerPoly = maxVertsPerPoly;
            SmoothingThreshold = smoothingThreshold;
            TraversableAreaBorderSize = traversableAreaBorderSize;

            // These values have dependancies and must be set last.
            MaxEdgeLength = maxEdgeLength;
            MinTraversableHeight = minTraversableHeight;
            MergeRegionSize = mergeRegionSize;
            MinIslandRegionSize = minIslandRegionSize;

            NMGenUtilEx.ApplyStandardLimits(ref this);
        }

        /// <summary>
        /// An instance loaded with the default configuration.
        /// </summary>
        /// <returns>An instance loaded with the default 
        /// configuration.
        /// </returns>
        public static NMGenParams GetDefault()
        {
            NMGenParams config = new NMGenParams();

            config.XZCellSize = DefaultXZCellSize;
            config.MinTraversableHeight = DefaultMinTraversableHeight;
            config.MaxTraversableStep = DefaultMaxTraversableStep;
            config.MaxTraversableSlope = DefaultMaxTraversableSlope;
            config.HeightfieldBorderSize = DefaultHeightfieldBorderSize;
            config.SmoothingThreshold = DefaultSmoothingThreshold;
            config.MaxVertsPerPoly = DefaultMaxVertsPerPoly;
            config.ClipLedges = DefaultClipLedges;
            config.MaxEdgeLength = DefaultMaxEdgeLength;

            config.TraversableAreaBorderSize = DefaultXZCellSize;
            config.YCellSize = DefaultMaxTraversableStep * 0.4f;
            config.EdgeMaxDeviation = DefaultXZCellSize * 10;
            config.ContourSampleDistance = DefaultXZCellSize * 40;
            config.ContourMaxDeviation = config.YCellSize * 20;
            config.MinIslandRegionSize = 
                (int)Math.Pow(config.TraversableAreaBorderSize * 5, 2);
            config.MergeRegionSize = 
                (int)Math.Pow(config.TraversableAreaBorderSize * 10, 2);

            return config;
        }

        /// <summary>
        /// Gets a compact machine readable string that is useful for
        /// serialization.
        /// </summary>
        /// <returns>A machine readable string representing the configuration.
        /// </returns>
        /// <seealso cref="FromMachineString"/>
        public string ToMachineString()
        {
            const string format = MachineTag
                + "{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}"
                + ":{10}:{11}:{12}:{13}:{14}:{15}";
            return string.Format(format
                , (this.ClipLedges ? 1 : 0)
                , this.ContourMaxDeviation
                , this.ContourSampleDistance
                , this.EdgeMaxDeviation
                , this.HeightfieldBorderSize
                , this.MaxEdgeLength
                , this.MaxTraversableSlope
                , this.MaxTraversableStep
                , this.MaxVertsPerPoly
                , this.MergeRegionSize
                , this.MinIslandRegionSize
                , this.MinTraversableHeight
                , this.SmoothingThreshold
                , this.TraversableAreaBorderSize
                , this.XZCellSize
                , this.YCellSize);
        }
            
        /// <summary>
        /// Geneates a configuation from a valid machine readable string. 
        /// </summary>
        /// <remarks>The beginning of the machine string can be identified
        /// by searching for <see cref="MachineTag"/></remarks>
        /// <param name="config">The machine string to translate.</param>
        /// <returns>The configuration derived from the machine string.</returns>
        /// <seealso cref="ToMachineString"/>
        public static NMGenParams FromMachineString(string config)
        {
            // Perform a smoke test.  Any other format errors in the string will
            // result in a runtime error.
            if (config == null || !config.StartsWith(MachineTag))
                return new NMGenParams();

            char[] delim = { ':' };
            string[] sa = config.Split(delim);

            // Remember that the value at index zero is a tag, not a configuration
            // value.
            bool clipLedges = (sa[1] == "0" ? false : true);
            float contourMaxDeviation = (float)Convert.ToDouble(sa[2]);
            float contourSampleDistance = (float)Convert.ToDouble(sa[3]);
            float edgeMaxDeviation = (float)Convert.ToDouble(sa[4]);
            float heightfieldBorderSize = (float)Convert.ToDouble(sa[5]);
            float maxEdgeLength = (float)Convert.ToDouble(sa[6]);
            float maxTraversableSlope = (float)Convert.ToDouble(sa[7]);
            float maxTraversableStep = (float)Convert.ToDouble(sa[8]);
            int maxVertsPerPoly = Convert.ToInt32(sa[9]);
            int mergeRegionSize = Convert.ToInt32(sa[10]);
            int minIslandRegionSize = Convert.ToInt32(sa[11]);
            float minTraversableHeight = (float)Convert.ToDouble(sa[12]);
            int smoothingThreshold = Convert.ToInt32(sa[13]);
            float traversableAreaBorderSize = (float)Convert.ToDouble(sa[14]);
            float xzCellSize = (float)Convert.ToDouble(sa[15]);
            float yCellSize = (float)Convert.ToDouble(sa[16]);

            return new NMGenParams(xzCellSize
                , yCellSize
                , minTraversableHeight
                , maxTraversableStep
                , maxTraversableSlope
                , clipLedges
                , traversableAreaBorderSize
                , heightfieldBorderSize
                , smoothingThreshold
                , minIslandRegionSize
                , mergeRegionSize
                , maxEdgeLength
                , edgeMaxDeviation
                , maxVertsPerPoly
                , contourSampleDistance
                , contourMaxDeviation);
        }

        /// <summary>
        /// Converts an area in world units into a cell count.
        /// </summary>
        /// <param name="worldArea">The area in world units.</param>
        /// <param name="xzCellSize">The cell size.</param>
        /// <returns>The number of cells covered by the world area.</returns>
        public static int GetCellArea(float worldArea, float xzCellSize)
        {
            return (int)Math.Ceiling(worldArea / (xzCellSize * xzCellSize));
        }

        /// <summary>
        /// Converts the number of cells into an area in world units.
        /// </summary>
        /// <param name="cellArea">The number of cells.</param>
        /// <param name="xzCellSize">The cell size.</param>
        /// <returns></returns>
        public static float GetWorldArea(int cellArea, float xzCellSize)
        {
            return cellArea * xzCellSize * xzCellSize;
        }
    }
}
