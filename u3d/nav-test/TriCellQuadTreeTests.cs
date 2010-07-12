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

namespace org.critterai.nav
{
    /// <summary>
    /// Summary description for TriCellQuadTreeTests
    /// </summary>
    [TestClass]
    public sealed class TriCellQuadTreeTests
    {

        /*
         * Design Notes:
         * 
         * So far, the tests are purely functional.  No validation is being performed 
         * that internal storage is correct.  Because of this, tests often just compare
         * the results from the tree against results from a full scan of all Cell.
         * 
         * Various tests in the suite require the proper functioning of NavPoly.
         * If there is a failure in this suite and in NavPoly, check and fix NavPoly first.
         */

        private const float TOLERANCE_STD = MathUtil.TOLERANCE_STD;

        private TestContext testContextInstance;

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
        public void SetUp()
        {
            mMesh = new CorridorMesh();
        }

        [TestMethod]
        public void TestFieldBounds() 
        {
            TriCellQuadTree tree = new TriCellQuadTree(-8, 2, 5, 7, 9);
            Assert.IsTrue(tree.BoundsMinX == -8);
            Assert.IsTrue(tree.BoundsMinZ == 2);
            Assert.IsTrue(tree.BoundsMaxX == 5);
            Assert.IsTrue(tree.BoundsMaxZ == 7);
        }
        
        [TestMethod]
        public void TestFieldMaxDepth() 
        {
            TriCellQuadTree tree = new TriCellQuadTree(-8, 2, 5, 7, 9);
            Assert.IsTrue(tree.MaxDepth == 9);
        }

