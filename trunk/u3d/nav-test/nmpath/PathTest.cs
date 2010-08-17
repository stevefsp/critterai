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
using org.critterai.math;
using UnityEngine;
using Path = org.critterai.nav.nmpath.MasterPath.Path;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Summary description for PathTest
    /// </summary>
    [TestClass]
    public class PathTest
    {

        private const float TOLERANCE_STD = MathUtil.TOLERANCE_STD;
        private const float OFFSET_SCALE = 0.1f;
        private const int PATH_ID = 5;
        
        private ITestMesh mMesh;
        private TriCell[] mCells;

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
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            mCells = TestUtil.GetAllCells(verts, indices);
            TestUtil.LinkAllCells(mCells);
        }

        [TestMethod]
        public void TestGetGoal() 
        {
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                TriCell[] pathCells = GetPathCells(iPath);
                MasterPath mp = new MasterPath(PATH_ID
                        , pathCells
                        , mMesh.PlaneTolerance
                        , OFFSET_SCALE);
                float[] points = mMesh.GetPathPoints(iPath);
                Path p = mp.GetPath(points[3], points[4], points[5]);
                Assert.IsTrue(p.GoalX == points[3]);
                Assert.IsTrue(p.GoalY == points[4]);
                Assert.IsTrue(p.GoalZ == points[5]);
                Vector3 goal = p.Goal;
                Assert.IsTrue(goal.x == points[3] && goal.y == points[4] && goal.z == points[5]);
                testCount++;
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }

        [TestMethod]
        public void TestId() 
        {
            MasterPath mp = new MasterPath(PATH_ID
                    , mCells[0]
                    , mMesh.PlaneTolerance
                    , OFFSET_SCALE);
            Assert.IsTrue(mp.GetPath(mCells[0].CentroidX
                    , mCells[0].CentroidY
                    , mCells[0].CentroidZ).Id == mp.Id);
        }

        [TestMethod]
        public void TestIsDisposed() 
        {
            MasterPath mp = new MasterPath(PATH_ID
                    , mCells[0]
                    , mMesh.PlaneTolerance
                    , OFFSET_SCALE);
            Path p = mp.GetPath(mCells[0].CentroidX
                    , mCells[0].CentroidY
                    , mCells[0].CentroidZ);
            Assert.IsTrue(p.IsDisposed == mp.IsDisposed);
            mp.Dispose();
            Assert.IsTrue(p.IsDisposed == mp.IsDisposed);
        }
        
        [TestMethod]
        public void TestIsInPathColumn() 
        {
            /*
             * Assuming that Cell class is functioning properly.
             * So not testing edge cases.
             * Only testing to make sure that the entire
             * column is being searched.
             */
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                TriCell[] pathCells = GetPathCells(iPath);
                MasterPath mp = new MasterPath(PATH_ID
                        , pathCells
                        , mMesh.PlaneTolerance
                        , OFFSET_SCALE);
                float[] points = mMesh.GetPathPoints(iPath);
                Path p = mp.GetPath(points[3], points[4], points[5]);
                float[] min = mMesh.MinVertex;
                Assert.IsFalse(p.IsInPathColumn(min[0] - 1, min[2] - 1));
                for (int iCell = 0; iCell < pathCells.Length; iCell++)
                {
                    Assert.IsTrue(p.IsInPathColumn(pathCells[iCell].CentroidX, pathCells[iCell].CentroidZ));
                    testCount++;    
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }    
        
        [TestMethod]
        public void TestForceYToPath() 
        {
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                TriCell[] pathCells = GetPathCells(iPath);
                MasterPath mp = new MasterPath(PATH_ID
                        , pathCells
                        , mMesh.PlaneTolerance
                        , OFFSET_SCALE);
                float[] points = mMesh.GetPathPoints(iPath);
                Path p = mp.GetPath(points[3], points[4], points[5]);
                float[] min = mMesh.MinVertex;
                Vector3 v;
                Assert.IsFalse(p.ForceYToPath(min[0] - 1, min[1] - 1, min[2] - 1, out v));
                for (int iCell = 0; iCell < pathCells.Length; iCell++)
                {
                    float centX = pathCells[iCell].CentroidX;
                    float centY = pathCells[iCell].CentroidY;
                    float centZ = pathCells[iCell].CentroidZ;
                    Assert.IsTrue(p.ForceYToPath(centX
                            , centY + mMesh.PlaneTolerance
                            , centZ
                            , out v));
                    Assert.IsTrue(Vector3Util.SloppyEquals(v, centX, centY, centZ, TOLERANCE_STD));
                    testCount++;
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }    
        
    //    [TestMethod]
    //    public void TestIsOnPath() 
    //    {
    //        int testCount = 0;
    //        for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
    //        {
    //            TriCell[] PathCells = GetPathCells(iPath);
    //            MasterPath mp = new MasterPath(PATH_ID
    //                    , PathCells
    //                    , mMesh.PlaneTolerance
    //                    , OFFSET_SCALE);
    //            float[] points = mMesh.GetPathPoints(iPath);
    //            Path p = mp.GetPath(points[3], points[4], points[5]);
    //            float[] Min = mMesh.MinVertex;
    //            Assert.IsFalse(p.isOnPath(Min[0] - 1, Min[1] - 1, Min[2] - 1, mMesh.PlaneTolerance));
    //            for (int iCell = 0; iCell < PathCells.Length; iCell++)
    //            {
    //                float x = PathCells[iCell].mCentroidX;
    //                float y = PathCells[iCell].mCentroidY;
    //                float z = PathCells[iCell].mCentroidZ;
    //                /*
    //                 * Algorithm is susceptible to floating point
    //                 * errors.  So can only test for a fuzzy edge.
    //                 */
    //                // Above
    //                Assert.IsTrue(p.isOnPath(x
    //                        , y + mMesh.PlaneTolerance - 4*TOLERANCE_STD
    //                        , z
    //                        , mMesh.PlaneTolerance));
    //                Assert.IsFalse(p.isOnPath(x
    //                        , y + mMesh.PlaneTolerance + 4*TOLERANCE_STD
    //                        , z
    //                        , mMesh.PlaneTolerance));
    //                // Below
    //                Assert.IsTrue(p.isOnPath(x
    //                        , y - mMesh.PlaneTolerance + 4*TOLERANCE_STD
    //                        , z
    //                        , mMesh.PlaneTolerance));
    //                Assert.IsFalse(p.isOnPath(x
    //                        , y - mMesh.PlaneTolerance - 4*TOLERANCE_STD
    //                        , z
    //                        , mMesh.PlaneTolerance));
    //                testCount++;
    //            }
    //        }
    //        if (testCount == 0)
    //            Assert.Fail("Mesh doesn't support test.");
    //    }
        
        [TestMethod]
        public void TestForceToPath() 
        {
            /* 
             * This is just a wrapper operation for a TriCell operation.
             * So not testing edge cases.
             * Basically, we are locating a wall on the each Cell that is not
             * part of the path, then creating a test point just outside that wall.
             */
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                TriCell[] pathCells = GetPathCells(iPath);
                MasterPath mp = new MasterPath(PATH_ID
                        , pathCells
                        , mMesh.PlaneTolerance
                        , OFFSET_SCALE);
                float[] points = mMesh.GetPathPoints(iPath);
                Path p = mp.GetPath(points[3], points[4], points[5]);
                for (int iCell = 0; iCell < pathCells.Length; iCell++)
                {
                    TriCell pathCell = pathCells[iCell];
                    int iLinkedWallPre = -1;
                    int iLinkedWallNext = -1;
                    if (iCell+1 < pathCells.Length)
                        iLinkedWallNext = pathCells[iCell].GetLinkIndex(pathCells[iCell+1]);
                    if (iCell-1 >= 0)
                        iLinkedWallPre = pathCells[iCell].GetLinkIndex(pathCells[iCell-1]);
                    for (int iWall = 0; iWall < pathCell.MaxLinks; iWall++)
                    {
                        if (iWall == iLinkedWallPre || iWall == iLinkedWallNext)
                            continue;
                        // Get a point that is slightly outside the wall.
                        Vector3 u = pathCell.GetWallLeftVertex(iWall);
                        Vector3 v = pathCell.GetWallRightVertex(iWall);
                        u = (u + v) / 2;
                        v = new Vector3((u.x - pathCell.CentroidX) * 0.1f
                            , (u.y - pathCell.CentroidY) * 0.1f
                            , (u.z - pathCell.CentroidZ) * 0.1f);
                        u += v;
                        Assert.IsFalse(pathCell.IsInColumn(u.x, u.z));  // Only a logic error check.
                        u = p.ForceToPath(u.x, u.y, u.z);
                        Assert.IsTrue(pathCell.IsInColumn(u.x, u.z));
                    }
                    testCount++;    
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }
        
        [TestMethod]
        public void TestGetTargetNoOffset() 
        {
            float[] pverts = {
                    -1, -5, -2
                    , 0, -5.5f, -1
                    , 0, -5, -2
                    , -1, -5.5f, -1
                    , -0.8f, -6, 0
                    , -2, -6, 0
                    , -1.2f, -5.5f, 1
                    , -2, -5.5f, 2
                    , -1, -5.5f, 2
                    , 0, -5, 2
                    , 0, -5, 1
                    , 1, -4.5f, 2
            };
            
            TriCell[] pcells = new TriCell[10];
            pcells[0] = new TriCell(pverts, 0, 1, 2);
            pcells[1] = new TriCell(pverts, 0, 3, 1);
            pcells[2] = new TriCell(pverts, 3, 4, 1);
            pcells[3] = new TriCell(pverts, 3, 5, 4);
            pcells[4] = new TriCell(pverts, 4, 5, 6);
            pcells[5] = new TriCell(pverts, 6, 5, 7);
            pcells[6] = new TriCell(pverts, 7, 8, 6);
            pcells[7] = new TriCell(pverts, 6, 8, 9);
            pcells[8] = new TriCell(pverts, 10, 6, 9);
            pcells[9] = new TriCell(pverts, 9, 11, 10);
            pcells[0].Link(pcells[1], true);
            pcells[1].Link(pcells[2], true);
            pcells[2].Link(pcells[3], true);
            pcells[3].Link(pcells[4], true);
            pcells[4].Link(pcells[5], true);
            pcells[5].Link(pcells[6], true);
            pcells[6].Link(pcells[7], true);
            pcells[7].Link(pcells[8], true);
            pcells[8].Link(pcells[9], true);
            
            Vector3 start = new Vector3(-1, -5, -2);
            Vector3 goal = new Vector3(0.2f, -5, 1.8f);
            
            MasterPath mp = new MasterPath(PATH_ID, pcells, 0.5f, 0);
            Path p = mp.GetPath(goal.x, goal.y, goal.z);
            
            Vector3[] targets = new Vector3[3];
            targets[0] = new Vector3(pverts[3*3+0], pverts[3*3+1], pverts[3*3+2]);
            targets[1] = new Vector3(pverts[6*3+0], pverts[6*3+1], pverts[6*3+2]);
            targets[2] = goal;
            
            Vector3 currTarget = new Vector3(start.x, start.y, start.z);
            
            for (int i = 0; i < targets.Length; i++)
            {
                Assert.IsTrue(p.GetTarget(currTarget.x, currTarget.y, currTarget.z, out currTarget));
                Assert.IsTrue(Vector3Util.SloppyEquals(currTarget, targets[i], TOLERANCE_STD));
            }
        }
        
        [TestMethod]
        public void TestGetTargetWithOffset() 
        {
            float[] pverts = {
                    -1, -5, -2
                    , 0, -5.5f, -1
                    , 0, -5, -2
                    , -1, -5.5f, -1
                    , -0.8f, -6, 0
                    , -2, -6, 0
                    , -1.2f, -5.5f, 1
                    , -2, -5.5f, 2
                    , -1, -5.5f, 2
                    , 0, -5, 2
                    , 0, -5, 1
                    , 1, -4.5f, 2
            };
            
            TriCell[] pcells = new TriCell[10];
            pcells[0] = new TriCell(pverts, 0, 1, 2);
            pcells[1] = new TriCell(pverts, 0, 3, 1);
            pcells[2] = new TriCell(pverts, 3, 4, 1);
            pcells[3] = new TriCell(pverts, 3, 5, 4);
            pcells[4] = new TriCell(pverts, 4, 5, 6);
            pcells[5] = new TriCell(pverts, 6, 5, 7);
            pcells[6] = new TriCell(pverts, 7, 8, 6);
            pcells[7] = new TriCell(pverts, 6, 8, 9);
            pcells[8] = new TriCell(pverts, 10, 6, 9);
            pcells[9] = new TriCell(pverts, 9, 11, 10);
            pcells[0].Link(pcells[1], true);
            pcells[1].Link(pcells[2], true);
            pcells[2].Link(pcells[3], true);
            pcells[3].Link(pcells[4], true);
            pcells[4].Link(pcells[5], true);
            pcells[5].Link(pcells[6], true);
            pcells[6].Link(pcells[7], true);
            pcells[7].Link(pcells[8], true);
            pcells[8].Link(pcells[9], true);
            
            Vector3 start = new Vector3(-1, -5, -2);
            Vector3 goal = new Vector3(0.2f, -5, 1.8f);
            
            MasterPath mp = new MasterPath(PATH_ID, pcells, 0.5f, OFFSET_SCALE);
            Path p = mp.GetPath(goal.x, goal.y, goal.z);
            
            Vector3[] targets = new Vector3[3];
            targets[0] = Vector3Util.TranslateToward(pverts[3*3+0], pverts[3*3+1], pverts[3*3+2]
                                                       , pverts[4*3+0], pverts[4*3+1], pverts[4*3+2]                                    
                                                       , OFFSET_SCALE);
            targets[1] = Vector3Util.TranslateToward(pverts[6*3+0], pverts[6*3+1], pverts[6*3+2]
                                                       , pverts[7*3+0], pverts[7*3+1], pverts[7*3+2]                                    
                                                       , OFFSET_SCALE);
            targets[2] = goal;
            
            Vector3 currTarget = new Vector3(start.x, start.y, start.z);
            
            for (int i = 0; i < targets.Length; i++)
            {
                Assert.IsTrue(p.GetTarget(currTarget.x, currTarget.y, currTarget.z, out currTarget));
                Assert.IsTrue(Vector3Util.SloppyEquals(currTarget, targets[i], TOLERANCE_STD));
            }
        }
        
        [TestMethod]
        public void TestGetPathPolys()
        {
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                TriCell[] pathCells = GetPathCells(iPath);
                MasterPath mp = new MasterPath(PATH_ID
                        , pathCells
                        , mMesh.PlaneTolerance
                        , OFFSET_SCALE);
                float[] points = mMesh.GetPathPoints(iPath);
                Path p = mp.GetPath(points[3], points[4], points[5]);
                Assert.IsTrue(p.PathPolyCount == pathCells.Length);
                Assert.IsTrue(p.PathVertCount == pathCells.Length * 3);
                float[] verts = new float[p.PathVertCount*3];
                int[] indices = new int[p.PathPolyCount*3];
                p.GetPathPolys(verts, indices);
                for (int iCell = 0; iCell < pathCells.Length; iCell++)
                {
                    int pPoly = iCell*3;
                    for (int iPolyVert = 0; iPolyVert < 3; iPolyVert++)
                    {
                        int pVert = indices[pPoly+iPolyVert]*3;
                        Assert.IsTrue(pathCells[iCell].GetVertexValue(iPolyVert, 0) == verts[pVert+0]);
                        Assert.IsTrue(pathCells[iCell].GetVertexValue(iPolyVert, 1) == verts[pVert+1]);
                        Assert.IsTrue(pathCells[iCell].GetVertexValue(iPolyVert, 2) == verts[pVert+2]);
                    }
                }
            }
        }
        
        private TriCell[] GetPathCells(int pathIndex)
        {
            int[] pathPolys = mMesh.GetPathPolys(pathIndex);
            TriCell[] result = new TriCell[pathPolys.Length];
            for (int i = 0; i < pathPolys.Length; i++)
            {
                result[i] = mCells[pathPolys[i]];
            }
            return result;
        }
    }
}
