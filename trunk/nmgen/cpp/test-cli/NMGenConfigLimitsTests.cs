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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace org.critterai.nmgen
{
    [TestClass]
    public sealed class NMGenConfigLimitsTests
    {

        [DllImport("cai-nmgen-cli")]
        private static extern void applyStandardLimits(
            ref Configuration config);

        [TestMethod]
        public void LowerLimitTest()
        {
            Configuration config = StandardConfig.GetInteropLowerLimitConfig();
            BuildConfig expected = StandardConfig.GetExpectedLowerLimits();

            applyStandardLimits(ref config);

            Assert.IsTrue(config.yResolution
                == expected.YResolution);
            Assert.IsTrue(config.xzResolution
                == expected.XZResolution);
            Assert.IsTrue(config.contourMaxDeviation
                == expected.ContourMaxDeviation);
            Assert.IsTrue(config.contourSampleDistance
                == expected.ContourSampleDistance);
            Assert.IsTrue(config.edgeMaxDeviation
                == expected.EdgeMaxDeviation);
            Assert.IsTrue(config.maxEdgeLength
                == expected.MaxEdgeLength);
            Assert.IsTrue(config.maxTraversableSlope
                == expected.MaxTraversableSlope);
            Assert.IsTrue(config.maxTraversableStep
                == expected.MaxTraversableStep);
            Assert.IsTrue(config.maxVertsPerPoly
                == expected.MaxVertsPerPoly);
            Assert.IsTrue(config.mergeRegionSize
                == expected.MergeRegionSize);
            Assert.IsTrue(config.minTraversableHeight
                == expected.MinTraversableHeight);
            Assert.IsTrue(config.minUnconnectedRegionSize
                == expected.MinUnconnectedRegionSize);
            Assert.IsTrue(config.smoothingThreshold
                == expected.SmoothingThreshold);
            Assert.IsTrue(config.traversableAreaBorderSize
                == expected.TraversableAreaBorderSize);
            Assert.IsTrue(config.heightfieldBorderSize
                == expected.HeightfieldBorderSize);
        }

        [TestMethod]
        public void UpperLimitTest()
        {
            Configuration config = StandardConfig.GetInteropUpperLimitConfig();
            BuildConfig expected = StandardConfig.GetExpectedUpperLimits();

            applyStandardLimits(ref config);

            Assert.IsTrue(config.yResolution
                == expected.YResolution);
            Assert.IsTrue(config.xzResolution
                == expected.XZResolution);
            Assert.IsTrue(config.contourMaxDeviation
                == expected.ContourMaxDeviation);
            Assert.IsTrue(config.contourSampleDistance
                == expected.ContourSampleDistance);
            Assert.IsTrue(config.edgeMaxDeviation
                == expected.EdgeMaxDeviation);
            Assert.IsTrue(config.maxEdgeLength
                == expected.MaxEdgeLength);
            Assert.IsTrue(config.maxTraversableSlope
                == expected.MaxTraversableSlope);
            Assert.IsTrue(config.maxTraversableStep
                == expected.MaxTraversableStep);
            Assert.IsTrue(config.maxVertsPerPoly
                == expected.MaxVertsPerPoly);
            Assert.IsTrue(config.mergeRegionSize
                == expected.MergeRegionSize);
            Assert.IsTrue(config.minTraversableHeight
                == expected.MinTraversableHeight);
            Assert.IsTrue(config.minUnconnectedRegionSize
                == expected.MinUnconnectedRegionSize);
            Assert.IsTrue(config.smoothingThreshold
                == expected.SmoothingThreshold);
            Assert.IsTrue(config.traversableAreaBorderSize
                == expected.TraversableAreaBorderSize);
            Assert.IsTrue(config.heightfieldBorderSize
                == expected.HeightfieldBorderSize);
        }
    }

}
