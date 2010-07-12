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
using org.critterai.math;
using org.critterai.math.geom;
using UnityEngine;

namespace org.critterai.nav
{
    /// <summary>
    /// Summary description for TriNavMeshTests
    /// </summary>
    [TestClass]
    public sealed class TriNavMeshTests
    {
        /*
         * Design notes:
         * 
         * Validity of these tests are highly dependent on the validity of
         * the NavPoly and NavPolyQuadTree classes.
         * 
         * Full functionality is not being tested.  Tests are abbreviated 
         * based on knowledge of the internal design of the class.
         */

        private TestContext testContextInstance;

        private ITestMesh mMesh;
        private const float PLANE_TOL = 0.5f;
        private const float OFFSET_SCALE = 0.05f;
        private const float TOLERANCE_STD = MathUtil.TOLERANCE_STD;

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
        }

        [TestMethod]
        public void TestBasics() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriNavMesh nm = TriNavMesh.Build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
            Assert.IsTrue(nm.PlaneTolerance == PLANE_TOL);
            Assert.IsTrue(nm.OffsetScale == OFFSET_SCALE);
        }

        [TestMethod]
        public void TestGetClosestCell() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TriNavMesh nm = TriNavMesh.Build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
            Vector3 v;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                float ax = verts[indices[pCell]*3];
                float ay = verts[indices[pCell]*3+1];
                float az = verts[indices[pCell]*3+2];
                float bx = verts[indices[pCell+1]*3];
                float by = verts[indices[pCell+1]*3+1];
                float bz = verts[indices[pCell+1]*3+2];
                float cx = verts[indices[pCell+2]*3];
                float cy = verts[indices[pCell+2]*3+1];
                float cz = verts[indices[pCell+2]*3+2];
                Vector3 cent = Polygon3.GetCentroid(ax, ay, az
                                                , bx, by, bz
                                                , cx, cy, cz);
                TriCell cell = nm.GetClosestCell(cent.x, cent.y, cent.z, true, out v);
                Assert.IsTrue(Vector3Util.SloppyEquals(v, cent, TOLERANCE_STD));
                for (int i = 0; i < cell.MaxLinks; i++)
                {
                    Assert.IsTrue(cell.GetVertex(i) == cells[iCell].GetVertex(i));
                }
            }
            float[] va = mMesh.MinVertex;
            TriCell np = nm.GetClosestCell(va[0] - TOLERANCE_STD
                    , va[1] - TOLERANCE_STD
                    , va[2] - TOLERANCE_STD
                    , true
                    , out v);
            Assert.IsTrue(np == null);
        }

        [TestMethod]
        public void TestIsValidPosition() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TriNavMesh nm = TriNavMesh.Build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                float ax = verts[indices[pCell]*3];
                float ay = verts[indices[pCell]*3+1];
                float az = verts[indices[pCell]*3+2];
                float bx = verts[indices[pCell+1]*3];
                float by = verts[indices[pCell+1]*3+1];
                float bz = verts[indices[pCell+1]*3+2];
                float cx = verts[indices[pCell+2]*3];
                float cy = verts[indices[pCell+2]*3+1];
                float cz = verts[indices[pCell+2]*3+2];
                Vector3 cent = Polygon3.GetCentroid(ax, ay, az
                                                , bx, by, bz
                                                , cx, cy, cz);
                Assert.IsTrue(nm.IsValidPosition(cent.x, cent.y, cent.z, TOLERANCE_STD));
                Assert.IsFalse(nm.IsValidPosition(cent.x, cent.y + 2*TOLERANCE_STD, cent.z, TOLERANCE_STD));
                Assert.IsFalse(nm.IsValidPosition(cent.x, cent.y - 2*TOLERANCE_STD, cent.z, TOLERANCE_STD));
            }
        }

        [TestMethod]
        public void TestHasLOSTrue() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            int[] losPolys = mMesh.GetLOSPolysTrue();
            float[] losPoints = mMesh.GetLOSPointsTrue();
            TriNavMesh nm = TriNavMesh.Build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
            int pathCount = losPoints.Length / 4;
            Vector3 trashV;
            for (int iPath = 0; iPath < pathCount; iPath++)
            {
                int pPathPoint = iPath*4;
                int pPathPoly = iPath*2;
                TriCell startPoly = cells[losPolys[pPathPoly]];
                startPoly = nm.GetClosestCell(startPoly.CentroidX
                        , startPoly.CentroidY
                        , startPoly.CentroidZ
                        , true, out trashV);
                TriCell endPoly = cells[losPolys[pPathPoly+1]];
                endPoly = nm.GetClosestCell(endPoly.CentroidX
                        , endPoly.CentroidY
                        , endPoly.CentroidZ
                        , true, out trashV);
                Boolean actual = TriNavMesh.HasLOS(losPoints[pPathPoint]
                        , losPoints[pPathPoint+1]
                        , losPoints[pPathPoint+2]
                        , losPoints[pPathPoint+3]
                        , startPoly
                        , endPoly
                        , OFFSET_SCALE);
                Assert.IsTrue(actual);
                // Reverse path.
                actual = TriNavMesh.HasLOS(losPoints[pPathPoint+2]
                          , losPoints[pPathPoint+3]
                          , losPoints[pPathPoint]
                          , losPoints[pPathPoint+1]
                          , endPoly
                          , startPoly
                        , OFFSET_SCALE);
                Assert.IsTrue(actual);
            }
        }

        [TestMethod]
        public void TestHasLOSFalse() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            int[] losPolys = mMesh.GetLOSPolysFalse();
            float[] losPoints = mMesh.GetLOSPointsFalse();
            TriNavMesh nm = TriNavMesh.Build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
            int pathCount = losPoints.Length / 4;
            Vector3 trashV;
            for (int iPath = 0; iPath < pathCount; iPath++)
            {
                int pPathPoint = iPath*4;
                int pPathPoly = iPath*2;
                TriCell startPoly = cells[losPolys[pPathPoly]];
                startPoly = nm.GetClosestCell(startPoly.CentroidX
                        , startPoly.CentroidY
                        , startPoly.CentroidZ
                        , true, out trashV);
                TriCell endPoly = cells[losPolys[pPathPoly+1]];
                endPoly = nm.GetClosestCell(endPoly.CentroidX
                        , endPoly.CentroidY
                        , endPoly.CentroidZ
                        , true, out trashV);
                Boolean actual = TriNavMesh.HasLOS(losPoints[pPathPoint]
                        , losPoints[pPathPoint+1]
                        , losPoints[pPathPoint+2]
                        , losPoints[pPathPoint+3]
                        , startPoly
                        , endPoly
                        , OFFSET_SCALE);
                Assert.IsFalse(actual);
                // Reverse path.
                actual = TriNavMesh.HasLOS(losPoints[pPathPoint+2]
                          , losPoints[pPathPoint+3]
                          , losPoints[pPathPoint]
                          , losPoints[pPathPoint+1]
                          , endPoly
                          , startPoly
                        , OFFSET_SCALE);
                Assert.IsFalse(actual);
            }
        }
    }
}
