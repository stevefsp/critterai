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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Summary description for DijkstraSearchTest
    /// </summary>
    [TestClass]
    public class DijkstraSearchTest
    {
        /*
         * Design notes:
         * 
         * This class is opaque.  Direct testing
         * of loaded values is limited.  So a good amount of testing
         * is mixed together.
         */
        
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
        public void TestConstructionDefaults() 
        {
            DijkstraSearch search = new DijkstraSearch();
            Assert.IsTrue(search.Goals == null);
            Assert.IsTrue(search.IsActive == false);
            Assert.IsTrue(search.PathCount == 0);
            Assert.IsTrue(search.State == SearchState.Uninitialized);
        }

        [TestMethod]
        public void TestGoals() 
        {
            DijkstraSearch search = new DijkstraSearch();
            float[] startPoint = mMesh.MultiPathStartPoint;
            float[] goalPoints = GetGoalPoints();
            search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , goalPoints
                       , GetStartCell()
                       , GetGoalCells()
                       , 2
                       , false);
            float[] gps = search.Goals;
            Assert.IsTrue(gps.Length == goalPoints.Length);
            for (int i = 0; i < gps.Length; i++)
                Assert.IsTrue(gps[i] == goalPoints[i]);
            search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , null
                       , GetStartCell()
                       , GetGoalCells()
                       , 2
                       , false);
            Assert.IsTrue(search.Goals == null);
        }
        
        [TestMethod]
        public void TestProcessAndResultFindAll() 
        {
            // Expect all paths to be foundStart.
            DijkstraSearch search = new DijkstraSearch();
            float[] startPoint = mMesh.MultiPathStartPoint;
            TriCell[] goalCells = GetGoalCells();
            SearchState state = search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , null
                       , GetStartCell()
                       , goalCells
                       , int.MaxValue
                       , false);
            Assert.IsTrue(state == SearchState.Initialized);
            Assert.IsTrue(search.State == SearchState.Initialized);
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
            Assert.IsTrue(state == SearchState.Complete);
            Assert.IsTrue(search.State == SearchState.Complete);
            Assert.IsTrue(search.PathCount == mMesh.MultiPathCount);
            for (int iExpected = 0; iExpected < mMesh.MultiPathCount; iExpected++)
            {
                TriCell[] expectedPath = GetPathCells(iExpected);
                Boolean foundMatch = false;
                for (int iResult = 0; iResult < search.PathCount; iResult++)
                {
                    TriCell[] resultPath = search.GetPathCells(iResult);
                    if (resultPath.Length != expectedPath.Length)
                        continue;
                    Boolean matched = true;
                    for (int iCell = 0; iCell < resultPath.Length; iCell++)
                    {
                        if (resultPath[iCell] != expectedPath[iCell])
                        {
                            matched = false;
                            break;
                        }
                    }
                    if (matched)
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                    Assert.Fail("No search paths match expected path " + iExpected + ".");
            }
        }
        
        [TestMethod]
        public void TestProcessAndResultFindFirst() 
        {
            // Expect only shortest path to be foundStart.
            DijkstraSearch search = new DijkstraSearch();
            float[] startPoint = mMesh.MultiPathStartPoint;
            TriCell[] goalCells = GetGoalCells();
            SearchState state = search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , null
                       , GetStartCell()
                       , goalCells
                       , int.MaxValue
                       , true);
            int maxProcessCalls = mMesh.PolyCount;
            int processCalls = 0;
            while (search.IsActive && processCalls <= maxProcessCalls)
            {
                state = search.Process();
                if (search.IsActive)
                    Assert.IsTrue(state == SearchState.Processing);
                processCalls++;
            }
            Assert.IsTrue(search.PathCount == 1);
            TriCell[] resultPath = search.GetPathCells(0);
            TriCell[] expectedPath = GetPathCells(mMesh.GetShortestMultiPath());
            Assert.IsTrue(resultPath.Length == expectedPath.Length);
            for (int iCell = 0; iCell < resultPath.Length; iCell++)
            {
                if (resultPath[iCell] != expectedPath[iCell])
                    Assert.Fail("Search path does not match expected path.");
            }
        }
        
        [TestMethod]
        public void TestProcessAndResultLimitDepthSuccess() 
        {
            // Shorten the mDepth of the search so that
            // all paths except longest path(s) are foundStart.
            DijkstraSearch search = new DijkstraSearch();
            float[] startPoint = mMesh.MultiPathStartPoint;
            TriCell[] goalCells = GetGoalCells();
            int depthLimit = GetMaxPathLength() - 2;
            SearchState state = search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , null
                       , GetStartCell()
                       , goalCells
                       , depthLimit
                       , false);
            int maxProcessCalls = mMesh.PolyCount;
            int processCalls = 0;
            while (search.IsActive && processCalls <= maxProcessCalls)
            {
                state = search.Process();
                if (search.IsActive)
                    Assert.IsTrue(state == SearchState.Processing);
                processCalls++;
            }
            Assert.IsTrue(search.PathCount > 0);
            int expectedPathCount = 0;
            for (int iExpected = 0; iExpected < mMesh.MultiPathCount; iExpected++)
            {
                TriCell[] expectedPath = GetPathCells(iExpected);
                if (expectedPath.Length > depthLimit + 1)
                    // Don't expect to see this path.
                    continue;
                expectedPathCount++;
                Boolean foundMatch = false;
                for (int iResult = 0; iResult < search.PathCount; iResult++)
                {
                    TriCell[] resultPath = search.GetPathCells(iResult);
                    if (resultPath.Length != expectedPath.Length)
                        continue;
                    Boolean matched = true;
                    for (int iCell = 0; iCell < resultPath.Length; iCell++)
                    {
                        if (resultPath[iCell] != expectedPath[iCell])
                        {
                            matched = false;
                            break;
                        }
                    }
                    if (matched)
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                    Assert.Fail("No search paths match expected path " + iExpected + ".");
            }
            Assert.IsTrue(search.PathCount == expectedPathCount);
        }

        [TestMethod]
        public void TestProcessAndResultLimitDepthFail() 
        {
            // Shorten search mDepth so that no paths are foundStart.
            DijkstraSearch search = new DijkstraSearch();
            float[] startPoint = mMesh.MultiPathStartPoint;
            TriCell[] goalCells = GetGoalCells();
            int depthLimit = GetMinPathLength() - 2;
            SearchState state = search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , null
                       , GetStartCell()
                       , goalCells
                       , depthLimit
                       , false);
            int maxProcessCalls = mMesh.PolyCount;
            int processCalls = 0;
            while (search.IsActive && processCalls <= maxProcessCalls)
            {
                state = search.Process();
                if (search.IsActive)
                    Assert.IsTrue(state == SearchState.Processing);
                processCalls++;
            }
            Assert.IsTrue(search.State == SearchState.Failed);
            Assert.IsTrue(search.PathCount == 0);
        }
        
        [TestMethod]
        public void TestProcessAndResultFail()
        {
            // Give a goal that is not part of the mesh.
            DijkstraSearch search = new DijkstraSearch();
            float[] startPoint = mMesh.MultiPathStartPoint;
            TriCell[] goalCells = new TriCell[2];
            float[] v = { 
                    0, 0, 0 
                    , 1, 0, 1
                    , 1, 0, 0
                    , 0, 0, 1
                };
            goalCells[0] = new TriCell(v, 0, 1, 2);
            goalCells[1] = new TriCell(v, 0, 3, 1);
            goalCells[0].Link(goalCells[1], true);
            SearchState state = search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , null
                       , GetStartCell()
                       , goalCells
                       , int.MaxValue
                       , false);
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
            Assert.IsTrue(search.PathCount == 0);
        }
        
        [TestMethod]
        public void TestResetAndReuse() 
        {
            // - Shortening the mDepth of the search so that
            // all paths except longest path(s) are foundStart.
            // - Reset
            // - Do a shortest path search.
            DijkstraSearch search = new DijkstraSearch();
            float[] startPoint = mMesh.MultiPathStartPoint;
            TriCell[] goalCells = GetGoalCells();
            int depthLimit = GetMaxPathLength() - 2;
            SearchState state = search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , GetGoalPoints()
                       , GetStartCell()
                       , goalCells
                       , depthLimit
                       , false);
            int maxProcessCalls = mMesh.PolyCount;
            int processCalls = 0;
            while (search.IsActive && processCalls <= maxProcessCalls)
            {
                state = search.Process();
                if (search.IsActive)
                    Assert.IsTrue(state == SearchState.Processing);
                processCalls++;
            }
            Assert.IsTrue(search.PathCount > 0);
            int expectedPathCount = 0;
            for (int iExpected = 0; iExpected < mMesh.MultiPathCount; iExpected++)
            {
                TriCell[] expectedPath = GetPathCells(iExpected);
                if (expectedPath.Length > depthLimit + 1)
                    // Don't expect to see this path.
                    continue;
                expectedPathCount++;
                Boolean foundMatch = false;
                for (int iResult = 0; iResult < search.PathCount; iResult++)
                {
                    TriCell[] resultPath = search.GetPathCells(iResult);
                    if (resultPath.Length != expectedPath.Length)
                        continue;
                    Boolean matched = true;
                    for (int iCell = 0; iCell < resultPath.Length; iCell++)
                    {
                        if (resultPath[iCell] != expectedPath[iCell])
                        {
                            matched = false;
                            break;
                        }
                    }
                    if (matched)
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                    Assert.Fail("No search paths match expected path " + iExpected + ".");
            }
            Assert.IsTrue(search.PathCount == expectedPathCount);
            // Reset
            search.Reset();
            Assert.IsTrue(search.PathCount == 0);
            Assert.IsTrue(search.Goals == null);
            // Search for shortest path.
            // Expect only shortest path to be foundStart.
            state = search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , null
                       , GetStartCell()
                       , goalCells
                       , int.MaxValue
                       , true);
            processCalls = 0;
            while (search.IsActive && processCalls <= maxProcessCalls)
            {
                state = search.Process();
                if (search.IsActive)
                    Assert.IsTrue(state == SearchState.Processing);
                processCalls++;
            }
            Assert.IsTrue(search.PathCount == 1);
            TriCell[] resultPathx = search.GetPathCells(0);
            TriCell[] expectedPathx = GetPathCells(mMesh.GetShortestMultiPath());
            Assert.IsTrue(resultPathx.Length == expectedPathx.Length);
            for (int iCell = 0; iCell < resultPathx.Length; iCell++)
            {
                if (resultPathx[iCell] != expectedPathx[iCell])
                    Assert.Fail("Search path does not match expected path.");
            }
        }
        
        [TestMethod]
        public void TestReuse() 
        {
            // - Shortening the mDepth of the search so that
            // all paths except longest path(s) are foundStart.
            // - Do a shortest path search.
            DijkstraSearch search = new DijkstraSearch();
            float[] startPoint = mMesh.MultiPathStartPoint;
            TriCell[] goalCells = GetGoalCells();
            int depthLimit = GetMaxPathLength() - 2;
            SearchState state = search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , GetGoalPoints()
                       , GetStartCell()
                       , goalCells
                       , depthLimit
                       , false);
            int maxProcessCalls = mMesh.PolyCount;
            int processCalls = 0;
            while (search.IsActive && processCalls <= maxProcessCalls)
            {
                state = search.Process();
                if (search.IsActive)
                    Assert.IsTrue(state == SearchState.Processing);
                processCalls++;
            }
            Assert.IsTrue(search.PathCount > 0);
            int expectedPathCount = 0;
            for (int iExpected = 0; iExpected < mMesh.MultiPathCount; iExpected++)
            {
                TriCell[] expectedPath = GetPathCells(iExpected);
                if (expectedPath.Length > depthLimit + 1)
                    // Don't expect to see this path.
                    continue;
                expectedPathCount++;
                Boolean foundMatch = false;
                for (int iResult = 0; iResult < search.PathCount; iResult++)
                {
                    TriCell[] resultPath = search.GetPathCells(iResult);
                    if (resultPath.Length != expectedPath.Length)
                        continue;
                    Boolean matched = true;
                    for (int iCell = 0; iCell < resultPath.Length; iCell++)
                    {
                        if (resultPath[iCell] != expectedPath[iCell])
                        {
                            matched = false;
                            break;
                        }
                    }
                    if (matched)
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                    Assert.Fail("No search paths match expected path " + iExpected + ".");
            }
            Assert.IsTrue(search.PathCount == expectedPathCount);
            // Search for shortest path.
            // Expect only shortest path to be foundStart.
            state = search.Initialize(startPoint[0], startPoint[1], startPoint[2]
                       , null
                       , GetStartCell()
                       , goalCells
                       , int.MaxValue
                       , true);
            Assert.IsTrue(search.Goals == null);
            processCalls = 0;
            while (search.IsActive && processCalls <= maxProcessCalls)
            {
                state = search.Process();
                if (search.IsActive)
                    Assert.IsTrue(state == SearchState.Processing);
                processCalls++;
            }
            Assert.IsTrue(search.PathCount == 1);
            TriCell[] resultPathx = search.GetPathCells(0);
            TriCell[] expectedPathx = GetPathCells(mMesh.GetShortestMultiPath());
            Assert.IsTrue(resultPathx.Length == expectedPathx.Length);
            for (int iCell = 0; iCell < resultPathx.Length; iCell++)
            {
                if (resultPathx[iCell] != expectedPathx[iCell])
                    Assert.Fail("Search path does not match expected path.");
            }
        }
        
        private TriCell[] GetGoalCells()
        {
            TriCell[] result = new TriCell[mMesh.MultiPathCount];
            for (int i = 0; i < mMesh.MultiPathCount; i++)
            {
                int[] pathIndices = mMesh.GetMultiPathPolys(i);
                result[i] = cells[pathIndices[pathIndices.Length-1]];
            }
            return result;
        }

        private TriCell[] GetPathCells(int index)
        {
            int[] pathIndices = mMesh.GetMultiPathPolys(index);
            TriCell[] result = new TriCell[pathIndices.Length];
            for (int i = 0; i < pathIndices.Length; i++)
            {
                result[i] = cells[pathIndices[i]];
            }
            return result;
        }
        
        private float[] GetGoalPoints()
        {
            float[] result = new float[mMesh.MultiPathCount*3];
            for (int i = 0; i < mMesh.MultiPathCount; i++)
            {
                float[] point = mMesh.GetMultiPathGoalPoint(i);
                result[i*3+0] = point[0];
                result[i*3+1] = point[1];
                result[i*3+2] = point[2];
            }
            return result;
        }

        private TriCell GetStartCell()
        {
            return cells[mMesh.GetMultiPathPolys(0)[0]];
        }
        
        private int GetMinPathLength()
        {
            int result = mMesh.GetMultiPathPolys(0).Length;
            for (int i = 1; i < mMesh.MultiPathCount; i++)
            {
                result = Math.Min(result, mMesh.GetMultiPathPolys(i).Length);
            }
            return result;
        }
        
        private int GetMaxPathLength()
        {
            int result = mMesh.GetMultiPathPolys(0).Length;
            for (int i = 1; i < mMesh.MultiPathCount; i++)
            {
                result = Math.Max(result, mMesh.GetMultiPathPolys(i).Length);
            }
            return result;
        }
    }
}
