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
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;
using org.critterai.mesh;

namespace org.critterai.nav.nmpath
{

    // TODO: Need to convert to use the Integration test data.

    /// <summary>
    /// Performs long running stress tests on a moderately
    /// complex mesh.
    /// This is a temporary test class.  It will be replaced
    /// by tests similar to those in the equivalent Java test suite.
    /// </summary>
    [TestClass]
    public class IntegrationTests
    {

        private IntegrationTestData testData;
        private float[] samples;

        [TestInitialize()]
        public void Setup()
        {
            testData = new IntegrationTestData(false);
            samples = testData.samplePoints.samples;
        }

        [TestMethod]
        public void IntegrationAStarSearchTest()
        {
            AStarSearch search = new AStarSearch(testData.heuristic);
            Vector3 trashV;
            for (int pStart = 0; pStart < samples.Length; pStart += 3)
            {
                // Dev Note: Purposely allowing start and goal to be the same.
                for (int pGoal = pStart; pGoal < samples.Length; pGoal += 3)
                {
                    TriCell startCell = testData.navMesh.GetClosestCell(
                        samples[pStart]
                        , samples[pStart+1]
                        , samples[pStart+2]
                        , true
                        , out trashV);
                    TriCell goalCell = testData.navMesh.GetClosestCell(
                        samples[pGoal]
                        , samples[pGoal + 1]
                        , samples[pGoal + 2]
                        , true
                        , out trashV);

                    search.Initialize(samples[pStart]
                        , samples[pStart + 1]
                        , samples[pStart + 2]
                        , samples[pGoal]
                        , samples[pGoal + 1]
                        , samples[pGoal + 2]
                        , startCell
                        , goalCell);

                    int maxLoopCount = testData.sourceIndices.Length / 3 + 1;

                    int loopCount = 0;
                    while (search.IsActive && loopCount < maxLoopCount)
                    {
                        search.Process();
                        loopCount++;
                    }

                    string state = "[" + pStart + ":" + pGoal + "]"
                        + "(" + samples[pStart]
                        + ", " + samples[pStart + 1]
                        + ", " + samples[pStart + 2]
                        + ") -> (" + samples[pGoal]
                        + ", " + samples[pGoal + 1]
                        + ", " + samples[pGoal + 2] + ")";
                    Assert.IsTrue(search.State == SearchState.Complete, state);

                    // Reverse points.
                    search.Initialize(samples[pGoal]
                        , samples[pGoal + 1]
                        , samples[pGoal + 2]
                        , samples[pStart]
                        , samples[pStart + 1]
                        , samples[pStart + 2]
                        , goalCell
                        , startCell);

                    loopCount = 0;
                    while (search.IsActive && loopCount < maxLoopCount)
                    {
                        search.Process();
                        loopCount++;
                    }

                    state = "(" + samples[pGoal]
                        + ", " + samples[pGoal + 1]
                        + ", " + samples[pGoal + 2]
                        + ") -> (" + samples[pStart]
                        + ", " + samples[pStart + 1]
                        + ", " + samples[pStart + 2];
                    Assert.IsTrue(search.State == SearchState.Complete, state);
                }
            } 
        }

