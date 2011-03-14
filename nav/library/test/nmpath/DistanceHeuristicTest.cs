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

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Summary description for DistanceHeuristicTest
    /// </summary>
    [TestClass]
    public sealed class DistanceHeuristicTest
    {
        private TestContext testContextInstance;

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

       [TestMethod]
        public void TestGetManhattan() 
        {
            Assert.IsTrue(DistanceHeuristic.GetManhattan(-3, 0, 0, -1, 0, 0) == 2);
            Assert.IsTrue(DistanceHeuristic.GetManhattan(-2, 0, -1, -2, 0, 2) == 3);
            Assert.IsTrue(DistanceHeuristic.GetManhattan(1, -1, -2, -1, 2, 2) == 9);
            Assert.IsTrue(DistanceHeuristic.GetManhattan(4, 2, 0, 2, 3, -1) == 4);
        }

        [TestMethod]
        public void TestGetLongestAxis() 
        {
            Assert.IsTrue(DistanceHeuristic.GetLongestAxis(-3, 0, 0, -1, 0, 0) == 2);
            Assert.IsTrue(DistanceHeuristic.GetLongestAxis(-2, 0, -1, -2, 0, 2) == 3);
            Assert.IsTrue(DistanceHeuristic.GetLongestAxis(1, -1, -2, -1, 2, 2) == 4);
            Assert.IsTrue(DistanceHeuristic.GetLongestAxis(1, -1, -2, -4, 2, 2) == 5);
            Assert.IsTrue(DistanceHeuristic.GetLongestAxis(1, -1, -2, -4, 5, 2) == 6);
        }

        [TestMethod]
        public void TestGetHeuristicValue() 
        {
            Assert.IsTrue(DistanceHeuristic.GetHeuristicValue(DistanceHeuristicType.Manhattan, -3, 0, 0, -1, 0, 0) == 2);
            Assert.IsTrue(DistanceHeuristic.GetHeuristicValue(DistanceHeuristicType.Manhattan, -2, 0, -1, -2, 0, 2) == 3);
            Assert.IsTrue(DistanceHeuristic.GetHeuristicValue(DistanceHeuristicType.Manhattan, 1, -1, -2, -1, 2, 2) == 9);
            Assert.IsTrue(DistanceHeuristic.GetHeuristicValue(DistanceHeuristicType.Manhattan, 4, 2, 0, 2, 3, -1) == 4);
            Assert.IsTrue(DistanceHeuristic.GetHeuristicValue(DistanceHeuristicType.LongestAxis, -3, 0, 0, -1, 0, 0) == 2);
            Assert.IsTrue(DistanceHeuristic.GetHeuristicValue(DistanceHeuristicType.LongestAxis, -2, 0, -1, -2, 0, 2) == 3);
            Assert.IsTrue(DistanceHeuristic.GetHeuristicValue(DistanceHeuristicType.LongestAxis, 1, -1, -2, -1, 2, 2) == 4);
            Assert.IsTrue(DistanceHeuristic.GetHeuristicValue(DistanceHeuristicType.LongestAxis, 1, -1, -2, -4, 2, 2) == 5);
            Assert.IsTrue(DistanceHeuristic.GetHeuristicValue(DistanceHeuristicType.LongestAxis, 1, -1, -2, -4, 5, 2) == 6);
        }
    }
}
