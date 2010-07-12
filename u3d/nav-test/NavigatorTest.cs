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
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.critterai.math;
using org.critterai.math.geom;
using UnityEngine;
using Navigator = org.critterai.nav.MasterNavigator.Navigator;
using Path = org.critterai.nav.MasterPath.Path;

namespace org.critterai.nav
{
    /// <summary>
    /// Summary description for NavigatorTest
    /// </summary>
    [TestClass]
    public sealed class NavigatorTest
    {
        /*
         * Design notes;
         * 
         * Limited to single threaded behavior.
         * Does not validate accuracy of throttling.
         * 
         * Internal behavior is being validated in this
         * suite since it is important to detect 
         * potential memory leak issues.
         * 
         * Disposal is tested during the internal tests.
         * 
         * Some tests Build on earlier tests.  So fix
         * failures in earlier tests first.
         */

        private const int MAX_PATH_AGE = 60000;
        private const int SEARCH_DEPTH = 2;
        private const float TOLERANCE_STD = MathUtil.TOLERANCE_STD;

        private ITestMesh mMesh;
        private TriCell[] mCells;
        private TriNavMesh mNavMesh;
        private Vector3[] mCents;

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
            // Dev Note: Keep the offset scale to zero.  Some tests depend on that.
            mNavMesh = TriNavMesh.Build(verts, indices, 5, mMesh.PlaneTolerance, 0);
            mCents = new Vector3[mCells.Length];
            for (int iCell = 0; iCell < mCells.Length; iCell++)
            {
                int pCell = iCell*3;
                Vector3 cent = Polygon3.GetCentroid(
                                                verts[indices[pCell]*3]
                                                , verts[indices[pCell]*3+1]
                                                , verts[indices[pCell]*3+2]
                                                , verts[indices[pCell+1]*3]
                                                , verts[indices[pCell+1]*3+1]
                                                , verts[indices[pCell+1]*3+2]
                                                , verts[indices[pCell+2]*3]
                                                , verts[indices[pCell+2]*3+1]
                                                , verts[indices[pCell+2]*3+2]);
                mCents[iCell] = cent;
            }
        }

        [TestMethod]
        public void TestIsValidLocation() 
        {
            // Tests locations on the mesh surface or completely outside.
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            MasterNavRequest<Boolean>.NavRequest req;
            MasterNavRequest<Boolean>.NavRequest req2;
            for (int iCell = 1; iCell < mCells.Length; iCell++)
            {
                req = nav.IsValidLocation(mCents[iCell].x
                                                , mCents[iCell].y
                                                , mCents[iCell].z
                                                , mMesh.PlaneTolerance);
                req2 = nav.IsValidLocation(mCents[iCell-1].x
                        , mCents[iCell-1].y
                        , mCents[iCell-1].z
                        , mMesh.PlaneTolerance);
                Assert.IsTrue(req != null);
                Assert.IsTrue(req.IsFinished == false);
                mnav.ProcessAll(false);
                Assert.IsTrue(req.IsFinished == true);
                Assert.IsTrue(req.State == NavRequestState.Complete);
                Assert.IsTrue(req.Data == true);
                Assert.IsTrue(req2.State == NavRequestState.Complete);
                Assert.IsTrue(req2.Data == true);
            }
            // Test standard failure.
            float[] minPoint = mMesh.MinVertex;
            req = nav.IsValidLocation(minPoint[0] - mMesh.Offset
                     , minPoint[1] - mMesh.Offset
                     , minPoint[2] - mMesh.Offset
                     , mMesh.PlaneTolerance);
            mnav.ProcessAll(false);
            Assert.IsTrue(req.State == NavRequestState.Complete);
            Assert.IsTrue(req.Data == false);
            // Test auto-failure after Nav is disposed.
            mnav.Dispose();
            Assert.IsTrue(nav.IsValidLocation(mCents[0].x, mCents[0].y, mCents[0].z
                    , mMesh.PlaneTolerance).State == NavRequestState.Failed);
        }
        