        [TestMethod]
        public void TestAddInBounds() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            float[] meshBounds = TestUtil.GetVertBounds(verts);
            TriCellQuadTree tree = new TriCellQuadTree(meshBounds[0]
                                                 , meshBounds[2]
                                                 , meshBounds[3]
                                                 , meshBounds[5]
                                                 , 10);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                Assert.IsTrue(tree.Add(cells[iCell]));
            }
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                Assert.IsFalse(tree.Add(cells[iCell]));
            }
        }

        [TestMethod]
        public void TestAddOutOfBounds() 
        {
            // Only performing basic tests.  Many more are possible.
            // Don't reduce any of the bound deltas to less than 3.
            float minX = -8;
            float minZ = 2;
            float maxX = 5;
            float maxZ = 7;
            TriCellQuadTree tree = new TriCellQuadTree(minX, minZ, maxX, maxZ, 10);
            // Corner outside checks.
            // Polygon below tree's minX/minZ bounds.
            float[] bverts = new float[9];
            bverts[0] = minX - TOLERANCE_STD;
            bverts[1] = 5;
            bverts[2] = minZ - TOLERANCE_STD;
            bverts[3] = bverts[0] - 1;
            bverts[4] = 5;
            bverts[5] = bverts[2] - 1;
            bverts[6] = bverts[0] - 0.5f;
            bverts[7] = 5;
            bverts[8] = bverts[2] - 0.5f;
            TriCell cell = new TriCell(bverts, 0, 1, 2);
            Assert.IsFalse(tree.Add(cell));
            // Polygon above tree's maxX/maxZ bounds.
            bverts[0] = maxX + TOLERANCE_STD;
            bverts[2] = maxZ + TOLERANCE_STD;
            bverts[3] = bverts[0] + 1;
            bverts[5] = bverts[2] + 1;
            bverts[6] = bverts[0] + 0.5f;
            bverts[8] = bverts[2] + 0.5f;
            cell = new TriCell(bverts, 0, 1, 2);
            Assert.IsFalse(tree.Add(cell));
            // Polygon below minX, above maxZ tree bounds.
            bverts[0] = minX - TOLERANCE_STD;
            bverts[2] = maxZ + TOLERANCE_STD;
            bverts[3] = bverts[0] - 1;
            bverts[5] = bverts[2] + 1;
            bverts[6] = bverts[0] - 0.5f;
            bverts[8] = bverts[2] + 0.5f;
            cell = new TriCell(bverts, 0, 1, 2);
            Assert.IsFalse(tree.Add(cell));
            // Polygon above maxX, below minZ tree bounds.
            bverts[0] = maxX + TOLERANCE_STD;
            bverts[2] = minZ - TOLERANCE_STD;
            bverts[3] = bverts[0] + 1;
            bverts[5] = bverts[2] - 1;
            bverts[6] = bverts[0] + 0.5f;
            bverts[8] = bverts[2] - 0.5f;
            cell = new TriCell(bverts, 0, 1, 2);
            Assert.IsFalse(tree.Add(cell));
            // Outside wall checks
            // Polygon below minX
            bverts[0] = minX - TOLERANCE_STD;
            bverts[2] = minZ + 0.5f;
            bverts[3] = bverts[0] - 1;
            bverts[5] = bverts[2] + 1;
            bverts[6] = bverts[0] - 0.5f;
            bverts[8] = bverts[2] + 0.5f;
            cell = new TriCell(bverts, 0, 1, 2);
            Assert.IsFalse(tree.Add(cell));
            // Polygon above maxX
            bverts[0] = maxX + TOLERANCE_STD;
            bverts[2] = minZ + 0.5f;
            bverts[3] = bverts[0] + 1;
            bverts[5] = bverts[2] + 1;
            bverts[6] = bverts[0] + 0.5f;
            bverts[8] = bverts[2] + 0.5f;
            cell = new TriCell(bverts, 0, 1, 2);
            Assert.IsFalse(tree.Add(cell));
            // Polygon below minY
            bverts[0] = minX + 0.5f;
            bverts[2] = minZ - TOLERANCE_STD;
            bverts[3] = bverts[0] + 1;
            bverts[5] = bverts[2] - 1;
            bverts[6] = bverts[0] + 0.5f;
            bverts[8] = bverts[2] - 0.5f;
            cell = new TriCell(bverts, 0, 1, 2);
            Assert.IsFalse(tree.Add(cell));
            // Polygon below minY
            bverts[0] = minX + 0.5f;
            bverts[2] = maxZ + TOLERANCE_STD;
            bverts[3] = bverts[0] + 1;
            bverts[5] = bverts[2] + 1;
            bverts[6] = bverts[0] + 0.5f;
            bverts[8] = bverts[2] + 0.5f;
            cell = new TriCell(bverts, 0, 1, 2);
            Assert.IsFalse(tree.Add(cell));
            // Overlapping
            bverts[0] = minX - TOLERANCE_STD;
            bverts[2] = minZ + 0.5f;
            bverts[3] = bverts[0] + 1;
            bverts[5] = bverts[2] + 1;
            bverts[6] = bverts[0] + 0.5f;
            bverts[8] = bverts[2] + 0.5f;
            cell = new TriCell(bverts, 0, 1, 2);
            Assert.IsFalse(tree.Add(cell));
        }
        
        [TestMethod]
        public void TestGetCells() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TriCellQuadTree tree = LoadTree(verts, cells);
            List<TriCell> list = new List<TriCell>();
            List<TriCell> cellList = tree.GetCells(list);
            Assert.IsTrue(cellList == list);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                Assert.IsTrue(cellList.Contains(cells[iCell]));
            }
            Assert.IsTrue(cells.Length == cellList.Count);
        }

        [TestMethod]
        public void TestGetCellsInColumn() 
        {
            /*
             * Not trying to test all edge cases since most of the work
             * in this operation depends on the proper functioning of NavPoly, and NavPoly
             * testing is expected to deal with edge cases.
             */
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TriCellQuadTree tree = LoadTree(verts, cells);
            // Full encompass.
            List<TriCell> list = new List<TriCell>();
            List<TriCell> cellList = tree.GetCellsInColumn(tree.BoundsMinX, tree.BoundsMinZ
                    , tree.BoundsMaxX, tree.BoundsMaxZ
                    , list);
            Assert.IsTrue(cellList == list);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                Assert.IsTrue(cellList.Contains(cells[iCell]));
            }
            Assert.IsTrue(cells.Length == cellList.Count);
            // Internal AABB
            float offsetX = 0.22f * (tree.BoundsMaxX - tree.BoundsMinX);  // Factor is arbitrary.
            float offsetZ = 0.22f * (tree.BoundsMaxZ - tree.BoundsMinZ);
            float minx = tree.BoundsMinX + offsetX;
            float minz = tree.BoundsMinZ + offsetZ;
            float maxx = tree.BoundsMaxX - offsetX;
            float maxz = tree.BoundsMaxZ - offsetZ;
            cellList = tree.GetCellsInColumn(minx, minz, maxx, maxz, list);    
            Assert.IsTrue(IsCellListGood(minx, minz, maxx, maxz, cells, cellList));
            // Offset AABB's
            for (int i = 0; i < 3; i++)
            {
                int xFactor = 0;
                int zFactor = 0;
                switch (i)
                {
                case 0: xFactor = 1; zFactor = 1; break;
                case 1: xFactor = -1; zFactor = 1; break;
                case 2: xFactor = -1; zFactor = -1; break;
                case 3: xFactor = 1; zFactor = -1; break;
                }
                minx = tree.BoundsMinX + offsetX * xFactor;
                minz = tree.BoundsMinZ + offsetZ * zFactor;
                maxx = tree.BoundsMaxX + offsetX * xFactor;
                maxz = tree.BoundsMaxZ + offsetZ * zFactor;
                cellList = tree.GetCellsInColumn(minx, minz, maxx, maxz, list);    
                Assert.IsTrue(IsCellListGood(minx, minz, maxx, maxz, cells, cellList));
            }
        }

        [TestMethod]
        public void TestGetCellsForPoint() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TriCellQuadTree tree = LoadTree(verts, cells);
            List<TriCell> allPolys = new List<TriCell>();
            foreach (TriCell cell in cells)
            {
                allPolys.Add(cell);
            }
            // Creating a duplicate mesh translated up the y-axis by 1.5 units.
            // This will improve testing by ensuring that all results return more
            // than a single Cell.
            float[] offsetVerts = mMesh.GetVerts();
            for (int pVert = 1; pVert < offsetVerts.Length; pVert += 3)
            {
                offsetVerts[pVert] += 1.5f;
            }
            TriCell[] offsetPolys = TestUtil.GetAllCells(offsetVerts, indices);
            for (int iCell = 0; iCell < offsetPolys.Length; iCell++)
            {
                tree.Add(offsetPolys[iCell]);
                allPolys.Add(offsetPolys[iCell]);
            }
            List<TriCell> list = new List<TriCell>();
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
                // Test by centroid.  (Internal to Cell.)
                List<TriCell> cellList = tree.GetCellsForPoint(cent.x, cent.z, list);
                Assert.IsTrue(cellList == list);
                Assert.IsTrue(cellList.Count == 2);
                // Test by vertex.
                Assert.IsTrue(IsCellListGood(cent.x, cent.z, allPolys, cellList));
                cellList = tree.GetCellsForPoint(ax, az, list);
                Assert.IsTrue(IsCellListGood(ax, az, allPolys, cellList));
                cellList = tree.GetCellsForPoint(bx, bz, list);
                Assert.IsTrue(IsCellListGood(bx, bz, allPolys, cellList));
                cellList = tree.GetCellsForPoint(cx, cz, list);
                Assert.IsTrue(IsCellListGood(cx, cz, allPolys, cellList));
                // Test by wall midpoint. (Only testing the AB wall.)
                cellList = tree.GetCellsForPoint((ax + bx) / 2, (az + bz) / 2, list);
                Assert.IsTrue(IsCellListGood((ax + bx) / 2, (az + bz) / 2, allPolys, cellList));
            }
        }

        [TestMethod]
        public void TestGetClosestCell() 
        {
            float[] verts = mMesh.GetVerts();
            int[] indices = mMesh.GetIndices();
            TriCell[] cells = TestUtil.GetAllCells(verts, indices);
            TriCellQuadTree tree = LoadTree(verts, cells);
            List<TriCell> allPolys = new List<TriCell>();
            int[] linkCells = mMesh.GetLinkPolys();
            foreach (TriCell cell in cells)
            {
                allPolys.Add(cell);
            }
            // Creating a duplicate mesh translated up the y-axis by 1.5 units.
            float[] offsetVerts = mMesh.GetVerts();
            float yOffset = 1.5f;
            for (int pVert = 1; pVert < offsetVerts.Length; pVert += 3)
            {
                offsetVerts[pVert] += yOffset;
            }
            TriCell[] offsetPolys = TestUtil.GetAllCells(offsetVerts, indices);
            for (int iCell = 0; iCell < offsetPolys.Length; iCell++)
            {
                tree.Add(offsetPolys[iCell]);
                allPolys.Add(offsetPolys[iCell]);
            }
            Vector3 v = new Vector3();
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
                Boolean ttype = false;
                // Loop twice.  Once for exhaustive search.  The next for in-column search.
                for (int i = 0; i < 2; i++)
                {
                    // Exact on centroid. (Layer 1)
                    TriCell closestPoly = tree.GetClosestCell(cent.x, cent.y, cent.z, ttype, out v);
                    Assert.IsTrue(closestPoly == cells[iCell]);
                    Assert.IsTrue(Vector3Util.SloppyEquals(v, cent, TOLERANCE_STD));
                    // Slightly above centroid. (Layer 1)
                    closestPoly = tree.GetClosestCell(cent.x, cent.y + 4*TOLERANCE_STD, cent.z, ttype, out v);
                    Assert.IsTrue(closestPoly == cells[iCell]);
                    Assert.IsTrue(Vector3Util.SloppyEquals(v, cent, TOLERANCE_STD));
                    // Nearer centroid of layer 2.
                    closestPoly = tree.GetClosestCell(cent.x, cent.y + yOffset - 4*TOLERANCE_STD, cent.z, ttype, out v);
                    Assert.IsTrue(closestPoly == offsetPolys[iCell]);
                    Assert.IsTrue(Vector3Util.SloppyEquals(v, cent.x, cent.y + yOffset, cent.z, TOLERANCE_STD));
                    // The next Set of tests just make sure one of the valid possibilities is returned,
                    // since the selection Process is arbitrary.
                    float mpx = (ax + bx) / 2;
                    float mpy = (ay + by) / 2;
                    float mpz = (az + bz) / 2;
                    closestPoly = tree.GetClosestCell(mpx, mpy, mpz, ttype, out v);
                    if (linkCells[pCell] == -1)
                        Assert.IsTrue(closestPoly == cells[iCell]);
                    else
                        Assert.IsTrue(closestPoly == cells[iCell] 
                                           || closestPoly == cells[linkCells[pCell]]);
                    Assert.IsTrue(Vector3Util.SloppyEquals(v, mpx, mpy, mpz, TOLERANCE_STD));
                    // Offset to level 2.
                    closestPoly = tree.GetClosestCell(mpx, mpy + yOffset - 4*TOLERANCE_STD, mpz, ttype, out v);
                    if (linkCells[pCell] == -1)
                        Assert.IsTrue(closestPoly == offsetPolys[iCell]);
                    else
                        Assert.IsTrue(closestPoly == offsetPolys[iCell] 
                                           || closestPoly == offsetPolys[linkCells[pCell]]);
                    ttype = true;
                }
            }
            // Test outside of mesh.
            float[] va = mMesh.MinVertex;
            // Restrict to column.
            TriCell cp = tree.GetClosestCell(va[0] - TOLERANCE_STD
                    , va[1] - TOLERANCE_STD
                    , va[2] - TOLERANCE_STD
                    , true
                    , out v);
            Assert.IsTrue(cp == null);
            // Exhaustive search.
            cp = tree.GetClosestCell(va[0] - TOLERANCE_STD
                    , va[1] - TOLERANCE_STD
                    , va[2] - TOLERANCE_STD
                    , false
                    , out v);
            int[] minCells = mMesh.GetMinVertexPolys();
            Boolean found = false;
            for (int i = 0; i < minCells.Length; i++)
            {
                if (cp == cells[minCells[i]])
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found);
        }
        
        ///
        ///Scans for Cell that intersect the the AABB and makes
        ///sure the generated list matches the provided list.
        ///
        private Boolean IsCellListGood(float minx, float minz
                , float maxx, float maxz
                , TriCell[] cells
                , List<TriCell> cellList)
        {
            List<TriCell> expectedPolys = new List<TriCell>();
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                if (cells[iCell].Intersects(minx, minz, maxx, maxz))
                    expectedPolys.Add(cells[iCell]);
            }
            if (cellList.Count != expectedPolys.Count)
                return false;
            foreach (TriCell cell in cellList)
            {
                if (!expectedPolys.Contains(cell))
                    return false;
            }
            return true;
        }
        
        ///
        ///Scans for Cell that contain the point (px, pz) and makes
        ///sure generated list matches the provided list.
        ///
        private Boolean IsCellListGood(float px, float pz
                , List<TriCell> cells
                , List<TriCell> cellList)
        {
            List<TriCell> expectedPolys = new List<TriCell>();
            foreach (TriCell cell in cells)
            {
                if (cell.IsInColumn(px, pz))
                    expectedPolys.Add(cell);
            }
            if (cellList.Count != expectedPolys.Count)
                return false;
            foreach (TriCell cell in cellList)
            {
                if (!expectedPolys.Contains(cell))
                    return false;
            }
            return true;
        }
        
        ///
        ///Loads a tree with the provided Cell.
        ///
        private TriCellQuadTree LoadTree(float[] verts, TriCell[] cells)
        {
            float[] meshBounds = TestUtil.GetVertBounds(verts);
            TriCellQuadTree tree = new TriCellQuadTree(meshBounds[0]
                                                 , meshBounds[2]
                                                 , meshBounds[3]  // + 0.1f
                                                 , meshBounds[5]  // + 0.1f
                                                 , 10);
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                tree.Add(cells[iCell]);
            }
            return tree;
        }
    }
}