        [TestMethod]
        public void IntegrationThreadedSearchTest()
        {
            ThreadedPlanner masterNavigator = PlannerUtil.GetThreadedPlanner(
                    testData.sourceVerts
                    , testData.sourceIndices
                    , testData.spacialDepth
                    , testData.planeTolerance
                    , testData.offsetScale
                    , testData.heuristic
                    , testData.frameLength
                    , testData.maxFrameTimeslice
                    , testData.maxPathAge
                    , testData.repairSearchDepth
                    , testData.searchPoolMax
                    , testData.maintenanceFrequency);

            masterNavigator.Start();

            Assert.IsTrue(masterNavigator.IsRunning);

            List<PathSearchInfo> searchInfo = new List<PathSearchInfo>(10000);
            MasterPlanner.Planner nav = masterNavigator.PathPlanner;

            for (int pStart = 0; pStart < samples.Length; pStart += 3)
            {
                // Dev Note: Purposely allowing start and goal to be the same.
                for (int pGoal = pStart; pGoal < samples.Length; pGoal += 3)
                {

                    Vector3 start = new Vector3(samples[pStart]
                        , samples[pStart + 1]
                        , samples[pStart + 2]);

                    Vector3 goal = new Vector3(samples[pGoal]
                        , samples[pGoal + 1]
                        , samples[pGoal + 2]);

                    PathSearchInfo si = new PathSearchInfo(start
                        , goal
                        , nav.GetPath(start.x, start.y, start.z
                            , goal.x, goal.y, goal.z));

                    searchInfo.Add(si);

                    goal = new Vector3(samples[pStart]
                        , samples[pStart + 1]
                        , samples[pStart + 2]);

                    start = new Vector3(samples[pGoal]
                        , samples[pGoal + 1]
                        , samples[pGoal + 2]);

                    si = new PathSearchInfo(start
                        , goal
                        , nav.GetPath(start.x, start.y, start.z
                            , goal.x, goal.y, goal.z));

                    searchInfo.Add(si);
                }
            }

            int maxLoopCount = 120;

            int loopCount = 0;
            while (searchInfo.Count > 0 && loopCount < maxLoopCount)
            {
                for (int i = searchInfo.Count - 1; i >= 0; i--)
                {
                    PathSearchInfo si = searchInfo[i];
                    if (si.pathRequest.IsFinished)
                    {
                        string state = si.start + " -> " + si.goal;
                        Assert.IsTrue(si.pathRequest.State == 
                            NavRequestState.Complete, state);
                        searchInfo.RemoveAt(i);
                    }
                }
                Thread.Sleep(1000);
                loopCount++;
            }
            Assert.IsTrue(searchInfo.Count == 0);
        }

        [TestMethod]
        public void IntegrationMeshConnectivityTest()
        {
            Vector3[] centroids = new Vector3[testData.cells.Length];
            for (int iCell = 0; iCell < testData.cells.Length; iCell++)
            {
                TriCell cell = testData.cells[iCell];
                centroids[iCell] = new Vector3(cell.CentroidX
                    , cell.CentroidY
                    , cell.CentroidZ);
            }

            ThreadedPlanner masterNavigator = PlannerUtil.GetThreadedPlanner(
                    testData.sourceVerts
                    , testData.sourceIndices
                    , testData.spacialDepth
                    , testData.planeTolerance
                    , testData.offsetScale
                    , testData.heuristic
                    , testData.frameLength
                    , testData.maxFrameTimeslice
                    , testData.maxPathAge
                    , testData.repairSearchDepth
                    , testData.searchPoolMax
                    , testData.maintenanceFrequency);

            masterNavigator.Start();

            Assert.IsTrue(masterNavigator.IsRunning);

            List<PathSearchInfo> searchInfo = 
                new List<PathSearchInfo>(centroids.Length*2);
            MasterPlanner.Planner nav = masterNavigator.PathPlanner;

            for (int pStart = 0; pStart < centroids.Length; pStart++)
            {
                // Dev Note: Purposely allowing start and goal to be the same.
                for (int pGoal = pStart; pGoal < centroids.Length; pGoal++)
                {
                    Vector3 start = centroids[pStart];
                    Vector3 goal = centroids[pGoal];

                    PathSearchInfo si = new PathSearchInfo(start
                        , goal
                        , nav.GetPath(start.x, start.y, start.z
                            , goal.x, goal.y, goal.z));

                    searchInfo.Add(si);

                    start = centroids[pGoal];
                    goal = centroids[pStart];

                    si = new PathSearchInfo(start
                        , goal
                        , nav.GetPath(start.x, start.y, start.z
                            , goal.x, goal.y, goal.z));

                    searchInfo.Add(si);
                }
            }

            int maxLoopCount = 240;

            int loopCount = 0;
            while (searchInfo.Count > 0 && loopCount < maxLoopCount)
            {
                for (int i = searchInfo.Count - 1; i >= 0; i--)
                {
                    PathSearchInfo si = searchInfo[i];
                    if (si.pathRequest.IsFinished)
                    {
                        string state = si.start + " -> " + si.goal;
                        Assert.IsTrue(si.pathRequest.State == 
                            NavRequestState.Complete, state);
                        searchInfo.RemoveAt(i);
                    }
                }
                Thread.Sleep(1000);
                loopCount++;
            }
            Assert.IsTrue(searchInfo.Count == 0);

        }
    }



    struct PathSearchInfo
    {
        public Vector3 start;
        public Vector3 goal;
        public MasterNavRequest<MasterPath.Path>.NavRequest pathRequest;

        public PathSearchInfo(Vector3 start
            , Vector3 goal
            , MasterNavRequest<MasterPath.Path>.NavRequest pathRequest)
        {
            this.start = start;
            this.goal = goal;
            this.pathRequest = pathRequest;
        }
    }

}
