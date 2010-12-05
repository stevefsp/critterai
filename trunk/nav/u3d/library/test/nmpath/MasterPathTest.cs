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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.critterai.math;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Summary description for MasterPathTest
    /// </summary>
    [TestClass]
    public sealed class MasterPathTest
    {

        /*
         * Design notes:
         * 
         * Default values and immutable fields are validated in constructor
         * tests.
         * Getters for mutable fields are tested with their associated mutators.
         */

        private const float OFFSET_SCALE = 0.1f;
        private const int PATH_ID = 5;
        
        private ITestMesh mMesh;
        private float[] verts;
        private int[] indices;
        private TriCell[] cells;

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

        [TestInitialize()]
        public void SetUp()
        {
            mMesh = new CorridorMesh();
            verts = mMesh.GetVerts();
            indices = mMesh.GetIndices();
            cells = TestUtil.GetAllCells(verts, indices);
            TestUtil.LinkAllCells(cells);
        }

        [TestMethod]
        public void TestConstructionSingle() 
        {
            MasterPath mp = new MasterPath(PATH_ID
                    , cells[0]
                    , mMesh.PlaneTolerance
                    , OFFSET_SCALE);
            Assert.IsTrue(mp.Size == 1);
            Assert.IsTrue(mp.GetCell(0) == cells[0]);
            Assert.IsTrue(mp.StartCell == cells[0]);
            Assert.IsTrue(mp.GoalCell == cells[0]);    
            Assert.IsTrue(mp.Id == PATH_ID);
            Assert.IsTrue(mp.IsDisposed == false);
        }
        
        [TestMethod]
        public void TestConstructorMain() 
        {
            TriCell[] pathCells = getPathCells(0);
            MasterPath mp = new MasterPath(PATH_ID
                    , pathCells
                    , mMesh.PlaneTolerance
                    , OFFSET_SCALE);
            Assert.IsTrue(mp.Size == pathCells.Length);
            Assert.IsTrue(mp.StartCell == pathCells[0]);
            Assert.IsTrue(mp.GoalCell == pathCells[pathCells.Length - 1]);        
            Assert.IsTrue(mp.Id == PATH_ID);
            Assert.IsTrue(mp.IsDisposed == false);
        }

        [TestMethod]
        public void TestDispose() 
        {
            MasterPath mp = new MasterPath(PATH_ID
                    , cells[0]
                    , mMesh.PlaneTolerance
                    , OFFSET_SCALE);
            Assert.IsTrue(mp.IsDisposed == false);
            mp.Dispose();
            Assert.IsTrue(mp.IsDisposed == true);
        }

        [TestMethod]
        public void TestResetTimestamp() 
        {
            // Can't to an exact test on this one.
            MasterPath mp = new MasterPath(PATH_ID
                    , cells[0]
                    , mMesh.PlaneTolerance
                    , OFFSET_SCALE);
            Assert.IsTrue(MathUtil.SloppyEquals(mp.Timestamp, DateTime.Now.Ticks, 2));
            try
            {
                Thread.Sleep(10);
            }
            finally { }
            long oldts = mp.Timestamp;
            mp.ResetTimestamp();
            Assert.IsTrue(MathUtil.SloppyEquals(mp.Timestamp, DateTime.Now.Ticks, 2));
            Assert.IsTrue(mp.Timestamp != oldts);
        }

        [TestMethod]
        public void TestGetPath() 
        {
            // Only performs basic validations.  The
            // full accuracy of the returned reference
            // is tested in the Path class's test suite.
            MasterPath mp = new MasterPath(PATH_ID
                    , cells[0]
                    , mMesh.PlaneTolerance
                    , OFFSET_SCALE);
            float[] points = mMesh.GetPathPoints(0);
            Assert.IsTrue(mp.GetPath(points[3], points[4], points[5])
                    != mp.GetPath(points[3], points[4], points[5]));  // New object per call.
            Assert.IsTrue(mp.GetPath(points[3], points[4], points[5]).Id == PATH_ID);
        }

        [TestMethod]
        public void TestGetCell() 
        {
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                TriCell[] pathCells = getPathCells(iPath);
                MasterPath mp = new MasterPath(PATH_ID
                        , pathCells
                        , mMesh.PlaneTolerance
                        , OFFSET_SCALE);
                for (int iCell = 0; iCell < mp.Size; iCell++)
                {
                    Assert.IsTrue(mp.GetCell(iCell) == pathCells[iCell]);
                    testCount++;
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }

        [TestMethod]
        public void TestGetCellIndex() 
        {
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                TriCell[] pathCells = getPathCells(iPath);
                MasterPath mp = new MasterPath(PATH_ID
                        , pathCells
                        , mMesh.PlaneTolerance
                        , OFFSET_SCALE);
                for (int iCell = 0; iCell < mp.Size; iCell++)
                {
                    Assert.IsTrue(mp.GetCellIndex(pathCells[iCell]) == iCell);
                    testCount++;
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }

        [TestMethod]
        public void TestGetRawCopy() 
        {
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                TriCell[] pathCells = getPathCells(iPath);
                MasterPath mp = new MasterPath(PATH_ID
                        , pathCells
                        , mMesh.PlaneTolerance
                        , OFFSET_SCALE);
                TriCell[] pc = new TriCell[mp.Size];
                TriCell[] outPathCells = mp.GetRawCopy(pc);
                Assert.IsTrue(outPathCells == pc);
                Assert.IsTrue(outPathCells.Length == pathCells.Length);
                for (int iCell = 0; iCell < outPathCells.Length; iCell++)
                {
                    Assert.IsTrue(outPathCells[iCell] == pathCells[iCell]);
                    testCount++;
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }
        
        private TriCell[] getPathCells(int pathIndex)
        {
            int[] pathPolys = mMesh.GetPathPolys(pathIndex);
            TriCell[] result = new TriCell[pathPolys.Length];
            for (int i = 0; i < pathPolys.Length; i++)
            {
                result[i] = cells[pathPolys[i]];
            }
            return result;
        }
    }
}