        [TestMethod]
        public void TestIsValidLocationTolerence() 
        {
            // Tests the tolerance functionality.
            // Note:  Can't test the exact edge due to floating point
            // errors.  Have to test just above and below the edge.
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            MasterNavRequest<Boolean>.NavRequest req;
            for (int iCell = 1; iCell < mCells.Length; iCell++)
            {
                float ytol = mMesh.PlaneTolerance*0.5f;
                req = nav.IsValidLocation(mCents[iCell].x
                                                , mCents[iCell].y + ytol - TOLERANCE_STD
                                                , mCents[iCell].z
                                                , ytol);
                mnav.ProcessAll(false);
                Assert.IsTrue(req.State == NavRequestState.Complete);
                Assert.IsTrue(req.Data == true);
                req = nav.IsValidLocation(mCents[iCell].x
                                                , mCents[iCell].y + ytol + TOLERANCE_STD
                                                , mCents[iCell].z
                                                , ytol);
                mnav.ProcessAll(false);
                Assert.IsTrue(req.State == NavRequestState.Complete);
                Assert.IsTrue(req.Data == false);
                req = nav.IsValidLocation(mCents[iCell].x
                                                , mCents[iCell].y - ytol + TOLERANCE_STD
                                                , mCents[iCell].z
                                                , ytol);
                mnav.ProcessAll(false);
                Assert.IsTrue(req.State == NavRequestState.Complete);
                Assert.IsTrue(req.Data == true);
                req = nav.IsValidLocation(mCents[iCell].x
                                                , mCents[iCell].y - ytol - TOLERANCE_STD
                                                , mCents[iCell].z
                                                , ytol);
                mnav.ProcessAll(false);
                Assert.IsTrue(req.State == NavRequestState.Complete);
                Assert.IsTrue(req.Data == false);
            }
        }
        
        [TestMethod]
        public void TestGetNearestValidLocationInternal() 
        {
            // Points are already internal to the mesh.
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            MasterNavRequest<Vector3>.NavRequest req;
            MasterNavRequest<Vector3>.NavRequest req2;
            for (int iCell = 1; iCell < mCells.Length; iCell++)
            {
                req = nav.GetNearestValidLocation(mCents[iCell].x
                                                , mCents[iCell].y
                                                , mCents[iCell].z);
                req2 = nav.GetNearestValidLocation(mCents[iCell-1].x
                        , mCents[iCell-1].y
                        , mCents[iCell-1].z);
                Assert.IsTrue(req != null);
                Assert.IsTrue(req.IsFinished == false);
                mnav.ProcessAll(false);
                Assert.IsTrue(req.IsFinished == true);
                Assert.IsTrue(req.State == NavRequestState.Complete);
                Assert.IsTrue(Vector3Util.SloppyEquals(req.Data, mCents[iCell].x
                        , mCents[iCell].y
                        , mCents[iCell].z
                        , TOLERANCE_STD));
                Assert.IsTrue(req2.State == NavRequestState.Complete);
                Assert.IsTrue(Vector3Util.SloppyEquals(req2.Data, mCents[iCell-1].x
                        , mCents[iCell-1].y
                        , mCents[iCell-1].z
                        , TOLERANCE_STD));
                Assert.IsTrue(mnav.NearestLocationJobCount == 0);
            }
            // Test auto-failure after Nav is disposed.
            mnav.Dispose();
            Assert.IsTrue(nav.GetNearestValidLocation(mCents[0].x, mCents[0].y, mCents[0].z)
                    .State == NavRequestState.Failed);
        }
        
        [TestMethod]
        public void TestGetNearestValidLocationExternal() 
        {
            // Point is outside the mesh.
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            MasterNavRequest<Vector3>.NavRequest req;
            float[] minPoint = mMesh.MinVertex;
            req = nav.GetNearestValidLocation(minPoint[0] - mMesh.Offset
                     , minPoint[1] - mMesh.Offset
                     , minPoint[2] - mMesh.Offset);
            mnav.ProcessAll(false);
            Assert.IsTrue(req.State == NavRequestState.Complete);
            int[] minCellIndices = mMesh.GetMinVertexPolys();
            Boolean found = false;
            for (int i = 0; i < minCellIndices.Length; i++)
            {
                TriCell cell = mCells[minCellIndices[i]];
                if (cell.IsInColumn(req.Data.x, req.Data.z))
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found);
        }
        
