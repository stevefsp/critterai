/*
 * Copyright (c) 2010 Stephen A. Pratt
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
using Math = System.Math;

/// <summary>
/// Specifies a configuration used to build navigation meshes.
/// </summary>
/// <remarks>
/// <p>The constructor is private.  Use the <see cref="GetDefault"/> method to 
/// get new instances of this class.</p>
/// <p>If you want to adjust the defaults to meat you need, just search for
/// constants that begin with "Default".  If there is no constant for
/// a configuation parameter, then it is a derived value.  In this case,
/// you will need to locate and update the "Standard" function that is used
/// to derive the value.</p>
/// </remarks>
[System.Serializable]
public sealed class NavmeshConfig
{
    /*
     * Design notes:
     * 
     * Why is there a NMGenConfig class and NavmeshGenerator.Configuration
     * structure that are exactly the same in data structure?  
     * Reason: Unity can't save custom structures. See
     * http://forum.unity3d.com/threads/36395-How-do-I-make-member-structs-accessible-in-the-editor.
     * So this class is used to attach a configuration to a Unity object,
     * while the structure is used to marshal data across the interop boundry.
     * 
     * This class is attached to Unity objects.  So it should never 
     * reference the NavmeshGenerator class.
     * 
     * This class is kept free of Unity references in order to permit
     * dual use with .NET.
     */

    // Default values.  (Only available for non-derived settings.)
    private const float DefaultXZResolution = 0.2f;
    private const float DefaultMinTraversableHeight = 1.5f;
    private const float DefaultMaxTraversableStep = 0.25f;
    private const float DefaultMaxTraversableSlope = 45.5f;
    private const float DefaultTraversableAreaBorderSize = DefaultXZResolution;
    private const float DefaultHeightfieldBorderSize = 0;
    private const int DefaultSmoothingThreshold = 1;
    private const int DefaultMinIslandRegionSize = 1024;
    private const int DefaultMergeRegionSize = 4096;
    private const int DefaultMaxVertsPerPoly = 6;
    private const bool DefaultClipLedges = false;

    // Values used to derive settings.

    private const float YResolutionFactor = 0.001f;
    private const float MinYResolution = 0.05f;
    private const float StepResolutionFactor = 0.4f;  // 1 / 2.5

    private const float XZResolutionFactor = 0.001f;
    private const float MinXZResolution = 0.1f;

    private const float MaxEdgeLengthFactor = 200;
    private const float MaxEdgeDevFactor = 10;

    private const float ContourSampleFactor = 0.001f;
    private const float ContourSampleFactorRough = 40;
    private const float ContourDeviationFactor = 20;

    /// <summary>
    /// The prefix tag for all machine strings.
    /// </summary>
    public const string MachineTag = "NMGC:";

    /// <summary>
    /// The xz-plane resolution to use when sampling the source geometry.
    /// </summary>
    public float xzResolution;

    /// <summary>
    /// The y-axis resolution to use when sampling the source geometry.
    /// </summary>
    public float yResolution;

    /// <summary>
    /// Minimum floor to ceiling height that will still allow the
    /// floor area to be considered traversable. (Usually the maximum
    /// agent height.)
    /// </summary>
    public float minTraversableHeight;

    /// <summary>
    /// Maximum ledge height that is considered to still be
    /// traversable.  Allows the mesh to flow over curbs and up/down
    /// stairways. (Usually how far up/down the agent can step.)
    /// </summary>
    public float maxTraversableStep;

    /// <summary>
    /// The maximum slope that is considered traversable. (In degrees.)
    /// </summary>
    public float maxTraversableSlope;

    /// <summary>
    /// Represents the closest any part of a mesh can get to an
    /// obstruction in the source geometry. (Usually the agent radius.)
    /// </summary>
    public float traversableAreaBorderSize;

    /// <summary>
    /// The closest the mesh may come to the xz-plane AABB of the
    /// source geometry.
    /// </summary>
    public float heightfieldBorderSize;

    /// <summary>
    /// The maximum length of polygon edges along the border of the
    /// mesh. (Extra vertices will be inserted if needed.)
    /// </summary>
    public float maxEdgeLength;

    /// <summary>
    /// The maximum distance the edges of the mesh may deviate from
    /// the source geometry. (Applies only to the xz-plane.)
    /// </summary>
    public float edgeMaxDeviation;

    /// <summary>
    /// Sets the sampling distance to use when matching the
    /// mesh surface to the source geometry.
    /// </summary>
    public float contourSampleDistance;

    /// <summary>
    /// The maximum distance the mesh surface can deviate from the
    /// surface of the source geometry. 
    /// </summary>
    public float contourMaxDeviation;

    /// <summary>
    /// The closest the mesh may come to the xz-plane AABB of the
    /// source geometry.
    /// </summary>
    public int smoothingThreshold;

    /// <summary>
    /// The minimum size allowed for isolated island meshes. (XZ cells.)
    /// (Prevents the formation of meshes that are too small to be"
    /// of use.)
    /// </summary>
    public int minIslandRegionSize;

    /// <summary>
    /// Any regions smaller than this size will, if possible, be
    /// merged with larger regions. (XZ cells.)
    /// </summary>
    public int mergeRegionSize;

    /// <summary>
    /// The maximum number of vertices per polygon for polygons
    /// generated during the contour to polygon conversion process.
    /// </summary>
    public int maxVertsPerPoly;

    /// <summary>
    /// Indicates whether ledges should be considered un-walkable
    /// </summary>
    public bool clipLedges;

    private float StandardYResolution
    {
        get { return maxTraversableStep * StepResolutionFactor; }
    }

    private float StandardMaxEdgeLength 
    {
        get { return xzResolution * MaxEdgeLengthFactor; }
    }

    private float StandardEdgeMaxDeviation 
    {
        get { return xzResolution * MaxEdgeDevFactor; }
    }

    private float StandardContourSampleDistance 
    {
        get { return xzResolution * ContourSampleFactorRough; }
    }

    private float StandardContourMaxDeviation
    {
        get { return yResolution * ContourDeviationFactor; }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    private NavmeshConfig()
    {
        /*
         * Design notes:
         * 
         * Don't make construction public.  It is too easy to make a bad
         * configuration from scratch.  Instead, adjust the various constants
         * and functions used by the GetDefault() method.
         */
    }

    /// <summary>
    /// Checks and adjusts certain settings that are likely to cause poor
    /// quality navigation meshes.
    /// </summary>
    /// <remarks>
    /// <p>Settings can have side effects on eachother, usually with one being the
    /// more important setting. For example, yResloution impacts accurate 
    /// detection of maxTraversableStep.  maxTraversable step is considered an
    /// important fixed value, so yResolution may be adjusted to meet the
    /// requirements of maxTraversable step.</p>
    /// <p>This method does not fix all issues, such as out of range values.  
    /// To get a full cleaning, fist call the ApplyMandatoryLimits method 
    /// provided by the CAI-NMGen libary (or its marshaller).  Then call 
    /// this method.</p>
    /// <p>The standard call order when applying muliple mutator methods
    /// is as follows:</p>
    /// <ol>
    /// <li><see cref="Derive"/></li>
    /// <li>The CAI-NMGen library's ApplyMandatoryLimits() method.
    /// (Or the one provided by the mashaller.)</li>
    /// <li><see cref="Clean()"/></li>
    /// <li><see cref="ApplyDecimalLimits()"/></li>
    /// </ol>
    /// </remarks>
    public void Clean()
    {
        /*
         * Design Notes: 
         * 
         * The order of some of the adjustments is significant, with dependant
         * adjustment being made later.
         * 
         */

        // Don't let things get too extreme.
        xzResolution = Math.Max(0.01f, xzResolution);
        maxTraversableStep = Math.Max(0.01f, maxTraversableStep);
        contourSampleDistance = Math.Max(0.01f, contourSampleDistance);

        // Need to make sure that the yResolution can support the step size.
        // But don't let things get too extreme.
        yResolution = Math.Min(StandardYResolution, yResolution);
        yResolution = Math.Max(0.01f, yResolution);

        // A height restriction less than 2 * yResolution will never produce
        // anything of worth.
        minTraversableHeight = Math.Max(yResolution * 2, minTraversableHeight);

        // If the following values are greather than zero, their effectiveness
        // is determined by their associated resolutions.
        traversableAreaBorderSize = (traversableAreaBorderSize == 0 ?
            0 : Math.Max(xzResolution, traversableAreaBorderSize));
        heightfieldBorderSize = (heightfieldBorderSize == 0 ?
            0 : Math.Max(xzResolution, heightfieldBorderSize));
        contourMaxDeviation = (contourMaxDeviation == 0 ?
            0 : Math.Max(yResolution, contourMaxDeviation));

        // Setting either of these values to zero can result in a lot
        // of useless artifacts.
        minIslandRegionSize = Math.Max(1, minIslandRegionSize);
        mergeRegionSize = Math.Max(1, mergeRegionSize);

    }

    /// <summary>
    /// Sets various settings based on the provided AABB bounds.
    /// </summary>
    /// <remarks>
    /// <p>The standard call order when applying muliple mutator methods
    /// is as follows:</p>
    /// <ol>
    /// <li><see cref="Derive"/></li>
    /// <li>The CAI-NMGen library's ApplyMandatoryLimits() method.
    /// (Or the one provided by the mashaller.)</li>
    /// <li><see cref="Clean()"/></li>
    /// <li><see cref="ApplyDecimalLimits()"/></li>
    /// </ol>
    /// </remarks>
    /// <param name="bounds">The AABB bounds of the source geometry associated 
    /// with this configuration.</param>
    public void Derive(float[] bounds)
    {
        if (bounds == null || bounds.Length < 6)
            return;

        float maxXZLength = Math.Max(bounds[3] - bounds[0]
            , bounds[2] - bounds[5]);

        xzResolution = maxXZLength * XZResolutionFactor;
        xzResolution = Math.Max(MinXZResolution, xzResolution);

        yResolution = (bounds[4] - bounds[1]) * YResolutionFactor;
        yResolution = Math.Max(MinYResolution, yResolution);

        contourSampleDistance = maxXZLength * ContourSampleFactor;
        contourMaxDeviation = yResolution * ContourDeviationFactor;
    }

    /// <summary>
    /// A convenience method that makes the settings more readable by
    /// rounding to a standard number of decimal places.
    /// (E.g. Changes '1.55193848458', to '1.55'.)
    /// </summary>
    /// <remarks>
    /// <p>The standard call order when applying muliple mutator methods
    /// is as follows:</p>
    /// <ol>
    /// <li><see cref="Derive"/></li>
    /// <li>The CAI-NMGen library's ApplyMandatoryLimits() method.
    /// (Or the one provided by the mashaller.)</li>
    /// <li><see cref="Clean()"/></li>
    /// <li><see cref="ApplyDecimalLimits()"/></li>
    /// </ol>
    /// </remarks>
    public void ApplyDecimalLimits()
    {
        contourMaxDeviation = (float)Math.Round(contourMaxDeviation, 2);
        contourSampleDistance = (float)Math.Round(contourSampleDistance, 2);
        edgeMaxDeviation = (float)Math.Round(edgeMaxDeviation, 2);
        heightfieldBorderSize = (float)Math.Round(heightfieldBorderSize, 2);
        maxEdgeLength = (float)Math.Round(maxEdgeLength, 2);
        maxTraversableSlope = (float)Math.Round(maxTraversableSlope, 2);
        maxTraversableStep = (float)Math.Round(maxTraversableStep, 2);
        minTraversableHeight = (float)Math.Round(minTraversableHeight, 2);
        traversableAreaBorderSize =
            (float)Math.Round(traversableAreaBorderSize, 2);
        xzResolution = (float)Math.Round(xzResolution, 2);
        yResolution = (float)Math.Round(yResolution, 2);
    }

    /// <summary>
    /// A configuration instance loaded with the default configuration.
    /// (All values are within allowed limits.)
    /// </summary>
    /// <returns>A configuration instance loaded with the default configuration.
    /// </returns>
    public static NavmeshConfig GetDefault()
    {
        NavmeshConfig result = new NavmeshConfig();
        result.xzResolution = DefaultXZResolution;
        result.minTraversableHeight = DefaultMinTraversableHeight;
        result.maxTraversableStep = DefaultMaxTraversableStep;
        result.maxTraversableSlope = DefaultMaxTraversableSlope;
        result.traversableAreaBorderSize = DefaultTraversableAreaBorderSize;
        result.heightfieldBorderSize = DefaultHeightfieldBorderSize;
        result.smoothingThreshold = DefaultSmoothingThreshold;
        result.minIslandRegionSize = DefaultMinIslandRegionSize;
        result.mergeRegionSize = DefaultMergeRegionSize;
        result.maxVertsPerPoly = DefaultMaxVertsPerPoly;
        result.clipLedges = DefaultClipLedges;

        result.yResolution = result.StandardYResolution;
        result.maxEdgeLength = result.StandardMaxEdgeLength;
        result.edgeMaxDeviation = result.StandardEdgeMaxDeviation;
        result.contourSampleDistance = result.StandardContourSampleDistance;
        result.contourMaxDeviation = result.StandardContourMaxDeviation;

        return result;
    }

    /// <summary>
    /// Gets a machine readable string that is useful for including in save
    /// files.
    /// </summary>
    /// <returns>The machine readable string representing the configuration.
    /// </returns>
    public string GetMachineString()
    {
        const string format = MachineTag 
            + "{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}"
            + ":{10}:{11}:{12}:{13}:{14}:{15}";
        return string.Format(format
            , (this.clipLedges ? 1 : 0)
            , this.contourMaxDeviation
            , this.contourSampleDistance
            , this.edgeMaxDeviation
            , this.heightfieldBorderSize
            , this.maxEdgeLength
            , this.maxTraversableSlope
            , this.maxTraversableStep
            , this.maxVertsPerPoly
            , this.mergeRegionSize
            , this.minIslandRegionSize
            , this.minTraversableHeight
            , this.smoothingThreshold
            , this.traversableAreaBorderSize
            , this.xzResolution
            , this.yResolution);
    }

    /// <summary>
    /// Geneates a configuation from a valid machine readable string. 
    /// </summary>
    /// <remarks>The beginning of the machine string can be identified
    /// the the following </remarks>
    /// <param name="config">The machine string to translate.</param>
    /// <returns>The configuration derived from the machine string.</returns>
    public static NavmeshConfig GetFromMachineString(string config)
    {
        // Perform a smoke test.  Any other format errors in the string will
        // result in a runtime error.
        if (config == null || !config.StartsWith(MachineTag))
            return null;

        char[] delim = { ':' };
        string[] sa = config.Split(delim);

        // Remember that the value at index zero is a tag, not a configuration
        // value.
        NavmeshConfig result = new NavmeshConfig();
        result.clipLedges = (sa[1] == "0" ? false : true);
        result.contourMaxDeviation = (float)Convert.ToDouble(sa[2]);
        result.contourSampleDistance = (float)Convert.ToDouble(sa[3]);
        result.edgeMaxDeviation = (float)Convert.ToDouble(sa[4]);
        result.heightfieldBorderSize = (float)Convert.ToDouble(sa[5]);
        result.maxEdgeLength = (float)Convert.ToDouble(sa[6]);
        result.maxTraversableSlope = (float)Convert.ToDouble(sa[7]);
        result.maxTraversableStep = (float)Convert.ToDouble(sa[8]);
        result.maxVertsPerPoly = Convert.ToInt32(sa[9]);
        result.mergeRegionSize = Convert.ToInt32(sa[10]);
        result.minIslandRegionSize = Convert.ToInt32(sa[11]);
        result.minTraversableHeight = (float)Convert.ToDouble(sa[12]);
        result.smoothingThreshold = Convert.ToInt32(sa[13]);
        result.traversableAreaBorderSize = (float)Convert.ToDouble(sa[14]);
        result.xzResolution = (float)Convert.ToDouble(sa[15]);
        result.yResolution = (float)Convert.ToDouble(sa[16]);
        return result;
    }

}
