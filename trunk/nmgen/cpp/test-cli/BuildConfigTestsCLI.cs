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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace org.critterai.nmgen
{
    [TestClass]
    public sealed class BuildConfigTestsCLI
    {
        [TestMethod]
        public void ConstructionTest()
        {
            BuildConfig config = StandardConfig.GetCLIValidConfig();

            Assert.IsTrue(config.YCellSize 
                == StandardConfig.yCellSize);
            Assert.IsTrue(config.XZCellSize 
                == StandardConfig.xzCellSize);
            Assert.IsTrue(config.ClipLedges 
                == StandardConfig.clipLedges);
            Assert.IsTrue(config.ContourMaxDeviation
                == StandardConfig.contourMaxDeviation);
            Assert.IsTrue(config.ContourSampleDistance
                == StandardConfig.contourSampleDistance);
            Assert.IsTrue(config.EdgeMaxDeviation 
                == StandardConfig.edgeMaxDeviation);
            Assert.IsTrue(config.MaxEdgeLength 
                == StandardConfig.maxEdgeLength);
            Assert.IsTrue(config.MaxTraversableSlope 
                == StandardConfig.maxTraversableSlope);
            Assert.IsTrue(config.MaxTraversableStep 
                == StandardConfig.maxTraversableStep);
            Assert.IsTrue(config.MaxVertsPerPoly 
                == StandardConfig.maxVertsPerPoly);
            Assert.IsTrue(config.MergeRegionSize 
                == StandardConfig.mergeRegionSize);
            Assert.IsTrue(config.MinTraversableHeight 
                == StandardConfig.minTraversableHeight);
            Assert.IsTrue(config.MinIslandRegionSize
                == StandardConfig.minIslandRegionSize);
            Assert.IsTrue(config.SmoothingThreshold 
                == StandardConfig.smoothingThreshold);
            Assert.IsTrue(config.TraversableAreaBorderSize 
                == StandardConfig.traversableAreaBorderSize);
            Assert.IsTrue(config.HeightfieldBorderSize
                == StandardConfig.heightfieldBoarderSize);
        }

        [TestMethod]
        public void LowerLimitTest()
        {
            BuildConfig config = StandardConfig.GetCLILowerLimitConfig();
            BuildConfig expected = StandardConfig.GetExpectedLowerLimits();

            config.ApplyLimits();
            
            Assert.IsTrue(config.YCellSize 
                == expected.YCellSize);
            Assert.IsTrue(config.XZCellSize 
                == expected.XZCellSize);
            Assert.IsTrue(config.ContourMaxDeviation 
                == expected.ContourMaxDeviation);
            Assert.IsTrue(config.ContourSampleDistance 
                == expected.ContourSampleDistance);
            Assert.IsTrue(config.EdgeMaxDeviation 
                == expected.EdgeMaxDeviation);
            Assert.IsTrue(config.MaxEdgeLength 
                == expected.MaxEdgeLength);
            Assert.IsTrue(config.MaxTraversableSlope 
                == expected.MaxTraversableSlope);
            Assert.IsTrue(config.MaxTraversableStep 
                == expected.MaxTraversableStep);
            Assert.IsTrue(config.MaxVertsPerPoly 
                == expected.MaxVertsPerPoly);
            Assert.IsTrue(config.MergeRegionSize 
                == expected.MergeRegionSize);
            Assert.IsTrue(config.MinTraversableHeight 
                == expected.MinTraversableHeight);
            Assert.IsTrue(config.MinIslandRegionSize 
                == expected.MinIslandRegionSize);
            Assert.IsTrue(config.SmoothingThreshold 
                == expected.SmoothingThreshold);
            Assert.IsTrue(config.TraversableAreaBorderSize 
                == expected.TraversableAreaBorderSize);
            Assert.IsTrue(config.HeightfieldBorderSize 
                == expected.HeightfieldBorderSize);
        }

        [TestMethod]
        public void UpperLimitTest()
        {
            BuildConfig config = StandardConfig.GetCLIUpperLimitConfig();
            BuildConfig expected = StandardConfig.GetExpectedUpperLimits();

            config.ApplyLimits();

            Assert.IsTrue(config.YCellSize
                == expected.YCellSize);
            Assert.IsTrue(config.XZCellSize
                == expected.XZCellSize);
            Assert.IsTrue(config.ContourMaxDeviation
                == expected.ContourMaxDeviation);
            Assert.IsTrue(config.ContourSampleDistance
                == expected.ContourSampleDistance);
            Assert.IsTrue(config.EdgeMaxDeviation
                == expected.EdgeMaxDeviation);
            Assert.IsTrue(config.MaxEdgeLength
                == expected.MaxEdgeLength);
            Assert.IsTrue(config.MaxTraversableSlope
                == expected.MaxTraversableSlope);
            Assert.IsTrue(config.MaxTraversableStep
                == expected.MaxTraversableStep);
            Assert.IsTrue(config.MaxVertsPerPoly
                == expected.MaxVertsPerPoly);
            Assert.IsTrue(config.MergeRegionSize
                == expected.MergeRegionSize);
            Assert.IsTrue(config.MinTraversableHeight
                == expected.MinTraversableHeight);
            Assert.IsTrue(config.MinIslandRegionSize
                == expected.MinIslandRegionSize);
            Assert.IsTrue(config.SmoothingThreshold
                == expected.SmoothingThreshold);
            Assert.IsTrue(config.TraversableAreaBorderSize
                == expected.TraversableAreaBorderSize);
            Assert.IsTrue(config.HeightfieldBorderSize
                == expected.HeightfieldBorderSize);
        }
    }
}
