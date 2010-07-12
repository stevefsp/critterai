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

namespace org.critterai.nav
{
    /// <summary>
    /// Performs long running stress tests on a moderately
    /// complex mesh.
    /// This is a temporary test class.  It will be replaced
    /// by tests similar to those in the equivalent Java test suite.
    /// </summary>
    [TestClass]
    public class IntegrationTests
    {
        private const String TEST_FILE_NAME = "assets.BISHomeGAS.obj";
        private const int goalCount = 1000;

        private const int spacialDepth = 8;
        private const float planeTolerance = 0.5f;
        private const float offsetScale = 0.1f;
        private const DistanceHeuristicType heuristic = DistanceHeuristicType.LongestAxis;
        private const int frameLength = 0;   // ticks (100ns)
        private const long maxFrameTimeslice = 20000; // ticks (100ns)
        private const int maxPathAge = 60000; // ms
        private const int repairSearchDepth = 2;
        private const int searchPoolMax = 40;
        private const long maintenanceFrequency = 500; // ms

        private Boolean mirrorMesh = false;

        private float[] mSourceVerts;
        private int[] mSourceIndices;
        private TriNavMesh mNavMesh;
        private float[] mMeshPoints;
        private Vector3 mMeshMin;
        private Vector3 mMeshMax;
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

        [ClassInitialize()]
        public static void SetupOnce(TestContext testContext)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            String[] rns = asm.GetManifestResourceNames();
            StreamReader reader = new StreamReader(
                asm.GetManifestResourceStream(typeof(IntegrationTests)
                , TEST_FILE_NAME));

            StreamWriter writer = new StreamWriter(TEST_FILE_NAME);
            writer.Write(reader.ReadToEnd());
            writer.Close(); 
        }

        [TestInitialize()]
        public void Setup()
        {
            QuickMesh mesh = new QuickMesh(TEST_FILE_NAME, true);
            mSourceVerts = mesh.verts;
            mSourceIndices = mesh.indices;
            mMeshMin = mesh.minBounds;
            mMeshMax = mesh.maxBounds;
            mesh = null;

            if (mirrorMesh)
            {
                for (int p = 0; p < mSourceVerts.Length; p += 3)
                {
                    mSourceVerts[p] *= -1;
                }
                for (int p = 0; p < mSourceIndices.Length; p += 3)
                {
                    int t = mSourceIndices[p + 1];
                    mSourceIndices[p + 1] = mSourceIndices[p + 2];
                    mSourceIndices[p + 2] = t;
                }
                float tmp = mMeshMin.x * -1;
                mMeshMin.x = mMeshMax.x * -1;
                mMeshMax.x = tmp;
            }

            mCells = TestUtil.GetAllCells(mSourceVerts, mSourceIndices);
            TestUtil.LinkAllCells(mCells);

            mNavMesh = TriNavMesh.Build(mSourceVerts
                , mSourceIndices
                , spacialDepth
                , planeTolerance
                , offsetScale);

            mMeshPoints = new float[goalCount*3];

            System.Random r = new System.Random(89725);

            Vector3 loc;
            for (int p = 0; p < mMeshPoints.Length; p += 3)
            {
                while (true)
                {
                    float x = mMeshMin.x + (float)r.NextDouble() * (mMeshMax.x - mMeshMin.x);
                    float y = mMeshMin.y + (float)r.NextDouble() * (mMeshMax.y - mMeshMin.y);
                    float z = mMeshMin.z + (float)r.NextDouble() * (mMeshMax.z - mMeshMin.z);

                    if (mNavMesh.IsValidPosition(x, y, z, planeTolerance))
                    {
                        TriCell c = mNavMesh.GetClosestCell(x, y, z, true, out loc);  // Snap y.
                        //if (p == 39)
                        //    Console.WriteLine(c);
                        mMeshPoints[p + 0] = loc.x;
                        mMeshPoints[p + 1] = loc.y;
                        mMeshPoints[p + 2] = loc.z;
                        break;
                    }
                }
            }
        }

