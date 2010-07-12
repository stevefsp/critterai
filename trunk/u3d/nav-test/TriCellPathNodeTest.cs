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
    /// Summary description for TriCellPathNodeTest
    /// </summary>
    [TestClass]
    public sealed class TriCellPathNodeTest
    {

        /*
         * Design notes:
         * 
         * There are a lot of dependencies.  The tests are ordered
         * from minimal to maximal dependence.
         * 
         * If the TriCell test suite fails, fix it before fixing any errors
         * from this test suite.
         */

        private const float TOLERANCE_STD = MathUtil.TOLERANCE_STD;

        private TestContext testContextInstance;

        private ITestMesh mMesh;
        private float[] verts;
        private int[] indices;
        private TriCell[] cells;
        private Vector3[] cents;

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
            verts = mMesh.GetVerts();
            indices = mMesh.GetIndices();
            cells = TestUtil.GetAllCells(verts, indices);
            TestUtil.LinkAllCells(cells);
            cents = new Vector3[cells.Length];
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
                cents[iCell] = cent;
            }
        }

        [TestMethod]
        public void TestCell()
        {
            int testCount = 0;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        break;
                    TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                    Vector3 cent = cents[iCell];
                    TriCellPathNode child = new TriCellPathNode(childCell
                            , parent
                            , cent.x, cent.y, cent.z);
                    Assert.IsTrue(child.Cell == childCell);
                    Assert.IsTrue(parent.Cell == cells[iCell]);
                    testCount++;
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }
        
        [TestMethod]
        public void TestParent() 
        {
            int testCount = 0;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    // Build a path 3 cells deep.
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        // Can't get to two cells deep.
                        break;
                    for (int iWallNext = 0; iWallNext < 3; iWallNext++)
                    {
                        TriCell childChildCell = childCell.GetLink(iWallNext);
                        if (childChildCell == null)
                            // Can't get to three cells deep.
                            break;
                        TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                        Vector3 cent = cents[iCell];
                        TriCellPathNode child = new TriCellPathNode(childCell
                                , parent
                                , cent.x, cent.y, cent.z);
                        TriCellPathNode childChild =  new TriCellPathNode(childChildCell
                                , child
                                , cent.x, cent.y, cent.z);
                        // Test accuracy.
                        Assert.IsTrue(parent.Parent == null);
                        Assert.IsTrue(child.Parent == parent);
                        Assert.IsTrue(childChild.Parent == child);
                        testCount++;
                    }
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }

        [TestMethod]
        public void TestPathSize() 
        {
            int testCount = 0;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    // Build a path 3 cells deep.
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        // Can't get to two cells deep.
                        break;
                    for (int iWallNext = 0; iWallNext < 3; iWallNext++)
                    {
                        TriCell childChildCell = childCell.GetLink(iWallNext);
                        if (childChildCell == null)
                            // Can't get to three cells deep.
                            break;
                        TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                        Vector3 cent = cents[iCell];
                        TriCellPathNode child = new TriCellPathNode(childCell
                                , parent
                                , cent.x, cent.y, cent.z);
                        TriCellPathNode childChild =  new TriCellPathNode(childChildCell
                                , child
                                , cent.x, cent.y, cent.z);
                        // Test accuracy.
                        Assert.IsTrue(parent.PathSize == 1);
                        Assert.IsTrue(child.PathSize == 2);
                        Assert.IsTrue(childChild.PathSize == 3);
                        testCount++;
                    }
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }
        
        [TestMethod]
        public void TestH()
        {
            int testCount = 0;
            float parentH = 20;
            float childH = 10;
            float childChildH = 5;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    // Build a path 3 cells deep.
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        // Can't get to two cells deep.
                        break;
                    for (int iWallNext = 0; iWallNext < 3; iWallNext++)
                    {
                        TriCell childChildCell = childCell.GetLink(iWallNext);
                        if (childChildCell == null)
                            // Can't get to three cells deep.
                            break;
                        TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                        parent.H = (parentH);
                        Vector3 cent = cents[iCell];
                        TriCellPathNode child = new TriCellPathNode(childCell
                                , parent
                                , cent.x, cent.y, cent.z);
                        child.H = (childH);
                        TriCellPathNode childChild =  new TriCellPathNode(childChildCell
                                , child
                                , cent.x, cent.y, cent.z);
                        childChild.H = (childChildH);
                        // Test accuracy.
                        Assert.IsTrue(parent.H == parentH);
                        float expected = childH;
                        float actual = child.H;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        expected = childChildH;
                        actual = childChild.H;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        testCount++;
                    }
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }

        [TestMethod]
        public void TestLocalG() 
        {
            int testCount = 0;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    // Build a path 3 cells deep.
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        // Can't get to two cells deep.
                        break;
                    for (int iWallNext = 0; iWallNext < 3; iWallNext++)
                    {
                        TriCell childChildCell = childCell.GetLink(iWallNext);
                        if (childChildCell == null)
                            // Can't get to three cells deep.
                            break;
                        TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                        Vector3 cent = cents[iCell];
                        TriCellPathNode child = new TriCellPathNode(childCell
                                , parent
                                , cent.x, cent.y, cent.z);
                        TriCellPathNode childChild =  new TriCellPathNode(childChildCell
                                , child
                                , cent.x, cent.y, cent.z);
                        // Test accuracy.
                        Assert.IsTrue(parent.LocalG == 0);
                        float expected = (float)Math.Sqrt(cells[iCell]
                                                 .GetLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall));
                        float actual = child.LocalG;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        expected = childCell
                            .GetLinkPointDistance(iWallNext, childCell.GetLinkIndex(cells[iCell]));
                        actual = childChild.LocalG;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        testCount++;
                    }
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }
        
        [TestMethod]
        public void TestG() 
        {
            int testCount = 0;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    // Build a path 3 cells deep.
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        // Can't get to two cells deep.
                        break;
                    for (int iWallNext = 0; iWallNext < 3; iWallNext++)
                    {
                        TriCell childChildCell = childCell.GetLink(iWallNext);
                        if (childChildCell == null)
                            // Can't get to three cells deep.
                            break;
                        TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                        Vector3 cent = cents[iCell];
                        TriCellPathNode child = new TriCellPathNode(childCell
                                , parent
                                , cent.x, cent.y, cent.z);
                        TriCellPathNode childChild =  new TriCellPathNode(childChildCell
                                , child
                                , cent.x, cent.y, cent.z);
                        // Test accuracy.
                        Assert.IsTrue(parent.G == 0);
                        float expected = (float)Math.Sqrt(cells[iCell]
                                                 .GetLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall));
                        float actual = child.G;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        expected += childCell
                            .GetLinkPointDistance(iWallNext, childCell.GetLinkIndex(cells[iCell]));
                        actual = childChild.G;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        testCount++;
                    }
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }
        
        [TestMethod]
        public void TestFWithoutHeuristic() 
        {
            int testCount = 0;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    // Build a path 3 cells deep.
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        // Can't get to two cells deep.
                        break;
                    for (int iWallNext = 0; iWallNext < 3; iWallNext++)
                    {
                        TriCell childChildCell = childCell.GetLink(iWallNext);
                        if (childChildCell == null)
                            // Can't get to three cells deep.
                            break;
                        TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                        Vector3 cent = cents[iCell];
                        TriCellPathNode child = new TriCellPathNode(childCell
                                , parent
                                , cent.x, cent.y, cent.z);
                        TriCellPathNode childChild =  new TriCellPathNode(childChildCell
                                , child
                                , cent.x, cent.y, cent.z);
                        // Test accuracy of F().
                        Assert.IsTrue(parent.F == 0);
                        float expected = (float)Math.Sqrt(cells[iCell]
                                                 .GetLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall));
                        float actual = child.F;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        expected += childCell
                            .GetLinkPointDistance(iWallNext, childCell.GetLinkIndex(cells[iCell]));
                        actual = childChild.F;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        testCount++;
                    }
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }

        [TestMethod]
        public void TestFWithHeuristic() 
        {
            int testCount = 0;
            float parentH = 20;
            float childH = 10;
            float childChildH = 5;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    // Build a path 3 cells deep.
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        // Can't get to two cells deep.
                        break;
                    for (int iWallNext = 0; iWallNext < 3; iWallNext++)
                    {
                        TriCell childChildCell = childCell.GetLink(iWallNext);
                        if (childChildCell == null)
                            // Can't get to three cells deep.
                            break;
                        TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                        parent.H = (parentH);
                        Vector3 cent = cents[iCell];
                        TriCellPathNode child = new TriCellPathNode(childCell
                                , parent
                                , cent.x, cent.y, cent.z);
                        child.H = (childH);
                        TriCellPathNode childChild =  new TriCellPathNode(childChildCell
                                , child
                                , cent.x, cent.y, cent.z);
                        childChild.H = (childChildH);
                        // Test accuracy of F().
                        Assert.IsTrue(parent.F == 0 + parentH);
                        float expected = (float)Math.Sqrt(cells[iCell]
                                                 .GetLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall))
                                                 + childH;
                        float actual = child.F;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        expected = (float)Math.Sqrt(cells[iCell]
                                           .GetLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall))
                                       + childCell
                                           .GetLinkPointDistance(iWallNext, childCell.GetLinkIndex(cells[iCell]))
                                       + childChildH;
                        actual = childChild.F;
                        Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE_STD));
                        testCount++;
                    }
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }
        
        [TestMethod]
        public void TestLoadPath() 
        {
            int testCount = 0;
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    // Build a path 3 cells deep.
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        // Can't get to two cells deep.
                        break;
                    for (int iWallNext = 0; iWallNext < 3; iWallNext++)
                    {
                        TriCell childChildCell = childCell.GetLink(iWallNext);
                        if (childChildCell == null)
                            // Can't get to three cells deep.
                            break;
                        TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                        Vector3 cent = cents[iCell];
                        TriCellPathNode child = new TriCellPathNode(childCell
                                , parent
                                , cent.x, cent.y, cent.z);
                        TriCellPathNode childChild =  new TriCellPathNode(childChildCell
                                , child
                                , cent.x, cent.y, cent.z);
                        // Parent
                        TriCell[] pcells = new TriCell[1];
                        parent.LoadPath(pcells);
                        pcells[0] = cells[iCell];
                        // Child
                        pcells = new TriCell[2];
                        child.LoadPath(pcells);
                        pcells[0] = cells[iCell];
                        pcells[1] = childCell;
                        // Child Child
                        pcells = new TriCell[3];
                        childChild.LoadPath(pcells);
                        pcells[0] = cells[iCell];
                        pcells[1] = childCell;
                        pcells[2] = childChildCell;
                        testCount++;
                    }
                }
            }
            if (testCount == 0)
                Assert.Fail("Mesh doesn't support test.");
        }
        
        [TestMethod]
        public void TestEstimateNewGAndSetParent() 
        {
            float[] verts = {
                    -4, 0, -2    // 0
                    , 2, 0, 4
                    , 0, 0, -2
                    , -4, 0, 0
                    , -2, 1, 0    // 4
            };
            
            Vector3 start = new Vector3(-2.8f, 0.2f, -0.4f);
            Vector3 midSA = new Vector3(-3, 0.5f, 0);
            Vector3 midSB = new Vector3(-1, 0.5f, -1);
            Vector3 midAG = new Vector3(-3, 0.5f, 1);
            Vector3 midBG = new Vector3(0, 0.5f, 2);
            
            float distSA = Vector3.Distance(start, midSA);
            float distSB = Vector3.Distance(start, midSB);
            float distAG = Vector3.Distance(midSA, midAG);
            float distBG = Vector3.Distance(midSB, midBG);
            
            float distSAG = distSA + distAG;
            float distSBG = distSB + distBG;
            
            TriCell cellStart = new TriCell(verts, 3, 4, 2);
            TriCell cellMidA = new TriCell(verts, 3, 0, 4);
            TriCell cellMidB = new TriCell(verts, 1, 2, 4);
            TriCell cellGoal = new TriCell(verts, 1, 4, 0);
            cellStart.Link(cellMidA, true);
            cellStart.Link(cellMidB, true);
            cellGoal.Link(cellMidA, true);
            cellGoal.Link(cellMidB, true);
            
            TriCellPathNode nodeStart = new TriCellPathNode(cellStart);
            TriCellPathNode nodeMidA = new TriCellPathNode(cellMidA, nodeStart, start.x, start.y, start.z);
            TriCellPathNode nodeMidB = new TriCellPathNode(cellMidB, nodeStart, start.x, start.y, start.z);
            TriCellPathNode nodeGoal = new TriCellPathNode(cellGoal, nodeMidA, start.x, start.y, start.z);
            
            // S-A-G
            float actual = nodeGoal.G;
            Assert.IsTrue(MathUtil.SloppyEquals(actual, distSAG, TOLERANCE_STD));
            // Estimate for S-B-G
            actual = nodeGoal.EstimateNewG(nodeMidB, start.x, start.y, start.z);
            Assert.IsTrue(MathUtil.SloppyEquals(actual, distSBG, TOLERANCE_STD));
            // Re-assign Parent
            nodeGoal.SetParent(nodeMidB, start.x, start.y, start.z);
            Assert.IsTrue(nodeGoal.Parent == nodeMidB);
            // S-B-G
            actual = nodeGoal.G;
            Assert.IsTrue(MathUtil.SloppyEquals(actual, distSBG, TOLERANCE_STD));
        }
    }
}
