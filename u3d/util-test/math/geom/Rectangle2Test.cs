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
using org.critterai.math.geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace org.critterai.math.geom
{
    
    
    /// <summary>
    ///This is a test class for Rectangle2Test and is intended
    ///to contain all Rectangle2Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Rectangle2Test
    {
        private TestContext testContextInstance;

        private const float XMIN = -3;
        private const float YMIN = 2;
        private const float XMAX = 2;
        private const float YMAX = 6;
        
        private const float TOLERANCE = MathUtil.TOLERANCE_STD;
        private const float OFFSET = 1.5f;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        ///A test for Rectangle2.IntersectsAABB
        ///</summary>
        [TestMethod()]
        public void IntersectsAABBTest()
        {
            // Complete overlap
            Assert.IsTrue(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMIN, YMIN, XMAX, YMAX));

            // A fully Rectangle2.Contains B
            Assert.IsTrue(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMIN + TOLERANCE, YMIN + TOLERANCE, XMAX - TOLERANCE, YMAX - TOLERANCE));

            // Wall tests

            // A xmin overlaps B xmax
            Assert.IsTrue(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                                , XMIN - OFFSET, YMIN + OFFSET, XMIN, YMAX - OFFSET));
            // A xmax overlaps B xmin
            Assert.IsTrue(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                                , XMAX, YMIN + OFFSET, XMAX + OFFSET, YMAX - OFFSET));
            // A ymin overlaps B ymax
            Assert.IsTrue(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                                , XMIN + OFFSET, YMIN - OFFSET, XMAX - OFFSET, YMIN));
            // A ymax overlaps B ymin
            Assert.IsTrue(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                                , XMIN + OFFSET, YMAX, XMAX - OFFSET, YMAX + OFFSET));

            // A xmin above B xmax
            Assert.IsFalse(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMIN - OFFSET, YMIN + OFFSET, XMIN - TOLERANCE, YMAX - OFFSET));
            // A xmax below B xmin
            Assert.IsFalse(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMAX + TOLERANCE, YMIN + OFFSET, XMAX + OFFSET, YMAX - OFFSET));
            // A ymin above B ymax
            Assert.IsFalse(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMIN + OFFSET, YMIN - OFFSET, XMAX - OFFSET, YMIN - TOLERANCE));
            // A ymax below B ymin
            Assert.IsFalse(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMIN + OFFSET, YMAX + TOLERANCE, XMAX - OFFSET, YMAX + OFFSET));

            // Corner tests.

            // B fully below A xmin and A ymin
            Assert.IsFalse(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMIN - OFFSET, YMIN - OFFSET, XMIN - TOLERANCE, YMIN - TOLERANCE));
            // B fully above A xmax and A ymax
            Assert.IsFalse(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMAX + TOLERANCE, YMAX + TOLERANCE, XMAX + OFFSET, YMAX + OFFSET));
            // B above and to right of A
            Assert.IsFalse(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMIN - OFFSET, YMAX + TOLERANCE, XMIN - TOLERANCE, YMAX + OFFSET));
            // B below and to the left of A
            Assert.IsFalse(Rectangle2.IntersectsAABB(XMIN, YMIN, XMAX, YMAX
                    , XMAX + TOLERANCE, YMIN - OFFSET, XMAX + OFFSET, YMIN - TOLERANCE));
        }

        /// <summary>
        ///A test for Rectangle2.Contains
        ///</summary>
        [TestMethod()]
        public void ContainsPointTest()
        {
            // Wall tests.

            // On x Min bounds.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN, YMIN + OFFSET));
            // On y Min bounds.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN + OFFSET, YMIN));
            // On x Max bounds.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMAX, YMIN + OFFSET));
            // On y Max bounds.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN + OFFSET, YMAX));

            // Inside x Min bounds.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN + TOLERANCE, YMIN + OFFSET));
            // Inside y Min bounds.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN + OFFSET, YMIN + TOLERANCE));
            // Inside x Max bounds.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMAX - TOLERANCE, YMIN + OFFSET));
            // Inside y Max bounds.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN + OFFSET, YMAX - TOLERANCE));

            // Outside x Min bounds.
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN - TOLERANCE, YMIN + OFFSET));
            // Outside y Min bounds.
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN + OFFSET, YMIN - TOLERANCE));
            // Outside x Max bounds.
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMAX + TOLERANCE, YMIN + OFFSET));
            // Outside y Max bounds.
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN + OFFSET, YMAX + TOLERANCE));

            // Corner tests.

            // On minX/minY corner
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN, YMIN));
            // On minX/maxY corner
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN, YMAX));
            // On maxX/maxY corner.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMAX, YMAX));
            // On maxX/minY corner.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMAX, YMIN));

            // Outside minX/minY corner
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN - TOLERANCE, YMIN - TOLERANCE));
            // Outside minX/maxY corner
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMIN - TOLERANCE, YMAX + TOLERANCE));
            // Outside maxX/maxY corner.
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMAX + TOLERANCE, YMAX + TOLERANCE));
            // Outside maxX/minY corner.
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN
                    , XMAX, YMAX
                    , XMAX + TOLERANCE, YMIN - TOLERANCE));
        }

        /// <summary>
        ///A test for Rectangle2.Contains
        ///</summary>
        [TestMethod()]
        public void ContainsAABBTest()
        {
            // A == B
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN, YMIN, XMAX, YMAX));
            // B Rectangle2.Contains A
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN - TOLERANCE, YMIN - TOLERANCE, XMAX + TOLERANCE, YMAX + TOLERANCE));
            // B slightly smaller than A.
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN + TOLERANCE, YMIN + TOLERANCE, XMAX - TOLERANCE, YMAX - TOLERANCE));

            // X-axis wall tests
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN, YMIN + OFFSET, XMAX - TOLERANCE, YMAX - OFFSET));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN, YMIN + OFFSET, XMAX + TOLERANCE, YMAX - OFFSET));
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN + TOLERANCE, YMIN + OFFSET, XMAX, YMAX - OFFSET));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN - TOLERANCE, YMIN + OFFSET, XMAX, YMAX - OFFSET));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN - OFFSET, YMIN + OFFSET, XMIN + TOLERANCE, YMAX - OFFSET));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMAX + TOLERANCE, YMIN + OFFSET, XMAX + OFFSET, YMAX - OFFSET));

            // Y-axis wall tests
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN + OFFSET, YMIN, XMAX - OFFSET, YMAX - TOLERANCE));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN + OFFSET, YMIN, XMAX - OFFSET, YMAX + TOLERANCE));
            Assert.IsTrue(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN + OFFSET, YMIN + TOLERANCE, XMAX - OFFSET, YMAX));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN + OFFSET, YMIN - TOLERANCE, XMAX - OFFSET, YMAX));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN + OFFSET, YMIN - OFFSET, XMAX - OFFSET, YMIN + TOLERANCE));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN + OFFSET, YMAX + TOLERANCE, XMAX - OFFSET, YMAX + OFFSET));

            // Corner tests
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN - OFFSET, YMIN - OFFSET, XMIN + TOLERANCE, YMIN + TOLERANCE));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMIN - OFFSET, YMAX - TOLERANCE, XMIN + TOLERANCE, YMAX + OFFSET));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMAX - TOLERANCE, YMAX - TOLERANCE, XMAX + OFFSET, YMAX + OFFSET));
            Assert.IsFalse(Rectangle2.Contains(XMIN, YMIN, XMAX, YMAX
                    , XMAX - TOLERANCE, YMIN - OFFSET, XMAX + OFFSET, YMIN + TOLERANCE));
        }

    }
}
