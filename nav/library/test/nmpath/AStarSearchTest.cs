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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Summary description for AStarSearchTest
    /// </summary>
    [TestClass]
    public sealed class AStarSearchTest
    {
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
        public void TestAStarSearch() 
        {
            AStarSearch search = new AStarSearch(DistanceHeuristicType.LongestAxis);
            Assert.IsTrue(search.GoalX == 0);
            Assert.IsTrue(search.GoalY == 0);
            Assert.IsTrue(search.GoalZ == 0);
            Assert.IsTrue(search.IsActive == false);
            Assert.IsTrue(search.State == SearchState.Uninitialized);
            Assert.IsTrue(search.PathCells == null);
        }
        
        [TestMethod]
        public void TestInitialize() 
        {
            AStarSearch search = new AStarSearch(DistanceHeuristicType.LongestAxis);
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                float[] pathPoints = mMesh.GetPathPoints(iPath);
                int[] pathCells = mMesh.GetPathPolys(iPath);
                SearchState result = search.Initialize(pathPoints[0]
                                             , pathPoints[1]
                                             , pathPoints[2]
                                             , pathPoints[3]
                                             , pathPoints[4]
                                             , pathPoints[5]
                                             , cells[pathCells[0]]
                                             , cells[pathCells[pathCells.Length-1]]);
                Assert.IsTrue(result == SearchState.Initialized);
                Assert.IsTrue(search.GoalX == pathPoints[3]);
                Assert.IsTrue(search.GoalY == pathPoints[4]);
                Assert.IsTrue(search.GoalZ == pathPoints[5]);
                Assert.IsTrue(search.IsActive == true);
                Assert.IsTrue(search.State == SearchState.Initialized);
                Assert.IsTrue(search.PathCells == null);
                testCount++;
            }
            if (testCount < 2)
                // Needs two loops to properly validate re-use.
                Assert.Fail("Mesh doesn't support test.");
        }
        
        [TestMethod]
        public void TestReset() 
        {
            AStarSearch search = new AStarSearch(DistanceHeuristicType.LongestAxis);
            if (mMesh.GetPathCount < 1)
                Assert.Fail("Mesh doesn't support test.");
            float[] pathPoints = mMesh.GetPathPoints(0);
            int[] pathCells = mMesh.GetPathPolys(0);
            SearchState result = search.Initialize(pathPoints[0]
                                         , pathPoints[1]
                                         , pathPoints[2]
                                         , pathPoints[3]
                                         , pathPoints[4]
                                         , pathPoints[5]
                                         , cells[pathCells[0]]
                                         , cells[pathCells[pathCells.Length-1]]);
            // Duplicated tests.
            Assert.IsTrue(result == SearchState.Initialized);
            Assert.IsTrue(search.GoalX == pathPoints[3]);
            Assert.IsTrue(search.GoalY == pathPoints[4]);
            Assert.IsTrue(search.GoalZ == pathPoints[5]);
            Assert.IsTrue(search.IsActive == true);
            Assert.IsTrue(search.State == SearchState.Initialized);
            Assert.IsTrue(search.PathCells == null);
            // Primary tests.
            search.Reset();
            Assert.IsTrue(search.GoalX == 0);
            Assert.IsTrue(search.GoalY == 0);
            Assert.IsTrue(search.GoalZ == 0);
            Assert.IsTrue(search.IsActive == false);
            Assert.IsTrue(search.State == SearchState.Uninitialized);
            Assert.IsTrue(search.PathCells == null);
        }

        [TestMethod]
        public void TestProcessAndResultSuccess() 
        {
            AStarSearch search = new AStarSearch(DistanceHeuristicType.LongestAxis);
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                float[] pathPoints = mMesh.GetPathPoints(iPath);
                int[] pathCells = mMesh.GetPathPolys(iPath);
                SearchState state = search.Initialize(pathPoints[0]
                                             , pathPoints[1]
                                             , pathPoints[2]
                                             , pathPoints[3]
                                             , pathPoints[4]
                                             , pathPoints[5]
                                             , cells[pathCells[0]]
                                             , cells[pathCells[pathCells.Length-1]]);
                int maxProcessCalls = mMesh.PolyCount;
                int processCalls = 1;
                Assert.IsTrue(search.Process() == SearchState.Processing);
                while (search.IsActive && processCalls <= maxProcessCalls)
                {
                    state = search.Process();
                    if (search.IsActive)
                        Assert.IsTrue(state == SearchState.Processing);
                    processCalls++;
                }
                Assert.IsTrue(search.State == SearchState.Complete);
                TriCell[] path = search.PathCells;
                Assert.IsTrue(path != null);
                Assert.IsTrue(path.Length == pathCells.Length);
                for (int i = 0; i < path.Length; i++)
                {
                    Assert.IsTrue(path[i] == cells[pathCells[i]]);
                }
                testCount++;
            }
            if (testCount < 2)
                // Needs two loops to properly validate re-use.
                Assert.Fail("Mesh doesn't support test.");
        }    
        
        [TestMethod]
        public void TestProcessAndResultFail() 
        {
            AStarSearch search = new AStarSearch(DistanceHeuristicType.LongestAxis);
            int testCount = 0;
            float[] v = { 
                    0, 0, 0 
                    , 1, 0, 1
                    , 1, 0, 0
                };
            TriCell goalCell = new TriCell(v, 0, 1, 2);
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                float[] pathPoints = mMesh.GetPathPoints(iPath);
                int[] pathCells = mMesh.GetPathPolys(iPath);
                SearchState state = search.Initialize(pathPoints[0]
                                             , pathPoints[1]
                                             , pathPoints[2]
                                             , v[0]
                                             , v[1]
                                             , v[2]
                                             , cells[pathCells[0]]
                                             , goalCell);
                int maxProcessCalls = mMesh.PolyCount;
                int processCalls = 0;
                while (search.IsActive && processCalls <= maxProcessCalls + 1)
                {
                    state = search.Process();
                    if (search.IsActive)
                        Assert.IsTrue(state == SearchState.Processing);
                    processCalls++;
                }
                Assert.IsTrue(search.State == SearchState.Failed);
                Assert.IsTrue(search.PathCells == null);
                testCount++;
            }
            if (testCount < 2)
                // Needs two loops to properly validate re-use.
                Assert.Fail("Mesh doesn't support test.");
        }    
        
        [TestMethod]
        public void TestEvaluate() 
        {
            AStarSearch search = new AStarSearch(DistanceHeuristicType.LongestAxis);
            int testCount = 0;
            if (mMesh.GetPathCount < 2)
                Assert.Fail("Mesh doesn't support test.");
            int[] otherPathCells = mMesh.GetPathPolys(0);
            TriCell[] opc = new TriCell[otherPathCells.Length];
            for (int i = 0; i < otherPathCells.Length; i++)
                opc[i] = cells[otherPathCells[i]];
            MasterPath omp = new MasterPath(2, opc, mMesh.PlaneTolerance, 0.1f);
            for (int iPath = 1; iPath < mMesh.GetPathCount; iPath++)
            {
                float[] pathPoints = mMesh.GetPathPoints(iPath);
                int[] pathCells = mMesh.GetPathPolys(iPath);
                TriCell[] pc = new TriCell[pathCells.Length];
                for (int i = 0; i < pathCells.Length; i++)
                    pc[i] = cells[pathCells[i]];
                MasterPath mp = new MasterPath(3, pc, mMesh.PlaneTolerance, 0.1f);
               List<MasterPath> mpl = new List<MasterPath>();
                mpl.Add(omp);
                mpl.Add(mp);
                Assert.IsTrue(search.Evaluate(mpl) == null);
                search.Initialize(pathPoints[0]
                                         , pathPoints[1]
                                         , pathPoints[2]
                                         , pathPoints[3]
                                         , pathPoints[4]
                                         , pathPoints[5]
                                         , cells[pathCells[0]]
                                         , cells[pathCells[pathCells.Length-1]]);
                Assert.IsTrue(search.Evaluate(mpl) == mp);
                mpl.Remove(mp);
                Assert.IsTrue(search.Evaluate(mpl) == null);
                Assert.IsTrue(search.Process() == SearchState.Processing);
                mpl.Add(mp);
                Assert.IsTrue(search.Evaluate(mpl) == mp);
                mpl.Remove(mp);
                Assert.IsTrue(search.Evaluate(mpl) == null);
                testCount++;
            }
            if (testCount < 2)
                // Needs two loops to properly validate re-use.
                Assert.Fail("Mesh doesn't support test.");
        }
    }
}