        [TestMethod]
        public void IntegrationAStarSearchTest()
        {
            AStarSearch search = new AStarSearch(heuristic);
            Vector3 trashV;
            for (int pStart = 0; pStart < mMeshPoints.Length; pStart += 3)
            {
                // Dev Note: Purposely allowing start and goal to be the same.
                for (int pGoal = pStart; pGoal < mMeshPoints.Length; pGoal += 3)
                {
                    TriCell startCell = mNavMesh.GetClosestCell(mMeshPoints[pStart]
                        , mMeshPoints[pStart+1]
                        , mMeshPoints[pStart+2]
                        , true
                        , out trashV);
                    TriCell goalCell = mNavMesh.GetClosestCell(mMeshPoints[pGoal]
                        , mMeshPoints[pGoal + 1]
                        , mMeshPoints[pGoal + 2]
                        , true
                        , out trashV);

                    search.Initialize(mMeshPoints[pStart]
                        , mMeshPoints[pStart + 1]
                        , mMeshPoints[pStart + 2]
                        , mMeshPoints[pGoal]
                        , mMeshPoints[pGoal + 1]
                        , mMeshPoints[pGoal + 2]
                        , startCell
                        , goalCell);

                    int maxLoopCount = mSourceIndices.Length / 3 + 1;

                    int loopCount = 0;
                    while (search.IsActive && loopCount < maxLoopCount)
                    {
                        search.Process();
                        loopCount++;
                    }

                    string state = "[" + pStart + ":" + pGoal + "]"
                        + "(" + mMeshPoints[pStart]
                        + ", " + mMeshPoints[pStart + 1]
                        + ", " + mMeshPoints[pStart + 2]
                        + ") -> (" + mMeshPoints[pGoal]
                        + ", " + mMeshPoints[pGoal + 1]
                        + ", " + mMeshPoints[pGoal + 2] + ")";
                    Assert.IsTrue(search.State == SearchState.Complete, state);

                    // Reverse points.
                    search.Initialize(mMeshPoints[pGoal]
                        , mMeshPoints[pGoal + 1]
                        , mMeshPoints[pGoal + 2]
                        , mMeshPoints[pStart]
                        , mMeshPoints[pStart + 1]
                        , mMeshPoints[pStart + 2]
                        , goalCell
                        , startCell);

                    loopCount = 0;
                    while (search.IsActive && loopCount < maxLoopCount)
                    {
                        search.Process();
                        loopCount++;
                    }

                    state = "(" + mMeshPoints[pGoal]
                        + ", " + mMeshPoints[pGoal + 1]
                        + ", " + mMeshPoints[pGoal + 2]
                        + ") -> (" + mMeshPoints[pStart]
                        + ", " + mMeshPoints[pStart + 1]
                        + ", " + mMeshPoints[pStart + 2];
                    Assert.IsTrue(search.State == SearchState.Complete, state);
                }
            } 
        }

        [TestMethod]
        public void IntegrationThreadedSearchTest()
        {
            ThreadedNavigator masterNavigator = NavUtil.GetThreadedNavigator(
                    mSourceVerts
                    , mSourceIndices
                    , spacialDepth
                    , planeTolerance
                    , offsetScale
                    , heuristic
                    , frameLength
                    , maxFrameTimeslice
                    , maxPathAge
                    , repairSearchDepth
                    , searchPoolMax
                    , maintenanceFrequency);

            masterNavigator.Start();

            Assert.IsTrue(masterNavigator.IsRunning);

            List<PathSearchInfo> searchInfo = new List<PathSearchInfo>(10000);
            MasterNavigator.Navigator nav = masterNavigator.Nav;

            for (int pStart = 0; pStart < mMeshPoints.Length; pStart += 3)
            {
                // Dev Note: Purposely allowing start and goal to be the same.
                for (int pGoal = pStart; pGoal < mMeshPoints.Length; pGoal += 3)
                {

                    Vector3 start = new Vector3(mMeshPoints[pStart]
                        , mMeshPoints[pStart + 1]
                        , mMeshPoints[pStart + 2]);

                    Vector3 goal = new Vector3(mMeshPoints[pGoal]
                        , mMeshPoints[pGoal + 1]
                        , mMeshPoints[pGoal + 2]);

                    PathSearchInfo si = new PathSearchInfo(start
                        , goal
                        , nav.GetPath(start.x, start.y, start.z, goal.x, goal.y, goal.z));

                    searchInfo.Add(si);

                    goal = new Vector3(mMeshPoints[pStart]
                        , mMeshPoints[pStart + 1]
                        , mMeshPoints[pStart + 2]);

                    start = new Vector3(mMeshPoints[pGoal]
                        , mMeshPoints[pGoal + 1]
                        , mMeshPoints[pGoal + 2]);

                    si = new PathSearchInfo(start
                        , goal
                        , nav.GetPath(start.x, start.y, start.z, goal.x, goal.y, goal.z));

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
                        Assert.IsTrue(si.pathRequest.State == NavRequestState.Complete, state);
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
            Vector3[] centroids = new Vector3[mCells.Length];
            for (int iCell = 0; iCell < mCells.Length; iCell++)
            {
                TriCell cell = mCells[iCell];
                centroids[iCell] = new Vector3(cell.CentroidX, cell.CentroidY, cell.CentroidZ);
            }

            ThreadedNavigator masterNavigator = NavUtil.GetThreadedNavigator(
                    mSourceVerts
                    , mSourceIndices
                    , spacialDepth
                    , planeTolerance
                    , offsetScale
                    , heuristic
                    , frameLength
                    , maxFrameTimeslice
                    , maxPathAge
                    , repairSearchDepth
                    , searchPoolMax
                    , maintenanceFrequency);

            masterNavigator.Start();

            Assert.IsTrue(masterNavigator.IsRunning);

            List<PathSearchInfo> searchInfo = new List<PathSearchInfo>(centroids.Length*2);
            MasterNavigator.Navigator nav = masterNavigator.Nav;

            for (int pStart = 0; pStart < centroids.Length; pStart++)
            {
                // Dev Note: Purposely allowing start and goal to be the same.
                for (int pGoal = pStart; pGoal < centroids.Length; pGoal++)
                {
                    Vector3 start = centroids[pStart];
                    Vector3 goal = centroids[pGoal];

                    PathSearchInfo si = new PathSearchInfo(start
                        , goal
                        , nav.GetPath(start.x, start.y, start.z, goal.x, goal.y, goal.z));

                    searchInfo.Add(si);

                    start = centroids[pGoal];
                    goal = centroids[pStart];

                    si = new PathSearchInfo(start
                        , goal
                        , nav.GetPath(start.x, start.y, start.z, goal.x, goal.y, goal.z));

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
                        Assert.IsTrue(si.pathRequest.State == NavRequestState.Complete, state);
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