        [TestMethod]
        public void TestDiscardPathRequest() 
        {
            // This test only tests that the functionality works
            // for one version of the Process operation.
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            float[] pathPoints = null;
            List<MasterNavRequest<Path>.NavRequest> reqs 
                = new List<MasterNavRequest<Path>.NavRequest>();
            int testCount = 0;
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                pathPoints = mMesh.GetPathPoints(iPath);
                reqs.Add(nav.GetPath(pathPoints[0]
                                     , pathPoints[1]
                                     , pathPoints[2]
                                     , pathPoints[3]
                                     , pathPoints[4]
                                     , pathPoints[5]));
                testCount++;
            }
            // Remember: Test mesh guarantees that all paths are > 2 in length. 
            mnav.ProcessOnce(false);
            pathPoints = mMesh.GetPathPoints(mMesh.GetPathCount - 1);
            reqs.Add(nav.GetPath(pathPoints[0]
                                 , pathPoints[1]
                                 , pathPoints[2]
                                 , pathPoints[3]
                                 , pathPoints[4]
                                 , pathPoints[5]));
            Assert.IsTrue(mnav.PathRequestCount == 1);
            testCount++;
            if (testCount < 2)
                // Needs two loops to properly validate re-use.
                Assert.Fail("Mesh doesn't support test.");
            // We now have at least one job and one Request.
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
            {
                Assert.IsTrue(req.IsFinished == false);
                nav.DiscardPathRequest(req);
            }
            mnav.ProcessOnce(false);
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
            {
                Assert.IsTrue(req.State == NavRequestState.Failed);
            }
            // Test disposal behavior.
            mnav.Dispose();
            nav.DiscardPathRequest(reqs[0]);
            Assert.IsTrue(mnav.DiscardPathRequestCount == 0);
        }
        
        [TestMethod]
        public void TestGetPathStandardProcessOnce() 
        {
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            List<MasterNavRequest<Path>.NavRequest> reqs 
                = RequestAllPaths(nav);
            int maxProcessCalls = mMesh.PolyCount + 2; // The increment is arbitrary.
            int processCalls = 1;
            List<MasterNavRequest<Path>.NavRequest> creqs 
                = new List<MasterNavRequest<Path>.NavRequest>();
            while (creqs.Count < mMesh.GetPathCount && processCalls <= maxProcessCalls)
            {
                mnav.ProcessOnce(false);
                for (int i = reqs.Count - 1; i > -1; i--)
                {
                    MasterNavRequest<Path>.NavRequest req = reqs[i];
                    if (req.IsFinished)
                    {
                        Assert.IsTrue(req.State == NavRequestState.Complete);
                        Assert.IsTrue(req.Data != null);
                        creqs.Add(req);
                        reqs.RemoveAt(i);
                    }
                }
                processCalls++;
            }
            // Make sure paths are valid by walking them to the end.
            int maxSteps = 100; // This number is arbitrary.
            List<int> seenPaths = new List<int>();
            foreach (MasterNavRequest<Path>.NavRequest req in creqs)
            {
                Path path = req.Data;
                Vector3 currentPoint;
                int iPath = GetPathStartPoint(path, out currentPoint);
                Assert.IsTrue(!seenPaths.Contains(iPath));
                seenPaths.Add(iPath);
                int step = 0;
                while (!(currentPoint.x == path.GoalX && currentPoint.y == path.GoalY && currentPoint.z == path.GoalZ)
                        && step < maxSteps)
                {
                    path.GetTarget(currentPoint.x
                            , currentPoint.y
                            , currentPoint.z
                            , out currentPoint);
                    step++;
                }
                // Make sure we have reached the goal from the expected
                // start point.
                Assert.IsTrue((currentPoint.x == path.GoalX && currentPoint.y == path.GoalY && currentPoint.z == path.GoalZ));
            }
        }
        
        [TestMethod]
        public void TestGetPathStandardProcessAll() 
        {
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            List<MasterNavRequest<Path>.NavRequest> reqs  
                    = RequestAllPaths(nav);
            List<MasterNavRequest<Path>.NavRequest> creqs 
                = new List<MasterNavRequest<Path>.NavRequest>();
            mnav.ProcessAll(false);
            for (int i = reqs.Count - 1; i > -1; i--)
            {
                MasterNavRequest<Path>.NavRequest req = reqs[i];
                Assert.IsTrue(req.IsFinished);
                Assert.IsTrue(req.State == NavRequestState.Complete);
                Assert.IsTrue(req.Data != null);
                creqs.Add(req);
                reqs.RemoveAt(i);
            }
            // Make sure paths are valid by walking them to the end.
            int maxSteps = 100; // This number is arbitrary.
            List<int> seenPaths = new List<int>();
            foreach (MasterNavRequest<Path>.NavRequest req in creqs)
            {
                Path path = req.Data;
                Vector3 currentPoint;
                int iPath = GetPathStartPoint(path, out currentPoint);
                Assert.IsTrue(!seenPaths.Contains(iPath));
                seenPaths.Add(iPath);
                int step = 0;
                while (!(currentPoint.x == path.GoalX && currentPoint.y == path.GoalY && currentPoint.z == path.GoalZ)
                        && step < maxSteps)
                {
                    path.GetTarget(currentPoint.x
                            , currentPoint.y
                            , currentPoint.z
                            , out currentPoint);
                    step++;
                }
                // Make sure we have reached the goal from the expected
                // start point.
                Assert.IsTrue((currentPoint.x == path.GoalX && currentPoint.y == path.GoalY && currentPoint.z == path.GoalZ));
            }
        }
        
        [TestMethod]
        public void TestGetPathStandardProcessThrottleBasic() 
        {
            // Only making sure that the processing can complete, not
            // that the throttling is functioning as expected.
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            List<MasterNavRequest<Path>.NavRequest> reqs 
                = RequestAllPaths(nav);
            List<MasterNavRequest<Path>.NavRequest> creqs 
                = new List<MasterNavRequest<Path>.NavRequest>();
            mnav.Process(false);
            for (int i = reqs.Count - 1; i > -1; i--)
            {
                MasterNavRequest<Path>.NavRequest req = reqs[i];
                Assert.IsTrue(req.IsFinished);
                Assert.IsTrue(req.State == NavRequestState.Complete);
                Assert.IsTrue(req.Data != null);
                creqs.Add(req);
                reqs.RemoveAt(i);
            }
            // Make sure paths are valid by walking them to the end.
            int maxSteps = 100; // This number is arbitrary.
            List<int> seenPaths = new List<int>();
            foreach (MasterNavRequest<Path>.NavRequest req in creqs)
            {
                Path path = req.Data;
                Vector3 currentPoint;
                int iPath = GetPathStartPoint(path, out currentPoint);
                Assert.IsTrue(!seenPaths.Contains(iPath));
                seenPaths.Add(iPath);
                int step = 0;
                while (!(currentPoint.x == path.GoalX && currentPoint.y == path.GoalY && currentPoint.z == path.GoalZ)
                        && step < maxSteps)
                {
                    path.GetTarget(currentPoint.x
                            , currentPoint.y
                            , currentPoint.z
                            , out currentPoint);
                    step++;
                }
                // Make sure we have reached the goal from the expected
                // start point.
                Assert.IsTrue((currentPoint.x == path.GoalX && currentPoint.y == path.GoalY && currentPoint.z == path.GoalZ));
            }
        }
        
        [TestMethod]
        public void TestGetPathStandardThrottling() 
        {
            // Tests whether throttling can complete is forced
            // to single step.
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , 1        // 1 nanosecond. <<<<<<<<<<<<<<<<<
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            List<MasterNavRequest<Path>.NavRequest> reqs 
                = RequestAllPaths(nav);
            int maxProcessCalls = mMesh.PolyCount + 2; // The increment is arbitrary.
            int processCalls = 1;
            List<MasterNavRequest<Path>.NavRequest> creqs 
                = new List<MasterNavRequest<Path>.NavRequest>();
            while (creqs.Count < mMesh.GetPathCount && processCalls <= maxProcessCalls)
            {
                mnav.Process(false);
                for (int i = reqs.Count - 1; i > -1; i--)
                {
                    MasterNavRequest<Path>.NavRequest req = reqs[i];
                    if (req.IsFinished)
                    {
                        Assert.IsTrue(req.State == NavRequestState.Complete);
                        Assert.IsTrue(req.Data != null);
                        creqs.Add(req);
                        reqs.RemoveAt(i);
                    }
                }
                processCalls++;
            }
            // Make sure paths are valid by walking them to the end.
            int maxSteps = 100; // This number is arbitrary.
            List<int> seenPaths = new List<int>();
            foreach (MasterNavRequest<Path>.NavRequest req in creqs)
            {
                Path path = req.Data;
                Vector3 currentPoint;
                int iPath = GetPathStartPoint(path, out currentPoint);
                Assert.IsTrue(!seenPaths.Contains(iPath));
                seenPaths.Add(iPath);
                int step = 0;
                while (!(currentPoint.x == path.GoalX && currentPoint.y == path.GoalY && currentPoint.z == path.GoalZ)
                        && step < maxSteps)
                {
                    path.GetTarget(currentPoint.x
                            , currentPoint.y
                            , currentPoint.z
                            , out currentPoint);
                    step++;
                }
                // Make sure we have reached the goal from the expected
                // start point.
                Assert.IsTrue((currentPoint.x == path.GoalX && currentPoint.y == path.GoalY && currentPoint.z == path.GoalZ));
            }
        }
        
        [TestMethod]
        public void TestGetPathFailure()
        {
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            MasterNavRequest<Path>.NavRequest reqInOut;
            MasterNavRequest<Path>.NavRequest reqOutIn;
            float[] minPoint = mMesh.MinVertex;
            minPoint[0] -= 1;
            minPoint[1] -= 1;
            minPoint[2] -= 1;
            for (int iCell = 1; iCell < mCells.Length; iCell++)
            {
                reqInOut = nav.GetPath(mCents[iCell].x
                                      , mCents[iCell].y
                                      , mCents[iCell].z
                                      , minPoint[0]
                                      , minPoint[1]
                                      , minPoint[2]);
                reqOutIn = nav.GetPath(minPoint[0]
                          , minPoint[1]
                          , minPoint[2]
                          , mCents[iCell].x
                          , mCents[iCell].y
                          , mCents[iCell].z);
                mnav.ProcessOnce(false);
                // Guaranteed to fail after one cycle.
                Assert.IsTrue(reqInOut.State == NavRequestState.Failed);
                Assert.IsTrue(reqOutIn.State == NavRequestState.Failed);
                Assert.IsTrue(reqInOut.Data == null);
                Assert.IsTrue(reqOutIn.Data == null);
            }
        }
        
        [TestMethod]
        public void TestPathReuse()
        {
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            List<MasterNavRequest<Path>.NavRequest> reqs 
                = RequestAllPaths(nav);
            mnav.ProcessAll(false);
            List<MasterNavRequest<Path>.NavRequest> creqs 
                = RequestAllPaths(nav);
            mnav.ProcessAll(false);
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                Assert.IsTrue(reqs[iPath].Data.Id == creqs[iPath].Data.Id);
            }
        }
        
        [TestMethod]
        public void TestInternalPathPool() 
        {
            int poolSize = (int)(mMesh.GetPathCount + 1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            RequestAllPaths(nav);
            mnav.ProcessAll(false);
            Assert.IsTrue(mnav.PathSearchPoolSize == mMesh.GetPathCount);
            RequestAllPaths(nav);
            RequestAllPaths(nav);
            mnav.ProcessAll(false);
            Assert.IsTrue(mnav.PathSearchPoolSize == poolSize);
            mnav.Dispose();
            Assert.IsTrue(mnav.PathSearchPoolSize == 0);
        }    
        
        [TestMethod]
        public void TestInternalRepairPool() 
        {
            int poolSize = (int)(mMesh.GetPathCount + 1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            
            List<MasterNavRequest<Path>.NavRequest> reqs = RequestAllPaths(nav);
            mnav.ProcessAll(false);
            
            // Create repair requests.
            float[] minVert = mMesh.MinVertex;
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                nav.RepairPath(minVert[0]
                        , minVert[1]
                        , minVert[2]
                        , req.Data);
            mnav.ProcessAll(false);
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                nav.RepairPath(minVert[0]
                        , minVert[1]
                        , minVert[2]
                        , req.Data);
            mnav.ProcessAll(false);
        
            Assert.IsTrue(mnav.RepairPoolSize <= poolSize);
            mnav.Dispose();
            Assert.IsTrue(mnav.RepairPoolSize == 0);
        }
        
        [TestMethod]
        public void TestRepairPathDepthOne() 
        {
            // Only a search mDepth of one from the original path.
            // Only one posible Link to original path.
            MasterNavigator mnav = BuildRepairNavigator();
            Navigator nav = mnav.Nav;
            Vector3 start = new Vector3(2.5f, 0, 0.2f);
            Vector3 goal = new Vector3(2.5f, 0, 1.8f);
            MasterNavRequest<Path>.NavRequest req = nav.GetPath(start.x, start.y, start.z
                    , goal.x, goal.y, goal.z);
            mnav.ProcessAll(false);
            Path path = req.Data;
            Vector3 target;
            Assert.IsTrue(path.GetTarget(start.x, start.y, start.z, out target));
            Assert.IsTrue(Vector3Util.SloppyEquals(target, goal, TOLERANCE_STD));
            Vector3 oneOff = new Vector3(1.8f, 0, 0.2f);
            Assert.IsFalse(path.GetTarget(oneOff.x, oneOff.y, oneOff.z, out target));
            req = nav.RepairPath(oneOff.x, oneOff.y, oneOff.z, path);
            Assert.IsTrue(req.State == NavRequestState.Processing);
            mnav.ProcessOnce(false);
            Assert.IsTrue(req.IsFinished == false);
            mnav.ProcessAll(true);
            Assert.IsTrue(req.IsFinished);
            Assert.IsTrue(req.State == NavRequestState.Complete);
            path = req.Data;
            Assert.IsTrue(path.GetTarget(oneOff.x, oneOff.y, oneOff.z, out target));
            Assert.IsTrue(Vector3Util.SloppyEquals(target, goal, TOLERANCE_STD));
        }
        
        [TestMethod]
        public void TestRepairPathDepthTwo() 
        {
            // A search mDepth of two from the original path.
            // Two possible links to original path.
            MasterNavigator mnav = BuildRepairNavigator();
            Navigator nav = mnav.Nav;
            Vector3 start = new Vector3(2.5f, 0, 0.2f);
            Vector3 goal = new Vector3(2.5f, 0, 1.8f);
            MasterNavRequest<Path>.NavRequest req = nav.GetPath(start.x, start.y, start.z
                    , goal.x, goal.y, goal.z);
            mnav.ProcessAll(false);
            Path path = req.Data;
            Vector3 target = new Vector3();
            Assert.IsTrue(path.GetTarget(start.x, start.y, start.z, out target));
            Assert.IsTrue(Vector3Util.SloppyEquals(target, goal, TOLERANCE_STD));
            Vector3 twoOff = new Vector3(1.2f, 0, 0.8f);
            Assert.IsFalse(path.GetTarget(twoOff.x, twoOff.y, twoOff.z, out target));
            req = nav.RepairPath(twoOff.x, twoOff.y, twoOff.z, path);
            mnav.ProcessAll(true);
            Assert.IsTrue(req.IsFinished);
            Assert.IsTrue(req.State == NavRequestState.Complete);
            path = req.Data;
            Assert.IsTrue(path.GetTarget(twoOff.x, twoOff.y, twoOff.z, out target));
            Assert.IsTrue(Vector3Util.SloppyEquals(target, goal, TOLERANCE_STD));
        }
        
        [TestMethod]
        public void TestRepairPathDepthThree() 
        {
            // A search mDepth of two from the original path.
            // Two possible links to original path.
            MasterNavigator mnav = BuildRepairNavigator();
            Navigator nav = mnav.Nav;
            Vector3 start = new Vector3(2.5f, 0, 0.2f);
            Vector3 goal = new Vector3(2.5f, 0, 1.8f);
            MasterNavRequest<Path>.NavRequest req = nav.GetPath(start.x, start.y, start.z
                    , goal.x, goal.y, goal.z);
            mnav.ProcessAll(false);
            Path path = req.Data;
            Vector3 target = new Vector3();
            Assert.IsTrue(path.GetTarget(start.x, start.y, start.z, out target));
            Assert.IsTrue(Vector3Util.SloppyEquals(target, goal, TOLERANCE_STD));
            Vector3 threeOff = new Vector3(0.8f, 0, 0.4f);
            Assert.IsFalse(path.GetTarget(threeOff.x, threeOff.y, threeOff.z, out target));
            req = nav.RepairPath(threeOff.x, threeOff.y, threeOff.z, path);
            mnav.ProcessAll(true);
            Assert.IsTrue(req.IsFinished);
            Assert.IsTrue(req.State == NavRequestState.Failed);
        }
        
        [TestMethod]
        public void TestPathAgingBasic()
        {
            // Validates that patch cache is cleared.
            int poolSize = (int)(mMesh.GetPathCount +1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , 500            // Short Max age.
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            List<MasterNavRequest<Path>.NavRequest> reqs 
                = RequestAllPaths(nav);
            mnav.ProcessAll(false);
            Assert.IsTrue(mnav.ActivePathCount == mMesh.MultiPathCount);
            try { Thread.Sleep(510); }
            catch (Exception e) { if (e == null) { };  Assert.Fail("Thread sleep interupted."); }
            mnav.ProcessAll(true);  // <<<< Maintenance
            Assert.IsTrue(mnav.ActivePathCount == 0);
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
                Assert.IsTrue(reqs[iPath].Data.IsDisposed);
        }

        [TestMethod]
        public void TestKeepPathAlive() 
        {
            // Validates that patch cache is cleared.
            int maxAge = 250;
            int poolSize = (int)(mMesh.GetPathCount + 1);
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , maxAge            // Short Max age.
                    , SEARCH_DEPTH
                    , poolSize);
            Navigator nav = mnav.Nav;
            List<MasterNavRequest<Path>.NavRequest> reqs 
                = RequestAllPaths(nav);
            mnav.ProcessAll(false);
            Assert.IsTrue(mnav.ActivePathCount == mMesh.MultiPathCount);
            
            try { Thread.Sleep(maxAge + 1); }
            catch (Exception e) { if (e == null) { };  Assert.Fail("Thread sleep interupted."); }
            for (int iPath = 0; iPath < reqs.Count; iPath++)
                nav.KeepPathAlive(reqs[iPath].Data);
            mnav.ProcessAll(true);
            Assert.IsTrue(mnav.ActivePathCount == reqs.Count);
            for (int iPath = 0; iPath < reqs.Count; iPath++)
                Assert.IsTrue(reqs[iPath].Data.IsDisposed == false);
            MasterNavRequest<Path>.NavRequest dreq = null;
            // Let the paths expire one by one.
            while (reqs.Count > 0)
            {
                try { Thread.Sleep(maxAge + 1); }
                catch (Exception e) { if (e == null) { };  Assert.Fail("Thread sleep interupted."); }    
                dreq = reqs[reqs.Count-1];
                reqs.RemoveAt(reqs.Count-1);
                for (int iPath = 0; iPath < reqs.Count; iPath++)
                    nav.KeepPathAlive(reqs[iPath].Data);
                mnav.ProcessAll(true);
                Assert.IsTrue(mnav.ActivePathCount == reqs.Count);
                Assert.IsTrue(dreq.Data.IsDisposed);
                for (int iPath = 0; iPath < reqs.Count; iPath++)
                    Assert.IsTrue(reqs[iPath].Data.IsDisposed == false);
            }
        }

        [TestMethod]
        public void TestInternalPathRequests()
        {
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , 10);
            Navigator nav = mnav.Nav;
            
            // Create path requests.
            List<MasterNavRequest<Path>.NavRequest> reqs = RequestAllPaths(nav);
            Assert.IsTrue(mnav.PathRequestCount == reqs.Count);
            Assert.IsTrue(mnav.PathJobCount == 0);
            Assert.IsTrue(mnav.ActivePathCount == 0);
            mnav.Dispose();
            Assert.IsTrue(mnav.PathJobCount == 0);
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                Assert.IsTrue(req.State == NavRequestState.Failed);
        }

        [TestMethod]
        public void TestInternalPathJobs()
        {
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , 10);
            Navigator nav = mnav.Nav;
            
            // Create path jobs
            List<MasterNavRequest<Path>.NavRequest> reqs = RequestAllPaths(nav);
            mnav.ProcessOnce(false);
            Assert.IsTrue(mnav.PathRequestCount == 0);
            Assert.IsTrue(mnav.PathJobCount == reqs.Count);
            Assert.IsTrue(mnav.ActivePathCount == 0);
            mnav.Dispose();
            Assert.IsTrue(mnav.PathJobCount == 0);
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                Assert.IsTrue(req.State == NavRequestState.Failed);
        }

        [TestMethod]
        public void TestInternalPaths()
        {
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , 10);
            Navigator nav = mnav.Nav;
            
            // Create active paths.
            List<MasterNavRequest<Path>.NavRequest> reqs = RequestAllPaths(nav);
            mnav.ProcessAll(false);
            Assert.IsTrue(mnav.PathRequestCount == 0);
            Assert.IsTrue(mnav.PathJobCount == 0);
            Assert.IsTrue(mnav.ActivePathCount == reqs.Count);
            mnav.Dispose();
            Assert.IsTrue(mnav.ActivePathCount == 0);
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                Assert.IsTrue(req.Data.IsDisposed);
        }    
        
        [TestMethod]
        public void TestInternalRepairRequests()
        {
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , 10);
            Navigator nav = mnav.Nav;
            
            // Create active paths.
            List<MasterNavRequest<Path>.NavRequest> reqs = RequestAllPaths(nav);
            mnav.ProcessAll(false);
            // Create repair requests.
            float[] minVert = mMesh.MinVertex;
            reqs.Clear();
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                reqs.Add(nav.RepairPath(minVert[0]
                        , minVert[1]
                        , minVert[2]
                        , req.Data));
            Assert.IsTrue(mnav.RepairRequestCount == reqs.Count);
            Assert.IsTrue(mnav.RepairJobCount == 0);
            mnav.Dispose();
            Assert.IsTrue(mnav.RepairRequestCount == 0);
            Assert.IsTrue(mnav.RepairJobCount == 0);
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                Assert.IsTrue(req.State == NavRequestState.Failed);
        }
        
        [TestMethod]
        public void TestInternalRepairJobs()
        {
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , 10);
            Navigator nav = mnav.Nav;
            
            // Create active paths.
            List<MasterNavRequest<Path>.NavRequest> reqs = RequestAllPaths(nav);
            mnav.ProcessAll(false);
            // Create repair requests.
            float[] minVert = mMesh.MinVertex;
            reqs.Clear();
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                reqs.Add(nav.RepairPath(minVert[0]
                        , minVert[1]
                        , minVert[2]
                        , req.Data));
            mnav.ProcessOnce(false);
            Assert.IsTrue(mnav.RepairRequestCount == 0);
            Assert.IsTrue(mnav.RepairJobCount == reqs.Count);
            mnav.Dispose();
            Assert.IsTrue(mnav.RepairRequestCount == 0);
            Assert.IsTrue(mnav.RepairJobCount == 0);
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                Assert.IsTrue(req.State == NavRequestState.Failed);
        }
        
        [TestMethod]
        public void TestInternalKeepAliveRequests()
        {
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , 10);
            Navigator nav = mnav.Nav;
            
            // Create active paths.
            List<MasterNavRequest<Path>.NavRequest> reqs = RequestAllPaths(nav);
            mnav.ProcessAll(false);
            // Create keep alive requests.
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                nav.KeepPathAlive(req.Data);
            Assert.IsTrue(mnav.KeepAliveRequestCount == reqs.Count);
            mnav.Dispose();
            Assert.IsTrue(mnav.KeepAliveRequestCount == 0);
        }
        
        [TestMethod]
        public void TestInternalCancelRequests()
        {
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , 10);
            Navigator nav = mnav.Nav;
            
            // Create active paths.
            List<MasterNavRequest<Path>.NavRequest> reqs = RequestAllPaths(nav);
            mnav.ProcessOnce(false);
            // Create cancellations.
            foreach (MasterNavRequest<Path>.NavRequest req in reqs)
                nav.DiscardPathRequest(req);
            Assert.IsTrue(mnav.DiscardPathRequestCount == reqs.Count);
            mnav.Dispose();
            Assert.IsTrue(mnav.DiscardPathRequestCount == 0);
        }
        
        [TestMethod]
        public void TestInternalImmediateRequests()
        {
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , 10);
            Navigator nav = mnav.Nav;
            
            // Create location requests (both types.
            MasterNavRequest<Boolean>.NavRequest validLocReq = nav.IsValidLocation(mCents[0].x
                    , mCents[0].y
                    , mCents[0].z
                    , mMesh.PlaneTolerance);
            Assert.IsTrue(mnav.ValidLocationRequestCount == 1);
            MasterNavRequest<Vector3>.NavRequest nearestLocReq = nav.GetNearestValidLocation(mCents[0].x
                    , mCents[0].y
                    , mCents[0].z);
            Assert.IsTrue(mnav.NearestLocationRequestCount == 1);
            
            mnav.Dispose();
            Assert.IsTrue(mnav.ValidLocationRequestCount == 0);
            Assert.IsTrue(mnav.NearestLocationRequestCount == 0);
            Assert.IsTrue(validLocReq.State == NavRequestState.Failed);
            Assert.IsTrue(nearestLocReq.State == NavRequestState.Failed);
        }
        
        [TestMethod]
        public void TestIsDisposed() 
        {
            // Tests locations on the mesh surface or completely outside.
            MasterNavigator mnav = new MasterNavigator(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , SEARCH_DEPTH
                    , 10);
            Navigator nav = mnav.Nav;
            Assert.IsTrue(nav.IsDisposed == false);
            mnav.Dispose();
            Assert.IsTrue(nav.IsDisposed);
        }
        
        private int GetPathStartPoint(Path path, out Vector3 point)
        {
            point = new Vector3();
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                float[] pathPoints = mMesh.GetPathPoints(iPath);
                if (pathPoints[3] == path.GoalX
                        && pathPoints[4] == path.GoalY
                        && pathPoints[5] == path.GoalZ)    
                {
                    point = new Vector3(pathPoints[0], pathPoints[1], pathPoints[2]);
                    return iPath;
                }   
            }
            return -1;
        }
        
        private List<MasterNavRequest<Path>.NavRequest> RequestAllPaths(Navigator navigator)
        {
            float[] pathPoints;
            List<MasterNavRequest<Path>.NavRequest> result 
                = new List<MasterNavRequest<Path>.NavRequest>();
            for (int iPath = 0; iPath < mMesh.GetPathCount; iPath++)
            {
                pathPoints = mMesh.GetPathPoints(iPath);
                result.Add(navigator.GetPath(pathPoints[0]
                                     , pathPoints[1]
                                     , pathPoints[2]
                                     , pathPoints[3]
                                     , pathPoints[4]
                                     , pathPoints[5]));
            }
            return result;
        }
        
        private MasterNavigator BuildRepairNavigator()
        {
            float[] verts = {
                    0, 0, 0        // 0
                    , 1, 0, 0
                    , 2, 0, 0
                    , 3, 0, 0
                    , 1, 0, 1
                    , 2, 0, 1    // 5
                    , 3, 0, 1
                    , 2, 0, 2
                    , 3, 0, 2    // 8
            };
            int [] indices = {
                    0, 4, 1        // 0
                    , 1, 4, 5
                    , 1, 5, 2
                    , 2, 5, 6
                    , 2, 6, 3
                    , 4, 7, 5    // 5
                    , 5, 7, 6
                    , 7, 8, 6    // 7
            };
             TriNavMesh mesh = TriNavMesh.Build(verts, indices, 2, 0.5f, 0);
            return new MasterNavigator(mesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , MAX_PATH_AGE
                    , 2
                    , 5);
        }
    }
}
