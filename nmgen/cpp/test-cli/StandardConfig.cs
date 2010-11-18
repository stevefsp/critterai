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

namespace org.critterai.nmgen
{
    public static class StandardConfig
    {

        /*
         * Design notes:
         * 
         * There should be no duplicate values in the standard value
         * constants.
         * 
         * Basing construction on BuildConfig since its structure is
         * guarenteed correct. (Unlike the Configuration structure which
         * is a manual copy of the unmanaged structure.)
         *  
         */
      

        // Design note: There should be no duplicate values
        // in these constants.
        public const float yResolution = 0.1f;
        public const float xzResolution = 0.2f;
        public const float minTraversableHeight = 2.1f;
        public const float maxTraversableStep = 0.5f;
        public const float maxTraversableSlope = 48.0f;
        public const bool clipLedges = true;
        public const float traversableAreaBorderSize = 0.3f;
        public const int smoothingThreshold = 2;
        public const int minUnconnectedRegionSize = 1000;
        public const int mergeRegionSize = 2000;
        public const float maxEdgeLength = 5.0f;
        public const float edgeMaxDeviation = 0.8f;
        public const int maxVertsPerPoly = 4;
        public const float contourSampleDistance = 1.0f;
        public const float contourMaxDeviation = 0.55f;
        public const float heightfieldBoarderSize = 0.15f;

        public static BuildConfig GetCLIValidConfig()
        {
            return new BuildConfig(xzResolution
                , yResolution
                , minTraversableHeight
                , maxTraversableStep
                , maxTraversableSlope
                , clipLedges
                , traversableAreaBorderSize
                , heightfieldBoarderSize
                , smoothingThreshold
                , minUnconnectedRegionSize
                , mergeRegionSize
                , maxEdgeLength
                , edgeMaxDeviation
                , maxVertsPerPoly
                , contourSampleDistance
                , contourMaxDeviation);
        }

        public static BuildConfig GetCLILowerLimitConfig()
        {
            return new BuildConfig(0
                , 0
                , 0
                , -1
                , -1
                , clipLedges
                , -1
                , -1
                , -1
                , 0
                , -1
                , -1
                , -1
                , 2
                , 0.89f
                , -1);
        }

        public static BuildConfig GetCLIUpperLimitConfig()
        {
            return new BuildConfig(xzResolution
                , yResolution
                , minTraversableHeight
                , maxTraversableStep
                , BuildConfig.MaxAllowedSlope + 0.1f
                , clipLedges
                , traversableAreaBorderSize
                , heightfieldBoarderSize
                , BuildConfig.MaxSmoothing + 1
                , minUnconnectedRegionSize
                , mergeRegionSize
                , maxEdgeLength
                , edgeMaxDeviation
                , maxVertsPerPoly
                , contourSampleDistance
                , contourMaxDeviation);
        }

        public static BuildConfig GetExpectedLowerLimits()
        {
            return new BuildConfig(BuildConfig.StandardEpsilon
                , BuildConfig.StandardEpsilon
                , BuildConfig.StandardEpsilon
                , 0
                , 0
                , false
                , 0
                , 0
                , 0
                , 1
                , 0
                , 0
                , 0
                , 3
                , 0
                , 0);
        }

        public static BuildConfig GetExpectedUpperLimits()
        {
            return new BuildConfig(xzResolution
                , yResolution
                , minTraversableHeight
                , maxTraversableStep
                , BuildConfig.MaxAllowedSlope
                , clipLedges
                , traversableAreaBorderSize
                , heightfieldBoarderSize
                , BuildConfig.MaxSmoothing
                , minUnconnectedRegionSize
                , mergeRegionSize
                , maxEdgeLength
                , edgeMaxDeviation
                , maxVertsPerPoly
                , contourSampleDistance
                , contourMaxDeviation);
        }

        public static Configuration GetInteropLowerLimitConfig()
        {
            return Translate(GetCLILowerLimitConfig());
        }

        public static Configuration GetInteropUpperLimitConfig()
        {
            return Translate(GetCLIUpperLimitConfig());
        }

        private static Configuration Translate(BuildConfig config)
        {
            Configuration result = new Configuration();
            result.xzResolution = config.XZResolution;
            result.yResolution = config.YResolution;
            result.minTraversableHeight = config.MinTraversableHeight;
            result.maxTraversableStep = config.MaxTraversableStep;
            result.maxTraversableSlope = config.MaxTraversableSlope;
            result.clipLedges = config.ClipLedges;
            result.traversableAreaBorderSize = config.TraversableAreaBorderSize;
            result.heightfieldBorderSize = config.HeightfieldBorderSize;
            result.smoothingThreshold = config.SmoothingThreshold;
            result.minUnconnectedRegionSize = config.MinUnconnectedRegionSize;
            result.mergeRegionSize = config.MergeRegionSize;
            result.maxEdgeLength = config.MaxEdgeLength;
            result.edgeMaxDeviation = config.EdgeMaxDeviation;
            result.maxVertsPerPoly = config.MaxVertsPerPoly;
            result.contourSampleDistance = config.ContourSampleDistance;
            result.contourMaxDeviation = config.ContourMaxDeviation;
            return result;
        }

    }
}
