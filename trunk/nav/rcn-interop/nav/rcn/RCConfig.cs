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
using System.Collections.Generic;
using System.Text;
using org.critterai.nav.rcn.externs;
using System.Runtime.Serialization;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Specifies a configuration to use when building navigation meshes.
    /// </summary>
    /// <remarks>
    /// <p>The values of various configuration settings can have significant 
    /// side effects on eachother.  See 
    /// <a href="http://www.critterai.org/nmgen_settings" target="_parent">
    /// Understanding Configuration Settings</a> for some helpful tips.</p>
    /// <p>There is no such thing as a 'zero' configuration.  Use the 
    /// <see cref="GetDefault"/> method to get a new instance with the default
    /// settings.</p>
    /// </remarks>
    [Serializable]
    public sealed class RCConfig
    {
        /// <summary>
        /// The prefix tag for all machine configuration strings.
        /// </summary>
        /// <seealso cref="GetMachineString"/>
        /// <seealso cref="GetFromMachineString"/>
        public const string MachineTag = "NMGC:";

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
        
        public const float MinCellSize = 0.01f;

        internal RCConfigEx root;

        private float mMinIslandRegionSize;
        private float mMergeRegionSize;

        /// <summary>
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// </summary>
        public float XZCellSize
        {
            get { return root.xzCellSize; }
            set { root.xzCellSize = Math.Max(MinCellSize, value); }
        }

        /// <summary>
        /// The y-axis voxel size to use when sampling the source geometry.
        /// </summary>
        public float YCellSize
        {
            get { return root.yCellSize; }
            set { root.yCellSize = Math.Max(MinCellSize, value); }
        }

        /// <summary>
        /// Minimum floor to 'ceiling' height that will still allow the
        /// floor area to be considered traversable.
        /// </summary>
        /// <remarks>
        /// <p>Permits detection of overhangs in the source geometry that make 
        /// the geometry below un-walkable.</p>
        /// <p>Usually the maximum client height.</p></remarks>
        public float MinTraversableHeight
        {
            get { return root.minTraversableHeight; }
            set 
            {
                // Must be > 3 times the yCellSize.
                root.minTraversableHeight = 
                    Math.Max(Math.Max(3 * MinCellSize, 3 * root.yCellSize)
                        , value); 
            }
        }

        /// <summary>
        /// Maximum ledge height that is considered to still be
        /// traversable.  
        /// </summary>
        /// <remarks>
        /// <p>Allows the mesh to flow over curbs and up/down
        /// stairways.</p>
        /// <p>Usually set to how far up/down the client can step.</p>
        /// </remarks>
        public float MaxTraversableStep
        {
            get { return root.maxTraversableStep; }
            set { root.maxTraversableStep = Math.Max(3 * MinCellSize, value); }
        }

        /// <summary>
        /// The maximum slope that is considered traversable. (In degrees.)
        /// </summary>
        public float MaxTraversableSlope
        {
            get { return root.maxTraversableSlope; }
            set
            {
                root.maxTraversableSlope =
                    Math.Max(0, Math.Min(RCConfigEx.MaxAllowedSlope, value));
            }
        }

        /// <summary>
        /// Represents the closest any part of a mesh should get to an
        /// obstruction in the source geometry. (Usually the client radius.)
        /// </summary>
        public float TraversableAreaBorderSize
        {
            get { return root.traversableAreaBorderSize; }
            set { root.traversableAreaBorderSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The closest the mesh should come to the xz-plane's AABB of the
        /// source geometry.
        /// </summary>
        public float HeightfieldBorderSize
        {
            get { return root.heightfieldBorderSize; }
            set { root.heightfieldBorderSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum allowed length of triangles edges on the border of the
        /// mesh. (Extra vertices will be inserted if needed.)
        /// </summary>
        /// <remarks>
        /// A value of zero disabled this feature.</remarks>
        public float MaxEdgeLength
        {
            get { return root.maxEdgeLength; }
            set 
            { 
                // Any value less than the xzCell size is irrelavant.  In
                // such cases, disabled feature by snapping to zero.
                root.maxEdgeLength = (value < root.xzCellSize ? 0 : value);
            }
        }

        /// <summary>
        /// The maximum distance the edges of the mesh should deviate from
        /// the source geometry. (Applies only to the xz-plane.)
        /// </summary>
        public float EdgeMaxDeviation
        {
            get { return root.edgeMaxDeviation; }
            set { root.edgeMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// Sets the sampling distance to use when matching the
        /// mesh surface to the source geometry.
        /// </summary>
        public float ContourSampleDistance
        {
            get { return root.contourSampleDistance; }
            set { root.contourSampleDistance = value < 0.9f ? 0 : value; }
        }

        /// <summary>
        /// The maximum distance the mesh surface should deviate from the
        /// surface of the source geometry. 
        /// </summary>
        public float ContourMaxDeviation
        {
            get { return root.contourMaxDeviation; }
            set { root.contourMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// The amount of smoothing to be performed when generating the distance 
        /// field used for deriving regions.
        /// </summary>
        public int SmoothingThreshold
        {
            get { return root.smoothingThreshold; }
            set
            {
                // Must be between 0 and max allowed.
                root.smoothingThreshold = Math.Max(0, 
                        Math.Min(RCConfigEx.MaxAllowedSmoothing, value));
            }
        }

        /// <summary>
        /// The minimum area in world units allowed for isolated island meshes.
        /// (Prevents the formation of meshes that are too small to be
        /// of use.)
        /// </summary>
        /// <remarks>
        /// <p>This value represents an area in world units.  This differs from 
        /// some other API's, which use voxel area.</p>
        /// </remarks>
        public float MinIslandRegionSize
        {
            get { return mMinIslandRegionSize; }
            set
            {
                mMinIslandRegionSize = Math.Max(0, value);
                root.minIslandRegionSize = (int)(mMinIslandRegionSize
                    / (root.xzCellSize * root.xzCellSize));
            }
        }

        /// <summary>
        /// Any regions with an area smaller than this value will, if possible, be
        /// merged with larger regions.
        /// </summary>
        /// <remarks>
        /// <p>This value represents an area in world units.  This differs from 
        /// some other API's, which use voxel area.</p>
        /// </remarks>
        public float MergeRegionSize
        {
            get { return mMergeRegionSize; }
            set
            {
                mMergeRegionSize = Math.Max(0, value);
                root.mergeRegionSize = (int)(mMergeRegionSize
                    / (root.xzCellSize * root.xzCellSize));
            }
        }

        /// <summary>
        /// The maximum number of vertices allowed for polygons
        /// generated during the contour to polygon conversion process.
        /// </summary>
        public int MaxVertsPerPoly
        {
            get { return root.maxVertsPerPoly; }
            set
            {
                // Must be between 3 and max allowed.
                root.maxVertsPerPoly = Math.Max(3
                    , Math.Min(RCConfigEx.MaxAllowedVertsPerPoly, value));
            }
        }

        /// <summary>
        /// Indicates whether ledges should be considered un-walkable
        /// </summary>
        public bool ClipLedges
        {
            get { return root.clipLedges; }
            set { root.clipLedges = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private RCConfig()
        {
            /*
             * Design notes:
             * 
             * Don't make construction public.  It is too easy to make a bad
             * configuration from scratch.
             */

            root = new RCConfigEx();
        }

        public void LoadFrom(float xzCellSize
                , float yCellSize
                , float minTraversableHeight
                , float maxTraversableStep
                , float maxTraversableSlope
                , bool clipLedges
                , float traversableAreaBorderSize
                , float heightfieldBorderSize
                , int smoothingThreshold
                , float minIslandRegionSize
                , float mergeRegionSize
                , float maxEdgeLength
                , float edgeMaxDeviation
                , int maxVertsPerPoly
                , float contourSampleDistance
                , float contourMaxDeviation)
        {
            this.XZCellSize = xzCellSize;
            this.YCellSize = yCellSize;
            this.ClipLedges = clipLedges;
            this.ContourMaxDeviation = contourMaxDeviation;
            this.ContourSampleDistance = contourSampleDistance;
            this.EdgeMaxDeviation = edgeMaxDeviation;
            this.HeightfieldBorderSize = heightfieldBorderSize;
            this.MaxEdgeLength = maxEdgeLength;
            this.MaxTraversableSlope = maxTraversableSlope;
            this.MaxTraversableStep = maxTraversableStep;
            this.MaxVertsPerPoly = maxVertsPerPoly;
            this.MergeRegionSize = mergeRegionSize;
            this.MinIslandRegionSize = minIslandRegionSize;
            this.MinTraversableHeight = minTraversableHeight;
            this.SmoothingThreshold = smoothingThreshold;
            this.TraversableAreaBorderSize = traversableAreaBorderSize;
        }

        /// <summary>
        /// A configuration instance loaded with the default configuration.
        /// (All values are within allowed limits.)
        /// </summary>
        /// <returns>A configuration instance loaded with the default configuration.
        /// </returns>
        public static RCConfig GetDefault()
        {
            RCConfig config = new RCConfig();

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
                (float)Math.Pow(config.TraversableAreaBorderSize * 5, 2);
            config.MergeRegionSize = 
                (float)Math.Pow(config.TraversableAreaBorderSize * 10, 2);

            return config;
        }

        /// <summary>
        /// Gets a compact machine readable string that is useful for including 
        /// in save files.
        /// </summary>
        /// <returns>A machine readable string representing the configuration.
        /// </returns>
        /// <seealso cref="GetFromMachineString"/>
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
        /// <seealso cref="GetMachineString"/>
        public static RCConfig FromMachineString(string config)
        {
            // Perform a smoke test.  Any other format errors in the string will
            // result in a runtime error.
            if (config == null || !config.StartsWith(MachineTag))
                return null;

            char[] delim = { ':' };
            string[] sa = config.Split(delim);

            // Remember that the value at index zero is a tag, not a configuration
            // value.
            RCConfig result = new RCConfig();
            result.ClipLedges = (sa[1] == "0" ? false : true);
            result.ContourMaxDeviation = (float)Convert.ToDouble(sa[2]);
            result.ContourSampleDistance = (float)Convert.ToDouble(sa[3]);
            result.EdgeMaxDeviation = (float)Convert.ToDouble(sa[4]);
            result.HeightfieldBorderSize = (float)Convert.ToDouble(sa[5]);
            result.MaxEdgeLength = (float)Convert.ToDouble(sa[6]);
            result.MaxTraversableSlope = (float)Convert.ToDouble(sa[7]);
            result.MaxTraversableStep = (float)Convert.ToDouble(sa[8]);
            result.MaxVertsPerPoly = Convert.ToInt32(sa[9]);
            result.MergeRegionSize = (float)Convert.ToDouble(sa[10]);
            result.MinIslandRegionSize = (float)Convert.ToDouble(sa[11]);
            result.MinTraversableHeight = (float)Convert.ToDouble(sa[12]);
            result.SmoothingThreshold = Convert.ToInt32(sa[13]);
            result.TraversableAreaBorderSize = (float)Convert.ToDouble(sa[14]);
            result.XZCellSize = (float)Convert.ToDouble(sa[15]);
            result.YCellSize = (float)Convert.ToDouble(sa[16]);
            return result;
        }

        public static RCConfigEx ToConfigEx(RCConfig config)
        {
            return config.root;  // Provides a copy.
        }

        public static RCConfig GetFrom(RCConfigEx source)
        {
            RCConfig config = RCConfig.GetDefault();

            config.XZCellSize = source.xzCellSize;
            config.YCellSize = source.yCellSize;
            config.ClipLedges = source.clipLedges;
            config.ContourMaxDeviation = source.contourMaxDeviation;
            config.ContourSampleDistance = source.contourSampleDistance;
            config.EdgeMaxDeviation = source.edgeMaxDeviation;
            config.HeightfieldBorderSize = source.heightfieldBorderSize;
            config.MaxEdgeLength = source.maxEdgeLength;
            config.MaxTraversableSlope = source.maxTraversableSlope;
            config.MaxTraversableStep = source.maxTraversableStep;
            config.MaxVertsPerPoly = source.maxVertsPerPoly;
            config.MergeRegionSize = GetWorldArea(source.mergeRegionSize
                , source.xzCellSize);
            config.MinIslandRegionSize = GetWorldArea(source.minIslandRegionSize
                , source.xzCellSize);
            config.MinTraversableHeight = source.minTraversableHeight;
            config.SmoothingThreshold = source.smoothingThreshold;
            config.TraversableAreaBorderSize = source.traversableAreaBorderSize;

            return config;
        }

        public static RCConfig GetFrom(float xzCellSize
                , float yCellSize
                , float minTraversableHeight
                , float maxTraversableStep
                , float maxTraversableSlope
                , bool clipLedges
                , float traversableAreaBorderSize
                , float heightfieldBorderSize
                , int smoothingThreshold
                , float minIslandRegionSize
                , float mergeRegionSize
                , float maxEdgeLength
                , float edgeMaxDeviation
                , int maxVertsPerPoly
                , float contourSampleDistance
                , float contourMaxDeviation)
        {
            RCConfig config = RCConfig.GetDefault();

            config.XZCellSize = xzCellSize;
            config.YCellSize = yCellSize;
            config.ClipLedges = clipLedges;
            config.ContourMaxDeviation = contourMaxDeviation;
            config.ContourSampleDistance = contourSampleDistance;
            config.EdgeMaxDeviation = edgeMaxDeviation;
            config.HeightfieldBorderSize = heightfieldBorderSize;
            config.MaxEdgeLength = maxEdgeLength;
            config.MaxTraversableSlope = maxTraversableSlope;
            config.MaxTraversableStep = maxTraversableStep;
            config.MaxVertsPerPoly = maxVertsPerPoly;
            config.MergeRegionSize = mergeRegionSize;
            config.MinIslandRegionSize = minIslandRegionSize;
            config.MinTraversableHeight = minTraversableHeight;
            config.SmoothingThreshold = smoothingThreshold;
            config.TraversableAreaBorderSize = traversableAreaBorderSize;

            return config;
        }

        public static int GetCellArea(float worldArea, float xzCellSize)
        {
            return (int)(worldArea / (xzCellSize * xzCellSize));
        }

        public static float GetWorldArea(int cellArea, float xzCellSize)
        {
            return cellArea * xzCellSize * xzCellSize;
        }
    }
}
