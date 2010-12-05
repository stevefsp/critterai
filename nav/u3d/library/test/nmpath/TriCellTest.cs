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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.critterai.math;
using org.critterai.math.geom;
using UnityEngine;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    ///This is a test class for TriCellTest and is intended
    ///to contain all TriCellTest Unit Tests
    ///</summary>
    [TestClass()]
    public sealed class TriCellTest
    {
        /*
         * Design notes:
         * 
         * The main test locations are around walls, vertices, and centroids.
         * 
         * The biggest assumption in the design is that
         * testing around wall midpoints is all that is needed for wall-based tests.
         * E.G. Testing intersection of a wall in the vicinity of the midpoint covers all needed
         * intersection tests for the wall.  No need to test around other points on the wall.
         * 
         */

        private TestContext testContextInstance;

        private const float TOLERANCE_STD = MathUtil.TOLERANCE_STD;

        private readonly System.Random mR = new System.Random(1);
        private ITestMesh mMesh;

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
        public void Setup()
        {
            mMesh = new CorridorMesh();
            
        }

        public void TestFieldCentroid()
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                TriCell cell = cells[iCell];
                Vector3 cent = Polygon3.GetCentroid(verts[indices[pCell]*3]
                                        , verts[indices[pCell]*3+1]
                                        , verts[indices[pCell]*3+2]
                                        , verts[indices[pCell+1]*3]
                                        , verts[indices[pCell+1]*3+1]
                                        , verts[indices[pCell+1]*3+2]
                                        , verts[indices[pCell+2]*3]
                                        , verts[indices[pCell+2]*3+1]
                                        , verts[indices[pCell+2]*3+2]);
                Assert.IsTrue(cell.CentroidX == cent.x);
                Assert.IsTrue(cell.CentroidY == cent.y);
                Assert.IsTrue(cell.CentroidZ == cent.z);            
            }
        }

        [TestMethod()]
        public void TestFieldBounds()
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                TriCell cell = cells[iCell];
                int matchesX = 0;
                int matchesZ = 0;
                for (int i = 0; i < 3; i++)
                {
                    Assert.IsTrue(verts[indices[pCell+i]*3] >= cell.BoundsMinX);
                    Assert.IsTrue(verts[indices[pCell+i]*3] <= cell.BoundsMaxX);
                    Assert.IsTrue(verts[indices[pCell+i]*3+2] >= cell.BoundsMinZ);
                    Assert.IsTrue(verts[indices[pCell+i]*3+2] <= cell.BoundsMaxZ);
                    if (verts[indices[pCell+i]*3] == cell.BoundsMinX 
                            || verts[indices[pCell+i]*3] == cell.BoundsMaxX)
                        matchesX++;
                    if (verts[indices[pCell+i]*3+2] == cell.BoundsMinZ
                            || verts[indices[pCell+i]*3+2] == cell.BoundsMaxZ)
                        matchesZ++;
                }
                Assert.IsTrue(matchesX >= 2);
                Assert.IsTrue(matchesZ >= 2);
            }
        }
        
        [TestMethod()]
        public void TestGetVertexValue() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            for (int pCell = 0; pCell < indices.Length; pCell += 3)
            {
                TriCell cell = new TriCell(verts
                        , indices[pCell]
                        , indices[pCell+1]
                        , indices[pCell+2]);
                for (int iVert = 0; iVert < 3; iVert++)
                {
                    for (int iValue = 0; iValue < 3; iValue++)
                    {
                        Assert.IsTrue(cell.GetVertexValue(iVert, iValue) 
                                == verts[indices[pCell+iVert]*3+iValue]);                        
                    }
                }
            }
        }

        [TestMethod()]
        public void TestGetVertex() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            for (int pCell = 0; pCell < indices.Length; pCell += 3)
            {
                TriCell cell = new TriCell(verts
                        , indices[pCell]
                        , indices[pCell+1]
                        , indices[pCell+2]);
                for (int iVert = 0; iVert < 3; iVert++)
                {
                    Vector3 vr = cell.GetVertex(iVert);
                    Assert.IsTrue(vr.x == verts[indices[pCell+iVert]*3]);
                    Assert.IsTrue(vr.y == verts[indices[pCell+iVert]*3+1]);
                    Assert.IsTrue(vr.z == verts[indices[pCell+iVert]*3+2]);
                }
            }
        }

        [TestMethod()]
        public void TestGetVertexIndex3D() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            for (int pCell = 0; pCell < indices.Length; pCell += 3)
            {
                TriCell cell = new TriCell(verts
                        , indices[pCell]
                        , indices[pCell+1]
                        , indices[pCell+2]);
                for (int iVert = 0; iVert < 3; iVert++)
                {
                    // Randomize the tolerance between zero and 0.001;
                    // Randomize the offset direction for x, y, and z.
                    float tol = (float)mR.NextDouble() / 1000;
                    float vx = verts[indices[pCell+iVert]*3] + tol * ((float)mR.NextDouble() > 0.5 ? 1 : -1);
                    float vy = verts[indices[pCell+iVert]*3+1] - tol * ((float)mR.NextDouble() > 0.5 ? 1 : -1);
                    float vz = verts[indices[pCell+iVert]*3+2] - tol * ((float)mR.NextDouble() > 0.5 ? 1 : -1);
                    // Note: Must increase tolerance slightly to account for floating point errors.
                    int vi = cell.GetVertexIndex(vx, vy, vz, 1.01f * tol);
                    Assert.IsTrue(vi == iVert);
                }
            }
        }

        [TestMethod()]
        public void TestGetVertexIndex2D() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            for (int pCell = 0; pCell < indices.Length; pCell += 3)
            {
                TriCell cell = new TriCell(verts
                        , indices[pCell]
                        , indices[pCell+1]
                        , indices[pCell+2]);
                for (int iVert = 0; iVert < 3; iVert++)
                {
                    // Randomize the tolerance between zero and 0.001;
                    // Randomize the offset direction for x and z.
                    float tol = (float)mR.NextDouble() / 1000;
                    float vx = verts[indices[pCell+iVert]*3] + tol * ((float)mR.NextDouble() > 0.5 ? 1 : -1);
                    float vz = verts[indices[pCell+iVert]*3+2] - tol * ((float)mR.NextDouble() > 0.5 ? 1 : -1);
                    // Note: Have to increase tolerance slightly to account for floating point errors.
                    int vi = cell.GetVertexIndex(vx, vz, 1.01f * tol);
                    Assert.IsTrue(vi == iVert);
                }
            }
        }

        [TestMethod()]
        public void TestNoLinks()
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            for (int pCell = 0; pCell < indices.Length; pCell += 3)
            {
                TriCell cell = new TriCell(verts
                        , indices[pCell]
                        , indices[pCell+1]
                        , indices[pCell+2]);
                Assert.IsNull(cell.GetLink(0));
                Assert.IsNull(cell.GetLink(1));
                Assert.IsNull(cell.GetLink(2));
                Assert.IsTrue(cell.LinkCount == 0);
            }
        }
        
        [TestMethod()]
        public void TestLinkCount() 
        {
            int[] linkCount = mMesh.GetLinkCounts();
            TriCell[] cells = TestUtil.GetAllCells(mMesh.GetVerts(), mMesh.GetIndices());
            TestUtil.LinkAllCells(cells);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                Assert.IsTrue(cells[iCell].LinkCount == linkCount[iCell]);
            }
        }

        [TestMethod()]
        public void TestVertCount() 
        {
            TriCell[] cells = TestUtil.GetAllCells(mMesh.GetVerts(), mMesh.GetIndices());
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                Assert.IsTrue(cells[iCell].MaxLinks == 3);
            }
        }

        [TestMethod()]
        public void TestGetLink() 
        {
            int[] linkCells = mMesh.GetLinkPolys();
            TriCell[] cells = TestUtil.GetAllCells(mMesh.GetVerts(), mMesh.GetIndices());
            TestUtil.LinkAllCells(cells);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iLink = 0; iLink < 3; iLink++)
                {
                    TriCell linkedPoly = cells[iCell].GetLink(iLink);
                    if (linkedPoly == null)
                        Assert.IsTrue(linkCells[iCell*3+iLink] == -1, iCell + ":" + iLink);
                    else
                        Assert.IsTrue(linkedPoly == cells[linkCells[iCell*3+iLink]], iCell + ":" + iLink);
                }
            }
        }

        [TestMethod()]
        public void TestGetLinkWall() 
        {
            int[] linkWalls = mMesh.GetLinkWalls();
            TriCell[] cells = TestUtil.GetAllCells(mMesh.GetVerts(), mMesh.GetIndices());
            TestUtil.LinkAllCells(cells);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iLink = 0; iLink < 3; iLink++)
                {
                    int linkedWall = cells[iCell].GetLinkWall(iLink);
                    Assert.IsTrue(linkedWall == linkWalls[iCell*3+iLink], iCell + ":" + iLink);
                }
            }
        }

        [TestMethod()]
        public void TestGetWallRightVertex() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    Vector3 v = cells[iCell].GetWallRightVertex(iWall);
                    int iVert = (iWall == 2 ? 0 : iWall + 1);
                    Assert.IsTrue(v.x == verts[indices[iCell*3+iVert]*3]);
                    Assert.IsTrue(v.y == verts[indices[iCell*3+iVert]*3+1]);
                    Assert.IsTrue(v.z == verts[indices[iCell*3+iVert]*3+2]);
                }
            }
        }

        [TestMethod()]
        public void TestGetWallLeftVertex() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    Vector3 v = cells[iCell].GetWallLeftVertex(iWall);
                    Assert.IsTrue(v.x == verts[indices[iCell*3+iWall]*3]);
                    Assert.IsTrue(v.y == verts[indices[iCell*3+iWall]*3+1]);
                    Assert.IsTrue(v.z == verts[indices[iCell*3+iWall]*3+2]);
                }
            }
        }

        [TestMethod()]
        public void TestGetWallDistance() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                Vector3 cent = Polygon3.GetCentroid(verts[indices[pCell]*3]
                                                , verts[indices[pCell]*3+1]
                                                , verts[indices[pCell]*3+2]
                                                , verts[indices[pCell+1]*3]
                                                , verts[indices[pCell+1]*3+1]
                                                , verts[indices[pCell+1]*3+2]
                                                , verts[indices[pCell+2]*3]
                                                , verts[indices[pCell+2]*3+1]
                                                , verts[indices[pCell+2]*3+2]);            
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float actual = cells[iCell].GetWallDistance(cent.x, cent.z, iWall);
                    float expected = (float)Math.Sqrt(Line2.GetPointLineDistanceSq(cent.x
                            , cent.z
                            , verts[indices[pCell+iWall]*3]
                            , verts[indices[pCell+iWall]*3+2]
                            , verts[indices[pCell+iNext]*3]
                            , verts[indices[pCell+iNext]*3+2]));
                    Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                }
            }
        }

        [TestMethod()]
        public void TestGetLinkPointDistanceSq2D() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                Vector3 cent = Polygon3.GetCentroid(verts[indices[pCell]*3]
                                                , verts[indices[pCell]*3+1]
                                                , verts[indices[pCell]*3+2]
                                                , verts[indices[pCell+1]*3]
                                                , verts[indices[pCell+1]*3+1]
                                                , verts[indices[pCell+1]*3+2]
                                                , verts[indices[pCell+2]*3]
                                                , verts[indices[pCell+2]*3+1]
                                                , verts[indices[pCell+2]*3+2]);            
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float actual = cells[iCell].GetLinkPointDistanceSq(cent.x, cent.z, iWall);
                    float midpointX 
                            = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                    float midpointZ 
                            = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                    float expected = Vector2Util.GetDistanceSq(cent.x, cent.z, midpointX, midpointZ);
                    Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                }
            }
        }

        [TestMethod()]
        public void TestGetLinkPointDistanceSq3D() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                Vector3 cent = Polygon3.GetCentroid(verts[indices[pCell]*3]
                                                , verts[indices[pCell]*3+1]
                                                , verts[indices[pCell]*3+2]
                                                , verts[indices[pCell+1]*3]
                                                , verts[indices[pCell+1]*3+1]
                                                , verts[indices[pCell+1]*3+2]
                                                , verts[indices[pCell+2]*3]
                                                , verts[indices[pCell+2]*3+1]
                                                , verts[indices[pCell+2]*3+2]);            
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float actual = cells[iCell].GetLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall);
                    float midpointX 
                            = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                    float midpointY 
                            = (verts[indices[pCell+iWall]*3+1] + verts[indices[pCell+iNext]*3+1]) / 2;
                    float midpointZ 
                            = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                    float expected = Vector3Util.GetDistanceSq(cent.x, cent.y, cent.z, midpointX, midpointY, midpointZ);
                    Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                }
            }
        }

        [TestMethod()]
        public void TestGetLinkPointDistance() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;    
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float midpointX 
                            = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                    float midpointY 
                            = (verts[indices[pCell+iWall]*3+1] + verts[indices[pCell+iNext]*3+1]) / 2;
                    float midpointZ 
                            = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                    for (int iOtherWall = 0; iOtherWall < 3; iOtherWall++)
                    {
                        int iOtherNext = (iOtherWall == 2 ? 0 : iOtherWall+1);
                        float otherMidpointX 
                                = (verts[indices[pCell+iOtherWall]*3] + verts[indices[pCell+iOtherNext]*3]) / 2;
                        float otherMidpointY 
                                = (verts[indices[pCell+iOtherWall]*3+1] + verts[indices[pCell+iOtherNext]*3+1]) / 2;
                        float otherMidpointZ 
                                = (verts[indices[pCell+iOtherWall]*3+2] + verts[indices[pCell+iOtherNext]*3+2]) / 2;
                        float actual = cells[iCell].GetLinkPointDistance(iWall, iOtherWall);
                        float expected = (float)Math.Sqrt(Vector3Util.GetDistanceSq(midpointX, midpointY, midpointZ
                                , otherMidpointX, otherMidpointY, otherMidpointZ));
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                    }
                }
            }
        }

        [TestMethod()]
        public void TestGetPlaneY() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                Vector3 cent = Polygon3.GetCentroid(verts[indices[pCell]*3]
                                                , verts[indices[pCell]*3+1]
                                                , verts[indices[pCell]*3+2]
                                                , verts[indices[pCell+1]*3]
                                                , verts[indices[pCell+1]*3+1]
                                                , verts[indices[pCell+1]*3+2]
                                                , verts[indices[pCell+2]*3]
                                                , verts[indices[pCell+2]*3+1]
                                                , verts[indices[pCell+2]*3+2]);
                float actual = cells[iCell].GetPlaneY(cent.x, cent.z);
                Assert.IsTrue(MathUtil.SloppyEquals(actual, cent.y, TOLERANCE_STD));
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float midpointX 
                            = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                    float midpointY 
                            = (verts[indices[pCell+iWall]*3+1] + verts[indices[pCell+iNext]*3+1]) / 2;
                    float midpointZ 
                            = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                    Assert.IsTrue(MathUtil.SloppyEquals(cells[iCell]
                                                .GetPlaneY(midpointX, midpointZ), midpointY, TOLERANCE_STD));
                }
            }
        }

        [TestMethod()]
        public void TestForceToColumn2D() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
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
                // First test offsets point from centroid.  Will always
                // still be within column.
                float offset = (float)mR.NextDouble() * mMesh.Offset;
                if (offset == 0)
                    // Can't let it be zero since we always need
                    // an offset from the centeroid.
                    offset = TOLERANCE_STD;
                float px = cent.x + offset * ((float)mR.NextDouble() > 0.5 ? 1 : -1);
                float pz = cent.z + offset * ((float)mR.NextDouble() > 0.5 ? 1 : -1);
                float offsetScale = 0.05f;
                Vector2 u = cells[iCell].ForceToColumn(px, pz, offsetScale);
                Assert.IsTrue(Triangle2.Contains(px, pz, ax, az, bx, bz, cx, cz));
                Assert.IsTrue(!(u.x == cent.x && u.y == cent.z));
                Assert.IsTrue(px == u.x);
                Assert.IsTrue(pz == u.y);
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    /*
                     * Second test moves point to outside the wall midpoint.
                     * Point is expected to be moved to slightly inside the wall.
                     * This is not a full test.  It does not validate the correct 
                     * (x, y) position along the wall the point is snapped to.
                     * It only validates that the point is internal to the column
                     * and within the expected tolerance of the correct wall.
                     */
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float midpointX 
                            = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                    float midpointZ 
                            = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                    Vector2 ln = Line2.GetNormalAB(verts[indices[pCell+iWall]*3]
                                                 , verts[indices[pCell+iWall]*3+2]
                                                 , verts[indices[pCell+iNext]*3]
                                                 , verts[indices[pCell+iNext]*3+2]);                
                    px = midpointX - ln.x;
                    pz = midpointZ - ln.y;
                    u = cells[iCell].ForceToColumn(px, pz, offsetScale);
                    Assert.IsTrue(!Triangle2.Contains(px, pz, ax, az, bx, bz, cx, cz));
                    Assert.IsTrue(!(u.x == cent.x && u.y == cent.z));
                    Assert.IsTrue(Triangle2.Contains(u.x, u.y, ax, az, bx, bz, cx, cz));
                    float maxDist = Vector2Util.GetDistanceSq(cent.x, cent.z, midpointX, midpointZ) * offsetScale;
                    float actualDist = Line2.GetPointSegmentDistanceSq(u.x, u.y
                                                    , verts[indices[pCell+iWall]*3], verts[indices[pCell+iWall]*3+2]
                                                    , verts[indices[pCell+iNext]*3], verts[indices[pCell+iNext]*3+2]);
                    Assert.IsTrue(actualDist <= maxDist);
                }
            }
        }

        [TestMethod()]
        public void TestIsInColumn() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                float ax = verts[indices[pCell]*3];
                float az = verts[indices[pCell]*3+2];
                float bx = verts[indices[pCell+1]*3];
                float bz = verts[indices[pCell+1]*3+2];
                float cx = verts[indices[pCell+2]*3];
                float cz = verts[indices[pCell+2]*3+2];
                // Test on vertices
                Assert.IsTrue(cells[iCell].IsInColumn(ax, az));
                Assert.IsTrue(cells[iCell].IsInColumn(bx, bz));
                Assert.IsTrue(cells[iCell].IsInColumn(cx, cz));
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float midpointX 
                            = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                    float midpointZ 
                            = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                    Vector2 ln = Line2.GetNormalAB(verts[indices[pCell+iWall]*3]
                                                 , verts[indices[pCell+iWall]*3+2]
                                                 , verts[indices[pCell+iNext]*3]
                                                 , verts[indices[pCell+iNext]*3+2]);    
                    ln = Vector2Util.ScaleTo(ln, 2*TOLERANCE_STD);
                    // Test slightly outside wall midpoint.
                    float px = midpointX - ln.x;
                    float pz = midpointZ - ln.y;
                    Assert.IsTrue(!Triangle2.Contains(px, pz, ax, az, bx, bz, cx, cz));
                    Assert.IsTrue(!cells[iCell].IsInColumn(px, pz));
                    // Test on midpoint.
                    Assert.IsTrue(Triangle2.Contains(midpointX, midpointZ, ax, az, bx, bz, cx, cz));
                    Assert.IsTrue(cells[iCell].IsInColumn(midpointX, midpointZ));
                    // Test slightly inside wall midpoint.
                    px = midpointX + ln.x;
                    pz = midpointZ + ln.y;
                    Assert.IsTrue(Triangle2.Contains(px, pz, ax, az, bx, bz, cx, cz));
                    Assert.IsTrue(cells[iCell].IsInColumn(px, pz));
                }
            }
        }

        [TestMethod()]
        public void TestGetLinkIndex() 
        {
            int[] linkCells = mMesh.GetLinkPolys();
            TriCell[] cells = TestUtil.GetAllCells(mMesh.GetVerts(), mMesh.GetIndices());
            TestUtil.LinkAllCells(cells);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iLink = 0; iLink < 3; iLink++)
                {
                    int iLinkedPoly = linkCells[iCell*3+iLink];
                    if (iLinkedPoly != -1)
                        Assert.IsTrue(cells[iCell].GetLinkIndex(cells[iLinkedPoly]) == iLink);                    
                }
            }
            float[] verts = {-1, 0, 0, 1, 0, 1, 0, 0, 1};
            TriCell np = new TriCell(verts, 0, 1, 2);
            Assert.IsTrue(cells[0].GetLinkIndex(np) == TriCell.NULL_INDEX);
        }
        
        [TestMethod()]
        public void TestLink()
        {
            // This test depends on GetLinkIndex() functioning properly.
            int[] linkCells = mMesh.GetLinkPolys();
            TriCell[] cells = TestUtil.GetAllCells(mMesh.GetVerts(), mMesh.GetIndices());
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iLink = 0; iLink < 3; iLink++)
                {
                    int iLinkedPoly = linkCells[iCell*3+iLink];
                    if (iLinkedPoly != -1 && iLinkedPoly < iLink)    
                    {
                        Assert.IsTrue(cells[iCell].Link(cells[iLinkedPoly], false) == iLink); // No reverse linking.
                        Assert.IsTrue(cells[iCell].GetLinkIndex(cells[iLinkedPoly]) == iLink);
                        Assert.IsTrue(cells[iLinkedPoly].GetLinkIndex(cells[iCell]) == TriCell.NULL_INDEX);
                    }
                }
            }
            cells = TestUtil.GetAllCells(mMesh.GetVerts(), mMesh.GetIndices());
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iLink = 0; iLink < 3; iLink++)
                {
                    int iLinkedPoly = linkCells[iCell*3+iLink];
                    if (iLinkedPoly != -1 && iLinkedPoly < iLink)    
                    {
                        Assert.IsTrue(cells[iCell].Link(cells[iLinkedPoly], true) == iLink);  // With reverse linking.
                        Assert.IsTrue(cells[iCell].GetLinkIndex(cells[iLinkedPoly]) == iLink);
                        int iReverseLink = -1;
                        for (int i = 0; i < 3; i++)
                        {
                            int il = linkCells[iLinkedPoly*3+i];
                            if (il == iCell)
                            {
                                iReverseLink = i;
                                break;
                            }    
                        }
                        Assert.IsTrue(iReverseLink != -1);  // This is a validity check of test Data.
                        Assert.IsTrue(cells[iLinkedPoly].GetLinkIndex(cells[iCell]) == iReverseLink);
                    }
                }
            }
        }
        
        [TestMethod()]
        public void TestIntersectsAABBCentered() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                float ax = verts[indices[pCell]*3];
                float az = verts[indices[pCell]*3+2];
                float bx = verts[indices[pCell+1]*3];
                float bz = verts[indices[pCell+1]*3+2];
                float cx = verts[indices[pCell+2]*3];
                float cz = verts[indices[pCell+2]*3+2];
                float minX = MathUtil.Min(ax, bx, cx);
                float maxX = MathUtil.Max(ax, bx, cx);
                float minZ = MathUtil.Min(az, bz, cz);
                float maxZ = MathUtil.Max(az, bz, cz);            
                // Polygon fully contained by AABB.
                Assert.IsTrue(cells[iCell].Intersects(minX, minZ, maxX, maxZ));
                Assert.IsTrue(cells[iCell].Intersects(minX - TOLERANCE_STD
                            , minZ - TOLERANCE_STD
                            , maxX + TOLERANCE_STD
                            , maxZ + TOLERANCE_STD));
                // AABB slightly smaller than Cell.
                Assert.IsTrue(cells[iCell].Intersects(minX + TOLERANCE_STD
                            , minZ + TOLERANCE_STD
                            , maxX - TOLERANCE_STD
                            , maxZ - TOLERANCE_STD));
                // Polygon fully Contains AABB
                Boolean firstPass = true;
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float midpointX 
                            = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                    float midpointZ 
                            = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                    if (firstPass)
                    {
                        minX = midpointX;
                        maxX = midpointX;
                        minZ = midpointZ;
                        maxZ = midpointZ;
                        firstPass = false;
                    }
                    else
                    {
                        minX = Math.Min(minX, midpointX);
                        maxX = Math.Max(maxX, midpointX);
                        minZ = Math.Min(minZ, midpointZ);
                        maxZ = Math.Max(maxZ, midpointZ);
                    }
                }
                Assert.IsTrue(cells[iCell].Intersects(minX, minZ, maxX, maxZ));
                Assert.IsTrue(cells[iCell].Intersects(minX - TOLERANCE_STD
                            , minZ - TOLERANCE_STD
                            , maxX + TOLERANCE_STD
                            , maxZ + TOLERANCE_STD));
                Assert.IsTrue(cells[iCell].Intersects(minX + TOLERANCE_STD
                            , minZ + TOLERANCE_STD
                            , maxX - TOLERANCE_STD
                            , maxZ - TOLERANCE_STD));
                
            }
        }

        [TestMethod()]
        public void TestIntersectsAABBOffset() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                float ax = verts[indices[pCell]*3];
                float az = verts[indices[pCell]*3+2];
                float bx = verts[indices[pCell+1]*3];
                float bz = verts[indices[pCell+1]*3+2];
                float cx = verts[indices[pCell+2]*3];
                float cz = verts[indices[pCell+2]*3+2];
                float polyMinX = MathUtil.Min(ax, bx, cx);
                float polyMaxX = MathUtil.Max(ax, bx, cx);
                float polyMinZ = MathUtil.Min(az, bz, cz);
                float polyMaxZ = MathUtil.Max(az, bz, cz);            
                // AABB shifted around the edges of Cell AABB
                // Not bothering with corner checks.
                Assert.IsFalse(cells[iCell].Intersects(polyMinX - 1, polyMinZ, polyMinX - TOLERANCE_STD, polyMaxZ));
                Assert.IsFalse(cells[iCell].Intersects(polyMaxX + TOLERANCE_STD, polyMinZ, polyMaxX + 1, polyMaxZ));
                Assert.IsFalse(cells[iCell].Intersects(polyMinX, polyMinZ - 1, polyMaxX, polyMinZ - TOLERANCE_STD));
                Assert.IsFalse(cells[iCell].Intersects(polyMinX, polyMaxZ + TOLERANCE_STD, polyMaxX, polyMaxZ + 1));
            }
        }
        
        [TestMethod()]
        public void TestIntersectsAABBNearVertices() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            /*
             * Shift distance is slightly longer than the length of the diagonal of a square
             * whose sides are 2*TOLERANCE_STD. This length is selected because, if applied
             * to an AABB centered over a vertex, it will shift the AABB completely
             * off the vertex.
             */
            float shiftDistance 
                = (float)Math.Sqrt(4*TOLERANCE_STD*TOLERANCE_STD+4*TOLERANCE_STD*TOLERANCE_STD) + TOLERANCE_STD;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                Vector3 cent = Polygon3.GetCentroid(verts[indices[pCell]*3]
                                                , verts[indices[pCell]*3+1]
                                                , verts[indices[pCell]*3+2]
                                                , verts[indices[pCell+1]*3]
                                                , verts[indices[pCell+1]*3+1]
                                                , verts[indices[pCell+1]*3+2]
                                                , verts[indices[pCell+2]*3]
                                                , verts[indices[pCell+2]*3+1]
                                                , verts[indices[pCell+2]*3+2]);
                for (int iVert = 0; iVert < 3; iVert++)
                {
                    float vx = verts[indices[pCell+iVert]*3];
                    float vz = verts[indices[pCell+iVert]*3+2];
                    // AABB centered on vertex.
                    float minX = vx - TOLERANCE_STD;
                    float minZ = vz - TOLERANCE_STD;
                    float maxX = vx + TOLERANCE_STD;
                    float maxZ = vz + TOLERANCE_STD;
                    Assert.IsTrue(cells[iCell].Intersects(minX, minZ, maxX, maxZ));
                    // Slightly outside vertex.
                    // Move AABB vertices away from centroid along centroid->vertex vector.
                    Vector2 dirCV = new Vector2(vx - cent.x, vz - cent.z);
                    Vector2Util.ScaleTo(dirCV, shiftDistance);
                    float sminX = minX + dirCV.x;
                    float sminZ = minZ + dirCV.y;
                    float smaxX = maxX + dirCV.x;
                    float smaxZ = maxZ + dirCV.y;
                    Assert.IsFalse(cells[iCell].Intersects(sminX, sminZ, smaxX, smaxZ));
                    // Slightly inside vertex.
                    sminX = minX - dirCV.x;
                    sminZ = minZ - dirCV.y;
                    smaxX = maxX - dirCV.x;
                    smaxZ = maxZ - dirCV.y;
                    Assert.IsTrue(cells[iCell].Intersects(sminX, sminZ, smaxX, smaxZ));
                }
            }
        }
        
        [TestMethod()]
        public void TestIntersectsAABBNearEdgeMidpoints() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            /*
             * Shift distance is slightly longer than the length of the diagonal of a square
             * whose sides are 2*TOLERANCE_STD. This length is selected because, if applied
             * to an AABB centered over a point, it will shift the AABB completely
             * off the point.
             */
            float shiftDistance 
                = 2*(float)Math.Sqrt(4*TOLERANCE_STD*TOLERANCE_STD+4*TOLERANCE_STD*TOLERANCE_STD) + TOLERANCE_STD;
            // shiftDistance = 0.1f;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                Vector3 cent = Polygon3.GetCentroid(verts[indices[pCell]*3]
                                                , verts[indices[pCell]*3+1]
                                                , verts[indices[pCell]*3+2]
                                                , verts[indices[pCell+1]*3]
                                                , verts[indices[pCell+1]*3+1]
                                                , verts[indices[pCell+1]*3+2]
                                                , verts[indices[pCell+2]*3]
                                                , verts[indices[pCell+2]*3+1]
                                                , verts[indices[pCell+2]*3+2]);
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float vx 
                            = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                    float vz 
                            = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                    // AABB centered on midpoint.
                    float minX = vx - TOLERANCE_STD;
                    float minZ = vz - TOLERANCE_STD;
                    float maxX = vx + TOLERANCE_STD;
                    float maxZ = vz + TOLERANCE_STD;
                    Assert.IsTrue(cells[iCell].Intersects(minX, minZ, maxX, maxZ));
                    // Slightly outside midpoint.
                    // Move AABB vertices away from centroid along centroid->midpoint vector.
                    Vector2 dirCV = new Vector2(vx - cent.x, vz - cent.z);
                    Vector2Util.ScaleTo(dirCV, shiftDistance);
                    float sminX = minX + dirCV.x;
                    float sminZ = minZ + dirCV.y;
                    float smaxX = maxX + dirCV.x;
                    float smaxZ = maxZ + dirCV.y;
                    Assert.IsFalse(cells[iCell].Intersects(sminX, sminZ, smaxX, smaxZ));
                    // Slightly inside midpoint.  (This test is a little redundant.  But why not, since
                    // it is easy enough to do.)
                    sminX = minX - dirCV.x;
                    sminZ = minZ - dirCV.y;
                    smaxX = maxX - dirCV.x;
                    smaxZ = maxZ - dirCV.y;
                    Assert.IsTrue(cells[iCell].Intersects(sminX, sminZ, smaxX, smaxZ));
                }
            }
        }
        
        [TestMethod()]
        public void TestGetClosestPolyArray() 
        {
            // This test depends on the proper functioning of GetPlaneY().
            int[] linkCells = mMesh.GetLinkPolys();
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TestUtil.LinkAllCells(cells);
            Vector3 surfacePoint = new Vector3();
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                /*
                 * Perform centroid test.
                 * Doing this to allow for better test of y-axis variance,
                 * which can't be done well on edge tests.
                 */
                Vector3 cent = Polygon3.GetCentroid(verts[indices[pCell]*3]
                                                    , verts[indices[pCell]*3+1]
                                                    , verts[indices[pCell]*3+2]
                                                    , verts[indices[pCell+1]*3]
                                                    , verts[indices[pCell+1]*3+1]
                                                    , verts[indices[pCell+1]*3+2]
                                                    , verts[indices[pCell+2]*3]
                                                    , verts[indices[pCell+2]*3+1]
                                                    , verts[indices[pCell+2]*3+2]);
                Assert.IsTrue(TriCell.GetClosestCell(cent.x
                                , cent.y + + mMesh.Offset * ((float)mR.NextDouble() > 0.5 ? 1 : -1)
                                , cent.z
                                , cells
                                , out surfacePoint) == cells[iCell]);
                Assert.IsTrue(surfacePoint.x == cent.x);
                Assert.IsTrue(MathUtil.SloppyEquals(surfacePoint.y, cent.y, TOLERANCE_STD));
                Assert.IsTrue(surfacePoint.z == cent.z);
                /*
                 * Perform edge tests.
                 * Only testing close to Cell plane to avoid the impact of slope
                 * on the result.
                 */
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float ax = verts[indices[pCell+iWall]*3];
                    float ay = verts[indices[pCell+iWall]*3+1];
                    float az = verts[indices[pCell+iWall]*3+2];
                    float bx = verts[indices[pCell+iNext]*3];
                    float by = verts[indices[pCell+iNext]*3+1];
                    float bz = verts[indices[pCell+iNext]*3+2];
                    float midpointX = (ax + bx) / 2;
                    float midpointY = (ay + by) / 2;
                    float midpointZ = (az + bz) / 2;
                    Vector2 ln = Line2.GetNormalAB(ax, az, bx, bz);
                    ln = Vector2Util.ScaleTo(ln, mMesh.Offset);
                    // Move inward from wall.
                    float px = midpointX + ln.x;
                    float py = midpointY;
                    float pz = midpointZ + ln.y;
                    Assert.IsTrue(TriCell.GetClosestCell(px, py, pz
                            , cells
                            , out surfacePoint) == cells[iCell]);
                    Assert.IsTrue(surfacePoint.x == px);
                    Assert.IsTrue(MathUtil.SloppyEquals(surfacePoint.y
                            , cells[iCell].GetPlaneY(px, pz)
                            , TOLERANCE_STD));
                    Assert.IsTrue(surfacePoint.z == pz);
                    // Move outward from wall.
                    px = midpointX - ln.x;
                    py = midpointY;
                    pz = midpointZ - ln.y;
                    TriCell actual = TriCell.GetClosestCell(px, py, pz
                            , cells
                            , out surfacePoint);
                    if (linkCells[pCell+iWall] == -1)
                    {
                        Assert.IsTrue(actual == cells[iCell]);
                        Vector2 ip = new Vector2();
                        Assert.IsTrue(
                                Line2.GetRelationship(ax, az
                                        , bx, bz
                                        , px, pz
                                        , cells[iCell].CentroidX, cells[iCell].CentroidZ
                                        , out ip) == LineRelType.SegmentsIntersect);
                        Assert.IsTrue(
                                MathUtil.SloppyEquals(surfacePoint.x,ip.x, TOLERANCE_STD));
                        Assert.IsTrue(
                                MathUtil.SloppyEquals(surfacePoint.y
                                        , cells[iCell].GetPlaneY(ip.x, ip.y)
                                        , TOLERANCE_STD));
                        Assert.IsTrue(
                                MathUtil.SloppyEquals(surfacePoint.z
                                        , ip.y
                                        , TOLERANCE_STD));    
                    }
                    else
                    {
                        Assert.IsTrue( actual == cells[linkCells[pCell+iWall]]);
                        Assert.IsTrue(surfacePoint.x == px);
                        Assert.IsTrue(MathUtil.SloppyEquals(surfacePoint.y
                                , cells[linkCells[pCell+iWall]].GetPlaneY(px, pz)
                                , TOLERANCE_STD));
                        Assert.IsTrue(surfacePoint.z == pz);                    
                    }
                }
            }
        }

        [TestMethod()]
        public void TestGetClosestCellList() 
        {
            // This test depends on the proper functioning of GetPlaneY().
            int[] linkCells = mMesh.GetLinkPolys();
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TestUtil.LinkAllCells(cells);
            
            List<TriCell> cellList = new List<TriCell>();
            foreach (TriCell cell in cells)
                cellList.Add(cell);
            Vector3 surfacePoint;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell*3;
                /*
                 * Perform centroid test.
                 * Doing this to allow for better test of y-axis variance,
                 * which can't be done well on edge tests.
                 */
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
                Assert.IsTrue(
                        TriCell.GetClosestCell(cent.x
                                , cent.y + + mMesh.Offset * ((float)mR.NextDouble() > 0.5 ? 1 : -1)
                                , cent.z
                                , cellList
                                , out surfacePoint) == cells[iCell]);
                Assert.IsTrue(
                        surfacePoint.x == cent.x);
                Assert.IsTrue(
                        MathUtil.SloppyEquals(surfacePoint.y, cent.y, TOLERANCE_STD));
                Assert.IsTrue(
                        surfacePoint.z == cent.z);
                /*
                 * Perform edge tests.
                 * Only testing close to Cell plane to avoid the impact of slope
                 * on the result.
                 */
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float ax = verts[indices[pCell+iWall]*3];
                    float ay = verts[indices[pCell+iWall]*3+1];
                    float az = verts[indices[pCell+iWall]*3+2];
                    float bx = verts[indices[pCell+iNext]*3];
                    float by = verts[indices[pCell+iNext]*3+1];
                    float bz = verts[indices[pCell+iNext]*3+2];
                    float midpointX = (ax + bx) / 2;
                    float midpointY = (ay + by) / 2;
                    float midpointZ = (az + bz) / 2;
                    Vector2 ln = Line2.GetNormalAB(ax, az, bx, bz);
                    ln = Vector2Util.ScaleTo(ln, mMesh.Offset);
                    // Move inward from wall.
                    float px = midpointX + ln.x;
                    float py = midpointY;
                    float pz = midpointZ + ln.y;
                    Assert.IsTrue(TriCell.GetClosestCell(px, py, pz
                            , cellList
                            , out surfacePoint) == cells[iCell]);
                    Assert.IsTrue(surfacePoint.x == px);
                    Assert.IsTrue(MathUtil.SloppyEquals(surfacePoint.y
                            , cells[iCell].GetPlaneY(px, pz)
                            , TOLERANCE_STD));
                    Assert.IsTrue(surfacePoint.z == pz);
                    // Move outward from wall.
                    px = midpointX - ln.x;
                    py = midpointY;
                    pz = midpointZ - ln.y;
                    TriCell actual = TriCell.GetClosestCell(px, py, pz
                            , cellList
                            , out surfacePoint);
                    if (linkCells[pCell+iWall] == -1)
                    {
                        Assert.IsTrue(actual == cells[iCell]);
                        Vector2 ip = new Vector2();
                        Assert.IsTrue(
                                Line2.GetRelationship(ax, az
                                        , bx, bz
                                        , px, pz
                                        , cells[iCell].CentroidX, cells[iCell].CentroidZ
                                        , out ip) == LineRelType.SegmentsIntersect);
                        Assert.IsTrue(
                                MathUtil.SloppyEquals(surfacePoint.x,ip.x, TOLERANCE_STD));
                        Assert.IsTrue(
                                MathUtil.SloppyEquals(surfacePoint.y
                                        , cells[iCell].GetPlaneY(ip.x, ip.y)
                                        , TOLERANCE_STD));
                        Assert.IsTrue(
                                MathUtil.SloppyEquals(surfacePoint.z
                                        , ip.y
                                        , TOLERANCE_STD));    
                    }
                    else
                    {
                        Assert.IsTrue( actual == cells[linkCells[pCell+iWall]]);
                        Assert.IsTrue(surfacePoint.x == px);
                        Assert.IsTrue(MathUtil.SloppyEquals(surfacePoint.y
                                , cells[linkCells[pCell+iWall]].GetPlaneY(px, pz)
                                , TOLERANCE_STD));
                        Assert.IsTrue(surfacePoint.z == pz);                    
                    }
                }
            }
        }

        [TestMethod()]
        public void TestGetPathRelationshipInternalSegments() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            Vector2 ip = new Vector2();
            TriCell ep;
            int ew;
            for (int iCell = 0; iCell < cells.Length; iCell++)
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
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float ax = verts[indices[pCell+iWall]*3];
                    float az = verts[indices[pCell+iWall]*3+2];
                    float bx = verts[indices[pCell+iNext]*3];
                    float bz = verts[indices[pCell+iNext]*3+2];
                    float midpointX = (ax + bx) / 2;
                    float midpointZ = (az + bz) / 2;
                    // All line segments begin or end on a wall or vertex.
                    // Edge case: Centroid to wall midpoint.
                    PathRelType relType = cells[iCell].GetPathRelationship(cent.x, cent.z
                            , midpointX, midpointZ
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    // Edge case: Centroid to vertex.
                    relType = cells[iCell].GetPathRelationship(cent.x, cent.z
                            , ax, az
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    // Edge case: Wall midpoint to centroid.
                    relType = cells[iCell].GetPathRelationship(midpointX, midpointZ
                            , cent.x, cent.z
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    // Edge case: Vertex to centroid.
                    relType = cells[iCell].GetPathRelationship(ax, az
                            , cent.x, cent.z
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    // All line segments are shifted off the walls and vertices.
                    // Centroid to just inside wall midpoint.
                    Vector2 offset = new Vector2(cent.x - midpointX, cent.z - midpointZ);
                    Vector2Util.ScaleTo(offset, mMesh.Offset);
                    relType = cells[iCell].GetPathRelationship(cent.x, cent.z
                            , midpointX + offset.x, midpointZ + offset.y
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    // Edge case: Just inside wall midpoint to centroid.
                    relType = cells[iCell].GetPathRelationship(midpointX + offset.x, midpointZ + offset.y
                            , cent.x, cent.z
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    offset = new Vector2(cent.x - ax, cent.z - az);
                    Vector2Util.ScaleTo(offset, mMesh.Offset);
                    // Centroid to just inside vertex.
                    relType = cells[iCell].GetPathRelationship(cent.x, cent.z
                            , ax + offset.x, az + offset.y
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    // Edge case: Just inside vertex to centroid.
                    relType = cells[iCell].GetPathRelationship(ax + offset.x, az + offset.y
                            , cent.x, cent.z
                            , out ep
                            , out ew
                            , out ip);
                }
            }
        }
        
        [TestMethod()]
        public void TestGetPathRelationshipBeginOutEndIn() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            Vector2 ip;
            TriCell ep;
            int ew;
            for (int iCell = 0; iCell < cells.Length; iCell++)
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
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float ax = verts[indices[pCell+iWall]*3];
                    float az = verts[indices[pCell+iWall]*3+2];
                    float bx = verts[indices[pCell+iNext]*3];
                    float bz = verts[indices[pCell+iNext]*3+2];
                    float midpointX = (ax + bx) / 2;
                    float midpointZ = (az + bz) / 2;
                    // Outside to wall midpoint.
                    Vector2 offset = new Vector2(cent.x - midpointX, cent.z - midpointZ);
                    Vector2Util.ScaleTo(offset, mMesh.Offset);
                    PathRelType relType = cells[iCell].GetPathRelationship(midpointX - offset.x, midpointZ - offset.y
                            , midpointX, midpointZ
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    // Outside to just inside wall midpoint.
                    relType = cells[iCell].GetPathRelationship(midpointX - offset.x, midpointZ - offset.y
                            , midpointX + offset.x, midpointZ + offset.y
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    offset = new Vector2(cent.x - ax, cent.z - az);
                    Vector2Util.ScaleTo(offset, mMesh.Offset);
                    // Outside to vertex
                    relType = cells[iCell].GetPathRelationship(ax - offset.x, az - offset.y
                            , ax, az
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                    // Edge case: Just inside vertex to centroid.
                    relType = cells[iCell].GetPathRelationship(ax - offset.x, az - offset.y
                            , ax + offset.x, az + offset.y
                            , out ep
                            , out ew
                            , out ip);
                    Assert.IsTrue(relType == PathRelType.EndingCell);
                }
            }
        }

        [TestMethod()]
        public void TestGetPathRelationshipBeginInEndOut() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TestUtil.LinkAllCells(cells);
            int[] linkCells = mMesh.GetLinkPolys();
            Vector2 ip;
            TriCell ep;
            int ew;
            for (int iCell = 0; iCell < cells.Length; iCell++)
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
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float ax = verts[indices[pCell+iWall]*3];
                    float az = verts[indices[pCell+iWall]*3+2];
                    float bx = verts[indices[pCell+iNext]*3];
                    float bz = verts[indices[pCell+iNext]*3+2];
                    float midpointX = (ax + bx) / 2;
                    float midpointZ = (az + bz) / 2;
                    // Outside to wall midpoint.
                    Vector2 offset = new Vector2(cent.x - midpointX, cent.z - midpointZ);
                    Vector2Util.ScaleTo(offset, mMesh.Offset);
                    PathRelType relType = cells[iCell].GetPathRelationship(midpointX, midpointZ
                            , midpointX - offset.x, midpointZ - offset.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.ExitingCell);
                    int iNextPoly = linkCells[pCell+iWall];
                    if (iNextPoly == -1)
                        Assert.IsTrue(ep == null);
                    else
                        Assert.IsTrue(ep == cells[iNextPoly]);
                    Assert.IsTrue(ew == iWall);
                    Assert.IsTrue(Vector2Util.SloppyEquals(ip, midpointX, midpointZ, TOLERANCE_STD));
                    // Outside to just inside wall midpoint.
                    relType = cells[iCell].GetPathRelationship(midpointX + offset.x, midpointZ + offset.y
                            , midpointX - offset.x, midpointZ - offset.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.ExitingCell);
                    iNextPoly = linkCells[pCell+iWall];
                    if (iNextPoly == -1)
                        Assert.IsTrue(ep == null);
                    else
                        Assert.IsTrue(ep == cells[iNextPoly]);
                    Assert.IsTrue(ew == iWall);
                    Assert.IsTrue(Vector2Util.SloppyEquals(ip, midpointX, midpointZ, TOLERANCE_STD));
                    offset = new Vector2(cent.x - ax, cent.z - az);
                    Vector2Util.ScaleTo(offset, mMesh.Offset);
                    // Outside to vertex
                    relType = cells[iCell].GetPathRelationship(ax, az
                            , ax - offset.x, az - offset.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.ExitingCell);
                    // Not testing exit wall information since which wall is chosen is undefined
                    // for vertices.
                    Assert.IsTrue(Vector2Util.SloppyEquals(ip, ax, az, TOLERANCE_STD));
                    // Edge case: Just inside vertex to centroid.               
                    relType = cells[iCell].GetPathRelationship(ax + offset.x, az + offset.y
                            , ax - offset.x, az - offset.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.ExitingCell);
                    // Not testing exit wall information since which wall is chosen is undefined
                    // for vertices.
                    Assert.IsTrue(Vector2Util.SloppyEquals(ip, ax, az, TOLERANCE_STD));
                }
            }
        }
        
        [TestMethod()]
        public void TestGetPathRelationshipCrossing() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TestUtil.LinkAllCells(cells);
            int[] linkCells = mMesh.GetLinkPolys();
            Vector2 ip;
            TriCell ep;
            int ew;
            for (int iCell = 0; iCell < cells.Length; iCell++)
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
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    int iNextNext = (iNext == 2 ? 0 : iNext+1);
                    // Current wall.
                    float wallax = verts[indices[pCell+iWall]*3];
                    float wallaz = verts[indices[pCell+iWall]*3+2];
                    float wallbx = verts[indices[pCell+iNext]*3];
                    float wallbz = verts[indices[pCell+iNext]*3+2];
                    float wallmpx = (wallax + wallbx) / 2;
                    float wallmpz = (wallaz + wallbz) / 2;
                    // Next wall.
                    float wallnax = verts[indices[pCell+iNext]*3];
                    float wallnaz = verts[indices[pCell+iNext]*3+2];
                    float wallnbx = verts[indices[pCell+iNextNext]*3];
                    float wallnbz = verts[indices[pCell+iNextNext]*3+2];
                    float wallnmpx = (wallnax + wallnbx) / 2;
                    float wallnmpz = (wallnaz + wallnbz) / 2;
                    // Outside next wall midpoint to outside this wall midpoint.
                    Vector2 offset = new Vector2(wallnmpx - wallmpx, wallnmpz - wallmpz);
                    Vector2Util.ScaleTo(offset, mMesh.Offset);
                    PathRelType relType = cells[iCell].GetPathRelationship(wallnmpx + offset.x
                            , wallnmpz + offset.y
                            , wallmpx - offset.x
                            , wallmpz - offset.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.ExitingCell);
                    int iNextPoly = linkCells[pCell+iWall];
                    if (iNextPoly == -1)
                        Assert.IsTrue(ep == null);
                    else
                        Assert.IsTrue(ep == cells[iNextPoly]);
                    Assert.IsTrue(ew == iWall);
                    Assert.IsTrue(Vector2Util.SloppyEquals(ip, wallmpx, wallmpz, TOLERANCE_STD));
                    // Outside Cell through centroid to outside vertex.
                    offset = new Vector2(wallax - cent.x, wallaz - cent.z);
                    Vector2 largeOffset = Vector2Util.ScaleTo(new Vector2(offset.x, offset.y), 20);
                    Vector2Util.ScaleTo(offset, mMesh.Offset);
                    relType = cells[iCell].GetPathRelationship(cent.x - largeOffset.x, cent.z - largeOffset.y
                            , wallax + offset.x, wallaz + offset.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.ExitingCell);
                    Assert.IsTrue(Vector2Util.SloppyEquals(ip, wallax, wallaz, TOLERANCE_STD));
                }
            }
        }
        
        [TestMethod()]
        public void TestGetPathRelationshipNoCross()
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            Vector2 ip;
            TriCell ep;
            int ew;
            for (int iCell = 0; iCell < cells.Length; iCell++)
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
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    int iNext = (iWall == 2 ? 0 : iWall+1);
                    float ax = verts[indices[pCell+iWall]*3];
                    float az = verts[indices[pCell+iWall]*3+2];
                    float bx = verts[indices[pCell+iNext]*3];
                    float bz = verts[indices[pCell+iNext]*3+2];
                    float midpointX = (ax + bx) / 2;
                    float midpointZ = (az + bz) / 2;
                    // Outside and away from wall midpoint.
                    Vector2 offset = new Vector2(cent.x - midpointX, cent.z - midpointZ);
                    Vector2Util.ScaleTo(offset, mMesh.Offset);
                    PathRelType relType = cells[iCell].GetPathRelationship(midpointX - offset.x
                            , midpointZ - offset.y
                            , midpointX - 2* offset.x
                            , midpointZ - 2* offset.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.NoRelationship);
                    // Outside and away from vertex.
                    relType = cells[iCell].GetPathRelationship(ax - offset.x, az - offset.y
                            , ax - 2* offset.x, az - 2* offset.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.NoRelationship);
                    // Shift wall away from Cell so there is no crossing.
                    Vector2 ln = Line2.GetNormalAB(ax, az, bx, bz);
                    Vector2Util.ScaleTo(ln, 2*TOLERANCE_STD);
                    relType = cells[iCell].GetPathRelationship(ax - ln.x, az - ln.y
                            , bx - ln.x, bz - ln.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.NoRelationship);
                    // Swap direction (BA) and test again.
                    relType = cells[iCell].GetPathRelationship(bx - ln.x, bz - ln.y
                            , ax - ln.x, az - ln.y
                            , out ep
                            , out ew
                            , out ip
                            );
                    Assert.IsTrue(relType == PathRelType.NoRelationship);
                }
            }
        }
        
        [TestMethod()]
        public void TestGetSafePoint()
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
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
                Vector3 cent = Polygon3.GetCentroid(
                                                ax, ay, az
                                                , bx, by, bz
                                                , cx, cy, cz);
                // Test: Point is already safe.
                float offsetScale = 0.05f;
                Vector2 u = cells[iCell].GetSafePoint(cent.x, cent.z, offsetScale);
                Assert.IsTrue(u.x == cent.x && u.y == cent.z);
                // Test: Point is on vertex. (Not safe.)
                for (int i = 0; i < 3; i++)
                {
                    float vx = verts[indices[pCell+i]*3];
                    float vz = verts[indices[pCell+i]*3+2];
                    int iNext = (i + 1 >= 3 ? 0 : i + 1);
                    int iNextNext = (iNext + 1 >= 3 ? 0 : iNext + 1);
                    u = cells[iCell].GetSafePoint(vx, vz, offsetScale);
                    /*
                     * The following validations are performed:
                     * - Is the point still in the Cell?
                     * - Is the point still within an acceptable distance of the vertex? (Not too far.)
                     * - Has the point been moved an acceptable distance from the vertex? (Not too close.)
                     * NOTE: Not really testing that the point was moved along the line toward the centroid.
                     * But this is a good enough Set of tests.
                     */
                    Assert.IsFalse(Vector2Util.SloppyEquals(u, vx, vz, TOLERANCE_STD));
                    Assert.IsTrue(Triangle2.Contains(u.x, u.y
                            , vx, vz
                            , verts[indices[pCell+iNext]*3], verts[indices[pCell+iNext]*3+2]
                            , verts[indices[pCell+iNextNext]*3], verts[indices[pCell+iNextNext]*3+2]));
                    float maxDist = Vector2Util.GetDistanceSq(vx, vz, cent.x, cent.z) * offsetScale;
                    float actualDist = Vector2Util.GetDistanceSq(vx, vz, u.x, u.y);
                    Assert.IsTrue(actualDist < maxDist);                
                }
            }
        }

        //[TestMethod()]
        //[DeploymentItem("cai-nav-u3d.dll")]
        //public void getSignedDistanceTest()
        //{

        //}

        //[TestMethod()]
        //[DeploymentItem("cai-nav-u3d.dll")]
        //public void getRelationshipTest()
        //{

        //}
    }
}
